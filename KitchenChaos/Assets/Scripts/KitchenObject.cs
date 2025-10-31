using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    //private ClearCounter clearCounter;
    private IKitchenObjectParent kitchenObjectParent;


    /// <summary>
    ///  给外部相关厨房物体的SO数据
    /// </summary>
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    /// <summary>
    /// 给外部用来设置该厨房物体的 父柜子
    /// </summary>
    /// <param name="kitchenObjectParent"></param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        //离开当前柜子 就要把当前柜子上的物体（数据）清空
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("这个柜子上已经有其它物体了");
        }

        //更新当前放置柜子上的 物体数据
        kitchenObjectParent.SetKitchenObject(this);

        this.transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetClearCounter()
    {
        return kitchenObjectParent;
    }
}
