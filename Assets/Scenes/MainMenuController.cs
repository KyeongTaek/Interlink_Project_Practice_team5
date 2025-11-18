using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // 게임 시작 버튼 연결 함수
    public void OnClickStart()
    {
        SceneManager.LoadScene("Game");
    }

    // 기록 버튼 연결 함수
    public void OnClickRecord()
    {
        SceneManager.LoadScene("Record");
    }

    // Stage1 진입
    public void OnClickStage1()
    {
        SceneManager.LoadScene("Stage1");
    }


    public GameObject quitPanel;

    // 게임 종료 버튼 연결 함수
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitPanel.activeSelf)
            {
                CloseQuitPanel();
            }
            else
            {
                ShowQuitPanel();
            }
        }
    }

    public void ShowQuitPanel()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuitPanel()
    {
        quitPanel.SetActive(false);
    }

    public void GameQuit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 끄기
        #else
            Application.Quit(); // 실제 게임에서 끄기
        #endif
    }
}