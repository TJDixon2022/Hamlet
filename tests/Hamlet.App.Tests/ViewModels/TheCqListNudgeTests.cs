using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
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

    /// <summary>**His own fourteen rows, with an empty log.**</summary>
    /// <remarks>
    /// **AN EMPTY LOG IS THE NORMAL CASE FOR THIS APPLICATION, NOT AN EDGE CASE.**
    /// The person it is for has never made a contact, and on that evening a great
    /// many rows are highlighted because everything genuinely is new. **No limit is
    /// added to make the number smaller.**
    /// </remarks>
    [Fact]
    public void HisOwnFourteenRowsWithAnEmptyLog()
    {
        var model = Panel(Log());

        foreach (var call in HisFourteen)
        {
            Heard(model, "02:11:15", "CQ " + call + " EK99");
        }

        Print(model);

        var marked = Marked(model);

        _output.WriteLine("highlighted: " + marked.Count + " of " + HisFourteen.Length);

        // **THE THREE THE INSTRUCTION NAMES**, less the one the table declines.
        Assert.Contains(marked, r => r.Sender == "TI2AIM");
        Assert.Contains(marked, r => r.Sender == "J38DX");

        // **RD6OB CANNOT BE HIGHLIGHTED BY ANY VERSION OF THIS FEATURE.**
        // `DxccPrefixes.EntityOf` declines the `RD6` prefix, so Hamlet does not know
        // where he is, and unit 252 rules it silent where the table declines. This
        // is a finding about the cited table, not about the mark.
        Assert.DoesNotContain(marked, r => r.Sender == "RD6OB");

        // **AND THERE IS NO CAP.** Unit 308 capped this at two; that cap was the
        // author's reading of §3.7 and was withdrawn in full on 2026-09-11. Thirteen
        // of his fourteen resolve and none has been worked, so far more than two
        // light up.
        Assert.True(
            marked.Count > 2,
            "only " + marked.Count + " rows were marked, which is a cap");
    }

    /// <summary>**With North America worked, only the unworked light up.**</summary>
    [Fact]
    public void WithNorthAmericaWorkedOnlyTheUnworkedLightUp()
    {
        var model = Panel(Log("W9ZZZ", "VE3ZZZ"));

        foreach (var call in HisFourteen)
        {
            Heard(model, "02:11:15", "CQ " + call + " EK99");
        }

        Print(model);

        var marked = Marked(model);

        // **THE TWO THAT RESOLVE AND ARE UNWORKED**, named.
        Assert.Contains(
            marked,
            r => r.Sender == "TI2AIM"
                 && r.NudgeTip.Contains("Costa Rica", StringComparison.Ordinal));

        Assert.Contains(marked, r => r.Sender == "J38DX");

        // **AND THE EIGHT NORTH AMERICAN ROWS GET NOTHING.**
        foreach (var worked in new[]
        {
            "VE3XN", "KJ3LLY", "W4JNC", "KD5USA", "KF9UG", "KS1WK", "N4ZEK",
            "KD8WYT",
        })
        {
            Assert.DoesNotContain(marked, r => r.Sender == worked);
        }
    }

    /// <summary>**The highlight is on the row template the CQ filter uses.**</summary>
    /// <remarks>
    /// **THIS IS THE ASSERTION THE EXISTING NINE WERE MISSING**, and its absence is
    /// how nine green tests coexisted with a screen carrying no marks. The `CQ`
    /// filter binds `DigitalVisibleDecodes`; a mark applied only to rows on the whole
    /// table would be invisible on the list he is actually reading.
    /// </remarks>
    [Fact]
    public void TheHighlightIsOnTheListTheCqFilterShows()
    {
        var model = Panel(Log("W9ZZZ"));

        model.ShowsCqOnly = true;

        Heard(model, "02:11:15", "CQ TI2AIM EK99");
        Heard(model, "02:11:15", "CQ W4JNC FM05");

        foreach (var row in model.DigitalVisibleDecodes)
        {
            _output.WriteLine(
                row.Sender.PadRight(8) + row.Nudge + "  [" + row.NudgeTip + "]");
        }

        // **THE ROW IS ON THE FILTERED LIST AT ALL**, which is the first half.
        Assert.Contains(model.DigitalVisibleDecodes, r => r.Sender == "TI2AIM");

        // **AND IT CARRIES THE MARK THERE.**
        Assert.Contains(
            model.DigitalVisibleDecodes,
            r => r.Sender == "TI2AIM" && r.IsNudged);

        Assert.DoesNotContain(
            model.DigitalVisibleDecodes,
            r => r.Sender == "W4JNC" && r.IsNudged);
    }

    /// <summary>**Nothing in the path ranks, scores or compares two candidates.**</summary>
    /// <remarks>
    /// **ASSERTED STRUCTURALLY, BY READING THE SOURCE** (Tim, 2026-09-11: *"If a line
    /// of code compares two candidate stations, it is wrong."*). It is a coarse
    /// check - it reads the one method that decides a mark and requires it to hold
    /// no comparison, no ordering and no cap.
    /// </remarks>
    [Fact]
    public void NothingInThePathRanksOrCompares()
    {
        var source = SourceOf("MainWindowViewModel.cs");

        var at = source.IndexOf(
            "private void MarkIfItOpensSomething", StringComparison.Ordinal);

        Assert.True(at > 0, "the marking method was not found");

        var body = source[at..source.IndexOf(
            "private static void Apply", at, StringComparison.Ordinal)];

        _output.WriteLine(body);

        foreach (var banned in new[]
        {
            "OrderBy", "OrderByDescending", "Max(", "Min(", "CompareTo",
            "weakest", "strongest", "MostMarked", "Count >=", "Rank",
        })
        {
            Assert.DoesNotContain(banned, body, StringComparison.Ordinal);
        }
    }

    /// <summary>**The log is read once across fourteen decodes in four slots.**</summary>
    /// <remarks>
    /// **THE PERFORMANCE SHAPE IS A HARD CONSTRAINT** and it is asserted as a read
    /// count rather than as elapsed time, which would measure this machine.
    /// </remarks>
    [Fact]
    public void TheLogIsReadOnceAcrossFourteenDecodesInFourSlots()
    {
        var model = Panel(Log("W9ZZZ"));

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
        var model = Panel(Log("W9ZZZ"));

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
        var model = Panel(Log("W9ZZZ"));

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

    /// <summary>**Fourteen qualifying rows carry fourteen marks.**</summary>
    /// <remarks>
    /// **§R16, TIM 2026-09-11: NO CAP, AND HE SAID SO IN WORDS** - *"I do not mind
    /// lots of achievement markers."* Nine of fourteen carried a quill on his own
    /// screen, and the five that did not were stations the cited prefix table
    /// declines or entities he has worked. The fourteen below are chosen so that
    /// every one of them resolves and none has been worked, which is the only way
    /// to ask this question without the table answering it first.
    /// </remarks>
    [Fact]
    public void FourteenQualifyingRowsCarryFourteenMarks()
    {
        // **NORTH AMERICA WORKED, SO BOTH KINDS ARE ON THE LIST AT ONCE.** Three
        // of the fourteen are counters and eleven are doors, which is the shape
        // the ruling is actually about: not fourteen of one thing, but every
        // station that would earn anything, marked as what it would earn.
        var model = Panel(Log("W9ZZZ"));

        foreach (var call in FourteenThatAllQualify)
        {
            Heard(model, "02:11:15", "CQ " + call + " EK99");
        }

        Print(model);

        var marked = Marked(model);

        _output.WriteLine(
            "qualifying " + FourteenThatAllQualify.Length
            + ", marked " + marked.Count);

        Assert.Equal(FourteenThatAllQualify.Length, model.DigitalDecodes.Count);
        Assert.Equal(FourteenThatAllQualify.Length, marked.Count);

        // **AND BOTH KINDS ARE PRESENT AND TOLD APART.**
        var doors = marked.Count(r => r.NudgeForm == AchievementMarkForm.Door);
        var counters = marked.Count(r => r.NudgeForm == AchievementMarkForm.Counter);

        _output.WriteLine("doors " + doors + ", counters " + counters);

        Assert.True(doors > 0, "no door was marked");
        Assert.True(counters > 0, "no counter was marked");
        Assert.Equal(marked.Count, doors + counters);
    }

    /// <summary>**A door row carries the ring, the orange, and it turns.**</summary>
    /// <remarks>
    /// **THIS IS COMPUTED AND NOT SEEN.** The control is built headless and its
    /// own `HasRing`, `LitBrush` and `IsOrbiting` are read; nothing here looks at
    /// a pixel.
    /// </remarks>
    [AvaloniaFact]
    public void ADoorRowCarriesTheRingAndTheOrangeAndSpins()
    {
        var model = Panel(Log("W9ZZZ"));

        Heard(model, "02:11:15", "CQ PY2ABC GG66");

        var row = model.DigitalDecodes.Single(r => r.Sender == "PY2ABC");

        Assert.Equal(NudgeKind.Door, row.Nudge);
        Assert.Equal(AchievementMarkForm.Door, row.NudgeForm);

        var mark = new AchievementMarkControl
        {
            Form = row.NudgeForm,
            IsNew = row.IsNudged,
        };

        _output.WriteLine(
            "door : ring=" + mark.HasRing
            + "  ink=" + Hex(mark.LitBrush)
            + "  orbiting=" + mark.IsOrbiting);

        Assert.True(mark.HasRing);
        Assert.True(mark.IsOrbiting);
        Assert.Equal("#FFC25E00", Hex(mark.LitBrush));

        // **AND IT DOES NOT SETTLE.** The tray's thirty seconds are unit 300's and
        // stay; a door is on the list he is reading and turns while it is there.
        mark.SettleForTests();

        _output.WriteLine("door, wound past the tray's settle : " + mark.IsOrbiting);

        Assert.True(mark.IsOrbiting);
    }

    /// <summary>**A counter row carries the still green quill and no ring.**</summary>
    [AvaloniaFact]
    public void ACounterRowCarriesTheStillGreenQuill()
    {
        var model = Panel(Log("W9ZZZ"));

        Heard(model, "02:11:15", "CQ XE1ABC EK99");

        var row = model.DigitalDecodes.Single(r => r.Sender == "XE1ABC");

        Assert.Equal(NudgeKind.Visible, row.Nudge);
        Assert.Equal(AchievementMarkForm.Counter, row.NudgeForm);

        var mark = new AchievementMarkControl
        {
            Form = row.NudgeForm,
            IsNew = row.IsNudged,
        };

        _output.WriteLine(
            "counter : ring=" + mark.HasRing
            + "  ink=" + Hex(mark.LitBrush)
            + "  orbiting=" + mark.IsOrbiting);

        Assert.False(mark.HasRing);
        Assert.False(mark.IsOrbiting);
        Assert.Equal("#FF3B6D11", Hex(mark.LitBrush));
    }

    /// <summary>**The two forms differ in shape, not only in color.**</summary>
    /// <remarks>
    /// **§0.6 IS THE WHOLE REASON THE RING IS THE CARRIER.** Roughly one man in
    /// twelve cannot separate this green from this orange, and this hobby's
    /// demographics make that a real slice of the people who will use it. A ring
    /// or no ring is a difference that survives a greyscale print.
    /// </remarks>
    [AvaloniaFact]
    public void TheTwoFormsDifferInShapeAndNotOnlyInColor()
    {
        var door = new AchievementMarkControl
        {
            Form = AchievementMarkForm.Door, IsNew = true,
        };

        var counter = new AchievementMarkControl
        {
            Form = AchievementMarkForm.Counter, IsNew = true,
        };

        _output.WriteLine(
            "door ring=" + door.HasRing + "  counter ring=" + counter.HasRing);

        Assert.NotEqual(door.HasRing, counter.HasRing);
        Assert.NotEqual(Hex(door.LitBrush), Hex(counter.LitBrush));

        // **AND THE ORANGE IS NOT THE MUSTARD RULED OUT FOR THE TRAY.**
        Assert.NotEqual("#FFEDC375", Hex(door.LitBrush));

        // The tray's own mark is untouched by either.
        var tray = new AchievementMarkControl { IsNew = true };

        Assert.Equal(AchievementMarkForm.Tray, tray.Form);
        Assert.True(tray.HasRing);
        Assert.Equal(Hex(counter.LitBrush), Hex(tray.LitBrush));
    }

    /// <summary>**A worked station carries neither quill.**</summary>
    [Fact]
    public void AWorkedStationCarriesNeitherQuill()
    {
        var model = Panel(Log("W9ZZZ"));

        Heard(model, "02:11:15", "CQ W1ABC FN20");

        var row = model.DigitalDecodes.Single(r => r.Sender == "W1ABC");

        _output.WriteLine(
            "worked : nudge=" + row.Nudge + "  nudged=" + row.IsNudged
            + "  tip=[" + row.NudgeTip + "]");

        Assert.Equal(NudgeKind.None, row.Nudge);
        Assert.False(row.IsNudged);
        Assert.False(row.NudgeIsDoor);
        Assert.Empty(row.NudgeTip);
    }

    /// <summary>**No area is named on a door, on the row or anywhere near it.**</summary>
    /// <remarks>
    /// **§3.1 IS ABSENT, NOT DIMMED** and this is the row-level half of it. The
    /// set already refuses to hand the entity out; this asserts that nothing on
    /// the way to the screen puts it back.
    /// </remarks>
    [Fact]
    public void NoAreaIsNamedOnADoorRow()
    {
        var model = Panel(Log("W9ZZZ"));

        Heard(model, "02:11:15", "CQ PY2ABC GG66");

        var row = model.DigitalDecodes.Single(r => r.Sender == "PY2ABC");

        _output.WriteLine("door row tip : [" + row.NudgeTip + "]");

        Assert.Equal(NudgeKind.Door, row.Nudge);

        foreach (var area in new[]
        {
            "Brazil", "South America", "Africa", "Asia", "Europe", "Oceania",
        })
        {
            Assert.DoesNotContain(area, row.NudgeTip, StringComparison.OrdinalIgnoreCase);
        }

        // It still says something, because a mark nobody can ask about is a mark
        // that teaches nothing.
        Assert.NotEmpty(row.NudgeTip);
    }

    /// <summary>A brush's color as `#AARRGGBB`, for an exact assertion.</summary>
    private static string Hex(Avalonia.Media.IBrush brush)
        => (brush is Avalonia.Media.ISolidColorBrush solid
            ? solid.Color.ToString()
            : brush.ToString() ?? "").ToUpperInvariant();

    /// <summary>
    /// Fourteen callsigns that every one resolve through the cited table and none
    /// of which has been worked against an empty log.
    /// </summary>
    /// <remarks>
    /// **HIS OWN FOURTEEN CANNOT ANSWER THE NO-CAP QUESTION** and that is a
    /// finding rather than a convenience: `RD6OB`'s prefix is declined by
    /// `DxccPrefixes`, so it can never be marked by any version of this feature,
    /// and eight of the rest are entities he has worked. These fourteen are chosen
    /// to isolate the cap from the table.
    /// </remarks>
    private static readonly string[] FourteenThatAllQualify =
    {
        "TI2AIM", "J38DX", "XE1ABC", "PY2ABC", "ZS6ABC", "JA1ABC", "EA3QQ",
        "G0XYZ", "LU1ABC", "9M2ABC", "HK3ABC", "CE3ABC", "5B4ABC", "OH2ABC",
    };

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

    /// <summary>The fourteen callsigns on his own screenshot.</summary>
    private static readonly string[] HisFourteen =
    {
        "TI2AIM", "J38DX", "RD6OB", "VE3XN", "KJ3LLY", "W4JNC", "KD5USA",
        "KF9UG", "KS1WK", "N4ZEK", "KD8WYT", "K1ABC", "W9ZZZ", "VE7AA",
    };

    /// <summary>One file of this repository own source, for a structural check.</summary>
    private static string SourceOf(string name)
    {
        var here = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !System.IO.File.Exists(
                   System.IO.Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return System.IO.File.ReadAllText(
            System.IO.Directory.GetFiles(here!.FullName, name, SearchOption.AllDirectories)
                .First(f => f.Contains("src", StringComparison.Ordinal)));
    }

    /// <summary>A panel holding a given position.</summary>
    private static MainWindowViewModel Panel(NudgeSet set)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.UseNudgeSetForTests(set);

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
