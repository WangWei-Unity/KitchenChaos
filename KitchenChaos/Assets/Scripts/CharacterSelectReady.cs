using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    private static CharacterSelectReady instance;

    public static CharacterSelectReady Instance => instance;

    //当玩家准备的数量变化时 执行的事件
    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> playerReadyDictionary;

    void Awake()
    {
        instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    /// <summary>
    /// 发送准备就绪的信息
    /// </summary>
    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    /// <summary>
    /// 通过ServerRpc传给服务器客户端玩家准备的信息 当所有玩家准备好 就可以开始游戏
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
        SetPlayerReadyClientRpc(rpcParams.Receive.SenderClientId);
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
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    /// <summary>
    /// 通过ClientRpc发给所有客户端 玩家的准备信息
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 当前玩家是否准备就绪
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
