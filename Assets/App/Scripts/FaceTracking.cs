using System;
using System.Collections;
using System.Collections.Generic;
using Musicality;
using UnityEngine;

public class FaceTracking : MonoBehaviour
{
    private OVRFaceExpressions _faceExpressions;
    private MidiOut _midiOut;

    [SerializeField] private List<FaceMapping> _faceMappings;

    [Serializable]
    public class FaceMapping
    {
        [SerializeField] private OVRFaceExpressions.FaceExpression _faceExpression;
        [SerializeField] private int _cc;

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _posMin;
        [SerializeField] private Vector3 _posMax;
        [SerializeField] private float _scaleMin = 0.5f;
        [SerializeField] private float _scaleMax = 0.5f;

        [SerializeField] private float _valRaw;
        [Range(0, 4)] [SerializeField] private float _valMultiplier = 1;
        [Range(0.01f, 0.3f)] [SerializeField] private float _valSmoothing;
        [Range(0, 1)] [SerializeField] private float _val;

        public void UpdateValAndSendCC(OVRFaceExpressions fe, float deltaTime, MidiOut midiOut)
        {
            _valRaw = fe[_faceExpression];
            _val = Mathf.Lerp(_val, Mathf.Min(1, _valRaw * _valMultiplier), deltaTime / _valSmoothing);
            midiOut.SendCC(_cc, _val);
        }

        public void UpdateVisuals()
        {
            // Debug.Log($"scale {Mathf.Lerp(_scaleMin, _scaleMax, _val)}  localPosition {Vector3.Lerp(_posMin, _posMax, _val)}");
            _transform.localScale = Vector3.one * Mathf.Lerp(_scaleMin, _scaleMax, _val);
            _transform.localPosition = Vector3.Lerp(_posMin, _posMax, _val);
        }

        public void UpdateValEditor()
        {
            UpdateVisuals();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _faceExpressions = GetComponent<OVRFaceExpressions>();
        _midiOut = GetComponent<MidiOut>();
    }

    private void OnValidate()
    {
        Debug.Log("onValidate");
        foreach (var faceMapping in _faceMappings)
        {
            faceMapping.UpdateValEditor();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_faceExpressions.ValidExpressions)
        {
            foreach (var faceMapping in _faceMappings)
            {
                faceMapping.UpdateValAndSendCC(_faceExpressions, Time.fixedUnscaledDeltaTime, _midiOut);
            }
        }
    }

    private void Update()
    {
        foreach (var faceMapping in _faceMappings)
        {
            faceMapping.UpdateVisuals();
        }
    }
}