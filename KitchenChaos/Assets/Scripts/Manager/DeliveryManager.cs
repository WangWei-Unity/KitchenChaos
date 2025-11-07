using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    //创建订单后ui的事件
    public event EventHandler OnRecipeSpawned;
    //完成订单后ui的事件
    public event EventHandler OnRecipeCompleted;
    //成功提交订单播放音效的事件
    public event EventHandler OnRecipeSuccess;
    //失败提交订单播放音效的事件
    public event EventHandler OnRecipeFailed;

    private static DeliveryManager instance;

    public static DeliveryManager Instance => instance;

    //需要处理的订单
    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount = 0;

    void Awake()
    {
        instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    /// <summary>
    /// 处理订单生成逻辑
    /// </summary>
    void Update()
    {
        if (!IsServer) return;

        //游戏没有开始 不生成订单
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (waitingRecipeSOList.Count < waitingRecipesMax)
        {
            spawnRecipeTimer -= Time.deltaTime;
            if (spawnRecipeTimer <= 0f)
            {
                spawnRecipeTimer = spawnRecipeTimerMax;

                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.RecipeSOList.Count);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    /// <summary>
    /// 通过ClientRpc让客户端订单与服务器一致
    /// </summary>
    /// <param name="waitingRecipeSOIndex"></param>
    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {

        RecipeSO waitingRecipeSO = recipeListSO.RecipeSOList[waitingRecipeSOIndex];
        
        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 处理订单交付逻辑
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    public bool DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            //只有配方和盘子上物体数量相同 才有可能提交成功
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentMatchesRecipe = true;
                //现在在遍历配方里的物体 是否盘子上都有
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        //组成物体匹配得上
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    //如果没有找到匹配的 说明配方和盘子里的物体无法匹配
                    if (!ingredientFound)
                    {
                        plateContentMatchesRecipe = false;
                    }
                }

                //成功匹配
                if (plateContentMatchesRecipe)
                {
                    DeliveryCorrectRecipeServerRpc(i);
                    return true;
                }
            }
        }

        //没有可以匹配的
        DeliveryIncorrectRecipeServerRpc();
        return false;
    }

    /// <summary>
    /// 通过ServerRpc让 服务器收到的所有客户端的提交正确的信息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DeliveryCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliveryCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    /// <summary>
    /// 通过ClientRpc 让服务器把接收到的正确提交信息 传输给所有客户端
    /// </summary>
    [ClientRpc]
    private void DeliveryCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        successfulRecipesAmount++;

        //匹配后将该配方移除
        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 通过ServerRpc让 服务器收到的所有客户端的提交不正确的信息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DeliveryIncorrectRecipeServerRpc()
    {
        DeliveryIncorrectRecipeClientRpc();
    }

    /// <summary>
    /// 通过ClientRpc 让服务器把接收到的不正确提交信息 传输给所有客户端
    /// </summary>
    [ClientRpc]
    private void DeliveryIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 获得当前所有等待订单
    /// </summary>
    /// <returns></returns>
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
    
    /// <summary>
    /// 传给外部成功提交的订单个数
    /// </summary>
    /// <returns></returns>
    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }
}
