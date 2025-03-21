using System;
using UnityEngine;

public class AudioConfig : MonoBehaviour
{
    [SerializeField] private int dspBufferSize = 128;

    private void Awake()
    {
        // SetBufferSize();
    }

    private void OnValidate()
    {
        // if (!Application.isPlaying) return;
        // SetBufferSize();
    }

    private void SetBufferSize()
    {
        // AudioSettings.SetDSPBufferSize();
        var conf = AudioSettings.GetConfiguration();
        var success = AudioSettings.Reset(new AudioConfiguration()
        {
            dspBufferSize = dspBufferSize,
            numRealVoices = conf.numRealVoices,
            numVirtualVoices = conf.numVirtualVoices,
            sampleRate = conf.sampleRate,
            speakerMode = conf.speakerMode,
        });

        if (success)
        {
            Debug.Log(
                $@"Set audio settings success:
 dspBufferSize: {conf.dspBufferSize}
 numRealVoices: {conf.numRealVoices}
 numVirtualVoices: {conf.numVirtualVoices}
 sampleRate: {conf.sampleRate}
 speakerMode: {conf.speakerMode}");
        }
        else
        {
            Debug.LogWarning($"AudioSettings.Reset failed. dspBufferSize: {conf.dspBufferSize}");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}