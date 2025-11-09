using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    private static KitchenGameMultiplayer instance;

    public static KitchenGameMultiplayer Instance => instance;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;

    void Awake()
    {
        instance = this;
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
