using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

/// <summary>
/// 所有按键类型
/// </summary>
public enum BTN_TYPE
{
    MOVE_UP,
    MOVE_DOWN,
    MOVE_LEFT,
    MOVE_RIGHT,

    INTERACT,
    INTERACT_ALTERNATE,
    PAUSE,
}

public class OptionsUI : MonoBehaviour
{
    private static OptionsUI instance;

    public static OptionsUI Instance => instance;

    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;

    //绑定按键的按钮
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAlternateButton;
    [SerializeField] private Button pauseButton;


    //绑定按键的文字
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAlternateText;
    [SerializeField] private TextMeshProUGUI pauseText;

    //记录当前改哪一个键
    private BTN_TYPE nowType;

    private GameInputInfo gameInputInfo;

    //每次改建后 传给外部的事件
    public event EventHandler OnBindingRebind;

    void Awake()
    {
        instance = this;

        //每次激活就读取按键数据
        gameInputInfo = DataManager.Instance.GetGameInputInfo();

        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        //按键修改
        moveUpButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.MOVE_UP);
        });
        moveDownButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.MOVE_DOWN);
        });
        moveLeftButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.MOVE_LEFT);
        });
        moveRightButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.MOVE_RIGHT);
        });
        interactButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.INTERACT);
        });
        interactAlternateButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.INTERACT_ALTERNATE);
        });
        pauseButton.onClick.AddListener(() =>
        {
            ChangeBtn(BTN_TYPE.PAUSE);
        });
    }

    void Start()
    {
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;

        UpdateVisual();
        UpdateBtnInfo();
        Hide();
    }

    /// <summary>
    /// 按下ESC也可以退出界面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 音效/背景音乐文字的改变
    /// </summary>
    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects : " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music : " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);
        UpdateBtnInfo();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        soundEffectsButton.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 确定改建类型
    /// </summary>
    /// <param name="type"></param>
    private void ChangeBtn(BTN_TYPE type)
    {
        nowType = type;
        //得到一次任意键输入
        InputSystem.onAnyButtonPress.CallOnce(ChangeBtnReally);
    }

    /// <summary>
    /// 真正的改建逻辑
    /// </summary>
    /// <param name="control"></param>
    private void ChangeBtnReally(InputControl control)
    {
        string[] strs = control.path.Split('/');
        string path = "<" + strs[1] + ">/" + strs[2];

        //只允许键盘输入
        if (strs[1] != "Keyboard") return;

        switch (nowType)
        {
            case BTN_TYPE.MOVE_UP:
                gameInputInfo.moveUp = path;
                break;
            case BTN_TYPE.MOVE_DOWN:
                gameInputInfo.moveDown = path;
                break;
            case BTN_TYPE.MOVE_LEFT:
                gameInputInfo.moveLeft = path;
                break;
            case BTN_TYPE.MOVE_RIGHT:
                gameInputInfo.moveRight = path;
                break;
            case BTN_TYPE.INTERACT:
                gameInputInfo.interact = path;
                break;
            case BTN_TYPE.INTERACT_ALTERNATE:
                gameInputInfo.interactAlternate = path;
                break;
            case BTN_TYPE.PAUSE:
                gameInputInfo.pause = path;
                break;
        }

        UpdateBtnInfo();

        //让玩家产生改键效果
        Player.Instance.ChangeInput();

        OnBindingRebind?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 更新键位显示信息
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

        //每次更新键位信息都进行一次存储
        DataManager.Instance.SaveGameInputInfo(gameInputInfo);
    }
}
