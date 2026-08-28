using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Explore;

/// <summary>The mode a stretch of band calls for, and why.</summary>
/// <param name="Mode">The operating mode.</param>
/// <param name="DataMode">Whether the data variant is wanted.</param>
/// <param name="Because">
/// The reason, in the app's voice, for the status line to say out loud.
/// </param>
public sealed record ModeTarget(CivMode Mode, bool DataMode, string Because)
{
    /// <summary>What the operator would see on the radio, e.g. "USB-D".</summary>
    public string Name => DataMode ? CivValues.Name(Mode) + "-D" : CivValues.Name(Mode);
}

/// <summary>Whether the automation is on, and whether it is standing down.</summary>
/// <param name="Enabled">The operator's setting.</param>
/// <param name="Suspended">
/// True after the operator set a mode themselves, until the next band change.
/// </param>
/// <param name="DoneAtHz">
/// Where the last confirmed write was made, or null (HM-OPEN-041).
/// </param>
/// <param name="DoneMode">What that write set, or null.</param>
/// <param name="DoneDataMode">Whether that write asked for the data variant.</param>
public sealed record ModeFollowState(
    bool Enabled,
    bool Suspended,
    long? DoneAtHz = null,
    CivMode? DoneMode = null,
    bool DoneDataMode = false)
{
    /// <summary>The state a fresh session starts in.</summary>
    /// <param name="enabled">The operator's setting.</param>
    /// <returns>The state.</returns>
    public static ModeFollowState Armed(bool enabled) => new(enabled, false);

    /// <summary>The radio confirmed a write, so it is not made again here.</summary>
    /// <param name="hz">Where the dial was.</param>
    /// <param name="mode">What was set.</param>
    /// <param name="dataMode">Whether the data variant was asked for.</param>
    /// <returns>The state carrying that memory.</returns>
    /// <remarks>
    /// **A CONFIRMED WRITE IS A FACT ABOUT WHAT HAMLET DID, AND IT IS NOT THE
    /// SAME FACT AS WHAT THE RADIO REPORTS** (HM-OPEN-041). The old test for
    /// "already there" read the rig's own state, so any field that came back
    /// unknown, stale, or a variant the radio does not report separately made
    /// every tick look like a fresh arrival: eighteen writes went out in one
    /// evening with the dial standing still, ten of them with nothing the
    /// operator did anywhere near them. Remembering the write closes that
    /// whatever the rig says, and the rig test stays as well, because either one
    /// alone would be a reason not to write.
    /// </remarks>
    public ModeFollowState Done(long hz, CivMode mode, bool dataMode)
        => this with { DoneAtHz = hz, DoneMode = mode, DoneDataMode = dataMode };

    /// <summary>
    /// The operator turned the mode knob, so Hamlet stops turning it.
    /// </summary>
    /// <remarks>
    /// THE OPERATOR'S OWN HAND ALWAYS WINS (HM-DEC-056). Somebody who sets a
    /// mode on purpose has said something, and an app that changed it back two
    /// seconds later would be arguing with them about their own radio.
    /// </remarks>
    public ModeFollowState SuspendedByOperator()
        => this with { Suspended = true, DoneAtHz = null, DoneMode = null };

    /// <summary>A band change re-arms it.</summary>
    /// <remarks>
    /// Because a band change is a fresh start rather than a continuation, and
    /// somebody who suspended the automation on 40 m almost certainly did not
    /// mean to switch it off forever.
    /// </remarks>
    public ModeFollowState Rearmed()
        => this with { Suspended = false, DoneAtHz = null, DoneMode = null };
}

/// <summary>What to do about the mode, as a value.</summary>
/// <param name="Write">Whether to write to the radio at all.</param>
/// <param name="Mode">The mode to set, when writing.</param>
/// <param name="DataMode">Whether the data variant is wanted.</param>
/// <param name="Narration">
/// What to say in the status line afterwards, or "" when nothing is happening.
/// </param>
public sealed record ModeFollowDecision(
    bool Write, CivMode Mode, bool DataMode, string Narration)
{
    /// <summary>Nothing to do.</summary>
    public static ModeFollowDecision Nothing { get; } = new(false, CivMode.Cw, false, "");
}

/// <summary>
/// Mode follows the map: what to set the radio to, and whether to set it at all
/// (HM-DEC-056).
/// </summary>
/// <remarks>
/// <para>A PURE FUNCTION, so every case is testable without a radio, the way
/// <c>ReconnectPlan.Decide</c> already is. The cases that matter are the ones
/// nobody exercises by hand: the operator's own mode change, a drag across three
/// neighborhoods, a write the radio never confirmed.</para>
/// <para>The sideband convention is cited rather than remembered. The IARU
/// Region 2 Band Plan states it plainly: "For SSB phone operations below 10 MHz
/// use lower sideband (LSB); above 10 MHz use upper sideband (USB)." The one
/// exception it names is 60 m, which Hamlet does not draw.</para>
/// <para>The data modes go to the upper sideband with the data variant on,
/// which is what every one of the mode communities' own frequency lists
/// assumes: the 070 Club states its numbers are "the transceiver settings for
/// USB operation", and WSJT-X and JS8Call publish dial frequencies on the same
/// footing (HM-DEC-054).</para>
/// </remarks>
public static class ModeFollowPlan
{
    /// <summary>Where the sideband convention changes over.</summary>
    /// <remarks>
    /// IARU Region 2 Band Plan, September 2020: below 10 MHz lower sideband,
    /// above it upper.
    /// </remarks>
    public const long SidebandChangeoverHz = 10_000_000;

    /// <summary>The mode a neighborhood calls for, or null when it says nothing.</summary>
    /// <param name="hood">The neighborhood, or null.</param>
    /// <returns>The target, or null.</returns>
    /// <remarks>
    /// Read from the map's own short label, which comes from the cited data file
    /// (HM-DEC-054), so the automation and the picture cannot disagree about
    /// what lives where. Open ground, the beacon block and the automatic
    /// stations say nothing, because nothing about them tells the operator what
    /// they would be doing there.
    /// </remarks>
    public static ModeTarget? TargetFor(Neighborhood? hood)
    {
        if (hood is null)
        {
            return null;
        }

        var label = hood.ShortName.Trim().ToUpperInvariant();

        if (label is "CW" or "CW DX" or "QRP")
        {
            return new ModeTarget(
                CivMode.Cw, false,
                "this is the Morse end of the band");
        }

        if (label is "FT8" or "FT4" or "JS8" or "PSK31" or "RTTY")
        {
            return new ModeTarget(
                CivMode.Usb, true,
                "this block is where the digital modes gather, and they are all "
                + "worked through the computer on the upper sideband");
        }

        if (label is "SSB" or "SSB DX")
        {
            var lower = hood.JumpHz < SidebandChangeoverHz;

            return new ModeTarget(
                lower ? CivMode.Lsb : CivMode.Usb, false,
                lower
                    ? "voice below ten megahertz is worked on the lower sideband, "
                      + "which is just what everybody settled on"
                    : "voice above ten megahertz is worked on the upper sideband, "
                      + "which is just what everybody settled on");
        }

        if (label == "AM")
        {
            return new ModeTarget(
                CivMode.Am, false,
                "this corner is the AM crowd, using the mode broadcast radio used");
        }

        return null;
    }

    /// <summary>Work out whether to change the radio's mode, and to what.</summary>
    /// <param name="state">Whether the automation is on and armed.</param>
    /// <param name="currentMode">The mode the radio is in, or null when unknown.</param>
    /// <param name="currentDataMode">
    /// Whether the radio is in the data variant, or null where nobody has read
    /// the flag. **Three-valued on purpose** (work instruction 042): it was a
    /// bare bool answering false for both "off" and "nobody has said", and
    /// against a target wanting the variant off an unread flag then compared
    /// equal to it, so the automation concluded the radio was already right
    /// without anybody having looked.
    /// </param>
    /// <param name="target">What the map calls for, or null.</param>
    /// <param name="frequencyHz">
    /// Where the dial is, or null where the caller cannot say (HM-OPEN-041).
    /// </param>
    /// <param name="workingCw">
    /// True when the operator is visibly working Morse: the CW terminal is
    /// decoding, or the dial is inside a CW segment of the band plan.
    /// </param>
    /// <returns>The decision.</returns>
    public static ModeFollowDecision Decide(
        ModeFollowState state,
        CivMode? currentMode,
        bool? currentDataMode,
        ModeTarget? target,
        long? frequencyHz = null,
        bool workingCw = false)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.Enabled || state.Suspended || target is null)
        {
            return ModeFollowDecision.Nothing;
        }

        // **NOTHING TAKES HIM OUT OF MORSE WHILE HE IS WORKING MORSE.**
        //
        // On 2026-08-18 this wrote USB with the data variant on, over and over,
        // while the operator sat on CW main street with the terminal decoding a
        // signal at 500 hertz. The send controls then refused `not_in_morse` for
        // sixty-six seconds: **he could not answer a station because the app had
        // moved his radio out from under him.**
        //
        // HM-DEC-056 already says the operator's own hand wins. A terminal that
        // is decoding and a dial inside a CW segment are that hand, as plainly as
        // turning the mode knob is. Following the map into a digital block is
        // defensible when he arrives there and is doing nothing else; overriding
        // what he is visibly doing is not, and the map is the weaker evidence of
        // the two (§0.0).
        //
        // It is silence rather than a refusal with a sentence, because the
        // operator has not asked for anything: this is the automation declining
        // to interrupt, and saying so every time the dial moved would be noise.
        if (workingCw && target.Mode != CivMode.Cw)
        {
            return ModeFollowDecision.Nothing;
        }

        // Already there. Not writing is the point: a command sent four times a
        // second through a tape drag would make the radio feel sluggish and the
        // app unreliable, which is the hardest kind of defect to attribute
        // (HM-DEC-050).
        if (currentMode == target.Mode && currentDataMode == target.DataMode)
        {
            return ModeFollowDecision.Nothing;
        }

        // **AND ALREADY DONE, WHICH IS A DIFFERENT QUESTION** (HM-OPEN-041). The
        // test above asks the radio and the test here asks the record of what
        // Hamlet did. A field that reads back unknown looks exactly like a radio
        // that has not been set yet, so without this the first test alone writes
        // the same command over and over at a dial nobody is touching.
        //
        // **AND IT USED TO SUPPRESS THE WRITE THAT MATTERED MOST** (work
        // instruction 042, task 1). The comment here claimed it "can only ever
        // reduce what goes out" and that was true only while the two tests
        // agreed. They disagree in exactly one situation, and it is the
        // operator's: the radio has **left** the mode Hamlet set it to. Nothing
        // ever cleared this memory, so once a successful write was recorded at
        // 14.074 the automation would never write that mode there again — the
        // operator turned the mode knob to CW, pressed the Digital tab, and
        // Hamlet declined because it remembered having done the job once. His
        // forced re-read corrected the **display**, which is why it read as
        // staleness; a re-read does not clear this memory, so the write still
        // would not have fired.
        //
        // So the memory may only speak where the ledger cannot contradict it.
        // A known field that differs from the target is the radio saying it has
        // moved, and that outranks Hamlet's recollection of its own last write
        // (§0.0). Where nothing is known to differ, the only way past the test
        // above was a field nobody has read — which is precisely the case this
        // guard was built for, and it still holds it.
        var ledgerContradictsTheMemory =
            (currentMode is { } known && known != target.Mode)
            || (currentDataMode is { } flag && flag != target.DataMode);

        if (!ledgerContradictsTheMemory
            && frequencyHz is { } hz
            && state.DoneAtHz == hz
            && state.DoneMode == target.Mode
            && state.DoneDataMode == target.DataMode)
        {
            return ModeFollowDecision.Nothing;
        }

        return new ModeFollowDecision(
            true, target.Mode, target.DataMode,
            $"Switched to {target.Name}, {target.Because}.");
    }
}
