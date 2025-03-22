using System;
using MixedReality.Toolkit.SpatialManipulation;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Musicality
{

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
        private ObjectManipulator _manipulator;
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
            
            _manipulator = valueDisplay.GetComponent<ObjectManipulator>();
            _manipulator.selectEntered.AddListener(OnSelectEntered);
            _manipulator.selectExited.AddListener(OnSelectExited);

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

        private void OnSelectExited(SelectExitEventArgs args)
        {
            _orbital.UpdateLinkedTransform = false;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            _orbital.UpdateLinkedTransform = true;
        }

        private void OnDestroy()
        {
            _manipulator.selectEntered.RemoveListener(OnSelectEntered);
            _manipulator.selectExited.RemoveListener(OnSelectExited);
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
    }
}