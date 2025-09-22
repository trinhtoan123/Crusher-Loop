using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public static SceneReloader instance;
    private static int previousSceneIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object persistent
        }
    }

    // Call this function to reload the current scene with a loading screen in between
    public static void ReloadSceneWithLoading()
    {
        previousSceneIndex = SceneManager.GetActiveScene().buildIndex; // Store current scene index
        SceneManager.LoadScene("LoadingScene"); // First, go to the loading scene
    }

    public void LoadPreviousScene()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Simulate loading time
        SceneManager.LoadScene(previousSceneIndex); // Reload the original scene
    }
}
