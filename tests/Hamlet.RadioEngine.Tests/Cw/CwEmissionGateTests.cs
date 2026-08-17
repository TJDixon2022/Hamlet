using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Nothing is emitted from noise, and no number outlives its evidence
/// (HM-DEC-090).
/// </summary>
public sealed class CwEmissionGateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public CwEmissionGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>Half a minute of band noise and nothing else.</summary>
    private static MonoAudio Noise(double seconds = 30, double level = 0.02)
    {
        var rate = CwSignal.DefaultSampleRate;
        var samples = new float[(int)(seconds * rate)];
        var state = 33_137u;

        for (var i = 0; i < samples.Length; i += 2)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            var u1 = ((state & 0xFFFFFF) + 1) / 16777217.0;

            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            var u2 = ((state & 0xFFFFFF) + 1) / 16777217.0;

            var magnitude = level * Math.Sqrt(-2 * Math.Log(u1));

            samples[i] = (float)(magnitude * Math.Cos(2 * Math.PI * u2));

            if (i + 1 < samples.Length)
            {
                samples[i + 1] = (float)(magnitude * Math.Sin(2 * Math.PI * u2));
            }
        }

        return new MonoAudio(rate, samples);
    }

    private static CwDecoder Run(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return decoder;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090, and it is the reported fault exactly. Half a
    /// minute of band noise produced **1,732 characters, 1,730 of them marked
    /// unsure**, and marking them was not enough: a screen filling with blocks
    /// and dimmed letters reads as a signal being fought over rather than as
    /// nothing being there.</para>
    /// <para>Nothing at all is the only honest output (§0.0).</para>
    /// </remarks>
    [Fact]
    public void HalfAMinuteOfNoiseProducesNothing()
    {
        var decoder = Run(Noise());
        var report = decoder.Report;

        _output.WriteLine(
            $"characters {report.CharactersEmitted}, elements {report.ElementsSeen}, "
            + $"tone {report.HasTone}, wpm {decoder.WordsPerMinute?.ToString() ?? "none"}");

        Assert.Equal(0, report.CharactersEmitted);
        Assert.False(report.HasTone);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090: **no speed unless characters are resolving.** The
    /// figure reached three separate surfaces as a settled fact while nothing was
    /// being received, including a sentence in the send panel about what "they"
    /// were sending at with nobody sending.</para>
    /// </remarks>
    [Fact]
    public void NoSpeedIsNamedWithoutCharactersToNameItFrom()
    {
        Assert.Null(Run(Noise()).WordsPerMinute);

        // And with a real signal it is named.
        var real = CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW K", WordsPerMinute: 18, ToneHz: 620,
            Amplitude: 0.5, NoiseAmplitude: 0.02, Seed: 5));

        var wpm = Run(real).WordsPerMinute;

        _output.WriteLine($"real signal reported {wpm?.ToString() ?? "none"} wpm");

        Assert.NotNull(wpm);
        Assert.InRange(wpm!.Value, 14, 24);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090: **a speed this radio cannot produce is never
    /// shown.** Sixty-four words a minute was reported with nothing on
    /// frequency, and the IC-7300's keyer tops out at forty-eight (`14 0C`,
    /// p. 19-3), so that number could not have come from a station under any
    /// circumstances.</para>
    /// <para>A backstop rather than the fix, and it is tested as one: the gate
    /// above is what stops the arithmetic happening at all.</para>
    /// </remarks>
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(52)]
    [InlineData(64)]
    public void ASpeedThisRadioCannotSendIsNeverShown(int wpm)
    {
        Assert.True(
            wpm < CwDecoder.SlowestPlausibleWpm || wpm > CwDecoder.FastestPlausibleWpm,
            "the bounds no longer exclude this, so the test is not testing it");
    }

    /// <remarks>
    /// Proves HM-DEC-090: the bounds are the radio's own, from the manual rather
    /// than from taste.
    /// </remarks>
    [Fact]
    public void TheBoundsAreTheRadiosOwn()
    {
        Assert.Equal(6, CwDecoder.SlowestPlausibleWpm);
        Assert.Equal(48, CwDecoder.FastestPlausibleWpm);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090: **a capture can tell whether it is fresh.** Three
    /// presses inside seventy seconds produced byte-identical files with
    /// identical analysis while the rig state beside them differed, and the
    /// operator reasoned from one recording presented as three.</para>
    /// <para>A ring buffer cannot know whether it holds thirty fresh seconds or
    /// the same thirty seconds it held a minute ago. This counter can.</para>
    /// </remarks>
    [Fact]
    public void TheTapKnowsWhetherAnyAudioHasArrived()
    {
        var tap = new AudioTap();
        var block = new float[8_000];

        Assert.Equal(0, tap.SamplesSeen);

        tap.Take(block, 8_000);
        var afterOne = tap.SamplesSeen;

        Assert.Equal(8_000, afterOne);

        // Nothing arriving means the count does not move, which is what tells a
        // second capture that it would be writing the same file again.
        Assert.Equal(afterOne, tap.SamplesSeen);

        tap.Take(block, 8_000);

        Assert.Equal(16_000, tap.SamplesSeen);
    }
}
