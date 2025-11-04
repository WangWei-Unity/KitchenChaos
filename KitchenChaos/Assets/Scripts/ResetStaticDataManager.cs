using UnityEngine;

public class ResetStaticDataManager
{
    /// <summary>
    /// 每次退出游戏 重置场景中的单例音效事件
    /// </summary>
    private void Awake()
    {
        BaseCounter.ResetStaticData();
        CuttingCounter.ResetStaticData();
        TrashCounter.ResetStaticData();
    }
}
