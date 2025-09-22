using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject adsObj;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float loadingDuration = 3f;
    //[SerializeField] private GameObject AdPanel;
    [SerializeField] private GameObject TermsObj;

    [SerializeField] private bool isGameLoading = false;

    public AsyncOperation operation;
    public bool isInitialized = false;
    public bool isLoadHome = false;

    void Start()
    {
        //AdPanel.SetActive(false);
        StartCoroutine(LoadSceneWithProgress());
    }

    IEnumerator LoadSceneWithProgress()
    {
        operation = SceneManager.LoadSceneAsync("Home");
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / loadingDuration) * 1f; // Max at 80%

            progressBar.value = progress;
            loadingText.text = (progress * 100f).ToString("F0") + "%";

            if (!isGameLoading)
            {
                if (progress >= 0.25f)
                {
                    TermsObj.SetActive(true);
                }

            }
            yield return null;
        }

        if (!isGameLoading)
        {

            progressBar.value = 1f;
            loadingText.text = "100%";
            yield return null;
        }
        operation.allowSceneActivation = true; 
    }

}
