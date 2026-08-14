using Hamlet.RadioEngine.Licensing;

namespace Hamlet.RadioEngine.Bands;

/// <summary>
/// Where a frequency stands in relation to the amateur bands.
/// </summary>
/// <param name="IsAmateur">
/// True when this frequency is inside an amateur allocation.
/// </param>
/// <param name="Headline">
/// The short verdict a surface can put beside the frequency, e.g. "not an
/// amateur band".
/// </param>
/// <param name="Detail">
/// The whole of it in plain language. Deliberately WITHOUT the sentence about
/// listening: every surface that shows this has its own place for that, and
/// saying it twice in one card reads as the app laboring the point.
/// </param>
/// <param name="Citation">The paragraph behind it, or "".</param>
/// <param name="NearestBand">
/// The amateur band closest to this frequency, so a surface can say which edge
/// was just crossed, or null when nothing is near.
/// </param>
public sealed record SpectrumStanding(
    bool IsAmateur,
    string Headline,
    string Detail,
    string Citation,
    CwBand? NearestBand);

/// <summary>
/// Whether a frequency is amateur spectrum at all, derived once and read by
/// every surface that speaks (HM-DEC-055).
/// </summary>
/// <remarks>
/// <para>THE FAILURE THIS EXISTS TO END. The operator tuned to the very top
/// edge of 20 m and the card said "yours to use, call away". A little further
/// and there is no amateur spectrum at all up there; the frequency belongs to
/// other services entirely. The privilege overlay was reading "past the end of
/// my data" as "no restriction found", which inverts the meaning of the
/// silence, in the one place where a confident error has legal
/// consequences (§0.0, HM-DEC-029).</para>
/// <para>ONE DERIVATION, EVERY SURFACE, which is HM-DEC-046's pattern applied
/// to the band edge. The map, the card, the dial tape and the rig display all
/// read this, so no two of them can disagree about whether the operator is
/// still on a ham band.</para>
/// <para>It is derived from the cited Part 97 data rather than from the band
/// plan's own edges, because the Extra class by definition reaches every band
/// edge and that table is already carried, checked and cited. A second copy of
/// a band edge is a second copy until they disagree (§0).</para>
/// <para>NOTHING HERE STOPS A DIAL. Tuning is never restricted and receiving is
/// never restricted (HM-DEC-029). The protection is the screen telling the
/// truth, not a locked control.</para>
/// </remarks>
public static class AmateurSpectrum
{
    /// <summary>The sentence that has to be on any out-of-band surface.</summary>
    public const string ListeningIsStillFine =
        "Listening here is fine, the same as anywhere. Nothing about this "
        + "restricts what you can receive.";

    /// <summary>True when the frequency is inside an amateur allocation.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <returns>True when it is amateur spectrum.</returns>
    public static bool IsAmateur(long frequencyHz)
        => IsAmateur(frequencyHz, PrivilegeData.Current);

    /// <summary>True when the frequency is inside an amateur allocation.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="privileges">The cited Part 97 data.</param>
    /// <returns>True when it is amateur spectrum.</returns>
    /// <remarks>
    /// Read against the Extra class, which reaches every band edge, so this
    /// answers "is this a ham band at all" rather than "may this operator
    /// transmit here". Those are different questions and only one of them
    /// depends on who is asking.
    /// </remarks>
    public static bool IsAmateur(long frequencyHz, PrivilegeData privileges)
    {
        ArgumentNullException.ThrowIfNull(privileges);

        return privileges.ClassBands.TryGetValue(LicenseClass.Extra, out var ranges)
               && ranges.Any(r => r.Contains(frequencyHz));
    }

    /// <summary>Work out where a frequency stands.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <returns>The standing, never null.</returns>
    public static SpectrumStanding Describe(long frequencyHz)
        => Describe(frequencyHz, PrivilegeData.Current);

    /// <summary>Work out where a frequency stands.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="privileges">The cited Part 97 data.</param>
    /// <returns>The standing, never null.</returns>
    public static SpectrumStanding Describe(long frequencyHz, PrivilegeData privileges)
    {
        ArgumentNullException.ThrowIfNull(privileges);

        var nearest = Nearest(frequencyHz);

        if (IsAmateur(frequencyHz, privileges))
        {
            return new SpectrumStanding(true, "", "", "", nearest);
        }

        // Which edge was crossed, in words, because "you have gone past the top
        // of 20 m" is a thing somebody can act on and "out of band" is not.
        var which = nearest is null
            ? "outside every amateur band"
            : frequencyHz > nearest.HighHz
                ? $"past the top of {nearest.Name}"
                : $"below the bottom of {nearest.Name}";

        return new SpectrumStanding(
            false,
            "not an amateur band",
            $"You are {which}, and this is not amateur spectrum at all. Other "
            + "services live up and down the dial from every ham band, and no "
            + "amateur license permits transmitting on any of them.",
            "97.301",
            nearest);
    }

    /// <summary>The amateur band nearest a frequency, or null when none is.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <returns>The band, or null.</returns>
    /// <remarks>
    /// Nearest by distance to the band rather than to its center, so a
    /// frequency a hair above 20 m answers 20 m rather than whichever band
    /// happens to be widest.
    /// </remarks>
    public static CwBand? Nearest(long frequencyHz)
        => BandPlan.Bands
            .OrderBy(b => Distance(b, frequencyHz))
            .FirstOrDefault();

    private static long Distance(CwBand band, long frequencyHz)
        => frequencyHz < band.LowHz ? band.LowHz - frequencyHz
            : frequencyHz > band.HighHz ? frequencyHz - band.HighHz
            : 0;
}
