using UnityEngine;

public class TransferResoures : MonoBehaviour
{
    [SerializeField] private MinePickUpRange minePickUpRange;
    [SerializeField] private PrepareDataToTransfer prepareDataToTransfer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prepareDataToTransfer = FindAnyObjectByType<PrepareDataToTransfer>();
        minePickUpRange = FindAnyObjectByType<MinePickUpRange>();
    }

    public void TransferResources()
    {
        Send(prepareDataToTransfer.GenerateJsonToTransfer(minePickUpRange.GetAllItems()));
        minePickUpRange.ClearAllItems();
    }
    public void Send(string json)
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
