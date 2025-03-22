using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musicality
{
    [Serializable]
    public struct NoteJI : INote
    {
        public NoteType NoteType => Musicality.NoteType.JI;
        public Ratio Val;

        public int Denominator => Val.Denominator;
        public int Numerator => Val.Numerator;

        public float AngleOnCircle()
        {
            var a = AngleOnSpiral() % 360;
            return CircleUtils.Mod(a, 360);
        }

        public float AngleOnSpiral() => 360 * Mathf.Log(Val.Decimal, 2);
        public float Freq(float diapason) => diapason * Val.Decimal;

        public int OctaveRelativeToDiapason() => Mathf.FloorToInt(Mathf.Log(Val.Decimal, 2));
        public float OctaveRelativeToDiapasonFloat() => Mathf.Log(Val.Decimal, 2);

        public string LabelPitch() => $"{Val.Numerator}:{Val.Denominator}";

        public string LabelPitchClass()
        {
            var constrained = ConstrainToOctave(); // TODO: also reduce
            return $"{constrained.Numerator}:{constrained.Denominator}";
        }

        public INote IntervalTo(INote other)
        {
            if (other.NoteType != NoteType)
            {
                Debug.LogWarning("Interval calculation between different noteTypes is not yet implemented");
                throw new NotImplementedException();
            }

            return (NoteJI)other - this;
        }

        public int MidiNum()
        {
            throw new NotImplementedException("JI needs a MIDI mapping");
        }

        public NoteJI(int numerator, int denominator)
        {
            Val = new Ratio(numerator, denominator);
        }

        public NoteJI(Ratio val)
        {
            Val = val;
        }

        public static implicit operator NoteJI(Ratio val) => new NoteJI(val);

        // FIXME
        public static NoteJI ConstrainToOctaveOtonal(int numerator)
        {
            if (numerator == 0) return new NoteJI(1, 1);
            var i = 0;
            while (numerator / Mathf.Pow(2, i) >= 2) i++;
            return new Ratio(numerator, Mathf.FloorToInt(Mathf.Pow(2, i)));
        }

        // TODO: this may not be quite right Val.Denominator != 1 is to fix 6:1 -> 3:2 (was showing 1:1)
        // public NoteJI ConstrainToOctave() => Val.Numerator == 1 || (Val.Denominator != 1 && Val.Numerator % 2 == 0) // if numerator is even, it's utonal
        //     ? ConstrainToOctaveUtonal(Val.Denominator)
        //     : ConstrainToOctaveOtonal(Val.Numerator);

        // TODO: is this subject to floating point errors? prob need some internal integer math instead of `.Decimal`
        public NoteJI ConstrainToOctave() =>
            Val.Decimal < 1
                ? new NoteJI(Val * 2).ConstrainToOctave()
                : Val.Decimal >= 2
                    ? new NoteJI(Val / 2).ConstrainToOctave()
                    : Val;

        public static NoteJI ConstrainToOctaveUtonal(int denominator)
        {
            if (denominator == 0) return new NoteJI(1, 1);
            var i = 0;
            while (Mathf.Pow(2, i) / denominator < 1) i++;
            return new Ratio(Mathf.FloorToInt(Mathf.Pow(2, i)), denominator);
        }

        private static Dictionary<int, HashSet<NoteJI>> _memoizedNotes = new Dictionary<int, HashSet<NoteJI>>();
        
        // TODO: This only includes the axes and not the whole lattice 😳
        public static HashSet<NoteJI> NotesFromPrimes(IEnumerable<int> primes, int coefficientExtent, bool isOtonalIncluded = true,
            bool isUtonalIncluded = false)
        {

            if (_memoizedNotes.TryGetValue(coefficientExtent, out var notesCached))
                return notesCached;
            
            var notes = new HashSet<NoteJI>();


            var lattice3Pow = coefficientExtent;
            var lattice5Pow = coefficientExtent;
            var lattice7Pow = coefficientExtent;
            var lattice11Pow = coefficientExtent;
            
            Debug.Log("makin notes from primes");
            for (var i3 = -lattice3Pow; i3 < lattice3Pow + 1; i3++)
            {
                
                for (var i5 = -lattice5Pow; i5 < lattice5Pow + 1; i5++)
                {
                
                    for (var i7 = -lattice7Pow; i7 < lattice7Pow + 1; i7++)
                    {
                        for (var i11 = -lattice11Pow; i11 < lattice11Pow + 1; i11++)
                        {
                            var i3Pow = Mathf.FloorToInt(Mathf.Pow(3, Mathf.Abs(i3)));
                            var i5Pow = Mathf.FloorToInt(Mathf.Pow(5, Mathf.Abs(i5)));
                            var i7Pow = Mathf.FloorToInt(Mathf.Pow(7, Mathf.Abs(i7)));
                            var i11Pow = Mathf.FloorToInt(Mathf.Pow(11, Mathf.Abs(i11)));
                        
                            // Debug.Log($"i3Pow: {i3Pow}");
                            var interval3 = i3 < 0 ? ConstrainToOctaveUtonal(i3Pow) : ConstrainToOctaveOtonal(i3Pow);

                            // Debug.Log($"i5Pow: {i5Pow}");
                            var interval5 = i5 < 0 ? ConstrainToOctaveUtonal(i5Pow) : ConstrainToOctaveOtonal(i5Pow);
                            
                            // Debug.Log($"i7Pow: {i7Pow}");
                            var interval7 = i7 < 0 ? ConstrainToOctaveUtonal(i7Pow) : ConstrainToOctaveOtonal(i7Pow);
                            
                            // Debug.Log($"i11Pow: {i11Pow}");
                            var interval11 = i11 < 0 ? ConstrainToOctaveUtonal(i11Pow) : ConstrainToOctaveOtonal(i11Pow);
                        
                            // var interval3 = NoteJI.ConstrainToOctaveOtonal(Mathf.FloorToInt(Mathf.Pow(3, i3)));
                            // var interval5 = NoteJI.ConstrainToOctaveOtonal(Mathf.FloorToInt(Mathf.Pow(5, i5)));
                            // var interval7 = NoteJI.ConstrainToOctaveOtonal(Mathf.FloorToInt(Mathf.Pow(7, i7)));

                            var sum = interval3 + interval5 + interval7 + interval11;
                            if (sum.Numerator == 0) continue; // TODO getting 0:1 somehow for i3Pow 81 i5Pow 625 i7Pow 2401 i11Pow 14641
                            // Debug.Log($"interval3 + interval5 + interval7 + interval11: {interval3 + interval5 + interval7 + interval11}");
                            try
                            {
                                var note = (sum).ConstrainToOctave();
                                // Debug.Log($"i3:{i3} i5:{i5} i7:{i7} i11: {i11} {interval3} + {interval5} + {interval7} + {interval11} = {interval3 + interval5 + interval7 + interval11} => {note}");
                                notes.Add(note);
                                // Debug.Log($"{note} added");
                            }
                            catch (Exception e)
                            {
                                Debug.Log($"missing note because of exception on i3 {i3} i5 {i5} i7 {i7} i11 {i11}");
                                Debug.LogException(e);
                            }
                            
                        }

                    }
                }
            }
            

            // foreach (var prime in primes)
            // {
            //     notes.Add(new NoteJI(1, 1));
            //     
            //     if (isOtonalIncluded)
            //     {
            //         // Debug.Log($"generating otonal notes for prime {prime} up through coefficientExtent {coefficientExtent}");
            //
            //         for (var i = 1; i <= coefficientExtent; i++)
            //             notes.Add(ConstrainToOctaveOtonal(Mathf.FloorToInt(Mathf.Pow(prime, i))));
            //     }
            //     
            //     if (isUtonalIncluded)
            //     {
            //         // Debug.Log($"generating utonal notes for prime {prime} down through -coefficientExtent {-coefficientExtent}");
            //
            //         for (var i = 1; i < coefficientExtent; i++)
            //             notes.Add(ConstrainToOctaveUtonal(Mathf.FloorToInt(Mathf.Pow(prime, i))));
            //     }
            // }

            _memoizedNotes.Add(coefficientExtent, notes);
            return notes;
        }

        public static NoteJI operator +(NoteJI a, NoteJI b) => a.Val * b.Val;
        public static NoteJI operator -(NoteJI a, NoteJI b) => a.Val / b.Val;
        
        // TODO: complement operator


        public bool Equals(INote other)
        {
            if (other == null) return false;
            if (other.NoteType != Musicality.NoteType.JI) return false;
            return ((NoteJI)other).Val == Val;
        }
        
        public override int GetHashCode() => Val.GetHashCode();


        public override string ToString()
        {
            return $"{Val.Numerator}:{Val.Denominator}";
        }
    }
}