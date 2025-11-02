using NUnit.Framework.Internal;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    /// <summary>
    /// 玩家与柜子的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        //如果柜子上没有物体 才可以让玩家放置物体
        if (!HasKitchenObject())
        {
            //只有玩家当前携带了物体 才可以放置到柜子上
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {
                
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {

            }
            //如果柜子上有物体 玩家空手
            //则将柜子上的物体移动到玩家身上
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
