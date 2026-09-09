using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 298, task 2: **nothing is visible until something adjacent has
/// been earned, and opening one thing reveals several.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT IS THE ONE
/// `ACHIEVEMENTS_PHILOSOPHY.md` §2 names outright**: *a wall of empty cards reads as
/// failure to somebody who has felt like a failure at this hobby for years.* The
/// obvious way to build an achievements screen is to enumerate the seven bands and
/// the six modes and draw a card for each, and that screen tells a man who has
/// worked one band that there are six he has not.</para>
/// <para>**AND ITS QUIETER TWIN**, which the instruction names: a header reading
/// *17 of 42 filled* advertises the thirty-five he cannot see. Unit 287's
/// `The first of each mode: 1 of 6` is exactly that shape.</para>
/// <para>**IT WAS WATCHED FAILING, AND WATCHING IT CORRECTED WHAT THIS CLASS
/// BELIEVED IT WAS GUARDING.** Putting the band loop on `HfBands.Names` instead of
/// `_log.Bands` did **not** fail: `Records` returns nothing for a band with no
/// contacts, so the empty groups were dropped by the `cards.Count > 0` line further
/// down. **That line is the guard**, and removing it as well is what produced
/// `AnUnworkedBandHasNoCards [FAIL]` with the 40 m group present on a log that had
/// never been near 40 m. Both were restored.</para>
/// <para>**SO §3.1 IS ENFORCED TWICE OVER HERE**, once by iterating his own log and
/// once by refusing to emit a group with nothing in it, and a later unit that
/// changes either should know the other exists.</para>
/// </remarks>
public sealed class Unit298UnlockTests
{
    private const string HisGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the screen is printed.</param>
    public Unit298UnlockTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A band he has never worked has no group and no cards.**</summary>
    [Fact]
    public void AnUnworkedBandHasNoCards()
    {
        var screen = Screen(TwentyMetreFt8());

        Print(screen);

        Assert.Contains(screen.Groups, g => g.Title == "20 m");

        Assert.DoesNotContain(screen.Groups, g => g.Title == "40 m");

        Assert.True(
            screen.Groups.All(g => !g.Key.Contains("40 m", StringComparison.Ordinal)),
            "a band he has never worked has a group: ["
            + string.Join(", ", screen.Groups.Select(g => g.Key)) + "]");
    }

    /// <summary>**A mode he has never worked has no group and no cards.**</summary>
    [Fact]
    public void AnUnworkedModeHasNoCards()
    {
        var screen = Screen(TwentyMetreFt8());

        Assert.Contains(screen.Groups, g => g.Title == "FT8");

        foreach (var never in new[] { "FT4", "CW", "PSK31", "WSPR", "Voice" })
        {
            Assert.DoesNotContain(screen.Groups, g => g.Title == never);
        }
    }

    /// <summary>**A continent he has never worked has no card.**</summary>
    /// <remarks>
    /// §3.1's own example: *no Africa card on day one.*
    /// </remarks>
    [Fact]
    public void AnUnworkedContinentHasNoCard()
    {
        var screen = Screen(TwentyMetreFt8());

        Assert.Contains(screen.Groups, g => g.Title == "North America");

        foreach (var never in new[] { "Africa", "Asia", "Europe", "Oceania" })
        {
            Assert.DoesNotContain(screen.Groups, g => g.Title == never);
        }
    }

    /// <summary>**One 40 m contact opens the 40 m group and more than one card.**</summary>
    /// <remarks>
    /// **§3.2: ONE CONTACT IN, THREE OR FOUR POSSIBILITIES OUT.** The instruction
    /// asks for *more than one card*; what it actually yields is printed, so the
    /// report quotes a measurement rather than the requirement.
    /// </remarks>
    [Fact]
    public void OneFortyMetreContactOpensAGroupAndSeveralCards()
    {
        var before = Screen(TwentyMetreFt8());

        var log = TwentyMetreFt8().ToList();
        log.Add(Contact("EI4GNB", "40m", "FT8", null, "IO63", -14, -09, "2026-09-10 02:20:00"));

        var after = Screen(log);

        _output.WriteLine("before : " + before.Groups.Count + " groups, "
            + before.Groups.Sum(g => g.Count) + " cards");
        _output.WriteLine("after  : " + after.Groups.Count + " groups, "
            + after.Groups.Sum(g => g.Count) + " cards");
        _output.WriteLine("");

        Print(after);

        var opened = after.OpenKeys.Except(before.OpenKeys).ToList();

        _output.WriteLine("");
        _output.WriteLine("opened : " + string.Join(", ", opened));

        Assert.Contains(after.Groups, g => g.Title == "40 m");

        var forty = after.Groups.Single(g => g.Title == "40 m");

        Assert.True(
            forty.Count > 1,
            "opening 40 m yielded " + forty.Count + " card, and §3.2 wants an "
            + "unlock to reveal more than it fills");

        Assert.True(
            after.Groups.Sum(g => g.Count) - before.Groups.Sum(g => g.Count) >= 3,
            "one contact yielded fewer than three new cards across the screen");
    }

    /// <summary>**Nothing on the screen counts what he has not done.**</summary>
    /// <remarks>
    /// **THE HEADER IS THE ONE THE INSTRUCTION SAYS MUST NOT SURVIVE.** Unit 287's
    /// reads `The first of each mode: 1 of 6`. This sweeps every line the screen puts
    /// up for a count with a denominator that is not itself something he has worked.
    /// </remarks>
    [Fact]
    public void NoHeaderCountsWhatHeHasNotDone()
    {
        var screen = Screen(TwentyMetreFt8());

        var lines = new List<string> { screen.OpenedLine };

        lines.AddRange(screen.Groups.Select(g => g.Summary));

        foreach (var line in lines)
        {
            _output.WriteLine("  | " + line);
        }

        // **THE ONE PERMITTED DENOMINATOR IS A CONTINENT'S ENTITY COUNT**, which is
        // §4's own *Europe · 4 of 65* and is a count of places that exist rather
        // than of slots on this screen.
        foreach (var line in lines.Where(l => !l.EndsWith("worked", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain(" of ", line, StringComparison.Ordinal);
        }
    }

    /// <summary>**An empty log shows no groups at all, and says so kindly.**</summary>
    [Fact]
    public void AnEmptyLogShowsNothingAndInvites()
    {
        var screen = Screen(Array.Empty<AdifLogRecord>());

        Assert.Empty(screen.Groups);
        Assert.False(screen.HasGroups);

        _output.WriteLine(screen.OpenedLine);

        Assert.Contains("first contact you log", screen.OpenedLine, StringComparison.Ordinal);

        // **NOTHING SHAMES** (§4). An empty screen is the one most likely to.
        foreach (var word in new[] { "failed", "no contacts", "none", "nothing yet." })
        {
            Assert.DoesNotContain(word, screen.OpenedLine, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**A record the log cannot fill does not appear.**</summary>
    /// <remarks>
    /// A contact with no grid square has no distance, so there is no furthest card;
    /// one with no reports has no signal cards. **Absent, never a blank or a zero.**
    /// </remarks>
    [Fact]
    public void ARecordTheLogCannotFillIsAbsent()
    {
        var bare = new[]
        {
            Contact("W1ABC", "20m", "FT8", null, null, null, null, "2026-09-10 02:00:00"),
        };

        var screen = Screen(bare);

        Print(screen);

        var records = screen.Groups.SelectMany(g => g.Cards).Select(c => c.Title).ToList();

        Assert.DoesNotContain(records, t => t.StartsWith("Furthest", StringComparison.Ordinal));
        Assert.DoesNotContain(records, t => t.StartsWith("Faintest", StringComparison.Ordinal));

        // The busiest day still stands: it needs only a time, and this record has one.
        Assert.Contains(records, t => t.StartsWith("Busiest day", StringComparison.Ordinal));
    }

    /// <summary>Print the whole screen, as the report quotes it.</summary>
    private void Print(AchievementScreen screen)
    {
        _output.WriteLine(screen.OpenedLine);

        foreach (var group in screen.Groups)
        {
            _output.WriteLine("");
            _output.WriteLine("[" + group.Title + "]  " + group.Summary
                + (group.HasNote ? "   (" + group.Note + ")" : ""));

            foreach (var card in group.Cards)
            {
                _output.WriteLine("    " + card.Title + " : " + card.Figure
                    + (card.HasStation ? "   " + card.Station : ""));
            }

            if (group.HasNudge)
            {
                _output.WriteLine("    -> " + group.Nudge);
            }
        }
    }

    /// <summary>The screen over a log.</summary>
    private static AchievementScreen Screen(IReadOnlyList<AdifLogRecord> records)
        => new(new AchievementLog(records, HisGrid));

    /// <summary>Three FT8 contacts on 20 metres, all in North America.</summary>
    private static IReadOnlyList<AdifLogRecord> TwentyMetreFt8()
        => new[]
        {
            Contact("W1ABC", "20m", "FT8", null, "FN42", -12, -07, "2026-09-10 02:00:00"),
            Contact("K9XP", "20m", "FT8", null, "EN52", -18, -03, "2026-09-10 02:01:00"),
            Contact("VE3ABC", "20m", "FT8", null, "FN03", -09, -11, "2026-09-10 02:02:00"),
        };

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(
        string call, string band, string mode, string? submode, string? grid,
        int? sent, int? received, string startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = mode,
                Submode = submode,
                GridSquare = grid,
                MyGridSquare = HisGrid,
                ReportSent = sent is { } s ? s.ToString("+00;-00", System.Globalization.CultureInfo.InvariantCulture) : null,
                ReportReceived = received is { } r ? r.ToString("+00;-00", System.Globalization.CultureInfo.InvariantCulture) : null,
                StartedUtc = DateTime.Parse(
                    startedUtc, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AdjustToUniversal
                        | System.Globalization.DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
