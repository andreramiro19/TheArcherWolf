using UnityEngine;

public class BatteryInteraction : InteractiveItem
{
    private InventorySystem playerInventory;
    private FlashlightSystem playerFlaslight;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();
        this.playerFlaslight = InventoryManager.instance.flashlightInHand.GetComponent<FlashlightSystem>();

        var itemHasBeenAdded = playerInventory.AddItem(itemId, count, this);
        if (itemHasBeenAdded)
        {
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
        var flashlight = playerInventory.FindItemByType(ItemDatabase.Type.FLASHLIGHT).Find(item => true);
        if (flashlight != null)
        {
            if (playerFlaslight.currentIntensity < playerFlaslight.maxIntensity)
            {
                playerFlaslight.SetMaxIntensity();
                playerInventory.SpendSingleItem(itemId);
                flashlight.UpdateAmountText();
            }
            else
            {
                Debug.Log("Maximum intensity");
            }
        }
        else
        {
            Debug.Log("The flashlight is missing from the inventory");
        }
    }

    public override void DropFromInventory()
    {
        playerInventory.DropItems(itemId, 1);
    }

    public override string GetStringCount()
    {
        return count.ToString();
    }
}
