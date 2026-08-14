namespace Hamlet.RadioEngine.Licensing;

/// <summary>What the guard rail decided about a transmit attempt.</summary>
/// <param name="MayTransmit">True when keying is permitted to proceed.</param>
/// <param name="Reason">Why, in plain language; empty when permitted.</param>
/// <param name="Citation">The paragraph behind a refusal, or "".</param>
/// <param name="WasOverridden">
/// True when the operator had switched the guard off, so the transmit is
/// proceeding on their own authority.
/// </param>
public sealed record TransmitDecision(
    bool MayTransmit, string Reason, string Citation, bool WasOverridden);

/// <summary>
/// The guard rail: the one place that decides whether Hamlet will key the
/// transmitter.
/// </summary>
/// <remarks>
/// <para>TRANSMIT ONLY, and that boundary is the whole design (HM-DEC-029).
/// Listening is never restricted — any license may receive anywhere, and a
/// program that implied otherwise would be teaching a beginner something
/// false about their own license. Nothing here is consulted when tuning,
/// when receiving, or when drawing. It is asked one question, at one moment:
/// may this key down.</para>
/// <para>THE TRANSMIT PATH IS HERE NOW, and it comes through this class.
/// <c>CwTransmitter</c> calls <see cref="Check"/> before it touches the radio,
/// every time, and honors the answer (HM-DEC-059). It was built ahead of that
/// path with its tests, so the day CW send landed the check was already proved
/// and the only remaining work was the call site. This paragraph used to say no
/// transmit path existed, which was true when it was written.</para>
/// <para>The override is deliberately not a setting to be forgotten. It is
/// passed in per call, so the decision records whether it was used and the UI
/// can keep it beside the transmit control rather than buried three screens
/// away: somebody deliberately keying outside their privileges should reach
/// for it consciously, and somebody tuning around should never meet it.</para>
/// </remarks>
public sealed class TransmitGuard
{
    private readonly PrivilegePlan _plan;

    /// <summary>Creates a guard over the shipped privileges data.</summary>
    public TransmitGuard()
        : this(new PrivilegePlan())
    {
    }

    /// <summary>Creates a guard over a supplied plan.</summary>
    /// <param name="plan">The privileges plan.</param>
    public TransmitGuard(PrivilegePlan plan) => _plan = plan;

    /// <summary>
    /// May Hamlet key the transmitter here?
    /// </summary>
    /// <param name="licenseClass">The operator's license class.</param>
    /// <param name="frequencyHz">Where they would transmit.</param>
    /// <param name="mode">What they would transmit.</param>
    /// <param name="guardEnabled">
    /// The operator's "only let me transmit where my license allows" setting.
    /// </param>
    /// <returns>The decision, with its reason and citation.</returns>
    /// <remarks>
    /// An unknown license class does not block transmitting. Hamlet has no
    /// business refusing to key a radio because it could not reach a lookup
    /// service — the operator holds the license and knows what it says. It
    /// warns, names what it does not know, and gets out of the way.
    /// </remarks>
    public TransmitDecision Check(
        LicenseClass licenseClass, long frequencyHz, TransmitMode mode, bool guardEnabled)
    {
        var verdict = _plan.Evaluate(licenseClass, frequencyHz, mode);

        if (verdict.MayTransmit)
        {
            return new TransmitDecision(true, "", verdict.Citation, false);
        }

        if (verdict.Status == PrivilegeStatus.Unknown)
        {
            return new TransmitDecision(
                true,
                "Hamlet does not know your license class, so it is not checking this "
                + "transmission. Set your class in Settings and it will.",
                "",
                false);
        }

        if (!guardEnabled)
        {
            return new TransmitDecision(true, verdict.Explanation, verdict.Citation, true);
        }

        return new TransmitDecision(false, verdict.Explanation, verdict.Citation, false);
    }
}
