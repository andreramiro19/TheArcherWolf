using UnityEngine;

public class NoteInteraction : InteractiveObject
{
    public string noteSound;

    public override void Interact(GameObject player = null)
    {
        AudioManager.instance.Play(noteSound);
        UIManager.instance.SetNotePanelActive();
    }
}
