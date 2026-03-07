using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private Transform playerBody; // Ссылка на игрока

    private Vector2 lookInput;
    private float xRotation = 0f;

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        if (playerBody == null) return;

        // Поворот игрока по горизонтали
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);

        // Поворот камеры по вертикали
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        
        if (invertY)
            mouseY = -mouseY;
            
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}