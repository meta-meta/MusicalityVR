using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace App.Scripts
{
    public class RigidBodyManipulatorNet : NetworkBehaviour
    {
        [SerializeField] private ObjectManipulator manipulator;
        private Rigidbody _rigidbody;

        public override void OnStartClient()
        {
            base.OnStartClient();
            manipulator.firstSelectEntered.AddListener(OnFirstSelectEntered);
            manipulator.lastSelectExited.AddListener(OnLastSelectExited);
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            manipulator.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
            manipulator.lastSelectExited.RemoveListener(OnLastSelectExited);
        }

        private void OnFirstSelectEntered(SelectEnterEventArgs arg0)
        {
            TakeOwnership(LocalConnection);
            // Debug.Log($"{name} owned: {IsOwner}");
        }

        private void OnLastSelectExited(SelectExitEventArgs arg0)
        {
            // Debug.Log($"{name} OnLastSelectExited owned: {IsOwner}");
        }

        private void Update()
        {
            if (IsClientInitialized)
            {
                _rigidbody.isKinematic = !IsOwner;
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TakeOwnership(NetworkConnection connection)
        {
            // Debug.Log($"TakeOwnership: {connection.ClientId}");
            GiveOwnership(connection);
        }
    }
}