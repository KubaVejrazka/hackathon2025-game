using UnityEngine;
using UnityEngine.InputSystem;

public class Camera : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 75f; // Speed of rotation
    [SerializeField] private float damping = 5f; // Damping factor for momentum
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction dragAction;
    private bool isDragging = false;

    private Vector2 currentVelocity; // Stores the current rotation velocity
    private Vector2 targetVelocity; // Stores the target rotation velocity

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
            dragAction = playerInput.actions["Drag"];
        }
    }

    private void OnEnable()
    {
        if (dragAction != null)
        {
            dragAction.performed += OnDragStarted;
            dragAction.canceled += OnDragEnded;
        }
    }

    private void OnDisable()
    {
        if (dragAction != null)
        {
            dragAction.performed -= OnDragStarted;
            dragAction.canceled -= OnDragEnded;
        }
    }

    private void Update()
    {
        if (isDragging && lookAction != null)
        {
            // Update target velocity based on input
            targetVelocity = lookAction.ReadValue<Vector2>() * rotationSpeed;
        }
        else
        {
            // Gradually reduce target velocity to zero when not dragging
            targetVelocity = Vector2.zero;
        }

        // Smoothly interpolate current velocity towards target velocity
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.deltaTime * damping);

        // Apply rotation based on current velocity
        RotatePivot(currentVelocity * Time.deltaTime);
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        isDragging = true;
    }

    private void OnDragEnded(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void RotatePivot(Vector2 lookInput)
    {
        float rotationX = lookInput.y;
        float rotationY = lookInput.x;

        // Apply rotation to the pivot GameObject
        transform.Rotate(Vector3.right, -rotationX, Space.Self);
        transform.Rotate(Vector3.up, rotationY, Space.World);
    }
}
