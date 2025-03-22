using Microsoft.MixedReality.Toolkit;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        [SerializeField] private ObjectManipulator manipulator;
        [SerializeField] private Tonnegg cycleRatio;
        [SerializeField] private Transform rotationContainer;
        [SerializeField] private float dayLengthMinutes;
        
        
        private bool _useRatio;
        private NoteJI _cycleRatio = new NoteJI(1, 1);

        private void Awake()
        {
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
            

            if (manipulator.isSelected)
            {
                foreach (var interactor in manipulator.interactorsSelecting)
                {
                    var toggleButton = interactor.handedness == InteractorHandedness.Left
                        ? OVRInput.RawButton.LHandTrigger
                        : OVRInput.RawButton.RHandTrigger;
                    
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