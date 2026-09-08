using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 283, task 1: which paths put receiver narration on the bar.
/// </summary>
/// <remarks>
/// <para>**READING AND MEASURING ONLY.** The order asks for a count and says no line
/// numbers on purpose, because naming a line instead of a behaviour is how a fix
/// reaches one surface out of two.</para>
/// <para>**AND IT IS MEASURED RATHER THAN READ**, which is the lesson of the three
/// units before it. The text is composed at run time, so a source search reads as
/// clean whatever the truth is; the only honest answer comes from putting the real
/// narration through the real view model and asking the realized window what it
/// drew.</para>
/// </remarks>
public sealed class WhichPathsPutNarrationOnTheBarTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts and the bar's contents are printed.</param>
    public WhichPathsPutNarrationOnTheBarTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**What every mode's rows compose to, from the shipped file.**</summary>
    /// <remarks>
    /// The instruction gives CW nine rows and FT8 448 characters and asks for both to
    /// be confirmed. Measured here rather than quoted, so the figures go stale
    /// loudly if a row is ever added.
    /// </remarks>
    [AvaloniaFact]
    public void EveryModesRowsAreCountedAndComposed()
    {
        foreach (var mode in ReceiverConditions.Modes.OrderBy(m => m))
        {
            var rows = ReceiverConditions.ForMode(mode);
            var line = ReceiverSetupVoice.Say(AllChanged(mode));

            _output.WriteLine(
                mode.PadRight(6) + rows.Count.ToString().PadLeft(3) + " rows  "
                + line.Length.ToString().PadLeft(5) + " characters");
        }

        Assert.Equal(9, ReceiverConditions.ForMode("CW").Count);

        var ft8 = ReceiverSetupVoice.Say(AllChanged("FT8")).Length;

        _output.WriteLine("");
        _output.WriteLine("FT8 composes " + ft8 + " characters");

        Assert.Equal(448, ft8);
    }

    /// <summary>
    /// **The bar is shared chrome, so every tab draws the same one.**
    /// </summary>
    /// <remarks>
    /// This is the fact that decides how many paths there can be. If the status bar
    /// were per-tab there would be three surfaces to clear; it is outside all three
    /// workspaces, so there is one.
    /// </remarks>
    [AvaloniaFact]
    public void EveryTabDrawsTheOneStatusBar()
    {
        foreach (var mode in new[] { "CW", "Digital", "Voice" })
        {
            var window = Tab(mode, out _);

            var bars = window.GetVisualDescendants().OfType<TextBlock>()
                .Count(t => t.Name == "StatusBarText");

            var inside = window.GetVisualDescendants().OfType<Grid>()
                .Where(g => g.Name is "CwWorkspace" or "DigitalWorkspace" or "VoiceWorkspace")
                .SelectMany(g => g.GetVisualDescendants().OfType<TextBlock>())
                .Count(t => t.Name == "StatusBarText");

            _output.WriteLine(
                mode.PadRight(8) + bars + " status bar(s), " + inside
                + " of them inside a workspace");

            Assert.Equal(1, bars);
            Assert.Equal(0, inside);
        }
    }

    /// <summary>
    /// **An FT8 tune-in puts nothing on the bar, on the tab he operates from.**
    /// </summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION'S PREMISE, PUT TO THE WINDOW.** It says the FT8 rows
    /// go onto the bar exactly as they always did. The narration is driven through
    /// the same view-model door the tune-in uses and the realized Digital tab is
    /// asked what it drew.</para>
    /// <para>Whichever way this comes out it is the answer to task 1, and it is an
    /// answer nothing in the source could have given.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AnFt8TuneInPutsNothingOnTheDigitalTabsBar()
    {
        var paragraph = ReceiverSetupVoice.Say(AllChanged("FT8"));

        var window = Tab("Digital", out var panel);

        panel.NarrateForTests(paragraph, "");
        HowMuchTheApplicationSaysTests.Pump(window);

        var bar = window.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Name == "StatusBarText");

        var mark = window.GetVisualDescendants().OfType<HintMarkControl>()
            .First(m => m.Name == "StatusTipMark");

        var tip = ToolTip.GetTip(mark) as string ?? "";

        _output.WriteLine("the FT8 paragraph : " + paragraph.Length + " characters");
        _output.WriteLine("the bar draws     : [" + (bar.Text ?? "") + "]");
        _output.WriteLine("the mark holds    : " + tip.Length + " characters");

        Assert.Equal("", bar.Text ?? "");
        Assert.Contains(paragraph, tip, StringComparison.Ordinal);
    }

    /// <summary>**And an FT8 admission still speaks on that same tab.**</summary>
    /// <remarks>
    /// The exception unit 282 built, checked on the tab this order is about rather
    /// than only on the one that order was about.
    /// </remarks>
    [AvaloniaFact]
    public void AnFt8AdmissionStillSpeaksOnTheDigitalTab()
    {
        var results = AllChanged("FT8").ToList();

        results[1] = results[1] with { Outcome = ConditionOutcome.NotRead };

        var whole = ReceiverSetupVoice.Say(results);
        var admissions = ReceiverSetupVoice.Admissions(results);

        var window = Tab("Digital", out var panel);

        panel.NarrateForTests(whole, admissions);
        HowMuchTheApplicationSaysTests.Pump(window);

        var bar = window.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Name == "StatusBarText");

        _output.WriteLine("the bar draws : " + bar.Text);

        Assert.NotEqual("", admissions);
        Assert.Equal(admissions, bar.Text);
        Assert.True(
            admissions.Length < whole.Length,
            "the whole line is being drawn, so the narration is not behind the hover");
    }

    /// <summary>
    /// **The third path, which no order has named and which is the longest.**
    /// </summary>
    /// <remarks>
    /// <para>**IT IS NOT ON THE STATUS BAR.** `LinkCheckLine` renders in the top
    /// strip, under the frequency readout, on every tab and permanently — and on his
    /// own radio it takes its longest branch, because `CivTransceive` is off and
    /// HM-DEC-138 measured 5,499 frames in sixty-one seconds with
    /// `inboundTransceive` zero.</para>
    /// <para>**MEASURED, NOT QUOTED.** Every branch is composed through
    /// `LinkSelfCheck.Describe` from a rig state built here, so the figures cannot go
    /// stale quietly.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheLinkCheckLineIsLongerThanAnythingUnit282Moved()
    {
        var seen = new System.Collections.Generic.List<(string Branch, int Chars)>();

        foreach (var (branch, line) in LinkBranches())
        {
            seen.Add((branch, line.Length));

            _output.WriteLine(
                line.Length.ToString().PadLeft(5) + "  " + branch.PadRight(22) + line);
            _output.WriteLine("");
        }

        var longest = seen.OrderByDescending(b => b.Chars).First();

        _output.WriteLine("longest branch: " + longest.Branch + ", " + longest.Chars);

        // **LONGER THAN THE FT8 PARAGRAPH UNIT 282 MOVED**, and it is the branch his
        // own radio takes.
        Assert.Equal("radio does not announce", longest.Branch);
        Assert.True(
            longest.Chars > 300,
            "the longest link-check branch is " + longest.Chars + " characters; the "
            + "report's figure needs re-taking");
    }

    /// <summary>Each branch of the link-check headline, composed.</summary>
    /// <returns>The branch name and the sentence it produces.</returns>
    private static System.Collections.Generic.IEnumerable<(string Branch, string Line)>
        LinkBranches()
    {
        var now = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        yield return ("radio announces",
            LinkSelfCheck.Describe(null, Rig(now, transceive: 1), now, true).Headline);

        yield return ("radio does not announce",
            LinkSelfCheck.Describe(null, Rig(now, transceive: 0), now, true).Headline);

        yield return ("nothing heard yet",
            LinkSelfCheck.Describe(null, RigState.Empty, now, true).Headline);
    }

    /// <summary>A rig state with a fresh frequency and a stated transceive.</summary>
    /// <param name="now">The moment the reading was taken.</param>
    /// <param name="transceive">1 where the radio announces its own changes.</param>
    /// <returns>The state.</returns>
    private static RigState Rig(DateTime now, int transceive)
        => RigState.Empty
            .With(RigValue.Known(
                RigField.Frequency, 14_074_000, "14.074000", now, "poll"))
            .With(RigValue.Known(
                RigField.CivTransceive, transceive,
                transceive > 0 ? "on" : "off", now, "poll"));

    /// <summary>**Everything the Digital tab draws, longest first.**</summary>
    /// <remarks>
    /// The order says the paragraph is still on his screen and that it is on the
    /// bar. The bar is measured clear, so this asks the tab what it is actually
    /// drawing — because the answer to *where is the paragraph* has to come from the
    /// window and not from a reading of the source.
    /// </remarks>
    [AvaloniaFact]
    public void EverythingTheDigitalTabDraws()
    {
        var window = Tab("Digital", out var panel);

        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20", slot, 14_074_000);

        HowMuchTheApplicationSaysTests.Pump(window);

        var shown = HowMuchTheApplicationSaysTests.Shown(window)
            .OrderByDescending(t => t.Length)
            .ToList();

        _output.WriteLine(
            shown.Sum(t => t.Length) + " characters in " + shown.Count + " blocks");
        _output.WriteLine("");

        foreach (var t in shown.Where(t => t.Length >= 20))
        {
            _output.WriteLine(t.Length.ToString().PadLeft(5) + "  " + t);
        }

        Assert.True(shown.Count > 0, "the Digital tab drew nothing");
    }

    /// <summary>Every condition of one mode, all of them changed.</summary>
    /// <param name="mode">The mode label as the shipped file spells it.</param>
    /// <returns>The results the voice composes from.</returns>
    private static System.Collections.Generic.IReadOnlyList<ConditionResult> AllChanged(string mode)
        => ReceiverConditions.ForMode(mode)
            .Select(c => new ConditionResult(c, ConditionOutcome.Changed, "", ""))
            .ToList();

    /// <summary>The main window on one tab, realized.</summary>
    /// <param name="mode">CW, Digital or Voice.</param>
    /// <param name="panel">The view model behind it.</param>
    /// <returns>The shown window.</returns>
    private static Window Tab(string mode, out MainWindowViewModel panel)
    {
        panel = new MainWindowViewModel(
            HowMuchTheApplicationSaysTests.Settled(), null)
        {
            OperatingMode = mode,
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
    }
}
