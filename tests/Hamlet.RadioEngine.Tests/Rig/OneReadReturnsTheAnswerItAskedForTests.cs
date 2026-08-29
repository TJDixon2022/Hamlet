using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A read is answered by the frame it asked for, whatever else the radio is
/// volunteering at the time.
/// </summary>
/// <remarks>
/// <para>**THE REPORTED FAULT: a single `26` read returns a stale answer, and the
/// query has to be issued twice before USB can be told from USB-D** (work
/// instruction 050, from the bench on 2026-08-28). The leading hypotheses were a
/// next-frame-wins reader and the two mechanisms that put unasked-for frames on
/// the bus — CI-V USB Port defaulting to "Link to [REMOTE]", which echoes
/// transmitted frames back (p. 12-8), and transceive, which broadcasts whenever
/// the dial moves.</para>
/// <para>**THESE ARE THE REPRODUCTION.** Each puts the suspected interference in
/// front of the true reply and asserts that one read still returns the true
/// reply. A reader that took the next frame available would fail every one of
/// them.</para>
/// </remarks>
public sealed class OneReadReturnsTheAnswerItAskedForTests
{
    private readonly ITestOutputHelper _output;

    public OneReadReturnsTheAnswerItAskedForTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The reader is fed its own echo, then the true reply.</summary>
    /// <remarks>
    /// **THE ECHO IS ON BY DEFAULT ON THIS RADIO.** "Link to [REMOTE]" puts every
    /// transmitted frame back on the bus, so a reader that matched only on
    /// arrival order would answer a `26` read with its own outgoing `26`.
    /// </remarks>
    [Fact]
    public async Task ItsOwnEchoDoesNotAnswerARead()
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_074_000 };

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        // USB-D: mode 01, data flag 01. The distinction command 04 cannot make.
        radio.SetModeAndData(0x01, dataMode: 1);
        radio.EchoOwnFramesBack = true;

        var values = await rig.ReadAsync(RigField.DataMode, RigState.Empty);
        var data = Single(values);

        _output.WriteLine(
            $"one read with the echo on: {data.Text} (known {data.IsKnown}), "
            + $"{rig.Link.Inbound} frames inbound");

        Assert.True(data.IsKnown, "the read came back unknown with the echo on");
        Assert.Equal(1, data.Number);

        // **PROOF THE INTERFERENCE ACTUALLY ARRIVED.** A test that passes because
        // the echo never reached the reader would prove nothing at all (§12.5):
        // the connect sweep plus this read is a known number of replies, and the
        // echoes are on top of it.
        Assert.True(
            rig.Link.Inbound > rig.Link.Answered,
            $"only {rig.Link.Inbound} frames arrived for {rig.Link.Answered} "
            + "answers, so no echo was injected and this proves nothing");
    }

    /// <summary>A transceive broadcast arrives while a read is in flight.</summary>
    /// <remarks>
    /// **THE OPERATOR'S KNOB IS THE OTHER SOURCE.** Transceive volunteers a
    /// frequency or mode frame whenever the dial moves, which is precisely when a
    /// scan is running and precisely when the answer matters.
    /// </remarks>
    [Fact]
    public async Task ATransceiveBroadcastDoesNotAnswerARead()
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_074_000 };

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        radio.SetModeAndData(0x01, dataMode: 1);
        radio.VolunteerTransceiveBeforeReplying = true;

        var values = await rig.ReadAsync(RigField.DataMode, RigState.Empty);
        var data = Single(values);

        _output.WriteLine(
            $"one read with a broadcast in the way: {data.Text} "
            + $"(known {data.IsKnown}), {rig.Link.InboundTransceive} transceive "
            + "frames arrived");

        Assert.True(data.IsKnown, "a transceive broadcast swallowed the read");
        Assert.Equal(1, data.Number);

        // The same proof: the broadcast has to have been on the wire.
        Assert.True(
            rig.Link.InboundTransceive > 0,
            "no transceive frame arrived, so this proves nothing");
    }

    /// <summary>
    /// One read tells USB from USB-D, which is the operator's own symptom.
    /// </summary>
    /// <remarks>
    /// **COMMAND `04` SAYS USB FOR BOTH** (HM-DEC-056). `26` is the only read
    /// that carries the data flag, and it is the one the bench report says had to
    /// be issued twice.
    /// </remarks>
    [Theory]
    [InlineData(0, "USB")]
    [InlineData(1, "USB-D")]
    public async Task OneReadTellsUsbFromUsbD(int dataMode, string expected)
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_074_000 };

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        radio.SetModeAndData(0x01, dataMode);
        radio.EchoOwnFramesBack = true;
        radio.VolunteerTransceiveBeforeReplying = true;

        var reads = 0;
        var answer = "";

        // One read. If this needs two, the loop below records it rather than
        // hiding it behind a retry.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            reads = attempt;

            var values = await rig.ReadAsync(RigField.DataMode, RigState.Empty);
            var data = Single(values);

            if (data.IsKnown && data.Number == dataMode)
            {
                answer = data.Text;

                break;
            }
        }

        _output.WriteLine(
            $"{expected}: correct after {reads} read(s), answer '{answer}'");

        Assert.Equal(1, reads);
    }

    /// <summary>The data-mode reading out of a read, or an unknown.</summary>
    private static RigValue Single(IReadOnlyList<RigValue> values)
    {
        foreach (var value in values)
        {
            if (value.Field == RigField.DataMode)
            {
                return value;
            }
        }

        return RigValue.Unknown(RigField.DataMode, "no reading");
    }

    /// <summary>A read that is never answered says unknown rather than guessing.</summary>
    /// <remarks>
    /// **THE OTHER HALF OF THE SAME RULE.** A reader that returns the next thing
    /// available cannot distinguish silence from a stale answer, and HM-DEC-056
    /// requires that anything unconfirmed stays unknown.
    /// </remarks>
    [Fact]
    public async Task AnUnansweredReadIsUnknownAndNotTheLastThingSeen()
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_074_000 };

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        radio.SetModeAndData(0x01, dataMode: 1);
        radio.AnswerNothing = true;

        var values = await rig.ReadAsync(RigField.DataMode, RigState.Empty);
        var data = Single(values);

        _output.WriteLine($"unanswered read: known {data.IsKnown}");

        Assert.False(data.IsKnown, "an unanswered read returned a value");
    }
}
