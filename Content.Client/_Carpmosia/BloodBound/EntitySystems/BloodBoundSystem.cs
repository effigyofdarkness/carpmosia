using System.Diagnostics;
using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Random;

namespace Content.Client.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<BloodBoundComponent.BloodboundEffect>(BloodboundEffect);
        base.Initialize();
    }

    private void BloodboundGlareEffect(EntityUid entity)
    {
        // todo: change to blodobound aura
        for (int i = 0; i < 10; i++) SpawnAttachedTo("JetpackEffect", entity.ToCoordinates().Offset(new System.Numerics.Vector2(_random.NextFloat(), _random.NextFloat())));
    }

    [SubscribeNetworkEvent]
    private void BloodboundEffect(BloodBoundComponent.BloodboundEffect args)
    {
        switch (args.Effect)
        {
            default:
            case "Glare":
                Debug.Assert(args.Target != null);
                BloodboundGlareEffect((EntityUid)args.Target);
                break;
        }
    }

    [SubscribeLocalEvent]
    private void OnBloodBoundGetIcons(Entity<BloodBoundComponent> entity, ref GetStatusIconsEvent args)
    {
        if (_playerManager.LocalSession?.AttachedEntity is { } playerEntity)
        {
            if (!HasComp<ShowAntagIconsComponent>(playerEntity) &&
                entity.Owner != playerEntity &&
                entity.Comp.Bound != playerEntity)
                return;
        }

        if (ProtoMan.TryIndex(entity.Comp.BloodBoundIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
