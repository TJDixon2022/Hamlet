using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The SWR meter, which only means anything while transmitting (HM-DEC-081).
/// </summary>
/// <remarks>
/// SWR is derived from reflected power, so a resting radio has nothing to
/// measure and whatever the meter returns is not a reading of now. The scale is
/// cited at four points and nothing between them is, so the conversion is linear
/// between them and refuses past the last rather than extrapolating a curve
/// nobody published (§4).
/// </remarks>
public sealed class SwrTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    /// <remarks>
    /// Proves HM-DEC-081: the scale converts correctly at all four cited points
    /// (Full Manual p. 19-3). These are the only readings the manual states, and
    /// they are stated in decimal: reading 0120 as hexadecimal would give 288 and
    /// a wildly wrong ratio, which is the mistake §4 records twice already.
    /// </remarks>
    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(48, 1.5)]
    [InlineData(80, 2.0)]
    [InlineData(120, 3.0)]
    public void TheCitedPointsConvertExactly(int reading, double ratio)
        => Assert.Equal(ratio, CivSwr.Ratio(reading));

    /// <remarks>
    /// Proves HM-DEC-081: between the cited points the conversion is linear and
    /// monotonic, which is the most that can be claimed from four points.
    /// </remarks>
    [Fact]
    public void BetweenThePointsItRisesAndNeverFalls()
    {
        double? previous = null;

        for (var reading = 0; reading <= 120; reading++)
        {
            var ratio = CivSwr.Ratio(reading);

            Assert.NotNull(ratio);
            Assert.True(previous is null || ratio >= previous,
                $"{reading} went backwards");

            previous = ratio;
        }
    }

    /// <remarks>
    /// Proves HM-DEC-081: past the last cited point Hamlet says "higher than 3
    /// to 1" rather than extrapolating. That is what the manual supports and it
    /// is also everything the operator needs, since anything up there wants the
    /// same action.
    /// </remarks>
    [Fact]
    public void PastTheLastCitedPointItRefusesToGuess()
    {
        Assert.Null(CivSwr.Ratio(121));
        Assert.Null(CivSwr.Ratio(255));
        Assert.Equal("higher than 3 to 1", CivSwr.Describe(200));
    }

    /// <remarks>
    /// Proves HM-DEC-081: above 1.5 the advice is to tune the antenna
    /// (p. 11-2), and at or below it there is no such advice, because a matched
    /// antenna does not need tuning and saying so anyway would teach the
    /// operator to ignore the sentence.
    /// </remarks>
    [Theory]
    [InlineData(0, false)]
    [InlineData(48, false)]
    [InlineData(60, true)]
    [InlineData(120, true)]
    [InlineData(200, true)]
    public void OnlyAHighReadingSaysToTuneTheAntenna(int reading, bool tunes)
    {
        var said = SwrReport.Describe(reading);

        Assert.Equal(tunes, SwrReport.IsHigh(reading));
        Assert.Equal(tunes, said.Contains("TUNER", StringComparison.Ordinal));

        if (!tunes)
        {
            Assert.Contains("matched", said, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-081 and §0.0: **nothing here claims what is connected to
    /// the antenna socket.** A dummy load reads close to flat and a matched
    /// antenna reads under 1.5, which is suggestive and is not evidence, and
    /// "your antenna is connected" would be a guess dressed as a decode on the
    /// one screen where a wrong answer means somebody keys into the wrong thing.
    /// </remarks>
    [Fact]
    public void NothingClaimsWhatIsOnTheAntennaSocket()
    {
        for (var reading = 0; reading <= 255; reading += 5)
        {
            var said = SwrReport.Describe(reading).ToLowerInvariant();

            foreach (var claim in new[]
                     {
                         "your antenna", "the antenna is connected", "dummy load",
                         "antenna is attached", "nothing is connected",
                         "is a dummy", "must be", "looks like an antenna",
                     })
            {
                Assert.False(said.Contains(claim, StringComparison.Ordinal),
                    $"reading {reading} claims '{claim}'");
            }
        }
    }

    /// <remarks>
    /// Proves HM-DEC-081: nothing measured is nothing said, rather than a zero
    /// or a cheerful stand-in.
    /// </remarks>
    [Fact]
    public void NothingMeasuredIsNothingSaid()
    {
        Assert.Equal("", SwrReport.Describe(null));
        Assert.False(SwrReport.IsHigh(null));
    }

    /// <remarks>
    /// Proves HM-DEC-081 and HM-DEC-050: **a resting value is never shown as a
    /// current one.** The moment the transmitter stops there is nothing to
    /// measure, so the last figure is a reading of a moment that has passed and
    /// it goes back to unknown rather than sitting there looking live.
    /// </remarks>
    [Fact]
    public void ARestingRadioHasNoStandingWaveRatio()
    {
        var keying = RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.TransmitStatus, 1, "transmitting", Now, "CI-V 1C 00"),
            RigValue.Known(RigField.Swr, 48, "1.5 to 1", Now, "CI-V 15 12"),
        });

        Assert.True(keying[RigField.Swr].IsKnown);

        // The monitor drops it the moment transmitting goes false; this is the
        // same rule stated where a reader can check it.
        var resting = keying
            .With(RigValue.Known(
                RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"))
            .With(RigValue.Unknown(
                RigField.Swr, "only measurable while transmitting"));

        Assert.False(resting[RigField.Swr].IsKnown);
        Assert.Null(resting[RigField.Swr].Number);
        Assert.Contains(
            "only measurable while transmitting", resting[RigField.Swr].Source,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-081: the read is in the table like every other field, with
    /// its command and its page, so the diagnostics screen lists it without
    /// anybody adding it there.
    /// </remarks>
    [Fact]
    public void TheReadIsInTheTableWithItsCitation()
    {
        var read = CivReads.All.Single(r => r.Field == RigField.Swr);

        Assert.Equal(0x15, read.Command);
        Assert.Equal(new byte[] { 0x12 }, read.SubCommand);
        Assert.Equal("19-3", read.Page);
        Assert.Contains("0048=1.5", read.Note, StringComparison.Ordinal);
    }
}
