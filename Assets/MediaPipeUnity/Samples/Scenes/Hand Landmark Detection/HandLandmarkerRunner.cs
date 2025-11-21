// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
    {
        [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

        private Experimental.TextureFramePool _textureFramePool;

        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

        // =================================================================
        // [변수] 감도 및 상태 저장
        // =================================================================
        private float _prevWristX = 0f;
        public float _swipeThreshold = 0.005f;
        private bool _isFistDetected = false;
        // =================================================================

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            // 초기화 로그
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Image Read Mode = {config.ImageReadMode}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumHands = {config.NumHands}");
            Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
            Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
            taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);
            screen.Initialize(imageSource);
            SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var result = HandLandmarkerResult.Alloc(options.numHands);

            var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused) yield return new WaitWhile(() => isPaused);

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                Mediapipe.Image image;
                switch (config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage) throw new System.Exception("ImageReadMode.GPU is not supported");
                        textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildGPUImage(glContext);
                        yield return waitForEndOfFrame;
                        break;
                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;
                        textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        yield return waitUntilReqDone;
                        if (req.hasError) { Debug.LogWarning($"Failed to read texture"); continue; }
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result)) _handLandmarkerResultAnnotationController.DrawNow(result);
                        else _handLandmarkerResultAnnotationController.DrawNow(default);
                        break;
                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result)) _handLandmarkerResultAnnotationController.DrawNow(result);
                        else _handLandmarkerResultAnnotationController.DrawNow(default);
                        break;
                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Mediapipe.Image image, long timestamp)
        {
            _handLandmarkerResultAnnotationController.DrawLater(result);

            // =================================================================
            // [통합 로직] 함수 호출 없이 여기서 바로 계산 (에러 원천 차단)
            // =================================================================
            if (result.handLandmarks != null && result.handLandmarks.Count > 0)
            {
                var firstHandWrapper = result.handLandmarks[0];

                if (firstHandWrapper.landmarks != null && firstHandWrapper.landmarks.Count > 0)
                {
                    var landmarks = firstHandWrapper.landmarks;
                    var wrist = landmarks[0]; // 손목

                    // ---------------------------------------------------------
                    // 1. 주먹 감지 (계산식 직접 입력)
                    // Tip(끝)과 Wrist(손목) 거리 vs PIP(중간)와 Wrist(손목) 거리 비교
                    // ---------------------------------------------------------

                    // 검지 (Index: 8 vs 6)
                    float distTip8 = (landmarks[8].x - wrist.x) * (landmarks[8].x - wrist.x) + (landmarks[8].y - wrist.y) * (landmarks[8].y - wrist.y);
                    float distPip6 = (landmarks[6].x - wrist.x) * (landmarks[6].x - wrist.x) + (landmarks[6].y - wrist.y) * (landmarks[6].y - wrist.y);
                    bool isIndexFolded = distTip8 < distPip6;

                    // 중지 (Middle: 12 vs 10)
                    float distTip12 = (landmarks[12].x - wrist.x) * (landmarks[12].x - wrist.x) + (landmarks[12].y - wrist.y) * (landmarks[12].y - wrist.y);
                    float distPip10 = (landmarks[10].x - wrist.x) * (landmarks[10].x - wrist.x) + (landmarks[10].y - wrist.y) * (landmarks[10].y - wrist.y);
                    bool isMiddleFolded = distTip12 < distPip10;

                    // 약지 (Ring: 16 vs 14)
                    float distTip16 = (landmarks[16].x - wrist.x) * (landmarks[16].x - wrist.x) + (landmarks[16].y - wrist.y) * (landmarks[16].y - wrist.y);
                    float distPip14 = (landmarks[14].x - wrist.x) * (landmarks[14].x - wrist.x) + (landmarks[14].y - wrist.y) * (landmarks[14].y - wrist.y);
                    bool isRingFolded = distTip16 < distPip14;

                    // 소지 (Pinky: 20 vs 18)
                    float distTip20 = (landmarks[20].x - wrist.x) * (landmarks[20].x - wrist.x) + (landmarks[20].y - wrist.y) * (landmarks[20].y - wrist.y);
                    float distPip18 = (landmarks[18].x - wrist.x) * (landmarks[18].x - wrist.x) + (landmarks[18].y - wrist.y) * (landmarks[18].y - wrist.y);
                    bool isPinkyFolded = distTip20 < distPip18;

                    // 네 손가락이 다 접히면 주먹(0)
                    if (isIndexFolded && isMiddleFolded && isRingFolded && isPinkyFolded)
                    {
                        if (!_isFistDetected)
                        {
                            Debug.Log("0");
                            _isFistDetected = true;
                        }
                    }
                    else
                    {
                        _isFistDetected = false;
                    }

                    // ---------------------------------------------------------
                    // 2. 흔들기 감지
                    // ---------------------------------------------------------
                    var currentWristX = landmarks[0].x;
                    float movement = currentWristX - _prevWristX;

                    if (movement > _swipeThreshold)
                    {
                        Debug.Log("1");
                    }
                    _prevWristX = currentWristX;
                }
            }
        }
    }
}