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
            //如果柜子上有物体 玩家手上的东西可以和柜子上的东西组合在一起的逻辑
            if (player.HasKitchenObject())
            {
                //如果玩家手上拿着盘子 就把柜子上的物体移动到玩家的盘子上
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //成功添加才销毁桌子上的物体
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenGameMultiplayer.Instance.DestroyKitchenObject(GetKitchenObject());
                    }
                }
                else
                {
                    //如果玩家手上拿的不是盘子 而柜子上是盘子 将玩家手上的物体放置到柜子上
                    if(GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            KitchenGameMultiplayer.Instance.DestroyKitchenObject(player.GetKitchenObject());
                        }
                    }
                }
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
