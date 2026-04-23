using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string Name;
    public Sprite Icon;
    public string Description;
    public bool CanBePass;
    public ResourceType resourceType; // Добавляем тип ресурса
    public string prefabName; // Имя префаба для восстановления

    // Конструктор из Item
    public ItemData(Item item)
    {
        Name = item.itemName;
        Icon = item.itemIcon;
        Description = item.Descriptions;
        CanBePass = item.CanBePass;

        // Сохраняем тип ресурса если есть
        if (item is IResourceble resourceItem)
        {
            resourceType = resourceItem.resourceType;
        }
        else
        {
            resourceType = ResourceType.None;
        }

        prefabName = item.itemName;
    }
    public ItemData() { }


    // Создание Item из данных (без привязки к GameObject)
    public Item CreateItem()
    {
        // Создаем временный GameObject
        GameObject tempObj = new GameObject($"Temp_{Name}");

        Item newItem;

        // Создаем правильный тип предмета
        if (resourceType != ResourceType.None)
        {
            ResourceItem resourceItem = tempObj.AddComponent<ResourceItem>();
            resourceItem.resourceType = resourceType;
            newItem = resourceItem;
        }
        else
        {
            newItem = tempObj.AddComponent<Item>();
        }

        // Заполняем данные
        newItem.itemName = Name;
        newItem.itemIcon = Icon;
        newItem.Descriptions = Description;
        newItem.CanBePass = CanBePass;

        tempObj.SetActive(false);

        return newItem;
    }
}

[Serializable]
public class ItemPrefabPair
{
    public string itemName;
    public GameObject prefab;
}

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int maxStack;
    public int currentAmount;

    public bool IsEmpty => currentAmount <= 0 || item == null;
    public bool IsFull => currentAmount >= maxStack;
}

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<ItemPrefabPair> itemPrefabs;
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float dropHeight = 1f;
    [Header("Item GameObjects in hand")]
    [SerializeField] private GameObject pickaxe;
    [SerializeField] private GameObject coal;
    [SerializeField] private GameObject iron;
    [SerializeField] private GameObject ironIngot;
    [SerializeField] private GameObject hatchet;

    [SerializeField] private InventorySlot[] inventorySlots;
    public event Action OnInventoryChanged;

    private int selectedSlotIndex = 0;

    public int SelectedSlotIndex => selectedSlotIndex;
    public ItemData SelectedItem => inventorySlots[selectedSlotIndex].item;

    void Start()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            inventorySlots = new InventorySlot[10];
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i] = new InventorySlot
                {
                    item = null,
                    maxStack = 10,
                    currentAmount = 0
                };
            }
        }

        // Убедимся, что кирка изначально выключена
        if (pickaxe != null)
        {
            pickaxe.SetActive(false);
        }
        if (hatchet != null)
        {
            hatchet.SetActive(false);
        }
        if (coal != null)
        {
            coal.SetActive(false);
        }
        if (iron != null)
        {
            iron.SetActive(false);
        }
        if (ironIngot != null) ironIngot.SetActive(false);

    }

    public void SelectSlot(int index)
    {
        if (index >= 0 && index < inventorySlots.Length)
        {
            int previousSlot = selectedSlotIndex;
            selectedSlotIndex = index;

            if (previousSlot != selectedSlotIndex)
            {
                UpdateItemVisibility(); // Убираем параметр
                OnInventoryChanged?.Invoke();
                Debug.Log($"Selected slot: {selectedSlotIndex + 1}");
            }
        }
    }

    private void UpdateItemVisibility()
    {
        // Сначала выключаем все предметы
        if (pickaxe != null) pickaxe.SetActive(false);
        if (hatchet != null) hatchet.SetActive(false);
        if (coal != null) coal.SetActive(false);
        if (iron != null) iron.SetActive(false);
        if (ironIngot != null) ironIngot.SetActive(false); // Добавляем

        // Если слот пуст, ничего не включаем
        if (inventorySlots[selectedSlotIndex].IsEmpty)
            return;

        // Включаем нужный предмет в зависимости от его имени
        string itemName = inventorySlots[selectedSlotIndex].item.Name;

        switch (itemName)
        {
            case "pickaxe":
                if (pickaxe != null)
                    pickaxe.SetActive(true);
                break;
            case "hatchet":
                if (hatchet != null)
                    hatchet.SetActive(true);
                break;

            case "Coal":
                if (coal != null)
                    coal.SetActive(true);
                break;

            case "Iron":
                if (iron != null)
                    iron.SetActive(true);
                break;

            case "IronIngot": // Добавляем случай для слитка железа
                if (ironIngot != null)
                    ironIngot.SetActive(true);
                break;
        }
    }

    public InventorySlot[] GetInventorySlots()
    {
        return inventorySlots;
    }

    public bool AddNewItem(Item item)
    {
        ItemData itemData = new ItemData(item);
        UpdateItemVisibility();
        if (item.Name == "pickaxe")
        {
            // Кладем кирку в инвентарь (особый случай - не стакается)
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    inventorySlots[i].item = itemData;
                    inventorySlots[i].currentAmount = 1;

                    // Если это первый слот и он выбран, показываем кирку
                    if (i == selectedSlotIndex)
                    {
                        UpdateItemVisibility();
                    }

                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            Debug.Log("Inventory is full!");
            return false;
        }
        if (item.Name == "hatchet")
        {
            // Кладем кирку в инвентарь (особый случай - не стакается)
            for (int i = 1; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    inventorySlots[i].item = itemData;
                    inventorySlots[i].currentAmount = 1;

                    // Если это второй слот и он выбран, показываем кирку
                    if (i == selectedSlotIndex)
                    {
                        UpdateItemVisibility();
                    }

                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            Debug.Log("Inventory is full!");
            return false;
        }

        // First try to stack with existing items (для стакающихся предметов)
        for (int i = 2; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].IsEmpty &&
                inventorySlots[i].item.Name == itemData.Name &&
                !inventorySlots[i].IsFull)
            {
                inventorySlots[i].currentAmount++;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Then try to find empty slot
        for (int i = 2; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                inventorySlots[i].item = itemData;
                inventorySlots[i].currentAmount = 1;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            if (!inventorySlots[slotIndex].IsEmpty)
            {
                inventorySlots[slotIndex].currentAmount--;

                if (inventorySlots[slotIndex].currentAmount <= 0)
                {
                    inventorySlots[slotIndex].item = null;

                    // Если удалили предмет из выбранного слота, обновляем видимость
                    if (slotIndex == selectedSlotIndex)
                    {
                        UpdateItemVisibility(); // Убираем параметр
                    }
                }

                OnInventoryChanged?.Invoke();
            }
        }
    }

    public void UseSelectedItem()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty)
        {
            Debug.Log($"Using {inventorySlots[selectedSlotIndex].item.Name}");
            RemoveItem(selectedSlotIndex);
        }
    }
    // В InventoryController.cs
    public Item GetSelectedItem()
    {
        Debug.Log($"Selected slot index: {selectedSlotIndex}");

        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventorySlots.Length)
        {
            Debug.Log("Selected slot index out of range!");
            return null;
        }

        if (inventorySlots[selectedSlotIndex].IsEmpty)
        {
            Debug.Log("Selected slot is empty!");
            return null;
        }

        if (inventorySlots[selectedSlotIndex].item == null)
        {
            Debug.Log("Selected item data is null!");
            return null;
        }

        // СОЗДАЕМ НОВЫЙ Item из данных, а не пытаемся получить старый
        Item newItem = inventorySlots[selectedSlotIndex].item.CreateItem();
        Debug.Log($"GetSelectedItem creating new item: {newItem?.itemName ?? "null"}");

        return newItem;
    }
    // Добавьте этот метод в класс InventoryController
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public void DropItem()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty)
        {
            ItemData itemData = inventorySlots[selectedSlotIndex].item;
            Debug.Log($"Dropping {itemData.Name}");

            GameObject prefab = GetPrefabByName(itemData.Name);

            if (prefab != null)
            {
                Vector3 spawnPosition = transform.position + transform.forward * dropDistance + Vector3.up * dropHeight;
                GameObject droppedItem = Instantiate(prefab, spawnPosition, Quaternion.identity);
                droppedItem.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

                // Отключаем аниматор если есть
                if (droppedItem.TryGetComponent<Animator>(out Animator anim))
                {
                    anim.enabled = false;
                }

                // Настройка Rigidbody для предотвращения проваливания
                if (droppedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    // Увеличиваем массу для лучшего контакта с землей
                    rb.mass = 1f;

                    // Включаем непрерывное обнаружение столкновений (важно!)
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                    // Настраиваем интерполяцию для плавного движения
                    rb.interpolation = RigidbodyInterpolation.Interpolate;

                    // Добавляем силу
                    rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
                }

                // Убеждаемся, что коллайдер настроен правильно
                if (droppedItem.TryGetComponent<Collider>(out Collider col))
                {
                    // Проверяем, что коллайдер не триггер
                    col.isTrigger = false;

                    // Если это MeshCollider, убеждаемся что он convex
                    if (col is MeshCollider meshCol)
                    {
                        meshCol.convex = true;
                    }
                }
            }

            RemoveItem(selectedSlotIndex);
        }
    }

    private GameObject GetPrefabByName(string itemName)
    {
        // Сначала ищем точное совпадение
        foreach (var pair in itemPrefabs)
        {
            if (pair.itemName == itemName)
            {
                return pair.prefab;
            }
        }

        // Если не нашли, пробуем искать без пробелов
        string noSpaceName = itemName.Replace(" ", "").ToLower();
        foreach (var pair in itemPrefabs)
        {
            if (pair.itemName == noSpaceName)
            {
                Debug.Log($"Found prefab for '{itemName}' using name '{noSpaceName}'");
                return pair.prefab;
            }
        }

        Debug.LogWarning($"Prefab for item '{itemName}' not found!");
        return null;
    }

    public GameObject GetPrefabOfSelectedWeapon()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty && SelectedItem != null)
        {
            switch (SelectedItem.Name)
            {
                case "pickaxe":
                    return pickaxe;
                case "hatchet":
                    return hatchet;
                case "Coal":
                    return coal;
                case "Iron":
                    return iron;
                case "IronIngot":
                    return ironIngot;
                default:
                    Debug.LogWarning($"Prefab for item '{SelectedItem.Name}' not found!");
                    return null;
            }
        }

        Debug.LogWarning($"No item selected or slot is empty!");
        return null;
    }
}