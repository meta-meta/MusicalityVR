using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using MixedReality.Toolkit;
using Shapes;
using TMPro;
using UnityEngine;

namespace Musicality
{
    public class IntervalVizShapes : ImmediateModeShapeDrawer
    {
        [SerializeField] private ShapesBlendMode blendMode = ShapesBlendMode.Additive;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float thickness = 0.001f;
        private readonly HashSet<MalletTonnegg> _malletTonneggs = new HashSet<MalletTonnegg>();
        private readonly HashSet<UnorderedPair> _mtPairsDrawn = new HashSet<UnorderedPair>();
        private Queue<TextElement> _typingPool = new Queue<TextElement>();
        private bool _drawLeft;
        private bool _drawRight;

        private struct MalletTonnegg
        {
            public MalletHead Mallet;
            public Tonnegg Tonnegg;
            public INote Note => Tonnegg.Note;
            private ValueTuple<int, int> _tuple;

            public MalletTonnegg(MalletHead m, Tonnegg t)
            {
                _tuple = (m.GetInstanceID(), t.GetInstanceID());
                Mallet = m;
                Tonnegg = t;
            }
            public override int GetHashCode() => _tuple.GetHashCode();
            public override bool Equals(object obj) => obj is MalletTonnegg && GetHashCode() == obj.GetHashCode();
        }

        private struct UnorderedPair
        {
            private readonly MalletTonnegg _a;
            private readonly MalletTonnegg _b;
            public UnorderedPair(MalletTonnegg a, MalletTonnegg b)
            {
                _a = a;
                _b = b;
            }
            public override int GetHashCode() => Math.Min((_a, _b).GetHashCode(), (_b, _a).GetHashCode());
            public override bool Equals(object obj) => obj is UnorderedPair && GetHashCode() == obj.GetHashCode();
        }
        
        private void Awake()
        {
            // _intervalAtoB = new TextElement();
            // _intervalBtoA = new TextElement();
            Tonnegg.OnMalletEnter += OnMalletEnter;
            Tonnegg.OnMalletHeadExit += OnMalletExit;
        }

        private void OnDestroy()
        {
            // _intervalAtoB.Dispose();
            // _intervalBtoA.Dispose();
            Tonnegg.OnMalletEnter -= OnMalletEnter;
            Tonnegg.OnMalletHeadExit -= OnMalletExit;
        }

        private void OnMalletEnter(Tonnegg tonnegg, MalletHead malletHead)
        {
            if (!malletHead.IsHoldable) return;
            _malletTonneggs.Add(new MalletTonnegg(malletHead, tonnegg));
        }

        private void OnMalletExit(Tonnegg tonnegg, MalletHead malletHead)
        {
            if (!malletHead.IsHoldable) return;
            if (!_malletTonneggs.Remove(new MalletTonnegg(malletHead, tonnegg)))
                Debug.LogWarning("couldn't remove mallet tonnegg");
        }

        private void DrawForHand(Camera cam, Handedness handedness)
        {
            foreach (var mtA in _malletTonneggs)
            {
                if (!mtA.Mallet.Handedness.IsMatch(handedness)) continue;
                
                foreach (var mtB in _malletTonneggs)
                {
                    if (mtA.Equals(mtB)) continue;
                    var pair = new UnorderedPair(mtA, mtB);
                    if (!_mtPairsDrawn.Add(pair)) continue;
                    
                    var intervalAtoB = mtA.Tonnegg.Note.IntervalTo(mtB.Tonnegg.Note);
                    var intervalBtoA = mtB.Tonnegg.Note.IntervalTo(mtA.Tonnegg.Note);
                   
                    var camPos = cam.transform.position;
                    var pointA = mtA.Mallet.Handle.position;
                    var pointB = mtB.Mallet.Handle.position;
                    var midpoint = Vector3.Lerp(pointA, pointB, 0.5f);
                    var midToCam1 = Vector3.Lerp(midpoint, camPos, 0.05f);
                    var midToCam2 = Vector3.Lerp(midpoint, camPos, 0.1f);
                    var pointAtoB = Vector3.Lerp(midToCam2, pointA, 0.2f);
                    var pointBtoA = Vector3.Lerp(midToCam2, pointB, 0.2f);
                    
                    var fRot = Quaternion.LookRotation(midpoint - camPos);

                    Draw.Color = new Color(0, 0, 0, 0.5f);
                    Draw.Disc(pointAtoB, fRot, 0.02f);
                    Draw.Disc(pointBtoA, fRot, 0.02f);
                   
                    Draw.Color = new Color(1f, 1, 1, 0.7f);
                    Draw.UseDashes = true;
                    Draw.FontSize = 0.2f;
                    
                    Draw.Color = new Color(0.5f, 0.5f, 1, 0.7f);
                    Draw.Line(pointA, midToCam1);
                    
                    Draw.PushMatrix();
                    Draw.Translate(0, 0.05f, 0);
                    Draw.Text(pointAtoB, fRot, intervalAtoB.LabelPitch(), font);
                    Draw.PopMatrix();
                    
                    Draw.Color = new Color(1f, 0.5f, 0.5f, 0.7f);
                    Draw.Line(pointB, midToCam1);

                    Draw.PushMatrix();
                    Draw.Translate(0, -0.05f, 0);
                    Draw.Text(pointBtoA, fRot, intervalBtoA.LabelPitch(), font);
                    Draw.PopMatrix();
                    
                    // TODO: multiple Draw.Line not working. deleted shaderCache, still no work
                    //https://shapes.userecho.com/communities/1/topics/359-several-drawline-not-working-in-immediatemodeshapedrawer-starting-with-the-sample
                }
            }
        }

        public override void DrawShapes(Camera cam)
        {

            using (Draw.Command(cam))
            {
                Draw.BlendMode = blendMode;

                // set up static parameters. these are used for all following Draw.Line calls
                Draw.LineGeometry = LineGeometry.Volumetric3D;
                Draw.ThicknessSpace = ThicknessSpace.Meters;
                Draw.Thickness = thickness;


                _mtPairsDrawn.Clear();
                if (_drawLeft) DrawForHand(cam, Handedness.Left);
                if (_drawRight) DrawForHand(cam, Handedness.Right);
            }
        }

        private void Update()
        {
            _drawRight = OVRInput.Get(OVRInput.RawTouch.A); // Q3 thumb touching A
            _drawLeft = OVRInput.Get(OVRInput.RawTouch.X); // Q3 thumb touching X
            
            
            // var lTouch = OVRInput.Get(OVRInput.RawTouch.LTouchpad);
            // var lTouchpadVec = OVRInput.Get(OVRInput.RawAxis2D.LTouchpad);
            // var rTouchpadVec = OVRInput.Get(OVRInput.RawAxis2D.RTouchpad);
            // var lTouchpadDown = OVRInput.Get(OVRInput.RawButton.LTouchpad);
            // var rTouchpadDown = OVRInput.Get(OVRInput.RawButton.RTouchpad);
            // var lThumbNearTouch = OVRInput.Get(OVRInput.RawNearTouch.LThumbButtons); // Q3 thumb near any buttons or touchpad
            // var lThumbForce = OVRInput.Get(OVRInput.RawAxis1D.LThumbRestForce); 
            // var lIndexCurl = OVRInput.Get(OVRInput.RawAxis1D.LIndexTriggerCurl); // Q3 index near trigger 1.0 when touching
            // var lIndexSlide = OVRInput.Get(OVRInput.RawAxis1D.LIndexTriggerSlide); // Q3 index touching trigger, 1.0 on outer edge
            // Debug.Log($"xTouch {xTouch} ltBtn {lTouchpadDown} ltVec {lTouchpadVec} ltTch {lTouch} lThNear {lThumbNearTouch} lThForce {lThumbForce} lCrl {lIndexCurl} lSl {lIndexSlide}");
        }
    }
}