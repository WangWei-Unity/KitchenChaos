using System;
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

            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }
}
