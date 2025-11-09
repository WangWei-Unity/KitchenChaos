using System;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        Hide();
    }

    /// <summary>
    /// 显示等待其它玩家进入游戏的界面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsLocalPlayerReady())
        {
            Show();
        }
    }

    /// <summary>
    /// 当进入倒计时阶段的时候 隐藏界面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
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
