using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The signal-to-noise ratio of one decoded FT4 message, in decibels in a 2500 Hz reference
/// bandwidth, measured from the power in the tone that was transmitted against the three tones that
/// were not, at the same instant.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE DEFINITION, IN FULL, BECAUSE "SNR" ALONE NAMES NOTHING.</b> A ratio needs a bandwidth to be
/// a number. This is the amateur weak-signal convention — the same one <c>Ft8DeepSignalToNoise</c>
/// reports in, so <b>a figure from this estimator and a figure from that one mean the same thing</b>
/// and an operator reading an FT4 row and an FT8 row is reading one scale:
/// </para>
/// <code>
///   SNR(dB) = 10 log10( signal power / noise power in a 2500 Hz reference bandwidth )
/// </code>
/// <para>
/// <b>WHAT IT IS NOT.</b> It is not a Costas sync score — that is a match count in no units. It is
/// not a level. It is not an assessment of the station, the path or the band. And it is <b>not a
/// decode confidence</b>: every message this is computed for has already passed the port's parity
/// gate and its CRC-14, and a low figure here does not make a message less likely to be the one that
/// was sent.
/// </para>
/// <para>
/// <b>PURE AND REPORT-ONLY.</b> Nothing in this file is called from a decode path. It changes no
/// ratio, no gate, no count and no decision. It takes samples, a place and a symbol sequence, and
/// returns a number. <b>That property is what licenses this file to exist in a phase whose plan
/// excludes sensitivity work</b>, and it is the same property
/// <c>Ft8DeepSignalToNoise</c> states in the same place;
/// <c>Ft4Unit294SnrAgreementTests</c> asserts it by decoding the same FT4 slot again after the
/// estimate has been taken and requiring the identical <c>Ft8SlotResult</c>.
/// </para>
/// <para>
/// <b>THE PUBLISHED DESCRIPTION.</b> FT4's frame — 105 symbols of 0.048 s, four tones spaced
/// 20.833 Hz, four four-symbol Costas groups and two ramps — is from Franke K9AN, Somerville G4WJS
/// and Taylor K1JT, <i>The FT4 and FT8 Communication Protocols</i>, QEX, July/August 2020, <b>the same
/// paper the port already cites and the one that covers both protocols</b>. <b>No route to this
/// arithmetic goes through WSJT-X source or <c>ft4_ft8_public/</c>.</b> The estimator below is the
/// ordinary non-coherent matched-filter energy estimator of detection theory applied to that frame.
/// </para>
/// <para>
/// <b>EVERY CONSTANT IS DERIVED AT FT4'S OWN PERIOD AND NONE IS TRANSLITERATED FROM FT8'S.</b>
/// <c>docs/unit251-snr-trace.md</c> §4 works the derivation out in general — the bin ratio is a
/// signal-to-noise ratio in a noise bandwidth of <c>1/T</c>, where <c>T</c> is the symbol period, and
/// the sample rate, the decimation and the filter length <b>all cancel</b>. Putting FT4's
/// <c>T = 0.048 s</c> through it gives a bin of <b>20.833 Hz</b> and a reference offset of
/// <c>10 log10(2500 / 20.833) = 20.79 dB</c>, against FT8's 6.25 Hz and 26.02 dB. <b>A derivation,
/// not a fit</b>, and not FT8's number reused.
/// </para>
/// <para>
/// <b>THE SPREAD IS WIDER THAN FT8'S AND THAT IS EXPECTED RATHER THAN A DEFECT.</b> The noise at each
/// instant is estimated from the <b>three</b> tones that were not sent, where FT8 has seven. Over a
/// whole frame that is 105 x 3 = 315 independent looks against FT8's 79 x 7 = 553, so the noise
/// estimate's own relative standard error is <c>sqrt(553/315) = 1.32</c> times FT8's — about 0.24 dB
/// against 0.18 dB from that source alone. <b>The figure is quoted from the ladder rather than from
/// this paragraph</b>: <c>Ft4Unit294SnrAgreementTests</c> measures it at FT4's own rungs.
/// </para>
/// <para>
/// <b>AND ONE MORE THING IS COARSER, NAMED BECAUSE IT IS A BIAS.</b> The baseband holds 24 samples in
/// an FT4 symbol where it holds 80 in an FT8 one, so <c>SampleAt</c>'s rounding leaves up to half a
/// baseband sample — 1 ms — of residual time error, which is 2.1 per cent of a 48 ms symbol against
/// 0.6 per cent of a 160 ms one. The matched filter loses roughly <c>(1 - |dt|/T)^2</c>, so the worst
/// case is <b>-0.18 dB</b> and the mean over a uniform residual is about <b>-0.06 dB</b>. It is
/// <em>low</em>, and the refinement's own bias is <em>high</em>, so the two partly cancel; the ladder
/// prints the measured bias rather than this arithmetic being trusted.
/// </para>
/// <para>
/// <b>IT READS LOW ON A STRONG STATION, AND THAT IS IN THE ARITHMETIC RATHER THAN A DEFECT OF IT.</b>
/// The noise is estimated from the three tones that were <em>not</em> sent at that instant, and GFSK
/// smoothing puts a little of the transmitted tone into its neighbours — so the noise estimate carries
/// a term proportional to the <b>signal</b>. At a weak rung that term is lost in the real noise; at a
/// strong one it is most of what the three wrong bins hold, and the ratio is pulled down. Measured
/// over <c>Ft4Unit294SnrAgreementTests</c>' 970 messages the mean signed error runs from
/// <b>-0.07 dB at -13 dB to -1.18 dB at -1 dB</b>. <b>It is reported and not corrected</b>: a
/// correction fitted to that table would turn this from a measurement into a fit, which is the one
/// thing the file above forbids. An operator reading it should know that a very strong station may be
/// reported about a decibel pessimistic, and that no station he can barely hear is.
/// </para>
/// <para>
/// <b>NOTHING HERE IS CALIBRATED TO THE LADDER</b>, and a later unit that fits a constant to it will
/// have turned this from a measurement into a fit.
/// </para>
/// </remarks>
public static class Ft4DeepSignalToNoise
{
    /// <summary>The reference bandwidth the published figures are quoted in, in hertz.</summary>
    /// <remarks>
    /// <b>The same 2500 Hz as FT8's, on purpose.</b> It is a convention — the nominal SSB channel a
    /// receiver hands to a decoder — and not a property of the signal, so it does not change with the
    /// protocol. If it did, an FT4 row and an FT8 row would carry different quantities under one
    /// column heading, which is the fault <c>Ft8SlotSnrs</c>' remarks argue against at length.
    /// </remarks>
    public const double ReferenceBandwidthHz = Ft8DeepSignalToNoise.ReferenceBandwidthHz;

    /// <summary>
    /// The floor <see cref="Ft4DeepBaseband.TonePowerGrid"/> folds into its decibels, undone here
    /// exactly rather than lived with.
    /// </summary>
    public const double DecibelFloor = Ft8DeepSignalToNoise.DecibelFloor;

    /// <summary>
    /// <b>How many of the 105 symbols must lie inside the slot before a figure is returned at all: 53.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THE RULE IS FT8'S AND THE NUMBER IS NOT.</b> FT8's <c>MinimumSymbols</c> is 40 of 79 — half
    /// the frame, so that an estimate taken over a fragment because the transmission ran off the end
    /// of what was captured is <em>no measurement</em> rather than a noisier one that a caller cannot
    /// tell apart from a real weak signal. <b>Half of 105 is 52.5, and it is rounded up to 53</b>, so
    /// that "at least half the frame" is true rather than nearly true.
    /// </para>
    /// <para>
    /// <b>It is a judgment and it is stated as one.</b> No document rules on it; the argument above is
    /// the whole of the reason, and a later unit changing it has to change a documented constant.
    /// </para>
    /// </remarks>
    public const int MinimumSymbols = (Ft4SymbolEncoder.SymbolCount + 1) / 2;

    /// <summary>How far either way in time the refinement looks, in seconds: 0.012.</summary>
    /// <remarks>
    /// <b>Half the FT4 waterfall's time step, which is the furthest a coarse candidate can be from the
    /// signal it found.</b> <c>Ft4WaterfallGeometry</c> oversamples time twice, so its step is half
    /// FT4's 0.048 s symbol and the furthest a signal can sit from a block boundary is half of that
    /// again. Derived rather than written down: FT8's equivalent is 0.040 s at its own period and
    /// neither number is the other's.
    /// </remarks>
    public static double TimeSearchSeconds =>
        Ft8Sharp.Ft4Timing.SymbolPeriodSeconds
        / Ft8WaterfallGeometry.DefaultTimeOversampling
        / 2.0;

    /// <summary>How far either way in frequency the refinement looks, in hertz: 5.21.</summary>
    /// <remarks>Half the FT4 waterfall's frequency step, for the same reason.</remarks>
    public static double FrequencySearchHz =>
        Ft4DeepBaseband.ToneSpacingHz
        / Ft8WaterfallGeometry.DefaultFrequencyOversampling
        / 2.0;

    /// <summary>
    /// <b>The noise bandwidth of one tone bin of a symbol-length correlation: the tone spacing,
    /// 20.833 Hz.</b>
    /// </summary>
    /// <remarks>
    /// <b>The reciprocal of FT4's symbol period, and nothing about the baseband.</b>
    /// <see cref="Ft4DeepBaseband.TonePowerGrid"/> correlates over exactly one symbol, and the matched
    /// filter for a tone of duration <c>T</c> has noise-equivalent bandwidth <c>1/T</c> whatever rate
    /// the samples arrive at. The derivation is <c>docs/unit251-snr-trace.md</c> §4, which is written
    /// in <c>T</c> and not in FT8's number; the property worth having is that the decimation, the
    /// filter length and the sample rate <b>all cancel</b>.
    /// </remarks>
    public static double BinBandwidthHz => Ft4DeepBaseband.ToneSpacingHz;

    /// <summary>
    /// <b>What carries a per-bin ratio to the 2500 Hz reference: 20.79 dB.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>10 log10(2500 / 20.8333) = 10 log10(120) = 20.7918 dB</c>, against FT8's
    /// <c>10 log10(400) = 26.0206</c>. <b>A derivation, not a fit.</b> Nothing was measured to produce
    /// it and nothing on the ladder was consulted; it is two published bandwidths and a logarithm.
    /// <b>The 5.23 dB it differs from FT8's by is the whole of why FT8's constant could not simply be
    /// reused</b>: an FT4 row measured through it would read 5.2 dB strong, which is a report an
    /// operator would send and another operator would write down.
    /// </para>
    /// <para>
    /// <b>The check that is not a calibration.</b> FT4 is about 3.5 dB less sensitive than FT8, so a
    /// threshold near <c>-17.5 dB</c> in 2500 Hz is <c>+3.3 dB</c> per bin through this constant — the
    /// transmitted tone holding about 2.1 times the power of a wrong one, symbol by symbol. That is
    /// the right order for a rate-one-half code carrying two bits a symbol, and it is quoted to check
    /// the sign and the magnitude of the constant and for nothing else.
    /// </para>
    /// </remarks>
    public static double ReferenceOffsetDecibels =>
        10.0 * Math.Log10(ReferenceBandwidthHz / BinBandwidthHz);

    /// <summary>
    /// <b>The ratio of one decoded FT4 message, from the slot's own audio.</b> Mixes, filters and
    /// decimates about the message's four tones and then measures.
    /// </summary>
    /// <param name="samples">The slot's audio, at <paramref name="sampleRate"/>.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The frequency of the lowest of the message's four tones.</param>
    /// <param name="startSeconds">
    /// When the message's first symbol began, in seconds from the start of the slot. <b>This is the
    /// start of the signal and not a candidate's nominal time</b> — see the remarks on the overload.
    /// </param>
    /// <param name="symbols">
    /// <see cref="Ft4SymbolEncoder.SymbolCount"/> tone indices, as
    /// <see cref="Ft4SymbolEncoder.Encode(ReadOnlySpan{byte})"/> produces them. <b>What was
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

        var baseband = Ft4DeepBaseband.Build(samples, sampleRate, baseFrequencyHz, settings);
        return Estimate(baseband, startSeconds, 0.0, symbols, refine);
    }

    /// <summary>
    /// <b>The ratio of one decoded FT4 message, from a baseband somebody already built.</b>
    /// </summary>
    /// <param name="baseband">The slot, mixed about the message's four tones.</param>
    /// <param name="startSeconds">
    /// When the message's first symbol began, in seconds from the start of the slot.
    /// </param>
    /// <param name="frequencyOffsetHz">
    /// How far the message's tones sit from where the baseband was told its base tone was.
    /// </param>
    /// <param name="symbols">The transmitted tone indices.</param>
    /// <param name="refine">Whether to look for the place before measuring at it.</param>
    /// <returns>The estimate, or <see cref="Ft8DeepSnrEstimate.NotMeasured"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="baseband"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>THE PLACE IS THE START OF THE SIGNAL, NOT A CANDIDATE'S NOMINAL TIME.</b> The two differ by
    /// a bias, exactly as they do on FT8, and <b>FT8's figure does not transfer</b>: it is minus one
    /// <em>FT8</em> symbol period. FT4's own is <see cref="Ft4CandidateTimeBiasSeconds"/>, measured
    /// rather than assumed. A caller that hands over <c>Ft8SlotMessage.TimeSeconds(geometry)</c>
    /// unbiased is measuring a window one symbol early, <b>which reads as an estimator that does not
    /// work rather than as a caller that got the place wrong.</b>
    /// </para>
    /// <para>
    /// <b>WHY IT REFINES, AND WHY THAT IS ALIGNMENT AND NOT CALIBRATION.</b> The correlation is a
    /// matched filter and is only matched at the right place. FT4's waterfall time step is 0.024 s and
    /// its frequency step 10.42 Hz, so a coarse candidate can sit a quarter of a symbol and a quarter
    /// of a tone spacing away from the signal: <c>(1 - 0.25)^2</c> in time is <b>-2.50 dB</b> and
    /// <c>sinc^2(0.25)</c> in frequency is <b>-0.91 dB</b> — the same two figures FT8 gets, because
    /// both are fractions of FT4's own symbol and tone rather than absolute quantities — and the
    /// energy that leaves the correct bin arrives in the wrong ones, where it inflates the noise
    /// estimate as well. The search below moves the window; it does not touch the arithmetic, and no
    /// constant is adjusted by it.
    /// </para>
    /// <para>
    /// <b>AND WHAT REFINING COSTS, STATED RATHER THAN HIDDEN.</b> Taking the best of a grid of noisy
    /// statistics biases the answer high, because the maximum of several draws is above their mean.
    /// <b>The effect is larger here than on FT8</b>, because the statistic is summed over 3 wrong bins
    /// a symbol rather than 7 and is therefore noisier to begin with.
    /// <c>Ft4Unit294SnrAgreementTests</c> quotes the unrefined figure beside the refined one at every
    /// rung so the trade is visible rather than absorbed.
    /// </para>
    /// <para>
    /// <b>THE TIME SEARCH STEPS BY ONE BASEBAND SAMPLE AND NOT FINER.</b>
    /// <c>Ft8DeepBaseband.SampleAt</c> rounds a time to the nearest baseband sample, so at 500 Hz two
    /// requested times inside 2 ms of each other are the same window and a finer step would be
    /// counting the same correlation twice. FT8's estimator takes a second, finer time pass for that
    /// reason and here it would buy nothing; there is a short second pass instead, to catch the
    /// interaction with the frequency move.
    /// </para>
    /// </remarks>
    public static Ft8DeepSnrEstimate Estimate(
        Ft4DeepBaseband baseband,
        double startSeconds,
        double frequencyOffsetHz,
        ReadOnlySpan<byte> symbols,
        bool refine = true)
    {
        ArgumentNullException.ThrowIfNull(baseband);
        CheckSymbols(symbols);

        Span<double> grid =
            new double[Ft4SymbolEncoder.SymbolCount * Ft4SymbolEncoder.ToneCount];

        var time = startSeconds;
        var frequency = frequencyOffsetHz;

        if (refine)
        {
            // A COORDINATE SEARCH AND NOT A GRID, for the reason FT8's estimator gives: the two axes
            // are close enough to separable over this extent, and twenty-five correlations rather
            // than the hundred and seventeen a product of the two would take.
            var sample = 1.0 / baseband.RateHz;

            time = BestTime(baseband, symbols, grid, time, frequency, TimeSearchSeconds, sample);
            frequency = BestFrequency(
                baseband, symbols, grid, time, frequency,
                FrequencySearchHz, FrequencySearchHz / 4.0);
            time = BestTime(baseband, symbols, grid, time, frequency, 2.0 * sample, sample);
        }

        var measured = Measure(baseband, symbols, grid, time, frequency);

        if (measured.Symbols < MinimumSymbols || !(measured.Signal > 0.0))
        {
            // NO MEASUREMENT, AND NOT A FLOOR. Too little of the frame inside the slot, or a correct
            // bin that does not stand above the three wrong ones at all. A clamped decibel figure
            // here would be indistinguishable on the screen from a measured weak signal - and worse
            // than that, it would go on the air as a report.
            return Ft8DeepSnrEstimate.NotMeasured;
        }

        var decibels =
            (10.0 * Math.Log10(measured.Signal / measured.Noise)) - ReferenceOffsetDecibels;

        return new Ft8DeepSnrEstimate(
            decibels,
            measured.Symbols,
            time - startSeconds,
            frequency - frequencyOffsetHz);
    }

    /// <summary>
    /// <b>The distance from an FT4 candidate's nominal time to the start of the signal it found:
    /// minus one FT4 symbol period.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>MEASURED, NOT INHERITED AND NOT DERIVED.</b> FT8's equivalent,
    /// <c>Ft8DeepSlotDecoder.CandidateTimeBiasSeconds</c>, is minus one <em>FT8</em> symbol period and
    /// unit 248 measured it rather than deriving it; nothing about that measurement carries to another
    /// protocol's search. <b>Work instruction 294 task 5 measured FT4's directly</b>, the one way that
    /// needs no estimator in the loop: synthesize a slot whose signal start is known exactly —
    /// <c>Ft4Waveform.PaddingSampleCount / rate</c> — decode it, and subtract the decoder's reported
    /// <c>Ft8SlotMessage.TimeSeconds(geometry)</c> from it. <c>Ft4Unit294SnrAgreementTests</c> takes
    /// that difference over every trial it runs and asserts it is this constant on every one of them,
    /// and prints the distribution.
    /// </para>
    /// <para>
    /// <b>That it comes out at exactly one symbol period, as FT8's does, is a result and not the
    /// reason.</b> The two searches share their structure — the Costas correlation's block index is
    /// one symbol ahead of the frame start in both — and the arithmetic here is written at FT4's
    /// period so that the two cannot be confused for one another.
    /// </para>
    /// </remarks>
    public static double Ft4CandidateTimeBiasSeconds => -Ft8Sharp.Ft4Timing.SymbolPeriodSeconds;

    /// <summary>
    /// The linear power behind one of <see cref="Ft4DeepBaseband.TonePowerGrid"/>'s decibel figures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THE FLOOR IS UNDONE EXACTLY.</b> The grid reports <c>10 log10(1e-12 + power)</c>, so the
    /// inverse is <c>10^(dB/10) - 1e-12</c> and not <c>10^(dB/10)</c>.
    /// </para>
    /// <para>
    /// <b>AND IT IS UNDONE BEFORE ANYTHING IS AVERAGED.</b> Averaging decibels averages logarithms,
    /// which is the logarithm of a geometric mean and not the power in the bins. The wrong bins hold
    /// noise whose per-bin power is exponentially distributed, and the geometric mean of an
    /// exponential sits <b>2.51 dB</b> below its arithmetic mean whatever the number of bins — so this
    /// is the same 2.5 dB error on FT4 as on FT8, and there are fewer bins here to notice it in.
    /// </para>
    /// </remarks>
    private static double Power(double decibels) =>
        Math.Max(0.0, Math.Pow(10.0, decibels / 10.0) - DecibelFloor);

    /// <summary>
    /// The transmitted tones' summed power, the three-tone noise estimate, and how many symbols both
    /// were taken over.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>SUM THE POWERS FIRST AND DIVIDE ONCE.</b> A per-symbol ratio is a ratio of exponential
    /// variates: its sample mean is dominated by whichever symbol drew the smallest denominator, and
    /// with only three bins in the denominator that is worse here than on FT8, not better. The sums
    /// are the non-coherent energy estimator.
    /// </para>
    /// <para>
    /// <b>THE NOISE IS TAKEN AT THE SAME INSTANT AND NOWHERE ELSE.</b> Three wrong bins of the same
    /// symbol are three independent looks at whatever else is in that 83 Hz at that moment. A floor
    /// taken from a quiet part of the slot would be measuring a different moment, and on a band where
    /// the interference is other FT4 stations that is a different quantity.
    /// </para>
    /// <para>
    /// <b>THE SUBTRACTION IS WHY IT IS SIGNAL AND NOT SIGNAL-PLUS-NOISE.</b> The correct bin holds the
    /// tone <em>and</em> its own share of the noise; the three wrong bins estimate that share.
    /// </para>
    /// <para>
    /// <b>A NaN SYMBOL IS DROPPED AND NOT REPLACED.</b> The grid leaves <see cref="double.NaN"/> where
    /// a symbol's window falls outside the baseband, and a zero or a floor substituted there would
    /// pull the sum toward a number nothing measured.
    /// </para>
    /// <para>
    /// <b>ALL 105 SYMBOLS ARE READ, THE TWO RAMPS INCLUDED, AND THAT IS A DECISION.</b> They carry no
    /// codeword bits but they do carry a known tone — upstream writes tone 0 and
    /// <see cref="Ft4SymbolEncoder"/> ports it — so what was transmitted is known for them exactly as
    /// for the other 103, and <b>a known tone is measurable</b>. What they differ in is the envelope:
    /// <c>Ft4Waveform</c> ramps the first and last eighth of a symbol, so those two windows hold about
    /// 92 per cent of a symbol's energy, which summed with 103 full ones is <b>about -0.007 dB</b>.
    /// Excluding them would drop the count from 105 to 103 and would make
    /// <see cref="MinimumSymbols"/> a fraction of a frame the estimator does not read.
    /// <c>Ft4Unit294SnrAgreementTests</c> measures the figure both ways rather than leaving this to be
    /// believed.
    /// </para>
    /// </remarks>
    private static (double Signal, double Noise, int Symbols) Measure(
        Ft4DeepBaseband baseband,
        ReadOnlySpan<byte> symbols,
        Span<double> grid,
        double startSeconds,
        double frequencyOffsetHz)
    {
        baseband.TonePowerGrid(startSeconds, frequencyOffsetHz, grid);

        var tones = Ft4SymbolEncoder.ToneCount;
        var correct = 0.0;
        var wrong = 0.0;
        var used = 0;

        for (var symbol = 0; symbol < Ft4SymbolEncoder.SymbolCount; symbol++)
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
        Ft4DeepBaseband baseband,
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
        Ft4DeepBaseband baseband,
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
    /// <b>Per symbol read, so a place near either end of the slot is not scored down for having fewer
    /// symbols inside it</b> — which would bias every search toward the middle of the slot. A place
    /// with no symbol inside the slot at all scores negative infinity and is never chosen.
    /// </remarks>
    private static double Score(
        Ft4DeepBaseband baseband,
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
        if (symbols.Length != Ft4SymbolEncoder.SymbolCount)
        {
            throw new ArgumentException(
                $"An FT4 transmission is {Ft4SymbolEncoder.SymbolCount} channel symbols and "
                + $"{symbols.Length} were given. There is no ratio to measure against part of a "
                + "frame.",
                nameof(symbols));
        }
    }
}
