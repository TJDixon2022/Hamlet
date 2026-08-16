using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Whether the operator's license covers transmitting here (HM-DEC-089).
/// </summary>
/// <param name="LicenseClass">What they hold, or Unknown.</param>
/// <param name="FrequencyHz">Where they are, or 0 when that is not known.</param>
/// <param name="GuardEnabled">
/// The operator's "only let me transmit where my license allows" setting, which
/// ships on (HM-DEC-029).
/// </param>
/// <param name="Plan">The privileges data, or null for the shipped one.</param>
/// <remarks>
/// <para>**HAMLET KNOWS THE CLASS, KNOWS THE FREQUENCY, AND ALREADY PRINTS THE
/// PRIVILEGE LINE WITH ITS CITATION. It must not offer a send the operator is
/// not licensed to make.** The guard has refused at the moment of the press
/// since HM-DEC-029; what this adds is that the buttons say so beforehand, and
/// that the refusal is a readiness state like every other, so it reaches the
/// decision record with everything that decided it (HM-DEC-077).</para>
/// <para>**ONE SOURCE.** It reads the same <see cref="PrivilegePlan"/> that the
/// band map's own "yours to use" line reads. Two sources for one fact is how the
/// frequency row went wrong.</para>
/// </remarks>
public sealed record TransmitPrivileges(
    LicenseClass LicenseClass,
    long FrequencyHz,
    bool GuardEnabled,
    PrivilegePlan? Plan = null)
{
    /// <summary>The shipped privileges data, built once.</summary>
    private static readonly PrivilegePlan Shipped = new();

    /// <summary>
    /// The verdict, or null when nothing here stands in the way.
    /// </summary>
    /// <param name="saw">What the readiness check has looked at so far.</param>
    /// <returns>A refusal, or null to carry on.</returns>
    public CwReadiness? Judge(List<DeterminedBy> saw)
    {
        ArgumentNullException.ThrowIfNull(saw);

        saw.Add(DeterminedBy.Fact("guardEnabled", GuardEnabled ? 1 : 0));
        saw.Add(DeterminedBy.Fact("frequencyHz", FrequencyHz));
        saw.Add(DeterminedBy.Fact("licenseClass", (int)LicenseClass));

        // THE GUARD IS THE OPERATOR'S TO SWITCH OFF, and that is what keeps this
        // from ever locking somebody out of their own transmitter, which is what
        // HM-DEC-065 was protecting (HM-DEC-029).
        if (!GuardEnabled)
        {
            return null;
        }

        if (FrequencyHz <= 0)
        {
            return new CwReadiness(
                CwReadyState.FrequencyUnknown, false,
                "Hamlet does not know what frequency the radio is on, so it cannot "
                + "tell whether your license covers transmitting here. It reads that "
                + "on connect, so give it a moment.",
                "", saw);
        }

        if (LicenseClass == LicenseClass.Unknown)
        {
            return new CwReadiness(
                CwReadyState.LicenseClassUnknown, false,
                "Hamlet does not know which class your license is, so it cannot "
                + "check this frequency against it. Set your class in Settings and "
                + "the button comes back. You can also switch the privilege check "
                + "off in Settings, which hands the decision back to you.",
                "", saw);
        }

        var plan = Plan ?? Shipped;
        var verdict = plan.Evaluate(LicenseClass, FrequencyHz, TransmitMode.Cw);

        if (verdict.MayTransmit)
        {
            return null;
        }

        // TWO REFUSALS, NOT ONE. Being outside the class's band entirely is a
        // stretch to move out of; a stretch the class holds but not in this mode
        // is one to listen in, and they call for different things (HM-DEC-029).
        var state = verdict.Status == PrivilegeStatus.ModeNotAuthorised
            ? CwReadyState.ListenOnly
            : CwReadyState.OutsidePrivileges;

        return new CwReadiness(
            state, false, verdict.Explanation + Elsewhere(plan), verdict.Citation, saw);
    }

    /// <summary>
    /// Where the operator could go instead, when there is somewhere.
    /// </summary>
    /// <returns>A sentence naming a frequency they may use, or "".</returns>
    /// <remarks>
    /// <para>**A DISABLED BUTTON THAT ONLY SAYS NO IS A DEAD END**, and this
    /// application exists for somebody who does not yet know where they are
    /// allowed to be. The nearest edge of the nearest stretch their own license
    /// covers is the one fact that turns a refusal into a next step.</para>
    /// <para>It says where and never tunes there. Moving somebody's radio because
    /// they pressed a send button would be Hamlet deciding something that is
    /// theirs (§0.4).</para>
    /// </remarks>
    private string Elsewhere(PrivilegePlan plan)
    {
        var band = Bands.BandPlan.BandFor(FrequencyHz);

        if (band is null)
        {
            return "";
        }

        var nearest = plan
            .SpansFor(band, LicenseClass)
            .Where(s => s.MayTransmit)
            .Select(s => Nearest(s))
            .OrderBy(hz => Math.Abs(hz - FrequencyHz))
            .FirstOrDefault();

        return nearest == 0
            ? ""
            : $" On this band your license covers {Megahertz(nearest)}, "
              + "so tuning there would let you call.";
    }

    /// <summary>The point in a span closest to where the operator is.</summary>
    private long Nearest(PrivilegeSpan span)
        => FrequencyHz < span.LowHz ? span.LowHz
            : FrequencyHz > span.HighHz ? span.HighHz
            : FrequencyHz;

    /// <summary>A frequency the way an operator says it.</summary>
    private static string Megahertz(long hz)
        => (hz / 1_000_000.0).ToString("0.000") + " MHz";
}
