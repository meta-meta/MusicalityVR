using System;
using Newtonsoft.Json;

namespace Musicality
{
    public interface INote : IEquatable<INote>
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public NoteType NoteType { get; }
        
        public float AngleOnCircle();
        public float AngleOnSpiral();
        public float Freq(float diapason); // TODO: investigate where floating point error might matter. do we want double?
        public int MidiNum();
        public int OctaveRelativeToDiapason();
        public float OctaveRelativeToDiapasonFloat();
        public string LabelPitch();
        public string LabelPitchClass();
        // public T ToOctave<T>(T note) where T : INote, new();

        public INote IntervalTo(INote other);
    }
}
