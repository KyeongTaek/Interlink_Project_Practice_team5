using UnityEngine;
using UnityEngine.UI;

public class LinearGraph : MonoBehaviour
{
    [Header("연결 설정")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private Sprite circleSprite;

    [Header("정답률 데이터 (0 ~ 100)")]
    public float[] answerRates;

    [Header("디자인 옵션")]
    public Color graphColor = Color.green;
    public float dotSize = 30f;
    public float lineThickness = 8f;

    private void Start()
    {
        if (answerRates != null && answerRates.Length > 0)
            ShowGraph(answerRates);
        else
            ShowGraph(new float[] { 10, 50, 30, 80, 100 }); // 테스트용
    }

    public void ShowGraph(float[] valueList)
    {
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        if (graphContainer == null || valueList.Length == 0) return;

        // [수정됨] Stretch 모드에서도 정상 작동하도록 rect.width 사용
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;

        float yMaximum = 100f;
        float xSize = graphWidth / (valueList.Length + 1);

        GameObject lastCircle = null;

        for (int i = 0; i < valueList.Length; i++)
        {
            float xPosition = (i + 1) * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;

            GameObject circle = CreateCircle(new Vector2(xPosition, yPosition));

            if (lastCircle != null)
            {
                CreateDotConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                                    circle.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircle = circle;
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject circleObj = new GameObject("circle", typeof(Image));
        circleObj.transform.SetParent(graphContainer, false);
        circleObj.GetComponent<Image>().sprite = circleSprite;
        circleObj.GetComponent<Image>().color = graphColor;

        RectTransform rect = circleObj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(dotSize, dotSize);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);

        return circleObj;
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject lineObj = new GameObject("dotConnection", typeof(Image));
        lineObj.transform.SetParent(graphContainer, false);
        lineObj.GetComponent<Image>().color = new Color(graphColor.r, graphColor.g, graphColor.b, 0.5f);

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(distance, lineThickness);
        rect.anchoredPosition = dotPositionA + dir * distance * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localEulerAngles = new Vector3(0, 0, angle);
    }
}