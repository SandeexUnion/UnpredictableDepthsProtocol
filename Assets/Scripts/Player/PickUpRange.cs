using UnityEngine;

public class PickUpRange : MonoBehaviour
{
    [SerializeField] InventoryController inventoryController;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            Debug.Log("Item in range: " + item.Name);
            inventoryController.AddNewItem(item);
            Destroy(other.gameObject); // Теперь это безопасно!
        }
    }

    void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
    }
}