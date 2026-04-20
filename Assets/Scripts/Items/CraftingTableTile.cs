using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Добавьте это

public class CraftingTableTile : MonoBehaviour, IInteractable
{
    public byte ID;
    public Item itemOnTile;
    public bool IsEmpty => itemOnTile == null;

    public InteractableType[] interactableTypes { get; set; } = new InteractableType[] { InteractableType.Craftable };

    [SerializeField] private GameObject itemVisual;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    // События для ЛКМ и ПКМ
    public UnityEvent<CraftingTableTile> OnTileSelected = new UnityEvent<CraftingTableTile>();
    public UnityEvent<CraftingTableTile> OnTileTakeItem = new UnityEvent<CraftingTableTile>();

    private bool isHighlighted = false;
    private Material originalMaterial;
    private Coroutine craftingEffectCoroutine;
    private bool isMouseOver = false;

    void Start()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (normalMaterial != null && meshRenderer != null)
        {
            originalMaterial = normalMaterial;
            meshRenderer.material = normalMaterial;
        }
        else if (normalMaterial == null && meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
            normalMaterial = meshRenderer.material;
        }

        // Инициализируем визуальные элементы
        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);

        if (itemVisual != null)
            itemVisual.SetActive(false);
    }

    void OnMouseDown()
    {
        // ЛКМ - положить предмет
        OnTileSelected?.Invoke(this);
    }

    void OnMouseOver()
    {
        isMouseOver = true;
    }

    void OnMouseExit()
    {
        isMouseOver = false;
        if (isHighlighted && IsEmpty)
        {
            SetHighlight(false);
        }
    }

    void Update()
    {
        // Проверяем ПКМ через новый Input System
        if (isMouseOver && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnTileTakeItem?.Invoke(this);
        }
    }

    void OnMouseEnter()
    {
        if (IsEmpty && !isHighlighted)
        {
            SetHighlight(true);
        }
    }

    public void SetHighlight(bool highlight)
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

    public void PlaceItem(Item item)
    {
        itemOnTile = item;

        if (itemVisual != null)
        {
            itemVisual.SetActive(true);
        }

        if (itemIcon != null && item.itemIcon != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }

        SetHighlight(false);
        Debug.Log($"Item {item.itemName} placed on tile {ID}");
    }

    public void RemoveItem()
    {
        itemOnTile = null;

        if (itemVisual != null)
            itemVisual.SetActive(false);

        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);

        Debug.Log($"Item removed from tile {ID}");
    }

    public void StartCraftingEffect()
    {
        if (craftingEffectCoroutine != null)
            StopCoroutine(craftingEffectCoroutine);

        craftingEffectCoroutine = StartCoroutine(CraftingEffect());
    }

    private System.Collections.IEnumerator CraftingEffect()
    {
        if (meshRenderer != null)
        {
            Color originalColor = meshRenderer.material.color;
            Material originalMat = meshRenderer.material;

            Material effectMat = new Material(originalMat);
            meshRenderer.material = effectMat;

            for (int i = 0; i < 5; i++)
            {
                effectMat.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                effectMat.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }

            meshRenderer.material = originalMat;
            Destroy(effectMat);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        craftingEffectCoroutine = null;
    }

    // Реализация IInteractable
    public void Interact()
    {
        OnMouseDown();
    }

    public bool CanInteractWith(Item tool)
    {
        if (tool is IResourceble)
            return true;

        if (tool == null)
            return true;

        return false;
    }

    public string GetTileInfo()
    {
        if (IsEmpty)
            return $"Tile {ID}: Empty";
        else
            return $"Tile {ID}: {itemOnTile.itemName}";
    }

    void OnDestroy()
    {
        OnTileSelected?.RemoveAllListeners();
        OnTileTakeItem?.RemoveAllListeners();
    }
}