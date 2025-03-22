using System;
using System.Collections.Generic;
using System.Text;

namespace Musicality
{
    // https://github.com/herrchmafi/Euclidean-Rhythm-Nodes/blob/master/Euclidean%20Rhythms/Assets/Scripts/EuclidNode.cs
    public class EuclideanRhythm
    {
        // https://rosettacode.org/wiki/Euclidean_rhythm#C#
        public static List<bool> BuildRhythm(int pulses, int beats)
        {
            var s = new List<List<bool>>();

            for (var i = 0; i < pulses; i++)
            {
                var innerList = new List<bool> { i < beats };
                s.Add(innerList);
            }

            var d = pulses - beats;
            pulses = Math.Max(beats, d);
            beats = Math.Min(beats, d);
            var z = d;

            while (z > 0 || beats > 1)
            {
                for (var i = 0; i < beats; i++)
                {
                    s[i].AddRange(s[s.Count - 1 - i]);
                }
                s = s.GetRange(0, s.Count - beats);
                z -= beats;
                d = pulses - beats;
                pulses = Math.Max(beats, d);
                beats = Math.Min(beats, d);
            }

            var ret = new List<bool>();
            foreach (var sublist in s)
            {
                ret.AddRange(sublist);
            }
            return ret;
        }
    }
}