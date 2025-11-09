using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    //当油炸时 进度条变化的事件
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    //状态切换时 柜台效果切换的事件
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0);
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0);
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    // void Start()
    // {
    //     //一开始等待
    //     state = State.Idle;
    // }

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChange;
        burningTimer.OnValueChanged += BurningTimer_OnValueChange;
        state.OnValueChanged += State_OnValueChange;
    }

    /// <summary>
    /// 油炸时间改变时 同步fryingTimer数据 从而更新油炸进度条
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="nowValue"></param>
    private void FryingTimer_OnValueChange(float previousValue, float nowValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;

        //油炸进度条
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    /// <summary>
    /// 烧焦时间改变时 同步burningTimer数据 从而更新烧焦进度条
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="nowValue"></param>
    private void BurningTimer_OnValueChange(float previousValue, float nowValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;

        //油炸进度条
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }
    
    /// <summary>
    /// 状态改变时 同步到所有客户端
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="nowValue"></param>
    private void State_OnValueChange(State previousValue, State nowValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state.Value
        });
        
        //if(state.Value == State.Burned || state.Value == State.Idle)
        //{          
            //油炸进度条重置
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0
            });
        //}
    }

    void Update()
    {
        if (!IsServer) return;

        //油炸逻辑
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                //油炸逻辑
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value >= fryingRecipeSO.fryingTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state.Value = State.Fried;

                        SetBurningRecipeSOClientRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));

                        burningTimer.Value = 0;
                    }
                    break;
                //烧焦处理
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value >= burningRecipeSO.burningTimerMax)
                    {
                        KitchenGameMultiplayer.Instance.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }   
    }

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
                //只有玩家当前携带的物体可以被油炸 才可以放置到柜子上
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
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
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //成功添加才销毁桌子上的物体
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenGameMultiplayer.Instance.DestroyKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc();
                    }
                }
            }
            //如果柜子上有物体 玩家空手
            //则将柜子上的物体移动到玩家身上
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    /// <summary>
    /// 当柜子上物体移动到玩家手上时 服务器要处理State数据的变化
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetStateIdleServerRpc()
    {
        //拿走物体后状态机重置
        state.Value = State.Idle;
    }
    
    /// <summary>
    /// 通过ServerRpc传给服务器初始化油炸对象的信息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0;
        
        //放上可油炸物体后开始油炸
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    /// <summary>
    /// 通过ClientRpc让每个客户端都可以初始化油炸对象
    /// /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        //得到油炸对象的SO数据 便于后续油炸处理
        fryingRecipeSO = GetFryingRecipeSOWithInput(KitchenGameMultiplayer.Instance.GetKitchenObjectFromIndex(kitchenObjectSOIndex));
    }
    
    /// <summary>
    /// 通过ClientRpc让每个客户端都可以初始化烧焦对象
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        //得到油炸对象的SO数据 便于后续烧焦处理
        burningRecipeSO = GetBurningingRecipeSOWithInput(KitchenGameMultiplayer.Instance.GetKitchenObjectFromIndex(kitchenObjectSOIndex));
    }

    /// <summary>
    /// 判断当前柜子上的物体是否可以油炸
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    /// <summary>
    /// 获得当前物体油炸后的数据资源
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        return null;
    }

    /// <summary>
    /// 获得当前物体 归属的FryingRecipeSO的数据
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    /// <summary>
    /// 获得油炸后的物体 归属的BurningRecipeSO的数据
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private BurningRecipeSO GetBurningingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    /// <summary>
    /// 判断当前是不是要炸焦的状态
    /// </summary>
    public bool isFried()
    {
        return state.Value == State.Fried;
    }

    public bool isIdle()
    {
        return state.Value == State.Idle;
    }

    public bool isBurned()
    {
        return state.Value == State.Burned;
    }
}
