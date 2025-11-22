using UnityEngine;
using UnityEngine.Video;

public class End : MonoBehaviour
{
    public GameObject videoPanel;      // 영상 패널
    public VideoPlayer videoPlayer;    // VideoPlayer 컴포넌트

    void Start()
    {
        // 영상 패널 켜기
        videoPanel.SetActive(true);

        // 영상 재생
        videoPlayer.Play();
    }
}
