using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class FloatToOscOut : MonoBehaviour
    {
        [SerializeField] private string oscAddr;
        [SerializeField] private bool sendValOnStart;
        // [SerializeField] private Action<float> onFloatEvent;
        private OscMessage _ccVal;
        private OscOut _oscOut;
        private SpiralVolumePicker _spiralVolumePicker;

        private void Start()
        {
            _oscOut = GameObject.Find("OSC").GetComponent<OscManager>().OscOutMaxMsp;
            _spiralVolumePicker = GetComponent<SpiralVolumePicker>();
            _spiralVolumePicker.OnVolumeChange += OnVolumeChange;
            _ccVal = new OscMessage(oscAddr);
            
            if (sendValOnStart) OnVolumeChange(_spiralVolumePicker.volume);
        }

        private void OnVolumeChange(float val)
        {
            _ccVal.Set(0, Mathf.FloorToInt(val * 127));
            _oscOut.Send(_ccVal);
        }
    }
}