using System;
using Shapes;
using UnityEngine;

namespace FaustUtilities_organ
{
    public class Organ : MonoBehaviour// ImmediateModeShapeDrawer
    {
        [SerializeField] private FaustPlugin_organ organ;
        [SerializeField] private float attackS = 0.1f;
        [SerializeField] private float decayS = 1;
        [SerializeField] private float velMul = 0.5f;
        [SerializeField] private float fMul = 10;
        [SerializeField] private Disc ring;

        private float _vel;
        private float _tAtStrike;


        private void FixedUpdate()
        {
            var tAttack = (Time.time - _tAtStrike) / attackS;
            var tDecay = (Time.time - (_tAtStrike + attackS)) / decayS;
            organ.setParameter(1,
                tAttack < 1
                    ? Mathf.Lerp(0.01f, _vel * velMul, tAttack)
                    : Mathf.Lerp(_vel * velMul, 0.01f, tDecay));

            var f = Mathf.Pow(2, Mathf.Lerp(13, 4, transform.localScale.x + 0.5f));
            organ.setParameter(0, f);
        }

        private void OnCollisionEnter(Collision other)
        {
            _vel = other.relativeVelocity.magnitude;
            _tAtStrike = Time.time;
        }

        private void Update()
        {
            var t = Time.time - _tAtStrike;
            ring.enabled = t < attackS + decayS;
            ring.Radius = t;
            ring.Color = Color.Lerp(Color.white, Color.clear, t / (attackS + decayS));
            ring.transform.rotation.SetLookRotation(ring.transform.position - Camera.main.transform.position, Vector3.up);
        }

        // public override void DrawShapes(Camera cam)
        // {
        //     // Draw.Command enqueues a set of draw commands to render in the given camera
        //     using( Draw.Command( cam ) ) { // all immediate mode drawing should happen within these using-statements
        //         var t = Time.time - _tAtStrike;
        //         if (t > attackS + decayS) return;
        //         Draw.Color = new Color(0.25f, 0.25f, 0.25f, 0.75f);
        //         Draw.BlendMode = ShapesBlendMode.Additive;
        //         Draw.Ring(transform.position, cam.transform.rotation, t);
        //         
        //         // Draw.ResetAllDrawStates(); // this makes sure no static draw states "leak" over to this scene
        //         // Draw.Matrix = transform.localToWorldMatrix; // this makes it draw in the local space of this transform
        //         // for( int i = 0; i < discCount; i++ ) {
        //         //     float t = i / (float)discCount;
        //         //     Color color = Color.HSVToRGB( t, 1, 1 );
        //         //     Vector2 pos = GetDiscPosition( t );
        //         //     Draw.Disc( pos, discRadius, color ); // This is the actual Shapes draw command
        //         // }
        //     }
        // }
    }
}