using UnityEngine;

public interface IInteractable
{
    public void Interact();
    public InteractableType[] interactableTypes { get; set; }
    public bool CanInteractWith(Item tool); // Новый метод
}
public enum InteractableType
{
    Axe,
    Pickaxe,
    Ore,
    Craftable,
    Fuel

}
