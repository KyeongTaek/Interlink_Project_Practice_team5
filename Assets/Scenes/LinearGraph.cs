using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class LinearGraph : MonoBehaviour
{
    [Header("?°ê²° ?¤ì •")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private RectTransform labelYTemplate;
    [SerializeField] private Image guidelineTemplate;
    [SerializeField] private RectTransform labelXTemplate; // [ì¶”ê?] Xì¶??¼ë²¨ ?œí”Œë¦?

    [Header("?•ë‹µë¥??°ì´??(0 ~ 100)")]
    public float[] answerRates;

    [Header("Xì¶??¤ì •")] // [ì¶”ê?]
    public string[] xAxisLabels; // êµ¬ì—­ ?´ë¦„ ?ìŠ¤??(?? "êµ¬ì—­ 1", "êµ¬ì—­ 2", "êµ¬ì—­ 3")
    public float xAxisLabelOffset = 20f; // Xì¶??¼ë²¨??ê·¸ë˜?„ì—???„ë˜ë¡??¨ì–´ì§€??ê±°ë¦¬

    [Header("?”ì???µì…˜")]
    public Color graphColor = Color.green;
    public float dotSize = 20f;
    public float lineThickness = 7f;

    [Header("?¬ë°± ?¤ì •")]
    public float verticalPaddingRatio = 0.03f;
    public float horizontalPaddingRatio = 0.03f;

    [Header("Yì¶??¤ì •")]
    public int yAxisInterval = 10;
    public float yAxisLabelOffset = 50f;
    public Color guidelineColor = new Color(0, 0, 0, 0.5f);
    public float labelFontSize = 21f;

    private void Start()
    {
        // ?œí”Œë¦?ë¹„í™œ?±í™” ?•ì¸
        if (labelYTemplate != null) labelYTemplate.gameObject.SetActive(false);
        if (guidelineTemplate != null) guidelineTemplate.gameObject.SetActive(false);
        if (labelXTemplate != null) labelXTemplate.gameObject.SetActive(false); // [ì¶”ê?]


        // load from db
        float[] test = new float[3];
        
        string sceneName = graphContainer.gameObject.name;
        if (sceneName == "Scenario1")
        {
            test = LoadData(1);
        }
        else if (sceneName == "Scenario2")
        {
            test = LoadData(2);
        }
        else if (sceneName == "Scenario3")
        {
            test = LoadData(3);
        }
        else if (sceneName == "Total")
        {
            test = LoadData();
        }
        ShowGraph(test);

        //if (answerRates != null && answerRates.Length > 0)
        //    ShowGraph(answerRates);
        //else
        //    ShowGraph(new float[] { 10, 50, 30 }); // 3ê°??”ì†Œ ?ŒìŠ¤?¸ìš©
    }

    public void ShowGraph(float[] valueList)
    {
        // ê¸°ì¡´ ê·¸ë˜???”ì†Œ???? œ (?ëµ)
        foreach (Transform child in graphContainer)
        {
            if (child.gameObject == labelYTemplate.gameObject ||
                child.gameObject == guidelineTemplate.gameObject ||
                child.gameObject == labelXTemplate.gameObject) continue; // [?˜ì •]
            Destroy(child.gameObject);
        }

        if (graphContainer == null || valueList.Length == 0) return;

        // 1. ?„ì²´ ?’ì´/?ˆë¹„ ë°??¬ë°± ê³„ì‚°
        float containerHeight = graphContainer.rect.height;
        float containerWidth = graphContainer.rect.width;

        float verticalPadding = containerHeight * verticalPaddingRatio;
        float horizontalPadding = containerWidth * horizontalPaddingRatio;

        float effectiveGraphHeight = containerHeight - (verticalPadding * 2);
        float effectiveGraphWidth = containerWidth - (horizontalPadding * 2);

        float yOffset = verticalPadding;
        float xOffset = horizontalPadding;

        float yMaximum = 100f;
        float xSize = effectiveGraphWidth / (valueList.Length + 1);

        GameObject lastCircle = null;

        // Yì¶??¼ë²¨ ë°?ê°€?´ë“œ ?¼ì¸ ?ì„±
        for (int i = 0; i <= yMaximum; i += yAxisInterval)
        {
            float yPosition = (i / yMaximum) * effectiveGraphHeight + yOffset;

            // ?¼ë²¨ ?ì„±
            CreateYAxisLabel(i.ToString(), new Vector2(xOffset - yAxisLabelOffset, yPosition));

            // ê°€?´ë“œ ?¼ì¸ ?ì„± (0???¼ì¸?€ ?œì™¸?˜ê±°???¤ë¥´ê²??œí˜„ ê°€??
            if (i >= 0) // 0???¼ì¸?€ ê·¸ë¦¬ì§€ ?Šê±°???„ìš”???°ë¼ ì¶”ê?
            {
                CreateGuideline(new Vector2(xOffset, yPosition), effectiveGraphWidth);
            }
        }
        // ê·¸ë˜????ë°???ê·¸ë¦¬ê¸?+ Xì¶??¼ë²¨ ?ì„±
        for (int i = 0; i < valueList.Length; i++)
        {
            float xPosition = xOffset + (i + 1) * xSize;
            float yPosition = (valueList[i] / yMaximum) * effectiveGraphHeight + yOffset;

            GameObject circle = CreateCircle(new Vector2(xPosition, yPosition));

            // [ì¶”ê?] Xì¶??¼ë²¨ ?ì„± (?°ì´??ê°œìˆ˜?€ ?¼ë²¨ ê°œìˆ˜ê°€ ë§ì•„????
            if (xAxisLabels != null && xAxisLabels.Length > i)
            {
                // ?¼ë²¨ ?„ì¹˜??ê·¸ë˜???„ë˜ìª??¬ë°±(yOffset)?ì„œ ?¤ì‹œ ?„ë˜ë¡?xAxisLabelOffset ë§Œí¼ ?´ë™
                Vector2 labelPosition = new Vector2(xPosition, yOffset - xAxisLabelOffset);
                CreateXAxisLabel(xAxisLabels[i], labelPosition);
            }

            if (lastCircle != null)
            {
                CreateDotConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                                     circle.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircle = circle;
        }
    }

    // ... (CreateCircle, CreateDotConnection, CreateYAxisLabel ?¨ìˆ˜???ëµ / ë³€ê²??†ìŒ)

    // [???¨ìˆ˜] Xì¶??¼ë²¨ ?ì„±
    private void CreateXAxisLabel(string labelText, Vector2 anchoredPosition)
    {
        if (labelXTemplate == null) { Debug.LogError("Xì¶??¼ë²¨ ?œí”Œë¦¿ì´ ?¤ì •?˜ì? ?Šì•˜?µë‹ˆ??"); return; }

        GameObject labelObj = Instantiate(labelXTemplate.gameObject, graphContainer);
        labelObj.SetActive(true);
        RectTransform rect = labelObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmpText = labelObj.GetComponent<TextMeshProUGUI>();

        if (tmpText == null) { Debug.LogError("Xì¶??¼ë²¨ ?œí”Œë¦¿ì— TextMeshProUGUI ì»´í¬?ŒíŠ¸ê°€ ?†ìŠµ?ˆë‹¤."); return; }

        tmpText.text = labelText;
        tmpText.fontSize = labelFontSize;
        tmpText.color = Color.black;
        tmpText.alignment = TextAlignmentOptions.Center; // [ì¤‘ìš”] ì¤‘ì•™ ?•ë ¬

        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150, labelFontSize * 1.5f);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
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

    // [???¨ìˆ˜] Yì¶??¼ë²¨ ?ì„±
    private void CreateYAxisLabel(string labelText, Vector2 anchoredPosition)
    {
        if (labelYTemplate == null) { Debug.LogError("Yì¶??¼ë²¨ ?œí”Œë¦¿ì´ ?¤ì •?˜ì? ?Šì•˜?µë‹ˆ??"); return; }

        GameObject labelObj = Instantiate(labelYTemplate.gameObject, graphContainer);
        labelObj.SetActive(true);
        RectTransform rect = labelObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmpText = labelObj.GetComponent<TextMeshProUGUI>();

        if (tmpText == null) { Debug.LogError("Yì¶??¼ë²¨ ?œí”Œë¦¿ì— TextMeshProUGUI ì»´í¬?ŒíŠ¸ê°€ ?†ìŠµ?ˆë‹¤."); return; }

        tmpText.text = labelText;
        tmpText.fontSize = labelFontSize;
        tmpText.color = Color.black; // ?¼ë²¨ ?‰ìƒ
        tmpText.alignment = TextAlignmentOptions.Right; // ?¤ë¥¸ìª??•ë ¬ (ê·¸ë˜?„ì—???¼ìª½?¼ë¡œ ?˜ì˜¤ê²?

        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(100, labelFontSize * 1.5f); // ?¼ë²¨ ?¬ê¸°
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    // [???¨ìˆ˜] ê°€?´ë“œ ?¼ì¸ ?ì„±
    private void CreateGuideline(Vector2 startPosition, float width)
    {
        if (guidelineTemplate == null) { Debug.LogError("ê°€?´ë“œ ?¼ì¸ ?œí”Œë¦¿ì´ ?¤ì •?˜ì? ?Šì•˜?µë‹ˆ??"); return; }

        GameObject lineObj = Instantiate(guidelineTemplate.gameObject, graphContainer);
        lineObj.SetActive(true);
        lineObj.GetComponent<Image>().color = guidelineColor; // ?¬ëª…???ˆëŠ” ?‰ìƒ

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(startPosition.x + width / 2, startPosition.y); // ?¼ì¸ ì¤‘ì•™ ?•ë ¬
        rect.sizeDelta = new Vector2(width, 2f); // ?¼ì¸ ê¸¸ì´?€ ?ê»˜
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }
    // ... (ê¸°ì¡´ CreateCircle, CreateDotConnection, CreateYAxisLabel, CreateGuideline ?¨ìˆ˜ ?¬í•¨)

    // load from db(scenario ver)
    private float[] LoadData(int sceneNum)
    {
        float[] rates = new float[3];

        string dbname = "/test.db";
        string connectionString = "URI=file:" + Application.streamingAssetsPath + dbname;
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        string tablename = "Record";

        IDbCommand dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "SELECT rate_stage_" + (2*sceneNum-1) + " FROM " + tablename + " ORDER BY session_id DESC LIMIT 3"; // get scenario rate(1 or 2 or 3) for latest 3 session
        Debug.Log(dbCommand.CommandText);

        IDataReader dataReader = dbCommand.ExecuteReader();

        int cnt = 0;
        while (dataReader.Read())
        {
            float rateStage = dataReader.GetFloat(0);
            rates[cnt++] = rateStage;
        }
        dataReader.Close();

        return rates;
    }
    
    // load from db(total ver)
    private float[] LoadData()
    {
        float[] rates = new float[3];

        string dbname = "/test.db";
        string connectionString = "URI=file:" + Application.streamingAssetsPath + dbname;
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        string tablename = "Record";

        IDbCommand dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "SELECT rate_total FROM " + tablename + " ORDER BY session_id DESC LIMIT 3"; // get scenario rate(1 or 2 or 3) for latest 3 session
        Debug.Log(dbCommand.CommandText);

        IDataReader dataReader = dbCommand.ExecuteReader();

        int cnt = 0;
        while (dataReader.Read())
        {
            float rateStage = dataReader.GetFloat(0);
            rates[cnt++] = rateStage;
        }
        dataReader.Close();

        return rates;
    }
}