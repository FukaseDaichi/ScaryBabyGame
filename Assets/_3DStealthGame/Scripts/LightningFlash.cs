using System.Collections;
using UnityEngine;

// 雷演出: 通常は真っ暗、ランダム間隔で雷鳴と共にディレクショナルライトと
// アンビエントを数回フリッカーさせて一瞬だけ全体を見せる。
[RequireComponent(typeof(Light), typeof(AudioSource))]
public class LightningFlash : MonoBehaviour
{
    public float minInterval = 6f;
    public float maxInterval = 14f;
    public float flashIntensity = 2.5f;
    public Color ambientBase = new Color(0.010f, 0.013f, 0.022f);
    public Color ambientFlash = new Color(0.35f, 0.40f, 0.55f);

    Light lightningLight;
    AudioSource thunderSource;

    void Awake()
    {
        lightningLight = GetComponent<Light>();
        thunderSource = GetComponent<AudioSource>();
        lightningLight.intensity = 0f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientBase;
    }

    void Start()
    {
        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            thunderSource.pitch = Random.Range(0.85f, 1.1f);
            thunderSource.Play();
            int pulses = Random.Range(2, 4);
            for (int i = 0; i < pulses; i++)
            {
                float peak = flashIntensity * Random.Range(0.6f, 1f);
                yield return Flash(peak, Random.Range(0.04f, 0.10f), Random.Range(0.10f, 0.25f));
                yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
            }
        }
    }

    IEnumerator Flash(float peak, float riseTime, float fallTime)
    {
        for (float t = 0f; t < riseTime; t += Time.deltaTime)
        {
            SetFlash(Mathf.Lerp(0f, peak, t / riseTime));
            yield return null;
        }
        for (float t = 0f; t < fallTime; t += Time.deltaTime)
        {
            SetFlash(Mathf.Lerp(peak, 0f, t / fallTime));
            yield return null;
        }
        SetFlash(0f);
    }

    void SetFlash(float intensity)
    {
        lightningLight.intensity = intensity;
        RenderSettings.ambientLight = Color.Lerp(ambientBase, ambientFlash, intensity / flashIntensity);
    }
}
