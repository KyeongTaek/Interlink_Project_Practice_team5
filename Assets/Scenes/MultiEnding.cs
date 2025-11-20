using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageSwitcher_MultiEnd : MonoBehaviour
{
    void Start()
    {
        float testrate = 40f;
        UpdateZone(testrate);
    } //임시 테스트용

    [Header("Zone Target")]
    public Image zoneImage;
    public TextMeshProUGUI endingText;

    [Header("Shared Sprites (All Zones Use These)")]
    public Sprite goodSprite;    // 80% 이상
    public Sprite normalSprite;  // 50% 이상
    public Sprite badSprite;     // 50% 미만

    [Header("Ending Messages")]
    [TextArea] public string goodMessage = "Good End\n정답률 : ";
    [TextArea] public string normalMessage = "Normal End\n정답률 : ";
    [TextArea] public string badMessage = "Bad End\n정답률 : ";

    private void SetImageBasedOnRate(Image target, float rate)
    {
        string scoreText = rate.ToString("F0") + "%";
        if (rate >= 80f)
        {
            target.sprite = goodSprite;
            if (endingText != null) endingText.text = goodMessage + scoreText; 
            if (endingText != null) endingText.color = Color.white;
        }
        else if (rate >= 50f)
        {
            target.sprite = normalSprite;
            if (endingText != null) endingText.text = normalMessage + scoreText;
            if (endingText != null) endingText.color = Color.white;
        }
        else
        {
            target.sprite = badSprite;
            if (endingText != null) endingText.text = badMessage + scoreText;
            if (endingText != null) endingText.color = Color.white;
        }
    }

    public void UpdateZone(float rate)
    {
        SetImageBasedOnRate(zoneImage, rate);
        Debug.Log($"Zone 1 정답률 ({rate}%) 적용 완료.");
    }

}
