using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Musicality
{
    public abstract class SpiralControl : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenSpiralKnobToGrabbable;
        [SerializeField] private bool isCircleForced;
        [SerializeField] private float heightPerRevolution = 0.25f;
        [SerializeField] protected GameObject knobVisual;
        [SerializeField] protected Microsoft.MixedReality.Toolkit.UI.ObjectManipulator knobManipulator;
        private HashSet<float> _anglesDeg;
        private Material _material;
        private bool IsCircle => isCircleForced || _anglesDeg.Any() && _anglesDeg.All(a => a <= 360f);
        private bool _areRefsSetup;
        private bool _isManipulatingKnob;
        private float PivotHeight(float angleDeg) => heightPerRevolution * (angleDeg / 360);
        private float _angle;
        private static readonly int MatRimColor = Shader.PropertyToID("_RimColor");
        private static readonly int MatRimPower = Shader.PropertyToID("_RimPower");
        protected float TrackRadius => knobVisual.transform.localPosition.magnitude;
        public Transform pivot;
        public bool isIrrationalSpiralTrack;
        

        public void SetRimPower(float power)
        {
            _material.SetFloat(MatRimPower, 8 - (power * 8));
        }

        private void SetRimColor(float angleOnCircle)
        {
            SetupRefsIfNeeded();
            Color.RGBToHSV(_material.GetColor(MatRimColor), out _, out var sat, out var val);
            _material.SetColor(MatRimColor, Color.HSVToRGB(angleOnCircle / 360, sat, val));
        }

        private void SetupRefsIfNeeded()
        {
            if (_areRefsSetup) return;
            _material = knobVisual.GetComponent<MeshRenderer>().sharedMaterial;
            _areRefsSetup = true;
        }

        // Start is called before the first frame update
        protected virtual void Awake()
        {
            SetupRefsIfNeeded();
        }
    
        private void OnEnable()
        {
            RefreshData();
            knobManipulator.OnManipulationEnded.AddListener(OnKnobManipulationEnded);
            knobManipulator.OnManipulationStarted.AddListener(OnKnobManipulationStart);
        }

        private void OnValidate()
        {
            // if (Application.isPlaying)
            //     RefreshData();
        }

        private void RefreshData()
        {
            _anglesDeg = CalculateAngles();
        
            // Debug.Log($"_angles {_angles}");
        
            if (IsCircle)
            {
                heightPerRevolution = 0;
            }

            UpdateAngleFromManipulation();

            MkSpiralTrack();
        }

        public void ForceRefreshData() => RefreshData();


        private Transform _spiralTrack;
        
        private void MkSpiralTrack()
        {
            if (_spiralTrack == null)
            {
                var gObjName = $"SpiralTrack--{name}";
                var gObj = transform.Find(gObjName)?.gameObject;
                gObj ??= Instantiate(Resources.Load<GameObject>("Prefabs/Line"), transform);
                gObj.name = gObjName;
                Debug.Log($"Creating {gObj.name}");
                _spiralTrack = gObj.transform;
                _spiralTrack.SetParent(transform);
            }

            var lineRen = _spiralTrack.GetComponent<LineRenderer>();
            
            foreach (var angleDeg in _anglesDeg)
            {
                // make notch for angle
            }

            var radius = TrackRadius;

            var ticks = 32;
            var degPerTick = 360f / ticks;
            var anglesOrdered = isCircleForced
                ? Enumerable.Range(0, ticks).Select(n => n * degPerTick)
                : isIrrationalSpiralTrack
                    ? Enumerable.Range(0, ticks * 8).Select(n => n * degPerTick)
                    : _anglesDeg
                        .OrderBy(a => a);

            var positions = anglesOrdered
                .Select(a => new Vector3(
                    Mathf.Sin(a * Mathf.Deg2Rad) * radius,
                    PivotHeight(a),
                    Mathf.Cos(a * Mathf.Deg2Rad) * radius))
                .ToArray();

            if (isIrrationalSpiralTrack)
            {
                Debug.Log($"IrrationalSpiralTrack {positions.Length}");
                foreach (var position in positions)
                {
                    Debug.Log(position);
                }
            }

            lineRen.loop = IsCircle;

            lineRen.positionCount = positions.Length;
            // Debug.Log(_anglesDeg.Select(a => a.ToString()).Aggregate<string>((a, b) => $"{a}, {b}"));
            // var colors = 
            lineRen.SetPositions(positions);
        }

        public void SetAngleFromNote(INote note) => SetAngle(note.AngleOnCircle(), note.AngleOnSpiral());

        protected void SetAngles(HashSet<float> angles)
        {
            _anglesDeg = angles;
        }

        protected void SetAngle(float angleOnCircle, float angleOnSpiral)
        {
            SetRimColor(angleOnCircle);
            var color = Color.HSVToRGB(angleOnCircle / 360, 1, 1);
            lineRenSpiralKnobToGrabbable.startColor = Color.clear;
            lineRenSpiralKnobToGrabbable.endColor = color;

            _angle = angleOnSpiral;
            pivot.localRotation = Quaternion.Euler(0, angleOnCircle, 0);
            UpdatePivotHeight();
        }

        protected abstract HashSet<float> CalculateAngles();

        private void OnDisable()
        {
            knobManipulator.OnManipulationEnded.RemoveListener(OnKnobManipulationEnded);
            knobManipulator.OnManipulationStarted.RemoveListener(OnKnobManipulationStart);
        }

        private Handedness _handedness;

        public void Vibe()
        {
            var controller = _handedness.IsLeft() ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            OVRInput.SetControllerVibration(1f, .1f, controller);
            CancelInvoke(nameof(VibeOff));
            Invoke(nameof(VibeOff), .05f);
        }

        private void VibeOff()
        {
            var controller = _handedness.IsLeft() ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            OVRInput.SetControllerVibration(0, 0, controller);
        }

        void OnKnobManipulationStart(ManipulationEventData evtData)
        {
            _isManipulatingKnob = true;
            _handedness = evtData.Pointer.Controller.ControllerHandedness;
            knobManipulator.GetComponent<MeshRenderer>().enabled = true;
            lineRenSpiralKnobToGrabbable.enabled = true;
        }

        void OnKnobManipulationEnded(ManipulationEventData evtData)
        {
            _isManipulatingKnob = false;
            knobManipulator.GetComponent<MeshRenderer>().enabled = false;
            lineRenSpiralKnobToGrabbable.enabled = false;
        }

        public static float NearestAngleOnCircle(IEnumerable<float> anglesDeg, float angleOnCircle) => anglesDeg
            .OrderBy(a => CircleUtils.DistOnCircle(a, angleOnCircle))// TODO: DistOnCircle check logic
            .DefaultIfEmpty(angleOnCircle)
            .First();
        
        void UpdateAngleFromManipulation()
        {
            // TODO: distance % 360   // TODO: DistOnCircle or DistOnSpiral  // TODO Make this more efficient
            // var nearestAngle = _angles
            //     .OrderBy(a => DistOnCircle(a, angle))
            //     .DefaultIfEmpty(angle)
            //     .First();

            var angleOnCircle = CircleUtils.Mod(_angle, 360);
            var nearestAngleOnCircle = NearestAngleOnCircle(_anglesDeg, angleOnCircle);
        
// angle: -357.618 rotationCount: 0  nearestAngleOnCircle: 0 nearestAngleOnSpiral 0  SO CLOSE
            var rotationCount = _angle >= 0
                ? Mathf.FloorToInt(_angle / 360)
                : Mathf.FloorToInt(Mathf.Abs(_angle) / 360) * -1;
            // var nearestAngleOnSpiral = (angle < 0 ? nearestAngleOnCircle - 360 : nearestAngleOnCircle) + rotationCount * 360;
            var nearestAngleOnSpiral = nearestAngleOnCircle == 0
                ? Mathf.RoundToInt(_angle / 360) * 360
                : (_angle < 0 ? nearestAngleOnCircle - 360 : nearestAngleOnCircle) + rotationCount * 360;
                // : Mathf.FloorToInt(angle / 360) * nearestAngleOnCircle; // maybe?
        
            // Debug.Log($"[SpiralControl] angle: {angle} rotationCount: {rotationCount}  nearestAngleOnCircle: {nearestAngleOnCircle} nearestAngleOnSpiral {nearestAngleOnSpiral}");
            // Debug.Log($"[SpiralControl]a: {angle} aoc: {angleOnCircle}  aos: {angleOnSpiral}   Nearest angle on circle: {nearestAngleOnCircle} nearestAngleOnSpiral {nearestAngleOnSpiral}");


            var (newAngleOnCircle, newAngleOnSpiral) = OnAngleUpdate(nearestAngleOnCircle, nearestAngleOnSpiral);
            if (float.IsNaN(newAngleOnCircle) || float.IsNaN(newAngleOnSpiral))
            {
                Debug.LogError($"NaN -- newAngleOnCircle {newAngleOnCircle}  newAngleOnSpiral {newAngleOnSpiral}");
            }
            else SetAngle(newAngleOnCircle, newAngleOnSpiral);
        }

        private void UpdatePivotHeight()
        {
            pivot.localPosition = new Vector3(0, PivotHeight(_angle), 0);
        }


        protected abstract (float, float) OnAngleUpdate(float angleOnCircle, float angleOnSpiral);


        // Update is called once per frame
        protected virtual void Update()
        {
            if (!_isManipulatingKnob) return;
       
            // align y axis with knobManipulator
            var lookPos = knobManipulator.transform.localPosition - pivot.localPosition;
            lookPos.y = 0;
            pivot.localRotation = Quaternion.LookRotation(lookPos);

            lineRenSpiralKnobToGrabbable.SetPosition(0, knobManipulator.transform.position);
            lineRenSpiralKnobToGrabbable.SetPosition(1, knobVisual.transform.position);

            // angle accumulates total rotation from diff in rotation per update
            var prevAngleOnCircle = CircleUtils.Mod(_angle, 360);
            var currAngleOfPivot = pivot.localEulerAngles.y;
            
            
            // FIXME. broken for <3 EDO and probably an indicator that this technique is fundamentally broken
            var diff = prevAngleOnCircle > 270 && currAngleOfPivot < 90
                ? currAngleOfPivot + (360 - prevAngleOnCircle)
                : prevAngleOnCircle < 90 && currAngleOfPivot > 270
                    ? 0 - (360 - currAngleOfPivot) - prevAngleOnCircle
                    : (currAngleOfPivot - prevAngleOnCircle);
            // Debug.Log($"diff {diff} prevCirc {prevAngleOnCircle} currPiv {currAngleOfPivot}  ");
            _angle += diff;

            // Debug.Log($"currAngle Y {currAngle}   angle {angle}");

            
            // try adding dummy pivot for getting Value (angle) then a separate display 
            // if (diff > 10) Value += diff; 

            UpdatePivotHeight();

            UpdateAngleFromManipulation();
        }
        
        
        
    }
}