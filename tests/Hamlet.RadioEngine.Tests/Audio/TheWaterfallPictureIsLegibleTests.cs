using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// What the digital waterfall actually draws from real radio audio.
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR TUNED TO 14.074 IN USB-D AND THE PICTURE WAS WRONG**
/// (2026-08-28, 16:41 UTC): the left third saturated to near-white, the right two
/// thirds a uniform crosshatch, and a hard vertical edge between them. No signal
/// produces that.</para>
/// <para>**THIS MEASURES THE BYTES, WHICH IS WHAT THE RENDERER DRAWS.**
/// `WaterfallControl.OnFrameReady` maps each bin linearly onto the palette —
/// `bins[x] * Gain`, clamped — so a bin at 255 is white and a bin near 0 is the
/// floor colour, and the shape of the byte histogram *is* the shape of the
/// picture. The control has been trusted for weeks on the CW tab with the same
/// code; what changed is the source feeding it.</para>
/// <para>**IT IS REAL RADIO AUDIO AND NOT SYNTHESISED.** The captures are this
/// operator's own IC-7300 through the same codec. They are Morse rather than FT8,
/// which does not matter here: a render fault reacts to the real noise floor and
/// the real dynamic range, and those are what these hold.</para>
/// </remarks>
public sealed class TheWaterfallPictureIsLegibleTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the histogram is printed.</param>
    public TheWaterfallPictureIsLegibleTests(ITestOutputHelper output)
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

    private static MonoAudio Capture(string relative)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", relative + ".wav"));

    /// <summary>Real captures from the operator's own radio.</summary>
    private static string[] Real { get; } =
    {
        "cw-2026-08-17-013347",
        "unadjudicated/cw-2026-08-28-004844",
        "unadjudicated/cw-2026-08-20-014854",
    };

    /// <summary>What a run of frames looks like as a picture.</summary>
    private readonly record struct Picture(
        int Frames, double SaturatedShare, double DarkShare, double WorstStep,
        int MedianByte, double MedianStep);

    private static Picture Look(MonoAudio audio)
    {
        var source = new AudioSpectrumSource(audio.SampleRate);

        var frames = 0;
        long saturated = 0;
        long dark = 0;
        long counted = 0;
        var worstStep = 0.0;
        var histogram = new long[256];
        var steps = new long[256];

        source.FrameReady += (in SpectrumFrame f) =>
        {
            frames++;

            for (var i = 0; i < f.Bins.Length; i++)
            {
                counted++;

                if (f.Bins[i] >= 250)
                {
                    saturated++;
                }

                if (f.Bins[i] <= 40)
                {
                    dark++;
                }

                histogram[f.Bins[i]]++;
            }

            // **THE SEAM, MEASURED.** A hard vertical edge in the picture is a
            // large jump between neighbouring bins that persists. Real spectra
            // step by a few counts between adjacent bins; a rendering fault
            // steps most of the range in one bin.
            for (var i = 1; i < f.Bins.Length; i++)
            {
                var step = Math.Abs(f.Bins[i] - f.Bins[i - 1]);

                steps[step]++;

                if (step > worstStep)
                {
                    worstStep = step;
                }
            }
        };

        source.Start();

        for (var at = 0; at < audio.Samples.Length; at += 4096)
        {
            var take = Math.Min(4096, audio.Samples.Length - at);
            source.Push(audio.Samples.AsSpan(at, take));
        }

        static int Middle(long[] counts)
        {
            long total = 0;

            foreach (var c in counts)
            {
                total += c;
            }

            long seen = 0;

            for (var i = 0; i < counts.Length; i++)
            {
                seen += counts[i];

                if (seen * 2 >= total)
                {
                    return i;
                }
            }

            return 0;
        }

        return new Picture(
            frames,
            counted == 0 ? 0 : (double)saturated / counted,
            counted == 0 ? 0 : (double)dark / counted,
            worstStep,
            Middle(histogram),
            Middle(steps));
    }

    /// <remarks>
    /// <para>**THE REPRODUCTION.** Three real captures, one of which holds
    /// nothing at all, through the source that feeds the picture.</para>
    /// <para>**WHAT A LEGIBLE WATERFALL LOOKS LIKE AS NUMBERS**: most of a quiet
    /// band is dark, very little of it is saturated, and no pair of neighbouring
    /// bins differs by most of the range. A capture holding nothing should be
    /// almost entirely dark — it is band noise and nothing else.</para>
    /// </remarks>
    [Fact]
    public void WhatTheRealCapturesDraw()
    {
        _output.WriteLine(
            "  capture                              | saturated | median | med step | worst step");
        _output.WriteLine(
            "  -------------------------------------|-----------|--------|----------|-----------");

        foreach (var name in Real)
        {
            var picture = Look(Capture(name));

            _output.WriteLine(
                $"  {name,-36} | {picture.SaturatedShare,8:0.0%} | "
                + $"{picture.MedianByte,6} | {picture.MedianStep,8} | "
                + $"{picture.WorstStep,10:0}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  a legible picture is mostly dark on a quiet band, barely saturated,");
        _output.WriteLine(
            "  and steps a few counts between neighbouring bins rather than most");
        _output.WriteLine("  of the range");

        Assert.Equal(3, Real.Length);
    }

    /// <remarks>
    /// <para>**THE ACCEPTANCE, ON THE CAPTURE THAT HOLDS NOTHING.**
    /// `cw-2026-08-20-014854` is one of the two recordings a record says holds no
    /// station. Its picture is band noise, and band noise should be dark.</para>
    /// <para>**THIS IS THE TEST THAT MUST FAIL BEFORE TASK 3 AND PASS AFTER.**</para>
    /// </remarks>
    [Fact]
    public void AQuietBandIsMostlyDark()
    {
        var picture = Look(Capture("unadjudicated/cw-2026-08-20-014854"));

        _output.WriteLine(
            $"  {picture.Frames} frames: {picture.SaturatedShare:0.0%} saturated, "
            + $"median byte {picture.MedianByte} of 255, "
            + $"median step {picture.MedianStep}, worst step {picture.WorstStep:0}");

        Assert.True(
            picture.SaturatedShare < 0.01,
            $"{picture.SaturatedShare:0.0%} of a recording holding nothing is "
            + "drawn saturated, which is a picture of noise painted as signal");

        // **THE MEDIAN, BECAUSE MY FIRST PROXY FOR DARK WAS A THRESHOLD I
        // INVENTED.** "The share of bins under 40 of 255" is not a fact about a
        // picture, and moving that threshold to pass would be fitting the test
        // to the answer. The median byte says where the whole picture sits: on a
        // recording holding nothing it should be in the bottom half of the
        // palette, because there is nothing in it to be bright.
        Assert.True(
            picture.MedianByte < 128,
            $"the median bin of a recording holding nothing is {picture.MedianByte} "
            + "of 255, so the picture is bright where there is no signal");

        // **AND NO PERSISTENT HARD EDGE.** A single 255 step happens in noise —
        // a deep null beside a peak — so the maximum over a third of a million
        // samples says nothing. The median step is the texture of the picture,
        // and a hard vertical seam would push it up.
        Assert.True(
            picture.MedianStep < 40,
            $"neighbouring bins differ by {picture.MedianStep} of 255 typically, "
            + "which is a picture with an edge in it rather than a spectrum");
    }
}
