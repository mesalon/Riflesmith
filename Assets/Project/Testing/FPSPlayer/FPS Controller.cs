using UnityEngine;

public class FPSController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    
    // Movement variables
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 2f;
    
    // Camera variables
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    
    // Movement state
    private Vector3 velocity;

    void Start()
    {
        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle movement
        HandleMovement();
        
        // Handle camera rotation
        HandleRotation();
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleMovement() {
        Vector3 move = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")) * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed);
        characterController.Move(move * Time.deltaTime);
    }

    void HandleRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (pitch)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
