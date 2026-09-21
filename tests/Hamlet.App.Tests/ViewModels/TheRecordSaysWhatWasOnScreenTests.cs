using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Criteria 3.1 and 3.2: **for every decoded row and every card, the record says what became of
/// it on the screen.**
/// </summary>
/// <remarks>
/// <para>**EVERY ASSERTION HERE READS THE FILE THE RUN WROTE**, and not one reads a view-model
/// property (work instruction 380 task 2, and section 10: *the record is the deliverable; a
/// property that agrees with it proves nothing about the file Tim sends back*). The lists are
/// driven for real - the same `AddDecodeRowForTests` door the decoder uses, the real
/// `ApplyDecodedFilter`, the real trim, the real `Reconcile`, the real panel toggles - and then
/// the JSONL is opened and asked.</para>
/// <para>**THE FAULT THIS GUARDS.** On 2026-09-12 a carrier put 262 characters on the air and
/// Tim's list stayed empty. The file said a line had been parsed; it did not say whether the row
/// reached the screen, so the only way anybody found out was a screenshot (R36, Tim,
/// 2026-09-11). A red here means that is true again.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). No port is opened, no device is enumerated, nothing
/// is keyed, and every sample lives in an array.</para>
/// </remarks>
public sealed class TheRecordSaysWhatWasOnScreenTests : IDisposable
{
    private const string OwnCall = "KC3QIS";
    private const string Grid = "FN00DJ";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the guard.</summary>
    /// <param name="output">Where the lines read back are printed.</param>
    public TheRecordSaysWhatWasOnScreenTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit380-record-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>3.1: a row that is drawn says so, on whichever side it is drawn on.</summary>
    [Fact]
    public void ADrawnRowSaysDrawnAndSaysWhichSideItIsOn()
    {
        var lines = Run(model => WithSixRows(model, cqOnly: false));

        var drawn = OnScreen(lines)
            .Where(e => Text(e, "state") == "drawn" && Text(e, "kind") == "row")
            .ToList();

        Assert.NotEmpty(drawn);

        // The left list, where nothing held the row back.
        Assert.Contains(drawn, e => Text(e, "by") == "");

        // And the operator's own side, which is drawn and NOT hidden - the record and
        // `DigitalHiddenCount` agree about that or one of them is lying (§0.0).
        Assert.Contains(drawn, e => Text(e, "by") == "addressed_to_operator");

        // Every drawn line says where the row was, so a reader can tell two apart.
        Assert.All(drawn, e => Assert.True(
            Has(e, "offsetHz") || Has(e, "slot"),
            "a drawn line with no where: " + e));

        // **AND NOW THE PLACE OF EVERY ROW IT COUNTS, NOT THE HEAD'S ALONE** (criterion 3.1,
        // work instruction 381 task 3). This is what the judging session read as missing: a
        // line saying five rows were drawn and carrying one offset answers *how many* and not
        // *which*.
        Assert.All(drawn, EveryItemIsAccountedFor);

        // The left list holds the five that nothing held back, and the file names all five.
        Assert.Equal(
            new long[] { 617, 884, 1084, 1802, 2205 },
            Places(drawn.Where(e => Text(e, "by") == "")).OrderBy(hz => hz).ToArray());

        // And the one addressed to him is named on his own side, by its own offset.
        Assert.Equal(
            new long[] { 1410 },
            Places(drawn.Where(e => Text(e, "by") == "addressed_to_operator")).ToArray());

        // **THE HEAD IS SIMPLY THE FIRST ELEMENT OF `at`**, so unit 380's readers still work.
        Assert.All(drawn, e => Assert.Equal(
            (long)Math.Round(Number(e, "offsetHz") ?? -1), Places(e).First()));
    }

    /// <summary>3.1: a row a gate held off the list says so, and names the gate.</summary>
    /// <remarks>
    /// **THIS IS THE LINE THAT WAS NOT THERE ON 2026-09-12.** A filtered-out row raises no
    /// change on the visible collection at all, so until this event the file could not tell a
    /// quiet band from a filter left on.
    /// </remarks>
    [Fact]
    public void AFilteredRowSaysFilteredAndNamesTheGateThatHeldIt()
    {
        var lines = Run(model =>
        {
            WithSixRows(model, cqOnly: false);

            // What he did: pressed CQ. The wholesale rebuild is only ever an answer
            // to something he did, and this is the answer.
            model.ShowsCqOnly = true;
        });

        var filtered = OnScreen(lines)
            .Where(e => Text(e, "state") == "filtered")
            .ToList();

        Assert.NotEmpty(filtered);

        // **NOT ONE OF THEM IS UNEXPLAINED.** A `filtered` with no gate is the state
        // R36 says is useless: it says a row is missing and not why.
        Assert.All(filtered, e => Assert.NotEqual("", Text(e, "by")));

        Assert.Contains(filtered, e => Text(e, "by") == "cq_filter");

        // And the count adds up to what the toggle actually hid.
        Assert.Equal(3, filtered.Where(e => Text(e, "by") == "cq_filter").Sum(e => Count(e)));

        // **AND THE FILE NAMES WHICH THREE** (criterion 3.1 and 3.3, work instruction 381 task
        // 3). *Three rows were held back* is what a reader had before tonight; *the rows at
        // 1,084, 1,802 and 2,205 Hz were held back by the CQ filter* is what the record says
        // now, and the three are exactly the rows that are neither a CQ nor addressed to him.
        Assert.All(filtered, EveryItemIsAccountedFor);

        Assert.Equal(
            new long[] { 1084, 1802, 2205 },
            Places(filtered.Where(e => Text(e, "by") == "cq_filter"))
                .OrderBy(hz => hz).ToArray());
    }

    /// <summary>3.1: the row cap says a row went, and says the trim took it.</summary>
    [Fact]
    public void ARowTheTrimTookSaysRemovedAndNamesTheTrim()
    {
        var lines = Run(model =>
        {
            var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

            // **THE WHOLE DOOR, CAP AND ALL.** `AddDecodeRowForTests` reaches `PlaceRow`
            // and not `AddDecodeRow`, so it never runs the trim - a finding about the
            // tree, reported in section 4 and not repaired in that hook.
            for (var at = 0; at <= MainWindowViewModel.MaxDigitalDecodes; at++)
            {
                model.AddDecodeRowThroughTheCapForTests(new Ft8Decode(
                    slot, 0.2, 500 + at, 24, "CQ " + Station(at) + " FN31"));
            }
        });

        var removed = OnScreen(lines)
            .Where(e => Text(e, "state") == "removed" && Text(e, "kind") == "row")
            .ToList();

        Assert.NotEmpty(removed);
        Assert.All(removed, e => Assert.Equal("trim", Text(e, "by")));

        // One row over the cap, so exactly one row was aged off.
        Assert.Equal(1, removed.Sum(e => Count(e)));

        // **AND THE FILE NAMES WHICH ROW THE TRIM TOOK** (criterion 3.1). The rows go on at
        // 500 Hz and up, oldest first, so the one that ages off is the one at 500 - and a
        // reader who was working 500 Hz is told so instead of being told that a row went.
        Assert.All(removed, EveryItemIsAccountedFor);

        Assert.Equal(new long[] { 500 }, Places(removed).ToArray());
    }

    /// <summary>
    /// 3.1: **a row Hamlet cannot place is declared absent, never invented and never zero.**
    /// </summary>
    /// <remarks>
    /// <para>**AN ABSENT FACT IS ABSENT** (CLAUDE.md §0.0, work instruction 381 task 3). A row
    /// whose <c>Hz</c> will not parse has no place, and the temptation is to write a 0 - which
    /// reads as a row heard at the bottom of the band. The line writes no element for it and
    /// carries <c>atDropped</c> instead, so the file says plainly that it is not claiming to
    /// name every row it counted.</para>
    /// <para>**AND `count` STILL COUNTS IT**, which is what makes the arithmetic
    /// <c>count == at.length + atDropped</c> a thing a reader can check the file against.</para>
    /// </remarks>
    [Fact]
    public void ARowHamletCannotPlaceIsCountedAndDeclaredRatherThanInvented()
    {
        var lines = Run(model =>
        {
            var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

            // Five rows Hamlet can place, and one it cannot: the decoder handed up a row
            // whose offset cell is not a number.
            foreach (var (hz, message) in new[]
            {
                ("617", "CQ TA3MPK KM39"),
                ("884", "CQ DX EA3QQ JN11"),
                ("1084", "KE9COB N5CH R+14"),
                ("1802", "W4WTM " + OwnCall + " R-11"),
                ("2205", "TNX FER QSO OM"),
                ("", "KE9COB W1AW R+03"),
            })
            {
                model.AddDecodeRowForTests(
                    slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                    "-11", "0.2", hz, message, slot, 14_074_000);
            }

            model.ShowsCqOnly = true;
        });

        var filtered = OnScreen(lines)
            .Where(e => Text(e, "state") == "filtered" && Text(e, "by") == "cq_filter")
            .ToList();

        Assert.NotEmpty(filtered);

        foreach (var line in lines.Where(l => Name(l) == "on_screen"))
        {
            _output.WriteLine("  " + line);
        }

        // Four rows are neither a CQ nor addressed to him, and the file still counts four.
        Assert.Equal(4, filtered.Sum(e => Count(e)));

        // **THREE ARE NAMED AND ONE IS DECLARED**, which is the honest answer and not a
        // silent gap: the placeless row is counted, absent from `at`, and confessed in
        // `atDropped`.
        Assert.Equal(
            new long[] { 1084, 1802, 2205 },
            Places(filtered).OrderBy(hz => hz).ToArray());

        Assert.Equal(1, filtered.Sum(e => Dropped(e)));

        // **NOTHING WAS INVENTED FOR IT.** A 0 would read as a row heard at the bottom of
        // the band, and the whole point of `atDropped` is that the file does not do that.
        Assert.DoesNotContain(Places(filtered), hz => hz == 0);

        // And the arithmetic the reader checks the file with holds on every line.
        Assert.All(OnScreen(lines), EveryItemIsAccountedFor);
    }

    /// <summary>3.1 and 3.2: what went behind a fold, which `panel_toggled` never said.</summary>
    [Fact]
    public void AFoldSaysWhatWasBehindIt()
    {
        var lines = Run(model =>
        {
            WithSixRows(model, cqOnly: false);
            WithACard(model);

            model.DigitalDecodedExpanded = false;
            model.DigitalMineExpanded = false;
        });

        var folded = OnScreen(lines).Where(e => Text(e, "state") == "folded").ToList();

        Assert.NotEmpty(folded);

        foreach (var line in lines.Where(
            l => Name(l) == "on_screen" && l.Contains("folded", StringComparison.Ordinal)))
        {
            _output.WriteLine("  " + line);
        }

        // The decoded panel folded over the rows that were on the left list.
        var decoded = folded
            .Where(e => Text(e, "by") == "digital.decoded" && Text(e, "kind") == "row")
            .ToList();

        Assert.NotEmpty(decoded);

        // Seven rows were on the left list when it shut: five of the six fixture rows -
        // the sixth is addressed to him - plus the CQ and the reply that raised the card.
        Assert.Equal(7, decoded.Sum(e => Count(e)));

        // And the For You panel folded over a row and a card, which is 3.2's fold.
        Assert.Contains(
            folded, e => Text(e, "by") == "digital.mine" && Text(e, "kind") == "row");

        Assert.Contains(
            folded, e => Text(e, "by") == "digital.mine" && Text(e, "kind") == "card");

        // **`panel_toggled` IS STILL WRITTEN AND WAS NOT REPLACED** (ruling 2 item 1).
        Assert.Contains(
            lines,
            l => Name(l) == "panel_toggled" && l.Contains("digital.decoded", StringComparison.Ordinal));

        // **AND THE FOLD NAMES WHAT WAS BEHIND IT, ROW BY ROW** (criterion 3.1, work
        // instruction 381 task 3). *Seven rows went behind the fold* and *the rows at 617,
        // 884, 1,084, 1,310, 1,310, 1,802 and 2,205 Hz went behind the fold* are the
        // difference between a count and a diagnosis - and the two at 1,310 are the carded
        // station's own two rows, which is why a place list is not a set.
        Assert.All(folded, EveryItemIsAccountedFor);

        Assert.Equal(
            new long[] { 617, 884, 1084, 1310, 1310, 1802, 2205 },
            Places(decoded).OrderBy(hz => hz).ToArray());

        // The For You fold names its row by its place too - the one row on his own side
        // when the panel shut, which is the carded station's reply.
        Assert.Equal(
            new long[] { CardOffset },
            Places(folded.Where(
                e => Text(e, "by") == "digital.mine" && Text(e, "kind") == "row")).ToArray());

        // **AND BOTH CARDS BEHIND THAT FOLD ARE NAMED BY THE OFFSET OF THE STATION THEY
        // STAND FOR** - never by the callsign the card map is keyed by (HM-DEC-018 §2.1).
        Assert.Equal(
            new long[] { CardOffset, 1410 },
            Places(folded.Where(
                e => Text(e, "by") == "digital.mine" && Text(e, "kind") == "card"))
                .OrderBy(hz => hz).ToArray());
    }

    /// <summary>3.2: a card appearing and a card dismissed are both in the file.</summary>
    [Fact]
    public void ACardAppearingAndACardDismissedAreBothWritten()
    {
        var lines = Run(model =>
        {
            WithACard(model);

            foreach (var who in model.DigitalCards.Select(c => c.Callsign).ToList())
            {
                model.ClearCardCommand.Execute(who);
            }

            model.RebuildCardsForTests();
        });

        var cards = OnScreen(lines).Where(e => Text(e, "kind") == "card").ToList();

        Assert.Contains(cards, e => Text(e, "state") == "drawn");
        Assert.Contains(
            cards, e => Text(e, "state") == "removed" && Text(e, "by") == "dismissed");

        // **3.2 RE-CHECKED AND NOT ASSUMED, because the payload grew** (work instruction 381
        // task 3). **A card's element is an offset or it is nothing** - never the callsign the
        // card map is keyed by, which is the sharpest edge in this unit (HM-DEC-018 §2.1).
        Assert.All(cards, EveryItemIsAccountedFor);

        Assert.All(cards, e => Assert.All(
            Places(e), hz => Assert.Equal(CardOffset, hz)));

        // And a card Hamlet has a measurement for IS named, so the rule is not met by
        // writing nothing at all.
        Assert.Contains(cards, e => Places(e).Count > 0);
    }

    /// <summary>
    /// 3.5, this unit's own half: **no `on_screen` line carries a callsign, a grid or a word of
    /// decoded text**, scanned over the serialised JSON.
    /// </summary>
    /// <remarks>
    /// **SCANNED, NOT TRUSTED TO THE CALL SITES** (HM-DEC-018 §2.1, and the `Psk31` category's
    /// own remarks). The thing a reader most wants to know about a hidden row is WHICH row, and
    /// the obvious way to say which is the station's callsign. This is the assertion that keeps
    /// the obvious thing from happening.
    /// </remarks>
    [Fact]
    public void NoOnScreenLineCarriesACallsignAGridOrAWordOfTheText()
    {
        var lines = Run(model =>
        {
            WithSixRows(model, cqOnly: false);
            WithACard(model);

            model.ShowsCqOnly = true;
            model.DigitalDecodedExpanded = false;
            model.DigitalMineExpanded = false;
            model.DigitalDecodedExpanded = true;
            model.DigitalMineExpanded = true;
        });

        var onScreen = lines.Where(l => Name(l) == "on_screen").ToList();

        Assert.NotEmpty(onScreen);

        _output.WriteLine(onScreen.Count + " on_screen lines scanned:");

        foreach (var line in onScreen)
        {
            _output.WriteLine("  " + line);
        }

        var forbidden = new[]
        {
            OwnCall, Grid, "W1AW", "TA3MPK", "EA3QQ", "KE9COB", "N5CH", "W4WTM",
            "FN31", "KM39", "JN11", "TNX", "QSO",
        };

        Assert.All(onScreen, line => Assert.All(forbidden, word => Assert.DoesNotContain(
            word, line, StringComparison.OrdinalIgnoreCase)));

        // **AND THE SCAN IS EXTENDED OVER `at` AND `atDropped`** (criterion 3.5 re-earned,
        // work instruction 381 task 3). The list is the newest ground and the most tempting:
        // the shortest way to say WHICH row is the callsign, and the card map is keyed by one.
        // **EVERY ELEMENT IS A NUMBER**, so nothing that could hold a name can be in there at
        // all - a stronger claim than scanning for the names this fixture happens to use.
        var withPlaces = OnScreen(lines).Where(e => Has(e, "at")).ToList();

        Assert.NotEmpty(withPlaces);

        Assert.All(withPlaces, e => Assert.All(
            e.GetProperty("at").EnumerateArray(),
            element => Assert.Equal(JsonValueKind.Number, element.ValueKind)));

        // `atDropped` is a number too, and it is only ever there where something was left out.
        Assert.All(OnScreen(lines), e => Assert.True(
            !Has(e, "atDropped") || Dropped(e) > 0,
            "an atDropped of nothing, which claims a gap that is not there: " + e));

        // And the scan is not vacuous: real places went through it.
        Assert.Contains(withPlaces, e => Places(e).Count > 1);
    }

    // ---- the plumbing, and it drives the real collections ---------------------

    /// <summary>Run one fixture and hand back every line the run wrote.</summary>
    private List<string> Run(Action<MainWindowViewModel> fixture)
    {
        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = OwnCall;
            settings.Operator.GridSquare = Grid;

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

            fixture(model);

            // **THE WINDOW IS 120 SECONDS OF WALL CLOCK** and a fixture takes
            // milliseconds, so the sampler is asked to close what it is holding. That
            // is the same call the mode change and the app's own stop make.
            model.FlushOnScreenForTests();
        }

        return Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();
    }

    /// <summary>The six rows' own offsets, one each, in the order they are placed.</summary>
    /// <remarks>
    /// **SIX ROWS AT ONE OFFSET COULD NOT TEST CRITERION 3.1** (work instruction 381 task 3).
    /// Unit 380's fixture heard every one of them at 1,240 Hz, so a record naming one row and a
    /// record naming all six read identically. Each row now has the offset Tim would read off the
    /// waterfall, which is what makes *the place of every row it counts* an assertion rather than
    /// a sentence.
    /// </remarks>
    private static readonly int[] SixOffsets = { 617, 884, 1084, 1410, 1802, 2205 };

    /// <summary>The offset the one carded station is heard on.</summary>
    private const int CardOffset = 1310;

    /// <summary>Six rows covering every shape the two predicates have to read.</summary>
    private static void WithSixRows(MainWindowViewModel model, bool cqOnly)
    {
        var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

        var messages = new[]
        {
            "CQ TA3MPK KM39",
            "CQ DX EA3QQ JN11",
            "KE9COB N5CH R+14",
            OwnCall + " W4WTM -07",
            "W4WTM " + OwnCall + " R-11",
            "TNX FER QSO OM",
        };

        for (var at = 0; at < messages.Length; at++)
        {
            model.AddDecodeRowForTests(
                slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                "-11", "0.2",
                SixOffsets[at].ToString(CultureInfo.InvariantCulture),
                messages[at], slot, 14_074_000);
        }

        model.ShowsCqOnly = cqOnly;
    }

    /// <summary>One station worked far enough to stand a card.</summary>
    private static void WithACard(MainWindowViewModel model)
    {
        var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

        foreach (var message in new[]
        {
            "CQ W1AW FN31",
            "W1AW " + OwnCall + " FN00",
            OwnCall + " W1AW -12",
        })
        {
            model.AddDecodeRowForTests(
                slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                "-11", "0.2",
                CardOffset.ToString(CultureInfo.InvariantCulture),
                message, slot, 14_074_000);

            slot = slot.AddSeconds(15);
        }

        model.CardsNowForTests = slot;
        model.RebuildCardsForTests();
    }

    private static string Station(int at)
        => "K" + (at % 9 + 1) + "AB" + (char)('A' + (at % 26));

    private static IEnumerable<JsonElement> OnScreen(IEnumerable<string> lines)
        => lines
            .Where(l => Name(l) == "on_screen")
            .Select(l => JsonDocument.Parse(l).RootElement.GetProperty("data"));

    private static string Name(string line)
    {
        try
        {
            return JsonDocument.Parse(line).RootElement
                .GetProperty("event").GetString() ?? "";
        }
        catch (JsonException)
        {
            return "";
        }
    }

    private static string Text(JsonElement data, string field)
        => data.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static int Count(JsonElement data)
        => data.TryGetProperty("count", out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : 0;

    private static bool Has(JsonElement data, string field)
        => data.TryGetProperty(field, out _);

    private static double? Number(JsonElement data, string field)
        => data.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    /// <summary>The places one line carries, in the order the file has them.</summary>
    private static List<long> Places(JsonElement data)
        => data.TryGetProperty("at", out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(e => e.GetInt64()).ToList()
            : new List<long>();

    /// <summary>The places a set of lines carries between them.</summary>
    private static List<long> Places(IEnumerable<JsonElement> lines)
        => lines.SelectMany(Places).ToList();

    /// <summary>How many of a line's items it is NOT claiming to name.</summary>
    private static int Dropped(JsonElement data)
        => data.TryGetProperty("atDropped", out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : 0;

    /// <summary>
    /// **Every item the line counts is either named or declared missing, and never neither.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ARITHMETIC A READER CHECKS THE FILE WITH** (work instruction 381 section 6
    /// ruling 1 item 4). `count == at.length + atDropped` means a reader who sees no
    /// `atDropped` knows the file is naming every row it counted, and one who sees an
    /// `atDropped` knows exactly how many it is not claiming to name. **Truncation is never
    /// silent, and neither is a place Hamlet never had.**
    /// </remarks>
    private static void EveryItemIsAccountedFor(JsonElement data)
        => Assert.Equal(Count(data), Places(data).Count + Dropped(data));
}
