using UnityEngine;

public class SensorData : MonoBehaviour
{
    public static SensorData Instance;

    [Header("Sensor Values")]
    public float moisture;
    public float ldr;
    public long touch;
    public string state;

    [Header("Threshold Values")]
    public float moistureWet = 600f;
    public float ldrBright = 500f;
    public long touchThreshold = 1000;

    public bool IsWet => moisture >= moistureWet;
    public bool IsBright => ldr >= ldrBright;
    public bool IsTouched => touch > touchThreshold;

    void Awake() {
        Instance = this;
    }
}