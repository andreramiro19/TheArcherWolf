using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 1.2f;
    public string interactableTag = "Interactable";

    [Header("Interaction UI")]
    public Image interactionIcon;
    public TMP_Text interactionText;

    [Header("Detection Settings")]
    public LayerMask obstacleLayer;

    private RectTransform iconRectTransform;
    private GameObject currentObject;
    private Transform interactionPoint;
    private float interactionPointHeight = 1f;

    private void Start()
    {
        interactionPoint = new GameObject("InteractionPoint").transform;
        interactionPoint.SetParent(transform);
        interactionPoint.localPosition = new Vector3(0f, interactionPointHeight, 0f);

        interactionIcon.gameObject.SetActive(false);
        iconRectTransform = interactionIcon.GetComponent<RectTransform>();
        interactionText.text = null;
    }

    private void Update()
    {
        HandleInteraction();
        IconPoint();
    }

    private void HandleInteraction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(interactionPoint.position, interactionDistance);
        Transform closestObject = null;

        if (hitColliders.Length > 0)
        {
            float minDistance = float.MaxValue;
            foreach (Collider collider in hitColliders)
            {
                if (!collider.CompareTag(interactableTag))
                {
                    continue;
                }

                float distance = Vector3.Distance(interactionPoint.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObject = collider.transform;
                }
            }

            if (InputManager.instance.InteractionTriggered && closestObject != null)
            {
                PickUpObject(closestObject.gameObject);
                InputManager.instance.SetInteractionTriggered(false);
            }

            SetCurrentObject(closestObject != null ? closestObject.gameObject : null);
        }
    }

    public void PickUpObject(GameObject objectToPick)
    {
        var player = this.gameObject;
        objectToPick.GetComponent<InteractiveObject>().Interact(player);
    }

    private void IconPoint()
    {
        if (currentObject != null)
        {
            Transform iconPoint = currentObject.transform.Find("IconPoint");

            if (iconPoint != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(iconPoint.position);
                iconRectTransform.position = screenPos;
            }
        }
    }

    public void SetCurrentObject(GameObject obj)
    {
        currentObject = obj;
        interactionIcon.gameObject.SetActive(currentObject != null);
        UpdateInteractionText();
    }

    private void UpdateInteractionText()
    {
        var interactionAction = InputManager.instance.playerControls.FindAction("Interaction");
        string keyBinding = interactionAction.GetBindingDisplayString();

        interactionText.text = currentObject != null ? $"Interact ({keyBinding})" : null;
    }
}
