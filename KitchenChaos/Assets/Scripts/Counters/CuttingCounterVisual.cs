using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private string CUT = "Cut";

    [SerializeField] private CuttingCounter cuttingCounter;

    private Animator animator;

    void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    void Start()
    {
        cuttingCounter.OnCut += CuttingCounter_OnCut;
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
