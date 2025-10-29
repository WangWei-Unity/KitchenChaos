using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 7;
    [SerializeField]
    private float roundSpeed = 7;

    //玩家子物体对应的模型位置
    //用于处理旋转逻辑
    public Transform playerVisual;

    private bool isWalking = false;

    void Update()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

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
            bool canMoveX = !Physics.CapsuleCast(this.transform.position, this.transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            //尝试只在z方向上移动
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
            bool canmoveZ = !Physics.CapsuleCast(this.transform.position, this.transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

            if (canMoveX)
            {
                //可以在x方向上移动
                canMove = true;
                moveDir = moveDirX;
            }
            if (canmoveZ)
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
            playerVisual.transform.rotation = Quaternion.Lerp(playerVisual.transform.rotation, Quaternion.LookRotation(moveDir), roundSpeed * Time.deltaTime);
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
