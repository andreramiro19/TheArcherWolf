using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Flashlight")]
    public GameObject flashlightInInventory;
    public GameObject flashlightInHand;
    private bool _hasFlashlight = false;
    private bool _isFlashlightActive = false;

    [Header("Knife")]
    public GameObject knifeInInventory;
    public GameObject knifeInHand;
    private bool _hasKnife = false;

    [Header("Pistol")]
    public GameObject pistolInInventory;
    public GameObject pistolInHand;
    private bool _hasPistol = false;

    private int currentWeaponIndex = -1;
    private List<GameObject> weaponsInHand;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        weaponsInHand = new List<GameObject> { knifeInHand, pistolInHand };

        SetWeaponState(knifeInHand, false);
        SetWeaponState(pistolInHand, false);
        SetWeaponState(flashlightInHand, false);

        UpdateInventoryStates();
    }

    public bool hasFlashlight
    {
        get => _hasFlashlight;
        set
        {
            _hasFlashlight = value;
            UpdateInventoryStates();
        }
    }

    public bool hasKnife
    {
        get => _hasKnife;
        set
        {
            _hasKnife = value;
            UpdateInventoryStates();
        }
    }

    public bool hasPistol
    {
        get => _hasPistol;
        set
        {
            _hasPistol = value;
            UpdateInventoryStates();
        }
    }

    private void UpdateInventoryStates()
    {
        if (!knifeInHand.activeSelf)
        {
            SetWeaponState(knifeInInventory, _hasKnife);
        }
        else
        {
            SetWeaponState(knifeInInventory, false);
        }

        if (!pistolInHand.activeSelf)
        {
            SetWeaponState(pistolInInventory, _hasPistol);
        }
        else
        {
            SetWeaponState(pistolInInventory, false);
        }

        if (!flashlightInHand.activeSelf)
        {
            SetWeaponState(flashlightInInventory, _hasFlashlight);
        }
        else
        {
            SetWeaponState(flashlightInInventory, false);
        }
    }

    public void SetFlashlightActive(bool active)
    {
        _isFlashlightActive = active;

        SetWeaponState(flashlightInHand, _isFlashlightActive);
        SetWeaponState(flashlightInInventory, !_isFlashlightActive);
    }

    public void SetFlashlightActiveOnPlayer(bool active)
    {
        SetWeaponState(flashlightInHand, active);
        SetWeaponState(flashlightInInventory, !active && _hasFlashlight);
    }

    public void SetPistolActive(bool active)
    {
        SetWeaponState(pistolInHand, active);
        SetWeaponState(pistolInInventory, !active && _hasPistol);
    }

    public void SetKnifeActive(bool active)
    {
        SetWeaponState(knifeInHand, active);
        SetWeaponState(knifeInInventory, !active && _hasKnife);
    }

    public void SetWeaponState(GameObject weapon, bool state)
    {
        weapon.SetActive(state);
    }

    public bool IsKnifeActive()
    {
        return currentWeaponIndex == 0;
    }

    public bool IsPistolActive()
    {
        return currentWeaponIndex == 1;
    }
}