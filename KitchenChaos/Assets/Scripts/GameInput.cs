using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private static GameInput instance;

    public static GameInput Instance => instance;

    [SerializeField]
    private PlayerInput playerInput;

    Vector2 inputVector;

    void Awake()
    {
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
