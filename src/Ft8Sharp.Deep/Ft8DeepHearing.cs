using Ft8Sharp.Dsp;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>One candidate's normalised log-likelihood ratios, kept so that a later slot can be added to
/// them.</b> Where the transmission appeared to be, and what was heard there.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the whole of what a slot has to remember for step 6.</b> Not the audio, not the
/// waterfall, not the samples: 174 floats and four small integers per candidate. A slot at the port's
/// candidate limit of 140 is about 97 kilobytes, so a history of eight slots is under a megabyte, and
/// that is the memory cost of combining stated rather than estimated.
/// </para>
/// <para>
/// <b>The ratios are the port's own, at the port's own scale</b> — <c>Ft8SoftSymbols.Extract</c>
/// followed by <c>Ft8SoftSymbols.Normalise</c>, which is exactly what <c>Ft8SlotDecoder.Decode</c>
/// hands its gate. Nothing has been re-scaled, re-weighted or re-interpreted on the way in.
/// </para>
/// </remarks>
/// <param name="Candidate">
/// Where the search said the transmission was. <b>The pairing rule's only input</b> — its
/// <c>FrequencyHz</c> and <c>TimeSeconds</c> against a geometry are what decide whether two hearings
/// are the same station.
/// </param>
/// <param name="Ratios">
/// 174 normalised ratios in codeword bit order, positive meaning the bit is more likely one. <b>Owned
/// by this record</b>: the decoder copies rather than handing out the buffer it re-uses per candidate.
/// </param>
public readonly record struct Ft8DeepHearing(Ft8Candidate Candidate, float[] Ratios);
