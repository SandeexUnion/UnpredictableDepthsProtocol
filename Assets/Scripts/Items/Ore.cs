using UnityEngine;

public class Ore : MonoBehaviour, IInteractable
{
    [SerializeField] private int hp = 10;
    [SerializeField] private int maxHp = 10;
    [SerializeField] GameObject orePrefab; // Префаб руды для спавна при разрушении
    [SerializeField] private float spawnOffset = 1.5f; // Смещение от игрока

    public void Interact()
    {
        hp -= 1;
        Debug.Log($"Ore took {1} damage, remaining HP: {hp}");

        if (hp <= 0)
        {
            SpawnOreBetween();
            Debug.Log("Ore destroyed!");
            hp = maxHp; // Восстанавливаем HP для повторного использования
        }
    }

    private void SpawnOreBetween()
    {
        // Находим игрока через FindObjectOfType
        InventoryController player = FindFirstObjectByType<InventoryController>();

        if (player != null)
        {
            // Получаем позиции
            Vector3 playerPos = player.transform.position;
            Vector3 orePos = transform.position;

            Vector3 spawnPos = (playerPos + orePos) / 2f;
            // Добавляем небольшое случайное смещение для естественности
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.2f, 0.2f),
                0,
                Random.Range(-0.2f, 0.2f)
            );

            // Спавним руду
            Instantiate(orePrefab, spawnPos + randomOffset, Quaternion.identity);

            Debug.Log($"Ore spawned between player and deposit at {spawnPos}");
        }
        else
        {
            // Если игрок не найден, спавним на месте текущей руды
            Instantiate(orePrefab, transform.position, Quaternion.identity);
            Debug.LogWarning("Player not found! Spawning ore at current position.");
        }
    }
}