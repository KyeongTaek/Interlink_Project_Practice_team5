using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트를 위한 TextMeshPro 사용 (설치 필요)

public class LinearGraph : MonoBehaviour
{
    [Header("연결 설정")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private RectTransform labelYTemplate; // Y축 라벨 템플릿
    [SerializeField] private Image guidelineTemplate; // 가이드 라인 템플릿

    [Header("정답률 데이터 (0 ~ 100)")]
    public float[] answerRates;

    [Header("디자인 옵션")]
    public Color graphColor = Color.green;
    public float dotSize = 20f;
    public float lineThickness = 7f;

    [Header("여백 설정")]
    public float verticalPaddingRatio = 0.03f; // 상하 여백 비율
    public float horizontalPaddingRatio = 0.03f; // [추가] 좌우 여백 비율

    [Header("Y축 설정")]
    public int yAxisInterval = 10; // Y축 텍스트/라인 간격 (예: 10, 20)
    public float yAxisLabelOffset = 40f; // Y축 라벨이 그래프에서 떨어지는 거리
    public Color guidelineColor = new Color(0, 0, 0, 1f); // 가이드 라인 색상 (투명한 흰색)
    public float labelFontSize = 22f; // 라벨 폰트 크기

    private void Start()
    {
        // TextMeshPro가 없으면 에러 발생 가능성 -> TMP를 먼저 설치하세요.
        if (labelYTemplate != null) labelYTemplate.gameObject.SetActive(false);
        if (guidelineTemplate != null) guidelineTemplate.gameObject.SetActive(false);


        if (answerRates != null && answerRates.Length > 0)
            ShowGraph(answerRates);
        else
            ShowGraph(new float[] { 10, 50, 30, 80, 100 }); // 테스트용
    }

    public void ShowGraph(float[] valueList)
    {
        // 기존 그래프 요소들 삭제
        foreach (Transform child in graphContainer)
        {
            // 템플릿은 삭제하면 안 되므로 예외 처리
            if (child.gameObject == labelYTemplate.gameObject ||
                child.gameObject == guidelineTemplate.gameObject) continue;
            Destroy(child.gameObject);
        }

        if (graphContainer == null || valueList.Length == 0) return;

        // 1. 전체 높이/너비 및 여백 계산
        float containerHeight = graphContainer.rect.height;
        float containerWidth = graphContainer.rect.width;

        float verticalPadding = containerHeight * verticalPaddingRatio;
        float horizontalPadding = containerWidth * horizontalPaddingRatio; // [추가]

        float effectiveGraphHeight = containerHeight - (verticalPadding * 2);
        float effectiveGraphWidth = containerWidth - (horizontalPadding * 2); // [추가]

        float yOffset = verticalPadding;
        float xOffset = horizontalPadding; // [추가]

        float yMaximum = 100f;
        float xSize = effectiveGraphWidth / (valueList.Length + 1); // [수정] 유효 너비 사용

        GameObject lastCircle = null;

        // Y축 라벨 및 가이드 라인 생성
        for (int i = 0; i <= yMaximum; i += yAxisInterval)
        {
            float yPosition = (i / yMaximum) * effectiveGraphHeight + yOffset;

            // 라벨 생성
            CreateYAxisLabel(i.ToString(), new Vector2(xOffset - yAxisLabelOffset, yPosition));

            // 가이드 라인 생성 (0점 라인은 제외하거나 다르게 표현 가능)
            if (i > 0) // 0점 라인은 그리지 않거나 필요에 따라 추가
            {
                CreateGuideline(new Vector2(xOffset, yPosition), effectiveGraphWidth);
            }
        }

        // 그래프 점 및 선 그리기
        for (int i = 0; i < valueList.Length; i++)
        {
            float xPosition = xOffset + (i + 1) * xSize; // [수정] xOffset 시작
            float yPosition = (valueList[i] / yMaximum) * effectiveGraphHeight + yOffset;

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

    // [새 함수] Y축 라벨 생성
    private void CreateYAxisLabel(string labelText, Vector2 anchoredPosition)
    {
        if (labelYTemplate == null) { Debug.LogError("Y축 라벨 템플릿이 설정되지 않았습니다."); return; }

        GameObject labelObj = Instantiate(labelYTemplate.gameObject, graphContainer);
        labelObj.SetActive(true);
        RectTransform rect = labelObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmpText = labelObj.GetComponent<TextMeshProUGUI>();

        if (tmpText == null) { Debug.LogError("Y축 라벨 템플릿에 TextMeshProUGUI 컴포넌트가 없습니다."); return; }

        tmpText.text = labelText;
        tmpText.fontSize = labelFontSize;
        tmpText.color = Color.black ; // 라벨 색상
        tmpText.alignment = TextAlignmentOptions.Right; // 오른쪽 정렬 (그래프에서 왼쪽으로 나오게)

        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(100, labelFontSize * 1.5f); // 라벨 크기
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    // [새 함수] 가이드 라인 생성
    private void CreateGuideline(Vector2 startPosition, float width)
    {
        if (guidelineTemplate == null) { Debug.LogError("가이드 라인 템플릿이 설정되지 않았습니다."); return; }

        GameObject lineObj = Instantiate(guidelineTemplate.gameObject, graphContainer);
        lineObj.SetActive(true);
        lineObj.GetComponent<Image>().color = guidelineColor; // 투명도 있는 색상

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(startPosition.x + width / 2, startPosition.y); // 라인 중앙 정렬
        rect.sizeDelta = new Vector2(width, 2f); // 라인 길이와 두께
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }
}