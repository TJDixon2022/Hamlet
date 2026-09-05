using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>Re-synthesises one decoded message at its measured place, fits the amplitude and the carrier
/// phase it arrived at, and subtracts it from the slot.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHAT THIS IS.</b> The multi-pass subtract-and-decode-again strategy described in Franke,
/// Somerville and Taylor, <em>The FT4 and FT8 Communication Protocols</em>, QEX, July/August 2020 —
/// the paper the port's own <c>NOTICE</c> cites for the waveform this subtracts. The least-squares
/// fit below is textbook arithmetic and is written out in <c>docs/unit253-subtraction.md</c> §2.
/// <b>No route to any of it goes through WSJT-X's source or <c>ft4_ft8_public/</c>.</b>
/// </para>
/// <para>
/// <b>WHAT THIS IS NOT.</b> It is not a gate, a threshold or an acceptance rule. Nothing here
/// decides that a message is real; it is handed a message the port's parity gate and CRC-14 already
/// accepted, and it writes samples. <b>It does not change how many codewords are put to those gates
/// per candidate per pass</b> — that stays at one, and a pass over a residual is an ordinary decode
/// of a different buffer.
/// </para>
/// <para>
/// <b>AND IT IS NOT A TRANSMIT PATH.</b> <c>CLAUDE.md</c> §0.2 governs keying a transmitter. What
/// this builds is a <c>float[]</c> that is subtracted from a copy of a received slot and dropped;
/// there is no device, no stream and no file. <c>Ft8DeepBoundaryTests</c> asserts that no Hamlet
/// assembly — and therefore no audio device — is reachable from <c>Ft8Sharp.Deep</c> at all, and
/// <c>Ft8DeepSubtractionTests.NothingInTheSiblingCanReachAnAudioDevice</c> asserts it again for this
/// type by name rather than leaving it to this paragraph.
/// </para>
/// <para>
/// <b>REFUSAL IS A CORRECT ANSWER AND IT IS LOUD.</b> A message whose 79 channel symbols
/// <c>Ft8DeepMessageSymbols.TryEncode</c> will not give up is <b>not subtracted</b>, and neither is
/// one with less than <c>Ft8DeepSubtractionSettings.MinimumSymbols</c> of its frame inside the slot.
/// Both return <c>Ft8DeepSubtractionFit.NotFitted</c>, which carries <see cref="double.NaN"/> in
/// every measured field rather than a zero, and both are counted in
/// <c>Ft8DeepSubtractionCounts</c>. A silent skip is how a stage comes to report a pass it did not
/// make.
/// </para>
/// <para>
/// <b>WHY A REAL SCALE FACTOR ALONE IS NOT ENOUGH, IN ONE LINE.</b> The copy in the slot is
/// <c>A sin(phi + theta)</c> for an unknown carrier phase; fitting one real gain solves to
/// <c>A cos(theta)</c> and leaves <c>A sin(theta) cos(phi)</c> behind, so it removes
/// <c>cos^2(theta)</c> of the energy — one half on average and <b>nothing at all at 90 degrees</b>,
/// with the gain reading zero while the whole transmission stays in the buffer. That is the shape of
/// the bug <c>Ft8DeepSubtractionTests.ASubtractedMessageNoLongerDecodesOutOfTheResidual</c> was
/// watched failing on before this fit existed.
/// </para>
/// </remarks>
public static class Ft8DeepMessageSubtractor
{
    /// <summary>
    /// The half-length of the FIR Hilbert transformer that builds the quadrature companion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Fifty, so the filter is 101 taps and about fifty of them are non-zero.</b> The ideal
    /// discrete Hilbert transformer is <c>h[k] = 2/(pi k)</c> for odd <c>k</c> and zero for even, so
    /// half the taps cost nothing. It is windowed rather than truncated.
    /// </para>
    /// <para>
    /// <b>THE ONLY ERROR A TYPE III FIR MAKES HERE IS AMPLITUDE.</b> Its antisymmetry gives it
    /// exactly 90 degrees of phase at every frequency by construction, so the quadrature is exact
    /// and only the magnitude ripples. The ripple is worst at DC and at Nyquist; a transmission
    /// occupies 50 Hz somewhere in the middle of a 200 Hz to 3000 Hz passband at a 6000 Hz Nyquist,
    /// which is nowhere near either end, and the residual amplitude error there bounds the
    /// achievable cancellation at about -40 dB. <b>That bound is the reason the decibels removed are
    /// reported rather than asserted against.</b>
    /// </para>
    /// </remarks>
    public const int HilbertHalfLength = 50;

    /// <summary>
    /// How many samples are summed into one block before the frequency search evaluates its sum.
    /// </summary>
    /// <remarks>
    /// <b>480 at 12 kHz, which is a 25 Hz decimated rate.</b> The frequency search evaluates
    /// <c>sum w[n] exp(j 2 pi d n / fs)</c> at fifty-one values of <c>d</c>; summing <c>w</c> in
    /// blocks first turns fifty-one sums of 151 680 terms into fifty-one sums of 316. The phase
    /// varies by <c>2 pi d B / fs</c> across a block, which at the ±0.5 Hz actually searched is
    /// 0.126 radians and costs <c>sinc(0.02) = 0.99934</c> of amplitude — three parts in ten
    /// thousand, against a cancellation floor of one part in a hundred.
    /// </remarks>
    public const int SearchBlockSamples = 480;

    /// <summary>
    /// How much finer the second frequency pass is than the first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>FORTY, AND THE NUMBER IS THE ONE THING IN THIS FILE THAT WAS MEASURED RATHER THAN
    /// DERIVED — because the derivation that was written down first was wrong.</b>
    /// <c>docs/unit253-subtraction.md</c> §2.5 predicted that a residual frequency error of half a
    /// 0.02 Hz step would cost "under 0.4 dB of cancellation". It costs far more than that, and the
    /// arithmetic is this: a frequency error <c>df</c> is not an amplitude error, it is a phase
    /// <em>ramp</em> of <c>2 pi df T</c> radians across the frame. The fit's constant phase absorbs
    /// the middle of the ramp and the ends are left, so the residual is the root-mean-square of a
    /// linear ramp of half-width <c>pi df T</c>, which is <c>pi df T / sqrt(3)</c>, and the
    /// cancellation floor is <c>-20 log10</c> of that.
    /// </para>
    /// <para>
    /// <b>At <c>T = 12.64</c> s that is 0.0004 Hz for -40 dB and 0.0044 Hz for -20 dB</b>, so a
    /// 0.02 Hz step is two orders of magnitude too coarse. The first run of
    /// <c>Ft8DeepSubtractionTests.AnOffGridTransmissionIsFoundAndRemovedByTheFitsOwnSearch</c>
    /// settled 0.0050 Hz from the truth and removed <b>18.89 dB</b>, which is what that formula
    /// predicts to a hundredth of a decibel, and the message decoded out of the residual. A second
    /// pass at a fortieth of the step - 0.0005 Hz, so a residual of 0.00025 Hz - puts the floor near
    /// -45 dB, below the other two limits in this file.
    /// </para>
    /// <para>
    /// <b>It costs nothing.</b> The block-summed evaluation makes a frequency trial about three
    /// hundred multiply-adds, so the fine pass is a few tens of thousands of operations against the
    /// several million the reference synthesis already spent.
    /// </para>
    /// </remarks>
    public const int FrequencyFineDivisor = 40;

    /// <summary>
    /// <b>Fits and subtracts one decoded message, in place, and says what it removed.</b>
    /// </summary>
    /// <param name="slot">
    /// The samples to subtract from. <b>Written in place</b>, so a caller that wants the original
    /// keeps its own copy — <c>Ft8DeepSlotDecoder</c> does exactly that and never writes a caller's
    /// buffer.
    /// </param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="symbols">
    /// The 79 channel symbols the message was carried on, as
    /// <c>Ft8DeepMessageSymbols.TryEncode</c> recovers them. <b>What was transmitted, guarded by
    /// that type's round trip.</b>
    /// </param>
    /// <param name="baseFrequencyHz">
    /// Where the lowest of the eight tones is thought to be — the coarse candidate's frequency,
    /// quantised to the waterfall's 3.125 Hz grid. <b>The fit refines it.</b>
    /// </param>
    /// <param name="startSeconds">
    /// When the first symbol is thought to have begun, in seconds from the start of the slot.
    /// <b>The start of the signal, not a candidate's nominal time</b>: a caller handing over
    /// <c>Ft8SlotMessage.TimeSeconds(geometry)</c> unbiased is a symbol early and
    /// <c>Ft8DeepSlotDecoder.CandidateTimeBiasSeconds</c> is the correction.
    /// </param>
    /// <param name="settings">How hard to look before subtracting, or null for the default.</param>
    /// <returns>What was fitted and removed, or <c>Ft8DeepSubtractionFit.NotFitted</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sample rate is not one a frame fits.</exception>
    public static Ft8DeepSubtractionFit Subtract(
        Span<float> slot,
        int sampleRate,
        ReadOnlySpan<byte> symbols,
        double baseFrequencyHz,
        double startSeconds,
        Ft8DeepSubtractionSettings? settings = null)
    {
        if (symbols.Length != Ft8SymbolEncoder.SymbolCount)
        {
            throw new ArgumentException(
                $"A transmission is {Ft8SymbolEncoder.SymbolCount} channel symbols and "
                + $"{symbols.Length} were given. There is nothing to subtract from part of a frame, "
                + "and nothing has been written to the slot.",
                nameof(symbols));
        }

        var used = settings ?? Ft8DeepSubtractionSettings.Default;

        // THE REFERENCE IS THE PORT'S OWN AND THE PORT DOES NOT MOVE. Unit amplitude, carrier
        // starting at zero phase, no padding: 79 * SamplesPerSymbol floats and nothing else.
        var reference = Ft8Waveform.Synthesize(symbols, sampleRate, (float)baseFrequencyHz);
        var companion = Hilbert(reference);

        var samplesPerSecond = (double)sampleRate;
        var start = (int)Math.Round(startSeconds * samplesPerSecond);

        var (from, to) = Overlap(start, reference.Length, slot.Length);
        var symbolSamples = Ft8Waveform.SamplesPerSymbol(sampleRate);
        var whole = (to - from) / symbolSamples;

        if (whole < Ft8DeepSubtractionSettings.MinimumSymbols)
        {
            // NOT A CLAMP AND NOT A PARTIAL SUBTRACTION. A fit taken over a fragment of a frame is
            // a different quantity, and writing its waveform into the buffer the next pass reads
            // would be inventing samples nobody transmitted.
            return Ft8DeepSubtractionFit.NotFitted;
        }

        var (offset, shiftHz) = Search(slot, reference, companion, start, sampleRate, used);
        var shifted = new double[reference.Length];
        var shiftedCompanion = new double[reference.Length];
        Rotate(reference, companion, shiftHz, sampleRate, shifted, shiftedCompanion);

        var (a, b, fitted) = Fit(slot, shifted, shiftedCompanion, offset);

        if (!fitted)
        {
            return Ft8DeepSubtractionFit.NotFitted;
        }

        var (before, after) = Remove(slot, shifted, shiftedCompanion, offset, a, b);

        return new Ft8DeepSubtractionFit(
            Math.Sqrt((a * a) + (b * b)),

            // THE SIGN. The Hilbert transform of sin is minus cos, so a reference sin(phi) and its
            // companion -cos(phi) combine as a sin(phi) - b cos(phi), which is A sin(phi + theta)
            // with a = A cos(theta) and b = -A sin(theta). Hence the negation.
            Math.Atan2(-b, a),
            offset / samplesPerSecond,
            baseFrequencyHz + shiftHz,
            after > 0.0 ? 10.0 * Math.Log10(before / after) : double.PositiveInfinity,
            whole,
            offset - start,
            shiftHz);
    }

    /// <summary>
    /// The range of the slot the reference actually overlaps, as a half-open sample interval.
    /// </summary>
    private static (int From, int To) Overlap(int start, int length, int slotLength) =>
        (Math.Max(0, start), Math.Min(slotLength, start + length));

    /// <summary>
    /// <b>The quadrature companion of the reference: its discrete Hilbert transform.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>h[k] = 2 / (pi k)</c> for odd <c>k</c> and zero for even, Blackman-windowed over
    /// <c>2 * HilbertHalfLength + 1</c> taps, applied <b>centred</b> rather than causally so the
    /// companion needs no delay compensation and stays sample-aligned with the reference. The DTFT
    /// of the ideal kernel is <c>-j</c> over the positive frequencies, so the transform of
    /// <c>sin(phi)</c> is <c>-cos(phi)</c> — which is the sign convention
    /// <see cref="Subtract"/> undoes when it reports a phase.
    /// </para>
    /// <para>
    /// <b>The first and last <see cref="HilbertHalfLength"/> samples are computed against zeros
    /// beyond the ends of the reference</b>, so the companion is slightly wrong there. That is 100
    /// samples of 151 680, and the reference is already ramped to zero over the first and last
    /// eighth of a symbol — 240 samples — by <c>Ft8Waveform.Synthesize</c>'s own raised cosine, so
    /// the region where the error lives is the region where there is almost nothing to be wrong
    /// about.
    /// </para>
    /// </remarks>
    private static double[] Hilbert(float[] reference)
    {
        var taps = new double[HilbertHalfLength + 1];
        var window = (2 * HilbertHalfLength) + 1;

        for (var k = 1; k <= HilbertHalfLength; k += 2)
        {
            // Blackman, evaluated at the tap's position in the full window rather than at k, so the
            // two halves of the antisymmetric kernel are weighted identically.
            var t = (double)(HilbertHalfLength + k) / (window - 1);
            var blackman = 0.42
                - (0.5 * Math.Cos(2.0 * Math.PI * t))
                + (0.08 * Math.Cos(4.0 * Math.PI * t));

            taps[k] = 2.0 / (Math.PI * k) * blackman;
        }

        var companion = new double[reference.Length];

        for (var n = 0; n < reference.Length; n++)
        {
            var sum = 0.0;
            for (var k = 1; k <= HilbertHalfLength; k += 2)
            {
                var back = n - k;
                var forward = n + k;
                var left = back >= 0 ? reference[back] : 0.0;
                var right = forward < reference.Length ? reference[forward] : 0.0;

                // h[-k] = -h[k]: the kernel is odd, which is what makes the phase exactly 90
                // degrees rather than approximately so.
                sum += taps[k] * (left - right);
            }

            companion[n] = sum;
        }

        return companion;
    }

    /// <summary>
    /// The reference and its companion, rotated by <paramref name="shiftHz"/>.
    /// </summary>
    /// <remarks>
    /// A frequency shift of a real signal is a rotation of its analytic form, so shifting costs a
    /// sine and a cosine per sample rather than a re-synthesis. With <c>D[n] = 2 pi d n / fs</c>,
    /// <c>x_d = x cos D - y sin D</c> and <c>y_d = x sin D + y cos D</c>.
    /// </remarks>
    private static void Rotate(
        float[] reference,
        double[] companion,
        double shiftHz,
        int sampleRate,
        Span<double> shifted,
        Span<double> shiftedCompanion)
    {
        var step = 2.0 * Math.PI * shiftHz / sampleRate;

        for (var n = 0; n < reference.Length; n++)
        {
            var angle = step * n;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            shifted[n] = (reference[n] * cos) - (companion[n] * sin);
            shiftedCompanion[n] = (reference[n] * sin) + (companion[n] * cos);
        }
    }

    /// <summary>
    /// <b>Where the transmission actually is: the sample offset and the frequency shift that leave
    /// the most energy in the fit.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A coordinate search and not a grid</b>, for the same reason
    /// <c>Ft8DeepSignalToNoise.Estimate</c> uses one: the two axes are close enough to separable
    /// over this extent, and a product of them would cost twenty-five times as much for a place that
    /// is already inside the basin.
    /// </para>
    /// <para>
    /// <b>Frequency first, because it is the one that matters and the one that is nearly free.</b>
    /// The correlation at every trial shift is one Fourier evaluation of <c>w[n] = r[n] x[n] + j r[n]
    /// y[n]</c>, and <c>w</c> is summed into blocks of <see cref="SearchBlockSamples"/> before the
    /// evaluation — see that constant. Time second, at the frequency found, one dot product a
    /// candidate offset.
    /// </para>
    /// <para>
    /// <b>What is maximised is the removed energy</b>, which for a reference whose two basis vectors
    /// are very nearly orthogonal and of equal norm is <c>|C|^2 / E</c>. The exact 2x2 is solved
    /// once, afterwards, at the place this returns.
    /// </para>
    /// </remarks>
    private static (int Offset, double ShiftHz) Search(
        ReadOnlySpan<float> slot,
        float[] reference,
        double[] companion,
        int start,
        int sampleRate,
        Ft8DeepSubtractionSettings settings)
    {
        var bestShift = 0.0;

        if (settings.FrequencySearchHz > 0.0)
        {
            bestShift = BestShift(
                Blocks(slot, reference, companion, start),
                0.0,
                settings.FrequencySearchHz,
                settings.FrequencyStepHz,
                sampleRate);
        }

        var offset = start;

        if (settings.TimeSearchSamples > 0)
        {
            var shifted = new double[reference.Length];
            var shiftedCompanion = new double[reference.Length];
            Rotate(reference, companion, bestShift, sampleRate, shifted, shiftedCompanion);

            var bestScore = double.NegativeInfinity;
            var coarse = Math.Max(1, settings.TimeSearchSamples / 4);

            // Coarse then fine on the same axis: a stride of a quarter of the extent, then every
            // sample within one stride of what that found.
            for (var pass = 0; pass < 2; pass++)
            {
                var stride = pass == 0 ? coarse : 1;
                var extent = pass == 0 ? settings.TimeSearchSamples : coarse;
                var centre = offset;

                for (var at = centre - extent; at <= centre + extent; at += stride)
                {
                    var score = OffsetScore(slot, shifted, shiftedCompanion, at);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        offset = at;
                    }
                }
            }
        }

        if (settings.FrequencySearchHz > 0.0)
        {
            // THE FINE FREQUENCY PASS, AT THE OFFSET THE TIME SEARCH FOUND. See
            // FrequencyFineDivisor: a residual frequency error is a phase ramp across the frame and
            // not an amplitude error, and half a coarse step of it caps cancellation at about 19 dB.
            var fineStep = settings.FrequencyStepHz / FrequencyFineDivisor;

            bestShift = BestShift(
                Blocks(slot, reference, companion, offset),
                bestShift,
                settings.FrequencyStepHz * 2.0,
                fineStep,
                sampleRate);
        }

        return (offset, bestShift);
    }

    /// <summary>The frequency shift about a centre with the most correlated energy at it.</summary>
    private static double BestShift(
        (double Real, double Imaginary, double Centre)[] blocks,
        double centre,
        double extent,
        double step,
        int sampleRate)
    {
        var best = centre;
        var bestScore = double.NegativeInfinity;

        for (var offset = -extent; offset <= extent + (step / 2.0); offset += step)
        {
            var at = centre + offset;
            var score = BlockScore(blocks, at, sampleRate);

            if (score > bestScore)
            {
                bestScore = score;
                best = at;
            }
        }

        return best;
    }

    /// <summary>
    /// <c>w[n] = r[n] x[n] + j r[n] y[n]</c>, summed into blocks, with each block's centre sample.
    /// </summary>
    private static (double Real, double Imaginary, double Centre)[] Blocks(
        ReadOnlySpan<float> slot,
        float[] reference,
        double[] companion,
        int start)
    {
        var count = (reference.Length + SearchBlockSamples - 1) / SearchBlockSamples;
        var blocks = new (double Real, double Imaginary, double Centre)[count];

        for (var block = 0; block < count; block++)
        {
            var from = block * SearchBlockSamples;
            var to = Math.Min(reference.Length, from + SearchBlockSamples);
            var real = 0.0;
            var imaginary = 0.0;

            for (var n = from; n < to; n++)
            {
                var at = start + n;
                if (at < 0 || at >= slot.Length)
                {
                    continue;
                }

                var sample = slot[at];
                real += sample * reference[n];
                imaginary += sample * companion[n];
            }

            blocks[block] = (real, imaginary, (from + to - 1) / 2.0);
        }

        return blocks;
    }

    /// <summary>The squared magnitude of the block-summed correlation at one frequency shift.</summary>
    private static double BlockScore(
        (double Real, double Imaginary, double Centre)[] blocks, double shiftHz, int sampleRate)
    {
        var step = 2.0 * Math.PI * shiftHz / sampleRate;
        var real = 0.0;
        var imaginary = 0.0;

        foreach (var (blockReal, blockImaginary, centre) in blocks)
        {
            var angle = step * centre;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            real += (blockReal * cos) - (blockImaginary * sin);
            imaginary += (blockReal * sin) + (blockImaginary * cos);
        }

        return (real * real) + (imaginary * imaginary);
    }

    /// <summary>The squared magnitude of the correlation at one sample offset.</summary>
    private static double OffsetScore(
        ReadOnlySpan<float> slot, double[] shifted, double[] shiftedCompanion, int offset)
    {
        var real = 0.0;
        var imaginary = 0.0;

        for (var n = 0; n < shifted.Length; n++)
        {
            var at = offset + n;
            if (at < 0 || at >= slot.Length)
            {
                continue;
            }

            var sample = slot[at];
            real += sample * shifted[n];
            imaginary += sample * shiftedCompanion[n];
        }

        return (real * real) + (imaginary * imaginary);
    }

    /// <summary>
    /// <b>The two-coefficient least-squares fit, solved as a 2x2 and not assumed diagonal.</b>
    /// </summary>
    /// <remarks>
    /// Minimises <c>sum (r[offset + n] - a x[n] - b y[n])^2</c> over the samples the reference
    /// overlaps. The normal equations are
    /// <c>[Sxx Sxy; Sxy Syy] [a; b] = [Sxr; Syr]</c>. <c>Sxy</c> is small — a Hilbert pair of a
    /// wideband-in-its-own-terms FM waveform is very nearly orthogonal — but it is computed and used
    /// rather than assumed zero, because assuming it is exactly the kind of nearly-true that shows
    /// up later as a floor on the cancellation that nobody can localise.
    /// </remarks>
    private static (double A, double B, bool Fitted) Fit(
        ReadOnlySpan<float> slot, double[] shifted, double[] shiftedCompanion, int offset)
    {
        var sxx = 0.0;
        var syy = 0.0;
        var sxy = 0.0;
        var sxr = 0.0;
        var syr = 0.0;

        for (var n = 0; n < shifted.Length; n++)
        {
            var at = offset + n;
            if (at < 0 || at >= slot.Length)
            {
                continue;
            }

            var x = shifted[n];
            var y = shiftedCompanion[n];
            var r = slot[at];

            sxx += x * x;
            syy += y * y;
            sxy += x * y;
            sxr += r * x;
            syr += r * y;
        }

        var determinant = (sxx * syy) - (sxy * sxy);

        if (!(determinant > 0.0) || double.IsNaN(determinant))
        {
            // NO FIT RATHER THAN A DIVISION. Two basis vectors that are not independent over the
            // overlap - which can only happen if the overlap is empty or degenerate - do not have a
            // least-squares solution, and inventing one would write a waveform nobody measured.
            return (double.NaN, double.NaN, false);
        }

        return (
            ((syy * sxr) - (sxy * syr)) / determinant,
            ((sxx * syr) - (sxy * sxr)) / determinant,
            true);
    }

    /// <summary>
    /// Subtracts the fitted waveform in place and reports the energy before and after over the
    /// samples it touched.
    /// </summary>
    private static (double Before, double After) Remove(
        Span<float> slot, double[] shifted, double[] shiftedCompanion, int offset, double a, double b)
    {
        var before = 0.0;
        var after = 0.0;

        for (var n = 0; n < shifted.Length; n++)
        {
            var at = offset + n;
            if (at < 0 || at >= slot.Length)
            {
                continue;
            }

            var was = slot[at];
            before += was * was;

            var now = (float)(was - ((a * shifted[n]) + (b * shiftedCompanion[n])));
            slot[at] = now;
            after += (double)now * now;
        }

        return (before, after);
    }
}
