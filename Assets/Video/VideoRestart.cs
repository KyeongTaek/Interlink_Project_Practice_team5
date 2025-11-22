using UnityEngine;
using UnityEngine.Video;

public class VideoReplayByEnter : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Update()
    {
        // 엔터키(KeyCode.Return) 입력 체크
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 영상의 프레임을 처음으로 돌리고 재생
            videoPlayer.frame = 0;
            videoPlayer.Play();
        }
    }
}
