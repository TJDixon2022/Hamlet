using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 222's substitution apparatus: the receive path with one stage at a time replaced by a
/// version that cannot be blamed.</b> Every line of it is in the test project and nothing here is a
/// candidate for adoption into the library.
/// </summary>
/// <remarks>
/// <para>
/// <b>The rule this file exists under, and it is the rule the night turns on.</b> A substitution that
/// decodes better is <em>evidence about where the loss is</em>. It is not a licence to adopt it, at
/// any size. Upstream's byte-quantised waterfall and its 25-iteration bound are its own choices, and
/// the plan's ruling that inheriting Goba's bugs is accepted is what forbids trading either of them
/// for decodes.
/// </para>
/// <para>
/// <b>Exactly one stage moves per row.</b> Where a row keeps the search, it keeps the search's own
/// candidate list off the byte waterfall the library actually builds — not a list re-derived from
/// the unquantised store, which would move two stages at once and make the row unreadable.
/// </para>
/// </remarks>
internal static class Unit222Budget
{
    /// <summary>What one substituted path returned for one slot.</summary>
    internal sealed record Trial(bool Returned, int Parity, int Checksum, int Text, string[] Wrong);

    /// <summary>
    /// <b>The receive path from a candidate list down, with the ratios supplied by the caller.</b>
    /// This is <c>Ft8SlotDecoder.Decode</c>'s own loop — the same gate, the same de-duplication key,
    /// the same message limit — with the one call that reads the waterfall replaced by a delegate.
    /// </summary>
    /// <remarks>
    /// <b>Written out rather than reached into.</b> <c>Ft8SlotDecoder</c> has no seam for a ratio
    /// source and adding one would be a library change made to serve a measurement, which is exactly
    /// what task 5's two conditions exist to prevent. So the loop is transcribed here, in the test
    /// project, and the library is left alone.
    /// </remarks>
    internal static Trial Run(
        IReadOnlyList<Ft8Candidate> candidates,
        Action<Ft8Candidate, float[]> fillRatios,
        int maxIterations,
        string expected)
    {
        var cache = new Ft8CallsignCache();
        var seen = new List<byte[]>();
        var texts = new List<string>();

        var parity = 0;
        var checksum = 0;
        var text = 0;

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var candidate in candidates)
        {
            fillRatios(candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios, cache, maxIterations);

            if (result.Status != Ft8CodewordStatus.ParityNeverSatisfied)
            {
                parity++;
            }

            if (result.Status is Ft8CodewordStatus.Decoded or Ft8CodewordStatus.MessageNotReadable)
            {
                checksum++;
            }

            if (result.Status != Ft8CodewordStatus.Decoded)
            {
                continue;
            }

            text++;

            LdpcDecoder.Decode(ratios, codeword, maxIterations);
            var key = codeword[..Ft8Payload.MessageBits];

            var already = false;
            foreach (var previous in seen)
            {
                if (key.AsSpan().SequenceEqual(previous))
                {
                    already = true;
                    break;
                }
            }

            if (already || texts.Count >= Ft8SlotDecoder.DefaultMessageLimit)
            {
                continue;
            }

            seen.Add(key);
            texts.Add(result.Message.Text);
        }

        return new Trial(
            texts.Contains(expected, StringComparer.Ordinal),
            parity,
            checksum,
            text,
            texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)).ToArray());
    }

    /// <summary>
    /// <b>The same spectrogram the library builds, kept in full precision.</b> Upstream's window,
    /// upstream's sliding frame, upstream's axis order — and the magnitude recorded both as the
    /// decibels that <em>would have been</em> truncated into a byte and as the linear power they came
    /// from.
    /// </summary>
    /// <remarks>
    /// <b>Faithful to <c>Ft8Monitor</c> in everything except the store.</b> The window coefficients
    /// carry the same 2/n normalisation folded in, the frame is zeroed at the start and slides by a
    /// sub-block, the imaginary part is squared first, and the decibels are formed through upstream's
    /// own <c>1e-12 +</c> floor and cast to single precision at the same point. What it does not do is
    /// call <see cref="Ft8Waterfall.StoredFor"/>. <b>That one difference is row C.</b>
    /// </remarks>
    internal sealed class Unquantised
    {
        private Unquantised(Ft8WaterfallGeometry geometry, double[] decibels, double[] power, int blocks)
        {
            Geometry = geometry;
            Decibels = decibels;
            Power = power;
            BlockCount = blocks;
        }

        internal Ft8WaterfallGeometry Geometry { get; }

        /// <summary>Decibels before the byte, in <see cref="Ft8Waterfall"/>'s own layout.</summary>
        internal double[] Decibels { get; }

        /// <summary>The linear power the decibels came from, same layout.</summary>
        internal double[] Power { get; }

        internal int BlockCount { get; }

        internal int IndexOf(int block, int timeSub, int freqSub, int bin) =>
            (block * Geometry.BlockStride)
            + (((timeSub * Geometry.FrequencyOversampling) + freqSub) * Geometry.BinCount)
            + bin;

        /// <summary>Analyses a slot, keeping every magnitude at full precision.</summary>
        internal static Unquantised Analyse(ReadOnlySpan<float> samples, Ft8WaterfallGeometry geometry)
        {
            var n = geometry.TransformLength;
            var advance = geometry.SubblockSize;
            var transform = new Ft8RealFft(n);

            var window = new double[n];
            var normalisation = 2.0f / n;
            for (var i = 0; i < n; i++)
            {
                window[i] = normalisation * Ft8Monitor.HannSquaredSine(i, n);
            }

            var frame = new double[n];
            var windowed = new double[n];
            var real = new double[transform.BinCount];
            var imaginary = new double[transform.BinCount];

            var decibels = new double[geometry.MagnitudeCount];
            var power = new double[geometry.MagnitudeCount];

            var blocks = 0;
            var offset = 0;

            for (var position = 0;
                position + geometry.BlockSize <= samples.Length && blocks < geometry.MaxBlocks;
                position += geometry.BlockSize)
            {
                var framePosition = position;

                for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
                {
                    Array.Copy(frame, advance, frame, 0, n - advance);
                    for (var i = n - advance; i < n; i++)
                    {
                        frame[i] = samples[framePosition];
                        framePosition++;
                    }

                    for (var i = 0; i < n; i++)
                    {
                        windowed[i] = window[i] * frame[i];
                    }

                    transform.Transform(windowed, real, imaginary);

                    for (var freqSub = 0; freqSub < geometry.FrequencyOversampling; freqSub++)
                    {
                        for (var bin = geometry.MinBin; bin < geometry.MaxBin; bin++)
                        {
                            var source = (bin * geometry.FrequencyOversampling) + freqSub;
                            var re = real[source];
                            var im = imaginary[source];
                            var p = (im * im) + (re * re);

                            power[offset] = p;
                            decibels[offset] = (float)(10.0 * Math.Log10(1e-12 + p));
                            offset++;
                        }
                    }
                }

                blocks++;
            }

            return new Unquantised(geometry, decibels, power, blocks);
        }

        /// <summary>The mean linear power over every bin of every block: the noise floor of a slot.</summary>
        internal double MeanPower()
        {
            var total = 0.0;
            var count = BlockCount * Geometry.BlockStride;
            for (var i = 0; i < count; i++)
            {
                total += Power[i];
            }

            return total / count;
        }

        /// <summary>
        /// <b>Row C's ratios: upstream's own extraction rule, on magnitudes that were never
        /// quantised.</b> Same forward Gray map, same value ordering, same largest-of-four-ones less
        /// largest-of-four-zeros, same three zeros for a block off the end.
        /// </summary>
        internal void ExtractUnquantised(Ft8Candidate candidate, Span<float> ratios)
        {
            Span<double> magnitudes = stackalloc double[Ft8SymbolEncoder.ToneCount];
            var gray = Ft8Tables.Ft8GrayMap;
            var bit = 0;

            for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
            {
                if (Ft8SymbolEncoder.IsSyncSymbol(symbol))
                {
                    continue;
                }

                var block = candidate.BlockOffset + symbol;
                if (block < 0 || block >= BlockCount)
                {
                    ratios[bit] = 0.0f;
                    ratios[bit + 1] = 0.0f;
                    ratios[bit + 2] = 0.0f;
                    bit += Ft8SymbolEncoder.BitsPerSymbol;
                    continue;
                }

                for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
                {
                    magnitudes[value] = Decibels[IndexOf(
                        block,
                        candidate.TimeSubOffset,
                        candidate.FrequencySubOffset,
                        candidate.BinOffset + gray[value])];
                }

                Ft8SoftSymbols.ExtractSymbol(magnitudes, ratios.Slice(bit, Ft8SymbolEncoder.BitsPerSymbol));
                bit += Ft8SymbolEncoder.BitsPerSymbol;
            }
        }

        /// <summary>
        /// <b>Row D's ratios: a second opinion formed from the physics rather than from upstream's
        /// shortcut.</b> The linear tone powers, divided by the noise power the fixture knows it
        /// added, combined by the log-sum-exp a noncoherent eight-tone detector actually wants.
        /// </summary>
        /// <param name="candidate">Where to read.</param>
        /// <param name="noisePower">The mean per-bin noise power of this slot, from the fixture.</param>
        /// <param name="ratios">The 174 ratios.</param>
        /// <remarks>
        /// <para>
        /// <b>Why this and not another max-of-something.</b> Upstream forms each ratio as the largest
        /// decibel magnitude among the four values whose bit is one, less the largest among the four
        /// whose bit is zero. That is a max-log approximation taken in the <em>logarithmic</em>
        /// domain, which is not the same approximation as a max-log in the likelihood domain, and it
        /// throws away three of the four candidates on each side. The textbook metric for a
        /// square-law detector on eight orthogonal tones in known noise is
        /// <c>log sum exp(P_v / N0)</c> over each half of the partition, and that is what this
        /// computes. <b>Nothing about it is proposed for the library</b>; it exists to say what the
        /// shortcut costs.
        /// </para>
        /// <para>
        /// <b>The one number it is allowed to know that the decoder is not</b> is the noise power,
        /// which is the fixture's own and is the point of the row: it isolates the ratio arithmetic
        /// from the problem of estimating a noise floor.
        /// </para>
        /// </remarks>
        internal void ExtractByPhysics(Ft8Candidate candidate, double noisePower, Span<float> ratios)
        {
            Span<double> scaled = stackalloc double[Ft8SymbolEncoder.ToneCount];
            var gray = Ft8Tables.Ft8GrayMap;
            var bit = 0;

            for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
            {
                if (Ft8SymbolEncoder.IsSyncSymbol(symbol))
                {
                    continue;
                }

                var block = candidate.BlockOffset + symbol;
                if (block < 0 || block >= BlockCount)
                {
                    ratios[bit] = 0.0f;
                    ratios[bit + 1] = 0.0f;
                    ratios[bit + 2] = 0.0f;
                    bit += Ft8SymbolEncoder.BitsPerSymbol;
                    continue;
                }

                for (var value = 0; value < Ft8SymbolEncoder.ToneCount; value++)
                {
                    scaled[value] = Power[IndexOf(
                        block,
                        candidate.TimeSubOffset,
                        candidate.FrequencySubOffset,
                        candidate.BinOffset + gray[value])] / noisePower;
                }

                for (var b = 0; b < Ft8SymbolEncoder.BitsPerSymbol; b++)
                {
                    var shift = Ft8SymbolEncoder.BitsPerSymbol - 1 - b;
                    ratios[bit + b] = (float)(
                        LogSumExp(scaled, shift, 1) - LogSumExp(scaled, shift, 0));
                }

                bit += Ft8SymbolEncoder.BitsPerSymbol;
            }
        }

        /// <summary>
        /// <c>log sum exp</c> over the four values whose bit at <paramref name="shift"/> is
        /// <paramref name="wanted"/>, shifted by the largest term so it cannot overflow.
        /// </summary>
        private static double LogSumExp(ReadOnlySpan<double> scaled, int shift, int wanted)
        {
            var largest = double.NegativeInfinity;
            for (var value = 0; value < scaled.Length; value++)
            {
                if (((value >> shift) & 1) == wanted && scaled[value] > largest)
                {
                    largest = scaled[value];
                }
            }

            var sum = 0.0;
            for (var value = 0; value < scaled.Length; value++)
            {
                if (((value >> shift) & 1) == wanted)
                {
                    sum += Math.Exp(scaled[value] - largest);
                }
            }

            return largest + Math.Log(sum);
        }
    }

    /// <summary>
    /// <b>Unit 221's curve, transcribed so a rate can be quoted as an equivalent shift in
    /// decibels.</b> The rungs either side of the collapse and nothing more; it is read by
    /// interpolation and never re-run here.
    /// </summary>
    private static readonly (double Decibels, double Rate)[] Curve =
    {
        (-17.0, 100.0), (-18.0, 99.3), (-19.0, 81.0), (-20.0, 23.9), (-21.0, 4.2), (-22.0, 0.0),
    };

    /// <summary>
    /// The ratio at which unit 221's as-is curve reaches <paramref name="rate"/>, and therefore how
    /// many decibels a row is worth. <b>Positive means the substitution bought sensitivity.</b>
    /// </summary>
    internal static string EquivalentShift(double rate)
    {
        if (rate <= 0.0)
        {
            return "below the curve";
        }

        if (rate >= Curve[0].Rate)
        {
            return ">= 4.0 dB";
        }

        for (var i = 0; i < Curve.Length - 1; i++)
        {
            var (highDb, highRate) = Curve[i];
            var (lowDb, lowRate) = Curve[i + 1];
            if (rate <= highRate && rate >= lowRate)
            {
                var fraction = (rate - lowRate) / (highRate - lowRate);
                var decibels = lowDb + (fraction * (highDb - lowDb));
                return $"{decibels - Unit222TraceTests.VerdictRungDecibels:+0.00;-0.00;0.00} dB";
            }
        }

        return "off the curve";
    }
}
