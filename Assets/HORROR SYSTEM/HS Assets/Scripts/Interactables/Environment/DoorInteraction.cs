using UnityEngine;

public class DoorInteraction : InteractiveObject
{
    public Animator doorAnimator;

    [Space]
    public bool needKey;
    public string keyId;

    [Space]
    public string doorLockedSound;
    public string doorOpenSound;
    public string doorCloseSound;

    protected bool isDoorOpen = false;

    private bool isFirstInteraction = true;

    public override void Interact(GameObject player = null)
    {
        var playerInventory = player.GetComponent<InventorySystem>();

        if (needKey && isFirstInteraction)
            CheckKeyPass(playerInventory);
        else
            OpenClose(playerInventory);
    }

    private void CheckKeyPass(InventorySystem playerInventory)
    {
        var keys = playerInventory.FindItemByType(ItemDatabase.Type.KEY);
        if (keys == null)
        {
            Locked();
            return;
        }

        var correctKey = keys.Find(key => keyId.Equals(key.itemId));
        if (correctKey != null)
        {
            OpenClose(playerInventory);
            isFirstInteraction = false;
        }
        else
        {
            Locked();
        }
    }

    private void OpenClose(InventorySystem playerInventory)
    {
        isDoorOpen = !isDoorOpen;

        doorAnimator.SetBool("DoorOpen", isDoorOpen);
        doorAnimator.SetBool("DoorClose", !isDoorOpen);

        if (isDoorOpen)
        {
            AudioManager.instance.Play(doorOpenSound);
        }
        else
        {
            AudioManager.instance.Play(doorCloseSound);
        }

        if (needKey)
        {
            playerInventory.SpendSingleItem(keyId);
        }
    }

    private void Locked()
    {
        AudioManager.instance.Play(doorLockedSound);
        doorAnimator.SetTrigger("DoorLocked");
    }
}
