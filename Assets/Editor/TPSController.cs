using UnityEngine;

public class TPSController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckDistance = 0.2f;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cameraSensitivity = 2f;
    [SerializeField] private float upperLookLimit = 80f;
    [SerializeField] private float lowerLookLimit = -80f;
    
    [Header("Input")]
    [SerializeField] private string horizontalInputName = "Horizontal";
    [SerializeField] private string verticalInputName = "Vertical";
    [SerializeField] private string mouseXInputName = "Mouse X";
    [SerializeField] private string mouseYInputName = "Mouse Y";
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    
    // Components
    private CharacterController characterController;
    [SerializeField] Camera mainCamera;
    private Transform cameraTransform;
    
    // Movement variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 moveDirection;
    private Vector3 velocity;
    private bool isSprinting;
    private bool jumpRequested;
    private float verticalRotation;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Start()
    {
        if (mainCamera != null && cameraTarget != null)
        {
            cameraTransform = mainCamera.transform;
            cameraTransform.position = cameraTarget.position;
            cameraTransform.rotation = cameraTarget.rotation;
        }
    }
    
    private void Update()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
        ApplyGravity();
        
        // Apply final movement
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void HandleInput()
    {
        // Movement input
        moveInput.x = Input.GetAxis(horizontalInputName);
        moveInput.y = Input.GetAxis(verticalInputName);
        
        // Look input
        lookInput.x = Input.GetAxis(mouseXInputName);
        lookInput.y = Input.GetAxis(mouseYInputName);
        
        // Jump input
        if (Input.GetKeyDown(jumpKey))
        {
            jumpRequested = true;
        }
        
        // Sprint input
        isSprinting = Input.GetKey(sprintKey);
    }
    
    private void HandleMovement()
    {
        // Calculate movement direction based on camera orientation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // Apply appropriate speed
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        
        // Set horizontal velocity
        velocity.x = moveDirection.x * currentSpeed;
        velocity.z = moveDirection.z * currentSpeed;
        
        // Handle jumping
        if (jumpRequested && IsGrounded())
        {
            velocity.y = jumpForce;
            jumpRequested = false;
        }
    }
    
    private void HandleRotation()
    {
        if (cameraTarget == null) return;
        
        // Horizontal rotation (rotate player)
        transform.Rotate(Vector3.up, lookInput.x * cameraSensitivity);
        
        // Vertical rotation (camera only)
        verticalRotation -= lookInput.y * cameraSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, lowerLookLimit, upperLookLimit);
        cameraTarget.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        
        // Update camera position and rotation
        if (cameraTransform != null)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTarget.position, Time.deltaTime * 10f);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, cameraTarget.rotation, Time.deltaTime * 10f);
        }
    }
    
    private void ApplyGravity()
    {
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant to keep character grounded
        }
        else
        {
            // Apply gravity
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }
    
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayers) || 
               characterController.isGrounded;
    }
}
