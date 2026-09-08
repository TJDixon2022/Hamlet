using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 284: the bar says what Hamlet could not do, and nothing else.
/// </summary>
/// <remarks>
/// <para>**FIVE ORDERS AIMED AT THIS PARAGRAPH AND EVERY ONE MISSED**, because each
/// was told where the text was and each location was wrong. So this stands the
/// application up in the state his screenshot shows — the simulated radio, the
/// Digital tab, an FT8 block — and asks the realized bar what it drew.</para>
/// <para>**THE CLAUSES ARE OF TWO KINDS AND THEY ARE IN ONE STRING.** *I could not
/// read the noise blanker, so I have not touched it* is an admission: Hamlet tried
/// and failed, and a fault speaks unasked. *The AGC usually wants to be slow here,
/// because…* is advice about a setting Hamlet has decided not to change. Moving the
/// second used to mean moving the first, which is why every sweep left both.</para>
/// <para>**IT DRIVES THE REAL PATH.** The rig is connected and the dial is set
/// through the view model's own commands, so what is asserted is what a tune-in
/// composes — not what a test seam was handed.</para>
/// </remarks>
public sealed class TheBarSaysWhatHamletCouldNotDoTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the bar and the hover are printed verbatim.</param>
    public TheBarSaysWhatHamletCouldNotDoTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The bar carries the admissions and none of the advice.**</summary>
    /// <remarks>
    /// Watched failing first against the tree as it stood: the bar carried both, and
    /// the red quoted the paragraph on his screen.
    /// </remarks>
    [AvaloniaFact]
    public async Task TheBarCarriesTheAdmissionsAndNoneOfTheAdvice()
    {
        var (window, panel) = await TunedToFt8Async();

        var bar = Named<TextBlock>(window, "StatusBarText").Text ?? "";
        var tip = ToolTip.GetTip(Named<HintMarkControl>(window, "StatusTipMark"))
            as string ?? "";

        _output.WriteLine("THE BAR RENDERS (" + bar.Length + " characters):");
        _output.WriteLine(bar);
        _output.WriteLine("");
        _output.WriteLine("THE HOVER HOLDS (" + tip.Length + " characters):");
        _output.WriteLine(tip);

        Assert.True(
            bar.Length > 0,
            "the bar is empty, so this fixture is not reproducing the state the "
            + "screenshot shows and nothing below proves anything");

        // **THE ADMISSIONS ARE ON THE BAR.** Hamlet tried to read a setting and
        // could not, and that is a fault (unit 282's rule, not undone here).
        Assert.Contains("could not read", bar, StringComparison.Ordinal);

        // **AND NONE OF THE ADVICE IS.** Both shapes `Cannot` produces: the one for
        // a control with a cited command whose value is unsettled, and the one for a
        // control Hamlet cannot reach at all.
        Assert.DoesNotContain("usually wants to be", bar, StringComparison.Ordinal);
        Assert.DoesNotContain("not settled well enough", bar, StringComparison.Ordinal);
        Assert.DoesNotContain("I cannot set from here", bar, StringComparison.Ordinal);
        Assert.DoesNotContain("scope span", bar, StringComparison.Ordinal);

        // **THE BAR NEVER CARRIES A REASON**, which is the structural form of the
        // same rule and does not depend on today's wording. Every advice clause
        // explains why - `Did` and `Cannot` both join their reason with *because* -
        // and neither admission shape has one: *I could not read the auto notch, so
        // I have not touched it* says what failed and stops. **A sixth kind of
        // clause arriving on the bar with a reason attached fails here** even if
        // nobody thinks to add a phrase for it.
        Assert.DoesNotContain(" because ", bar, StringComparison.Ordinal);

        // And every sentence on it is one of the two admission shapes.
        foreach (var sentence in bar.Split(". ", StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.True(
                sentence.StartsWith("I could not read the", StringComparison.Ordinal)
                || sentence.StartsWith("I asked for the", StringComparison.Ordinal),
                "the bar carries a sentence that is not an admission: \"" + sentence + "\"");
        }

        GC.KeepAlive(panel);
    }

    /// <summary>**Every advice clause arrived on the hover.**</summary>
    /// <remarks>
    /// §0.0 and HM-DEC-092: a sentence moved behind a hover and a sentence deleted
    /// look identical on the screen, and only one of them is allowed.
    /// </remarks>
    [AvaloniaFact]
    public async Task EveryAdviceClauseIsOnTheHover()
    {
        var (window, panel) = await TunedToFt8Async();

        var bar = Named<TextBlock>(window, "StatusBarText").Text ?? "";
        var tip = ToolTip.GetTip(Named<HintMarkControl>(window, "StatusTipMark"))
            as string ?? "";

        _output.WriteLine("bar   : " + bar.Length + " characters");
        _output.WriteLine("hover : " + tip.Length + " characters");

        foreach (var phrase in new[]
        {
            "usually wants to be",
            "not settled well enough for me to change it on your radio",
            "scope span",
            "could not read",
        })
        {
            _output.WriteLine("  hover carries \"" + phrase + "\": "
                + tip.Contains(phrase, StringComparison.Ordinal));

            Assert.Contains(phrase, tip, StringComparison.Ordinal);
        }

        // **THE HOVER IS THE WHOLE LINE**, admissions included, because a hover that
        // sometimes says nothing teaches somebody not to bother hovering.
        Assert.True(
            tip.Length > bar.Length,
            "the hover is no longer than the bar, so the advice did not arrive");

        GC.KeepAlive(panel);
    }

    /// <summary>**The whole tab measured in the state the screenshot shows.**</summary>
    /// <remarks>
    /// Task 4's number. The idle tab is not the surface this unit changed, so the
    /// measurement is taken with the radio connected and the tune-in composed —
    /// which is the state five units' worth of idle measurements could not see.
    /// </remarks>
    [AvaloniaFact]
    public async Task TheTabIsMeasuredInTheReproducedState()
    {
        var (window, panel) = await TunedToFt8Async();

        var shown = HowMuchTheApplicationSaysTests.Shown(window).ToList();

        _output.WriteLine(
            shown.Sum(t => t.Length) + " characters in " + shown.Count
            + " blocks, Digital tab, connected and tuned to the FT8 block");
        _output.WriteLine("");

        foreach (var t in shown.OrderByDescending(t => t.Length).Take(6))
        {
            _output.WriteLine(t.Length.ToString().PadLeft(5) + "  " + t);
        }

        Assert.True(shown.Count > 0, "the tab drew nothing");

        GC.KeepAlive(panel);
    }

    /// <summary>
    /// The application connected to the simulated radio, on the Digital tab, on FT8.
    /// </summary>
    /// <returns>The realized window and the panel behind it.</returns>
    /// <remarks>
    /// <para>**THE STATE THE SCREENSHOT SHOWS.** The training radio answers no
    /// setting read, so every owned condition comes back as one Hamlet could not
    /// read; the scope span has no cited command at all and comes back as one it
    /// cannot reach. Those are the two kinds in his paragraph.</para>
    /// <para>**THROUGH THE VIEW MODEL'S OWN COMMANDS.** Connecting and tuning are
    /// done the way the operator does them, because a fixture that assembles the
    /// state by hand is a fixture built from the same assumption as the code
    /// (§12.5) — which is how five units' worth of green tests sat beside this
    /// paragraph.</para>
    /// </remarks>
    private static async Task<(Window Window, MainWindowViewModel Panel)> TunedToFt8Async()
    {
        var panel = new MainWindowViewModel(
            HowMuchTheApplicationSaysTests.Settled(), null)
        {
            OperatingMode = "Digital",
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        panel.SelectedPort = MainWindowViewModel.TrainingRadio;

        // **THE TOGGLE IS A TOGGLE.** Pressing it on a panel that is already
        // connected disconnects, which is what the first run of this fixture did:
        // the bar read `Disconnected` and nothing was composed at all.
        if (!panel.IsConnected)
        {
            await panel.ToggleConnectCommand.ExecuteAsync(null);
        }

        HowMuchTheApplicationSaysTests.Pump(window);

        // **THE BAND IS SELECTED BEFORE THE DIAL IS SET.** `OnFrequencyHzChanged`
        // clamps the operator's own tuning to the band map on screen, so setting
        // 14.074 while the panel is on 40 m lands somewhere on 40 m instead — which
        // is what the second run of this fixture did, and the bar composed nothing.
        panel.SelectBandCommand.Execute(
            panel.Bands.First(b => b.Band.Name == "20 m"));

        HowMuchTheApplicationSaysTests.Pump(window);

        // 14.074 MHz is the FT8 block, which is the row the paragraph is composed
        // from and the frequency he was on.
        panel.FrequencyHz = 14_074_000;

        HowMuchTheApplicationSaysTests.Pump(window);

        await panel.FollowTheMapForTests();

        HowMuchTheApplicationSaysTests.Pump(window);

        return (window, panel);
    }

    /// <summary>One named control on the realized window.</summary>
    /// <typeparam name="T">What it should be.</typeparam>
    /// <param name="window">Where to look.</param>
    /// <param name="name">Its name in the markup.</param>
    /// <returns>The control.</returns>
    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "no " + typeof(T).Name + " named " + name + " on the realized window");

        return found!;
    }
}
