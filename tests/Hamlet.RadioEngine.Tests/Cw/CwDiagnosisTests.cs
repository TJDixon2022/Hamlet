using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The decoder says what it can see, and captures what it heard (HM-DEC-088).
/// </summary>
public sealed class CwDiagnosisTests
{
    private static MonoAudio Signal(double amplitude, double noise)
        => CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW K", WordsPerMinute: 18, ToneHz: 620,
            Amplitude: amplitude, NoiseAmplitude: noise, Seed: 4242));

    /// <remarks>
    /// <para>Proves HM-DEC-088, and it is the point of the whole item: **a strong
    /// signal that will not resolve and an empty band used to produce the same
    /// screen.** They are completely different problems and the operator could
    /// not tell them apart.</para>
    /// <para>What is asserted is that the two produce different sentences, and
    /// that the one about a tone names a pitch, because a diagnosis without a
    /// number in it is a shrug.</para>
    /// </remarks>
    [Fact]
    public void AToneThatWillNotResolveReadsDifferentlyFromAnEmptyBand()
    {
        // Nothing but noise, at a level a receiver really produces.
        var quiet = new CwDecodeReport(
            new AudioLevel(-24, -30, -31, false, 30),
            620, 0.4, HasTone: false, 0, 0, 0, 0);

        // Something is plainly there and nothing is coming out.
        var stuck = new CwDecodeReport(
            new AudioLevel(-14, -22, -34, false, 30),
            620, 8.0, HasTone: true, 60, 0, 0, 0);

        var empty = CwDecodeStory.Describe(quiet, listening: true);
        var tone = CwDecodeStory.Describe(stuck, listening: true);

        Assert.NotEqual(empty, tone);
        Assert.Contains("620 hertz", tone, StringComparison.Ordinal);
        Assert.Contains("not resolving", tone, StringComparison.Ordinal);
        Assert.DoesNotContain("hertz", empty, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-088 and §0.0: **everything said here is a measurement
    /// of the audio and never a claim about a station.** This is the trap the
    /// speed estimate fell into once, where a number derived from noise reached
    /// the screen as a fact about an operator.</para>
    /// <para>So no words a decoder could only know by inferring something about
    /// a person appear in any of these passages, at any level, including the ones
    /// describing a signal it cannot read.</para>
    /// </remarks>
    [Fact]
    public void NothingItSaysIsAClaimAboutAnybody()
    {
        string[] forbidden =
        {
            "words a minute", "wpm", "callsign", "operator", "station is",
            "sending badly", "your antenna", "your radio is", "poor operator",
        };

        var reports = new[]
        {
            CwDecodeReport.None,
            new CwDecodeReport(
                new AudioLevel(-70, -75, -78, false, 30), 0, double.NaN, false, 0, 0, 0, 0),
            new CwDecodeReport(
                new AudioLevel(-1, -6, -40, true, 30), 700, 20, true, 100, 0, 0, 0),
            new CwDecodeReport(
                new AudioLevel(-14, -22, -34, false, 30), 620, 8, true, 60, 0, 0, 0),
            new CwDecodeReport(
                new AudioLevel(-14, -22, -34, false, 30), 620, 14, true, 60, 55, 9, 1),
        };

        foreach (var report in reports)
        {
            var said = CwDecodeStory.Describe(report, listening: true)
                + " " + CwDecodeStory.Summarize(report, listening: true);

            foreach (var word in forbidden)
            {
                Assert.DoesNotContain(word, said, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <remarks>
    /// Proves HM-DEC-088: near-silence at the input is named as the separate
    /// audio path it is, because that is the difference between a decoder that
    /// needs fixing and a level nobody knew existed. Clipping is named too, since
    /// it is the opposite failure and equally fatal.
    /// </remarks>
    [Fact]
    public void TheTwoAudioPathFailuresAreBothNamed()
    {
        var silent = CwDecodeStory.Describe(
            new CwDecodeReport(
                new AudioLevel(-80, -85, -88, false, 30), 0, double.NaN, false, 0, 0, 0, 0),
            listening: true);

        var loud = CwDecodeStory.Describe(
            new CwDecodeReport(
                new AudioLevel(-0.1, -5, -40, true, 30), 700, 25, true, 90, 0, 0, 0),
            listening: true);

        Assert.Contains("two separate", silent, StringComparison.Ordinal);
        Assert.Contains("flattened", loud, StringComparison.Ordinal);
        Assert.NotEqual(silent, loud);
    }

    /// <remarks>
    /// Proves HM-DEC-088: nothing is said at all when characters are arriving.
    /// A diagnosis printed beside a working decode is noise, and noise beside a
    /// working thing is what teaches somebody to stop reading the notices.
    /// </remarks>
    [Fact]
    public void ItSaysNothingWhileItIsWorking()
    {
        var working = new CwDecodeReport(
            new AudioLevel(-14, -22, -34, false, 30), 620, 14, true, 60, 55, 9, 1);

        Assert.Equal("", CwDecodeStory.Describe(working, listening: true));
        Assert.Equal("", CwDecodeStory.Summarize(working, listening: true));

        // And nothing at all when the decoder is not even running.
        Assert.Equal("", CwDecodeStory.Describe(working, listening: false));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-088: **the tap holds exactly what the decoder was
    /// fed.** Without a recording of a signal the operator could hear and the
    /// decoder could not, the next three sessions would argue about audio nobody
    /// can look at (§0.0.1).</para>
    /// <para>The round trip through a real WAV file is part of it, because a
    /// capture that cannot be read back is not evidence.</para>
    /// </remarks>
    [Fact]
    public void ACaptureHoldsWhatTheDecoderHeardAndSurvivesAWavFile()
    {
        var audio = Signal(0.5, 0.05);
        var decoder = new CwDecoder(audio.SampleRate, 620);

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
        }

        var captured = decoder.Tap.Snapshot();

        Assert.NotNull(captured);
        Assert.Equal(audio.SampleRate, captured!.SampleRate);
        Assert.Equal(audio.Samples.Length, captured.Samples.Length);

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            Assert.Equal(audio.Samples[i], captured.Samples[i]);
        }

        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".wav");

        try
        {
            WavAudio.Write(path, captured);
            var back = WavAudio.Read(path);

            Assert.Equal(captured.SampleRate, back.SampleRate);
            Assert.Equal(captured.Samples.Length, back.Samples.Length);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-088: the tap keeps only the last half minute, so a session
    /// left running all evening holds a bounded amount of audio rather than the
    /// evening.
    /// </remarks>
    [Fact]
    public void TheTapKeepsTheLastHalfMinuteAndNoMore()
    {
        var tap = new AudioTap();
        var block = new float[8_000];

        for (var second = 0; second < 60; second++)
        {
            Array.Fill(block, second / 100f);
            tap.Take(block, 8_000);
        }

        var held = tap.Snapshot();

        Assert.NotNull(held);
        Assert.Equal(8_000 * AudioTap.SecondsKept, held!.Samples.Length);

        // And what it kept is the end, not the beginning.
        Assert.Equal(0.59f, held.Samples[^1], precision: 3);
    }

    /// <remarks>
    /// Proves HM-DEC-088: the level meter tells a signal from silence and names
    /// clipping, which is the whole of what makes "the decoder is being handed
    /// near-silence" a diagnosis rather than a guess.
    /// </remarks>
    [Fact]
    public void TheLevelMeterSeparatesSilenceFromSignalFromOverload()
    {
        var quiet = new AudioTap();
        var loud = new AudioTap();
        var over = new AudioTap();

        var samples = new float[8_000];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 600 * i / 8000.0);
        }

        var tiny = samples.Select(v => v * 0.0002f).ToArray();
        var fine = samples.Select(v => v * 0.3f).ToArray();
        var flat = samples.Select(v => Math.Clamp(v * 4f, -1f, 1f)).ToArray();

        quiet.Take(tiny, 8_000);
        loud.Take(fine, 8_000);
        over.Take(flat, 8_000);

        Assert.True(quiet.Level.NearlySilent);
        Assert.False(loud.Level.NearlySilent);
        Assert.False(loud.Level.Clipping);
        Assert.True(over.Level.Clipping);
        Assert.True(loud.Level.PeakDb > quiet.Level.PeakDb + 40);
    }

    /// <remarks>
    /// Proves HM-DEC-088: what Windows is doing is reported where it was read and
    /// stays silent where it was not. A capture level nobody read, described as a
    /// level, would send the operator to adjust something that was never the
    /// problem.
    /// </remarks>
    [Fact]
    public void TheWindowsSideIsReportedOnlyWhereItWasRead()
    {
        Assert.Equal("", CaptureAdvice.Describe(CaptureHealth.Unknown));
        Assert.False(CaptureHealth.Unknown.IsAProblem);

        var low = new CaptureHealth("USB Audio CODEC", 0.2, false);
        var muted = new CaptureHealth("USB Audio CODEC", 0.8, true);
        var fineOne = new CaptureHealth("USB Audio CODEC", 0.9, false);

        Assert.Contains("20 percent", CaptureAdvice.Describe(low), StringComparison.Ordinal);
        Assert.Contains("muted", CaptureAdvice.Describe(muted), StringComparison.Ordinal);
        Assert.Equal("", CaptureAdvice.Describe(fineOne));

        // The enhancements are named and never diagnosed, because Hamlet cannot
        // read them (§0.0).
        Assert.Contains(
            "cannot read", CaptureAdvice.EnhancementsNote, StringComparison.Ordinal);
    }
}
