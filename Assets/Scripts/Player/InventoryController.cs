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

    // Конструктор для создания из Item компонента
    public ItemData(Item item)
    {
        Name = item.Name;
        Icon = item.Icon;
        Description = item.Descriptions;
        CanBePass = item.CanBePass;
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
    public ItemData item; // Изменено с Item на ItemData
    public int maxStack;
    public int currentAmount;

    public bool IsEmpty => currentAmount <= 0 || item == null;
    public bool IsFull => currentAmount >= maxStack;
}

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<ItemPrefabPair> itemPrefabs; // Список пар имя-префаб
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float dropHeight = 1f;

    [SerializeField] private InventorySlot[] inventorySlots;
    public event Action OnInventoryChanged;

    private int selectedSlotIndex = 0;

    // Public property to get selected slot
    public int SelectedSlotIndex => selectedSlotIndex;

    // Public property to get selected item
    public ItemData SelectedItem => inventorySlots[selectedSlotIndex].item;

    void Start()
    {
        // Initialize inventory slots if not set in inspector
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
    }

    // Удаляем Update метод - теперь ввод обрабатывается через Input System

    public void SelectSlot(int index)
    {
        if (index >= 0 && index < inventorySlots.Length)
        {
            int previousSlot = selectedSlotIndex;
            selectedSlotIndex = index;

            // Only invoke if selection actually changed
            if (previousSlot != selectedSlotIndex)
            {
                OnInventoryChanged?.Invoke();
                Debug.Log($"Selected slot: {selectedSlotIndex + 1}");
            }
        }
    }

    // Public method to get inventory slots
    public InventorySlot[] GetInventorySlots()
    {
        return inventorySlots;
    }

    public void AddNewItem(Item item)
    {
        // Создаем данные предмета из компонента
        ItemData itemData = new ItemData(item);

        // First try to stack with existing items
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].IsEmpty &&
                inventorySlots[i].item.Name == itemData.Name &&
                !inventorySlots[i].IsFull)
            {
                inventorySlots[i].currentAmount++;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // Then try to find empty slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                inventorySlots[i].item = itemData; // Сохраняем данные, а не ссылку
                inventorySlots[i].currentAmount = 1;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        Debug.Log("Inventory is full!");
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
            // Add usage logic here
            RemoveItem(selectedSlotIndex);
        }
    }

    public void DropItem()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty)
        {
            ItemData itemData = inventorySlots[selectedSlotIndex].item;
            Debug.Log($"Dropping {itemData.Name}");

            // Находим префаб по имени
            GameObject prefab = GetPrefabByName(itemData.Name);

            if (prefab != null)
            {
                // Позиция для спавна перед персонажем
                Vector3 spawnPosition = transform.position + transform.forward * dropDistance + Vector3.up * dropHeight;

                // Спавним предмет
                GameObject droppedItem = Instantiate(prefab, spawnPosition, Quaternion.identity);

                // Добавляем небольшую случайную ротацию
                droppedItem.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

                // Можно добавить физический импульс для реализма
                if (droppedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
                }
            }

            RemoveItem(selectedSlotIndex);
        }
    }

    private GameObject GetPrefabByName(string itemName)
    {
        foreach (var pair in itemPrefabs)
        {
            if (pair.itemName == itemName)
            {
                return pair.prefab;
            }
        }
        Debug.LogWarning($"Prefab for item '{itemName}' not found!");
        return null;
    }
}