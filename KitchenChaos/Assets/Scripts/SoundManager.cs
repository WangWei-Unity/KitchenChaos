using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PALYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    private static SoundManager instance;

    public static SoundManager Instance => instance;

    void Awake()
    {
        instance = this;

        volume = PlayerPrefs.GetFloat(PALYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.Instance.OnPickupSomething += Player_OnPickupSomething;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    /// <summary>
    /// 提交正确的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlayeSound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }

    /// <summary>
    /// 提交错误的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlayeSound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
    }

    /// <summary>
    /// 切割时的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        PlayeSound(audioClipRefsSO.chop, (sender as CuttingCounter).transform.position);
    }

    /// <summary>
    /// 玩家拾取物体的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Player_OnPickupSomething(object sender, EventArgs e)
    {
        PlayeSound(audioClipRefsSO.objectPickUp, Player.Instance.transform.position);
    }

    /// <summary>
    /// 放置物体到柜子上时的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
    {
        PlayeSound(audioClipRefsSO.objectDrop, (sender as BaseCounter).transform.position);
    }
    
    /// <summary>
    /// 丢弃物体时播放的音效
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
    {
        PlayeSound(audioClipRefsSO.trash, (sender as TrashCounter).transform.position);
    }

    /// <summary>
    /// 随机播放音效
    /// </summary>
    /// <param name="audioClipArray"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    private void PlayeSound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlayeSound(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], position, volume);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    private void PlayeSound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    /// <summary>
    /// 专门处理玩家移动时的脚步声
    /// </summary>
    /// <param name="position"></param>
    public void PlayFootstepSound(Vector3 position, float volume = 1f)
    {
        PlayeSound(audioClipRefsSO.footStep, position, volume);
    }

    /// <summary>
    /// 调节音效大小
    /// </summary>
    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0;
        }

        PlayerPrefs.SetFloat(PALYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 获得音效大小
    /// </summary>
    /// <returns></returns>
    public float GetVolume()
    {
        return volume;
    }
}
