using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Transmit;

/// <summary>What became of one attempt to put a transmission on the air.</summary>
public enum Ft8TransmitOutcome
{
    /// <summary>
    /// **Every sample was played to the named sound card and the keying frames were
    /// written to the port.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS WAS CALLED `Sent` AND THAT WORD WAS FALSE** (work instruction 303
    /// task 6). It said *the whole signal went out and the radio came back to
    /// receive*, and **`ft8_transmission` wrote 21 records reading `Sent` with
    /// `cameOutOfTransmit: OrdinaryUnkey` for transmissions that never keyed a
    /// transmitter.** Those records were then read back as evidence that the path
    /// worked, which is §0.0 broken in the one place this project treats as
    /// evidence.</para>
    /// <para>**WHAT THE CODE ACTUALLY CHECKS, AND IT IS ALL IT CHECKS.** A CI-V
    /// PTT-on frame was handed to the serial port without throwing; the sink reported
    /// playing every sample it was given; a PTT-off frame was handed to the port
    /// without throwing. **That is three local successes and not one fact about the
    /// radio.**</para>
    /// <para>**WHAT IT CANNOT TELL YOU.** Writing bytes to a COM port succeeds
    /// whether or not a radio is listening at the other end, so nothing here knows
    /// the frame was understood, that the transmitter keyed, that any power was made,
    /// or that the audio reached the radio rather than a laptop speaker.</para>
    /// <para>**HAMLET COULD KNOW SOME OF IT AND THIS PATH DOES NOT ASK.** The radio
    /// reports transmit state on `1C 00`, which HM-DEC-147 already polls four times a
    /// second, and power on `15 11`, which HM-DEC-082 already reads as *power made*.
    /// **Joining those to this outcome is a real repair and it is not this unit's**:
    /// the instruction scopes task 6 to the word.</para>
    /// </remarks>
    Played,

    /// <summary>The licence gate did not positively permit it. Nothing keyed.</summary>
    RefusedByLicence,

    /// <summary>
    /// The request could not be sent as asked - no audio, or it would not fit in
    /// the slot from the moment it was to start. Nothing keyed.
    /// </summary>
    RefusedAsUnsendable,

    /// <summary>The sink threw, or returned having played less than it was given.</summary>
    AudioFailed,

    /// <summary>Somebody cancelled it while it was going out.</summary>
    Cancelled,

    /// <summary>A write to the radio threw - going in, or on the way out.</summary>
    PortFailed,
}

/// <summary>How the radio came out of transmit, which is never left implied.</summary>
/// <remarks>
/// **A NORMAL UNKEY IS NOT AN ABORT.** They are different events with different
/// meanings for the operator and for anyone reading the record afterwards: one
/// says a transmission finished, the other says something went wrong while the
/// transmitter was on. A result that could not tell them apart would make every
/// successful transmission look like a fault.
/// </remarks>
public enum UnkeyRoute
{
    /// <summary>Nothing ever keyed, so nothing had to come back.</summary>
    NothingWasKeyed,

    /// <summary>The ordinary <c>1C 00 00</c> write, on the path where all was well.</summary>
    OrdinaryUnkey,

    /// <summary>
    /// <see cref="TransmitAbort.Fire"/>, and at least one of its two frames
    /// reached the radio.
    /// </summary>
    TheAbort,

    /// <summary>
    /// The abort fired and neither frame got out. **Reported as what it is**: the
    /// radio may still be transmitting and nothing in software can say otherwise.
    /// </summary>
    NothingReachedTheRadio,
}

/// <summary>
/// The operator's intent, whole, in one value.
/// </summary>
/// <remarks>
/// <para>**THIS TYPE IS HOW THE ONE-CLICK RULE IS ENFORCED BY SHAPE** (§0.2).
/// <see cref="Ft8TransmitSequence"/> has no method that starts a transmission
/// without being handed one of these, so there is no entry point a scheduler,
/// a decode or a contact state machine could reach that does not begin with
/// somebody having asked for exactly this message, at exactly this frequency,
/// in exactly this slot.</para>
/// <para>**THE MOMENT COMES IN RATHER THAN BEING READ.** The sequence never asks
/// what time it is. A path that could read a clock could decide for itself that
/// the moment had come, and that is the fault this phase cannot survive.</para>
/// <para>**A SEND CARRIES A SLOT, OR IT CARRIES NOW** (`PHASE_PLAN.md` §R10, Tim,
/// 2026-09-11; work instruction 318 task 3). The constructor every FT8 and FT4 caller
/// has used since unit 253 makes a slotted send, exactly as it always did.
/// <see cref="Now"/> makes a send with no slot, for a mode whose transmission lasts as
/// long as its text: <see cref="HasSlot"/> is false, its audio is an
/// <see cref="UnslottedTransmission"/>, and **there is no slot start, no offset and no
/// grid to read** - not a default <see cref="DateTime"/> standing in for a slot, which the
/// evidence record would then carry as a moment that never existed. Asking a send with no
/// slot for any of those three, or for an FT8 <see cref="Transmission"/>, throws, so an
/// FT8-only assumption that meets a PSK31 send fails where it stands instead of inventing
/// a value.</para>
/// <para>**AND NOW IS HANDED IN TOO, NEVER READ.** A send with no slot is fired by the
/// operator's own action through <see cref="Ft8ArmedSend.NowAsync"/>, not by anything that
/// looks at a clock.</para>
/// </remarks>
public sealed record OperatorSend
{
    /// <summary>**The longest a send with no slot may be: thirty seconds.**</summary>
    /// <remarks>
    /// <para>**WHY A CAP AT ALL** (§R10). A slotted send is held to its slot and cannot run
    /// past the boundary. A send with no slot has nothing to hold it, and PSK31 is a
    /// continuous carrier at full duty (§3.3), so without a number here a composing fault
    /// or a runaway text would be a carrier that simply stays on.</para>
    /// <para>**WHY THIRTY.** It is the most §R10 allows, and every macro §R2 rules fits under
    /// it with its idle: measured on work instruction 318's bench at 48 000 Hz, CQ is 11.46 s,
    /// the answer 7.84 s, the confirmation 16.26 s and the report - the longest - 24.80 s to
    /// W1AW and **28.19 s to VP2V/W1AW**. Anything shorter than about 28.2 s would refuse the
    /// report to a compound callsign, which is a contact §R2 has to be able to make; the
    /// 1.8 s left over is all the room a longer name or place in Settings gets, and a report
    /// longer than that is refused with its length rather than cut short.</para>
    /// </remarks>
    public const double LongestUnslottedSeconds = 30;

    private readonly Ft8Transmission? _transmission;
    private readonly UnslottedTransmission? _unslotted;
    private readonly DateTime _slotStartUtc;
    private readonly double _startSecondsIntoSlot;
    private readonly SlotGrid _grid = SlotGrid.Ft8;

    /// <summary>A send in a slot - the one every FT8 and FT4 caller makes.</summary>
    /// <param name="Transmission">The audio, and what it says.</param>
    /// <param name="FrequencyHz">Where it would go out - what the gate is asked about.</param>
    /// <param name="LicenseClass">The operator's class, as Settings holds it.</param>
    /// <param name="GuardEnabled">
    /// The operator's "only let me transmit where my licence allows" setting. **On
    /// this path, false is a refusal** - see <see cref="Ft8TransmitSequence"/>.
    /// </param>
    /// <param name="SlotStartUtc">
    /// The start of the slot this belongs to, in true UTC. Recorded, not waited for.
    /// </param>
    /// <param name="StartSecondsIntoSlot">
    /// How far after the boundary the signal begins. The sequence checks that the
    /// whole transmission still fits and records the figure; **it does not wait for
    /// the moment to arrive**, because waiting is watching a clock.
    /// </param>
    /// <remarks>
    /// **THE PARAMETER NAMES ARE THE ONES THE POSITIONAL RECORD HAD**, so every caller that
    /// named its arguments still compiles and still means the same thing.
    /// </remarks>
    public OperatorSend(
        Ft8Transmission Transmission,
        long FrequencyHz,
        LicenseClass LicenseClass,
        bool GuardEnabled,
        DateTime SlotStartUtc,
        double StartSecondsIntoSlot)
    {
        _transmission = Transmission;
        this.FrequencyHz = FrequencyHz;
        this.LicenseClass = LicenseClass;
        this.GuardEnabled = GuardEnabled;
        _slotStartUtc = SlotStartUtc;
        _startSecondsIntoSlot = StartSecondsIntoSlot;
        HasSlot = true;
    }

    private OperatorSend(
        UnslottedTransmission unslotted, long frequencyHz, LicenseClass licenseClass, bool guardEnabled)
    {
        _unslotted = unslotted;
        FrequencyHz = frequencyHz;
        LicenseClass = licenseClass;
        GuardEnabled = guardEnabled;
    }

    /// <summary>**A send with no slot**, to be fired by the operator's action rather than a boundary.</summary>
    /// <param name="transmission">The audio, and the shape of what it says.</param>
    /// <param name="frequencyHz">Where it would go out - what the gate is asked about.</param>
    /// <param name="licenseClass">The operator's class, as Settings holds it.</param>
    /// <param name="guardEnabled">The licence guard setting. **False is a refusal on this path.**</param>
    /// <returns>A send whose <see cref="HasSlot"/> is false.</returns>
    /// <exception cref="ArgumentNullException">There is no transmission.</exception>
    public static OperatorSend Now(
        UnslottedTransmission transmission, long frequencyHz, LicenseClass licenseClass, bool guardEnabled)
    {
        ArgumentNullException.ThrowIfNull(transmission);

        return new OperatorSend(transmission, frequencyHz, licenseClass, guardEnabled);
    }

    /// <summary>True for a send in a slot; false for a send fired now.</summary>
    public bool HasSlot { get; }

    /// <summary>The FT8 or FT4 audio, and what it says.</summary>
    /// <exception cref="InvalidOperationException">This send has no slot, and so no FT8 transmission.</exception>
    public Ft8Transmission Transmission => HasSlot ? _transmission! : throw NoSlot(nameof(Transmission));

    /// <summary>The audio of a send with no slot, or null for a slotted send.</summary>
    public UnslottedTransmission? Unslotted => _unslotted;

    /// <summary>Where it would go out - what the gate is asked about.</summary>
    public long FrequencyHz { get; }

    /// <summary>The operator's class, as Settings holds it.</summary>
    public LicenseClass LicenseClass { get; }

    /// <summary>The licence guard setting. **On this path, false is a refusal.**</summary>
    public bool GuardEnabled { get; }

    /// <summary>The start of the slot this belongs to, in true UTC.</summary>
    /// <exception cref="InvalidOperationException">This send has no slot.</exception>
    public DateTime SlotStartUtc => HasSlot ? _slotStartUtc : throw NoSlot(nameof(SlotStartUtc));

    /// <summary>How far after the boundary the signal begins.</summary>
    /// <exception cref="InvalidOperationException">This send has no slot.</exception>
    public double StartSecondsIntoSlot
        => HasSlot ? _startSecondsIntoSlot : throw NoSlot(nameof(StartSecondsIntoSlot));

    /// <summary>The samples the sink is handed, whichever kind of send this is.</summary>
    public float[] Samples => HasSlot ? Transmission.Samples : _unslotted!.Samples;

    /// <summary>Their rate, whichever kind of send this is.</summary>
    public int SampleRate => HasSlot ? Transmission.SampleRate : _unslotted!.SampleRate;

    /// <summary>
    /// **Which grid the slot above is a slot on.**
    /// </summary>
    /// <remarks>
    /// <para>**IT BELONGS ON THIS RECORD AND NOT ON THE SEQUENCE** (work
    /// instruction 293 task 3). <see cref="Ft8TransmitSequence"/> is built once,
    /// when the radio connects, and outlives any number of presses of the mode
    /// strip; a grid held there would be a second place the operator's choice
    /// lives, able to disagree with the audio in the very record it was being
    /// asked about. **The grid is part of what he asked for** - this message, at
    /// this frequency, in this slot, on this grid - and the whole reason this type
    /// exists is that his intent travels as one value.</para>
    /// <para>**FT8'S GRID IS THE DEFAULT, WHICH IS THE STATUS QUO RATHER THAN A
    /// CLAIM.** Every caller that existed before this property got FT8's fifteen
    /// seconds and still gets them, which is the same rule
    /// <c>DigitalModes.Grid()</c> already states for anything that is not FT4. It
    /// is an <c>init</c> property for the reason <c>Ft8SlotWatch.Grid</c> is one:
    /// a grid that could change after the send was armed would leave the boundary
    /// it was armed for and the boundary it is measured against on two different
    /// grids.</para>
    /// <para>**AND IT CARRIES NO CLOCK AND NO SCHEDULE.** A <see cref="SlotGrid"/>
    /// is two numbers and a name; nothing on it waits, fires or decides that a
    /// moment has come.</para>
    /// <para>**A SEND WITH NO SLOT HAS NO GRID** (work instruction 318 task 3), and
    /// asking one for it throws rather than answering FT8's.</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">This send has no slot.</exception>
    public SlotGrid Grid
    {
        get => HasSlot ? _grid : throw NoSlot(nameof(Grid));
        init => _grid = value;
    }

    /// <summary>What asking a send with no slot about a slot says.</summary>
    private InvalidOperationException NoSlot(string member)
        => new($"this send has no slot, so it has no {member}: it is {_unslotted?.Mode} audio "
            + "fired by the operator's own action, and nothing about a slot applies to it.");

    /// <summary>The record's text form, which never asks a send with no slot about a slot.</summary>
    /// <remarks>
    /// **A SLOTTED SEND PRINTS THE MEMBERS THE POSITIONAL RECORD PRINTED**, in the same order.
    /// </remarks>
    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        if (HasSlot)
        {
            builder.Append("Transmission = ").Append(_transmission)
                .Append(", FrequencyHz = ").Append(FrequencyHz)
                .Append(", LicenseClass = ").Append(LicenseClass)
                .Append(", GuardEnabled = ").Append(GuardEnabled)
                .Append(", SlotStartUtc = ").Append(_slotStartUtc)
                .Append(", StartSecondsIntoSlot = ").Append(_startSecondsIntoSlot)
                .Append(", Grid = ").Append(_grid);
        }
        else
        {
            builder.Append("Unslotted = ").Append(_unslotted)
                .Append(", FrequencyHz = ").Append(FrequencyHz)
                .Append(", LicenseClass = ").Append(LicenseClass)
                .Append(", GuardEnabled = ").Append(GuardEnabled)
                .Append(", HasSlot = False");
        }

        return true;
    }
}

/// <summary>What one run of the sequence did, in full.</summary>
/// <param name="Outcome">Which of the six ways it ended.</param>
/// <param name="Reason">Why, in the operator's words. Empty on success.</param>
/// <param name="Citation">
/// The paragraph behind a licence refusal, straight from the gate. Empty
/// otherwise.
/// </param>
/// <param name="Keyed">True where the port took the keying frame.</param>
/// <param name="UnkeyedNormally">True where the ordinary unkey write was taken.</param>
/// <param name="Abort">
/// What <see cref="TransmitAbort.Fire"/> did, or null where it was not needed.
/// **Carried rather than swallowed**: the abort's own remarks require every
/// caller from step 3 onward to log it, and a silent abort cannot be diagnosed.
/// </param>
/// <param name="Played">What the sink said it did, or null if it was never reached.</param>
/// <param name="SamplesOffered">How many samples were handed to the sink.</param>
/// <param name="SecondsOffered">How long that is, at the transmission's own rate.</param>
public sealed record TransmitRun(
    Ft8TransmitOutcome Outcome,
    string Reason,
    string Citation,
    bool Keyed,
    bool UnkeyedNormally,
    AbortRecord? Abort,
    PlayedAudio? Played,
    int SamplesOffered,
    double SecondsOffered)
{
    /// <summary>True only where the whole transmission went out and the radio unkeyed.</summary>
    /// <remarks>
    /// **RENAMED FROM `Sent` BY WORK INSTRUCTION 303**, because it never meant sent.
    /// See <see cref="Ft8TransmitOutcome.Played"/> for what it does mean and for the
    /// 21 records that were read back as evidence of a working path.
    /// </remarks>
    public bool AudioWentOut => Outcome == Ft8TransmitOutcome.Played;

    /// <summary>How the radio came back to receive.</summary>
    public UnkeyRoute CameOutOfTransmit
    {
        get
        {
            if (UnkeyedNormally)
            {
                return UnkeyRoute.OrdinaryUnkey;
            }

            if (Abort is null)
            {
                // KEYED, WITH NEITHER ROUTE OUT TAKEN, IS NOT "NOTHING WAS
                // KEYED". The sequence makes that state unreachable; the record
                // still refuses to describe it as safe, because a result that
                // reads well on a path nobody has is how the fault gets shipped.
                return Keyed ? UnkeyRoute.NothingReachedTheRadio : UnkeyRoute.NothingWasKeyed;
            }

            return Abort.AnythingReachedTheRadio
                ? UnkeyRoute.TheAbort
                : UnkeyRoute.NothingReachedTheRadio;
        }
    }

    /// <summary>
    /// True where nothing this run did can have left the radio transmitting.
    /// </summary>
    /// <remarks>
    /// Three ways to be true and one way to be false, and the false one is a port
    /// that took no bytes at all - which no software above it can repair.
    /// </remarks>
    public bool RadioIsInReceive => CameOutOfTransmit != UnkeyRoute.NothingReachedTheRadio;
}

/// <summary>
/// One transmission, from the licence gate to the unkey.
/// </summary>
/// <remarks>
/// <para>**THE UNKEY IS THE REASON THIS TYPE EXISTS.** Keying a radio is one
/// write; coming back out of transmit on every path a transmission can end -
/// including the ones nobody thought of - is the part that has to be built
/// once, in one place, with nothing else able to key around it. So the keying
/// write lives here and nowhere else, inside a <c>try</c> whose <c>finally</c>
/// either unkeys or fires the abort, and <c>CivConstants.PttOn</c> has this one
/// use site (§0.2).</para>
/// <para>**IT IMPLEMENTS NEITHER END.** The CI-V end is
/// <see cref="ISerialPort"/>, which already existed; the audio end is
/// <see cref="ITransmitAudioSink"/>, which this unit declares and does not
/// implement for any real device. It opens nothing, plays nothing, and enumerates
/// nothing.</para>
/// <para>**NOTHING HERE CAN DECIDE TO TRANSMIT.** There is no clock read, no
/// waiting of any kind, no repetition, and no entry point that does not take an
/// <see cref="OperatorSend"/>. The slot's start and the offset into it arrive as
/// values and are recorded rather than waited for; a path that waits for a moment
/// is a path that can arrive at one on its own.
/// <c>NothingInTheSequenceCanStartATransmissionOnItsOwn</c> reads this file and
/// fails on a hit.</para>
/// <para>**THE GATE IS INSIDE THE PATH, NOT BESIDE IT** (§0.2), and it is
/// narrower here than <see cref="TransmitGuard.Check"/> is in general: only a
/// permit that was not overridden lets anything key. See
/// <see cref="Permits"/>.</para>
/// <para>**Nothing in this repository calls it.** The first caller is step 5's
/// right-click, which is the only thing that should ever be able to say "now".
/// </para>
/// </remarks>
public sealed class Ft8TransmitSequence
{
    /// <summary>FT8 is data, and that is what the gate is asked about.</summary>
    private const TransmitMode Mode = TransmitMode.Data;

    private readonly ISerialPort _port;
    private readonly ITransmitAudioSink _sink;
    private readonly TransmitGuard _guard;
    private readonly ITelemetry _telemetry;
    private readonly byte _radioAddress;
    private readonly byte _controllerAddress;

    /// <summary>Build a sequence over a transport and a sink.</summary>
    /// <param name="port">The CI-V transport, in whatever state it is in.</param>
    /// <param name="sink">Where the samples go.</param>
    /// <param name="guard">The licence gate, or a fresh one over the shipped data.</param>
    /// <param name="telemetry">Where the record goes, or nowhere.</param>
    /// <param name="radioAddress">The radio's CI-V address.</param>
    /// <param name="controllerAddress">This application's CI-V address.</param>
    public Ft8TransmitSequence(
        ISerialPort port,
        ITransmitAudioSink sink,
        TransmitGuard? guard = null,
        ITelemetry? telemetry = null,
        byte radioAddress = CivConstants.DefaultRadioAddress,
        byte controllerAddress = CivConstants.DefaultControllerAddress)
    {
        _port = port ?? throw new ArgumentNullException(nameof(port));
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _guard = guard ?? new TransmitGuard();
        _telemetry = telemetry ?? NullTelemetry.Instance;
        _radioAddress = radioAddress;
        _controllerAddress = controllerAddress;
    }

    /// <summary>
    /// Check the gate, key, hand the samples to the sink, unkey.
    /// </summary>
    /// <param name="send">What the operator asked for.</param>
    /// <param name="cancellationToken">Stops the transmission.</param>
    /// <returns>What happened, including how the radio came out of transmit.</returns>
    /// <remarks>
    /// <para>**IT NEVER THROWS AND IT NEVER LEAVES THE RADIO KEYED.** A sink that
    /// throws, a sink that returns early, a cancellation, a port that dies on the
    /// way out and a fault anywhere in this method's own body all end the same
    /// way: the abort fires, its record comes back in the result, and the caller
    /// is told which of the two unkeys happened.</para>
    /// <para>**THE ABORT IS THE ABNORMAL PATH AND ONLY THE ABNORMAL PATH.** On a
    /// clean run the ordinary <c>1C 00 00</c> write ends the transmission and
    /// <see cref="TransmitRun.Abort"/> is null - a successful transmission does
    /// not pretend to be a fault.</para>
    /// </remarks>
    public async Task<TransmitRun> RunAsync(
        OperatorSend send, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(send);

        // **THE STAGE IS WRITTEN BEFORE THE STAGE RUNS** (work instruction 305
        // task 3). A transmission the operator watched the radio make left no
        // record at all, because only outcomes were written and this path
        // produced none - so *never entered* and *entered and died* were one
        // empty file.
        var stages = new List<string>();

        void Entering(string stage, string? detail = null)
        {
            stages.Add(stage);
            SendStage.Entered(_telemetry, stage, detail);
        }

        Entering(SendStage.GateAsked, send.FrequencyHz.ToString(
            System.Globalization.CultureInfo.InvariantCulture) + " Hz");

        // THE GATE FIRST, AND NOTHING BEFORE IT BUT A NULL CHECK. Everything
        // after this line can key a radio; nothing before it can.
        var decision = _guard.Check(
            send.LicenseClass, send.FrequencyHz, Mode, send.GuardEnabled);

        if (!Permits(decision, out var refusal))
        {
            return Recorded(
                send,
                Refused(Ft8TransmitOutcome.RefusedByLicence, refusal, decision.Citation),
                stages);
        }

        if (!Sendable(send, out var unsendable))
        {
            return Recorded(
                send,
                Refused(Ft8TransmitOutcome.RefusedAsUnsendable, unsendable, string.Empty),
                stages);
        }

        var samples = send.Samples;
        var seconds = samples.Length / (double)send.SampleRate;

        var outcome = Ft8TransmitOutcome.Played;
        var reason = string.Empty;
        var keyed = false;
        var unkeyedNormally = false;
        AbortRecord? abort = null;
        PlayedAudio? played = null;

        try
        {
            Entering(SendStage.Keyed);

            await _port.WriteAsync(Frame(CivConstants.PttOn), cancellationToken)
                .ConfigureAwait(false);
            keyed = true;

            Entering(
                SendStage.HandedToTheSoundCard,
                samples.Length.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + " samples");

            var went = await _sink
                .PlayAsync(samples, send.SampleRate, cancellationToken)
                .ConfigureAwait(false);
            played = went;

            if (went.SamplesPlayed != samples.Length)
            {
                // A SHORT PLAY HAS TWO CAUSES SINCE UNIT 263 AND THEY ARE NOT THE
                // SAME SENTENCE TO PUT IN FRONT OF AN OPERATOR. One is the sound
                // card letting him down; the other is him pressing the button.
                // Before the stop could reach the sink there was only ever the
                // first, so this branch read every short play as a fault.
                if (cancellationToken.IsCancellationRequested)
                {
                    outcome = Ft8TransmitOutcome.Cancelled;
                    reason =
                        $"the transmission was stopped after {went.SamplesPlayed} of "
                        + $"{samples.Length} samples, so the rest of it did not go out.";
                }
                else
                {
                    // A SINK THAT STOPPED EARLY IS A FAILURE, NOT A SUCCESS WITH A
                    // SMALLER NUMBER. Half a transmission on the air is a signal
                    // nobody can decode occupying somebody else's slot.
                    outcome = Ft8TransmitOutcome.AudioFailed;
                    reason =
                        $"the audio path played {went.SamplesPlayed} of {samples.Length} "
                        + "samples and returned, so only part of the transmission went out.";
                }
            }
        }
        catch (OperationCanceledException)
        {
            outcome = Ft8TransmitOutcome.Cancelled;
            reason = "the transmission was canceled while it was going out.";
        }
        catch (Exception ex)
        {
            outcome = keyed ? Ft8TransmitOutcome.AudioFailed : Ft8TransmitOutcome.PortFailed;
            reason = keyed
                ? $"the audio path failed while the radio was transmitting: {ex.Message}"
                : $"the radio would not take the keying frame: {ex.Message}";
        }
        finally
        {
            // THE UNKEY, ON EVERY PATH OUT OF THE BLOCK ABOVE. Nothing in it can
            // return, throw or be cancelled past this point.
            if (outcome == Ft8TransmitOutcome.Played)
            {
                try
                {
                    Entering(SendStage.Unkeyed);

                    // CancellationToken.None: whoever cancelled wanted the
                    // transmission stopped, which is the opposite of wanting this
                    // write skipped.
                    await _port.WriteAsync(Frame(CivConstants.PttOff), CancellationToken.None)
                        .ConfigureAwait(false);
                    unkeyedNormally = true;
                }
                catch (Exception ex)
                {
                    outcome = Ft8TransmitOutcome.PortFailed;
                    reason =
                        "the transmission went out but the radio would not take the frame that "
                        + $"ends it: {ex.Message}";
                }
            }

            if (!unkeyedNormally)
            {
                // EVEN WHERE THE KEYING WRITE ITSELF THREW. A write that threw is
                // not a write that is known not to have arrived, and the abort
                // costs two frames at a port that is probably already dead.
                abort = TransmitAbort.Fire(_port, _radioAddress, _controllerAddress);
            }
        }

        return Recorded(
            send,
            new TransmitRun(
                outcome, reason, string.Empty, keyed, unkeyedNormally, abort, played,
                samples.Length, seconds),
            stages);
    }

    /// <summary>
    /// Writes the transmission's shape to telemetry and hands the run back
    /// unchanged.
    /// </summary>
    /// <param name="send">What the operator asked for.</param>
    /// <param name="run">What became of it.</param>
    /// <param name="stages">Which stages the path entered, in order.</param>
    /// <returns><paramref name="run"/>.</returns>
    /// <remarks>
    /// <para>**AFTER THE RADIO IS OUT OF TRANSMIT, NEVER BEFORE.** Writing a
    /// record is not urgent and coming out of transmit is; nothing goes between
    /// the end of the audio and the unkey.</para>
    /// <para>**IT CARRIES THE SHAPE AND NOT THE WORDS** (HM-DEC-018).
    /// <see cref="TransmitRecord"/> has nowhere to put
    /// <c>Ft8Transmission.Text</c> or <c>ReadsBackAs</c>, which is why the length
    /// is passed and the message is not.</para>
    /// <para>**AND THE LENGTH IS THE ENCODED MESSAGE'S** (work instruction 272,
    /// task 3). It was <c>Text.Length</c> - the composed string's, the words the
    /// operator asked for - which for a message that put a callsign on the wire as
    /// a hash counted something that was never transmitted. On 2026-09-07 the line
    /// read <c>messageType: Standard, messageLength: 16</c> for a slot that
    /// actually carried <c>&lt;CQ KC3QIS&gt; &lt;FN00DJ&gt;</c> and decoded to
    /// nothing. **A length that measures the wrong thing is worse than no
    /// length**, so it counts <c>ReadsBackAs</c>, and the flag beside it says
    /// which kind of message went out.</para>
    /// <para>A refusal is recorded too, with nothing offered and nothing keyed. A
    /// transmission that did not happen because the licence gate said no is worth
    /// exactly as much to somebody reading the log as one that did.</para>
    /// </remarks>
    private TransmitRun Recorded(
        OperatorSend send, TransmitRun run, IReadOnlyList<string>? stages = null)
    {
        var record = send.HasSlot
            ? new TransmitRecord(
                send.SlotStartUtc,
                send.StartSecondsIntoSlot,
                send.FrequencyHz,
                run.SecondsOffered,
                send.Transmission.SampleRate,
                run.SamplesOffered,
                send.Transmission.Type,
                send.Transmission.ReadsBackAs.Length,
                send.Transmission.CarriesHashedCallsign,
                run.Outcome,
                run.CameOutOfTransmit,
                run.Keyed)
            : new TransmitRecord(
                SlotStartUtc: null,
                StartSecondsIntoSlot: null,
                FrequencyHz: send.FrequencyHz,
                DurationSeconds: run.SecondsOffered,
                SampleRate: send.SampleRate,
                SampleCount: run.SamplesOffered,
                MessageType: null,
                MessageLength: send.Unslotted!.MessageLength,
                CarriedHashedCallsign: null,
                Outcome: run.Outcome,
                CameOutOfTransmit: run.CameOutOfTransmit,
                Keyed: run.Keyed,
                Mode: send.Unslotted.Mode,
                Fit: send.Unslotted.Fit,
                AudioSeconds: send.Unslotted.Seconds);

        var bag = new Dictionary<string, object?>(record.ToBag());

        // **WHICH STAGES THIS TRANSMISSION ACTUALLY REACHED** (work instruction 305
        // task 3). The record stays exactly what it was and gains one field, so a
        // reader with only this line still learns where the path got to.
        bag["stagesEntered"] = stages is { Count: > 0 }
            ? string.Join(" | ", stages)
            : StartupSnapshot.Unknown;

        _telemetry.Write(
            TelemetryCategory.Transmit, TransmitRecord.EventName, bag, record.Level);

        return run;
    }

    /// <summary>
    /// Whether the gate positively permitted this - which is narrower than
    /// <see cref="TransmitGuard.Check"/>'s own answer.
    /// </summary>
    /// <param name="decision">What the gate said.</param>
    /// <param name="refusal">Why this path refuses anyway, in the operator's words.</param>
    /// <returns>True only for a permit that was not overridden.</returns>
    /// <remarks>
    /// <para>**THE GATE PERMITS IN THREE WAYS AND THIS PATH ACCEPTS ONE.**
    /// <c>Check</c> returns <c>MayTransmit: true</c> when the privileges permit
    /// it, when the licence class is unknown, and when the operator's guard
    /// toggle is off. §0.2 says the Settings check is not bypassable from any
    /// send path, so on this path the last two are refusals.</para>
    /// <para>**AND NOTHING EXISTING IS LOOSENED BY THAT.** <c>Check</c> is not
    /// changed and no existing caller's behaviour moves - <c>CwTransmitter</c>
    /// keeps exactly the answer it has today. This is new code being stricter
    /// than the general answer, which is the only direction a send path may
    /// differ in. What the other callers should do is the owner's question and is
    /// not touched here.</para>
    /// <para>**AN UNKNOWN CLASS IS A REFUSAL BECAUSE FT8 IS NOT CW.** The gate's
    /// own reasoning for permitting - the operator holds the licence and knows
    /// what it says - is about a person with their hand on a key. This path hands
    /// a slot of audio to a radio; the person is one click away from it and the
    /// message is one nobody reads before it goes.</para>
    /// </remarks>
    private static bool Permits(TransmitDecision decision, out string refusal)
    {
        if (!decision.MayTransmit)
        {
            refusal = decision.Reason;
            return false;
        }

        if (decision.WasOverridden)
        {
            refusal =
                "the license guard is switched off in Settings, so Hamlet has no answer it can "
                + "stand behind about whether this frequency is inside your privileges. It will "
                + $"not key on that. What the guard said when it was last asked: {decision.Reason}";
            return false;
        }

        if (string.IsNullOrEmpty(decision.Citation))
        {
            // THE GATE'S OWN SENTENCE, AND THEN WHAT THIS PATH DOES ABOUT IT.
            // Saying it again in slightly different words would read as two
            // refusals for one reason.
            refusal = string.IsNullOrEmpty(decision.Reason)
                ? "nothing checked this transmission against your license privileges, and Hamlet "
                  + "will not key one it could not check."
                : $"{decision.Reason} Hamlet will not key a transmission nothing checked.";
            return false;
        }

        refusal = string.Empty;
        return true;
    }

    /// <summary>
    /// **A send with no slot that would run on is refused here, before it is armed**, and
    /// the refusal is written as a record.
    /// </summary>
    /// <param name="send">What the operator asked for.</param>
    /// <returns>The refusal, already recorded, or null where the send may be armed.</returns>
    /// <remarks>
    /// <para>**ONLY A SEND WITH NO SLOT IS ASKED** (work instruction 318 task 3). A slotted
    /// send is armed exactly as it always was and meets its fit inside
    /// <see cref="RunAsync"/>, so nothing on the FT8 and FT4 path writes a line it did not
    /// write before.</para>
    /// <para>**IT KEYS NOTHING AND ASKS NO GATE.** It measures the audio against the cap and
    /// writes what it found. The gate is asked when the send is fired, first, as it is for
    /// every send - and <see cref="Sendable"/> asks this same question again there, as the
    /// backstop for a send that reached <see cref="RunAsync"/> without being armed.</para>
    /// </remarks>
    internal TransmitRun? RefusedBeforeArming(OperatorSend send)
    {
        ArgumentNullException.ThrowIfNull(send);

        if (send.HasSlot || SendableWithNoSlot(send, out var why))
        {
            return null;
        }

        return Recorded(send, Refused(Ft8TransmitOutcome.RefusedAsUnsendable, why, string.Empty));
    }

    /// <summary>Whether a send with no slot has audio, and no more of it than the cap.</summary>
    /// <param name="send">A send whose <see cref="OperatorSend.HasSlot"/> is false.</param>
    /// <param name="why">What is wrong with it, in the operator's words.</param>
    /// <returns>True where it may go.</returns>
    /// <remarks>
    /// **NO SLOT, SO NO SLOT FIT** (§R10). What holds it is
    /// <see cref="OperatorSend.LongestUnslottedSeconds"/>, and the sentence reads the number
    /// off that constant rather than writing it here.
    /// </remarks>
    private static bool SendableWithNoSlot(OperatorSend send, out string why)
    {
        var audio = send.Unslotted!;

        switch (audio.Fit)
        {
            case UnslottedFit.NoAudio:
                why = "there is no audio in this transmission, so there is nothing to send.";
                return false;

            case UnslottedFit.LongerThanTheCap:
                why =
                    $"this is {audio.Seconds:0.##} s of {audio.Mode} audio, and a send with no slot "
                    + $"may be at most {OperatorSend.LongestUnslottedSeconds:0} s so that a continuous "
                    + "carrier cannot run on. Nothing keyed.";
                return false;

            default:
                why = string.Empty;
                return true;
        }
    }

    /// <summary>Whether there is a transmission here that fits where it is going.</summary>
    /// <param name="send">What the operator asked for.</param>
    /// <param name="why">What is wrong with it, in the operator's words.</param>
    /// <returns>True where it can be sent as asked.</returns>
    /// <remarks>
    /// <para>**THE FIT IS ARITHMETIC, NOT A WAIT.** <c>SlotGrid.TransmissionFits</c>
    /// answers whether the tones still land inside the slot when they start this
    /// far after the boundary. A transmission that would run past the boundary into
    /// the next slot is refused before anything keys, rather than truncated on the
    /// air.</para>
    /// <para>**AND IT IS ASKED ABOUT THE GRID THE TRANSMISSION IS ON** (work
    /// instruction 293 task 3). This measured against <c>Ft8Slots</c> literals in
    /// four places, so an FT4 transmission - 7.5 s of slot - was tested against
    /// fifteen and accepted whatever it was. **Every number below now comes off
    /// <c>send.Grid</c> and none of them is written here**, including the two in the
    /// sentences the operator reads: the open 4.48-against-5.04 question is the
    /// owner's and a literal in a refusal would be answering it.</para>
    /// <para>**A SEND WITH NO SLOT IS ASKED ONLY ABOUT THE CAP** (work instruction 318
    /// task 3), by <see cref="SendableWithNoSlot"/>, before anything below reads a slot.</para>
    /// </remarks>
    private static bool Sendable(OperatorSend send, out string why)
    {
        if (!send.HasSlot)
        {
            return SendableWithNoSlot(send, out why);
        }

        var grid = send.Grid;

        if (send.Transmission.Samples.Length == 0)
        {
            why = "there is no audio in this transmission, so there is nothing to send.";
            return false;
        }

        if (send.StartSecondsIntoSlot < 0 || send.StartSecondsIntoSlot >= grid.SlotSeconds)
        {
            why =
                $"a transmission cannot start {send.StartSecondsIntoSlot:0.###} s into a "
                + $"{grid.SlotSeconds:0} s slot.";
            return false;
        }

        var left = grid.SlotSeconds - send.StartSecondsIntoSlot;

        if (!grid.TransmissionFits(left))
        {
            why =
                $"starting {send.StartSecondsIntoSlot:0.###} s into the slot leaves "
                + $"{left:0.###} s of it, and an {grid.Name} transmission needs "
                + $"{grid.TransmissionSeconds:0.##} s. It would run into the next slot.";
            return false;
        }

        var audioSeconds =
            send.Transmission.Samples.Length / (double)send.Transmission.SampleRate;

        if (audioSeconds > left + 1e-6)
        {
            // THE PADDED SLOT IS THE COMMONEST THING CAUGHT HERE AND IT IS NOT THE
            // ONLY ONE. Ft8Composer.Compose returns a whole slot with the signal
            // centred, which is what a decoder reads and not what goes on the air;
            // ComposeSignal is the route that goes out. But this branch fires on any
            // audio longer than the slot has room for, whatever produced it.
            //
            // **SO THE SENTENCE STATES THE MEASUREMENT AND OFFERS THE CAUSE AS A
            // LIKELIHOOD** (work instruction 294 task 7, §12.1). It read "a padded
            // slot is what a decoder reads, not what goes on the air" as a flat
            // assertion of the cause, which is a diagnosis this line has not made:
            // it has measured two lengths. Every number in it was right and stays
            // right, and all of them still come off `send.Grid`.
            why =
                $"this is {audioSeconds:0.###} s of audio and only {left:0.###} s of the slot is "
                + $"left after {send.StartSecondsIntoSlot:0.###} s. An {grid.Name} transmission is "
                + $"{grid.TransmissionSeconds:0.##} s of tones with no silence on either end. "
                + "The commonest cause is a padded slot - what a decoder reads rather than what "
                + "goes on the air - but what is measured here is the length and not the reason.";
            return false;
        }

        why = string.Empty;
        return true;
    }

    /// <summary>A refusal, with nothing keyed and the sink never touched.</summary>
    private static TransmitRun Refused(Ft8TransmitOutcome outcome, string reason, string citation)
        => new(outcome, reason, citation, false, false, null, null, 0, 0);

    /// <summary>The transmit/receive frame, on or off.</summary>
    /// <param name="state">
    /// <c>CivConstants.PttOn</c> or <c>CivConstants.PttOff</c>. **The only two
    /// bytes this method is ever passed, and both call sites are in
    /// <see cref="RunAsync"/>** - one inside the <c>try</c>, one inside the
    /// <c>finally</c>.
    /// </param>
    private byte[] Frame(byte state)
        => new CivFrame(
            _radioAddress,
            _controllerAddress,
            CivConstants.CmdTransceiverControl,
            new[] { CivConstants.SubPtt, state }).ToWireBytes();
}
