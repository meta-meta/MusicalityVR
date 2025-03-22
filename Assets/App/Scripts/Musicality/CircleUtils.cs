using UnityEngine;

namespace Musicality
{
    public static class CircleUtils
    {
        public static int Mod (int n, int mod) => ((n < 0 ? mod : 0) + n % mod) % mod;
        public static float Mod (float n, float mod) => ((n < 0 ? mod : 0) + n % mod) % mod;
        
        // public static T Mod<T> (T n, T mod) where T : int => ((n < 0 ? mod : 0) + n % mod) % mod;

        public static float DistOnCircle(float a, float b) => 180 - Mathf.Abs(Mathf.Abs(a - b) % 360 - 180);

    }
}