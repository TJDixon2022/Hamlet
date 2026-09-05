using System;
using System.Collections.Generic;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>Two or more hearings of the same transmission, added together before anything decodes them.</b>
/// Normalise each, add, re-normalise, and hand the result to the port's gate exactly as if it had come
/// out of one slot.
/// </summary>
/// <remarks>
/// <para>
/// <b>The arithmetic is standard soft-decision theory and comes from nobody's source.</b> Two
/// independent observations of the same transmitted bit carry log-likelihood ratios that add: if
/// <c>L1</c> and <c>L2</c> are the log-odds the bit is one given each observation, the log-odds given
/// both is <c>L1 + L2</c>, because the observations are conditionally independent given the bit and
/// the log of a product is a sum. That is the whole of the 3 dB two repeats are worth, and it is
/// textbook — no WSJT-X source and no <c>ft4_ft8_public/</c> was read for it, and <c>ft8_lib</c>, which
/// <c>Ft8Sharp</c> ports, has nothing of the kind to port.
/// </para>
/// <para>
/// <b>What is cited is the frame these ratios belong to</b>: 174 bits in codeword order carrying a
/// 77-bit payload and a CRC-14, from S. Franke K9AN, B. Somerville G4WJS and J. Taylor K1JT, <em>The
/// FT4 and FT8 Communication Protocols</em>, QEX, July/August 2020. The combination is only meaningful
/// because position <c>i</c> means the same codeword bit in every hearing, and that is the protocol's
/// fact rather than this library's. See <c>src/Ft8Sharp.Deep/porting-notes.md</c>.
/// </para>
/// <para>
/// <b>EACH INPUT IS NORMALISED, THEN SUMMED, THEN THE SUM IS NORMALISED.</b>
/// <see cref="Ft8SoftSymbols.Normalise"/> records that the port's belief propagation is <em>not
/// scale-free</em> — <c>fast_tanh</c> is a rational approximation with a hard clamp — so a summed
/// vector left sitting at twice upstream's scale is a different experiment rather than a better one.
/// Normalising the inputs first is what makes the weighting mean something: without it, whichever
/// hearing happened to have the larger raw magnitudes would dominate whatever weight it was given.
/// </para>
/// <para>
/// <b>The weighting was chosen by measurement, not by argument, and the measurement did not separate
/// the two.</b> Unit 247 task 1 took the closest candidate in each of two independent hearings over
/// one whole 51-trial block at -21 dB and combined them both ways, against the codeword the ladder
/// knows it transmitted:
/// </para>
/// <code>
/// weighting                          median distance   at or below 17 (the recovery threshold)
/// neither - slot A alone                          31                              2 of 51
/// neither - slot B alone                          31                              0 of 51
/// Equal                                           18                             23 of 51
/// ByPreNormalisationVariance                      18                             24 of 51
/// </code>
/// <para>
/// <b>One trial of 51 is not a difference, and the report says so rather than reading it as one.</b>
/// <see cref="Ft8DeepCombineWeighting.Equal"/> is therefore the default, on the ground that the ladder
/// delivers both hearings within a few hundredths of a decibel of each other — which is exactly the
/// condition under which equal weight is the optimal one — and that the simpler rule is the one that
/// cannot go wrong. Variance weighting exists for the fading case the ladder does not present, and
/// <see cref="Ft8DeepCombineWeighting.ByPreNormalisationVariance"/> says why the proxy it uses is a
/// proxy. <b>Nothing here was tuned to a target.</b>
/// </para>
/// <para>
/// <b>Nothing here decides that a message is real.</b> This type produces a ratio vector. Whether it
/// is a message is <c>Ft8CodewordDecoder</c>'s parity gate and CRC-14 gate and nothing else, and there
/// is no checksum anywhere in this library.
/// </para>
/// <para>
/// <b>It never throws on degenerate input</b> — all zero, all equal, infinite, not-a-number — because
/// it is called on noise, on candidates that are nothing but noise, and on pairings that are wrong.
/// A ratio that is not finite is not evidence and is read as <em>no opinion</em>, which is upstream's
/// own rule for a symbol whose block fell outside the waterfall.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
public static class Ft8DeepSoftCombiner
{
    /// <summary>How many log-likelihood ratios one transmission carries. The port's number.</summary>
    public const int RatioCount = LdpcDecoder.RatioCount;

    /// <summary>
    /// Combines several hearings of one transmission into one ratio vector.
    /// </summary>
    /// <param name="hearings">
    /// Two or more sets of <see cref="RatioCount"/> ratios in the port's convention — <b>positive
    /// means the bit is more likely one</b> — each read from a different slot at the same codeword
    /// position. <b>They are not modified.</b> One hearing is allowed and is a copy through the
    /// normalisation, which is what makes a combined path safe to run with nothing to combine yet.
    /// </param>
    /// <param name="weighting">How much each hearing counts for.</param>
    /// <param name="combined">
    /// <see cref="RatioCount"/> ratios out, normalised to <see cref="Ft8SoftSymbols.NormalisedVariance"/>
    /// and ready to hand to <c>Ft8CodewordDecoder.Decode</c>.
    /// </param>
    /// <returns>
    /// <b>The variance the sum carried before it was re-normalised</b>, which is the number that says
    /// whether the hearings agreed. Each input arrives at variance
    /// <see cref="Ft8SoftSymbols.NormalisedVariance"/>; two independent hearings sum to about twice
    /// it, two that agree on every bit to about four times it, and two of the same noise to about
    /// twice it as well. <b>Reported rather than believed</b> — every measurement this unit takes
    /// prints it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="hearings"/> is null, or one of them is.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// There are no hearings, or one of them is not <see cref="RatioCount"/> long, or
    /// <paramref name="combined"/> is not.
    /// </exception>
    public static float Combine(
        IReadOnlyList<float[]> hearings,
        Ft8DeepCombineWeighting weighting,
        Span<float> combined)
    {
        ArgumentNullException.ThrowIfNull(hearings);

        if (combined.Length != RatioCount)
        {
            throw new ArgumentException(
                $"A combination is {RatioCount} ratios and a span of {combined.Length} was given. "
                + "Refused rather than filled as far as it goes: a short span would hand the port's "
                + "gate a codeword whose tail is whatever was in the buffer.",
                nameof(combined));
        }

        if (hearings.Count == 0)
        {
            throw new ArgumentException(
                "There is nothing to combine. A caller with no hearings has a pairing fault, and "
                + "returning a vector of zeros would put 174 bits of no opinion in front of the "
                + "port's gate and let it read as a decode that failed rather than as a call that "
                + "should not have been made.",
                nameof(hearings));
        }

        combined.Clear();

        var scratch = new float[RatioCount];
        var weights = new double[hearings.Count];
        var totalWeight = 0.0;

        for (var h = 0; h < hearings.Count; h++)
        {
            var hearing = hearings[h];
            ArgumentNullException.ThrowIfNull(hearing, nameof(hearings));

            if (hearing.Length != RatioCount)
            {
                throw new ArgumentException(
                    $"Hearing {h} carries {hearing.Length} ratios and a transmission carries "
                    + $"{RatioCount}. Two hearings can only be added position by position when both "
                    + "positions mean the same codeword bit.",
                    nameof(hearings));
            }

            // A ratio that is not finite is not evidence. Read as no opinion, which is upstream's own
            // rule for a symbol whose block fell outside the waterfall, so a degenerate input costs
            // the combination nothing rather than poisoning all 174 positions through the variance.
            for (var i = 0; i < RatioCount; i++)
            {
                var value = hearing[i];
                scratch[i] = float.IsFinite(value) ? value : 0.0f;
            }

            // THE PORT'S OWN NORMALISATION, ON A COPY. The caller's array is left as it was: a
            // combiner that rescaled its inputs in place would change what the single-slot path
            // afterwards saw, and the whole value of this stage is that it only ever adds.
            var variance = Ft8SoftSymbols.Normalise(scratch);

            var weight = weighting switch
            {
                Ft8DeepCombineWeighting.ByPreNormalisationVariance =>
                    variance > 0.0f && float.IsFinite(variance) ? variance : 0.0,
                _ => 1.0,
            };

            weights[h] = weight;
            totalWeight += weight;

            for (var i = 0; i < RatioCount; i++)
            {
                combined[i] += (float)(weight * scratch[i]);
            }
        }

        // Every weight zero means every hearing was flat - all ratios identical, which
        // Ft8SoftSymbols.Normalise leaves untouched and reports a zero variance for. Fall back to
        // equal weight rather than returning 174 zeros, so the result is still the hard decision the
        // inputs agreed on instead of silently becoming no opinion everywhere.
        if (totalWeight <= 0.0)
        {
            combined.Clear();
            for (var h = 0; h < hearings.Count; h++)
            {
                var hearing = hearings[h];
                for (var i = 0; i < RatioCount; i++)
                {
                    var value = hearing[i];
                    combined[i] += float.IsFinite(value) ? value : 0.0f;
                }
            }
        }

        var summedVariance = Ft8SoftSymbols.Variance(combined);
        Ft8SoftSymbols.Normalise(combined);
        return summedVariance;
    }

    /// <summary>
    /// Combines two hearings. The case step 6 is about, without a list allocation at the call site.
    /// </summary>
    /// <param name="first">The earlier slot's ratios. Not modified.</param>
    /// <param name="second">The later slot's ratios. Not modified.</param>
    /// <param name="weighting">How much each hearing counts for.</param>
    /// <param name="combined">The combination, normalised and ready for the port's gate.</param>
    /// <returns>The variance of the sum before it was re-normalised.</returns>
    /// <remarks>
    /// <b>An array literal rather than a collection expression, deliberately.</b> A collection
    /// expression targeting <c>IReadOnlyList&lt;T&gt;</c> makes the compiler emit a synthesised
    /// <c>&lt;&gt;z__ReadOnlyArray</c> type into this assembly, and
    /// <c>Ft8DeepSlotDecoderTests.TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole</c>
    /// asserts the sibling's type list whole. A plain array implements the interface with no extra
    /// type, and the tripwire stays a statement about this library rather than about the compiler.
    /// </remarks>
    public static float Combine(
        float[] first,
        float[] second,
        Ft8DeepCombineWeighting weighting,
        Span<float> combined) =>
        Combine(new[] { first, second }, weighting, combined);
}
