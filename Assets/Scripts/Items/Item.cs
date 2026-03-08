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

    public void Drop()
    {
        
    }

    public IItem PickUp()
    {
        throw new System.NotImplementedException();
    }
}
   
