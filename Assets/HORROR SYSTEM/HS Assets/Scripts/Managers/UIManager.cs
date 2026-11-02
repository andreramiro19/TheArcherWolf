using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject interactionPanel;
    public GameObject inventoryPanel;
    public GameObject pauseMenuPanel;
    public GameObject notePanel;
    public GameObject ammoPanel;
    public GameObject knifePanel;
    public GameObject crosshairPanel;

    [Header("UI Text")]
    public TMP_Text ammoCount;
    public TMP_Text notificationText;

    [Header("System")]
    public MenuController menuSystem;

    private bool isNotePanelActive;
    private bool isInventoryPanelActive;
    private bool isPausePanelActive;
    private bool isAmmoPanelActive;
    private bool isKnifePanelActive;
    private bool isCrosshairPanelActive;

    private bool wasNotePanelActiveBeforePause;
    private bool wasInventoryPanelActiveBeforePause;

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

    void Start()
    {
        interactionPanel.SetActive(true);

        notePanel.SetActive(false);
        inventoryPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        ammoPanel.SetActive(false);
        knifePanel.SetActive(false);
        crosshairPanel.SetActive(false);

        notificationText.text = null;

        isNotePanelActive = false;
        isInventoryPanelActive = false;
        isPausePanelActive = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void PlayerDisable()
    {
        if (isInventoryPanelActive || isNotePanelActive || isPausePanelActive)
        {
            global::PlayerDisable.instance.DisablePlayer();
        }
        else
        {
            global::PlayerDisable.instance.EnablePlayer();
        }
    }

    public void SetNotePanelActive()
    {
        if (isPausePanelActive)
            return;

        isNotePanelActive = !isNotePanelActive;
        notePanel.SetActive(isNotePanelActive);

        if (isNotePanelActive)
        {
            DisableOtherPanels();
        }
        else
        {
            EnableOtherPanels();
        }

        PlayerDisable();
    }

    public void SetInventoryPanelActive(bool setInventoryPanel)
    {
        if (isPausePanelActive)
            return;

        isInventoryPanelActive = setInventoryPanel;
        inventoryPanel.SetActive(setInventoryPanel);

        ManageCursorVisibility(setInventoryPanel);

        if (setInventoryPanel)
        {
            DisableOtherPanels();
        }
        else
        {
            EnableOtherPanels();
        }

        PlayerDisable();

        if (!setInventoryPanel)
        {
            ContextMenu.HideContextMenu();
            InventoryEquipmentState.ExecuteActions();
        }
    }

    public void SetPausePanelActive(bool setPausePanel)
    {
        isPausePanelActive = setPausePanel;
        pauseMenuPanel.SetActive(setPausePanel);

        ManageCursorVisibility(setPausePanel);

        if (setPausePanel)
        {
            wasNotePanelActiveBeforePause = isNotePanelActive;
            wasInventoryPanelActiveBeforePause = isInventoryPanelActive;

            if (isNotePanelActive)
            {
                isNotePanelActive = false;
                notePanel.SetActive(false);
            }

            if (isInventoryPanelActive)
            {
                isInventoryPanelActive = false;
                inventoryPanel.SetActive(false);
                ContextMenu.HideContextMenu();
                InventoryEquipmentState.ExecuteActions();
            }

            DisableOtherPanels();
        }
        else
        {
            if (wasNotePanelActiveBeforePause)
            {
                isNotePanelActive = true;
                notePanel.SetActive(true);
            }

            if (wasInventoryPanelActiveBeforePause)
            {
                isInventoryPanelActive = true;
                inventoryPanel.SetActive(true);
            }

            EnableOtherPanels();
        }

        PlayerDisable();
    }

    private void DisableOtherPanels()
    {
        isAmmoPanelActive = ammoPanel.activeSelf;
        isKnifePanelActive = knifePanel.activeSelf;
        isCrosshairPanelActive = crosshairPanel.activeSelf;

        interactionPanel.SetActive(false);
        if (isAmmoPanelActive) ammoPanel.SetActive(false);
        if (isKnifePanelActive) knifePanel.SetActive(false);
        if (isCrosshairPanelActive) crosshairPanel.SetActive(false);
    }

    private void EnableOtherPanels()
    {
        interactionPanel.SetActive(true);
        if (isAmmoPanelActive) ammoPanel.SetActive(true);
        if (isKnifePanelActive) knifePanel.SetActive(true);
        if (isCrosshairPanelActive) crosshairPanel.SetActive(true);
    }

    public void SetCrosshairPanelActive(bool isCrosshairPanelActive)
    {
        crosshairPanel.SetActive(isCrosshairPanelActive);
    }

    public void SetAmmoPanelActive(bool isAmmoPanelActive)
    {
        ammoPanel.SetActive(isAmmoPanelActive);
    }

    public void SetKnifePanelActive(bool isKnifePanelActive)
    {
        knifePanel.SetActive(isKnifePanelActive);
    }

    public void UpdateAmmoCountUI(int currentAmmo, int totalAmmo)
    {
        ammoCount.text = $"{currentAmmo}/{totalAmmo}";
    }

    private void ManageCursorVisibility(bool isActive)
    {
        if (isActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ShowEquipmentAlreadyInInventory(string text)
    {
        StopAllCoroutines();
        notificationText.text = text;
        StartCoroutine(HideInventoryFullNotificationAfterDelay(2f));
    }

    public void ShowInventoryFullNotification()
    {
        StopAllCoroutines();
        notificationText.text = "No space in inventory";
        StartCoroutine(HideInventoryFullNotificationAfterDelay(2f));
    }

    private IEnumerator HideInventoryFullNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationText.text = "";
    }
}
