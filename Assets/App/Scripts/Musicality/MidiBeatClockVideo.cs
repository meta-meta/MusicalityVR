using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Musicality
{
    public class MidiBeatClockVideo : MonoBehaviour, IReactComponent<MidiBeatClockVideo.State>
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Tonnegg tonneggClockRatio;

        public struct State
        {
            public NoteJI ClockRatio;
            public string VideoClipPath;

            public static State Default => new State()
            {
                ClockRatio = new NoteJI(1, 1),
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
                ClockRatio = (NoteJI)tonneggClockRatio.Note,
            };

        int IReactComponent<State>.ComponentId { get; set; }

        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            _initialState = initialState;
            Invoke(nameof(Init),
                0.1f); // TODO: correct this hack to ensure that BeatsNote and PulsesNote are UnpitchedMidi. We have to wait for SpiralNotePicker to set that up
        }

        public void Unmount()
        {
        }

        private bool _isInit;

        private void Init()
        {
            if (_isInit)
            {
                Debug.LogWarning("already called Init");
                return;
            }

            UpdateFromState(_initialState);

            MidiBeatClock.OnClockPulse += OnClockPulse;
            videoPlayer.seekCompleted += OnSeekCompleted;

            if (!videoPlayer.isPlaying) videoPlayer.Play();
        }

        private void OnSeekCompleted(VideoPlayer source)
        {
            StartCoroutine(WaitToUpdateRenderTextureBeforeEndingSeek());
        }

        IEnumerator WaitToUpdateRenderTextureBeforeEndingSeek()
        {
            yield return new WaitForEndOfFrame();
            _seekDone = true;
        }

        public void UpdateFromState(State nextState)
        {
            tonneggClockRatio.SetNote(nextState.ClockRatio);
        }

        #endregion


        private bool _isDestroying;

        private void OnDestroy()
        {
            _isDestroying = true;
            MidiBeatClock.OnClockPulse -= OnClockPulse;
            videoPlayer.seekCompleted -= OnSeekCompleted;
        }

        private void OnBeatsOrPulsesChange(int noteindex, INote note)
        {
            Debug.LogWarning("Not implemented yet");
        }

        public float frame;
        private float _frame;
        private const float QuarterNote = 60f / (24 * 4); // 60 per quarter note at 1:1

        private void OnClockPulse(int pulseCount)
        {
            if (_isDestroying)
            {
                // TODO FIXME somehow
                // Debug.Log("destroying. whyy is this getting called?");
                return;
            }

            var framesPerPulse = tonneggClockRatio.Note.Freq(QuarterNote);
            _frame = (pulseCount * framesPerPulse);
        }

        private bool _seekDone = true;

        public bool isPlaying;

        private void Update()
        {
            UpdateVideo();
        }

        private void UpdateVideo()
        {
            if (!videoPlayer.isPrepared)
            {
                Debug.LogWarning("video not prepared");
                return;
            }
            
            // https://discussions.unity.com/t/changing-videoplayer-frame-from-code-doesnt-update-frame-solved/893491/2
            //If you are currently seeking there is no point to seek again.
            if (!isPlaying) return;
            if (!_seekDone) return;

            // You should pause while you seek for better stability
            videoPlayer.Pause();
            videoPlayer.frame = (long)_frame % (long)videoPlayer.frameCount;
            frame = videoPlayer.frame;
            _seekDone = false;
        }
    }
}