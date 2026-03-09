using UnityEngine;

public class Item :  MonoBehaviour, IItem
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    public string Name => itemName;
    public Sprite Icon => itemIcon;

    public string Descriptions { get; set; }
    public bool CanBePass { get; set; }
    string IItem.Name { get; set ; }

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
}
   
