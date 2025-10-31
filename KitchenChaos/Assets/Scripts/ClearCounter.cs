using NUnit.Framework.Internal;
using UnityEngine;

public class ClearCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    /// <summary>
    /// 玩家与柜子的交互逻辑
    /// </summary>
    public void Interact(Player player)
    {
        //如果当前柜子上没有物体 就创建一个SO里存储的物体
        if (kitchenObject == null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab, counterTopPoint);
            kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(this);
        }
        //如果当前柜子上有物体了 就让玩家拾取这个物体
        else
        {
            kitchenObject.SetKitchenObjectParent(player);
        }
    }

    /// <summary>
    /// 当厨房物体的父对象发生改变时候 将该父对象的生成物体的位置传给厨房物体
    /// </summary>
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    /// <summary>
    /// 当有新物体放到桌子上时 及时更改kitchenObject的数据
    /// </summary>
    /// <param name="kitchenObject"></param>
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    /// <summary>
    /// 得到当前的kitchenObject
    /// </summary>
    /// <returns></returns>
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    /// <summary>
    /// 当有物体离开柜子时 清空规则上的物体数据
    /// </summary>
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    /// <summary>
    /// 判断当前的柜子上是否已经放了物体了
    /// 空柜子上才可以放物体
    /// </summary>
    /// <returns></returns>
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
