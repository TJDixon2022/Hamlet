using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-034: the decoder reports the speed estimate with a proof state of
/// proved, hypothesis or none. Verification row 034: T, any condition, the
/// proof-state field present, synthetic (work instruction 451, task 2;
/// PHASE_PLAN.md 5.5).
/// </summary>
/// <remarks>
/// <para>**EVERY CASE IS BUILT BY THE GENERATOR AT 15 dB OVER ITS SHAPED NOISE
/// BAND** (V-06), and its truth comes from construction: the speed sent, where
/// the keyed audio ends, and where the speed and the pitch change. None of it is
/// read back from what the decoder reports (CLAUDE.md 12.5). The decoder starts
/// cold at the operator's 600 Hz and is fed a hop at a time, as the floors feed
/// it.</para>
/// <list type="bullet">
/// <item>**none:** the band and no tone. No hop is proved or a hypothesis.</item>
/// <item>**proved:** 20 wpm at 640 Hz, the CQ twice. Over the middle half of the
/// message most hops are proved, and every proved hop's speed is within 10% of
/// 20 (HM-REQ-031's measure, here only as the proved case's sanity).</item>
/// <item>**first moments:** on the same send, before the number is first named,
/// no hop is proved, and every hop whose window holds a reading is a hypothesis.
/// The tree's window of this kind is the re-acquisition after the acquisition
/// follow off 600 Hz; where the tree has none the case says so.</item>
/// <item>**stale hold:** 20 wpm, then the band alone. Past the lapse no hop is
/// proved, and at least one holds its reading as a hypothesis.</item>
/// <item>**re-acquiring:** 16 wpm at 640 Hz, then 24 wpm (50% faster) at 740 Hz,
/// a different station. Over a window's length from the second station's first
/// mark no hop is proved, and every hop with a reading is a hypothesis. In the
/// second of band before that mark, anything proved is the first station's 16.
/// (Run first from the join itself, 72 hops in its first 0.35 s were proved at
/// 16, the first station's own speed before anything had changed:
/// `.run-unit/unit451-req034-green2.txt`.)</item>
/// </list>
/// <para>**THE LAPSE IS SEVEN SECONDS AFTER THE KEYED AUDIO ENDS**: the survey's
/// three seconds of history, one survey interval of half a second, the tracker's
/// six surveys of recent keying, three seconds, and half a second over. Author's,
/// overrulable.</para>
/// <para>**A SPEED CHANGE AT ONE PITCH IS PRINTED, NOT JUDGED.** The tree has no
/// re-acquisition for it: the window straddles the two speeds and names a number,
/// which is HM-REQ-032's to judge.</para>
/// <para>**WHAT THIS DOES NOT PROVE** (12.5): that the state is right on the air.
/// Textbook spacing, one tone at a time, generated noise; the real set's hops by
/// state are task 1's table, and an inferred key is not proof (V-13).</para>
/// </remarks>
public sealed class TheSpeedSaysWhetherItWasProvedTests
{
    /// <summary>How long after the keyed audio ends its evidence is taken to have lapsed.</summary>
    private const double LapseSeconds = 7.0;

    /// <summary>The generator's band before the first mark of every rendering, `CwFixtureGenerator.cs` 140.</summary>
    private const double LeadInSeconds = 1.0;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cases are printed.</param>
    public TheSpeedSaysWhetherItWasProvedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One hop's report.</summary>
    /// <param name="Seconds">The end of the hop.</param>
    /// <param name="Wpm">The speed named, or null.</param>
    /// <param name="Reading">True when the window holds a reading.</param>
    /// <param name="State">Its proof state.</param>
    private sealed record Hop(double Seconds, int? Wpm, bool Reading, CwSpeedProof State);

    /// <summary>
    /// HM-REQ-034: none on an empty band, proved on a keyed send read well after
    /// its first characters, and a hypothesis where the reading is withheld,
    /// held past its keying, or straddles a change of station.
    /// </summary>
    [Fact]
    public void HmReq034TheSpeedIsReportedAsProvedHypothesisOrNone()
    {
        var failures = new List<string>();
        var seed = 20260451;

        // none: the band, the tone four hundred decibels down.
        var noneHops = Drive(Render(Keyed(20, 640, seed++) with { SignalToNoiseDb = -400 }));

        Check(failures, "none", noneHops.All(h => h.State == CwSpeedProof.None),
            $"{noneHops.Count(h => h.State != CwSpeedProof.None)} of {noneHops.Count} hops not none");

        // proved: 20 wpm at 640, twice through the CQ so the message has a middle.
        var keyed = Render(Keyed(20, 640, seed++));
        var provedHops = Drive(keyed);
        var (first, last) = Keying(keyed);
        var middle = provedHops.Where(h => h.Seconds >= first + ((last - first) / 4) && h.Seconds <= last - ((last - first) / 4)).ToList();
        var proved = provedHops.Where(h => h.State == CwSpeedProof.Proved).ToList();

        Check(failures, "proved", middle.Count(h => h.State == CwSpeedProof.Proved) * 2 > middle.Count,
            $"{middle.Count(h => h.State == CwSpeedProof.Proved)} of {middle.Count} middle hops proved");
        Check(failures, "proved, within 10% of 20 wpm", proved.Count > 0 && proved.All(h => h.Wpm is { } w && Math.Abs(w - 20) <= 2),
            $"proved hops by speed {string.Join(", ", proved.GroupBy(h => h.Wpm).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}"))}");

        // first moments: before the number is first named.
        var named = provedHops.FindIndex(h => h.Wpm is not null);
        var before = named < 0 ? provedHops.ToList() : provedHops.Take(named).ToList();
        var withheld = before.Where(h => h.Reading).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-034 | first moments, the window | number first named at {(named < 0 ? double.NaN : provedHops[named].Seconds):0.00} s | {withheld.Count} hops before it hold a reading{(withheld.Count == 0 ? " - the tree has no such window here" : "")}"));
        Check(failures, "first moments", before.All(h => h.State != CwSpeedProof.Proved) && withheld.All(h => h.State == CwSpeedProof.Hypothesis),
            $"before the number: {Counts(before)} of {before.Count} hops; with a reading: {Counts(withheld)} of {withheld.Count}");

        // stale hold: 20 wpm, then the band for as long again.
        var (stale, ends) = Followed(Keyed(20, 640, seed++), seed++);
        var after = Drive(stale).Where(h => h.Seconds >= ends + LapseSeconds).ToList();

        Check(failures, "stale hold", after.Count > 0 && after.All(h => h.State != CwSpeedProof.Proved) && after.Any(h => h.State == CwSpeedProof.Hypothesis),
            string.Create(Invariant, $"past {ends + LapseSeconds:0.0} s: {Counts(after)} of {after.Count} hops, {after.Count(h => h.Wpm is not null)} of them naming a number"));

        // re-acquiring: 16 wpm at 640, then 24 wpm at 740, a different station.
        // The second station's first mark is the generator's lead-in after the
        // join; before it the band holds the first station's tail and nothing new.
        var (change, joinAt) = Joined(Keyed(16, 640, seed++, once: true), Keyed(24, 740, seed++));
        var at = joinAt + LeadInSeconds;
        var changeHops = Drive(change);
        var tail = changeHops.Where(h => h.Seconds >= joinAt && h.Seconds < at && h.State == CwSpeedProof.Proved).ToList();
        var inside = changeHops.Where(h => h.Seconds >= at && h.Seconds < at + CwProbabilisticStream.WindowSeconds).ToList();
        var reading = inside.Where(h => h.Reading).ToList();

        Check(failures, "re-acquiring, the first station's tail", tail.All(h => h.Wpm is { } w && Math.Abs(w - 16) <= 1.6),
            string.Create(Invariant, $"{joinAt:0.0} to {at:0.0} s, before the second station keys: proved hops by speed, 16 sent {string.Join(", ", tail.GroupBy(h => h.Wpm).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}"))}"));
        Check(failures, "re-acquiring", reading.Count > 0 && inside.All(h => h.State != CwSpeedProof.Proved) && reading.All(h => h.State == CwSpeedProof.Hypothesis),
            string.Create(Invariant, $"{at:0.0} to {at + CwProbabilisticStream.WindowSeconds:0.0} s from the second station's first mark: {Counts(inside)} of {inside.Count} hops; with a reading {Counts(reading)} of {reading.Count}"));
        foreach (var g in inside.Where(h => h.State == CwSpeedProof.Proved).GroupBy(h => h.Wpm))
        {
            _output.WriteLine(string.Create(Invariant,
                $"HM-REQ-034 | re-acquiring, proved inside the window | {g.Key} wpm x{g.Count()} from {g.Min(h => h.Seconds):0.00} to {g.Max(h => h.Seconds):0.00} s"));
        }

        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-034 | re-acquiring, after the window | proved hops by speed, 24 sent {string.Join(", ", changeHops.Where(h => h.Seconds >= at + CwProbabilisticStream.WindowSeconds && h.State == CwSpeedProof.Proved).GroupBy(h => h.Wpm).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}"))}"));

        // printed, not judged: the same change at one pitch, which the tree does not re-acquire.
        var (onePitch, at2) = Joined(Keyed(16, 640, seed++, once: true), Keyed(24, 640, seed++));
        var oneHops = Drive(onePitch).Where(h => h.Seconds >= at2 && h.Seconds < at2 + CwProbabilisticStream.WindowSeconds).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-034 | speed change at one pitch, printed not judged (HM-REQ-032's) | {at2:0.0} to {at2 + CwProbabilisticStream.WindowSeconds:0.0} s: {Counts(oneHops)} | proved hops by speed, 16 then 24 sent: {string.Join(", ", oneHops.Where(h => h.State == CwSpeedProof.Proved).GroupBy(h => h.Wpm).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}"))}"));

        Assert.True(failures.Count == 0, "HM-REQ-034 red on: " + string.Join("; ", failures));

        // The field is present and carries all three values across these cases:
        // verification row 034's inspection half.
        var seen = noneHops.Concat(provedHops).Concat(changeHops).Select(h => h.State).ToHashSet();

        Assert.Contains(CwSpeedProof.None, seen);
        Assert.Contains(CwSpeedProof.Proved, seen);
        Assert.Contains(CwSpeedProof.Hypothesis, seen);
    }

    private void Check(List<string> failures, string name, bool ok, string detail)
    {
        _output.WriteLine($"HM-REQ-034 | {name} | {(ok ? "green" : "RED")} | {detail}");

        if (!ok)
        {
            failures.Add($"{name} ({detail})");
        }
    }

    private static string Counts(IEnumerable<Hop> hops)
        => string.Join(", ", Enum.GetValues<CwSpeedProof>().Select(s => $"{s} {hops.Count(h => h.State == s)}"));

    /// <summary>The state the report carries.</summary>
    /// <remarks>
    /// Watched red first on HEAD's field alone, a named number proved and anything
    /// else none: first moments (2107 hops with a reading called none), the stale
    /// hold (780 of 6121 hops past the lapse proved) and re-acquiring (1172 of 2400
    /// proved, 1228 none) (`.run-unit/unit451-req034-red2.txt`). The final cases
    /// on the same field, once more: first moments 2107, the stale hold 780 of
    /// 6121, re-acquiring from the second station's first mark 972 of 2400 proved
    /// and 1428 none (`.run-unit/unit451-req034-red3.txt`).
    /// </remarks>
    private static CwSpeedProof StateOf(CwDecoder decoder) => decoder.Report.SpeedProof;

    private static CwFixtureRecipe Keyed(double wpm, double hz, int seed, bool once = false)
        => SyntheticCq.Recipe(wpm, CwFixtureCatalogue.EasyDb, seed) with
        {
            Text = once ? SyntheticCq.Text : SyntheticCq.Text + " " + SyntheticCq.Text,
            ToneHz = hz,
            DriftHz = 0,
        };

    private static MonoAudio Render(CwFixtureRecipe recipe) => CwFixtureGenerator.Generate(recipe).Audio;

    /// <summary>Where the keying starts and ends in a rendering, measured from the audio.</summary>
    private static (double First, double Last) Keying(MonoAudio audio)
    {
        var keying = RenderedKeying.Measure(audio);
        var lead = 1.0;
        var span = (keying.Marks.Sum() + keying.Spaces.Sum()) / 1000.0;

        return (lead, lead + span);
    }

    /// <summary>The keyed recipe, then the band for as long again, past a whole window.</summary>
    /// <returns>The audio, and the second the keyed rendering ends.</returns>
    private static (MonoAudio Audio, double Ends) Followed(CwFixtureRecipe recipe, int seed)
        => Joined(recipe, recipe with { SignalToNoiseDb = -400, Seed = seed });

    /// <summary>One rendering, then another, each with its own noise band.</summary>
    /// <returns>The audio, and the second the first rendering ends.</returns>
    private static (MonoAudio Audio, double Ends) Joined(CwFixtureRecipe first, CwFixtureRecipe second)
    {
        var a = Render(first);
        var b = Render(second);
        var joined = new float[a.Samples.Length + b.Samples.Length];

        a.Samples.CopyTo(joined.AsSpan());
        b.Samples.CopyTo(joined.AsSpan(a.Samples.Length));

        return (new MonoAudio(a.SampleRate, joined), a.Samples.Length / (double)a.SampleRate);
    }

    private static List<Hop> Drive(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var hop = decoder.Tracker.HopSamples;
        var hops = new List<Hop>();

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            hops.Add(new Hop((at + hop) / (double)audio.SampleRate, decoder.WordsPerMinute, decoder.Reading.Text.Length > 0, StateOf(decoder)));
        }

        decoder.Flush();

        return hops;
    }
}
