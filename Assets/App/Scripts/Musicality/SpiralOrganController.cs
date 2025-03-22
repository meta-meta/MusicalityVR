using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class SpiralOrganController : MonoBehaviour, IReactComponent<SpiralOrganController.State>
    {
        public static readonly Dictionary<string, SpiralOrganController> OrganControllers =
            new Dictionary<string, SpiralOrganController>();
        
        #region IReactComponent

        [SuppressMessage("ReSharper", "UnassignedField.Global")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        public struct State
        {
            public List<OrganPartialState> Partials;
            public NoteIrrational Diapason;
            public int OscPortAmbisonicVisualizer;
            public int OscPortAudioObject;
            public int OscPortDirectivityShaper;
            public string OscAddress; // "/organ", "/organ2"
        }

        [SuppressMessage("ReSharper", "UnassignedField.Global")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        public struct OrganPartialState
        {
            public NoteIrrational Interval;
            public float Amplitude;
            public float Release;
        }
        
        public GameObject GameObject => gameObject;

        public State CurrentState => new State()
        {
        };

        public int ComponentId { get; set; }
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            // SetOscMessages();
            _oscOutMax = GameObject.Find("OSC").GetComponent<OscManager>().OscOutMaxMsp;

            OrganPartial CreatePartial(int i)
            {
                var partial = Instantiate(pickerPrefab, drawbars).GetComponent<SpiralIrrationalIntervalPicker>();
                if (i == 0) // this is weird. I think this is just saying draw track starting at this partial's position?
                {
                    partial.isIrrationalSpiralTrack = true; // draw track
                    partial.ForceRefreshData();
                }

                // SendAmplitude(partial.SliderAmplitude.initialVal, i);
                // SendInterval(new NoteIrrational(i + 1), i);

                return new OrganPartial()
                {
                    AmplitudeSlider = partial.SliderAmplitude,
                    ReleaseSlider = partial.SliderDecay,
                    Index = i,
                    IntervalPicker = partial,
                };
            }

            _partials = Enumerable.Range(0, 16)
                .Select(CreatePartial)
                .ToList();

            foreach (var partial in _partials)
            {
                partial.AmplitudeSlider.ValueChanged += val =>
                {
                    SendAmplitude(val, partial.Index);
                    partial.IntervalPicker.SetRimPower(val);
                };

                partial.ReleaseSlider.ValueChanged += val =>
                {
                    SendRelease(val, partial.Index);
                };

                partial.IntervalPicker.IntervalChanged += interval =>
                {
                    SendInterval(interval, partial.Index);

                    // temporarily display the partial's interval
                    intervalDisplay.SetNote(interval);
                    CancelInvoke(nameof(ResetDisplay));
                    Invoke(nameof(ResetDisplay), 0.5f);
                };
            }

            diapasonPicker.IntervalChanged += SetDiapason;
            volumePicker.OnVolumeChange += SendVolume;
            
            UpdateFromState(initialState);
            
            SendVolume(volumePicker.volume);
            NoteOffAll();

            OrganControllers[_oscAddr] = this;
        }

        public void Unmount()
        {
            NoteOffAll();
            SendVolume(0);
            OrganControllers.Remove(_oscAddr);
            // TODO: noteoffs, zero out partials, or wholesale reset organ patch 
        }

        public void UpdateFromState(State nextState)
        {
            if (nextState.OscAddress != "/organ" && nextState.OscAddress != "/organ2")
                throw new Exception("Only 2 organs are available: /organ and /organ2");
            
            _oscAddr = nextState.OscAddress;
            SetOscMessages();
            
            SetDiapason(nextState.Diapason);
            diapasonPicker.SetAngleFromNote(nextState.Diapason);

            var idx = 0;
            
            foreach (var p in nextState.Partials)
            {
                var partial = _partials[idx];
                partial.AmplitudeSlider.SetValue(p.Amplitude);
                partial.ReleaseSlider.SetValue(p.Release);
                partial.IntervalPicker.SetAngleFromNote(p.Interval);
                partial.IntervalPicker.UpdateColors(p.Interval);
                SendInterval(p.Interval, idx);
                idx++;
            }
            
            audioObjectIem.SetPortOut(nextState.OscPortAudioObject);
            directivityShaperIem.SetPortOut(nextState.OscPortDirectivityShaper);
            ambisonicVisualizer.SetPortIn(nextState.OscPortAmbisonicVisualizer);

            _materialIndex = nextState.OscAddress == "/organ2" ? 1 : 0;
            intervalDisplay.SetMaterial(_materialIndex);
            speakerAnchor.GetComponent<MeshRenderer>().material = speakerMaterials[_materialIndex];
            
            foreach (var tonnegg in _noteIndicesToTonneggs.Values) tonnegg.SetMaterial(_materialIndex);
        }

        private int _materialIndex;
        
        #endregion
        
        
        
        public struct OrganPartial
        {
            public SpiralIrrationalIntervalPicker IntervalPicker;
            public SpiralOrganSlider AmplitudeSlider;
            public SpiralOrganSlider ReleaseSlider;
            public int Index;
        }

        #region Inspector

        [Header("Config")] 
        [SerializeField] private float decayCurveExponent = 7;
        [SerializeField] private float decaySeconds = 15;

        [Header("Component Refs")] 
        [SerializeField] private AmbisonicVisualizer ambisonicVisualizer;
        [SerializeField] private AudioObjectIEM audioObjectIem;
        [SerializeField] private DirectivityShaperIEM directivityShaperIem;
        [SerializeField] private GameObject notePrefab;
        [SerializeField] private GameObject pickerPrefab;
        [SerializeField] private GameObject speakerAnchor;
        [SerializeField] private List<Material> speakerMaterials;
        [SerializeField] private SpiralIrrationalIntervalPicker diapasonPicker;
        [SerializeField] private SpiralVolumePicker volumePicker;
        [SerializeField] private Tonnegg intervalDisplay;
        [SerializeField] private Transform drawbars;
        [SerializeField] private Transform notesContainer;
        
        [Header("Lattice")] 
        [Range(0, 100)] public int lattice3Pow = 4;
        [Range(0, 100)] public int lattice5Pow = 4;
        [Range(0, 100)] public int lattice7Pow = 4;
        [SerializeField] private float latticeNoteScale = 0.1f;
        [SerializeField] private float latticeSpacing = 0.2f;

        #endregion
        
        private List<OrganPartial> _partials;
        private NoteIrrational _diapason;
        private OscMessage _drawbarAndAmpBank1;
        private OscMessage _drawbarAndAmpBank2;
        private OscMessage _drawbarAndDecayBank1;
        private OscMessage _drawbarAndDecayBank2;
        private OscMessage _drawbarAndFreqMultBank1;
        private OscMessage _drawbarAndFreqMultBank2;
        private OscMessage _noteAndCustomFreq;
        private OscMessage _noteAndVel;
        private OscMessage _volume;
        private OscOut _oscOutMax;
        private readonly Dictionary<int, Tonnegg> _noteIndicesToTonneggs = new Dictionary<int, Tonnegg>();
        private string _oscAddr = "/organ";


        private void SetOscMessages()
        {
            _drawbarAndAmpBank1 = new OscMessage($"{_oscAddr}/drawbarBank1/drawbarAndAmp");
            _drawbarAndDecayBank1 = new OscMessage($"{_oscAddr}/drawbarBank1/drawbarAndDecay");
            _drawbarAndFreqMultBank1 = new OscMessage($"{_oscAddr}/drawbarBank1/drawbarAndFreqMult");
            
            _drawbarAndAmpBank2 = new OscMessage($"{_oscAddr}/drawbarBank2/drawbarAndAmp");
            _drawbarAndDecayBank2 = new OscMessage($"{_oscAddr}/drawbarBank2/drawbarAndDecay");
            _drawbarAndFreqMultBank2 = new OscMessage($"{_oscAddr}/drawbarBank2/drawbarAndFreqMult");
            
            _noteAndCustomFreq = new OscMessage($"{_oscAddr}/noteAndCustomFreq");
            _noteAndVel = new OscMessage($"{_oscAddr}/noteAndVel");
            _volume = new OscMessage($"{_oscAddr}/volume");
        }

        private void SetDiapason(NoteIrrational diapason)
        {
            _diapason = diapason;
            ResetDisplay();
            foreach (var pair in _noteIndicesToTonneggs)
            {
                SendNoteAndCustomFreq(pair.Key, pair.Value.Note.Freq(_diapason.Val));
            }
        }

        private void MkNote()
        {
            var hpad = Mathf.PI / 12;

            var noteCount = _noteIndicesToTonneggs.Count;
            var notePadCmp = MkNote(new NoteJI(1, 1),
                new Vector3(
                    0.3f * Mathf.Cos(-noteCount * hpad),
                    0,
                    0.3f * Mathf.Sin(-noteCount * hpad) //+ Mathf.Sin(s * Mathf.PI / 48)
                ),
                Quaternion.identity);
            
            notePadCmp.transform.LookAt(notesContainer, Vector3.up);
        }

        private Tonnegg MkNote(NoteJI interval, Vector3 localPos, Quaternion localRot)
        {
            var noteObj = Instantiate(notePrefab, notesContainer);
            noteObj.transform.localPosition = localPos;
            noteObj.transform.localRotation = localRot;

            var notePadCmp = noteObj.GetComponent<Tonnegg>();
            
            // notePadCmp.oscAddr = oscAddr;
            notePadCmp.SetNote(interval);

            AddNote(noteObj);
            return notePadCmp;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            
            // clear existing notes (except note[0] and set it to 1:1)
            if (_noteIndicesToTonneggs.Count == 0) return;
            
            var oneToOne = _noteIndicesToTonneggs.Values.Where(n =>
            {
                var val = ((NoteJI)n.Note).Val;
                return val.Denominator == 1 && val.Numerator == 1;
            }).First();
            notesContainer.position = oneToOne.transform.position;
            notesContainer.rotation = oneToOne.transform.rotation;
            // notesContainer.localScale = oneToOne.transform.localScale;
            foreach (var notePad in _noteIndicesToTonneggs.Values) Destroy(notePad.gameObject);
            _noteIndicesToTonneggs.Clear();
            NoteOffAll();
            
            for (var i3 = -lattice3Pow; i3 < lattice3Pow + 1; i3++)
            {
                for (var i5 = -lattice5Pow; i5 < lattice5Pow + 1; i5++)
                {
                    for (var i7 = -lattice7Pow; i7 < lattice7Pow + 1; i7++)
                    {
                        var i3Pow = Mathf.FloorToInt(Mathf.Pow(3, Mathf.Abs(i3)));
                        var i5Pow = Mathf.FloorToInt(Mathf.Pow(5, Mathf.Abs(i5)));
                        var i7Pow = Mathf.FloorToInt(Mathf.Pow(7, Mathf.Abs(i7)));
                        
                        var interval3 = i3 < 0 ? NoteJI.ConstrainToOctaveUtonal(i3Pow) : NoteJI.ConstrainToOctaveOtonal(i3Pow);
                        var interval5 = i5 < 0 ? NoteJI.ConstrainToOctaveUtonal(i5Pow) : NoteJI.ConstrainToOctaveOtonal(i5Pow);
                        var interval7 = i7 < 0 ? NoteJI.ConstrainToOctaveUtonal(i7Pow) : NoteJI.ConstrainToOctaveOtonal(i7Pow);
                        
                        var notePadCmp = MkNote(
                            (interval3 + interval5 + interval7).ConstrainToOctave(),
                            new Vector3(i3 * latticeSpacing, i5 * latticeSpacing, i7 * latticeSpacing),
                            Quaternion.identity);
                        
                        notePadCmp.ToggleNotePicker(false);
                        notePadCmp.transform.localScale = Vector3.one * latticeNoteScale;
                        notePadCmp.GetComponent<ObjectManipulator>().HostTransform =
                            notesContainer;

                    }
                }
            }
            
            Debug.Log($"created ${_noteIndicesToTonneggs.Count - 1} notes");
        }

        public void AddNote(GameObject noteObj)
        {
            var tonnegg = noteObj.GetComponent<Tonnegg>();
            var highestAvail = Enumerable.Range(0, 129).First(i => !_noteIndicesToTonneggs.ContainsKey(i));
            if (highestAvail == 128)
            {
                Debug.LogWarning("No more note indices available");
                return;
            }

            tonnegg.isOscBypassed = true;
            tonnegg.isContinuousVelocity = true;
            
            tonnegg.noteIndex = highestAvail;
            SendNoteAndCustomFreq(tonnegg.noteIndex, tonnegg.Note.Freq(_diapason.Val));

            tonnegg.NoteEvent += (i, n, vel) => SendNoteAndVel(i, Mathf.FloorToInt(vel * 127f));
            tonnegg.NoteChanged += (i, n) => SendNoteAndCustomFreq(i, n.Freq(_diapason.Val));

            _noteIndicesToTonneggs.Add(tonnegg.noteIndex, tonnegg);
            
            _materialIndex = _oscAddr == "/organ2" ? 1 : 0;
            tonnegg.SetMaterial(_materialIndex);
        }

        public void RemoveNote(GameObject noteObj)
        {
            var notePadCmp = noteObj.GetComponent<Tonnegg>();
            SendNoteAndVel(notePadCmp.noteIndex, 0);
            _noteIndicesToTonneggs.Remove(notePadCmp.noteIndex);
        }
        
        private void NoteOffAll ()
        {
            foreach (var i in Enumerable.Range(0, 128)) SendNoteAndVel(i, 0);
        }


        private void SendNoteAndVel(int note, int vel)
        {
            _noteAndVel.Set(0, note);
            _noteAndVel.Set(1, vel * 10 /* TODO: temp */);
            _oscOutMax.Send(_noteAndVel);
        }

        private void SendNoteAndCustomFreq(int note, float freq)
        {
            _noteAndCustomFreq.Set(0, note);
            _noteAndCustomFreq.Set(1, freq);
            _oscOutMax.Send(_noteAndCustomFreq);
        }

        private void SendAmplitude(float val, int indexOfPartial)
        {
            var amplitude = val == 0 ? 0 : 1 + Mathf.FloorToInt(Mathf.Pow(val, 2) * 127);
            if (indexOfPartial < 8)
            {
                _drawbarAndAmpBank1.Set(0, indexOfPartial);
                _drawbarAndAmpBank1.Set(1, amplitude);
                _oscOutMax.Send(_drawbarAndAmpBank1);
            }
            else
            {
                _drawbarAndAmpBank2.Set(0, indexOfPartial - 8);
                _drawbarAndAmpBank2.Set(1, Mathf.FloorToInt(val * 227));
                _oscOutMax.Send(_drawbarAndAmpBank2);
            }
        }

        private void SendRelease(float val, int indexOfPartial)
        {
            var decay = val == 0 ? 0 : 1 + Mathf.FloorToInt(decaySeconds * 1000 * Mathf.Pow(val, decayCurveExponent));
            
            if (indexOfPartial < 8)
            {
                _drawbarAndDecayBank1.Set(0, indexOfPartial);
                _drawbarAndDecayBank1.Set(1, decay);
                _oscOutMax.Send(_drawbarAndDecayBank1);
            }
            else
            {
                _drawbarAndDecayBank2.Set(0, indexOfPartial - 8);
                _drawbarAndDecayBank2.Set(1, decay);
                _oscOutMax.Send(_drawbarAndDecayBank2);
            }
        }

        private void SendInterval(NoteIrrational interval, int indexOfPartial)
        {
            if (indexOfPartial < 8)
            {
                _drawbarAndFreqMultBank1.Set(0, indexOfPartial);
                _drawbarAndFreqMultBank1.Set(1, interval.Val);
                _oscOutMax.Send(_drawbarAndFreqMultBank1);
            }
            else
            {
                _drawbarAndFreqMultBank2.Set(0, indexOfPartial - 8);
                _drawbarAndFreqMultBank2.Set(1, interval.Val);
                _oscOutMax.Send(_drawbarAndFreqMultBank2);
            }
        }

        private void SendVolume(float volume)
        {
            _volume.Set(0, volume * 127);
            _oscOutMax.Send(_volume);
        }


        private void ResetDisplay() => intervalDisplay.SetNote(_diapason);
    }
}