using UnityEngine;

namespace Musicality
{
    [RequireComponent(typeof(VelocityProbe))]
    public class Mallet : MonoBehaviour, IReactComponent<Mallet.State>
    {
        #region IReactComponent

        public struct State
        {

            public SerializableTransform Transform;
        }
        
        public GameObject GameObject => gameObject;

        public State CurrentState => new State()
        {
        };

        public int ComponentId { get; set; }
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            UpdateFromState(initialState);
        }

        private SpiralOrganController _connectedOrganController;

        public void Unmount()
        {
        }

        public void UpdateFromState(State nextState)
        {
        }
        
        #endregion
    }
}