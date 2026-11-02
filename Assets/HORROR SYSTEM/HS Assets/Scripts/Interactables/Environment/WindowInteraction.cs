using UnityEngine;

public class WindowInteraction : InteractiveObject
{
    public Animator windowAnimator;
    public string windowSound;

    protected bool isWindowOpen = false;

    public override void Interact(GameObject player = null)
    {
        isWindowOpen = !isWindowOpen;
        AudioManager.instance.Play(windowSound);

        windowAnimator.SetBool("WindowOpen", isWindowOpen);
        windowAnimator.SetBool("WindowClose", !isWindowOpen);
    }
}
