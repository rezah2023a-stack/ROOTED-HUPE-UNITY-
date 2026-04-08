using UnityEngine;
using System.IO.Ports;

public class ArduinoTest : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "/dev/cu.usbmodem11401";
    public int baudRate = 9600;

    private SerialPort serial;

    void Start() {
        serial = new SerialPort(portName, baudRate);
        serial.ReadTimeout = 100;

        try {
            serial.Open();
            Debug.Log("Serial port opened: " + portName);
        }
        catch {
            Debug.LogError("Failed to open serial port: " + portName);
        }
    }

    void Update() {
        if (serial == null || !serial.IsOpen) return;

        try {
            string data = serial.ReadLine().Trim();
            ParseData(data);
        }
        catch { }
    }

    void ParseData(string data) {
        // Expected format: "650,320,1200,Spring"
        string[] values = data.Split(',');
        if (values.Length < 4) return;

        try {
            SensorData.Instance.moisture = float.Parse(values[0]);
            SensorData.Instance.ldr      = float.Parse(values[1]);
            SensorData.Instance.touch    = long.Parse(values[2]);
            SensorData.Instance.state    = values[3];

            SeasonManager.Instance.UpdateWeather();

            Debug.Log($"Moisture: {values[0]} | LDR: {values[1]} | Touch: {values[2]} | State: {values[3]}");
        }
        catch {
            Debug.LogWarning("Failed to parse data: " + data);
        }
    }

    void OnDestroy() {
        if (serial != null && serial.IsOpen) serial.Close();
    }
}