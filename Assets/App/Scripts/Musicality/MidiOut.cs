using System.Collections.Generic;
using UnityEngine;

namespace Musicality
{
    public class MidiOut : MonoBehaviour
    {
        MidiProbe _probe;
        List<MidiOutPort> _ports = new List<MidiOutPort>();

        private MidiOutPort ewiOut;

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
                Debug.Log("MIDI-out port found: " + name);
                
                // _ports.Add(IsRealPort(name) ? new MidiOutPort(i) : null);
                if (name.StartsWith("LM-EWI"))
                {
                    Debug.Log("Adding: " + name);
                    ewiOut = new MidiOutPort(i);
                }
            }
        }

        // Close and release all the opened ports.
        void DisposePorts()
        {
            ewiOut?.Dispose();
            foreach (var p in _ports) p?.Dispose();
            _ports.Clear();
        }

        System.Collections.IEnumerator Start()
        {
            _probe = new MidiProbe(MidiProbe.Mode.Out);

            yield return new WaitForSeconds(0.1f);

            // Send an all-sound-off message.
            // foreach (var port in _ports) port?.SendAllOff(0);

            ScanPorts();
            
            // for (var i = 0; true; i++)
            // {
            //     var note = 40 + (i % 30);
            //
            //     Debug.Log("MIDI Out: Note On " + note);
            //     ewiOut?.SendNoteOn(0, note, 100);
            //
            //     yield return new WaitForSeconds(0.1f);
            //
            //     Debug.Log("MIDI Out: Note Off " + note);
            //     ewiOut?.SendNoteOff(0, note);
            //
            //     yield return new WaitForSeconds(0.1f);
            // }
        }

        public void SendCC(int cc, float value)
        {
            ewiOut?.SendControlChange(0, cc, Mathf.RoundToInt(value * 127));
        }

        void Update()
        {
            // Rescan when the number of ports changed.
            // if (_ports.Count != _probe.PortCount)
            // {
            //     DisposePorts();
            //     ScanPorts();
            // }
        }

        void OnDestroy()
        {
            _probe?.Dispose();
            DisposePorts();
        }
    }
}