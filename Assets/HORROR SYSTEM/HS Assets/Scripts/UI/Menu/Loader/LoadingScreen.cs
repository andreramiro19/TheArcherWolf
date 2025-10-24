using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float minShowDuration = 1.5f;
    [SerializeField] private float progressSpeed = 0.5f;

    private Coroutine fadeRoutine;
    private Coroutine progressRoutine;
    private float targetProgress;
    private bool isLoading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideImmediate();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        progressSlider.value = 0f;
        targetProgress = 0f;
        isLoading = true;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(UpdateProgressBar());
    }

    public void Hide()
    {
        isLoading = false;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(DelayedHide());
    }

    private IEnumerator UpdateProgressBar()
    {
        float startTime = Time.time;
        float elapsed = 0f;

        while (isLoading || elapsed < minShowDuration)
        {
            elapsed = Time.time - startTime;
            float normalizedTime = Mathf.Clamp01(elapsed / minShowDuration);

            float currentProgress = Mathf.Lerp(0f, targetProgress, normalizedTime);
            progressSlider.value = Mathf.Min(currentProgress, targetProgress);

            yield return null;
        }

        while (progressSlider.value < 1f)
        {
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, 1f, progressSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator DelayedHide()
    {
        while (progressSlider.value < 1f)
        {
            yield return null;
        }

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void UpdateProgress(float progress)
    {
        targetProgress = progress;
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}