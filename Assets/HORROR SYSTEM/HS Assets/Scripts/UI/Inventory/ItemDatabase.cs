using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new();

    public void AddItem(Item newItem)
    {
        items.Add(newItem);
    }

    public Item GetItem(string itemId)
    {
        return items.Find(item => item.id == itemId);
    }

    [System.Serializable]
    public class Item
    {
        public string itemName;
        public string description;
        public string id;
        public Sprite itemSprite;
        public Type type;
        public GameObject prefab;
    }

    public enum Type
    {
        BATTERY,
        FLASHLIGHT,
        AMMO,
        KNIFE,
        MEDKIT,
        PISTOL,
        KEY
    }
}