using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (LoadingScreen.Instance == null)
        {
            GameObject loadingScreenPrefab = Resources.Load<GameObject>("Loading_Screen");
            GameObject.Instantiate(loadingScreenPrefab);
        }

        LoadingScreen.Instance.Show();
        LoadingScreen.Instance.StartCoroutine(LoadSceneAsync(sceneName));
    }

    private static IEnumerator LoadSceneAsync(string sceneName)
    {
        LoadingScreen.Instance.Show();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            LoadingScreen.Instance.UpdateProgress(realProgress);

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        LoadingScreen.Instance.Hide();
    }
}