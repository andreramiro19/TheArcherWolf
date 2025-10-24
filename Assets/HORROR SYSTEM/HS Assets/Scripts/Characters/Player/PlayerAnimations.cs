using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Player Animator")]
    public Animator playerAnimator;

    [Header("Player Animation Rigs")]
    public Rig flashlightSpineRig;
    public Rig knifeSpineRig;
    public Rig pistolSpineRig;
    public float lerpSpeed;

    public void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    public void SetMovementParameters(float moveX, float moveZ)
    {
        playerAnimator.SetFloat("MoveX", moveX);
        playerAnimator.SetFloat("MoveZ", moveZ);
    }

    public float GetMoveX()
    {
        return playerAnimator.GetFloat("MoveX");
    }

    public float GetMoveZ()
    {
        return playerAnimator.GetFloat("MoveZ");
    }

    public void RunningAnimation(bool isRunning)
    {
        playerAnimator.SetBool("Running", isRunning);
    }


    public void SetFlashlightIdle(bool isFlashlight)
    {
        playerAnimator.SetBool("IdleFlashlight", isFlashlight);
    }

    public void SetPistolIdle(bool isIdle)
    {
        playerAnimator.SetBool("IdlePistol", isIdle);
    }

    public void SetPistolAttack(bool isAiming, bool isShoot, bool isReloading)
    {
        playerAnimator.SetBool("PistolAiming", isAiming);
        playerAnimator.SetBool("PistolShoot", isShoot);
        playerAnimator.SetBool("PistolReload", isReloading);
    }

    public void CancelPistolAnimation()
    {
        playerAnimator.SetBool("PistolAiming", false);
        playerAnimator.SetBool("PistolShoot", false);
        playerAnimator.SetBool("PistolReload", false);
    }

    public float GetReloadAnimationDuration()
    {
        AnimationClip[] clips = playerAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "Pistol_Reload")
            {
                return clip.length;
            }
        }
        return 0f;
    }

    public void SetKnifeIdle(bool isIdle)
    {
        playerAnimator.SetBool("IdleKnife", isIdle);
    }

    public void SetKnifeAiming(bool isAimingKnife)
    {
        playerAnimator.SetBool("KnifeAiming", isAimingKnife);
    }

    public void SetKnifeAttack()
    {
        playerAnimator.SetTrigger("KnifeAttack");
    }

    public void SetDeath()
    {
        playerAnimator.SetTrigger("Death");
    }

    public void ResetRigWeights()
    {
        SetRigWeight(pistolSpineRig, 0f);
        SetRigWeight(flashlightSpineRig, 0f);
        SetRigWeight(knifeSpineRig, 0f);
    }

    public void UpdateRigWeight(float pistolSpineRigWeight, float flashlightSpineRigWeight, float knifeSpineRigWeight)
    {
        UpdateRigWeight(pistolSpineRig, pistolSpineRigWeight);
        UpdateRigWeight(flashlightSpineRig, flashlightSpineRigWeight);
        UpdateRigWeight(knifeSpineRig, knifeSpineRigWeight);
    }

    private void UpdateRigWeight(Rig rig, float targetWeight)
    {
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, lerpSpeed * Time.deltaTime);
        if (Mathf.Abs(rig.weight - targetWeight) < 0.01f)
        {
            rig.weight = targetWeight;
        }
    }

    private void SetRigWeight(Rig rig, float weight)
    {
        rig.weight = weight;
    }
}
