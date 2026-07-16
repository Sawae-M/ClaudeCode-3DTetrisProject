using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Text     scoreText;
    Text     levelText;
    Text     nextPieceText;
    GameObject gameOverPanel;
    Text     finalScoreText;

    void Awake() => BuildUI();

    void BuildUI()
    {
        // ── Canvas ──────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        Transform ct = canvas.transform;

        // ── スコア（右上） ───────────────────────────────
        var scorePanel = MakeBox(ct, new Vector2(1,1), new Vector2(-10,-10), new Vector2(170, 64));
        scoreText = MakeText(scorePanel, "SCORE\n0", 18);

        // ── レベル（右上・スコアの下） ───────────────────
        var levelPanel = MakeBox(ct, new Vector2(1,1), new Vector2(-10,-84), new Vector2(170, 40));
        levelText = MakeText(levelPanel, "LEVEL  1", 18);

        // ── NEXT（左上） ────────────────────────────────
        var nextPanel = MakeBox(ct, new Vector2(0,1), new Vector2(10,-10), new Vector2(170, 40));
        nextPieceText = MakeText(nextPanel, "NEXT  -", 18);

        // ── 操作説明（左下） ────────────────────────────
        var ctrlPanel = MakeBox(ct, new Vector2(0,0), new Vector2(10,10), new Vector2(230, 185));
        MakeText(ctrlPanel,
            "[ ピース操作 ]\n" +
            "←→↑↓  移動\n" +
            "Space  高速落下\n\n" +
            "[ カメラ視点 ]\n" +
            "W/S  上下の面\n" +
            "A/D  左右の面",
            13);

        // ── ゲームオーバー（中央） ───────────────────────
        gameOverPanel = MakeBox(ct, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(320, 180)).gameObject;
        finalScoreText = MakeText(gameOverPanel.transform, "GAME OVER\nSCORE: 0", 24, TextAnchor.MiddleCenter);

        // RESTART ボタン
        var btnGO = new GameObject("Restart");
        btnGO.transform.SetParent(gameOverPanel.transform, false);
        var br = btnGO.AddComponent<RectTransform>();
        br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = new Vector2(0, 14);
        br.sizeDelta        = new Vector2(180, 42);
        btnGO.AddComponent<Image>().color = new Color(0.15f, 0.6f, 0.15f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => GameManager.Instance.Restart());
        MakeText(btnGO.transform, "RESTART", 18, TextAnchor.MiddleCenter);

        gameOverPanel.SetActive(false);
    }

    // ── ヘルパー ────────────────────────────────────────────
    static RectTransform MakeBox(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color? col = null)
    {
        var go = new GameObject("Box");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = r.pivot = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        go.AddComponent<Image>().color = col ?? new Color(0f, 0f, 0f, 0.65f);
        return r;
    }

    static Text MakeText(Transform parent, string text, int size = 14,
        TextAnchor anchor = TextAnchor.UpperLeft)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(8, 8);
        r.offsetMax = new Vector2(-8, -8);
        var t = go.AddComponent<Text>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = Color.white;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = anchor;
        t.supportRichText = true;
        t.resizeTextForBestFit = false;
        t.horizontalOverflow   = HorizontalWrapMode.Wrap;
        t.verticalOverflow     = VerticalWrapMode.Overflow;
        return t;
    }

    // ── 外部 API ────────────────────────────────────────────
    public void UpdateScore(int score)  { if (scoreText)     scoreText.text     = $"SCORE\n{score:N0}"; }
    public void UpdateLevel(int level)  { if (levelText)     levelText.text     = $"LEVEL  {level}"; }
    public void UpdateNextPiece(PieceDefinition next)
    {
        if (nextPieceText) nextPieceText.text = $"NEXT  {next?.pieceName ?? "-"}";
    }
    public void UpdateAll()
    {
        UpdateScore(GameManager.Instance.Score);
        UpdateLevel(GameManager.Instance.Level);
    }
    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreText) finalScoreText.text = $"GAME OVER\nSCORE: {GameManager.Instance.Score:N0}";
    }
    public void HideGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }
}
