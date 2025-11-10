using System;
using UnityEngine;

public class PauseMultiPlayersUI : MonoBehaviour
{
    void Start()
    {
        KitchenGameManager.Instance.OnMultiplayerGamePaused += KitchenGameManager_OnMultiplayerGamePaused;
        KitchenGameManager.Instance.OnMultiplayerGameUnpaused += KitchenGameManager_OnMultiplayerGameUnpaused;

        Hide();
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnMultiplayerGamePaused(object sender, EventArgs e)
    {
        Show();
    }
    
    /// <summary>
    /// 继续游戏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnMultiplayerGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
