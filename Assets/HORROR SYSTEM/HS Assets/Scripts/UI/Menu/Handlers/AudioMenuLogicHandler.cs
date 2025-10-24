using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioMenuLogicHandler : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Master Volume")]
    public Slider masterVolumeSlider;
    public TMP_Text masterValueText;
    private const string GeneralVolumeParam = "Master";

    [Header("Music Volume")]
    public Slider musicVolumeSlider;
    public TMP_Text musicValueText;
    private const string MusicVolumeParam = "Music";

    [Header("SFX Volume")]
    public Slider SFXVolumeSlider;
    public TMP_Text SFXValueText;
    private const string SFXVolumeParam = "SFX";

    private void Start()
    {
        LoadAudioSettings();

        ConfigureSlider(masterVolumeSlider, "MasterVolume", masterValueText, SetMasterVolumeListener);
        ConfigureSlider(musicVolumeSlider, "MusicVolume", musicValueText, SetMusicVolumeListener);
        ConfigureSlider(SFXVolumeSlider, "SFXVolume", SFXValueText, SetSFXVolumeListener);
    }

    private void ConfigureSlider(Slider slider, string volumeKey, TMP_Text volumeText, UnityEngine.Events.UnityAction<float> callback)
    {
        slider.onValueChanged.AddListener(callback);

        float savedVolume = PlayerPrefs.GetFloat(volumeKey, 100f);
        slider.value = savedVolume;
        volumeText.text = $"{(int)savedVolume}";
    }

    private void SetMasterVolumeListener(float value)
    {
        audioMixer.SetFloat(GeneralVolumeParam, ToLogarithmicScale(value));
        PlayerPrefs.SetFloat("MasterVolume", value);
        masterValueText.text = $"{(int)value}";
    }

    private void SetMusicVolumeListener(float value)
    {
        audioMixer.SetFloat(MusicVolumeParam, ToLogarithmicScale(value));
        PlayerPrefs.SetFloat("MusicVolume", value);
        musicValueText.text = $"{(int)value}";
    }

    private void SetSFXVolumeListener(float value)
    {
        audioMixer.SetFloat(SFXVolumeParam, ToLogarithmicScale(value));
        PlayerPrefs.SetFloat("SFXVolume", value);
        SFXValueText.text = $"{(int)value}";
    }

    public void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            float generalVolume = PlayerPrefs.GetFloat("MasterVolume");
            masterVolumeSlider.value = generalVolume;
            audioMixer.SetFloat(GeneralVolumeParam, ToLogarithmicScale(generalVolume));
            masterValueText.text = $"{(int)generalVolume}";
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            musicVolumeSlider.value = musicVolume;
            audioMixer.SetFloat(MusicVolumeParam, ToLogarithmicScale(musicVolume));
            musicValueText.text = $"{(int)musicVolume}";
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            SFXVolumeSlider.value = sfxVolume;
            audioMixer.SetFloat(SFXVolumeParam, ToLogarithmicScale(sfxVolume));
            SFXValueText.text = $"{(int)sfxVolume}";
        }
    }

    private float ToLogarithmicScale(float volume)
    {
        volume = Mathf.Max(0.0001f, volume / 100);
        return Mathf.Log10(volume) * 20;
    }
}