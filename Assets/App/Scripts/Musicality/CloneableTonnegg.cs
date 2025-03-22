using UnityEngine;

namespace Musicality
{
    public class CloneableTonnegg : Cloneable<Tonnegg.State>
    {
        [SerializeField] private Tonnegg tonnegg;
        protected override IReactComponent<Tonnegg.State> reactComponent => tonnegg;
    }
}