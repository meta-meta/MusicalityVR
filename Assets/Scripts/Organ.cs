using System;
using UnityEngine;

namespace FaustUtilities_organ
{
    public class Organ : MonoBehaviour
    {
        [SerializeField] private FaustPlugin_organ organ;
        [SerializeField] private float decayS = 1;
        [SerializeField] private float velMul = 0.5f;
        [SerializeField] private float fMul = 10;

        private float vel;
        private float tAtStrike;

        private float f = 100;
        private float fNext;

        private void FixedUpdate()
        {
            var t = (Time.time - tAtStrike) / decayS;
            organ.setParameter(1, Mathf.Lerp(vel * velMul, 0.01f, t));
            f = Mathf.Lerp(f, fNext, t);
            organ.setParameter(0, f);
        }

        private void OnCollisionEnter(Collision other)
        {
            vel = other.relativeVelocity.magnitude;
            fNext = f + (1 - transform.localPosition.y) * fMul;
            tAtStrike = Time.time;
        }
    }
}