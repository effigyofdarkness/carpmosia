using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Animations;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Random;
using System.Diagnostics;

namespace Content.Client.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private AnimationPlayerSystem _animation = default!;

    // copied from Content.Client/Weapons/Melee/MeleeWeaponSystem.Effects.cs
    private Animation GetFadeAnimation(SpriteComponent sprite, float start, float end)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(end),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Color, start),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), end)
                    }
                }
            }
        };
    }

    private void BloodboundGlareEffect(EntityCoordinates coords)
    {
        // todo: change to blodobound aura
        for (int i = 0; i < 10; i++)
        {
            EntityUid spreadEffect = SpawnAttachedTo("JetpackEffect", coords.Offset(new System.Numerics.Vector2(_random.NextFloat(), _random.NextFloat())));
            Debug.Assert(TryComp<SpriteComponent>(spreadEffect, out var sprite));
            _animation.Play(spreadEffect, GetFadeAnimation(sprite, 0, 1), "effect-fade");
        }
    }

    [SubscribeNetworkEvent]
    private void BloodboundEffect(BloodBoundComponent.BloodboundEffect args)
    {
        switch (args.Effect)
        {
            default:
            case "Glare":
                BloodboundGlareEffect(_entityManager.GetEntity(args.Target).ToCoordinates());
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
