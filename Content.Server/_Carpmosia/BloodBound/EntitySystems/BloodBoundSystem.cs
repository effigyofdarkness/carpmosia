using System.Linq;
using Robust.Shared.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Roles;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.Roles.Components;
using Content.Shared.Jittering;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Server.Stunnable;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Shared.Mobs.Systems;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Coordinates;

namespace Content.Server.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private MindSystem _mindSystem = default!;
    [Dependency] private RoleSystem _roleSystem = default!;
    [Dependency] private TargetObjectiveSystem _targetObjectiveSystem = default!;
    [Dependency] private StunSystem _stunSystem = default!;
    [Dependency] private SharedJitteringSystem _jittering = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private StutteringSystem _stuttering = default!;
    [Dependency] private PopupSystem _popupSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private ActionsSystem _actionsSystem = default!;

    [SubscribeLocalEvent]
    private void OnBloodBoundShutdown(Entity<BloodBoundComponent> entity, ref ComponentShutdown args)
    {
        if (!_mindSystem.TryGetMind(entity, out var mindId, out var mind))
            return;

        if (_roleSystem.MindHasRole<BloodBoundRoleComponent>(mindId, out var role))
        {
            // Initial no longer has to worry about keeping the converted alive or on the shuttle
            if (role.Value.Comp2.Bound != null &&
                _mindSystem.TryGetMind(role.Value.Comp2.Bound.Value, out _, out var boundMind))
            {
                foreach (var objective in boundMind.Objectives)
                {
                    if (!HasComp<BloodBoundTargetComponent>(objective))
                        continue;

                    _targetObjectiveSystem.SetTarget(objective, EntityUid.Invalid);
                }
            }

            _roleSystem.MindRemoveRole<BloodBoundRoleComponent>(mindId);
        }

        int? objectiveToRemove = null;

        var i = 0;
        foreach (var objective in mind.Objectives)
        {
            if (HasComp<ConvertedBloodBoundObjectiveComponent>(objective))
            {
                objectiveToRemove = i;
                break;
            }

            i++;
        }

        if (objectiveToRemove != null)
            _mindSystem.TryRemoveObjective(mindId, mind, objectiveToRemove.Value);
    }

    [SubscribeLocalEvent]
    private void OnBloodBoundGlare(Entity<InitialBloodBoundComponent> entity,
        ref BloodBoundGlareActionEvent args)
    {
        if (!CanGlare(entity, args.Target, out var reason))
        {
            _popupSystem.PopupEntity(
                Loc.GetString(reason, ("target", Identity.Entity(args.Target, _entityManager))),
                args.Target,
                entity,
                PopupType.MediumCaution
            );
            return;
        }

        if (entity.Comp.GlareStunTime != null)
        {
            _stunSystem.TryUpdateParalyzeDuration(args.Target, entity.Comp.GlareStunTime);
            _jittering.DoJitter(args.Target, (TimeSpan) entity.Comp.GlareStunTime, true, 10, 5, false);
            _stuttering.DoStutter(args.Target, (TimeSpan) entity.Comp.GlareStunTime, false);
        }
        _damageable.ChangeDamage(
            (args.Target, _entityManager.GetComponent<DamageableComponent>(args.Target)),
            new DamageSpecifier(ProtoMan.Index((ProtoId<DamageTypePrototype>)"Slash"), 25),
            true
        );
        if (entity.Comp.GlareStunTime != null) _actionsSystem.SetCooldown(entity.Comp.BloodyGlareActionEntity, (TimeSpan) entity.Comp.GlareStunTime);
        RaiseNetworkEvent(new BloodBoundComponent.BloodboundEffect(args.Target, null, "Glare"));
        _popupSystem.PopupEntity(
            Loc.GetString(
                Loc.GetString("blood-bound-glare"),
                ("initial", Identity.Entity(entity, _entityManager)),
                ("target", Identity.Entity(args.Target, _entityManager))
            ),
            args.Target,
            PopupType.LargeCaution);
        SpawnAtPosition("BloodCrystal", args.Target.ToCoordinates());
    }

    private bool CanGlare(Entity<InitialBloodBoundComponent> entity,
        EntityUid target,
        [NotNullWhen(false)] out string? reason)
    {
        if (!TryComp<BloodBoundComponent>(target, out var convertedComp))
        {
            reason = "blood-bound-glare-failed-not-bound";
            return false;
        };
        if (convertedComp.Bound != entity.Owner)
        {
            reason = "blood-bound-glare-failed-not-bound";
            return false;
        }
        if (!_mobStateSystem.IsAlive(target))
        {
            reason = "blood-bound-glare-failed-not-alive";
            return false;
        }
        reason = "";
        return true;
    }
}
