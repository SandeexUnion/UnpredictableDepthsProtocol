using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;

    public void OnInventoryControls(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        float value = context.ReadValue<float>();
        int slotIndex = Mathf.RoundToInt(value) - 1;

        if (slotIndex >= 0 && slotIndex < 10)
        {
            inventoryController?.SelectSlot(slotIndex);
            Debug.Log($"Selected slot {slotIndex + 1} (value: {value})");
        }
    }
}