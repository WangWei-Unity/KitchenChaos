using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 池子的容器——列容器
/// </summary>
public class PoolData
{
    //对象挂载的父节点
    public GameObject fatherObj;
    //存储对象的容器
    public List<GameObject> poolList;

    public PoolData(GameObject obj, GameObject poolObj)
    {
        //给我们的抽屉 创建一个父对象 并且把它作为我们pool对象的子物体
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = poolObj.transform;
        poolList = new List<GameObject>();
        PushObj(obj);
    }

    /// <summary>
    /// 往抽屉里面压东西
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        //存起来
        poolList.Add(obj);
        //设置父对象
        obj.transform.parent = fatherObj.transform;
        //失活 让其隐藏
        obj.SetActive(false);
    }

    /// <summary>
    /// 从抽屉里面取东西
    /// </summary>
    /// <returns></returns>
    public GameObject GetObj()
    {
        GameObject obj = null;
        //取出第一个
        obj = poolList[0];
        poolList.RemoveAt(0);
        //激活 让其显示
        obj.SetActive(true);
        //断开了父子关系
        obj.transform.parent = null;
        return obj;
    }
}
