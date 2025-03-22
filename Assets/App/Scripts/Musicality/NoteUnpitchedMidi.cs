using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Musicality
{
    public struct NoteUnpitchedMidi : INote
    {
        public NoteUnpitchedMidi(int val, string label, UnpitchedNoteCollection noteCollection)
        {
            Label = label;
            Val = val;
            NoteCollection = noteCollection;
        }

        public NoteUnpitchedMidi GetNoteInSet(int val) => NotesInSet.First(n => n.Val == val);
        
        public NoteType NoteType => NoteType.UnpitchedMidi;

        public float AngleOnCircle()
        {
            var a = AngleOnSpiral() % 360;
            return CircleUtils.Mod(a, 360);
        }

        public float AngleOnSpiral() => Val * (360f / NotesInSet.Count);

        // TODO: this is if A = diapson but we're using JI freq = diapson * JI Interval so use 60 instead so 0(12ET) lines up with 1:1 
        // public float Freq(float diapason) => Mathf.Pow(2, ((Val - 69) / (float)OctaveDivisions)) * diapason;
        public float Freq(float diapason)
        {
            throw new System.NotImplementedException("Freq N/A for UnpitchedMidi.");
        }

        public int MidiNum() => Val;
        public int OctaveRelativeToDiapason() => 0;
        public float OctaveRelativeToDiapasonFloat() => 0;

        public int Val;
        public string Label;
        private HashSet<NoteUnpitchedMidi> NotesInSet => UnpitchedNoteCollections.Get(NoteCollection);
        public UnpitchedNoteCollection NoteCollection;

        public string LabelPitch() => LabelPitchClass();

        public string LabelPitchClass()
        {
            var i = Val;
            return NotesInSet.First(n => n.Val == i).Label;
        }

        public INote IntervalTo(INote other)
        {
            throw new System.NotImplementedException();
        }


        public bool Equals(INote other)
        {
            if (other == null) return false;
            if (other.NoteType != Musicality.NoteType.UnpitchedMidi) return false;
            return ((NoteUnpitchedMidi)other).Val == Val;
            // TODO: account for notesInSet
        }
        
        public override int GetHashCode() => (Val).GetHashCode();

        
        // public T ToOctave<T>(T note) where T : INote, new()
        // {
        //     return new NoteEDO(60, 12);
        // }
    }
}