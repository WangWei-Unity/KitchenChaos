using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    private static KitchenGameManager instance;

    public static KitchenGameManager Instance => instance;

    //状态切换时的事件
    public event EventHandler OnStateChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;

    private float gamePlayingTimer = 10f;
    private float gamePlayingTimerMax = 10f;



    void Awake()
    {
        instance = this;
        state = State.WaitingToStart;
    }

    void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer <= 0)
                {
                    state = State.CountdownToStart;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
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
        Debug.Log(state);
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
}
