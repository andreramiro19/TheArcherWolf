using UnityEngine;

public class FlashlightInteraction : InteractiveItem
{
    private InventorySystem playerInventory;
    private FlashlightSystem playerFlaslight;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();

        var flashlightInInventory = this.playerInventory.FindItemByType(ItemDatabase.Type.FLASHLIGHT);
        if (flashlightInInventory != null)
        {
            UIManager.instance.ShowEquipmentAlreadyInInventory("You already have a flashlight");
            AudioManager.instance.Play(failurePickUpSound);
            return;
        }

        var itemHasBeenAdded = this.playerInventory.AddItem(itemId, count, this);

        if (itemHasBeenAdded)
        {
            this.playerFlaslight = player.GetComponent<PlayerController>().flashlightSystem;
            InventoryManager.instance.hasFlashlight = true;
            AudioManager.instance.Play(pickUpSound);
            Destroy(0);
        }
        else
        {
            FailurePickUp();
        }
    }

    public override void UseInInventory()
    {
        InventoryEquipmentState.TakeEquipment(this);
    }

    public override void DropFromInventory()
    {
        playerInventory.DropItems(itemId, 1);
        InventoryEquipmentState.RemoveEquipment(this);
        InventoryManager.instance.hasFlashlight = false;
        InventoryManager.instance.SetFlashlightActiveOnPlayer(false);
    }

    public override string GetStringCount()
    {
        int percentage = Mathf.FloorToInt(playerFlaslight.currentIntensity / playerFlaslight.maxIntensity * 100);
        return percentage + "%";
    }
}
