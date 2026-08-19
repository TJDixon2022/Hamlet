using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Whether the dial is being tracked, and whether the record can say so.
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT THAT WAS NOT A MEASUREMENT.** Six sessions were
/// counted for frequency observations carrying broadcast provenance and the
/// answer was zero, which was read as the broadcast path being dead. There is no
/// broadcast provenance in the vocabulary: `DeterminedBy.From` mapped every known
/// value to `read` whatever produced it. The same count returns zero on every
/// telemetry file this project has written, including the builds that tracked the
/// dial perfectly, so it separates nothing.</para>
/// <para>These tests exist so that question is answerable by a number rather than
/// by an argument, and so the next session cannot take the absence of a word for
/// the absence of a thing.</para>
/// </remarks>
public sealed class DialTrackingTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;
    private static readonly DateTime Now = new(2026, 8, 18, 20, 0, 0, DateTimeKind.Utc);

    private static byte[] FromRadio(byte command, params byte[] data)
        => new CivFrame(Controller, Radio, command, data).ToWireBytes();

    private static byte[] Broadcast(byte command, params byte[] data)
        => new CivFrame(CivConstants.BroadcastAddress, Radio, command, data)
            .ToWireBytes();

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

    /// <remarks>
    /// Proves the record can now tell the two apart: a value the radio
    /// volunteered says `broadcast`, and one Hamlet asked for says `read`. Until
    /// this held, "broadcast by any label 0" was what a working radio produced.
    /// </remarks>
    [Fact]
    public void AVolunteeredValueAndAPolledOneAreDifferentWordsInTheFile()
    {
        var pushed = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "transceive 00");
        var asked = RigValue.Known(
            RigField.Frequency, 7_030_000, "7.030", Now, "CI-V 03");

        Assert.Equal(
            DeterminedBy.Broadcast,
            DeterminedBy.From(pushed, Now).Provenance);
        Assert.Equal(
            DeterminedBy.Read,
            DeterminedBy.From(asked, Now).Provenance);
    }

    /// <remarks>
    /// Proves the mode knob is covered by the same word, since it arrives the
    /// same way (`transceive 01`) and had the same blind spot.
    /// </remarks>
    [Fact]
    public void TheModeKnobCountsAsBroadcastToo()
    {
        var pushed = RigValue.Known(
            RigField.Mode, 3, "CW", Now, "transceive 01");

        Assert.True(pushed.IsBroadcast);
        Assert.Equal(DeterminedBy.Broadcast, DeterminedBy.From(pushed, Now).Provenance);
    }

    /// <remarks>
    /// Proves an unread value is untouched by any of this: unknown stays unknown
    /// and never becomes a mechanism (HM-DEC-050).
    /// </remarks>
    [Fact]
    public void AnUnreadValueIsNotBroadcastAndNotRead()
    {
        var value = RigState.Empty[RigField.Frequency];

        Assert.False(value.IsBroadcast);
        Assert.Equal(DeterminedBy.Unknown, DeterminedBy.From(value, Now).Provenance);
    }

    /// <remarks>
    /// Proves the wire counters: every frame is counted before any test can
    /// discard it, and the radio's own announcements are counted as themselves.
    /// This is the number that settles whether the radio broadcasts at all.
    /// </remarks>
    [Fact]
    public async Task EveryInboundFrameIsCountedBeforeAnythingFiltersIt()
    {
        var (rig, port) = await ConnectAsync();
        using var _ = rig;
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        var before = rig.Link;

        port.EnqueueIncoming(Broadcast(
            CivConstants.CmdTransceiveFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        await WaitForKnownFrequencyAsync(monitor);

        var after = rig.Link;

        Assert.True(after.Inbound > before.Inbound);
        Assert.Equal(before.InboundBroadcast + 1, after.InboundBroadcast);
        Assert.Equal(before.InboundTransceive + 1, after.InboundTransceive);
        Assert.True(after.InboundFromRadio > before.InboundFromRadio);
        Assert.NotNull(after.LastBroadcastUtc);
        Assert.True(after.IsRadioBroadcasting);
    }

    /// <remarks>
    /// Proves the honest null: a link nothing has arrived on says it does not
    /// know whether the radio broadcasts, rather than saying no. A quiet link and
    /// a silent radio are different facts (HM-DEC-050).
    /// </remarks>
    [Fact]
    public void BeforeAnythingArrivesTheAnswerIsNotKnownRatherThanNo()
    {
        Assert.Null(CivLinkHealth.Unknown.IsRadioBroadcasting);

        var quiet = CivLinkHealth.Unknown with { Inbound = 4 };
        Assert.False(quiet.IsRadioBroadcasting);
    }

    /// <remarks>
    /// <para>Proves the state the application has actually been in and nothing
    /// noticed: **the frequency maintained by the session poll alone.** The sweep
    /// HM-DEC-109 added is a backstop for a broadcast that went astray, and a
    /// backstop carrying the whole load is a defect wearing the clothes of a
    /// working feature.</para>
    /// <para>So the poll rate stays what that ruling made it, and the broadcast
    /// path is asserted separately: the radio's own announcement of the dial has
    /// a read that names the mechanism, and the value it produces says
    /// `broadcast` rather than `read`. If either half goes, this fails, and the
    /// session that removed it has to say why.</para>
    /// </remarks>
    [Fact]
    public async Task TheFrequencyIsNeverLeftToThePollAlone()
    {
        // **THE BACKSTOP IS AT LIVE RATE NOW, AND THAT IS THE REPAIR.** HM-DEC-109
        // put the frequency on the session sweep, and the operator then watched
        // Hamlet take thirty seconds to follow his own hand: a backstop carrying
        // the whole load at half-minute cadence is the failure, not the cure.
        Assert.Equal(RigPollRate.Live, RigPollPlan.RateFor(RigField.Frequency));

        // And the thing it backs up: a broadcast for the frequency is a
        // mechanism this engine knows by name.
        Assert.NotNull(CivReads.BroadcastFor(RigField.Frequency));

        var (rig, port) = await ConnectAsync();
        using var _ = rig;
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        port.EnqueueIncoming(Broadcast(
            CivConstants.CmdTransceiveFrequency, 0x00, 0x40, 0x07, 0x14, 0x00));

        await WaitForKnownFrequencyAsync(monitor);

        var value = monitor.State[RigField.Frequency];

        Assert.True(
            value.IsBroadcast,
            "the dial's own push has to reach the model as a broadcast; if this "
            + "fails the frequency is being maintained by the poll alone, which "
            + "is the state the app shipped in for two builds with nothing red");
    }

    private static async Task WaitForKnownFrequencyAsync(RigStateMonitor monitor)
    {
        for (var i = 0; i < 200; i++)
        {
            if (monitor.State[RigField.Frequency].IsKnown)
            {
                return;
            }

            await Task.Delay(10);
        }
    }
}
