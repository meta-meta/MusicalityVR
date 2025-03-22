using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Musicality
{
    public class SpiralNotePicker : SpiralControl
    {
        private readonly Dictionary<float, INote> _anglesToNotes = new Dictionary<float, INote>();
        private readonly HashSet<NoteEDO> _notesEdo = new HashSet<NoteEDO>();
        private readonly HashSet<NoteJI> _notesJi = new HashSet<NoteJI>();
        private readonly HashSet<NoteUnpitchedMidi> _notesUnpitched = new HashSet<NoteUnpitchedMidi>();
        public Action<INote> onNoteSelect;
        public List<int> jiPrimes = new List<int>() { 3 };
        public UnpitchedNoteCollection unpitchedNoteCollection = UnpitchedNoteCollection.CR78;
        public bool isEdoIncluded;
        public bool isUnpitched;
        public bool jiIsOtonalIncluded = true;
        public bool jiIsUtonalIncluded;
        public int edo = 12;
        public int jiExtent = 3;

        public struct State
        {
            public HashSet<INote> Notes;
            
            
            // TODO: deserialize :TuningDomain. perhaps there should be a central repo of tuning systems that a notePicker can reference instead of maintaining one per notePicker
        }
        
        public void SetAngleFromNote(INote note, bool addToSetIfMissing = false)
        {
            // TODO: _anglesToNotes should probably refer to some static collection so that the added note is in a cloned note's options
            if (addToSetIfMissing) AddNoteToSet(note);
            SetAngle(note.AngleOnCircle(), note.AngleOnSpiral());
        }
        
        public void AddNoteToSet(INote n)
        {
            var angle = n.AngleOnCircle();
            if (!_anglesToNotes.ContainsKey(angle))
            {
                _anglesToNotes.Add(angle, n);
                SetAngles(new HashSet<float>(_anglesToNotes.Keys));
            }
        }

        protected override HashSet<float> CalculateAngles()
        {
            _anglesToNotes.Clear();
            _notesEdo.Clear();
            _notesJi.Clear();
            _notesUnpitched.Clear();
            
            if (isUnpitched)
            {
                _notesUnpitched.UnionWith(UnpitchedNoteCollections.Get(unpitchedNoteCollection));
                foreach (var note in _notesUnpitched) _anglesToNotes.Add(note.AngleOnCircle(), note);
                return new HashSet<float>(_anglesToNotes.Keys);
            }

            if (jiIsOtonalIncluded && jiIsUtonalIncluded) // TODO: implement in NoteJI.NotesFromPrimes
            {
                _notesJi.UnionWith(NoteJI.NotesFromPrimes(jiPrimes, jiExtent, jiIsOtonalIncluded, jiIsUtonalIncluded));
                foreach (var noteJI in _notesJi)
                {
                    var angleOnCircle = noteJI.AngleOnCircle();
                    if (_anglesToNotes.ContainsKey(angleOnCircle))
                        Debug.Log(
                            $"[SpiralNotePicker] _anglesToNotes already contains key {angleOnCircle}. Skipping note {noteJI}");
                    else _anglesToNotes.Add(angleOnCircle, noteJI);
                }
            }
           
            if (isEdoIncluded)
            {
                var isJiIncluded = jiIsOtonalIncluded || jiIsUtonalIncluded;
                var edoNotes = Enumerable.Range(0, edo)
                    .Select(i => new NoteEDO(i + 60 /*(isJiIncluded ? 60 : 0)*/, edo));
                _notesEdo.UnionWith(edoNotes);
                // Add EDO notes (except where a JI interval has the same angle like 1:1)  
                foreach (var note in _notesEdo.Where(n => !_anglesToNotes.ContainsKey(n.AngleOnCircle())))
                    _anglesToNotes.Add(note.AngleOnCircle(), note);
            }

            return new HashSet<float>(_anglesToNotes.Keys);
        }

        protected override (float, float) OnAngleUpdate(float angleOnCircle, float angleOnSpiral)
        {
            var n = _anglesToNotes[angleOnCircle]; // positions of notes around a circle; use these along with octave (from spiral) to get actual note

            var octave = Mathf.FloorToInt(angleOnSpiral / 360);

            INote note;
            // angleOnSpiral -30 octave -1  noteEDO.Val 11 note.Val -1  pitch -1 octaveRelDia 0
            if (n is NoteEDO noteEDO)
            {
                note = new NoteEDO(noteEDO.Val + edo * octave, edo);
                // Debug.Log($"angleOnSpiral {angleOnSpiral} octave {octave}  noteEDO.Val {noteEDO.Val} note.Val {noteEDO.Val + 12 * octave}  pitch {note.LabelPitch()} octaveRelDia {note.OctaveRelativeToDiapason()} ");
            }
            else if (n is NoteJI noteJI)
            {
                // Debug.Log($"angleOnSpiral {angleOnSpiral} pitch {noteJI.LabelPitch()}  ctave {octave}  {noteJI.OctaveRelativeToDiapason()} ");


                var pow = Mathf.FloorToInt(Mathf.Pow(2, Mathf.Abs(octave)));


                var num = noteJI.Val.Numerator * (octave > 0 ? pow : 1);
                var den = noteJI.Val.Denominator * (octave < 0 ? pow : 1);
                // Debug.Log($"noteJi {noteJI.LabelPitchClass()} num {num}  den {den}");
                note = new NoteJI(num, den);
            }
            else if (n is NoteUnpitchedMidi)
            {
                note = n;
            }
            else
            {
                // Debug.LogWarning($"picking an unsupported INote {n}");
                return (angleOnCircle, angleOnSpiral);
            }

            onNoteSelect?.Invoke(note);
            return (angleOnCircle, angleOnSpiral);
        }
    }
}