using UnityEngine;

[CreateAssetMenu(fileName = "StoryPage", menuName = "Archer Wolf/Story/Story Page")]
public class StoryPage : ScriptableObject
{
    public enum PagePanDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
        ZoomIn,
        ZoomOut
    }

    [Header("Content")]
    public Sprite illustration;

    [TextArea(3, 10)]
    public string text;

    [Header("Movement (Ken Burns)")]
    public PagePanDirection panDirection = PagePanDirection.None;

    [Tooltip("Left/Right/Up/Down: UI units per second. Zoom In/Out: scale change per second (e.g. 0.02 = 2%/s).")]
    public float panSpeed = 10f;

    [Header("Audio")]
    [Tooltip("Optional voiced narration for this page's text.")]
    public AudioClip narration;

    [Tooltip("Seconds after the page becomes visible before narration starts. 0 = plays immediately.")]
    public float narrationDelay = 0f;

    [Header("Overrides")]
    [Tooltip("Leave at -1 to use the sequence's default typing speed.")]
    public float charsPerSecondOverride = -1f;

    [Tooltip("Leave at -1 to use the sequence's default delay after the line finishes.")]
    public float delayAfterLineOverride = -1f;
}
