using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    private static CharacterSelectReady instance;

    public static CharacterSelectReady Instance => instance;

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
}
