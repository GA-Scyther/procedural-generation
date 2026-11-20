using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -19.62f; // Snappy physics feel

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private Transform cameraTransform;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void Start()
    {
        // Lock and hide mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Pitch (Vertical Camera Rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw (Horizontal Character Rotation)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        // Reset downward velocity when grounded to prevent buildup
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant force to stay glued to slopes
        }

        // Horizontal Movement relative to player orientation
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Jumping
        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}