using UnityEngine;

public class PlayerDisable : MonoBehaviour
{
    public static PlayerDisable instance;

    public bool isDisable = false;

    private void Awake()
    {
        instance = this;
    }

    public void DisablePlayer()
    {
        if (!isDisable)
        {
            isDisable = true;
            Time.timeScale = 0;
        }
    }

    public void EnablePlayer()
    {
        isDisable = false;
        Time.timeScale = 1;
    }
}
