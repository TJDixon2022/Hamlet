using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Keying the transmitter, and every rule that stands in front of it
/// (HM-DEC-059, §0.2).
/// </summary>
/// <remarks>
/// This is the feature the whole app has been walking toward and it is the one
/// where a mistake puts a signal on the air. So the decision logic is pure and
/// proved without a radio, the way <c>ReconnectPlan</c> already is, and the
/// frames are proved against canned byte sequences (HM-DEC-007).
/// </remarks>
public sealed class CwTransmitTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 19, 0, 0, DateTimeKind.Utc);

    private static RigState ReadyState(int breakIn = 1, int mode = (int)CivMode.Cw)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, mode, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, breakIn, breakIn == 0 ? "off" : "semi",
                Now, "CI-V 16 47"),
        });

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    private static TransmitContext Context(
        RigState? state = null,
        LicenseClass cls = LicenseClass.General,
        long hz = 7_030_000,
        bool guard = true,
        bool connected = true,
        RigCapabilities? capabilities = null)
        => new(cls, hz, guard, connected, capabilities ?? Radio, state ?? ReadyState());

    // ---- The message the radio will take -------------------------------

    /// <remarks>
    /// THIRTY CHARACTERS IS THE LIMIT (Full Manual p. 19-11), and the UI never
    /// presents a message it cannot actually send. A longer one splits in the
    /// engine, at the spaces, so a callsign is never cut in half.
    /// </remarks>
    [Fact]
    public void AMessageOverThirtyCharactersSplitsAtTheSpaces()
    {
        const string long1 = "CQ CQ CQ DE KC3QIS KC3QIS KC3QIS K";

        var pieces = CwMessage.Split(long1);

        Assert.True(pieces.Count > 1, "it should not have fitted in one");
        Assert.All(pieces, p => Assert.True(
            p.Length <= CwMessage.MaximumLength, $"'{p}' is {p.Length} characters"));

        // Every word survives whole, and in order.
        Assert.Equal(long1, string.Join(" ", pieces));
        Assert.DoesNotContain(pieces, p => p.StartsWith(' ') || p.EndsWith(' '));
    }

    /// <remarks>
    /// Proves a message that fits is one piece and is not touched, since the
    /// ordinary case must not pay for the long one.
    /// </remarks>
    [Fact]
    public void AMessageThatFitsIsSentAsOnePiece()
    {
        var pieces = CwMessage.Split("W1ABC DE KC3QIS K");

        Assert.Single(pieces);
        Assert.Equal("W1ABC DE KC3QIS K", pieces[0]);
    }

    /// <remarks>
    /// Proves the character set is the manual's own (p. 19-11). A character the
    /// radio cannot send is dropped rather than substituted, because a message
    /// that quietly became a different message is the prime directive broken on
    /// the way out.
    /// </remarks>
    [Fact]
    public void CharactersTheKeyerCannotSendAreDroppedRatherThanSubstituted()
    {
        Assert.Equal("CQ DE KC3QIS", CwMessage.Clean("cq  de  kc3qis"));

        // The tilde and the hash are simply gone, and nothing stands in for
        // them: a substitute would change the message on its way out.
        Assert.Equal("HELLO", CwMessage.Clean("h~e#llo"));
        Assert.Equal("KC3QIS", CwMessage.Clean("  kc3qis  "));

        Assert.False(CwMessage.IsSendable("CQ DE KC3QIS!"));
        Assert.True(CwMessage.IsSendable("CQ DE KC3QIS/P"));

        // The punctuation the manual lists, all of it sendable.
        Assert.True(CwMessage.IsSendable("/?.-,:'()=+\"@"));
    }

    // ---- The guard, which is absolute ---------------------------------

    /// <remarks>
    /// THE GUARD IS CALLED BEFORE EVERY KEY (HM-DEC-029, §0.2). This proves it
    /// structurally rather than by inspection: a sender that records every call
    /// is handed a frequency outside the operator's privileges, and it is never
    /// reached at all.
    /// </remarks>
    [Fact]
    public async Task NothingIsKeyedWhereTheLicenseDoesNotReach()
    {
        var sender = new RecordingSender();
        var transmitter = new CwTransmitter(sender);

        // 14.100 MHz is inside 20 m and outside a Technician's privileges.
        var outcome = await transmitter.SendAsync(
            "CQ DE KC3QIS K",
            Context(cls: LicenseClass.Technician, hz: 14_100_000));

        Assert.False(outcome.Sent);
        Assert.NotEmpty(outcome.Detail);
        Assert.NotEmpty(outcome.Citation);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// Proves the guard is consulted before the radio is, which is the ordering
    /// that matters: the question with legal consequences is answered first, and
    /// a refusal never reaches the port at all.
    /// </remarks>
    [Fact]
    public void TheCheckAnswersWithoutTouchingTheRadio()
    {
        var sender = new RecordingSender();
        var transmitter = new CwTransmitter(sender);

        var refused = transmitter.Check(
            Context(cls: LicenseClass.Technician, hz: 14_100_000));

        Assert.False(refused.Sent);
        Assert.Empty(sender.Sent);
        Assert.Equal(0, sender.Aborts);
    }

    /// <remarks>
    /// Proves the operator who switched the guard off is obeyed and the fact is
    /// recorded, which is HM-DEC-029's own design: they hold the license and
    /// Hamlet says what it thinks rather than refusing.
    /// </remarks>
    [Fact]
    public async Task WithTheGuardOffTheOperatorsOwnAuthorityCarriesIt()
    {
        var sender = new RecordingSender();
        var transmitter = new CwTransmitter(sender);

        var outcome = await transmitter.SendAsync(
            "CQ DE KC3QIS K",
            Context(cls: LicenseClass.Technician, hz: 14_100_000, guard: false));

        Assert.True(outcome.Sent);
        Assert.True(outcome.GuardOverridden);
        Assert.Single(sender.Sent);
    }

    // ---- The precondition nobody had written down ---------------------

    /// <remarks>
    /// THE BREAK-IN PRECONDITION IS CHECKED AND REPORTED RATHER THAN ASSUMED
    /// (HM-DEC-049, p. 19-7 footnote 2). With break-in off, a message sent with
    /// command 17 is accepted and never transmitted, so Hamlet would otherwise
    /// send a correct frame, get a correct acknowledgement, and report a success
    /// that never left the antenna.
    /// </remarks>
    [Fact]
    public async Task WithBreakInOffItSaysSoRatherThanSendingIntoSilence()
    {
        var sender = new RecordingSender();
        var transmitter = new CwTransmitter(sender);

        var outcome = await transmitter.SendAsync(
            "CQ DE KC3QIS K", Context(state: ReadyState(breakIn: 0)));

        Assert.False(outcome.Sent);
        Assert.Contains("Break-in is off", outcome.Detail, StringComparison.Ordinal);
        Assert.Contains("19-7", outcome.Citation, StringComparison.Ordinal);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// AN UNREAD SETTING IS NOT PERMISSION. Hamlet reads break-in on connect, and
    /// until it has, "I do not know whether this will go out" is a different
    /// answer from "it will" and only one of them is honest (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnreadBreakInSettingRefusesRatherThanAssuming()
    {
        var noBreakIn = RigState.Empty.With(
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"));

        var readiness = TransmitReadiness.Check(true, Radio, noBreakIn);

        Assert.False(readiness.MaySend);
        Assert.Contains("has not read", readiness.Detail, StringComparison.Ordinal);

        // AND IT IS ITS OWN STATE NOW (HM-DEC-077). Unread and off used to
        // produce one verdict, so neither the file nor the screen could tell
        // them apart, and they call for completely different things: waiting,
        // against walking over to the radio.
        Assert.Equal(CwReadyState.BreakInUnknown, readiness.State);
        Assert.Equal("break_in_unknown", readiness.Reason);

        var off = TransmitReadiness.Check(
            true, Radio,
            noBreakIn.With(
                RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47")));

        Assert.Equal(CwReadyState.BreakInOff, off.State);
        Assert.NotEqual(readiness.Detail, off.Detail);
    }

    /// <remarks>
    /// Proves the other preconditions answer too, each in its own words, because
    /// "nothing happened" is useless and "the radio is in USB rather than Morse"
    /// is something somebody can act on.
    /// </remarks>
    [Theory]
    [InlineData(false, true, CwReadyState.NotConnected)]
    [InlineData(true, false, CwReadyState.RadioCannotTransmit)]
    public void EveryReasonItCannotSendIsNamedAsItself(
        bool connected, bool canTransmit, CwReadyState expected)
    {
        var capabilities = canTransmit
            ? Radio
            : Radio with { CanTransmit = false };

        var readiness = TransmitReadiness.Check(
            connected, connected ? capabilities : null, ReadyState());

        Assert.Equal(expected, readiness.State);
        Assert.False(readiness.MaySend);
        Assert.NotEmpty(readiness.Detail);
    }

    /// <remarks>
    /// Proves the radio being in the wrong mode is caught before the send rather
    /// than after. The keyer only sends Morse, and a message handed to it in
    /// sideband goes nowhere.
    /// </remarks>
    [Fact]
    public void SendingWhileTheRadioIsInSidebandIsRefusedAndExplained()
    {
        var readiness = TransmitReadiness.Check(
            true, Radio, ReadyState(mode: (int)CivMode.Usb));

        Assert.Equal(CwReadyState.NotInMorse, readiness.State);
        Assert.Contains("Morse", readiness.Detail, StringComparison.Ordinal);
    }

    // ---- The abort ----------------------------------------------------

    /// <remarks>
    /// THE ABORT WORKS MID-SEND AND DOES NOT DEPEND ON THE SEND LOOP NOTICING
    /// ANYTHING (§0.2). It is a same-thread call that returns nothing, because
    /// the moment somebody wants a transmitter to stop is the moment they cannot
    /// wait for a task to be scheduled.
    /// </remarks>
    [Fact]
    public async Task TheAbortStopsASendAlreadyInFlight()
    {
        var rig = new BlockingRig();
        var sender = new KeyerCwSender(rig);

        // Four pieces, so there are boundaries to stop at.
        var send = sender.SendAsync(
            "CQ CQ CQ DE KC3QIS KC3QIS K PSE AGN QRS TU 73 SK DE KC3QIS");

        await rig.WaitForFirstSend();
        sender.Abort();
        rig.Release();

        var result = await send;

        Assert.Equal(CwSendOutcome.Aborted, result.Outcome);
        Assert.True(result.PiecesSent < result.PiecesTotal);
        Assert.Equal(1, rig.Aborts);
    }

    /// <remarks>
    /// Proves the abort is safe when nothing is sending and safe twice. An abort
    /// that could fail is not an abort, and it is the one control somebody
    /// reaches for when something has already gone wrong.
    /// </remarks>
    [Fact]
    public void TheAbortIsSafeWhenThereIsNothingToStop()
    {
        var rig = new BlockingRig();
        var sender = new KeyerCwSender(rig);

        sender.Abort();
        sender.Abort();

        Assert.Equal(2, rig.Aborts);
    }

    /// <remarks>
    /// THE STOP ON THE WIRE. Command 17 carrying FF, which the manual states as
    /// a message rather than a sub-command: "FF stops sending CW messages"
    /// (p. 19-11).
    /// </remarks>
    [Fact]
    public async Task TheStopFrameIsCommand17CarryingFf()
    {
        var port = new FakeSerialPort();
        using var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(new CivFrame(
            CivConstants.DefaultControllerAddress, CivConstants.DefaultRadioAddress,
            CivConstants.CmdReadFrequency,
            new byte[] { 0x00, 0x30, 0x07, 0x07, 0x00 }).ToWireBytes());

        Assert.True(await connect);

        var sent = new List<CivFrame>();
        rig.FrameTrace += (outgoing, frame) =>
        {
            if (outgoing)
            {
                sent.Add(frame);
            }
        };

        rig.AbortCw();

        var frame = Assert.Single(sent);
        Assert.Equal(CivConstants.CmdSendCwMessage, frame.Command);
        Assert.Equal(new byte[] { 0xFF }, frame.Data);
    }

    // ---- Nothing unattended -------------------------------------------

    /// <remarks>
    /// NOTHING TRANSMITS UNATTENDED (§0.2). Proved structurally: the transmitter
    /// has no timer, no retry and no loop, so there is nothing in it that could
    /// key without being asked. A send happens because a person pressed
    /// something.
    /// </remarks>
    [Fact]
    public void TheTransmitterHasNoWayToKeyWithoutBeingAsked()
    {
        var type = typeof(CwTransmitter);

        var fields = type.GetFields(
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.NonPublic);

        Assert.All(fields, f => Assert.DoesNotContain(
            "Timer", f.FieldType.Name, StringComparison.OrdinalIgnoreCase));

        // And it raises nothing, so nothing can subscribe itself to a send.
        Assert.Empty(type.GetEvents());
    }

    /// <remarks>
    /// Proves a failed send is not repeated. Retrying a transmission on the
    /// operator's behalf is unattended transmission by another name, and the
    /// message says so rather than leaving them wondering.
    /// </remarks>
    [Fact]
    public async Task ASendTheRadioDidNotTakeIsNotRepeated()
    {
        var rig = new RefusingRig();
        var sender = new KeyerCwSender(rig);

        var result = await sender.SendAsync("CQ DE KC3QIS K");

        Assert.Equal(CwSendOutcome.NoAnswer, result.Outcome);
        Assert.Equal(1, rig.Attempts);
        Assert.Contains(
            "Nothing is repeated automatically", result.Detail, StringComparison.Ordinal);
    }

    // ---- Farnsworth, declared rather than hidden ------------------------

    /// <remarks>
    /// FARNSWORTH IS AN EXPLICIT KNOWN-UNKNOWN, NOT A HIDDEN ABSENCE
    /// (HM-DEC-059). The radio's CW-KEY SET menu offers dot/dash ratio, rise
    /// time, paddle polarity and key type, and nothing for the gaps between
    /// characters (Full Manual p. 4-21). The keyer path therefore cannot do it,
    /// says so, and the UI reads this rather than offering a control that
    /// silently does nothing.
    /// </remarks>
    [Fact]
    public void TheKeyerPathSaysItCannotWidenCharacterSpacing()
    {
        var sender = new KeyerCwSender(new BlockingRig());
        var transmitter = new CwTransmitter(sender);

        Assert.False(sender.SupportsCharacterSpacing);
        Assert.False(transmitter.SupportsCharacterSpacing);
        Assert.Equal(CwMessage.MaximumLength, sender.MaximumMessageLength);
        Assert.NotEmpty(sender.PathName);
    }

    /// <remarks>
    /// Proves the training radio refuses rather than pretending. A practice radio
    /// that answered "sent" would teach somebody their first call went out when
    /// nothing left the house (HM-DEC-026, HM-DEC-030).
    /// </remarks>
    [Fact]
    public async Task TheTrainingRadioRefusesToSend()
    {
        var sender = new KeyerCwSender(new TrainingRig());

        var result = await sender.SendAsync("CQ DE KC3QIS K");

        Assert.Equal(CwSendOutcome.NotSupported, result.Outcome);
        Assert.NotEmpty(result.Detail);
    }

    // ---- Fakes ---------------------------------------------------------

    /// <summary>Records what it was asked to send and never keys anything.</summary>
    private sealed class RecordingSender : ICwSender
    {
        public List<string> Sent { get; } = new();

        public int Aborts { get; private set; }

        public bool SupportsCharacterSpacing => false;

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public string PathName => "recording";

        public Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.FromResult(new CwSendResult(CwSendOutcome.Sent, "", 1, 1));
        }

        public void Abort() => Aborts++;
    }

    /// <summary>A rig whose first send blocks until it is released.</summary>
    private sealed class BlockingRig : IRig
    {
        private readonly TaskCompletionSource _firstSend = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TaskCompletionSource _release = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public int Aborts { get; private set; }

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "Blocking radio", false, true, false, true, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task WaitForFirstSend() => _firstSend.Task;

        public void Release() => _release.TrySetResult();

        public async Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
        {
            _firstSend.TrySetResult();
            await _release.Task.ConfigureAwait(false);
            return true;
        }

        public void AbortCw() => Aborts++;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(7_030_000L);

        public Task SetFrequencyHzAsync(long hz, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(new[] { RigValue.Unknown(field) });

        public Task<RigWriteResult> SetSettingAsync(

                Hamlet.RadioEngine.Civ.CivWrite write, int value,

                CancellationToken cancellationToken = default)

                => Task.FromResult(RigWriteResult.NotSupported("test rig"));
        public Task<RigWriteResult> SetModeAsync(
        CivMode mode, bool dataMode, byte? filterSlot, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("blocking radio"));

        private void Unused()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
        }
    }

    /// <summary>A rig that takes nothing and counts how often it was asked.</summary>
    private sealed class RefusingRig : IRig
    {
        public int Attempts { get; private set; }

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "Refusing radio", false, true, false, true, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
        {
            Attempts++;
            return Task.FromResult(false);
        }

        public void AbortCw()
        {
        }

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(7_030_000L);

        public Task SetFrequencyHzAsync(long hz, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(new[] { RigValue.Unknown(field) });

        public Task<RigWriteResult> SetSettingAsync(

                Hamlet.RadioEngine.Civ.CivWrite write, int value,

                CancellationToken cancellationToken = default)

                => Task.FromResult(RigWriteResult.NotSupported("test rig"));
        public Task<RigWriteResult> SetModeAsync(
        CivMode mode, bool dataMode, byte? filterSlot, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("refusing radio"));

        private void Unused()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
        }
    }
}
