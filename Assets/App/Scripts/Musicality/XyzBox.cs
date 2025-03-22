using System;
using UnityEngine;

namespace Musicality
{
    public class XyzBox : MonoBehaviour
    {
        [SerializeField] private Transform pointer;
        public Action<Vector3> PosUpdate; // 0 - 1f
        private Collider _collider;
        private Material _material;
        private static readonly int MatColor = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _material = GetComponent<Renderer>().material;
        }

        private void SetColor(Vector3 pos)
        {
            _material.SetColor(MatColor, new Color(pos.x , pos.y, pos.z, 0.4f));
        }
        
        private void Update()
        {
            if (!_collider.bounds.Contains(pointer.position)) return;
            var posDiff = transform.InverseTransformPoint(pointer.position) + Vector3.one * 0.5f;

            // Debug.Log(posDiff);
            PosUpdate?.Invoke(posDiff);
            SetColor(posDiff);
        }
    }
}