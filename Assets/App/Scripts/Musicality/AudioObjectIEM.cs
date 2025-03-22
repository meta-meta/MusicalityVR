using System;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

//[ExecuteInEditMode]
namespace Musicality
{
    public class AudioObjectIEM : MonoBehaviour
    {
        [Range(0, 35)] public int color;
        [SerializeField] private RoomEncoderIEM roomEncoder;
        [SerializeField] private TMP_Text nametag;
        [SerializeField] private bool AlwaysSendPosition = true;
        [SerializeField] private int PortIn;
        [SerializeField] private int PortOut;
        private SpiralVolumePicker _volumePicker;
        private Material _material;
        private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator _manipulator;
        private OscIn _oscInRoomEncoder;
        private OscMessage _roomX;
        private OscMessage _roomY;
        private OscMessage _roomZ;
        private OscOut _oscOutRoomEncoder;
        private Vector3 _pos;
        private bool _isGrabbed;
        private static readonly OscMessage SourceX = new OscMessage($"/RoomEncoder/sourceX");
        private static readonly OscMessage SourceY = new OscMessage($"/RoomEncoder/sourceY");
        private static readonly OscMessage SourceZ = new OscMessage($"/RoomEncoder/sourceZ");
        
        
        private static readonly int MatColor = Shader.PropertyToID("_Color");
        private static readonly int MatHoverColorOverride = Shader.PropertyToID("_HoverColorOverride");
        private static readonly int MatRimColor = Shader.PropertyToID("_RimColor");
        private static readonly int MatRimPower = Shader.PropertyToID("_RimPower");

        private MaterialPropertyBlock _materialPropertyBlock;
        private MeshRenderer  _meshRenderer;
    
        private void Awake()
        {
            _manipulator = GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _materialPropertyBlock = new MaterialPropertyBlock();
            _volumePicker = GetComponentInChildren<SpiralVolumePicker>();
        }

        private void OnEnable()
        {
            _manipulator.OnManipulationStarted.AddListener(OnManipulationStart);
            _manipulator.OnManipulationEnded.AddListener(OnManipulationEnd);
            if (_volumePicker) _volumePicker.OnVolumeChange += OnVolumeChange;
        }

        private void OnVolumeChange(float val)
        {
            LightSet(val);
        }

        private void OnManipulationEnd(ManipulationEventData arg0)
        {
            _isGrabbed = false;
        }

        private void OnDisable()
        {
            _manipulator.OnManipulationStarted.RemoveListener(OnManipulationStart);
            _manipulator.OnManipulationEnded.RemoveListener(OnManipulationEnd);
            if (_volumePicker) _volumePicker.OnVolumeChange -= OnVolumeChange;
        }

        private void OnManipulationStart(ManipulationEventData arg0)
        {
            _isGrabbed = true;
        }

        private void Start()
        {
            roomEncoder = FindObjectOfType<RoomEncoderIEM>();
            var ip = GameObject.Find("OSC").GetComponent<OscManager>().RemoteIpAddress;
        
            if (PortOut > 0)
            {
                SetPortOut(PortOut);
            }

            if (PortIn > 0)
            {
                _oscInRoomEncoder = gameObject.AddComponent<OscIn>();
                _oscInRoomEncoder.Open(PortIn);
                _oscInRoomEncoder.MapFloat($"/RoomEncoder/sourceX", OnReceiveSourceX);
                _oscInRoomEncoder.MapFloat($"/RoomEncoder/sourceY", OnReceiveSourceY);
                _oscInRoomEncoder.MapFloat($"/RoomEncoder/sourceZ", OnReceiveSourceZ);
            }
        
            // SetColor(color);

            SendPositionToRoomEncoder();
        }

        public void SetPortOut(int port)
        {
            PortOut = port;
            Debug.Log($"{name} PortOut {PortOut}");
            if (!_oscOutRoomEncoder) _oscOutRoomEncoder = gameObject.AddComponent<OscOut>();
            _oscOutRoomEncoder.Close();
            var ip = GameObject.Find("OSC").GetComponent<OscManager>().RemoteIpAddress;
            _oscOutRoomEncoder.Open(PortOut, ip);
        }

        void OnReceiveSourceX(float val)
        {
            if (!_isGrabbed)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, val);
            }
        }

        void OnReceiveSourceY(float val)
        {
            if (!_isGrabbed)
            {
                transform.position = new Vector3(-val, transform.position.y, transform.position.z);
            }
        }

        void OnReceiveSourceZ(float val)
        {
            if (!_isGrabbed)
            {
                transform.position = new Vector3(transform.position.x, val, transform.position.z);
            }
        }

        private void UpdateNametag()
        {
            nametag.text = gameObject.name;
        }

        private void OnValidate()
        {
            return; // build errors
            _material = Application.isPlaying
                ? GetComponent<MeshRenderer>().material
                : GetComponent<MeshRenderer>().sharedMaterial;
        
            SetColor(color);
            UpdateNametag();
        }

        public void SetColor(int color)
        {
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            Color.RGBToHSV(_materialPropertyBlock.GetColor(MatColor), out _, out var sat, out var val);
            _materialPropertyBlock.SetColor(MatColor, Color.HSVToRGB(color / 36f, sat, val));
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        const float LightRimPower = 8f;

        private void LightSet(float val)
        {
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(MatRimPower, Mathf.Lerp(LightRimPower, 1f, val));
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }


        private void SendPositionToRoomEncoder()
        {
            _pos = roomEncoder.RoomOrigin.InverseTransformPoint(transform.position);
            SourceX.Set(0, _pos.z);
            SourceY.Set(0, -_pos.x);
            SourceZ.Set(0, _pos.y);
            _oscOutRoomEncoder.Send(SourceX);
            _oscOutRoomEncoder.Send(SourceY);
            _oscOutRoomEncoder.Send(SourceZ);
        }

        private void FixedUpdate()
        {
            if (_isGrabbed || AlwaysSendPosition)
            {
                SendPositionToRoomEncoder();
            }
        }
    }
}