using UnityEngine;

[CreateAssetMenu(fileName = "SmeltingConfigSO", menuName = "Scriptable Objects/SmeltingConfigSO")]
public class SmeltingConfigSO : ScriptableObject
{
    public int maxFuelCells = 3;
    public int maxOreCells = 3;
    public float meltingTime = 5f;
}
