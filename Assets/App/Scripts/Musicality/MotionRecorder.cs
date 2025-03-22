using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musicality
{
    public class MotionRecorder : MonoBehaviour
    {
        [SerializeField] private bool isRecording = false;
        [SerializeField] private bool loop = true;
        private readonly List<Vector3> _positions = new List<Vector3>();
        private int _pulseCountAtRecStart;

        private void OnEnable()
        {
            MidiBeatClock.OnClockPulse += OnClockPulse;
        }

        private void OnDisable()
        {
            MidiBeatClock.OnClockPulse -= OnClockPulse;
        }

        public void Record()
        {
            isRecording = true;
        }
        
        public void Stop()
        {
            isRecording = false;
        }

        public void Clear()
        {
            _positions.Clear();
        }

        private void OnClockPulse(int pulseCount)
        {
            if (isRecording)
            {
                if (_positions.Count == 0)
                    _pulseCountAtRecStart = pulseCount;
                
                _positions.Add(transform.position);
            }
            else
            {
                var currentIndex = pulseCount - _pulseCountAtRecStart;

                if (loop)
                {
                    // TODO: anything before _pulseCountAtRecStart will play backwards. feature? bug?
                    transform.position = _positions[Math.Abs(currentIndex % _positions.Count)];
                }
                else
                {
                    if (currentIndex >= 0 && currentIndex < _positions.Count)
                    {
                        transform.position = _positions[currentIndex];
                    }
                }
            }
        }
    }
}