using System;
using UnityEngine;

namespace Musicality
{
    public struct NoteIrrational : INote
    {
        public static NoteIrrational FromAngle(float angleOnSpiral) => new NoteIrrational(Mathf.Pow(2, angleOnSpiral / 360f));
        public NoteType NoteType => Musicality.NoteType.Irrational;

        public float AngleOnCircle()
        {
            var a = AngleOnSpiral() % 360;
            return a > 0 ? a : a + 360;
        }

        public float AngleOnSpiral() => Val == 0 ? 0 : 360 * Mathf.Log(Val, 2);
        public float Freq(float diapason)
        {
            throw new System.NotImplementedException();
        }

        public float Val;
        public int OctaveRelativeToDiapason() => Mathf.FloorToInt(Mathf.Log(Val, 2));
        public float OctaveRelativeToDiapasonFloat() => Mathf.Log(Val, 2);

        public static implicit operator NoteIrrational(float value) => new NoteIrrational(value);
        public string LabelPitch() => Val.ToString("G6"); // https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        // public string LabelPitch() => $"{Val,7:.00}"; // https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        public string LabelPitchClass() => LabelPitch(); // TODO: Reduce to octave
        public INote IntervalTo(INote other)
        {
            if (other.NoteType != NoteType)
            {
                Debug.LogWarning($"Interval calculation between different noteTypes is not yet implemented");
                throw new NotImplementedException();
            }

            return new NoteIrrational(((NoteIrrational)other).Val - Val);
        }

        public int MidiNum()
        {
            throw new System.NotImplementedException("Irrational has no MIDI mapping. Perhaps this should not be INote but something more abstract.");
        }
        
        public NoteIrrational(float val)
        {
            Val = val;
        }
        
        public bool Equals(INote other)
        {
            if (other == null) return false;
            if (other.NoteType != Musicality.NoteType.Irrational) return false;
            return ((NoteIrrational)other).Val == Val;
        }
        
        public override int GetHashCode() => Val.GetHashCode();
    }
}