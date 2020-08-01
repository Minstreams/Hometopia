using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;


public class CamMoveTemp : MonoBehaviour
{
    private Camera cam;
    private const float sqrt2 = 1.41421f;
    private void Start()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        Debug.Log("TouchSimulation Enabled!");
        cam = transform.GetChild(0).GetComponent<Camera>();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += OnFingerUp;
    }
    private float lastDistance = 1;
    private float lastCamSize = 5;
    private void Update()
    {
        Vector2 drag = Vector2.zero;
        switch (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count)
        {
            case 1:
                drag = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].delta;
                break;
            case 2:
                float dis =
                    Vector2.Distance
                    (
                    UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                    UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition
                    );
                cam.orthographicSize = Mathf.Clamp(lastDistance / dis * lastCamSize, 1, 5);
                drag =
                    0.5f *
                    (
                    UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].delta +
                    UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].delta
                    );
                break;
            default:
                return;
        }
        drag.y *= sqrt2;
        float rotY = transform.localRotation.eulerAngles.y;
        float sinY = Mathf.Sin(rotY);
        float cosY = Mathf.Cos(rotY);
        Vector3 dir = new Vector3(-drag.x * cosY + drag.y * sinY, 0, -drag.y * cosY - drag.x * sinY) * cam.orthographicSize * 2 / Screen.currentResolution.height;
        transform.Translate(dir);
    }
    private void OnFingerUp(Finger finger)
    { }
    private void OnFingerDown(Finger finger)
    {
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            float dis = Vector2.Distance(
                UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition
                );
            lastDistance = dis;
            lastCamSize = cam.orthographicSize;
        }
    }
#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.Label("Touched Fingers:" + UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count);
        GUILayout.Label("TouchScreen:" + UnityEngine.InputSystem.Touchscreen.current);
    }
#endif
}
