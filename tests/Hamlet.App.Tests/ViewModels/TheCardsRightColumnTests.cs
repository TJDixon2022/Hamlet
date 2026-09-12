using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 327 task 4: **the card's right column, and no caption under the map.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"the real estate to the right of the map is valuable"* - and
/// it was empty - and *"remove the info from below the map, I really never noticed it."*
/// </para>
/// <para>**ONLY WHAT HAMLET KNOWS FOR CERTAIN, AND ABSENT IS NOT DASHED** (§0.0). Every
/// line has its own visibility and a fact that is missing takes its line with it. **A dash
/// would claim a measurement was attempted and failed**, and a station who never sent a
/// grid is not a station whose grid failed to read.</para>
/// <para>**COMPUTED, NOT SEEN.** The words are read off the view model and the markup is
/// read off a realized headless window; nobody looked at the application.</para>
/// </remarks>
public sealed class TheCardsRightColumnTests
{
    /// <summary>Vienna, JN88 - a long path and a clear compass word from FN00.</summary>
    private const string FarGrid = "JN88";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the column is printed.</param>
    public TheCardsRightColumnTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: the six facts render from a full card.**
    /// </summary>
    /// <remarks>
    /// **THE ORDER IS THE INSTRUCTION'S AND SO IS THE CONTENT**: distance and compass,
    /// grid, his time by the sun, what he last said and when, how many messages, and why he
    /// is marked.
    /// </remarks>
    [Fact]
    public void TheSixFactsRenderFromAFullCard()
    {
        var card = Card(FarGrid, said: "73");

        card.UseNudge(new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2));

        foreach (var line in new[]
        {
            card.DistanceLine, card.GridLine, card.SolarTimeLine,
            card.LastHeardLine, card.MessageCountLine, card.NudgeLine,
        })
        {
            _output.WriteLine(line);
        }

        // **DISTANCE AND DIRECTION, ONE LINE, NO DEGREES** (HM-DEC-038).
        Assert.True(card.HasDistanceLine);
        Assert.Contains("miles", card.DistanceLine, StringComparison.Ordinal);
        Assert.Contains("·", card.DistanceLine, StringComparison.Ordinal);
        Assert.DoesNotContain("°", card.DistanceLine, StringComparison.Ordinal);

        Assert.True(card.HasGridLine);
        Assert.Equal("Grid " + FarGrid, card.GridLine);

        // **THE TIME SAYS WHAT IT IS.** A clock reading offered as *his local time* would
        // be a claim about a time zone Hamlet does not know (§0.0).
        Assert.True(card.HasSolarTimeLine);
        Assert.Contains("His time:", card.SolarTimeLine, StringComparison.Ordinal);
        Assert.Contains("by the sun", card.SolarTimeLine, StringComparison.Ordinal);

        // **A READING CARRIES ITS AGE** (HM-DEC-111).
        Assert.True(card.HasLastHeardLine);
        Assert.Contains("Last heard: 73", card.LastHeardLine, StringComparison.Ordinal);
        Assert.Contains("ago", card.LastHeardLine, StringComparison.Ordinal);

        Assert.True(card.HasMessages);
        Assert.Equal("1 message", card.MessageCountLine);

        Assert.True(card.HasNudgeLine);
        Assert.Equal("New country", card.NudgeLine);

        Assert.True(card.HasRightColumn);
    }

    /// <summary>
    /// **Assertion 2: a card with no grid renders without the grid line and without the
    /// time.**
    /// </summary>
    /// <remarks>
    /// **ABSENT, NOT DASHED** (§0.0). The solar time is worked out from his longitude, so a
    /// station with no grid has no time either, and neither line is drawn rather than drawn
    /// empty. The distance goes with them, for the same reason.
    /// </remarks>
    [Fact]
    public void ACardWithNoGridRendersWithoutTheGridLineAndWithoutTheTime()
    {
        var card = Card(grid: null, said: "73");

        _output.WriteLine("grid     : [" + card.GridLine + "] shown " + card.HasGridLine);
        _output.WriteLine("time     : [" + card.SolarTimeLine + "] shown " + card.HasSolarTimeLine);
        _output.WriteLine("distance : [" + card.DistanceLine + "] shown " + card.HasDistanceLine);

        Assert.False(card.HasGridLine);
        Assert.Empty(card.GridLine);

        Assert.False(card.HasSolarTimeLine);
        Assert.Empty(card.SolarTimeLine);

        Assert.False(card.HasDistanceLine);

        // **AND NOTHING ANYWHERE IN THE COLUMN IS A DASH.**
        foreach (var line in new[]
        {
            card.DistanceLine, card.GridLine, card.SolarTimeLine,
            card.LastHeardLine, card.MessageCountLine, card.NudgeLine,
        })
        {
            Assert.NotEqual("—", line.Trim());
            Assert.NotEqual("-", line.Trim());
        }

        // **AND A STATION WHO HAS SAID NOTHING HAS NO *last heard* EITHER.**
        var silent = Card(FarGrid, said: null);

        Assert.False(silent.HasLastHeardLine);
        Assert.Empty(silent.LastHeardLine);
    }

    /// <summary>
    /// **Assertion 3: a door's line names nothing, and the popup opens from it.**
    /// </summary>
    /// <remarks>
    /// **§3.1 HOLDS ON THE CARD AS WELL AS ON THE LIST.** *Would open a new area* is the
    /// category; the area is not in this object at all.
    /// </remarks>
    [Fact]
    public void ADoorsLineNamesNothingAndThePopupOpensFromIt()
    {
        var card = Card(FarGrid, said: "73");

        card.UseNudge(new NudgeReason(NudgeKind.Door, "", "", 0));

        _output.WriteLine("line  : " + card.NudgeLine);
        _output.WriteLine("reason: " + card.NudgeReasonLine);
        _output.WriteLine("earns : " + card.NudgeEarnsLine);

        Assert.Equal("Would open a new area", card.NudgeLine);
        Assert.Equal(NudgeWords.DoorReason, card.NudgeReasonLine);
        Assert.Equal(NudgeWords.Earns, card.NudgeEarnsLine);

        foreach (var continent in new[]
        {
            "Africa", "Antarctica", "Asia", "Europe", "North America", "Oceania",
            "South America",
        })
        {
            Assert.DoesNotContain(
                continent, card.NudgeLine + card.NudgeReasonLine,
                StringComparison.OrdinalIgnoreCase);
        }

        Assert.False(card.NudgeIsOpen);

        card.OpenTheNudgeCommand.Execute(null);

        Assert.True(card.NudgeIsOpen, "the card's reason line did not open the popup");

        card.CloseTheNudgeCommand.Execute(null);

        Assert.False(card.NudgeIsOpen);

        // **AND *confirmed* IS NOWHERE** (§4).
        Assert.DoesNotContain(
            "confirm",
            card.NudgeLine + card.NudgeReasonLine + card.NudgeEarnsLine,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Assertion 4: no caption is bound under the map, and the column is.**
    /// </summary>
    /// <remarks>
    /// **THE MARKUP IS THE CLAIM HERE AND IT IS READ OFF A REALIZED WINDOW.** The caption
    /// Tim asked to be removed was `Globe.Caption` under the small map on the card's face;
    /// the enlarged map keeps its own, where there is room for it and where he went looking
    /// for it.
    /// </remarks>
    [AvaloniaFact]
    public void NoCaptionIsBoundUnderTheMapAndTheColumnIs()
    {
        var markup = System.IO.File.ReadAllText(
            System.IO.Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var captions = Occurrences(markup, "Text=\"{Binding Globe.Caption}\"");

        _output.WriteLine("Globe.Caption is bound " + captions + " time(s) in MainWindow.axaml");

        // **ONE, AND IT IS THE ENLARGED MAP'S.** It was two before this unit: the card's
        // face and the popup.
        Assert.Equal(1, captions);

        foreach (var name in new[]
        {
            "CardRightColumn", "CardDistanceLine", "CardGridLine", "CardSolarTimeLine",
            "CardLastHeardLine", "CardMessageCountLine", "CardNudgeLine",
        })
        {
            Assert.True(
                markup.Contains("x:Name=\"" + name + "\"", StringComparison.Ordinal),
                name + " is not in the markup");
        }
    }

    private static int Occurrences(string text, string what)
    {
        var count = 0;
        var at = 0;

        while ((at = text.IndexOf(what, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += what.Length;
        }

        return count;
    }

    /// <summary>One conversation card, one message in, heard ten seconds ago.</summary>
    private static Ft8ContactCard Card(string? grid, string? said)
    {
        var at = new DateTime(2026, 9, 12, 11, 2, 0, DateTimeKind.Utc);

        var facts = new Ft8CardFacts(
            Callsign: "OE1ABC",
            State: Ft8ContactState.YourMove,
            Slots: 1,
            LastAtUtc: at,
            FirstAtUtc: at,
            YouCalledHim: false,
            HeCameBack: false,
            HisMessages: 1,
            YourMessages: 0,
            HeardInAll: 1,
            ReportFromHim: null,
            ReportToHim: null,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: grid,
            HisLastPayload: said,
            YourLastMessage: null);

        return new Ft8ContactCard(
            facts,
            "FN00",
            Ft8CardActionKind.None,
            "",
            "",
            at.AddSeconds(10));
    }

    private static string Root()
    {
        var here = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
            && !System.IO.File.Exists(
                System.IO.Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        return here?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
