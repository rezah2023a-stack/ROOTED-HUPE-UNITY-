using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Hierarchy Objects")]
    public GameObject Wintergroup;
    public GameObject Rain;
    public GameObject SpringGroup;

    [Header("Wind")]
    public GameObject WindFar_Left;
    public GameObject WindClose;
    public GameObject WindZone;

    [Header("Lighting")]
    public Light DirectionalLight;
    public float fadeSpeed = 2f;

    [Header("Global Volume")]
    public Volume globalVolume;

    [Header("Terrain")]
    public Terrain terrain;

    [Header("Transition Speed")]
    public float transitionDuration = 8f;

    // Terrain layer index
    // 0 = Snow, 1 = Melting, 2 = Grass, 3 = Cliffs
    private int SNOW    = 0;
    private int MELTING = 1;
    private int GRASS   = 2;

    private string currentState = "";
    private bool isSpringLocked = false; // once Spring is reached, lock permanently
    private ColorAdjustments colorAdjustments;

    void Awake() {
        Instance = this;
    }

    void Start() {
        // Cache color adjustments from Global Volume profile
        if (globalVolume != null)
            globalVolume.profile.TryGet(out colorAdjustments);

        SetWinter();
    }

    public void UpdateWeather() {
        // Once Spring is reached, lock the state permanently
        if (isSpringLocked) return;

        string newState = SensorData.Instance.state;

        // Ignore if state has not changed
        if (newState == currentState) return;
        currentState = newState;

        Debug.Log("State changed to: " + newState);

        switch (newState) {
            case "Winter": SetWinter();                 break;
            case "Rain":   StartCoroutine(SetRain());   break;
            case "Clear":  StartCoroutine(SetClear());  break;
            case "Spring": StartCoroutine(SetSpring()); break;
            case "Touch":  StartCoroutine(SetTouch());  break;
        }
    }

    // ─────────────────────────────────────────────
    // WINTER — fog ON, cold blue light
    // ─────────────────────────────────────────────
    void SetWinter() {
        Wintergroup.SetActive(true);
        Rain.SetActive(false);
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(true);
        WindClose.SetActive(true);
        WindZone.SetActive(true);

        // Enable fog for winter
        SetFog(true, new Color(0.78f, 0.85f, 0.88f), 0.002f);

        if (DirectionalLight != null) {
            DirectionalLight.color     = new Color(0.7f, 0.8f, 1f);
            DirectionalLight.intensity = 0.6f;
        }

        RenderSettings.ambientLight = new Color(0.55f, 0.65f, 0.78f);

        if (colorAdjustments != null)
            colorAdjustments.colorFilter.value = new Color(0.88f, 0.92f, 1f);

        SetTerrainLayer(SNOW);
    }

    // ─────────────────────────────────────────────
    // RAIN — fog ON, dark overcast
    // ─────────────────────────────────────────────
    IEnumerator SetRain() {
        Wintergroup.SetActive(false);
        Rain.SetActive(true);
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(true);
        WindClose.SetActive(true);
        WindZone.SetActive(true);

        // Enable fog, heavier than winter
        SetFog(true, new Color(0.60f, 0.65f, 0.70f), 0.001f);

        yield return StartCoroutine(TransitionLighting(
            fromLightColor:   new Color(0.7f,  0.8f,  1f),
            toLightColor:     new Color(0.6f,  0.7f,  0.9f),
            fromIntensity:    DirectionalLight.intensity,
            toIntensity:      0.5f,
            fromAmbient:      new Color(0.55f, 0.65f, 0.78f),
            toAmbient:        new Color(0.45f, 0.50f, 0.60f),
            fromVolumeFilter: new Color(0.88f, 0.92f, 1f),
            toVolumeFilter:   new Color(0.75f, 0.80f, 0.90f),
            duration:         transitionDuration * 0.5f
        ));

        SetTerrainLayer(SNOW);
    }

    // ─────────────────────────────────────────────
    // CLEAR — fog OFF, bright winter sun
    // ─────────────────────────────────────────────
    IEnumerator SetClear() {
        Wintergroup.SetActive(false);
        Rain.SetActive(false);
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(false);
        WindClose.SetActive(false);
        WindZone.SetActive(true);

        // Disable fog for clear sky
        SetFog(false, new Color(0.85f, 0.88f, 0.92f), 0.004f);

        yield return StartCoroutine(TransitionLighting(
            fromLightColor:   DirectionalLight.color,
            toLightColor:     new Color(1f,    0.95f, 0.85f),
            fromIntensity:    DirectionalLight.intensity,
            toIntensity:      0.9f,
            fromAmbient:      RenderSettings.ambientLight,
            toAmbient:        new Color(0.65f, 0.70f, 0.80f),
            fromVolumeFilter: colorAdjustments != null
                                  ? colorAdjustments.colorFilter.value
                                  : Color.white,
            toVolumeFilter:   new Color(0.95f, 0.95f, 1f),
            duration:         transitionDuration * 0.5f
        ));

        SetTerrainLayer(SNOW);
    }

    // ─────────────────────────────────────────────
    // SPRING — fog OFF, warm golden light, locked permanently
    // ─────────────────────────────────────────────
    IEnumerator SetSpring() {
        SpringGroup.SetActive(true);
        Rain.SetActive(false);
        WindFar_Left.SetActive(false);
        WindClose.SetActive(false);
        WindZone.SetActive(true);

        // Disable fog for spring
        SetFog(false, new Color(0.94f, 0.91f, 0.86f), 0.003f);

        Coroutine lightingCo = StartCoroutine(TransitionLighting(
            fromLightColor:   DirectionalLight.color,
            toLightColor:     new Color(1f, 0.75f, 0.85f), 
            fromIntensity:    DirectionalLight.intensity,
            toIntensity:      1.8f,  
            fromAmbient:      RenderSettings.ambientLight,
            toAmbient:        new Color(1f, 0.75f, 0.85f),  
            fromVolumeFilter: colorAdjustments != null
                                ? colorAdjustments.colorFilter.value
                                : Color.white,
            toVolumeFilter:   new Color(1f, 0.82f, 0.90f),
            duration:         transitionDuration
));

        // Terrain transition runs alongside lighting
        yield return StartCoroutine(TransitionTerrain(SNOW, MELTING, transitionDuration * 0.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(TransitionTerrain(MELTING, GRASS, transitionDuration * 0.5f));

        yield return lightingCo;

        Wintergroup.SetActive(false);

        // Lock Spring state permanently
        isSpringLocked = true;
        Debug.Log("Spring locked!");
    }

    // ─────────────────────────────────────────────
    // TOUCH — fog OFF, instant bloom
    // ─────────────────────────────────────────────
    IEnumerator SetTouch() {
        SpringGroup.SetActive(true);

        // Disable fog for touch
        SetFog(false, new Color(0.94f, 0.91f, 0.86f), 0.003f);

        yield return StartCoroutine(TransitionLighting(
            fromLightColor:   DirectionalLight.color,
            toLightColor:     new Color(1f,    0.95f, 0.8f),
            fromIntensity:    DirectionalLight.intensity,
            toIntensity:      1.2f,
            fromAmbient:      RenderSettings.ambientLight,
            toAmbient:        new Color(0.78f, 0.91f, 0.63f),
            fromVolumeFilter: colorAdjustments != null
                                  ? colorAdjustments.colorFilter.value
                                  : Color.white,
            toVolumeFilter:   new Color(1f, 0.96f, 0.91f),
            duration:         transitionDuration * 0.5f
        ));

        yield return StartCoroutine(TransitionTerrain(SNOW, GRASS, transitionDuration * 0.5f));
    }

    // ─────────────────────────────────────────────
    // Toggle fog on or off with color and density
    // ─────────────────────────────────────────────
    void SetFog(bool enabled, Color color, float density) {
        RenderSettings.fog        = enabled;
        RenderSettings.fogColor   = color;
        RenderSettings.fogDensity = density;
    }

    // ─────────────────────────────────────────────
    // Smoothly lerp all lighting values simultaneously
    // ─────────────────────────────────────────────
    IEnumerator TransitionLighting(
        Color fromLightColor,  Color toLightColor,
        float fromIntensity,   float toIntensity,
        Color fromAmbient,     Color toAmbient,
        Color fromVolumeFilter,Color toVolumeFilter,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            // SmoothStep eases in and out naturally
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (DirectionalLight != null) {
                DirectionalLight.color     = Color.Lerp(fromLightColor, toLightColor, t);
                DirectionalLight.intensity = Mathf.Lerp(fromIntensity,  toIntensity,  t);
            }

            RenderSettings.ambientLight = Color.Lerp(fromAmbient, toAmbient, t);

            if (colorAdjustments != null)
                colorAdjustments.colorFilter.value = Color.Lerp(
                    fromVolumeFilter, toVolumeFilter, t);

            yield return null;
        }

        // Snap to exact final values
        if (DirectionalLight != null) {
            DirectionalLight.color     = toLightColor;
            DirectionalLight.intensity = toIntensity;
        }
        RenderSettings.ambientLight = toAmbient;
        if (colorAdjustments != null)
            colorAdjustments.colorFilter.value = toVolumeFilter;
    }

    // ─────────────────────────────────────────────
    // Instantly set terrain to one layer
    // ─────────────────────────────────────────────
    void SetTerrainLayer(int layerIndex) {
        if (terrain == null) return;

        float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0,
            terrain.terrainData.alphamapWidth,
            terrain.terrainData.alphamapHeight);

        for (int y = 0; y < maps.GetLength(0); y++)
            for (int x = 0; x < maps.GetLength(1); x++) {
                for (int i = 0; i < maps.GetLength(2); i++)
                    maps[y, x, i] = 0f;
                maps[y, x, layerIndex] = 1f;
            }

        terrain.terrainData.SetAlphamaps(0, 0, maps);
    }

    // ─────────────────────────────────────────────
    // Smoothly blend terrain between two layers
    // ─────────────────────────────────────────────
    IEnumerator TransitionTerrain(int fromLayer, int toLayer, float duration) {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float blend = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0,
                terrain.terrainData.alphamapWidth,
                terrain.terrainData.alphamapHeight);

            for (int y = 0; y < maps.GetLength(0); y++)
                for (int x = 0; x < maps.GetLength(1); x++) {
                    for (int i = 0; i < maps.GetLength(2); i++)
                        maps[y, x, i] = 0f;
                    maps[y, x, fromLayer] = 1f - blend;
                    maps[y, x, toLayer]   = blend;
                }

            terrain.terrainData.SetAlphamaps(0, 0, maps);
            yield return null;
        }

        SetTerrainLayer(toLayer);
    }
}