using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    void Start()
    {
        UpdateZone1(40f);
        UpdateZone2(60f);
        UpdateZone3(85f);
    } //임시 테스트용
    // ▼▼▼ 1. 구역별 UI Image 컴포넌트 변수 (3개) ▼▼▼
    [Header("Zone Targets")]
    public Image zone1Image;
    public Image zone2Image;
    public Image zone3Image;

    // ▼▼▼ 2. 모든 구역이 공유할 Sprite 에셋 변수 (3개) ▼▼▼
    [Header("Shared Sprites (All Zones Use These)")]
    public Sprite goodSprite;    // 80% 이상
    public Sprite normalSprite;  // 50% 이상
    public Sprite badSprite;     // 50% 미만

    // ------------------------------------------------------------------
    // 핵심 로직: 정답률에 따라 이미지를 설정하는 재활용 함수
    // ------------------------------------------------------------------
    // 이제 SetImageBasedOnRate 함수는 공유 Sprite만 사용합니다.
    private void SetImageBasedOnRate(Image target, float rate)
    {
        if (rate >= 80f)
        {
            target.sprite = goodSprite; // 공유 good Sprite 사용
        }
        else if (rate >= 50f)
        {
            target.sprite = normalSprite; // 공유 normal Sprite 사용
        }
        else
        {
            target.sprite = badSprite; // 공유 bad Sprite 사용
        }
    }

    // ------------------------------------------------------------------
    // ▼▼▼ 3. 외부에서 호출할 구역별 함수 (코드는 더 간결해짐) ▼▼▼
    // ------------------------------------------------------------------

    public void UpdateZone1(float rate)
    {
        SetImageBasedOnRate(zone1Image, rate);
        Debug.Log($"Zone 1 정답률 ({rate}%) 적용 완료.");
    }

    public void UpdateZone2(float rate)
    {
        SetImageBasedOnRate(zone2Image, rate);
        Debug.Log($"Zone 2 정답률 ({rate}%) 적용 완료.");
    }

    public void UpdateZone3(float rate)
    {
        SetImageBasedOnRate(zone3Image, rate);
        Debug.Log($"Zone 3 정답률 ({rate}%) 적용 완료.");
    }
}
