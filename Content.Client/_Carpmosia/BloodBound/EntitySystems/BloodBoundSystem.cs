using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Animations;
using Robust.Client.Player;
using Robust.Shared.Random;

namespace Content.Client.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private AnimationPlayerSystem _animation = default!;
    [Dependency] private SpriteSystem _sprite = default!;

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

    private void BloodboundGlareEffect(EntityUid target)
    {
        for (int i = 0; i < 20; i++)
        {
            EntityUid spreadEffect = SpawnAttachedTo("BloodGlareAura",
                target.ToCoordinates().Offset(
                    new System.Numerics.Vector2(_random.NextFloat(-1 / 2, 1 / 2), _random.NextFloat(-1 / 2, 1 / 2))
                )
            );
            TryComp<SpriteComponent>(spreadEffect, out var sprite);
            if (sprite == null) continue;
            int selectedIteration = _random.Next(1, 3);
            _sprite.LayerSetRsiState((spreadEffect, sprite), 0, "aura-iteration-" + selectedIteration.ToString());
            _animation.Play(spreadEffect, GetFadeAnimation(sprite, 0, 1), "effect-fade");
        }
    }

    private void BloodboundAltarSpawnEffect(EntityUid target)
    {
        
    }

    [SubscribeNetworkEvent]
    private void BloodboundEffect(BloodBoundComponent.BloodboundEffect args)
    {
        _entityManager.TryGetEntity(args.Target, out var target);
        if (target == null) return;
        switch (args.Effect)
        {
            case "Glare":
                BloodboundGlareEffect(target.Value);
                break;
            case "AltarSpawn":
                BloodboundAltarSpawnEffect(target.Value);
                break;
            default:
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
