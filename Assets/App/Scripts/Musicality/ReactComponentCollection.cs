using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Musicality
{
    public abstract class ReactComponentCollection<T>
    {
        private readonly Dictionary<int, IReactComponent<T>> _idsToReactComponents = new Dictionary<int, IReactComponent<T>>();
        private readonly GameObject _prefab;
        private readonly Transform _parent;

        public bool TryGetComponent(int id, out IReactComponent<T> component) =>
            _idsToReactComponents.TryGetValue(id, out component);
        
        public struct SpawnTransforms
        {
            public SerializableTransform? LocalTransform;
            public SerializableTransform? SpawnFromCam;
            public SerializableTransform? SpawnFromSpawner;
            public int? SpawnerId;
            public bool SpawnerIsParent;
        }

        protected ReactComponentCollection(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        protected void OnClone(int cmpId, string spawnTransformsJson, string stateJson, int cloneSourceId)
        {
            OnInitOrUpdate(cmpId, spawnTransformsJson, stateJson);

            var clone = _idsToReactComponents[cmpId];
            var source = _idsToReactComponents[cloneSourceId];
            clone.GameObject.transform.SetParent(source.GameObject.transform.parent, true);

            // while manipulating, source is in hand, clone is created in source's place
            // swap ids so that state manager can refer to clone as source
            clone.ComponentId = cloneSourceId;
            source.ComponentId = cmpId;
            _idsToReactComponents[cloneSourceId] = clone;
            _idsToReactComponents[cmpId] = source;
        }

        protected void OnInitOrUpdate(int cmpId, string spawnTransformsJson, string stateJson)
        {
            var spawnTransforms = JsonConvert.DeserializeObject<SpawnTransforms>(spawnTransformsJson);
            var state = JsonConvert.DeserializeObject<T>(stateJson);

            if (_idsToReactComponents.TryGetValue(cmpId, out var existingCmp))
            {
                existingCmp.UpdateFromState(state);
            }
            else
            {
                var camTr = CameraCache.Main.transform;
                var go = Object.Instantiate(_prefab, camTr.position + camTr.forward, camTr.rotation, _parent);

                // TODO: these optional transforms should apply to update as well
                if (spawnTransforms.LocalTransform.HasValue)
                {
                    var tr = spawnTransforms.LocalTransform.Value;
                    // Debug.Log($"using LocalTransform {tr.localPosition} {tr.localRotationEuler} {tr.localScale}");
                    go.transform.localPosition = tr.localPosition;
                    go.transform.localRotation = Quaternion.Euler(tr.localRotationEuler); // TODO: choose between localRotationEuler and localRotation depending on which is not identity
                    go.transform.localScale = tr.localScale;
                }
                else if (spawnTransforms.SpawnFromCam.HasValue)
                {
                    var tr = spawnTransforms.SpawnFromCam.Value;
                    // Debug.Log($"using SpawnFromCam {tr.localPosition} {tr.localRotationEuler} {tr.localScale}");
                    go.transform.localPosition =  camTr.TransformPoint(tr.localPosition);
                    go.transform.localRotation = camTr.rotation * Quaternion.Euler(tr.localRotationEuler); // TODO: choose between localRotationEuler and localRotation depending on which is not identity
                    go.transform.localScale = tr.localScale;
                }
                else if (spawnTransforms.SpawnFromSpawner.HasValue)
                {
                    if (!spawnTransforms.SpawnerId.HasValue)
                    {
                        Debug.LogError("SpawnFromSpawner is set but SpawnerId is null");
                        return;
                    }
                    
                    var manager = _parent.GetComponent<ReactComponentManagerOsc>();
                    if (manager.TryGetSpawner(spawnTransforms.SpawnerId.Value, out var spawner))
                    {
                        var spawnFrom = spawner.GameObject.transform;
                        var tr = spawnTransforms.SpawnFromSpawner.Value;
                        if (spawner.CurrentState.IsParent || spawnTransforms.SpawnerIsParent) // TODO: probably want to choose just one of these
                        {
                            go.transform.SetParent(spawnFrom);
                            go.transform.localPosition = tr.localPosition;
                            go.transform.localRotation = Quaternion.Euler(tr.localRotationEuler); // TODO: choose between localRotationEuler and localRotation depending on which is not identity
                            go.transform.localScale = tr.localScale;
                        }
                        else
                        {
                            go.transform.localPosition = spawnFrom.TransformPoint(tr.localPosition);
                            go.transform.localRotation =
                                spawnFrom.rotation * Quaternion.Euler(tr.localRotationEuler); // TODO: choose between localRotationEuler and localRotation depending on which is not identity
                            go.transform.localScale = tr.localScale;
                        }
                        
                    }
                }
                
                var reactCmp = go.GetComponent<IReactComponent<T>>();
                reactCmp.ComponentCollection = this;
                reactCmp.ComponentId = cmpId;
                reactCmp.Init(state);
                _idsToReactComponents.Add(cmpId, reactCmp);
            }
        }
        protected void OnDestroy(int cmpId)
        {
            if (!_idsToReactComponents.TryGetValue(cmpId, out var existingCmp)) return;
            existingCmp.Unmount();
            _idsToReactComponents.Remove(cmpId);
            Object.Destroy(existingCmp.GameObject);
        }

        public abstract void SendCmp(SerializableTransform transform, T state, int? cloneSourceId = null);
        public abstract void SendCmpDelete(int id);
        public abstract void SendFn(string name);
    }
}