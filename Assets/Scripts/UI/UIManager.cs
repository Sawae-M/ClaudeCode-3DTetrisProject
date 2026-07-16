using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    TextMeshProUGUI scoreText;
    TextMeshProUGUI levelText;
    TextMeshProUGUI nextPieceText;
    GameObject      gameOverPanel;
    TextMeshProUGUI finalScoreText;

    void Awake() => BuildUI();

    void BuildUI()
    {
        // ── Canvas ──────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        Transform ct = canvas.transform;

        // ── スコア（右上） ───────────────────────
        var scorePanel = Box(ct, new Vector2(1, 1), new Vector2(-10, -10), new Vector2(160, 70));
        scoreText = Label(scorePanel, "SCORE\n0", 16);

        // ── レベル（右上・スコアの下） ─────────
        var levelPanel = Box(ct, new Vector2(1, 1), new Vector2(-10, -90), new Vector2(160, 50));
        levelText = Label(levelPanel, "LEVEL  1", 16);

        // ── NEXT（左上） ─────────────────────────
        var nextPanel = Box(ct, new Vector2(0, 1), new Vector2(10, -10), new Vector2(160, 50));
        nextPieceText = Label(nextPanel, "NEXT  -", 16);

        // ── 操作説明（左下） ─────────────────────
        var ctrlPanel = Box(ct, new Vector2(0, 0), new Vector2(10, 10), new Vector2(230, 215));
        Label(ctrlPanel,
            "<b>─ ピース操作 ─</b>\n" +
            "← → ↑ ↓    移動（視点基準）\n" +
            "Space          落下加速\n\n" +
            "<b>─ カメラ視点 ─</b>\n" +
            "W / S    上下の面へ移動\n" +
            "A / D    左右の面へ移動\n\n" +
            "<b>─ ヒント ─</b>\n" +
            "視点を変えると落下方向も変わる",
            12);

        // ── ゲームオーバー（中央） ───────────────
        gameOverPanel = Box(ct, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(340, 200)).gameObject;
        finalScoreText = Label(gameOverPanel.transform, "GAME OVER\nSCORE: 0", 22);

        var btnGO   = new GameObject("Restart");
        btnGO.transform.SetParent(gameOverPanel.transform, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        // ゲームオーバーパネル下部に配置
        btnRect.anchorMin        = new Vector2(0.5f, 0);
        btnRect.anchorMax        = new Vector2(0.5f, 0);
        btnRect.pivot            = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 12);
        btnRect.sizeDelta        = new Vector2(180, 44);
        var btnImg  = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.55f, 0.15f);
        var btn  = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => GameManager.Instance.Restart());
        var btnLbl = Label(btnGO.transform, "RESTART", 18);
        btnLbl.alignment = TextAlignmentOptions.Center;

        gameOverPanel.SetActive(false);
    }

    // ── ヘルパー ────────────────────────────────────────
    // 固定サイズのパネルを作る。pivot は anchor と同じ（コーナー基準）
    static RectTransform Box(Transform parent,
        Vector2 anchor, Vector2 pos, Vector2 size,
        Color? col = null)
    {
        var go   = new GameObject("Box");
        go.transform.SetParent(parent, false);
        var r    = go.AddComponent<RectTransform>();
        r.anchorMin        = anchor;
        r.anchorMax        = anchor;
        r.pivot            = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        var img  = go.AddComponent<Image>();
        img.color = col ?? new Color(0f, 0f, 0f, 0.6f);
        return r;
    }

    // パネルを全面に覆うラベル（パディング 8px）
    static TextMeshProUGUI Label(Transform parent, string text, int size = 14)
    {
        var go  = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var r   = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(8,  8);
        r.offsetMax = new Vector2(-8, -8);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.richText  = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        return tmp;
    }

    // ── 外部 API ────────────────────────────────────────
    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = $"SCORE\n{score:N0}";
    }

    public void UpdateLevel(int level)
    {
        if (levelText) levelText.text = $"LEVEL  {level}";
    }

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
        if (finalScoreText)
            finalScoreText.text = $"GAME OVER\nSCORE: {GameManager.Instance.Score:N0}";
    }

    public void HideGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }
}
