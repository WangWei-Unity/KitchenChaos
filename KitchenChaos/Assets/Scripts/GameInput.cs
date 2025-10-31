using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private static GameInput instance;

    public static GameInput Instance => instance;

    public event EventHandler OnInteractAction;

    [SerializeField] private PlayerInput playerInput;

    Vector2 inputVector;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    public Vector2 GetMovementVectorNormalized()
    {
        playerInput.onActionTriggered += (context) =>
        {
            if (context.phase == InputActionPhase.Performed)
            {
                switch (context.action.name)
                {
                    case "Move":
                        inputVector = context.ReadValue<Vector2>();
                        break;
                    case "Interact":
                        //对于EventHandler委托传入的参数
                        //第一个参数：触发事件的对象
                        //第二个参数：事件参数 在这不需要传递任何额外数据
                        OnInteractAction?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
            if(context.phase == InputActionPhase.Canceled)
            {
                inputVector = Vector2.zero;
            }
        };

        return inputVector.normalized;
    }
}
