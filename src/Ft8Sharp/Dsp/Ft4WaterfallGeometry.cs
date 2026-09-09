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
