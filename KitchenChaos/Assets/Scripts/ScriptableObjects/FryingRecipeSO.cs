using UnityEngine;

[CreateAssetMenu()]
public class FryingRecipeSO : ScriptableObject
{
    //原物体
    public KitchenObjectSO input;
    //油炸后的物体
    public KitchenObjectSO output;
    //最多可以油炸的时间
    public float fryingTimerMax;
}
