using UnityEngine;
using System.Collections;

public class LightFlickering : MonoBehaviour
{
    [SerializeField] private Light spotLight;
    [SerializeField] private Material neonMaterial;
    [SerializeField] private Color neonBaseColor = Color.white;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float minDelay = 0.5f;
    [SerializeField] private float maxDelay = 1f;

    private void OnEnable() => StartCoroutine(FlickerRoutine());

    private void OnDisable()
    {
        StopAllCoroutines();
        if (spotLight) spotLight.intensity = maxIntensity;
        neonMaterial?.SetColor("_EmissionColor", neonBaseColor);
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float t = Random.Range(minIntensity, maxIntensity);

            if (spotLight) spotLight.intensity = t;
            neonMaterial?.SetColor("_EmissionColor", neonBaseColor * t);

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }
}