using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainPanel;
    public string nextSceneName;
    public Button changeSceneButton;
    public Button settingsButton;
    public Button exitGameButton;
    public Button restartButton;

    [Header("Settings Menu")]
    public GameObject settingPanel;
    public Button controlSettingsButton;
    public Button audioSettingsButton;
    public Button graphicsSettingsButton;
    public Button backToMainMenuButton;

    [Header("Settings Panels")]
    public GameObject controlPanel;
    public GameObject audioPanel;
    public GameObject graphicsPanel;

    [Header("Start BlackScreen")]
    public CanvasGroup blackScreenCanvasGroup;
    public float fadeDuration = 2f;

    private void Start()
    {
        InitializeNavigationLogic();
        ShowMenu();

        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    private void InitializeNavigationLogic()
    {
        // Main menu
        if (changeSceneButton != null) ChangeScene();
        if (settingsButton != null) ShowSettingMenu();
        if (exitGameButton != null) ExitGame();
        if (restartButton != null) RestartScene();

        // Settings menu
        if (controlSettingsButton != null) ShowControlMenu();
        if (audioSettingsButton != null) ShowAudioMenu();
        if (graphicsSettingsButton != null) ShowGraphicsMenu();
        if (backToMainMenuButton != null) BackToMainMenu();
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        blackScreenCanvasGroup.alpha = 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            blackScreenCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        blackScreenCanvasGroup.alpha = 0f;
        blackScreenCanvasGroup.gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        mainPanel.SetActive(true);
    }

    private void ChangeScene()
    {
        changeSceneButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(nextSceneName);
            Time.timeScale = 1;
        });
    }

    private void RestartScene()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneManager.GetActiveScene().name);
                Time.timeScale = 1;
            });
        }
    }

    private void ShowSettingMenu()
    {
        settingsButton.onClick.AddListener(() =>
        {
            mainPanel.SetActive(false);
            settingPanel.SetActive(true);
            controlPanel.SetActive(true);
        });
    }

    private void ExitGame()
    {
        exitGameButton.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    private void ShowControlMenu()
    {
        controlSettingsButton.onClick.AddListener(() =>
        {
            controlPanel.SetActive(true);
            audioPanel.SetActive(false);
            graphicsPanel.SetActive(false);
        });
    }

    private void ShowAudioMenu()
    {
        audioSettingsButton.onClick.AddListener(() =>
        {
            controlPanel.SetActive(false);
            audioPanel.SetActive(true);
            graphicsPanel.SetActive(false);
        });
    }

    private void ShowGraphicsMenu()
    {
        graphicsSettingsButton.onClick.AddListener(() =>
        {
            controlPanel.SetActive(false);
            audioPanel.SetActive(false);
            graphicsPanel.SetActive(true);
        });
    }

    private void BackToMainMenu()
    {
        backToMainMenuButton.onClick.AddListener(() =>
        {
            mainPanel.SetActive(true);
            settingPanel.SetActive(false);
            controlPanel.SetActive(false);
            audioPanel.SetActive(false);
            graphicsPanel.SetActive(false);
        });
    }
}