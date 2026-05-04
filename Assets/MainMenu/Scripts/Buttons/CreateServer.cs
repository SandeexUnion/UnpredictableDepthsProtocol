using UnityEngine;

public class CreateServer : MonoBehaviour
{
    UDPListener listener;
    PanelManager panelManager;

    void Start()
    {
        listener = FindAnyObjectByType<UDPListener>();
        panelManager = FindAnyObjectByType<PanelManager>();
    }
    public void Create()
    {
        listener.CreateServer();
        panelManager.EnablePanel(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
