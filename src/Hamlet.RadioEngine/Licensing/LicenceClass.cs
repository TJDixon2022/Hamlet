namespace Hamlet.RadioEngine.Licensing;

/// <summary>US amateur operator licence classes.</summary>
/// <remarks>
/// Ordered by privilege, weakest first, so a caller can ask whether one class
/// covers another. <see cref="Unknown"/> sorts below everything on purpose:
/// it must never satisfy a privilege comparison by accident.
/// </remarks>
public enum LicenceClass
{
    /// <summary>Not yet known. Never treated as any privilege level.</summary>
    Unknown = 0,

    /// <summary>Novice — no longer issued, still held and still valid.</summary>
    Novice = 1,

    /// <summary>Technician.</summary>
    Technician = 2,

    /// <summary>General.</summary>
    General = 3,

    /// <summary>Advanced — no longer issued, still held and still valid.</summary>
    Advanced = 4,

    /// <summary>Amateur Extra.</summary>
    Extra = 5,
}

/// <summary>How the operator's licence class came to be known.</summary>
/// <remarks>
/// Provenance travels with the value (HM-DEC-028). A class Hamlet looked up
/// and a class the operator typed are different kinds of fact, and the one
/// case where the difference decides behaviour — a lookup disagreeing with a
/// hand-set value — is exactly the case where getting it wrong would be
/// rewriting somebody's licence for them.
/// </remarks>
public enum LicenceClassSource
{
    /// <summary>Nothing has set it.</summary>
    Unset = 0,

    /// <summary>Looked up from a service republishing FCC ULS data.</summary>
    LookedUp = 1,

    /// <summary>The operator chose it themselves.</summary>
    EnteredByOperator = 2,
}

/// <summary>What an amateur station transmits.</summary>
/// <remarks>
/// The four the regulation distinguishes in 47 CFR 97.305(c). Deliberately
/// not the same axis as the neighborhood map's cultural regions: "the digital
/// corner" is where people put RTTY and PSK31 by convention, while
/// <see cref="Data"/> is where the FCC permits them. Both encodings exist and
/// neither is derived from the other (HM-DEC-029).
/// </remarks>
public enum TransmitMode
{
    /// <summary>Morse. Permitted on any frequency authorised to the operator
    /// (97.305(a)), which is why it is not in the emission table.</summary>
    Cw,

    /// <summary>RTTY and data.</summary>
    Data,

    /// <summary>Voice.</summary>
    Phone,

    /// <summary>Image, including SSTV and fax.</summary>
    Image,
}
