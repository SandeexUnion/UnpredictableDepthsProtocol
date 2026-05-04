using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPListener : MonoBehaviour
{
    public UdpClient udpClient;
    [SerializeField] int port = 9000;
    bool isListening = false;

    public async void SendResources(string jsonData, IPEndPoint target)
    {
        if (udpClient == null)
        {
            Debug.LogError("UDP client not initialized!");
            return;
        }

        // Формируем пакет с типом "RESOURCES"
        string packet = $"RESOURCES|{jsonData}";
        byte[] data = System.Text.Encoding.UTF8.GetBytes(packet);

        await udpClient.SendAsync(data, data.Length, target);
        Debug.Log($"Отправлено ресурсов на {target}");
    }
    public void CreateServer()
    {
        if (udpClient == null)
        {
            udpClient = new UdpClient(port);
            GameNetworkManager.Instance.IsHost = true;
            StartListening();
        }
    }

    public void StartListening()
    {
        if (!isListening)
        {
            isListening = true;
            _ = Listen();
        }
    }

    public async Task Listen()
    {
        while (true)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                string message = System.Text.Encoding.UTF8.GetString(result.Buffer);
                Debug.Log($"Received: {message} from {result.RemoteEndPoint}");

                if (message == "HELLO" && GameNetworkManager.Instance.IsHost)
                {
                    await SendMessage("WELCOME", result.RemoteEndPoint);
                    GameNetworkManager.Instance.SetOtherPlayerEndpoint(
                        result.RemoteEndPoint.Address.ToString(),
                        result.RemoteEndPoint.Port
                    );
                }
                else if (message == "WELCOME" && !GameNetworkManager.Instance.IsHost)
                {
                    GameNetworkManager.Instance.SetOtherPlayerEndpoint(
                        result.RemoteEndPoint.Address.ToString(),
                        result.RemoteEndPoint.Port
                    );
                }
                else if (message.StartsWith("RESOURCES|"))
                {
                    // Извлекаем JSON из пакета
                    string jsonData = message.Substring("RESOURCES|".Length);
                    Debug.Log($"Получен JSON ресурсов: {jsonData}");
                    OnResourcesReceived?.Invoke(jsonData, result.RemoteEndPoint);

                }

            }
            catch (System.Exception e)
            {
                Debug.LogError($"Listen error: {e.Message}");
                break;
            }
        }
    }
    public System.Action<string, IPEndPoint> OnResourcesReceived;

    public async Task SendMessage(string message, IPEndPoint endpoint)
    {
        if (udpClient == null)
        {
            Debug.LogError("UDP client not initialized!");
            return;
        }

        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length, endpoint);
        Debug.Log($"Sent: {message} to {endpoint}");
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }
}