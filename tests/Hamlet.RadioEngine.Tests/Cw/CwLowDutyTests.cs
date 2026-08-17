using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A station answering a call, which is keyed for a moment in half a minute
/// (HM-DEC-090).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE SHAPE OF THE SIGNAL THE DECODER WAS DEAF TO, AND EVERY
/// SYNTHETIC TEST IN THIS REPOSITORY PASSED WHILE IT WAS.** The fixtures are all
/// a message filling its own recording, keyed maybe a third of the time. A
/// station answering somebody keys for a second and a half in thirty seconds,
/// and nothing here had ever measured that.</para>
/// <para>The captured evidence reported a signal fifty decibels out of the noise
/// as minus nought point six, and put the tone twenty hertz from where it was.
/// Both came from measuring a keyed signal across the time it was not keyed
/// (§0.0).</para>
/// </remarks>
public sealed class CwLowDutyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public CwLowDutyTests(ITestOutputHelper output) => _output = output;

    /// <summary>The pitch the operator's radio was set to.</summary>
    private const double NominalHz = 600;

    /// <summary>Where the station actually was, in the real capture.</summary>
    private const double ActualHz = 627;

    /// <summary>
    /// A short burst of Morse in a long stretch of band noise.
    /// </summary>
    /// <param name="seconds">How long the whole recording runs.</param>
    /// <param name="toneHz">Where the station is.</param>
    /// <param name="amplitude">How loud it is.</param>
    /// <param name="noise">The band noise it sits in.</param>
    /// <returns>The audio.</returns>
    /// <remarks>
    /// Built rather than recorded, because the real captures are on the
    /// operator's machine and this repository has none yet. What it reproduces is
    /// the property that broke everything: a strong narrow tone present for a
    /// small fraction of the recording.
    /// </remarks>
    private static MonoAudio Answering(
        double seconds = 30, double toneHz = ActualHz,
        double amplitude = 0.5, double noise = 0.01)
    {
        // The message itself, keyed fast, the way somebody answering a call
        // sends: short, quick, and over.
        var burst = CwSignal.Generate(new CwSignalRequest(
            "R R DE W1AW", WordsPerMinute: 40, ToneHz: toneHz,
            Amplitude: amplitude, NoiseAmplitude: 0, Seed: 991));

        var rate = burst.SampleRate;
        var samples = new float[(int)(seconds * rate)];

        // Band noise across the whole recording, at the same level the burst
        // sits in, so the ratio is a property of the file rather than of where
        // the burst happens to be.
        var state = 20_261u;

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

            var magnitude = noise * Math.Sqrt(-2 * Math.Log(u1));

            samples[i] = (float)(magnitude * Math.Cos(2 * Math.PI * u2));

            if (i + 1 < samples.Length)
            {
                samples[i + 1] = (float)(magnitude * Math.Sin(2 * Math.PI * u2));
            }
        }

        // The burst, a third of the way in, so there is quiet either side.
        var at = samples.Length / 3;

        for (var i = 0; i < burst.Samples.Length && at + i < samples.Length; i++)
        {
            samples[at + i] = Math.Clamp(samples[at + i] + burst.Samples[i], -1f, 1f);
        }

        return new MonoAudio(rate, samples);
    }

    private static CwDecoder Run(MonoAudio audio, double startAtHz = NominalHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startAtHz);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return decoder;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090, and it is the whole item. **A strong station
    /// keyed for a fraction of the recording must read as a strong station.**</para>
    /// <para>The captured evidence reported 2.2, 2.1, 8.5 and minus nought point
    /// six decibels on signals measured independently at thirty-six to fifty-one
    /// decibels above the band. Every one of those is what an average over
    /// mostly-silence returns.</para>
    /// </remarks>
    [Fact]
    public void AStationKeyedForAMomentReadsAsAStrongStation()
    {
        var report = Run(Answering()).Report;

        _output.WriteLine(
            $"tone {report.ToneHz:0} Hz, snr {report.SnrDb:0.0} dB, "
            + $"hasTone {report.HasTone}");

        Assert.True(
            report.SnrDb >= 20,
            $"a station this far out of the noise reported {report.SnrDb:0.0} dB");

        Assert.True(report.HasTone);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090: **the tone is found where it actually is.** The
    /// real capture had the station 27 Hz off the operator's own pitch, and a
    /// sender's note rarely lands on yours. The tracker used to answer from a
    /// per-bin average, so at low duty it chose whichever bin the noise favored
    /// and was twenty hertz out on a signal fifty decibels up.</para>
    /// <para>Within one bin, which is twenty-five hertz, is as close as this
    /// design can be and is close enough to center a filter on.</para>
    /// </remarks>
    [Fact]
    public void TheToneIsFoundWhereItActuallyIs()
    {
        foreach (var hz in new double[] { 575, 595, 627, 700 })
        {
            var report = Run(Answering(toneHz: hz)).Report;

            _output.WriteLine($"sent {hz:0} Hz, found {report.ToneHz:0} Hz");

            Assert.True(
                Math.Abs(report.ToneHz - hz) <= 25,
                $"sent {hz:0} Hz and the tracker answered {report.ToneHz:0} Hz");
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-090 and §0.0: **an empty band still reads as an empty
    /// band.** The fix must not be bought by calling noise a signal, which is the
    /// failure this whole file exists to prevent in the other direction.</para>
    /// </remarks>
    [Fact]
    public void AnEmptyBandStillReadsAsEmpty()
    {
        var quiet = Answering(amplitude: 0);
        var report = Run(quiet).Report;

        _output.WriteLine($"empty band: snr {report.SnrDb:0.0} dB, tone {report.HasTone}");

        Assert.False(
            report.HasTone,
            $"noise alone reported a tone at {report.SnrDb:0.0} dB above itself");
    }

    /// <remarks>
    /// Proves HM-DEC-090: the held figure falls away when the station stops, so a
    /// signal that has gone is not reported as still there. It holds across the
    /// gaps inside a message and lets go over about ten seconds.
    /// </remarks>
    [Fact]
    public void TheHeldFigureLetsGoWhenTheStationStops()
    {
        var decoder = Run(Answering());
        var afterTheBurst = decoder.Report.SnrDb;

        // Half a minute of nothing but band noise, fed straight in.
        var quiet = Answering(seconds: 30, amplitude: 0);
        using var source = new BufferedAudioSource(quiet);
        decoder.Listen(source);
        source.PumpAll();

        _output.WriteLine(
            $"held {afterTheBurst:0.0} dB, and {decoder.Report.SnrDb:0.0} dB "
            + "half a minute later");

        Assert.True(decoder.Report.SnrDb < afterTheBurst - 5);
    }
}
