using TMPro;
using UnityEngine;

namespace Musicality
{
    public class Spawner : MonoBehaviour, IReactComponent<Spawner.State>
    {
        [SerializeField] private GameObject viz;
        [SerializeField] private Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator;
        [SerializeField] private TextMeshPro label;
        
        private Spawner.State _state;
        
        public struct State
        {
            public string Label;
            public bool IsLocked;
            public bool IsParent;
        }

        public GameObject GameObject => gameObject;
        public State CurrentState => _state;
        public int ComponentId { get; set; }
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            UpdateFromState(initialState);
        }

        public void Unmount()
        {
        }

        public void UpdateFromState(State nextState)
        {
            label.gameObject.SetActive(!string.IsNullOrWhiteSpace(nextState.Label));
            label.text = nextState.Label;
            objectManipulator.enabled = !nextState.IsLocked;
            _state = nextState;
        }
    }
}