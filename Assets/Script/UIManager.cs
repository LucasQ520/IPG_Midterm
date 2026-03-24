using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text reactorHealthText;

    [Header("Cooldowns")]
    public Image shockwaveFill;
    public Image arrowFill;
    public Image burstFill;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void UpdateReactorHealth(int current, int max)
    {
        if (reactorHealthText != null)
        {
            reactorHealthText.text = "Reactor: " + current + " / " + max;
        }
    }

    public void UpdateCooldowns(PlayerController player)
    {
        if (player == null) return;

        if (shockwaveFill != null)
        {
            float value = 1f - Mathf.Clamp01(player.ShockwaveTimer / player.ShockwaveCooldown);
            shockwaveFill.fillAmount = value;
        }

        if (arrowFill != null)
        {
            float value = 1f - Mathf.Clamp01(player.ArrowTimer / player.ArrowCooldown);
            arrowFill.fillAmount = value;
        }

        if (burstFill != null)
        {
            float value = 1f - Mathf.Clamp01(player.BurstTimer / player.BurstCooldown);
            burstFill.fillAmount = value;
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }
    }
}