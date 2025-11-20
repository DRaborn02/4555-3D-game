using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data")]
    public PlayerDeviceData playerDeviceData;  // <-- NEW

    private int totalPlayers = 0;
    private int deadPlayers = 0;

    [Header("UI")]
    public GameObject deathScreen;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (playerDeviceData == null)
        {
            Debug.LogError("GameManager missing PlayerDeviceData reference!");
            return;
        }

        // Use number of assigned devices as the number of players
        totalPlayers = playerDeviceData.devices.Count;
        deadPlayers = 0;

        if (deathScreen != null)
            deathScreen.SetActive(false);

        Debug.Log($"GameManager initialized. Total Players = {totalPlayers}");
    }

    public void OnPlayerDied()
    {
        deadPlayers++;
        Debug.Log($"Player died. Total dead = {deadPlayers}/{totalPlayers}");

        if (deadPlayers >= totalPlayers)
        {
            ShowDeathScreen();
        }
    }

    private void ShowDeathScreen()
    {
        if (deathScreen != null)
            deathScreen.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("All players dead — showing death screen");
    }

    // UI Buttons
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
