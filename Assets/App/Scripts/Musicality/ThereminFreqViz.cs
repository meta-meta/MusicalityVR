using Microsoft.MixedReality.Toolkit;
using Shapes;
using TMPro;
using UnityEngine;

namespace Musicality
{
    [ExecuteAlways] public class ThereminFreqViz : ImmediateModeShapeDrawer
    {
        private Theremin _theremin;
        [SerializeField] private ShapesBlendMode blendMode = ShapesBlendMode.Additive;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float freqMult = 1f;
        [SerializeField] private float thickness = 0.001f;

        private void Awake()
        {
            _theremin = GetComponent<Theremin>();
            _freqText = new TextElement();
        }

        private void OnDestroy()
        {
            _freqText.Dispose();
        }

        private Color _lineColor = new Color(0.5f, 1, 1, 0.25f);
        private TextElement _freqText;
        
        public override void DrawShapes( Camera cam )
        {
            if (!_theremin.isOn) return;

            using( Draw.Command( cam ) )
            {
                Draw.BlendMode = blendMode;

                // set up static parameters. these are used for all following Draw.Line calls
                Draw.LineGeometry = LineGeometry.Volumetric3D;
                Draw.ThicknessSpace = ThicknessSpace.Meters;
                Draw.Thickness = thickness;

                
                // set static parameter to draw in the local space of this object
                // Draw.Matrix = transform.localToWorldMatrix;
                // var color = OkColor.OkHslToSrgb((((360 * Math.Log(_theremin.Frequency, 2)) % 360) / 360, 1, 1));
                // var rgb = OkUnityBridge.RgbToColor(color);
                
                var rgb = Color.HSVToRGB(((360 * Mathf.Log(_theremin.Frequency, 2)) % 360) / 360, 1, 1);
                Draw.Triangle(_theremin.FreqPointerPos, _theremin.FreqBotPos, _theremin.FreqTopPos, Color.clear, rgb, rgb);



                
                
                var midpoint = Vector3.Lerp(_theremin.FreqBotPos, _theremin.FreqTopPos, 0.5f);
                var dir = (_theremin.FreqPointerPos - midpoint).normalized;
                var max = midpoint + dir * _theremin.FreqDistMax;

                Draw.Line(midpoint, max);
                // Draw.Line(midpoint, _theremin.FreqPointerPos, 0.003f);
                
                var f = _theremin.FreqAtDistMax;
                
                var fRot = CameraCache.Main.transform.rotation;
                Draw.Color = new Color(1f, 1, 1, 0.7f);
                Draw.PushMatrix();
                Draw.FontSize = 0.3f;
                Draw.Translate(0, 0.05f, 0);
                // Draw.Text(_freqText, midpoint + dir * 0.1f, fRot, $"{_theremin.Frequency.ToString("G4")}Hz", font);
                Draw.Text(_freqText, _theremin.FreqPointerPos, fRot, $"{_theremin.Frequency,8:.00}Hz", font);
                Draw.PopMatrix();

                Draw.Color = new Color(1f, 1, 1, 0.1f);

                while (f < _theremin.FreqAtDistMin)
                {
                    var dist = _theremin.FreqToDist(f);
                    var point = midpoint + dir * dist;

                    Draw.Disc(point, cam.transform.position - point, 0.01f);
                    var lineHalfLen = (15 - Mathf.Log(f, 2)) * 0.01f;
                    Draw.Line(point + Vector3.up * lineHalfLen, point - Vector3.up * lineHalfLen);

                    f *= Mathf.Pow(2, 0.5f);

                    
                    // TODO: multiple Draw.Line not working. deleted shaderCache, still no work
                    //https://shapes.userecho.com/communities/1/topics/359-several-drawline-not-working-in-immediatemodeshapedrawer-starting-with-the-sample
                    // Draw.Line(point + Vector3.up, point - Vector3.up);
                    // f *= 2;
                    // f *= Mathf.Pow(2, -0.5f);
                    //
                    
                    var dist2 = _theremin.FreqToDist(f);
                    var point2 = midpoint + dir * dist2;
                    
                    Draw.Disc(point2, cam.transform.position - point2, 0.005f);
                    Draw.Line(point2 + Vector3.up * (lineHalfLen * 0.5f), point2 - Vector3.up * (lineHalfLen * 0.5f));
                    
                    f *= Mathf.Pow(2, 0.5f);
                }
            }

        }
    }
}