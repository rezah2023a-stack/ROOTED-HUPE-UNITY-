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
        Debug.Log("Received: " + data);

        if (data == "Rain") {
            // Trigger rain in Unity
            SeasonManager.Instance.TriggerRain();
        }
        else if (data == "Spotlight1") {
            SpotlightManager.Instance.TurnOnSpotlight(0);
        }
        else if (data == "Spotlight2") {
            SpotlightManager.Instance.TurnOnSpotlight(1);
        }
        else if (data == "Spotlight3") {
            SpotlightManager.Instance.TurnOnSpotlight(2);
        }
        else if (data == "Spotlight4") {
            SpotlightManager.Instance.TurnOnSpotlight(3);
        }
        else if (data == "Spring") {
            SeasonManager.Instance.TriggerSpring();
        }
        else if (data == "Touch") {
            SeasonManager.Instance.TriggerTouch();
        }
    }

    void OnDestroy() {
        if (serial != null && serial.IsOpen) serial.Close();
    }
}