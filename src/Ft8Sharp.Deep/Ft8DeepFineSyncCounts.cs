namespace Ft8Sharp.Deep;

/// <summary>
/// <b>What the fine synchronisation stage did in one slot, beside the five counts the port already
/// returns.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>A rate that moved with no visible re-sync activity behind it is not evidence</b>, which is the
/// only reason these exist. They live here rather than on <c>Ft8SlotResult</c> because that is the
/// port's own record and this phase changes no line of the port.
/// </para>
/// <para>
/// <b>The offset distribution is what says the search is doing what it claims.</b> A stage that
/// reported candidates re-synced but moved every one of them by zero would be a stage that ran and
/// did nothing, and the mean and worst shift are what distinguish the two.
/// </para>
/// </remarks>
/// <param name="Offered">
/// Candidates the port's gates refused, which is the set fine sync is allowed to touch.
/// </param>
/// <param name="Resynced">
/// Candidates actually re-synced and re-submitted. <b>One extra submission each and never more.</b>
/// Below <paramref name="Offered"/> exactly when the slot had no samples behind it - see
/// <paramref name="RefusedForWantOfSamples"/>.
/// </param>
/// <param name="Accepted">
/// Re-synced codewords <b>the port then accepted</b> through its own parity gate and CRC-14 gate.
/// Nothing in this library decides a message is real.
/// </param>
/// <param name="OnTimeEdge">
/// How many winners sat on the time boundary of the search grid. <b>A high share means the extent is
/// too small and the search is reporting its edge rather than a peak.</b>
/// </param>
/// <param name="OnFrequencyEdge">The same on the frequency axis.</param>
/// <param name="RefusedForWantOfSamples">
/// Candidates that could not be re-synced because the slot was decoded from a waterfall and a
/// waterfall carries no samples. <b>The waterfall-only entry point does not throw and does not
/// pretend to have re-synced; it says so here.</b>
/// </param>
/// <param name="TotalTimeShiftSeconds">
/// The sum of the absolute time shifts applied, so a caller can take the mean without this record
/// having to hold one.
/// </param>
/// <param name="TotalFrequencyShiftHz">The same on the frequency axis.</param>
/// <param name="WorstTimeShiftSeconds">The largest absolute time shift applied.</param>
/// <param name="WorstFrequencyShiftHz">The largest absolute frequency shift applied.</param>
public readonly record struct Ft8DeepFineSyncCounts(
    int Offered,
    int Resynced,
    int Accepted,
    int OnTimeEdge,
    int OnFrequencyEdge,
    int RefusedForWantOfSamples,
    double TotalTimeShiftSeconds,
    double TotalFrequencyShiftHz,
    double WorstTimeShiftSeconds,
    double WorstFrequencyShiftHz)
{
    /// <summary>The mean absolute time shift applied, or zero when nothing was re-synced.</summary>
    public double MeanTimeShiftSeconds =>
        Resynced == 0 ? 0.0 : TotalTimeShiftSeconds / Resynced;

    /// <summary>The mean absolute frequency shift applied, or zero when nothing was re-synced.</summary>
    public double MeanFrequencyShiftHz =>
        Resynced == 0 ? 0.0 : TotalFrequencyShiftHz / Resynced;

    /// <summary>
    /// <b>How many codewords this stage put to the port's CRC-14 in one slot</b>, which is exactly
    /// <see cref="Resynced"/> - one per candidate re-synced, never more.
    /// </summary>
    /// <remarks>
    /// Every one of them is an independent chance of a false accept at about one in 16 384;
    /// <c>Ft8DeepCombineSettings.ExpectedFalseAccepts</c> is that arithmetic already written down and
    /// takes this number.
    /// </remarks>
    public int Submissions => Resynced;
}
