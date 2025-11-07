using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";

    [SerializeField]
    private Player player;

    //玩家身上的Animator组件
    private Animator animator;

    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        
        animator.SetBool(IS_WALKING, player.IsWalking());
    }
}
