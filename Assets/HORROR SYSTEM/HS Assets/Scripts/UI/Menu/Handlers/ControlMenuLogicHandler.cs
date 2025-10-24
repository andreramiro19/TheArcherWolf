using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ControlMenuLogicHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    public InputActionAsset playerControls;

    [Header("Rebinding Settings")]
    public Button[] rebindButtons;
    public string[] actionsToRebind;

    [Header("Mouse Sensitivity Settings")]
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;

    private void Start()
    {
        LoadRebindingSettings();
        InitializeRebindButtonsLogic();
        InitializeMouseSensitivitySliderLogic();
    }

    private void InitializeRebindButtonsLogic()
    {
        for (int i = 0; i < rebindButtons.Length; i++)
        {
            int index = i;
            UpdateButtonText(index);
            rebindButtons[i].onClick.AddListener(() => StartRebinding(index));
        }
    }

    public void LoadRebindingSettings()
    {
        foreach (var action in playerControls)
        {
            string bindingJson = PlayerPrefs.GetString(action.id.ToString(), string.Empty);
            if (!string.IsNullOrEmpty(bindingJson))
            {
                action.LoadBindingOverridesFromJson(bindingJson);
            }
        }
    }

    private void UpdateButtonText(int buttonIndex)
    {
        if (buttonIndex >= actionsToRebind.Length) return;

        var actionNameWithBinding = actionsToRebind[buttonIndex];
        var actionAndBinding = actionNameWithBinding.Split('/');
        var actionName = actionAndBinding[0];
        var bindingName = actionAndBinding.Length > 1 ? actionAndBinding[1] : null;

        var action = playerControls.FindAction(actionName);
        if (action != null)
        {
            int bindingIndex = GetBindingIndex(action, bindingName);
            if (bindingIndex != -1)
            {
                var binding = action.bindings[bindingIndex];
                var buttonText = rebindButtons[buttonIndex].GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = InputControlPath.ToHumanReadableString(
                        binding.effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                }
            }
        }
    }

    private int GetBindingIndex(InputAction action, string bindingName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (bindingName == null ||
                (!action.bindings[i].isPartOfComposite && action.bindings[i].name.Equals(bindingName, StringComparison.OrdinalIgnoreCase)) ||
                (action.bindings[i].isPartOfComposite && action.bindings[i].name.Equals(bindingName, StringComparison.OrdinalIgnoreCase)))
            {
                return i;
            }
        }
        return -1;
    }

    public void StartRebinding(int actionIndex)
    {
        var actionNameWithBinding = actionsToRebind[actionIndex];
        var actionAndBinding = actionNameWithBinding.Split('/');
        var actionName = actionAndBinding[0];
        var bindingName = actionAndBinding.Length > 1 ? actionAndBinding[1] : null;

        var action = playerControls.FindAction(actionName);
        if (action == null)
            return;

        int bindingIndex = GetBindingIndex(action, bindingName);
        if (bindingIndex == -1)
            return;

        var buttonText = rebindButtons[actionIndex].GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = "Waiting for input";
        }

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .OnComplete(operation =>
            {
                action.Enable();
                PlayerPrefs.SetString(action.id.ToString(), action.SaveBindingOverridesAsJson());
                operation.Dispose();
                UpdateButtonText(actionIndex);
                EventSystem.current.SetSelectedGameObject(null);
            })
            .Start();
    }

    private void InitializeMouseSensitivitySliderLogic()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 5f);
        sensitivitySlider.value = savedSensitivity;
        sensitivityValueText.text = $"{(int)sensitivitySlider.value}";

        sensitivitySlider.onValueChanged.AddListener(
            sensitivity => {
                sensitivityValueText.text = $"{(int)sensitivity}";
                PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
                PlayerPrefs.Save();
            }
        );
    }
}