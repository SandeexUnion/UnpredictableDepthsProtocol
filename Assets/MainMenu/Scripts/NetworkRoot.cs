using UnityEngine;

public class NetworkRoot : MonoBehaviour
{
    void Awake()
    {
        // Делаем этот объект и всех его детей неуничтожаемыми
        DontDestroyOnLoad(gameObject);
    }
}