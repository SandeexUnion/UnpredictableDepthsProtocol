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

        // Сортируем по дистанции
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"Interacted with {hit.collider.name} at distance {hit.distance}");
                interactable.Interact();
                Debug.Log(animationsController==null);
                Debug.Log(inventoryController == null);
                Debug.Log(inventoryController.GetPrefabOfSelectedWeapon().GetComponent<Animator>() == null);
                animationsController.ExecuteAnimationOfWeapon(inventoryController.GetPrefabOfSelectedWeapon().GetComponent<Animator>());
                return;
            }
        }
    }
}