using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CraftingTable : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private int width = 3;
    [SerializeField] private int height = 3;
    [SerializeField] private LayerMask tileLayerMask;

    public InteractableType[] interactableTypes { get; set; } = new InteractableType[] { InteractableType.Craftable };

    [System.Serializable]
    public class CraftingRecipe
    {
        public string Name;
        public CraftingIngredient[] Ingredients; // Теперь используем Ingredient с количеством
        public GameObject ResultPrefab;
        public Sprite ResultIcon;
        public string ResultName;
        public string ResultDescription;
        public Vector2Int[] pattern; // Паттерн размещения на сетке
    }

    [System.Serializable]
    public class CraftingIngredient
    {
        public string itemName;     // Название предмета
        public ResourceType resourceType; // Тип ресурса
        public int amount;          // Количество
    }

    [SerializeField] private CraftingRecipe[] craftingRecipes;

    private CraftingTableTile[,] tiles;
    private Item[,] itemsOnGrid;
    private bool isCrafting = false;
    private CraftingTableTile highlightedTile;

    // Хранилище ресурсов (как в Forge)
    private List<ResourceData>[] craftingCells; // Ячейки для ресурсов
    private int maxCells = 9; // 3x3 = 9 ячеек

    [System.Serializable]
    public class ResourceData
    {
        public string itemName;
        public ResourceType resourceType;
        public Item item;

        public ResourceData(Item item, ResourceType type)
        {
            this.item = item;
            this.itemName = item.itemName;
            this.resourceType = type;
        }
    }

    void Awake()
    {
        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void Start()
    {
        InitializeGrid();
        InitializeCraftingCells();
    }

    void Update()
    {
        UpdateHighlight();
    }

    void InitializeGrid()
    {
        tiles = new CraftingTableTile[width, height];
        itemsOnGrid = new Item[width, height];

        var allTiles = GetComponentsInChildren<CraftingTableTile>();
        foreach (var tile in allTiles)
        {
            if (tile.ID < width * height)
            {
                int x = tile.ID % width;
                int y = tile.ID / width;
                tiles[x, y] = tile;

                // Подписываемся на оба события
                tile.OnTileSelected.AddListener(OnTileSelected);
                tile.OnTileTakeItem.AddListener(OnTileTakeItem); // Добавьте эту строку
            }
        }
    }

    // Новый метод для ПКМ - забираем предмет
    public void OnTileTakeItem(CraftingTableTile tile)
    {
        if (!tile.IsEmpty)
        {
            TakeItemFromTile(tile);
            Debug.Log("Item taken from tile!");
        }
        else
        {
            Debug.Log("Tile is empty, nothing to take!");
        }
    }

    void InitializeCraftingCells()
    {
        craftingCells = new List<ResourceData>[maxCells];
        for (int i = 0; i < maxCells; i++)
        {
            craftingCells[i] = new List<ResourceData>();
        }
    }

    private void UpdateHighlight()
    {
        if (highlightedTile != null)
        {
            highlightedTile.SetHighlight(false);
            highlightedTile = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f, tileLayerMask))
        {
            CraftingTableTile tile = hit.collider.GetComponent<CraftingTableTile>();
            if (tile != null && tile.IsEmpty)
            {
                highlightedTile = tile;
                highlightedTile.SetHighlight(true);
            }
        }
    }

    public void OnTileSelected(CraftingTableTile tile)
    {
        if (inventoryController == null) return;

        Item selectedItem = inventoryController.GetSelectedItem();

        if (CanInteractWith(selectedItem))
        {
            TryAddResource(tile, selectedItem);
        }
        else if (selectedItem == null && !tile.IsEmpty)
        {
            TakeItemFromTile(tile);
        }
    }

    private void TryAddResource(CraftingTableTile tile, Item item)
    {
        // Проверяем, реализует ли предмет интерфейс IResourceble
        IResourceble resourceItem = item as IResourceble;

        if (resourceItem == null)
        {
            Debug.Log($"Item {item.itemName} is not a resource! It needs to be a ResourceItem type.");
            return;
        }

        // Находим координаты клетки
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == tile && itemsOnGrid[x, y] == null)
                {
                    int cellIndex = y * width + x;

                    // СОЗДАЕМ КОПИЮ ПРЕДМЕТА вместо использования ссылки
                    Item clonedItem = CloneItem(item);

                    // Добавляем ресурс в ячейку
                    ResourceData resourceData = new ResourceData(clonedItem, resourceItem.resourceType);
                    craftingCells[cellIndex].Add(resourceData);

                    // Отображаем предмет на сетке
                    itemsOnGrid[x, y] = clonedItem;
                    tile.PlaceItem(clonedItem);

                    // Удаляем предмет из инвентаря (оригинал)
                    inventoryController.RemoveItem(inventoryController.SelectedSlotIndex);

                    Debug.Log($"Added {clonedItem.itemName} (Type: {resourceItem.resourceType}) to cell ({x}, {y})");
                    Debug.Log(GetCraftingTableInfo());

                    // Проверяем возможность крафта
                    TryCraft();
                    return;
                }
            }
        }
    }

    // Новый метод для клонирования предмета
    private Item CloneItem(Item original)
    {
        // Создаем временный GameObject для клонирования
        GameObject tempObj = new GameObject($"Cloned_{original.itemName}");

        // Проверяем, является ли оригинал ResourceItem
        IResourceble originalResource = original as IResourceble;

        Item newItem;

        if (originalResource != null)
        {
            // Создаем ResourceItem
            ResourceItem resourceItem = tempObj.AddComponent<ResourceItem>();
            resourceItem.resourceType = originalResource.resourceType;
            newItem = resourceItem;
        }
        else
        {
            // Создаем обычный Item
            newItem = tempObj.AddComponent<Item>();
        }

        // Копируем данные
        newItem.itemName = original.itemName;
        newItem.itemIcon = original.itemIcon;
        newItem.Descriptions = original.Descriptions;
        newItem.CanBePass = original.CanBePass;

        // Важно: не активируем объект, он будет просто хранить данные
        tempObj.SetActive(false);

        return newItem;
    }

    private void TakeItemFromTile(CraftingTableTile tile)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == tile && itemsOnGrid[x, y] != null)
                {
                    Item itemOnTile = itemsOnGrid[x, y];
                    int cellIndex = y * width + x;

                    // Проверяем, есть ли ресурс в хранилище
                    if (craftingCells[cellIndex].Count > 0)
                    {
                        ResourceData resourceToRemove = craftingCells[cellIndex][0];

                        // СОЗДАЕМ НОВЫЙ ПРЕДМЕТ ДЛЯ ИНВЕНТАРЯ
                        Item itemToAdd = CreateItemForInventory(resourceToRemove);

                        // Пытаемся добавить в инвентарь
                        if (AddItemToInventory(itemToAdd))
                        {
                            // Удаляем из хранилища
                            craftingCells[cellIndex].RemoveAt(0);

                            // Очищаем сетку и визуал
                            itemsOnGrid[x, y] = null;
                            tile.RemoveItem();

                            Debug.Log($"Took {resourceToRemove.itemName} from tile ({x}, {y})");

                            // Уничтожаем клон предмета, если он существует
                            if (itemOnTile != null && itemOnTile.gameObject != null)
                            {
                                Destroy(itemOnTile.gameObject);
                            }
                        }
                        else
                        {
                            Debug.Log("Inventory is full!");
                        }
                    }
                    return;
                }
            }
        }
    }

    // Новый метод для создания предмета для инвентаря
    private Item CreateItemForInventory(ResourceData resource)
    {
        // Создаем GameObject для предмета
        GameObject newItemObj = new GameObject($"Inv_{resource.itemName}");

        Item newItem;

        // Создаем правильный тип предмета
        if (resource.resourceType != ResourceType.None)
        {
            // Это ресурс - создаем ResourceItem
            ResourceItem resourceItem = newItemObj.AddComponent<ResourceItem>();
            resourceItem.resourceType = resource.resourceType;
            newItem = resourceItem;
        }
        else
        {
            // Обычный предмет
            newItem = newItemObj.AddComponent<Item>();
        }

        // Заполняем данные
        newItem.itemName = resource.itemName;
        newItem.itemIcon = resource.item.itemIcon;
        newItem.Descriptions = resource.item.Descriptions;
        newItem.CanBePass = true;

        // Деактивируем GameObject (он нужен только для хранения данных)
        newItemObj.SetActive(false);

        return newItem;
    }

    private void TryCraft()
    {
        if (isCrafting) return;

        foreach (var recipe in craftingRecipes)
        {
            if (MatchesRecipe(recipe))
            {
                StartCoroutine(CraftItem(recipe));
                break;
            }
        }
    }

    private bool MatchesRecipe(CraftingRecipe recipe)
    {
        // Собираем все ресурсы с сетки
        Dictionary<string, int> availableResources = new Dictionary<string, int>();
        Dictionary<ResourceType, int> availableByType = new Dictionary<ResourceType, int>();

        for (int i = 0; i < craftingCells.Length; i++)
        {
            foreach (var resource in craftingCells[i])
            {
                // По имени предмета
                if (availableResources.ContainsKey(resource.itemName))
                    availableResources[resource.itemName]++;
                else
                    availableResources[resource.itemName] = 1;

                // По типу ресурса
                if (availableByType.ContainsKey(resource.resourceType))
                    availableByType[resource.resourceType]++;
                else
                    availableByType[resource.resourceType] = 1;
            }
        }

        // Проверяем каждый ингредиент
        foreach (var ingredient in recipe.Ingredients)
        {
            int requiredAmount = ingredient.amount;

            // Проверяем по имени предмета
            if (availableResources.ContainsKey(ingredient.itemName) &&
                availableResources[ingredient.itemName] >= requiredAmount)
            {
                continue;
            }

            // Проверяем по типу ресурса
            if (availableByType.ContainsKey(ingredient.resourceType) &&
                availableByType[ingredient.resourceType] >= requiredAmount)
            {
                continue;
            }

            return false; // Ингредиент не найден
        }

        // Дополнительно проверяем паттерн размещения (если указан)
        if (recipe.pattern != null && recipe.pattern.Length > 0)
        {
            return CheckPattern(recipe);
        }

        return true;
    }

    private bool CheckPattern(CraftingRecipe recipe)
    {
        // Очищаем сетку от пустых мест для проверки паттерна
        Item[,] patternGrid = new Item[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (itemsOnGrid[x, y] != null)
                {
                    patternGrid[x, y] = itemsOnGrid[x, y];
                }
            }
        }

        // Проверяем соответствие паттерну
        foreach (var pos in recipe.pattern)
        {
            if (pos.x >= width || pos.y >= height) return false;

            if (patternGrid[pos.x, pos.y] == null)
                return false;
        }

        // Проверяем, что нет лишних предметов вне паттерна
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (patternGrid[x, y] != null && !recipe.pattern.Contains(new Vector2Int(x, y)))
                    return false;
            }
        }

        return true;
    }

    private IEnumerator CraftItem(CraftingRecipe recipe)
    {
        isCrafting = true;
        Debug.Log($"Crafting {recipe.Name}...");

        // Визуальный эффект
        foreach (var tile in tiles)
        {
            if (!tile.IsEmpty)
            {
                tile.StartCraftingEffect();
            }
        }

        yield return new WaitForSeconds(1f);

        // Удаляем использованные ресурсы
        ConsumeResources(recipe);

        // Создаем результат
        CreateResult(recipe);

        isCrafting = false;
        Debug.Log($"Crafted {recipe.Name}!");
    }

    private void ConsumeResources(CraftingRecipe recipe)
    {
        Dictionary<string, int> toConsume = new Dictionary<string, int>();

        // Считаем, сколько нужно потребить каждого ресурса
        foreach (var ingredient in recipe.Ingredients)
        {
            if (toConsume.ContainsKey(ingredient.itemName))
                toConsume[ingredient.itemName] += ingredient.amount;
            else
                toConsume[ingredient.itemName] = ingredient.amount;
        }

        // Проходим по всем ячейкам и удаляем ресурсы
        for (int i = 0; i < craftingCells.Length; i++)
        {
            for (int j = craftingCells[i].Count - 1; j >= 0; j--)
            {
                ResourceData resource = craftingCells[i][j];

                if (toConsume.ContainsKey(resource.itemName) && toConsume[resource.itemName] > 0)
                {
                    toConsume[resource.itemName]--;
                    craftingCells[i].RemoveAt(j);

                    // Очищаем визуальное отображение
                    int x = i % width;
                    int y = i / width;
                    itemsOnGrid[x, y] = null;
                    tiles[x, y].RemoveItem();
                }
            }
        }
    }

    private void CreateResult(CraftingRecipe recipe)
    {
        GameObject craftedItem;

        // Используем префаб если есть
        if (recipe.ResultPrefab != null)
        {
            craftedItem = Instantiate(recipe.ResultPrefab, dropPoint.position, Quaternion.identity);
        }
        else
        {
            craftedItem = new GameObject(recipe.ResultName);
        }

        // Настраиваем компонент Item
        Item item = craftedItem.GetComponent<Item>();
        if (item == null) item = craftedItem.AddComponent<Item>();

        item.itemName = recipe.ResultName;
        item.itemIcon = recipe.ResultIcon;
        item.Descriptions = recipe.ResultDescription;
        item.CanBePass = true;

        // Добавляем физику для выброса
        if (craftedItem.GetComponent<Rigidbody>() == null)
            craftedItem.AddComponent<Rigidbody>();

        Rigidbody rb = craftedItem.GetComponent<Rigidbody>();
        Vector3 throwForce = transform.forward * 2f + Vector3.up * 2f;
        rb.AddForce(throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f), ForceMode.Impulse);

        // Если предмет должен быть ресурсом, добавляем интерфейс
        if (item is IResourceble resourceItem)
        {
            // Настраиваем тип ресурса для результата
            // resourceItem.resourceType = ResourceType.Metal; // Пример
        }

        Debug.Log($"Created {item.itemName}!");
    }

    private bool AddItemToInventory(Item item)
    {
        GameObject tempItem = new GameObject("TempItem");
        Item tempItemComponent = tempItem.AddComponent<Item>();
        tempItemComponent.itemName = item.itemName;
        tempItemComponent.itemIcon = item.itemIcon;
        tempItemComponent.Descriptions = item.Descriptions;

        bool added = inventoryController.AddNewItem(tempItemComponent);
        Destroy(tempItem);

        return added;
    }

    public bool CanInteractWith(Item tool)
    {
        // Проверяем, является ли предмет ресурсом
        if (tool is IResourceble)
            return true;

        return tool == null; // Можно брать предметы руками
    }

    public void Interact()
    {
        Debug.Log("Click on a specific tile to place or take items!");
    }

    public string GetCraftingTableInfo()
    {
        string info = "Crafting Table Grid:\n";
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (itemsOnGrid[x, y] != null)
                    info += $"[{itemsOnGrid[x, y].itemName}] ";
                else
                    info += "[Empty] ";
            }
            info += "\n";
        }
        return info;
    }
}