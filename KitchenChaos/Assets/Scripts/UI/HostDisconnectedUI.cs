using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectedUI : MonoBehaviour
{
    private static HostDisconnectedUI instance;
    public static HostDisconnectedUI Instance => instance;

    [SerializeField] private Button mainMenuButton;
    private ulong cachedHostId;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cachedHostId = NetworkManager.ServerClientId;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        Hide();
    }

    /// <summary>
    /// 主机玩家断开连接后其它客户端的处理
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        //主机断开连接 会让客户端本身也断开连接 也就是此时clientId会等于本地id
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Show();
        }
    }

    public void Show()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[HostDisconnectedUI] UI 已被销毁，忽略 Show 调用。");
            return;
        }
        
        gameObject.SetActive(true);
        mainMenuButton.Select();
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
