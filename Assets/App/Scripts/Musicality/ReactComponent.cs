using UnityEngine;

namespace Musicality
{
    public class ReactComponent<T> : MonoBehaviour, IReactComponent<T>
    {
        private int _componentId;

        public int ComponentId()
        {
            throw new System.NotImplementedException();
        }

        public GameObject GameObject => gameObject;
        
        public T CurrentState { get; }

        int IReactComponent<T>.ComponentId
        {
            get => _componentId;
            set => _componentId = value;
        }

        public ReactComponentCollection<T> ComponentCollection { get; set; }

        public void Init(T initialState)
        {
            throw new System.NotImplementedException();
        }

        public void Unmount()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateFromState(T nextState)
        {
            throw new System.NotImplementedException();
        }
    }
}