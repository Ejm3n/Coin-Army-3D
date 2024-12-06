using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineResolutionHandler : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;

    private void Awake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        SetFieldOfView();
    }

    private void SetFieldOfView()
    {
        float screenRatio = (1.0f * Screen.height) / (1.0f * Screen.width);
        if (1.7f < screenRatio && screenRatio < 1.8f)
        {
            _camera.m_Lens.FieldOfView = 60;
        }
        if (2.1f < screenRatio && screenRatio < 2.2f)
        {
            _camera.m_Lens.FieldOfView = 75;
        }
    }
}