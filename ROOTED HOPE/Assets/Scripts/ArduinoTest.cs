using UnityEngine;
using System.IO.Ports;

public class ArduinoTest : MonoBehaviour
{
    SerialPort serial;
    public string portName = "/dev/cu.usbmodem11401";

    SeasonManager seasonManager;

    public int moisture = 0;
    public int ldr      = 0;
    public int fsr      = 0;
    public int season   = 0;

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
                string[] values = data.Split(',');

                if (values.Length >= 4)
                {
                    moisture = int.Parse(values[0]);
                    ldr      = int.Parse(values[1]);
                    fsr      = int.Parse(values[2]);
                    season   = int.Parse(values[3]);

                    Debug.Log("Moisture: " + moisture + " LDR: " + ldr + " FSR: " + fsr + " Season: " + season);

                    if (seasonManager != null)
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