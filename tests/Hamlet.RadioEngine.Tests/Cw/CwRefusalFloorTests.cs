using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The decoder goes quiet rather than copying into the noise (HM-DEC-097).
/// </summary>
/// <remarks>
/// <para>**THE RULING WAS MADE ON A SWEEP AND WENT UNBUILT FOR FOUR SESSIONS.**
/// It said the decoder refuses below nought decibels rather than copying into
/// the band where it is half wrong, and nothing in the decoder implemented a
/// floor: the streaming pass gated on coherence and a plausible speed, the
/// settled pass on six decibels of contrast, and neither is what the ruling
/// describes. The sweep kept emitting a full nine characters all the way down to
/// minus five, of which 44 percent were invented at minus two.</para>
/// <para>**SEVENTEEN IS THAT RULING TRANSLATED, NOT RENEGOTIATED.** Its decibels
/// are the broadband ratio a fixture is generated at, and this decoder measures
/// inside a narrow tone filter reading about seventeen higher for the same
/// audio.</para>
/// </remarks>
public sealed class CwRefusalFloorTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sweep is printed.</param>
    public CwRefusalFloorTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the floor exists at the number Tim ruled. **Fifteen was rejected**
    /// because it lets minus two decibels through, which is the case the ruling
    /// names; **ten was rejected** as leaving open the whole band the ruling
    /// exists to silence.
    /// </remarks>
    [Fact]
    public void TheFloorIsSeventeenAndItIsNamedRatherThanScattered()
    {
        Assert.Equal(17.0, CwConfidenceModel.RefusalFloorDb);

        // And it sits above the point where a character stops being worth
        // showing at full strength, or the floor would never bite first.
        Assert.True(CwConfidenceModel.RefusalFloorDb > CwConfidenceModel.PoorSignalDb);
    }

    /// <remarks>
    /// <para>**THE PROPERTY THE RULING EXISTS FOR: NOTHING IS EVER INVENTED.**
    /// Below the floor the decoder emits no characters at all, so there is
    /// nothing on screen for the operator to act on and be wrong about. Silence
    /// is the honest output when there is nothing readable there (§0.0).</para>
    /// </remarks>
    [Fact]
    public void NothingIsEmittedAnywhereBelowTheFloor()
    {
        var sweep = CwSensitivity.Sweep();

        _output.WriteLine(CwSensitivity.Report(sweep));

        var worst = sweep.OrderByDescending(p => p.Wrong).First();

        _output.WriteLine("");
        _output.WriteLine($"worst wrong share {worst.Wrong:0.00} at {worst.SnrDb:0.0} dB");

        // **ZERO, NOT A SMALL NUMBER.** A decoder that invents nothing at any
        // level is what the ruling asked for, and it is what the floor delivers.
        Assert.Equal(0.0, worst.Wrong);
    }

    /// <remarks>
    /// <para>Proves the floor did not cost the top of the range. A refusal that
    /// silenced a signal the operator can plainly hear would be worse than the
    /// copying it replaced: the whole point is to decode almost anything a
    /// trained ear can copy and to say nothing below that.</para>
    /// </remarks>
    [Fact]
    public void AStrongSignalIsUntouchedByTheFloor()
    {
        var sweep = CwSensitivity.Sweep();

        foreach (var point in sweep.Where(p => p.SnrDb >= 6.0))
        {
            _output.WriteLine($"{point.SnrDb,5:0.0} dB  right {point.Correct:0.00}  "
                + $"emitted {point.Emitted}");

            Assert.Equal(1.0, point.Correct);
            Assert.Equal(9, point.Emitted);
        }
    }
}
