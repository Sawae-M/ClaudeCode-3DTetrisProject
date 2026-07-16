using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ゲーム画面上に操作説明を常時表示するコンポーネント
// Canvas > Panel > ControlsUI にアタッチして使う
public class ControlsUI : MonoBehaviour
{
    [Header("UI References (任意: 未設定時は自動生成)")]
    public TextMeshProUGUI controlsText;

    static readonly string ControlsContent =
        "<b>[ ピース操作 ]</b>\n" +
        "← → ↑ ↓  移動（視点基準）\n" +
        "Space        落下加速\n" +
        "\n" +
        "<b>[ キューブ回転 ]</b>\n" +
        "Q / E   Z軸回転\n" +
        "Z / X   X軸回転\n" +
        "C / V   Y軸回転\n" +
        "\n" +
        "<b>[ カメラ視点 ]</b>\n" +
        "W / A / S / D  視点移動";

    void Awake()
    {
        if (controlsText == null)
            controlsText = GetComponentInChildren<TextMeshProUGUI>();

        if (controlsText != null)
            controlsText.text = ControlsContent;
    }

    // 実行時に Canvas 上に自動生成するユーティリティ
    public static void CreateOnCanvas(Canvas canvas)
    {
        // 背景パネル
        var panelGO  = new GameObject("ControlsPanel");
        panelGO.transform.SetParent(canvas.transform, false);

        var rect = panelGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot     = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta        = new Vector2(220, 200);

        var img   = panelGO.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.55f);

        // テキスト
        var textGO = new GameObject("ControlsText");
        textGO.transform.SetParent(panelGO.transform, false);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin        = Vector2.zero;
        textRect.anchorMax        = Vector2.one;
        textRect.offsetMin        = new Vector2(8, 8);
        textRect.offsetMax        = new Vector2(-8, -8);

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = ControlsContent;
        tmp.fontSize  = 12;
        tmp.color     = Color.white;
        tmp.richText  = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        var ui = panelGO.AddComponent<ControlsUI>();
        ui.controlsText = tmp;
    }
}
