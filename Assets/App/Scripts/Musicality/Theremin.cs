using MixedReality.Toolkit.SpatialManipulation;
using OscSimpl;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Musicality
{
    public class Theremin : MonoBehaviour
    {
        [SerializeField] private FloatPicker dutyCyclePicker;
        [SerializeField] private FloatPicker freqDistMaxPicker;
        [SerializeField] private FloatPicker freqDistMinPicker;
        [SerializeField] private FloatPicker noisePicker;
        [SerializeField] private FloatPicker voicePicker;
        [SerializeField] private XRSimpleInteractable powerSwitch;
        [SerializeField] private Transform ampObj;
        [SerializeField] private Transform freqObj;
        [SerializeField] private Transform freqObjBot;
        [SerializeField] private Transform freqObjTop;
        [SerializeField] private Transform pointerAmp;
        [SerializeField] private Transform pointerFreq;
        [SerializeField] private float ampDistMax = 0.5f;
        [SerializeField] private float ampDistMin = 0.1f;
        [SerializeField] private float freqDistMax = 1.5f;
        private OVRInput.Controller _controllerLeft;
        private OVRInput.Controller _controllerRight;
        private ObjectManipulator _objManipulator;
        private OscBundle _oscBundle;
        private OscMessage _aModD;
        private OscMessage _aModW;
        private OscMessage _amp;
        private OscMessage _click;
        private OscMessage _dutyCycle;
        private OscMessage _fModD;
        private OscMessage _fModW;
        private OscMessage _freq;
        private OscMessage _noise;
        private OscMessage _priX;
        private OscMessage _priY;
        private OscMessage _secX;
        private OscMessage _secY;
        private OscMessage _voice;
        private OscOut _oscOutMax;
        private Vector2 _priThumb;
        private Vector2 _secThumb;
        private const string Addr = "/theremin";
        private float _frequency;
        private float freqAtDistMax => freqDistMaxPicker.Val;
        private float freqAtDistMin => freqDistMinPicker.Val;
        private static readonly int MatRimPower = Shader.PropertyToID("_RimPower");
        public Vector3 FreqBotPos => freqObjBot.position;
        public Vector3 FreqPointerPos => pointerFreq.position;
        public Vector3 FreqTopPos => freqObjTop.position;
        public bool isOn;
        public float FreqAtDistMax => freqAtDistMax;
        public float FreqAtDistMin => freqAtDistMin;
        public float FreqDistMax => freqDistMax;
        public float Frequency => _frequency;

        // Use this for initialization
        void Start()
        {
            _oscOutMax = GameObject.Find("OSC").GetComponent<OscManager>().OscOutMaxMsp;

        
            _freq = new OscMessage($"{Addr}/freq");
            _amp = new OscMessage($"{Addr}/amp");
            _priX = new OscMessage($"{Addr}/pri/x");
            _priY = new OscMessage($"{Addr}/pri/y");
            _secX = new OscMessage($"{Addr}/sec/x");
            _secY = new OscMessage($"{Addr}/sec/y");
            _click = new OscMessage($"{Addr}/click");
            _click.Set(0, 1);
            _noise = new OscMessage($"{Addr}/noise").Set(0, noisePicker.Val);
            _voice = new OscMessage($"{Addr}/voice").Set(0, voicePicker.Val);
            _dutyCycle = new OscMessage($"{Addr}/dutyCycle").Set(0, dutyCyclePicker.Val);
            _fModW = new OscMessage($"{Addr}/fModW").Set(0, 0);
            _fModD = new OscMessage($"{Addr}/fModD").Set(0, 0);
            _aModW = new OscMessage($"{Addr}/aModW").Set(0, 0);
            _aModD = new OscMessage($"{Addr}/aModD").Set(0, 0);

            _oscBundle = new OscBundle();
            // _oscBundle.Add(_freq);
            // _oscBundle.Add(_amp);
            // _oscBundle.Add(_priX);
            // _oscBundle.Add(_priY);
            // _oscBundle.Add(_secX);
            // _oscBundle.Add(_secY);
            // _oscBundle.Add(_noise);
            // _oscBundle.Add(_voice);

            _controllerLeft = OVRInput.Controller.LTouch;
            _controllerRight = OVRInput.Controller.RTouch;

            _objManipulator = GetComponent<ObjectManipulator>();
        
            powerSwitch.selectEntered.AddListener(OnPowerButtonClick);

            dutyCyclePicker.OnChange += f =>
            {
                _dutyCycle.Set(0, Mathf.Clamp01(f));
                _oscOutMax.Send(_dutyCycle);
            };
            
            noisePicker.OnChange += f =>
            {
                _noise.Set(0, Mathf.Clamp01(f));
                _oscOutMax.Send(_noise);
            };
            
            voicePicker.OnChange += f =>
            {
                _voice.Set(0, Mathf.Clamp01(f));
                _oscOutMax.Send(_voice);
            };
        }

        private void OnPowerButtonClick(SelectEnterEventArgs args)
        {
            isOn = !isOn;
        }

        private static float Dist(Transform t1, Transform t2) =>
            Vector3.Distance(t1.position, t2.position);

        private void Toggle()
        {
            wasOn = isOn;
        
            _amp.Set(0, 0);
            _noise.Set(0, isOn ? 0.1f : 0);
            _voice.Set(0, isOn ? 1 : 0);
            _oscOutMax.Send(_amp);
            _oscOutMax.Send(_noise);
            _oscOutMax.Send(_voice);
        
            ampObj.GetComponent<MeshRenderer>().material.SetFloat(MatRimPower, isOn ? 0.5f : 8);
            freqObj.GetComponent<MeshRenderer>().material.SetFloat(MatRimPower, isOn ? 0.5f : 8);
            powerSwitch.GetComponentInChildren<MeshRenderer>().material.SetFloat(MatRimPower, isOn ? 0.5f : 8);
            powerSwitch.transform.localRotation = Quaternion.Euler(0, 0, isOn ? 180 : 0);
        }

        /// <summary>
        ///   <para>Project point onto a line.</para>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector3 = lineEnd - lineStart;
            float magnitude = vector3.magnitude;
            Vector3 lhs = vector3;
            if ((double) magnitude > 9.999999974752427E-07)
                lhs /= magnitude;
            float num = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0.0f, magnitude);
            return lineStart + lhs * num;
        }

        /// <summary>
        ///   <para>Calculate distance between a point and a line.</para>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            // HandleUtility not available in build
            // return Vector3.Magnitude(HandleUtility.ProjectPointLine(point, lineStart, lineEnd) - point);
            return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
        }
        
        public float FreqToDist(float freq)
        {
            var from = Mathf.Log(freqAtDistMin, 2);
            var to = Mathf.Log(freqAtDistMax, 2);
            return Mathf.InverseLerp(from, to, Mathf.Log(freq, 2)) * freqDistMax;
        }

        private bool wasOn;
        // Update is called once per frame
        void Update() // FixedUpdate doesn't seem to matter unless there's some way to get ControllerPosition faster than Update frame
        {
            if (wasOn != isOn)
                Toggle();

            if (isOn)
            {
                // TODO: FixedUpdate version of Orbital
                // var fPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                
                // HandleUtility not available in build
                // var dist = HandleUtility.DistancePointLine(FreqPointerPos, FreqBotPos, FreqTopPos);
                var dist = DistancePointLine(FreqPointerPos, FreqBotPos, FreqTopPos);

                var from = Mathf.Log(freqAtDistMin, 2);
                var to = Mathf.Log(freqAtDistMax, 2);
                
                _frequency = Mathf.Pow(2, Mathf.Lerp(from, to, dist / freqDistMax));
            // TODO: inverse: freq -> distance to find position of octaves

                // Debug.Log($"dist {dist}  freq {_frequency}  d {d}");
            
                _freq.Set(0, _frequency);

                var distAmp = Dist(ampObj, pointerAmp);

                _amp.Set(0, Mathf.Pow(Mathf.Clamp01((distAmp - ampDistMin) / (ampDistMax - ampDistMin)), 4)); // https://www.dr-lex.be/info-stuff/volumecontrols.html#:~:text=The%20solution%20to%20implementing%20a,what%20we%20want(2).

                _priThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _controllerLeft);
                _secThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _controllerRight);
                _priX.Set(0, _priThumb.x);
                _priY.Set(0, _priThumb.y);
                _secX.Set(0, _secThumb.x);
                _secY.Set(0, _secThumb.y);
                // noise.Set(0, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerLeft));
                // voice.Set(0, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerRight));

                var lGrip = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger); 
                var lIxCurl = 1f - OVRInput.Get(OVRInput.RawAxis1D.LIndexTriggerCurl); // Q3 index near trigger 1.0 when touching
                var rGrip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger); 
                var rIxCurl = 1f - OVRInput.Get(OVRInput.RawAxis1D.RIndexTriggerCurl); // Q3 index near trigger 1.0 when touching

                var fModW = 8f * Mathf.Pow(rIxCurl, 1);
                var fModD = Mathf.Log(_frequency, 2) * Mathf.Pow(rGrip, 2);
                
                _aModD.Set(0, Mathf.Pow(lGrip, 2));
                _aModW.Set(0, 1f + 8f * Mathf.Pow(lIxCurl, 1f ));
                _fModD.Set(0, fModD);
                _fModW.Set(0, fModW);
                _oscOutMax.Send(_aModD);
                _oscOutMax.Send(_aModW);
                _oscOutMax.Send(_fModD);
                _oscOutMax.Send(_fModW);
                
                
                _oscOutMax.Send(_freq);
                _oscOutMax.Send(_amp);
                _oscOutMax.Send(_priX);
                _oscOutMax.Send(_priY);
                _oscOutMax.Send(_secX);
                _oscOutMax.Send(_secY);
                _oscOutMax.Send(_noise);
                _oscOutMax.Send(_voice);
                // _oscOutMax.Send(_oscBundle);

            
                if (OVRInput.GetDown(OVRInput.Button.One, _controllerLeft)) _oscOutMax.Send(_click);
            }
        
            // if (GetComponent<Manipulate>().IsTouching && (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            //                                               OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)))
            // {
        
            // }
        }
    }
}