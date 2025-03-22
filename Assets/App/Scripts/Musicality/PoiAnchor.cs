using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Musicality
{
    public class PoiAnchor : MonoBehaviour
    {
        [SerializeField] private FloatPicker angularDrag;
        [SerializeField] private FloatPicker damper;
        [SerializeField] private FloatPicker drag;
        [SerializeField] private FloatPicker maxDistance;
        [SerializeField] private FloatPicker spring;
        [SerializeField] private ObjectManipulator objectManipulator;
        [SerializeField] private Rigidbody twirlingRigidBody;
        [SerializeField] private SpringJoint springJoint;
        
        private void Awake()
        {
            twirlingRigidBody.angularDamping = angularDrag.Val;
            angularDrag.OnChange += f => twirlingRigidBody.angularDamping = f;
            
            twirlingRigidBody.linearDamping = drag.Val;
            drag.OnChange += f => twirlingRigidBody.linearDamping = f;    
            
            springJoint.maxDistance = maxDistance.Val;
            maxDistance.OnChange += f => springJoint.maxDistance = f;
            
            springJoint.spring = spring.Val;
            spring.OnChange += f => springJoint.spring = f;
           
            springJoint.damper = damper.Val;
            damper.OnChange += f => springJoint.damper = f;
        }

        private void Update()
        {
            if (objectManipulator.isSelected)
            {
                foreach (var interactor in objectManipulator.interactorsSelecting)
                {
                    // var isThumbstickButtonDown = OVRInput.GetDown(_handedness.IsLeft()
                    //     ? OVRInput.RawButton.LThumbstick
                    //     : OVRInput.RawButton.RThumbstick);

                    var isLeft = interactor.handedness == InteractorHandedness.Left;
                
                    var isGravityButtonDown = OVRInput.GetDown(isLeft
                        ? OVRInput.RawButton.Y
                        : OVRInput.RawButton.B);
                
                    var isKinematicButtonDown = OVRInput.GetDown(isLeft
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

                    var thumbstickPos = OVRInput.Get(isLeft
                        ? OVRInput.RawAxis2D.LThumbstick
                        : OVRInput.RawAxis2D.RThumbstick);
                
                    // TODO: thumbstick for spring max len or damp or spring 
                
                    // Touch Pro only
                    // var touchpadPos = OVRInput.Get(isLeft
                    //     ? OVRInput.RawAxis2D.LTouchpad
                    //     : OVRInput.RawAxis2D.RTouchpad);

                    // var thumbstickAngle = (450 - Mathf.Atan2(thumbstickPos.y, thumbstickPos.x) * Mathf.Rad2Deg) % 360;
                }
               
            }
        }
    }
}