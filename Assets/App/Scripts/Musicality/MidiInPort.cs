using System;
using RtMidiDll = RtMidi.Unmanaged;

namespace Musicality
{
    unsafe public sealed class MidiInPort : IDisposable
    {
        RtMidiDll.Wrapper* _rtmidi;

        public Action<byte, byte, byte> OnNoteOn;
        public Action<byte, byte> OnNoteOff;
        public Action<byte, byte, byte> OnControlChange;
        public Action OnClockPulse;
        public Action<int> OnSPP;

        public MidiInPort(int portNumber)
        {
            _rtmidi = RtMidiDll.InCreateDefault();
            
            // https://github.com/thestk/rtmidi/blob/master/tests/midiclock.cpp#L57
            RtMidiDll.InIgnoreTypes(_rtmidi, true, false, true);

            if (_rtmidi != null && _rtmidi->ok)
                RtMidiDll.OpenPort(_rtmidi, (uint)portNumber, "RtMidi In");

            if (_rtmidi == null || !_rtmidi->ok)
                throw new InvalidOperationException("Failed to set up a MIDI input port.");
        }

        ~MidiInPort()
        {
            if (_rtmidi == null || !_rtmidi->ok) return;

            RtMidiDll.InFree(_rtmidi);
        }

        public void Dispose()
        {
            if (_rtmidi == null || !_rtmidi->ok) return;

            RtMidiDll.InFree(_rtmidi);
            _rtmidi = null;

            GC.SuppressFinalize(this);
        }

        public void ProcessMessages()
        {
            if (_rtmidi == null || !_rtmidi->ok) return;

            byte* msg = stackalloc byte [32];

            while (true)
            {
                ulong size = 32;
                var stamp = RtMidiDll.InGetMessage(_rtmidi, msg, ref size);
                if (stamp < 0 || size == 0) break;

                var status = (byte)(msg[0] >> 4);
                var channel = (byte)(msg[0] & 0xf);

                // Debug.Log($"{msg[0]}  {msg[1]}  {msg[2]}");
                // https://en.wikipedia.org/wiki/MIDI_beat_clock
                if (msg[0] == 242)
                {
                    // elapsed MIDI beats
                    // 1 MIDI beat = a 16th note = 6 clock pulses (max 16380)
                    OnSPP?.Invoke((msg[1] + msg[2] * 128) * 6);
                }
                else if (msg[0] == 248)
                {
                    OnClockPulse?.Invoke();
                }
                else if (status == 9)
                {
                    if (msg[2] > 0)
                        OnNoteOn?.Invoke(channel, msg[1], msg[2]);
                    else
                        OnNoteOff?.Invoke(channel, msg[1]);
                }
                else if (status == 8)
                {
                    OnNoteOff?.Invoke(channel, msg[1]);
                }
                else if (status == 0xb)
                {
                    OnControlChange?.Invoke(channel, msg[1], msg[2]);
                }
            }
        }
    }
}
