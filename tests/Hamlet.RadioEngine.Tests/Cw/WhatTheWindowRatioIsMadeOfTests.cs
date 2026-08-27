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

        var low = db.Min();
        var high = db.Max();
        var cut = low + ((high - low) / 2);

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
}
