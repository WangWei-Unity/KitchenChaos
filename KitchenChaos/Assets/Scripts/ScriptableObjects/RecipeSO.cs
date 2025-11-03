using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
    //配置最后提交的食物配方
    public List<KitchenObjectSO> kitchenObjectSOList;
    public string recipeName;
}
