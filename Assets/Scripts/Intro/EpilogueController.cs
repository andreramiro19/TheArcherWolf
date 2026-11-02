using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a StoryPage/StoryPageSequence the same way StoryIntroController does,
/// but for an ending scene (e.g. Prologo1): Skip jumps to the last page instead
/// of leaving the scene, the last page also types out a FinalText line, and the
/// scene ends by swapping the Skip button for a Menu button rather than
/// auto-loading another scene.
/// </summary>
public class EpilogueController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StoryPageSequence sequence;

    [Header("References")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private TMP_Text finalText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private AudioSource narrationSource;

    [Header("Skip / Menu Buttons")]
    [SerializeField] private Button skipButton;
    [SerializeField] private CanvasGroup skipButtonGroup;
    [SerializeField] private Button menuButton;
    [SerializeField] private CanvasGroup menuButtonGroup;
    [SerializeField] private float buttonFadeDuration = 0.5f;

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 30f;
    [SerializeField] private float delayBeforeText = 1f;
    [SerializeField] private float delayAfterLine = 2f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Movement (Ken Burns)")]
    [Tooltip("Starting zoom used for Left/Right/Up/Down/ZoomOut so panning doesn't reveal empty edges.")]
    [SerializeField] private float panOverscan = 1.15f;

    [Header("Final Text")]
    [TextArea(2, 6)]
    [SerializeField] private string finalMessage = "Fim.";

    [Header("Menu Scene")]
    [SerializeField] private string menuSceneName = "GothicPalace_OverGrown_URP";

    private int currentPageIndex;
    private Coroutine playRoutine;
    private Coroutine panRoutine;
    private Coroutine narrationRoutine;
    private bool skipRequested;

    private void Awake()
    {
        if (skipButton) skipButton.onClick.AddListener(SkipToEnd);
        if (menuButton) menuButton.onClick.AddListener(GoToMenu);

        SetGroupImmediate(skipButtonGroup, 1f);
        SetGroupImmediate(menuButtonGroup, 0f);

        if (finalText)
        {
            finalText.text = string.Empty;
            finalText.maxVisibleCharacters = 0;
        }
    }

    private void Start()
    {
        // Cursor.lockState persists across scene loads — Gameplay leaves it
        // Locked/hidden for camera control, which would otherwise make the
        // Menu button unclickable here.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playRoutine = StartCoroutine(PlaySequence(0, sequence.pages.Length - 1));
    }

    private IEnumerator PlaySequence(int fromIndex, int toIndex)
    {
        SetFadeAlpha(1f);

        for (currentPageIndex = fromIndex; currentPageIndex <= toIndex; currentPageIndex++)
        {
            StoryPage page = sequence.pages[currentPageIndex];
            bool isLastPage = currentPageIndex == sequence.pages.Length - 1;

            SetupPage(page);

            yield return Fade(1f, 0f);
            if (skipRequested) yield break;

            StartNarration(page);

            yield return WaitSkippable(delayBeforeText);
            if (skipRequested) yield break;

            yield return TypeText(storyText, page.text, SpeedFor(page));
            if (skipRequested) yield break;

            if (isLastPage)
            {
                yield return EndOnLastPage();
                yield break;
            }

            float delay = page.delayAfterLineOverride >= 0f ? page.delayAfterLineOverride : delayAfterLine;
            yield return WaitSkippable(delay);
            if (skipRequested) yield break;

            yield return Fade(0f, 1f);
        }
    }

    private IEnumerator EndOnLastPage()
    {
        if (finalText)
        {
            yield return TypeText(finalText, finalMessage, charsPerSecond);
        }

        yield return CrossfadeToMenuButton();
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

    private float SpeedFor(StoryPage page)
    {
        return page.charsPerSecondOverride > 0f ? page.charsPerSecondOverride : charsPerSecond;
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

    private IEnumerator TypeText(TMP_Text target, string content, float speed)
    {
        target.text = content;
        target.maxVisibleCharacters = 0;
        target.ForceMeshUpdate();

        int totalChars = target.textInfo.characterCount;

        int visible = 0;
        while (visible < totalChars)
        {
            if (skipRequested)
            {
                yield break;
            }

            visible++;
            target.maxVisibleCharacters = visible;
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

    // Wired to the Skip button's onClick.
    public void SkipToEnd()
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

        StartCoroutine(SkipToLastPageRoutine());
    }

    private IEnumerator SkipToLastPageRoutine()
    {
        yield return Fade(fadeImage.color.a, 1f);

        // Reset so the last-page-only run below can actually play through
        // instead of bailing out immediately.
        skipRequested = false;

        playRoutine = StartCoroutine(PlaySequence(sequence.pages.Length - 1, sequence.pages.Length - 1));
    }

    private IEnumerator CrossfadeToMenuButton()
    {
        float t = 0f;
        float skipStart = skipButtonGroup ? skipButtonGroup.alpha : 0f;
        float menuStart = menuButtonGroup ? menuButtonGroup.alpha : 0f;

        while (t < buttonFadeDuration)
        {
            t += Time.deltaTime;
            float p = t / buttonFadeDuration;

            if (skipButtonGroup) skipButtonGroup.alpha = Mathf.Lerp(skipStart, 0f, p);
            if (menuButtonGroup) menuButtonGroup.alpha = Mathf.Lerp(menuStart, 1f, p);

            yield return null;
        }

        SetGroupImmediate(skipButtonGroup, 0f);
        SetGroupImmediate(menuButtonGroup, 1f);
    }

    private void SetGroupImmediate(CanvasGroup group, float alpha)
    {
        if (!group)
        {
            return;
        }

        group.alpha = alpha;
        group.interactable = alpha > 0.5f;
        group.blocksRaycasts = alpha > 0.5f;
    }

    // Wired to the Menu button's onClick.
    public void GoToMenu()
    {
        SceneLoader.LoadScene(menuSceneName);
    }
}
