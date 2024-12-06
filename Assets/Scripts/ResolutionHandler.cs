using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Camera))]
public class ResolutionHandler : MonoBehaviour
{
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset GraphicsSettings;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        SetFieldOfView();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void OnDestroy()
    {
        GraphicsSettings.renderScale = 1f;
    }

    private void LateUpdate()
    {
        SetFieldOfView();

        GraphicsSettings.renderScale = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(1280f, 2340f, Screen.height));
    }

    private void SetFieldOfView()
    {
        float screenRatio = Screen.height / (float)Screen.width;
        if (!ScreenManager.Default.SlotsAreVisible)
        {
            _camera.fieldOfView -= 5f;
            _camera.transform.position += _camera.transform.up * -0.35f;
        }
        //if (screenRatio > 1.9f)
        {
            float modifier = Mathf.Clamp(screenRatio - 1.9f, -0.1f, 0.5f);
            if (!ScreenManager.Default.SlotsAreVisible)
            {
                _camera.fieldOfView *= 1f + modifier * 0.5f;
                _camera.transform.position += _camera.transform.up * modifier * 2.5f;
            }
            else
            {
                _camera.transform.position += _camera.transform.up * modifier * 0.25f;
            }
        }
    }
}