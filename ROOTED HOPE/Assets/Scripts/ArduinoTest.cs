using UnityEngine;
using System.IO.Ports;

public class ArduinoTest : MonoBehaviour
{
    SerialPort serial;
    public string portName = "/dev/cu.usbmodem1301";

    void Start()
    {
        serial = new SerialPort(portName, 9600);
        serial.Open();
        Debug.Log("Connect!");
    }

    void Update()
    {
        if (serial.IsOpen)
        {
            try
            {
                string data = serial.ReadLine();
                Debug.Log("moisture: " + data);
            }
            catch { }
        }
    }

    void OnApplicationQuit()
    {
        serial.Close();
    }
}