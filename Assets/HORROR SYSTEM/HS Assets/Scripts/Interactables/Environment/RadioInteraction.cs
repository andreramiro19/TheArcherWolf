using UnityEngine;

public class RadioInteraction : InteractiveObject
{
    public AudioSource radioAudioSource;
    public string radioButtonSound;

    protected bool isRadio = false;

    private void Start()
    {
        radioAudioSource.Stop();
    }

    public override void Interact(GameObject player = null)
    {
        isRadio = !isRadio;
        AudioManager.instance.Play(radioButtonSound);

        UpdateRadioState();
    }

    private void UpdateRadioState()
    {
        if (isRadio)
        {
            radioAudioSource.Play();
        }
        else
        {
            radioAudioSource.Stop();
        }
    }
}
