using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Musicality
{
    public class SpiralOrganSlider : MonoBehaviour
    {
        private ObjectManipulator _knobManipulator;
        private bool _isManipulatingKnob;
        public float Val { get; private set; }
        public delegate void ValueChangedDel(float val);
        public event ValueChangedDel ValueChanged;
        public float initialVal;
        public float maxZ = 0.15f;
        public float minZ = 0.0625f;
        public LineRenderer lineRen;
        
        private void OnEnable()
        {
            _knobManipulator = GetComponent<ObjectManipulator>();
        }

        private void Start()
        {
            // SetPosition(Mathf.Lerp(minZ, maxZ, initialVal));
        }

        public void SetValue(float v)
        {
            Val = v;
            ValueChanged?.Invoke(Val);
        }
        
        private void SetPosition(float z)
        {
            transform.localPosition = Vector3.forward * Mathf.Clamp(z, minZ, maxZ);
            SetValue(Mathf.InverseLerp(minZ, maxZ, z));
        }

        private void Update()
        {
            if (_knobManipulator.isSelected) SetPosition(transform.localPosition.z);
            else transform.localPosition = Vector3.forward * Mathf.Lerp(minZ, maxZ, Val);

            lineRen.SetPosition(0, transform.parent.TransformPoint(Vector3.forward * Mathf.Lerp(minZ, maxZ, Val)));
            lineRen.SetPosition(1, transform.parent.TransformPoint(Vector3.forward * (minZ - transform.localScale.z)));
        }
    }
}