using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    public static MusicManager Instance => instance;

    private const string PALYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    private AudioSource audioSource;
    private float volume = .3f;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();

        volume = PlayerPrefs.GetFloat(PALYER_PREFS_MUSIC_VOLUME, .3f);
        audioSource.volume = volume;
    }

    /// <summary>
    /// 调节背景音乐大小
    /// </summary>
    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0;
        }
        audioSource.volume = volume;

        PlayerPrefs.SetFloat(PALYER_PREFS_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 获得背景音乐大小
    /// </summary>
    /// <returns></returns>
    public float GetVolume()
    {
        return volume;
    }
}
