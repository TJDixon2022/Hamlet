using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 283, task 2: the paragraph that was never on the status bar.
/// </summary>
/// <remarks>
/// <para>**UNITS 280, 281 AND 282 ALL SWEPT THE STATUS BAR AND THIS IS NOT ON IT.**
/// `LinkCheckLine` is drawn in the top strip, under the frequency readout,
/// permanently and on every tab. Its longest branch is **304 characters**, and it is
/// the branch this operator's own radio takes: `CivTransceive` is off, and HM-DEC-138
/// measured 5,499 frames in sixty-one seconds with `inboundTransceive` zero.</para>
/// <para>**TWO OF THE FIVE BRANCHES ARE FAULTS AND STILL SPEAK.** A stale frequency
/// and a frequency nobody has heard are the two things this class was built to say
/// out loud — a display that is current about a value that is not is §0.0 broken by
/// omission, and it cost two builds. The other three are Hamlet describing itself
/// working, and one of them ends in advice about a setting he could change.</para>
/// </remarks>
public sealed class TheTopStripStopsLecturingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each branch and what it draws are printed.</param>
    public TheTopStripStopsLecturingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The 304-character branch draws nothing and is all on the mark.**</summary>
    /// <remarks>
    /// Watched failing first: bound to `LinkCheckLine`, the top strip drew the whole
    /// paragraph on every tab.
    /// </remarks>
    [AvaloniaFact]
    public void TheRadioThatDoesNotAnnounceSaysNothingOnTheStrip()
    {
        var check = LinkSelfCheck.Describe(null, Rig(transceive: 0), Now, true);

        _output.WriteLine("headline : " + check.Headline.Length + " characters");
        _output.WriteLine("concern  : [" + check.Concern + "]");

        Assert.True(
            check.Headline.Length > 300,
            "this is meant to be the long branch and it is "
            + check.Headline.Length + " characters");

        Assert.Equal("", check.Concern);

        foreach (var mode in new[] { "CW", "Digital", "Voice" })
        {
            var window = Strip(mode, check);

            var drawn = Named<TextBlock>(window, "LinkCheckConcernText");
            var mark = Named<HintMarkControl>(window, "LinkCheckMark");
            var tip = ToolTip.GetTip(mark) as string ?? "";

            _output.WriteLine(
                mode.PadRight(8) + "strip draws [" + (drawn.Text ?? "")
                + "], mark holds " + tip.Length);

            Assert.False(drawn.IsEffectivelyVisible);

            // **NOTHING IS DELETED** (§0.0, HM-DEC-092). Every word is one hover
            // away, including the advice about CI-V Transceive.
            Assert.Contains(check.Headline, tip, StringComparison.Ordinal);
            Assert.Contains("CI-V", tip, StringComparison.Ordinal);
        }
    }

    /// <summary>**A stale frequency still speaks, and only the stale part.**</summary>
    /// <remarks>
    /// The reason this class exists. He turned the dial, Hamlet followed thirty
    /// seconds later, and the number was drawn confidently four times a second while
    /// being a minute old.
    /// </remarks>
    [AvaloniaFact]
    public void AStaleFrequencyIsReadWithoutHovering()
    {
        var read = Now - TimeSpan.FromSeconds(45);

        var state = RigState.Empty.With(RigValue.Known(
            RigField.Frequency, 14_074_000, "14.074000", read, "poll"));

        var check = LinkSelfCheck.Describe(null, state, Now, true);

        _output.WriteLine("headline : " + check.Headline);
        _output.WriteLine("concern  : " + check.Concern);

        Assert.NotEqual("", check.Concern);
        Assert.Contains("old", check.Concern, StringComparison.Ordinal);

        var window = Strip("Digital", check);
        var drawn = Named<TextBlock>(window, "LinkCheckConcernText");

        Assert.True(drawn.IsEffectivelyVisible);
        Assert.Equal(check.Concern, drawn.Text);
    }

    /// <summary>**A frequency nobody has heard still speaks.**</summary>
    [AvaloniaFact]
    public void AFrequencyNobodyHasHeardIsReadWithoutHovering()
    {
        var check = LinkSelfCheck.Describe(null, RigState.Empty, Now, true);

        _output.WriteLine("concern : " + check.Concern);

        Assert.Contains("has not heard", check.Concern, StringComparison.Ordinal);

        var window = Strip("Digital", check);
        var drawn = Named<TextBlock>(window, "LinkCheckConcernText");

        Assert.True(drawn.IsEffectivelyVisible);
        Assert.Equal(check.Concern, drawn.Text);
    }

    /// <summary>**A radio that is keeping up says nothing at all.**</summary>
    /// <remarks>
    /// The narration branches, both of them. A line that congratulates itself on
    /// every poll is one nobody reads by the third time.
    /// </remarks>
    [AvaloniaFact]
    public void ARadioThatIsKeepingUpSaysNothing()
    {
        foreach (var transceive in new[] { 0, 1 })
        {
            var check = LinkSelfCheck.Describe(null, Rig(transceive), Now, true);

            _output.WriteLine(
                "transceive " + transceive + " -> headline "
                + check.Headline.Length + ", concern [" + check.Concern + "]");

            Assert.NotEqual("", check.Headline);
            Assert.Equal("", check.Concern);
        }
    }

    /// <summary>The moment every case is measured at.</summary>
    private static DateTime Now { get; }
        = new(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

    /// <summary>A rig state with a fresh frequency and a stated transceive.</summary>
    /// <param name="transceive">1 where the radio announces its own changes.</param>
    /// <returns>The state.</returns>
    private static RigState Rig(int transceive)
        => RigState.Empty
            .With(RigValue.Known(
                RigField.Frequency, 14_074_000, "14.074000", Now, "poll"))
            .With(RigValue.Known(
                RigField.CivTransceive, transceive,
                transceive > 0 ? "on" : "off", Now, "poll"));

    /// <summary>
    /// A realized window whose strip is showing one link check.
    /// </summary>
    /// <param name="mode">Which tab.</param>
    /// <param name="check">The check the strip should be drawing.</param>
    /// <returns>The shown window.</returns>
    /// <remarks>
    /// **THE VIEW MODEL COMPOSES ITS OWN CHECK FROM THE RIG IT HOLDS**, and a test
    /// cannot hand it one without a radio. So the strip is asserted against the
    /// check's own two strings and the bindings are asserted against the seam the
    /// view model exposes — which is what decides what is drawn.
    /// </remarks>
    private static Window Strip(string mode, LinkCheck check)
    {
        var panel = new MainWindowViewModel(
            HowMuchTheApplicationSaysTests.Settled(), null)
        {
            OperatingMode = mode,
        };

        panel.UseLinkCheckForTests(check);

        var window = new MainWindow { DataContext = panel };

        window.Show();
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
            "no " + typeof(T).Name + " named " + name + " on the realized window");

        return found!;
    }
}
