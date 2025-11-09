using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    //private ClearCounter clearCounter;
    private IKitchenObjectParent kitchenObjectParent;
    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = this.GetComponent<FollowTransform>();
    }


    /// <summary>
    /// 给外部相关厨房物体的SO数据
    /// </summary>
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    /// <summary>
    /// 给外部用来设置该厨房物体的 父对象（柜子或玩家）
    /// </summary>
    /// <param name="kitchenObjectParent"></param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    /// <summary>
    /// 通过serverRpc让客户端得到对象状态 发送给服务器
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    /// <summary>
    /// 通过ClientRpc把对象状态再发给所有客户端来同步
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        //离开当前父对象 就要把当前父对象上的物体（数据）清空
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("这个柜子上已经有其它物体了");
        }

        //更新当前放置父对象上的 物体数据
        kitchenObjectParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    public IKitchenObjectParent GetClearCounter()
    {
        return kitchenObjectParent;
    }

    /// <summary>
    /// 处理切割物体的逻辑
    /// </summary>
    public void DestroySelf()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();

        if (netObj.IsSpawned)
        {
            netObj.Despawn(); // 从网络中移除对象
        }
        
        Destroy(gameObject);
    }

    /// <summary>
    /// 删除物体父对象上的物体数据
    /// </summary>
    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }

    /// <summary>
    /// 判断当前物体是否是盘子
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    /// <returns></returns>
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }



    /// <summary>
    /// 产生一个物体的静态方法
    /// </summary>
    /// <returns></returns>
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    /// <summary>
    /// 删除某个物体
    /// </summary>
    /// <param name="kitchenObject"></param>
    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }
}
