using UnityEngine;
using System.Collections;

public class SeasonManager : MonoBehaviour
{
    [Header("Season Groups")]
    public GameObject winterGroup;
    public GameObject springGroup;
    public GameObject rain;

    [Header("Lighting")]
    public Light directionalLight;

    [Header("Terrain")]
    public Terrain terrain;

    [Header("Transition Speed")]
    public float transitionDuration = 8f; // Increase for slower transition

    [Header("Test")]
    public int testSeason = 0; // 0 = winter, 1 = rain, 2 = spring

    private int currentSeason = -1;

    // Terrain layer indices
    // 0 = missing
    // 1 = cliffs
    // 2 = snow
    // 3 = grass
    // 4 = melting snow

    void Start()
    {
        SetWinter();
    }

    void Update()
    {
        SetSeason(testSeason);
    }

    public void SetSeason(int season)
    {
        if (currentSeason == season) return;
        currentSeason = season;

        if (season == 0) SetWinter();
        else if (season == 1) StartCoroutine(SetRain());
        else if (season == 2) StartCoroutine(SetSpring());
    }

    void SetWinter()
    {
        // Stage 0: Winter
        winterGroup?.SetActive(true);
        springGroup?.SetActive(false);
        rain?.SetActive(false);

        if (directionalLight != null)
        {
            directionalLight.color = new Color(0.7f, 0.8f, 1f);
            directionalLight.intensity = 0.6f;
        }

        SetTerrainLayer(2); // snow
    }

    IEnumerator SetRain()
    {
        // Stage 1: Rain - snow melting
        winterGroup?.SetActive(true);
        springGroup?.SetActive(false);
        rain?.SetActive(true);

        if (directionalLight != null)
        {
            directionalLight.color = new Color(0.6f, 0.7f, 0.9f);
            directionalLight.intensity = 0.5f;
        }

        // Snow slowly transitions to melting snow
        yield return StartCoroutine(TransitionTerrain(2, 4, transitionDuration));
    }

    IEnumerator SetSpring()
    {
        // Stage 2: Spring
        winterGroup?.SetActive(false);
        springGroup?.SetActive(true);
        rain?.SetActive(false);

        if (directionalLight != null)
        {
            directionalLight.color = new Color(1f, 0.95f, 0.8f);
            directionalLight.intensity = 1.2f;
        }

        // Melting snow slowly transitions to grass
        yield return StartCoroutine(TransitionTerrain(4, 3, transitionDuration));
    }

    void SetTerrainLayer(int layerIndex)
    {
        if (terrain == null) return;

        float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0,
            terrain.terrainData.alphamapWidth,
            terrain.terrainData.alphamapHeight);

        for (int y = 0; y < maps.GetLength(0); y++)
        {
            for (int x = 0; x < maps.GetLength(1); x++)
            {
                for (int i = 0; i < maps.GetLength(2); i++)
                    maps[y, x, i] = 0f;

                maps[y, x, layerIndex] = 1f;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, maps);
    }

    IEnumerator TransitionTerrain(int fromLayer, int toLayer, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float blend = Mathf.SmoothStep(0f, 1f, elapsed / duration); // Smooth transition

            float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0,
                terrain.terrainData.alphamapWidth,
                terrain.terrainData.alphamapHeight);

            for (int y = 0; y < maps.GetLength(0); y++)
            {
                for (int x = 0; x < maps.GetLength(1); x++)
                {
                    for (int i = 0; i < maps.GetLength(2); i++)
                        maps[y, x, i] = 0f;

                    maps[y, x, fromLayer] = 1f - blend;
                    maps[y, x, toLayer] = blend;
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, maps);
            yield return null;
        }

        SetTerrainLayer(toLayer);
    }
}