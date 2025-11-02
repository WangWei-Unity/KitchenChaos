using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject
{
    //柜子上物体的预设体
    public Transform prefab;
    //对应图片
    public Sprite sprite;
    //对应名字
    public string objectName;
}
