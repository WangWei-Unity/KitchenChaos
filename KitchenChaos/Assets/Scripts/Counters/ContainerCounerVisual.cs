using System.Security.Cryptography;
using UnityEngine;

public class ContainerCounerVisual : MonoBehaviour
{
    private string OPEN_CLOSE = "OpenClose";

    [SerializeField] private ContainerCounter containerCounter;

    private Animator animator;

    void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    void Start()
    {
        containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;
    }

    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
