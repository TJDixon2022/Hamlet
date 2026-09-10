using System;
using System.Linq;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// Work instruction 303 task 6: **`Sent` did not mean sent.**
/// </summary>
/// <remarks>
/// <para>**THE MOST SERIOUS ITEM IN THE UNIT, AND IT IS ONE WORD.**
/// `ft8_transmission` wrote **21 records reading `outcome: Sent` and
/// `cameOutOfTransmit: OrdinaryUnkey` for transmissions that never keyed a
/// transmitter**, and those records were then read back as evidence that the send
/// path worked. **That is §0.0 broken in the one place this project treats as
/// evidence** (§0.0.1: the app's own record must be enough to tell whether the fault
/// is in the signal, the radio, or Hamlet).</para>
/// <para>**WHAT THE CODE CHECKS IS THREE LOCAL SUCCESSES.** A PTT-on frame handed to
/// the serial port without throwing, a sink that reported playing every sample, and
/// a PTT-off frame handed to the port without throwing. **Writing bytes to a COM port
/// succeeds whether or not a radio is listening at the other end.**</para>
/// <para>**SO THE WORD IS NOW `Played`**, which is what was actually established. The
/// 21 records are not deleted: they are what happened, and the fault was the
/// word.</para>
/// </remarks>
public sealed class Unit303SentMeansSentTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the outcome names are printed.</param>
    public Unit303SentMeansSentTests(ITestOutputHelper output) => _output = output;

    /// <summary>**No outcome claims the transmitter was keyed.**</summary>
    /// <remarks>
    /// **THE WORD IS THE WHOLE OF THIS TASK.** Nothing in this enum can be read as
    /// *it went on the air*, because nothing in the path that sets it asks the radio
    /// anything.
    /// </remarks>
    [Fact]
    public void NoOutcomeClaimsTheTransmitterWasKeyed()
    {
        var names = Enum.GetNames<Ft8TransmitOutcome>();

        foreach (var name in names)
        {
            _output.WriteLine("  " + name);
        }

        Assert.DoesNotContain("Sent", names);

        // **AND THE HONEST WORD IS THERE**, naming what was established.
        Assert.Contains("Played", names);
    }

    /// <summary>**The successful outcome is named for what was established.**</summary>
    [Fact]
    public void TheSuccessfulOutcomeIsNamedForWhatWasEstablished()
    {
        var run = new TransmitRun(
            Ft8TransmitOutcome.Played,
            "the audio played",
            string.Empty,
            Keyed: true,
            UnkeyedNormally: true,
            Abort: null,
            Played: null,
            SamplesOffered: 180_000,
            SecondsOffered: 12.6);

        _output.WriteLine("outcome        : " + run.Outcome);
        _output.WriteLine("audio went out : " + run.AudioWentOut);
        _output.WriteLine(
            "what that means: the PTT-on frame was written to the port, every "
            + "sample was played, and the PTT-off frame was written. Nothing here "
            + "asked the radio anything.");

        Assert.True(run.AudioWentOut);
        Assert.Equal(Ft8TransmitOutcome.Played, run.Outcome);
    }

    /// <summary>**A played transmission is still info; a failure is still worse.**</summary>
    /// <remarks>
    /// **THE LEVEL DID NOT CHANGE WITH THE WORD.** What changed is what the word
    /// asserts, not how loud the record is.
    /// </remarks>
    [Fact]
    public void TheLevelStillSeparatesAPlayedRunFromAFailedOne()
    {
        foreach (var outcome in Enum.GetValues<Ft8TransmitOutcome>())
        {
            var record = Record(outcome);

            _output.WriteLine(outcome.ToString().PadRight(22) + record.Level);
        }

        Assert.Equal(TelemetryLevel.Info, Record(Ft8TransmitOutcome.Played).Level);

        Assert.NotEqual(
            TelemetryLevel.Info, Record(Ft8TransmitOutcome.AudioFailed).Level);
    }

    private static TransmitRecord Record(Ft8TransmitOutcome outcome)
        => new(
            SlotStartUtc: new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc),
            StartSecondsIntoSlot: 0.5,
            FrequencyHz: 14_074_000,
            DurationSeconds: 12.6,
            SampleRate: 48_000,
            SampleCount: 180_000,
            MessageType: Ft8MessageType.Standard,
            MessageLength: 13,
            CarriedHashedCallsign: false,
            Outcome: outcome,
            CameOutOfTransmit: UnkeyRoute.OrdinaryUnkey,
            Keyed: true);
}
