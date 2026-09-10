using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 305 task 3: **the slot clock is on screen whether or not any
/// card exists.**
/// </summary>
/// <remarks>
/// <para>**IT USED TO LIVE INSIDE A CARD.** The countdown was in the item template of
/// `DigitalContactCards`, whose `ItemsSource` is the card list **and** which carries
/// `IsVisible="{Binding HasDigitalCards}"` - so with no cards it could not draw for
/// two reasons rather than one. On the evening the operator pressed CQ and nobody
/// answered, the thing he was watching to decide whether anybody had come back was
/// not on the screen at all.</para>
/// <para>**THE SLOT CLOCK IS ALWAYS KNOWABLE** (unit 286). It derives from the system
/// clock and the measured offset, and neither waits on a decode.</para>
/// <para>**THESE ARE COMPUTED PLACEMENTS, NOT SEEN ONES.** Nothing in this repository
/// can look at a picture; the window is built headless and the visual tree is walked,
/// which says a control exists, is visible and is where it is - and says nothing about
/// what it looks like.</para>
/// </remarks>
public sealed class TheSlotClockTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheSlotClockTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The countdown is on screen with no cards at all.**</summary>
    [AvaloniaFact]
    public void TheCountdownIsOnScreenWithNoCards()
    {
        var window = Standing(out var model);

        _output.WriteLine("cards on the panel : " + model.DigitalCards.Count);

        var clock = Find(window, "SlotClock");

        _output.WriteLine("slot clock found   : " + (clock is not null));
        _output.WriteLine("visible            : " + (clock?.IsVisible ?? false));
        _output.WriteLine("count reads        : " + model.TurnRingCount);

        // **NO CARDS**, which is the state the fault was found in.
        Assert.Empty(model.DigitalCards);

        // **AND THE CLOCK IS THERE ANYWAY.**
        Assert.NotNull(clock);
        Assert.True(clock!.IsVisible, "the slot clock is not visible");

        // **AND IT IS NOT INSIDE THE CARD LIST**, which is the whole move.
        Assert.DoesNotContain(
            clock.GetVisualAncestors().OfType<Control>(),
            c => c.Name == "DigitalContactCards");
    }

    /// <summary>**It runs from the same boundary the send gate is compared against.**</summary>
    /// <remarks>
    /// **TWO CLOCKS DISAGREEING BY A SECOND IS WORSE THAN ONE CLOCK**, and the
    /// operator is watching this one to decide whether anybody answered. The number
    /// on the face is asserted against `Ft8Turn.Read` computed independently from the
    /// same offset and grid - the values the armed send is measured against.
    /// </remarks>
    [AvaloniaFact]
    public void TheCountdownRunsFromTheSameSlotBoundaryAsTheSendGate()
    {
        var window = Standing(out var model);

        var offset = new ClockOffset(0.033, DateTime.UtcNow);

        model.ClockOffset = offset;
        model.RefreshTurnForTests();

        var onScreen = Find(window, "SlotClockCountText") as TextBlock;

        var gate = Ft8Turn.Read(
            DateTime.UtcNow, offset, theirLastSlotUtc: null);

        _output.WriteLine("what the face reads : " + onScreen?.Text);
        _output.WriteLine("what the gate reads : " + gate.CountText);
        _output.WriteLine("state               : " + gate.State);

        Assert.NotNull(onScreen);

        // **THE SAME SECOND, WITHIN THE ONE THE CLOCK MOVED THROUGH.** Both were
        // read from the wall clock a few milliseconds apart, so a difference of one
        // is the second turning over and anything larger is two clocks.
        var shown = int.Parse(
            onScreen!.Text ?? "0", CultureInfo.InvariantCulture);

        var expected = int.Parse(
            gate.CountText.Length > 0 ? gate.CountText : "0",
            CultureInfo.InvariantCulture);

        Assert.True(
            Math.Abs(shown - expected) <= 1,
            "the face reads " + shown + " and the gate reads " + expected);
    }

    /// <summary>**It survives the card panel being collapsed.**</summary>
    [AvaloniaFact]
    public void TheCountdownSurvivesTheCardPanelBeingCollapsed()
    {
        var window = Standing(out var model);

        model.DigitalDecodedExpanded = false;

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var clock = Find(window, "SlotClock");

        _output.WriteLine("panel expanded  : " + model.DigitalDecodedExpanded);
        _output.WriteLine("slot clock here : " + (clock?.IsVisible ?? false));

        Assert.NotNull(clock);
        Assert.True(
            clock!.IsVisible,
            "the slot clock went away with the panel it is supposed to sit above");
    }

    /// <summary>One control by name, anywhere in the window.</summary>
    private static Control? Find(Window window, string name)
        => window.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(c => c.Name == name);

    /// <summary>The real window, standing, on the Digital tab, with nothing decoded.</summary>
    private static MainWindow Standing(out MainWindowViewModel model)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        var window = new MainWindow { DataContext = model };

        window.Show();

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return window;
    }
}
