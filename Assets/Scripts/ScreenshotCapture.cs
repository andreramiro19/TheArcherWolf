using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

            // SuperSize = 1 (1920x1080 se o jogo estiver em FullHD)
            ScreenCapture.CaptureScreenshot(fileName, 1);

            Debug.Log("Screenshot salva: " + fileName);
        }
    }
}