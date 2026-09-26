using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The proof state HM-REQ-034 asks for, read off the decoder hop by hop with
/// nothing in `src` changed (work instruction 451, task 1; PHASE_PLAN.md 5.5). The
/// sibling of <see cref="WhatThePitchCanSayItProvedTests"/>.
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** The state is the proposed one, read
/// from what the decoder already computes:</para>
/// <list type="bullet">
/// <item>**proved:** <see cref="CwDecoder.WordsPerMinute"/> names a number
/// (`CwDecoder.cs` 445-454: a character read, not re-acquiring, in the plausible
/// range), the window's unit behind it was measured from the keying rather than
/// won on the grid (`CwProbabilisticStream.cs` 431-435 and the marks' overrule at
/// 505-514), and the tracker found keying within half the mixdown filter of the
/// pitch being read inside its own recent span, six surveys or three seconds
/// (<see cref="CwToneTracker.KeyingRecently"/>, `CwToneTracker.cs` 1217-1224; the
/// half-width `CwDecoder.cs` 733 already uses for "the same sender").</item>
/// <item>**hypothesis:** the window holds a reading and one of those fails: the
/// clock is re-acquiring, the number is outside the plausible range, the unit was
/// won on the grid, or no keying is current at the pitch (a hold past its keying).</item>
/// </list>
/// <para>**WHY SIX SURVEYS AND NOT THE LATEST ONE.** Run first on the latest survey
/// alone (`.run-unit/unit451-trace-speed-state-run1.txt`), a clean 12 wpm send at
/// 15 dB was proved on 900 of 5681 hops: the survey does not confirm keying on
/// every half second of a slow sender, and the tree already says why that is the
/// wrong question for a reading that trails it (`CwToneTracker.cs` 702-712).</para>
/// <list type="bullet">
/// <item>**none:** nothing has been read yet, or the gate refused the whole window,
/// so the grid's winner describes no reading.</item>
/// </list>
/// <para>**THE UNIT'S PROVENANCE IS NOT PUBLISHED AT HEAD**, so it is recomputed
/// here on each read hop from the stream's own envelope with the stream's own
/// calls; that is why this reads a private field.</para>
/// </remarks>
public sealed class WhatTheSpeedCanSayItProvedTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly string[] States = { "proved", "hypothesis", "none" };

    private static readonly FieldInfo Envelope = typeof(CwProbabilisticStream).GetField("_envelope", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwProbabilisticStream has no field _envelope");

    private static readonly FieldInfo LastKeyedHz = typeof(CwToneTracker).GetField("_lastKeyedHz", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwToneTracker has no field _lastKeyedHz");

    /// <summary>The stream's refill guard, in hops: nothing is read below it.</summary>
    private static readonly int RefillHops = (int)(CwProbabilisticStream.RefillSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheSpeedCanSayItProvedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One hop: what HEAD shows, the proposed state and the path under it.</summary>
    /// <param name="Seconds">The end of the hop.</param>
    /// <param name="Shown">What HEAD's guarded answer names, or null.</param>
    /// <param name="Rolling">The window's winning speed, 0 before any read.</param>
    /// <param name="State">proved, hypothesis or none.</param>
    /// <param name="Path">Which way the decoder came to report what it reports.</param>
    internal sealed record Hop(double Seconds, int? Shown, double Rolling, string State, string Path);

    /// <summary>Whether the unit behind the last read was measured, recomputed from the window.</summary>
    /// <param name="stream">The stream, on a hop it has just read.</param>
    /// <returns>True when the estimator or the marks' overrule set the speed.</returns>
    internal static bool UnitWasMeasured(CwProbabilisticStream stream)
    {
        var window = ((double[])Envelope.GetValue(stream)!).Take(stream.EnvelopeHops).ToArray();
        var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);

        if (measured.IsReady
            && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
            && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm)
        {
            return true;
        }

        var marksUnit = CwUnitEstimator.MarkUnit(
            CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds).Marks);

        return !double.IsNaN(marksUnit)
            && marksUnit > 0
            && Math.Abs(stream.Last.WordsPerMinute - (1200.0 / marksUnit)) < 1e-6;
    }

    /// <summary>The proposed state and its path, from the decoder's public face and the unit's provenance.</summary>
    internal static (string State, string Path) Proposed(CwDecoder decoder, bool unitMeasured)
    {
        var reading = decoder.Reading;

        if (reading.WordsPerMinute <= 0)
        {
            return ("none", "nothing read yet: the window has not refilled");
        }

        if (reading.Text.Length == 0)
        {
            return ("none", "the gate refused the window: the grid's winner over it is no reading");
        }

        if (decoder.SpeedIsReacquiring)
        {
            return ("hypothesis", "a reading, the clock re-acquiring after a follow: number withheld");
        }

        if (decoder.WordsPerMinute is null)
        {
            return ("hypothesis", "a reading outside the plausible range: number withheld");
        }

        if (!unitMeasured)
        {
            return ("hypothesis", "number named, the unit won on the grid and not measured");
        }

        var lastKeyedHz = (double)LastKeyedHz.GetValue(decoder.Tracker)!;
        var atPitch = !double.IsNaN(lastKeyedHz)
            && Math.Abs(lastKeyedHz - decoder.Stream.ToneHz) < CwProbabilisticDecoder.BandwidthHz / 2;

        if (!decoder.Tracker.KeyingRecently)
        {
            return ("hypothesis", "number named, measured, held past its keying: no keying found for six surveys");
        }

        if (!atPitch)
        {
            return ("hypothesis", "number named, measured, the recent keying is elsewhere than the pitch being read");
        }

        var onLatest = decoder.Tracker.Verdict.Keyed is { } k
            && Math.Abs(k.ToneHz - decoder.Stream.ToneHz) < CwProbabilisticDecoder.BandwidthHz / 2;

        return ("proved", onLatest
            ? "number named, unit measured, keying at the pitch on the latest survey"
            : "number named, unit measured, keying at the pitch within six surveys, not the latest");
    }

    /// <summary>Drive the decoder cold, hop by hop, and keep each hop's state and path.</summary>
    internal static IReadOnlyList<Hop> Drive(MonoAudio audio, double startingHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startingHz);
        var hop = decoder.Tracker.HopSamples;
        var hops = new List<Hop>();
        var measured = false;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Stream.HopsSinceAnswer == 0 && decoder.Stream.EnvelopeHops >= RefillHops)
            {
                measured = UnitWasMeasured(decoder.Stream);
            }

            var (state, path) = Proposed(decoder, measured);

            hops.Add(new Hop((at + hop) / (double)audio.SampleRate, decoder.WordsPerMinute, decoder.Reading.WordsPerMinute, state, path));
        }

        decoder.Flush();

        return hops;
    }

    /// <summary>
    /// Task 1: per recording, the hops in each state and the hops HEAD shows a
    /// number on that the state calls a hypothesis; on the synthetic set, the
    /// proved hops' speed against the constructed speed; then every path's count.
    /// </summary>
    [Fact]
    public void EveryHopsSpeedState()
    {
        var totals = new Dictionary<string, Tally>(StringComparer.Ordinal);
        var paths = new Dictionary<string, int>(StringComparer.Ordinal);

        _output.WriteLine("row | set | recording | hops proved | hypothesis | none | hops HEAD shows a number | of those hypothesis | state at the end | proved speeds (wpm x hops)");

        foreach (var name in WhatTheStrayLettersRestOnTests.KeyedRecordings.Select(k => k.Name))
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

            Row("real", name, audio, 600, double.NaN, totals, paths);
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));

            Row("synthetic", recipe.Name, audio, SyntheticCq.StartingPitchHz, recipe.WordsPerMinute, totals, paths);
        }

        foreach (var (group, t) in totals.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            _output.WriteLine(string.Create(Invariant,
                $"total | {group} | {t.Recordings} recordings | hops proved {t.Hops[0]} | hypothesis {t.Hops[1]} | none {t.Hops[2]} | HEAD shows a number on {t.Shown} | of those hypothesis {t.ShownHypothesis}, in {t.RecordingsShownHypothesis} recordings | recordings ending proved {t.Ends[0]}, hypothesis {t.Ends[1]}, none {t.Ends[2]} | key {(group == "synthetic" ? "exact" : "inferred")}"));

            if (group == "synthetic")
            {
                _output.WriteLine(string.Create(Invariant,
                    $"HM-REQ-031, evidence only | synthetic proved hops {t.Hops[0]} | more than 10% off the constructed speed {t.ProvedOff} | HEAD's shown hops more than 10% off {t.ShownOff} of {t.Shown} | key exact"));
            }
        }

        foreach (var (path, count) in paths.OrderByDescending(p => p.Value))
        {
            _output.WriteLine($"path | {path} | {count} hops over every recording above");
        }
    }

    private void Row(
        string group, string name, MonoAudio audio, double startingHz, double constructedWpm,
        Dictionary<string, Tally> totals, Dictionary<string, int> paths)
    {
        var hops = Drive(audio, startingHz);
        var count = new int[3];

        foreach (var h in hops)
        {
            count[Array.IndexOf(States, h.State)]++;
            paths[h.Path] = paths.GetValueOrDefault(h.Path) + 1;
        }

        var shown = hops.Count(h => h.Shown is not null);
        var shownHypothesis = hops.Count(h => h.Shown is not null && h.State == "hypothesis");
        var end = hops.Count == 0 ? "none" : hops[^1].State;
        var proved = hops.Where(h => h.State == "proved").ToList();
        var speeds = string.Join(", ", proved.GroupBy(h => h.Shown).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}"));

        _output.WriteLine(string.Create(Invariant,
            $"row | {group} | {name} | {count[0]} | {count[1]} | {count[2]} | {shown} | {shownHypothesis} | {end} | {speeds}"));

        if (!totals.TryGetValue(group, out var tally))
        {
            tally = new Tally();
            totals[group] = tally;
        }

        tally.Recordings++;
        tally.Shown += shown;
        tally.ShownHypothesis += shownHypothesis;
        tally.RecordingsShownHypothesis += shownHypothesis > 0 ? 1 : 0;
        tally.Ends[Array.IndexOf(States, end)]++;

        for (var i = 0; i < 3; i++)
        {
            tally.Hops[i] += count[i];
        }

        if (double.IsNaN(constructedWpm))
        {
            return;
        }

        bool Off(int? wpm) => wpm is { } w && Math.Abs(w - constructedWpm) > 0.10 * constructedWpm;

        var provedOff = proved.Where(h => Off(h.Shown)).ToList();

        tally.ProvedOff += provedOff.Count;
        tally.ShownOff += hops.Count(h => Off(h.Shown));

        // The finding task 1 asks for: proved on a hop more than 10% off, with its path.
        foreach (var g in provedOff.GroupBy(h => (h.Shown, h.Path)))
        {
            _output.WriteLine(string.Create(Invariant,
                $"proved off | {name} | sent {constructedWpm:0} wpm | reported {g.Key.Shown} | {g.Count()} hops from {g.Min(h => h.Seconds):0.0} to {g.Max(h => h.Seconds):0.0} s | path {g.Key.Path}"));
        }
    }

    private sealed class Tally
    {
        public int Recordings { get; set; }

        public int Shown { get; set; }

        public int ShownHypothesis { get; set; }

        public int RecordingsShownHypothesis { get; set; }

        public int ProvedOff { get; set; }

        public int ShownOff { get; set; }

        public int[] Hops { get; } = new int[3];

        public int[] Ends { get; } = new int[3];
    }
}
