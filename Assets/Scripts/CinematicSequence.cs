using System;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CinematicSequence : MonoBehaviour
{
    [Serializable]
    public class Waypoint
    {
        public Transform point;

        [Tooltip("Time to move from the previous waypoint to this one. Ignored on the first waypoint.")]
        public float moveTime = 5f;
    }

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cinematicCamera;

    [Header("Points")]
    [Tooltip("Camera moves through these in order as one continuous curve (Catmull-Rom) — it doesn't stop at intermediate points. Each entry's Move Time is how long the leg from the previous one takes (ignored for the first entry).")]
    [SerializeField] private Waypoint[] points;

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("Events")]
    [Tooltip("Fired once the cinematic has fully finished (player camera already active and faded in).")]
    [SerializeField] private UnityEvent onCinematicComplete;

    private void Start()
    {
        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        if (points == null || points.Length == 0 || points[0].point == null)
        {
            yield break;
        }

        cinematicCamera.transform.SetPositionAndRotation(
            points[0].point.position,
            points[0].point.rotation);

        // Inicia Fade In e movimento simultaneamente
        Coroutine fadeIn = StartCoroutine(Fade(1f, 0f));
        Coroutine move = points.Length > 1 ? StartCoroutine(MoveAlongPath()) : null;

        if (move != null)
        {
            yield return move;
        }

        // Garante que o Fade terminou
        yield return fadeIn;

        // Fade Out ao chegar no ponto final
        yield return Fade(0f, 1f);

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj) obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj) obj.SetActive(true);
        }

        yield return Fade(1f, 0f);

        onCinematicComplete?.Invoke();
    }

    /// <summary>
    /// Moves continuously through every waypoint as a single Catmull-Rom
    /// curve, so the camera never slows to a stop at intermediate points —
    /// only at the very start and end of the whole path.
    /// </summary>
    IEnumerator MoveAlongPath()
    {
        int n = points.Length;

        float[] cumulativeTime = new float[n];
        cumulativeTime[0] = 0f;
        for (int i = 1; i < n; i++)
        {
            cumulativeTime[i] = cumulativeTime[i - 1] + Mathf.Max(0.0001f, points[i].moveTime);
        }

        float totalTime = cumulativeTime[n - 1];
        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Min(elapsed, totalTime);

            int seg = 1;
            while (seg < n - 1 && t > cumulativeTime[seg])
            {
                seg++;
            }

            float segStart = cumulativeTime[seg - 1];
            float segDuration = cumulativeTime[seg] - segStart;
            float u = segDuration > 0f ? Mathf.Clamp01((t - segStart) / segDuration) : 1f;

            Vector3 p0 = PointAt(seg - 2).position;
            Vector3 p1 = PointAt(seg - 1).position;
            Vector3 p2 = PointAt(seg).position;
            Vector3 p3 = PointAt(seg + 1).position;

            cinematicCamera.transform.position = CatmullRom(p0, p1, p2, p3, u);
            cinematicCamera.transform.rotation = Quaternion.Slerp(PointAt(seg - 1).rotation, PointAt(seg).rotation, u);

            yield return null;
        }

        Transform last = points[n - 1].point;
        cinematicCamera.transform.SetPositionAndRotation(last.position, last.rotation);
    }

    /// <summary>Clamps to the valid waypoint range, so the curve doesn't overshoot past the first/last point.</summary>
    Transform PointAt(int index)
    {
        int clamped = Mathf.Clamp(index, 0, points.Length - 1);
        return points[clamped].point;
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(
                startAlpha,
                endAlpha,
                timer / fadeDuration);

            fadeImage.color = color;

            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
}
