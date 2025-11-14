using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRender;
    [SerializeField] private MeshRenderer bodyMeshRender;

    private Material material;

    void Awake()
    {
        //复制新的材质 这个可以单独改某一个角色的颜色
        material = new Material(headMeshRender.material);
        headMeshRender.material = material;
        bodyMeshRender.material = material;
    }

    void Start()
    {
        
    }

    /// <summary>
    /// 设置玩家颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
