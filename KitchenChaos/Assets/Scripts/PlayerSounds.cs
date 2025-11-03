using System.Threading;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    private float footstepTimer;
    private float footstepTimerMax = .1f;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        // footstepTimer -= Time.deltaTime;
        // if(footstepTimer <= 0)
        // {
        //     footstepTimer = footstepTimerMax;

        //     if (player.IsWalking())
        //     {
        //         float volume = 1f;
        //         SoundManager.Instance.PlayFootstepSound(this.transform.position, volume);
        //     }
        // }
        if (player.IsWalking())
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                footstepTimer = footstepTimerMax;

                float volume = 1f;
                SoundManager.Instance.PlayFootstepSound(this.transform.position, volume);
            }
        }
    }
}
