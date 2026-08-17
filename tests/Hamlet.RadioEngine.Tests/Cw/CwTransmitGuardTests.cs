using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Telling the operator's own transmission from the band (HM-DEC-095).
/// </summary>
public sealed class CwTransmitGuardTests
{
    private static CwTransmitGuard Guard() => new(0.005);

    /// <remarks>
    /// Proves HM-DEC-095: band noise is not a transmission, however quiet the
    /// band gets. A five hundred hertz filter on a dead band still delivers
    /// something in the minus twenties, and nothing there may be blocked.
    /// </remarks>
    [Theory]
    [InlineData(-15)]
    [InlineData(-30)]
    [InlineData(-55)]
    public void BandNoiseIsNeverMistakenForTransmitting(double dbfs)
    {
        var guard = Guard();

        for (var i = 0; i < 200; i++)
        {
            Assert.False(guard.Observe(dbfs));
        }

        Assert.Equal(0, guard.Transmissions);
    }

    /// <remarks>
    /// Proves HM-DEC-095: the mute itself. On full break-in the receiver drops
    /// the audio between the operator's own elements, and what arrives is the
    /// codec's residue rather than the band.
    /// </remarks>
    [Fact]
    public void AMutedReceiverIsRecognized()
    {
        var guard = Guard();

        guard.Observe(-20);
        Assert.True(guard.Observe(-75));
        Assert.True(guard.IsMuted);
        Assert.Equal(1, guard.Transmissions);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-095, and it is the bound the synthetic fixtures
    /// found. **A muted receiver is quiet and an empty file is zero.** Measured
    /// across this repository, the mutes in the real recording bottom out between
    /// minus eighty and minus eighty-four; the gaps in synthesized Morse measure
    /// minus two hundred and forty.</para>
    /// <para>Without this the guard blocked eighty percent of every clean fixture
    /// and deleted the decode outright.</para>
    /// </remarks>
    [Fact]
    public void DigitalSilenceIsNotAMutedReceiver()
    {
        var guard = Guard();

        guard.Observe(-20);

        Assert.False(guard.Observe(-240));
        Assert.False(guard.IsMuted);
        Assert.False(guard.IsBlocked);
        Assert.Equal(0, guard.Transmissions);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-095: the hold. The receiver comes back over a few tens
    /// of milliseconds and the first thing through the gate is a gain ramp, not a
    /// signal. Measuring that ramp as an element is how the slivers between the
    /// operator's own dits became a confident run of E and T.</para>
    /// </remarks>
    [Fact]
    public void TheGuardHoldsPastTheMomentTheAudioReturns()
    {
        var guard = Guard();

        guard.Observe(-20);
        guard.Observe(-75);

        // Audio is back, and the guard is not.
        Assert.True(guard.Observe(-20));

        var held = 0;
        while (guard.Observe(-20))
        {
            held++;

            if (held > 200)
            {
                break;
            }
        }

        var seconds = held * 0.005;

        Assert.InRange(seconds, CwTransmitGuard.HoldSeconds * 0.5, CwTransmitGuard.HoldSeconds);
    }

    /// <remarks>
    /// Proves HM-DEC-095: a run of mutes is counted as the transmissions it is,
    /// so a capture can say how much of what it heard was the operator himself
    /// (§0.0.1).
    /// </remarks>
    [Fact]
    public void SeparateTransmissionsAreCountedSeparately()
    {
        var guard = Guard();

        for (var t = 0; t < 3; t++)
        {
            for (var i = 0; i < 10; i++)
            {
                guard.Observe(-75);
            }

            // Long enough that the hold expires between them.
            for (var i = 0; i < 60; i++)
            {
                guard.Observe(-20);
            }
        }

        Assert.Equal(3, guard.Transmissions);
        Assert.False(guard.IsBlocked);
    }
}
