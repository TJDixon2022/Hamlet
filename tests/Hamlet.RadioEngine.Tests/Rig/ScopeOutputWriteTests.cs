using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Asking the radio to send its spectrum, and being able to say afterwards which
/// rung of FACT-003's ladder the attempt died on.
/// </summary>
/// <remarks>
/// <para>**SIX CONNECTS REPORTED `noanswer` ON `27 11` AND NOBODY COULD SAY WHAT
/// THAT MEANT.** One outcome covered a radio that never spoke and a radio that
/// acknowledged the frame and then read the setting back as off — the ladder's
/// first rung and its second, arriving under one word. They are faults in
/// different places: silence is the link, disagreement is the radio declining
/// while saying yes.</para>
/// <para>**Nothing here is evidence about the radio** (HM-DEC-093). It is
/// evidence about what Hamlet does with each answer, which is what was
/// unreadable.</para>
/// </remarks>
public sealed class ScopeOutputWriteTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(FromRadio(
            CivConstants.CmdReadFrequency, 0x00, 0x30, 0x07, 0x07, 0x00));

        Assert.True(await connect);
        return (rig, port);
    }

    /// <summary>
    /// Answer the write, then answer the readback once it is actually on the
    /// wire.
    /// </summary>
    /// <remarks>
    /// **THE ANSWER IS SCRIPTED AFTER THE QUESTION IS ASKED**, which the read
    /// tests in this folder already learned the hard way: enqueuing both at once
    /// races the read loop against the second request registering, and the loop
    /// wins often enough to make a test flaky rather than wrong.
    /// </remarks>
    private static async Task AnswerAsync(
        FakeSerialPort port, params byte[] readback)
    {
        port.EnqueueIncoming(FromRadio(CivConstants.ResultOk));

        var read = new CivFrame(Radio, Controller, 0x27, new byte[] { 0x11 })
            .ToWireBytes();

        for (var i = 0; i < 200; i++)
        {
            if (Contains(port.Written, read))
            {
                break;
            }

            await Task.Delay(10);
        }

        port.EnqueueIncoming(FromRadio(0x27, readback));
    }

    private static bool Contains(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i + needle.Length <= haystack.Length; i++)
        {
            var hit = true;

            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    hit = false;
                    break;
                }
            }

            if (hit)
            {
                return true;
            }
        }

        return false;
    }

    /// <remarks>
    /// Proves HM-DEC-092: the write goes out as `27 11 01`, which is the
    /// command the manual names on p. 19-7 and the one the operator's evening
    /// reported as unanswered. Sent, so rung one of the ladder is about the
    /// answer rather than about whether Hamlet asked.
    /// </remarks>
    [Fact]
    public async Task TheCommandOnTheWireIs2711()
    {
        var (rig, port) = await ConnectAsync();

        var write = rig.SetSettingAsync(CivWrites.ScopeOutput, 1);

        await AnswerAsync(port, 0x11, 0x01);
        await write;

        var wire = port.Written;
        var sent = new CivFrame(Radio, Controller, 0x27, new byte[] { 0x11, 0x01 })
            .ToWireBytes();

        Assert.Contains(
            string.Join(" ", sent.Select(b => b.ToString("X2"))),
            string.Join(" ", wire.Select(b => b.ToString("X2"))),
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves FACT-003 rung one: silence is `no_answer`, and it says so in the
    /// token as well as in the state.
    /// </remarks>
    [Fact]
    public async Task SilenceIsNoAnswer()
    {
        var (rig, port) = await ConnectAsync();

        // Nothing is enqueued: the radio never speaks.
        var result = await rig.SetSettingAsync(CivWrites.ScopeOutput, 1);

        Assert.Equal(RigWriteOutcome.NoAnswer, result.Outcome);
        Assert.Equal("no_answer", result.Reason);
        Assert.False(result.Worked);
    }

    /// <remarks>
    /// Proves FACT-003 rung two: an acknowledgement followed by a readback that
    /// still says off is its own outcome, distinct from silence. This is the
    /// case that used to be indistinguishable, and it is the one that says the
    /// radio is refusing while answering politely.
    /// </remarks>
    [Fact]
    public async Task AnAcknowledgementWithTheSettingStillOffIsItsOwnFault()
    {
        var (rig, port) = await ConnectAsync();

        var write = rig.SetSettingAsync(CivWrites.ScopeOutput, 1);

        await AnswerAsync(port, 0x11, 0x00);
        var result = await write;

        Assert.Equal(RigWriteOutcome.ReadBackDisagreed, result.Outcome);
        Assert.Equal("read_back_disagreed", result.Reason);
        Assert.False(result.Worked);
        Assert.NotEqual("no_answer", result.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-084: a write is confirmed by reading it back rather than by
    /// the acknowledgement, so the confirmed case needs both.
    /// </remarks>
    [Fact]
    public async Task ConfirmedNeedsTheReadbackToAgree()
    {
        var (rig, port) = await ConnectAsync();

        var write = rig.SetSettingAsync(CivWrites.ScopeOutput, 1);

        await AnswerAsync(port, 0x11, 0x01);
        var result = await write;

        Assert.True(result.Worked);
        Assert.Equal("confirmed", result.Reason);
    }

    /// <remarks>
    /// Proves HM-DEC-077: every write outcome has its own token, so counting
    /// failures by cause across sessions means something.
    /// </remarks>
    [Fact]
    public void NoTwoWriteOutcomesShareAToken()
    {
        var tokens = Enum.GetValues<RigWriteOutcome>()
            .Select(o => new RigWriteResult(o, "", "27 11").Reason)
            .ToList();

        Assert.Equal(tokens.Count, tokens.Distinct().Count());
    }
}
