using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A MARK IS THE ENVELOPE OVER A THRESHOLD, AT ANY PITCH** (work instruction 476 task 1,
/// step 12 criterion 12.1, R90, HM-DEC-185).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"Think you're an oscilloscope. Once we hit a certain
/// amplitude, regardless of frequency, that's probably a character. Everything else is
/// noise."*</para>
/// <para>**NO RECORDING** (R88). Every sample here is made in the test: a tone keyed on and
/// off at a known pitch and known times, over seeded Gaussian noise, so where the marks
/// belong is known exactly and nothing was inferred.</para>
/// <para>**A HOP IS JUDGED ONLY WHERE ITS WINDOW IS WHOLLY INSIDE ONE STATE.** The envelope
/// is read over the last ten or so milliseconds, so the hop or two straddling a keying edge
/// sees both and is not asserted either way.</para>
/// </remarks>
public sealed class AMarkIsTheEnvelopeOverAThresholdTests
{
    private const int Rate = 48_000;
    private const int Chunk = 960;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the per-hop tallies are printed.</param>
    public AMarkIsTheEnvelopeOverAThresholdTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves that a tone keyed at 742 Hz, 28 dB over the noise in a 500 Hz filter, reads as
    /// marks where the key was down and gaps where it was up, and the pitch reads the tone.
    /// </remarks>
    [Fact]
    public void MarksLandWhereTheToneWasOnAndThePitchReadsIt()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(742, 500);

        var result = Drive(detector, 742, toneAmplitude: 0.3, noiseRms: 0.06, seed: 7300);

        Assert.Equal(492, detector.Reading.PassbandLowHz, 3);
        Assert.Equal(992, detector.Reading.PassbandHighHz, 3);
        Assert.True(detector.Reading.PassbandFromRig);

        AssertMarksAndGaps(result);
        AssertPitch(result, 742);
    }

    /// <remarks>
    /// Proves the pitch is not searched from 300 to 900 Hz: a tone at 1250 Hz with the rig's
    /// filter unknown is found over the whole audio band, and its pitch reads 1250.
    /// </remarks>
    [Fact]
    public void ATonePastNineHundredIsFoundWithTheRigUnknown()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(null, null);

        var result = Drive(detector, 1250, toneAmplitude: 0.3, noiseRms: 0.06, seed: 1250);

        Assert.False(detector.Reading.PassbandFromRig);
        Assert.Equal(CwEnvelopeDetector.WholeBandLowHz, detector.Reading.PassbandLowHz, 3);
        Assert.Equal(CwEnvelopeDetector.WholeBandHighHz, detector.Reading.PassbandHighHz, 3);

        AssertMarksAndGaps(result);
        AssertPitch(result, 1250);
    }

    /// <remarks>
    /// Proves a weaker station, its tone 15 dB over the noise in the filter - below the
    /// smallest swing on the owner's rows - is still keyed where it was keyed.
    /// </remarks>
    [Fact]
    public void AToneFifteenDecibelsOverTheNoiseStillKeys()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(600, 500);

        // Noise at 0.06 RMS spread over 24 kHz puts 500/24000 of 0.0036 in the filter,
        // -41.2 dB; a sine of amplitude 0.069 is -26.2 dB, so 15 dB over the noise's mean.
        // The floor rides the noise's troughs, so the envelope stands a little more over it.
        var result = Drive(detector, 600, toneAmplitude: 0.069, noiseRms: 0.06, seed: 600);

        _output.WriteLine($"tone over floor at mid-dah: {result.MidDahOverFloorDb:0.0} dB");
        Assert.InRange(result.MidDahOverFloorDb, 12, 24);

        AssertMarksAndGaps(result);
        AssertPitch(result, 600);
    }

    /// <remarks>
    /// Proves a station ten decibels over the noise in the filter - the weak end of the four
    /// the owner heard on 2026-09-28 at 15:38 - is keyed where it was keyed, its long dahs
    /// unbroken bars, under the wobble a tone at its measured contrast has (work instruction
    /// 479, R93). **Red at unit 479 and left red**: at the 1.5 dB floor it never makes a first
    /// pair, so its contrast is never measured and the formula never widens its tolerance.
    /// </remarks>
    [Fact]
    public void AToneTenDecibelsOverTheNoiseStillKeys()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(600, 500);

        // 0.069 was 15 dB over the noise in the filter; five decibels under it is 0.0388.
        var result = Drive(detector, 600, toneAmplitude: 0.069 * Math.Pow(10, -5 / 20.0), noiseRms: 0.06, seed: 610);

        _output.WriteLine($"tone over floor at mid-dah: {result.MidDahOverFloorDb:0.0} dB");

        AssertMarksAndGaps(result);
        AssertPitch(result, 600);
    }

    /// <remarks>
    /// Proves noise alone raises no mark over four seconds, so the count of marks the row
    /// carries is a count of keying and not of noise.
    /// </remarks>
    [Fact]
    public void NoiseAloneRaisesNoMark()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(700, 500);

        var noise = new Noise(4242, 0.06);
        var samples = new float[Chunk];

        for (var n = 0; n < Rate * 5; n += Chunk)
        {
            for (var i = 0; i < Chunk; i++)
            {
                samples[i] = noise.Next();
            }

            detector.Process(samples);
        }

        var history = detector.History();

        _output.WriteLine($"hops {history.Count}, marked {history.Count(h => h.Mark)}");
        Assert.Equal(0, history.Count(h => h.Mark));
        Assert.Equal(0, detector.Reading.MarksLast4s);
        Assert.False(detector.Reading.Mark);
        Assert.True(double.IsNaN(detector.Reading.PitchHz));
        Assert.Equal("no tone", CwEnvelopeDetector.ToneLine(detector.Reading));
    }

    /// <remarks>
    /// Proves the run reads the dah: at the end of a 180 ms dah the mark has lasted about
    /// 180 ms, and the count of marks in the last four seconds is the number keyed.
    /// </remarks>
    [Fact]
    public void TheRunReadsTheDahAndTheMarksAreCounted()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(742, 500);

        var result = Drive(detector, 742, toneAmplitude: 0.3, noiseRms: 0.06, seed: 180);

        _output.WriteLine($"longest run {result.LongestMarkRunMs:0} ms, marks {detector.Reading.MarksLast4s}");
        Assert.InRange(result.LongestMarkRunMs, 170, 200);
        Assert.Equal(Keying.Count(k => k.On), detector.Reading.MarksLast4s);
    }

    /// <remarks>
    /// Proves the two lines are measured and decide nothing (work instruction 477, R91): the
    /// dashed line is the found bin's gap level, the solid line sits midway between it and the
    /// bar level, every mark stands over both, and no margin constant is left to set them.
    /// </remarks>
    [Fact]
    public void TheLinesAreTheGapLevelAndTheMidpointMeasured()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(742, 500);

        Drive(detector, 742, toneAmplitude: 0.3, noiseRms: 0.06, seed: 9);

        var bin = detector.Bins().Where(b => b.Keying || b.BarsLastSecond > 0).MaxBy(b => b.BarDb)!;
        var marks = detector.History().Where(h => h.Mark).ToList();

        _output.WriteLine($"bin {bin.Hz:0} Hz: bar {bin.BarDb:0.0} dB, gap {bin.GapDb:0.0} dB");
        Assert.Null(typeof(CwEnvelopeDetector).GetField("ThresholdMarginDb"));
        Assert.NotEmpty(marks);
        Assert.All(marks, h => Assert.True(h.EnvelopeDb > h.ThresholdDb && h.ThresholdDb > h.FloorDb));
    }

    /// <summary>C, Q at 20 words a minute: 60 ms dit, 180 ms dah, after 0.8 s of noise.</summary>
    private static readonly (bool On, double Ms)[] Keying =
    {
        (false, 800),
        (true, 180), (false, 60), (true, 60), (false, 60), (true, 180), (false, 60), (true, 60),
        (false, 180),
        (true, 180), (false, 60), (true, 180), (false, 60), (true, 60), (false, 60), (true, 180),
        (false, 500),
    };

    private sealed record DriveResult(
        IReadOnlyList<CwScopeHop> History,
        IReadOnlyList<bool?> Truth,
        IReadOnlyList<CwEnvelopeReading> MarkReadings,
        double LongestMarkRunMs,
        double MidDahOverFloorDb,
        IReadOnlyList<CwBarBin> Bins);

    private static DriveResult Drive(
        CwEnvelopeDetector detector, double toneHz, double toneAmplitude, double noiseRms, int seed)
    {
        // The whole keyed signal, sample by sample, with the key state beside it.
        var on = new List<bool>();

        foreach (var (state, ms) in Keying)
        {
            on.AddRange(Enumerable.Repeat(state, (int)Math.Round(ms * Rate / 1000)));
        }

        var noise = new Noise(seed, noiseRms);
        var audio = new float[on.Count];

        for (var n = 0; n < audio.Length; n++)
        {
            var tone = on[n] ? toneAmplitude * Math.Sin(2 * Math.PI * toneHz * n / Rate) : 0;
            audio[n] = (float)(tone + noise.Next());
        }

        var markReadings = new List<CwEnvelopeReading>();
        var longest = 0.0;

        // The application hands audio over in 960-sample chunks; the detector walks hops
        // inside them, so a reading taken after each chunk is the state at its last hop.
        for (var offset = 0; offset < audio.Length; offset += Chunk)
        {
            detector.Process(audio.AsSpan(offset, Math.Min(Chunk, audio.Length - offset)));

            var reading = detector.Reading;

            if (reading.Mark)
            {
                markReadings.Add(reading);
                longest = Math.Max(longest, reading.RunMs);
            }
        }

        // Also the longest run the history itself holds, read hop by hop.
        var history = detector.History();
        var run = 0;

        foreach (var hop in history)
        {
            run = hop.Mark ? run + 1 : 0;
            longest = Math.Max(longest, run * detector.HopMs);
        }

        // Truth per hop: the envelope window ends at the hop's last sample and reaches back
        // one envelope window; a hop is judged only where that window is wholly one state.
        var hops = audio.Length / detector.HopSamples;
        var first = hops - history.Count;
        var truth = new List<bool?>();

        for (var i = 0; i < history.Count; i++)
        {
            var end = (first + i + 1) * detector.HopSamples;
            var start = Math.Max(0, end - detector.EnvelopeWindowSamples);
            var states = on.GetRange(start, end - start).Distinct().ToList();

            // The detector's first quarter second builds its floor and judges nothing.
            truth.Add(states.Count == 1 && end > Rate * 3 / 10 ? states[0] : null);
        }

        // Mid-way through the first dah, the envelope over the floor.
        var midDah = (int)((800 + 90) * Rate / 1000.0 / detector.HopSamples) - first;
        var over = history[midDah].EnvelopeDb - history[midDah].FloorDb;

        return new DriveResult(history, truth, markReadings, longest, over, detector.Bins());
    }

    private void AssertMarksAndGaps(DriveResult result)
    {
        var onHops = 0;
        var offHops = 0;
        var missed = 0;
        var marked = 0;

        for (var i = 0; i < result.History.Count; i++)
        {
            switch (result.Truth[i])
            {
                case true:
                    onHops++;
                    missed += result.History[i].Mark ? 0 : 1;
                    break;

                case false:
                    offHops++;
                    marked += result.History[i].Mark ? 1 : 0;
                    break;
            }
        }

        _output.WriteLine($"key down hops {onHops}, missed {missed}; key up hops {offHops}, marked {marked}");
        _output.WriteLine("bins " + string.Join(" ", result.Bins.Where(b => b.Bars > 0).Select(b => $"{b.Hz:0}:{b.Bars}/{b.Gaps}/{b.BarDb:0.0}/{b.GapDb:0.0}")));

        // Hop by hop: '#' key down and marked, 'o' key down and missed, '!' key up and marked,
        // '.' key up and not, ' ' straddling an edge and not judged.
        _output.WriteLine("hops: " + string.Concat(result.History.Select((h, i) => result.Truth[i] switch
        {
            true => h.Mark ? '#' : 'o',
            false => h.Mark ? '!' : '.',
            _ => ' ',
        })).TrimStart('.', ' '));

        Assert.True(onHops > 100, "the signal was keyed for too few judged hops to prove anything");
        Assert.True(offHops > 100, "the signal was silent for too few judged hops to prove anything");
        Assert.Equal(0, missed);
        Assert.Equal(0, marked);
    }

    private void AssertPitch(DriveResult result, double toneHz)
    {
        Assert.NotEmpty(result.MarkReadings);

        foreach (var reading in result.MarkReadings)
        {
            Assert.InRange(reading.PitchHz, toneHz - 25, toneHz + 25);
            Assert.True(reading.ContrastDb > 6, $"contrast {reading.ContrastDb:0.0} dB");
        }

        var last = result.MarkReadings[^1];
        _output.WriteLine(CwEnvelopeDetector.ToneLine(last));
        Assert.StartsWith("tone ", CwEnvelopeDetector.ToneLine(last), StringComparison.Ordinal);
        Assert.Contains(" dB over the band", CwEnvelopeDetector.ToneLine(last), StringComparison.Ordinal);
    }

    /// <summary>Seeded Gaussian noise, Box-Muller.</summary>
    private sealed class Noise
    {
        private readonly Random _random;
        private readonly double _rms;

        public Noise(int seed, double rms)
        {
            _random = new Random(seed);
            _rms = rms;
        }

        public float Next()
        {
            var u1 = 1.0 - _random.NextDouble();
            var u2 = _random.NextDouble();

            return (float)(_rms * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }
    }
}
