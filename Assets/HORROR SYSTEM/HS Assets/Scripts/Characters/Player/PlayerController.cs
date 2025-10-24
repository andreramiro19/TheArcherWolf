using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTarget;
    public float turnSmoothness = 0.1f;

    [Header("Camera Controller")]
    public PlayerCamera cameraController;

    [Header("Flashlight Settings")]
    public FlashlightSystem flashlightSystem;
    public string flashlightSound;
    public string getFlashlightSound;

    [Header("Knife Settings")]
    public KnifeSystem knifeSystem;
    public string knifeSound;
    public string getKnifeSound;

    [Header("Pistol Settings")]
    public PistolSystem pistolSystem;
    public float fireRate = 0.5f;
    public string getPistolSound;
    public string shootSound;
    public string magazineSound;
    public string reloadSound;

    private float targetAngleY;
    private float currentAngleY;
    private float angleDifference;

    private bool isRunning = false;

    private bool isFlashlight = false;

    private bool isKnife = false;
    private bool isKnifeAttack = false;
    private bool isKnifeAiming = false;
    
    private bool isPistol = false;
    private bool isPistolAiming = false;
    private bool isPistolShoot = false;
    private bool isShoot = false;

    private bool isInventoryPanel = false;
    private bool isPausePanel = false;

    private PlayerAnimations playerAnimations;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerAnimations = GetComponent<PlayerAnimations>();
        playerAnimations.ResetRigWeights();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        CheckPauseInput();

        if (playerHealth.IsPlayerDead()) return;

        HandleInput();

        Move();
        Rotation();

        if (isKnife)
        {
            KnifeSystem();
        }

        if (isPistol)
        {
            PistolSystem();
        }

        UpdateRigWeightBasedOnEquipment();
    }

    // ------------------------------------------------------------------------------------
    // MOVEMENT
    // ------------------------------------------------------------------------------------

    private void Move()
    {
        float horizontalInput = InputManager.instance.MoveInput.x;
        float verticalInput = InputManager.instance.MoveInput.y;

        float movementSpeed = 3f;
        float moveX = Mathf.MoveTowards(playerAnimations.GetMoveX(), horizontalInput, Time.deltaTime * movementSpeed);
        float moveZ = Mathf.MoveTowards(playerAnimations.GetMoveZ(), verticalInput, Time.deltaTime * movementSpeed);

        if (isKnifeAiming || isPistolAiming)
        {
            isRunning = false;
        }
        else
        {
            isRunning = (InputManager.instance.RunningValue > 0) && (Mathf.Abs(moveZ) > 0f || Mathf.Abs(moveX) > 0f);
        }

        playerAnimations.SetMovementParameters(moveX, moveZ);
        playerAnimations.RunningAnimation(isRunning);
    }

    private void Rotation()
    {
        if (playerAnimations.GetMoveX() != 0 || playerAnimations.GetMoveZ() != 0 || isFlashlight || isPistolAiming || isKnifeAiming)
        {
            Vector3 directionToTarget = playerTarget.position - transform.position;
            directionToTarget.y = 0f;

            targetAngleY = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            currentAngleY = Mathf.SmoothDampAngle(currentAngleY, targetAngleY, ref angleDifference, turnSmoothness);
            transform.rotation = Quaternion.Euler(0f, currentAngleY, 0f);
        }
    }

    // ------------------------------------------------------------------------------------
    // INPUT
    // ------------------------------------------------------------------------------------

    private void HandleInput()
    {
        InputFlashlight();
        InputKnife();
        InputPistol();
        CheckInventoryInput();
    }

    // ------------------------------------------------------------------------------------
    // UI PANELS
    // ------------------------------------------------------------------------------------

    private void CheckInventoryInput()
    {
        if (InputManager.instance.InventoryTriggered)
        {
            SetInventoryPanel();
            InputManager.instance.SetInventoryTriggered(false);
        }
    }

    private void CheckPauseInput()
    {
        if (InputManager.instance.PauseTriggered)
        {
            SetPausePanel();
            InputManager.instance.SetPauseTriggered(false);
        }
    }

    public void SetInventoryPanel()
    {
        isInventoryPanel = !isInventoryPanel;
        UIManager.instance.SetInventoryPanelActive(isInventoryPanel);
    }

    public void SetPausePanel()
    {
        isPausePanel = !isPausePanel;
        UIManager.instance.SetPausePanelActive(isPausePanel);
    }

    // ------------------------------------------------------------------------------------
    // FLASHLIGHT
    // ------------------------------------------------------------------------------------

    private void InputFlashlight()
    {
        if (InputManager.instance.FlashlightTriggered)
        {
            if (InventoryManager.instance.hasFlashlight)
            {
                FlashlightSwitch();
            }
            InputManager.instance.SetFlashlightTriggered(false);
        }
    }

    public void FlashlightSwitch()
    {
        isFlashlight = !isFlashlight;
        playerAnimations.SetFlashlightIdle(isFlashlight);
    }

    public void SetFlashlighState(bool state)
    {
        this.isFlashlight = state;
        InventoryManager.instance.hasFlashlight = state;
        playerAnimations.SetFlashlightIdle(state);
    }

    public void FlashlightSwitchAniamtionEvent()
    {
        InventoryManager.instance.SetFlashlightActive(isFlashlight);
        AudioManager.instance.Play(getFlashlightSound);
    }

    public void FlashlightDropAnimationEvent()
    {
        InventoryManager.instance.SetFlashlightActiveOnPlayer(false);
        AudioManager.instance.Play(getFlashlightSound);
    }

    public void FlashlightSystemAnimationEvent()
    {
        bool isFlashlightEnabled = isFlashlight;
        AudioManager.instance.Play(flashlightSound);
        flashlightSystem.ToggleFlashlight(isFlashlightEnabled);
    }

    // ------------------------------------------------------------------------------------
    // SWITCH WEAPON WITH DELAY
    // ------------------------------------------------------------------------------------

    private IEnumerator SwitchWeaponWithDelay(System.Action switchAction)
    {
        yield return new WaitForSeconds(0.7f);
        switchAction();
    }

    // ------------------------------------------------------------------------------------
    // KNIFE
    // ------------------------------------------------------------------------------------

    private void InputKnife()
    {
        if (!isPistolAiming && !isKnifeAiming && !isKnifeAttack && !isShoot)
        {
            if (InputManager.instance.Weapon1Triggered)
            {
                if (InventoryManager.instance.hasKnife)
                {
                    KnifeSwitch();
                }
                InputManager.instance.SetWeapon1Triggered(false);
            }
        }
    }

    public void KnifeSwitch()
    {
        if (isPistol)
        {
            isPistol = false;
            PistolStateLogic();

            StartCoroutine(SwitchWeaponWithDelay(() => {
                isKnife = true;
                KnifeStateLogic();
            }));
        }
        else
        {
            isKnife = !isKnife;
            KnifeStateLogic();
        }
    }

    public void SetKnifeState(bool active)
    {
        isKnife = active;

        if (isPistol && active)
        {
            StartCoroutine(SwitchWeaponWithDelay(KnifeStateLogic));

            isPistol = false;
            PistolStateLogic();
        }
        else
        {
            KnifeStateLogic();
        }
    }

    private void KnifeStateLogic()
    {
        playerAnimations.SetKnifeIdle(isKnife);
        UIManager.instance.SetKnifePanelActive(isKnife);
    }

    public void KnifeSwitchAniamtionEvent()
    {
        InventoryManager.instance.SetKnifeActive(isKnife); 
        AudioManager.instance.Play(getKnifeSound);
    }

    public void KnifeDropAnimationEvent()
    {
        InventoryManager.instance.SetKnifeActive(false);
        AudioManager.instance.Play(getKnifeSound);
    }

    public void KnifeSystem()
    {
        if (isInventoryPanel)
        {
            isKnifeAiming = false;
            playerAnimations.SetKnifeAiming(isKnifeAiming);
            cameraController.AimingKnife(isKnifeAiming);
            return;
        }

        isKnifeAiming = (InputManager.instance.AimingValue > 0);
        cameraController.AimingKnife(isKnifeAiming);

        playerAnimations.SetKnifeAiming(isKnifeAiming);

        if (isKnifeAiming && InputManager.instance.CombatValue > 0)
        {
            playerAnimations.SetKnifeAttack();
            InputManager.instance.ResetCombatValue();
        }
    }

    public void KnifeAttackAniamtionEvent()
    {
        AudioManager.instance.Play(knifeSound);
    }

    public void EnableKnifeCollider()
    {
        if (knifeSystem != null)
        {
            knifeSystem.EnableCollider();
        }
    }

    public void DisableKnifeCollider()
    {
        if (knifeSystem != null)
        {
            knifeSystem.DisableCollider();
        }
    }

    public bool IsKnifeActive()
    {
        return isKnife;
    }

    // ------------------------------------------------------------------------------------
    // PISTOL
    // ------------------------------------------------------------------------------------

    private void InputPistol()
    {
        if (!isPistolAiming && !isKnifeAiming && !isKnifeAttack && !isShoot)
        {
            if (InputManager.instance.Weapon2Triggered)
            {
                if (InventoryManager.instance.hasPistol)
                {
                    PistolSwitch();
                }
                InputManager.instance.SetWeapon2Triggered(false);
            }
        }
    }

    public void PistolSwitch()
    {
        if (isKnife)
        {
            isKnife = false;
            KnifeStateLogic();

            StartCoroutine(SwitchWeaponWithDelay(() => {
                isPistol = true;
                PistolStateLogic();
            }));
        }
        else
        {
            isPistol = !isPistol;
            PistolStateLogic();
        }
    }

    public void SetPistolState(bool active)
    {
        isPistol = active;

        if (isKnife && active)
        {
            StartCoroutine(SwitchWeaponWithDelay(PistolStateLogic));

            isKnife = false;
            KnifeStateLogic();
        }
        else
        {
            PistolStateLogic();
        }
    }

    private void PistolStateLogic()
    {
        playerAnimations.SetPistolIdle(isPistol);
        UIManager.instance.SetAmmoPanelActive(isPistol);
    }

    public void PistolSwitchAniamtionEvent()
    {
        InventoryManager.instance.SetPistolActive(isPistol);
        AudioManager.instance.Play(getPistolSound);
    }

    public void PistolDropAnimationEvent()
    {
        InventoryManager.instance.SetPistolActive(false);
        AudioManager.instance.Play(getPistolSound);
    }

    public void PistolSystem()
    {
        if (isInventoryPanel)
        {
            isPistolAiming = false;
            cameraController.AimingPistol(isPistolAiming);
            playerAnimations.CancelPistolAnimation();
            UIManager.instance.SetCrosshairPanelActive(isPistolAiming);
            return;
        }

        isPistolAiming = (InputManager.instance.AimingValue > 0);
        cameraController.AimingPistol(isPistolAiming);
        UIManager.instance.SetCrosshairPanelActive(isPistolAiming);

        isShoot = (InputManager.instance.CombatValue > 0);

        if (isPistolAiming && isShoot)
        {
            if (!isPistolShoot)
            {
                isPistolShoot = true;
                if (pistolSystem.GetCurrentAmmo() > 0)
                {
                    pistolSystem.PistolShoot();
                }
                else
                {
                    AudioManager.instance.Play(magazineSound);
                }
                InvokeRepeating("Shoot", 0f, fireRate);
            }
        }
        else if (!isShoot && isPistolShoot)
        {
            isPistolShoot = false;
            CancelInvoke("Shoot");
        }

        playerAnimations.SetPistolAttack(isPistolAiming, isShoot && pistolSystem.GetCurrentAmmo() > 0, false);

        if (InputManager.instance.ReloadTriggered && pistolSystem.CanReload())
        {
            StartCoroutine(ReloadAfterAnimation());
            InputManager.instance.SetReloadTriggered(false);
        }
    }

    private IEnumerator ReloadAfterAnimation()
    {
        playerAnimations.SetPistolAttack(isPistolAiming, false, true);

        float reloadAnimationDuration = playerAnimations.GetReloadAnimationDuration();

        yield return new WaitForSeconds(reloadAnimationDuration);

        pistolSystem.PistolReloading();
    }

    public void PistolReloadAniamtionEvent()
    {
        AudioManager.instance.Play(reloadSound);
    }

    private void Shoot()
    {
        if (pistolSystem.GetCurrentAmmo() > 0)
        {
            pistolSystem.PistolShoot();
        }
        else
        {
            AudioManager.instance.Play(magazineSound);
        }
    }

    public void PistolShootAniamtionEvent()
    {
        if (pistolSystem.GetCurrentAmmo() > 0)
        {
            cameraController.ScreenShake();
            AudioManager.instance.Play(shootSound);
        }
    }

    public bool IsPistolActive()
    {
        return isPistol;
    }

    // ------------------------------------------------------------------------------------
    // RIG WEIGHT
    // ------------------------------------------------------------------------------------

    private void UpdateRigWeightBasedOnEquipment()
    {
        float pistolSpineRigWeight = 0f;
        float flashlightSpineRigWeight = 0f;
        float knifeSpineRigWeight = 0f;

        if (!isRunning)
        {
            if (isKnifeAiming && isKnife)
            {
                knifeSpineRigWeight = 1f;
            }

            if (isPistolAiming && isPistol)
            {
                pistolSpineRigWeight = 1f;
            }

            if (isFlashlight)
            {
                flashlightSpineRigWeight = 1f;
            }
        }

        playerAnimations.UpdateRigWeight(pistolSpineRigWeight, flashlightSpineRigWeight, knifeSpineRigWeight);
    }
}
