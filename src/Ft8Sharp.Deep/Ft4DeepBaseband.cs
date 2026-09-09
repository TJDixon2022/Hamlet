using System;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>One FT4 slot's audio, mixed to complex baseband, filtered, decimated, and readable as the four
/// tone powers of every symbol of a frame at a time and a frequency no grid names.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SEAM, AND WHY IT IS THIS ONE.</b> Work instruction 294 task 3 names two honest
/// alternatives: give <see cref="Ft8DeepBaseband"/> a geometry and let FT8 pass its own, or build
/// FT4's mixing and correlation beside it. <b>This is neither: it composes.</b> Mixing a real
/// passband signal to complex baseband, low-pass filtering it and decimating is <b>protocol-free</b>
/// — nothing in it knows how many tones there are, how long a symbol is or how many symbols a frame
/// holds — so this type asks <see cref="Ft8DeepBaseband.Build"/> for exactly that and does FT4's own
/// tone correlation over the samples it exposes. <b>Not one line of
/// <see cref="Ft8DeepBaseband"/> changed</b>, which matters because fine sync, message subtraction
/// and the slot decoder all build one and the 1.2 dB is a closed phase's number; and <b>the 401-tap
/// filter and the phase-block mixer are not copied</b>, which is what the port's whole argument is
/// against.
/// </para>
/// <para>
/// <b>THE ONE PRICE OF THE REUSE, STATED RATHER THAN HIDDEN.</b>
/// <see cref="Ft8DeepBaseband.Build"/> refuses a rate at which <em>FT8's</em> symbol is not a whole
/// number of baseband samples, so an FT4 baseband inherits that precondition even though FT4 does not
/// need it. At the 12 kHz grid every slot in this project is put on, decimating by 24 gives 500 Hz,
/// which is 80 samples in FT8's 0.160 s symbol and <b>24 in FT4's 0.048 s symbol</b>, so both hold
/// and nothing is refused. A rate where they disagreed would be refused with FT8's message, which
/// would be the wrong message; it is named here rather than papered over.
/// </para>
/// <para>
/// <b>WHERE THE TONES ARE IS MEASURED AND NOT ASSUMED.</b> <see cref="Ft8DeepBaseband"/> mixes about
/// the centre of <em>its</em> eight tones, which is three and a half of <em>its</em> tone spacings
/// above the base frequency it was given. That convention is FT8's and this type does not repeat it,
/// invert it or correct for it: it reads <see cref="Ft8DeepBaseband.CentreFrequencyHz"/> back and
/// places FT4's four tones relative to whatever it says. <b>So there is no FT8 constant anywhere in
/// FT4's arithmetic</b>, and if that convention ever moved this would follow it rather than break.
/// The offset is small either way — FT4's four tones sit inside ±41 Hz of the mix and the low-pass is
/// flat to about 68 Hz, so every tone gets the same gain, which is the property
/// <c>Ft8DeepBasebandSettings</c>' first arithmetic paragraph exists to guarantee.
/// </para>
/// <para>
/// <b>THE WINDOW IS RECTANGULAR AND EXACTLY ONE FT4 SYMBOL LONG.</b> FT4's tone spacing is the
/// reciprocal of its symbol period — <c>1 / 0.048 = 20.833 Hz</c>, from
/// <see cref="Ft8Sharp.Ft4Timing.ToneSpacingHz"/> and typed nowhere here — so over exactly one symbol
/// the four tone exponentials are orthogonal and the correlation below is the matched filter for the
/// alphabet, exactly as it is for FT8's eight over 0.160 s. Any taper widens each tone's response
/// until it overlaps its neighbours.
/// </para>
/// <para>
/// <b>PURE AND REPORT-ONLY.</b> Nothing in this file is called from a decode path. It changes no
/// ratio, no gate, no count and no decision. It takes samples and a place and returns powers.
/// </para>
/// <para>
/// <b>THE PUBLISHED DESCRIPTION.</b> FT4's frame — 105 symbols of 0.048 s, four tones spaced
/// 20.833 Hz, four four-symbol Costas groups and two ramps — is from Franke K9AN, Somerville G4WJS
/// and Taylor K1JT, <i>The FT4 and FT8 Communication Protocols</i>, QEX, July/August 2020, the same
/// paper the port already cites. The multirate DSP it is applied to is textbook — Oppenheim and
/// Schafer, Lyons, or Harris on multirate filtering. <b>No route to this arithmetic goes through
/// WSJT-X source or <c>ft4_ft8_public/</c>.</b>
/// </para>
/// </remarks>
public sealed class Ft4DeepBaseband
{
    private readonly Ft8DeepBaseband _mixed;

    /// <summary>
    /// The four tone exponentials over one FT4 symbol window, laid out <c>[tone * L + n]</c>, built
    /// once because they depend on neither where in the slot the window sits nor on the fine
    /// frequency offset. See <see cref="TonePowerGrid"/>.
    /// </summary>
    private readonly double[] _toneCos;
    private readonly double[] _toneSin;

    private Ft4DeepBaseband(Ft8DeepBaseband mixed, double baseFrequencyHz, int samplesPerSymbol)
    {
        _mixed = mixed;
        BaseFrequencyHz = baseFrequencyHz;
        SamplesPerSymbol = samplesPerSymbol;

        var tones = Ft4SymbolEncoder.ToneCount;
        _toneCos = new double[tones * samplesPerSymbol];
        _toneSin = new double[tones * samplesPerSymbol];

        for (var tone = 0; tone < tones; tone++)
        {
            for (var n = 0; n < samplesPerSymbol; n++)
            {
                var (sin, cos) = Math.SinCos(-2.0 * Math.PI * tone * n / samplesPerSymbol);
                _toneCos[(tone * samplesPerSymbol) + n] = cos;
                _toneSin[(tone * samplesPerSymbol) + n] = sin;
            }
        }
    }

    /// <summary>The frequency of the lowest of the message's four tones, in hertz.</summary>
    public double BaseFrequencyHz { get; }

    /// <summary>Baseband samples in one FT4 symbol. Always whole — <see cref="Build"/> refuses otherwise.</summary>
    public int SamplesPerSymbol { get; }

    /// <summary>The frequency the audio was actually mixed down about.</summary>
    /// <remarks>
    /// <b>Read back rather than assumed.</b> It is <see cref="Ft8DeepBaseband"/>'s own choice and this
    /// type takes it as given; the tone placement in <see cref="TonePowerGrid"/> is relative to it.
    /// </remarks>
    public double CentreFrequencyHz => _mixed.CentreFrequencyHz;

    /// <summary>Baseband samples per second.</summary>
    public double RateHz => _mixed.RateHz;

    /// <summary>How many baseband samples there are.</summary>
    public int Length => _mixed.Length;

    /// <summary>How this was mixed, filtered and decimated.</summary>
    public Ft8DeepBasebandSettings Settings => _mixed.Settings;

    /// <summary>
    /// <b>The FT4 tone spacing, in hertz: 20.833.</b> The reciprocal of the symbol period, which is
    /// what makes the four tones orthogonal over one symbol.
    /// </summary>
    /// <remarks>
    /// From <see cref="Ft8Sharp.Ft4Timing.ToneSpacingHz"/>, which is itself
    /// <c>1 / SymbolPeriodSeconds</c>. There is no <c>0.048</c> and no <c>20.833</c> in this file.
    /// </remarks>
    public static double ToneSpacingHz => Ft8Sharp.Ft4Timing.ToneSpacingHz;

    /// <summary>
    /// Mixes, filters and decimates one slot about the four tones starting at
    /// <paramref name="baseFrequencyHz"/>.
    /// </summary>
    /// <param name="samples">The slot's audio. May be any length, including empty.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The frequency of the lowest of the four tones, in hertz.</param>
    /// <param name="settings">How to mix, filter and decimate, or null for the default.</param>
    /// <returns>The baseband.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sample rate is not positive, or the settings do not leave a whole number of baseband
    /// samples in a symbol — FT4's, or, inherited from <see cref="Ft8DeepBaseband.Build"/>, FT8's.
    /// </exception>
    /// <remarks>
    /// <b>An empty or very short slot is an ordinary answer and not an error.</b> The result has few
    /// or no baseband samples in it and every symbol window then falls outside, which
    /// <see cref="TonePowerGrid"/> reports as <see cref="double.NaN"/> — no measurement rather than a
    /// wrong one.
    /// </remarks>
    public static Ft4DeepBaseband Build(
        ReadOnlySpan<float> samples,
        int sampleRate,
        double baseFrequencyHz,
        Ft8DeepBasebandSettings? settings = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);

        var used = settings ?? Ft8DeepBasebandSettings.Default;
        var length = SamplesPerFt4Symbol(used, sampleRate);

        if (length == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                used.Decimation,
                $"A decimation of {used.Decimation} at {sampleRate} Hz does not leave a whole number "
                + $"of baseband samples in an FT4 symbol of "
                + $"{Ft8Sharp.Ft4Timing.SymbolPeriodSeconds:F3} s. A fractional symbol window is a "
                + "resampling problem and the four tone correlations stop being orthogonal, so it is "
                + "refused rather than rounded to.");
        }

        // THE MIXING, THE FILTERING AND THE DECIMATION, REUSED WHOLE AND UNCHANGED. None of the
        // three knows anything about a protocol. The base frequency handed over is FT4's own; where
        // that puts the mix centre is FT8's convention and is read back below rather than predicted.
        var mixed = Ft8DeepBaseband.Build(samples, sampleRate, baseFrequencyHz, used);

        return new Ft4DeepBaseband(mixed, baseFrequencyHz, length);
    }

    /// <summary>
    /// Baseband samples in one FT4 symbol at an input rate, or zero where the rate does not divide
    /// into a whole number of them.
    /// </summary>
    /// <param name="settings">The decimation.</param>
    /// <param name="sampleRate">Samples per second of the audio.</param>
    /// <returns>24 at the default settings and 12 kHz.</returns>
    /// <remarks>
    /// <b>The tolerance is loose for the reason <c>Ft8DeepBasebandSettings.SamplesPerSymbol</c>
    /// gives</b>: <see cref="Ft8Sharp.Ft4Timing.SymbolPeriodSeconds"/> is a float, so
    /// <c>500 * 0.048f</c> is 24.0000002 rather than 24. The question being asked is whether the rate
    /// divides the symbol, not whether a float round-trips.
    /// </remarks>
    public static int SamplesPerFt4Symbol(Ft8DeepBasebandSettings settings, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var exact = settings.DecimatedRateHz(sampleRate) * Ft8Sharp.Ft4Timing.SymbolPeriodSeconds;
        var whole = (int)Math.Round(exact);
        return Math.Abs(exact - whole) < 1e-4 && whole > 0 ? whole : 0;
    }

    /// <summary>
    /// <b>The four tone powers of every symbol of one FT4 frame, in decibels, read at a start time
    /// this baseband was told rather than at any grid position.</b>
    /// </summary>
    /// <param name="startSeconds">
    /// When the frame's first symbol begins, in seconds from the start of the slot. <b>Continuous</b>
    /// — it is not a block index and it is not rounded to one.
    /// </param>
    /// <param name="frequencyOffsetHz">
    /// How far the four tones sit from <see cref="BaseFrequencyHz"/>, in hertz. <b>Continuous</b>, and
    /// small: the fine search never moves a candidate by more than a quarter of a tone, which is far
    /// inside the low-pass, so no second mixing pass is needed.
    /// </param>
    /// <param name="decibels">
    /// <c>SymbolCount x ToneCount</c> magnitudes, laid out <c>[symbol * 4 + tone]</c>, <b>indexed by
    /// tone</b>. A symbol whose window falls outside the baseband is left as
    /// <see cref="double.NaN"/>.
    /// </param>
    /// <returns>How many symbols were filled.</returns>
    /// <exception cref="ArgumentException"><paramref name="decibels"/> is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>THE PRE-ROTATION, AND IT CARRIES NO PROTOCOL CONSTANT.</b> One rotation is folded in: the
    /// distance from where the audio was actually mixed to where FT4's tone 0 sits, including any
    /// fine frequency offset. After it, tone <c>k</c> is plain bin <c>k</c> of an <c>L</c>-point
    /// transform and the four exponentials are the fixed table built in the constructor. The step is
    /// <c>-2 pi (base + offset - centre) / rate</c> and nothing else; where FT8's equivalent carries
    /// a <c>+2 pi * 3.5 / L</c> term, that term is <em>this same expression</em> evaluated at FT8's
    /// own centring convention, and writing it out as a distance rather than as a constant is what
    /// keeps FT4's arithmetic free of FT8's geometry.
    /// </para>
    /// <para>
    /// <b>ALL 105 SYMBOLS, RAMPS INCLUDED.</b> FT4's symbols 0 and 104 carry no codeword bits, but
    /// they do carry a <em>tone</em> — upstream writes tone 0 there and
    /// <see cref="Ft4SymbolEncoder"/> ports it — so what was transmitted is known for them exactly as
    /// it is for the other 103, and a known tone is measurable. What the ramps do differ in is the
    /// <em>envelope</em>: <c>Ft4Waveform</c> applies a raised-cosine over the first and last eighth
    /// of a symbol, so those two windows hold about 92 per cent of a symbol's energy. Summed with
    /// 103 full ones that is a few thousandths of a decibel low, and
    /// <c>Ft4Unit294SnrAgreementTests</c> measures it both ways rather than leaving this paragraph to
    /// be believed. <b>Excluding them is the caller's to do</b>, and nothing here does it silently.
    /// </para>
    /// <para>
    /// Decibels are <c>10 log10(1e-12 + power)</c>, which is <c>Ft8DeepBaseband</c>'s own conversion
    /// including its floor, so the two grids are on one scale and
    /// <c>Ft4DeepSignalToNoise</c> can invert the floor exactly the way
    /// <c>Ft8DeepSignalToNoise</c> does.
    /// </para>
    /// </remarks>
    public int TonePowerGrid(double startSeconds, double frequencyOffsetHz, Span<double> decibels)
    {
        var length = SamplesPerSymbol;
        var tones = Ft4SymbolEncoder.ToneCount;

        if (decibels.Length != Ft4SymbolEncoder.SymbolCount * tones)
        {
            throw new ArgumentException(
                $"The grid is {Ft4SymbolEncoder.SymbolCount} symbols of {tones} tones and a span of "
                + $"{decibels.Length} was given.",
                nameof(decibels));
        }

        decibels.Fill(double.NaN);

        var real = _mixed.Real;
        var imaginary = _mixed.Imaginary;

        var rotationCos = new double[length];
        var rotationSin = new double[length];
        var step = -2.0 * Math.PI
            * (BaseFrequencyHz + frequencyOffsetHz - CentreFrequencyHz) / RateHz;
        for (var n = 0; n < length; n++)
        {
            (rotationSin[n], rotationCos[n]) = Math.SinCos(step * n);
        }

        var filled = 0;

        for (var symbol = 0; symbol < Ft4SymbolEncoder.SymbolCount; symbol++)
        {
            var start = _mixed.SampleAt(
                startSeconds + (symbol * Ft8Sharp.Ft4Timing.SymbolPeriodSeconds));

            if (start < 0 || start + length > Length)
            {
                // A symbol whose window falls outside the slot keeps its NaN. The estimator drops
                // it rather than substituting a floor.
                continue;
            }

            for (var tone = 0; tone < tones; tone++)
            {
                var toneOffset = tone * length;
                var sumReal = 0.0;
                var sumImaginary = 0.0;

                for (var n = 0; n < length; n++)
                {
                    var re = real[start + n];
                    var im = imaginary[start + n];

                    // The pre-rotation and then the tone's own bin, in one pass.
                    var pr = (re * rotationCos[n]) - (im * rotationSin[n]);
                    var pi = (re * rotationSin[n]) + (im * rotationCos[n]);

                    var tc = _toneCos[toneOffset + n];
                    var ts = _toneSin[toneOffset + n];

                    sumReal += (pr * tc) - (pi * ts);
                    sumImaginary += (pr * ts) + (pi * tc);
                }

                var power = (sumReal * sumReal) + (sumImaginary * sumImaginary);
                decibels[(symbol * tones) + tone] = 10.0 * Math.Log10(1e-12 + power);
            }

            filled++;
        }

        return filled;
    }
}
