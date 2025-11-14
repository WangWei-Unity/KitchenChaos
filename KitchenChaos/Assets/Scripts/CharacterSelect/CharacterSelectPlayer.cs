using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            if (playerData.clientId == 0) return;
            KitchenGameMultiplayer.Instance.KickPlayer(playerData.clientId);
            KitchenGameLobby.Instance.KickLobby(playerData.playerId.ToString());
        });
    }

    void Start()
    {
        //连入玩家数量变化 执行的函数
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        //当角色准备的数量改变
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        //只在服务器可以看见踢人选项
        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

        UpdatePlayer();
    }

    /// <summary>
    /// 当角色准备的数量改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    /// <summary>
    /// 当玩家数量变化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }
    
    /// <summary>
    /// 检查当前玩家的索引是否连接
    /// </summary>
    private void UpdatePlayer()
    {
        if (KitchenGameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));

            playerNameText.text = playerData.playerName.ToString();

            playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[YourClassName] Show() 被调用但对象已销毁，忽略。");
            return;
        }
    
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[YourClassName] Hide() 被调用但对象已销毁，忽略。");
            return;
        }
        
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        //连入玩家数量变化 执行的函数
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        //当角色准备的数量改变
        CharacterSelectReady.Instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
    }
}
