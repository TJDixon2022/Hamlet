using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-036: when the pitch is refined for the same station, the decoder
/// retains its timing state. Verification row 036 (work instruction 451, task 2;
/// PHASE_PLAN.md 5.5). Measured, not repaired: red is committed red.
/// </summary>
/// <remarks>
/// <para>**TWO REFINEMENTS, BOTH THE TREE'S OWN.** The station's note steps 25 Hz,
/// one coarse bin, the refinement's own limit (`CwToneTracker.cs` 248,
/// `ConfirmWithinHz`), and the tracker is checked to have followed it; that is
/// what a dial nudge under the 500 Hz line does to the audio: the application
/// does not call `Retuned()` for it (`MainWindowViewModel.cs` 13606-13610), and
/// the tracker's refinement (`Switch` with `refining`, 1236-1260, or the fine
/// bank's in-reach reading, 1181-1190) follows the note. And the operator's
/// `Lock()` at the measured peak, which re-points the mixdown.</para>
/// <para>**AGAINST A CONTROL, SO THE DECODER'S OWN WOBBLE IS NOT COUNTED AS A
/// LOSS.** Each case is decoded twice from the same seeds: once with the
/// refinement and once without. The timing state is the clock's
/// (<see cref="CwDecoder.SpeedIsReacquiring"/> and the discontinuity it is
/// measured from), the speed named, and the stream's held gap structure. Retained
/// means: no re-acquisition the control does not have, the discontinuity mark
/// not moved, the held structure not dropped, and the named speed within the
/// decoder's resolution of the control's at every hop, one word a minute, the
/// rounding <see cref="CwDecoder.WordsPerMinute"/> applies.</para>
/// <para>**WHAT THIS DOES NOT PROVE** (12.5): the construction is one textbook
/// sender at 18 wpm, 15 dB, and a clean step; a drifting real note is not it.</para>
/// </remarks>
public sealed class ARefinementKeepsTheTimingTests
{
    /// <summary>The step: one coarse bin, `ConfirmWithinHz`, the refinement's own limit.</summary>
    private const double StepHz = 25;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly FieldInfo Discontinuity = typeof(CwDecoder).GetField("_samplesAtDiscontinuity", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwDecoder has no field _samplesAtDiscontinuity");

    private static readonly FieldInfo StructureHeld = typeof(CwProbabilisticStream).GetField("_structureHeld", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("CwProbabilisticStream has no field _structureHeld");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cases are printed.</param>
    public ARefinementKeepsTheTimingTests(ITestOutputHelper output)
        => _output = output;

    private sealed record Hop(double Seconds, int? Wpm, bool Reacquiring, long Discontinuity, bool StructureHeld, double ToneHz, int Retunes, int Follows);

    /// <summary>HM-REQ-036: a 25 Hz refinement of the note, and the operator's lock, keep the timing.</summary>
    [Fact]
    public void HmReq036ARefinementForTheSameStationKeepsTheTiming()
    {
        var failures = new List<string>();
        var seed = 20260036;

        // The note steps one coarse bin, 25 Hz, at the join: the same sender, the
        // same timing, the largest move the tracker still calls a refinement.
        var a = Keyed(640, seed);
        var b = Keyed(640, seed + 1);
        var (control, at) = Joined(a, b);
        var (stepped, _) = Joined(a, b with { ToneHz = 640 + StepHz });
        var steppedHops = Drive(stepped, double.NaN);

        // The construction's own check: the tracker went with the note by a
        // refinement and not a follow, so the refinement path ran. Where it did
        // not, the case tests nothing. How close it lands is HM-REQ-092's.
        var end = steppedHops[^1];
        var join = steppedHops.Last(h => h.Seconds <= at);
        var refined = end.ToneHz - join.ToneHz >= StepHz / 2 && end.Follows == join.Follows && end.Retunes > join.Retunes;

        Check(failures, "a 25 Hz step of the note", "the tracker refined with the note, no follow", refined,
            string.Create(Invariant, $"pitch {join.ToneHz:0.0} at the join, {end.ToneHz:0.0} at the end, {640 + StepHz:0} sent; retunes {join.Retunes} to {end.Retunes}, follows {join.Follows} to {end.Follows}"));
        Compare(failures, "a 25 Hz step of the note", Drive(control, double.NaN), steppedHops, at);

        // The operator locks at the measured peak three quarters of the way
        // through the first send, well after its first characters.
        var (one, _) = Joined(a, b);
        var lockAt = at * 3 / 4;

        Compare(failures, "the operator's lock at the measured peak", Drive(one, double.NaN), Drive(one, lockAt), lockAt);

        Assert.True(failures.Count == 0, "HM-REQ-036 red on: " + string.Join("; ", failures));
    }

    private void Compare(List<string> failures, string name, IReadOnlyList<Hop> control, IReadOnlyList<Hop> refined, double at)
    {
        var before = refined.Last(h => h.Seconds <= at);
        var after = refined.Where(h => h.Seconds > at).ToList();
        var pairs = control.Zip(refined).Where(p => p.Second.Seconds > at).ToList();
        var last = after[^1];

        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-036 | {name} | construction | at {at:0.00} s | pitch before {before.ToneHz:0.0}, 3 s after {refined.First(h => h.Seconds >= at + 3).ToneHz:0.0}, at the end {last.ToneHz:0.0} | retunes {before.Retunes} to {last.Retunes}, follows {before.Follows} to {last.Follows} | control's retunes {control[^1].Retunes}, follows {control[^1].Follows}"));

        Check(failures, name, "a speed named just before", before.Wpm is not null, $"{before.Wpm?.ToString(Invariant) ?? "null"} wpm at {before.Seconds:0.00} s");

        var reacquired = pairs.Count(p => p.Second.Reacquiring && !p.First.Reacquiring);

        Check(failures, name, "no re-acquisition the control does not have", reacquired == 0, $"{reacquired} of {pairs.Count} hops after");

        var moved = after.Count(h => h.Discontinuity != before.Discontinuity);

        Check(failures, name, "the discontinuity mark not moved", moved == 0, $"{moved} of {after.Count} hops after");

        var dropped = pairs.Count(p => p.First.StructureHeld && !p.Second.StructureHeld);

        Check(failures, name, "the held gap structure not dropped", dropped == 0, $"{dropped} of {pairs.Count} hops, held before {before.StructureHeld}");

        var withheld = pairs.Count(p => p.First.Wpm is not null && p.Second.Wpm is null);
        var off = pairs.Count(p => p.First.Wpm is { } c && p.Second.Wpm is { } r && Math.Abs(c - r) > 1);

        Check(failures, name, "the speed within 1 wpm of the control's", withheld == 0 && off == 0,
            $"withheld where the control names one {withheld}, more than 1 wpm apart {off}, of {pairs.Count} hops; speeds after {string.Join(", ", after.GroupBy(h => h.Wpm).OrderBy(g => g.Key).Select(g => $"{g.Key?.ToString(Invariant) ?? "null"} x{g.Count()}"))}");
    }

    private void Check(List<string> failures, string name, string what, bool ok, string detail)
    {
        _output.WriteLine($"HM-REQ-036 | {name} | {what} | {(ok ? "green" : "RED")} | {detail}");

        if (!ok)
        {
            failures.Add($"{name}, {what} ({detail})");
        }
    }

    private static CwFixtureRecipe Keyed(double hz, int seed)
        => SyntheticCq.Recipe(18, CwFixtureCatalogue.EasyDb, seed) with { ToneHz = hz, DriftHz = 0 };

    private static (MonoAudio Audio, double At) Joined(CwFixtureRecipe first, CwFixtureRecipe second)
    {
        var x = CwFixtureGenerator.Generate(first).Audio;
        var y = CwFixtureGenerator.Generate(second).Audio;
        var joined = new float[x.Samples.Length + y.Samples.Length];

        x.Samples.CopyTo(joined.AsSpan());
        y.Samples.CopyTo(joined.AsSpan(x.Samples.Length));

        return (new MonoAudio(x.SampleRate, joined), x.Samples.Length / (double)x.SampleRate);
    }

    private static List<Hop> Drive(MonoAudio audio, double lockAt)
    {
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var hop = decoder.Tracker.HopSamples;
        var hops = new List<Hop>();
        var locked = double.IsNaN(lockAt);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            var seconds = (at + hop) / (double)audio.SampleRate;

            if (!locked && seconds >= lockAt)
            {
                decoder.Lock();
                locked = true;
            }

            hops.Add(new Hop(
                seconds,
                decoder.WordsPerMinute,
                decoder.SpeedIsReacquiring,
                (long)Discontinuity.GetValue(decoder)!,
                (bool)StructureHeld.GetValue(decoder.Stream)!,
                decoder.Stream.ToneHz,
                decoder.Tracker.Retunes,
                decoder.Tracker.Follows));
        }

        decoder.Flush();

        return hops;
    }
}
