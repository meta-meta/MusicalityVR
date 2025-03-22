using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Musicality
{
    public class SpiralVolumePicker : SpiralControl // TODO: int midi pot
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private TMP_Text volumeText;
        protected override HashSet<float> CalculateAngles() => new HashSet<float>();
        public Action<float> OnVolumeChange;
        public float volume;

        protected override (float, float) OnAngleUpdate(float angleOnCircle, float angleOnSpiral)
        {
            // var volume = Mathf.Pow(2, angleOnSpiral / 360f);

            var newVolume = Mathf.Clamp(angleOnSpiral / 360f, 0, 1);

            // prevent jumping from 1 to 0 or 0 to 1
            // if (volume > 330 && newVolume < 30 || volume < 30 && newVolume > 330)
            // {
            //     newVolume = volume;
            // }

            var newAngle = newVolume * 360f;
            
            volume = newVolume;
            // Debug.Log($"{transform.parent.name} - volume {100 * volume}%");
            volumeText.text = $"{100 * volume}%";
            var color = Color.HSVToRGB(newAngle / 360, 1, 1);
            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = color;
            OnVolumeChange?.Invoke(volume);
            return (newAngle, newAngle);
        }

        protected override void Update()
        {
            base.Update();
            lineRenderer.SetPosition(0, transform.parent.position);
            lineRenderer.SetPosition(1, knobVisual.transform.position);
        }
    }
}