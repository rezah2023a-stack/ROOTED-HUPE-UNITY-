using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    public static SpotlightManager Instance;

    [Header("Spotlight Parents")]
    public GameObject spotlight1Parent;
    public GameObject spotlight2Parent;
    public GameObject spotlight3Parent;
    public GameObject spotlight4Parent;

    [Header("Settings")]
    public float ldrThreshold = 500f;

    void Awake() {
        Instance = this;
    }

    void Start() {
        // All spotlights OFF at start
        spotlight1Parent.SetActive(false);
        spotlight2Parent.SetActive(false);
        spotlight3Parent.SetActive(false);
        spotlight4Parent.SetActive(false);
    }

    public void TurnOnSpotlight(int index) {
        Debug.Log("Spotlight " + (index + 1) + " ON");
        switch (index) {
            case 0: spotlight1Parent.SetActive(true); break;
            case 1: spotlight2Parent.SetActive(true); break;
            case 2: spotlight3Parent.SetActive(true); break;
            case 3: spotlight4Parent.SetActive(true); break;
        }
    }
}