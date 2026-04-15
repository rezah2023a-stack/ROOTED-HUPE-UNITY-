using UnityEngine;

public class SensorData : MonoBehaviour
{
    public static SensorData Instance;

    [Header("Sensor Values")]
    public float moisture;
    public float ldr;
    public string state;

    [Header("Threshold Values")]
    public float moistureWet = 600f;
    public float ldrBright = 500f;

    public bool IsWet => moisture >= moistureWet;
    public bool IsBright => ldr >= ldrBright;

    void Awake() {
        Instance = this;
    }
}