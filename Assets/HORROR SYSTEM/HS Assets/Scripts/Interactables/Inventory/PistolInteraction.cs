using UnityEngine;

public class PistolInteraction : InteractiveItem
{
    private InventorySystem playerInventory;
    private PistolSystem pistolSystem;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerInventory = player.GetComponent<InventorySystem>();
        this.pistolSystem = player.GetComponent<PlayerController>().pistolSystem;

        var pistolInInventory = this.playerInventory.FindItemByType(ItemDatabase.Type.PISTOL);
        if (pistolInInventory != null)
        {
            UIManager.instance.ShowEquipmentAlreadyInInventory("You already have a pistol");
            AudioManager.instance.Play(failurePickUpSound);
            return;
        }

        var itemHasBeenAdded = playerInventory.AddItem(itemId, count, this);
        if (itemHasBeenAdded)
        {
            AudioManager.instance.Play(pickUpSound);
            InventoryManager.instance.hasPistol = true;
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
        InventoryManager.instance.hasPistol = false;
        InventoryManager.instance.SetPistolActive(false);
    }

    public override string GetStringCount()
    {
        int currentAmmoInMagazine = pistolSystem.currentAmmo;
        int magazineCapacity = pistolSystem.magazineCapacity;
        return currentAmmoInMagazine + "/" + magazineCapacity;
    }
}
