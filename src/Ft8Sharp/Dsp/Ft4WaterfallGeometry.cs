namespace Ft8Sharp.Dsp;

/// <summary>
/// Every extent of an <b>FT4</b> waterfall, derived at FT4's own symbol period — by upstream's own
/// arithmetic, in upstream's own precision.
/// </summary>
/// <remarks>
/// <para>
/// <b>Re-derived at 0.048f rather than inherited from 0.160f.</b>
/// <see cref="Ft8WaterfallGeometry"/>'s remarks record a float-against-double truncation match
/// derived at FT8's period specifically, and nothing about that derivation carries to another
/// number. Unit 289 re-took it here, and the answer is different in kind:
/// </para>
/// <code>
///                     in float (upstream, and this)    the same constant in double
///   block size        12000 * 0.048f -> 576.0f -> 576   576.000005007  -> 576
///   first kept bin      200 * 0.048f ->   9.6f ->   9     9.60000008   ->   9
///   last kept bin      3000 * 0.048f -> 144.0f -> 145   144.00000125   -> 145
///   blocks in a slot  7.5f / 0.048f  -> 156.25f-> 156   156.24999864   -> 156
/// </code>
/// <para>
/// <b>At FT4's period the two columns agree everywhere, and at FT8's they do not.</b> That is the
/// finding rather than a formality: FT8's geometry turns on keeping the arithmetic single precision —
/// a double-precision block size comes out 1919 rather than 1920 and a double-precision first bin 31
/// rather than 32, which would shift every reported frequency by a whole tone — and FT4's does not
/// turn on it at all. The precision is kept single anyway, because it is what upstream does and
/// because a port that is right for a reason that stopped applying is a port waiting to be wrong.
/// <c>Ft4WaterfallGeometryTests</c> computes both columns and prints them rather than leaving this
/// paragraph to be believed.
/// </para>
/// <para>
/// <b>The passband is upstream's own and is FT8's.</b> <c>decode_ft8.c</c> hands 200 Hz and 3000 Hz
/// to <c>monitor_init</c> for both protocols; only the symbol period changes. So FT4 keeps 136 bins
/// where FT8 keeps 449 — the bins are wider, because a bin is one tone spacing and FT4's tones are
/// 20.833 Hz apart rather than 6.25.
/// </para>
/// <para>
/// <b>Every timing figure comes from <see cref="Ft4Timing"/>.</b> There is no <c>0.048f</c> and no
/// <c>7.5f</c> in this file.
/// </para>
/// </remarks>
public sealed class Ft4WaterfallGeometry : Ft8WaterfallGeometry
{
    /// <summary>
    /// Analyses per symbol. <b>4, where <see cref="Ft8WaterfallGeometry.DefaultTimeOversampling"/>
    /// and the demo application both use 2, and it is the second number in the FT4 path that is not
    /// upstream's.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why it moved, and it is a measurement rather than a preference.</b> Unit 296 measured
    /// FT4's decode rate on a lattice of twenty-five placements spanning one whole tone spacing and
    /// one whole symbol period — a lattice defined against the protocol so that no analysis grid can
    /// flatter it, five divisions a side because no power-of-two oversampling divides five. At
    /// -13 dB, where the on-grid rate is exactly one, <b>2 kept 808 of 900 placement-averaged and
    /// 19 of 36 at its worst cell; 4 keeps 894 of 900 and 34 of 36</b>, and the on-grid cell does
    /// not move. Interpolated on an on-grid ladder taken a decibel at a time, the worst placement
    /// cost <b>2.4 dB at 2 and under half a decibel at 4</b>. The whole table is in
    /// <c>docs/unit296-runs/grid-sweep.txt</c>.
    /// </para>
    /// <para>
    /// <b>Why the loss was on this axis and not the frequency one, which the same sweep settled.</b>
    /// <see cref="Ft8Monitor.ProcessBlock"/> analyses a sliding frame of
    /// <see cref="Ft8WaterfallGeometry.TransformLength"/> = <c>BlockSize × FrequencyOversampling</c>
    /// samples, so <b>frequency oversampling lengthens the analysis window</b> — at 2 the transform
    /// spans two whole FT4 symbol periods and at 4 it spans four. FT4 is 4-FSK and its tone changes
    /// every symbol, so a window spanning four symbols sees four different tones and the Costas
    /// correlation is smeared away: 2x4 and 4x4 both fall from 36 of 36 to <b>0 of 36 on grid</b> at
    /// -13 dB, and losing the on-grid decodes first is not what an off-grid problem looks like.
    /// <b>Time oversampling costs nothing of the kind</b> — it changes only how often the
    /// same-length frame is sampled, so it buys sub-symbol alignment resolution with no smearing,
    /// and sub-symbol alignment is the axis a real station's placement actually varies on.
    /// <see cref="DefaultFrequencyOversampling"/> therefore stays at upstream's 2.
    /// </para>
    /// <para>
    /// <b>What it costs.</b> One FT4 slot decodes in <b>151 ms against a 7.5 s slot</b> — 2.0% of
    /// it, in a Debug build — where 2 spends 0.9%. <b>Zero wrong decodes</b> in 2700 slots at this
    /// grid and 10800 across the whole sweep, counted per cell and per rung: a denser grid makes
    /// more hypotheses and more hypotheses is how a decoder starts inventing messages, so it was
    /// looked for everywhere rather than summed at the end.
    /// </para>
    /// <para>
    /// <b>Why this is not a divergence on a constant, in the sense the standing ruling forbids.</b>
    /// Exactly the licence unit 289 recorded for <see cref="Ft4SyncSearch.DefaultLastBlockOffset"/>,
    /// and it is worth stating in the same terms. <c>freq_osr</c> and <c>time_osr</c> are
    /// <em>parameters</em> of <c>monitor_init</c>; 2 and 2 are <c>demo/decode_ft8.c</c>'s file-scope
    /// judgement about how much work to do, the same class of number as <c>kMin_score</c> and
    /// <c>kMax_candidates</c>. <b>Nothing about the modulation, the tone spacing, the symbol period,
    /// the symbol count, the sync patterns, the Costas tables, the codeword, the CRC or the waveform
    /// changes</b>, and none of them is reachable from here. <b>A caller who asks for upstream's own
    /// 2 and 2 still gets upstream's own result</b>, and
    /// <c>Ft4Unit296TheGridMovedAndFt8sDidNotTests</c> asserts that they can.
    /// </para>
    /// <para>
    /// <b>And FT8's grid does not move by any route.</b>
    /// <see cref="Ft8WaterfallGeometry.DefaultTimeOversampling"/> and
    /// <see cref="Ft8WaterfallGeometry.DefaultFrequencyOversampling"/> still read 2 and 2; these
    /// constants shadow them for FT4 alone. The port's byte-fidelity to upstream on FT8 is the
    /// instrument every sensitivity figure in this project leans on.
    /// </para>
    /// </remarks>
    public new const int DefaultTimeOversampling = 4;

    /// <summary>
    /// Bins per tone spacing. <b>2, which is upstream's, and the sweep that moved
    /// <see cref="DefaultTimeOversampling"/> is the reason this one did not.</b>
    /// </summary>
    /// <remarks>
    /// It is written out here rather than inherited so that the two FT4 numbers sit together and
    /// the one that stayed is as visible as the one that moved. 2 is the largest value that does
    /// not make the analysis frame span more than one FT4 symbol change — see
    /// <see cref="DefaultTimeOversampling"/>'s remarks, where the measurement is.
    /// </remarks>
    public new const int DefaultFrequencyOversampling =
        Ft8WaterfallGeometry.DefaultFrequencyOversampling;

    /// <summary>Builds an FT4 geometry, or refuses it.</summary>
    /// <param name="sampleRate">Samples per second of the audio to be analysed.</param>
    /// <param name="minFrequencyHz">The low end of the passband kept in the waterfall.</param>
    /// <param name="maxFrequencyHz">The high end of the passband kept in the waterfall.</param>
    /// <param name="timeOversampling">Analyses per symbol.</param>
    /// <param name="frequencyOversampling">Bins per tone spacing.</param>
    public Ft4WaterfallGeometry(
        int sampleRate = DefaultSampleRate,
        float minFrequencyHz = DefaultMinFrequencyHz,
        float maxFrequencyHz = DefaultMaxFrequencyHz,
        int timeOversampling = DefaultTimeOversampling,
        int frequencyOversampling = DefaultFrequencyOversampling)
        : base(
            Ft4Timing.SymbolPeriodSeconds,
            Ft4Timing.SlotSeconds,
            sampleRate,
            minFrequencyHz,
            maxFrequencyHz,
            timeOversampling,
            frequencyOversampling)
    {
    }
}
