using UnityEngine;


public interface IItem : IPickeble{
    public string Name {get;set;}
    public string Descriptions {get;set;}
    public bool CanBePass {get;set;}

}
