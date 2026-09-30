namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One mark the envelope detector called: when it began and ended, the pitch it was keyed at,
/// how loud it was, and how far it stood over its gaps (work instruction 490, R103).
/// </summary>
/// <param name="Sequence">The order the detector called it in, from one; a reader asks for the marks after the last it saw.</param>
/// <param name="FromSeconds">Where it began, on the audio clock of every sample the detector was handed.</param>
/// <param name="ToSeconds">Where it ended, on the same clock.</param>
/// <param name="PitchHz">The bin at the peak of the tone's lobe over the mark's own hops.</param>
/// <param name="LevelDb">That bin's mean level over the mark's own hops, in dB.</param>
/// <param name="ContrastDb">That level over the bin's measured gap level, in dB; NaN where the bin has none.</param>
/// <remarks>
/// **THE THREE MEASUREMENTS THE DECODER WAS NEVER GIVEN** (R103). The owner: *"frequency,
/// amplitude, and duration. Those define a character."* The detector measured all three for every
/// bar it called and handed the decoder a pitch to mix at and nothing else.
/// </remarks>
public sealed record CwMark(
    long Sequence,
    double FromSeconds,
    double ToSeconds,
    double PitchHz,
    double LevelDb,
    double ContrastDb)
{
    /// <summary>How long the key was down, in milliseconds.</summary>
    public double LengthMs => (ToSeconds - FromSeconds) * 1000;

    /// <summary>
    /// Whether the detector was keying at the mark's peak, or within two bins of it, when it was
    /// called: bars there had paired and cleared their gaps' wander in the last second (work
    /// instruction 493). A station's second mark on is keyed; noise never is.
    /// </summary>
    public bool Keyed { get; init; }

    /// <summary>
    /// How far the bar sat inside the shape of a keyed tone when it was called: its five properties
    /// and their product (work instruction 502, R110). Null for a mark not built by the detector.
    /// </summary>
    public CwMarkShape? Shape { get; init; }
}

/// <summary>
/// How far one bar sits inside the shape of a keyed tone: five properties, each from nought to one
/// as a distance from the ideal, and their product (work instruction 502, R110, HM-DEC-206).
/// </summary>
/// <param name="Flatness">One less the top's RMS wander over the flatness tolerance at its contrast.</param>
/// <param name="Edges">For the rise and the fall, two over the hops each took to half amplitude, at most one; multiplied.</param>
/// <param name="Narrowness">Its bin over the louder of the bins 300 Hz either side, over 15 dB, at most one.</param>
/// <param name="Contrast">Its level over its gaps, over 15 dB, at most one.</param>
/// <param name="Length">One up to a 5 WPM dah, and that over its length beyond.</param>
public sealed record CwMarkShape(double Flatness, double Edges, double Narrowness, double Contrast, double Length)
{
    /// <summary>The product of the five: how far inside the shape the bar sits, nought to one.</summary>
    public double Score => Flatness * Edges * Narrowness * Contrast * Length;

    /// <summary>The five and the score, as the hover and the report print them.</summary>
    public override string ToString()
        => string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"shape {Score:0.00} (flat {Flatness:0.00}, edges {Edges:0.00}, narrow {Narrowness:0.00}, contrast {Contrast:0.00}, length {Length:0.00})");
}

/// <summary>The marks called since a reader last asked, and how much audio the detector has heard.</summary>
/// <param name="Marks">The marks, in the order they were called.</param>
/// <param name="HeardSeconds">The detector's audio clock now, in seconds.</param>
public sealed record CwMarkBatch(IReadOnlyList<CwMark> Marks, double HeardSeconds);
