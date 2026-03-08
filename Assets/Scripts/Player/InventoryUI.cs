using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlotUI
    {
        public Image iconImage;
        public GameObject arrow;
        public Text countText;
        public Text slotNumberText; // Текст для отображения номера слота (1-0)
        public Image backgroundImage; // Фон слота для подсветки
        [HideInInspector] public ItemData item; // было Item
    }

    private InventoryController inventoryController;

    public ItemSlotUI[] itemSlots = new ItemSlotUI[10];
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;

    [Header("Colors")]
    [SerializeField] private Color selectedSlotColor = Color.yellow;
    [SerializeField] private Color normalSlotColor = Color.white;
    [SerializeField] private Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    void Start()
    {
        // Find InventoryController in the scene
        inventoryController = FindFirstObjectByType<InventoryController>();

        if (inventoryController == null)
        {
            Debug.LogError("InventoryController not found in scene!");
            return;
        }

        // Subscribe to inventory change event
        inventoryController.OnInventoryChanged += UpdateUI;

        // Set up slot number texts
        SetupSlotNumbers();

        // Initial UI update
        UpdateUI();
    }

    void SetupSlotNumbers()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].slotNumberText != null)
            {
                // Отображаем 1-9 для первых 9 слотов, и 0 для последнего
                string numberText = (i == 9) ? "0" : (i + 1).ToString();
                itemSlots[i].slotNumberText.text = numberText;
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (inventoryController != null)
        {
            inventoryController.OnInventoryChanged -= UpdateUI;
        }
    }

    void UpdateUI()
    {
        if (inventoryController == null) return;

        // Get inventory slots from controller
        var inventorySlots = inventoryController.GetInventorySlots();
        int selectedIndex = inventoryController.SelectedSlotIndex;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < inventorySlots.Length)
            {
                var invSlot = inventorySlots[i];

                // Update icon
                if (itemSlots[i].iconImage != null)
                {
                    if (!invSlot.IsEmpty && invSlot.item != null)
                    {
                        itemSlots[i].iconImage.sprite = invSlot.item.Icon;
                        itemSlots[i].iconImage.enabled = true;
                        itemSlots[i].iconImage.color = Color.white;
                    }
                    else
                    {
                        itemSlots[i].iconImage.sprite = null;
                        itemSlots[i].iconImage.enabled = false;
                    }
                }

                // Update count text
                if (itemSlots[i].countText != null)
                {
                    if (!invSlot.IsEmpty && invSlot.currentAmount > 1)
                    {
                        itemSlots[i].countText.text = invSlot.currentAmount.ToString();
                        itemSlots[i].countText.enabled = true;
                    }
                    else
                    {
                        itemSlots[i].countText.enabled = false;
                    }
                }

                // Update selection indicator (arrow and background)
                bool isSelected = (i == selectedIndex);

                // Arrow indicator
                if (itemSlots[i].arrow != null)
                {
                    itemSlots[i].arrow.SetActive(isSelected);
                }

                // Background color
                if (itemSlots[i].backgroundImage != null)
                {
                    if (isSelected)
                    {
                        itemSlots[i].backgroundImage.color = selectedSlotColor;
                    }
                    else if (invSlot.IsEmpty)
                    {
                        itemSlots[i].backgroundImage.color = emptySlotColor;
                    }
                    else
                    {
                        itemSlots[i].backgroundImage.color = normalSlotColor;
                    }
                }
            }
        }
    }

    // Call this when item is used/selected (for mouse clicks)
    public void OnSlotClicked(int slotIndex)
    {
        if (inventoryController != null)
        {
            inventoryController.SelectSlot(slotIndex);
        }
    }
}