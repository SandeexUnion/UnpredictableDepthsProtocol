using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private GameObject point;
    [SerializeField] private float interactDistance;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private float interactCooldown = 0.5f;
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

    public void OnAltAttackButtonPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (canInteract)
        {
            AltInteract();
            lastInteractionTime = Time.time;
        }
        else
        {
            Debug.Log($"Alt interact on cooldown. Wait {lastInteractionTime + interactCooldown - Time.time:F1}s");
        }
    }

    public void Interact()
    {
        Ray ray = new Ray(point.transform.position, point.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Item selectedItem = inventoryController?.GetSelectedItem();

        // ДОБАВЬТЕ ЭТОТ ЛОГ
        Debug.Log($"Interact: Selected item = {(selectedItem != null ? selectedItem.itemName : "null")}");

        foreach (RaycastHit hit in hits)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                if (interactable.CanInteractWith(selectedItem))
                {
                    Debug.Log($"Interacted with {hit.collider.name} using {(selectedItem != null ? selectedItem.itemName : "hands")}");
                    interactable.Interact();

                    // Анимация для ЛКМ
                    PlayWeaponAnimation(selectedItem);
                    return;
                }
                else
                {
                    Debug.Log($"Cannot interact with {hit.collider.name} using {(selectedItem != null ? selectedItem.itemName : "hands")}");
                }
            }
            if (hit.collider.gameObject.tag == "Button")
            {
                hit.collider.gameObject.GetComponent<ResourceTransferManager>()?.SendCartResources();
            }
        }
    }

    public void AltInteract()
    {
        Ray ray = new Ray(point.transform.position, point.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            CraftingTableTile tile = hit.collider.GetComponent<CraftingTableTile>();
            if (tile != null)
            {
                tile.OnTileTakeItem?.Invoke(tile);
                Debug.Log($"Alt interacted with {hit.collider.name}");
                return;
            }
        }
    }

    private void PlayWeaponAnimation(Item selectedItem)
    {
        // ИСПРАВЛЕНО: убрана лишняя проверка
        if (animationsController == null)
        {
            Debug.Log("AnimationsController is null");
            return;
        }

        if (selectedItem == null)
        {
            Debug.Log("No item selected, skipping animation");
            return;
        }

        Debug.Log($"Playing animation for: {selectedItem.itemName}");

        GameObject weaponPrefab = inventoryController?.GetPrefabOfSelectedWeapon();
        if (weaponPrefab == null)
        {
            Debug.Log($"Weapon prefab is null for {selectedItem.itemName}");
            return;
        }

        Animator weaponAnimator = weaponPrefab.GetComponent<Animator>();
        if (weaponAnimator == null)
        {
            Debug.Log($"No Animator on {selectedItem.itemName} prefab");
            return;
        }

        animationsController.ExecuteAnimationOfWeapon(weaponAnimator);
    }

    private void Update()
    {
        Debug.DrawRay(point.transform.position, point.transform.forward);
    }
}