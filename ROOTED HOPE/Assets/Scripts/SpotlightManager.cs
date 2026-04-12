using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    public static SpotlightManager Instance;

    [Header("Spotlights")]
    public Light spotlight1;
    public Light spotlight2;
    public Light spotlight3;
    public Light spotlight4;

    [Header("Settings")]
    public float ldrThreshold = 500f;
    public float fadeSpeed = 2f;
    public float maxIntensity = 3f;

    void Awake() {
        Instance = this;
    }

    void Start() {
        // All spotlights OFF at start
        SetSpotlight(spotlight1, false);
        SetSpotlight(spotlight2, false);
        SetSpotlight(spotlight3, false);
        SetSpotlight(spotlight4, false);
    }

    void Update() {
        // Smooth fade every frame
        FadeSpotlight(spotlight1);
        FadeSpotlight(spotlight2);
        FadeSpotlight(spotlight3);
        FadeSpotlight(spotlight4);
    }

    public void UpdateSpotlights(float ldr1, float ldr2, float ldr3, float ldr4) {
        SetSpotlight(spotlight1, ldr1 >= ldrThreshold);
        SetSpotlight(spotlight2, ldr2 >= ldrThreshold);
        SetSpotlight(spotlight3, ldr3 >= ldrThreshold);
        SetSpotlight(spotlight4, ldr4 >= ldrThreshold);
    }

    void SetSpotlight(Light spotlight, bool isLit) {
        // Store target intensity in light's bounceIntensity as temp variable
        spotlight.bounceIntensity = isLit ? maxIntensity : 0f;
    }

    void FadeSpotlight(Light spotlight) {
        // Smoothly fade toward target intensity
        spotlight.intensity = Mathf.Lerp(
            spotlight.intensity,
            spotlight.bounceIntensity,
            Time.deltaTime * fadeSpeed
        );
    }
}