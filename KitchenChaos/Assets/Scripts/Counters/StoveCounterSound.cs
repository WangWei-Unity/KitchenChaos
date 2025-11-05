using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    /// <summary>
    /// 进度变化时的警告音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgreeAmout = .5f;

        //炸焦状态 且 进度到达一半 才开始播放音效
        playWarningSound = e.progressNormalized >= burnShowProgreeAmout && stoveCounter.isFried();
    }

    /// <summary>
    /// 状态切换时的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool playeSound = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;
        if (playeSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0)
            {
                float warningSoundTimerMax = .2f;
                warningSoundTimer = warningSoundTimerMax;

                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
    }
}
