using System;
using System.Collections;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using UnityEngine;

namespace Musicality
{
    public class Interval : MonoBehaviour
    {
        #region Inspector

        [Header("Config")]
        [SerializeField] private bool isOctaveDisplayed = true;

        [Header("Component Refs")]
        [SerializeField] private SpiralNotePicker notePicker;
        [SerializeField] private Transform visual;
        [SerializeField] private LineRenderer lrA;
        [SerializeField] private LineRenderer lrB;
    
        #endregion

        private IEnumerator _lightUpCoroutine;
        private INote _note = new NoteJI(1,1);
        private Material _material;
        private TextMeshPro _pitchClassLabel;
        private TextMeshPro _sub;
        private TextMeshPro _sup;
        private bool _areRefsSetup;
        private static readonly int MatColor = Shader.PropertyToID("_Color");
        private static readonly int MatHoverColorOverride = Shader.PropertyToID("_HoverColorOverride");
        private static readonly int MatRimColor = Shader.PropertyToID("_RimColor");
        private static readonly int MatRimPower = Shader.PropertyToID("_RimPower");


        private void SetupRefsIfNeeded()
        {
            if (_areRefsSetup) return;

            _material = Application.isPlaying
                ? visual.GetComponent<MeshRenderer>().material
                : visual.GetComponent<MeshRenderer>().sharedMaterial;
            _pitchClassLabel = transform.Find("PitchClassLabel")?.GetComponent<TextMeshPro>();
            _sub = transform.Find("sub")?.GetComponent<TextMeshPro>();
            _sup = transform.Find("sup")?.GetComponent<TextMeshPro>();
            _areRefsSetup = true;
            notePicker = GetComponentInChildren<SpiralNotePicker>();
        }

        // Start is called before the first frame update
        private void Awake()
        {
            SetupRefsIfNeeded();
            SetupStyle();
        }

        private void OnDestroy()
        {
        }

        private void SetHue(int colorName)
        {
            Color.RGBToHSV(_material.GetColor(colorName), out _, out var sat, out var val);
            _material.SetColor(colorName, Color.HSVToRGB(_note.AngleOnCircle() / 360, sat, val));
        }

        private void SetupStyle()
        {
            if (_pitchClassLabel)
            {
                _pitchClassLabel.text = _note.LabelPitchClass();
            }

            var oct = isOctaveDisplayed ? _note.OctaveRelativeToDiapason() : 0;
            var carets = string.Concat(Enumerable.Repeat("^", Math.Abs(oct)));
            _sup.text = oct > 0 ? carets : "";
            _sub.text = oct < 0 ? carets : "";

            SetHue(MatColor);
            SetHue(MatHoverColorOverride);
            SetHue(MatRimColor);
        }

        private void OnValidate()
        {
            SetupRefsIfNeeded();
            SetupStyle();
        }

        private bool _isInitialized;
        private Transform _trA, _trB;
    
        public void CalculateAndSetInterval(INote nB, INote nA, Transform trB, Transform trA)
        {
            if (nA.NoteType != nB.NoteType)
            {
                Debug.LogWarning($"Interval calculation between different noteTypes is not yet implemented");
                return;
            }

            _trA = trA;
            _trB = trB;

            UpdatePos();
        
            _note = nA.IntervalTo(nB);
            SetupRefsIfNeeded();
            SetupStyle();
        
            _lightUpCoroutine = LightUp(0.2f);
            _isInitialized = true;
        }

        private void UpdatePos()
        {
            var camPos = CameraCache.Main.transform.position;
            transform.position = Vector3.Lerp(_trA.position, _trB.position, .5f);
            transform.position = Vector3.Lerp(transform.position, camPos, 0.2f);
            transform.rotation = Quaternion.LookRotation(camPos - transform.position, Vector3.up);
        
            lrA.SetPosition(0, _trA.position);
            lrA.SetPosition(1, transform.position);
            lrB.SetPosition(0, _trB.position);
            lrB.SetPosition(1, transform.position);
        }

        private void Update()
        {
            if (_isInitialized) UpdatePos();
        }

        IEnumerator LightUp(float val)
        {
            const float duration = 1f;
            var timeout = /*vel * */duration;
            while (true)
            {
                yield return null;
                SetLight(Mathf.Lerp(0, val, timeout / duration));

                if (timeout <= 0f)
                {
                    SetLight(0);
                    break;
                }

                timeout -= Time.deltaTime;
            }
        }

        const float LightRimPower = 8f;

        private void SetLight(float val)
        {
            _material.SetFloat(MatRimPower, Mathf.Lerp(LightRimPower, 1f, val));
        }
    }
}