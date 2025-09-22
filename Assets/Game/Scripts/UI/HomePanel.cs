using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomePanel : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Awake() {
        playButton.onClick.AddListener(PlayButton);
    }
    private void OnDestroy() {
        playButton.onClick.RemoveListener(PlayButton);
    }
   
    public void PlayButton()
    {
        SceneManager.LoadScene("Game");
    }
}
