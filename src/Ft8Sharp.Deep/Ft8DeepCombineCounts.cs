namespace Ft8Sharp.Deep;

/// <summary>
/// <b>What the combining stage did in one slot, beside the five counts the port returns and the four
/// the ordered statistics stage returns.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>These exist because a rate that moved with no visible combining activity behind it is not
/// evidence.</b> A scoreboard column that gained decodes while <see cref="Accepted"/> stayed at zero
/// would be measuring something other than what it claims to.
/// </para>
/// <para>
/// <b><see cref="Submitted"/> is the false-accept budget, spent and counted.</b> Every one of them is
/// an independent chance of the port's CRC-14 accepting a message nobody sent, at about one in 16 384,
/// so it is the number a report multiplies out — see
/// <see cref="Ft8DeepCombineSettings.ExpectedFalseAccepts"/>. The gap between
/// <see cref="Offered"/> and <see cref="Submitted"/> is the pairing rule refusing a pair on its
/// frequency and time tolerances; the gap between <see cref="Submitted"/> and <see cref="Accepted"/>
/// is <b>the port refusing</b>, which is the ordinary case and is the whole reason the gate is where
/// it is.
/// </para>
/// </remarks>
/// <param name="Offered">
/// Candidate pairs the rule looked at: every candidate in this slot against every candidate in every
/// remembered slot. <b>Not a cost</b> — a pair costs a frequency comparison and a time comparison.
/// </param>
/// <param name="Submitted">
/// <b>Combinations put to the port's parity gate and CRC-14 gate.</b> The budget. Bounded by
/// <c>candidates × MaximumPartners × HistoryDepth</c> and never larger.
/// </param>
/// <param name="Accepted">
/// Of those, how many the port took past both of its gates into a message. <b>The port's verdict and
/// never this library's.</b>
/// </param>
/// <param name="Added">
/// Of those accepted, how many were messages this slot's single-slot path had not already returned.
/// <b>Combining only ever adds</b>, so this is the number of decodes attributable to the stage.
/// </param>
public readonly record struct Ft8DeepCombineCounts(int Offered, int Submitted, int Accepted, int Added);
