using UnityEngine;
using System.IO.Ports;

public class ArduinoTest : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portNumber = "11401"; // only change this number in Inspector
    public int baudRate = 9600;

    private SerialPort serial;
    private string portName => "/dev/cu.usbmodem" + portNumber;

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
        // Expected format: "moisture,ldr1,ldr2,ldr3,ldr4,touch,state"
        string[] values = data.Split(',');
        if (values.Length < 7) return;

        try {
            SensorData.Instance.moisture = float.Parse(values[0]);
            SensorData.Instance.ldr      = float.Parse(values[1]); // ldr1 as main ldr
            SensorData.Instance.touch    = long.Parse(values[5]);
            SensorData.Instance.state    = values[6];

            // Update each spotlight individually
            SpotlightManager.Instance.UpdateSpotlights(
                float.Parse(values[1]), // ldr1
                float.Parse(values[2]), // ldr2
                float.Parse(values[3]), // ldr3
                float.Parse(values[4])  // ldr4
            );

            SeasonManager.Instance.UpdateWeather();

            Debug.Log($"Moisture: {values[0]} | LDR1: {values[1]} | LDR2: {values[2]} | LDR3: {values[3]} | LDR4: {values[4]} | Touch: {values[5]} | State: {values[6]}");
        }
        catch {
            Debug.LogWarning("Failed to parse data: " + data);
        }
    }

    void OnDestroy() {
        if (serial != null && serial.IsOpen) serial.Close();
    }
}