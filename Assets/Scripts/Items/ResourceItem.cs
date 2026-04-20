using UnityEngine;

public class ResourceItem : Item, IResourceble
{
    [SerializeField] private ResourceType _resourceType;

    public ResourceType resourceType
    {
        get => _resourceType;
        set => _resourceType = value;
    }

    public void Recycle()
    {
        Debug.Log($"Recycling {itemName}");
        Destroy(gameObject);
    }

    // Важно: копируем тип ресурса при создании
    public void Initialize(Item source, ResourceType type)
    {
        itemName = source.itemName;
        itemIcon = source.itemIcon;
        Descriptions = source.Descriptions;
        CanBePass = source.CanBePass;
        _resourceType = type;
    }
}