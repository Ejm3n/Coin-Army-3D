using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IMouseEventProxyTarget
{
    public bool Exists();
    public Vector3 GetPosition();
    public Vector2 GetSize();
    public void OnPointerDown();
    public void OnPointerDrag();
    public void OnPointerUp();
}

public class MouseEventProxy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public IMouseEventProxyTarget Target;

    private bool _isPressed;

    private RectTransform _ownRect;

    void Start()
    {
        _ownRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!Target.Exists())
        {
            Destroy(gameObject);
            return;
        }

        if (_isPressed)
        {
            Target.OnPointerDrag();
        }

        _ownRect.position = Camera.main.WorldToScreenPoint(Target.GetPosition());
        _ownRect.sizeDelta = Target.GetSize();
        _ownRect.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData data)
    {
        _isPressed = true;
        Target.OnPointerDown();
    }

    public void OnPointerUp(PointerEventData data)
    {
        _isPressed = false;
        Target.OnPointerUp();
    }
}
