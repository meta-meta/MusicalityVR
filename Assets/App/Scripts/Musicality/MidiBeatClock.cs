using System;
using System.Collections;
using UnityEngine;

namespace Musicality
{
    public class MidiBeatClock : MonoBehaviour
    {
        MidiProbe _probe;
        private MidiInPort _beatClock;
        private static int _pulseCount;
        public static Action<int> OnClockPulse;
        public static int PulseCount => _pulseCount;

        // Scan and open all the available output ports.
        void ScanPorts()
        {
            for (var i = 0; i < _probe.PortCount; i++)
            {
                var name = _probe.GetPortName(i);
                Debug.Log("MIDI-in port found: " + name);

                if (name.StartsWith("BeatClock"))
                {
                    Debug.Log("Adding: " + name);

                    _beatClock = new MidiInPort(i)
                    {
                        OnSPP = pulseCount =>
                        {
                            _pulseCount = pulseCount;
                            OnClockPulse?.Invoke(_pulseCount);
                        },
                        OnClockPulse = () =>
                        {
                            ++_pulseCount;
                        }
                    };
                }
            }
        }

        IEnumerator Start()
        {
            _probe = new MidiProbe(MidiProbe.Mode.In);
            yield return new WaitForSeconds(0.1f);
            ScanPorts();
        }

        private void FixedUpdate()
        {
            _beatClock?.ProcessMessages();
            OnClockPulse?.Invoke(_pulseCount); // was in OnClockPulse, but not need to spam several invocations in 1 FixedUpdate
        }

        void OnDestroy()
        {
            _probe?.Dispose();
            _beatClock?.Dispose();
        }
    }
}