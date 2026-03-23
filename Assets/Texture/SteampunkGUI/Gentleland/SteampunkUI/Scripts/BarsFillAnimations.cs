using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gentleland.StemapunkUI.DemoAndExample
{
    public class BarsFillAnimations : MonoBehaviour
    {
        private Slider[] sliders;
        private float[] shifts;
        private float[] fillTimeInSeconds;
        private float[] durations = { 0.75f, 1.0f, 1.2f, 1.5f, 2.0f, 3.0f };

        private void Start()
        {
            sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            shifts = new float[sliders.Length];
            fillTimeInSeconds = new float[sliders.Length];

            for (int i = 0; i < sliders.Length; i++)
            {
                int index = Random.Range(0, durations.Length); // index int đúng chuẩn
                fillTimeInSeconds[i] = durations[index];
                shifts[i] = Random.Range(0f, fillTimeInSeconds[i]);
            }
        }

        private void Update()
        {
            if (sliders == null || sliders.Length == 0)
                return;

            for (int i = 0; i < sliders.Length; i++)
            {
                float t = (shifts[i] + Time.realtimeSinceStartup) % (fillTimeInSeconds[i] * 2);
                float fill;

                if (t > fillTimeInSeconds[i])
                {
                    fill = 1.0f - (t - fillTimeInSeconds[i]) / fillTimeInSeconds[i];
                }
                else
                {
                    fill = t / fillTimeInSeconds[i];
                }

                sliders[i].value = fill;
            }
        }
    }
}
