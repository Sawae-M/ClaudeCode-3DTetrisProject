using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Text       scoreText;
    Text       levelText;
    Text       nextPieceText;
    GameObject gameOverPanel;
    Text       finalScoreText;

    void Awake() => BuildUI();

    void BuildUI()
    {
        // ── Canvas（スケーリングなし・ピクセル等倍） ────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        // ConstantPixelSize にすることでフォントテクスチャを等倍描画 → 鮮明
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        canvasGO.AddComponent<GraphicRaycaster>();
        Transform ct = canvas.transform;

        // ── スコア（右上） ──────────────────────────────────
        var scorePanel = MakeBox(ct, new Vector2(1,1), new Vector2(-10,-10), new Vector2(180, 76));
        scoreText = MakeText(scorePanel, "SCORE\n0", 22);

        // ── レベル（右上・スコアの下） ──────────────────────
        var levelPanel = MakeBox(ct, new Vector2(1,1), new Vector2(-10,-96), new Vector2(180, 46));
        levelText = MakeText(levelPanel, "LEVEL  1", 22);

        // ── NEXT（左上） ─────────────────────────────────────
        var nextPanel = MakeBox(ct, new Vector2(0,1), new Vector2(10,-10), new Vector2(200, 46));
        nextPieceText = MakeText(nextPanel, "NEXT  -", 22);

        // ── 操作説明（左下） ─────────────────────────────────
        var ctrlPanel = MakeBox(ct, new Vector2(0,0), new Vector2(10,10), new Vector2(240, 260));
        MakeText(ctrlPanel,
            "[ 移動 ]\n" +
            "← →  左右\n" +
            "↑ ↓  奥・手前\n" +
            "Space  高速落下\n\n" +
            "[ 回転 ]\n" +
            "Q / E  Y軸回転\n" +
            "Z / X  X軸回転\n\n" +
            "[ 視点 ]\n" +
            "W / S  上下\n" +
            "A / D  左右",
            16);

        // ── ゲームオーバー（中央） ──────────────────────────
        gameOverPanel = MakeBox(ct, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(360, 200)).gameObject;
        finalScoreText = MakeText(gameOverPanel.transform, "GAME OVER\nSCORE: 0", 28, TextAnchor.MiddleCenter);

        var btnGO = new GameObject("Restart");
        btnGO.transform.SetParent(gameOverPanel.transform, false);
        var br = btnGO.AddComponent<RectTransform>();
        br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = new Vector2(0, 14);
        br.sizeDelta        = new Vector2(200, 48);
        btnGO.AddComponent<Image>().color = new Color(0.15f, 0.6f, 0.15f);
        btnGO.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance.Restart());
        MakeText(btnGO.transform, "RESTART", 22, TextAnchor.MiddleCenter);

        gameOverPanel.SetActive(false);
    }

    // ────── ヘルパー ───────────────────────────────────────────
    static Font GetFont(int size)
    {
        // システムフォントを動的生成（Arial 系で高品質レンダリング）
        string[] names = Font.GetOSInstalledFontNames();
        foreach (var n in names)
        {
            string nl = n.ToLower();
            if (nl == "arial" || nl == "helvetica" || nl.Contains("gothic"))
                return Font.CreateDynamicFontFromOSFont(n, size);
        }
        // フォールバック: Unity ビルトインフォント
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    static RectTransform MakeBox(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color? col = null)
    {
        var go = new GameObject("Box");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = r.pivot = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        go.AddComponent<Image>().color = col ?? new Color(0f, 0f, 0f, 0.70f);
        return r;
    }

    static Text MakeText(Transform parent, string text, int size = 16,
        TextAnchor anchor = TextAnchor.UpperLeft)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(10, 10);
        r.offsetMax = new Vector2(-10, -10);
        var t = go.AddComponent<Text>();
        t.text      = text;
        t.fontSize  = size;
        t.font      = GetFont(size);
        t.color     = Color.white;
        t.alignment = anchor;
        t.supportRichText     = true;
        t.resizeTextForBestFit = false;
        t.horizontalOverflow  = HorizontalWrapMode.Wrap;
        t.verticalOverflow    = VerticalWrapMode.Overflow;
        return t;
    }

    // ────── 外部 API ──────────────────────────────────────────
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
