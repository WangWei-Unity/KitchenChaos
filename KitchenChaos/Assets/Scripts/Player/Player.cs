using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    //当任意新玩家创建时 调用的事件
    public static event EventHandler OnAnyPlayerSpawned;
    //当任意玩家拾取物体时 调用的事件
    public static event EventHandler OnAnyPickedSomething;
    
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    private static Player instance;

    public static Player LocalInstance => instance;

    void Awake()
    {
        //instance = this;
    }

    //拾取物体时的音效事件
    public event EventHandler OnPickupSomething;

    //用于处理玩家靠近柜子时 柜子的闪烁
    public event EventHandler<OnSelectedCounterChangeEventArgs> OnSelectedCounterChange;
    public class OnSelectedCounterChangeEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
        public OnSelectedCounterChangeEventArgs(BaseCounter selectedCounter)
        {
            this.selectedCounter = selectedCounter;
        }
    }

    [SerializeField] private float moveSpeed = 7;
    [SerializeField] private float roundSpeed = 7;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;

    //玩家子物体对应的模型位置
    //用于处理旋转逻辑
    [SerializeField] private Transform playerVisualTransform;
    [SerializeField] private PlayerVisual playerVisual;
    //拾取物体放置的位置
    [SerializeField] private Transform kitchenObjectPoint;
    [SerializeField] private List<Vector3> spawnPositionList;

    private bool isWalking = false;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private PlayerInput playerInput;

    void Start()
    {
        playerInput = this.GetComponent<PlayerInput>();

        playerInput.actions = DataManager.Instance.GetActionAsset();
        playerInput.actions.Enable();

        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    public override void OnNetworkSpawn()
    {
        //只有是自己本身的网络生成的对象 才会实例化本地的Player数据
        if (IsOwner)
        {
            instance = this;
        }

        this.transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    /// <summary>
    /// 玩家断开连接后的处理
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    /// <summary>
    /// 与切割柜子的交互 f键
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        //不在游戏时间不可以交互
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    /// <summary>
    /// 靠近柜台按e键后 做的交互处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        //不在游戏时间不可以交互
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        GameInput.Instance.GetMovementVectorNormalized();

        //不在游戏时间不可以交互
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            isWalking = false;
            return;
        }

        //HandleMovementServerAuth();
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    /// <summary>
    /// 处理与柜台的交互逻辑
    /// </summary>
    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        //通过射线检测 判断前方是否有柜子
        float interactDistance = 2f;
        RaycastHit raycastHit;
        if (Physics.Raycast(this.transform.position, lastInteractDir, out raycastHit, interactDistance, counterLayerMask))
        {
            //不要用“危险”的标签来做判断
            //TryGetComponent相比GetComponent帮助我们进行了判空处理
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //得到ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    /// <summary>
    /// 帧同步
    /// </summary>
    private void HandleMovementServerAuth()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        HandleMovementServerRpc(inputVector);
    }

    /// <summary>
    /// 通过ServerRpc实现客户端移动的同步
    /// </summary>
    /// <param name="inputVector"></param>
    [ServerRpc]
    private void HandleMovementServerRpc(Vector2 inputVector)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        float playerRadius = .7f;
        float playerHeight = 2f;
        float moveDistance = moveSpeed * Time.deltaTime;
        bool canMove = !Physics.CapsuleCast(this.transform.position, this.transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            //尝试只在x方向上移动
            //这是为了防止向斜方向进行移动时，由于检测到物体而导致无法正常移动
            //正常情况下，应该沿着平行于被检测到物体的方向进行移动
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            bool canMoveX = (moveDir.x < -.5f || moveDir.x > .5f) && !Physics.CapsuleCast(this.transform.position, this.transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            //尝试只在z方向上移动
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
            bool canMoveZ = (moveDir.z < -.5f || moveDir.z > .5f) && !Physics.CapsuleCast(this.transform.position, this.transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

            if (canMoveX)
            {
                //可以在x方向上移动
                canMove = true;
                moveDir = moveDirX;
            }
            if (canMoveZ)
            {
                //可以在z方向上移动
                canMove = true;
                moveDir = moveDirZ;
            }
        }

        if (canMove)
        {
            //移动
            this.transform.Translate(moveSpeed * moveDir * Time.deltaTime);
        }

        //只有在移动状态才会 旋转
        if (moveDir != Vector3.zero)
        {
            playerVisualTransform.transform.rotation = Quaternion.Lerp(playerVisualTransform.transform.rotation, Quaternion.LookRotation(moveDir), roundSpeed * Time.deltaTime);
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        float playerRadius = .7f;
        float moveDistance = moveSpeed * Time.deltaTime;
        bool canMove = !Physics.BoxCast(this.transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionsLayerMask);

        if (!canMove)
        {
            //尝试只在x方向上移动
            //这是为了防止向斜方向进行移动时，由于检测到物体而导致无法正常移动
            //正常情况下，应该沿着平行于被检测到物体的方向进行移动
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            bool canMoveX = (moveDir.x < -.5f || moveDir.x > .5f) && !Physics.BoxCast(this.transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);
            //尝试只在z方向上移动
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
            bool canMoveZ = (moveDir.z < -.5f || moveDir.z > .5f) && !Physics.BoxCast(this.transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMoveX)
            {
                //可以在x方向上移动
                canMove = true;
                moveDir = moveDirX;
            }
            if (canMoveZ)
            {
                //可以在z方向上移动
                canMove = true;
                moveDir = moveDirZ;
            }
        }

        if (canMove)
        {
            //移动
            this.transform.Translate(moveSpeed * moveDir * Time.deltaTime);
        }

        //只有在移动状态才会 旋转
        if (moveDir != Vector3.zero)
        {
            playerVisualTransform.transform.rotation = Quaternion.Lerp(playerVisualTransform.transform.rotation, Quaternion.LookRotation(moveDir), roundSpeed * Time.deltaTime);
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    /// <summary>
    /// 设置当前射线检测到的柜子 并执行切换柜子时的事件
    /// </summary>
    /// <param name="selectedCounter"></param>
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChange?.Invoke(this, new OnSelectedCounterChangeEventArgs(selectedCounter));
    }

    /// <summary>
    /// 当厨房物体的父对象发生改变时候 将该父对象的生成物体的位置传给厨房物体
    /// </summary>
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectPoint;
    }

    /// <summary>
    /// 当有新物体放到玩家身上时 及时更改kitchenObject的数据
    /// </summary>
    /// <param name="kitchenObject"></param>
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickupSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
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
    /// 当有物体离开玩家时 清空玩家上的物体数据
    /// </summary>
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    /// <summary>
    /// 判断当前玩家身上是否已经放了物体了
    /// 玩家空手才可以放物体
    /// </summary>
    /// <returns></returns>
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    /// <summary>
    /// 让玩家产生改键效果
    /// </summary>
    public void ChangeInput()
    {
        //改键就是改变 我们PlayerInput上关联的输入配置信息
        playerInput.actions = DataManager.Instance.GetActionAsset();
        playerInput.actions.Enable();
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public override void OnDestroy()
    {
        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
    }
}
