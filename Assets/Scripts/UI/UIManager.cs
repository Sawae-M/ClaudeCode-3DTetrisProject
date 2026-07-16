using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Canvas・HUD・操作説明をすべてコードで自動生成する
public class UIManager : MonoBehaviour
{
    // 外部から参照可能（GameManager等から呼ぶ）
    TextMeshProUGUI scoreText;
    TextMeshProUGUI levelText;
    TextMeshProUGUI nextPieceText;
    GameObject      gameOverPanel;
    TextMeshProUGUI finalScoreText;

    void Awake()
    {
        BuildUI();
    }

    // ────────────────────────────────────────
    //  UI 自動生成
    // ────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── スコア・レベル（右上） ──
        var hudPanel = MakePanel(canvas.transform, new Vector2(1,1), new Vector2(1,1),
                                 new Vector2(-160, -10), new Vector2(150, 80));
        scoreText = MakeLabel(hudPanel, "SCORE\n0",    new Vector2(0,1), new Vector2(0,1),
                              new Vector2(5,  -5), new Vector2(140, 35));
        levelText = MakeLabel(hudPanel, "LEVEL\n1",    new Vector2(0,0), new Vector2(0,0),
                              new Vector2(5,   5), new Vector2(140, 35));

        // ── NEXT（左上） ──
        var nextPanel = MakePanel(canvas.transform, new Vector2(0,1), new Vector2(0,1),
                                  new Vector2(10, -10), new Vector2(130, 50));
        nextPieceText = MakeLabel(nextPanel, "NEXT\n-", new Vector2(0,1), new Vector2(1,0),
                                  new Vector2(5, -5), new Vector2(-5, 5));

        // ── 操作説明（左下） ──
        var ctrlPanel = MakePanel(canvas.transform, new Vector2(0,0), new Vector2(0,0),
                                  new Vector2(10, 10), new Vector2(220, 210));
        MakeLabel(ctrlPanel,
            "<b>[ ピース操作 ]</b>\n" +
            "← → ↑ ↓  移動（視点基準）\n" +
            "Space        落下加速\n\n" +
            "<b>[ キューブ回転 ]</b>\n" +
            "Q / E   Z軸回転\n" +
            "Z / X   X軸回転\n" +
            "C / V   Y軸回転\n\n" +
            "<b>[ カメラ視点 ]</b>\n" +
            "W / A / S / D  視点移動",
            new Vector2(0,1), new Vector2(1,0),
            new Vector2(8, -8), new Vector2(-8, 8), fontSize: 11);

        // ── ゲームオーバー（中央） ──
        gameOverPanel = MakePanel(canvas.transform, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                                  new Vector2(-150, -80), new Vector2(300, 160)).gameObject;
        finalScoreText = MakeLabel(gameOverPanel.transform, "GAME OVER\nSCORE: 0",
                                   new Vector2(0,1), new Vector2(1,0),
                                   new Vector2(10,-10), new Vector2(-10,10), fontSize: 20);
        // Restart ボタン
        var btnGO   = new GameObject("RestartButton");
        btnGO.transform.SetParent(gameOverPanel.transform, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin        = new Vector2(0.5f, 0);
        btnRect.anchorMax        = new Vector2(0.5f, 0);
        btnRect.pivot            = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 10);
        btnRect.sizeDelta        = new Vector2(160, 40);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => GameManager.Instance.Restart());
        var btnLabel = MakeLabel(btnGO.transform, "RESTART",
                                 Vector2.zero, Vector2.one,
                                 Vector2.zero, Vector2.zero, fontSize: 18);
        btnLabel.alignment = TextAlignmentOptions.Center;

        gameOverPanel.SetActive(false);
    }

    // ────────────────────────────────────────
    //  ヘルパー
    // ────────────────────────────────────────
    static RectTransform MakePanel(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 sizeDelta,
        Color? color = null)
    {
        var go   = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin        = anchorMin;
        rect.anchorMax        = anchorMax;
        rect.pivot            = anchorMin;
        rect.anchoredPosition = offsetMin;
        rect.sizeDelta        = sizeDelta;
        var img  = go.AddComponent<Image>();
        img.color = color ?? new Color(0, 0, 0, 0.55f);
        return rect;
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        int fontSize = 13)
    {
        var go   = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        var tmp  = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.richText  = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        return tmp;
    }

    // ────────────────────────────────────────
    //  外部から呼ぶ更新メソッド
    // ────────────────────────────────────────
    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = $"SCORE\n{score:N0}";
    }

    public void UpdateLevel(int level)
    {
        if (levelText) levelText.text = $"LEVEL\n{level}";
    }

    public void UpdateNextPiece(PieceDefinition next)
    {
        if (nextPieceText) nextPieceText.text = $"NEXT\n{next?.pieceName ?? "-"}";
    }

    public void UpdateAll()
    {
        var gm = GameManager.Instance;
        UpdateScore(gm.Score);
        UpdateLevel(gm.Level);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreText)
            finalScoreText.text = $"GAME OVER\nSCORE: {GameManager.Instance.Score:N0}";
    }
}
