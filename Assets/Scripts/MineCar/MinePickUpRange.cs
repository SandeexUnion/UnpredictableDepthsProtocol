using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MinePickUpRange : MonoBehaviour, IInteractable
{
    [Header("Storage")]
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    [Header("References")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private Transform dropPoint;

    // IInteractable implementation
    public InteractableType[] interactableTypes { get; set; } = new InteractableType[] { InteractableType.Ore, InteractableType.Fuel };

    // Properties
    public bool IsFull => items.Count >= maxCapacity;
    public int CurrentCount => items.Count;
    public int MaxCapacity => maxCapacity;

    // Events
    public UnityEvent<MinePickUpRange> OnItemTaken = new UnityEvent<MinePickUpRange>(); // Событие для забирания
    public System.Action<ItemData> OnItemAdded;
    public System.Action<ItemData> OnItemRemoved;

    // Для отслеживания наведения мыши
    //private bool isMouseOver = true;
    private bool isHighlighted = false;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;

    [Header("Visual")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;

    void Awake()
    {
        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (normalMaterial != null && meshRenderer != null)
        {
            originalMaterial = normalMaterial;
            meshRenderer.material = normalMaterial;
        }
        else if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
            normalMaterial = meshRenderer.material;
        }
    }

    void Start()
    {
        if (items == null) items = new List<ItemData>();
    }

    void OnMouseOver()
    {
        //isMouseOver = true;
    }

    void OnMouseExit()
    {
        //isMouseOver = false;
        if (isHighlighted)
        {
            SetHighlight(false);
        }
    }

    void OnMouseEnter()
    {
        if (!isHighlighted)
        {
            SetHighlight(true);
        }
    }

    void OnMouseDown()
    {
        // ЛКМ - кладём предмет
        Interact();
    }

    void Update()
    {
        // Проверяем ПКМ через новый Input System (как в CraftingTableTile)
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryTakeItem();
        }
    }

    private void SetHighlight(bool highlight)
    {
        if (meshRenderer == null) return;

        isHighlighted = highlight;

        if (highlight && highlightMaterial != null)
        {
            meshRenderer.material = highlightMaterial;
        }
        else if (!highlight && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
        else if (!highlight && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    // IInteractable implementation для ЛКМ
    public void Interact()
    {
        if (inventoryController == null)
        {
            Debug.LogError("InventoryController not found!");
            return;
        }

        Item selectedItem = inventoryController.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("Выберите предмет в инвентаре, чтобы положить в вагонетку!");
            return;
        }

        // Пытаемся добавить предмет в вагонетку
        TryAddResource(selectedItem);
    }

    public bool CanInteractWith(Item tool)
    {
        if (tool == null) return false;

        // Проверяем, является ли предмет ресурсом
        if (tool is IResourceble)
            return true;

        // Или проверяем по типу через имя (как в Forge)
        ResourceType resourceType = GetResourceTypeByName(tool.Name);
        return resourceType != ResourceType.None;
    }

    private void TryAddResource(Item item)
    {
        if (IsFull)
        {
            Debug.Log($"Вагонетка полна! Нельзя добавить {item.itemName}");
            return;
        }

        ResourceType resourceType = GetResourceTypeByName(item.Name);

        if (resourceType == ResourceType.None)
        {
            Debug.Log($"Предмет {item.Name} нельзя положить в вагонетку! Можно класть только руду и топливо.");
            return;
        }

        // Создаём ItemData
        ItemData data = new ItemData(item);
        data.resourceType = resourceType;

        // Добавляем в список
        items.Add(data);

        // Удаляем из инвентаря
        inventoryController.RemoveItem(inventoryController.SelectedSlotIndex);

        Debug.Log($"Добавлен {data.Name} (тип: {resourceType}) в вагонетку. {CurrentCount}/{maxCapacity}");
        OnItemAdded?.Invoke(data);
    }

    public void TryTakeItem()
    {
        if (items.Count == 0)
        {
            Debug.Log("Вагонетка пуста! Нечего забирать.");
            return;
        }

        // Берём последний предмет (как в Forge - последний добавленный)
        ItemData dataToTake = items[items.Count - 1];

        // Создаём предмет для инвентаря
        Item itemToAdd = CreateItemFromData(dataToTake);

        // Пытаемся добавить в инвентарь
        if (AddItemToInventory(itemToAdd))
        {
            items.RemoveAt(items.Count - 1);
            Debug.Log($"Забран {dataToTake.Name} из вагонетки. {CurrentCount}/{maxCapacity}");
            OnItemRemoved?.Invoke(dataToTake);
            OnItemTaken?.Invoke(this);
        }
        else
        {
            Debug.Log("Инвентарь полон! Освободите место.");
        }
    }
    public void AddNewResources(List<ItemData> newItems)
    {

        foreach (var item in newItems)
        {
            if (items.Count < maxCapacity)
            {
                items.Add(item);
                OnItemAdded?.Invoke(item);
            }
            else
            {
                Debug.Log($"Вагонетка полна! Невозможно добавить {item.Name}");
            }
        }
    }

    private Item CreateItemFromData(ItemData data)
    {
        // Создаём временный GameObject
        GameObject tempObj = new GameObject($"Temp_{data.Name}");

        Item newItem;

        // Проверяем, является ли предмет ресурсом
        if (data.resourceType != ResourceType.None)
        {
            ResourceItem resourceItem = tempObj.AddComponent<ResourceItem>();
            resourceItem.resourceType = data.resourceType;
            newItem = resourceItem;
        }
        else
        {
            newItem = tempObj.AddComponent<Item>();
        }

        // Заполняем данные
        newItem.itemName = data.Name;
        newItem.itemIcon = data.Icon;
        newItem.Descriptions = data.Description;
        newItem.CanBePass = data.CanBePass;

        tempObj.SetActive(false);
        return newItem;
    }

    private bool AddItemToInventory(Item item)
    {
        // Создаём временный объект для добавления
        GameObject tempItem = new GameObject("TempItem");
        Item tempItemComponent = tempItem.AddComponent<Item>();
        tempItemComponent.itemName = item.itemName;
        tempItemComponent.itemIcon = item.itemIcon;
        tempItemComponent.Descriptions = item.Descriptions;

        // Если это ResourceItem, копируем тип
        if (item is ResourceItem resourceItem)
        {
            ResourceItem tempResource = tempItem.AddComponent<ResourceItem>();
            tempResource.resourceType = resourceItem.resourceType;
            tempResource.itemName = item.itemName;
            tempResource.itemIcon = item.itemIcon;
            tempResource.Descriptions = item.Descriptions;
            tempItemComponent = tempResource;
        }

        bool added = inventoryController.AddNewItem(tempItemComponent);
        Destroy(tempItem);

        return added;
    }

    // Методы из Forge для определения типов
    private ResourceType GetResourceTypeByName(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "iron":
            case "ironore":
            case "copper":
            case "copperore":
            case "gold":
            case "goldore":
                return ResourceType.Metal;
            case "coal":
            case "wood":
                return ResourceType.Fuel;
            default:
                return ResourceType.None;
        }
    }

    // Метод для отправки ресурсов (будет вызываться при отправке тележки)
    public List<ItemData> GetAllItems()
    {
        List<ItemData> allItems = new List<ItemData>(items);
        items.Clear();
        Debug.Log($"Отправлено {allItems.Count} ресурсов из вагонетки");
        return allItems;
    }

    // Метод для получения ресурсов (когда тележка приезжает)
    public void AddItems(List<ItemData> receivedItems)
    {
        TryTakeItem();
        int addedCount = 0;
        int droppedCount = 0;

        foreach (var item in receivedItems)
        {
            if (items.Count < maxCapacity)
            {
                items.Add(item);
                OnItemAdded?.Invoke(item);
                addedCount++;
            }
            else
            {
                DropItemToGround(item);
                droppedCount++;
            }
        }

        Debug.Log($"Получено ресурсов: добавлено {addedCount}, выпало на землю {droppedCount}. В вагонетке: {CurrentCount}/{maxCapacity}");
    }
    public void ClearAllItems() { items.Clear(); }
    private void DropItemToGround(ItemData item)
    {
        if (dropPoint == null) return;

        // Создаём GameObject для выпавшего предмета
        GameObject droppedItem = new GameObject(item.Name);
        droppedItem.transform.position = dropPoint.position;

        // Добавляем компонент Item
        Item itemComponent = droppedItem.AddComponent<Item>();
        itemComponent.itemName = item.Name;
        itemComponent.itemIcon = item.Icon;
        itemComponent.Descriptions = item.Description;
        itemComponent.CanBePass = true;

        // Если это ресурс, добавляем компонент ResourceItem
        if (item.resourceType != ResourceType.None)
        {
            ResourceItem resourceItem = droppedItem.AddComponent<ResourceItem>();
            resourceItem.resourceType = item.resourceType;
            resourceItem.itemName = item.Name;
            resourceItem.itemIcon = item.Icon;
            resourceItem.Descriptions = item.Description;
            resourceItem.CanBePass = true;

            // Удаляем обычный Item, оставляем ResourceItem
            Destroy(itemComponent);
        }

        // Добавляем физику
        Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 2f + Random.insideUnitSphere * 1f, ForceMode.Impulse);

        Debug.Log($"Нет места в вагонетке, {item.Name} выпал на землю");
    }

    // Вспомогательные методы
    public string GetInfo()
    {
        string info = $"Вагонетка: {CurrentCount}/{maxCapacity}\n";
        foreach (var item in items)
        {
            info += $"- {item.Name} ({(item.resourceType == ResourceType.Metal ? "Руда" : "Топливо")})\n";
        }
        return info;
    }

    public bool HasItems => items.Count > 0;

    // Очистка событий при уничтожении
    void OnDestroy()
    {
        OnItemTaken?.RemoveAllListeners();
    }
}