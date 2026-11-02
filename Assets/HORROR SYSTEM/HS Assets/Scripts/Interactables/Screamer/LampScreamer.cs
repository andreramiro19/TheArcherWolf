using UnityEngine;

public class LampScreamer : InteractiveObject
{
    public Material lampMaterial;
    public Light lampPointLight;

    [Space]
    public string emissionKeyword = "_EMISSION";

    [Space]
    public string brokenLightSound;

    private void Start()
    {
        lampPointLight.enabled = true;
    }

    public override void Interact(GameObject player = null)
    {
        lampPointLight.enabled = false;
        lampMaterial.DisableKeyword(emissionKeyword);
        AudioManager.instance.Play(brokenLightSound);
    }
}
