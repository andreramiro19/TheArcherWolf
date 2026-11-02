using UnityEngine;

public class LampInteraction : InteractiveObject
{
    public Material lampMaterial;
    public Light lampPointLight;

    [Space]
    public string emissionKeyword = "_EMISSION";

    [Space]
    public string lampSound;

    protected bool isLmap = true;

    private void Start()
    {
        lampPointLight.enabled = isLmap;
        UpdateLampState();
    }

    public override void Interact(GameObject player = null)
    {
        isLmap = !isLmap;
        AudioManager.instance.Play(lampSound);

        UpdateLampState();
    }

    private void UpdateLampState()
    {
        lampPointLight.enabled = isLmap;

        if (lampPointLight.enabled)
        {
            lampMaterial.EnableKeyword(emissionKeyword);
        }
        else
        {
            lampMaterial.DisableKeyword(emissionKeyword);
        }
    }
}
