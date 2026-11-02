using UnityEngine;

public class AmmoBoxInteraction : InteractiveItem
{
    public int maxAmountToDrop = 20;

    private InventorySystem playerInventory;
    private PistolSystem pistolSystem;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();
        this.pistolSystem = InventoryManager.instance.pistolInHand.GetComponent<PistolSystem>();

        var itemHasBeenAdded = playerInventory.AddItem(itemId, count, this);
        if (itemHasBeenAdded)
        {
            pistolSystem.UpdateInventoryAmmoUI(count);
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
        if (playerInventory.FindItemByType(ItemDatabase.Type.PISTOL) == null)
        {
            return;
        }
        pistolSystem.PistolReloading();
    }

    public override void DropFromInventory()
    {
        playerInventory.DropItems(itemId, maxAmountToDrop);
    }

    public override string GetStringCount()
    {
        return count.ToString();
    }
}
