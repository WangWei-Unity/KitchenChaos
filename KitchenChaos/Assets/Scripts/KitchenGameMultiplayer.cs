using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_AMOUNT = 4;

    private static KitchenGameMultiplayer instance;

    public static KitchenGameMultiplayer Instance => instance;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;

    //玩家试图连接游戏时执行的事件
    public event EventHandler OnTryingToJoinGame;
    //玩家试图连接游戏失败时执行的事件
    public event EventHandler OnFailedToJoinGame;


    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 开启主机
    /// </summary>
    public void StartHost()
    {
        //当新玩家中途加入游戏处理的事件
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// 新玩家加入游戏处理的逻辑
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
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartClient();
    }
    
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
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
}
