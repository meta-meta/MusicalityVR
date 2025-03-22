using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using MRTK_Custom.Dock;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Musicality
{
    public class Tonnegg : MonoBehaviour, IMixedRealityFocusHandler, IReactComponent<Tonnegg.State>
    {
        #region Inspector

        [Header("Config")] 
        public INote Note = new NoteJI(1, 1);
        public bool isContinuousVelocity = true;
        public bool isOscBypassed;
        [SerializeField] private float velMult = 0.1f;
        [SerializeField] private float velMultLight = 1.3f;
        [SerializeField] private float velMultVibe = 0.5f;
        [SerializeField] private bool retainMainColor;
        public int noteIndex; // for addressing a note that can be retuned
        public string oscAddr;

        [Header("Component Refs")]
        
        [SerializeField] private SpiralNotePicker notePicker;
        [SerializeField] private TextMeshPro pitchClassLabel;
        [SerializeField] private TextMeshPro sub; // optional
        [SerializeField] private TextMeshPro sup;
        [SerializeField] private CapsuleCollider col;
        [SerializeField] private Transform visual;
        [SerializeField] private List<Material> materials;

        private bool _areRefsSetup;

        private void SetupRefsIfNeeded()
        {
            if (_areRefsSetup) return;

            var viz = visual ?? transform.Find("Visual") ?? transform; // prefab has Visual child but some NotePads still do not
            // _material = Application.isPlaying
            //     ? viz.GetComponent<MeshRenderer>().material
            //     : viz.GetComponent<MeshRenderer>().sharedMaterial;
            _meshRenderer = viz.GetComponent<MeshRenderer>();
            _materialPropertyBlock = new MaterialPropertyBlock(); // https://web.archive.org/web/20220220070627/http://thomasmountainborn.com/2016/05/25/materialpropertyblocks/

            if (pitchClassLabel == null)
                pitchClassLabel = transform.Find("PitchClassLabel")?.GetComponent<TextMeshPro>();
            if (sub == null) sub = transform.Find("sub")?.GetComponent<TextMeshPro>();
            if (sup == null) sup = transform.Find("sup")?.GetComponent<TextMeshPro>();
            if (col == null) col = GetComponent<CapsuleCollider>();
            _velocityProbe = transform.EnsureComponent<VelocityProbe>();
            _areRefsSetup = true;
            notePicker = GetComponentInChildren<SpiralNotePicker>();
            if (notePicker) notePicker.onNoteSelect += OnNoteSelect;
        }
        #endregion

        private Coroutine _lightUpCoroutine;
        private INote _noteWhileHoverDock = new NoteJI(1, 1);
        private MalletHead _malletHead;
        private MaterialPropertyBlock _materialPropertyBlock;
        private MeshRenderer  _meshRenderer;
        private OscManager _oscManager;
        private OscMessage _noteAndVel; // TODO: this should be in an instrument controller like how SpiralOrganController is setup. UNLESS it introduces latency
        private VelocityProbe _velocityProbe;
        private bool _isMalletTouching;
        private float _velCurrent;
        
        private static readonly int MatColor = Shader.PropertyToID("_Color");
        private static readonly int MatHoverColorOverride = Shader.PropertyToID("_HoverColorOverride");
        private static readonly int MatRimColor = Shader.PropertyToID("_RimColor");
        private static readonly int MatRimPower = Shader.PropertyToID("_RimPower");

        public delegate void NoteChangeDelegate(int noteIndex, INote note);
        public delegate void NoteEventDelegate(int noteIndex, INote note, float vel);

        public event NoteChangeDelegate NoteChanged;
        public event NoteEventDelegate NoteEvent;

        public static Action<Tonnegg, MalletHead> OnMalletEnter;
        public static Action<Tonnegg, MalletHead> OnMalletHeadExit;

        private void OnNoteSelect(INote selectedNote)
        {
            if (SetNote(selectedNote)) notePicker.Vibe();
        }

        // Start is called before the first frame update
        private void Awake()
        {
            _oscManager = GameObject.Find("OSC")?.GetComponent<OscManager>();
            if (!_oscManager) Debug.LogWarning("OscManager not found");

            SetupRefsIfNeeded();
            UpdateView(Note);
            
            DockableSetup();
            CloneableSetup();
            
            MalletHead.OnMalletDestroy += OnMalletDestroy;
        }

        private void OnMalletDestroy(MalletHead malletHead)
        {
            MalletHeadExit(malletHead);
            _isMalletTouching = false;
            _malletHead = null;
        }

        // private void OnValidate()
        // {
        //     SetupRefsIfNeeded();
        //     UpdateView(Note);
        // }

        private void OnDestroy()
        {
            DockableCleanup();
            MalletHead.OnMalletDestroy -= OnMalletDestroy;

            if (notePicker) notePicker.onNoteSelect -= OnNoteSelect;
            NoteOff();
            MalletHeadExit(_malletHead);
        }
    


        // TODO: only call from notePicker
        public bool SetNote(INote n)
        {
            if (Note.Equals(n)) return false;

            Note = n;
            SetupRefsIfNeeded();
            UpdateView(Note);
            NoteChanged?.Invoke(noteIndex, Note);
            return true;
        }

        private void Update()
        {
            if (isContinuousVelocity && _isMalletTouching)
            {
                LightSet(_velCurrent);
                _malletHead.Vibe(_velCurrent * velMultVibe, false);
            }

            if (Input.GetMouseButtonDown(1) && _focusHandedness != Handedness.None && notePicker)
                ToggleNotePicker(!notePicker.gameObject.activeSelf);

            if (_isFocusedWithController)
            {
                var isButtonDown = OVRInput.GetDown(_focusHandedness.IsLeft()
                    ? OVRInput.RawButton.X
                    : OVRInput.RawButton.A);

                var isMouseButton = Input.GetMouseButtonDown(1);

                if (isButtonDown && notePicker) ToggleNotePicker(!notePicker.gameObject.activeSelf);
            }
        }

        public void ToggleNotePicker(bool isOn) => notePicker.gameObject.SetActive(isOn);

        private void FixedUpdate()
        {
            if (isContinuousVelocity && _isMalletTouching) NoteOn(VelFromPosition());
        }

        private float VelFromPosition()
        {
            var tr = transform;
            var scaleZ = tr.lossyScale.z;
            var backward = -tr.forward;
            var plane = new Plane(-backward, tr.position + backward * (col.height * (scaleZ / 2)));
            var trPointer = _malletHead.transform;
            var distFromFront = plane.GetDistanceToPoint(trPointer.position - backward * (trPointer.lossyScale.z / 2));
            return Mathf.Clamp01(_malletHead.GainEnvelopeVal * distFromFront / scaleZ);
        }

        private float VelFromVelocityProbe()
        {
            var impactVelocity = (_velocityProbe.velocityVec - _malletHead.velocityVec).magnitude;
            return Mathf.Clamp01(impactVelocity * velMult * _malletHead.GainEnvelopeVal);
        }

        /* When isTrigger=false, collision with other objects */
        private void OnCollisionEnter(Collision other)
        {
            NoteOn(Mathf.Clamp01(Mathf.Pow((other.impulse.magnitude / Time.fixedDeltaTime) / 70f, 1)));
        }


        /* Trigger does not require that one of the objects not be kinematic. */
        /* This is for kinematic mallets */
        private void OnTriggerEnter(Collider other)
        {
            var malletHead = other.GetComponent<MalletHead>();
            MalletHeadEnter(malletHead);
        }

        private void MalletHeadEnter(MalletHead malletHead)
        {
            if (malletHead == null) return;
            if (_malletHead != null) MalletHeadExit(_malletHead);

            _malletHead = malletHead;
            
            _isMalletTouching = true;
            OnMalletEnter?.Invoke(this, _malletHead);

            if (!isContinuousVelocity)
            {
                var vel = _malletHead.ignoreVelocityProbe 
                    ? VelFromPosition()
                    : VelFromVelocityProbe();
                
                // Debug.Log($"{vel} {_malletHead.ignoreVelocityProbe}");
                
                _velCurrent = vel;
                NoteOn(vel);
                _malletHead.Vibe(vel);
                LightUp(vel);
            }
        }

        
        
        private void OnTriggerExit(Collider other)
        {
            var mallet = other.GetComponent<MalletHead>();
            MalletHeadExit(mallet);
        }

        private void MalletHeadExit(MalletHead malletHead)
        {
            if (malletHead == null) return;
            if (_malletHead == null) return;
            if (_malletHead.GetInstanceID() != malletHead.GetInstanceID()) return;
            
            if (isContinuousVelocity)
            {
                LightSet(0);
                _malletHead.VibeOff();
            }
            else
            {
                malletHead.Vibe(0.025f);
            }
            
            NoteOff();
            OnMalletHeadExit?.Invoke(this, _malletHead);
            _isMalletTouching = false;
            _malletHead = null;
            _velCurrent = 0;
        }

        private void NoteOn(float vel)
        {
            if (!isOscBypassed) // Pianoteq
            {
                switch (Note)
                {
                    case NoteEDO _:
                    case NoteUnpitchedMidi _:
                        _noteAndVel.Set(0, Note.MidiNum());
                        _noteAndVel.Set(1, Mathf.FloorToInt(vel * 127f));
                        _oscManager.OscOutMaxMsp.Send(_noteAndVel);
                        break;
                    default:
                        Debug.LogWarning($"unimplemented JI note {Note.LabelPitch()}");
                        break;
                }
            }
            else // XR Organ
            {
                NoteEvent?.Invoke(noteIndex, Note, vel);
                _velCurrent = vel;
            }
        }
        
        private void NoteOff()
        {
            if (!isOscBypassed) // Pianoteq
            {
                switch (Note)
                {
                    case NoteEDO _:
                    case NoteUnpitchedMidi _:
                        _noteAndVel.Set(0, Note.MidiNum());
                        _noteAndVel.Set(1, 0);
                        _oscManager.OscOutMaxMsp.Send(_noteAndVel);
                        break;
                    default:
                        Debug.LogWarning($"unimplemented JI note {Note.LabelPitch()}");
                        break;
                }
            }
            else // XR Organ
            {
                NoteEvent?.Invoke(noteIndex, Note, 0);
            }
        }

        #region View

        private void SetHue(int colorName, INote note)
        {
            // Color.RGBToHSV(_material.GetColor(colorName), out _, out var sat, out var val);
            var hue = note.NoteType == NoteType.EDO
                ? ((note.MidiNum() * 5) % 12) / 12d
                : note.NoteType == NoteType.Function
                    ? 0
                    : note.AngleOnCircle() / 360d;

            var sat = note.NoteType == NoteType.Function ? 0 : 1d;
            var lum = note.NoteType == NoteType.Function
                ? 0.7d
                : 0.5d + (note.NoteType == NoteType.Irrational ? 0 : note.OctaveRelativeToDiapasonFloat() * .1d);

            var color = OkColor.OkHslToSrgb((hue + 0.08333d /*+30deg for red*/, sat, lum));
          
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(colorName, OkUnityBridge.RgbToColor(color));
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void UpdateView(INote note)
        {
            bool isOctaveDisplayed;
            if (note.NoteType == NoteType.EDO)
                isOctaveDisplayed = true;
            else if (note.NoteType == NoteType.Function)
                isOctaveDisplayed = false;
            else if (note.NoteType == NoteType.Irrational)
                isOctaveDisplayed = false;
            else if (note.NoteType == NoteType.JI)
                isOctaveDisplayed = true;
            else if (note.NoteType == NoteType.RawFreq)
                isOctaveDisplayed = false;
            else if (note.NoteType == NoteType.UnpitchedMidi)
                isOctaveDisplayed = false;
            else
                throw new ArgumentOutOfRangeException();

            pitchClassLabel.text = note.LabelPitchClass();
            var oct = isOctaveDisplayed ? note.OctaveRelativeToDiapason() : 0;
            var carets = string.Concat(Enumerable.Repeat("^", Math.Abs(oct)));
            sup.text = oct > 0 ? carets : "";
            sub.text = oct < 0 ? carets : "";

            if (!retainMainColor) SetHue(MatColor, note);
            SetHue(MatHoverColorOverride, note);
            SetHue(MatRimColor, note);
        }
        public void LightUp(float velocity)
        {
            // https://answers.unity.com/questions/300864/how-to-stop-a-co-routine-in-c-instantly.html
            if (_lightUpCoroutine != null) StopCoroutine(_lightUpCoroutine);
            _lightUpCoroutine = StartCoroutine(LightUpCoroutine(velocity));
        }

        public void SetMaterial(int index)
        {
            var viz = visual ?? transform.Find("Visual") ?? transform; // prefab has Visual child but some NotePads still do not
            var m = materials[index];
            if (!m)
            {
                Debug.LogWarning($"No material for Tonnegg with index {index}");
                return;
            }
            
            viz.GetComponent<MeshRenderer>().material = m;
            UpdateView(Note);
        }

        private IEnumerator LightUpCoroutine(float vel)
        {
            const float duration = 5f;
            var timeout = /*vel * */duration;
            while (true)
            {
                yield return null;
                LightSet(Mathf.Lerp(0, vel, timeout / duration));

                if (timeout <= 0f)
                {
                    LightSet(0);
                    break;
                }

                timeout -= Time.deltaTime;
            }
        }

        const float LightRimPower = 8f;

        private void LightSet(float val)
        {
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(MatRimPower, Mathf.Lerp(LightRimPower, 1f, val * velMultLight));
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        #endregion

        #region IMixedRealityFocusHandler

        private Handedness _focusHandedness;
        private bool _isFocusedWithController;

        public void OnFocusEnter(FocusEventData eventData)
        {
            _focusHandedness = eventData.Pointer.Controller.ControllerHandedness;
            _isFocusedWithController = eventData.Pointer.Controller.InputSource.SourceType == InputSourceType.Controller;
        }

        public void OnFocusExit(FocusEventData eventData)
        {
            _focusHandedness = Handedness.None;
            _isFocusedWithController = false;
        }

        #endregion

        #region IReactComponent

        public struct State
        {
            [JsonConverter(typeof(NoteJsonConverter))]
            public INote Note;
            
            public SpiralNotePicker.State NotePicker;
            public string Instrument;
        }

        private string _instrument;
        
        public GameObject GameObject => gameObject;

        public State CurrentState => new State()
        {
            Note = Note,
            Instrument = _instrument,
        };

        public int ComponentId { get; set; }
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            _instrument = initialState.Instrument;

            if (SpiralOrganController.OrganControllers.TryGetValue(initialState.Instrument, out var organController))
            {
                _connectedOrganController = organController;
                _connectedOrganController.AddNote(gameObject);
                var materialIndex = initialState.Instrument == "/organ2" ? 1 : 0;
                SetMaterial(materialIndex);
            } else if (initialState.Instrument == "/fn")
            {
                isOscBypassed = true;
                isContinuousVelocity = false;
                NoteEvent += (index, note, vel) =>
                {
                    if (vel > 0) ComponentCollection.SendFn(((NoteFunction)note).Val);
                };
            } else // /cr78 or /808
            {
                oscAddr = initialState.Instrument;
                _noteAndVel = new OscMessage($"{oscAddr}/note");
                var materialIndex = initialState.Instrument == "/cr78" ? 2 : 3;
                SetMaterial(materialIndex);
                
                // TODO: State.NotePicker
                notePicker.isUnpitched = true;
                notePicker.unpitchedNoteCollection = initialState.Instrument == "/cr78"
                    ? UnpitchedNoteCollection.CR78
                    : UnpitchedNoteCollection.TR808;
                notePicker.ForceRefreshData();
            }
            
            ToggleNotePicker(false); // TODO: add to state
            
            UpdateFromState(initialState);

        }

        private SpiralOrganController _connectedOrganController;

        public void Unmount()
        {
            if (_connectedOrganController)
            {
                _connectedOrganController.RemoveNote(gameObject);
            }
        }

        public void UpdateFromState(State nextState)
        {
            if (_instrument != nextState.Instrument)
            {
                // TODO: 
                // unsubscribe listeners
                // subscribe new listeners
            }
            
            notePicker.SetAngleFromNote(nextState.Note, true);
            SetNote(nextState.Note);
        }
        
        #endregion
        
        #region Cloneable
        private NoteJI _noteWhileCloneSelector = new NoteJI(1, 1);

        private void CloneableSetup()
        {
            var cloneable = GetComponent<CloneableTonnegg>();
            if (!cloneable) return;
            cloneable.onThumbstickAngleMagnitude += OnThumbstickAngleMagnitude;
            cloneable.onClone += OnClone;
        }

        private void OnClone(IReactComponent<State> reactComponent)
        {
            _isThumbstickSelectorDisabled = true;
            // Debug.Log($"original {cloneEvent.original.GetComponent<NotePad>().Note.LabelPitch()} -> {_noteWhileCloneSelector.LabelPitch()}  clone {cloneEvent.clone.GetComponent<NotePad>().Note.LabelPitch()}");
        
            // cloneEvent.clone.GetComponent<Tonnegg>().SetNote(Note);
            // var clonedNotepad = cloneEvent.clone.GetComponent<NotePad>().SetNote(Note);
        
            // TODO: implement for EDO
            if (Note.NoteType == NoteType.JI)
            {
                notePicker.SetAngleFromNote(_noteWhileCloneSelector, true);
                SetNote(_noteWhileCloneSelector);
                _noteWhileCloneSelector = (NoteJI)Note;
            }
        }

        private bool _isThumbstickSelectorDisabled;

        private void OnThumbstickAngleMagnitude(float angle, float magnitude)
        {
            // TODO: implement for EDO
            if (Note.NoteType != NoteType.JI) return;
            var noteJi = (NoteJI)Note;

            if (_isThumbstickSelectorDisabled && magnitude >= .1f) return;

            if (magnitude < .1f)
            {
                _isThumbstickSelectorDisabled = false;
                _noteWhileCloneSelector = noteJi;
                UpdateView(_noteWhileCloneSelector);
            }
            else
            {
                var nearestAngle = SpiralControl.NearestAngleOnCircle(AnglesToIntervals.Keys, angle);
                var interval = AnglesToIntervals[nearestAngle];
                _noteWhileCloneSelector = noteJi.Val + interval;
                UpdateView(_noteWhileCloneSelector);
            }
        }
        
        private static readonly Dictionary<float, NoteJI> AnglesToIntervals = new Dictionary<float, NoteJI>()
        {
            { new NoteJI(3, 2).AngleOnCircle(), new NoteJI(3, 2) },
            { new NoteJI(4, 3).AngleOnCircle(), new NoteJI(4, 3) },
            { new NoteJI(5, 3).AngleOnCircle(), new NoteJI(5, 3) },
            { new NoteJI(5, 4).AngleOnCircle(), new NoteJI(5, 4) },
            { new NoteJI(6, 5).AngleOnCircle(), new NoteJI(6, 5) },
            { new NoteJI(9, 8).AngleOnCircle(), new NoteJI(9, 8) },
            { new NoteJI(15, 8).AngleOnCircle(), new NoteJI(15, 8) },
        };

        #endregion

        #region Dockable
        
        private DockableCustom _dockable;

        private void DockableSetup()
        {
            _dockable = GetComponent<DockableCustom>();
            if (_dockable)
            {
                _dockable.OnDocked += OnDocked;
                _dockable.OnDockHover += OnDockHover;
                _dockable.OnDockLeave += OnDockLeave;
                _dockable.OnUndocked += OnUndocked;
            }
        }
        
        private void OnDockLeave(DockPositionCustom obj)
        {
            Debug.Log("DockLeave");
            SetNote(_noteWhileHoverDock);
        }

        private void OnDockHover(DockPositionCustom dockPosition)
        {
            Debug.Log("DockHover");
            _noteWhileHoverDock = Note;
            if (dockPosition.Note != null) SetNote(dockPosition.Note);
        }

        private void OnUndocked()
        {
            Debug.Log("Undocked");
            SetNote(_noteWhileHoverDock);
        }

        private void OnDocked(DockPositionCustom dockPosition)
        {
            Debug.Log("Docked");
            if (dockPosition.Note != null) SetNote(dockPosition.Note);
        }

        private void DockableCleanup()
        {
            if (_dockable)
            {
                _dockable.OnDocked -= OnDocked;
                _dockable.OnUndocked -= OnUndocked;
            }
        }
        
        #endregion
    }
}