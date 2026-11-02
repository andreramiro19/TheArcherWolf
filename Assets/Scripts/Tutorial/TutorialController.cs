using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialController : MonoBehaviour
{
    private class PageVideoState
    {
        public VideoPlayer player;
        public RenderTexture renderTexture;
        public bool prepared;
    }

    [Header("Data")]
    [SerializeField] private TutorialSequence sequence;

    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Video")]
    [Tooltip("Each page gets its own VideoPlayer + RenderTexture, created once and reused, so switching pages never tears down/recreates a decode session (that repeated Stop+Prepare churn was crashing the GPU driver).")]
    [SerializeField] private int videoTextureWidth = 960;
    [SerializeField] private int videoTextureHeight = 540;

    [Header("Behaviour")]
    [Tooltip("Show automatically as soon as this object starts. Leave off when something else (e.g. CinematicSequence's onCinematicComplete event) triggers ShowDelayed() instead.")]
    [SerializeField] private bool showOnStart = false;

    [Tooltip("Seconds to wait after ShowDelayed() is called before the tutorial actually pauses and appears.")]
    [SerializeField] private float delayBeforeShow = 2f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Audio Ducking")]
    [Tooltip("Game audio volume multiplier while the tutorial is open (0.4 = 60% quieter).")]
    [SerializeField, Range(0f, 1f)] private float duckedVolumeMultiplier = 0.4f;

    [Header("Events")]
    [Tooltip("Fired as soon as the tutorial appears on screen.")]
    [SerializeField] private UnityEvent onShown;
    [Tooltip("Fired once the tutorial has fully closed (fade-out done, game unpaused).")]
    [SerializeField] private UnityEvent onClosed;

    private int currentIndex;
    private bool isOpen;
    private Coroutine fadeRoutine;
    private float previousListenerVolume = 1f;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;

    private readonly Dictionary<TutorialPage, PageVideoState> videoStates = new Dictionary<TutorialPage, PageVideoState>();
    private PageVideoState currentVideoState;

    private void Awake()
    {
        if (previousButton) previousButton.onClick.AddListener(Previous);
        if (nextButton) nextButton.onClick.AddListener(Next);
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        if (showOnStart)
        {
            Show();
        }
        else
        {
            HideImmediate();
        }
    }

    private void OnDestroy()
    {
        foreach (var state in videoStates.Values)
        {
            if (state.renderTexture)
            {
                state.renderTexture.Release();
                Destroy(state.renderTexture);
            }
        }

        videoStates.Clear();
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Previous();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Next();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void ShowDelayed()
    {
        StartCoroutine(ShowDelayedRoutine());
    }

    private IEnumerator ShowDelayedRoutine()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        Show();
    }

    public void Show()
    {
        if (sequence == null || sequence.pages == null || sequence.pages.Length == 0)
        {
            Debug.LogWarning("[Tutorial] No TutorialSequence/pages assigned.", this);
            return;
        }

        isOpen = true;
        panelRoot.SetActive(true);
        Time.timeScale = 0f;

        previousListenerVolume = AudioListener.volume;
        AudioListener.volume = previousListenerVolume * duckedVolumeMultiplier;

        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentIndex = 0;
        DisplayPage(currentIndex);

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadePanel(0f, 1f));

        onShown?.Invoke();
    }

    public void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        PauseCurrentVideo();

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        yield return FadePanel(panelCanvasGroup.alpha, 0f);

        panelRoot.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.volume = previousListenerVolume;
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockState;

        onClosed?.Invoke();
    }

    private IEnumerator FadePanel(float from, float to)
    {
        float t = 0f;
        panelCanvasGroup.alpha = from;

        while (t < fadeDuration)
        {
            // Unscaled: the panel must still animate while Time.timeScale is 0.
            t += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = to;
    }

    private void ShowPageVideo(TutorialPage page)
    {
        PauseCurrentVideo();

        if (!videoDisplay || page.video == null)
        {
            if (videoDisplay) videoDisplay.texture = null;
            currentVideoState = null;
            return;
        }

        if (!videoStates.TryGetValue(page, out PageVideoState state))
        {
            state = CreateVideoState(page);
            videoStates[page] = state;
        }

        currentVideoState = state;
        videoDisplay.texture = state.renderTexture;

        if (state.prepared)
        {
            state.player.Play();
        }
        // If not prepared yet, the prepareCompleted callback below will Play()
        // it automatically once ready, but only if it's still the current page.
    }

    private PageVideoState CreateVideoState(TutorialPage page)
    {
        var rt = new RenderTexture(videoTextureWidth, videoTextureHeight, 0)
        {
            name = $"TutorialVideoRT_{page.name}"
        };

        var go = new GameObject($"TutorialVideoPlayer_{page.name}");
        go.transform.SetParent(transform, false);

        var player = go.AddComponent<VideoPlayer>();
        player.playOnAwake = false;
        player.isLooping = true;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = rt;
        player.clip = page.video;

        var state = new PageVideoState
        {
            player = player,
            renderTexture = rt,
            prepared = false
        };

        player.prepareCompleted += _ =>
        {
            state.prepared = true;

            // Only auto-play if the user is still looking at this page
            // (they may have already navigated away while it was preparing).
            if (currentVideoState == state)
            {
                player.Play();
            }
        };

        player.Prepare();

        return state;
    }

    private void PauseCurrentVideo()
    {
        if (currentVideoState != null && currentVideoState.player)
        {
            currentVideoState.player.Pause();
        }
    }

    public void Next()
    {
        if (currentIndex >= sequence.pages.Length - 1)
        {
            return;
        }

        currentIndex++;
        DisplayPage(currentIndex);
    }

    public void Previous()
    {
        if (currentIndex <= 0)
        {
            return;
        }

        currentIndex--;
        DisplayPage(currentIndex);
    }

    private void DisplayPage(int index)
    {
        TutorialPage page = sequence.pages[index];

        if (descriptionText)
        {
            descriptionText.text = page.description;
        }

        if (pageCounterText)
        {
            pageCounterText.text = $"{index + 1} / {sequence.pages.Length}";
        }

        ShowPageVideo(page);

        if (previousButton)
        {
            previousButton.gameObject.SetActive(index > 0);
        }

        if (nextButton)
        {
            nextButton.gameObject.SetActive(index < sequence.pages.Length - 1);
        }
    }

    private void HideImmediate()
    {
        isOpen = false;
        panelRoot.SetActive(false);

        if (panelCanvasGroup)
        {
            panelCanvasGroup.alpha = 0f;
        }
    }
}
