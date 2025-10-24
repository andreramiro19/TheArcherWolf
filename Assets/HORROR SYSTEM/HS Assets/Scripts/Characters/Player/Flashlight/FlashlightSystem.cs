using UnityEngine;

public class FlashlightSystem : MonoBehaviour
{
    [Header("Light")]
    public Light spotLight = null;

    [Header("Intensity Settings")]
    public int maxIntensity;
    public float currentIntensity;
    public float intensityChange;

    [Header("Audio Settings")]
    public string reloadFlashlightSound;

    private void Start()
    {
        currentIntensity = maxIntensity;
        spotLight.intensity = currentIntensity;
        spotLight.enabled = false;
    }

    private void Update()
    {
        BatteryChange();
    }

    public void SetMaxIntensity()
    {
        currentIntensity = maxIntensity;
        AudioManager.instance.Play(reloadFlashlightSound);
    }

    public void ToggleFlashlight(bool isFlashlightOn)
    {
        if (spotLight.enabled != isFlashlightOn)
        {
            spotLight.enabled = isFlashlightOn;

            if (isFlashlightOn)
            {
                BatteryChange();
            }
        }
    }

    private void BatteryChange()
    {
        currentIntensity -= intensityChange * Time.deltaTime;
        spotLight.intensity = Mathf.Lerp(0f, maxIntensity, currentIntensity / maxIntensity);

        if (currentIntensity < 0)
        {
            currentIntensity = 0;
        }
    }
}