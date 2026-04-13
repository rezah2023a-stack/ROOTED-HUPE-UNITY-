using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    public static SpotlightManager Instance;

    [Header("Spotlight Parents")]
    public GameObject spotlight1;
    public GameObject spotlight2;
    public GameObject spotlight3;
    public GameObject spotlight4;

    [Header("Settings")]
    public float ldrThreshold = 500f;

    void Awake() {
        Instance = this;
    }

    void Start() {
        // All spotlights OFF at start
        spotlight1.SetActive(false);
        spotlight2.SetActive(false);
        spotlight3.SetActive(false);
        spotlight4.SetActive(false);
    }

    public void UpdateSpotlights(float ldr1, float ldr2, float ldr3, float ldr4) {
        spotlight1.SetActive(ldr1 >= ldrThreshold);
        spotlight2.SetActive(ldr2 >= ldrThreshold);
        spotlight3.SetActive(ldr3 >= ldrThreshold);
        spotlight4.SetActive(ldr4 >= ldrThreshold);
    }
}