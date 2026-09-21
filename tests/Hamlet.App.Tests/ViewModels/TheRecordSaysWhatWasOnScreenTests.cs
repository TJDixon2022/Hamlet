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

    /// <summary>Six rows covering every shape the two predicates have to read.</summary>
    private static void WithSixRows(MainWindowViewModel model, bool cqOnly)
    {
        var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

        foreach (var message in new[]
        {
            "CQ TA3MPK KM39",
            "CQ DX EA3QQ JN11",
            "KE9COB N5CH R+14",
            OwnCall + " W4WTM -07",
            "W4WTM " + OwnCall + " R-11",
            "TNX FER QSO OM",
        })
        {
            model.AddDecodeRowForTests(
                slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                "-11", "0.2", "1240", message, slot, 14_074_000);
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
                "-11", "0.2", "1240", message, slot, 14_074_000);

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
}
