using System.Net;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager Instance { get; private set; }

    public IPEndPoint OtherPlayerEndpoint { get; private set; }
    public IPEndPoint PlayerEndpoint { get; private set; }
    public bool IsHost { get; set; }
    public bool IsConnected { get; private set; }
    public bool IsConnectedToOtherPlayer => OtherPlayerEndpoint != null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при загрузке сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetOtherPlayerEndpoint(string ip, int port)
    {
        OtherPlayerEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        IsConnected = true;
        Debug.Log($"Other player set to: {OtherPlayerEndpoint}");

        // Переход на геймплей
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public bool CanSendResources()
    {
        if (!IsConnectedToOtherPlayer)
        {
            Debug.LogWarning("Нет подключения к другому игроку!");
            return false;
        }
        return true;
    }
    public void SetPlayerEndpoint(string ip, int port)
    {
        PlayerEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        Debug.Log($"My endpoint: {PlayerEndpoint}");
    }

    public IPEndPoint GetOtherPlayerEndpoint() => OtherPlayerEndpoint;
    public IPEndPoint GetPlayerEndpoint() => PlayerEndpoint;
}