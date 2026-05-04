using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ReadIP : MonoBehaviour
{
    [SerializeField] Text ipText;

    void Start()
    {
        string localIP = "127.0.0.1";

        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                // Не только 192, но и 10.x.x.x, 172.16.x.x и т.д.
                localIP = ip.ToString();
                break;
            }
        }

        ipText.text = $"Your address: {localIP}:9000";
        GameNetworkManager.Instance.SetPlayerEndpoint(localIP, 9000);
    }
}
