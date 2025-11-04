using System;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    //当物体放置时播放音效的事件
    public static event EventHandler OnAnyObjectPlacedHere;

    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    /// <summary>
    /// 普通柜子与玩家间的交互
    /// </summary>
    /// <param name="player"></param>
    public virtual void Interact(Player player)
    {
        //避免子类没有重写该方法 导致调用时出现问题
        Debug.LogError("BaseCounter.Interact();");
    }

    /// <summary>
    /// 切割柜子与玩家间的交互
    /// </summary>
    /// <param name="player"></param>
    public virtual void InteractAlternate(Player player)
    {
        //避免子类没有重写该方法 导致调用时出现问题
        Debug.Log("BaseCounter.InteractAlternate();");
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

        if (kitchenObject != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
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
    /// 当有物体离开柜子时 清空柜子上的物体数据
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
