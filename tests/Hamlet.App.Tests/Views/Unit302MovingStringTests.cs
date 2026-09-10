using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 302 task 1: **find the string on the Digital tab that moves.**
/// </summary>
/// <remarks>
/// <para>**STAND THE APPLICATION UP. DO NOT SEARCH THE SOURCE.** That is the
/// instruction's own rule and it is unit 284's finding: the moving text is composed
/// at run time, so a source search comes back empty and reads as clean. Five orders
/// before 284 proved the opposite way round.</para>
/// <para>**WHAT THIS DOES.** It realizes the Digital tab exactly as
/// `HowMuchTheApplicationSaysTests` does, writes down every string on it with the
/// control that owns it, and saves that to a file. Run it twice with time in
/// between and the two files differ by precisely the moving string and nothing
/// else.</para>
/// <para>**THE DEFECT IT CHASES HAS APPEARED THREE NIGHTS RUNNING**: 1281 and 1293
/// an hour apart in unit 300, then 1259 in unit 301, while the CW tab held at 528
/// and the Voice tab at 518 to the character. **A ceiling set against a moving
/// number cannot be set correctly**, so two rows have been red and unraisable.</para>
/// </remarks>
public sealed class Unit302MovingStringTests
{
    /// <summary>Where a run's transcript is left for the next run to compare.</summary>
    private static string Transcript(string run)
        => Path.Combine(Path.GetTempPath(), "hamlet-unit302-" + run + ".txt");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the transcript is printed.</param>
    public Unit302MovingStringTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Write down everything the Digital tab says, right now.**</summary>
    /// <remarks>
    /// **A PROBE THAT ALWAYS PASSES.** It records rather than judges: the judgement
    /// is the diff between two of these, and a run that failed would leave nothing
    /// to diff.
    /// </remarks>
    [AvaloniaFact]
    public void WriteDownWhatTheDigitalTabSaysNow()
    {
        var run = Environment.GetEnvironmentVariable("HAMLET_UNIT302_RUN") ?? "a";

        var window = HowMuchTheApplicationSaysTests.SurfaceForSweep(
            "MainWindow — Digital tab, working");

        var lines = new List<string>();
        var total = 0;

        foreach (var (owner, text) in Said(window))
        {
            total += text.Length;

            lines.Add(
                text.Length.ToString("0000", CultureInfo.InvariantCulture)
                + "  " + owner.PadRight(28) + "|" + text + "|");
        }

        lines.Sort(StringComparer.Ordinal);

        var stamp = DateTime.Now.ToString(
            "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        File.WriteAllLines(
            Transcript(run),
            new[]
            {
                "run " + run + " at " + stamp,
                "total " + total.ToString(CultureInfo.InvariantCulture),
                "blocks " + lines.Count.ToString(CultureInfo.InvariantCulture),
            }.Concat(lines));

        _output.WriteLine("run " + run + " at " + stamp);
        _output.WriteLine("total  " + total);
        _output.WriteLine("blocks " + lines.Count);
        _output.WriteLine("written to " + Transcript(run));

        window.Close();
    }

    /// <summary>**Diff the two runs and name what moved.**</summary>
    /// <remarks>
    /// **RUN `WriteDownWhatTheDigitalTabSaysNow` TWICE FIRST**, with
    /// `HAMLET_UNIT302_RUN` set to `a` and then to `b`, some minutes apart. With
    /// only one transcript on disk this prints that and stops, because a diff
    /// against nothing would read as *nothing moved*.
    /// </remarks>
    [AvaloniaFact]
    public void DiffTheTwoRuns()
    {
        var a = Transcript("a");
        var b = Transcript("b");

        if (!File.Exists(a) || !File.Exists(b))
        {
            _output.WriteLine(
                "only one transcript on disk, so there is nothing to diff yet.");
            _output.WriteLine("  a: " + (File.Exists(a) ? "present" : "missing"));
            _output.WriteLine("  b: " + (File.Exists(b) ? "present" : "missing"));

            return;
        }

        var first = File.ReadAllLines(a);
        var second = File.ReadAllLines(b);

        _output.WriteLine(first[0] + "   ->   " + second[0]);
        _output.WriteLine(first[1] + "   ->   " + second[1]);
        _output.WriteLine(first[2] + "   ->   " + second[2]);
        _output.WriteLine("");

        var onlyInFirst = first.Skip(3).Except(second.Skip(3), StringComparer.Ordinal)
            .ToList();
        var onlyInSecond = second.Skip(3).Except(first.Skip(3), StringComparer.Ordinal)
            .ToList();

        _output.WriteLine("ONLY IN THE FIRST RUN  (" + onlyInFirst.Count + ")");

        foreach (var line in onlyInFirst)
        {
            _output.WriteLine("  - " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("ONLY IN THE SECOND RUN (" + onlyInSecond.Count + ")");

        foreach (var line in onlyInSecond)
        {
            _output.WriteLine("  + " + line);
        }
    }

    /// <summary>**Diff one surface against itself, seconds apart, in one run.**</summary>
    /// <remarks>
    /// **THE TWO-FILE DIFF FOUND THE COUNTDOWN AND MISSED A BIGGER MOVER.** The idle
    /// Digital tab measured 1284 and then 1309 with its live readouts present in one
    /// sweep and absent in the next, which is whole blocks of text appearing rather
    /// than a number ticking. This takes both snapshots in one run so the difference
    /// can be read directly.
    /// </remarks>
    [AvaloniaFact]
    public void DiffASurfaceAgainstItselfSecondsApart()
    {
        foreach (var surface in new[]
        {
            "MainWindow — Digital tab",
            "MainWindow — Digital tab, working",
        })
        {
            _output.WriteLine("=== " + surface + " ===");

            var first = Snapshot(surface);

            var until = DateTime.UtcNow.AddSeconds(2.0);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var second = Snapshot(surface);

            foreach (var line in first.Except(second, StringComparer.Ordinal))
            {
                _output.WriteLine("  - " + line);
            }

            foreach (var line in second.Except(first, StringComparer.Ordinal))
            {
                _output.WriteLine("  + " + line);
            }

            _output.WriteLine("");
        }
    }

    /// <summary>**Measure one surface repeatedly and print the spread.**</summary>
    [AvaloniaFact]
    public void MeasureTheIdleTabRepeatedly()
    {
        var seen = new List<int>();
        var previousTotal = -1;
        List<string> previous = new();

        for (var i = 0; i < 34; i++)
        {
            var window = HowMuchTheApplicationSaysTests.SurfaceForSweep(
                "MainWindow — Digital tab");

            var total = HowMuchTheApplicationSaysTests.CharsForSweep(window);

            var now = Said(window)
                .Select(pair =>
                    pair.Text.Length.ToString("0000", CultureInfo.InvariantCulture)
                    + "  " + pair.Owner.PadRight(24) + "|" + pair.Text + "|")
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToList();

            seen.Add(total);

            _output.WriteLine(
                "run " + i.ToString("00", CultureInfo.InvariantCulture) + ": "
                + total.ToString(CultureInfo.InvariantCulture)
                + " chars, " + now.Count + " blocks");

            // **DUMP THE DIFFERENCE AT THE MOMENT THE TOTAL MOVES**, which is the
            // only moment it can be seen: a snapshot pair taken at a fixed gap
            // catches the tick and misses the flip.
            if (previousTotal >= 0 && total != previousTotal)
            {
                _output.WriteLine(
                    "      *** MOVED " + previousTotal + " -> " + total);

                foreach (var line in previous.Except(now, StringComparer.Ordinal))
                {
                    _output.WriteLine("      - " + line);
                }

                foreach (var line in now.Except(previous, StringComparer.Ordinal))
                {
                    _output.WriteLine("      + " + line);
                }
            }

            previousTotal = total;
            previous = now;

            window.Close();

            var until = DateTime.UtcNow.AddSeconds(1.1);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            "spread: " + seen.Min() + " to " + seen.Max()
            + "   (" + (seen.Max() - seen.Min()) + " apart)");
    }

    /// <summary>Every line one surface says right now.</summary>
    private static List<string> Snapshot(string surface)
    {
        var window = HowMuchTheApplicationSaysTests.SurfaceForSweep(surface);

        try
        {
            return Said(window)
                .Select(pair =>
                    pair.Text.Length.ToString("0000", CultureInfo.InvariantCulture)
                    + "  " + pair.Owner.PadRight(24) + "|" + pair.Text + "|")
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToList();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Everything the window says, with the control that says it.</summary>
    /// <remarks>
    /// **THE SAME SWEEP THE CEILING USES**, so what is listed here is exactly what
    /// is counted there: visible `TextBlock` and `GlossaryTextControl` text, with the
    /// byline left out because it rotates by design.
    /// </remarks>
    internal static IEnumerable<(string Owner, string Text)> Said(Window root)
        => root.GetVisualDescendants()
            .Select(v => v switch
            {
                TextBlock block when block.IsEffectivelyVisible
                    => (Owner(block), block.Text),
                GlossaryTextControl gloss when gloss.IsEffectivelyVisible
                    => (Owner(gloss), gloss.Text),
                _ => ((string?)null, (string?)null),
            })
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Item2))
            .Select(pair => (pair.Item1 ?? "(unnamed)", pair.Item2!))
            .Where(pair => pair.Item1 != "Byline");

    /// <summary>The nearest name anything up the tree carries.</summary>
    private static string Owner(Control control)
    {
        for (var at = (Control?)control; at is not null; at = at.Parent as Control)
        {
            if (at.Name is { Length: > 0 } name)
            {
                return name;
            }
        }

        return "(unnamed)";
    }
}
