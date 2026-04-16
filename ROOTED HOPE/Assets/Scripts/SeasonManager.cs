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

    [Header("Winter Effects")]
    public ParticleSystem snowParticle;
    public ParticleSystem fogParticle;

    [Header("Wind")]
    public GameObject WindFar_Left;
    public GameObject WindClose;
    public GameObject WindZone;

    [Header("Audio")]
    public AudioSource[] windAudios;
    public AudioSource rainAudio;
    public AudioSource springAudio;
    public AudioSource waterAudio;

    [Header("Lighting")]
    public Light DirectionalLight;
    public float fadeSpeed = 2f;

    [Header("Global Volume")]
    public Volume globalVolume;

    [Header("Terrain")]
    public Terrain terrain;
    public float grassDensity = 1f;

    [Header("Skybox")]
    public Material winterSkybox;
    public Material springSkybox;

    [Header("Water")]
    public Renderer waterRenderer;

    [Header("Transition Speed")]
    public float transitionDuration = 8f;

    [Header("Spring Timer")]
    public float springDuration = 30f; // seconds before reset to winter

    // Terrain layer index
    // 0 = Snow, 1 = Melting, 2 = Grass, 3 = Cliffs
    private int SNOW = 0;
    private int GRASS = 2;

    private string currentState = "";
    private bool isSpringLocked = false;
    private ColorAdjustments colorAdjustments;

    // Track how many spotlights are on
    private int spotlightCount = 0;

    void Awake() {
        Instance = this;
    }

    void Start() {
        if (globalVolume != null)
            globalVolume.profile.TryGet(out colorAdjustments);

        SetWinter();
    }

    // ─────────────────────────────────────────────
    // Public trigger methods called from ArduinoTest
    // ─────────────────────────────────────────────
    public void TriggerRain() {
        if (isSpringLocked) return;
        if (currentState == "Rain") return;
        currentState = "Rain";
        Debug.Log("State changed to: Rain");
        StartCoroutine(SetRain());
    }

    public void TriggerSpotlight() {
        if (isSpringLocked) return;
        spotlightCount++;
        Debug.Log("Spotlight count: " + spotlightCount);

        if (spotlightCount == 1) {
            // 1st spotlight → rain gets lighter
            StartCoroutine(FadeRain(0.5f));
        }
        else if (spotlightCount == 2) {
            // 2nd spotlight → rain gets even lighter
            StartCoroutine(FadeRain(0.2f));
        }
        else if (spotlightCount == 3) {
            // 3rd spotlight → rain almost gone
            StartCoroutine(FadeRain(0.05f));
        }
        else if (spotlightCount == 4) {
            // 4th spotlight → rain fades out completely
            currentState = "Clear";
            Debug.Log("State changed to: Clear");
            StartCoroutine(SetClear());
        }
    }

    public void TriggerSpring() {
        if (isSpringLocked) return;
        if (currentState == "Spring") return;
        currentState = "Spring";
        Debug.Log("State changed to: Spring");
        StartCoroutine(SetSpring());
    }

    // ─────────────────────────────────────────────
    // Spring timer — reset to winter after springDuration
    // ─────────────────────────────────────────────
    IEnumerator SpringTimer() {
        isSpringLocked = true;
        Debug.Log("Spring locked for " + springDuration + " seconds!");

        yield return new WaitForSeconds(springDuration);

        // Reset all flags
        isSpringLocked = false;
        spotlightCount = 0;
        currentState = "";

        // Turn off all spotlights
        if (SpotlightManager.Instance != null) {
            SpotlightManager.Instance.TurnOffAllSpotlights();
        }

        Debug.Log("Spring unlocked! Back to winter.");
        SetWinter();
    }

    // ─────────────────────────────────────────────
    // WINTER — fog ON, cold blue light, no grass
    // ─────────────────────────────────────────────
    void SetWinter() {
        Wintergroup.SetActive(true);
        Rain.SetActive(false);
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(true);
        WindClose.SetActive(true);
        WindZone.SetActive(true);

        // Enable snow and fog particles
        if (snowParticle != null) {
            snowParticle.gameObject.SetActive(true);
            var emission = snowParticle.emission;
            emission.rateOverTime = 100f;
        }
        if (fogParticle != null) {
            fogParticle.gameObject.SetActive(true);
            var emission = fogParticle.emission;
            emission.rateOverTime = 100f;
        }

        SetFog(true, new Color(0.78f, 0.85f, 0.88f), 0.002f);

        if (DirectionalLight != null) {
            DirectionalLight.color     = new Color(0.7f, 0.8f, 1f);
            DirectionalLight.intensity = 0.6f;
        }

        RenderSettings.ambientLight = new Color(0.55f, 0.65f, 0.78f);

        if (colorAdjustments != null)
            colorAdjustments.colorFilter.value = new Color(0.88f, 0.92f, 1f);

        SetTerrainLayer(SNOW);
        SetTerrainDetails(0f);

        // Set winter skybox
        if (winterSkybox != null) {
            RenderSettings.skybox = winterSkybox;
            DynamicGI.UpdateEnvironment();
        }

        // Wind audio full volume in winter
        SetWindVolume(1f);

        // Spring audio off
        if (springAudio != null) {
            springAudio.volume = 0f;
            springAudio.Stop();
        }

        // Water audio off in winter
        if (waterAudio != null) {
            waterAudio.volume = 0f;
            waterAudio.Stop();
        }
    }

    // ─────────────────────────────────────────────
    // RAIN — fog ON, dark overcast, snow fades out
    // ─────────────────────────────────────────────
    IEnumerator SetRain() {
        Rain.SetActive(true);
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(true);
        WindClose.SetActive(true);
        WindZone.SetActive(true);

        SetFog(true, new Color(0.60f, 0.65f, 0.70f), 0.001f);

        // Fade out snow + fog particles, fade in rain, fade out wind simultaneously
        StartCoroutine(FadeOutWinterEffects());
        StartCoroutine(FadeAudio(rainAudio, 0f, 1f, 3f));
        StartCoroutine(FadeWindAudio(1f, 0.2f, 3f));

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
        SetTerrainDetails(0f);
    }

    // ─────────────────────────────────────────────
    // Fade out snow and fog particles smoothly
    // ─────────────────────────────────────────────
    IEnumerator FadeOutWinterEffects() {
        if (snowParticle == null && fogParticle == null) {
            Wintergroup.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
        float duration = 4f;

        // Get initial rates
        ParticleSystem.EmissionModule snowEmission = snowParticle != null ? snowParticle.emission : default;
        ParticleSystem.EmissionModule fogEmission  = fogParticle  != null ? fogParticle.emission  : default;

        float snowStartRate = snowParticle != null ? snowEmission.rateOverTime.constant : 0f;
        float fogStartRate  = fogParticle  != null ? fogEmission.rateOverTime.constant  : 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (snowParticle != null) {
                var e = snowParticle.emission;
                e.rateOverTime = Mathf.Lerp(snowStartRate, 0f, t);
            }
            if (fogParticle != null) {
                var e = fogParticle.emission;
                e.rateOverTime = Mathf.Lerp(fogStartRate, 0f, t);
            }

            yield return null;
        }

        if (snowParticle != null) snowParticle.gameObject.SetActive(false);
        if (fogParticle  != null) fogParticle.gameObject.SetActive(false);
        Wintergroup.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // Fade rain particle emission rate
    // ─────────────────────────────────────────────
    IEnumerator FadeRain(float targetEmission) {
        ParticleSystem rainPS = Rain.GetComponent<ParticleSystem>();
        if (rainPS == null) yield break;

        var emission = rainPS.emission;
        float startRate = emission.rateOverTime.constant;
        float targetRate = startRate * targetEmission;
        float elapsed = 0f;
        float duration = 3f;

        float startVolume = rainAudio != null ? rainAudio.volume : 0f;
        float targetVolume = startVolume * targetEmission;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            emission.rateOverTime = Mathf.Lerp(startRate, targetRate, t);

            if (rainAudio != null)
                rainAudio.volume = Mathf.Lerp(startVolume, targetVolume, t);

            yield return null;
        }

        emission.rateOverTime = targetRate;
    }

    // ─────────────────────────────────────────────
    // Fade out rain completely then disable
    // ─────────────────────────────────────────────
    IEnumerator FadeOutRain() {
        ParticleSystem rainPS = Rain.GetComponent<ParticleSystem>();

        if (rainPS == null) {
            Rain.SetActive(false);
            yield break;
        }

        var emission = rainPS.emission;
        float startRate = emission.rateOverTime.constant;
        float startVolume = rainAudio != null ? rainAudio.volume : 0f;
        float elapsed = 0f;
        float duration = 3f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            emission.rateOverTime = Mathf.Lerp(startRate, 0f, t);

            if (rainAudio != null)
                rainAudio.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        Rain.SetActive(false);
        emission.rateOverTime = startRate;

        if (rainAudio != null)
            rainAudio.volume = startVolume;
    }

    // ─────────────────────────────────────────────
    // CLEAR — fog OFF, rain fades out, sun appears
    // ─────────────────────────────────────────────
    IEnumerator SetClear() {
        SpringGroup.SetActive(false);
        WindFar_Left.SetActive(false);
        WindClose.SetActive(false);
        WindZone.SetActive(true);

        SetFog(false, new Color(0.85f, 0.88f, 0.92f), 0.004f);

        // Fade out rain slowly
        yield return StartCoroutine(FadeOutRain());

        yield return StartCoroutine(TransitionLighting(
            fromLightColor:   DirectionalLight.color,
            toLightColor:     new Color(1f, 0.95f, 0.85f),
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
        SetTerrainDetails(0f);
    }

    // ─────────────────────────────────────────────
    // SPRING — fog OFF, warm golden light, grass grows
    // ─────────────────────────────────────────────
    IEnumerator SetSpring() {
        SpringGroup.SetActive(true);
        WindFar_Left.SetActive(false);
        WindClose.SetActive(false);
        WindZone.SetActive(true);

        SetFog(false, new Color(0.94f, 0.91f, 0.86f), 0.003f);

        // Fade out rain + wind, fade in spring birds + water simultaneously
        StartCoroutine(FadeOutRain());
        StartCoroutine(FadeWindAudio(GetWindVolume(), 0f, 3f));
        StartCoroutine(FadeInSpring());
        StartCoroutine(FadeInWater());

        // Set spring skybox
        if (springSkybox != null) {
            RenderSettings.skybox = springSkybox;
            DynamicGI.UpdateEnvironment();
        }

        // Fade water to warm spring color
        if (waterRenderer != null) {
            Material mat = waterRenderer.material;
            mat.SetFloat("_Smoothness", 0.8f);

            StartCoroutine(FadeWaterColor(
                new Color(0.2f, 0.4f, 0.6f),
                new Color(0.3f, 0.55f, 0.75f),
                transitionDuration
            ));
        }

        Coroutine lightingCo = StartCoroutine(TransitionLighting(
            fromLightColor:   DirectionalLight.color,
            toLightColor:     new Color(1f, 0.95f, 0.85f),
            fromIntensity:    DirectionalLight.intensity,
            toIntensity:      4f,
            fromAmbient:      RenderSettings.ambientLight,
            toAmbient:        new Color(0.85f, 0.9f, 1f),
            fromVolumeFilter: colorAdjustments != null
                                ? colorAdjustments.colorFilter.value
                                : Color.white,
            toVolumeFilter:   new Color(1f, 0.97f, 0.92f),
            duration:         transitionDuration
        ));

        yield return StartCoroutine(TransitionTerrain(SNOW, GRASS, transitionDuration));
        yield return StartCoroutine(FadeTerrainDetails(0f, grassDensity, transitionDuration));
        yield return lightingCo;

        Wintergroup.SetActive(false);

        // Start spring timer instead of permanent lock
        StartCoroutine(SpringTimer());
        Debug.Log("Spring started! Will reset in " + springDuration + " seconds.");
    }

    // ─────────────────────────────────────────────
    // Fade water color from cold to warm
    // ─────────────────────────────────────────────
    IEnumerator FadeWaterColor(Color from, Color to, float duration) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            waterRenderer.material.color = Color.Lerp(from, to, t);
            yield return null;
        }
        waterRenderer.material.color = to;
    }

    // ─────────────────────────────────────────────
    // Fade in spring birds audio
    // ─────────────────────────────────────────────
    IEnumerator FadeInSpring() {
        if (springAudio == null) yield break;

        springAudio.volume = 0f;
        springAudio.Play();

        yield return StartCoroutine(FadeAudio(springAudio, 0f, 1f, 5f));
    }

    // ─────────────────────────────────────────────
    // Fade in water audio in spring
    // ─────────────────────────────────────────────
    IEnumerator FadeInWater() {
        if (waterAudio == null) yield break;

        waterAudio.volume = 0f;
        waterAudio.Play();

        yield return StartCoroutine(FadeAudio(waterAudio, 0f, 1f, 5f));
    }

    // ─────────────────────────────────────────────
    // Set all wind audio volumes instantly
    // ─────────────────────────────────────────────
    void SetWindVolume(float volume) {
        if (windAudios == null) return;
        foreach (AudioSource wind in windAudios)
            if (wind != null)
                wind.volume = volume;
    }

    // ─────────────────────────────────────────────
    // Get current wind volume (from first source)
    // ─────────────────────────────────────────────
    float GetWindVolume() {
        if (windAudios == null || windAudios.Length == 0) return 0f;
        return windAudios[0] != null ? windAudios[0].volume : 0f;
    }

    // ─────────────────────────────────────────────
    // Fade all wind audio sources simultaneously
    // ─────────────────────────────────────────────
    IEnumerator FadeWindAudio(float fromVolume, float toVolume, float duration) {
        if (windAudios == null || windAudios.Length == 0) yield break;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            foreach (AudioSource wind in windAudios)
                if (wind != null)
                    wind.volume = Mathf.Lerp(fromVolume, toVolume, t);
            yield return null;
        }

        foreach (AudioSource wind in windAudios)
            if (wind != null)
                wind.volume = toVolume;
    }

    // ─────────────────────────────────────────────
    // Universal audio fade helper
    // ─────────────────────────────────────────────
    IEnumerator FadeAudio(AudioSource audio, float fromVolume, float toVolume, float duration) {
        if (audio == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            audio.volume = Mathf.Lerp(fromVolume, toVolume, t);
            yield return null;
        }

        audio.volume = toVolume;
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
    // Instantly set terrain detail density
    // ─────────────────────────────────────────────
    void SetTerrainDetails(float density) {
        if (terrain == null) return;
        terrain.detailObjectDensity = density;
        terrain.Flush();
    }

    // ─────────────────────────────────────────────
    // Smoothly fade terrain detail density
    // ─────────────────────────────────────────────
    IEnumerator FadeTerrainDetails(float from, float to, float duration) {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            terrain.detailObjectDensity = Mathf.Lerp(from, to, t);
            terrain.Flush();
            yield return null;
        }

        terrain.detailObjectDensity = to;
        terrain.Flush();
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
    // Smoothly blend terrain with Perlin Noise pattern
    // ─────────────────────────────────────────────
    IEnumerator TransitionTerrain(int fromLayer, int toLayer, float duration) {
        float elapsed = 0f;
        TerrainData td = terrain.terrainData;
        int w = td.alphamapWidth;
        int h = td.alphamapHeight;

        float[,] randomOffset = new float[h, w];
        float noiseScale = 0.008f;
        float randomSeedX = Random.Range(0f, 100f);
        float randomSeedY = Random.Range(0f, 100f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                randomOffset[y, x] = (Mathf.PerlinNoise(
                    x * noiseScale + randomSeedX,
                    y * noiseScale + randomSeedY) - 0.5f) * 1.2f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float baseBlend = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float[,,] maps = td.GetAlphamaps(0, 0, w, h);

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    float blend = Mathf.Clamp01(baseBlend + randomOffset[y, x]);

                    for (int i = 0; i < maps.GetLength(2); i++)
                        maps[y, x, i] = 0f;

                    maps[y, x, fromLayer] = 1f - blend;
                    maps[y, x, toLayer]   = blend;
                }
            }

            td.SetAlphamaps(0, 0, maps);
            yield return null;
        }

        SetTerrainLayer(toLayer);
    }
}