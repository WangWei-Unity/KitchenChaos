using System;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI gamepadMoveText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAlternateText;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI gamepadInteractText;
    [SerializeField] private TextMeshProUGUI gamepadInteractAlternateText;
    [SerializeField] private TextMeshProUGUI gamepadPauseText;

    private GameInputInfo gameInputInfo;

    void Start()
    {
        //每次激活就读取按键数据
        gameInputInfo = DataManager.Instance.GetGameInputInfo();

        OptionsUI.Instance.OnBindingRebind += OptionsUI_OnBindingRebind;
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        UpdateBtnInfo();

        Show();
    }

    /// <summary>
    /// 游戏状态切换
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

    /// <summary>
    /// 每次外部改建 都更新一次显示信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OptionsUI_OnBindingRebind(object sender, EventArgs e)
    {
        //读取最新数据
        gameInputInfo = DataManager.Instance.GetGameInputInfo();
        UpdateBtnInfo();
    }

    /// <summary>
    /// 界面显示时 更新显示信息
    /// </summary>
    private void UpdateBtnInfo()
    {
        moveUpText.text = gameInputInfo.moveUp.Split('/')[1];
        moveDownText.text = gameInputInfo.moveDown.Split('/')[1];
        moveLeftText.text = gameInputInfo.moveLeft.Split('/')[1];
        moveRightText.text = gameInputInfo.moveRight.Split('/')[1];

        interactText.text = gameInputInfo.interact.Split('/')[1];
        interactAlternateText.text = gameInputInfo.interactAlternate.Split('/')[1];
        pauseText.text = gameInputInfo.pause.Split('/')[1];
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
