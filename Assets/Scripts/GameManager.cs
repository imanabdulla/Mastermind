using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject startPanel, playerPanel, aiPanel;
    public event Action OnGameStart;

    public static GameManager instance;
    void Awake() => instance = this;   

    public void StartGame()
    {
        startPanel.SetActive(false);
        playerPanel.SetActive(true);
        aiPanel.SetActive(true);
        OnGameStart();
    }

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
