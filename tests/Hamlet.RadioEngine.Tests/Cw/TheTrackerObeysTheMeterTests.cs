using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE TRACKER OBEYS THE METER** (work instruction 477 task 2, step 12 criterion 12.4 first
/// half, HM-DEC-186).
/// </summary>
/// <remarks>
/// <para>**WHAT THE OWNER'S ROWS SHOWED, 2026-09-28.** On 14.0327 the meter said 600 Hz at the
/// first press and the tracker reached 600 twenty-two seconds later, when the survey admitted
/// it; on 14.0321 the tracker reached 375 Hz and still told the decoder nobody was keying on
/// four rows of six. The rows are the evidence the wiring was missing; nothing here is fitted
/// to them.</para>
/// <para>**NO RECORDING** (R88). The meter's reading is driven, as the app hands it over; the
/// audio is seeded synthetic noise, or a synthetic keyed tone where the survey must be given
/// something of its own to admit.</para>
/// </remarks>
public sealed class TheTrackerObeysTheMeterTests
{
    private const int Rate = 48_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches are printed.</param>
    public TheTrackerObeysTheMeterTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves that when the meter says keying at 600 Hz while the tracker holds 500, the
    /// tracker mixes at 600 on the next hop, as a measured pitch the decoder takes.
    /// </remarks>
    [Fact]
    public void AMeterSayingKeyingAt600MovesTheTrackerThereOnTheNextHop()
    {
        var tracker = new CwToneTracker(Rate, 500);
        var noise = new Noise(600);

        Feed(tracker, noise, Rate / 2);
        _output.WriteLine($"before: {tracker.ToneHz:0} Hz, measured {tracker.HasMeasuredPitch}");
        Assert.Equal(500, tracker.ToneHz, 0);

        tracker.FollowMeter(Keying(600, swingDb: 24));
        Feed(tracker, noise, tracker.HopSamples);

        _output.WriteLine($"after one hop: {tracker.ToneHz:0} Hz, measured {tracker.HasMeasuredPitch}, keying {tracker.HasKeying}");
        Assert.Equal(600, tracker.ToneHz, 0);
        Assert.True(tracker.HasMeasuredPitch);
    }

    /// <remarks>
    /// Proves the keying flag follows the meter's verdict and not a swing: a meter reading of
    /// keying on an 18.6 dB swing - the station at 01:16:22 - sets HasKeying, and a meter that
    /// stops saying keying lets it go back to the survey's own verdict.
    /// </remarks>
    [Fact]
    public void AMeterSayingKeyingOnAnEighteenDecibelSwingSetsHasKeying()
    {
        var tracker = new CwToneTracker(Rate, 500);
        var noise = new Noise(186);

        Feed(tracker, noise, Rate);
        Assert.False(tracker.HasKeying);

        tracker.FollowMeter(Keying(375, swingDb: 18.6));
        Feed(tracker, noise, tracker.HopSamples);

        _output.WriteLine($"meter keying at 375 on 18.6 dB: HasKeying {tracker.HasKeying}, {tracker.ToneHz:0} Hz");
        Assert.True(tracker.HasKeying);
        Assert.Equal(375, tracker.ToneHz, 0);

        tracker.FollowMeter(KeyingReading.None);
        Feed(tracker, noise, Rate);

        _output.WriteLine($"meter listening: HasKeying {tracker.HasKeying}");
        Assert.False(tracker.HasKeying);
    }

    /// <remarks>
    /// Proves the meter's pitch still wins while the meter says keying there, against the
    /// survey's own moves: a keyed tone at 750 Hz, the meter saying 600. **What the survey did
    /// here, measured**: on the stub it admitted no candidate in eight seconds and moved the
    /// tracker to 750 by its cold-start rule (point at the loudest thing); with the meter
    /// obeyed it moves nothing and every hop reads 600. A survey admission elsewhere is held off
    /// by the same branch and is not separately exercised here.
    /// </remarks>
    [Fact]
    public void TheMetersPitchWinsOverTheSurveyWhileTheMeterSaysKeying()
    {
        var tracker = new CwToneTracker(Rate, 600);
        var noise = new Noise(750);

        tracker.FollowMeter(Keying(600, swingDb: 24));

        // PARIS at 20 words a minute, keyed at 750 Hz, for eight seconds.
        var dit = Rate * 60 / 1000;
        var pattern = new[] { 1, 1, 3, 1, 3, 1, 1, 3, 1, 3, 1, 3, 1, 1, 3, 1, 3, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 7 };
        var on = true;
        var n = 0;
        var chunk = new float[tracker.HopSamples];
        var seen = new List<double>();

        while (n < Rate * 8)
        {
            foreach (var units in pattern)
            {
                for (var i = 0; i < units * dit; i++, n++)
                {
                    var tone = on ? 0.3 * Math.Sin(2 * Math.PI * 750 * n / Rate) : 0;
                    chunk[n % chunk.Length] = (float)(tone + noise.Next());

                    if (n % chunk.Length == chunk.Length - 1)
                    {
                        tracker.Process(chunk, n - chunk.Length + 1, _ => { });
                        seen.Add(tracker.ToneHz);
                    }
                }

                on = !on;
            }
        }

        var admitted = tracker.CoarseCandidates().Select(c => c.ToneHz).ToList();

        _output.WriteLine($"survey admits: {string.Join(", ", admitted)}; tracker read {seen.Min():0} to {seen.Max():0} Hz");
        Assert.All(seen, hz => Assert.Equal(600, hz, 0));
    }

    private static KeyingReading Keying(double hz, double swingDb)
        => new(KeyingVerdict.Keying, hz, 48, swingDb, 30, 0.27, false, 48);

    private static void Feed(CwToneTracker tracker, Noise noise, int samples)
    {
        var chunk = new float[samples];

        for (var i = 0; i < samples; i++)
        {
            chunk[i] = noise.Next();
        }

        tracker.Process(chunk, 0, _ => { });
    }

    /// <summary>Seeded Gaussian noise, Box-Muller.</summary>
    private sealed class Noise
    {
        private readonly Random _random;

        public Noise(int seed) => _random = new Random(seed);

        public float Next()
        {
            var u1 = 1.0 - _random.NextDouble();
            var u2 = _random.NextDouble();

            return (float)(0.03 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }
    }
}
