using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 282, task 1: the paragraph three sweeps walked past.
/// </summary>
/// <remarks>
/// <para>**IT IS NOT A STRING ANYBODY CAN GREP FOR.** It is composed at run time by
/// `ReceiverSetupVoice.Say` out of the rows of `mode-receiver-conditions.json`, whose
/// only literal text in `src/` is XML doc comments — **so a source search returns
/// empty and reads exactly like a clean sweep.** That is §12.5, and it is why units
/// 280 and 281 both measured the status bar honestly and neither saw this: on a
/// headless training radio no receiver setup ever runs, so the bar was holding a
/// short line at measurement time and an 884-character paragraph in his shack.</para>
/// <para>**SO THIS DRIVES THE COMPOSER RATHER THAN THE SCREEN'S IDLE STATE**, and
/// then reads the realized bar.</para>
/// </remarks>
public sealed class TheStatusBarStopsLecturingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the paragraph and its length are printed.</param>
    public TheStatusBarStopsLecturingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The paragraph is real, and this is how long it is.**</summary>
    /// <remarks>
    /// The unit's `NUMBER`. Measured from the shipped conditions file rather than
    /// quoted from the instruction, because a figure nobody re-derived is a figure
    /// that goes stale the first time a row is added.
    /// </remarks>
    [AvaloniaFact]
    public void TheComposedParagraphIsMeasuredFromTheShippedRows()
    {
        foreach (var mode in new[] { "CW", "FT8" })
        {
            var line = ReceiverSetupVoice.Say(AllChanged(mode));

            _output.WriteLine(mode + ": " + line.Length + " characters");
            _output.WriteLine("  " + line);
            _output.WriteLine("");

            Assert.True(
                line.Length > 0,
                mode + " composes nothing, so the fixture is not exercising the "
                + "composer this task is about");
        }

        // **THE CW BLOCK IS THE ONE HE OPERATES ON** and it is the long one.
        Assert.True(
            ReceiverSetupVoice.Say(AllChanged("CW")).Length > 500,
            "the CW paragraph is not the length this task was written for; the "
            + "conditions file may have changed and the report's figure with it");
    }

    /// <summary>**None of it reaches the bar, and all of it is on the mark.**</summary>
    /// <remarks>
    /// Watched failing first: bound to `StatusText`, the bar drew the whole
    /// paragraph.
    /// </remarks>
    [AvaloniaFact]
    public void TheParagraphIsOnTheMarkAndNotOnTheBar()
    {
        var paragraph = ReceiverSetupVoice.Say(AllChanged("CW"));

        var window = Bar(panel => panel.NarrateForTests(paragraph, ""), out _);

        var text = Named<TextBlock>(window, "StatusBarText").Text ?? "";
        var mark = Named<HintMarkControl>(window, "StatusTipMark");
        var tip = ToolTip.GetTip(mark) as string ?? "";

        _output.WriteLine("the bar draws  : [" + text + "]  (" + text.Length + ")");
        _output.WriteLine("the mark holds : " + tip.Length + " characters");

        Assert.Equal("", text);

        // **NOTHING IS DELETED** (§0.0, HM-DEC-092). Every word is one hover away.
        Assert.Contains(paragraph, tip, StringComparison.Ordinal);
    }

    /// <summary>**A fault still speaks unasked.**</summary>
    /// <remarks>
    /// The single exception to the ruling, and the one that could have gone wrong
    /// quietly: a plain assignment is a fault by default, so a site nobody
    /// classified stays on the screen.
    /// </remarks>
    [AvaloniaFact]
    public void AFaultIsStillReadWithoutHovering()
    {
        const string Wrong = "No answer on COM7. It is usually the cable, the baud "
            + "rate or the port number.";

        var window = Bar(panel => panel.StatusText = Wrong, out _);

        var text = Named<TextBlock>(window, "StatusBarText").Text ?? "";

        _output.WriteLine("the bar draws: " + text);

        Assert.Equal(Wrong, text);
    }

    /// <summary>
    /// **An admission inside a narration is not carried off with it.**
    /// </summary>
    /// <remarks>
    /// <para>`ReceiverSetupVoice.Say` composes one string out of five kinds of
    /// clause, and three of them are Hamlet saying it does not know something.
    /// **Moving the whole string to a hover would have hidden all three**, which is
    /// §0.0 broken by omission and what the instruction forbids in as many
    /// words.</para>
    /// <para>Watched failing first: with `Narrate(say)` and no second argument, a
    /// run in which the radio confirmed nothing drew an empty bar.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AnAdmissionInsideTheParagraphStillSpeaks()
    {
        var results = AllChanged("CW").ToList();

        // One condition the radio would not confirm, in among eight that worked.
        results[3] = results[3] with { Outcome = ConditionOutcome.NotConfirmed };

        var whole = ReceiverSetupVoice.Say(results);
        var admissions = ReceiverSetupVoice.Admissions(results);

        _output.WriteLine("the whole line : " + whole.Length + " characters");
        _output.WriteLine("of which spoken: " + admissions.Length + " characters");
        _output.WriteLine("  " + admissions);

        Assert.NotEqual("", admissions);
        Assert.DoesNotContain("turned the auto notch off", admissions, StringComparison.Ordinal);

        var window = Bar(panel => panel.NarrateForTests(whole, admissions), out _);

        var text = Named<TextBlock>(window, "StatusBarText").Text ?? "";
        var tip = ToolTip.GetTip(Named<HintMarkControl>(window, "StatusTipMark")) as string ?? "";

        // The admission is read without hovering...
        Assert.Equal(admissions, text);

        // ...and the whole line, narration included, is still behind the mark.
        Assert.Contains(whole, tip, StringComparison.Ordinal);
        Assert.True(
            tip.Length > text.Length,
            "the hover is no longer than the bar, so the narration was lost");
    }

    /// <summary>Every condition of one mode, all of them changed.</summary>
    /// <param name="mode">CW or FT8.</param>
    /// <returns>The results the voice composes from.</returns>
    /// <remarks>
    /// **FROM THE SHIPPED FILE, NOT FROM A HAND-WRITTEN LIST** (§12.5). A fixture
    /// built from the same assumption as the code proves nothing about the code, and
    /// the whole reason this paragraph survived three sweeps is that nobody measured
    /// what the real rows compose to.
    /// </remarks>
    private static IReadOnlyList<ConditionResult> AllChanged(string mode)
        => ReceiverConditions.ForMode(mode)
            .Select(c => new ConditionResult(c, ConditionOutcome.Changed, "", ""))
            .ToList();

    /// <summary>A realized main window with the status line in a given state.</summary>
    /// <param name="set">What to do to the panel before the layout is pumped.</param>
    /// <param name="panel">The panel, so the caller can keep it alive.</param>
    /// <returns>The shown window.</returns>
    private static Window Bar(Action<MainWindowViewModel> set, out MainWindowViewModel panel)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        panel = new MainWindowViewModel(settings, null) { OperatingMode = "CW" };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        set(panel);

        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
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
            "no " + typeof(T).Name + " named " + name + " on the realized window. "
            + "Found: [" + string.Join(", ", window.GetVisualDescendants().OfType<T>()
                .Where(c => c.Name is not null).Select(c => c.Name)) + "]");

        return found!;
    }
}
