using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How a slot is mixed down to complex baseband, filtered and decimated before a candidate is
/// read at a position the waterfall's grid cannot name.</b> Every number here has arithmetic behind
/// it rather than taste, and the arithmetic is written out beside it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why baseband at all.</b> <c>Ft8Waterfall</c> holds magnitudes only, quantised to bytes at half
/// a decibel, on a grid of 960-sample sub-blocks and 3.125 Hz transform bins. It has no phase in it
/// and no samples behind it, so nothing that starts from a waterfall can ask what the signal looked
/// like at a time or a frequency the grid does not name. The samples can.
/// </para>
/// <para>
/// <b>Textbook DSP, cited as such.</b> Mixing a real passband signal to complex baseband by
/// multiplication with a complex exponential, low-pass filtering with a windowed-sinc FIR and
/// decimating is standard digital signal processing - see any of Oppenheim and Schafer, Lyons, or
/// Harris on multirate filtering. <b>Nothing here is taken from WSJT-X or from any other decoder's
/// source.</b> The FT8 frame it is applied to - 79 symbols of 0.160 s, eight tones spaced 6.25 Hz -
/// is from the published protocol description: Franke K9AN, Somerville G4WJS and Taylor K1JT, <i>The
/// FT4 and FT8 Communication Protocols</i>, QEX, July/August 2020.
/// </para>
/// <para>
/// <b>The arithmetic behind the three numbers, at the 12 kHz rate every figure in this phase was
/// taken at.</b>
/// </para>
/// <list type="number">
/// <item>
/// <description>
/// <b>What must be passed.</b> The eight tones span <c>8 x 6.25 = 50 Hz</c>. Mixed at the centre of
/// that span they sit at <c>+/- 21.875 Hz</c>. The fine search moves the mixing frequency by up to
/// <c>+/- 1.5625 Hz</c> - a quarter of a tone, which is the whole of what the coarse grid leaves
/// undetermined - and the GFSK smoothing puts skirts on each tone. <b>The passband must therefore be
/// flat to at least +/- 25 Hz</b>, and flat rather than merely present: a response that sloped across
/// the span would give the outer tones less gain than the inner ones, and
/// <c>Ft8SoftSymbols.ExtractSymbol</c> compares tone magnitudes against one another.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>What must be rejected.</b> After decimation by <see cref="Decimation"/> to
/// <see cref="DecimatedRateHz"/>, energy at any frequency <c>f</c> folds into the kept band wherever
/// <c>|f - n x DecimatedRateHz| &lt; 25</c> for some <c>n &gt;= 1</c>. At the settings below the
/// nearest such band begins at <b>475 Hz</b>, so the filter must be in its stopband by then.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>What that costs in taps.</b> A Blackman-windowed sinc has a transition width of about
/// <c>5.5 x fs / N</c> and a stopband about 58 dB down. At <c>fs = 12000</c> and
/// <c>N = <see cref="FilterLength"/> = 401</c> the transition is about <b>165 Hz</b>, so a cutoff of
/// <see cref="CutoffHz"/> = 150 Hz is flat to about <b>68 Hz</b> and in its stopband by about
/// <b>232 Hz</b> - inside the 475 Hz the decimation requires, with the whole of the eight-tone span
/// well inside the flat part. <b>A shorter filter cannot do both</b>: at 121 taps the transition is
/// 545 Hz and there is no cutoff that is both above 25 Hz and below 475.
/// </description>
/// </item>
/// </list>
/// <para>
/// <b>Why 24 and not more or less.</b> The decimated rate must divide the sample rate exactly, must
/// give a whole number of samples in a symbol so that a symbol window is a window and not a
/// resampling, and must be fine enough that the fine search's time step is not set by it.
/// <c>12000 / 24 = 500 Hz</c> gives <b>80 samples in a symbol</b>, so the finest time the extractor
/// can be commanded to is 2 ms - one fortieth of a symbol, and one twentieth of the 40 ms the coarse
/// grid leaves undetermined. Decimating by 48 would halve the filtering cost and leave 40 samples a
/// symbol, which is 4 ms, and that is the same order as the step the fine search wants to take.
/// </para>
/// </remarks>
public sealed class Ft8DeepBasebandSettings
{
    /// <summary>The longest filter this library will build. A bound, not a tuning.</summary>
    public const int MaximumFilterLength = 4001;

    /// <summary>
    /// <b>The settings this library uses when nobody names any</b>, and the ones every figure
    /// unit 248 recorded was taken at.
    /// </summary>
    public static Ft8DeepBasebandSettings Default { get; } = new(24, 401, 150.0);

    /// <summary>Builds settings, or refuses them.</summary>
    /// <param name="decimation">Input samples per baseband sample.</param>
    /// <param name="filterLength">Taps in the low-pass. Odd, so the group delay is a whole sample.</param>
    /// <param name="cutoffHz">The one-sided cutoff of the low-pass, in hertz at the input rate.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The decimation is below one, the filter length is not odd or is above
    /// <see cref="MaximumFilterLength"/>, or the cutoff is not positive.
    /// </exception>
    public Ft8DeepBasebandSettings(int decimation, int filterLength, double cutoffHz)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(decimation, 1);

        if (filterLength < 3 || filterLength % 2 == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filterLength),
                filterLength,
                "A linear-phase low-pass is built with an odd number of taps so that its group "
                + "delay is a whole number of samples and can be removed exactly. An even length "
                + "would leave half a sample of delay that no index can compensate, which is a "
                + "systematic time error in every position this library reports.");
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(filterLength, MaximumFilterLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cutoffHz);

        Decimation = decimation;
        FilterLength = filterLength;
        CutoffHz = cutoffHz;
    }

    /// <summary>Input samples per baseband sample.</summary>
    public int Decimation { get; }

    /// <summary>Taps in the low-pass.</summary>
    public int FilterLength { get; }

    /// <summary>The one-sided cutoff of the low-pass, in hertz at the input rate.</summary>
    public double CutoffHz { get; }

    /// <summary>The rate the baseband is held at, given an input rate.</summary>
    public double DecimatedRateHz(int sampleRate) => (double)sampleRate / Decimation;

    /// <summary>
    /// Baseband samples in one FT8 symbol at an input rate, or zero when the rate does not divide
    /// into a whole number of them.
    /// </summary>
    /// <remarks>
    /// <b>A whole number is required and is not rounded to.</b> A symbol window of 79.6 samples is a
    /// resampling problem wearing a window's clothes, and the eight tone correlations below stop
    /// being orthogonal the moment the window is not exactly one symbol long.
    /// </remarks>
    public int SamplesPerSymbol(int sampleRate)
    {
        var exact = DecimatedRateHz(sampleRate) * Ft8WaterfallGeometry.SymbolPeriodSeconds;
        var whole = (int)Math.Round(exact);
        // The tolerance is loose because Ft8WaterfallGeometry.SymbolPeriodSeconds is a float and
        // 500 x 0.160f is 80.0000012, not 80. The question being asked is whether the rate divides
        // the symbol, not whether a float round-trips.
        return Math.Abs(exact - whole) < 1e-4 && whole > 0 ? whole : 0;
    }

    /// <summary>
    /// The span of the eight tones, in hertz. <c>ToneCount x ToneSpacing</c>, and the number the
    /// passband requirement above is read off.
    /// </summary>
    public static double ToneSpanHz =>
        Ft8SymbolEncoder.ToneCount / (double)Ft8WaterfallGeometry.SymbolPeriodSeconds;
}
