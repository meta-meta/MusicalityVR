using UnityEngine;

namespace Musicality
{
    public class ReactComponentManagerOsc : MonoBehaviour
    {
        private OscIn _oscIn;
        private OscOut _oscOut;

        private ReactComponentCollectionOsc<Mallet.State> _mallets;
        private ReactComponentCollectionOsc<MidiBeatClockWheel.State> _beatWheels;
        private ReactComponentCollectionOsc<Spawner.State> _spawners;
        private ReactComponentCollectionOsc<SpiralOrganController.State> _organs;
        private ReactComponentCollectionOsc<Tonnegg.State> _tonneggs;
        
        [SerializeField] private GameObject prefabMallet;
        [SerializeField] private GameObject prefabMidiBeatClockWheel;
        [SerializeField] private GameObject prefabTonnegg;
        [SerializeField] private GameObject prefabOrgan;
        [SerializeField] private GameObject prefabSpawner;

        private void OnEnable()
        {
            _oscIn = gameObject.AddComponent<OscIn>();
            _oscIn.Open(8020);
            _oscOut = gameObject.AddComponent<OscOut>();
            _oscOut.Open(9004);

            var parentTransform = transform;
            _beatWheels = new ReactComponentCollectionOsc<MidiBeatClockWheel.State>(prefabMidiBeatClockWheel, parentTransform, _oscOut, "/react/beatWheel");
            _mallets = new ReactComponentCollectionOsc<Mallet.State>(prefabMallet, parentTransform, _oscOut, "/react/mallet");
            _organs = new ReactComponentCollectionOsc<SpiralOrganController.State>(prefabOrgan, parentTransform, _oscOut, "/react/organ");
            _spawners = new ReactComponentCollectionOsc<Spawner.State>(prefabSpawner, parentTransform, _oscOut, "/react/spawner");
            _tonneggs = new ReactComponentCollectionOsc<Tonnegg.State>(prefabTonnegg, parentTransform, _oscOut, "/react/tonnegg");

            _oscIn.Map(_beatWheels.OscAddr, _beatWheels.OnOscMessage);
            _oscIn.Map(_mallets.OscAddr, _mallets.OnOscMessage);
            _oscIn.Map(_organs.OscAddr, _organs.OnOscMessage);
            _oscIn.Map(_spawners.OscAddr, _spawners.OnOscMessage);
            _oscIn.Map(_tonneggs.OscAddr, _tonneggs.OnOscMessage);
        }

        private void OnDisable()
        {
            _oscIn.UnmapAll(_beatWheels.OscAddr);
            _oscIn.UnmapAll(_mallets.OscAddr);
            _oscIn.UnmapAll(_organs.OscAddr);
            _oscIn.UnmapAll(_spawners.OscAddr);
            _oscIn.UnmapAll(_tonneggs.OscAddr);
            _oscIn.Close();
            _oscOut.Close();
        }
        
        public bool TryGetSpawner(int id, out IReactComponent<Spawner.State> spawner) => _spawners.TryGetComponent(id, out spawner);
    }
}