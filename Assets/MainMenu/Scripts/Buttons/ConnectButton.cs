using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class ConnectButton : MonoBehaviour
{
    ConnectToServer connector;
    [SerializeField] InputField ipInputField;

    void Start()
    {
        connector = FindAnyObjectByType<ConnectToServer>();
    }

    public void Connect()
    {
        string[] parts = ipInputField.text.Split(':');
        if (parts.Length != 2)
        {
            Debug.LogError("Invalid format. Use IP:PORT");
            return;
        }

        string ip = parts[0];
        if (!int.TryParse(parts[1], out int port))
        {
            Debug.LogError("Invalid port");
            return;
        }

        connector.Connect(ip, port);
    }
}
