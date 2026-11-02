using System.Collections;
using TMPro;
using UnityEngine;

public class AreaTitleCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text areaNameText;

    [Header("Content")]
    [Tooltip("Used when Show On Start is on. Call Show(string) directly to reuse this same component for a different area later (e.g. from a zone-entry trigger).")]
    [SerializeField] private string areaName = "Becos Decadentes";

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float holdDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [SerializeField] private bool showOnStart = true;

    private Coroutine playRoutine;

    private void Start()
    {
        canvasGroup.alpha = 0f;

        if (showOnStart)
        {
            Show(areaName);
        }
    }

    public void Show(string displayName)
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        areaNameText.text = displayName;
        playRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
