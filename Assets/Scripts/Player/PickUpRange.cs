using UnityEngine;

public class PickUpRange : MonoBehaviour
{
    [SerializeField] InventoryController inventoryController;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем на Item (общий класс для всех предметов)
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            Debug.Log("Item in range: " + item.Name);
            inventoryController.AddNewItem(item);
            Destroy(other.gameObject);
            return;
        }

        // Проверяем на Resource
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
        {
            Debug.Log("Resource in range: " + resource.Name);

            // Создаем Item из Resource
            Item itemFromResource = CreateItemFromResource(resource);
            inventoryController.AddNewItem(itemFromResource);
            Destroy(other.gameObject);
        }
    }

    private Item CreateItemFromResource(Resource resource)
    {
        // Создаем объект Item и заполняем его данными из Resource
        GameObject itemObject = new GameObject($"Item_{resource.Name}");
        Item item = itemObject.AddComponent<Item>();

        // Заполняем данные Item
        item.itemName = resource.Name;
        item.itemIcon = resource.Icon;
        item.Descriptions = resource.Descriptions;
        item.CanBePass = resource.CanBePass;

        return item;
    }

    void Start()
    {
        if (inventoryController == null)
            inventoryController = FindAnyObjectByType<InventoryController>();
    }
}