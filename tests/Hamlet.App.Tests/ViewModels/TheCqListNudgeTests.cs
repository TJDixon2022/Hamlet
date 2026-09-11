using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 308 task 5: **the CQ list marks the callers who would open
/// something.**
/// </summary>
/// <remarks>
/// <para>**THERE IS NO RARITY SCORE AND NONE IS INVENTED** (Tim, 2026-09-10). The
/// achievement set already knows what is in play; the mark is the intersection of the
/// unearned cards with tonight's callers, and nothing else is computed.</para>
/// <para>**§3.7 DECIDES WHETHER THIS WORKS OR GRATES.** *If everything is marked,
/// nothing is.* An unmarked station must not read as worthless - the person he most
/// wants to work may be an ordinary domestic contact, and that is still a QSO.</para>
/// <para>**§0.2 IS ABSOLUTE HERE.** A mark is a nudge, never an arming. Nothing in
/// this feature touches a path that keys the transmitter.</para>
/// </remarks>
public sealed class TheCqListNudgeTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the marks are printed.</param>
    public TheCqListNudgeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**An unearned country is a candidate; a worked one is not.**</summary>
    [Fact]
    public void AnUnearnedCountryIsACandidateAndAWorkedOneIsNot()
    {
        var set = Log("VE3ZZZ", "W9ZZZ");

        foreach (var (callsign, expected, why) in new[]
        {
            ("VE3ABC", NudgeKind.None, "Canada, already worked"),
            ("W1ABC", NudgeKind.None, "the United States, already worked"),
            ("XE1ABC", NudgeKind.Visible, "Mexico, unworked, on a continent he has"),
            ("PY2ABC", NudgeKind.Door, "Brazil, on a continent he has never worked"),
        })
        {
            var (kind, entity) = set.WouldOpen(callsign);

            _output.WriteLine(
                callsign.PadRight(8) + kind.ToString().PadRight(9)
                + (entity.Length > 0 ? entity : "(unnamed)").PadRight(16)
                + why);

            Assert.Equal(expected, kind);
        }
    }

    /// <summary>**A door is marked and its area is never named.**</summary>
    /// <remarks>
    /// **§3.1 IS ABSENT, NOT DIMMED.** The CQ list must not be the thing that tells
    /// him Eastern Europe exists, so a door carries no entity and no continent out of
    /// the set at all.
    /// </remarks>
    [Fact]
    public void ADoorIsMarkedAndItsAreaIsNotNamed()
    {
        var set = Log("W9ZZZ");

        var (kind, entity) = set.WouldOpen("PY2ABC");

        _output.WriteLine("PY2ABC -> " + kind + ", entity [" + entity + "]");

        Assert.Equal(NudgeKind.Door, kind);
        Assert.Empty(entity);
    }

    /// <summary>**The cap holds at two when six callers are candidates.**</summary>
    [Fact]
    public void TheCapHoldsAtTwoWhenSixCallersAreCandidates()
    {
        var model = Panel();

        foreach (var call in new[]
        {
            "XE1ABC", "CO2ABC", "HI3ABC", "HH2ABC", "J68ABC", "V31ABC",
        })
        {
            Heard(model, "02:11:15", "CQ " + call + " EK99");
        }

        Print(model);

        Assert.Equal(
            MainWindowViewModel.MostMarkedStations,
            model.DigitalDecodes.Count(r => r.Nudge != NudgeKind.None));
    }

    /// <summary>**A marked station stays marked across three rebuilds.**</summary>
    /// <remarks>
    /// **STICKY PER STATION** (Tim, 2026-09-10). A mark that flickers on a station
    /// who has not changed, and vanishes as he reaches for it, is worse than no mark.
    /// </remarks>
    [Fact]
    public void AMarkedStationStaysMarkedAcrossThreeRebuilds()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ XE1ABC EK99");

        var first = model.DigitalDecodes.Single(r => r.Sender == "XE1ABC").Nudge;

        Assert.NotEqual(NudgeKind.None, first);

        for (var slot = 0; slot < 3; slot++)
        {
            Heard(model, "02:1" + (2 + slot) + ":15", "CQ W1ABC FN20");

            var still = model.DigitalDecodes
                .First(r => r.Sender == "XE1ABC")
                .Nudge;

            _output.WriteLine("after rebuild " + (slot + 1) + ": XE1ABC is " + still);

            Assert.Equal(first, still);
        }
    }

    /// <summary>**At the cap, a door displaces the weakest visible mark.**</summary>
    [Fact]
    public void ADoorDisplacesTheWeakestVisibleMarkAndTheCountStaysTwo()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ XE1ABC EK99");
        Heard(model, "02:11:15", "CQ CO2ABC EL82");

        Assert.Equal(2, Marked(model).Count);

        // **A DOOR ARRIVES.** A continent he has never worked outranks a country on
        // one he has, so it takes a mark - and the count does not become three.
        Heard(model, "02:11:30", "CQ PY2ABC GG66");

        Print(model);

        var marked = Marked(model);

        Assert.Equal(2, marked.Count);
        Assert.Contains(marked, r => r.Sender == "PY2ABC");
        Assert.Contains(marked, r => r.Nudge == NudgeKind.Door);
    }

    /// <summary>**Two candidates of the same kind: the earlier caller keeps it.**</summary>
    [Fact]
    public void ArrivalOrderBreaksTheTieBetweenTwoOfAKind()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ XE1ABC EK99");
        Heard(model, "02:11:30", "CQ CO2ABC EL82");
        Heard(model, "02:11:45", "CQ HI3ABC FK58");

        Print(model);

        var marked = Marked(model);

        Assert.Equal(2, marked.Count);

        // **THE FIRST TWO KEEP THEM**, because nothing about the third is stronger.
        Assert.Contains(marked, r => r.Sender == "XE1ABC");
        Assert.Contains(marked, r => r.Sender == "CO2ABC");
        Assert.DoesNotContain(marked, r => r.Sender == "HI3ABC");
    }

    /// <summary>**The log is read once across fourteen decodes in four slots.**</summary>
    /// <remarks>
    /// **THE PERFORMANCE SHAPE IS A HARD CONSTRAINT** and it is asserted as a read
    /// count rather than as elapsed time, which would measure this machine.
    /// </remarks>
    [Fact]
    public void TheLogIsReadOnceAcrossFourteenDecodesInFourSlots()
    {
        var model = Panel();

        for (var slot = 0; slot < 4; slot++)
        {
            for (var message = 0; message < 14; message++)
            {
                Heard(
                    model,
                    "02:1" + slot + ":15",
                    "CQ XE" + message + "ABC EK99");
            }
        }

        _output.WriteLine(
            "56 decodes, and the nudge set was built "
            + model.NudgeSetBuilds + " time(s)");

        Assert.Equal(1, model.NudgeSetBuilds);
    }

    /// <summary>**No unmarked row is dimmed or annotated by this feature.**</summary>
    /// <remarks>
    /// **§3.7.** An unmarked station is not a lesser station. The only fade on this
    /// list is unit 279's worked mark, which is a different fact and predates this.
    /// </remarks>
    [Fact]
    public void NoUnmarkedRowIsDimmedOrAnnotated()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ XE1ABC EK99");
        Heard(model, "02:11:15", "CQ W1ABC FN20");

        var unmarked = model.DigitalDecodes.Single(r => r.Sender == "W1ABC");

        _output.WriteLine("unmarked row opacity : " + unmarked.RowOpacity);
        _output.WriteLine("unmarked row nudge   : " + unmarked.Nudge);
        _output.WriteLine("unmarked row tip     : [" + unmarked.NudgeTip + "]");

        Assert.Equal(NudgeKind.None, unmarked.Nudge);

        // **FULL STRENGTH**, because he has not been worked and nothing about him is
        // lesser.
        Assert.Equal(1.0, unmarked.RowOpacity);

        Assert.Empty(unmarked.NudgeTip);
    }

    /// <summary>**Nothing in a marked row reaches a send path.**</summary>
    /// <remarks>
    /// **A MARK IS A NUDGE, NEVER AN ARMING** (§0.2). One click, one transmission.
    /// </remarks>
    [Fact]
    public void NothingInAMarkedRowReachesASendPath()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ XE1ABC EK99");

        var marked = model.DigitalDecodes.Single(r => r.Sender == "XE1ABC");

        _output.WriteLine("marked: " + marked.Sender + ", " + marked.Nudge);

        Assert.NotEqual(NudgeKind.None, marked.Nudge);

        // **NOTHING IS ARMED AND NOTHING IS COMPOSED.** The mark is a property on a
        // row; there is no armed send anywhere in this model, and the send line still
        // reads as it does before anybody has pressed anything.
        _output.WriteLine("send line: " + model.DigitalSendLine);

        Assert.False(model.HasSomethingToStop);

        Assert.DoesNotContain(
            "Sending", model.DigitalSendLine, StringComparison.Ordinal);

        Assert.DoesNotContain(
            "armed", model.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
    }

    private static List<DigitalDecodeRow> Marked(MainWindowViewModel model)
        => model.DigitalDecodes.Where(r => r.Nudge != NudgeKind.None).ToList();

    private void Print(MainWindowViewModel model)
    {
        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                row.Sender.PadRight(8) + row.Nudge.ToString().PadRight(9)
                + "opacity " + row.RowOpacity.ToString("0.00", CultureInfo.InvariantCulture)
                + "  [" + row.NudgeTip + "]");
        }

        _output.WriteLine("");
    }

    /// <summary>A position in which exactly these stations have been worked.</summary>
    /// <remarks>
    /// **THE ENTITY NAMES COME FROM THE CITED TABLE AND NOT FROM THIS FILE.** Writing
    /// them out by hand is how a fixture comes to disagree with the code it is
    /// checking: the table calls the United States *United States of America*, and a
    /// test that spelled it differently would have passed for the wrong reason
    /// (§12.5).
    /// </remarks>
    private static NudgeSet Log(params string[] workedCallsigns)
    {
        var entities = workedCallsigns
            .Select(DxccPrefixes.EntityOf)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        return new NudgeSet(
            entities, entities.Select(DxccContinents.Of));
    }

    /// <summary>A panel whose log holds the United States and nothing else.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.UseNudgeSetForTests(Log("W9ZZZ"));

        return model;
    }

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
