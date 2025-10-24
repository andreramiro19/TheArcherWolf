using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineVirtualCamera defaultCamera;
    public CinemachineVirtualCamera PistolCamera;
    public CinemachineVirtualCamera KnifeCamera;
    public CinemachineImpulseSource screenShake;

    [Header("Camera Settings")]
    public float minAngle = -60f;
    public float maxAngle = 60f;

    [Header("Target Settings")]
    public Transform cameraTarget;
    public float cameraTargetSpeed = 5f;
    public float turnSmooth = 0.1f;

    [Header("Rotation Settings")]
    public float YRotationSpeed = 1f;
    public float XRotationSpeed = 1f;

    private float smoothX;
    private float smoothY;
    private float smoothXVelocity;
    private float smoothYVelocity;
    private float lookAngle;
    private float tiltAngle;

    private void LateUpdate()
    {
        PerformCameraRotation();
    }

    private void PerformCameraRotation()
    {
        if (Time.timeScale > 0)
        {
            float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 5f);

            float mouseX = InputManager.instance.LookInput.x;
            float mouseY = InputManager.instance.LookInput.y;

            smoothX = Mathf.SmoothDamp(smoothX, mouseX, ref smoothXVelocity, turnSmooth);
            smoothY = Mathf.SmoothDamp(smoothY, mouseY, ref smoothYVelocity, turnSmooth);

            lookAngle += smoothX * YRotationSpeed * sensitivity;
            tiltAngle -= smoothY * XRotationSpeed * sensitivity;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);

            cameraTarget.rotation = Quaternion.Euler(tiltAngle, lookAngle, 0f);
        }
    }

    public void AimingKnife(bool isAiming)
    {
        if (isAiming)
        {
            defaultCamera.Priority = 0;
            KnifeCamera.Priority = 10;
        }
        else
        {
            defaultCamera.Priority = 10;
            KnifeCamera.Priority = 0;
        }
    }

    public void AimingPistol(bool isAiming)
    {
        if (isAiming)
        {
            defaultCamera.Priority = 0;
            PistolCamera.Priority = 10;
        }
        else
        {
            defaultCamera.Priority = 10;
            PistolCamera.Priority = 0;
        }
    }

    public void ScreenShake()
    {
        if (screenShake != null)
        {
            screenShake.GenerateImpulse();
        }
    }
}