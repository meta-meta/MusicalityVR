using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Musicality
{
    public class SunMovement : MonoBehaviour
    {
        private static readonly int SunSize = Shader.PropertyToID("_SunSize");
        private static readonly int SunSizeConvergence = Shader.PropertyToID("_SunSizeConvergence");
        private static readonly int AtmosphericThickness = Shader.PropertyToID("_AtmosphereThickness");
        private static readonly int SkyTint = Shader.PropertyToID("_SkyTint");
        private static readonly int GroundColor = Shader.PropertyToID("_GroundColor");
        private static readonly int Exposure = Shader.PropertyToID("_Exposure");
       
        [SerializeField] private FloatPicker atmosphere;
        [SerializeField] private FloatPicker convergence;
        [SerializeField] private FloatPicker exposure;
        [SerializeField] private FloatPicker groundHue;
        [SerializeField] private FloatPicker groundLum;
        [SerializeField] private FloatPicker groundSat;
        [SerializeField] private FloatPicker skyHue;
        [SerializeField] private FloatPicker skyLum;
        [SerializeField] private FloatPicker skySat;
        [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator manipulator;
        [SerializeField] private Tonnegg cycleRatio;
        [SerializeField] private Transform rotationContainer;
        [SerializeField] private float dayLengthMinutes;
        
        
        private bool IsManip => _manipHand.IsMatch(Handedness.Any);
        private Handedness _manipHand;
        private bool _useRatio;
        private NoteJI _cycleRatio = new NoteJI(1, 1);

        private void Awake()
        {
            manipulator.OnManipulationStarted.AddListener(OnManipulationStart);
            manipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
            cycleRatio.NoteChanged += CycleRatioOnNoteChanged;
            
            atmosphere.OnChange += f => RenderSettings.skybox.SetFloat(AtmosphericThickness, f);
            convergence.OnChange += f => RenderSettings.skybox.SetFloat(SunSizeConvergence, f);
            exposure.OnChange += f => RenderSettings.skybox.SetFloat(Exposure, f);
            // RenderSettings.skybox.SetFloat(SkyTint, Mathf.Pow(transform.localScale.x, 2) + 0.1f);
            // RenderSettings.skybox.SetFloat(GroundColor, Mathf.Pow(transform.localScale.x, 2) + 0.1f);
        }

        private void CycleRatioOnNoteChanged(int noteindex, INote note)
        {
            _useRatio = true;
            _cycleRatio = note as NoteJI? ?? default;
        }

        private void OnManipulationEnded(ManipulationEventData arg0)
        {
            _manipHand = Handedness.None;
        }

        private void OnManipulationStart(ManipulationEventData arg0)
        {
            _manipHand = arg0.Pointer.Controller.ControllerHandedness;
        }

        private const float QuarterNote = 360f / (24 * 4); // 1/4 rev per quarter note at 1:1

        private void Update()
        {
            if (_useRatio)
            {
                var degPerPulse = _cycleRatio.Freq(QuarterNote);
                rotationContainer.localRotation = Quaternion.Euler(-degPerPulse * MidiBeatClock.PulseCount, 0, 0);
            }
            else
            {
                var dayLengthSeconds = dayLengthMinutes * 60;
                var angle = (360 * Time.deltaTime) / (dayLengthSeconds);
                rotationContainer.Rotate(Vector3.left, angle);
            }
            

            if (IsManip)
            {
                var toggleButton = _manipHand.IsLeft()
                    ? OVRInput.RawButton.LHandTrigger
                    : OVRInput.RawButton.RHandTrigger;
                if (OVRInput.GetDown(toggleButton))
                {
                    CameraCache.Main.clearFlags = CameraCache.Main.clearFlags == CameraClearFlags.Skybox
                        ? CameraClearFlags.SolidColor
                        : CameraClearFlags.Skybox;
                    Debug.LogWarning("TODO: enable/disable passthrough?");
                }

                var sunScale = manipulator.HostTransform.localScale.x;
                Debug.Log(sunScale);
                RenderSettings.skybox.SetFloat(SunSize, Mathf.Clamp01(Mathf.Pow(sunScale - 0.01f, 2) * 10));
            }
        }
    }
}