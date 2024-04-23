using System;
using System.Collections.Generic;
using FishNet.Object;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace App.Scripts
{
    public class AvatarNet : NetworkBehaviour
    {
        [SerializeField] private Orbital orbitalHead;
        [SerializeField] private List<MeshRenderer> renderersThirdPerson;

        private void Awake()
        {
        }

        public override void OnStartClient()
        {
            Debug.Log("AvatarNet OnStartClient");
            base.OnStartClient();
            orbitalHead.enabled = base.IsOwner;
            foreach (var meshRenderer in renderersThirdPerson) meshRenderer.enabled = !base.IsOwner;
        }

        public override void OnStartServer()
        {
            Debug.Log("AvatarNet OnStartServer");
            orbitalHead.enabled = false;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
        }

        [Client(RequireOwnership = true)]
        private void DoOwnerStuff()
        {
            
        }
        
        private void Update()
        {
            
        }
    }
}