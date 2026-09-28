using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **BARS, NOT WAVES** (work instruction 477 task 1, step 12 criterion 12.1 rewritten, R91,
/// HM-DEC-186).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"We can tune out garbage by seeing if the signal stays at an
/// amplitude for a period. Real signals will be bars, not waves."*</para>
/// <para>**NO RECORDING** (R88). A keyed tone at a known pitch and speed over noise whose level
/// wanders; a steady carrier; loud noise alone. Every sample is made here.</para>
/// </remarks>
public sealed class ABarIsALevelThatHoldsTests
{
    private const int Rate = 48_000;
    private const int Chunk = 960;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tallies are printed.</param>
    public ABarIsALevelThatHoldsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves a tone keyed at 650 Hz, 20 words a minute, over noise whose level wanders 12 dB
    /// top to bottom twice a second, makes bars on the key-down hops and gaps on the key-up
    /// hops, reads as keying, and reads its pitch.
    /// </remarks>
    [Fact]
    public void AKeyedToneOverWanderingNoiseMakesBarsOnTheMarks()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(700, 500);

        var on = KeyedCq();
        var audio = Make(on, n => on[n] ? 0.3 * Math.Sin(2 * Math.PI * 650 * n / Rate) : 0, Wandering(0.03, 650));
        var everKeying = Feed(detector, audio, out var readings);

        var history = detector.History();
        var first = (audio.Length / detector.HopSamples) - history.Count;
        var (onHops, missed, offHops, marked) = Tally(history, on, first, detector);

        var pitch = readings.Where(r => r.Mark).Select(r => r.PitchHz).ToList();
        var keyedBin = detector.Bins().Single(b => b.Hz == detector.Reading.PitchHz || b.Hz == pitch.LastOrDefault());

        _output.WriteLine($"keyed: key down hops {onHops}, missed {missed}; key up hops {offHops}, marked {marked}");
        _output.WriteLine($"keyed: bars in the found bin in the last second {keyedBin.BarsLastSecond}, marks in 4 s {detector.Reading.MarksLast4s}, keying seen {everKeying}");
        _output.WriteLine($"keyed: pitch {pitch.Min():0} to {pitch.Max():0} Hz, bar {keyedBin.BarDb:0.0} dB, gap {keyedBin.GapDb:0.0} dB, contrast {readings.Last(r => r.Mark).ContrastDb:0.0} dB");
        _output.WriteLine("keyed: bins " + string.Join(" ", detector.Bins().Select(b => $"{b.Hz:0}:{b.Bars}/{b.Gaps}/{b.BarDb:0.0}")));

        Assert.True(onHops > 100 && offHops > 100, "too few judged hops to prove anything");
        Assert.Equal(0, missed);
        Assert.Equal(0, marked);
        Assert.True(everKeying);
        Assert.NotEmpty(pitch);
        Assert.All(pitch, p => Assert.InRange(p, 625, 675));
        Assert.Equal(Keying.Count(k => k.On), detector.Reading.MarksLast4s);
        Assert.True(readings.Last(r => r.Mark).ContrastDb > 6);
    }

    /// <remarks>
    /// Proves a steady carrier is one bar and no gap: it holds a level and never drops, so
    /// it is not keying, and the scope shows no mark for it.
    /// </remarks>
    [Fact]
    public void ASteadyCarrierIsOneBarWithNoGapAndNotKeying()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(700, 500);

        var on = Enumerable.Range(0, Rate * 4).Select(n => n > Rate * 8 / 10).ToArray();
        var audio = Make(on, n => on[n] ? 0.3 * Math.Sin(2 * Math.PI * 700 * n / Rate) : 0, Wandering(0.03, 700));
        var everKeying = Feed(detector, audio, out _);

        var carrier = detector.Bins().Single(b => b.Hz == 700);

        _output.WriteLine($"carrier: bars {carrier.Bars}, gaps {carrier.Gaps}, keying {carrier.Keying}, keying seen {everKeying}, marked hops {detector.History().Count(h => h.Mark)}");

        Assert.Equal(1, carrier.Bars);
        Assert.Equal(0, carrier.Gaps);
        Assert.False(carrier.Keying);
        Assert.False(everKeying);
        Assert.DoesNotContain(detector.History(), h => h.Mark);
        Assert.Equal(0, detector.Reading.MarksLast4s);
    }

    /// <remarks>
    /// Proves loud noise alone, over the whole audio band with the rig unknown - 117 bins -
    /// never reads as keying in any bin across thirty seconds: noise does not hold a level.
    /// </remarks>
    [Fact]
    public void LoudNoiseAloneIsNeverKeying()
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(null, null);

        var on = new bool[Rate * 30];
        var audio = Make(on, _ => 0, Wandering(0.5, 0));
        var everKeying = Feed(detector, audio, out _);

        var bins = detector.Bins();

        _output.WriteLine($"noise: bins {bins.Count}, bars in the last second {bins.Sum(b => b.BarsLastSecond)}, keying seen {everKeying}, marked hops {detector.History().Count(h => h.Mark)}");

        Assert.True(bins.Count > 100, "the whole band was not swept");
        Assert.False(everKeying);
        Assert.DoesNotContain(detector.History(), h => h.Mark);
        Assert.Equal(0, detector.Reading.MarksLast4s);
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

    private static bool[] KeyedCq()
    {
        var on = new List<bool>();

        foreach (var (state, ms) in Keying)
        {
            on.AddRange(Enumerable.Repeat(state, (int)Math.Round(ms * Rate / 1000)));
        }

        return on.ToArray();
    }

    /// <summary>Seeded Gaussian noise whose level wanders 12 dB, peak to trough, twice a second.</summary>
    private static Func<int, double> Wandering(double rms, int seed)
    {
        var random = new Random(4770 + seed);

        return n =>
        {
            var level = rms * Math.Pow(10, 6 * Math.Sin(2 * Math.PI * 2 * n / Rate) / 20);
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            return level * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        };
    }

    private static float[] Make(bool[] on, Func<int, double> tone, Func<int, double> noise)
    {
        var audio = new float[on.Length];

        for (var n = 0; n < audio.Length; n++)
        {
            audio[n] = (float)(tone(n) + noise(n));
        }

        return audio;
    }

    private bool Feed(CwEnvelopeDetector detector, float[] audio, out List<CwEnvelopeReading> readings)
    {
        readings = new List<CwEnvelopeReading>();
        var keying = 0;

        for (var offset = 0; offset < audio.Length; offset += Chunk)
        {
            detector.Process(audio.AsSpan(offset, Math.Min(Chunk, audio.Length - offset)));
            readings.Add(detector.Reading);

            if (detector.Reading.Keying && keying == 0)
            {
                _output.WriteLine($"first keying at {offset / (double)Rate:0.000} s: " + string.Join(" ", detector.Bins().Where(b => b.Keying).Select(b => $"{b.Hz:0} Hz bars {b.Bars} gaps {b.Gaps} bar {b.BarDb:0.0} gap {b.GapDb:0.0}")));
            }

            keying += detector.Reading.Keying ? 1 : 0;
        }

        _output.WriteLine($"chunks read keying: {keying} of {readings.Count}");

        return keying > 0;
    }

    /// <summary>A hop is judged only where its window is wholly one key state.</summary>
    private static (int OnHops, int Missed, int OffHops, int Marked) Tally(
        IReadOnlyList<CwScopeHop> history, bool[] on, int first, CwEnvelopeDetector detector)
    {
        var onHops = 0;
        var missed = 0;
        var offHops = 0;
        var marked = 0;

        for (var i = 0; i < history.Count; i++)
        {
            var end = (first + i + 1) * detector.HopSamples;
            var start = Math.Max(0, end - detector.EnvelopeWindowSamples);
            var states = on[start..end].Distinct().ToList();

            if (states.Count != 1)
            {
                continue;
            }

            if (states[0])
            {
                onHops++;
                missed += history[i].Mark ? 0 : 1;
            }
            else
            {
                offHops++;
                marked += history[i].Mark ? 1 : 0;
            }
        }

        return (onHops, missed, offHops, marked);
    }
}
