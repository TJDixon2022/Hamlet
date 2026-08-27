using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the emission gate's window ratio is made of, on a station the filter is
/// already pointed at.
/// </summary>
/// <remarks>
/// <para>**THE QUESTION THREE UNITS MISSED.** Unit 1.11.30 measured the running
/// decoder pointed within seven hertz of two stations the operator can hear,
/// emitting nothing from either. Unit 1.11.29 measured those same windows at
/// ratios of 0.44 to 0.90 against a floor of 1.40. **So the station scores worse
/// than the noise beside it while the filter is on it**, and acquisition was
/// never what refused them.</para>
/// <para>**THE HYPOTHESIS, AND IT IS A HYPOTHESIS.**
/// <see cref="CwProbabilisticResult.LikelihoodRatio"/> is the whole window's
/// margin divided by the window's hop count, so a station sending for a fifth of
/// the window is divided by the four fifths it is silent for, while a station
/// sending continuously is not. **HM-DEC-090 already found and fixed this exact
/// shape**: the reported signal-to-noise and the located pitch were both averages
/// over the silence in a recording, and both became held peaks. The emission gate
/// was never given the same treatment.</para>
/// <para>**IT MEASURES AND ASSERTS ALMOST NOTHING.** If the hypothesis is wrong
/// the number says so, and a test that demanded the answer would decide it in
/// advance.</para>
/// </remarks>
public sealed class WhatTheWindowRatioIsMadeOfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatTheWindowRatioIsMadeOfTests(ITestOutputHelper output)
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

    /// <summary>
    /// The recordings the order names, each at the pitch its station sits on.
    /// </summary>
    private static (string Name, double ToneHz, string What)[] Cases { get; } =
    {
        ("cw-2026-08-22-014113", 600.0, "pointed within 7 Hz, silent"),
        ("cw-2026-08-26-125941", 400.0, "pointed within 4 Hz, silent"),
        ("cw-2026-08-24-012403", 439.81, "reads, the control"),
        ("cw-2026-08-20-014854", 600.0, "holds nothing"),
        ("cw-2026-08-20-014935", 825.0, "holds nothing"),
    };

    /// <summary>
    /// How much of an envelope is the station keying, and how loud is it then?
    /// </summary>
    /// <param name="envelope">One magnitude per hop.</param>
    /// <returns>
    /// The fraction of hops above the midpoint of the envelope's own range, in
    /// decibels, and the ratio of the keyed mean to the unkeyed mean.
    /// </returns>
    /// <remarks>
    /// **CUT AT THE MIDPOINT OF THE ENVELOPE'S OWN DECIBEL RANGE**, which is what
    /// the gate's own detector does and is the same cut every other instrument in
    /// this repository uses on an envelope. It is not a claim that keying was
    /// found; it is a way of asking which hops are the loud ones.
    /// </remarks>
    private static (double Duty, double KeyedOverQuiet) Duty(
        IReadOnlyList<double> envelope)
    {
        var db = new double[envelope.Count];

        for (var i = 0; i < envelope.Count; i++)
        {
            db[i] = 20 * Math.Log10(Math.Max(envelope[i], 1e-12));
        }

        // **THE MIDPOINT OF THE RANGE IS THE WRONG CUT AND THE FIRST RUN OF
        // THIS TEST PROVED IT.** One unusually quiet hop drags the minimum far
        // below the noise floor, the midpoint goes with it, and every capture
        // reports a duty near one — which would have rejected the dilution
        // hypothesis on an artifact of the cut rather than on the audio.
        //
        // **SO THE CUT IS FITTED TO THE TWO HEAPS**, the same shape the rest of
        // this repository uses on an envelope: start at the midpoint, take the
        // mean of each side, cut halfway between those means, repeat. Two
        // iterations settle it on every capture here.
        var cut = db.Min() + ((db.Max() - db.Min()) / 2);

        for (var pass = 0; pass < 8; pass++)
        {
            var hi = 0.0;
            var hiCount = 0;
            var lo = 0.0;
            var loCount = 0;

            foreach (var value in db)
            {
                if (value >= cut)
                {
                    hi += value;
                    hiCount++;
                }
                else
                {
                    lo += value;
                    loCount++;
                }
            }

            if (hiCount == 0 || loCount == 0)
            {
                break;
            }

            var next = ((hi / hiCount) + (lo / loCount)) / 2;

            if (Math.Abs(next - cut) < 1e-9)
            {
                break;
            }

            cut = next;
        }

        var keyed = 0;
        var keyedSum = 0.0;
        var quietSum = 0.0;

        for (var i = 0; i < db.Length; i++)
        {
            if (db[i] >= cut)
            {
                keyed++;
                keyedSum += envelope[i];
            }
            else
            {
                quietSum += envelope[i];
            }
        }

        var quiet = db.Length - keyed;

        return (
            db.Length == 0 ? 0 : (double)keyed / db.Length,
            keyed == 0 || quiet == 0
                ? double.NaN
                : (keyedSum / keyed) / (quietSum / quiet));
    }

    /// <remarks>
    /// <para>**THE MEASUREMENT TASKS 3 AND 4 REST ON.** Three quantities per
    /// recording: the ratio the gate actually sees, the duty the station is
    /// sending at, and what the ratio would be if the silence between
    /// transmissions were not averaged into it.</para>
    /// <para>**THE THIRD IS ESTIMATED FROM THE FIRST TWO AND IS LABELLED AS AN
    /// ESTIMATE.** The window ratio is a sum over hops divided by the hop count,
    /// so dividing by the keyed fraction instead of by the whole gives what the
    /// same evidence would score if it were pooled over the keyed hops alone. It
    /// is arithmetic on a published quantity rather than a second decoder, and it
    /// is exactly the quantity task 3 would have to build properly.</para>
    /// </remarks>
    [Fact]
    public void WhatTheRatioIsMadeOf()
    {
        _output.WriteLine(
            $"  12 s window, gate {CwProbabilisticDecoder.Gate:0.00} per hop");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                | pitch  | ratio | duty  | keyed/quiet | "
            + "ratio/duty | chars | what");
        _output.WriteLine(
            "  -----------------------|--------|-------|-------|-------------|"
            + "------------|-------|-----");

        foreach (var (name, toneHz, what) in Cases)
        {
            var slice = Tail(Capture(name), CwProbabilisticStream.WindowSeconds);

            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, toneHz);

            var read = CwProbabilisticDecoder.DecodeUngated(envelope, toneHz);
            var (duty, lift) = Duty(envelope);

            var pooled = duty > 0 ? read.LikelihoodRatio / duty : double.NaN;

            _output.WriteLine(
                $"  {name,-22} | {toneHz,6:0.0} | {read.LikelihoodRatio,5:0.00} | "
                + $"{duty,5:0.000} | {lift,11:0.00} | {pooled,10:0.00} | "
                + $"{read.Characters.Count,5} | {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  'ratio/duty' is the estimate: what the same evidence scores if "
            + "pooled over");
        _output.WriteLine(
            "  the keyed hops rather than over the whole window. It is arithmetic "
            + "on a");
        _output.WriteLine(
            "  published quantity, not a second decoder.");

        Assert.Equal(5, Cases.Length);
    }

    /// <remarks>
    /// <para>**IS THE FILTER REALLY ON THE STATION?** The whole diagnosis rests
    /// on "pointed within seven hertz and silent anyway", and seven hertz is a
    /// claim about where the station is, taken from what the operator hears.
    /// If the ratio peaks sharply somewhere else, the station is not where the
    /// order says and every conclusion built on that sentence is wrong.</para>
    /// <para>Swept one hertz at a time across sixty, which is the integrator's
    /// own width, so a peak inside the passband would show.</para>
    /// </remarks>
    [Fact]
    public void TheRatioAcrossThePitchesEitherSideOfTheStation()
    {
        foreach (var (name, centreHz, what) in Cases)
        {
            var slice = Tail(Capture(name), CwProbabilisticStream.WindowSeconds);

            var bestHz = 0.0;
            var best = double.NegativeInfinity;
            var atCentre = 0.0;

            for (var hz = centreHz - 30; hz <= centreHz + 30; hz += 1.0)
            {
                var envelope = CwProbabilisticDecoder.Envelope(
                    slice.Samples, slice.SampleRate, hz);

                var ratio = CwProbabilisticDecoder
                    .DecodeUngated(envelope, hz).LikelihoodRatio;

                if (Math.Abs(hz - centreHz) < 0.5)
                {
                    atCentre = ratio;
                }

                if (ratio > best)
                {
                    best = ratio;
                    bestHz = hz;
                }
            }

            _output.WriteLine(
                $"  {name,-22} centre {centreHz,6:0.0} -> {atCentre,5:0.00}   "
                + $"best {bestHz,6:0.0} -> {best,5:0.00}   "
                + $"gain {best - atCentre,5:0.00}   {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  gate {CwProbabilisticDecoder.Gate:0.00}; a station whose best "
            + "pitch still sits under it is not");
        _output.WriteLine("  being refused for being mistuned");

        Assert.Equal(5, Cases.Length);
    }
}
