using UnityEngine;

namespace Musicality
{
    public class CloneableMallet : Cloneable<Mallet.State>
    {
        [SerializeField] private Mallet mallet;
        protected override IReactComponent<Mallet.State> reactComponent => mallet;
    }
}