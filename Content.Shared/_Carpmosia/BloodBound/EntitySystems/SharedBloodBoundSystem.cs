using Content.Shared.Actions;
using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.Coordinates;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared.BloodBound.EntitySystems;

public abstract partial class SharedBloodBoundSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private const string BloodBoundXmlGeneratedName = "BloodBoundRitualUserInterface";
    [SubscribeLocalEvent]
    private void SummonAltar(Entity<InitialBloodBoundComponent> entity, ref BloodBoundComponent.SummonAltarRequest args)
    {
        EntityUid altar = PredictedSpawnAtPosition("BloodBoundAltar", entity.Owner.ToCoordinates());
    }

    [SubscribeLocalEvent]
    private void OnBloodBoundRitual(Entity<BloodBoundComponent> entity, ref BloodBoundRitualActionEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(entity, out var userInterfaceComp))
            return;

        if (!HasComp<BloodBoundComponent>(entity))
            return;

        if (!_ui.IsUiOpen((entity, userInterfaceComp), BloodBoundRitualUiKey.Key, args.Performer))
        {
            _ui.OpenUi((entity, userInterfaceComp), BloodBoundRitualUiKey.Key, args.Performer);
        }
    }

    [SubscribeLocalEvent]
    private void OnBloodBoundMapInit(Entity<BloodBoundComponent> entity, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(entity, ref entity.Comp.RitualActionEntity, entity.Comp.RitualAction);

        var userInterfaceComp = EnsureComp<UserInterfaceComponent>(entity);
        _ui.SetUi((entity, userInterfaceComp), BloodBoundRitualUiKey.Key, new InterfaceData(BloodBoundXmlGeneratedName));

        Dirty(entity);
    }

    [SubscribeLocalEvent]
    private void OnInitialBloodBoundMapInit(Entity<InitialBloodBoundComponent> entity, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(entity, ref entity.Comp.ConvertActionEntity, entity.Comp.ConvertAction);
        _actionsSystem.AddAction(entity, ref entity.Comp.CheckConvertActionEntity, entity.Comp.CheckConvertAction);
        _actionsSystem.AddAction(entity, ref entity.Comp.BloodyGlareActionEntity, entity.Comp.BloodyGlareAction);
        Dirty(entity);
    }

    [SubscribeLocalEvent]
    private void OnInitialBloodBoundShutdown(Entity<InitialBloodBoundComponent> entity, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(entity.Comp.ConvertActionEntity);
        _actionsSystem.RemoveAction(entity.Comp.CheckConvertActionEntity);
        _actionsSystem.RemoveAction(entity.Comp.BloodyGlareActionEntity);
    }

    [SubscribeLocalEvent]
    private void OnBloodBoundAttemptGetState(
        Entity<BloodBoundComponent> entity,
        ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not {} uid)
            return true;

        return HasComp<BloodBoundComponent>(uid) || HasComp<ShowAntagIconsComponent>(uid);
    }
}
