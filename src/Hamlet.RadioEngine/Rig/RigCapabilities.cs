namespace Hamlet.RadioEngine.Rig;

/// <summary>What a particular radio can actually do.</summary>
/// <param name="Model">Display name, e.g. "IC-7300".</param>
/// <param name="HasSpectrumScope">True when it streams a spectrum scope.</param>
/// <param name="HasBuiltInCwKeyer">True when it keys Morse from sent text.</param>
/// <param name="HasUsbAudio">True when it presents a USB audio codec.</param>
/// <param name="CanTransmit">True when it can be made to transmit at all.</param>
/// <param name="SupportedBandNames">
/// Band names from <see cref="Bands.BandPlan"/> the radio covers.
/// </param>
/// <remarks>
/// <para>HM-DEC-003 kept Hamlet to one radio behind an <c>IRig</c> interface,
/// and named multi-rig support as the condition for revisiting. This is that
/// revisit arriving early and cheaply (HM-DEC-030): the UI stops assuming
/// IC-7300 features, and a radio without one degrades honestly instead of
/// showing a control that does nothing.</para>
/// <para>Reported by the implementation, never configured on it — the same
/// shape as <see cref="IRig.IsSimulated"/>, and for the same reason. A radio
/// is the only thing that knows what it is.</para>
/// </remarks>
public sealed record RigCapabilities(
    string Model,
    bool HasSpectrumScope,
    bool HasBuiltInCwKeyer,
    bool HasUsbAudio,
    bool CanTransmit,
    IReadOnlyList<string> SupportedBandNames)
{
    /// <summary>True when this radio covers the named band.</summary>
    /// <param name="bandName">Band name, e.g. "40 m".</param>
    /// <returns>True when supported.</returns>
    public bool Supports(string bandName)
        => SupportedBandNames.Contains(bandName, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// A capability set claiming nothing, for a radio that has not said.
    /// </summary>
    /// <remarks>
    /// Every flag false. An unknown radio must not inherit the IC-7300's
    /// features by default — that is the assumption this type exists to
    /// remove.
    /// </remarks>
    public static RigCapabilities Unknown { get; } = new(
        "unknown radio",
        HasSpectrumScope: false,
        HasBuiltInCwKeyer: false,
        HasUsbAudio: false,
        CanTransmit: false,
        Array.Empty<string>());
}
