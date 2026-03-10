using UnityEngine;

public class Ore : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int hp = 10;
    [SerializeField] private int maxHp = 10;
    [SerializeField] GameObject orePrefab; // Префаб руды для спавна при разрушении

    public void Interact()
    {
        hp -= 1;
        Debug.Log($"Ore took {1} damage, remaining HP: {hp}");
        if (hp <= 0)
        {
            Instantiate( orePrefab );
            Debug.Log("Ore destroyed!");
            hp=maxHp; // Восстанавливаем HP для повторного использования
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
