using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 374 task 1: **measure both repairs before either is made.** Printed, not
/// asserted - nothing here asserts anything about new behavior, and no source file changes at this
/// task.
/// </summary>
/// <remarks>
/// <para>**WHY THE LADDER EXISTS.** Unit 373's floor met criterion 1.3 and, in meeting it, made the
/// 780-to-620 band flat: at width 1100 the panel row is `max(window height - 707, 73)`, so from 780
/// down to 620 it is 73 px at every height and `TheWorkingPanelsLoseHeightBeforeTheTopRowLosesAny`
/// has no transition to read an order off. Its `shrinks.Count >= 2` precondition is the only red on
/// criterion 1.2. Work instruction 374 §6's first ruling extends the sweep **upward** rather than
/// retuning the floor, and <see cref="TheHeightLadderAtTwoWidths"/> is where the new top comes
/// from: it is measured, not chosen.</para>
/// <para>**WHY THE RECORD TRACE EXISTS.** §R35 records `cq_pressed` writing `detail: Ft8` under
/// PSK31, seen on 2026-09-11 and 09-14, which is criterion 2.1's fault. Nine sites in
/// `MainWindowViewModel` write `_digitalMode.ToString()`, and `_digitalMode` is a two-member enum
/// whose mapping answers `Ft8` for PSK31 and for Olivia. <see cref="WhatTheRecordSaysUnderEachLabel"/>
/// prints what each of the strip's four labels actually writes, and
/// <see cref="WhatTheSendPathWritesUnderPsk31AndOlivia"/> prints which send events fire under the
/// two unslotted modes, so §6's second ruling is applied to a measurement rather than to a reading
/// of the file.</para>
/// <para>**COMPUTED, NOT SEEN** (`SHACK_FACTS.md` FACT-004). Every height number is a `Bounds`
/// rectangle off a realized headless window; every record line is read back off the file the
/// application wrote. **Nothing reaches a device** (CLAUDE.md §0.2): the send path ends at a
/// `FakePort` and a `FakeSink`, which is where it ends in every app test, and the ladder presses
/// nothing at all.</para>
/// </remarks>
public sealed class Unit374TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the ladder and the record are printed.</param>
    public Unit374TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>The two widths the instruction names: the size Hamlet opens at, and its minimum.</summary>
    private static readonly double[] Widths = { 1100, 900 };

    /// <summary>
    /// 1040 down to 620 in steps of 40 - no coarser than the 60 px the instruction allows, and it
    /// lands on every height of the sweep the type carries today.
    /// </summary>
    private static readonly double[] Ladder =
        { 1040, 1000, 960, 920, 880, 840, 800, 780, 740, 700, 660, 620 };

    /// <summary>The strip's four labels, which is what criterion 2.1 calls the sub-mode.</summary>
    private static readonly string[] Labels = { "PSK31", "Olivia", "FT8", "FT4" };

    private const long On20m = 14_070_000;
    private const double HisHz = 1234;

    // -----------------------------------------------------------------------------------------
    // 1. THE HEIGHT LADDER, AND IT DECIDES THE SWEEP
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// **Task 1 item 1.** At widths 1100 and 900, and at both of the type's `plain` values, the
    /// window box, `TopRow`, the panel row, the three panels together, the send area, and the
    /// canvas scroller's extent and viewport - then the highest height at which the panel row
    /// stops growing, and how many shrink transitions lie above it and below it.
    /// </summary>
    [AvaloniaFact]
    public void TheHeightLadderAtTwoWidths()
    {
        var verdicts = new List<string>();

        foreach (var width in Widths)
        {
            foreach (var plain in new[] { false, true })
            {
                var read = Ladder.Select(h => Measure(width, h, plain)).ToList();

                _output.WriteLine("");
                _output.WriteLine(
                    "WIDTH " + Px(width) + ", " + Which(plain)
                    + "   asked  drawn  TopRow  panelRow  panels  send  canvas viewport/extent");

                foreach (var row in read)
                {
                    _output.WriteLine(
                        "    " + Px(row.Asked).PadLeft(6) + Px(row.Drawn).PadLeft(7)
                        + Px(row.TopRow).PadLeft(8) + Px(row.PanelRow).PadLeft(10)
                        + Px(row.Panels).PadLeft(8) + Px(row.SendArea).PadLeft(6)
                        + "    " + Px(row.Viewport) + " / " + Px(row.Extent));
                }

                // **WHERE THE FLOOR BEGINS TO BIND**, read as the highest height in the ladder at
                // which the panel row is already down to what it is at the bottom of the ladder.
                var bottom = read[^1].PanelRow;
                var binds = read.FirstOrDefault(r => r.PanelRow <= bottom + 0.5);

                // **THE SHRINK RULE IS THE TEST'S OWN**, copied from line 114 of
                // `TheWindowGivesUpHeightInOneOrderTests` so the counts mean what its precondition
                // means: the three panels together, or `TopRow`, smaller than at the height above.
                var above = 0;
                var below = 0;

                for (var i = 1; i < read.Count; i++)
                {
                    var shrank = read[i].Panels < read[i - 1].Panels - 0.5
                        || read[i].TopRow < read[i - 1].TopRow - 0.5;

                    if (!shrank)
                    {
                        continue;
                    }

                    if (binds is not null && read[i].Asked < binds.Asked)
                    {
                        below++;
                    }
                    else
                    {
                        above++;
                    }
                }

                var where = binds is null
                    ? "the panel row never stops growing in this ladder"
                    : "the panel row stops growing at " + Px(binds.Asked)
                      + " px, where it is " + Px(binds.PanelRow) + " px";

                _output.WriteLine("  " + where);
                _output.WriteLine("  shrink transitions above it: " + above);
                _output.WriteLine("  shrink transitions below it: " + below);

                verdicts.Add(
                    Px(width) + " " + Which(plain) + ": floor binds at "
                    + (binds is null ? "nowhere" : Px(binds.Asked)) + ", "
                    + above + " shrinks above, " + below + " below");
            }
        }

        _output.WriteLine("");
        _output.WriteLine("WHAT THE LADDER SAYS, IN ONE LINE EACH");

        foreach (var verdict in verdicts)
        {
            _output.WriteLine("  " + verdict);
        }

        // **AND THE ANSWER TASK 2 TAKES**: the lowest top that gives the guard two shrinks at
        // width 1100 on BOTH windows while every height from 780 to 620 stays in the sweep.
        _output.WriteLine("");
        _output.WriteLine("THE TOP THE SWEEP NEEDS, MEASURED AT WIDTH 1100");

        foreach (var top in Ladder.Where(h => h > 780))
        {
            var kept = Ladder.Where(h => h <= top).ToArray();
            var counts = new List<int>();

            foreach (var plain in new[] { false, true })
            {
                var read = kept.Select(h => Measure(1100, h, plain)).ToList();
                var shrinks = 0;

                for (var i = 1; i < read.Count; i++)
                {
                    if (read[i].Panels < read[i - 1].Panels - 0.5
                        || read[i].TopRow < read[i - 1].TopRow - 0.5)
                    {
                        shrinks++;
                    }
                }

                counts.Add(shrinks);
            }

            _output.WriteLine(
                "  a sweep topped at " + Px(top) + " (" + kept.Length + " heights): "
                + counts[0] + " shrinks on the pinned-facts window, " + counts[1]
                + " on the window with content"
                + (counts.All(c => c >= 2) ? "   <- satisfies the precondition" : ""));
        }
    }

    // -----------------------------------------------------------------------------------------
    // 2. WHAT THE RECORD SAYS TODAY, PRINTED RATHER THAN QUOTED
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// **Task 1 item 2, first half.** With each of the strip's four labels chosen in turn, CQ is
    /// pressed and the `operator_action` line it writes is printed: `action`, `mode` and `detail`.
    /// </summary>
    /// <remarks>
    /// **NOTHING REACHES A DEVICE** (§0.2). The panel is given a `FakePort` and a `FakeSink`, which
    /// is where every app test's send path ends, and no slot is waited on: what is read back is
    /// what was written to the file, and the press is written before anything it triggers.
    /// </remarks>
    [AvaloniaFact]
    public void WhatTheRecordSaysUnderEachLabel()
    {
        _output.WriteLine("WHAT CQ WRITES, BY THE LABEL THE OPERATOR PRESSED");
        _output.WriteLine("  label     action      mode     detail");

        foreach (var label in Labels)
        {
            var lines = Session(label, model => model.SendCallToAnyoneCommand.Execute(null));

            foreach (var line in lines.Where(l => Action(l) == "cq_pressed"))
            {
                _output.WriteLine(
                    "  " + label.PadRight(10) + Action(line).PadRight(12)
                    + Field(line, "mode").PadRight(9) + Field(line, "detail"));
            }
        }

        _output.WriteLine("");
        _output.WriteLine("THE OTHER THREE OPERATOR ACTIONS, WHERE A PRESS IS REACHABLE");
        _output.WriteLine("  label     action                  mode     detail");

        foreach (var label in Labels)
        {
            var lines = Session(label, model =>
            {
                Hear(model);

                var row = model.DigitalDecodes.FirstOrDefault();

                if (row is null)
                {
                    return;
                }

                model.AnswerPsk31Command.Execute(row);
                model.OpenPsk31CardCommand.Execute(row);

                var card = model.DigitalCards.FirstOrDefault();

                if (card is null)
                {
                    return;
                }

                card.TypedText = "tnx fer the call";
                model.SendTypedPsk31Command.Execute(card);
            });

            var wanted = new[]
            {
                "psk31_answer_pressed", "psk31_typed_pressed", "psk31_card_opened",
            };

            var seen = lines.Where(l => wanted.Contains(Action(l))).ToList();

            if (seen.Count == 0)
            {
                _output.WriteLine("  " + label.PadRight(10) + "none of the three was reachable");

                continue;
            }

            foreach (var line in seen)
            {
                _output.WriteLine(
                    "  " + label.PadRight(10) + Action(line).PadRight(24)
                    + Field(line, "mode").PadRight(9) + Field(line, "detail"));
            }
        }
    }

    /// <summary>
    /// **Task 1 item 2, second half.** Every event name the send path writes under PSK31 and under
    /// Olivia, with its mode field, so §6's second ruling is applied to what fires rather than to a
    /// reading of the file.
    /// </summary>
    /// <remarks>
    /// **THE SLOTTED SITES ARE THE QUESTION.** Line 15675 sends PSK31 and Olivia down
    /// `SendUnslotted` and returns, above the three slotted writes at 15703, 15740 and 15791 - so
    /// if none of those three names appears here, they are unreachable under these two labels and
    /// §R14 says a unit does not edit a line to look busy.
    /// </remarks>
    [AvaloniaFact]
    public void WhatTheSendPathWritesUnderPsk31AndOlivia()
    {
        foreach (var label in new[] { "PSK31", "Olivia" })
        {
            var lines = Session(label, model =>
            {
                model.SendCallToAnyoneCommand.Execute(null);
                Settle(model);
            });

            _output.WriteLine("");
            _output.WriteLine("UNDER " + label + ", THE SEND PATH WROTE");
            _output.WriteLine("  event                              mode / stage / detail / reason");

            foreach (var line in lines)
            {
                var name = Field(line, "event");

                if (name.Length == 0)
                {
                    continue;
                }

                var mode = Field(line, "mode");
                var stage = Field(line, "stage");
                var detail = Field(line, "detail");
                var reason = Field(line, "reason");
                var action = Action(line);

                _output.WriteLine(
                    "  " + (action.Length > 0 ? name + " [" + action + "]" : name).PadRight(36)
                    + (mode.Length > 0 ? "mode=" + mode + "  " : "")
                    + (stage.Length > 0 ? "stage=" + stage + "  " : "")
                    + (detail.Length > 0 ? "detail=" + detail + "  " : "")
                    + (reason.Length > 0 ? "reason=" + reason : ""));
            }

            _output.WriteLine("  events in the file: " + lines.Count);
        }
    }

    // -----------------------------------------------------------------------------------------
    // The ladder's plumbing
    // -----------------------------------------------------------------------------------------

    /// <summary>One window at one height, in the shape the ladder prints.</summary>
    /// <param name="Asked">The height the window was asked for.</param>
    /// <param name="Drawn">The height it drew.</param>
    /// <param name="TopRow">The top row's drawn height.</param>
    /// <param name="PanelRow">The working panels' shared row height.</param>
    /// <param name="Panels">The three working panels' heights added up.</param>
    /// <param name="SendArea">The reserved send area's drawn height.</param>
    /// <param name="Viewport">`WorkspaceCanvasScroller`'s viewport height.</param>
    /// <param name="Extent">`WorkspaceCanvasScroller`'s extent height.</param>
    private sealed record Measured(
        double Asked, double Drawn, double TopRow, double PanelRow, double Panels,
        double SendArea, double Viewport, double Extent);

    private static Measured Measure(double width, double height, bool plain)
    {
        var window = plain
            ? TheWorkingPanelsTests.Realized(width, height, null)
            : TheTopRowTests.Realized(width, height, null, null);

        try
        {
            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var panels = TheWorkingPanelsTests.Panels(window);
            var canvas = TheTopRowTests.Named<ScrollViewer>(window, "WorkspaceCanvasScroller");

            return new Measured(
                height,
                window.Bounds.Height,
                TheTopRowTests.RectIn(TheTopRowTests.Named<Control>(window, "TopRow"), window).Height,
                panels[0].Rect.Height,
                panels.Sum(p => p.Rect.Height),
                TheTopRowTests.RectIn(
                    TheTopRowTests.Named<Control>(window, "DigitalSendReserved"), window).Height,
                canvas.Viewport.Height,
                canvas.Extent.Height);
        }
        finally
        {
            window.Close();
        }
    }

    private static string Which(bool plain)
        => plain ? "the window with content" : "the pinned-facts window";

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    // -----------------------------------------------------------------------------------------
    // The record's plumbing
    // -----------------------------------------------------------------------------------------

    /// <summary>One panel on one label, one act, its telemetry read back.</summary>
    private static IReadOnlyList<string> Session(string label, Action<MainWindowViewModel> act)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit374-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.13.61", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = Psk31Corpus.Load().Operator;
            settings.Operator.GridSquare = "FN00DJ";
            settings.Operator.OperatorName = "Tim";
            settings.Operator.Location = "Trafford PA";
            settings.Operator.LicenseClass = LicenseClass.General;

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                DigitalNewestFirst = false,
            };

            model.SelectedBand = model.Bands.First(
                b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
            model.FrequencyHz = On20m;

            model.UseWorkedBeforeForTests(
                new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));

            // **THE REAL ROUTE, NOT THE BACKING FIELD.** `ChosenDigitalMode` is assigned in exactly
            // one place in the application - the chip press - and this is that press.
            model.ChooseDigitalModeCommand.Execute(label);

            // **THE SEND PATH ENDS WHERE IT ENDS IN EVERY APP TEST** (§0.2): a fake port and a fake
            // sink. No device is opened, nothing reaches a sound card and nothing is on the air.
            model.UseRigPortForTests(new FakePort());
            model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));

            act(model);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            telemetry.Dispose();

            return Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this trace's business.
            }
        }
    }

    /// <summary>Hands the panel one heard station, so a row and a card are reachable.</summary>
    private static void Hear(MainWindowViewModel model)
    {
        var corpus = Psk31Corpus.Load();

        var his = corpus.Transcripts
            .Single(t => t.Name == "01-textbook")
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(1)
            .Select(l => l.Text + "\n");

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, string.Concat(his)) });
    }

    /// <summary>Lets an unslotted send finish; the click fires it and does not wait (§R10).</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static string Action(string line)
        => line.Contains("operator_action", StringComparison.Ordinal)
            ? Field(line, "action")
            : "";

    /// <summary>One field out of one jsonl line, wherever in it the writer put it.</summary>
    private static string Field(string line, string name)
    {
        var key = "\"" + name + "\":\"";
        var at = line.IndexOf(key, StringComparison.Ordinal);

        if (at < 0)
        {
            return "";
        }

        var from = at + key.Length;
        var to = line.IndexOf('"', from);

        return to < 0 ? "" : line[from..to];
    }
}
