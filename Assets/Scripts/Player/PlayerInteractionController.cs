using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private GameObject point;
    [SerializeField] private float interactDistance;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private float interactCooldown = 0.5f; // Задержка между интеракциями в секундах
    [SerializeField] private AnimationsController animationsController;

    private float lastInteractionTime;
    private bool canInteract => Time.time >= lastInteractionTime + interactCooldown;

    public void OnAttackButtonPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (canInteract)
        {
            Interact();
            lastInteractionTime = Time.time;
        }
        else
        {
            Debug.Log($"Interact on cooldown. Wait {lastInteractionTime + interactCooldown - Time.time:F1}s");
        }
    }

    public void Interact()
    {
        Ray ray = new Ray(point.transform.position, point.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Получаем выбранный предмет
        Item selectedItem = inventoryController?.GetSelectedItem();

        foreach (RaycastHit hit in hits)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // Проверяем совместимость
                if (interactable.CanInteractWith(selectedItem))
                {
                    Debug.Log($"Interacted with {hit.collider.name} using {selectedItem?.Name ?? "hands"}");
                    interactable.Interact();

                    // Анимация оружия
                    if (animationsController != null && selectedItem.Name != null)
                    {
                        GameObject weaponPrefab = inventoryController.GetPrefabOfSelectedWeapon();
                        if (weaponPrefab != null && weaponPrefab.GetComponent<Animator>() != null)
                        {
                            animationsController.ExecuteAnimationOfWeapon(weaponPrefab.GetComponent<Animator>());
                        }
                    }
                    return;
                }
                else
                {
                    Debug.Log($"Cannot interact with {hit.collider.name} using {selectedItem?.Name ?? "hands"}");
                }
            }
        }
    }

    
}