using MixedReality.Toolkit.SpatialManipulation;
using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class BirdSynth : MonoBehaviour
    {
        [SerializeField] private Orbital pointerA;
        [SerializeField] private Orbital pointerB;
        [SerializeField] private XyzBox boxA;
        [SerializeField] private XyzBox boxB;
        private OscBundle _oscBundle;
        private OscMessage _aGrip;
        private OscMessage _aTrigger;
        private OscMessage _aX;
        private OscMessage _aY;
        private OscMessage _aZ;
        private OscMessage _bGrip;
        private OscMessage _bTrigger;
        private OscMessage _bX;
        private OscMessage _bY;
        private OscMessage _bZ;
        private OscOut _oscOutMax;
        private const string Addr = "/birdSynth";
        private bool _isOn;
        public bool isOn;

        private void Start()
        {
            _oscOutMax = GameObject.Find("OSC").GetComponent<OscManager>().OscOutMaxMsp;
            _aGrip = new OscMessage($"{Addr}/a/grip");
            _aTrigger = new OscMessage($"{Addr}/a/trigger");
            _aX = new OscMessage($"{Addr}/a/x");
            _aY = new OscMessage($"{Addr}/a/y");
            _aZ = new OscMessage($"{Addr}/a/z");
            _bGrip = new OscMessage($"{Addr}/b/grip");
            _bTrigger = new OscMessage($"{Addr}/b/trigger");
            _bX = new OscMessage($"{Addr}/b/x");
            _bY = new OscMessage($"{Addr}/b/y");
            _bZ = new OscMessage($"{Addr}/b/z");
            _oscBundle = new OscBundle();
            _oscBundle.Add(_aGrip);
            _oscBundle.Add(_aTrigger);
            _oscBundle.Add(_aX);
            _oscBundle.Add(_aY);
            _oscBundle.Add(_aZ);
            _oscBundle.Add(_bGrip);
            _oscBundle.Add(_bTrigger);
            _oscBundle.Add(_bX);
            _oscBundle.Add(_bY);
            _oscBundle.Add(_bZ);

            boxA.PosUpdate += v3 =>
            {
                _aX.Set(0, v3.x);
                _aY.Set(0, v3.y);
                _aZ.Set(0, v3.z);
            };
            
            boxB.PosUpdate += v3 =>
            {
                _bX.Set(0, v3.x);
                _bY.Set(0, v3.y);
                _bZ.Set(0, v3.z);
            };
        }

        private Vector2 _aThumbstickPrev;
        private Vector2 _bThumbstickPrev;

        private void Update()
        {
            if (isOn)
            {
                if (!_isOn)
                {
                    
                }

                var aThumbstick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
                var bThumbstick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
                var aThumVel = (aThumbstick - _aThumbstickPrev) / Time.deltaTime;
                var bThumVel = (bThumbstick - _bThumbstickPrev) / Time.deltaTime;
                // TODO add impulse to pointer on spring
                
                
                _aGrip.Set(0, OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger));
                _aTrigger.Set(0, OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger));
                _bGrip.Set(0, OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger));
                _bTrigger.Set(0, OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger));
                
                _oscOutMax.Send(_oscBundle);
            }
            else
            {
                if (_isOn)
                {
                    
                }
            }

            pointerA.gameObject.SetActive(isOn);
            pointerB.gameObject.SetActive(isOn);
            _isOn = isOn;
        }
    }
}