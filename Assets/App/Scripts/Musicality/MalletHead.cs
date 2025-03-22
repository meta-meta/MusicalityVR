using System;
using System.Collections;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Musicality
{
    [RequireComponent(typeof(VelocityProbe))]
    public class MalletHead : MonoBehaviour, IImplement
    {
        [SerializeField] private Holdable _holdable;
        public IEnvelopeFloat GainEnvelope;
        public float GainEnvelopeVal => GainEnvelope?.GetEnvelopeFloat() ?? 1;
        public bool IsHoldable => _holdable;
        public Handedness Handedness => _holdable.Handedness;
        public Transform Handle => _holdable.transform;
        public static Action<MalletHead> OnMalletDestroy;
    
        public OVRInput.Controller Controller =>
            _holdable.Handedness.IsLeft() ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

        public Vector3 velocityVec => GetComponent<VelocityProbe>().velocityVec;

        public bool ignoreVelocityProbe;


        private IEnumerator _vibeOffCoroutine;

        public void Vibe(float strength, bool isImpulse = true)
        {
            if (!_holdable) return;

            if (_vibeOffCoroutine != null)
            {
                StopCoroutine(_vibeOffCoroutine);
                _vibeOffCoroutine = null;
            }
        
            var controller = Controller;
            if (!_holdable.IsHeldOrManipulatedByController)
            {
                OVRInput.SetControllerVibration(0, 0, Controller);
                return;
            }

            OVRInput.SetControllerVibration(1f, strength, controller);

            if (isImpulse)
            {
                _vibeOffCoroutine = VibeOff(0.05f, controller);
                StartCoroutine(_vibeOffCoroutine);
            }
        }

        private IEnumerator VibeOff(float timeout, OVRInput.Controller controller)
        {
            while (true)
            {
                yield return null;
                timeout -= Time.deltaTime;
                if (timeout <= 0f)
                {
                    OVRInput.SetControllerVibration(0, 0, controller);
                    break;
                }
            }
        }

        public void VibeOff()
        {
            if (!_holdable || !_holdable.IsHeldOrManipulatedByController) return;
            OVRInput.SetControllerVibration(0, 0, Controller);
        }

        private void OnDestroy()
        {
            OnMalletDestroy?.Invoke(this);
        }
    }
}