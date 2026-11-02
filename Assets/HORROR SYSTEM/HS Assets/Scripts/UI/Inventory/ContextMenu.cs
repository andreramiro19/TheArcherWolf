using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject contextMenuPrefab;
    private static GameObject currentContextMenu;
    private static ContextMenu currentCellWithMenu;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentContextMenu == null)
            {
                ShowContextMenu();
            }
            else
            {
                HideContextMenu();
            }
        }
    }

    private void ShowContextMenu()
    {
        if (currentContextMenu != null)
        {
            Destroy(currentContextMenu);
        }

        currentContextMenu = Instantiate(contextMenuPrefab, transform.root);
        currentContextMenu.transform.position = transform.position;
        currentCellWithMenu = this;

        GameObject inventoryPanel = GameObject.FindGameObjectWithTag("InventoryPanel");
        currentContextMenu.transform.SetParent(inventoryPanel.transform, false);

        Button dropButton = currentContextMenu.transform.Find("DropButton").GetComponent<Button>();
        Button useButton = currentContextMenu.transform.Find("UseButton").GetComponent<Button>();

        var item = this.GetComponentInParent<InventoryItem>();
        var itemType = item.GetDatabaseItem().type;

        if (ItemDatabase.Type.KEY.Equals(itemType))
        {
            useButton.gameObject.SetActive(false);
        }
        else
        {
            useButton.gameObject.SetActive(true);
        }

        dropButton.onClick.AddListener(() => item.GetInteractiveItem().DropFromInventory());
        useButton.onClick.AddListener(() => item.GetInteractiveItem().UseInInventory());
    }

    public static void HideContextMenu()
    {
        if (currentContextMenu != null)
        {
            Destroy(currentContextMenu);
            currentContextMenu = null;
            currentCellWithMenu = null;
        }
    }

    private void OnDropButtonClick()
    {
        HideContextMenu();
    }

    private void OnUseButtonClick()
    {
        HideContextMenu();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        HideContextMenu();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentContextMenu != null && currentCellWithMenu == this)
        {
            currentContextMenu.transform.position = transform.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }
}