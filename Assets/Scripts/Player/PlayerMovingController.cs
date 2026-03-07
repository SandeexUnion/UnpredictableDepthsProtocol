using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovingController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f; 
    [SerializeField] private float staminaRegenRate = 15f; 
    
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    
    private float currentStamina;
    private bool isRunning = false;
    private bool isExhausted = false;
    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentStamina = maxStamina;
        currentSpeed = walkSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        // Зажимаем Shift
        if (context.performed && !isExhausted)
        {
            isRunning = true;
        }
        // Отпускаем Shift
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    void Update()
    {
        HandleStamina();
        HandleMovement();
        HandleGravity();
    }

    void HandleStamina()
    {
        
        if (isRunning && !isExhausted && moveInput.magnitude > 0.1f) 
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentSpeed = runSpeed;
            
            
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
                isRunning = false; 
            }
        }
        
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentSpeed = walkSpeed;
            
            
            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isExhausted = false;
            }
        }
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    
    void OnGUI()
    {
        
        GUI.Label(new Rect(10, 10, 200, 20), $"Stamina: {currentStamina:F0}/{maxStamina}");
        GUI.Label(new Rect(10, 30, 200, 20), $"State: {(isRunning ? "Running" : isExhausted ? "Exhausted" : "Walking")}");
    }
}