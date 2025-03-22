using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Musicality
{
    public class Holdable : MonoBehaviour
    {
        [SerializeField] private ObjectManipulator _objectManipulator;
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
            _objectManipulator.firstHoverEntered.AddListener(OnFirstHoverEntered);
            _objectManipulator.lastHoverExited.AddListener(OnLastHoverExited);
            _objectManipulator.selectEntered.AddListener(OnSelectEntered);
            _objectManipulator.selectExited.AddListener(OnSelectExited);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            _isBeingManipulated = false;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            _isBeingManipulated = true;
            
            Handedness = args.interactorObject.handedness.ToHandedness();
            if (args.interactorObject?.transform.TryGetComponent<XRController>(out var controller) ?? false)
            {
                _isController = true;
            }
            else _isController = false;
        }

        private void OnDisable()
        {
            _objectManipulator.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
            _objectManipulator.lastHoverExited.RemoveListener(OnLastHoverExited);
            _objectManipulator.selectEntered.RemoveListener(OnSelectEntered);
            _objectManipulator.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnFirstHoverEntered(HoverEnterEventArgs args)
        {
            if (_isHeld || _isBeingManipulated) return;
            
            _isFocused = true;
            Handedness = args.interactorObject.handedness.ToHandedness();
            _tAtLastGrip = 0;
            
            if (args.interactorObject?.transform.TryGetComponent<XRController>(out var controller) ?? false)
            {
                _isController = true;
            }
            else _isController = false;
        }

        private void OnLastHoverExited(HoverExitEventArgs args)
        {
            _isFocused = false;
            if (!_isHeld) Handedness = Handedness.None;
        }

        private void Update()
        {
            var isGripDown = OVRInput.GetDown(Handedness == Handedness.Left
                ? OVRInput.RawButton.LHandTrigger
                : OVRInput.RawButton.RHandTrigger);
            
            if (_isHeld && isGripDown) ReleaseMe();
            
            if (_objectManipulator.isHovered && isGripDown)
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
            _solverHandler.TrackedHandedness = Handedness;
            _solverHandler.enabled = true;

            // TODO: custom settings for other objects
            _solverHandler.AdditionalOffset = new Vector3(0, -0.02f, 0.005f);
            _solverHandler.AdditionalRotation = new Vector3(-40f, Handedness == Handedness.Left ? 7f : -7f, 0);

            // TODO: figure this out. option to not have a "docked" position 
            // _solverHandler.AdditionalOffset = _solverHandler.transform.position - _solverHandler.TransformTarget.position;
            // _solverHandler.AdditionalRotation = (transform.rotation * Quaternion.Inverse(_solverHandler.TransformTarget.rotation)).eulerAngles;
        }
    }
}