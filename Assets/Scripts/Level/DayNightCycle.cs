using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDurationInSeconds = 120f;

    [Range(0f, 1f)]
    public float currentTimeOfDay = 0.3f;

    [Header("Sun & Moon")]
    public Light sunLight;
    public AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Gradient sunColorGradient;

    [Header("Environment Colors")]
    public Gradient ambientColorGradient;
    public Gradient fogColorGradient;

    [Header("Train Lights (Optional)")]
    public GameObject[] trainHeadlights;

    private void Update()
    {
        currentTimeOfDay += (Time.deltaTime / dayDurationInSeconds);
        if (currentTimeOfDay >= 1f)
        {
            currentTimeOfDay = 0f;
        }

        UpdateSunRotation();
        UpdateLighting();
        UpdateTrainLights();
    }

    private void UpdateSunRotation()
    {
        float sunAngle = (currentTimeOfDay * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0f);
    }

    private void UpdateLighting()
    {
        if (sunLight != null)
        {
            sunLight.intensity = lightIntensityCurve.Evaluate(currentTimeOfDay);
            sunLight.color = sunColorGradient.Evaluate(currentTimeOfDay);
        }

        RenderSettings.ambientLight = ambientColorGradient.Evaluate(currentTimeOfDay);

        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = fogColorGradient.Evaluate(currentTimeOfDay);
        }
    }

    private void UpdateTrainLights()
    {
        bool isNight = currentTimeOfDay < 0.2f || currentTimeOfDay > 0.8f;

        if (trainHeadlights != null)
        {
            foreach (GameObject lightObj in trainHeadlights)
            {
                if (lightObj != null && lightObj.activeSelf != isNight)
                {
                    lightObj.SetActive(isNight);
                }
            }
        }
    }
}