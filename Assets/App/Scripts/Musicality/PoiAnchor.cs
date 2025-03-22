using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Musicality
{
    public class PoiAnchor : MonoBehaviour
    {
        [SerializeField] private FloatPicker angularDrag;
        [SerializeField] private FloatPicker damper;
        [SerializeField] private FloatPicker drag;
        [SerializeField] private FloatPicker maxDistance;
        [SerializeField] private FloatPicker spring;
        [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator;
        [SerializeField] private Rigidbody twirlingRigidBody;
        [SerializeField] private SpringJoint springJoint;
        
        private Handedness _handedness;
        private bool _isBeingManipulated;
        private bool _isController;


        private void Awake()
        {
            twirlingRigidBody.angularDrag = angularDrag.Val;
            angularDrag.OnChange += f => twirlingRigidBody.angularDrag = f;
            
            twirlingRigidBody.drag = drag.Val;
            drag.OnChange += f => twirlingRigidBody.drag = f;    
            
            springJoint.maxDistance = maxDistance.Val;
            maxDistance.OnChange += f => springJoint.maxDistance = f;
            
            springJoint.spring = spring.Val;
            spring.OnChange += f => springJoint.spring = f;
           
            springJoint.damper = damper.Val;
            damper.OnChange += f => springJoint.damper = f;
        }

        private void Update()
        {
            if (_isBeingManipulated)
            {
                // var isThumbstickButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                //     ? OVRInput.RawButton.LThumbstick
                //     : OVRInput.RawButton.RThumbstick);
                
                var isGravityButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                    ? OVRInput.RawButton.Y
                    : OVRInput.RawButton.B);
                
                var isKinematicButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                    ? OVRInput.RawButton.A
                    : OVRInput.RawButton.X);

                // if (isThumbstickButtonDown || Input.GetKeyDown(KeyCode.C))
                //     Clone();

                if (isGravityButtonDown)
                {
                    twirlingRigidBody.useGravity = !twirlingRigidBody.useGravity;
                }
                
                if (isKinematicButtonDown)
                {
                    twirlingRigidBody.isKinematic = !twirlingRigidBody.isKinematic;
                }

                var thumbstickPos = OVRInput.Get(_handedness.IsLeft()
                    ? OVRInput.RawAxis2D.LThumbstick
                    : OVRInput.RawAxis2D.RThumbstick);
                
                // TODO: thumbstick for spring max len or damp or spring 
                
                // Touch Pro only
                // var touchpadPos = OVRInput.Get(_handedness.IsLeft()
                //     ? OVRInput.RawAxis2D.LTouchpad
                //     : OVRInput.RawAxis2D.RTouchpad);

                // var thumbstickAngle = (450 - Mathf.Atan2(thumbstickPos.y, thumbstickPos.x) * Mathf.Rad2Deg) % 360;
            }
        }
        
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
        
        private void OnManipulationStart(ManipulationEventData eventData)
        {
            _isBeingManipulated = true;
            _handedness = eventData.Pointer.Controller.ControllerHandedness;
            _isController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
        }

        private void OnManipulationEnd(ManipulationEventData eventData)
        {
            _isBeingManipulated = false;
        }
    }
}