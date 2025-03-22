using Musicality;
using UnityEngine;

public interface IReactComponent<T>
{
    public GameObject GameObject { get; }
    public T CurrentState { get; }
    public int ComponentId { get; set; }
    public ReactComponentCollection<T> ComponentCollection { get; set; }
    public void Init(T initialState);
    public void Unmount();
    public void UpdateFromState(T nextState);
}