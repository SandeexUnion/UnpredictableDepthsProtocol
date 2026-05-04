using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    
    public void DisableAllPanels()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }
    public void EnablePanel(int index)
    {
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogError("Index out of range");
            return;
        }
        
        DisableAllPanels();
        panels[index].SetActive(true);
    }
    public void BackButton()
    {
        DisableAllPanels();
        panels[0].SetActive(true);
    }
    void Start()
    {
        EnablePanel(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
