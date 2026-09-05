namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How much each hearing of a repeated transmission counts for when they are added together.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Equal weight is optimal when the hearings carry the same signal-to-noise ratio, and that is
/// exactly what the ladder delivers</b> — the same clean audio mixed with two draws from the same
/// distribution, delivered within a few thousandths of a decibel of each other. On real air it is not:
/// a station that faded between slots is heard once well and once badly, and counting the bad hearing
/// as hard as the good one drags the sum toward the noise.
/// </para>
/// <para>
/// <b>So both exist, and unit 247 task 1 measured which one to make the default</b> rather than
/// arguing it. See <see cref="Ft8DeepSoftCombiner"/>'s remarks for the numbers and the choice.
/// </para>
/// </remarks>
public enum Ft8DeepCombineWeighting
{
    /// <summary>
    /// Every hearing counts the same. Optimal when they carry the same ratio, and the default.
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Each hearing counts in proportion to the variance its ratios carried <em>before</em>
    /// <c>Ft8SoftSymbols.Normalise</c> rescaled them, which is the only measure of that hearing's
    /// strength this library has without re-measuring the audio.
    /// </summary>
    /// <remarks>
    /// <b>It is a proxy and it is labelled as one.</b> The pre-normalisation variance of a candidate's
    /// 174 ratios is large when the eight tone magnitudes are far apart and small when they are not,
    /// so it rises with signal strength — but it also rises with a strong interferer sitting on the
    /// same bins, and it is measured on quantised half-decibel magnitudes. Step 5 of this phase is
    /// where a real per-message ratio would come from.
    /// </remarks>
    ByPreNormalisationVariance = 1,
}
