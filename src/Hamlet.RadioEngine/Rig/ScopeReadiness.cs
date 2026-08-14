namespace Hamlet.RadioEngine.Rig;

/// <summary>Why the waterfall is not receiving the radio's own spectrum.</summary>
public enum ScopeReadyState
{
    /// <summary>Everything the radio needs is in place.</summary>
    Ready,

    /// <summary>This radio has no spectrum scope (HM-DEC-030).</summary>
    NoScope,

    /// <summary>The scope itself is switched off on the radio.</summary>
    ScopeOff,

    /// <summary>The scope is on and its output to the computer is not.</summary>
    OutputOff,

    /// <summary>Hamlet has not read the two settings yet.</summary>
    NotRead,

    /// <summary>
    /// The settings read as on and no waveform data has arrived anyway.
    /// </summary>
    NothingArriving,
}

/// <summary>What Hamlet can say about the scope stream.</summary>
/// <param name="State">The verdict.</param>
/// <param name="IsReady">True only when sweeps should be arriving.</param>
/// <param name="Detail">One sentence, or "" when all is well.</param>
/// <param name="Citation">The manual page behind it, or "".</param>
/// <param name="WhereToLook">
/// The menus that control it, named as the radio names them, or "" when there
/// is nothing for anybody to go and look at.
/// </param>
public sealed record ScopeStatus(
    ScopeReadyState State, bool IsReady, string Detail, string Citation,
    string WhereToLook = "");

/// <summary>
/// Whether the radio's scope stream can reach Hamlet, and what is missing
/// (HM-DEC-062).
/// </summary>
/// <remarks>
/// <para>THE SAME SHAPE AS THE BREAK-IN PRECONDITION, and for the same reason
/// (HM-DEC-059). Command <c>27 00</c> outputs waveform data only when the scope
/// is on and the scope data output is on (p. 19-7), and the output setting adds
/// two more of its own: it can only be set with "Unlink from [REMOTE]" selected
/// on the CI-V USB port screen and 115200 on the CI-V baud rate screen
/// (footnote 4). None of that is a command Hamlet can send, and a waterfall that
/// sat empty without saying why would be the app looking broken while the answer
/// was four menu screens away.</para>
/// <para>NOTHING HERE WRITES. It reads the two settings and reports. Turning
/// somebody's scope on is a change to their radio and it is not this session's
/// to make.</para>
/// <para>Pure: capabilities and state in, a verdict out (§5).</para>
/// </remarks>
public static class ScopeReadiness
{
    /// <summary>The manual pages behind the preconditions.</summary>
    public const string Citation =
        "IC-7300 Full Manual, command table and footnote 4, p. 19-7; "
        + "the two menu settings, pp. 12-8 and 12-9 (A7292-4EX-6)";

    /// <summary>
    /// Where the two settings live, named exactly as the radio names them.
    /// </summary>
    /// <remarks>
    /// <para>THE ONE PLACE HAMLET GIVES AN INSTRUCTION, and it is narrow on
    /// purpose (HM-DEC-067, narrowing HM-DEC-050). Hamlet reports consequences
    /// rather than telling anybody how to set their radio. The exception is a
    /// feature the operator asked for that cannot work at all until a switch
    /// only they can reach is thrown, and this is that: neither of these is a
    /// command, so no amount of code makes the stream arrive.</para>
    /// <para>The names are quoted from the radio's own menus rather than
    /// paraphrased, because a paraphrase sends somebody hunting for a screen
    /// that does not exist. Read column-aware from the Full Manual, publication
    /// A7292-4EX-6, CI-V USB Port on p. 12-8 and CI-V USB Baud Rate on p. 12-9
    /// (HM-DEC-071).</para>
    /// <para>Footnote 4 on p. 19-7 writes "CI-V Baud Rate" and the radio has two
    /// screens by nearly that name, one for the [REMOTE] jack and one for the
    /// USB port. Hamlet talks to the radio over the USB cable, so the USB screen
    /// is the one that gates this and the one named here (Tim, 2026-08-14).</para>
    /// </remarks>
    public const string WhereToLook =
        "Both settings are on the radio under MENU, then SET, then Connectors. "
        + "CI-V USB Port wants Unlink from [REMOTE], and CI-V USB Baud Rate wants "
        + "115200, which is the rate Hamlet talks at anyway.";

    /// <summary>
    /// What Hamlet says when the settings look right and nothing arrives.
    /// </summary>
    public const string NothingArrivingDetail =
        "The radio is not sending waveform data. Hamlet reads the two settings "
        + "that control it and they both look right from here, so the next thing "
        + "worth a look is the pair on the radio's own menus.";

    /// <summary>Can the scope stream reach Hamlet right now?</summary>
    /// <param name="capabilities">What the connected radio can do, or null.</param>
    /// <param name="state">Everything Hamlet has read from it.</param>
    /// <param name="sweepsSeen">
    /// How many sweeps have actually arrived, or -1 when nobody is counting.
    /// Zero with everything switched on is its own answer, and it is the one
    /// somebody stares at (HM-DEC-067).
    /// </param>
    /// <returns>The verdict, never null.</returns>
    public static ScopeStatus Check(
        RigCapabilities? capabilities, RigState state, long sweepsSeen = -1)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (capabilities is null)
        {
            return new ScopeStatus(
                ScopeReadyState.NotRead, false,
                "Nothing is connected, so there is no spectrum to draw.", "");
        }

        if (!capabilities.HasSpectrumScope)
        {
            return new ScopeStatus(
                ScopeReadyState.NoScope, false,
                $"{capabilities.Model} has no spectrum scope, so there is nothing "
                + "for the waterfall to show.", "");
        }

        var on = state[RigField.ScopeOn];
        var output = state[RigField.ScopeOutput];

        if (!on.IsKnown || !output.IsKnown)
        {
            return new ScopeStatus(
                ScopeReadyState.NotRead, false,
                "Hamlet has not read the scope settings yet. It asks for them on "
                + "connect, so give it a moment.", Citation);
        }

        if (on.Number is 0)
        {
            return new ScopeStatus(
                ScopeReadyState.ScopeOff, false,
                "The scope is switched off on the radio. Turn it on there and the "
                + "waterfall fills in.", Citation);
        }

        if (output.Number is 0)
        {
            return new ScopeStatus(
                ScopeReadyState.OutputOff, false,
                "The scope is running on the radio and it is not sending the data "
                + "to the computer. That switch lives on the radio, and it only "
                + "works alongside two settings there.", Citation, WhereToLook);
        }

        // EVERYTHING READS AS ON AND NOTHING HAS ARRIVED, which is the case
        // somebody actually sits and stares at (HM-DEC-067). An empty waterfall
        // that says nothing looks like a broken program, and the answer is
        // usually a pair of menu screens away.
        if (sweepsSeen is 0)
        {
            return new ScopeStatus(
                ScopeReadyState.NothingArriving, false,
                NothingArrivingDetail, Citation, WhereToLook);
        }

        return new ScopeStatus(ScopeReadyState.Ready, true, "", Citation);
    }
}
