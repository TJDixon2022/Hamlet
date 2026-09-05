using System;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>Where a candidate actually is, found rather than assumed.</b> The result of one fine search:
/// the position, what it moved by, the score it found there, and whether it stopped at the edge of
/// its own grid.
/// </summary>
/// <param name="StartSeconds">
/// When the frame's first symbol begins, in seconds into the slot. Continuous.
/// </param>
/// <param name="FrequencyOffsetHz">
/// How far the tones sit from where the baseband was mixed, in hertz. Continuous.
/// </param>
/// <param name="Score">The Costas correlation there, in decibels. See <c>Ft8DeepBaseband.SyncScore</c>.</param>
/// <param name="TimeShiftSeconds">How far the search moved the candidate in time.</param>
/// <param name="FrequencyShiftHz">How far it moved it in frequency.</param>
/// <param name="OnTimeEdge">
/// <b>Whether the winner sat on the time boundary of the search grid.</b> A high rate of these means
/// the extent is too small and the search is reporting its edge rather than a peak.
/// </param>
/// <param name="OnFrequencyEdge">The same on the frequency axis.</param>
public readonly record struct Ft8DeepFineSyncResult(
    double StartSeconds,
    double FrequencyOffsetHz,
    double Score,
    double TimeShiftSeconds,
    double FrequencyShiftHz,
    bool OnTimeEdge,
    bool OnFrequencyEdge);

/// <summary>
/// <b>The fine search: the position a coarse candidate is actually at, in sub-symbol time and
/// sub-hertz frequency, by Costas correlation against the samples.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing is told to it.</b> It is handed a baseband and the candidate's own nominal start, and
/// it returns the position in its grid that maximises the correlation of the 21 known sync symbols.
/// No truth, no message, no expected symbols. <c>SearchFixture.Truth</c> appears in unit 248's
/// oracle measurement and in no scored column.
/// </para>
/// <para>
/// <b>Deterministic by construction.</b> There is no random start, no early exit and no ordering
/// that depends on anything but the settings: the whole grid is visited in one fixed order, the
/// candidate's own position is scored first, and a later position replaces it only on a strictly
/// greater score - so a tie leaves the candidate where the coarse search put it rather than moving
/// it for nothing.
/// </para>
/// <para>
/// <b>Textbook, and cited as such.</b> Correlating a known sequence against a received one to find
/// its position is standard detection theory. The sequence being correlated - three seven-symbol
/// Costas arrays at symbols 0, 36 and 72 of a 79-symbol frame - is from the published protocol
/// description, Franke K9AN, Somerville G4WJS and Taylor K1JT, <i>The FT4 and FT8 Communication
/// Protocols</i>, QEX, July/August 2020, and the arrays themselves are the port's
/// <c>Ft8Tables.Ft8CostasPattern</c> reached through the port's <c>Ft8SymbolEncoder</c> layout.
/// <b>No decoder's source was read for any of it.</b>
/// </para>
/// </remarks>
public sealed class Ft8DeepFineSync
{
    /// <summary>Builds a search.</summary>
    /// <param name="settings">The extent and the step, or null for <c>Default</c>.</param>
    public Ft8DeepFineSync(Ft8DeepFineSyncSettings? settings = null) =>
        Settings = settings ?? Ft8DeepFineSyncSettings.Default;

    /// <summary>The extent and the step this search uses.</summary>
    public Ft8DeepFineSyncSettings Settings { get; }

    /// <summary>
    /// Finds the position around <paramref name="startSeconds"/> that maximises the Costas
    /// correlation.
    /// </summary>
    /// <param name="baseband">The slot, mixed about this candidate's eight tones.</param>
    /// <param name="startSeconds">
    /// The candidate's own nominal start, in seconds into the slot. <b>The centre of the search and
    /// nothing more.</b>
    /// </param>
    /// <exception cref="ArgumentNullException">The baseband is null.</exception>
    /// <remarks>
    /// <b>A baseband with nothing in it returns the position it was given, unmoved, with a score of
    /// negative infinity</b> - which is an ordinary answer for a slot that held no samples there and
    /// not an error. Nothing downstream treats a moved candidate differently from an unmoved one
    /// except the counts.
    /// </remarks>
    public Ft8DeepFineSyncResult Search(Ft8DeepBaseband baseband, double startSeconds)
    {
        ArgumentNullException.ThrowIfNull(baseband);

        // THE CANDIDATE'S OWN POSITION IS THE INCUMBENT. A tie anywhere else does not displace it,
        // which is what makes "the search moved this candidate" mean something.
        var bestScore = baseband.SyncScore(startSeconds, 0.0);
        var bestTime = 0;
        var bestFrequency = 0;

        for (var t = -Settings.TimeStepCount; t <= Settings.TimeStepCount; t++)
        {
            var seconds = startSeconds + (t * Settings.TimeStepSeconds);

            for (var f = -Settings.FrequencyStepCount; f <= Settings.FrequencyStepCount; f++)
            {
                if (t == 0 && f == 0)
                {
                    continue;
                }

                var hertz = f * Settings.FrequencyStepHz;
                var score = baseband.SyncScore(seconds, hertz);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTime = t;
                    bestFrequency = f;
                }
            }
        }

        var timeShift = bestTime * Settings.TimeStepSeconds;
        var frequencyShift = bestFrequency * Settings.FrequencyStepHz;

        return new Ft8DeepFineSyncResult(
            startSeconds + timeShift,
            frequencyShift,
            bestScore,
            timeShift,
            frequencyShift,
            Math.Abs(bestTime) == Settings.TimeStepCount,
            Math.Abs(bestFrequency) == Settings.FrequencyStepCount);
    }
}
