using UnityEngine;

namespace Musicality
{
    public class FloatUtils
    {
        public static float Scale(float inMin, float inMax, float outMin, float outMax, float val) =>
            Mathf.Lerp(
                outMin,
                outMax,
                Mathf.InverseLerp(inMin, inMax, val));
    }
}