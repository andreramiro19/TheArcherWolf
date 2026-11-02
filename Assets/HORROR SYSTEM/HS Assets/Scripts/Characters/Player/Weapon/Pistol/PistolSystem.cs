using UnityEngine;

public class PistolSystem : MonoBehaviour
{
    [Header("Ammo Settings")]
    public float reloadTime = 2f;
    public int magazineCapacity = 20;
    private int ammoCountValue = 0;
    public int currentAmmo = 20;

    [Header("Shooting Settings")]
    public float fireRate = 0.2f;
    public Transform bulletPrefab;
    public Transform shootPoint;
    public LayerMask bulletColliderLayer;

    [Header("Shooting VFX")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletEffect;

    private bool canShoot = true;

    private void Start()
    {
        currentAmmo = magazineCapacity;
        UpdateAmmoUI();
    }

    public void PistolShoot()
    {
        if (canShoot && currentAmmo > 0)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        canShoot = false;

        if (currentAmmo > 0)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            if (mouseWorldPosition != Vector3.zero)
            {
                Vector3 aimDir = (mouseWorldPosition - shootPoint.position).normalized;
                Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));

                muzzleFlash.Play();
                bulletEffect.Play();

                currentAmmo--;
                UpdateAmmoUI();
            }
        }

        Invoke("ResetShoot", fireRate);
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, bulletColliderLayer))
        {
            return raycastHit.point;
        }

        return Vector3.zero;
    }

    private void ResetShoot()
    {
        canShoot = true;
    }

    public bool CanReload()
    {
        return canShoot &&
               currentAmmo < magazineCapacity &&
               InventorySystem.instance.FindItemByType(ItemDatabase.Type.AMMO) != null;
    }

    public void PistolReloading()
    {
        var playerInventory = InventorySystem.instance;
        var ammoItem = playerInventory.FindItemByType(ItemDatabase.Type.AMMO).Find(item => true);

        if (ammoItem == null)
        {
            return;
        }

        int ammoToLoad = Mathf.Min(magazineCapacity - currentAmmo, ammoItem.GetInteractiveItem().count);
        currentAmmo += ammoToLoad;
        ammoItem.GetInteractiveItem().count -= ammoToLoad;
        ammoCountValue = ammoItem.GetInteractiveItem().count;

        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        UIManager.instance.UpdateAmmoCountUI(currentAmmo, ammoCountValue);
    }

    public void UpdateInventoryAmmoUI(int ammo)
    {
        ammoCountValue += ammo;
        UIManager.instance.UpdateAmmoCountUI(currentAmmo, ammoCountValue);
    }
}
