using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ResourceTransferManager : MonoBehaviour
{
    [SerializeField] private UDPListener udpListener;
    [SerializeField] private PrepareDataToTransfer dataSerializer;
    [SerializeField] private MinePickUpRange localCart; // Ваша тележка

    void Start()
    {
        if (udpListener == null)
            udpListener = FindAnyObjectByType<UDPListener>();

        if (dataSerializer == null)
            dataSerializer = FindAnyObjectByType<PrepareDataToTransfer>();

        if (localCart == null)
            localCart = FindAnyObjectByType<MinePickUpRange>();

        // Подписываемся на событие получения ресурсов
        if (udpListener != null)
        {
            udpListener.OnResourcesReceived += OnResourcesReceived;
        }
    }

    // Вызывается, когда игрок нажимает кнопку "Отправить тележку"
    public void SendCartResources()
    {
        // Проверяем, есть ли ресурсы в тележке
        if (localCart == null || !localCart.HasItems)
        {
            Debug.Log("Тележка пуста! Нечего отправлять.");
            return;
        }

        // Получаем все ресурсы из тележки
        List<ItemData> itemsToSend = localCart.GetAllItems();

        if (itemsToSend == null || itemsToSend.Count == 0)
        {
            Debug.Log("Нет ресурсов для отправки");
            return;
        }

        // Сериализуем в JSON
        string jsonData = dataSerializer.GenerateJsonToTransfer(itemsToSend);

        // Получаем адрес другого игрока
        IPEndPoint otherPlayer = GameNetworkManager.Instance.GetOtherPlayerEndpoint();

        if (otherPlayer == null)
        {
            Debug.LogError("Другой игрок не найден! Сначала подключитесь.");
            return;
        }

        // Отправляем
        udpListener.SendResources(jsonData, otherPlayer);

        // Очищаем локальную тележку (ресурсы ушли)
        localCart.ClearAllItems();

        Debug.Log($"Отправлено {itemsToSend.Count} ресурсов другому игроку");

        // Визуальный эффект отправки (анимация тележки)
        StartCoroutine(AnimateCartSending());
    }

    // Обработка полученных ресурсов
    private void OnResourcesReceived(string jsonData, IPEndPoint sender)
    {
        Debug.Log($"Получены ресурсы от {sender}");

        // Десериализуем JSON обратно в список предметов
        List<ItemData> receivedItems = dataSerializer.ParseJsonToItems(jsonData);

        if (receivedItems == null || receivedItems.Count == 0)
        {
            Debug.LogWarning("Получен пустой список ресурсов");
            return;
        }

        // Добавляем ресурсы в локальную тележку
        if (localCart != null)
        {
            localCart.AddItems(receivedItems);
            Debug.Log($"Получено {receivedItems.Count} ресурсов в тележку");

            // Визуальный эффект получения
            StartCoroutine(AnimateCartReceiving());
        }
    }

    private System.Collections.IEnumerator AnimateCartSending()
    {
        // TODO: Анимация отъезда тележки
        Debug.Log("📤 Тележка отправляется...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("📤 Тележка в пути!");
    }

    private System.Collections.IEnumerator AnimateCartReceiving()
    {
        // TODO: Анимация приезда тележки
        Debug.Log("📥 Тележка прибывает...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("📥 Ресурсы выгружены!");
    }

    void OnDestroy()
    {
        if (udpListener != null)
            udpListener.OnResourcesReceived -= OnResourcesReceived;
    }
}