namespace Ft8Sharp;

/// <summary>
/// <b>The one place FT4's timing lives.</b> Every FT4 number in this library that depends on how
/// long a symbol is, or how long a slot is, is computed from here and from nowhere else.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it is a type of its own rather than two constants on the synthesizer.</b> There is an open
/// question about FT4's timing that is with the owner and that no unit may settle:
/// <c>PHASE_PLAN.md</c> step 1 calls FT4's transmission <b>4.48 seconds</b>, while upstream's
/// <c>FT4_SYMBOL_PERIOD</c> is <c>0.048f</c> (<c>ft8/constants.h:14</c>), which puts 105 symbols at
/// <b>5.04 s</b> and the tone spacing at <b>20.833 Hz</b>. The string <c>4.48</c> appears nowhere in
/// the pinned clone. They cannot both be true, settling it needs the cited QEX paper or a real
/// off-air recording, and this tree has neither.
/// </para>
/// <para>
/// <b>So this library builds on upstream's figure and puts it where a ruling costs one edit.</b>
/// <see cref="SymbolPeriodSeconds"/> is <c>0.048f</c> because the standing ruling is that
/// <c>Ft8Sharp</c> is a faithful MIT port and a deliberate divergence from upstream is not a unit's
/// to make. If the owner rules the other way, this file is the whole of the change: the synthesizer,
/// the waterfall geometry, the tone spacing, the slot length and the block count all read from here.
/// </para>
/// <para>
/// <b>Single precision, and that is load-bearing.</b> Upstream holds the symbol period in a
/// <c>float</c> and computes the block size, the first kept bin and the last kept bin by multiplying
/// it and truncating, so which way each product falls when the fraction is discarded is part of what
/// is being ported. <c>0.048f</c> is not 0.048 — it is 0.0480000004172325134 — and the difference
/// decides the truncations. See <c>Ft4WaterfallGeometry</c>, which re-derives every one of them at
/// this period rather than inheriting FT8's.
/// </para>
/// <para>
/// <b>Nothing here is a slot schedule.</b> <see cref="SlotSeconds"/> is how long a buffer of one
/// transmission's worth of audio is, which is what the synthesizer lays out and what the analysis
/// sizes itself to. It says nothing about UTC, nothing about when to transmit, and nothing in this
/// library reads a clock.
/// </para>
/// </remarks>
public static class Ft4Timing
{
    /// <summary>
    /// How long one FT4 channel symbol lasts, in seconds. Upstream's <c>FT4_SYMBOL_PERIOD</c>,
    /// <c>ft8/constants.h:14</c>.
    /// </summary>
    public const float SymbolPeriodSeconds = 0.048f;

    /// <summary>
    /// How long an FT4 slot is, in seconds, signal and silence together. Upstream's
    /// <c>FT4_SLOT_TIME</c>, <c>ft8/constants.h:15</c>.
    /// </summary>
    public const float SlotSeconds = 7.5f;

    /// <summary>
    /// The spacing between adjacent FT4 tones, in hertz — the reciprocal of the symbol period, which
    /// is what upstream's <c>dphi_peak = 2 * M_PI * hmod / n_spsym</c> with <c>hmod = 1</c> comes to.
    /// </summary>
    public const float ToneSpacingHz = 1.0f / SymbolPeriodSeconds;

    /// <summary>
    /// How long the signal itself occupies, in seconds: the symbol count times the symbol period.
    /// <b>5.04 s on upstream's constant</b>, which is the figure the 4.48 disagreement is about.
    /// </summary>
    public const float OccupancySeconds = Encode.Ft4SymbolEncoder.SymbolCount * SymbolPeriodSeconds;
}
