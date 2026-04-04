using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    [Header("Winter Objects")]
    public GameObject snow;
    public GameObject fog;
    public GameObject chillyFog;

    [Header("Spring Objects")]
    public GameObject springGroup;
    public GameObject rain;
    public GameObject butterfly;

    [Header("Lighting")]
    public Light directionalLight;

    private int currentSeason = -1;

    void Start()
    {
        SetWinter();
    }

    public void SetSeason(int season)
    {
        if (currentSeason == season) return;
        currentSeason = season;

        if (season == 0) SetWinter();
        else SetSpring();
    }

    void SetWinter()
    {
        snow?.SetActive(true);
        fog?.SetActive(true);
        chillyFog?.SetActive(true);
        springGroup?.SetActive(false);
        rain?.SetActive(false);
        butterfly?.SetActive(false);

        if (directionalLight != null)
        {
            directionalLight.color = new Color(0.7f, 0.8f, 1f);
            directionalLight.intensity = 0.6f;
        }
    }

    void SetSpring()
    {
        snow?.SetActive(false);
        fog?.SetActive(false);
        chillyFog?.SetActive(false);
        springGroup?.SetActive(true);
        rain?.SetActive(true);
        butterfly?.SetActive(true);

        if (directionalLight != null)
        {
            directionalLight.color = new Color(1f, 0.95f, 0.8f);
            directionalLight.intensity = 1.2f;
        }
    }
}