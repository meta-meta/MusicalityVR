using System;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Musicality
{
    public abstract class Cloneable<T> : MonoBehaviour
    {
        [SerializeField] private ObjectManipulator objectManipulator;
        private Handedness _handedness;
        private bool _isBeingManipulated;
        private bool _isFocused;
        public Action<IReactComponent<T>> onClone;
        public Action<float, float> onThumbstickAngleMagnitude;

        protected abstract IReactComponent<T> reactComponent { get; }

        
        private void OnEnable()
        {
            objectManipulator.selectEntered.AddListener(OnSelectEntered);
            objectManipulator.selectExited.AddListener(OnSelectExited);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            _isBeingManipulated = false;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            
            _isBeingManipulated = true;
            _trAtManipulationStart = new SerializableTransform(transform);
            _handedness = args.interactorObject.handedness.ToHandedness();
        }

        private void OnDisable()
        {
            objectManipulator.selectEntered.RemoveListener(OnSelectEntered);
            objectManipulator.selectExited.RemoveListener(OnSelectExited);
        }

        private SerializableTransform _trAtManipulationStart;
        
        // private void OnManipulationStart(ManipulationEventData eventData)
        // {
        //     _isBeingManipulated = true;
        //     _trAtManipulationStart = new SerializableTransform(transform);
        //     _handedness = eventData.Pointer.Controller.ControllerHandedness;
        //     _isController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
        // }
        //
        // private void OnManipulationEnd(ManipulationEventData eventData)
        // {
        //     _isBeingManipulated = false;
        // }

        private void Update()
        {
            if (_isBeingManipulated)
            {
                var isThumbstickButtonDown = OVRInput.GetDown(_handedness == Handedness.Left
                    ? OVRInput.RawButton.LThumbstick
                    : OVRInput.RawButton.RThumbstick);
                
                var isDeleteButtonDown = OVRInput.GetDown(_handedness == Handedness.Left
                    ? OVRInput.RawButton.Y
                    : OVRInput.RawButton.B);

                if (isThumbstickButtonDown || Input.GetKeyDown(KeyCode.C))
                    Clone();
                
                if (isDeleteButtonDown || Input.GetKeyDown(KeyCode.Delete))
                    Delete();

                var thumbstickPos = OVRInput.Get(_handedness == Handedness.Left
                    ? OVRInput.RawAxis2D.LThumbstick
                    : OVRInput.RawAxis2D.RThumbstick);
                
                // Touch Pro only
                // var touchpadPos = OVRInput.Get(_handedness.IsLeft()
                //     ? OVRInput.RawAxis2D.LTouchpad
                //     : OVRInput.RawAxis2D.RTouchpad);

                var thumbstickAngle = (450 - Mathf.Atan2(thumbstickPos.y, thumbstickPos.x) * Mathf.Rad2Deg) % 360;
                onThumbstickAngleMagnitude?.Invoke(thumbstickAngle, thumbstickPos.magnitude);
            }
        }

        private void Clone()
        {
            reactComponent.ComponentCollection.SendCmp(_trAtManipulationStart, reactComponent.CurrentState, reactComponent.ComponentId);
            onClone?.Invoke(reactComponent);
        }

        private void Delete()
        {
            reactComponent.ComponentCollection.SendCmpDelete(reactComponent.ComponentId);
        }
    }
}