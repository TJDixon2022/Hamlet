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
}

/// <summary>The marks called since a reader last asked, and how much audio the detector has heard.</summary>
/// <param name="Marks">The marks, in the order they were called.</param>
/// <param name="HeardSeconds">The detector's audio clock now, in seconds.</param>
public sealed record CwMarkBatch(IReadOnlyList<CwMark> Marks, double HeardSeconds);
