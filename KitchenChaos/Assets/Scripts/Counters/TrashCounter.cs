using System;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    //当物体被丢掉时 播放的音效
    public static event EventHandler OnAnyObjectTrashed;

    new public static void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }

    /// <summary>
    /// 玩家与垃圾桶的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

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
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
    }
}
