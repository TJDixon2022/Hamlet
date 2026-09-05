using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The signal-to-noise ratio of one decoded message, in decibels in a 2500 Hz reference
/// bandwidth, measured from the power in the tone that was transmitted against the seven tones that
/// were not, at the same instant.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS THE FIRST TYPE IN THIS TREE THAT IS ACTUALLY A SIGNAL-TO-NOISE RATIO</b>, so it says
/// which definition, in which bandwidth, and what its spread is. <c>Ft8SlotLevel</c> is a level and
/// says so at length; <c>Ft8SlotCensus.TopSyncScores</c> are Costas match counts and say so at
/// length; both of those paragraphs exist because a number under a heading is read as a measurement.
/// This one is a measurement, and the paragraphs below are what bounds it.
/// </para>
/// <para>
/// <b>THE DEFINITION, IN FULL, BECAUSE "SNR" ALONE NAMES NOTHING.</b> A ratio needs a bandwidth to
/// be a number. This is the amateur weak-signal convention - the one the published FT8 threshold of
/// about -21 dB is quoted against:
/// </para>
/// <code>
///   SNR(dB) = 10 log10( signal power / noise power in a 2500 Hz reference bandwidth )
/// </code>
/// <para>
/// <b>WHAT IT IS NOT.</b> It is not a Costas sync score - that is a match count in no units. It is
/// not a level - <c>Ft8SlotLevel</c> measures how loud the audio was and says nothing about what is
/// in it. It is not an assessment of the station, the path or the band. It is not the ratio in the
/// signal's own 50 Hz occupancy, which reads about 17 dB better and is not comparable to anything
/// anybody quotes. And it is <b>not a decode confidence</b>: every message this is computed for has
/// already passed the port's parity gate and its CRC-14, and a low figure here does not make a
/// message less likely to be the one that was sent.
/// </para>
/// <para>
/// <b>PURE AND REPORT-ONLY.</b> Nothing in this file is called from a decode path. It changes no
/// ratio, no gate, no count and no decision. It takes samples, a place and a symbol sequence, and
/// returns a number.
/// </para>
/// <para>
/// <b>THE PUBLISHED DESCRIPTION.</b> The frame, the three seven-symbol Costas arrays, the eight-tone
/// alphabet and the description of FT8's own per-message signal-to-noise estimate are from Franke,
/// Somerville and Taylor, <em>The FT4 and FT8 Communication Protocols</em>, QEX, July/August 2020.
/// <b>No route to this arithmetic goes through WSJT-X source or <c>ft4_ft8_public/</c>.</b> The
/// estimator below is the ordinary non-coherent matched-filter energy estimator of detection theory
/// applied to that frame, and every constant in it is derived in
/// <c>docs/unit251-snr-trace.md</c> §4 from the tone spacing and the symbol period alone.
/// </para>
/// <para>
/// <b>THE SPREAD, MEASURED.</b> Over the ladder, in <c>Ft8Unit251SnrAgreementTests</c>. The mean
/// absolute error and the 95th percentile of the absolute error against the delivered ratio are
/// quoted there with the trial count, the rungs and the placement, and the figures move with the
/// placement: a station in the middle of a waterfall cell is a different measurement from one on a
/// bin centre. <b>Nothing here is calibrated to the ladder</b>, and a later unit that fits a
/// constant to it will have turned this from a measurement into a fit.
/// </para>
/// </remarks>
public static class Ft8DeepSignalToNoise
{
    /// <summary>The reference bandwidth the published FT8 figures are quoted in, in hertz.</summary>
    /// <remarks>
    /// The same constant, for the same reason, as the ladder's own
    /// <c>SignalToNoise.ReferenceBandwidthHz</c> in <c>tests/Ft8Sharp.Tests/Dsp</c>. It is a
    /// convention - the nominal SSB channel a receiver hands to a decoder - and not a property of
    /// the signal, which is exactly why it has to be stated rather than assumed.
    /// </remarks>
    public const double ReferenceBandwidthHz = 2500.0;

    /// <summary>
    /// The floor <see cref="Ft8DeepBaseband.TonePowerGrid"/> folds into its decibels, undone here
    /// exactly rather than lived with.
    /// </summary>
    public const double DecibelFloor = 1e-12;

    /// <summary>
    /// <b>How many of the 79 symbols must lie inside the slot before a figure is returned at all.</b>
    /// </summary>
    /// <remarks>
    /// Half the frame. An estimate taken over twelve symbols because the transmission ran off the
    /// end of what was captured is a different quantity from one taken over seventy-nine, and a
    /// caller cannot tell them apart from the number. Below this there is <b>no measurement</b>
    /// rather than a noisier one.
    /// </remarks>
    public const int MinimumSymbols = 40;

    /// <summary>How far either way in time the refinement looks, in seconds.</summary>
    /// <remarks>
    /// Half the waterfall's time step, which is the furthest a coarse candidate can be from the
    /// signal it found. See <see cref="Estimate(Ft8DeepBaseband, double, double, ReadOnlySpan{byte}, bool)"/>.
    /// </remarks>
    public const double TimeSearchSeconds = 0.040;

    /// <summary>How far either way in frequency the refinement looks, in hertz.</summary>
    /// <remarks>Half the waterfall's frequency step, for the same reason.</remarks>
    public const double FrequencySearchHz = 1.6;

    /// <summary>
    /// <b>The noise bandwidth of one tone bin of a symbol-length correlation: the tone spacing.</b>
    /// </summary>
    /// <remarks>
    /// <b>6.25 Hz, and it is the reciprocal of the symbol period rather than anything about the
    /// baseband.</b> <see cref="Ft8DeepBaseband.TonePowerGrid"/> correlates over exactly one symbol,
    /// and the matched filter for a tone of duration <c>T</c> has noise-equivalent bandwidth
    /// <c>1/T</c> whatever rate the samples arrive at. The full derivation is in
    /// <c>docs/unit251-snr-trace.md</c> §4; the property worth having is that the decimation, the
    /// filter length and the sample rate <b>all cancel</b>, so changing any of them does not move
    /// the number.
    /// </remarks>
    public static double BinBandwidthHz => Ft8DeepBaseband.ToneSpacingHz;

    /// <summary>
    /// <b>What carries a per-bin ratio to the 2500 Hz reference: 26.02 dB.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>10 log10(2500 / 6.25) = 10 log10(400) = 26.0206 dB</c>. <b>A derivation, not a fit.</b>
    /// Nothing was measured to produce it and nothing on the ladder was consulted; it is two
    /// published bandwidths and a logarithm.
    /// </para>
    /// <para>
    /// <b>The check that is not a calibration.</b> The published FT8 threshold is about -21 dB in
    /// 2500 Hz, which through this constant is <c>+5.0 dB</c> per bin - the transmitted tone holding
    /// about 3.2 times the power of a wrong one, symbol by symbol. That is the right order for a
    /// rate-one-half code carrying three bits a symbol, and it is quoted to check the sign and the
    /// magnitude of the constant and for nothing else.
    /// </para>
    /// </remarks>
    public static double ReferenceOffsetDecibels =>
        10.0 * Math.Log10(ReferenceBandwidthHz / BinBandwidthHz);

    /// <summary>
    /// <b>The ratio of one decoded message, from the slot's own audio.</b> Mixes, filters and
    /// decimates about the message's eight tones and then measures.
    /// </summary>
    /// <param name="samples">The slot's audio, at <paramref name="sampleRate"/>.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The frequency of the lowest of the message's eight tones.</param>
    /// <param name="startSeconds">
    /// When the message's first symbol began, in seconds from the start of the slot. <b>This is the
    /// start of the signal and not a candidate's nominal time</b> - see the remarks.
    /// </param>
    /// <param name="symbols">
    /// <see cref="Ft8SymbolEncoder.SymbolCount"/> tone indices, as
    /// <see cref="Ft8SymbolEncoder.Encode(ReadOnlySpan{byte})"/> produces them. <b>What was
    /// transmitted, not what was received.</b>
    /// </param>
    /// <param name="settings">How to mix, filter and decimate, or null for the default.</param>
    /// <param name="refine">Whether to look for the place before measuring at it.</param>
    /// <returns>The estimate, or <see cref="Ft8DeepSnrEstimate.NotMeasured"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sample rate is not positive, or the settings do not leave a whole number of baseband
    /// samples in a symbol.
    /// </exception>
    /// <remarks>
    /// <b>ONE BASEBAND BUILD A MESSAGE, AND IT IS THE COST OF THIS WHOLE FEATURE.</b>
    /// <c>Ft8DeepSlotDecoder</c> builds a baseband only for candidates the port refused and only
    /// with fine sync on, so <b>a message that decoded has no baseband behind it</b> and one has to
    /// be made. Unit 248 measured mixing, filtering and searching together at 9.2 ms a candidate and
    /// recorded that the mixing and the 401-tap filter are the expensive part. A caller with several
    /// messages in one waterfall bin should build once and use the overload below.
    /// </remarks>
    public static Ft8DeepSnrEstimate Estimate(
        ReadOnlySpan<float> samples,
        int sampleRate,
        double baseFrequencyHz,
        double startSeconds,
        ReadOnlySpan<byte> symbols,
        Ft8DeepBasebandSettings? settings = null,
        bool refine = true)
    {
        CheckSymbols(symbols);

        var baseband = Ft8DeepBaseband.Build(samples, sampleRate, baseFrequencyHz, settings);
        return Estimate(baseband, startSeconds, 0.0, symbols, refine);
    }

    /// <summary>
    /// <b>The ratio of one decoded message, from a baseband somebody already built.</b>
    /// </summary>
    /// <param name="baseband">The slot, mixed about the message's eight tones.</param>
    /// <param name="startSeconds">
    /// When the message's first symbol began, in seconds from the start of the slot.
    /// </param>
    /// <param name="frequencyOffsetHz">
    /// How far the message's tones sit from where the baseband was mixed.
    /// </param>
    /// <param name="symbols">The transmitted tone indices.</param>
    /// <param name="refine">Whether to look for the place before measuring at it.</param>
    /// <returns>The estimate, or <see cref="Ft8DeepSnrEstimate.NotMeasured"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="baseband"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>THE PLACE IS THE START OF THE SIGNAL, NOT A CANDIDATE'S NOMINAL TIME.</b> The two differ
    /// by <see cref="Ft8DeepSlotDecoder.CandidateTimeBiasSeconds"/>, which is exactly minus one
    /// symbol period and was measured by unit 248 rather than derived. A caller that hands over
    /// <c>Ft8SlotMessage.TimeSeconds(geometry)</c> unbiased is measuring a window one symbol early,
    /// which reads as an estimator that does not work rather than as a caller that got the place
    /// wrong.
    /// </para>
    /// <para>
    /// <b>WHY IT REFINES, AND WHY THAT IS ALIGNMENT AND NOT CALIBRATION.</b> The correlation is a
    /// matched filter and is only matched at the right place. The waterfall's time step is 0.080 s
    /// and its frequency step 3.125 Hz, so a coarse candidate can sit a quarter of a symbol and half
    /// a tone-spacing quarter away from the signal: <c>(1 - 0.25)^2</c> in time is <b>-2.50 dB</b>
    /// and <c>sinc^2(0.25)</c> in frequency is <b>-0.91 dB</b>, and the energy that leaves the
    /// correct bin arrives in the wrong ones, where it inflates the noise estimate as well.
    /// <b>Up to 3.4 dB, low, against a gate of 2 dB</b>, depending only on where the station
    /// happened to land in the analysis cell. The search below moves the window; it does not touch
    /// the arithmetic, and no constant is adjusted by it.
    /// </para>
    /// <para>
    /// <b>AND WHAT REFINING COSTS, STATED RATHER THAN HIDDEN.</b> Taking the best of a grid of noisy
    /// statistics biases the answer high, because the maximum of several draws is above their mean.
    /// The statistic here is summed over up to 79 symbols and 7 wrong bins each, so the effect is
    /// small - a few tenths of a decibel at the weakest rungs and less above them - but it is not
    /// zero and it is in the direction that flatters the number.
    /// <c>Ft8Unit251SnrAgreementTests</c> quotes the unrefined figure beside the refined one at
    /// every rung so the trade is visible.
    /// </para>
    /// </remarks>
    public static Ft8DeepSnrEstimate Estimate(
        Ft8DeepBaseband baseband,
        double startSeconds,
        double frequencyOffsetHz,
        ReadOnlySpan<byte> symbols,
        bool refine = true)
    {
        ArgumentNullException.ThrowIfNull(baseband);
        CheckSymbols(symbols);

        Span<double> grid = new double[Ft8SymbolEncoder.SymbolCount * Ft8SymbolEncoder.ToneCount];

        var time = startSeconds;
        var frequency = frequencyOffsetHz;

        if (refine)
        {
            // A COORDINATE SEARCH AND NOT A GRID. Time, then frequency, then time again at a
            // twentieth of the first step - twenty-seven correlations rather than the hundred and
            // seventy a product of the two axes would take, and the two axes are close enough to
            // separable over this extent for it. The residual after the third pass is under a
            // thousandth of a symbol and a fifth of a hertz, which is -0.01 and -0.02 dB.
            time = BestTime(baseband, symbols, grid, time, frequency, TimeSearchSeconds, 0.010);
            frequency = BestFrequency(
                baseband, symbols, grid, time, frequency, FrequencySearchHz, 0.40);
            time = BestTime(baseband, symbols, grid, time, frequency, 0.005, 0.00125);
        }

        var measured = Measure(baseband, symbols, grid, time, frequency);

        if (measured.Symbols < MinimumSymbols || !(measured.Signal > 0.0))
        {
            // NO MEASUREMENT, AND NOT A FLOOR. Too little of the frame inside the slot, or a
            // correct bin that does not stand above the seven wrong ones at all. A clamped decibel
            // figure here would be indistinguishable on the screen from a measured weak signal,
            // which is the fault CLAUDE.md 0.0 names.
            return Ft8DeepSnrEstimate.NotMeasured;
        }

        var decibels = (10.0 * Math.Log10(measured.Signal / measured.Noise)) - ReferenceOffsetDecibels;

        return new Ft8DeepSnrEstimate(
            decibels,
            measured.Symbols,
            time - startSeconds,
            frequency - frequencyOffsetHz);
    }

    /// <summary>
    /// The linear power behind one of <see cref="Ft8DeepBaseband.TonePowerGrid"/>'s decibel figures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THE FLOOR IS UNDONE EXACTLY.</b> The grid reports <c>10 log10(1e-12 + power)</c>, so the
    /// inverse is <c>10^(dB/10) - 1e-12</c> and not <c>10^(dB/10)</c>. The clamp catches the
    /// rounding case where a bin that held nothing comes back a hair below zero.
    /// </para>
    /// <para>
    /// <b>AND IT IS UNDONE BEFORE ANYTHING IS AVERAGED.</b> Averaging decibels averages logarithms,
    /// which is the logarithm of a geometric mean and not the power in the bins. The wrong bins hold
    /// noise whose per-bin power is exponentially distributed, and the geometric mean of an
    /// exponential sits <b>2.51 dB</b> below its arithmetic mean. A noise floor 2.5 dB low is an SNR
    /// 2.5 dB high, on a gate of 2 dB.
    /// </para>
    /// </remarks>
    private static double Power(double decibels) =>
        Math.Max(0.0, Math.Pow(10.0, decibels / 10.0) - DecibelFloor);

    /// <summary>
    /// The transmitted tones' summed power, the seven-tone noise estimate, and how many symbols
    /// both were taken over.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>SUM THE POWERS FIRST AND DIVIDE ONCE.</b> A per-symbol ratio is a ratio of exponential
    /// variates: its sample mean is dominated by whichever symbol drew the smallest denominator, and
    /// at the rungs this is used at that is most of them. The sums are the non-coherent energy
    /// estimator.
    /// </para>
    /// <para>
    /// <b>THE NOISE IS TAKEN AT THE SAME INSTANT AND NOWHERE ELSE.</b> Seven wrong bins of the same
    /// symbol are seven independent looks at whatever else is in that 50 Hz at that moment. A floor
    /// taken from a quiet part of the slot would be measuring a different moment, and on a band
    /// where the interference is other FT8 stations that is a different quantity.
    /// </para>
    /// <para>
    /// <b>THE SUBTRACTION IS WHY IT IS SIGNAL AND NOT SIGNAL-PLUS-NOISE.</b> The correct bin holds
    /// the tone <em>and</em> its own share of the noise; the seven wrong bins estimate that share.
    /// </para>
    /// <para>
    /// <b>A NaN SYMBOL IS DROPPED AND NOT REPLACED.</b> The grid leaves
    /// <see cref="double.NaN"/> where a symbol's window falls outside the baseband, and a zero or a
    /// floor substituted there would pull the mean toward a number nothing measured.
    /// </para>
    /// </remarks>
    private static (double Signal, double Noise, int Symbols) Measure(
        Ft8DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        Span<double> grid,
        double startSeconds,
        double frequencyOffsetHz)
    {
        baseband.TonePowerGrid(startSeconds, frequencyOffsetHz, grid);

        var tones = Ft8SymbolEncoder.ToneCount;
        var correct = 0.0;
        var wrong = 0.0;
        var used = 0;

        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            var row = grid.Slice(symbol * tones, tones);

            if (double.IsNaN(row[0]))
            {
                continue;
            }

            var total = 0.0;
            for (var tone = 0; tone < tones; tone++)
            {
                total += Power(row[tone]);
            }

            var here = Power(row[symbols[symbol]]);
            correct += here;
            wrong += (total - here) / (tones - 1);
            used++;
        }

        return (correct - wrong, wrong, used);
    }

    /// <summary>The time in a window about <paramref name="centre"/> with the most signal in it.</summary>
    private static double BestTime(
        Ft8DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        Span<double> grid,
        double centre,
        double frequencyOffsetHz,
        double extent,
        double step)
    {
        var best = centre;
        var bestScore = double.NegativeInfinity;

        for (var offset = -extent; offset <= extent + (step / 2.0); offset += step)
        {
            var at = centre + offset;
            var score = Score(baseband, symbols, grid, at, frequencyOffsetHz);

            if (score > bestScore)
            {
                bestScore = score;
                best = at;
            }
        }

        return best;
    }

    /// <summary>The frequency in a window about <paramref name="centre"/> with the most signal in it.</summary>
    private static double BestFrequency(
        Ft8DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        Span<double> grid,
        double startSeconds,
        double centre,
        double extent,
        double step)
    {
        var best = centre;
        var bestScore = double.NegativeInfinity;

        for (var offset = -extent; offset <= extent + (step / 2.0); offset += step)
        {
            var at = centre + offset;
            var score = Score(baseband, symbols, grid, startSeconds, at);

            if (score > bestScore)
            {
                bestScore = score;
                best = at;
            }
        }

        return best;
    }

    /// <summary>
    /// What the search maximises: the signal energy, per symbol read, at one place.
    /// </summary>
    /// <remarks>
    /// <b>Per symbol read, so a place near either end of the slot is not scored down for having
    /// fewer symbols inside it</b> - which would bias every search toward the middle of the slot.
    /// <c>Ft8DeepBaseband.SyncScore</c> averages rather than sums for the same reason. A place with
    /// no symbol inside the slot at all scores negative infinity and is never chosen.
    /// </remarks>
    private static double Score(
        Ft8DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        Span<double> grid,
        double startSeconds,
        double frequencyOffsetHz)
    {
        var (signal, _, used) = Measure(baseband, symbols, grid, startSeconds, frequencyOffsetHz);
        return used == 0 ? double.NegativeInfinity : signal / used;
    }

    private static void CheckSymbols(ReadOnlySpan<byte> symbols)
    {
        if (symbols.Length != Ft8SymbolEncoder.SymbolCount)
        {
            throw new ArgumentException(
                $"A transmission is {Ft8SymbolEncoder.SymbolCount} channel symbols and "
                + $"{symbols.Length} were given. There is no ratio to measure against part of a "
                + "frame.",
                nameof(symbols));
        }
    }
}

/// <summary>
/// <b>One message's signal-to-noise ratio, and enough about how it was taken to refuse it.</b>
/// </summary>
/// <param name="Decibels">
/// The ratio in the 2500 Hz reference bandwidth, or <see cref="double.NaN"/> where nothing was
/// measured. <b>NaN and never a floor</b>, for the reason
/// <c>Hamlet.RadioEngine.Audio.Ft8SlotLevel</c> gives at length: a substituted number is
/// indistinguishable downstream from a measured one.
/// </param>
/// <param name="Symbols">
/// How many of the 79 symbols had their window inside the slot. An estimate over 40 of them is a
/// different quantity from one over 79 and a caller cannot tell from the decibels.
/// </param>
/// <param name="TimeAdjustmentSeconds">
/// How far the refinement moved the window from the place it was given. <b>Diagnostic only.</b> A
/// distribution of these piling up against
/// <see cref="Ft8DeepSignalToNoise.TimeSearchSeconds"/> says the extent is too narrow.
/// </param>
/// <param name="FrequencyAdjustmentHz">The same, in frequency.</param>
/// <remarks>
/// <b>THE MEANING OF THIS NUMBER NEVER CHANGES BETWEEN SURFACES</b> - the panel, the telemetry line
/// and the capture sidecar carry the same decibels in the same reference bandwidth from the same
/// estimator, and <b>a message with no measurement carries nothing rather than a zero</b>.
/// </remarks>
public readonly record struct Ft8DeepSnrEstimate(
    double Decibels,
    int Symbols,
    double TimeAdjustmentSeconds,
    double FrequencyAdjustmentHz)
{
    /// <summary>Nothing was measured.</summary>
    public static Ft8DeepSnrEstimate NotMeasured { get; } = new(double.NaN, 0, 0.0, 0.0);

    /// <summary>True where there is a figure here.</summary>
    public bool IsMeasured => !double.IsNaN(Decibels);
}
