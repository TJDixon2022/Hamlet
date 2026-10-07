using Hamlet.RadioEngine.Licensing;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **THE MAP FOLLOWS THE TAB** (work instruction 551, HM-DEC-255): what family a block of the band is painted and named as,
/// on the tab the operator is on.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-07**, on the CW tab at 7.0475 MHz, W1AW's frequency: *"Why is this region purple? It is CW,
/// right?"* The header said `Morse · yours to use` and the map painted the stretch as data, because RTTY and FT4 gather
/// there. *"When we are on the CW tab it should reflect CW, and on the Data tab it should reflect data."*</para>
/// <para>**SHARED IS THE LICENSE'S WORD, NOT THE CONVENTION'S.** A block is shared where the operator's license lets him
/// send both Morse and data over all of it: the conventions say who gathers there, and the regulation says both may. The
/// voice end allows no data, a DX window his class may not use allows nothing, and past the band edge is not amateur
/// spectrum, so none of those is shared and none changes. With his class unknown, the regulation is asked whether any
/// class may send both.</para>
/// <para>**ON THE CW TAB A SHARED BLOCK IS MORSE; ON THE DIGITAL TAB, DATA; ON THE VOICE TAB, AS THE DATA SAYS.** The map
/// and the header both read this, so they cannot disagree. It describes and never writes: nothing here reaches the radio
/// (HM-DEC-207, R111).</para>
/// </remarks>
public static class ModeLens
{
    /// <summary>What a shared block is named on the CW tab.</summary>
    public const string CwName = "CW";

    /// <summary>What a shared block is named on the Digital tab.</summary>
    public const string DataName = "Data";

    /// <summary>The family a tab paints shared blocks as, or null on the Voice tab and elsewhere.</summary>
    /// <param name="tab">The tab's name: "CW", "Digital" or "Voice".</param>
    /// <returns>The family, or null.</returns>
    public static ModeFamily? For(string? tab) => tab switch
    {
        "CW" => ModeFamily.Cw,
        "Digital" => ModeFamily.Digital,
        _ => null,
    };

    /// <summary>Whether the license lets the operator send both Morse and data over all of a block.</summary>
    /// <param name="hood">The block.</param>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's class; unknown asks whether any class may.</param>
    /// <returns>True where both may be sent everywhere in it.</returns>
    public static bool IsShared(Neighborhood hood, PrivilegePlan plan, LicenseClass licenseClass)
    {
        ArgumentNullException.ThrowIfNull(hood);
        ArgumentNullException.ThrowIfNull(plan);

        if (hood.Family is not (ModeFamily.Cw or ModeFamily.Digital or ModeFamily.Open) || hood.HighHz <= hood.LowHz)
        {
            return false;
        }

        // Both ends and the middle: privilege boundaries in this data fall on block edges or between blocks, and a block
        // that straddles one is not shared.
        long[] points = [hood.LowHz, hood.LowHz + ((hood.HighHz - hood.LowHz) / 2), hood.HighHz];

        bool May(long hz, TransmitMode mode) => licenseClass == LicenseClass.Unknown
            ? plan.LowestClassFor(hz, mode) != LicenseClass.Unknown
            : plan.Evaluate(licenseClass, hz, mode).MayTransmit;

        return points.All(hz => May(hz, TransmitMode.Cw) && May(hz, TransmitMode.Data));
    }

    /// <summary>A block as the tab paints and names it.</summary>
    /// <param name="hood">The block.</param>
    /// <param name="lens">The tab's family, or null.</param>
    /// <param name="shared">Whether the block is shared (<see cref="IsShared"/>).</param>
    /// <returns>The block with the tab's family and, where that changed it, the tab's name; the block itself otherwise.</returns>
    public static Neighborhood Apply(Neighborhood hood, ModeFamily? lens, bool shared)
    {
        ArgumentNullException.ThrowIfNull(hood);

        if (lens is not { } family || !shared || hood.Family == family)
        {
            return hood;
        }

        return hood with
        {
            Family = family,
            ShortName = family == ModeFamily.Cw ? CwName : DataName,
        };
    }

    /// <summary>A band's blocks as a tab paints and names them.</summary>
    /// <param name="hoods">The blocks.</param>
    /// <param name="tab">The tab's name.</param>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <returns>The blocks, in order, each through <see cref="Apply"/>.</returns>
    public static IReadOnlyList<Neighborhood> On(
        IReadOnlyList<Neighborhood> hoods, string? tab, PrivilegePlan plan, LicenseClass licenseClass)
    {
        ArgumentNullException.ThrowIfNull(hoods);

        var lens = For(tab);

        return lens is null
            ? hoods
            : hoods.Select(h => Apply(h, lens, IsShared(h, plan, licenseClass))).ToList();
    }
}
