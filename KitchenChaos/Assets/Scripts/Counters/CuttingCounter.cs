using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CuttingCounter : BaseCounter, IHasProgress
{
    //当在任意柜子上进行切割时 都会播放音效的事件
    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }


    //当切片时 进度条变化的事件
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    //处理切割动画播放的事件 刚放上物体的时候不播放切割动画
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgess;

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
                //只有玩家当前携带的物体可以被切片 才可以放置到柜子上
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgess = 0;

                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    //初始化进度条
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)cuttingProgess / cuttingRecipeSO.cuttingProgessMax
                    });
                }
            }
            else
            {

            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                //如果玩家手上拿着盘子 就把柜子上的物体移动到玩家的盘子上
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //成功添加才销毁桌子上的物体
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
            }
            //如果柜子上有物体 玩家空手
            //则将柜子上的物体移动到玩家身上
            else
            {
                if (cuttingProgess == 0 || !HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
                {
                    GetKitchenObject().SetKitchenObjectParent(player);
                }
            }
        }
    }

    /// <summary>
    /// 切割物体的交互
    /// </summary>
    /// <param name="player"></param>
    public override void InteractAlternate(Player player)
    {
        //如果有物体 并且 当前物体可以被切片 才进行切片操作
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            cuttingProgess++;

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)cuttingProgess / cuttingRecipeSO.cuttingProgessMax
            });

            //只有切割达到要求次数 才会切片成功
            if (cuttingProgess >= cuttingRecipeSO.cuttingProgessMax)
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    /// <summary>
    /// 判断当前柜子上的物体是否可以切片
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    /// <summary>
    /// 获得当前物体切割后的数据资源
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        return null;
    }
    
    /// <summary>
    /// 获得当前物体 归属的CuttingRecipeSO的数据
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
