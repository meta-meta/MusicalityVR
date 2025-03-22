using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musicality
{
    public class MidiBeatClockWheel : MonoBehaviour, IEnvelopeFloat, IReactComponent<MidiBeatClockWheel.State>
    {
        [SerializeField] private GameObject malletBallPrefab;
        [SerializeField] private GameObject pulseEmptyPrefab;
        [SerializeField] private Tonnegg tonneggBeats;
        [SerializeField] private Tonnegg tonneggClockRatio;
        [SerializeField] private Tonnegg tonneggPulses;
        [SerializeField] private Transform rotationContainer;
        [SerializeField] private Transform wheel;
        [SerializeField] private int beats = 1;
        [SerializeField] private int pulses = 12;
        private NoteUnpitchedMidi BeatsNote => (NoteUnpitchedMidi)tonneggBeats.Note;
        private NoteUnpitchedMidi PulsesNote => (NoteUnpitchedMidi)tonneggBeats.Note;
        private Rigidbody _rigidbody;
        private float _envelope = 1;
        private readonly List<GameObject> _malletBallPool = new List<GameObject>();
        private readonly List<GameObject> _pulseEmptyPool = new List<GameObject>();
        public float GetEnvelopeFloat() => _envelope;

        public struct State
        {
            public NoteJI ClockRatio;
            public int Beats;
            public int Pulses;

            public static State Default => new State()
            {
                Beats = 4,
                ClockRatio = new NoteJI(1, 1),
                Pulses = 12,
            };
        }

        private void OnEnable()
        {
            Init(State.Default);
        }

        #region IReactComponent
        
        private State _initialState;

        public GameObject GameObject => gameObject;

        public State CurrentState =>
            new State()
            {
                Beats = beats,
                ClockRatio = (NoteJI)tonneggClockRatio.Note,
                Pulses = pulses
            };

        int IReactComponent<State>.ComponentId { get; set; }
        
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            _initialState = initialState;
            Invoke(nameof(Init), 0.1f); // TODO: correct this hack to ensure that BeatsNote and PulsesNote are UnpitchedMidi. We have to wait for SpiralNotePicker to set that up
        }

        public void Unmount()
        {
        }

        public void UpdateFromState(State nextState) =>
            UpdateFromState(nextState.Beats, nextState.Pulses, nextState.ClockRatio);

        private bool _isInit;
        private void Init()
        {
            if (_isInit)
            {
                Debug.LogWarning("already called Init");
                return;
            }
            
            UpdateFromState(_initialState.Beats, _initialState.Pulses, _initialState.ClockRatio);

            MidiBeatClock.OnClockPulse += OnClockPulse;
            _rigidbody = wheel.GetComponent<Rigidbody>();
            tonneggBeats.NoteChanged += OnBeatsOrPulsesChange;
            tonneggPulses.NoteChanged += OnBeatsOrPulsesChange;

            tonneggClockRatio.NoteEvent += (i, n, vel) =>
            {
                _envelope = vel > 0 ? vel : 1; // no envelope if no mallet
            };
        }

        private void UpdateFromState(int nextBeats, int nextPulses, INote nextClockRatio)
        {
            tonneggBeats.SetNote(BeatsNote.GetNoteInSet(nextBeats));
            tonneggPulses.SetNote(PulsesNote.GetNoteInSet(nextPulses));
            SetRhythm();
            tonneggClockRatio.SetNote(nextClockRatio);
        }
        
        #endregion
        
        

        private bool _isDestroying;
        private void OnDestroy()
        {
            _isDestroying = true;
            MidiBeatClock.OnClockPulse -= OnClockPulse;
            tonneggBeats.NoteChanged -= OnBeatsOrPulsesChange;
            tonneggPulses.NoteChanged -= OnBeatsOrPulsesChange;
        }

        private void OnBeatsOrPulsesChange(int noteindex, INote note) => SetRhythm();

        private void SetRhythm()
        {
            beats = tonneggBeats.Note.MidiNum();
            pulses = tonneggPulses.Note.MidiNum();

            if (pulses < beats)
            {
                tonneggBeats.SetNote(new NoteUnpitchedMidi(pulses, pulses.ToString(),
                    ((NoteUnpitchedMidi)tonneggBeats.Note).NoteCollection));
                return;
            }

            var rhy = EuclideanRhythm.BuildRhythm(pulses, beats);

            foreach (var o in _malletBallPool) o.SetActive(false);
            foreach (var o in _pulseEmptyPool) o.SetActive(false);

            for (var i = 0; i < rhy.Count; i++)
            {
                var theta = ((Mathf.PI * 2) / rhy.Count) * i;
                var r = 0.5f;
                var pos = new Vector3(r * Mathf.Sin(theta), 0, r * Mathf.Cos(theta));

                if (rhy[i])
                {
                    var isNew = _malletBallPool.Count <= i;
                    var malletBall = isNew
                        ? Instantiate(malletBallPrefab, wheel)
                        : _malletBallPool[i];
                    if (isNew) _malletBallPool.Add(malletBall);
                        
                    malletBall.SetActive(true);
                    var malletHead = malletBall.GetComponent<MalletHead>();
                    malletHead.GainEnvelope = this;
                    malletHead.ignoreVelocityProbe = true; // for relative positional velocity rather than relying on angular velocity of wheel
                    malletBall.transform.localPosition = pos;
                }
                else
                {
                    var isNew = _pulseEmptyPool.Count <= i;
                    var pulseEmpty = isNew
                        ? Instantiate(pulseEmptyPrefab, wheel)
                        : _pulseEmptyPool[i];
                    if (isNew) _pulseEmptyPool.Add(pulseEmpty);
                        
                    pulseEmpty.SetActive(true);
                    pulseEmpty.transform.localPosition = pos;
                }
            }
        }

        private const float QuarterNote = 360f / (24 * 4); // 1/4 rev per quarter note at 1:1

        private void OnClockPulse(int pulseCount)
        {
            if (_isDestroying)
            {
                // TODO FIXME somehow
                // Debug.Log("destroying. whyy is this getting called?");
                return;
            }
            var degPerPulse = tonneggClockRatio.Note.Freq(QuarterNote);
            _rigidbody.MoveRotation(rotationContainer.rotation * Quaternion.Euler(0, degPerPulse * pulseCount, 0));
        }
    }
}