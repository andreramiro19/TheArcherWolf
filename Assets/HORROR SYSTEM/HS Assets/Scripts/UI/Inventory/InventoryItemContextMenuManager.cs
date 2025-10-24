using UnityEngine;
using UnityEngine.UI;
using static InteractiveItem;

public class InventoryItemContextMenuManager : MonoBehaviour
{
    public static InventoryItemContextMenuManager Instance;

    public GameObject inventoryContextButtonPrefab;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void CreateContext(GameObject item, InteractiveItem interactiveItem)
    {
        var itemTransform = item.transform;
        var itemGridLayoutGroupTransform = itemTransform.GetComponentInChildren<GridLayoutGroup>().transform;

        interactiveItem.avalaibleInteractions.ForEach(interaction =>
        {
            if (interaction.Equals(AvalaibleInteraction.USE))
            {
                var itemContexMenuButton1 = Instantiate(inventoryContextButtonPrefab, itemTransform);
                itemContexMenuButton1.GetComponent<Button>().onClick.AddListener(() =>
                {
                    interactiveItem.UseInInventory();
                });
                itemContexMenuButton1.transform.SetParent(itemGridLayoutGroupTransform, false);
            }

            if (interaction.Equals(AvalaibleInteraction.DROP))
            {
                var itemContexMenuButton2 = Instantiate(inventoryContextButtonPrefab, itemTransform);
                itemContexMenuButton2.GetComponent<Button>().onClick.AddListener(() =>
                {
                    interactiveItem.DropFromInventory();
                });
                itemContexMenuButton2.transform.SetParent(itemGridLayoutGroupTransform, false);
            }
        });
    }
}