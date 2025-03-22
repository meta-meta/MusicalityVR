using UnityEngine;

namespace Musicality
{
    public class BpmReaper : MonoBehaviour
    {
        [SerializeField] private SpiralIrrationalIntervalPicker bpmPicker;
        [SerializeField] private Tonnegg intervalDisplay;
        
        private OscIn _oscInReaper;
        private OscOut _oscOutReaper;
        private static readonly OscMessage FTempoRaw = new OscMessage($"f/tempo/raw");

        private void Start()
        {
            bpmPicker.IntervalChanged += SetBpm;

            var oscManager = FindObjectOfType<OscManager>();
            _oscInReaper = oscManager.OscInReaper;
            _oscOutReaper = oscManager.OscOutReaper;
            
            _oscInReaper.MapFloat("/tempo/raw", OnReaperTempoChanged);
            
            bpmPicker.SetAngleFromNote((NoteIrrational)60);
            intervalDisplay.SetNote((NoteIrrational)60);

        }

        private void OnReaperTempoChanged(float bpm)
        {
            // Debug.Log($"OnReaperTempoChanged: {bpm}");
            // bpmPicker.SetAngleFromNote((NoteIrrational)bpm);
            intervalDisplay.SetNote((NoteIrrational)bpm);
        }

        private void SetBpm(NoteIrrational bpm)
        {
            FTempoRaw.Set(0, bpm.Val);
            _oscOutReaper.Send(FTempoRaw);
        }

        private void OnDestroy()
        {
            bpmPicker.IntervalChanged -= SetBpm;
        }
    }
}