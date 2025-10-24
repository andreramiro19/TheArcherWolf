using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public class GraphicsMenuLogicHandler : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    public Resolution[] resolutions;

    [Header("Quality")]
    public TMP_Dropdown qualityDropdown;

    [Header("FullScreen")]
    public TMP_Dropdown fullScreenDropdown;
    private FullScreenMode[] fullScreenModes = { FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };

    private void Start()
    {
        PopulateResolutionDropdown();
        PopulateQualityDropdown();
        PopulateFullScreenDropdown();
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions.Distinct().Reverse().ToArray();

            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            foreach (Resolution resolution in resolutions)
            {
                options.Add($"{resolution.width}x{resolution.height}({(int)resolution.refreshRateRatio.value})Hz");
            }
            resolutionDropdown.AddOptions(options);

            resolutionDropdown.value = FindMatchingResolutionIndex();
            resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
        }
    }

    private int FindMatchingResolutionIndex()
    {
        int index = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    private void ApplyResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            RefreshRate refreshRate = resolution.refreshRateRatio;
            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow, resolution.refreshRateRatio);
        }
    }

    private void PopulateQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            string[] qualities = QualitySettings.names;
            List<string> options = new List<string>();

            foreach (string quality in qualities)
            {
                options.Add(quality);
            }
            qualityDropdown.AddOptions(options);

            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(ApplyQuality);
        }
    }

    private void ApplyQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void PopulateFullScreenDropdown()
    {
        if (fullScreenDropdown != null)
        {
            fullScreenDropdown.ClearOptions();
            fullScreenDropdown.AddOptions(
                fullScreenModes.Select(mode => mode.ToString()).ToList()
            );

            fullScreenDropdown.value = Array.IndexOf(fullScreenModes, Screen.fullScreenMode);
            fullScreenDropdown.onValueChanged.AddListener(
                index => Screen.fullScreenMode = fullScreenModes[index]
            );
        }
    }
}