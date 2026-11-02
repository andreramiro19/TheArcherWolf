using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ContextMenu.HideContextMenu();
    }
}
