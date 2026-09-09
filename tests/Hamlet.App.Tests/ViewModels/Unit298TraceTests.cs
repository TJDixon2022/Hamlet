using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 298, task 7: **every card traced to a log fact, and the whole
/// set put through `ACHIEVEMENTS_PHILOSOPHY.md` §5's four questions.**
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS AN ACHIEVEMENT CLAIMED THAT WAS NOT EARNED** (§0.0, and
/// the instruction says so outright). A screen of his own record is one he will trust
/// without checking, more than he would trust a decode, because it is *his*.</para>
/// <para>**QUESTIONS 1 AND 4 ARE MACHINE-CHECKABLE AND THEY ARE CHECKED HERE.**
/// Question 1 - can it be computed from the log alone - is proved by removing the
/// field and watching the card vanish. Question 4 - does its hover teach - is proved
/// by sweeping every card for a hover that is a paragraph rather than a label.</para>
/// <para>**QUESTIONS 2 AND 3 ARE JUDGEMENTS AND ARE ANSWERED IN THE REPORT**, card by
/// card, because no assertion can tell *points at something worth trying* from
/// *points at something*. What is asserted here is the half of question 3 that can
/// be: nothing on the screen shames.</para>
/// </remarks>
public sealed class Unit298TraceTests
{
    private const string HisGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public Unit298TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **§5 question 1: every record card vanishes when its log field does.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE TRACE, DONE BY SUBTRACTION.** A card that survives the removal
    /// of the field it claims to be computed from is computed from something else,
    /// and that something else is a guess.
    /// </remarks>
    [Fact]
    public void EveryRecordVanishesWithTheFieldItIsComputedFrom()
    {
        var whole = Titles(Screen(Full()));

        _output.WriteLine("with everything: " + string.Join(", ", whole));

        foreach (var (field, strip) in new (string, Func<AdifContact, AdifContact>)[]
        {
            ("GRIDSQUARE", c => c with { GridSquare = null }),
            ("RST_RCVD", c => c with { ReportReceived = null }),
            ("RST_SENT", c => c with { ReportSent = null }),
            ("QSO_DATE", c => c with { StartedUtc = null }),
        })
        {
            var without = Titles(Screen(Full().Select(
                r => r with { Contact = strip(r.Contact) }).ToList()));

            var gone = whole.Except(without).ToList();

            _output.WriteLine("");
            _output.WriteLine("without " + field + ", these go: "
                + string.Join(", ", gone));

            Assert.True(
                gone.Count > 0,
                "removing " + field + " took no card off the screen, so whatever "
                + "was drawn from it was not drawn from it");
        }
    }

    /// <summary>**§5 question 1, the other half: no card outlives its field.**</summary>
    /// <remarks>
    /// A log stripped of every optional field must leave nothing behind that claims
    /// a distance, a report or a date.
    /// </remarks>
    [Fact]
    public void ABareLogClaimsNoFigureItCannotSupport()
    {
        var bare = Full().Select(r => r with
        {
            Contact = r.Contact with
            {
                GridSquare = null,
                ReportSent = null,
                ReportReceived = null,
                StartedUtc = null,
            },
        }).ToList();

        var titles = Titles(Screen(bare));

        _output.WriteLine("a log with a callsign, a band and a mode and nothing "
            + "else shows: " + (titles.Count == 0 ? "no record cards" : string.Join(", ", titles)));

        foreach (var claim in new[] { "Furthest", "Faintest", "Busiest", "First" })
        {
            Assert.DoesNotContain(titles, t => t.StartsWith(claim, StringComparison.Ordinal));
        }
    }

    /// <summary>**§5 question 4: every card on the screen teaches on its hover.**</summary>
    [Fact]
    public void EveryCardHoverTeaches()
    {
        var screen = Screen(Full());

        var every = screen.Scopes.SelectMany(t => t.Groups).SelectMany(g => g.Cards)
            .Concat(screen.Places.SelectMany(p => p.Cards))
            .Concat(screen.Challenges)
            .ToList();

        _output.WriteLine("cards on the screen: " + every.Count);

        foreach (var card in every)
        {
            Assert.True(
                card.HasDetail,
                card.Title + " has no hover, and §5 question 4 asks whether its "
                + "hover teaches him something true about radio");

            Assert.True(
                card.Detail.Length > 150,
                card.Title + " has a hover of " + card.Detail.Length
                + " characters, which is a label rather than an explanation");

            // **A HOVER THAT ONLY REPEATS THE TITLE TEACHES NOTHING.**
            Assert.NotEqual(card.Title, card.Detail);
        }
    }

    /// <summary>**§4: nothing anywhere on the screen shames.**</summary>
    /// <remarks>
    /// **THE HALF OF QUESTION 3 THAT CAN BE ASSERTED.** Whether an unearned card
    /// *feels* like an invitation is a judgement; whether it uses the vocabulary of
    /// failure is not.
    /// </remarks>
    [Fact]
    public void NothingOnTheScreenShames()
    {
        var screen = Screen(Full());

        var words = screen.Scopes.SelectMany(t => t.Groups)
            .Concat(screen.Places)
            .SelectMany(g => new[] { g.Title, g.Summary, g.Note, g.Nudge }
                .Concat(g.Cards.SelectMany(c => new[]
                {
                    c.Title, c.Figure, c.Station, c.Progress, c.Detail,
                })))
            .Concat(screen.Challenges.SelectMany(c => new[]
            {
                c.Title, c.Figure, c.Station, c.Progress, c.Detail,
            }))
            .Concat(new[] { screen.OpenedLine })
            .ToList();

        foreach (var shame in new[]
        {
            "failed", "failure", "you have not operated", "streak",
            "still have not", "missing out", "should have", "you have only",
            "you did not", "no thanks to", "behind",
        })
        {
            var found = words.FirstOrDefault(
                w => w.Contains(shame, StringComparison.OrdinalIgnoreCase));

            Assert.True(found is null, "the screen says \"" + shame + "\" in: " + found);
        }
    }

    /// <summary>**§4: the screen never says confirmed.**</summary>
    /// <remarks>
    /// **DXCC AND WORKED ALL STATES ARE COUNTED BY CONFIRMATIONS AND HAMLET HAS
    /// NONE.** It has contacts. The one place the word may appear is a hover
    /// explaining that difference, which is teaching rather than claiming.
    /// </remarks>
    [Fact]
    public void NoCountSaysConfirmed()
    {
        var screen = Screen(Full());

        var counts = screen.Places.Select(p => p.Summary)
            .Concat(screen.Places.SelectMany(p => p.Cards.Select(c => c.Figure)))
            .Concat(screen.Scopes.Select(t => t.Summary))
            .Concat(new[] { screen.OpenedLine })
            .ToList();

        foreach (var line in counts)
        {
            _output.WriteLine("  | " + line);

            Assert.DoesNotContain("confirm", line, StringComparison.OrdinalIgnoreCase);
        }

        // And every continent count says the word it does mean.
        Assert.All(
            screen.Places.Where(p => p.Key.StartsWith("continent-", StringComparison.Ordinal)),
            p => Assert.EndsWith("worked", p.Summary, StringComparison.Ordinal));
    }

    /// <summary>The screen over a log.</summary>
    private static AchievementScreen Screen(IReadOnlyList<AdifLogRecord> records)
    {
        var log = new AchievementLog(records, HisGrid);

        return new AchievementScreen(log, AchievementChallenges.For(log, HisGrid));
    }

    /// <summary>Every record card title on the screen.</summary>
    private static List<string> Titles(AchievementScreen screen)
        => screen.Scopes.SelectMany(t => t.Groups)
            .SelectMany(g => g.Cards)
            .Select(c => c.Title)
            .Distinct()
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToList();

    /// <summary>A log with every optional field filled.</summary>
    private static IReadOnlyList<AdifLogRecord> Full()
        => new[]
        {
            Contact("W1ABC", "20m", "FT8", "FN42", -12, -07, "2026-09-10 02:00:00"),
            Contact("EI4GNB", "40m", "FT8", "IO63", -14, -09, "2026-09-10 02:20:00"),
            Contact("TI2ABC", "20m", "FT8", "EJ79", -13, -08, "2026-09-10 02:30:00"),
        };

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(
        string call, string band, string mode, string? grid,
        int? sent, int? received, string startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = mode,
                GridSquare = grid,
                MyGridSquare = HisGrid,
                ReportSent = sent?.ToString("+00;-00", CultureInfo.InvariantCulture),
                ReportReceived = received?.ToString("+00;-00", CultureInfo.InvariantCulture),
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
