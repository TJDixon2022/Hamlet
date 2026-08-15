using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What Hamlet says beside the send controls on the day the antenna is real
/// (HM-DEC-074).
/// </summary>
/// <remarks>
/// <para>THE DUMMY LOAD LINE HAS DONE ITS JOB AND IS RETIRED. It said to key
/// into a dummy load because the keying code had never run, which was true and
/// is not any more (HM-DEC-008). Leaving it up would be the app telling somebody
/// something it no longer believes, and a warning nobody needs is a warning
/// everybody learns to read past.</para>
/// <para>WHAT REPLACES IT IS SHORTER AND TRUE. Hamlet cannot tell what is
/// connected to the antenna socket: nothing in the CI-V read table reports it,
/// and the SWR meter only says anything while transmitting. So it does not
/// pretend to know. It says once, calmly, that the operator is the one who knows
/// which one he is on, and stops.</para>
/// <para>EVERY LINE HERE IS A CONSEQUENCE AND NOT AN INSTRUCTION, the same
/// treatment the noise controls got (HM-DEC-050,
/// <see cref="RigObservations"/>). It names a value that was read, says what
/// follows from it, and stops. No imperative, nothing that says anything is
/// broken, and nothing said from a value that was not read.</para>
/// <para>Pure: state in, sentences out (§5).</para>
/// </remarks>
public static class TransmitNotes
{
    /// <summary>
    /// What Hamlet says about what the antenna socket is connected to.
    /// </summary>
    /// <remarks>
    /// Said once, above the buttons, where it is read before a first press
    /// rather than after. It is not a caution and it does not scold: knowing
    /// which one is on the back of the radio is ordinary operating, and the
    /// sentence exists because the app used to say something stronger and no
    /// longer should.
    /// </remarks>
    public const string WhatIsConnected =
        "Hamlet cannot see what is on the back of the radio, so it will not "
        + "guess whether this is going into a dummy load or an antenna. You are "
        + "the one who knows which, and it is worth knowing before the first "
        + "press rather than after.";

    /// <summary>
    /// Below this, the power setting is worth a sentence.
    /// </summary>
    /// <remarks>
    /// A quarter of the radio's range. The reason to say anything at all is the
    /// specific evening this was written for: somebody turns the power down for
    /// a dummy load test, connects an antenna, and cannot work out why the band
    /// has gone quiet. The number is read from the radio rather than assumed.
    /// </remarks>
    public const int QuietPowerPercent = 25;

    /// <summary>At or above this, the radio is effectively wide open.</summary>
    public const int FullPowerPercent = 95;

    /// <summary>
    /// What the power setting means for a call, or "" when there is nothing to
    /// say or nothing was read.
    /// </summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <returns>One sentence, or "".</returns>
    /// <remarks>
    /// <para>A PERCENTAGE AND NEVER A WATTAGE. The radio reports this as a
    /// position on its own scale (Full Manual p. 19-3), and turning that into
    /// watts needs the radio's power curve, which §4 does not have a citation
    /// for. A figure in watts would be Hamlet inventing a number on the one
    /// screen where a number decides whether somebody keys a transmitter
    /// (§0.0).</para>
    /// <para>Nothing is said in the middle of the range, because a radio set
    /// somewhere ordinary is not news and a line that always appears is a line
    /// nobody reads.</para>
    /// </remarks>
    public static string PowerNote(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var power = state[RigField.RfPower];

        if (!power.IsKnown || power.Number is not { } percent)
        {
            return "";
        }

        if (percent <= QuietPowerPercent)
        {
            return $"The radio's power is set to about {percent} percent of its "
                + "range. That is a real signal and plenty of people work the "
                + "world on far less, and it also means a thin report says "
                + "something about the power rather than about your sending.";
        }

        return percent >= FullPowerPercent
            ? "The radio's power is near the top of its range."
            : "";
    }

    /// <summary>
    /// Everything worth saying beside the send controls right now.
    /// </summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <returns>The lines, in reading order, possibly just the one.</returns>
    public static IReadOnlyList<string> For(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var said = new List<string> { WhatIsConnected };
        var power = PowerNote(state);

        if (power.Length > 0)
        {
            said.Add(power);
        }

        return said;
    }
}
