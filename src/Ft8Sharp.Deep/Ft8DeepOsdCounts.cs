namespace Ft8Sharp.Deep;

/// <summary>
/// <b>What the ordered statistics stage did in one slot, beside the five counts the port already
/// returns.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>These exist because a rate that moved with no visible OSD activity behind it is not
/// evidence.</b> A scoreboard column that gained decodes while <see cref="Accepted"/> stayed at zero
/// would be measuring something other than what it claims to.
/// </para>
/// <para>
/// <b><see cref="Accepted"/> is the port's verdict and not this library's.</b> It counts codewords
/// that <c>Ft8CodewordDecoder</c> took past its own parity gate and its own CRC-14 gate and turned
/// into a message. <see cref="Produced"/> counts what OSD handed over, which is one per candidate it
/// was offered, and the gap between the two is the port refusing - which is the ordinary case and is
/// the whole reason the gate is where it is.
/// </para>
/// </remarks>
/// <param name="Offered">
/// Candidates on which belief propagation returned <c>ParityNeverSatisfied</c> and OSD was therefore
/// asked. Where the port converged, OSD is never run and the port's answer stands.
/// </param>
/// <param name="Produced">
/// Codewords OSD returned. One per candidate offered: the search always ends with a best-ranked
/// codeword, and <b>exactly one of them is submitted to the gate</b>.
/// </param>
/// <param name="Accepted">Of those, how many the port took past both of its gates into a message.</param>
/// <param name="Reencodings">
/// Codewords formed and ranked across the whole slot. <b>The cost, reported rather than estimated</b>,
/// because what an order buys and what it costs is one of step 2's exit criteria.
/// </param>
public readonly record struct Ft8DeepOsdCounts(int Offered, int Produced, int Accepted, long Reencodings);
