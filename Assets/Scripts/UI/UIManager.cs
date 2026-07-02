using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI nextPieceText;
    public TextMeshProUGUI gravityText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.Instance.Restart());
    }

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

    public void UpdateGravity(GravityDirection dir)
    {
        if (gravityText) gravityText.text = $"GRAVITY\n{dir}";
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
        if (finalScoreText) finalScoreText.text = $"SCORE: {GameManager.Instance.Score:N0}";
    }
}
