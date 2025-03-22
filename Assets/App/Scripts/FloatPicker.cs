using System;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;

namespace Musicality
{
    using ObjectManipulator = Microsoft.MixedReality.Toolkit.UI.ObjectManipulator;

    public class FloatPicker : MonoBehaviour
    {
        [SerializeField] private SpiralIrrationalIntervalPicker picker;
        [SerializeField] private TMP_Text labelTmp;
        [SerializeField] private Tonnegg valueDisplay;
        [SerializeField] private Transform orbitalTarget;
        [SerializeField] private bool valLoop;
        [SerializeField] private float valInitial = 0.5f;
        [SerializeField] private float valMax = 1;
        [SerializeField] private float valMin = 0;
        [SerializeField] private string label = "float picker";
        private Orbital _orbital;
        private SolverHandler _solverHandler;

        public Action<float> OnChange;
        public float Val { get; private set; }

        private void Awake()
        {
            _orbital = valueDisplay.GetComponent<Orbital>();
            _solverHandler = valueDisplay.GetComponent<SolverHandler>();

            if (orbitalTarget)
            {
                // var diff = transform.position - orbitalTarget.position;
                var diff = orbitalTarget.InverseTransformPointUnscaled(transform.position);
                _solverHandler.AdditionalOffset = diff;
                
                // var diffRot = orbitalTarget.rotation * Quaternion.Inverse(transform.rotation);
                // _solverHandler.AdditionalRotation = diffRot.eulerAngles;
                
                _solverHandler.TransformOverride = orbitalTarget;
            }
            
            var manipulator = valueDisplay.GetComponent<ObjectManipulator>();
            manipulator.OnManipulationStarted.AddListener(OnManipulationStart);
            manipulator.OnManipulationEnded.AddListener(OnManipulationEnd);

            picker.ClampAtMaxVal = valMax + 1;
            Val = valInitial;
            valueDisplay.SetNote(new NoteIrrational(valInitial));
            picker.SetAngleFromNote(new NoteIrrational(valInitial + 1));
            
            picker.IntervalChanged += note =>
            {
                var nextVal = note.Val - 1;
                if (nextVal < valMin) nextVal = valLoop ? valMax : valMin;
                if (nextVal > valMax) nextVal = valLoop ? valMin : valMax;
                valueDisplay.SetNote(new NoteIrrational(nextVal));
                _orbital.UpdateLinkedTransform = true;
                Val = nextVal;
                OnChange?.Invoke(nextVal);
            };
        }

        private void Start()
        {
            OnChange?.Invoke(valInitial);
            _orbital.UpdateLinkedTransform = false;
        }

        private void OnValidate()
        {
            labelTmp.text = label;
        }

        private void OnManipulationEnd(ManipulationEventData arg0)
        {
            _orbital.UpdateLinkedTransform = false;
        }

        private void OnManipulationStart(ManipulationEventData arg0)
        {
            _orbital.UpdateLinkedTransform = true;
        }

    }
}