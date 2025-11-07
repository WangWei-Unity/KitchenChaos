using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    private static KitchenGameManager instance;

    public static KitchenGameManager Instance => instance;

    //状态切换时的事件
    public event EventHandler OnStateChanged;
    //暂停时处理的事件
    public event EventHandler OnGamePaused;
    //取消暂停时处理的事件
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float countdownToStartTimer = 1f;

    private float gamePlayingTimer = 300f;
    private float gamePlayingTimerMax = 300f;

    private bool isGamePause = false;

    void Awake()
    {
        instance = this;
        state = State.WaitingToStart;
    }

    void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

        //测试代码 直接进入倒计时环节
        state = State.CountdownToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 按下交互键 退出等待开始阶段
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }  
    }

    /// <summary>
    /// 按下暂停键的处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        //当前状态为游戏阶段或倒计时阶段 才可以暂停
        if (state == State.GamePlaying || state == State.CountdownToStart)
        {
            TogglePauseGame();
        }
    }

    void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer <= 0)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer <= 0)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }
    }

    /// <summary>
    /// 不在GamePlaying的时候 人物不可以移动交互
    /// </summary>
    /// <returns></returns>
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    /// <summary>
    /// 判断当前是不是倒计时阶段
    /// </summary>
    /// <returns></returns>
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    /// <summary>
    /// 判断当前是不是等待开始阶段
    /// </summary>
    /// <returns></returns>
    public bool IsWaitingToStarttActive()
    {
        return state == State.WaitingToStart;
    }

    /// <summary>
    /// 得到当前倒计时剩余时间 便于ui更新
    /// </summary>
    /// <returns></returns>
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    /// <summary>
    /// 判断当前是不是游戏结束状态
    /// </summary>
    /// <returns></returns>
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    /// <summary>
    /// 传给外部 当前游戏已经进行的时间的归一化数据 用于显示游戏倒计时ui
    /// </summary>
    /// <returns></returns>
    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (float)gamePlayingTimer / gamePlayingTimerMax;
    }

    /// <summary>
    /// 暂停游戏的处理
    /// </summary>
    public void TogglePauseGame()
    {
        isGamePause = !isGamePause;
        if (isGamePause)
        {
            Time.timeScale = 0;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
