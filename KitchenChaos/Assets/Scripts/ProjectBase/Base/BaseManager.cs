using UnityEngine;

/// <summary>
/// 没继承Mono的单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseManager<T> where T : class, new()
{
    private static T instance;

    public static T GetInstance()
    {
        if(instance == null)
        {
            instance = new T();
        }
        return instance;
    }
}