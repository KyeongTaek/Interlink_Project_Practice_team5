using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;   // Scene1에서 재생할 영상
    public GameObject videoPanel;     // 영상 패널

    void Start()
    {
        // 처음에는 영상만 보이도록 설정
        videoPanel.SetActive(true);

        // 영상 끝났을 때 자동 호출되는 이벤트 등록
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    // 영상 끝났을 때 실행되는 함수
    void OnVideoEnd(VideoPlayer vp)
    {
        // Scene2로 이동
        SceneManager.LoadScene("Scene2");
        // 또는 SceneManager.LoadScene(1);  // 빌드 인덱스로 이동
    }
}
