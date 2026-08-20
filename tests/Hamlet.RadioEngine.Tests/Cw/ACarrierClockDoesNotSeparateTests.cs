using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether the on and off edges landing on a fitted clock can tell a station
/// from an empty band. **Measured, and they cannot** (HM-OPEN-054).
/// </summary>
/// <remarks>
/// <para>**THE CANDIDATE.** Keying is periodic and noise is not, so the moments
/// the signal turns on and off should fall on a grid of one unit interval where
/// somebody is sending and nowhere in particular where nobody is. Fitting a clock
/// to the transitions asks nothing about characters, which is what made it worth
/// trying: a test for whether the decoder may speak at all cannot rest on the
/// structure it is trying to read.</para>
/// <para>**WHAT WAS FITTED.** For a candidate interval, every transition is turned
/// into an angle by how far through the interval it falls, and the length of the
/// mean of those angles is taken. One means every edge lands at the same point in
/// the cycle; nought means they are spread evenly around it. The interval is swept
/// from 15 to 250 milliseconds and the best one kept. Nothing is taken from the
/// speed estimator and no interval is assumed, so the answer is fitted from the
/// edges and from nothing else.</para>
/// <para>**IT SEPARATES BEAUTIFULLY ON SYNTHESIZED AUDIO AND NOT AT ALL ON REAL
/// AUDIO.** The easy tier fits its own dit to the millisecond and scores 0.83 to
/// 0.99. Real captures score 0.34 to 0.47 across a whole recording whether they
/// hold a station or not, because a recording with a station in it is mostly band
/// noise as well.</para>
/// <para>**AND AT THE MOMENT OF EMISSION, WHICH IS WHERE A GATE WOULD LIVE, THE
/// TWO OVERLAP.** In the tree as it stands, `cw-2026-08-18-003016` emits a real
/// character at an agreement of 0.389 while `cw-2026-08-20-014854`, which holds no
/// keying at any pitch, emits its one character at 0.470. **In the configuration
/// this gate exists to enable** — with `Refine` applied, which is what the gate was
/// commissioned to unblock — that recording invents nine characters at agreements
/// from 0.456 to 0.533, while `prosigns-easy` emits a real one at 0.493 and
/// `tightfist-easy` at 0.497. HM-DEC-114 makes those pass or fail, so a gate low
/// enough to keep the easy tier whole still admits everything the empty band
/// invents.</para>
/// <para>These stay in the tree so the finding is reproducible rather than
/// remembered. They assert the overlap, which is what was measured; they classify
/// nothing and nothing in `src` reads them.</para>
/// </remarks>
public sealed class ACarrierClockDoesNotSeparateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public ACarrierClockDoesNotSeparateTests(ITestOutputHelper output)
        => _output = output;

    private static MonoAudio Captured(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static MonoAudio Fixture(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, "..", "receiver", name + ".wav"));

    /// <summary>
    /// How tightly a set of transition times lands on a grid of one interval, and
    /// which interval fits best.
    /// </summary>
    /// <param name="edges">When the key went down or up, in seconds.</param>
    /// <returns>Agreement from nought to one, and the interval in milliseconds.</returns>
    public static (double Agreement, double IntervalMs) Fit(IReadOnlyList<double> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);

        if (edges.Count < 8)
        {
            return (0, 0);
        }

        var best = 0.0;
        var bestMs = 0.0;

        for (var ms = 15.0; ms <= 250.0; ms += 0.5)
        {
            var interval = ms / 1000;
            double x = 0, y = 0;

            foreach (var edge in edges)
            {
                var angle = 2 * Math.PI * edge / interval;

                x += Math.Cos(angle);
                y += Math.Sin(angle);
            }

            var agreement = Math.Sqrt((x * x) + (y * y)) / edges.Count;

            if (agreement > best)
            {
                best = agreement;
                bestMs = ms;
            }
        }

        return (best, bestMs);
    }

    /// <summary>Every moment the gate turned the key on or off, in seconds.</summary>
    private static List<double> Transitions(MonoAudio audio, double startHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var hop = decoder.Tracker.HopSamples;
        var edges = new List<double>();
        var seen = 0;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            edges.Add(at / (double)audio.SampleRate);
            seen = decoder.Report.ElementsSeen;
        }

        return edges;
    }

    /// <remarks>
    /// <para>Proves the candidate works where the audio is clean: the easy tier
    /// fits its own dit and its edges land on it almost exactly.</para>
    /// </remarks>
    /// <param name="name">The fixture.</param>
    /// <param name="expectedMs">The dit it is sent at.</param>
    [Theory]
    [InlineData("coverage-easy", 100)]
    [InlineData("exchange-easy", 100)]
    [InlineData("prosigns-easy", 100)]
    [InlineData("fast-easy", 48)]
    [InlineData("tightfist-easy", 88)]
    public void OnCleanAudioTheEdgesLandOnTheSendersOwnDit(string name, int expectedMs)
    {
        var (agreement, ms) = Fit(Transitions(Fixture(name), 600));

        _output.WriteLine($"{name}: {agreement:0.000} at {ms:0} ms");

        Assert.True(agreement > 0.65, $"{name} agreed only {agreement:0.000}");
        Assert.InRange(ms, expectedMs - 6, expectedMs + 6);
    }

    /// <remarks>
    /// <para>Proves the candidate fails where it would have to work: **over a
    /// whole real recording the agreement says nothing about whether a station is
    /// in it.** `cw-2026-08-17-134712` holds `N4L` (HM-DEC-144) and scores no
    /// better than `cw-2026-08-20-014935`, which holds nothing, because both are
    /// mostly band noise.</para>
    /// </remarks>
    [Fact]
    public void OverAWholeRealRecordingItSaysNothing()
    {
        var withStation = Fit(Transitions(Captured("cw-2026-08-17-134712"), 600));
        var without = Fit(Transitions(
            Captured("unadjudicated/cw-2026-08-20-014935"), 600));

        _output.WriteLine(
            $"134712 (a station): {withStation.Agreement:0.000} at {withStation.IntervalMs:0} ms");
        _output.WriteLine(
            $"014935 (nothing):   {without.Agreement:0.000} at {without.IntervalMs:0} ms");

        Assert.True(
            withStation.Agreement < without.Agreement + 0.1,
            "the recording with a station in it scored clearly higher, which would "
            + "mean this candidate works after all");
    }

    /// <remarks>
    /// <para>Proves it where a gate would actually stand: **the agreement at the
    /// moment a character is emitted overlaps between real captures and an empty
    /// band.** `cw-2026-08-17-134712`'s own callsign window scores 0.68 at 48 ms,
    /// which is
    /// promising and is not enough, because a gate has to pass every real
    /// character on the easy tier too and those come out as low as 0.49.</para>
    /// </remarks>
    [Fact]
    public void AtTheMomentOfEmissionTheTwoOverlap()
    {
        var callsign = Transitions(Captured("cw-2026-08-17-134712"), 500)
            .Where(e => e is >= 21.4 and <= 23.05)
            .ToList();

        var (inside, insideMs) = Fit(callsign);

        _output.WriteLine(
            $"the N4L window: {inside:0.000} at {insideMs:0} ms over {callsign.Count} edges");

        // The station's own window does agree with a clock, and its interval is
        // near the 56.3 ms HM-DEC-144 measured by hand.
        Assert.True(inside > 0.6, $"the callsign window agreed only {inside:0.000}");

        // **AND HERE IS THE OVERLAP, IN THE TREE AS IT STANDS.** A real capture
        // emits real characters at agreements below the one at which a recording
        // holding no keying emits its invented character, so no line drawn on
        // this statistic separates them.
        var real = LowestAgreementAtEmission(
            Captured("unadjudicated/cw-2026-08-18-003016"), 600);
        var invented = LowestAgreementAtEmission(
            Captured("unadjudicated/cw-2026-08-20-014854"), 600);

        _output.WriteLine(
            $"a real character comes out at {real:0.000}; "
            + $"an invented one at {invented:0.000}");

        Assert.True(
            real < invented,
            "every real character came out above everything the empty band "
            + "invented, which would leave room for a gate after all");
    }

    private double LowestAgreementAtEmission(MonoAudio audio, double startHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var hop = decoder.Tracker.HopSamples;
        var edges = new List<double>();
        var lowest = 1.0;
        var seen = 0;

        decoder.CharacterDecoded += _ =>
        {
            if (edges.Count < 20)
            {
                return;
            }

            var (agreement, _) = Fit(edges.Skip(edges.Count - 20).ToList());

            lowest = Math.Min(lowest, agreement);
        };

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            edges.Add(at / (double)audio.SampleRate);
            seen = decoder.Report.ElementsSeen;
        }

        decoder.Flush();

        return lowest;
    }
}
