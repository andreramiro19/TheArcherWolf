using System.Collections.Generic;
using UnityEngine;

public class InteractiveItem : InteractiveObject
{
    public string itemId;
    public int count;
    public string pickUpSound;
    public string failurePickUpSound;
    public List<AvalaibleInteraction> avalaibleInteractions = new() { AvalaibleInteraction.USE, AvalaibleInteraction.DROP };

    private ItemDatabase.Item databaseItem;

    public override void Interact(GameObject player = null)
    {
        base.Interact(player);
    }

    protected void Start()
    {
        var database = InventorySystem.instance.itemDatabase;
        var item = database.GetItem(itemId);
        if (item == null)
        {
            Debug.LogError("Item with ID " + itemId + " doesnt exist in Database");
        }
        databaseItem = item;
    }

    protected void FailurePickUp()
    {
        UIManager.instance.ShowInventoryFullNotification();
        AudioManager.instance.Play(failurePickUpSound);
    }

    public virtual void UseInInventory() { }

    public virtual void DropFromInventory() { }

    public virtual string GetStringCount()
    {
        return "";
    }

    public enum AvalaibleInteraction
    {
        USE,
        DROP
    }

    public ItemDatabase.Item GetDatabaseItem()
    {
        return databaseItem;
    }
}
