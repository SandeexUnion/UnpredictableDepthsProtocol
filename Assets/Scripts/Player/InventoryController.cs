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
    private Item item;

    public ItemData(Item item)
    {
        Name = item.Name;
        Icon = item.Icon;
        Description = item.Descriptions;
        CanBePass = item.CanBePass;
        this.item = item;
    }
    public Item GetItem()
    {
        return item;
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
    [SerializeField] private GameObject pickaxe;
    [SerializeField] private GameObject coal;
    [SerializeField] private GameObject iron;

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
        if (coal != null)
        {
            coal.SetActive(false);
        }
        if (iron != null)
        {
            iron.SetActive(false);
        }

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
        if (coal != null) coal.SetActive(false);
        if (iron != null) iron.SetActive(false);

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

            case "Coal":
                if (coal != null)
                    coal.SetActive(true);
                break;

            case "Iron":
                if (iron != null)
                    iron.SetActive(true);
                break;
        }
    }

    public InventorySlot[] GetInventorySlots()
    {
        return inventorySlots;
    }

    public void AddNewItem(Item item)
    {
        ItemData itemData = new ItemData(item);

        if (item.Name == "pickaxe")
        {
            // Кладем кирку в инвентарь
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    inventorySlots[i].item = itemData;
                    inventorySlots[i].currentAmount = 1;

                    // Если это первый слот и он выбран, показываем кирку
                    if (i == selectedSlotIndex)
                    {
                        UpdateItemVisibility(); // Убираем параметр
                    }

                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
            Debug.Log("Inventory is full!");
            return;
        }

        // First try to stack with existing items
        for (int i = 1; i < inventorySlots.Length; i++)
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
        for (int i = 1; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                inventorySlots[i].item = itemData;
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
    public Item GetSelectedItem()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty && inventorySlots[selectedSlotIndex].item != null)
        {
            return inventorySlots[selectedSlotIndex].item.GetItem();
        }
        return null;
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

    public GameObject GetPrefabOfSelectedWeapon()
    {
        if (!inventorySlots[selectedSlotIndex].IsEmpty &&
            SelectedItem.Name == "pickaxe")
        {
            return pickaxe;
        }

        Debug.LogWarning($"Prefab for item '{SelectedItem?.Name}' not found!");
        return null;
    }
}