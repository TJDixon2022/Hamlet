using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Cw;

/// <summary>The five links between pressing a button and being heard.</summary>
/// <remarks>
/// ONLY THE LAST ONE IS ABOUT OTHER PEOPLE, and that is the whole insight. Four
/// of the five are machine-checkable and none of those four need another human
/// being to cooperate (HM-DEC-082).
/// </remarks>
public enum TransmitLink
{
    /// <summary>Hamlet handed the message to the radio and it acknowledged.</summary>
    CommandSent,

    /// <summary>The radio keyed.</summary>
    RadioKeyed,

    /// <summary>The amplifier made power.</summary>
    PowerMade,

    /// <summary>The power went into a real load.</summary>
    LoadMatched,

    /// <summary>Somebody was listening, and copied it.</summary>
    Copied,
}

/// <summary>
/// Everything Hamlet measured about one transmission (HM-DEC-082).
/// </summary>
/// <param name="Acknowledged">The radio took the message.</param>
/// <param name="KeyedSeconds">How long it keyed, or null when unmeasured.</param>
/// <param name="PowerReading">The Po meter's worst-case reading, or null.</param>
/// <param name="SwrReading">The SWR meter's worst-case reading, or null.</param>
/// <param name="Reports">How many skimmers reported this operator.</param>
/// <param name="SkimmersListening">
/// How many skimmers were reporting on this band, or null when it could not be
/// obtained. **Null and zero are different facts** and must stay so.
/// </param>
/// <param name="BandName">Which band, for the sentence.</param>
public sealed record TransmitEvidence(
    bool Acknowledged,
    double? KeyedSeconds,
    int? PowerReading,
    int? SwrReading,
    int Reports,
    int? SkimmersListening,
    string BandName);

/// <summary>
/// What happened between pressing the button and being heard, link by link
/// (HM-DEC-082).
/// </summary>
/// <remarks>
/// <para>THE QUESTION THIS APPLICATION EXISTS TO ANSWER. "Am I speaking into the
/// void, as in nothing is going out, or am I on the air and nobody is
/// listening?" Six years of not being able to tell those apart is the problem
/// Hamlet was built for, and everything else in it is scaffolding around this
/// one sentence.</para>
/// <para>A FAILURE AT LINK 3 AND A FAILURE AT LINK 5 ARE COMPLETELY DIFFERENT
/// FACTS ABOUT THE WORLD, and until now they looked identical to the operator:
/// silence. One means his station is broken. The other means his station works
/// and the band was short or nobody was pointed his way. He cannot act on the
/// first without knowing it is the first.</para>
/// <para>**EVERY NUMBER IS MEASURED OR IT IS NOT SHOWN.** §0.0 governs here more
/// tightly than anywhere else in the application. A link Hamlet could not read
/// says so, because "Hamlet could not read the power meter" is honest and
/// useful, while a plausible figure is a guess dressed as a decode and destroys
/// the only thing this feature is for.</para>
/// <para>**AND IT NEVER DIAGNOSES THE STATION.** "Made no power" is a reading.
/// "Your antenna is disconnected" is a guess about somebody's equipment, and the
/// prohibition that governs the SWR report (HM-DEC-081) governs the whole chain.
/// Hamlet reports measurements and the operator draws conclusions, because he is
/// the one standing next to the radio.</para>
/// <para>Pure: evidence in, sentences out. No clock, no radio (§5).</para>
/// </remarks>
public static class TransmitChain
{
    /// <summary>
    /// Where the chain broke, or null when nothing did.
    /// </summary>
    /// <param name="evidence">What was measured.</param>
    /// <returns>The failed link, or null.</returns>
    /// <remarks>
    /// A LINK THAT COULD NOT BE READ IS NOT A FAILED LINK. Not knowing whether
    /// power was made is a different thing from knowing none was, and reporting
    /// the first as the second would tell somebody their station is broken on the
    /// strength of a read that did not come back.
    /// </remarks>
    public static TransmitLink? BrokeAt(TransmitEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        if (!evidence.Acknowledged)
        {
            return TransmitLink.CommandSent;
        }

        if (evidence.KeyedSeconds is <= 0)
        {
            return TransmitLink.RadioKeyed;
        }

        if (evidence.PowerReading is { } power && CivPowerOut.IsSilent(power))
        {
            return TransmitLink.PowerMade;
        }

        return evidence.Reports == 0 ? TransmitLink.Copied : null;
    }

    /// <summary>
    /// What to tell the operator after a send.
    /// </summary>
    /// <param name="evidence">What was measured.</param>
    /// <returns>Two or three sentences, in the app's voice.</returns>
    /// <remarks>
    /// LINKS THAT SUCCEEDED ARE STATED BRIEFLY AND THE ONE THAT FAILED GETS THE
    /// WORDS. Somebody whose station is working does not want a five line audit
    /// every time he calls, and somebody whose station is not wants to know which
    /// part.
    /// </remarks>
    public static string Describe(TransmitEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        // LINK 1. The radio never took it, so nothing below this is known and
        // none of it may be guessed at.
        if (!evidence.Acknowledged)
        {
            return "The radio did not take that message, so Hamlet cannot say "
                + "what went out. Nothing is repeated automatically.";
        }

        // LINK 2. It never keyed, so there was nothing to measure after it.
        if (evidence.KeyedSeconds is <= 0)
        {
            return "Hamlet handed the message over and the radio never keyed, so "
                + "nothing went on the air.";
        }

        var keyed = Keyed(evidence.KeyedSeconds);

        // LINK 3, and this is the one that separates a broken station from a
        // quiet band. It is the reason the whole chain exists.
        if (evidence.PowerReading is { } reading && CivPowerOut.IsSilent(reading))
        {
            return $"The radio keyed{keyed} and made no power. Nothing went on "
                + "the air.";
        }

        var made = Made(evidence);

        if (evidence.PowerReading is null)
        {
            return $"Your call went out and the radio keyed{keyed}. Hamlet could "
                + "not read the power meter, so it cannot say whether anything "
                + "left the antenna. " + Listening(evidence);
        }

        return $"Your call went out. The radio keyed{keyed} and made {made}. "
            + Listening(evidence);
    }

    /// <summary>How long it keyed, as a clause, or "".</summary>
    private static string Keyed(double? seconds)
        => seconds is { } value and > 0
            ? $" for {value.ToString("0", System.Globalization.CultureInfo.InvariantCulture)} seconds"
            : "";

    /// <summary>What it made, and into what.</summary>
    /// <remarks>
    /// A PERCENTAGE AND NEVER A WATTAGE (HM-DEC-074, HM-DEC-082). The meter
    /// reports a position on its own scale and Icom's faces are not linear in
    /// watts, so the arithmetic that looks obvious is wrong and §4 has no
    /// citation for the real curve.
    /// </remarks>
    private static string Made(TransmitEvidence evidence)
    {
        var power = evidence.PowerReading is { } reading
            ? CivPowerOut.Describe(reading)
            : "";

        if (evidence.SwrReading is not { } swr)
        {
            return power.Length > 0
                ? power
                : "power Hamlet could not measure";
        }

        var match = CivSwr.Describe(swr);

        return power.Length > 0
            ? $"{power} into a {match} match"
            : $"power Hamlet could not measure, into a {match} match";
    }

    /// <summary>
    /// Who was listening, and whether anybody copied it.
    /// </summary>
    /// <remarks>
    /// <para>ZERO SKIMMERS AND AN UNKNOWN NUMBER OF SKIMMERS ARE DIFFERENT
    /// EVENTS. "None of them copied you" is worth nothing without knowing how
    /// many "them" there were, and an absent number reads as zero to somebody who
    /// has been disappointed before. So a count that could not be obtained says
    /// it could not be obtained.</para>
    /// <para>AND THE COUNT IS OF SKIMMERS THAT REPORTED SOMEBODY, which is a
    /// lower bound on how many were awake rather than a census of who was
    /// listening. A machine hearing nothing publishes nothing, so Hamlet cannot
    /// count it, and saying "41 were listening" would claim more than the
    /// measurement supports.</para>
    /// </remarks>
    private static string Listening(TransmitEvidence evidence)
    {
        var band = evidence.BandName.Length > 0
            ? $" on {evidence.BandName}"
            : "";

        if (evidence.Reports > 0)
        {
            return evidence.Reports == 1
                ? "One skimmer reported hearing you."
                : $"{evidence.Reports} skimmers reported hearing you.";
        }

        if (evidence.SkimmersListening is not { } skimmers)
        {
            return "Hamlet could not find out how many skimmers were awake"
                + $"{band}, so it cannot say whether anybody was in a position "
                + "to hear you. None reported you.";
        }

        if (skimmers == 0)
        {
            return $"No skimmer reported hearing anybody{band} while you were "
                + "calling, so there may have been no machine listening at all. "
                + "That is not the same as nobody hearing you.";
        }

        var many = skimmers == 1
            ? "One skimmer was reporting other stations"
            : $"{skimmers} skimmers were reporting other stations";

        return $"{many}{band} while you were calling, and none of them reported "
            + "you. Skimmer coverage is uneven, so that is not proof nobody "
            + "heard you.";
    }
}
