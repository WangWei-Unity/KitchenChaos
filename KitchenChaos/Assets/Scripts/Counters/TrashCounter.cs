using UnityEngine;

public class TrashCounter : BaseCounter
{
    /// <summary>
    /// 玩家与垃圾桶的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            player.GetKitchenObject().DestroySelf();
        }
    }
}
