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
/// **Unit 381 task 1: what a PLACE costs inside a line that is already being written.**
/// </summary>
/// <remarks>
/// <para>**THIS TYPE BUILDS NOTHING, CHANGES NO SOURCE FILE, REPAIRS NOTHING AND ASSERTS NOTHING
/// ABOUT THE PRODUCT** (work instruction 381 task 1). It reads what the record says today, weighs
/// three encodings of a place on the file, and does the arithmetic that decides the cap. **If a
/// measurement contradicts the instruction, the measurement wins and it goes in section 4** -
/// that is how unit 380 found the squelch.</para>
/// <para>**THE QUESTION IS NOT THE ONE UNIT 380 ANSWERED.** Unit 380 proved that one line per row
/// costs 876 kB an hour and cannot be had. This asks what a place costs INSIDE a line that is
/// already paid for, where the 142-byte schema-B envelope is not paid again.</para>
/// <para>**EVERY ITEM HERE NEEDS NO WINDOW**, so every one is a <c>[Fact]</c> and none is an
/// <c>[AvaloniaFact]</c> - unit 380's item 6, which work instruction 381 task 1 carries as
/// permission rather than making this unit disclose it again. Nothing draws, nothing is shown and
/// no scroller is settled.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). No port is opened, no device is enumerated, nothing is
/// keyed, and every sample lives in an array.</para>
/// </remarks>
public sealed class Unit381Trace : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>Criterion 3.4's ceiling, in bytes. It is read and never re-read.</summary>
    private const double BudgetBytesAnHour = 50 * 1024;

    /// <summary>What unit 380 measured the lines themselves at, in bytes an hour.</summary>
    private const double UnitThreeEightyLineBytesAnHour = 12.5 * 1024;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every measurement is printed.</param>
    public Unit381Trace(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit381-trace-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>
    /// **Item 1: the before, per row, from the four-signal fixture with the CQ filter on.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE EXHIBIT THE JUDGING SESSION READ, REPRODUCED RATHER THAN QUOTED.** The four
    /// carriers are taken out of the record itself - <c>psk31_carrier_appeared</c> carries each
    /// one's offset - so *which four* is a reading of the file and not a number copied from an
    /// instruction.
    /// </remarks>
    [Fact]
    public void Item1TheBeforePerRowOverTheFourSignalFixture()
    {
        var lines = Run(model =>
        {
            model.ChooseDigitalModeCommand.Execute("PSK31");

            Play(model, "psk31-four-signals.wav");
        });

        var carriers = Data(lines, "psk31_carrier_appeared")
            .Select(e => Number(e, "offsetHz"))
            .Where(hz => hz is not null)
            .Select(hz => Math.Round(hz!.Value))
            .Distinct()
            .OrderBy(hz => hz)
            .ToList();

        var onScreen = Data(lines, "on_screen").Where(e => Text(e, "kind") == "row").ToList();

        _output.WriteLine("ITEM 1 - THE BEFORE, PER ROW, FOUR-SIGNAL FIXTURE, CQ FILTER ON");
        _output.WriteLine("");
        _output.WriteLine("  the on_screen lines the record actually wrote:");

        foreach (var line in lines.Where(l => Name(l) == "on_screen"))
        {
            _output.WriteLine("    " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("  the carriers the record itself names, off psk31_carrier_appeared:");
        _output.WriteLine("    " + string.Join(
            ", ", carriers.Select(hz => hz.ToString("0", CultureInfo.InvariantCulture) + " Hz")));
        _output.WriteLine("");
        _output.WriteLine("  WHAT THE FILE SAYS BECAME OF EACH ONE, BY NAME:");

        var named = 0;

        foreach (var hz in carriers)
        {
            var mine = onScreen
                .Where(e => Number(e, "offsetHz") is { } at && Math.Abs(at - hz) < 1.0)
                .ToList();

            if (mine.Count > 0)
            {
                named++;

                _output.WriteLine("    " + hz.ToString("0", CultureInfo.InvariantCulture)
                    + " Hz : NAMED - the file carries this offset on "
                    + string.Join(" and ", mine.Select(e =>
                        "a " + Text(e, "state") + " line"
                        + (Text(e, "by").Length > 0 ? " by " + Text(e, "by") : "")
                        + " with count " + Count(e))));
            }
            else
            {
                _output.WriteLine("    " + hz.ToString("0", CultureInfo.InvariantCulture)
                    + " Hz : COUNTED ONLY - no on_screen line carries this offset. It is inside "
                    + "somebody else's count and the file cannot be asked about it.");
            }
        }

        var counted = onScreen.Sum(e => Count(e));

        _output.WriteLine("");
        _output.WriteLine("  THE HONEST SENTENCE:");
        _output.WriteLine("    the file NAMES " + named + " of " + carriers.Count
            + " carriers by their own place, and COUNTS " + (carriers.Count - named)
            + " of them. Across every on_screen row line it counts " + counted
            + " row mentions in " + onScreen.Count + " lines, carrying "
            + onScreen.Count(e => Has(e, "offsetHz")) + " offsets between them - "
            + "one place per LINE, never one per ROW.");
        _output.WriteLine("");
        _output.WriteLine("    3.1 asks for the row's offset or slot and 3.3 asks WHICH rows "
            + "were hidden. A count answers HOW MANY. That is the right half of both criteria "
            + "and it is not the whole of either.");
    }

    /// <summary>**Item 2: the before, per row, from the gate that does fire.**</summary>
    /// <remarks>
    /// **THE FOUR-SIGNAL ANSWER IS *NOTHING WAS HIDDEN*, SO A GATE THAT FIRES HAS TO BE WEIGHED
    /// BESIDE IT.** Six FT8-shaped rows with the CQ toggle on: three held, and the question is
    /// which of the three the file names.
    /// </remarks>
    [Fact]
    public void Item2TheBeforePerRowOverTheGateThatDoesFire()
    {
        var offsets = new[] { 617, 884, 1084, 1410, 1802, 2205 };

        var messages = new[]
        {
            "CQ TA3MPK KM39",
            "CQ DX EA3QQ JN11",
            "KE9COB N5CH R+14",
            OwnCall + " W4WTM -07",
            "W4WTM " + OwnCall + " R-11",
            "TNX FER QSO OM",
        };

        var lines = Run(model =>
        {
            var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

            for (var at = 0; at < messages.Length; at++)
            {
                model.AddDecodeRowForTests(
                    slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                    "-11", "0.2",
                    offsets[at].ToString(CultureInfo.InvariantCulture),
                    messages[at], slot, 14_074_000);
            }
        });

        var rows = Data(lines, "on_screen").Where(e => Text(e, "kind") == "row").ToList();

        _output.WriteLine("ITEM 2 - THE BEFORE, PER ROW, SIX FT8-SHAPED ROWS, CQ FILTER ON");
        _output.WriteLine("");
        _output.WriteLine("  the rows offered, by offset, with what each one is:");

        for (var at = 0; at < messages.Length; at++)
        {
            var kind = messages[at].StartsWith("CQ", StringComparison.Ordinal)
                ? "a CQ"
                : messages[at].Contains(OwnCall, StringComparison.Ordinal)
                    ? "addressed to the operator"
                    : "between two other stations";

            _output.WriteLine("    " + offsets[at].ToString(CultureInfo.InvariantCulture)
                + " Hz : " + kind);
        }

        _output.WriteLine("");
        _output.WriteLine("  the on_screen lines the record actually wrote:");

        foreach (var line in lines.Where(l => Name(l) == "on_screen"))
        {
            _output.WriteLine("    " + line);
        }

        var held = rows.Where(e => Text(e, "state") == "filtered").ToList();

        _output.WriteLine("");
        _output.WriteLine("  THE GATE THAT FIRED: " + held.Sum(e => Count(e))
            + " rows held, in " + held.Count + " line(s), naming "
            + string.Join(" and ", held.Select(e => Text(e, "by"))) + ".");
        _output.WriteLine("");
        _output.WriteLine("  WHICH OF THE HELD ROWS THE FILE NAMES:");

        foreach (var line in held)
        {
            var head = Number(line, "offsetHz");

            _output.WriteLine("    the filtered line carries offsetHz "
                + (head is { } hz ? hz.ToString("0.#", CultureInfo.InvariantCulture) : "(absent)")
                + " and a count of " + Count(line) + " - so it NAMES 1 of " + Count(line)
                + " and COUNTS " + (Count(line) - 1) + ".");
        }

        _output.WriteLine("");
        _output.WriteLine("    A reader asking *the station I was working at 2,205 Hz vanished, "
            + "what took it* gets a count of three and one offset that is not his. That is the "
            + "criterion this unit exists to close.");
    }

    /// <summary>**Item 3: the row rate, re-measured and not inherited.**</summary>
    /// <remarks>
    /// **UNIT 380 MEASURED 3,360 ROWS AN HOUR AND THIS DOES NOT TRUST IT** (work instruction 381
    /// task 1 item 3). The same path, the same shack reading of fourteen a slot, counted off the
    /// file rather than off the fixture's own loop counter.
    /// </remarks>
    [Fact]
    public void Item3TheRowRateReMeasured()
    {
        const int Slots = 8;
        const int PerSlot = 14;

        var lines = Run(model => Busy(model, Slots, PerSlot));

        var rows = Data(lines, "on_screen")
            .Where(e => Text(e, "kind") == "row" && Text(e, "state") == "filtered")
            .Sum(e => Count(e));

        var slotsAnHour = 3600.0 / Ft8Slots.SlotSeconds;
        var anHour = rows / (double)Slots * slotsAnHour;

        _output.WriteLine("ITEM 3 - THE ROW RATE, RE-MEASURED");
        _output.WriteLine("  slots driven            : " + Slots
            + " at " + Ft8Slots.SlotSeconds + " s");
        _output.WriteLine("  decodes offered         : " + (Slots * PerSlot));
        _output.WriteLine("  rows the FILE accounts for: " + rows);
        _output.WriteLine("  rows a slot, off the file : "
            + (rows / (double)Slots).ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine("  slots an hour           : "
            + slotsAnHour.ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("  ROWS AN HOUR            : "
            + anHour.ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("  unit 380 measured       : 3360");
        _output.WriteLine("  AGREES WITH 3,360       : "
            + (Math.Abs(anHour - 3360) < 1 ? "YES, exactly" : "NO - " + anHour));
    }

    /// <summary>
    /// **Items 4 and 5: the weight of one place, on the file, in three encodings, and the
    /// arithmetic that follows.**
    /// </summary>
    /// <remarks>
    /// <para>**EVERY FIGURE IS A DIFFERENCE BETWEEN TWO LINES IN A FILE, NEVER `.Length` ON A
    /// STRING THIS TEST BUILT** (work instruction 381 task 1 item 4). The baseline line is written
    /// through <see cref="AppEvents.OnScreen"/> itself; each candidate is the same payload with
    /// one array added, written through the same sink, and the bytes are read back off the
    /// file.</para>
    /// <para>**THE PLACES ARE REAL ONES.** They are the offsets the busy-band fixture actually
    /// hands the table, so the digit counts are the digit counts a reader will meet.</para>
    /// </remarks>
    [Fact]
    public void Items4And5WhatAPlaceCostsAndWhetherItFits()
    {
        const int Places = 14;

        var whole = new List<object?>();
        var tenths = new List<object?>();
        var strings = new List<object?>();

        for (var at = 0; at < Places; at++)
        {
            var hz = 500.37 + (at * 97.44);

            whole.Add((long)Math.Round(hz));
            tenths.Add(Math.Round(hz, 1));
            strings.Add("214100|" + Math.Round(hz).ToString("0", CultureInfo.InvariantCulture));
        }

        var weighed = Weigh(new (string Name, List<object?>? At)[]
        {
            ("baseline, no place list at all", null),
            ("a bare number array in whole Hz", whole),
            ("a bare number array at one decimal", tenths),
            ("an array of slot+offset strings", strings),
        });

        var baseline = weighed[0].Bytes;

        _output.WriteLine("ITEM 4 - WHAT A PLACE COSTS, WEIGHED ON THE FILE");
        _output.WriteLine("  places carried in each candidate : " + Places);
        _output.WriteLine("");

        foreach (var (name, bytes) in weighed)
        {
            var added = bytes - baseline;

            _output.WriteLine("  " + name.PadRight(36) + " : " + bytes + " bytes on the file"
                + (added == 0
                    ? ""
                    : ", " + added + " added, "
                        + (added / (double)Places).ToString("0.00", CultureInfo.InvariantCulture)
                        + " BYTES A PLACE"));
        }

        // ---- item 5, the arithmetic -------------------------------------------

        const int BusySlots = 80;
        const int BusyPerSlot = 14;

        var busy = Run(model => Busy(model, BusySlots, BusyPerSlot));

        var busyLines = busy.Where(l => Name(l) == "on_screen").ToList();
        var lineBytes = busyLines.Sum(l => System.Text.Encoding.UTF8.GetByteCount(l) + 2);

        // **EVERY ITEM THE FILE COUNTS IS A PLACE THE FILE WOULD CARRY.** `count` is the number
        // of items in the group, so the total of `count` over every line IS the size of the
        // place lists that line would hold.
        var places = Data(busy, "on_screen").Sum(e => Count(e));

        var seconds = BusySlots * Ft8Slots.SlotSeconds;
        var lineBytesAnHour = lineBytes / (double)seconds * 3600;
        var placesAnHour = places / (double)seconds * 3600;

        _output.WriteLine("");
        _output.WriteLine("ITEM 5 - THE ARITHMETIC, SHOWN");
        _output.WriteLine("  the busy band re-driven  : " + BusySlots + " slots x "
            + BusyPerSlot + " rows = " + seconds + " s of band");
        _output.WriteLine("  on_screen lines written  : " + busyLines.Count);
        _output.WriteLine("  bytes of lines on the file: " + lineBytes + " = "
            + (lineBytesAnHour / 1024).ToString("0.0", CultureInfo.InvariantCulture)
            + " kB an hour (unit 380 measured 12.5)");
        _output.WriteLine("  items those lines count  : " + places + " = "
            + placesAnHour.ToString("0", CultureInfo.InvariantCulture) + " PLACES AN HOUR");
        _output.WriteLine("");
        _output.WriteLine("  what each encoding would then cost, against 3.4's 50 kB:");

        (string Name, double PerPlace, double Total)? fits = null;

        foreach (var (name, bytes) in weighed.Skip(1))
        {
            var perPlace = (bytes - baseline) / (double)Places;
            var placeBytesAnHour = perPlace * placesAnHour;
            var total = lineBytesAnHour + placeBytesAnHour;

            _output.WriteLine("    " + name.PadRight(36) + " : "
                + perPlace.ToString("0.00", CultureInfo.InvariantCulture) + " B/place, "
                + (placeBytesAnHour / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB of places + "
                + (lineBytesAnHour / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB of lines = "
                + (total / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB AN HOUR - "
                + (total <= BudgetBytesAnHour ? "FITS" : "DOES NOT FIT"));

            if (total <= BudgetBytesAnHour && fits is null)
            {
                fits = (name, perPlace, total);
            }
        }

        _output.WriteLine("");

        if (fits is { } won)
        {
            var headroom = BudgetBytesAnHour - won.Total;

            _output.WriteLine("  THE ENCODING THAT FITS : " + won.Name
                + ", at " + won.PerPlace.ToString("0.00", CultureInfo.InvariantCulture)
                + " bytes a place and "
                + (won.Total / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB an hour, "
                + (headroom / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB under the ceiling.");

            // **THE CAP IS ARITHMETIC AND NEVER A NUMBER ANYBODY LIKED** (ruling 1 item 4). The
            // budget left for places after the lines are paid for, divided by what a place
            // costs, divided by the lines an hour the sampler actually writes, is the most
            // places one line may carry. The cap is the largest power of two at or under it.
            var linesAnHour = busyLines.Count / (double)seconds * 3600;
            var placeBudget = BudgetBytesAnHour - lineBytesAnHour;
            var perLine = placeBudget / won.PerPlace / linesAnHour;

            var cap = 1;

            while (cap * 2 <= perLine)
            {
                cap *= 2;
            }

            var worst = Data(busy, "on_screen").Max(e => Count(e));

            _output.WriteLine("  THE CAP THAT FOLLOWS   :");
            _output.WriteLine("    lines an hour, measured        : "
                + linesAnHour.ToString("0", CultureInfo.InvariantCulture));
            _output.WriteLine("    budget left for places        : "
                + (placeBudget / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB an hour = "
                + (placeBudget / won.PerPlace).ToString("0", CultureInfo.InvariantCulture)
                + " places an hour");
            _output.WriteLine("    places one line may carry     : "
                + perLine.ToString("0.0", CultureInfo.InvariantCulture));
            _output.WriteLine("    THE CAP, largest power of two at or under it : " + cap);
            _output.WriteLine("    the largest group this band produced : " + worst
                + " items, so the measured band truncates "
                + (worst <= cap ? "NOTHING and every row is named" : "and atDropped fires"));
            _output.WriteLine("    a cap is still written because a group is bounded by nothing "
                + "but the window, and a band thirty times this one would reach it.");
            _output.WriteLine("  DOES THE WINDOW MOVE   : NO. Widening the window buys envelope "
                + "bytes only - about "
                + (lineBytesAnHour / 1024).ToString("0.0", CultureInfo.InvariantCulture)
                + " kB an hour in total - because places scale with rows and not with windows "
                + "(ruling 2 item 2c). It is the smallest lever and it is not needed.");
        }
        else
        {
            _output.WriteLine("  NO ENCODING FITS. Ruling 2 item 1: the ceiling holds and 3.1 "
                + "is reported partial. The cap of ruling 1 item 4 is then the whole subject.");
        }
    }

    /// <summary>**Item 6: where the identities live and what would leak.**</summary>
    /// <remarks>
    /// **THE KEY SHAPES ARE PRINTED AND NEVER A KEY** (work instruction 381 task 1 item 6). The
    /// card map is keyed by callsign, and that is the sharpest edge in this unit: a list that
    /// carried identities instead of places would put a man's callsign in the file (HM-DEC-018
    /// §2.1).
    /// </remarks>
    [Fact]
    public void Item6WhereTheIdentitiesLiveAndWhatWouldLeak()
    {
        _output.WriteLine("ITEM 6 - THE THREE MAPS, THEIR KEY SHAPES, AND WHAT WOULD LEAK");
        _output.WriteLine("");
        _output.WriteLine("  _onScreenRows      (MainWindowViewModel.cs:2853)");
        _output.WriteLine("    keyed by         : the DigitalDecodeRow OBJECT, by reference");
        _output.WriteLine("    window identity  : \"s|\" + <the row object's runtime hash>");
        _output.WriteLine("    would it leak    : no name, but it is a handle on a process and "
            + "means nothing to a reader. It is not a place.");
        _output.WriteLine("");
        _output.WriteLine("  _onScreenTextRows  (MainWindowViewModel.cs:2864)");
        _output.WriteLine("    keyed by         : the channel the listener is holding");
        _output.WriteLine("    window identity  : \"t|\" + <channel id>, or "
            + "\"t|ended|\" + <the row object's runtime hash> once the carrier retires");
        _output.WriteLine("    would it leak    : no name, and still not a place - the channel "
            + "id is an ordinal and a reader cannot turn it into a frequency.");
        _output.WriteLine("");
        _output.WriteLine("  _onScreenCards     (MainWindowViewModel.cs:2875)");
        _output.WriteLine("    keyed by         : THE CALLSIGN");
        _output.WriteLine("    window identity  : \"c|\" + <the callsign> "
            + "(MainWindowViewModel.cs:3111)");
        _output.WriteLine("    WOULD IT LEAK    : YES, VERBATIM. Two characters of prefix and "
            + "then a man's callsign, which is his name, his address and his licence. "
            + "HM-DEC-018 section 2.1 draws the line here and the answer is that a card is "
            + "COUNTED AND NOT NAMED where Hamlet has no offset for it.");
        _output.WriteLine("");
        _output.WriteLine("  THE RULE THAT FOLLOWS: `at` carries PLACES and NEVER identities. A "
            + "card's element is the offset of the station it stands for where _technicalSeen "
            + "has one, and NOTHING AT ALL where it does not - an absent fact is absent and "
            + "never zero, and never a substitute drawn from a key (section 0.0).");
        _output.WriteLine("");
        _output.WriteLine("  AND ONE READING OF THE TREE, while the maps are open: OnScreenBy "
            + "(src/Hamlet.App/Telemetry/OnScreen.cs:53) holds five tokens - \"\", cq_filter, "
            + "addressed_to_operator, trim, dismissed - and there is NO Squelch token in it. "
            + "Work instruction 381 section 5 asks whether one is still in the list. It is not; "
            + "unit 380 never added one.");
    }

    // ---- the plumbing ---------------------------------------------------------

    /// <summary>Writes one baseline line and one candidate a place list, and weighs each.</summary>
    /// <param name="candidates">The list to hang on each line, or null for the baseline.</param>
    /// <returns>Each candidate's name and the bytes its line occupies in the file.</returns>
    /// <remarks>
    /// **THE BASELINE IS WRITTEN BY THE REAL WRITER** and the candidates are the real writer's own
    /// payload with one key added, through the real sink, read back off the real file. That is
    /// what makes the difference a measurement of a line and not of a string.
    /// </remarks>
    private List<(string Name, int Bytes)> Weigh(
        (string Name, List<object?>? At)[] candidates)
    {
        var folder = Path.Combine(_folder, "weigh-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "381", _ => true))
        {
            foreach (var (_, at) in candidates)
            {
                if (at is null)
                {
                    AppEvents.OnScreen(
                        telemetry, OnScreenKind.Row, OnScreenState.Filtered,
                        OnScreenBy.CqFilter, isTextOnly: false,
                        offsetHz: 500.4, slot: "214100", dialHz: 14_074_000,
                        count: 14, subMode: "FT8");

                    continue;
                }

                // The real writer's own payload, key for key, with the one key this unit is
                // weighing added to it - written through the same sink into the same file.
                telemetry.Write(
                    TelemetryCategory.Decode,
                    "on_screen",
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["kind"] = "row",
                        ["state"] = "filtered",
                        ["by"] = OnScreenBy.CqFilter,
                        ["count"] = 14,
                        ["subMode"] = "FT8",
                        ["offsetHz"] = 500.4,
                        ["slot"] = "214100",
                        ["dialHz"] = 14_074_000L,
                        ["at"] = at,
                    });
            }
        }

        var lines = Directory.GetFiles(folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Where(l => Name(l) == "on_screen")
            .ToList();

        if (lines.Count != candidates.Length)
        {
            throw new InvalidOperationException(
                "weighed " + lines.Count + " lines for " + candidates.Length + " candidates");
        }

        return candidates
            .Select((c, at) => (
                c.Name,
                System.Text.Encoding.UTF8.GetByteCount(lines[at]) + 2))
            .ToList();
    }

    private static void Busy(MainWindowViewModel model, int slots, int perSlot)
    {
        var at = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

        for (var slot = 0; slot < slots; slot++)
        {
            model.OnScreenNowForTests = at;

            for (var row = 0; row < perSlot; row++)
            {
                model.AddDecodeRowThroughTheCapForTests(new Ft8Decode(
                    at, 0.2, 500 + (row * 97), 24, Message(slot, row)));
            }

            at = at.AddSeconds(Ft8Slots.SlotSeconds);
        }
    }

    /// <summary>A message neither a CQ nor addressed to him, so the CQ toggle holds it.</summary>
    private static string Message(int slot, int row)
        => Station((slot * 41) + row) + " " + Station((slot * 41) + row + 7) + " R-11";

    private static string Station(int at)
        => "K" + (at % 9 + 1) + "A" + (char)('A' + (at % 26)) + (char)('A' + (at / 26 % 26));

    private List<string> Run(Action<MainWindowViewModel> fixture)
    {
        var folder = Path.Combine(_folder, "run-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "381", _ => true))
        {
            var settings = new AppSettings
            {
                ReconnectOnStartup = false,
                DecodedShowCq = true,
            };

            settings.Operator.Callsign = OwnCall;
            settings.Operator.GridSquare = "FN00DJ";

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                TapForTests = new AudioTap(),
                ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
            };

            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

            fixture(model);

            model.FlushOnScreenForTests();
        }

        return Directory.GetFiles(folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();
    }

    private static void Play(MainWindowViewModel model, string fixture)
    {
        var audio = WavAudio.Read(Path.Combine(Root(), "assets", "fixtures", fixture));
        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
            model.LookForASlotForTests();
        }
    }

    private static IEnumerable<JsonElement> Data(IEnumerable<string> lines, string eventName)
        => lines
            .Where(l => Name(l) == eventName)
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

    private static double? Number(JsonElement data, string field)
        => data.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    private static int Count(JsonElement data)
        => data.TryGetProperty("count", out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : 0;

    private static bool Has(JsonElement data, string field)
        => data.TryGetProperty(field, out _);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
