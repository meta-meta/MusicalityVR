using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Musicality
{
    public class MidiIn : MonoBehaviour
    {
        #region Private members
        
            MidiProbe _probe;
            List<MidiInPort> _ports = new List<MidiInPort>();


            private MidiInPort _beatClock;
            private int _pulseCount;

            [SerializeField] private Transform _wheel;
            [SerializeField] private Tonnegg tonnegg;
            
            

            // Does the port seem real or not?
            // This is mainly used on Linux (ALSA) to filter automatically generated
            // virtual ports.
            bool IsRealPort(string name)
            {
                return !name.Contains("Through") && !name.Contains("RtMidi");
            }

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
                                OnClockChange();
                            },
                            OnClockPulse = () =>
                            {
                                ++_pulseCount;
                                OnClockChange();
                            }
                        };
                    }
        
                    // _ports.Add(IsRealPort(name) ? new MidiInPort(i)
                    //     {
                    //         OnNoteOn = (byte channel, byte note, byte velocity) =>
                    //             Debug.Log(string.Format("{0} [{1}] On {2} ({3})", name, channel, note, velocity)),
                    //
                    //         OnNoteOff = (byte channel, byte note) =>
                    //             Debug.Log(string.Format("{0} [{1}] Off {2}", name, channel, note)),
                    //
                    //         OnControlChange = (byte channel, byte number, byte value) =>
                    //             Debug.Log(string.Format("{0} [{1}] CC {2} ({3})", name, channel, number, value))
                    //     } : null
                    // );
                }
            }

            void OnClockChange()
            {
                var quarterNote = 360f / 24; // 1 rev per quarter note at 1:1
                var degPerPulse = tonnegg.Note.Freq(quarterNote);
                // Debug.Log($"{degPerPulse} {_pulseCount}");
                _wheel.localRotation = Quaternion.Euler(0, degPerPulse * _pulseCount, 0);
            }
        
            // Close and release all the opened ports.
            void DisposePorts()
            {
                _beatClock?.Dispose();
                foreach (var p in _ports) p?.Dispose();
                _ports.Clear();
            }
        
            #endregion
        
            #region MonoBehaviour implementation
        
            IEnumerator Start()
            {
                tonnegg.SetNote(new NoteJI(1, 1));
                
                _probe = new MidiProbe(MidiProbe.Mode.In);
                yield return new WaitForSeconds(0.1f);

                ScanPorts();
            }
        
            void Update()
            {
                // // Rescan when the number of ports changed.
                // if (_ports.Count != _probe.PortCount)
                // {
                //     DisposePorts();
                //     ScanPorts();
                // }
                //
                // // Process queued messages in the opened ports.
                // foreach (var p in _ports) p?.ProcessMessages();
                
                // _beatClock?.ProcessMessages();
            }

            private void FixedUpdate()
            {
                _beatClock?.ProcessMessages();
            }

            void OnDestroy()
            {
                _probe?.Dispose();
                DisposePorts();
            }
        
            #endregion
    }
}