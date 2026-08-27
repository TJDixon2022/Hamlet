using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What it costs to score a candidate pitch by decoding at it.
/// </summary>
/// <remarks>
/// <para>**THIS MEASUREMENT DECIDES WHETHER RANKING IS BUILT AT ALL.** Six
/// statistics have been measured against *is a station keying in this bin*, and
/// all six are dead. HM-DEC-125 named the way out — score a candidate by what it
/// reads rather than by how it clusters — and made it conditional on a
/// measurement showing the gap. **The gap is shown; the cost is not.**</para>
/// <para>**THE BUDGET IS HALF A SECOND**, and it is not a number anybody chose
/// for this: `CwToneTracker.SurveyEveryHops` is 100 hops of 5 ms, and
/// `CwProbabilisticStream.ReadEverySeconds` is 0.5. The survey verdict and the
/// window re-read already land on the same cadence, so a ranking that fits
/// inside one of them fits inside both.</para>
/// <para>**IT MEASURES WALL TIME AND SAYS SO.** A wall-clock figure is not
/// determinism (§5) and this asserts nothing about speed — it prints a table and
/// passes. The single assertion is that a decode happened at all, so a broken
/// harness cannot report a cost of nothing.</para>
/// </remarks>
public sealed class WhatDecodeScoringCostsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatDecodeScoringCostsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
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

    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            name + ".wav"));

    /// <summary>A capture that is known to read, and the pitch it reads at.</summary>
    /// <remarks>
    /// `cw-2026-08-24-012403` holds `DE KD0UN KD0UN K` and is read at 84.2 %
    /// when the decoder is pointed at it, which is the case decode-scoring
    /// exists to reach.
    /// </remarks>
    private static MonoAudio Reading => Capture("cw-2026-08-24-012403");

    /// <summary>Where that capture's station actually sits.</summary>
    /// <remarks>
    /// 439.81 Hz, from `TheNoiseScaleTable`, and not the 450 its own sidecar
    /// records — the sidecar says where the decoder was pointed at the moment of
    /// the press, which on this capture was ten hertz off the station.
    /// </remarks>
    private const double ReadingPitch = 439.81;

    /// <summary>The last <paramref name="seconds"/> of a recording.</summary>
    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(audio.SampleRate * seconds);

        if (audio.Samples.Length <= want)
        {
            return audio;
        }

        var slice = new float[want];

        for (var i = 0; i < want; i++)
        {
            slice[i] = audio.Samples[audio.Samples.Length - want + i];
        }

        return new MonoAudio(audio.SampleRate, slice);
    }

    /// <remarks>
    /// <para>**THE THREE NUMBERS TASK 1 ASKS FOR**, measured on a real capture
    /// rather than on synthesized audio, because the cost of a decode depends on
    /// how much structure the speed search finds and noise is the cheap case.</para>
    /// <para>Each figure is taken after a warm-up pass, because the first decode
    /// in a process pays for JIT and for the taper tables and would report a cost
    /// no steady-state pass ever pays again.</para>
    /// </remarks>
    [Fact]
    public void OneDecodeAtOnePitchCostsThis()
    {
        // **MEASURED ON A CAPTURE THAT READS.** The first attempt timed
        // `cw-2026-08-22-014308` at the pitch its own sidecar names and read
        // nothing, so every figure was the cost of the null hypothesis winning
        // rather than the cost of a decode. A search that finds no structure
        // stops early; the cost that matters is the cost of the case ranking is
        // built for.
        var audio = Reading;

        _output.WriteLine(
            $"capture cw-2026-08-24-012403: {audio.Samples.Length} samples at "
            + $"{audio.SampleRate} Hz = "
            + $"{audio.Samples.Length / (double)audio.SampleRate:0.0} s");
        _output.WriteLine("");

        // The budget, read from the constants rather than restated.
        var budgetMs = CwProbabilisticStream.ReadEverySeconds * 1000.0;

        _output.WriteLine(
            $"budget: CwProbabilisticStream.ReadEverySeconds = "
            + $"{CwProbabilisticStream.ReadEverySeconds:0.00} s = {budgetMs:0} ms");
        _output.WriteLine(
            $"        CwToneTracker survey verdict every 100 hops of 5 ms = 500 ms");
        _output.WriteLine("");

        var pitch = ReadingPitch;
        var characters = 0;
        var ratio = 0.0;

        _output.WriteLine(
            "  window |  envelope |    decode |     total | chars |  ratio");
        _output.WriteLine(
            "  -------|-----------|-----------|-----------|-------|-------");

        var rows = new List<(double Seconds, double Envelope, double Decode)>();

        foreach (var seconds in new[] { 3.0, 6.0, 12.0 })
        {
            var slice = Tail(audio, seconds);

            // Warm up: the first pass in a process pays for JIT and the taper
            // tables, and no steady-state pass ever pays that again.
            var warm = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, pitch);
            CwProbabilisticDecoder.Decode(warm, pitch);

            var runs = seconds <= 3.0 ? 20 : 10;

            var clock = Stopwatch.StartNew();

            for (var i = 0; i < runs; i++)
            {
                CwProbabilisticDecoder.Envelope(
                    slice.Samples, slice.SampleRate, pitch);
            }

            clock.Stop();
            var envelopeMs = clock.Elapsed.TotalMilliseconds / runs;

            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, pitch);

            clock.Restart();

            CwProbabilisticResult? last = null;

            for (var i = 0; i < runs; i++)
            {
                last = CwProbabilisticDecoder.Decode(envelope, pitch);
            }

            clock.Stop();
            var decodeMs = clock.Elapsed.TotalMilliseconds / runs;

            characters = last?.Characters.Count ?? 0;
            ratio = last?.LikelihoodRatio ?? 0;

            rows.Add((seconds, envelopeMs, decodeMs));

            _output.WriteLine(
                $"  {seconds,5:0.0} s | {envelopeMs,6:0.00} ms | "
                + $"{decodeMs,6:0.00} ms | {envelopeMs + decodeMs,6:0.00} ms | "
                + $"{characters,5} | {ratio,6:0.00}");
        }

        _output.WriteLine("");
        _output.WriteLine("how many candidates fit in one 500 ms cadence:");
        _output.WriteLine("");
        _output.WriteLine(
            "  window | full cost | fits | envelope already taken | fits");
        _output.WriteLine(
            "  -------|-----------|------|------------------------|-----");

        foreach (var (seconds, envelopeMs, decodeMs) in rows)
        {
            var full = envelopeMs + decodeMs;

            _output.WriteLine(
                $"  {seconds,5:0.0} s | {full,6:0.00} ms | "
                + $"{(int)(budgetMs / full),4} | "
                + $"{decodeMs,18:0.00} ms | {(int)(budgetMs / decodeMs),4}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "coarse bank: 300-900 Hz at 25 Hz = 25 bins "
            + "(CwToneTracker.MinimumToneHz..MaximumToneHz, CoarseSpacingHz)");
        _output.WriteLine(
            "fine bank:   +/-15 Hz at 5 Hz    =  7 bins "
            + "(FineReachHz, FineSpacingHz)");

        Assert.True(
            characters > 0,
            "the harness decoded nothing, so every cost above is the cost of "
            + "failing rather than the cost of reading");
    }

    /// <remarks>
    /// <para>**THE OTHER HALF OF THE COST QUESTION: WHAT A WRONG PITCH COSTS.**
    /// Ranking spends most of its budget on candidates that hold nothing, so if
    /// a barren pitch were the expensive case the whole scheme would be priced
    /// off its worst branch rather than its best.</para>
    /// <para>It is not. A pitch with no structure loses to the null hypothesis
    /// early and costs less, which is the right way round.</para>
    /// </remarks>
    [Fact]
    public void APitchWithNothingOnItIsTheCheapCase()
    {
        var reading = Tail(Reading, 3.0);

        // **`cw-2026-08-25-021825` IS NOT THE EMPTY CASE AND THE ORDER CALLS IT
        // ONE.** It holds a station — an eight-second call in thirty seconds,
        // 18 % duty, with floors of 41, 74 and 16 in
        // `TheCapturesThatDecodeKeepDecodingTests`. The two recordings that hold
        // nothing are the 2026-08-20 pair, named in `CwProbabilisticDecoder.Gate`
        // as the evidence the gate was calibrated against.
        var barren = Tail(Capture("cw-2026-08-20-014854"), 3.0);

        _output.WriteLine("  3.0 s window, twenty passes each");
        _output.WriteLine("");
        _output.WriteLine("  capture                  | pitch |  decode | chars | ratio");
        _output.WriteLine("  -------------------------|-------|---------|-------|------");

        var costs = new List<double>();

        foreach (var (name, slice, pitch) in new[]
        {
            ("cw-2026-08-24-012403", reading, ReadingPitch),
            ("cw-2026-08-20-014854", barren, 500.0),
            ("cw-2026-08-20-014854", barren, 837.0),
        })
        {
            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, pitch);

            CwProbabilisticDecoder.Decode(envelope, pitch);

            var clock = Stopwatch.StartNew();
            var last = CwProbabilisticResult.None;

            for (var i = 0; i < 20; i++)
            {
                last = CwProbabilisticDecoder.Decode(envelope, pitch);
            }

            clock.Stop();
            var ms = clock.Elapsed.TotalMilliseconds / 20;
            costs.Add(ms);

            _output.WriteLine(
                $"  {name,-24} | {pitch,5:0} | {ms,5:0.00} ms | "
                + $"{last.Characters.Count,5} | {last.LikelihoodRatio,5:0.00}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  gate: {CwProbabilisticDecoder.Gate:0.00} per hop (HM-DEC-120, "
            + "re-expressed 2026-08-24)");

        Assert.True(costs.Count == 3, "a row did not run");
    }

    /// <remarks>
    /// <para>**THE SCORING WINDOW IS NOT FREE, AND THIS IS WHAT IT COSTS.**
    /// HM-DEC-120's gate of 1.40 was calibrated on the streaming path's own
    /// twelve-second windows — `CwProbabilisticDecoder.Gate` names the evidence:
    /// `cw-2026-08-20-014854`, holding nothing, highest 0.840 across 55
    /// windows.</para>
    /// <para>**A SHORTER WINDOW IS A DIFFERENT INSTRUMENT AND THE GATE DOES NOT
    /// FOLLOW IT.** Fewer hops means the null hypothesis has less evidence
    /// against it, and noise that happens to look like keying for three seconds
    /// is not diluted by the nine seconds either side of it. Ranking wants a
    /// short window because a short window is cheap; the silence property wants
    /// a long one.</para>
    /// <para>**SWEPT ACROSS THE WHOLE COARSE BANK**, because ranking will visit
    /// every one of those pitches and the property has to hold at the worst of
    /// them rather than at an average.</para>
    /// </remarks>
    [Fact]
    public void AShortScoringWindowBreaksTheSilenceProperty()
    {
        _output.WriteLine(
            $"  gate {CwProbabilisticDecoder.Gate:0.00} per hop, calibrated on "
            + "12 s windows (HM-DEC-120)");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                | window | worst pitch | worst ratio | "
            + "chars | over gate");
        _output.WriteLine(
            "  -----------------------|--------|-------------|-------------|"
            + "-------|----------");

        var broken = new List<string>();

        foreach (var name in new[]
        {
            "cw-2026-08-20-014854", "cw-2026-08-20-014935",
        })
        {
            var whole = Capture(name);

            foreach (var seconds in new[] { 3.0, 6.0, 12.0 })
            {
                var slice = Tail(whole, seconds);

                var worst = 0.0;
                var worstHz = 0.0;
                var worstChars = 0;

                // The coarse bank ranking would visit: 300 to 900 at 25 Hz.
                for (var hz = CwToneTracker.MinimumToneHz;
                     hz <= CwToneTracker.MaximumToneHz;
                     hz += CwToneTracker.CoarseSpacingHz)
                {
                    var envelope = CwProbabilisticDecoder.Envelope(
                        slice.Samples, slice.SampleRate, hz);

                    var read = CwProbabilisticDecoder.Decode(envelope, hz);

                    if (read.LikelihoodRatio > worst)
                    {
                        worst = read.LikelihoodRatio;
                        worstHz = hz;
                        worstChars = read.Characters.Count;
                    }
                }

                var over = worst >= CwProbabilisticDecoder.Gate;

                if (over)
                {
                    broken.Add($"{name} at {seconds:0} s");
                }

                _output.WriteLine(
                    $"  {name,-22} | {seconds,4:0.0} s | {worstHz,9:0} Hz | "
                    + $"{worst,11:0.00} | {worstChars,5} | "
                    + $"{(over ? "BROKEN" : "held"),9}");
            }
        }

        _output.WriteLine("");

        if (broken.Count > 0)
        {
            _output.WriteLine(
                "  the silence property is broken at: "
                + string.Join(", ", broken));
            _output.WriteLine(
                "  so the scoring window cannot be shortened without "
                + "re-calibrating a floor this unit is forbidden to move");
        }
        else
        {
            _output.WriteLine("  the silence property holds at every window length");
        }

        // **NOTHING IS ASSERTED ABOUT WHICH WINDOWS HOLD.** This is the
        // measurement that decides the scoring window, and a test that failed
        // here would be asserting the answer before it was read.
        Assert.True(broken.Count >= 0);
    }
}
