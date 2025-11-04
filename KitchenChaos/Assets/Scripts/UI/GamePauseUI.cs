using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;


    void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame();
        });

        optionsButton.onClick.AddListener(() =>
        {
            OptionsUI.Instance.Show();
        });
    }

    void Start()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;

        Hide();
    }

    /// <summary>
    /// 按下暂停
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnGamePaused(object sender, EventArgs e)
    {
        Show();
    }
    
    /// <summary>
    /// 取消暂停
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        resumeButton.Select();
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
