using System;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    //当物体被丢掉时 播放的音效
    public static event EventHandler OnAnyObjectTrashed;

    /// <summary>
    /// 玩家与垃圾桶的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            player.GetKitchenObject().DestroySelf();

            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }
}
