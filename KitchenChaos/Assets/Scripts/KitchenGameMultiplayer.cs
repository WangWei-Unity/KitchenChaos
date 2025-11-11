using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_AMOUNT = 4;

    private static KitchenGameMultiplayer instance;

    public static KitchenGameMultiplayer Instance => instance;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    //玩家试图连接游戏时执行的事件
    public event EventHandler OnTryingToJoinGame;
    //玩家试图连接游戏失败时执行的事件
    public event EventHandler OnFailedToJoinGame;
    //当连入玩家的数量发生变化时执行的事件
    public event EventHandler OnPlayerDataNetworkListChanged;

    private NetworkList<PlayerData> playerDataNetworkList;


    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    /// <summary>
    /// 当playerDataNetworkList数量变化时 调用的事件函数
    /// </summary>
    /// <param name="changeEvent"></param>
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 开启主机
    /// </summary>
    public void StartHost()
    {
        //当新玩家试图中途加入游戏处理的事件
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        //每当有客户端连接成功（包括主机自己），就会调用
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        //当有玩家断开连接时 传给主机
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// 当有玩家断开连接时 传给主机
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i< playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientId == clientId)
            {
                //断开连接
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 每当有客户端连接成功（包括主机自己），就会调用
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId()
        });
    }

    /// <summary>
    /// 新玩家试图加入游戏处理的逻辑
    /// </summary>
    /// <param name="connectionApprovalRequest"></param>
    /// <param name="connectionApprovalResponse"></param>
    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
        // if (KitchenGameManager.Instance.IsWaitingToStartActive())
        // {
        //     //同意连接
        //     connectionApprovalResponse.Approved = true;
        //     //同意创建玩家 必须手动创建
        //     connectionApprovalResponse.CreatePlayerObject = true;
        // }
        // else
        // {
        //     //不同意连接
        //     connectionApprovalResponse.Approved = false;
        // }
    }

    /// <summary>
    /// 开启客户端
    /// </summary>
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        //无法正常连接执行的事件
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartClient();
    }
    
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 产生一个物体的静态方法
    /// </summary>
    /// <returns></returns>
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    /// <summary>
    /// 通过ServerRpc让每个客户端都可以 传递生成物体的数据给服务器
    /// </summary>
    /// <param name="kitchenObjectSOIndex"></param>
    /// <param name="kitchenObjectParent"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectFromIndex(kitchenObjectSOIndex);

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        //下发生成信息给所有客户端
        kitchenObjectNetworkObject.Spawn(true);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    /// <summary>
    /// 得到物体所在列表的序列号 因为ServerRpc没法自动解析自定义数据KitchenObjectSO 需要int媒介
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <returns></returns>
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    /// <summary>
    /// 得到对应序列号下的KitchenObjectSO数据
    /// </summary>
    /// <param name="kitchenObjectSOIndex"></param>
    /// <returns></returns>
    public KitchenObjectSO GetKitchenObjectFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    /// <summary>
    /// 删除某个对象
    /// </summary>
    /// <param name="kitchenObject"></param>
    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    /// <summary>
    /// 通过ServerRpc通知服务器删除对象
    /// </summary>
    /// <param name="kitchenObjectNetworkObjectReference"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);

        kitchenObject.DestroySelf();
    }

    /// <summary>
    /// 通过ClientRpc发布给每个客户端清理对应父对象上的物体数据
    /// </summary>
    /// <param name="kitchenObjectNetworkObjectReference"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        kitchenObject.ClearKitchenObjectOnParent();
    }

    /// <summary>
    /// 当前想要在玩家选择界面显示的玩家是否连入网络了
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    /// <summary>
    /// 通过clientId得到玩家数据的序号
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 通过玩家id得到PlayerData的数据
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    /// <summary>
    /// 得到当前网络下的对象的PlayerData数据
    /// </summary>
    /// <returns></returns>
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// 提供给外部得到 PlayerData数据的方法
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    /// <summary>
    /// 给外部颜色的选择
    /// </summary>
    /// <param name="colorId"></param>
    /// <returns></returns>
    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    /// <summary>
    /// 发给服务器变化颜色的通知
    /// </summary>
    /// <param name="colorId"></param>
    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    /// <summary>
    /// 通过ServerRpc传给服务器玩家颜色的变化
    /// </summary>
    /// <param name="colorId"></param>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangePlayerColorServerRpc(int colorId, RpcParams rpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);

        //playerDataNetworkList[playerDataIndex]不能直接修改
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.colorId = colorId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    /// <summary>
    /// 判断当前网络里有没有这个序号的颜色了 有的话当前网络就不能变成这个颜色false
    /// </summary>
    /// <param name="colorId"></param>
    /// <returns></returns>
    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获得第一个无人使用的颜色id
    /// </summary>
    /// <returns></returns>
    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 踢掉对应玩家
    /// </summary>
    /// <param name="clientId"></param>
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
}
