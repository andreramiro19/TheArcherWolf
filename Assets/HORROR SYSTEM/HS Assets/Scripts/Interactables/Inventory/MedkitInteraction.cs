using UnityEngine;

public class MedkitInteraction : InteractiveItem
{
    private PlayerHealth playerHealth;
    private InventorySystem playerInventory;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);

        this.playerHealth = player.GetComponent<PlayerHealth>();
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

    public override void UseInInventory()
    {
        if (playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.SetMaxHealth();
            playerInventory.SpendSingleItem(itemId);
        }
        Debug.Log("Maximum health");
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
