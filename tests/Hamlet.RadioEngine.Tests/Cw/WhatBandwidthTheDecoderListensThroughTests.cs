using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How wide the CW detection filter actually is, which instrument is which, and
/// what holding it narrow does.
/// </summary>
/// <remarks>
/// <para>**THERE ARE TWO ENVELOPES IN THIS REPOSITORY AND THEY ARE NOT THE SAME
/// WIDTH.** The decoder listens through a Hann-tapered Goertzel over its analysis
/// window: fifty milliseconds is thirty hertz, forty is thirty-eight, twenty is
/// seventy-five. `KeyingEnvelope`, which is the keying meter and the independent
/// witness, is a ten millisecond boxcar over the quadrature arms, which is a
/// hundred hertz. **A figure measured with one is not a figure about the
/// other**, and this project has already written a work order that way.</para>
/// <para>**AND THE DECODER'S WIDTH IS CHOSEN BY THE SPEED IT BELIEVES**, so a fit
/// dragged short by chatter reads as a fast fist and opens the filter. Eight of
/// the nine real recordings here spend most of their time in the twenty
/// millisecond window, fitted at twenty-two to fifty-six words a minute, on
/// senders working near fourteen.</para>
/// <para>**HOLDING THE WINDOW LONG IN TIME IS WORTH A GREAT DEAL AND CANNOT BE
/// HAD BY CHANGING A CONSTANT.** Held at fifty milliseconds this decoder reads
/// considerably more off the two recordings with the most content in them, and
/// the two holding no keying stay silent. The same width applied by changing the
/// constants breaks HM-DEC-120, because the survey that finds a station in the
/// first place shares that window. **Nothing in `src` was changed on the strength
/// of this**; the measurement is here so the next session starts from it rather
/// than from a formula.</para>
/// </remarks>
public sealed class WhatBandwidthTheDecoderListensThroughTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the widths and the readings are printed.</param>
    public WhatBandwidthTheDecoderListensThroughTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;
    private const double Centre = 600;

    /// <summary>Gaussian noise with a flat spectral density.</summary>
    private static MonoAudio Noise(int seed, double seconds)
    {
        var random = new Random(seed);
        var samples = new float[(int)(Rate * seconds)];

        for (var i = 0; i < samples.Length; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            samples[i] = (float)(0.05 * Math.Sqrt(-2 * Math.Log(u1))
                * Math.Cos(2 * Math.PI * u2));
        }

        return new MonoAudio(Rate, samples);
    }

    /// <summary>
    /// The mean noise power the tracker measures at its own bin, at one window.
    /// </summary>
    /// <remarks>
    /// Noise power through a filter is proportional to that filter's noise
    /// bandwidth, so this is the bandwidth measured rather than computed from a
    /// window length.
    /// </remarks>
    private static double MeanNoiseDb(int wordsPerMinute)
    {
        var tracker = new CwToneTracker(Rate, Centre);
        var audio = Noise(7300, 4);
        var hop = tracker.HopSamples;
        var sum = 0.0;
        var count = 0;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            tracker.FollowSpeed(wordsPerMinute);
            tracker.Process(
                audio.Samples.AsSpan((int)at, hop),
                at,
                reading =>
                {
                    if (double.IsNaN(reading.PowerDb) || double.IsInfinity(reading.PowerDb))
                    {
                        return;
                    }

                    sum += reading.PowerDb;
                    count++;
                });
        }

        return count == 0 ? double.NaN : sum / count;
    }

    /// <remarks>
    /// <para>Proves the two instruments are different widths, so a figure taken
    /// with one is never quoted about the other (HM-DEC-091).</para>
    /// </remarks>
    [Fact]
    public void TheDecoderIsNarrowerThanTheKeyingMeter()
    {
        var slow = MeanNoiseDb(12);
        var acquiring = MeanNoiseDb(0);
        var fast = MeanNoiseDb(30);

        _output.WriteLine($"slow fist, 50 ms window      {slow,7:0.00} dB of noise");
        _output.WriteLine($"acquiring, 40 ms window      {acquiring,7:0.00} dB");
        _output.WriteLine($"fast fist, 20 ms window      {fast,7:0.00} dB");
        _output.WriteLine("");
        _output.WriteLine($"the fast window admits {fast - slow:0.00} dB more noise");
        _output.WriteLine(
            $"KeyingEnvelope smooths at {KeyingEnvelope.SmoothingHz:0} Hz, "
            + $"which is a {1000 / KeyingEnvelope.SmoothingHz:0} ms boxcar");

        // A longer window, less noise, every time. That is the whole of what
        // detection bandwidth means here.
        Assert.True(slow < acquiring, "the 50 ms window admitted more noise than the 40");
        Assert.True(acquiring < fast, "the 40 ms window admitted more noise than the 20");

        // And the meter is wider than any of them: a hundred hertz against
        // thirty to seventy-five.
        Assert.True(KeyingEnvelope.SmoothingHz >= 100);
    }

    private static IReadOnlyList<string> RealRecordings()
    {
        var folder = CapturedSignalTests.Folder;

        return Directory.GetFiles(folder, "*.wav")
            .Concat(Directory.GetFiles(Path.Combine(folder, "unadjudicated"), "*.wav"))
            .OrderBy(p => p)
            .ToList();
    }

    /// <remarks>
    /// <para>Proves the loop: **the filter widens on the strength of a speed the
    /// filter's own width helped get wrong.** Every recording but one is fitted
    /// well above the eighteen words a minute at which the window shortens.</para>
    /// </remarks>
    [Fact]
    public void MostRealRecordingsSitInTheWidestWindow()
    {
        var wide = 0;
        var total = 0;

        foreach (var path in RealRecordings())
        {
            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, Centre);
            var hop = decoder.Tracker.HopSamples;
            var hops = new Dictionary<int, int>();

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

                var window = decoder.Tracker.WindowSamples;

                hops[window] = hops.TryGetValue(window, out var n) ? n + 1 : 1;
            }

            decoder.Flush();

            var count = hops.Values.Sum();
            var widest = hops
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .First();

            var widestMs = widest * 1000.0 / audio.SampleRate;

            _output.WriteLine(
                $"{Path.GetFileNameWithoutExtension(path),-28} "
                + $"fitted {decoder.Timing.WordsPerMinute,3} wpm, mostly "
                + $"{widestMs:0} ms ({1500 / widestMs:0} Hz), "
                + $"{hops[widest] * 100.0 / count:0}% of the time");

            total++;

            if (widestMs <= 25)
            {
                wide++;
            }
        }

        _output.WriteLine("");
        _output.WriteLine($"{wide} of {total} spend most of their time at 75 Hz");

        Assert.True(
            wide >= total - 2,
            "the recordings have stopped landing in the widest window, which "
            + "would mean the fitted speeds have come down and this finding no "
            + "longer holds");
    }

    /// <summary>What the decoder reads with its window held, rather than followed.</summary>
    /// <param name="path">The recording.</param>
    /// <param name="heldWpm">
    /// The speed the tracker is told after each hop, which fixes the window for
    /// the next measurement. Null leaves the decoder's own choice alone.
    /// </param>
    private static (int Characters, string Text) Read(string path, int? heldWpm)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, path));
        var decoder = new CwDecoder(audio.SampleRate, Centre);
        var hop = decoder.Tracker.HopSamples;
        var text = new List<string>();

        decoder.CharacterDecoded += c => text.Add(c.Text);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            // The decoder sets the window from its own fit at the end of every
            // hop. This overrides it, so the next measurement is made through the
            // held window and nothing else about the decoder changes.
            if (heldWpm is { } wpm)
            {
                decoder.Tracker.FollowSpeed(wpm);
            }
        }

        decoder.Flush();

        return (decoder.Report.CharactersEmitted, string.Concat(text));
    }

    /// <remarks>
    /// <para>Proves what a held window is worth on the two real recordings with
    /// the most content in them: **considerably more, and legible where it was
    /// fragments.**</para>
    /// <para>Nothing is asserted about what was said. Neither recording has an
    /// adjudicated answer key and a session may not write one (§12.5), so the
    /// text is printed and the count is what is checked.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("cw-2026-08-18-004507.wav")]
    [InlineData("unadjudicated/cw-2026-08-18-003016.wav")]
    public void HoldingTheWindowLongInTimeReadsMore(string name)
    {
        var followed = Read(name, null);
        var held = Read(name, 12);

        _output.WriteLine(name);
        _output.WriteLine($"  window followed:   {followed.Characters,3}  '{followed.Text}'");
        _output.WriteLine($"  window held 50 ms: {held.Characters,3}  '{held.Text}'");

        Assert.True(
            held.Characters > followed.Characters,
            $"holding the window read {held.Characters} against {followed.Characters}");
    }

    /// <remarks>
    /// <para>Proves the half that matters: **holding the window long does not make
    /// a recording with no keying in it speak** (HM-DEC-090, HM-DEC-120). Held at
    /// forty milliseconds instead of fifty it does, four characters out of
    /// `cw-2026-08-20-014854`, which is why the figure here is fifty and one
    /// reason none of this was shipped.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854.wav")]
    [InlineData("unadjudicated/cw-2026-08-20-014935.wav")]
    public void HoldingItLongStillSaysNothingAboutAnEmptyBand(string name)
    {
        var held = Read(name, 12);

        _output.WriteLine($"{name}: {held.Characters} characters '{held.Text}'");

        Assert.Equal(0, held.Characters);
    }
}
