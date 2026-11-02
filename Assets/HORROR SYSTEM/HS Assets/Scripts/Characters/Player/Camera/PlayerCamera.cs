using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Cinemachine 3")]
    public CinemachineCamera defaultCamera;
    public CinemachineCamera pistolCamera;
    public CinemachineCamera knifeCamera;
    public CinemachineImpulseSource screenShake;

    [Header("Camera Settings")]
    public float minAngle = -60f;
    public float maxAngle = 60f;

    [Header("Target Settings")]
    public Transform cameraTarget;
    public float turnSmooth = 0.1f;

    [Header("Rotation Settings")]
    public float yRotationSpeed = 1f;
    public float xRotationSpeed = 1f;

    private float smoothX;
    private float smoothY;
    private float smoothXVelocity;
    private float smoothYVelocity;
    private float lookAngle;
    private float tiltAngle;

    private void Awake()
    {
        InitializeCinemachine3Cameras();
    }

    private void InitializeCinemachine3Cameras()
    {
        SetupCinemachineCamera(defaultCamera);
        SetupCinemachineCamera(pistolCamera);
        SetupCinemachineCamera(knifeCamera);
    }

    private void SetupCinemachineCamera(CinemachineCamera camera)
    {
        if (camera == null) return;

        camera.Priority = 0;
        camera.Follow = cameraTarget;
        camera.LookAt = cameraTarget;

        var lens = camera.Lens;
        lens.FieldOfView = 60f;
        lens.NearClipPlane = 0.1f;
        lens.FarClipPlane = 1000f;
        camera.Lens = lens;
    }

    private void LateUpdate()
    {
        PerformManualCameraRotation();
    }

    private void PerformManualCameraRotation()
    {
        if (Time.timeScale > 0 && cameraTarget != null)
        {
            float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 5f);

            float mouseX = InputManager.instance.LookInput.x;
            float mouseY = InputManager.instance.LookInput.y;

            smoothX = Mathf.SmoothDamp(smoothX, mouseX, ref smoothXVelocity, turnSmooth);
            smoothY = Mathf.SmoothDamp(smoothY, mouseY, ref smoothYVelocity, turnSmooth);

            lookAngle += smoothX * yRotationSpeed * sensitivity;
            tiltAngle -= smoothY * xRotationSpeed * sensitivity;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);

            cameraTarget.rotation = Quaternion.Euler(tiltAngle, lookAngle, 0f);
        }
    }

    public void AimingKnife(bool isAiming)
    {
        if (defaultCamera == null || knifeCamera == null) return;

        if (isAiming)
        {
            defaultCamera.Priority = 0;
            knifeCamera.Priority = 10;
        }
        else
        {
            defaultCamera.Priority = 10;
            knifeCamera.Priority = 0;
        }
    }

    public void AimingPistol(bool isAiming)
    {
        if (defaultCamera == null || pistolCamera == null) return;

        if (isAiming)
        {
            defaultCamera.Priority = 0;
            pistolCamera.Priority = 10;
        }
        else
        {
            defaultCamera.Priority = 10;
            pistolCamera.Priority = 0;
        }
    }

    public void ScreenShake()
    {
        if (screenShake != null)
        {
            screenShake.GenerateImpulse();
        }
    }

    public void SetActiveCamera(CinemachineCamera camera)
    {
        if (camera == null) return;

        if (defaultCamera != null) defaultCamera.Priority = 0;
        if (pistolCamera != null) pistolCamera.Priority = 0;
        if (knifeCamera != null) knifeCamera.Priority = 0;

        camera.Priority = 10;
    }
}