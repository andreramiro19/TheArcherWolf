using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryIntroController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StoryPageSequence sequence;

    [Header("References")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Button skipButton;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 30f;
    [SerializeField] private float delayBeforeText = 1f;
    [SerializeField] private float delayAfterLine = 2f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Movement (Ken Burns)")]
    [Tooltip("Starting zoom used for Left/Right/Up/Down/ZoomOut so panning doesn't reveal empty edges.")]
    [SerializeField] private float panOverscan = 1.15f;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    private int currentPageIndex;
    private Coroutine playRoutine;
    private Coroutine panRoutine;
    private Coroutine narrationRoutine;
    private bool skipRequested;

    private void Awake()
    {
        if (skipButton)
        {
            skipButton.onClick.AddListener(SkipIntro);
        }
    }

    private void Start()
    {
        playRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        SetFadeAlpha(1f);

        for (currentPageIndex = 0; currentPageIndex < sequence.pages.Length; currentPageIndex++)
        {
            StoryPage page = sequence.pages[currentPageIndex];

            // Sprite/text swap happens while the screen is fully black.
            SetupPage(page);

            yield return Fade(1f, 0f);
            if (skipRequested) break;

            // Narration runs on its own independent, per-page delay, decoupled
            // from the text's delay/typing timeline.
            StartNarration(page);

            yield return WaitSkippable(delayBeforeText);
            if (skipRequested) break;

            yield return TypeText(page);
            if (skipRequested) break;

            float delay = page.delayAfterLineOverride >= 0f ? page.delayAfterLineOverride : delayAfterLine;
            yield return WaitSkippable(delay);
            if (skipRequested) break;

            yield return Fade(0f, 1f);
        }

        StopPan();
        StopNarration();

        yield return FadeToBlackWithMusic();
        LoadGameplay();
    }

    private void SetupPage(StoryPage page)
    {
        illustrationImage.sprite = page.illustration;
        storyText.text = string.Empty;
        storyText.maxVisibleCharacters = 0;

        illustrationImage.rectTransform.anchoredPosition = Vector2.zero;
        illustrationImage.rectTransform.localScale = Vector3.one * StartScaleFor(page.panDirection);

        StopPan();
        panRoutine = StartCoroutine(AnimatePan(page));

        StopNarration();
    }

    private float StartScaleFor(StoryPage.PagePanDirection direction)
    {
        return direction == StoryPage.PagePanDirection.None || direction == StoryPage.PagePanDirection.ZoomIn
            ? 1f
            : panOverscan;
    }

    private IEnumerator AnimatePan(StoryPage page)
    {
        RectTransform rt = illustrationImage.rectTransform;

        while (true)
        {
            float dt = Time.deltaTime;

            switch (page.panDirection)
            {
                case StoryPage.PagePanDirection.Left:
                    rt.anchoredPosition += new Vector2(-page.panSpeed * dt, 0f);
                    break;
                case StoryPage.PagePanDirection.Right:
                    rt.anchoredPosition += new Vector2(page.panSpeed * dt, 0f);
                    break;
                case StoryPage.PagePanDirection.Up:
                    rt.anchoredPosition += new Vector2(0f, page.panSpeed * dt);
                    break;
                case StoryPage.PagePanDirection.Down:
                    rt.anchoredPosition += new Vector2(0f, -page.panSpeed * dt);
                    break;
                case StoryPage.PagePanDirection.ZoomIn:
                    rt.localScale += Vector3.one * (page.panSpeed * dt);
                    break;
                case StoryPage.PagePanDirection.ZoomOut:
                    rt.localScale -= Vector3.one * (page.panSpeed * dt);
                    break;
            }

            yield return null;
        }
    }

    private void StopPan()
    {
        if (panRoutine != null)
        {
            StopCoroutine(panRoutine);
            panRoutine = null;
        }
    }

    private void StartNarration(StoryPage page)
    {
        StopNarration();

        if (!narrationSource || page.narration == null)
        {
            return;
        }

        narrationRoutine = StartCoroutine(PlayNarrationDelayed(page));
    }

    private IEnumerator PlayNarrationDelayed(StoryPage page)
    {
        if (page.narrationDelay > 0f)
        {
            yield return WaitSkippable(page.narrationDelay);
            if (skipRequested)
            {
                yield break;
            }
        }

        narrationSource.clip = page.narration;
        narrationSource.Play();
    }

    private void StopNarration()
    {
        if (narrationRoutine != null)
        {
            StopCoroutine(narrationRoutine);
            narrationRoutine = null;
        }

        if (narrationSource)
        {
            narrationSource.Stop();
        }
    }

    private IEnumerator TypeText(StoryPage page)
    {
        storyText.text = page.text;
        storyText.maxVisibleCharacters = 0;
        storyText.ForceMeshUpdate();

        int totalChars = storyText.textInfo.characterCount;
        float speed = page.charsPerSecondOverride > 0f ? page.charsPerSecondOverride : charsPerSecond;

        int visible = 0;
        while (visible < totalChars)
        {
            if (skipRequested)
            {
                yield break;
            }

            visible++;
            storyText.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(1f / speed);
        }
    }

    private IEnumerator WaitSkippable(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (skipRequested)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        SetFadeAlpha(from);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
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

    public void SkipIntro()
    {
        if (skipRequested)
        {
            return;
        }

        skipRequested = true;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        StopPan();
        StopNarration();

        StartCoroutine(SkipRoutine());
    }

    private IEnumerator SkipRoutine()
    {
        yield return FadeToBlackWithMusic();
        LoadGameplay();
    }

    private IEnumerator FadeToBlackWithMusic()
    {
        float startAlpha = fadeImage.color.a;
        float startVolume = musicSource ? musicSource.volume : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = t / fadeDuration;
            SetFadeAlpha(Mathf.Lerp(startAlpha, 1f, p));

            if (musicSource)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, p);
            }

            yield return null;
        }

        SetFadeAlpha(1f);

        if (musicSource)
        {
            musicSource.volume = 0f;
        }
    }

    private void LoadGameplay()
    {
        SceneLoader.LoadScene(gameplaySceneName);
    }
}
