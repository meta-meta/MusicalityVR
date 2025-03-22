using System;
using UnityEngine;

namespace Musicality
{
    public struct NoteEDO : INote
    {
        public NoteEDO(int val, int octaveDivisions)
        {
            Val = val;
            OctaveDivisions = octaveDivisions;
        }

        public NoteType NoteType => Musicality.NoteType.EDO;

        public float AngleOnCircle()
        {
            var a = AngleOnSpiral() % 360;
            return CircleUtils.Mod(a, 360);
        }

        public float AngleOnSpiral() => Val * (360f / OctaveDivisions);

        // TODO: this is if A = diapson but we're using JI freq = diapson * JI Interval so use 60 instead so 0(12ET) lines up with 1:1 
        // public float Freq(float diapason) => Mathf.Pow(2, ((Val - 69) / (float)OctaveDivisions)) * diapason;
        public float Freq(float diapason) => Mathf.Pow(2, ((Val - 60) / (float)OctaveDivisions)) * diapason;

        public int MidiNum() => Val; // diapson = C4  (not A 440) - TODO: wrangle that
        public int OctaveRelativeToDiapason() => Mathf.FloorToInt((Val - 60) / (float)OctaveDivisions);
        public float OctaveRelativeToDiapasonFloat() => (Val - 60) / (float)OctaveDivisions;

        public int OctaveDivisions;
        public int Val;
        public static implicit operator NoteEDO(int value) => new NoteEDO() { Val = value, OctaveDivisions = 12 };

        public string LabelPitch()
        {
            if (OctaveDivisions != 12)
                throw new System.NotImplementedException("Only 12 EDO is implemented.");
            return Val.ToString();
        }

        public string LabelPitchClass()
        {
            Debug.Log($"val {Val}  oct {OctaveDivisions}");
            var pc = CircleUtils.Mod(Val, OctaveDivisions);
            if (OctaveDivisions == 12)
                return pc switch
                {
                    11 => "Ɛ",
                    10 => "૪",
                    _ => pc.ToString()
                };

            return $"{pc}\\{OctaveDivisions}";
        }

        public INote IntervalTo(INote other)
        {
            if (other.NoteType != NoteType)
            {
                Debug.LogWarning($"Interval calculation between different noteTypes is not yet implemented");
                throw new NotImplementedException();
            }

            var otherNoteEdo = (NoteEDO)other;

            if (OctaveDivisions != otherNoteEdo.OctaveDivisions)
                throw new System.NotImplementedException("IntervalTo with different OctaveDivisions not implemented.");

            return new NoteEDO(otherNoteEdo.Val - Val, OctaveDivisions);
        }

        public bool Equals(INote other)
        {
            if (other == null) return false;
            if (other.NoteType != Musicality.NoteType.EDO) return false;
            var noteEdo = (NoteEDO)other;
            return noteEdo.Val == Val && noteEdo.OctaveDivisions == OctaveDivisions;
        }

        public override int GetHashCode() => (Val, OctaveDivisions).GetHashCode();

        // public static bool operator ==(NoteEDO)

        // public T ToOctave<T>(T note) where T : INote, new()
        // {
        //     return new NoteEDO(60, 12);
        // }
    }
}