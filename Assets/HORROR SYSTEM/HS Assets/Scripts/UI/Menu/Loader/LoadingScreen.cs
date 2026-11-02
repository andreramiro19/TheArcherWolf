using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Slider progressSlider;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;
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
        PickRandomBackground();
        progressSlider.value = 0f;
        targetProgress = 0f;
        isLoading = true;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        // Assumes the caller already faded its own scene to black; this just
        // reveals the loading content from that black, same as the Intro.
        SetFadeAlpha(1f);
        fadeRoutine = StartCoroutine(Fade(1f, 0f));

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
        float startTime = Time.unscaledTime;
        float elapsed = 0f;

        while (isLoading || elapsed < minShowDuration)
        {
            elapsed = Time.unscaledTime - startTime;
            float normalizedTime = Mathf.Clamp01(elapsed / minShowDuration);

            float currentProgress = Mathf.Lerp(0f, targetProgress, normalizedTime);
            progressSlider.value = Mathf.Min(currentProgress, targetProgress);

            yield return null;
        }

        while (progressSlider.value < 1f)
        {
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, 1f, progressSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    private IEnumerator DelayedHide()
    {
        while (progressSlider.value < 1f)
        {
            yield return null;
        }

        // Cover the loading content in black before the caller reveals the next scene.
        yield return Fade(0f, 1f);

        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        SetFadeAlpha(from);

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private void PickRandomBackground()
    {
        if (backgroundImage && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            backgroundImage.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
        }
    }

    public void UpdateProgress(float progress)
    {
        targetProgress = progress;
    }

    private void HideImmediate()
    {
        SetFadeAlpha(0f);
        gameObject.SetActive(false);
    }
}
