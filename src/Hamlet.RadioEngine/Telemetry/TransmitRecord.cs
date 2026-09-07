using Ft8Sharp.Message;
using Hamlet.RadioEngine.Transmit;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// The shape of one transmission, for the record - and nothing it said.
/// </summary>
/// <remarks>
/// <para>**HM-DEC-018, AND THE QUESTION IS ALREADY ANSWERED.** Telemetry never
/// carries a callsign, decoded message content, or anything identifying a person
/// or a contact. <c>DECISIONS.md</c> records that ruling applied to exactly this
/// case - a CW <c>CQ</c> that was transmitted - and rules that *"the length, the
/// count, the duration, the frequency and the mode make the transmission fully
/// diagnosable and identify nobody"*. That is what this record holds.</para>
/// <para>**THE SHAPE REFUSES RATHER THAN THE CALL SITE REMEMBERING.** Not one of
/// its parameters is a string. A moment, five numbers, three enumerations and
/// two flags - so <c>Ft8Transmission.Text</c> and <c>ReadsBackAs</c> have nowhere
/// to be put, not by accident, not in a hurry, and not behind a flag. It is the
/// same reasoning that gave <see cref="DecodeWindow"/> nowhere to hold decoded
/// text, and <c>ATransmitRecordCannotCarryTheMessage</c> asserts it by
/// reflection rather than by inspection.</para>
/// <para>**WHAT IT IS FOR.** A transmission that went out and got no answer, a
/// slot that started late, a run of transmissions on a frequency the operator
/// did not expect, an abort that fired: every one of those is answerable from
/// these fields, and none of them needs to know what was said.</para>
/// </remarks>
/// <param name="SlotStartUtc">The start of the slot it belongs to, in true UTC.</param>
/// <param name="StartSecondsIntoSlot">How far after that boundary the signal began.</param>
/// <param name="FrequencyHz">Where it went out.</param>
/// <param name="DurationSeconds">How long the audio was.</param>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="SampleCount">How many samples were handed to the audio path.</param>
/// <param name="MessageType">
/// Which of FT8's message types carried it - the *type*, which is a fact about
/// the format, never the message.
/// </param>
/// <param name="MessageLength">
/// **How many characters the ENCODED message was** - <c>Ft8Transmission
/// .ReadsBackAs</c>, the message as its own bits read back, which is what went on
/// the air. A count identifies nobody; the ruling names it explicitly.
/// </param>
/// <param name="CarriedHashedCallsign">
/// **Whether a callsign travelled as a 22-bit hash rather than as a callsign.**
/// A receiver can put a name to that hash only if it heard the whole callsign in
/// the same slot, so a slot carrying one hashed message decodes to nothing at
/// all - which is what happened twice on a live antenna on 2026-09-07 while the
/// line for it read <c>messageType: Standard, messageLength: 16</c>. **It is a
/// flag about the encoding and it names nobody**; which callsign was hashed is
/// exactly what it does not say.
/// </param>
/// <param name="Outcome">How the run ended.</param>
/// <param name="CameOutOfTransmit">How the radio came back to receive.</param>
/// <param name="Keyed">Whether the radio took the keying frame.</param>
public sealed record TransmitRecord(
    DateTime SlotStartUtc,
    double StartSecondsIntoSlot,
    long FrequencyHz,
    double DurationSeconds,
    int SampleRate,
    int SampleCount,
    Ft8MessageType MessageType,
    int MessageLength,
    bool CarriedHashedCallsign,
    Ft8TransmitOutcome Outcome,
    UnkeyRoute CameOutOfTransmit,
    bool Keyed)
{
    /// <summary>The event name this is written under.</summary>
    public const string EventName = "ft8_transmission";

    /// <summary>
    /// A transmission that did not go out cleanly is worth finding by scanning.
    /// </summary>
    /// <remarks>
    /// The same reasoning as <c>AWindowThatRejectedEverythingIsAWarning</c>: a
    /// normal transmission is the ordinary state of a station working somebody,
    /// and an abort, a refusal or a dead port is not.
    /// </remarks>
    public TelemetryLevel Level => Outcome == Ft8TransmitOutcome.Sent
        ? TelemetryLevel.Info
        : TelemetryLevel.Warn;

    /// <summary>The values, as telemetry carries them.</summary>
    /// <returns>Numbers, flags, and three enumeration names.</returns>
    /// <remarks>
    /// **THE THREE STRINGS COME FROM CLOSED SETS AND ARE NAMED AS SUCH.** They
    /// are the names of enumeration members declared in this repository and in
    /// <c>Ft8Sharp</c>; a message cannot be one, because a message is not a
    /// member of an enumeration. The test asserts membership rather than
    /// trusting the shape here.
    /// </remarks>
    public IReadOnlyDictionary<string, object?> ToBag()
        => new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["slotStartUtc"] = SlotStartUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            ["startSecondsIntoSlot"] = StartSecondsIntoSlot,
            ["frequencyHz"] = FrequencyHz,
            ["durationSeconds"] = DurationSeconds,
            ["sampleRate"] = SampleRate,
            ["sampleCount"] = SampleCount,
            ["messageType"] = MessageType.ToString(),
            ["messageLength"] = MessageLength,
            ["carriedHashedCallsign"] = CarriedHashedCallsign,
            ["outcome"] = Outcome.ToString(),
            ["cameOutOfTransmit"] = CameOutOfTransmit.ToString(),
            ["keyed"] = Keyed,
        };
}
