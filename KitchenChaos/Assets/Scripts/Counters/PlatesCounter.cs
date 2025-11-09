using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    //当前的盘子数
    private int platesSpawnedAmount;
    //可以产生的最大盘子数
    private int platesSpawnedAmountMax = 4;

    void Update()
    {
        if (!IsServer) return;

        if (platesSpawnedAmount < platesSpawnedAmountMax)
        {
            spawnPlateTimer += Time.deltaTime;
            //每到一定时间 就产生一个盘子
            if (spawnPlateTimer > spawnPlateTimerMax)
            {
                //重置计时器
                spawnPlateTimer = 0f;
                SpawnPlateServerRpc();
            }
        }
    }

    /// <summary>
    /// 通过ServerRpc发送服务器
    /// </summary>
    [Rpc(SendTo.Server)]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnPlateClientRpc()
    {
        platesSpawnedAmount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 玩家与柜子的交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
        //如果玩家当前空手 才可以拿盘子
        if (!player.HasKitchenObject())
        {
            //如果盘子数量大于0 才可以拿盘子
            if (platesSpawnedAmount > 0)
            {
                platesSpawnedAmount--;

                //在玩家手上产生一个盘子
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                InteractLogicClientRpc();
            }
        }
    }
    
    /// <summary>
    /// 通过ServerRpc得到客户端播放了动画的信息
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractLogicSeverRpc()
    {
        InteractLogicClientRpc();
    }
    
    /// <summary>
    /// 通过ClientRpc将播放动画的信息传递给所有客户端
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void InteractLogicClientRpc()
    {
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
