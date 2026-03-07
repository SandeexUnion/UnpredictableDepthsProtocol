
public interface IResourceble : IItem{
    public ResourceType resourceType {get;set;}
    public void Recycle();
    
}

public enum ResourceType{
    Fuel,
    Metal,
    Mineral
}
