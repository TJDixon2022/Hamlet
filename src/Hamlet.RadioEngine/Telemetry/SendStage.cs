namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// One stage of the send path, written the moment it is entered.
/// </summary>
/// <remarks>
/// <para>**A TRANSMISSION THE OPERATOR WATCHED THE RADIO MAKE PRODUCED NO RECORD
/// AT ALL** (work instruction 305 task 3). The path stopped somewhere between the
/// button and the air, and the file could not say where, because the only thing
/// written was the outcome - so a path that never started and a path that died at
/// its last step left the same nothing.</para>
/// <para>**ENTRY BEFORE OUTCOME, WHICH IS THE STANDARD THIS UNIT EXISTS FOR.**
/// Each stage writes that it was entered before it does the thing that can fail,
/// so the last stage in the file is the stage it stopped at. Unit 303 got this
/// right for exactly one call - the clock query - and it was never generalised.</para>
/// <para>**THE NAMES ARE STABLE TOKENS** (§8.1), never display strings, so a
/// comparison across two evenings is a comparison of the same words.</para>
/// <para>**AND NOTHING PERSONAL GOES IN ONE** (§2.1). A stage name, a mode and a
/// sample count describe the path; who it was addressed to and what it said do
/// not, and this payload has nowhere to put them.</para>
/// </remarks>
public static class SendStage
{
    /// <summary>The event a stage entry is written under.</summary>
    public const string EventName = "send_stage";

    /// <summary>The message was turned into a signal.</summary>
    public const string Composed = "composed";

    /// <summary>What the signal reads back as was checked.</summary>
    public const string ReadBack = "read_back";

    /// <summary>The transmission was armed for a slot boundary.</summary>
    public const string Armed = "armed";

    /// <summary>The boundary arrived and the send was handed over.</summary>
    public const string BoundaryReached = "boundary_reached";

    /// <summary>The licence gate was asked.</summary>
    public const string GateAsked = "gate_asked";

    /// <summary>The keying frame was written to the radio.</summary>
    public const string Keyed = "keyed";

    /// <summary>The samples were handed to the sound card.</summary>
    public const string HandedToTheSoundCard = "handed_to_the_sound_card";

    /// <summary>The frame that ends the transmission was written.</summary>
    public const string Unkeyed = "unkeyed";

    /// <summary>Write that a stage was entered.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="stage">Which stage, as one of the tokens above.</param>
    /// <param name="detail">A stable token or a count, or null.</param>
    /// <param name="slotted">
    /// True where this stage belongs to a send in a slot, false where it belongs to a
    /// send with no slot, and **null where the caller did not measure it** - which is
    /// every caller that existed before `PHASE_PLAN.md` §R10 made a second kind of send
    /// possible.
    /// </param>
    /// <remarks>
    /// <para>**IT IS AN ENTRY AND NEVER AN OUTCOME**, which is why there is no result
    /// here to record. What became of the stage is the next line in the file, or
    /// its absence.</para>
    /// <para>**A FIELD NOBODY MEASURED IS ABSENT, NOT FALSE** (§0.0, work instruction 323
    /// task 1c). `slotted` answers *which of the two send paths was this*, and the stages
    /// written from inside <see cref="Transmit.Ft8TransmitSequence"/> serve both, so
    /// writing `false` there would claim a no-slot send on every FT8 transmission. The
    /// key is written only where the caller knows, and its absence reads as *this line
    /// predates the question*.</para>
    /// </remarks>
    public static void Entered(
        ITelemetry? telemetry, string stage, string? detail = null, bool? slotted = null)
    {
        var bag = new Dictionary<string, object?>
        {
            ["stage"] = stage,
            ["entered"] = true,
            ["detail"] = string.IsNullOrWhiteSpace(detail)
                ? StartupSnapshot.Unknown
                : detail,
        };

        if (slotted is { } known)
        {
            bag["slotted"] = known;
        }

        telemetry?.Write(TelemetryCategory.Transmit, EventName, bag);
    }
}
