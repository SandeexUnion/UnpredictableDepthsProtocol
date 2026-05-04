using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDataList
{
    public List<ItemData> items;
}

public class PrepareDataToTransfer : MonoBehaviour
{
    // Сериализация (было)
    public string GenerateJsonToTransfer(List<ItemData> items)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("Нет предметов для сериализации");
            return "{}";
        }

        ItemDataList wrapper = new ItemDataList();
        wrapper.items = items;

        string json = JsonUtility.ToJson(wrapper);
        Debug.Log($"Сериализовано {items.Count} предметов");
        return json;
    }

    // ДЕСЕРИАЛИЗАЦИЯ (новый метод)
    public List<ItemData> ParseJsonToItems(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}")
        {
            Debug.LogWarning("Пустой JSON");
            return new List<ItemData>();
        }

        try
        {
            ItemDataList wrapper = JsonUtility.FromJson<ItemDataList>(json);

            if (wrapper == null || wrapper.items == null)
            {
                Debug.LogWarning("Не удалось распарсить JSON");
                return new List<ItemData>();
            }

            Debug.Log($"Десериализовано {wrapper.items.Count} предметов");
            return wrapper.items;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга JSON: {e.Message}");
            return new List<ItemData>();
        }
    }
}