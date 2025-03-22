using System;
using UnityEngine;

namespace Musicality
{
    public class Webcam : MonoBehaviour
    {
        private void Awake()
        {
            WebCamDevice device = new WebCamDevice();
            foreach (var webCamDevice in WebCamTexture.devices)
            {
                Debug.Log($"webcame: {webCamDevice.name}");
                if (webCamDevice.name.StartsWith("USB"))
                    device = webCamDevice;
            }
            
            WebCamTexture webcamTexture = new WebCamTexture(device.name);
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }
    }
}