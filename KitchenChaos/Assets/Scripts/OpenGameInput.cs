using UnityEngine;

/// <summary>
/// 通过该脚本 开启全局的GameInput逻辑
/// </summary>
public class OpenGameInput : MonoBehaviour
{
    void Update()
    {
        GameInput.Instance.GetMovementVectorNormalized();
    }
}
