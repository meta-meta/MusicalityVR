using Newtonsoft.Json;
using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class ReactComponentCollectionOsc<T> : ReactComponentCollection<T>
    {
        public ReactComponentCollectionOsc(GameObject prefab, Transform parent, OscOut oscOut, string oscAddr) : base(prefab, parent)
        {
            _oscOut = oscOut;
            OscAddr = oscAddr;
        }

        private readonly OscOut _oscOut;
        private string _spawnTransformsJson;
        private string _stateJson;

        public string OscAddr { get; }

        public void OnOscMessage(OscMessage msg)
        {
            if (!msg.TryGet(0, out int cmpId))
            {
                Debug.LogWarning($"ReactComponentOsc message received without component id");
                return;
            }

            if (msg.TryGet(1, ref _spawnTransformsJson) && msg.TryGet(2, ref _stateJson))
            {
                if (msg.TryGet(3, out int cloneSourceId))
                {
                    OnClone(cmpId, _spawnTransformsJson, _stateJson, cloneSourceId);
                }
                else
                {
                    OnInitOrUpdate(cmpId, _spawnTransformsJson, _stateJson);
                }
            }
            else
            {
                OnDestroy(cmpId);
            }
        }

        public override void SendCmp(SerializableTransform transform, T state, int? cloneSourceId = null)
        {
            var msg = new OscMessage(OscAddr);
            msg.Add(JsonConvert.SerializeObject(transform));
            msg.Add(JsonConvert.SerializeObject(state));
            if (cloneSourceId.HasValue) msg.Add(cloneSourceId.Value);
            _oscOut.Send(msg);
        }

        public override void SendCmpDelete(int id)
        {
            _oscOut.Send(OscAddr, id);
        }

        public override void SendFn(string name)
        {
            _oscOut.Send("/fn", name);
        }
    }
}