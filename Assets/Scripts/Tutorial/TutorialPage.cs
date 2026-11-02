using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "TutorialPage", menuName = "Archer Wolf/Tutorial/Tutorial Page")]
public class TutorialPage : ScriptableObject
{
    [Header("Content")]
    [Tooltip("Optional. Leave empty until the clip is ready — the page still works without video.")]
    public VideoClip video;

    [TextArea(2, 6)]
    public string description;
}
