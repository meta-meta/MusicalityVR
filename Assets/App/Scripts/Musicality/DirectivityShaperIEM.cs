using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class DirectivityShaperIEM : MonoBehaviour
    {
        [SerializeField] private Transform ambisonicVisualizer;
        [SerializeField] private Transform band0;
        [SerializeField] private Transform band1;
        [SerializeField] private Transform band2;
        [SerializeField] private Transform band3;
        [SerializeField] private int portOut;
        private OscOut _oscOutDirectivityShaper;
        private RoomEncoderIEM _roomEncoder;
        private static readonly OscMessage ProbeAzimuth = new OscMessage($"/DirectivityShaper/probeAzimuth");
        private static readonly OscMessage ProbeElevation = new OscMessage($"/DirectivityShaper/probeElevation");
        private static readonly OscMessage ProbeRoll = new OscMessage($"/DirectivityShaper/probeRoll");
        private static readonly OscMessage BandAzimuth0 = new OscMessage($"/DirectivityShaper/azimuth0");
        private static readonly OscMessage BandElevation0 = new OscMessage($"/DirectivityShaper/elevation0");
        private static readonly OscMessage BandAzimuth1 = new OscMessage($"/DirectivityShaper/azimuth1");
        private static readonly OscMessage BandElevation1 = new OscMessage($"/DirectivityShaper/elevation1");
        private static readonly OscMessage BandAzimuth2 = new OscMessage($"/DirectivityShaper/azimuth2");
        private static readonly OscMessage BandElevation2 = new OscMessage($"/DirectivityShaper/elevation2");
        private static readonly OscMessage BandAzimuth3 = new OscMessage($"/DirectivityShaper/azimuth3");
        private static readonly OscMessage BandElevation3 = new OscMessage($"/DirectivityShaper/elevation3");

        private void Start()
        {
            _roomEncoder = FindObjectOfType<RoomEncoderIEM>();

            if (portOut > 0)
            {
                SetPortOut(portOut);
            }
        }

        public void SetPortOut(int port)
        {
            portOut = port;
            Debug.Log($"{name} PortOut {portOut}");
            if (!_oscOutDirectivityShaper) _oscOutDirectivityShaper = gameObject.AddComponent<OscOut>();
            _oscOutDirectivityShaper.Close();
            var ip = GameObject.Find("OSC").GetComponent<OscManager>().RemoteIpAddress;
            _oscOutDirectivityShaper.Open(portOut, ip);
        }

        private void Update()
        {
            ambisonicVisualizer.rotation = _roomEncoder.RoomOrigin.rotation;
        }

        private void FixedUpdate()
        {
            // DirectivityShaper uses same osc port
            var rotOg = (Quaternion.Inverse(_roomEncoder.RoomOrigin.rotation) * transform.rotation);
            var rot = rotOg.eulerAngles;
            ProbeAzimuth.Set(0, -(rot.y - 180));
            ProbeElevation.Set(0, rot.x - 180);
            ProbeRoll.Set(0, rot.z - 180);
            _oscOutDirectivityShaper.Send(ProbeAzimuth);
            _oscOutDirectivityShaper.Send(ProbeElevation);
            _oscOutDirectivityShaper.Send(ProbeRoll);

            SendBand(band0, BandAzimuth0, BandElevation0, rotOg);
            SendBand(band1, BandAzimuth1, BandElevation1, rotOg);
            SendBand(band2, BandAzimuth2, BandElevation2, rotOg);
            SendBand(band3, BandAzimuth3, BandElevation3, rotOg);
        }

        private void SendBand(Transform band, OscMessage azimuth, OscMessage elevation, Quaternion rotOg)
        {
            var dir = rotOg * band.localPosition;
            CartesianToSpherical(dir, out _, out var az, out var el);
            azimuth.Set(0, az - 90);
            elevation.Set(0, el);
            _oscOutDirectivityShaper.Send(azimuth);
            _oscOutDirectivityShaper.Send(elevation);
        }


        // https://blog.nobel-joergensen.com/2010/10/22/spherical-coordinates-in-unity/
        public static void CartesianToSpherical(Vector3 cartCoords, out float outRadius, out float outAzimuth,
            out float outElevation)
        {
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
                                   + (cartCoords.y * cartCoords.y)
                                   + (cartCoords.z * cartCoords.z));
            outAzimuth = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outAzimuth += Mathf.PI;
            outElevation = Mathf.Asin(cartCoords.y / outRadius);

            outAzimuth *= Mathf.Rad2Deg;
            outElevation *= Mathf.Rad2Deg;
        }
    }
}