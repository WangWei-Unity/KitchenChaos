using System;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    void Start()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    /// <summary>
    /// 玩家连接失败
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if(messageText.text == "" || messageText.text == "[Disconnect Event][Client-0][TransportClientId-4294967296][MaxConnectionAttempts] Connection closed due to maximum connection attempts reached.")
        {
            messageText.text = "Failed to connect";
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 以防第二次进入游戏时 上一次的事件没有清理干净
    /// </summary>
    void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
    }
}
