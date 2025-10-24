using UnityEngine;

public class KeyInteraction : InteractiveItem
{
    private InventorySystem playerInventory;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();

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

    public override void DropFromInventory()
    {
        playerInventory.DropItems(itemId, 1);
    }
}
