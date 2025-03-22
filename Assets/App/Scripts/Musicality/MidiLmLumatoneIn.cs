using System;
using System.Collections;
using UnityEngine;

namespace Musicality
{
    public class MidiLmLumatoneIn : MonoBehaviour
    {
        MidiProbe _probe;
        private MidiInPort _midiInPort;
        private int _pulseCount;
        public static Action<byte, byte, byte> NoteOn; // channel, note, vel
        public static Action<byte, byte> NoteOff;  // channel, note
        public static Action<byte, byte, byte> CC;  // channel, note

        private bool _isReady;
        
        // Scan and open all the available output ports.
        void ScanPorts()
        {
            for (var i = 0; i < _probe.PortCount; i++)
            {
                var name = _probe.GetPortName(i);
                Debug.Log("[lumatone] MIDI-in port found: " + name);

                if (name.StartsWith("LM-Lumatone-in"))
                {
                    Debug.Log("Adding: " + name);

                    _midiInPort = new MidiInPort(i)
                    {
                        OnNoteOn = (byte channel, byte note, byte velocity) =>
                        {
                            NoteOn?.Invoke(channel, note, velocity);
                            // Debug.Log(string.Format("{0} [{1}] On {2} ({3})", name, channel, note, velocity));
                        },

                        OnNoteOff = (byte channel, byte note) =>
                        {
                            NoteOff?.Invoke(channel, note);

                            // Debug.Log(string.Format("{0} [{1}] Off {2}", name, channel, note));
                        },

                        OnControlChange = (byte channel, byte number, byte value) =>
                        {
                            CC?.Invoke(channel, number, value);
                            // Debug.Log(string.Format("{0} [{1}] CC {2} ({3})", name, channel, number, value));
                        }
                    };
                    _isReady = true;
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
            if (!_isReady) return;
            _midiInPort.ProcessMessages();
        }

        void OnDestroy()
        {
            _probe?.Dispose();
            _midiInPort?.Dispose();
        }
    }
}