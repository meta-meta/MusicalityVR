using System;

namespace Musicality
{
    public struct NoteFunction : INote
    {
        public NoteFunction(string val)
        {
            Val = val;
        }

        public NoteType NoteType => NoteType.Function;

        public float AngleOnCircle() => 0;

        public float AngleOnSpiral() => 0;

        public float Freq(float diapason)
        {
            throw new NotImplementedException("Freq N/A for Function.");
        }

        public int MidiNum() => 0;
        public int OctaveRelativeToDiapason() => 0;
        public float OctaveRelativeToDiapasonFloat() => 0;

        public string Val;

        public string LabelPitch() => LabelPitchClass();

        public string LabelPitchClass() => Val;

        public INote IntervalTo(INote other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(INote other)
        {
            if (other == null) return false;
            if (other.NoteType != NoteType.Function) return false;
            return ((NoteFunction)other).Val == Val;
        }
        
        public override int GetHashCode() => (Val).GetHashCode();
    }
}