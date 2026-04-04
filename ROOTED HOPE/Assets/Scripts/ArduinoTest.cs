using UnityEngine;
using System.IO.Ports;

public class ArduinoTest : MonoBehaviour
{
    SerialPort serial;
    public string portName = "/dev/cu.usbmodem11401";

    SeasonManager seasonManager;

    void Start()
    {
        seasonManager = FindObjectOfType<SeasonManager>();
        serial = new SerialPort(portName, 9600);
        serial.Open();
        Debug.Log("Connected!");
    }

    void Update()
    {
        if (serial.IsOpen)
        {
            try
            {
                string data = serial.ReadLine();
                Debug.Log("Moisture: " + data);

                string[] values = data.Split(',');
                if (values.Length >= 4)
                {
                    int season = int.Parse(values[3]);
                    seasonManager.SetSeason(season);
                }
            }
            catch { }
        }
    }

    void OnApplicationQuit()
    {
        serial.Close();
    }
}