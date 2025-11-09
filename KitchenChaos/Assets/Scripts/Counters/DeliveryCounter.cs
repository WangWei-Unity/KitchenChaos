using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    private static DeliveryCounter instance;
    public static DeliveryCounter Instance => instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 玩家与柜子的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            //只允许提交盘子上的物体
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                
                //提交成功才销毁物体
                if (DeliveryManager.Instance.DeliveryRecipe(plateKitchenObject))
                {
                    KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                }
            }
        }
    }
}
