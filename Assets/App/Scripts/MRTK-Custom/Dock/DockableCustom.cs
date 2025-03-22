// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

namespace MRTK_Custom.Dock
{
    /// <summary>
    /// Add a Dockable component to any object that has a <see cref="DockableCustom"/> and an <see cref="ObjectManipulator"/>
    /// or <see cref="Microsoft.MixedReality.Toolkit.UI.ManipulationHandler"/> to make it dockable in Docks. That allows this object to be used
    /// as part of a palette, shelf or navigation bar together with other objects.
    /// </summary>
    /// <seealso cref="Dock"/>
    /// <seealso cref="DockPositionCustom"/>
    // [AddComponentMenu("Scripts/MRTK/Experimental/Dock/Dockable")]
    public class DockableCustom : MonoBehaviour
    {
        [Experimental]
        [SerializeField, ReadOnly]
        [Tooltip("Current state of this dockable in regards to a dock.")]
        private DockingState dockingState = DockingState.Undocked;

        [SerializeField]
        [Tooltip("Time to animate any move/scale into or out of the dock.")]
        private float moveLerpTime = 0.1f;

        [SerializeField]
        [Tooltip("Time to animate an element when it's following the dock (use 0 for tight attachment)")]
        private float moveLerpTimeWhenDocked = 0.05f;

        /// <summary>
        /// True if this object can currently be docked, false otherwise.
        /// </summary>
        public bool CanDock => dockingState == DockingState.Undocked || dockingState == DockingState.Undocking;

        /// <summary>
        /// True if this object can currently be undocked, false otherwise.
        /// </summary>
        public bool CanUndock => dockingState == DockingState.Docked;

        // Constants
        private const float DistanceTolerance = 0.01f; // in meters
        private const float AngleTolerance = 3.0f; // in degrees
        private const float ScaleTolerance = 0.01f; // in percentage

        private DockPositionCustom dockedPosition = null;
        private Vector3 dockedPositionScale = Vector3.one;

        private HashSet<DockPositionCustom> overlappingPositions = new HashSet<DockPositionCustom>();
        private Vector3 originalScale = Vector3.one;
        private bool isDragging = false;
        private ObjectManipulator objectManipulator;


        public Action<DockPositionCustom> OnDocked;
        public Action OnUndocked;
        public Action<DockPositionCustom> OnDockHover;
        public Action<DockPositionCustom> OnDockLeave;

        /// <summary>
        /// Subscribes to manipulation events.
        /// </summary>
        private void OnEnable()
        {
            objectManipulator = gameObject.GetComponent<ObjectManipulator>();
            objectManipulator.selectEntered.AddListener(OnSelectEntered);
            objectManipulator.selectExited.AddListener(OnSelectExited);

            Assert.IsNotNull(gameObject.GetComponent<Collider>(), "A Dockable object must have a Collider component.");
        }

        /// <summary>
        /// Unsubscribes from manipulation events.
        /// </summary>
        private void OnDisable()
        {
            objectManipulator.selectEntered.RemoveListener(OnSelectEntered);
            objectManipulator.selectExited.RemoveListener(OnSelectExited);
            objectManipulator = null;

            if (dockedPosition != null)
            {
                dockedPosition.DockedObject = null;
                dockedPosition = null;
            }

            overlappingPositions.Clear();
            dockingState = DockingState.Undocked;
        }
        
        // from MRTK2 QuaternionExtensions
        /// <summary>
        /// Determines if the angle between two quaternions is within a given tolerance.
        /// </summary>
        /// <param name="q1">The first quaternion.</param>
        /// <param name="q2">The second quaternion.</param>
        /// <param name="angleTolerance">The maximum angle that will cause this to return true.</param>
        /// <returns>True if the quaternions are aligned within the tolerance, false otherwise.</returns>
        public static bool AlignedEnough(Quaternion q1, Quaternion q2, float angleTolerance)
        {
            return Mathf.Abs(Quaternion.Angle(q1, q2)) < angleTolerance;
        }

        /// <summary>
        /// Updates the transform and state of this object every frame, depending on 
        /// manipulations and docking state.
        /// </summary>
        public void Update()
        {
            if (isDragging && overlappingPositions.Count > 0)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    closestPosition.GetComponentInParent<DockCustom>().TryMoveToFreeSpace(closestPosition);
                }
            }

            if (dockingState == DockingState.Docked || dockingState == DockingState.Docking)
            {
                Assert.IsNotNull(dockedPosition, "When a dockable is docked, its dockedPosition must be valid.");
                Assert.AreEqual(dockedPosition.DockedObject, this, "When a dockable is docked, its dockedPosition reference the dockable.");

                var lerpTime = dockingState == DockingState.Docked ? moveLerpTimeWhenDocked : moveLerpTime;

                if (!isDragging)
                {
                    // Don't override dragging
                    transform.position = Solver.SmoothTo(transform.position, dockedPosition.transform.position, Time.deltaTime, lerpTime);
                    transform.rotation = Solver.SmoothTo(transform.rotation, dockedPosition.transform.rotation, Time.deltaTime, lerpTime);
                }

                transform.localScale = Solver.SmoothTo(transform.localScale, dockedPositionScale, Time.deltaTime, lerpTime);

                if (VectorExtensions.CloseEnoughTo(dockedPosition.transform.position, transform.position, DistanceTolerance) &&
                    AlignedEnough(dockedPosition.transform.rotation, transform.rotation, AngleTolerance) &&
                    AboutTheSameSize(dockedPositionScale.x, transform.localScale.x))
                {
                    // Finished docking
                    if (dockingState != DockingState.Docked) OnDocked?.Invoke(dockedPosition);
                    dockingState = DockingState.Docked;

                    // Snap to position
                    transform.position = dockedPosition.transform.position;
                    transform.rotation = dockedPosition.transform.rotation;
                    transform.localScale = dockedPositionScale;
                }
            }
            else if (dockedPosition == null && dockingState == DockingState.Undocking)
            {
                transform.localScale = Solver.SmoothTo(transform.localScale, originalScale, Time.deltaTime, moveLerpTime);

                if (AboutTheSameSize(originalScale.x, transform.localScale.x))
                {
                    // Finished undocking
                    dockingState = DockingState.Undocked;
                    OnUndocked?.Invoke();

                    // Snap to size
                    transform.localScale = originalScale;
                }
            }
        }

        /// <summary>
        /// Docks this object in a given <see cref="DockPositionCustom"/>.
        /// </summary>
        /// <param name="position">The <see cref="DockPositionCustom"/> where we'd like to dock this object.</param>
        public void Dock(DockPositionCustom position)
        {
            if (!CanDock)
            {
                Debug.LogError($"Trying to dock an object that was not undocked. State = {dockingState}");
                return;
            }

            Debug.Log($"Docking object {gameObject.name} on position {position.gameObject.name}");

            dockedPosition = position;
            dockedPosition.DockedObject = this;
            float scaleToFit = gameObject.GetComponent<Collider>().bounds.GetScaleToFitInside(dockedPosition.GetComponent<Collider>().bounds);
            dockedPositionScale = transform.localScale * scaleToFit;

            if (dockingState == DockingState.Undocked)
            {
                // Only register the original scale when first docking
                originalScale = transform.localScale;
            }

            dockingState = DockingState.Docking;
        }

        /// <summary>
        /// Undocks this <see cref="DockableCustom"/> from the current <see cref="DockPositionCustom"/> where it is docked.
        /// </summary>
        public void Undock()
        {
            if (!CanUndock)
            {
                Debug.LogError($"Trying to undock an object that was not docked. State = {dockingState}");
                return;
            }

            Debug.Log($"Undocking object {gameObject.name} from position {dockedPosition.gameObject.name}");

            dockedPosition.DockedObject = null;
            dockedPosition = null;
            dockedPositionScale = Vector3.one;
            dockingState = DockingState.Undocking;
        }

        #region Collision events

        void OnTriggerEnter(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPositionCustom>();
            if (dockPosition != null)
            {
                overlappingPositions.Add(dockPosition);
                OnDockHover?.Invoke(dockPosition);
                Debug.Log($"{gameObject.name} collided with {dockPosition.name}");
            }
        }

        void OnTriggerExit(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPositionCustom>();
            if (overlappingPositions.Contains(dockPosition))
            {
                overlappingPositions.Remove(dockPosition);
                OnDockLeave?.Invoke(dockPosition);
            }
        }

        #endregion

        #region Manipulation events

        private void OnSelectEntered(SelectEnterEventArgs arg0)
        {
            isDragging = true;

            if (CanUndock)
            {
                Undock();
            }
        }
        
        private void OnSelectExited(SelectExitEventArgs arg0)
        {
            isDragging = false;

            if (overlappingPositions.Count > 0 && CanDock)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    if (!closestPosition.GetComponentInParent<DockCustom>().TryMoveToFreeSpace(closestPosition))
                    {
                        return;
                    }
                }

                Dock(closestPosition);
            }
        }

        #endregion

        /// <summary>
        /// Gets the overlapping <see cref="DockPositionCustom"/> that is closest to this Dockable.
        /// </summary>
        /// <returns>The overlapping <see cref="DockPositionCustom"/> that is closest to this <see cref="DockableCustom"/>, or null if no positions overlap.</returns>
        private DockPositionCustom GetClosestPosition()
        {
            var bounds = gameObject.GetComponent<Collider>().bounds;
            var minDistance = float.MaxValue;
            DockPositionCustom closestPosition = null;
            foreach (var position in overlappingPositions)
            {
                var distance = (position.gameObject.GetComponent<Collider>().bounds.center - bounds.center).sqrMagnitude;
                if (closestPosition == null || distance < minDistance)
                {
                    closestPosition = position;
                    minDistance = distance;
                }
            }

            return closestPosition;
        }

        #region Helpers

        private static bool AboutTheSameSize(float scale1, float scale2)
        {
            Assert.AreNotEqual(0.0f, scale2, "Cannot compare scales with an object that has scale zero.");
            return Mathf.Abs(scale1 / scale2 - 1.0f) < ScaleTolerance;
        }

        #endregion
    }
}