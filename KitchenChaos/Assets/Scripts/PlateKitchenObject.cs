using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    //组装物体的显隐事件
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    //允许放置到盘子上的物体
    [SerializeField] private List<KitchenObjectSO> vaildKitchenObjectSOList;

    //盘子上当前放的所有物体
    private List<KitchenObjectSO> kitchenObjectSOList;

    /// <summary>
    /// 如果不重写Awake的话 无法实例化followTransform数据 导致无法正常使用Plate
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    /// <summary>
    /// 添加放置物体
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <returns></returns>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        //如果该物体不允许放置在盘子上 直接退出函数
        if (!vaildKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        //如果已经有这个物体了 就无法添加到列表里
        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        //如果没有 才可以添加
        else
        {
            AddIngredientServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));

            return true;
        }
    }

    /// <summary>
    /// 通过ServerRpc传给服务器添加配料的消息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    /// <summary>
    /// 通过ClientRpc传给所有客户端添加配料
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectFromIndex(kitchenObjectSOIndex);
        kitchenObjectSOList.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            kitchenObjectSO = kitchenObjectSO
        });
    }

    /// <summary>
    /// 获取餐盘上物体的数据
    /// </summary>
    /// <returns></returns>
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}
