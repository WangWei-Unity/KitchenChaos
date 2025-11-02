using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    private enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForaward,
        CameraForawardInverted,
    }

    [SerializeField] private Mode mode;

    void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case Mode.CameraForaward:
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForawardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}
