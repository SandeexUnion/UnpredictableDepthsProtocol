using UnityEngine;

public class Tree : MonoBehaviour, IInteractable
{
    [SerializeField] private int hp = 10;
    [SerializeField] private int maxHp = 10;
    [SerializeField] GameObject woodPrefab; // Префаб доски для спавна при разрушении
    [SerializeField] private float spawnOffset = 1.5f; // Смещение от игрока

    [SerializeField] private InteractableType[] supportedTools = new InteractableType[] { InteractableType.Axe };
    public InteractableType[] interactableTypes { get; set; }

    public bool CanInteractWith(Item tool)
    {
        if (tool.Name == null) return false;

        // Проверяем, является ли инструмент топором
        return tool.Name.ToLower().Contains("hatchet");
    }
    public void Interact()
    {
        hp -= 1;
        Debug.Log($"Tree took {1} damage, remaining HP: {hp}");

        if (hp <= 0)
        {
            SpawnWoodBetween();
            Debug.Log("Tree destroyed!");
            hp = maxHp; // Восстанавливаем HP для повторного использования
        }
    }

    private void SpawnWoodBetween()
    {
        // Находим игрока через FindObjectOfType
        InventoryController player = FindFirstObjectByType<InventoryController>();

        if (player != null)
        {
            // Получаем позиции
            Vector3 playerPos = player.transform.position;
            Vector3 treePos = transform.position;

            Vector3 spawnPos = (playerPos + treePos) / 2f;
            // Добавляем небольшое случайное смещение для естественности
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.2f, 0.2f),
                0,
                Random.Range(-0.2f, 0.2f)
            );

            // Спавним руду
            Instantiate(woodPrefab, spawnPos + randomOffset, Quaternion.identity);

            Debug.Log($"Ore spawned between player and deposit at {spawnPos}");
        }
        else
        {
            // Если игрок не найден, спавним на месте текущей руды
            Instantiate(woodPrefab, transform.position, Quaternion.identity);
            Debug.LogWarning("Player not found! Spawning ore at current position.");
        }
    }
}
