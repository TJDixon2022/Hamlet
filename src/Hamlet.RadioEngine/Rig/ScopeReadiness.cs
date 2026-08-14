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
}

/// <summary>What Hamlet can say about the scope stream.</summary>
/// <param name="State">The verdict.</param>
/// <param name="IsReady">True only when sweeps should be arriving.</param>
/// <param name="Detail">One sentence, or "" when all is well.</param>
/// <param name="Citation">The manual page behind it, or "".</param>
public sealed record ScopeStatus(
    ScopeReadyState State, bool IsReady, string Detail, string Citation);

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
        "IC-7300 Full Manual, command table and footnote 4, p. 19-7";

    /// <summary>Can the scope stream reach Hamlet right now?</summary>
    /// <param name="capabilities">What the connected radio can do, or null.</param>
    /// <param name="state">Everything Hamlet has read from it.</param>
    /// <returns>The verdict, never null.</returns>
    public static ScopeStatus Check(RigCapabilities? capabilities, RigState state)
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
                + "works with the CI-V USB port set to unlink from remote and the "
                + "CI-V baud rate at 115200.", Citation);
        }

        return new ScopeStatus(ScopeReadyState.Ready, true, "", Citation);
    }
}
