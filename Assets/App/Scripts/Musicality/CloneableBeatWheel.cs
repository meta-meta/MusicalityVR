using UnityEngine;

namespace Musicality
{
    public class CloneableBeatWheel : Cloneable<MidiBeatClockWheel.State>
    {
        [SerializeField] private MidiBeatClockWheel beatWheel;
        protected override IReactComponent<MidiBeatClockWheel.State> reactComponent => beatWheel;
    }
}