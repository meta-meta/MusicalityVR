using UnityEngine;

namespace Musicality
{
    public class OscManager : MonoBehaviour
    {
        public string RemoteIpAddress;
        public int PortOutMaxMsp;
        public int PortInMaxMsp;
 
        public int PortOutReaper;
        public int PortInReaper;

        public OscIn OscInMaxMsp;
        public OscOut OscOutMaxMsp;
     
        public string RemoteIpAddressReaper;
        public OscIn OscInReaper;
        public OscOut OscOutReaper;
    
        // Start is called before the first frame update
        void Awake()
        {
            if (RemoteIpAddress.Length > 0)
            {
                if (PortOutMaxMsp > 0)
                {
                    OscOutMaxMsp = gameObject.AddComponent<OscOut>();
                    OscOutMaxMsp.Open(PortOutMaxMsp, RemoteIpAddress);    
                } else Debug.LogWarning("PortOutMaxMsp not defined");
            
                if (PortInMaxMsp > 0)
                {
                    OscInMaxMsp = gameObject.AddComponent<OscIn>();
                    OscInMaxMsp.Open(PortInMaxMsp);    
                } else Debug.LogWarning("PortOutMaxMsp not defined");
                
            }
            else Debug.LogWarning("OscManager needs a RemoteIpAddress");
        
            if (RemoteIpAddressReaper.Length > 0)
            {
                if (PortOutReaper > 0)
                {
                    OscOutReaper = gameObject.AddComponent<OscOut>();
                    OscOutReaper.Open(PortOutReaper, RemoteIpAddressReaper);    
                } else Debug.LogWarning("PortOutReaper not defined");
            
                if (PortInReaper > 0)
                {
                    OscInReaper = gameObject.AddComponent<OscIn>();
                    OscInReaper.Open(PortInReaper);    
                } else Debug.LogWarning("PortOutReaper not defined");
            }
            else Debug.LogWarning("OscManager needs a RemoteIpAddressReaper");
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
