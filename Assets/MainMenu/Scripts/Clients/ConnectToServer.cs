using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.IO;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviour
{
    UDPListener udpListener;

    void Start()
    {
        udpListener = FindAnyObjectByType<UDPListener>();
    }

    public async void Connect(string ip, int port)
    {
        // Важно: клиент тоже должен слушать порт!
        if (udpListener.udpClient == null)
        {
            // Клиенту нужен свой порт (не тот же, что у сервера)
            var clientPort = 9001; // Можно сделать настраиваемым
            udpListener.udpClient = new UdpClient(clientPort);
        }

        // Запускаем прослушивание (еще не запущено)
        udpListener.StartListening();

        // Небольшая задержка перед отправкой
        await Task.Delay(100);

        byte[] data = System.Text.Encoding.UTF8.GetBytes("HELLO");
        var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        await udpListener.udpClient.SendAsync(data, data.Length, endpoint);
        Debug.Log($"HELLO sent to {ip}:{port}");
    }
}
