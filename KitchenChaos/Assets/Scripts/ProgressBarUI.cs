using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private CuttingCounter cuttingCounter;
    [SerializeField] private Image barImage;

    void Start()
    {
        cuttingCounter.OnProgressChanged += CuttingCounter_OnProgressChanged;

        barImage.fillAmount = 0;

        Hide();
    }

    /// <summary>
    /// 处理进度条
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CuttingCounter_OnProgressChanged(object sender, CuttingCounter.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;

        if(e.progressNormalized == 0f || e.progressNormalized == 1f)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
