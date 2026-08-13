namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// The four families every mode and every stretch of band belongs to.
/// </summary>
/// <remarks>
/// <para>This is the axis Hamlet colors by, across every surface — the
/// neighborhood map, the field guide, and anything built later (HM-DEC-032).
/// Four is deliberate: it is few enough to hold in your head after seeing the
/// legend once, which is the whole point of coloring by it.</para>
/// <para>It is a CULTURAL grouping, not a regulatory one. <see cref="Digital"/>
/// covers RTTY, PSK31 and FT8 because they sound and look alike to a newcomer
/// and live together on the band, while 47 CFR 97.305 lumps RTTY and data
/// together and treats image separately. Both encodings exist, neither derives
/// from the other, and the privilege overlay uses the regulatory one
/// (HM-DEC-029).</para>
/// </remarks>
public enum ModeFamily
{
    /// <summary>Morse. Amber, everywhere.</summary>
    Cw,

    /// <summary>RTTY, PSK31, FT8, JS8 — anything a computer decodes.</summary>
    Digital,

    /// <summary>Voice.</summary>
    Phone,

    /// <summary>Open space, or a stretch that hosts a mixture.</summary>
    Open,
}
