using UnityEngine;

public class Resource : MonoBehaviour, IResourceble
{
    public ResourceType resourceType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    public string Name => itemName;
    public Sprite Icon => itemIcon;
    public string Descriptions { get; set; }
    public bool CanBePass { get; set; }
    string IItem.Name { get; set; }

    public void Recycle()
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    

    public void SetData(ItemData data)
    {
        itemName = data.Name;
        itemIcon = data.Icon;
        Descriptions = data.Description;
        CanBePass = data.CanBePass;
    }
    public void Drop()
    {

    }

    public IItem PickUp()
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
