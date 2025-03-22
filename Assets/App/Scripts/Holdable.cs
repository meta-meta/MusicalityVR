using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace Musicality
{
    public class Holdable : MonoBehaviour, IMixedRealityFocusHandler
    {
        [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator _objectManipulator;
        [SerializeField] private Orbital _orbital;
        [SerializeField] private SolverHandler _solverHandler;
        [SerializeField] private float doubleGripSeconds = 0.4f;
        public Handedness Handedness { get; private set; }
        public bool IsHeldOrManipulatedByController => _isController && (_isBeingManipulated || _isHeld);

        private bool _isController;
        private bool _isFocused;
        private bool _isBeingManipulated;
        private bool _isHeld;
        private float _tAtLastGrip;

        private void OnEnable()
        {
            _objectManipulator.OnManipulationStarted.AddListener(OnManipulationStart);
            _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnd);
        }

        private void OnDisable()
        {
            _objectManipulator.OnManipulationStarted.RemoveListener(OnManipulationStart);
            _objectManipulator.OnManipulationEnded.RemoveListener(OnManipulationEnd);
        }

        private void OnManipulationStart(ManipulationEventData eventData)
        {
            _isBeingManipulated = true;
            Handedness = eventData.Pointer.Controller.ControllerHandedness;
            _isController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
        }
        
        private void OnManipulationEnd(ManipulationEventData eventData)
        {
            _isBeingManipulated = false;
        }

        public void OnFocusEnter(FocusEventData eventData)
        {
            if (_isHeld || _isBeingManipulated) return;
            
            _isFocused = true;
            Handedness = eventData.Pointer.Controller.ControllerHandedness;
            _isController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
            _tAtLastGrip = 0;
            Debug.Log($"{name} focused with {Handedness} {eventData.Pointer.Controller.InputSource.SourceName}");
        }

        public void OnFocusExit(FocusEventData eventData)
        {
            _isFocused = false;
            if (!_isHeld) Handedness = Handedness.None;
            Debug.Log($"{name} lost focus with {eventData.Pointer.Controller.ControllerHandedness} {eventData.Pointer.Controller.InputSource.SourceName}");
        }

        private void Update()
        {
            var isGripDown = OVRInput.GetDown(Handedness.IsLeft()
                ? OVRInput.RawButton.LHandTrigger
                : OVRInput.RawButton.RHandTrigger);
            
            if (_isHeld && isGripDown) ReleaseMe();

            if (_isFocused && isGripDown)
            {
                var t = Time.time;
                Debug.Log($"hold {t} prev at {_tAtLastGrip}");
                if (t - _tAtLastGrip < doubleGripSeconds) HoldMe();
                _tAtLastGrip = t;
            }
        }

        private void ReleaseMe()
        {
            _isHeld = false;
            _objectManipulator.enabled = true;
            _orbital.enabled = false;
            _solverHandler.enabled = false;
            if (!_isFocused) Handedness = Handedness.None;
        }

        private void HoldMe()
        {
            Debug.Log($"Holding {name} with {Handedness}");
            _isFocused = false;
            _isHeld = true;
            _objectManipulator.enabled = false;
            _orbital.enabled = true;
            _solverHandler.TrackedHandness = Handedness;
            _solverHandler.enabled = true;

            // TODO: custom settings for other objects
            _solverHandler.AdditionalOffset = new Vector3(0, -0.02f, 0.005f);
            _solverHandler.AdditionalRotation = new Vector3(-40f, Handedness.IsLeft() ? 7f : -7f, 0);

            // TODO: figure this out. option to not have a "docked" position 
            // _solverHandler.AdditionalOffset = _solverHandler.transform.position - _solverHandler.TransformTarget.position;
            // _solverHandler.AdditionalRotation = (transform.rotation * Quaternion.Inverse(_solverHandler.TransformTarget.rotation)).eulerAngles;
        }
    }
}