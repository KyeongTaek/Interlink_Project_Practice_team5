using UnityEngine;
using UnityEngine.Video;

public class Scene2Manager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;       // Scene2에서 재생할 영상
    public GameObject videoPanel;         // 영상 화면 포함 패널

    [Header("Follow Panel")]
    public GameObject followPanel;        // "이제 따라해봐요" 등의 UI

    [Header("Finish Panel")]
    public GameObject finishPanel;        // "수고했어요!" 패널

    void Start()
    {
        // 시작 시 표시될 패널 설정
        videoPanel.SetActive(true);
        followPanel.SetActive(true);
        finishPanel.SetActive(false);

        // 영상 자동 재생
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    // 교육자가 누르는 버튼
    public void OnDoneButtonClicked()
    {
        followPanel.SetActive(false);
        finishPanel.SetActive(true);
    }

    // 다시 보기 버튼 (원하면)
    public void OnReplayButtonClicked()
    {
        finishPanel.SetActive(false);
        followPanel.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    // 다음 씬으로 넘어가는 버튼 (필요하면 사용)
    public void OnNextSceneButton()
    {
        // Scene3로 이동하고 싶으면 아래 주석 해제
        // SceneManager.LoadScene("Scene3");
    }
}
