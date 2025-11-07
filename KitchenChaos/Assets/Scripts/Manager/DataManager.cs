using UnityEngine;
using UnityEngine.InputSystem;

public class DataManager
{
    private static DataManager instance = new DataManager();

    public static DataManager Instance => instance;

    
    //数据存储
    private const string MOVE_UP = "moveUp";
    private const string MOVE_DOWN = "moveDown";
    private const string MOVE_LEFT = "moveLeft";
    private const string MOVE_RIGHT = "moveRight";
    private const string INTERACT = "interact";
    private const string INTERACT_ALTERNATE = "interactAlternate";
    private const string PAUSE = "pause";

    //输入的数据
    private GameInputInfo gameInputInfo;
    public GameInputInfo GameInputInfo => gameInputInfo;
    private string jsonStr;
    private const string JSON_GAME_INUPT = "GameInput";

    private DataManager()
    {
        //获取存储的按键数据
        gameInputInfo = new GameInputInfo();
        gameInputInfo = GetGameInputInfo();

        jsonStr = Resources.Load<TextAsset>(JSON_GAME_INUPT).text;
    }

    /// <summary>
    /// 改建（json数据）
    /// </summary>
    /// <returns></returns>
    public InputActionAsset GetActionAsset()
    {
        //上键
        string str = jsonStr.Replace("<moveUp>", gameInputInfo.moveUp);
        //下键
        str = str.Replace("<moveDown>", gameInputInfo.moveDown);
        //左键
        str = str.Replace("<moveLeft>", gameInputInfo.moveLeft);
        //右键
        str = str.Replace("<moveRight>", gameInputInfo.moveRight);
        //交互
        str = str.Replace("<interact>", gameInputInfo.interact);
        //切割
        str = str.Replace("<interactAlternate>", gameInputInfo.interactAlternate);
        //暂停
        str = str.Replace("<pause>", gameInputInfo.pause);

        return InputActionAsset.FromJson(str);
    }

    /// <summary>
    /// 存储按键数据
    /// </summary>
    /// <param name="gameInputInfo"></param>
    public void SaveGameInputInfo(GameInputInfo gameInputInfo)
    {
        PlayerPrefs.SetString(MOVE_UP, gameInputInfo.moveUp);
        PlayerPrefs.SetString(MOVE_DOWN, gameInputInfo.moveDown);
        PlayerPrefs.SetString(MOVE_LEFT, gameInputInfo.moveLeft);
        PlayerPrefs.SetString(MOVE_RIGHT, gameInputInfo.moveRight);
        PlayerPrefs.SetString(INTERACT, gameInputInfo.interact);
        PlayerPrefs.SetString(INTERACT_ALTERNATE, gameInputInfo.interactAlternate);
        PlayerPrefs.SetString(PAUSE, gameInputInfo.pause);
    }

    /// <summary>
    /// 读取按键数据
    /// </summary>
    /// <returns></returns>
    public GameInputInfo GetGameInputInfo()
    {
        gameInputInfo.moveUp = PlayerPrefs.GetString(MOVE_UP, "<Keyboard>/w");
        gameInputInfo.moveDown = PlayerPrefs.GetString(MOVE_DOWN, "<Keyboard>/s");
        gameInputInfo.moveLeft = PlayerPrefs.GetString(MOVE_LEFT, "<Keyboard>/a");
        gameInputInfo.moveRight = PlayerPrefs.GetString(MOVE_RIGHT, "<Keyboard>/d");
        gameInputInfo.interact = PlayerPrefs.GetString(INTERACT, "<Keyboard>/e");
        gameInputInfo.interactAlternate = PlayerPrefs.GetString(INTERACT_ALTERNATE, "<Keyboard>/f");
        gameInputInfo.pause = PlayerPrefs.GetString(PAUSE, "<Keyboard>/escape");

        return gameInputInfo;
    }
}
