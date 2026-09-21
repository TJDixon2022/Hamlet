using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
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
/// Work instruction 380 task 1: **what the record says about a decoded row today, and what an
/// hour of saying more would cost.**
/// </summary>
/// <remarks>
/// <para>**A TRACE, NOT A CRITERION.** It builds nothing, changes no source file, repairs
/// nothing and asserts nothing about the product. Everything it prints is composed on the
/// development machine - **computed, not seen** (FACT-004). No port is opened, no device is
/// enumerated and nothing is keyed.</para>
/// <para>**WHAT DECIDES TASK 3.** Item 3's arithmetic - a counted row rate times a measured
/// byte length against criterion 3.4's 50 kB an hour - is where the sampling budget comes from.
/// A budget chosen and then found to pass is worth nothing (work instruction 380 section 6
/// ruling 2 item 5).</para>
/// <para>**WHY THERE ARE TWO METHODS WHERE THE INSTRUCTION NAMES ONE.** Items 1 to 5 and item 7
/// are view-model readings and run as the plain <c>[Fact]</c> the instruction asks for. Item 6
/// is about a <c>ScrollViewer</c>, which only exists inside a built window, and a plain
/// <c>[Fact]</c> cannot build one - it needs <c>[AvaloniaFact]</c>. Splitting it also
/// keeps the six items that do not need a window out of the way of the headless dispatcher-loop
/// fault (unit 375's item 3). Both run by name in one filtered invocation, and the deviation is
/// reported in section 4.</para>
/// </remarks>
public sealed class Unit380TraceTests : IDisposable
{
    private const string OwnCall = "KC3QIS";

    /// <summary>Messages in the 2026-09-04 21:41 UTC slot, on the shack machine.</summary>
    /// <remarks>
    /// **THE PROJECT'S ONE MEASURED FT8 ROW RATE**, cited from
    /// <see cref="HowFastTheDecodedTableGrowsTests"/>, which says in its own remarks that this
    /// tree has no radio and no FT8 audio fixture. What this trace COUNTS is how many of a slot
    /// this busy actually reach the table through the real path, and what a row costs in bytes;
    /// what it takes from the shack is only how many the decoder offered.
    /// </remarks>
    private const int BusySlotDecodes = 14;

    /// <summary>How many busy slots the row-rate fixture runs for.</summary>
    private const int BusySlots = 8;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public Unit380TraceTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit380-trace-" + Guid.NewGuid().ToString("N"));

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
            // A left-over temp folder is not a trace failure.
        }
    }

    /// <summary>Items 1 to 5 and item 7, in the instruction's order.</summary>
    [Fact]
    public void Unit380Trace()
    {
        ItemOne();
        ItemTwo();
        ItemThree();
        ItemFour();
        ItemFive();
        ItemSeven();
    }

    // ---- 1. The four-signal fixture, and this is 3.3's before -----------------

    private void ItemOne()
    {
        Head("1. THE FOUR-SIGNAL FIXTURE - 3.3's BEFORE");

        foreach (var cqOnly in new[] { false, true })
        {
            var lines = new List<string>();

            using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
            {
                var model = Listening(telemetry, cqOnly);

                Play(model, "psk31-four-signals.wav");

                _output.WriteLine("psk31-four-signals.wav, carriers 700 1100 1600 2200, "
                    + "CQ filter " + (cqOnly ? "ON" : "off"));

                Lists(model);

                model.ChooseDigitalModeCommand.Execute("FT8");
            }

            lines.AddRange(ReadLines());

            _output.WriteLine("  telemetry lines written by this run: " + lines.Count);

            foreach (var line in lines)
            {
                _output.WriteLine("    " + line);
            }

            _output.WriteLine("  by event name: " + Tally(lines));
            _output.WriteLine("");
        }

        _output.WriteLine("THE ANSWER, IN ONE SENTENCE:");
        _output.WriteLine("  From those lines alone a reader CANNOT say which of the four rows "
            + "reached the screen: not one line names a list, a gate or a fate - "
            + "psk31_line_parsed says a line was parsed and psk31_squelch says what the "
            + "squelch read, and both are about the carrier and never about the row.");
        _output.WriteLine("");
    }

    // ---- 2. The same question with a gate that does fire ----------------------

    private void ItemTwo()
    {
        Head("2. A GATE THAT DOES FIRE - THE CQ TOGGLE ON AN FT8-SHAPED ROW, "
            + "AND A SHUT SQUELCH ON A TEXT ROW");

        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var model = WithFt8Rows(telemetry, cqOnly: true);

            _output.WriteLine("six FT8-shaped rows, CQ filter ON:");
            Lists(model);

            _output.WriteLine("  every row and where it went:");

            foreach (var row in model.DigitalDecodes)
            {
                _output.WriteLine("    " + Side(model, row).PadRight(9) + row.Message);
            }
        }

        foreach (var line in ReadLines())
        {
            _output.WriteLine("    " + line);
        }

        _output.WriteLine("");

        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var model = Listening(telemetry, cqOnly: false);

            Play(model, "psk31-snr-10db-1000hz.wav");

            _output.WriteLine("psk31-snr-10db-1000hz.wav, a carrier at the edge of the squelch:");

            Lists(model);

            foreach (var row in model.DigitalDecodes)
            {
                _output.WriteLine("    " + Side(model, row).PadRight(9)
                    + "textOnly=" + row.IsTextOnly
                    + " heardNotReadable=" + row.HeardNotReadable
                    + " chars=" + row.Message.Length);
            }

            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        var shut = ReadLines();

        _output.WriteLine("  squelch lines: "
            + string.Join(" | ", shut.Where(l => l.Contains("psk31_squelch", StringComparison.Ordinal))));
        _output.WriteLine("  by event name: " + Tally(shut));
        _output.WriteLine("");

        _output.WriteLine("WHAT THE TWO CASES SHOW, AND IT DISAGREES WITH SECTION 5:");
        _output.WriteLine("  the CQ toggle DOES hold FT8-shaped rows off the left list, and "
            + "nothing in the file says so or names it.");
        _output.WriteLine("  the squelch is NOT a gate on a text row's VISIBILITY at all. "
            + "Over this fixture it opened and shut many times on one carrier and the row "
            + "stayed on a list throughout; where nothing has been read yet the row is "
            + "still drawn, carrying the heard-not-readable words instead of text. The "
            + "squelch gates what the row SAYS, never whether it is drawn. Section 4 has it.");
        _output.WriteLine("");
    }

    // ---- 3. The row rate, which sets the budget -------------------------------

    private void ItemThree()
    {
        Head("3. THE ROW RATE, THE BYTE LENGTH, AND THE BUDGET");

        var placed = 0;
        var changes = 0;

        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var model = Model(telemetry, cqOnly: false);

            model.DigitalDecodes.CollectionChanged += (_, _) => changes++;
            model.DigitalVisibleDecodes.CollectionChanged += (_, _) => changes++;
            model.DigitalMineDecodes.CollectionChanged += (_, _) => changes++;

            var before = model.DigitalDecodes.Count;

            for (var slot = 0; slot < BusySlots; slot++)
            {
                var start = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc)
                    .AddSeconds(slot * Ft8Slots.SlotSeconds);

                for (var at = 0; at < BusySlotDecodes; at++)
                {
                    model.AddDecodeRowForTests(
                        start.ToString("HHmmss", CultureInfo.InvariantCulture),
                        "-1" + (at % 10),
                        "0.2",
                        (500 + (at * 97)).ToString(CultureInfo.InvariantCulture),
                        BusyMessage(slot, at),
                        start,
                        14074000);
                }
            }

            placed = model.DigitalDecodes.Count - before;

            _output.WriteLine("a busy FT8 fixture - " + BusySlotDecodes
                + " decodes a slot (the shack's 2026-09-04 21:41 UTC reading) over "
                + BusySlots + " slots:");
            _output.WriteLine("  decodes offered      : " + (BusySlots * BusySlotDecodes));
            _output.WriteLine("  rows PLACED, counted : " + placed);
            _output.WriteLine("  collection changes   : " + changes
                + "  (the number of moments a row's list membership actually changed)");
        }

        ReadLines();

        var rowsPerSlot = placed / (double)BusySlots;
        var slotsPerHour = 3600.0 / Ft8Slots.SlotSeconds;
        var rowsPerHour = rowsPerSlot * slotsPerHour;
        var changesPerHour = changes / (double)BusySlots * slotsPerHour;

        _output.WriteLine("  rows per slot        : " + rowsPerSlot.ToString("0.00", CultureInfo.InvariantCulture));
        _output.WriteLine("  slots per hour       : " + slotsPerHour.ToString("0", CultureInfo.InvariantCulture)
            + "  (FT8 slot " + Ft8Slots.SlotSeconds + " s)");
        _output.WriteLine("  ROWS PER HOUR        : " + rowsPerHour.ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("  list changes per hour: " + changesPerHour.ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("");

        var envelope = MeasureLine("the schema-B envelope alone, empty data",
            new Dictionary<string, object?>(StringComparer.Ordinal));

        var full = MeasureLine("the full candidate - a slotted row",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["kind"] = "row",
                ["state"] = "filtered",
                ["by"] = "cq_filter",
                ["whereHz"] = 1234.5,
                ["slot"] = "214135",
                ["dialHz"] = 14074000L,
                ["count"] = 1,
                ["subMode"] = "FT8",
            });

        var lean = MeasureLine("the lean candidate - a text row, no slot and no dial",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["kind"] = "row",
                ["state"] = "drawn",
                ["by"] = "",
                ["whereHz"] = 1234.5,
                ["count"] = 1,
                ["subMode"] = "PSK31",
            });

        _output.WriteLine("  BYTES, MEASURED ON THE FILE:");
        _output.WriteLine("    envelope alone : " + envelope);
        _output.WriteLine("    full candidate : " + full
            + "  (" + (full - envelope) + " of payload)");
        _output.WriteLine("    lean candidate : " + lean
            + "  (" + (lean - envelope) + " of payload)");
        _output.WriteLine("");

        var budget = 50.0 * 1024;
        var perHourBytes = rowsPerHour * full;

        _output.WriteLine("  the arithmetic, one event per row at the full candidate:");
        _output.WriteLine("    " + rowsPerHour.ToString("0", CultureInfo.InvariantCulture)
            + " rows an hour x " + full + " bytes = "
            + (perHourBytes / 1024).ToString("0.0", CultureInfo.InvariantCulture)
            + " kB an hour, against 3.4's 50 kB");
        _output.WriteLine("    VERDICT: " + (perHourBytes <= budget
            ? "ONE EVENT PER ROW FITS. The literal reading of 3.1 is available."
            : "ONE EVENT PER ROW DOES NOT FIT - it is "
                + (perHourBytes / budget).ToString("0.0", CultureInfo.InvariantCulture)
                + " times the budget. The counted reading of section 6 ruling 1 item 2 "
                + "is the one that can hold."));
        _output.WriteLine("");

        var linesAt50 = budget / full;
        var linesAt25 = budget / 2 / full;

        _output.WriteLine("  WHAT 50 kB AN HOUR BUYS, at " + full + " bytes a line:");
        _output.WriteLine("    lines an hour        : "
            + Math.Floor(linesAt50).ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("    seconds between lines: "
            + (3600 / linesAt50).ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine("    per slot             : "
            + (linesAt50 / slotsPerHour).ToString("0.00", CultureInfo.InvariantCulture)
            + " events a slot - LESS THAN ONE, so the window cannot be the slot");
        _output.WriteLine("  and at half of it, 25 kB an hour:");
        _output.WriteLine("    lines an hour        : "
            + Math.Floor(linesAt25).ToString("0", CultureInfo.InvariantCulture));
        _output.WriteLine("    seconds between lines: "
            + (3600 / linesAt25).ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine("");

        _output.WriteLine("  THE HEAD BUDGET THAT FOLLOWS, and it is arithmetic and not a "
            + "number anybody liked:");

        foreach (var windowSeconds in new[] { 15.0, 30.0, 60.0, 120.0 })
        {
            var windows = 3600 / windowSeconds;
            var perWindow = linesAt50 / windows;

            _output.WriteLine("    a " + windowSeconds.ToString("0", CultureInfo.InvariantCulture)
                + " s window gives " + windows.ToString("0", CultureInfo.InvariantCulture)
                + " windows an hour and " + perWindow.ToString("0.0", CultureInfo.InvariantCulture)
                + " lines a window at 50 kB");
        }

        _output.WriteLine("");
    }

    /// <summary>Write one candidate line and measure the file it landed in.</summary>
    /// <remarks>
    /// **MEASURED ON THE FILE AND NOT ESTIMATED FROM THE STRING** (task 1 item 3). The
    /// serialiser, the schema-B envelope, the line ending and the encoding all count against
    /// 3.4's 50 kB, and only the file knows all four.
    /// </remarks>
    private int MeasureLine(string label, IReadOnlyDictionary<string, object?> data)
    {
        var folder = Path.Combine(_folder, "one-line-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "380", _ => true))
        {
            telemetry.Write(TelemetryCategory.Decode, "on_screen", data);
        }

        var file = Directory.GetFiles(folder, "*.jsonl").Single();
        var text = File.ReadAllText(file);
        var bytes = (int)new FileInfo(file).Length;

        _output.WriteLine("  " + label + ", " + bytes + " bytes, verbatim off the file:");
        _output.WriteLine("    " + text.TrimEnd('\r', '\n'));

        Directory.Delete(folder, true);

        return bytes;
    }

    // ---- 4. The card side ------------------------------------------------------

    private void ItemFour()
    {
        Head("4. THE CARDS - WHAT Reconcile DOES AND WHAT THE RECORD SAYS ABOUT IT");

        var appeared = 0;
        var removed = 0;
        var moved = 0;

        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var model = Model(telemetry, cqOnly: false);

            model.DigitalCards.CollectionChanged += (_, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        appeared += e.NewItems?.Count ?? 0;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        removed += e.OldItems?.Count ?? 0;
                        break;
                    case NotifyCollectionChangedAction.Move:
                        moved++;
                        break;
                }
            };

            var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

            foreach (var message in new[]
            {
                "CQ W1AW FN31",
                "W1AW " + OwnCall + " FN00",
                OwnCall + " W1AW -12",
                "W1AW " + OwnCall + " R-09",
                "CQ DX EA3QQ JN11",
            })
            {
                model.AddDecodeRowForTests(
                    slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                    "-11", "0.2", "1240", message, slot, 14074000);

                slot = slot.AddSeconds(Ft8Slots.SlotSeconds);
            }

            model.CardsNowForTests = slot;
            model.RebuildCardsForTests();

            _output.WriteLine("an FT8 exchange and a CQ, through the real card path:");
            _output.WriteLine("  cards standing : " + model.DigitalCards.Count);
            _output.WriteLine("  appeared       : " + appeared);
            _output.WriteLine("  removed        : " + removed);
            _output.WriteLine("  moved          : " + moved);

            foreach (var card in model.DigitalCards)
            {
                _output.WriteLine("    card: " + card.Callsign
                    + ", " + card.StateWord + ", offers " + card.ActionLabel);
            }

            // **AND ONE DISMISSED**, so the trace has a remove through `Reconcile` and
            // not only an insert. The ledger, the log and the record are untouched by it
            // (the command's own remarks); only the panel changes.
            var standing = model.DigitalCards.Select(c => c.Callsign).ToList();

            foreach (var who in standing)
            {
                model.ClearCardCommand.Execute(who);
            }

            model.RebuildCardsForTests();

            _output.WriteLine("  after dismissing every card by hand:");
            _output.WriteLine("    cards standing : " + model.DigitalCards.Count);
            _output.WriteLine("    appeared       : " + appeared);
            _output.WriteLine("    removed        : " + removed);
        }

        var lines = ReadLines();

        _output.WriteLine("  telemetry the whole card run wrote: " + Tally(lines));

        foreach (var line in lines)
        {
            _output.WriteLine("    " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("THE FOUR CARD STATES 3.2 LISTS, AND WHAT THE FILE DISTINGUISHES TODAY:");
        _output.WriteLine("  appeared    : NOT WRITTEN. decodes_drawn carries a card COUNT per "
            + "slot, so a reader can subtract two slots and infer that one appeared - it "
            + "cannot say which, or where, or why.");
        _output.WriteLine("  dismissed   : NOT WRITTEN. The same count falling is the only trace.");
        _output.WriteLine("  folded      : NOT WRITTEN. panel_toggled says the panel shut and "
            + "carries no count of what was inside it.");
        _output.WriteLine("  scrolled out: NOT WRITTEN, and Hamlet has no handle on it today.");
        _output.WriteLine("");
    }

    // ---- 5. The fold, before ---------------------------------------------------

    private void ItemFive()
    {
        Head("5. THE FOLD - panel_toggled VERBATIM, AND WHAT WAS INSIDE");

        using (var telemetry = new JsonlTelemetry(_folder, "380", _ => true))
        {
            var model = WithFt8Rows(telemetry, cqOnly: false);

            model.CardsNowForTests = new DateTime(2026, 9, 21, 21, 45, 0, DateTimeKind.Utc);
            model.RebuildCardsForTests();

            _output.WriteLine("at the moment the decoded panel folds:");
            _output.WriteLine("  rows on the left list : " + model.DigitalVisibleDecodes.Count);
            _output.WriteLine("  rows on the whole table: " + model.DigitalDecodes.Count);

            model.DigitalDecodedExpanded = false;

            _output.WriteLine("at the moment the For You panel folds:");
            _output.WriteLine("  rows on the right side: " + model.DigitalMineDecodes.Count);
            _output.WriteLine("  cards in the panel    : " + model.DigitalCards.Count);

            model.DigitalMineExpanded = false;

            model.DigitalDecodedExpanded = true;
            model.DigitalMineExpanded = true;
        }

        var lines = ReadLines();

        foreach (var line in lines.Where(
            l => l.Contains("panel_toggled", StringComparison.Ordinal)))
        {
            _output.WriteLine("    " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("  panel_toggled carries the panel key and open/shut AND NOTHING ELSE. "
            + "How many rows and how many cards went behind the fold - the numbers printed "
            + "above - are exactly what the event does not carry.");
        _output.WriteLine("");
    }

    // ---- 7. The privacy walk, before -------------------------------------------

    private void ItemSeven()
    {
        Head("7. THE PRIVACY WALK, BEFORE");

        var source = Path.Combine(Root(), "src", "Hamlet.App", "Telemetry", "AppEvents.cs");
        var writers = File.ReadLines(source)
            .Count(l => l.StartsWith("    public static void ", StringComparison.Ordinal));

        var guard = Path.Combine(
            Root(), "tests", "Hamlet.App.Tests", "Telemetry", "CallsignPrivacyTests.cs");

        var expected = File.ReadLines(guard)
            .Select(l => l.Trim())
            .First(l => l.StartsWith("private const int ExpectedEventMethodCount", StringComparison.Ordinal));

        _output.WriteLine("  public static void writers on AppEvents : " + writers);
        _output.WriteLine("  CallsignPrivacyTests says               : " + expected);
        _output.WriteLine("");
    }

    // ---- 6. The scroller, and whether it is readable headlessly ----------------

    /// <summary>Item 6: extent, viewport, offset and the visible index range, headless.</summary>
    /// <remarks>
    /// **SECTION 6 RULING 1 ITEM 3 DEPENDS ON THIS ANSWER.** A per-panel scroll-settle event
    /// carrying a first and last visible index is only writable if Hamlet can read that range;
    /// if it cannot, that is a finding and never an invented number (CLAUDE.md 0.0).
    /// </remarks>
    [AvaloniaFact]
    public void Unit380TraceTheScroller()
    {
        Head("6. THE SCROLLER - IS THE VISIBLE INDEX RANGE READABLE HEADLESSLY?");

        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = OwnCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null) { OperatingMode = "Digital" };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        var slot = new DateTime(2026, 9, 21, 21, 41, 0, DateTimeKind.Utc);

        for (var at = 0; at < 40; at++)
        {
            model.AddDecodeRowForTests(
                slot.ToString("HHmmss", CultureInfo.InvariantCulture),
                "-1" + (at % 10), "0.2",
                (500 + (at * 37)).ToString(CultureInfo.InvariantCulture),
                "CQ " + Station(at) + " FN31", slot, 14074000);
        }

        var window = new Hamlet.App.Views.MainWindow { DataContext = model };

        window.Width = 1100;
        window.Height = 780;
        window.Show();

        Dispatcher();

        var rows = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalDecodedRows");

        if (rows is null)
        {
            _output.WriteLine("  DigitalDecodedRows is not in the drawn window - "
                + "the index range cannot be read here at all.");
            return;
        }

        var scroller = rows.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();

        if (scroller is null)
        {
            _output.WriteLine("  the decoded list has no ScrollViewer ancestor in the drawn "
                + "window - section 6 ruling 1 item 3 cannot be met without new view code.");
            return;
        }

        _output.WriteLine("  the decoded list's scroller is UNNAMED in MainWindow.axaml "
            + "(MainWindow.axaml 4871), reached here as the ItemsControl's nearest "
            + "ScrollViewer ancestor. A ScrollChanged handler would need it named.");
        _output.WriteLine("  rows bound: " + model.DigitalVisibleDecodes.Count);
        _output.WriteLine("");

        foreach (var offset in new[] { 0.0, 200.0 })
        {
            scroller.Offset = new Vector(scroller.Offset.X, offset);

            Dispatcher();

            var range = VisibleRange(rows, scroller);

            _output.WriteLine("  at offset " + Px(scroller.Offset.Y) + ":");
            _output.WriteLine("    extent   " + Px(scroller.Extent.Height)
                + "  viewport " + Px(scroller.Viewport.Height)
                + "  offset " + Px(scroller.Offset.Y));
            _output.WriteLine("    first visible index " + range.First
                + ", last visible index " + range.Last
                + ", containers realized " + range.Realized
                + " of " + model.DigitalVisibleDecodes.Count);
        }

        _output.WriteLine("");
        _output.WriteLine("THE ANSWER:");
        _output.WriteLine("  the index range IS readable without new view code. The decoded "
            + "ItemsControl does not virtualize, so every row has a realized container, and "
            + "a container's Bounds against the scroller's offset and viewport gives the "
            + "first and last visible index by arithmetic. What task 2 adds at the view is a "
            + "ScrollChanged handler and a name on the scroller - no control, no visual "
            + "element, no layout change.");
        _output.WriteLine("");

        window.Close();
    }

    private static (int First, int Last, int Realized) VisibleRange(
        ItemsControl rows, ScrollViewer scroller)
    {
        var first = -1;
        var last = -1;
        var realized = 0;

        var top = scroller.Offset.Y;
        var bottom = top + scroller.Viewport.Height;

        for (var at = 0; at < rows.ItemCount; at++)
        {
            if (rows.ContainerFromIndex(at) is not Control container)
            {
                continue;
            }

            realized++;

            var box = container.Bounds;
            var start = box.Y;
            var end = box.Y + box.Height;

            if (end <= top || start >= bottom)
            {
                continue;
            }

            if (first < 0)
            {
                first = at;
            }

            last = at;
        }

        return (first, last, realized);
    }

    private static void Dispatcher()
        => Avalonia.Threading.Dispatcher.UIThread.RunJobs();

    private static string Px(double value)
        => value.ToString("0", CultureInfo.InvariantCulture) + " px";

    // ---- the plumbing ---------------------------------------------------------

    private void Head(string title)
    {
        _output.WriteLine("");
        _output.WriteLine("=== " + title + " ===");
        _output.WriteLine("");
    }

    private void Lists(MainWindowViewModel model)
    {
        _output.WriteLine("  table " + model.DigitalDecodes.Count
            + ", visible " + model.DigitalVisibleDecodes.Count
            + ", mine " + model.DigitalMineDecodes.Count);
        _output.WriteLine("  shown " + model.DigitalShownCount
            + ", mineCount " + model.DigitalMineCount
            + ", hidden " + model.DigitalHiddenCount);
    }

    private static string Side(MainWindowViewModel model, DigitalDecodeRow row)
        => model.DigitalVisibleDecodes.Contains(row)
            ? "left"
            : model.DigitalMineDecodes.Contains(row)
                ? "right"
                : "NOWHERE";

    private static void Play(MainWindowViewModel model, string fixture)
    {
        var audio = WavAudio.Read(Fixture(fixture));
        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, count), audio.SampleRate);
            model.LookForASlotForTests();
        }
    }

    private static MainWindowViewModel Model(JsonlTelemetry telemetry, bool cqOnly)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, DecodedShowCq = cqOnly };

        settings.Operator.Callsign = OwnCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private static MainWindowViewModel Listening(JsonlTelemetry telemetry, bool cqOnly)
    {
        var model = Model(telemetry, cqOnly);

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    private static MainWindowViewModel WithFt8Rows(JsonlTelemetry telemetry, bool cqOnly)
    {
        var model = Model(telemetry, cqOnly);

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
                "-11", "0.2", "1240", message, slot, 14074000);
        }

        return model;
    }

    private static string BusyMessage(int slot, int at)
        => "CQ " + Station((slot * BusySlotDecodes) + at) + " FN31";

    private static string Station(int at)
        => "K" + (at % 9 + 1) + "AB" + (char)('A' + (at % 26));

    private List<string> ReadLines()
    {
        var files = Directory.GetFiles(_folder, "*.jsonl");
        var lines = files.SelectMany(File.ReadAllLines).ToList();

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // A file still held is not a trace failure.
            }
        }

        return lines;
    }

    private static string Tally(IEnumerable<string> lines)
    {
        var names = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            string name;

            try
            {
                name = JsonDocument.Parse(line).RootElement
                    .GetProperty("event").GetString() ?? "?";
            }
            catch (JsonException)
            {
                name = "?";
            }

            names[name] = names.GetValueOrDefault(name) + 1;
        }

        return names.Count == 0
            ? "(none)"
            : string.Join(", ", names.OrderBy(p => p.Key).Select(p => p.Key + " " + p.Value));
    }

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

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
