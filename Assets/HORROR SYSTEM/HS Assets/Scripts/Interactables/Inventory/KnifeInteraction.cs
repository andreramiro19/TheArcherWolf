using UnityEngine;

public class KnifeInteraction : InteractiveItem
{
    private InventorySystem playerInventory;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();

        var knifeInInventory = this.playerInventory.FindItemByType(ItemDatabase.Type.KNIFE);
        if (knifeInInventory != null)
        {
            UIManager.instance.ShowEquipmentAlreadyInInventory("You already have a knife");
            AudioManager.instance.Play(failurePickUpSound);
            return;
        }

        var itemHasBeenAdded = playerInventory.AddItem(itemId, count, this);
        if (itemHasBeenAdded)
        {
            AudioManager.instance.Play(pickUpSound);
            InventoryManager.instance.hasKnife = true;
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
        InventoryManager.instance.hasKnife = false;
        InventoryManager.instance.SetKnifeActive(false);
    }
}
