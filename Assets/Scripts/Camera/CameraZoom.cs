using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 1f; // Speed of zooming
    [SerializeField] private float minZoom = 5f;  // Minimum zoom level
    [SerializeField] private float maxZoom = 20f; // Maximum zoom level

    private UnityEngine.Camera _camera; // Explicitly use Unity's Camera

    private void Start()
    {
        _camera = GetComponent<UnityEngine.Camera>();
        if (_camera == null)
        {
            Debug.LogError("CameraZoom script must be attached to a GameObject with a Unity Camera component.");
        }
    }

    public void OnZoom(InputValue value)
    {
        if (_camera == null) return;

        float scrollValue = value.Get<float>();
        if (_camera.orthographic)
        {
            // Adjust orthographic size for orthographic projection
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scrollValue * zoomSpeed, minZoom, maxZoom);
        }
        else
        {
            // Adjust field of view for perspective projection
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - scrollValue * zoomSpeed, minZoom, maxZoom);
        }
    }
}
