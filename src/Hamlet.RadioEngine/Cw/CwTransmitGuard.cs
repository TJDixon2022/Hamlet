namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Notices when the audio has gone away because the operator is transmitting,
/// and stops the decoder learning anything from it (HM-DEC-095).
/// </summary>
/// <remarks>
/// <para>**THIS IS WHY A REAL CONTACT DECODED AS NOTHING.** In the recording made
/// while a station was answering him, the operator is transmitting for eighteen
/// of the thirty seconds. On full break-in the receiver mutes between his own
/// elements, so what reaches the sound card is his keying cut into the band: fifty
/// to eighty decibels of near-digital silence, hundreds of times, with about
/// twenty-four milliseconds of transmit-receive hang either side of each one.</para>
/// <para>The gate's trackers had no idea any of that was happening. The noise
/// floor chased the silence all the way down, the peak came with it, and by the
/// time the answering station arrived both were calibrated to a band that does
/// not exist. Twelve hundred and eleven elements came out of that recording and
/// one character, and the elements were his own transmission being measured as
/// somebody else's.</para>
/// <para>**WHAT MAKES A MUTE RECOGNIZABLE IS THAT IT IS BROADBAND.** A signal
/// fading takes one note down; a receiver muting takes the whole audio band down
/// together, to a level no band noise ever reaches. So the test is on total power
/// across everything, not on the tone.</para>
/// <para>Three parts, and each has a failure it prevents:</para>
/// <list type="bullet">
/// <item>**Freeze rather than adapt.** While muted the trackers hold exactly
/// where they were, so the operator's own transmission teaches them nothing.</item>
/// <item>**Hold past recovery.** The receiver comes back over a few tens of
/// milliseconds and the first thing through is a ramp, not a signal. Holding a
/// hundred and fifty milliseconds means the ramp is never measured as an
/// element.</item>
/// <item>**Clamp the floor.** Even frozen, a floor that has previously been
/// dragged toward digital silence never recovers within a recording. The clamp
/// puts a bottom under it that no real band noise goes below.</item>
/// </list>
/// <para>**AND WHAT IS HEARD BETWEEN HIS OWN ELEMENTS IS NOT EVIDENCE.** The
/// slivers of somebody else's signal audible in the gaps of his own keying are
/// cut at both ends by him. Their lengths are facts about his fist, and decoding
/// them produces a confident string of E and T, which is the most seductive wrong
/// output this feature can produce because it looks exactly like a weak station
/// being read (§0.0).</para>
/// </remarks>
public sealed class CwTransmitGuard
{
    /// <summary>
    /// Below this the audio is not quiet, it is absent, in decibels full scale.
    /// </summary>
    /// <remarks>
    /// Minus sixty. Measured against these recordings, the quietest the band
    /// itself ever gets in a five hundred hertz filter is around minus fifty-five
    /// and the mutes go to minus eighty and below, so this sits in a gap nothing
    /// occupies rather than on a boundary anything crosses.
    /// </remarks>
    public const double MuteBelowDbfs = -60;

    /// <summary>
    /// Below this it is not a muted receiver, in decibels full scale.
    /// </summary>
    /// <remarks>
    /// <para>**A MUTED RECEIVER IS QUIET. AN EMPTY FILE IS ZERO, AND THEY ARE A
    /// HUNDRED AND FIFTY DECIBELS APART** (HM-DEC-095). Measured across this
    /// repository: the mutes in the real recording made on full break-in bottom
    /// out between minus eighty and minus eighty-four, because the radio stops
    /// the audio while the codec carries on streaming, so what arrives is the
    /// converter's own residue rather than nothing. Synthesized Morse has exact
    /// digital zero between its elements, which measures minus two hundred and
    /// forty.</para>
    /// <para>Without this bound the guard read every gap in every synthetic
    /// fixture as the operator transmitting and blocked eighty percent of a clean
    /// recording, which deleted the decode entirely. That is the failure this
    /// bound exists to prevent, and it was found by the fixtures rather than
    /// reasoned about in advance, which is the argument for having them.</para>
    /// <para>A station whose mute path really does deliver digital zero loses the
    /// guard and decodes as it did before it existed. That is the safe direction:
    /// the guard is an improvement that can be absent, not a correctness
    /// requirement that can be wrong.</para>
    /// </remarks>
    public const double SilenceBelowDbfs = -90;

    /// <summary>
    /// How long after the audio returns before it counts, in seconds.
    /// </summary>
    /// <remarks>
    /// A hundred and fifty milliseconds. The radio's own transmit-receive
    /// changeover takes something like twenty-four, and what follows it is the
    /// receiver's gain recovering rather than a signal arriving. This is several
    /// times the changeover on purpose: the cost of waiting is a fraction of one
    /// element and the cost of not waiting is measuring a gain ramp as one.
    /// </remarks>
    public const double HoldSeconds = 0.150;

    /// <summary>
    /// The lowest the noise floor may ever be believed to be, in decibels.
    /// </summary>
    /// <remarks>
    /// **A BOTTOM UNDER THE FLOOR, SO IT CAN NEVER CHASE DIGITAL SILENCE.** Real
    /// band noise in a narrow filter does not go below about minus seventy on
    /// this radio, and a floor that has been pulled to minus a hundred and twenty
    /// by a mute reports every subsequent breath of noise as forty decibels of
    /// signal.
    /// </remarks>
    public const double FloorFloorDb = -75;

    private readonly int _holdHops;

    private int _held;

    /// <summary>Creates a guard.</summary>
    /// <param name="hopSeconds">How long one measurement covers.</param>
    public CwTransmitGuard(double hopSeconds)
        => _holdHops = Math.Max(1, (int)Math.Round(HoldSeconds / Math.Max(1e-6, hopSeconds)));

    /// <summary>True when this measurement may not be learned from.</summary>
    public bool IsBlocked { get; private set; }

    /// <summary>True when the audio is actually absent right now.</summary>
    public bool IsMuted { get; private set; }

    /// <summary>How many measurements have been blocked altogether.</summary>
    public long BlockedHops { get; private set; }

    /// <summary>How many separate transmissions have been seen.</summary>
    public int Transmissions { get; private set; }

    /// <summary>
    /// Judge one measurement by the total power in it.
    /// </summary>
    /// <param name="broadbandDbfs">Power across the whole audio band.</param>
    /// <returns>True when this measurement is blocked.</returns>
    public bool Observe(double broadbandDbfs)
    {
        var muted = broadbandDbfs <= MuteBelowDbfs
            && broadbandDbfs > SilenceBelowDbfs;

        if (muted)
        {
            if (!IsMuted)
            {
                Transmissions++;
            }

            _held = _holdHops;
        }
        else if (_held > 0)
        {
            _held--;
        }

        IsMuted = muted;
        IsBlocked = muted || _held > 0;

        if (IsBlocked)
        {
            BlockedHops++;
        }

        return IsBlocked;
    }

    /// <summary>Forget everything.</summary>
    public void Reset()
    {
        _held = 0;
        IsBlocked = false;
        IsMuted = false;
        BlockedHops = 0;
        Transmissions = 0;
    }
}
