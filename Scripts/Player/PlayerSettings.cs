using UnityEngine;
public class PlayerSettings : MonoBehaviour
{
    public float sensitivity;
    public float grassDistance;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}