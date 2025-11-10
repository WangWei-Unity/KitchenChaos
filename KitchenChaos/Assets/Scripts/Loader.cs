using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene
    }

    private static Scene targetScene;

    /// <summary>
    /// 开始游戏用LoadingScene做过度
    /// </summary>
    /// <param name="targetScene"></param>
    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// 通过Netcode切换场景 同步到所有客户端
    /// </summary>
    /// <param name="targetScene"></param>
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
    
    /// <summary>
    /// 在LoadingScene中去切换游戏场景
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
