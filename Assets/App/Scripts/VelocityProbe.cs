using UnityEngine;

public class VelocityProbe : MonoBehaviour
    {
        public Vector3 velocityVec;

        private Vector3 _currPos;
        private Vector3 _prevPos;
        
        private void FixedUpdate()
        {
            _currPos = transform.position;
            velocityVec = (_currPos - _prevPos) / Time.fixedDeltaTime;
            _prevPos = _currPos;
        }
    }