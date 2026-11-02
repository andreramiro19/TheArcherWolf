using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Invector.vCharacterController;

public class Chapter1Sequence : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private vThirdPersonInput playerInput;
    [SerializeField] private vThirdPersonController playerCharacter;
    [Tooltip("Used to suppress the 'You Died' HUD text for this one scripted, intentional death.")]
    [SerializeField] private vHUDController hudController;

    [Header("Fade (shared with the opening CinematicSequence)")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Waves")]
    [SerializeField] private EnemyGroupMonitor wave1;
    [SerializeField] private EnemyGroupMonitor wave2;

    [Header("Ambush Cinematic")]
    [Tooltip("The Cinemachine camera rig GameObject used for cinematic shots (same one the opening CinematicSequence uses). Its output Camera must have a higher Depth than the player camera so it visually covers it while active — the player camera itself is never disabled, so Camera.main stays valid for vLockOn/vHeadTrack.")]
    [SerializeField] private GameObject cinematicCameraRigObject;
    [Tooltip("GameObject holding a CinematicSequence component configured for the ambush reveal shot. Kept inactive until the ambush starts. Its Object To Disable should be the cinematic camera rig itself; Object To Enable should be left empty. Its onCinematicComplete should call this component's OnBattleResumed().")]
    [SerializeField] private GameObject ambushCinematicSequenceObject;

    [Header("Music")]
    [Tooltip("Its own volume (e.g. 50) is already set correctly in the Inspector — this script only calls Play() at the right moment.")]
    [SerializeField] private AudioSource battleMusic;

    [Header("Death")]
    [SerializeField] private string deathSceneName = "Prologo1";
    [Tooltip("How long the fade-to-black takes on death (separate from the regular cinematic fade duration).")]
    [SerializeField] private float deathFadeDuration = 3f;
    [Tooltip("How long the screen stays fully black after fading out, before the Prologo1 scene starts loading.")]
    [SerializeField] private float deathBlackHoldDuration = 2f;

    private void Start()
    {
        // Locked immediately; only released once the tutorial closes
        // (OnTutorialClosed, wired to TutorialController's onClosed event).
        // The player object is still inactive at this point (the opening
        // CinematicSequence only enables it at the very end), so its own
        // Awake/Start haven't run yet — StopCharacter() would NullRef against
        // its not-yet-initialized Rigidbody. There's nothing to stop yet
        // anyway (game just loaded), so skip that part here.
        LockPlayer(true, stopMovement: false);

        if (playerCharacter)
        {
            playerCharacter.onDead.AddListener(OnPlayerDied);
        }
    }

    private void LockPlayer(bool locked, bool stopMovement = true)
    {
        if (playerInput)
        {
            playerInput.SetLockAllInput(locked);
            playerInput.SetLockCameraInput(locked);
        }

        // Locking input only stops new input from being read — it doesn't
        // cancel whatever movement was already in progress (e.g. the player
        // was mid-run), so explicitly halt the character too.
        if (locked && stopMovement && playerCharacter)
        {
            playerCharacter.StopCharacter();
        }
    }

    // Wired to TutorialController's "On Shown" event.
    public void OnTutorialShown()
    {
        Debug.Log("[Chapter1Sequence] OnTutorialShown() called.", this);

        if (!battleMusic)
        {
            Debug.LogWarning("[Chapter1Sequence] Battle Music is not assigned — nothing to play.", this);
            return;
        }

        if (battleMusic.isPlaying)
        {
            Debug.Log("[Chapter1Sequence] Battle Music is already playing, skipping Play().", this);
            return;
        }

        Debug.Log($"[Chapter1Sequence] Playing Battle Music ('{battleMusic.clip}').", this);
        battleMusic.Play();
    }

    // Wired to TutorialController's "On Closed" event.
    public void OnTutorialClosed()
    {
        LockPlayer(false);

        if (wave1)
        {
            wave1.Activate();
        }
    }

    // Wired to Wave1's EnemyGroupMonitor "On Group Defeated" event.
    public void OnWave1Defeated()
    {
        StartCoroutine(AmbushSequence());
    }

    private IEnumerator AmbushSequence()
    {
        LockPlayer(true);

        // This ambush is designed to kill the player — suppress the normal
        // "You Died" HUD text just for this one intentional, scripted death.
        if (hudController)
        {
            hudController.deathText = string.Empty;
        }

        yield return Fade(0f, 1f);

        // The player camera is never disabled (breaks Camera.main-dependent
        // Invector scripts like vLockOn/vHeadTrack) — the cinematic camera
        // just needs to be active with a higher Depth to visually cover it.
        if (cinematicCameraRigObject) cinematicCameraRigObject.SetActive(true);

        // Force-target the player so they charge in immediately regardless of
        // distance, instead of waiting for their own proximity detection.
        Transform target = playerCharacter ? playerCharacter.transform : null;
        if (wave2) wave2.Activate(target);

        if (ambushCinematicSequenceObject)
        {
            ambushCinematicSequenceObject.SetActive(true);
        }
        else
        {
            // No ambush cinematic configured — just reveal and resume immediately.
            yield return Fade(1f, 0f);
            OnBattleResumed();
        }
    }

    // Wired to the ambush CinematicSequence's "On Cinematic Complete" event.
    public void OnBattleResumed()
    {
        LockPlayer(false);
    }

    private void OnPlayerDied(GameObject target)
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return Fade(fadeImage.color.a, 1f, deathFadeDuration);
        yield return new WaitForSeconds(deathBlackHoldDuration);
        SceneLoader.LoadScene(deathSceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        yield return Fade(from, to, fadeDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        Color c = fadeImage.color;
        c.a = from;
        fadeImage.color = c;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
