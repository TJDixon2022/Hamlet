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
/// its parameters is a string. A moment, numbers, enumerations and flags - so
/// <c>Ft8Transmission.Text</c> and <c>ReadsBackAs</c> have nowhere to be put, not by
/// accident, not in a hurry, and not behind a flag. It is the same reasoning that gave
/// <see cref="DecodeWindow"/> nowhere to hold decoded text, and
/// <c>ATransmitRecordCannotCarryTheMessage</c> asserts it by reflection rather than by
/// inspection.</para>
/// <para>**WHAT IT IS FOR.** A transmission that went out and got no answer, a
/// slot that started late, a run of transmissions on a frequency the operator
/// did not expect, an abort that fired: every one of those is answerable from
/// these fields, and none of them needs to know what was said.</para>
/// <para>**A SEND WITH NO SLOT RECORDS NO SLOT** (`PHASE_PLAN.md` §R10; work instruction
/// 318 task 3). Its slot start, its offset, its FT8 message type and its hashed-callsign
/// flag are null and are not written at all - a slot in the record where there was none
/// would be evidence of a moment that never existed. What it writes instead is
/// **which mode went out**, how its audio measured against
/// <see cref="OperatorSend.LongestUnslottedSeconds"/>, and that cap's number.</para>
/// <para>**A SLOTTED SEND WRITES EXACTLY WHAT IT ALWAYS WROTE**, key for key and in the
/// same order, which <c>TheFt8AndFt4SendsAreByteIdenticalTests</c> pins.</para>
/// </remarks>
/// <param name="SlotStartUtc">The start of the slot it belongs to, in true UTC, or null where it had none.</param>
/// <param name="StartSecondsIntoSlot">How far after that boundary the signal began, or null where there was no slot.</param>
/// <param name="FrequencyHz">Where it went out.</param>
/// <param name="DurationSeconds">How long the audio offered to the sound card was.</param>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="SampleCount">How many samples were handed to the audio path.</param>
/// <param name="MessageType">
/// Which of FT8's message types carried it - the *type*, which is a fact about
/// the format, never the message. Null for a mode that has no FT8 message type.
/// </param>
/// <param name="MessageLength">
/// **How many characters the ENCODED message was** - <c>Ft8Transmission
/// .ReadsBackAs</c>, the message as its own bits read back, which is what went on
/// the air; for a send with no slot, the characters of its text. A count identifies
/// nobody; the ruling names it explicitly.
/// </param>
/// <param name="CarriedHashedCallsign">
/// **Whether a callsign travelled as a 22-bit hash rather than as a callsign.**
/// A receiver can put a name to that hash only if it heard the whole callsign in
/// the same slot, so a slot carrying one hashed message decodes to nothing at
/// all - which is what happened twice on a live antenna on 2026-09-07 while the
/// line for it read <c>messageType: Standard, messageLength: 16</c>. **It is a
/// flag about the encoding and it names nobody**; which callsign was hashed is
/// exactly what it does not say. Null for a mode with no hashing.
/// </param>
/// <param name="Outcome">How the run ended.</param>
/// <param name="CameOutOfTransmit">How the radio came back to receive.</param>
/// <param name="Keyed">Whether the radio took the keying frame.</param>
/// <param name="Mode">Which mode a send with no slot was in, or null for a slotted send.</param>
/// <param name="Fit">How a send with no slot measured against the cap, or null for a slotted send.</param>
/// <param name="AudioSeconds">
/// How long a send with no slot's audio was, whether or not any of it was offered - which
/// is how a refusal over the cap says by how much. Null for a slotted send.
/// </param>
public sealed record TransmitRecord(
    DateTime? SlotStartUtc,
    double? StartSecondsIntoSlot,
    long FrequencyHz,
    double DurationSeconds,
    int SampleRate,
    int SampleCount,
    Ft8MessageType? MessageType,
    int MessageLength,
    bool? CarriedHashedCallsign,
    Ft8TransmitOutcome Outcome,
    UnkeyRoute CameOutOfTransmit,
    bool Keyed,
    UnslottedMode? Mode = null,
    UnslottedFit? Fit = null,
    double? AudioSeconds = null)
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
    public TelemetryLevel Level => Outcome == Ft8TransmitOutcome.Played
        ? TelemetryLevel.Info
        : TelemetryLevel.Warn;

    /// <summary>The values, as telemetry carries them.</summary>
    /// <returns>Numbers, flags, and enumeration names.</returns>
    /// <remarks>
    /// <para>**THE STRINGS COME FROM CLOSED SETS AND ARE NAMED AS SUCH.** They
    /// are the names of enumeration members declared in this repository and in
    /// <c>Ft8Sharp</c>; a message cannot be one, because a message is not a
    /// member of an enumeration. The test asserts membership rather than
    /// trusting the shape here.</para>
    /// <para>**A FIELD THAT IS NULL IS NOT WRITTEN**, so a slotted send's line has the
    /// twelve keys it always had in the order it always had them, and a send with no slot's
    /// line has no slot key to misread.</para>
    /// </remarks>
    public IReadOnlyDictionary<string, object?> ToBag()
    {
        var bag = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (Mode is { } mode)
        {
            bag["mode"] = mode.ToString();
        }

        if (SlotStartUtc is { } slot)
        {
            bag["slotStartUtc"] = slot.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        }

        if (StartSecondsIntoSlot is { } into)
        {
            bag["startSecondsIntoSlot"] = into;
        }

        bag["frequencyHz"] = FrequencyHz;
        bag["durationSeconds"] = DurationSeconds;
        bag["sampleRate"] = SampleRate;
        bag["sampleCount"] = SampleCount;

        if (MessageType is { } type)
        {
            bag["messageType"] = type.ToString();
        }

        bag["messageLength"] = MessageLength;

        if (CarriedHashedCallsign is { } hashed)
        {
            bag["carriedHashedCallsign"] = hashed;
        }

        bag["outcome"] = Outcome.ToString();
        bag["cameOutOfTransmit"] = CameOutOfTransmit.ToString();
        bag["keyed"] = Keyed;

        if (Fit is { } fit)
        {
            bag["fit"] = fit.ToString();
        }

        if (AudioSeconds is { } audio)
        {
            bag["audioSeconds"] = audio;
        }

        if (Mode is not null)
        {
            bag["longestSeconds"] = OperatorSend.LongestUnslottedSeconds;
        }

        return bag;
    }
}
