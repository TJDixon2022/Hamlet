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
/// <para>**ONE METHOD**, on the marks a pattern gate already holds, so the grid's own sequences can be judged by it later without a
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

    /// <summary>
    /// Whether the sequence is a shadow of <paramref name="other"/>: the other is more than <see cref="CwEnvelopeDetector.HandoverHz"/>
    /// away, stronger by its own contrast, and <paramref name="share"/> or more of the sequence's marks start within
    /// <paramref name="startMs"/> of its marks. A sequence is never a shadow of a weaker station, nor of one at its own pitch.
    /// </summary>
    /// <param name="marks">The sequence judged.</param>
    /// <param name="pitchHz">Its pitch.</param>
    /// <param name="other">The other station's marks.</param>
    /// <param name="otherPitchHz">The other station's pitch.</param>
    /// <param name="startMs">The start tolerance, in milliseconds.</param>
    /// <param name="share">The share that makes a shadow.</param>
    /// <returns>True where it is a shadow.</returns>
    public static bool IsShadowOf(IReadOnlyList<CwMark> marks, double pitchHz, IReadOnlyList<CwMark> other, double otherPitchHz, double startMs = ShadowStartMs, double share = ShadowShare)
        => Math.Abs(pitchHz - otherPitchHz) > CwEnvelopeDetector.HandoverHz
            && ContrastOf(other) > ContrastOf(marks)
            && ShareOf(marks, other, startMs) >= share;
}
