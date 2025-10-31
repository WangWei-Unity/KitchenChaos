using UnityEngine;

/// <summary>
/// 
/// </summary>
public interface IKitchenObjectParent
{
    /// <summary>
    /// 当厨房物体的父对象发生改变时候 将该父对象的生成物体的位置传给厨房物体
    /// </summary>
    public Transform GetKitchenObjectFollowTransform();

    /// <summary>
    /// 当有新物体放到桌子上时 及时更改kitchenObject的数据
    /// </summary>
    /// <param name="kitchenObject"></param>
    public void SetKitchenObject(KitchenObject kitchenObject);

    /// <summary>
    /// 得到当前的kitchenObject
    /// </summary>
    /// <returns></returns>
    public KitchenObject GetKitchenObject();

    /// <summary>
    /// 当有物体离开柜子时 清空规则上的物体数据
    /// </summary>
    public void ClearKitchenObject();

    /// <summary>
    /// 判断当前的柜子上是否已经放了物体了
    /// 空柜子上才可以放物体
    /// </summary>
    /// <returns></returns>
    public bool HasKitchenObject();
}
