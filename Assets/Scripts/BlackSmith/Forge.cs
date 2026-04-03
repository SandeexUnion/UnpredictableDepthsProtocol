using UnityEngine;

public class Forge : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private int maxFuelCells = 3;
    [SerializeField] private int maxOreCells = 3;
    [SerializeField] private float meltingTime = 5f;
    [SerializeField] private Transform dropPoint;
    public InteractableType[] interactableTypes { get; set; } = new InteractableType[] { InteractableType.Ore, InteractableType.Fuel };
    // Результаты плавки для разных комбинаций
    [System.Serializable]
    public class SmeltingResult
    {
        public string oreType;      // Тип руды (Iron, Copper, Gold и т.д.)
        public string fuelType;     // Тип топлива (Coal, Wood и т.д.)
        public string resultName;   // Имя результата
        public GameObject resultPrefab; // Префаб результата
        public Sprite resultIcon;   // Иконка результата
        public string resultDescription; // Описание результата
    }

    [SerializeField] private SmeltingResult[] smeltingRecipes;

    private int[] fuelCells;
    private int[] oreCells;
    private string[] fuelTypes;     // Храним типы топлива в ячейках
    private string[] oreTypes;      // Храним типы руды в ячейках
    private bool isMelting = false;
    private float currentMeltingTime = 0f;
    private string currentOreType;
    private string currentFuelType;

    public bool IsOreCellsFull => IsArrayFull(oreCells);
    public bool IsFuelCellsFull => IsArrayFull(fuelCells);
    public bool HasResources => GetTotalItems(oreCells) > 0 && GetTotalItems(fuelCells) > 0;

    void Awake()
    {
        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void Start()
    {
        fuelCells = new int[maxFuelCells];
        oreCells = new int[maxOreCells];
        fuelTypes = new string[maxFuelCells];
        oreTypes = new string[maxOreCells];
    }

    void Update()
    {
        if (isMelting)
        {
            currentMeltingTime += Time.deltaTime;

            if (currentMeltingTime >= meltingTime)
            {
                CompleteMelting();
            }
        }
    }
    public bool CanInteractWith(Item tool)
    {
        if (tool == null) return false;

        // Получаем тип инструмента из имени предмета
        InteractableType toolType = GetToolType(tool.Name);

        // Проверяем, есть ли этот тип в списке поддерживаемых
        foreach (var type in interactableTypes)
        {
            if (type == toolType)
                return true;
        }
        return false;
    }

    private InteractableType GetToolType(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "iron":
                return InteractableType.Ore;
            case "copper":
                return InteractableType.Ore;
            case "gold":
                return InteractableType.Ore;
            case "coal":
                return InteractableType.Fuel;
            case "wood":
                return InteractableType.Fuel;
            default:
                return InteractableType.Craftable;
        }
    }

public void Interact()
    {
        if (isMelting)
        {
            Debug.Log("Forge is currently melting. Please wait.");
            return;
        }

        if (inventoryController == null)
        {
            Debug.LogError("InventoryController not found!");
            return;
        }

        Item selectedItem = inventoryController.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("No item selected!");
            return;
        }

        TryAddResource(selectedItem);
    }

    private void TryAddResource(Item item)
    {
        ResourceType resourceType = GetResourceTypeByName(item.Name);
        string specificType = GetSpecificTypeByName(item.Name);

        if (resourceType == ResourceType.None)
        {
            Debug.Log($"Item {item.Name} is not a resource!");
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Metal:
                AddOre(item, specificType);
                break;
            case ResourceType.Fuel:
                AddFuel(item, specificType);
                break;
            default:
                Debug.Log("Unknown resource type.");
                break;
        }
    }

    private ResourceType GetResourceTypeByName(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "iron":
                return ResourceType.Metal;
            case "ironore":
                return ResourceType.Metal;
            case "copper":
                return ResourceType.Metal;
            case "copperore":
                return ResourceType.Metal;
            case "gold":
                return ResourceType.Metal;
            case "goldore":
                return ResourceType.Metal;
            case "coal":
                return ResourceType.Fuel;
            case "wood":
                return ResourceType.Fuel;
            default:
                return ResourceType.None;
        }
    }

    private string GetSpecificTypeByName(string itemName)
    {
        string lowerName = itemName.ToLower();

        if (lowerName.Contains("iron"))
            return "Iron";
        if (lowerName.Contains("copper"))
            return "Copper";
        if (lowerName.Contains("gold"))
            return "Gold";
        if (lowerName.Contains("coal"))
            return "Coal";
        if (lowerName.Contains("wood"))
            return "Wood";

        return lowerName;
    }

    private void AddOre(Item item, string oreType)
    {
        for (int i = 0; i < oreCells.Length; i++)
        {
            if (oreCells[i] == 0)
            {
                oreCells[i] = 1;
                oreTypes[i] = oreType;
                inventoryController.RemoveItem(inventoryController.SelectedSlotIndex);
                Debug.Log($"Ore {oreType} added to cell {i}. Total ore: {GetTotalItems(oreCells)}");

                TryStartMelting();
                return;
            }
        }

        Debug.Log("All ore cells are full!");
    }

    private void AddFuel(Item item, string fuelType)
    {
        for (int i = 0; i < fuelCells.Length; i++)
        {
            if (fuelCells[i] == 0)
            {
                fuelCells[i] = 1;
                fuelTypes[i] = fuelType;
                inventoryController.RemoveItem(inventoryController.SelectedSlotIndex);
                Debug.Log($"Fuel {fuelType} added to cell {i}. Total fuel: {GetTotalItems(fuelCells)}");

                TryStartMelting();
                return;
            }
        }

        Debug.Log("All fuel cells are full!");
    }

    private void TryStartMelting()
    {
        if (!isMelting && HasResources)
        {
            // Определяем, какие ресурсы будут использованы для плавки
            DetermineResourcesForMelting();
            StartMelting();
        }
    }

    private void DetermineResourcesForMelting()
    {
        // Берем первый попавшийся ресурс каждого типа
        for (int i = 0; i < oreCells.Length; i++)
        {
            if (oreCells[i] == 1)
            {
                currentOreType = oreTypes[i];
                break;
            }
        }

        for (int i = 0; i < fuelCells.Length; i++)
        {
            if (fuelCells[i] == 1)
            {
                currentFuelType = fuelTypes[i];
                break;
            }
        }

        Debug.Log($"Preparing to melt {currentOreType} using {currentFuelType}");
    }

    private void StartMelting()
    {
        isMelting = true;
        currentMeltingTime = 0f;
        Debug.Log($"Started melting {currentOreType} with {currentFuelType}! Will complete in {meltingTime} seconds.");
    }

    private void CompleteMelting()
    {
        isMelting = false;

        // Очищаем использованные ресурсы
        ClearResources();

        // Создаем результат в зависимости от комбинации
        CreateResult(currentOreType, currentFuelType);

        Debug.Log("Melting completed!");
    }

    private void ClearResources()
    {
        // Очищаем одну ячейку руды
        for (int i = 0; i < oreCells.Length; i++)
        {
            if (oreCells[i] == 1)
            {
                oreCells[i] = 0;
                oreTypes[i] = null;
                break;
            }
        }

        // Очищаем одну ячейку топлива
        for (int i = 0; i < fuelCells.Length; i++)
        {
            if (fuelCells[i] == 1)
            {
                fuelCells[i] = 0;
                fuelTypes[i] = null;
                break;
            }
        }

        Debug.Log($"Resources cleared. Remaining ore: {GetTotalItems(oreCells)}, fuel: {GetTotalItems(fuelCells)}");
    }

    private void CreateResult(string oreType, string fuelType)
    {
        // Ищем подходящий рецепт
        SmeltingResult recipe = FindRecipe(oreType, fuelType);

        if (recipe == null)
        {
            Debug.LogWarning($"No recipe found for {oreType} and {fuelType}!");
            return;
        }

        // Если есть префаб для отображения, спавним его с физикой
        if (recipe.resultPrefab != null)
        {

            // Создаем объект
            GameObject droppedItem = Instantiate(recipe.resultPrefab, new Vector3(dropPoint.position.x, dropPoint.position.y, dropPoint.position.z), Quaternion.identity);
            droppedItem.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 15), 0);

            // Добавляем компонент Item, чтобы предмет можно было подобрать
            Item itemComponent = droppedItem.GetComponent<Item>();
            if (itemComponent == null)
            {
                itemComponent = droppedItem.AddComponent<Item>();
            }

            // Настраиваем параметры предмета
            // ВАЖНО: Убираем пробел из имени для совместимости с InventoryController
            string itemName = recipe.resultName.Replace(" ", ""); // "IronIngot" вместо "Iron Ingot"
            itemComponent.itemName = itemName;
            itemComponent.itemIcon = recipe.resultIcon;
            itemComponent.Descriptions = recipe.resultDescription;
            itemComponent.CanBePass = true;

            // Отключаем аниматор если есть
            if (droppedItem.TryGetComponent<Animator>(out Animator anim))
            {
                anim.enabled = false;
            }

            // Добавляем силу для выброса
            if (droppedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Сила выброса: вперед и немного вверх
                Vector3 throwForce = transform.forward * 3f + Vector3.up * 3f;
                rb.AddForce(throwForce, ForceMode.Impulse);

                // Добавляем случайное вращение
                rb.AddTorque(new Vector3(
                    UnityEngine.Random.Range(-5f, 5f),
                    UnityEngine.Random.Range(-5f, 5f),
                    UnityEngine.Random.Range(-5f, 5f)
                ), ForceMode.Impulse);
            }

            Debug.Log($"Created {itemName} and threw it out of the forge!");
        }
        else
        {
            // Если префаба нет, создаем просто Item объект
            GameObject resultObject = new GameObject(recipe.resultName);
            Item resultItem = resultObject.AddComponent<Item>();

            // ВАЖНО: Убираем пробел из имени
            string itemName = recipe.resultName.Replace(" ", "");
            resultItem.itemName = itemName;
            resultItem.itemIcon = recipe.resultIcon;
            resultItem.Descriptions = recipe.resultDescription;
            resultItem.CanBePass = true;

            // Позиционируем и выбрасываем
            resultObject.transform.position = transform.position + transform.forward * 2f + Vector3.up * 1f;

            Rigidbody rb = resultObject.AddComponent<Rigidbody>();
            Vector3 throwForce = transform.forward * 3f + Vector3.up * 2f;
            rb.AddForce(throwForce, ForceMode.Impulse);

            Debug.Log($"Created {itemName} and threw it out (no prefab)!");
        }
    }

    private SmeltingResult FindRecipe(string oreType, string fuelType)
    {
        foreach (var recipe in smeltingRecipes)
        {
            if (recipe.oreType == oreType && recipe.fuelType == fuelType)
            {
                return recipe;
            }
        }

        // Если точное совпадение не найдено, ищем рецепт только по руде (топливо любое)
        foreach (var recipe in smeltingRecipes)
        {
            if (recipe.oreType == oreType && string.IsNullOrEmpty(recipe.fuelType))
            {
                return recipe;
            }
        }

        return null;
    }

    private int GetTotalItems(int[] array)
    {
        int total = 0;
        foreach (int item in array)
        {
            total += item;
        }
        return total;
    }

    private bool IsArrayFull(int[] array)
    {
        foreach (int item in array)
        {
            if (item == 0) return false;
        }
        return true;
    }

    public string GetForgeInfo()
    {
        string oreInfo = "Ore: ";
        for (int i = 0; i < oreCells.Length; i++)
        {
            oreInfo += oreCells[i] == 1 ? $"[{oreTypes[i]}] " : "[Empty] ";
        }

        string fuelInfo = "Fuel: ";
        for (int i = 0; i < fuelCells.Length; i++)
        {
            fuelInfo += fuelCells[i] == 1 ? $"[{fuelTypes[i]}] " : "[Empty] ";
        }

        return $"{oreInfo}\n{fuelInfo}\n" +
               $"Status: {(isMelting ? $"Melting {currentOreType}... {currentMeltingTime:F1}/{meltingTime:F1}s" : "Ready")}";
    }
}