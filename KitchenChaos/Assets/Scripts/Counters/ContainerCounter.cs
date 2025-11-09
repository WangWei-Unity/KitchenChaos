using System;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    //处理玩家交互时 播放柜台动画的事件
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    /// <summary>
    /// 玩家与柜子的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        //如果玩家身上没有物体 才可以拿一个新物体给玩家
        if (!player.HasKitchenObject())
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            InteractLogicSeverRpc();
        }
    }

    /// <summary>
    /// 通过ServerRpc得到客户端播放了动画的信息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractLogicSeverRpc()
    {
        InteractLogicClientRpc();
    }
    
    /// <summary>
    /// 通过ClientRpc将播放动画的信息传递给所有客户端
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }
}
