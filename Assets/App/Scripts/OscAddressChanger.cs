using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OscAddressChanger : MonoBehaviour
{
    private OscIn _oscIn;
    private OscOut _oscOut;
    private int _port;
    private string _ip;
    
    // Start is called before the first frame update
    void Start()
    {
        var oscObj = GameObject.Find("OSC");
        _oscIn = oscObj.GetComponent<OscIn>();
        _oscOut = oscObj.GetComponent<OscOut>();
        _ip = _oscOut.remoteIpAddress;
        _port = _oscOut.port;
        _oscIn.Map("/ip", (msg =>
        {
            msg.TryGet(0, ref _ip);
            _oscOut.Close();
            _oscOut.Open(_port, _ip);
        }));
        _oscIn.MapInt("/port", (port =>
        {
            _port = port;
            _oscOut.Close();
            _oscOut.Open(_port, _ip);
        }));
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
