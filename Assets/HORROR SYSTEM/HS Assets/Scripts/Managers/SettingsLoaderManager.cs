using UnityEngine;

public class SettingsLoaderManager : MonoBehaviour
{
    public ControlMenuLogicHandler controlMenuLogicHandler;
    public AudioMenuLogicHandler audioMenuLogicHandler;

    void Start()
    {
        controlMenuLogicHandler.LoadRebindingSettings();
        audioMenuLogicHandler.LoadAudioSettings();
    }
}
