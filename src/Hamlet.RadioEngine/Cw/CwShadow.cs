namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **A SHADOW IS TOLD BY ITS TIMING** (work instruction 571, HM-DEC-275). The owner's ruling, 2026-10-13, option A: a sequence
/// whose marks start, most of them, within <see cref="ShadowStartMs"/> of a stronger station's own marks, more than the same-station
/// distance away, is that station's shadow and not a station. It is judged over the sequence, the way the pattern gate judges
/// everything else, so one coincidence never holds a real mark back.
/// </summary>
/// <remarks>
/// <para>**THE TRACE THAT ORDERED IT** (work instruction 570): the carrier at seed 5209 printed through a lane 75 Hz from a station
/// the grid already held, every one of its marks starting 10 to 30 ms after one of that station's own. A per-mark rule silenced it
/// and cost the board six points and a 5 dB CQ.</para>
/// <para>**ONE METHOD**, on the marks a pattern gate already holds, so the grid's own sequences are judged by it (work instruction 573) without a
/// second copy.</para>
/// </remarks>
internal static class CwShadow
{
    /// <summary>How near a mark's start must be to one of the stronger station's starts to count: 30 ms, the carrier trace's figure.</summary>
    public const double ShadowStartMs = 30;

    /// <summary>The share of the sequence's marks that must start so for it to be a shadow: two thirds.</summary>
    public const double ShadowShare = 2.0 / 3;

    /// <summary>How many of the sequence's last marks are judged: ten, the pattern gate's own reach for a hand's two kinds.</summary>
    public const int JudgedMarks = 10;

    /// <summary>
    /// **HOW FAR THE RULE REACHES** (work instruction 573, HM-DEC-277): where the grid is judged (<see cref="JudgesTheGrid"/>), a
    /// sequence is judged against every stronger station more
    /// than 25 Hz away, one lane's spacing and the smallest distance at which the chain tells two pitches apart. It reached only
    /// past the handover's same-station distance, <see cref="CwEnvelopeDetector.HandoverHz"/>, 50 Hz, and the shadow live beside
    /// W1AW sat exactly 50 Hz from it. The handover's own distance is not changed: a station 25 to 50 Hz from a stronger one is
    /// judged for shadow here, and is still the same station when the handover asks whether the grid holds it.
    /// </summary>
    public const double ShadowReachHz = CwEnvelopeDetector.BinSpacingHz;

    /// <summary>
    /// **THE RULE JUDGES THE GRID** (work instruction 573, HM-DEC-277): before the sender gate is handed a grid sequence's marks,
    /// the sequence is judged by <see cref="IsShadowOf"/> against every stronger station Hamlet holds, grid and bank; a shadow's
    /// marks are not handed on, so it is never picked, and a held one is let go by the gate's own release. **Off**, and the reach
    /// with it, so off reads exactly as before: measured on, W1AW read 2079 against 2111, 36 wrong against 20, most of it in the
    /// 13 and 10 WPM stretch of piece 1; and beside a lockstep shadow the synthetic station lost its opening on and off alike.
    /// </summary>
    public const bool JudgesTheGrid = false;

    /// <summary>How far the rule reaches on a detector: <see cref="ShadowReachHz"/> where it judges the grid, and the handover's 50 Hz, as before work instruction 573, where it does not.</summary>
    /// <param name="judgesTheGrid">Whether the grid is judged.</param>
    /// <returns>The reach in hertz.</returns>
    public static double ReachHz(bool judgesTheGrid) => judgesTheGrid ? ShadowReachHz : CwEnvelopeDetector.HandoverHz;

    /// <summary>
    /// The share of a sequence's last <see cref="JudgedMarks"/> marks that start within <paramref name="startMs"/> of a mark of
    /// <paramref name="other"/>. Nought where either holds no mark.
    /// </summary>
    /// <param name="marks">The sequence judged, in the order keyed.</param>
    /// <param name="other">The other station's marks.</param>
    /// <param name="startMs">The start tolerance, in milliseconds.</param>
    /// <returns>Nought to one.</returns>
    public static double ShareOf(IReadOnlyList<CwMark> marks, IReadOnlyList<CwMark> other, double startMs = ShadowStartMs)
    {
        var judged = marks.TakeLast(JudgedMarks).ToList();

        if (judged.Count == 0 || other.Count == 0)
        {
            return 0;
        }

        var within = startMs / 1000;
        return judged.Count(m => other.Any(o => Math.Abs(o.FromSeconds - m.FromSeconds) <= within)) / (double)judged.Count;
    }

    /// <summary>A sequence's contrast, the mean of its last marks' own: what "stronger" is judged by.</summary>
    /// <param name="marks">The sequence's marks.</param>
    /// <returns>Its contrast in dB; NaN where it holds none.</returns>
    public static double ContrastOf(IReadOnlyList<CwMark> marks)
        => marks.TakeLast(JudgedMarks).Select(m => m.ContrastDb).DefaultIfEmpty(double.NaN).Average();

    /// <summary>A sequence's level, the mean of its last marks' own (work instruction 573).</summary>
    /// <param name="marks">The sequence's marks.</param>
    /// <returns>Its level in dB; NaN where it holds none.</returns>
    public static double LevelOf(IReadOnlyList<CwMark> marks)
        => marks.TakeLast(JudgedMarks).Select(m => m.LevelDb).DefaultIfEmpty(double.NaN).Average();

    /// <summary>
    /// Whether the sequence is a shadow of <paramref name="other"/>: the other is more than <see cref="ShadowReachHz"/>
    /// away, stronger by its own contrast, and, within the handover's same-station distance, louder by its level too, where a
    /// station's own skirt shows as much contrast as the station (work instruction 573); and <paramref name="share"/> or more of
    /// the sequence's marks start within <paramref name="startMs"/> of its marks. A sequence is never a shadow of a weaker
    /// station, nor of one at its own pitch.
    /// </summary>
    /// <param name="marks">The sequence judged.</param>
    /// <param name="pitchHz">Its pitch.</param>
    /// <param name="other">The other station's marks.</param>
    /// <param name="otherPitchHz">The other station's pitch.</param>
    /// <param name="startMs">The start tolerance, in milliseconds.</param>
    /// <param name="share">The share that makes a shadow.</param>
    /// <param name="reachHz">How far away the stronger station must be: <see cref="ShadowReachHz"/>, or the handover's 50 Hz where the grid is not judged.</param>
    /// <returns>True where it is a shadow.</returns>
    public static bool IsShadowOf(IReadOnlyList<CwMark> marks, double pitchHz, IReadOnlyList<CwMark> other, double otherPitchHz, double startMs = ShadowStartMs, double share = ShadowShare, double reachHz = ShadowReachHz)
        => Math.Abs(pitchHz - otherPitchHz) > reachHz
            && ContrastOf(other) > ContrastOf(marks)
            && (Math.Abs(pitchHz - otherPitchHz) > CwEnvelopeDetector.HandoverHz || LevelOf(other) > LevelOf(marks))
            && ShareOf(marks, other, startMs) >= share;
}
