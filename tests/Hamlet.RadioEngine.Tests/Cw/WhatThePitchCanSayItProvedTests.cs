using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The proof state HM-REQ-093 asks for, read off the tracker hop by hop with
/// nothing in `src` changed, beside the pitch instrument (work instruction 450,
/// task 1; PHASE_PLAN.md 4.6).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** The state is the proposed one, read
/// from what the tracker already publishes:</para>
/// <list type="bullet">
/// <item>**proved:** a pitch is held (<see cref="CwToneTracker.HasMeasuredPitch"/>)
/// and the latest survey's confirmed keyed verdict names that same pitch
/// (<see cref="CwToneTracker.Verdict"/>). Every path that sets the reported pitch
/// sets it from the verdict it stores beside it, so the two are one number only
/// while the survey that set it is the latest.</item>
/// <item>**hypothesis:** a pitch is held and the latest survey does not name it:
/// a hold from an earlier survey, or a move pending mid-character.</item>
/// <item>**none:** no pitch is held; the tracker points at the operator's setting
/// or at the loudest bin from cold, and HEAD already reports that as unmeasured.</item>
/// </list>
/// <para>**THE INSTRUMENT IS 447'S AND NEVER ENTERS THE DECODER** (R76). A window's
/// state is the one held on most of its hops, the weaker on a tie; its error is the
/// tracker's median over the window less the instrument's pitch, as
/// <see cref="WhatPitchTheDecoderIsOnTests"/> computes MET-PITCH-ERR.</para>
/// <para>**WHAT WAS DROPPED** (task 1's drop candidate): the cross over all 69
/// captures. This runs the 23 keyed recordings, the files more than 25 Hz off at
/// entry, and the synthetic set.</para>
/// </remarks>
public sealed class WhatThePitchCanSayItProvedTests
{
    /// <summary>The files 447's printer put more than 25 Hz off at 450's entry, HEAD `706e3874`.</summary>
    internal static IReadOnlyList<string> OffAtEntry { get; } = new[]
    {
        "unadjudicated/cw-2026-08-20-014935", "unadjudicated/cw-2026-08-22-014308",
        "unadjudicated/cw-2026-08-22-031838", "unadjudicated/cw-2026-08-25-012823",
        "unadjudicated/cw-2026-08-28-005158", "unadjudicated/cw-2026-08-28-005218",
        "unadjudicated/cw-2026-08-28-005243", "unadjudicated/cw-2026-08-31-002424",
        "unadjudicated/cw-2026-08-31-002443",
    };

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly string[] States = { "proved", "hypothesis", "none" };

    private static readonly FieldInfo HopsSinceSurvey = typeof(CwToneTracker).GetField("_hopsSinceSurvey", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwToneTracker has no field _hopsSinceSurvey");

    private static readonly FieldInfo ReadingDb = typeof(CwToneTracker).GetField("_readingDb", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwToneTracker has no field _readingDb");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatThePitchCanSayItProvedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One hop: the pitch, the proposed state and the path under it.</summary>
    /// <param name="ToneHz">The tracker's reported pitch after the hop.</param>
    /// <param name="State">proved, hypothesis or none.</param>
    /// <param name="Path">Which way the tracker came to hold what it holds.</param>
    /// <param name="FloorRefused">
    /// On a survey hop in hypothesis, whether an admitted candidate far from the
    /// held pitch sat more than 10 dB under `_readingDb`, the station-level floor
    /// 449 left uncleared.
    /// </param>
    internal sealed record Hop(double ToneHz, string State, string Path, bool FloorRefused);

    /// <summary>The proposed state from the tracker's public face, as `src` would compute it.</summary>
    /// <param name="tracker">The tracker, after a hop.</param>
    /// <returns>proved, hypothesis or none.</returns>
    internal static string ProposedState(CwToneTracker tracker)
        => !tracker.HasMeasuredPitch ? "none"
            : tracker.Verdict.Keyed is { } keyed && keyed.ToneHz == tracker.ToneHz ? "proved"
            : "hypothesis";

    /// <summary>Drive the decoder cold, hop by hop, and keep each hop's state and path.</summary>
    /// <param name="audio">The audio.</param>
    /// <param name="startingHz">The operator's pitch it starts from.</param>
    /// <returns>Every hop, and the samples per hop.</returns>
    internal static (IReadOnlyList<Hop> Hops, int HopSamples) Drive(MonoAudio audio, double startingHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startingHz);
        var tracker = decoder.Tracker;
        var hop = tracker.HopSamples;
        var hops = new List<Hop>();

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            var state = ProposedState(tracker);
            var path = state switch
            {
                "proved" => "keyed verdict on the latest survey names the pitch",
                "none" => tracker.Follows == 0
                    ? "operator's configured pitch, nothing chose"
                    : "from-cold point, the loudest bin, nothing confirmed",
                _ => tracker.Verdict.Keyed is not null
                    ? "hold, a move to a keyed candidate elsewhere pending mid-character"
                    : tracker.KeyingRecently
                        ? "hold, keying found within the last 3 s, not on this survey"
                        : "hold past its keying, none found for 3 s or more",
            };
            var floorRefused = false;

            if (state == "hypothesis" && (int)HopsSinceSurvey.GetValue(tracker)! == 0)
            {
                var reading = (double)ReadingDb.GetValue(tracker)!;

                floorRefused = !double.IsNaN(reading) && tracker.CoarseCandidates().Any(c =>
                    Math.Abs(c.ToneHz - tracker.ToneHz) > 15
                    && !double.IsNaN(c.KeyedDb)
                    && c.KeyedDb < reading - CwToneSurvey.InterferenceLiftDb
                    && (double.IsNaN(c.LiftDb) || c.LiftDb >= CwToneSurvey.InterferenceLiftDb));
            }

            hops.Add(new Hop(tracker.ToneHz, state, path, floorRefused));
        }

        decoder.Flush();

        return (hops, hop);
    }

    /// <summary>
    /// Task 1: per recording, the hops in each state and each state's windows more
    /// than 25 Hz off the instrument; then the same totals, the paths, and the
    /// state on every window of the files more than 25 Hz off.
    /// </summary>
    [Fact]
    public void EveryHopsStateBesideTheInstrument()
    {
        var real = WhatTheStrayLettersRestOnTests.KeyedRecordings.Select(k => k.Name).ToList();
        var names = real.Concat(OffAtEntry.Where(n => !real.Contains(n))).ToList();

        _output.WriteLine("row | set | recording | hops proved | hypothesis | none | windows proved (>25 Hz off) | hypothesis (>25) | none (>25) | hops a stale floor refused on");

        var totals = new Dictionary<string, Tally>(StringComparer.Ordinal);
        var paths = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var name in names)
        {
            var set = real.Contains(name) ? (OffAtEntry.Contains(name) ? "real, keyed and over 25" : "real, keyed") : "over 25, not keyed";
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

            Row(set.StartsWith("real", StringComparison.Ordinal) ? "real" : "over 25, not keyed", set, name, audio, 600, totals, paths, OffAtEntry.Contains(name));
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));

            Row("synthetic", string.Create(Invariant, $"synthetic, sent at {recipe.ToneHz:0} Hz"), recipe.Name, audio, SyntheticCq.StartingPitchHz, totals, paths, false);
        }

        foreach (var (group, t) in totals.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            _output.WriteLine(string.Create(Invariant,
                $"total | {group} | {t.Recordings} recordings | hops proved {t.Hops[0]} | hypothesis {t.Hops[1]} | none {t.Hops[2]} | windows proved {t.Windows[0]} ({t.Over[0]} >25) | hypothesis {t.Windows[1]} ({t.Over[1]} >25) | none {t.Windows[2]} ({t.Over[2]} >25) | stale-floor refusals {t.FloorRefused} | key {(group == "synthetic" ? "exact" : "inferred")}"));
        }

        foreach (var (path, count) in paths.OrderByDescending(p => p.Value))
        {
            _output.WriteLine($"path | {path} | {count} hops over every recording above");
        }
    }

    private void Row(
        string group, string set, string name, MonoAudio audio, double startingHz,
        Dictionary<string, Tally> totals, Dictionary<string, int> paths, bool offAtEntry)
    {
        var (hops, hopSamples) = Drive(audio, startingHz);
        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);
        var hopCount = new int[3];
        var windowCount = new int[3];
        var over = new int[3];
        var floorRefused = hops.Count(h => h.FloorRefused);

        foreach (var h in hops)
        {
            hopCount[Array.IndexOf(States, h.State)]++;
            paths[h.Path] = paths.GetValueOrDefault(h.Path) + 1;
        }

        foreach (var w in windows)
        {
            var from = (int)Math.Floor(w.StartSeconds * audio.SampleRate / hopSamples);
            var to = Math.Min(hops.Count, (int)Math.Ceiling(w.EndSeconds * audio.SampleRate / hopSamples));

            if (to <= from)
            {
                continue;
            }

            var inside = hops.Skip(from).Take(to - from).ToList();

            // The state held on most hops; on a tie the weaker, which claims less.
            var state = inside
                .GroupBy(h => h.State)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => Array.IndexOf(States, g.Key))
                .First().Key;
            var s = Array.IndexOf(States, state);
            var err = Median(inside.Select(h => h.ToneHz)) - w.Hz;
            var off = Math.Abs(err) > WhatPitchTheDecoderIsOnTests.FlagHz;

            windowCount[s]++;

            if (off)
            {
                over[s]++;
            }

            // The finding task 1 asks for: proved on a window the instrument puts
            // more than 25 Hz off, with the paths its hops came by.
            var provedHops = inside.Where(h => h.State == "proved").ToList();
            var provedErr = provedHops.Count == 0 ? double.NaN : Median(provedHops.Select(h => h.ToneHz)) - w.Hz;

            if (!double.IsNaN(provedErr) && Math.Abs(provedErr) > WhatPitchTheDecoderIsOnTests.FlagHz)
            {
                _output.WriteLine(string.Create(Invariant,
                    $"proved off | {name} | {w.StartSeconds:0} to {w.EndSeconds:0} s | instrument {w.Hz:0.0} | proved hops {provedHops.Count} of {inside.Count} at median {Median(provedHops.Select(h => h.ToneHz)):0.0} | err {provedErr:+0.0;-0.0;0.0} | window's state {state} | paths {string.Join(", ", inside.GroupBy(h => h.Path).Select(g => $"{g.Key} x{g.Count()}"))}"));
            }

            if (offAtEntry && off)
            {
                _output.WriteLine(string.Create(Invariant,
                    $"off window | {name} | {w.StartSeconds:0} to {w.EndSeconds:0} s | instrument {w.Hz:0.0} | tracker {Median(inside.Select(h => h.ToneHz)):0.0} | err {err:+0.0;-0.0;0.0} | state {state} | hops {string.Join(", ", States.Select(x => $"{x} {inside.Count(h => h.State == x)}"))}"));
            }
        }

        _output.WriteLine(string.Create(Invariant,
            $"row | {set} | {name} | {hopCount[0]} | {hopCount[1]} | {hopCount[2]} | {windowCount[0]} ({over[0]}) | {windowCount[1]} ({over[1]}) | {windowCount[2]} ({over[2]}) | {floorRefused}"));

        if (!totals.TryGetValue(group, out var tally))
        {
            tally = new Tally();
            totals[group] = tally;
        }

        tally.Recordings++;
        tally.FloorRefused += floorRefused;

        for (var i = 0; i < 3; i++)
        {
            tally.Hops[i] += hopCount[i];
            tally.Windows[i] += windowCount[i];
            tally.Over[i] += over[i];
        }
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();

        return sorted.Count == 0
            ? double.NaN
            : sorted.Count % 2 == 1
                ? sorted[sorted.Count / 2]
                : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2.0;
    }

    private sealed class Tally
    {
        public int Recordings { get; set; }

        public int FloorRefused { get; set; }

        public int[] Hops { get; } = new int[3];

        public int[] Windows { get; } = new int[3];

        public int[] Over { get; } = new int[3];
    }
}
