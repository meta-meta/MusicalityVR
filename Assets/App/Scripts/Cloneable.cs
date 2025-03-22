using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Musicality
{
    public abstract class Cloneable<T> : MonoBehaviour
    {
        [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator;
        private Handedness _handedness;
        private bool _isBeingManipulated;
        private bool _isController;
        private bool _isFocused;
        public Action<IReactComponent<T>> onClone;
        public Action<float, float> onThumbstickAngleMagnitude;

        protected abstract IReactComponent<T> reactComponent { get; }

        
        private void OnEnable()
        {
            objectManipulator.OnManipulationStarted.AddListener(OnManipulationStart);
            objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnd);
        }

        private void OnDisable()
        {
            objectManipulator.OnManipulationStarted.RemoveListener(OnManipulationStart);
            objectManipulator.OnManipulationEnded.RemoveListener(OnManipulationEnd);
        }

        private SerializableTransform _trAtManipulationStart;
        
        private void OnManipulationStart(ManipulationEventData eventData)
        {
            _isBeingManipulated = true;
            _trAtManipulationStart = new SerializableTransform(transform);
            _handedness = eventData.Pointer.Controller.ControllerHandedness;
            _isController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
        }

        private void OnManipulationEnd(ManipulationEventData eventData)
        {
            _isBeingManipulated = false;
        }

        private void Update()
        {
            if (_isBeingManipulated)
            {
                var isThumbstickButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                    ? OVRInput.RawButton.LThumbstick
                    : OVRInput.RawButton.RThumbstick);
                
                var isDeleteButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                    ? OVRInput.RawButton.Y
                    : OVRInput.RawButton.B);

                if (isThumbstickButtonDown || Input.GetKeyDown(KeyCode.C))
                    Clone();
                
                if (isDeleteButtonDown || Input.GetKeyDown(KeyCode.Delete))
                    Delete();

                var thumbstickPos = OVRInput.Get(_handedness.IsLeft()
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