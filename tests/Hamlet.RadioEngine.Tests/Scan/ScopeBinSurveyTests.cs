using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// What the waterfall can and cannot say about a band (HM-DEC-107 phase 6).
/// </summary>
/// <remarks>
/// <para>**IT CANNOT IDENTIFY MORSE AND NOTHING HERE PRETENDS OTHERWISE.** Four
/// and a half sweeps a second against a sixty millisecond dit aliases the keying
/// away completely. What survives is occupancy and movement, and those sort a
/// band into places worth listening to and places that are not.</para>
/// <para>The signals here are built from the three shapes the brief names: a
/// steady carrier, an operator sending, and empty spectrum.</para>
/// </remarks>
public sealed class ScopeBinSurveyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the ranking is printed.</param>
    public ScopeBinSurveyTests(ITestOutputHelper output) => _output = output;

    private const int Bins = 64;
    private const long LowHz = 7_000_000;
    private const long HighHz = 7_050_000;

    /// <summary>
    /// Push sweeps through a survey, with a carrier and an operator on it.
    /// </summary>
    /// <param name="sweeps">How many sweeps to send.</param>
    /// <param name="carrierBin">Where a steady carrier sits, or -1.</param>
    /// <param name="keyedBin">Where somebody is sending, or -1.</param>
    /// <param name="duty">What share of the time that person is keying.</param>
    private static ScopeBinSurvey Feed(
        int sweeps, int carrierBin, int keyedBin, double duty = 0.55)
    {
        var survey = new ScopeBinSurvey();
        var bins = new byte[Bins];

        // Deterministic wobble, so the same call gives the same answer (§5).
        var state = 20260817u;

        for (var s = 0; s < sweeps; s++)
        {
            for (var b = 0; b < Bins; b++)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;

                // Band noise: a floor with a couple of counts of movement.
                var value = 20 + (state % 4);

                if (b == carrierBin)
                {
                    // Switched on and left on: loud, and it does not move.
                    value = 90 + (state % 2);
                }
                else if (b == keyedBin)
                {
                    // Somebody sending: present about half the time, and the
                    // difference between key down and key up is large.
                    var down = (s * 7 % 100) < duty * 100;
                    value = down ? 85 + (state % 3) : 21 + (state % 3);
                }

                bins[b] = (byte)value;
            }

            survey.Observe(new SpectrumFrame(LowHz, HighHz, DateTime.UnixEpoch, bins));
        }

        return survey;
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 6: **a bin nobody has watched for long enough is
    /// not ranked.** Ten seconds is the least that can tell an operator pausing
    /// between calls from an empty patch of band, and a scan that acted sooner
    /// would be touring noise (§0.0).
    /// </remarks>
    [Fact]
    public void ABinWatchedTooBrieflyIsNotRanked()
    {
        var survey = Feed(sweeps: 10, carrierBin: -1, keyedBin: 30);

        Assert.Empty(survey.Ranked());
        Assert.Empty(survey.Describe());
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 6, and it is the whole point of the ranking:
    /// **the operator outranks the carrier, which is louder.** Anything sorting
    /// by strength tours the birdies and never reaches a person, because a
    /// carrier is the loudest thing on many bands and is always there.</para>
    /// </remarks>
    [Fact]
    public void SomebodySendingOutranksALouderSteadyCarrier()
    {
        var survey = Feed(sweeps: 120, carrierBin: 12, keyedBin: 40);
        var ranked = survey.Ranked();

        foreach (var bin in ranked.Take(5))
        {
            _output.WriteLine($"{bin.CenterHz} Hz  score {bin.Score:0.000}  "
                + $"presence {bin.Presence:P0}  swing {bin.Variability:0.0}  "
                + $"lift {bin.LiftCounts:0}");
        }

        Assert.NotEmpty(ranked);

        var best = ranked[0];
        var expected = LowHz + (long)Math.Round(40 * (double)(HighHz - LowHz) / (Bins - 1));

        Assert.Equal(expected, best.CenterHz);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 6: **a steady carrier is named as one rather
    /// than merely pushed down the list.** Anything loud and unmoving inside the
    /// receive passband sets the gain for everything quieter, which is something
    /// the operator can act on (§1.4).</para>
    /// </remarks>
    [Fact]
    public void ASteadyCarrierIsNamedAsSteady()
    {
        var survey = Feed(sweeps: 120, carrierBin: 12, keyedBin: 40);
        var steady = survey.Steady();

        foreach (var bin in steady)
        {
            _output.WriteLine($"steady at {bin.CenterHz} Hz, "
                + $"presence {bin.Presence:P0}, swing {bin.Variability:0.0}");
        }

        Assert.Single(steady);

        var expected = LowHz + (long)Math.Round(12 * (double)(HighHz - LowHz) / (Bins - 1));

        Assert.Equal(expected, steady[0].CenterHz);

        // **DEMOTED RATHER THAN DELETED, WHICH IS WHAT THE RULING ASKS FOR.**
        // The carrier is still the second loudest thing on this band and there is
        // no honesty in pretending it is not there; what matters is that a scan
        // working down the list reaches every operator before it reaches any
        // carrier. Here it scores about a hundred and sixty times lower.
        var ranked = survey.Ranked();
        var carrier = ranked.Single(b => b.LooksSteady);
        var person = ranked.First(b => !b.LooksSteady);

        _output.WriteLine($"keyed {person.Score:0.000} against carrier {carrier.Score:0.000}");

        Assert.True(
            person.Score > carrier.Score * 10,
            $"the carrier scored {carrier.Score:0.000} against the operator's "
            + $"{person.Score:0.000}, which is not a demotion worth the name");
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 6 and §0.0: **empty spectrum produces no
    /// candidates.** A scanner that always has somewhere to go next is not
    /// measuring anything, and it would send the operator round an empty band
    /// all evening.
    /// </remarks>
    [Fact]
    public void AnEmptyBandOffersNothingToStopOn()
    {
        var survey = Feed(sweeps: 120, carrierBin: -1, keyedBin: -1);

        _output.WriteLine($"{survey.Ranked().Count} candidates from an empty band");

        Assert.Empty(survey.Ranked());
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 6: **a change of span throws the history
    /// away.** The bins mean different frequencies afterwards, so carrying the
    /// counts across would report occupancy at places nothing was ever measured,
    /// and a scan changes span deliberately.</para>
    /// </remarks>
    [Fact]
    public void MovingTheRadioForgetsWhatWasMeasuredElsewhere()
    {
        var survey = Feed(sweeps: 120, carrierBin: -1, keyedBin: 40);

        Assert.NotEmpty(survey.Ranked());

        var bins = new byte[Bins];
        Array.Fill(bins, (byte)20);

        survey.Observe(new SpectrumFrame(
            14_000_000, 14_050_000, DateTime.UnixEpoch, bins));

        _output.WriteLine($"after moving band: {survey.Sweeps} sweeps, "
            + $"{survey.Ranked().Count} candidates");

        Assert.Equal(1, survey.Sweeps);
        Assert.Empty(survey.Ranked());
        Assert.Equal((14_000_000L, 14_050_000L), survey.Span);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 6: the frequency a candidate names comes from the
    /// sweep's own header and not from anything Hamlet assumed, which is what
    /// makes it safe for a scan to tune there (§0.0, §0.2.1).
    /// </remarks>
    [Fact]
    public void ACandidatesFrequencyComesFromTheSweepHeader()
    {
        var survey = Feed(sweeps: 120, carrierBin: -1, keyedBin: 40);
        var best = survey.Ranked()[0];

        Assert.InRange(best.CenterHz, LowHz, HighHz);

        var (low, high) = survey.Span;

        Assert.Equal(LowHz, low);
        Assert.Equal(HighHz, high);
    }
}
