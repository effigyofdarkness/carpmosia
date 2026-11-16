using Robust.Client.UserInterface;
using Content.Client.UserInterface.Controls;
using JetBrains.Annotations;
using Content.Shared.BloodBound.Components;
using Content.Client.Stylesheets.Palette;
using Robust.Shared.Utility;

namespace Content.Client.BloodBound.EntitySystems;

[UsedImplicitly]
public sealed partial class BloodBoundRitualUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        Update();
        _menu.OpenOverMouseScreenPosition();
    }

    public override void Update()
    {
        if (_menu == null) return;

        var options = CreateRitualOptions();
        _menu.SetButtons(options);

        base.Update();
    }

    private void DoRitual(string action)
    {
        switch (action)
        {
            case "summon-altar":
                SendPredictedMessage(new BloodBoundComponent.SummonAltarRequest());
                break;
            default:
                break;
        }
    }

    private IEnumerable<RadialMenuOptionBase> CreateRitualOptions()
    {
        var buttons = new List<RadialMenuOptionBase>();

        var summonAltar = new RadialMenuActionOption<string>(DoRitual, "summon-altar")
        {
            IconSpecifier = RadialMenuIconSpecifier.With("AltarChaos"),
            ToolTip = Loc.GetString("blood-bound-summon-altar"),
            BackgroundColor = Palettes.Slate.Element,
            HoverBackgroundColor = Palettes.Red.Element
        };
        buttons.Add(summonAltar);

        var createCandles = new RadialMenuActionOption<string>(DoRitual, "create-candles")
        {
            IconSpecifier = RadialMenuIconSpecifier.With("Candle"),
            ToolTip = Loc.GetString("blood-bound-create-candles"),
            BackgroundColor = Palettes.Slate.Element,
            HoverBackgroundColor = Palettes.Red.Element
        };
        buttons.Add(createCandles);

        var initiateRitual = new RadialMenuActionOption<string>(DoRitual, "start-ritual")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Texture(new ResPath("Interface/Alerts/bleed.rsi"))),
            ToolTip = Loc.GetString("blood-bound-start-ritual"),
            BackgroundColor = Palettes.Slate.Element,
            HoverBackgroundColor = Palettes.Red.Element
        };
        buttons.Add(initiateRitual);

        return buttons;
    }
}