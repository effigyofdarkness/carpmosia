using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.BloodBound.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BloodBoundComponent : Component
{
    [Serializable, NetSerializable]
    public sealed class BloodboundEffect : EntityEventArgs
    {
        public NetEntity Target { get; }
        public string? Effect { get; }
        // there has to be a better way to handle these args
        public BloodboundEffect(NetEntity target, string effect)
        {
            Target = target;
            Effect = effect;
        }
    }

    [DataField, AutoNetworkedField]
    public EntityUid? Bound;

    [DataField]
    public ProtoId<StatusIconPrototype> BloodBoundIcon = "BloodBoundFaction";

    [DataField]
    public TimeSpan? DeconversionStunTime = TimeSpan.FromSeconds(3);

    public override bool SessionSpecific => true;

}
