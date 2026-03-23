using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SimplePauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backToMapButton;

    [Header("Button Text References")]
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private TMP_Text restartText;
    [SerializeField] private TMP_Text backToMapText;

    private bool isPaused = false;

    private void Start()
    {
        // Setup pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }

        // Setup pause panel buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (backToMapButton != null)
        {
            backToMapButton.onClick.AddListener(BackToMap);
        }

        // Set button texts (optional)
        if (resumeText != null) resumeText.text = "Continue Game";
        if (restartText != null) restartText.text = "Restart";
        if (backToMapText != null) backToMapText.text = "Back to map";

        // Hide pause panel initially
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f; // Reset timescale
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void BackToMap()
    {
        Time.timeScale = 1f; // Reset timescale
        // Load level select scene - change "LevelSelect" to your actual scene name
        SceneManager.LoadScene("LevelSelect");
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (backToMapButton != null)
        {
            backToMapButton.onClick.RemoveListener(BackToMap);
        }
    }

    // For external control
    public void ShowPausePanel()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HidePausePanel()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

