namespace Ft8Sharp.Ldpc;

/// <summary>
/// What <see cref="LdpcDecoder"/> found: how many of the code's parity checks the recovered
/// bits still fail, and how hard it had to try.
/// </summary>
/// <remarks>
/// <para>
/// <b>There is no message here and no payload.</b> The decoder is handed 174 numbers and
/// returns 174 bits and these two counts; what those bits mean is the gate's question, not
/// this type's. A decoder that could return a message could be suspected of having been told
/// one.
/// </para>
/// <para>
/// <b><see cref="UnsatisfiedChecks"/> is upstream's <c>*ok</c>, and zero means success.</b>
/// Upstream's <c>ft8/ldpc.h</c> carries a comment saying <c>ok == 87 means success</c>; that
/// comment is wrong twice over -- <c>bp_decode</c> assigns <c>*ok = min_errors</c>, which
/// starts at <see cref="Ft8Tables.LdpcM"/> and falls toward zero, and 87 is not
/// <see cref="Ft8Tables.LdpcM"/> either. The code was followed and the comment was not; see
/// <c>UpstreamLdpcDecoderInventoryTests</c>, which asserts the stale comment is still there so
/// that a re-pin correcting it forces the reading to be re-taken.
/// </para>
/// <para>
/// <b>It is the running minimum across the iterations, not the last iteration's count</b>, and
/// that is upstream's shape rather than a convenience. It means a decode that never reaches
/// zero reports the closest it ever came, which is a more useful number than wherever it
/// happened to stop.
/// </para>
/// </remarks>
public readonly struct LdpcDecodeResult
{
    internal LdpcDecodeResult(int unsatisfiedChecks, int iterations)
    {
        UnsatisfiedChecks = unsatisfiedChecks;
        Iterations = iterations;
    }

    /// <summary>
    /// The fewest of the <see cref="Ft8Tables.LdpcM"/> parity checks left unsatisfied by any
    /// iteration's hard decision. <b>Zero, and only zero, means the bits form a codeword.</b>
    /// </summary>
    public int UnsatisfiedChecks { get; }

    /// <summary>
    /// How many times a hard decision was formed before the decoder stopped -- at most the
    /// maximum it was given.
    /// </summary>
    /// <remarks>
    /// <b>Upstream reports no such count and this is an addition, not a divergence.</b> It
    /// changes no decision the decoder makes; it exists because the cost of correction is a
    /// thing this project has to measure rather than assume, and a count nobody can see cannot
    /// be measured. Iteration 1 is the decision taken on the raw ratios with no message passed
    /// at all, because upstream checks parity at the top of the loop rather than the bottom.
    /// </remarks>
    public int Iterations { get; }

    /// <summary>
    /// Whether the recovered bits satisfy every parity check. <b>This is not enough to return a
    /// message</b> -- the CRC has still to agree, and that gate is <c>Ft8CodewordDecoder</c>'s.
    /// </summary>
    public bool ParitySatisfied => UnsatisfiedChecks == 0;
}
