using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-093: the decoder reports pitch with a proof state of proved, hypothesis
/// or none. Verification row 093: I and T, any condition, proof-state field,
/// present, synthetic (work instruction 450, task 2; PHASE_PLAN.md 4.6).
/// </summary>
/// <remarks>
/// <para>**EVERY CASE IS BUILT BY THE GENERATOR AT 15 dB OVER ITS SHAPED NOISE
/// BAND** (V-06), and its truth comes from construction: which pitch was keyed,
/// where the keyed audio ends, and where a louder unkeyed carrier sits. None of
/// it is read back from what the tracker reports (CLAUDE.md 12.5). The decoder
/// starts cold at the operator's 600 Hz and is fed a hop at a time, as the floors
/// feed it.</para>
/// <list type="bullet">
/// <item>**none:** the band and no tone. Nothing is held on any hop.</item>
/// <item>**proved:** 640 Hz keyed. The instrument puts the audio within one of its
/// bins of 640, and over the middle half of the message most hops are proved. How
/// far the proved pitch sits from 640 is printed and not judged: that is
/// HM-REQ-092, whose N is TBD, and the tracker's choice is 4.4's (work instruction
/// 450, section 7).</item>
/// <item>**level:** 625 Hz keyed under a steady 525 Hz carrier six decibels
/// louder, then the keying stops and the carrier stays. No hop on the carrier is
/// proved, and once the keying's evidence has lapsed nothing is.</item>
/// <item>**stale hold:** 640 Hz keyed, then the keying stops, once into the band
/// alone and once into the key held down at 640. Past the lapse the pitch is
/// still held and is a hypothesis, not proved.</item>
/// </list>
/// <para>**THE SURVEY-CANDIDATE CASE IS NOT HERE, BECAUSE THE TREE HAS NO SUCH
/// WINDOW.** Before a candidate is confirmed the tracker holds no pitch at all
/// (`CwToneTracker.cs` 318, 1026, 1048), so there is no candidate's pitch for the
/// state to describe; the level case takes its place.</para>
/// <para>**THE LAPSE IS FOUR SECONDS AFTER THE KEYED AUDIO ENDS**: the survey's
/// three seconds of history (<see cref="CwToneSurvey"/>'s default), one survey
/// interval of half a second, and half a second over. Author's, overrulable.</para>
/// <para>**WHAT THIS DOES NOT PROVE** (12.5): that the state is right on the air.
/// One tone, textbook spacing, generated noise; the real set's hops by state are
/// task 1's table, and an inferred key is not proof (V-13).</para>
/// </remarks>
public sealed class ThePitchSaysWhetherItWasProvedTests
{
    /// <summary>How long after the keyed audio ends its evidence is taken to have lapsed.</summary>
    private const double LapseSeconds = 4.0;

    /// <summary>The tracker's fine spacing, in hertz: its own resolution.</summary>
    private const double FineSpacingHz = 5;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cases are printed.</param>
    public ThePitchSaysWhetherItWasProvedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One hop's report, as the operator's sheet would read it.</summary>
    /// <param name="Seconds">The end of the hop.</param>
    /// <param name="ToneHz">The pitch reported.</param>
    /// <param name="State">Its proof state.</param>
    private sealed record Hop(double Seconds, double ToneHz, CwPitchProof State);

    /// <summary>
    /// HM-REQ-093: none on an empty band, proved on a keyed tone read after its
    /// keying is confirmed, never proved on a louder unkeyed carrier, and a
    /// hypothesis once the keying has stopped.
    /// </summary>
    [Fact]
    public void HmReq093ThePitchIsReportedAsProvedHypothesisOrNone()
    {
        var failures = new List<string>();
        var seed = 20260450;

        // none: the band, the tone four hundred decibels down.
        var empty = Render(Keyed(640, seed++) with { SignalToNoiseDb = -400 });
        var noneHops = Drive(empty);

        Check(failures, "none", noneHops.All(h => h.State == CwPitchProof.None),
            $"{noneHops.Count(h => h.State != CwPitchProof.None)} of {noneHops.Count} hops not none");

        // proved: 640 keyed, twice through the CQ so the message has a middle.
        var keyed = Render(Keyed(640, seed++));
        var windows = CwPitchInstrument.Measure(keyed.Samples, keyed.SampleRate);
        var provedHops = Drive(keyed);
        var first = windows.Min(w => w.StartSeconds);
        var last = windows.Max(w => w.EndSeconds);
        var middle = provedHops.Where(h => h.Seconds >= first + ((last - first) / 4) && h.Seconds <= last - ((last - first) / 4)).ToList();
        var proved = provedHops.Where(h => h.State == CwPitchProof.Proved).ToList();

        Check(failures, "proved, the construction", windows.All(w => Math.Abs(w.Hz - 640) <= CwPitchInstrument.BinHz),
            string.Create(Invariant, $"instrument {string.Join(", ", windows.Select(w => w.Hz.ToString("0.0", Invariant)))}"));
        Check(failures, "proved", middle.Count(h => h.State == CwPitchProof.Proved) * 2 > middle.Count,
            $"{middle.Count(h => h.State == CwPitchProof.Proved)} of {middle.Count} middle hops proved");

        // How far the proved pitch sits from the construction is HM-REQ-092's,
        // whose N is TBD: printed, never judged. Proved says the latest survey
        // confirmed keying at the bin it names, not that the bin is within N Hz.
        _output.WriteLine(string.Create(Invariant,
            $"HM-REQ-092, printed not judged | proved hops by reported pitch, 640 sent | {string.Join(", ", proved.GroupBy(h => h.ToneHz).OrderBy(g => g.Key).Select(g => $"{g.Key:0.0} x{g.Count()}"))} | within the {FineSpacingHz:0} Hz fine spacing {proved.Count(h => Math.Abs(h.ToneHz - 640) <= FineSpacingHz)} of {proved.Count}"));

        // level: 625 keyed under a 525 carrier six decibels louder, which stays on
        // after the keying stops.
        var (level, levelEnds) = WithCarrier(Keyed(625, seed++), 525, 6, seed++);
        var levelHops = Drive(level);

        Check(failures, "level, on the carrier", levelHops.Where(h => Math.Abs(h.ToneHz - 525) <= 15).All(h => h.State != CwPitchProof.Proved),
            $"{levelHops.Count(h => Math.Abs(h.ToneHz - 525) <= 15 && h.State == CwPitchProof.Proved)} hops proved at the carrier");
        Check(failures, "level, after the keying", levelHops.Where(h => h.Seconds >= levelEnds + LapseSeconds).All(h => h.State != CwPitchProof.Proved),
            $"{levelHops.Count(h => h.Seconds >= levelEnds + LapseSeconds && h.State == CwPitchProof.Proved)} of {levelHops.Count(h => h.Seconds >= levelEnds + LapseSeconds)} hops past the lapse proved, beside the carrier alone");

        // stale hold: the keying stops, into the band and into the key held down.
        foreach (var (name, heldDown) in new[] { ("stale hold, the band", false), ("stale hold, key held down", true) })
        {
            var (stale, ends) = ThenStops(Keyed(640, seed++), heldDown, seed++);
            var after = Drive(stale).Where(h => h.Seconds >= ends + LapseSeconds).ToList();

            Check(failures, name, after.Count > 0 && after.All(h => h.State == CwPitchProof.Hypothesis),
                string.Create(Invariant, $"past {ends + LapseSeconds:0.0} s: {string.Join(", ", Enum.GetValues<CwPitchProof>().Select(s => $"{s} {after.Count(h => h.State == s)}"))} of {after.Count} hops"));
        }

        Assert.True(failures.Count == 0, "HM-REQ-093 red on: " + string.Join("; ", failures));

        // The field is present and carries all three values across these cases:
        // verification row 093's inspection half.
        var seen = noneHops.Concat(provedHops).Select(h => h.State).ToHashSet();

        Assert.Contains(CwPitchProof.None, seen);
        Assert.Contains(CwPitchProof.Proved, seen);
        Assert.Contains(CwPitchProof.Hypothesis, seen);
    }

    private void Check(List<string> failures, string name, bool ok, string detail)
    {
        _output.WriteLine($"HM-REQ-093 | {name} | {(ok ? "green" : "RED")} | {detail}");

        if (!ok)
        {
            failures.Add($"{name} ({detail})");
        }
    }

    /// <summary>The state the report carries.</summary>
    /// <remarks>
    /// Watched red first with the state derived from HEAD's two flags, asserted to
    /// hypothesis, measured to proved, else none: the level case past the lapse and
    /// both stale holds, 841 of 841 hops proved each
    /// (`.run-unit/unit450-req093-red.txt`).
    /// </remarks>
    private static CwPitchProof StateOf(CwDecodeReport report) => report.PitchProof;

    private static CwFixtureRecipe Keyed(double hz, int seed)
        => SyntheticCq.Recipe(20, CwFixtureCatalogue.EasyDb, seed) with
        {
            Text = SyntheticCq.Text + " " + SyntheticCq.Text,
            ToneHz = hz,
            DriftHz = 0,
        };

    private static MonoAudio Render(CwFixtureRecipe recipe) => CwFixtureGenerator.Generate(recipe).Audio;

    /// <summary>The keyed recipe, then the band for fifteen seconds more.</summary>
    /// <returns>The audio, and the second the keyed rendering ends.</returns>
    private static (float[] Samples, double Ends) Followed(CwFixtureRecipe recipe, int seed)
    {
        var keyed = Render(recipe);
        var band = Render(recipe with { Text = "CQ CQ CQ", SignalToNoiseDb = -400, Seed = seed });
        var joined = new float[keyed.Samples.Length + band.Samples.Length];

        keyed.Samples.CopyTo(joined.AsSpan());
        band.Samples.CopyTo(joined.AsSpan(keyed.Samples.Length));

        return (joined, keyed.Samples.Length / (double)keyed.SampleRate);
    }

    /// <summary>The tone's peak amplitude at a level over the band, the generator's own rule.</summary>
    private static double Peak(float[] samples, double overDb)
    {
        // The band's RMS from the lead-in, before any mark, as the instrument's
        // own two-station case reads it.
        var lead = (int)(0.9 * CwFixtureGenerator.SampleRate);
        var bandRms = Math.Sqrt(samples.Take(lead).Sum(v => (double)v * v) / lead);

        return Math.Sqrt(2) * bandRms * Math.Pow(10, overDb / 20);
    }

    private static (MonoAudio Audio, double Ends) WithCarrier(CwFixtureRecipe recipe, double carrierHz, double louderDb, int seed)
    {
        var (samples, ends) = Followed(recipe, seed);
        var peak = Peak(samples, recipe.SignalToNoiseDb + louderDb);

        for (var n = 0; n < samples.Length; n++)
        {
            samples[n] += (float)(peak * Math.Sin(2 * Math.PI * carrierHz * n / CwFixtureGenerator.SampleRate));
        }

        return (new MonoAudio(CwFixtureGenerator.SampleRate, samples), ends);
    }

    private static (MonoAudio Audio, double Ends) ThenStops(CwFixtureRecipe recipe, bool heldDown, int seed)
    {
        var (samples, ends) = Followed(recipe, seed);

        if (heldDown)
        {
            // The key goes down at the join and stays down: the same note at the
            // keyed level, with the generator's five millisecond rise.
            var peak = Peak(samples, recipe.SignalToNoiseDb);
            var from = (int)Math.Round(ends * CwFixtureGenerator.SampleRate);
            var rise = (int)(0.005 * CwFixtureGenerator.SampleRate);

            for (var n = from; n < samples.Length; n++)
            {
                var ramp = Math.Min(1.0, (n - from) / (double)rise);

                samples[n] += (float)(ramp * peak * Math.Sin(2 * Math.PI * recipe.ToneHz * n / CwFixtureGenerator.SampleRate));
            }
        }

        return (new MonoAudio(CwFixtureGenerator.SampleRate, samples), ends);
    }

    private static IReadOnlyList<Hop> Drive(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var hop = decoder.Tracker.HopSamples;
        var hops = new List<Hop>();

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            var report = decoder.Report;

            hops.Add(new Hop((at + hop) / (double)audio.SampleRate, report.ToneHz, StateOf(report)));
        }

        decoder.Flush();

        return hops;
    }
}
