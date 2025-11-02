using UnityEngine;

[CreateAssetMenu()]
public class CuttingRecipeSO : ScriptableObject
{
    //原物体
    public KitchenObjectSO input;
    //切片后的物体
    public KitchenObjectSO output;
    //最多可以切割的次数
    public int cuttingProgessMax;
}
