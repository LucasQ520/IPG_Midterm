using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Reactor reactor;
    public UIManager uiManager;
    public Transform reactorCenter;

    [Header("Game Settings")]
    public int startingReactorHealth = 25;

    private int reactorHealth;
    private int score;
    private bool gameOver;

    public int Score => score;
    public int ReactorHealth => reactorHealth;
    public bool IsGameOver => gameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Time.timeScale = 1f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        reactorHealth = startingReactorHealth;
        score = 0;
        gameOver = false;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (gameOver) return;
        score += amount;
        UpdateUI();
    }

    public void DamageReactor(int amount)
    {
        if (gameOver) return;

        reactorHealth -= amount;
        if (reactorHealth < 0) reactorHealth = 0;

        if (reactor != null)
        {
            reactor.PlayHitFeedback();
        }

        UpdateUI();

        if (reactorHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Time.timeScale = 0f;

        if (uiManager != null)
        {
            uiManager.ShowGameOver(score);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateReactorHealth(reactorHealth, startingReactorHealth);
        }
    }
}