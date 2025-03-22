using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Musicality
{
    public class SpiralIrrationalIntervalPicker : SpiralControl
    {
        [SerializeField] private LineRenderer lineRenAmplitude;
        [SerializeField] private LineRenderer lineRenDecay;
        [SerializeField] private float precisionRadiusMultiplier = 2;
        protected override HashSet<float> CalculateAngles() => new HashSet<float>();
        public SpiralOrganSlider SliderAmplitude;
        public SpiralOrganSlider SliderDecay;
        public delegate void IntervalChangedDel(NoteIrrational note);
        public event IntervalChangedDel IntervalChanged;

        public float ClampAtMaxVal = Mathf.Infinity;

        protected override void Awake()
        {
            base.Awake();
            SliderAmplitude.lineRen = lineRenAmplitude;
            SliderDecay.lineRen = lineRenDecay;
        }

        private float _prevTruncatedFreq;
        protected override (float, float) OnAngleUpdate(float angleOnCircle, float angleOnSpiral)
        {
            // truncate to integer when near knob radius, more precision when pulling outward
            var knobToPivot = knobManipulator.transform.localPosition - pivot.localPosition;
            knobToPivot.y = 0;
            var radius = TrackRadius * precisionRadiusMultiplier;
            var decimalPlaces = Mathf.FloorToInt(Mathf.Max(0, (knobToPivot.magnitude - radius) / radius));
            var freq = Mathf.Pow(2, angleOnSpiral / 360f);
            var mult = Mathf.Pow(10, decimalPlaces);
            var truncatedFreq = Mathf.Floor(freq * mult) / mult;
            var newNote = new NoteIrrational(Mathf.Min(Mathf.Max(1, truncatedFreq), ClampAtMaxVal));

            if (Mathf.Abs(truncatedFreq - _prevTruncatedFreq) > .001f)
            {
                _prevTruncatedFreq = truncatedFreq;
                Vibe();
            }
            
            IntervalChanged?.Invoke(newNote);

            UpdateColors(newNote);
            var newAngleOnCircle = newNote.AngleOnCircle();
            var newAngleOnSpiral = newNote.AngleOnSpiral();
            return (newAngleOnCircle, newAngleOnSpiral);
        }

        public void UpdateColors(NoteIrrational interval)
        {
            var angleOnCircle = interval.AngleOnCircle();

            var colorAmp = Color.HSVToRGB(angleOnCircle / 360, 1, 1);
            lineRenAmplitude.startColor = SliderAmplitude.gameObject.activeSelf ? colorAmp : Color.clear;
            lineRenAmplitude.endColor = colorAmp;
            
            var colorDecay = Color.HSVToRGB(((angleOnCircle + 200) / 360) % 360, 0.7f, 0.5f);
            lineRenDecay.startColor = SliderDecay.gameObject.activeSelf ? colorDecay : Color.clear;
            lineRenDecay.endColor = colorAmp;
        }

        protected override void Update()
        {
            base.Update();

            lineRenAmplitude.enabled = SliderAmplitude.gameObject.activeSelf;
            // if (SliderAmplitude.gameObject.activeSelf)
            // {
            //     lineRenAmplitude.SetPosition(0, SliderAmplitude.transform.position);
            //     lineRenAmplitude.SetPosition(1, knobVisual.transform.position);
            // }

            lineRenDecay.enabled = SliderDecay.gameObject.activeSelf;
            // if (SliderDecay.gameObject.activeSelf)
            // {
            //     lineRenDecay.SetPosition(0, SliderDecay.transform.position);
            //     lineRenDecay.SetPosition(1, SliderAmplitude.transform.position);
            // }
            
            SliderDecay.minZ = Mathf.Lerp(SliderAmplitude.minZ, SliderAmplitude.maxZ, SliderAmplitude.Val) + SliderAmplitude.transform.localScale.z;
            SliderDecay.maxZ = SliderDecay.minZ + (SliderAmplitude.maxZ - SliderAmplitude.minZ);
        }
    }
}