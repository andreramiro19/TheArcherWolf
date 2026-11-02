using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("Item Settings")]
    public string itemId;
    public string itemDescription;
    public Sprite itemSprite;

    [Header("Amount Settings")]
    public TMP_Text amountText;

    private CanvasGroup canvasGroup;
    private Vector2 offset;
    private Transform originalParent;

    private InteractiveItem interactiveItem;
    private ItemDatabase.Item databaseItem;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    public void SetDatabaseItem(ItemDatabase.Item item)
    {
        this.databaseItem = item;
    }

    public ItemDatabase.Item GetDatabaseItem()
    {
        return this.databaseItem;
    }

    private void Start()
    {
        InitializeSlot();
    }

    private void Update()
    {
        UpdateAmountText();
    }

    public void SetInteractiveItem(InteractiveItem item)
    {
        this.interactiveItem = item;
    }

    public InteractiveItem GetInteractiveItem()
    {
        return this.interactiveItem;
    }

    private void InitializeSlot()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup not found on ItemSlot");
        }

        amountText = GetComponentInChildren<TMP_Text>();

        UpdateAmountText();
    }

    public void UpdateAmountText()
    {
        if (amountText != null)
        {
            amountText.text = interactiveItem.GetStringCount();
            amountText.gameObject.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventorySystem.instance != null)
        {
            var resultItemDescrition = string.IsNullOrEmpty(itemDescription) ? string.Empty : itemDescription;
            InventorySystem.instance.UpdateItemDescription(resultItemDescrition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventorySystem.instance != null)
        {
            InventorySystem.instance.UpdateItemDescription(string.Empty);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup == null)
        {
            return;
        }
        originalParent = this.transform.parent;
        Vector2 offset2D = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
        offset = offset2D;
        canvasGroup.alpha = 0.2f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvasGroup == null) return;
        offset = eventData.position - (Vector2)this.transform.position;
        this.transform.position = eventData.position - offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;

        Transform targetSlot = FindFreeTargetSlot();

        if (targetSlot == null)
        {
            this.transform.SetParent(originalParent);
            this.transform.localPosition = Vector3.zero;
        }
        else
        {
            this.transform.SetParent(targetSlot);
            this.transform.localPosition = Vector3.zero;
            this.transform.SetAsLastSibling();
        }
    }

    private Transform FindFreeTargetSlot()
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            var hitObject = result.gameObject;

            if (hitObject.CompareTag("InventorySlot") && !InventorySystem.instance.IsSlotOccupied(hitObject))
            {
                return hitObject.transform;
            }
        }
        return null;
    }
}