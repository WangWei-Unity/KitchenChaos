using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    private static KitchenGameManager instance;

    public static KitchenGameManager Instance => instance;

    //状态切换时的事件
    public event EventHandler OnStateChanged;
    //暂停时处理的事件
    public event EventHandler OnGamePaused;
    //取消暂停时处理的事件
    public event EventHandler OnGameUnpaused;
    //当本地玩家准备好后处理的事件
    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);

    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(10f);
    private float gamePlayingTimerMax = 90f;

    private bool isGamePause = false;
    private Dictionary<ulong, bool> playerReadyDictionary;

    void Awake()
    {
        instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State nowValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 按下交互键 退出等待开始阶段
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }
    
    /// <summary>
    /// 通过ServerRpc传给服务器客户端玩家准备的信息 当所有玩家准备好 就可以开始游戏
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        //检测当前所有的玩家是否都已经连接
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || playerReadyDictionary[clientId] == false)
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            state.Value = State.CountdownToStart;
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
        if (state.Value == State.GamePlaying || state.Value == State.CountdownToStart)
        {
            TogglePauseGame();
        }
    }

    void Update()
    {
        if (!IsServer) return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value <= 0)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value <= 0)
                {
                    state.Value = State.GameOver;
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
        return state.Value == State.GamePlaying;
    }

    /// <summary>
    /// 判断当前是不是倒计时阶段
    /// </summary>
    /// <returns></returns>
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    /// <summary>
    /// 判断当前是不是等待开始阶段
    /// </summary>
    /// <returns></returns>
    public bool IsWaitingToStarttActive()
    {
        return state.Value == State.WaitingToStart;
    }

    /// <summary>
    /// 得到当前倒计时剩余时间 便于ui更新
    /// </summary>
    /// <returns></returns>
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    /// <summary>
    /// 判断当前是不是游戏结束状态
    /// </summary>
    /// <returns></returns>
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    /// <summary>
    /// 传给外部 本地玩家是否准备就绪
    /// </summary>
    /// <returns></returns>
    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    /// <summary>
    /// 传给外部 当前游戏已经进行的时间的归一化数据 用于显示游戏倒计时ui
    /// </summary>
    /// <returns></returns>
    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (float)gamePlayingTimer.Value / gamePlayingTimerMax;
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
