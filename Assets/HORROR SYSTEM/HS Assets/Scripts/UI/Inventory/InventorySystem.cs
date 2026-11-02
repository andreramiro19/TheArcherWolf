using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance;

    [Header("Inventory Slots")]
    public List<InventoryItem> itemSlots;

    [Header("Slot Settings")]
    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab;

    public GridLayoutGroup gridLayoutGroup;
    public TMP_Text descriptionText;

    [Header("Slot Configuration")]
    public int rows;
    public int cols;
    private int inventoryCapacity;

    [Header("Database")]
    public ItemDatabase itemDatabase;

    private List<GameObject> slots;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (inventorySlotPrefab == null || gridLayoutGroup == null || itemDatabase == null)
        {
            Debug.LogError("One or more essential components are not assigned");
            return;
        }

        descriptionText.text = null;

        slots = new List<GameObject>();

        inventoryCapacity = rows * cols;
        for (int i = 0; i < inventoryCapacity; i++)
        {
            var newSlot = Instantiate(inventorySlotPrefab);
            newSlot.transform.SetParent(gridLayoutGroup.transform, false);
            newSlot.transform.localPosition = Vector3.zero;
            newSlot.name = $"Slot_{i}";
            slots.Add(newSlot);
        }
    }

    public void UpdateInventoryUI()
    {
        ContextMenu.HideContextMenu();

        slots.ForEach(slot =>
        {
            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null)
            {
                item.UpdateAmountText();
            }
        });
    }

    public bool AddItem(string itemId, int amountToAdd, InteractiveItem interactiveItem)
    {
        ItemDatabase.Item item = itemDatabase.GetItem(itemId);
        if (item == null)
        {
            Debug.LogError($"The item with the ID '{itemId}' was not found in the database");
            return false;
        }

        var slot = FindExistingItemSlot(itemId);
        if (slot != null)
        {
            UpdateExistingSlot(slot.transform, amountToAdd);
            return true;
        }
        else if (amountToAdd > 0)
        {
            return PlaceItemInFreeSlot(item, amountToAdd, interactiveItem);
        }

        return false;
    }

    public void DeleteItem(string itemId)
    {
        var slot = FindExistingItemSlot(itemId);
        var item = slot.GetComponentInChildren<InventoryItem>();
        item.transform.SetParent(null, false);
        Destroy(item);
    }

    public void SpendSingleItem(string itemId)
    {
        var slot = FindExistingItemSlot(itemId);
        if (slot == null)
        {
            Debug.Log("No item with id " + itemId + " in inventory");
            return;
        }

        var item = slot.GetComponentInChildren<InventoryItem>();

        if (item.GetInteractiveItem().count > 0)
        {
            item.GetInteractiveItem().count--;
            item.UpdateAmountText();
        }

        if (item.GetInteractiveItem().count == 0)
        {
            ContextMenu.HideContextMenu();
            DeleteItem(itemId);
        }
    }

    public int SpendItems(string itemId, int amountToSpend)
    {
        var slot = FindExistingItemSlot(itemId);
        var item = slot.GetComponentInChildren<InventoryItem>();

        int inventoryItemAmount = item.GetInteractiveItem().count;
        if (inventoryItemAmount - amountToSpend <= 0)
        {
            ContextMenu.HideContextMenu();
            DeleteItem(itemId);
            return inventoryItemAmount;
        }

        item.GetInteractiveItem().count -= amountToSpend;
        item.UpdateAmountText();
        return amountToSpend;
    }

    public void DropItems(string itemId, int amountToDrop)
    {
        var slot = FindExistingItemSlot(itemId);
        var item = slot.GetComponentInChildren<InventoryItem>();

        var droppedItem = SpawnDroppedItem(item);

        int inventoryItemAmount = item.GetInteractiveItem().count;
        if (inventoryItemAmount - amountToDrop <= 0)
        {
            ContextMenu.HideContextMenu();
            DeleteItem(itemId);
            droppedItem.GetComponent<InteractiveItem>().count = inventoryItemAmount;
        }
        else
        {
            item.GetInteractiveItem().count -= amountToDrop;
            item.UpdateAmountText();
            droppedItem.GetComponent<InteractiveItem>().count = amountToDrop;
        }
    }

    private GameObject SpawnDroppedItem(InventoryItem item)
    {
        var itemPrefab = item.GetDatabaseItem().prefab;
        return Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public List<InventoryItem> FindItemByType(ItemDatabase.Type type)
    {
        var slotsByType = slots.FindAll(slot =>
        {
            var itemsInSlot = slot.GetComponentsInChildren<InventoryItem>();
            foreach (var item in itemsInSlot)
            {
                if (item.GetDatabaseItem().type.Equals(type))
                {
                    return true;
                }
            }
            return false;
        });

        var items = slotsByType.ConvertAll<InventoryItem>(slot =>
        {
            return slot.GetComponentInChildren<InventoryItem>();
        });

        if (!items.Any())
        {
            return null;
        }

        return items;
    }

    private GameObject FindExistingItemSlot(string itemId)
    {
        var slot = slots.Find(slot =>
        {
            var itemsInSlot = slot.GetComponentsInChildren<InventoryItem>();
            foreach (var item in itemsInSlot)
            {
                if (item.itemId == itemId)
                {
                    return true;
                }
            }
            return false;
        });

        return slot;
    }

    private void UpdateExistingSlot(Transform slotTransform, int amountToAdd)
    {
        var item = slotTransform.GetComponentInChildren<InventoryItem>();
        if (item != null)
        {
            item.GetInteractiveItem().count += amountToAdd;
            item.UpdateAmountText();
        }
        else
        {
            Debug.LogError("A slot was found, but the InventoryItem component was not found");
        }
    }

    private bool PlaceItemInFreeSlot(ItemDatabase.Item item, int amountToAdd, InteractiveItem interactiveItem)
    {
        var freeSlot = slots.Find(slot =>
        {
            if (slot != null && !IsSlotOccupied(slot))
            {
                return true;
            }
            return false;
        });

        if (freeSlot != null)
        {
            PutItem(item, amountToAdd, freeSlot, interactiveItem);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void PutItem(ItemDatabase.Item databaseItem, int amountToAdd, GameObject slot, InteractiveItem interactiveItem)
    {
        var itemGameobject = Instantiate(inventoryItemPrefab, slot.transform);
        itemGameobject.transform.localPosition = Vector3.zero;
        var item = itemGameobject.GetComponent<InventoryItem>();

        item.itemId = databaseItem.id;
        item.itemDescription = databaseItem.description;
        item.itemSprite = databaseItem.itemSprite;
        item.spawnPoint = spawnPoint;
        item.SetDatabaseItem(databaseItem);

        item.SetInteractiveItem(interactiveItem);

        var itemImage = item.GetComponentInChildren<Image>();
        if (itemImage != null)
        {
            itemImage.sprite = databaseItem.itemSprite;
            itemImage.enabled = databaseItem.itemSprite != null;
        }
        else
        {
            Debug.LogError("The Image component could not be found in the itemSlotPrefab for the slot");
        }
    }

    public bool IsSlotOccupied(GameObject slotGameObject)
    {
        return slotGameObject.GetComponentsInChildren<InventoryItem>().Length > 0;
    }

    public void UpdateItemDescription(string newDescription)
    {
        if (descriptionText != null)
        {
            descriptionText.text = newDescription;
        }
        else
        {
            Debug.LogError("Description text not found in InventoryManager");
        }
    }
}