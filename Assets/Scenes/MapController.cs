using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동 필수

public class MapController : MonoBehaviour
{
    [Header("연결할 요소")]
    public Transform playerIcon;      // 지도 위를 움직일 캐릭터
    public Transform[] stageNodes;    // 스테이지 1~5 + 화산 위치 (총 6개)

    [Header("설정")]
    public float moveSpeed = 10f;     // 캐릭터 이동 속도

    // 내부 변수
    private int currentIndex = 0;     // 현재 캐릭터가 서 있는 위치 인덱스 (0 ~ 5)
    private int unlockedStage = 1;    // 해금된 최대 스테이지 (기본 1)

    void Start()
    {
        unlockedStage = PlayerPrefs.GetInt("ClearedLevel", 6); //테스트용
        // 1. 저장된 클리어 정보를 불러옵니다. (기본값 1)
        // "ClearedLevel"이라는 키로 저장한다고 가정합니다.
        // 예: 5스테이지 클리어 시 -> 6(화산)이 저장되어 있어야 함.
        // unlockedStage = PlayerPrefs.GetInt("ClearedLevel", 1);

        // 2. 캐릭터를 현재 해금된 가장 마지막 위치나 0번 위치에 둡니다.
        currentIndex = Mathf.Clamp(unlockedStage - 1, 0, stageNodes.Length - 1);
        playerIcon.position = stageNodes[currentIndex].position;
    }

    void Update()
    {
        HandleMovementInput();
        HandleEnterInput();
        MoveCharacterSmoothly();
    }

    // 1. 화살표 키 이동 로직
    void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 다음 스테이지가 있고 && 그 스테이지가 해금되었다면 이동
            if (currentIndex < stageNodes.Length - 1 && (currentIndex + 1) < unlockedStage)
            {
                currentIndex++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 뒤로는 언제든 갈 수 있음
            if (currentIndex > 0)
            {
                currentIndex--;
            }
        }
    }

    // 2. 부드러운 이동 처리
    void MoveCharacterSmoothly()
    {
        playerIcon.position = Vector3.Lerp(playerIcon.position, stageNodes[currentIndex].position, moveSpeed * Time.deltaTime);
    }

    // 3. 엔터 키 입장 로직
    void HandleEnterInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            EnterStage();
        }
    }

    void EnterStage()
    {
        // 배열 인덱스는 0부터 시작하므로 +1을 해서 스테이지 번호 계산
        // 인덱스 0 = 스테이지 1 ... 인덱스 4 = 스테이지 5, 인덱스 5 = 화산

        if (currentIndex == 5) // 화산(엔딩) 위치라면
        {
            Debug.Log("화산 진입! 멀티 엔딩 씬으로 이동");
            SceneManager.LoadScene("MultiEnding");
        }
        else // 일반 스테이지라면
        {
            string sceneName = "Stage" + (currentIndex + 1); // "Stage1", "Stage2" ...
            Debug.Log(sceneName + " 진입");
            SceneManager.LoadScene(sceneName);
        }
    }
}