using System.Diagnostics;
using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// <b>Step 5, criterion 1, and the number this unit exists to produce:</b> how much damage the
/// FT8 LDPC(174,91) code survives, measured rather than asserted.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nobody handed this project a correcting power and inventing one would be the worst thing
/// that could be done with this criterion.</b> There is no number in the plan, none in the pin
/// and none in the QEX paper that says how many hard bit errors this decoder recovers from at
/// this iteration bound on this code. So it is swept: <c>k</c> bit positions of a known
/// codeword's ratios are flipped, over many trials, at every <c>k</c> from none up to well past
/// the point recovery collapses, and the table is printed before any bound is asserted.
/// </para>
/// <para>
/// <b>THE NUMBER THAT MATTERS MOST IS NOT THE RECOVERY RATE.</b> It is the count of trials that
/// returned a message which was <em>not</em> the message that went in. A decoder that shows an
/// operator a callsign nobody sent is worse than one that shows nothing, and that refusal is
/// what this project is for. CRC-14 has an undetected-error floor of roughly one in sixteen
/// thousand, so <b>the honest answer may not be zero at large <c>k</c></b>; the count is
/// reported with that arithmetic beside it rather than asserted to be zero, and nothing is
/// tuned toward it.
/// </para>
/// <para>
/// <b>A trial recovers only when the 77 bits that come back are the 77 bits that went in.</b>
/// Not "something came back", not "parity was satisfied" -- the message itself, compared byte
/// for byte after the decoder has answered. The truth appears only in the assertion; the
/// decoder is never told it.
/// </para>
/// </remarks>
public class Ft8LdpcCorrectingPowerTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcCorrectingPowerTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// How many independent trials at each error count.
    /// </summary>
    /// <remarks>
    /// <b>Chosen for the wrong-message count's denominator, not for the recovery rate's.</b> A
    /// recovery rate is settled to within a percent or two by a couple of hundred trials; the
    /// number this test leads with is a count of trials that returned the wrong message, and
    /// that number means nothing without the total it is out of. Four hundred at each of
    /// <see cref="MaxK"/>+1 error counts is about ten seconds of the suite, which is what a
    /// five-figure denominator costs.
    /// </remarks>
    private const int TrialsPerK = 400;

    /// <summary>
    /// The largest number of flipped bits swept. Chosen to run well past collapse so the shape is
    /// visible rather than only the edge -- the criterion's second clause is about what happens
    /// beyond the correcting power, and a sweep that stops at the knee cannot show it.
    /// </summary>
    private const int MaxK = 44;

    /// <summary>The seed the sweep's damage is drawn from. Stated so the table is reproducible.</summary>
    private const int BaseSeed = 21_501;

    private enum Outcome
    {
        /// <summary>The 77 bits that came back are the 77 bits that went in.</summary>
        Recovered,

        /// <summary>A message came back and it was a different message. The failure this project refuses.</summary>
        WrongMessage,

        /// <summary>Parity was satisfied by some other codeword and the CRC caught it. Nothing returned.</summary>
        CrcRejected,

        /// <summary>Parity was never satisfied. Nothing returned.</summary>
        NoDecode,
    }

    private sealed class Row
    {
        public int K;
        public int Trials;
        public int Recovered;
        public int WrongMessage;
        public int CrcRejected;
        public int NoDecode;
        public long IterationTotal;
        public int WorstIterations;
    }

    /// <summary>
    /// THE SWEEP. <c>k</c> down the side; the recovery rate, the iteration cost and the
    /// wrong-message count across the row.
    /// </summary>
    [Fact]
    public void TheCorrectingPowerIsSweptOverBitFlipsAndTheWrongMessageCountIsReported()
    {
        var corpus = EncodeCorpus.Build();
        var codewords = corpus.Select(entry => SoftCodeword.CodewordBitsFor(entry.Message)).ToArray();

        var rows = new List<Row>();
        var stopwatch = Stopwatch.StartNew();

        for (var k = 0; k <= MaxK; k++)
        {
            // A seed per row, so a row is reproducible on its own and the rows do not depend on
            // the order they were run in.
            var random = new Random(BaseSeed + k);
            var row = new Row { K = k, Trials = TrialsPerK };

            for (var trial = 0; trial < TrialsPerK; trial++)
            {
                var index = trial % corpus.Count;
                var ratios = SoftCodeword.RatiosFor(codewords[index]);
                SoftCodeword.FlipDistinctPositions(ratios, k, random);

                var bits = new byte[Ft8Tables.LdpcN];
                var result = LdpcDecoder.Decode(ratios, bits);

                row.IterationTotal += result.Iterations;
                row.WorstIterations = Math.Max(row.WorstIterations, result.Iterations);

                switch (Judge(result, bits, corpus[index].Message))
                {
                    case Outcome.Recovered: row.Recovered++; break;
                    case Outcome.WrongMessage: row.WrongMessage++; break;
                    case Outcome.CrcRejected: row.CrcRejected++; break;
                    default: row.NoDecode++; break;
                }
            }

            rows.Add(row);
        }

        stopwatch.Stop();

        // ---- THE TABLE, PRINTED BEFORE ANY BOUND IS ASSERTED -------------------------------
        _output.WriteLine($"corpus messages {corpus.Count}, trials per k {TrialsPerK}, "
            + $"maxIterations {LdpcDecoder.DefaultMaxIterations}, seed {BaseSeed} + k");
        _output.WriteLine($"confident ratio magnitude {SoftCodeword.ConfidentMagnitude:F4} "
            + "(the magnitude at which a hard array already has upstream's variance of 24)");
        _output.WriteLine(string.Empty);
        _output.WriteLine("   k | trials | recovered  rate% | wrongMsg | crcRejected | noDecode | iters mean  worst");
        _output.WriteLine("-----+--------+------------------+----------+-------------+----------+------------------");

        foreach (var row in rows)
        {
            _output.WriteLine(
                $"{row.K,4} | {row.Trials,6} | {row.Recovered,9} {100.0 * row.Recovered / row.Trials,6:F1} | "
                + $"{row.WrongMessage,8} | {row.CrcRejected,11} | {row.NoDecode,8} | "
                + $"{(double)row.IterationTotal / row.Trials,10:F2} {row.WorstIterations,6}");
        }

        var totalTrials = rows.Sum(r => r.Trials);
        var totalWrong = rows.Sum(r => r.WrongMessage);
        var lastAllRecovered = rows.Where(r => r.Recovered == r.Trials).Select(r => r.K).DefaultIfEmpty(-1).Max();
        var firstNoneRecovered = rows.Where(r => r.Recovered == 0).Select(r => (int?)r.K).FirstOrDefault();

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE THREE NUMBERS, IN WORDS");
        _output.WriteLine($"  largest k at which EVERY trial recovered : {lastAllRecovered} "
            + $"({TrialsPerK} of {TrialsPerK} trials)");
        _output.WriteLine($"  first k at which recovery reached ZERO   : "
            + $"{(firstNoneRecovered.HasValue ? firstNoneRecovered.Value.ToString() : "not reached by k = " + MaxK)}");
        _output.WriteLine($"  WRONG MESSAGES RETURNED, WHOLE SWEEP     : {totalWrong} out of {totalTrials} trials");
        _output.WriteLine($"    CRC-14's undetected-error floor is 1 in 2^14 = 16384, so a codeword that");
        _output.WriteLine($"    converged to some OTHER valid codeword escapes the gate about that often.");
        _output.WriteLine($"    Trials that reached a wrong codeword at all (crcRejected + wrongMsg): "
            + $"{rows.Sum(r => r.CrcRejected + r.WrongMessage)}");
        _output.WriteLine($"    Expected wrong messages at that rate     : "
            + $"{rows.Sum(r => r.CrcRejected + r.WrongMessage) / 16384.0:F4}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {totalTrials} decodes in {stopwatch.ElapsedMilliseconds} ms "
            + $"= {(double)stopwatch.ElapsedMilliseconds / totalTrials:F3} ms each");

        // ---- THE HONEST-FAILURE HALF, THE CRITERION'S SECOND CLAUSE ------------------------
        var collapsed = rows.LastOrDefault(r => r.Recovered == 0);
        if (collapsed is not null)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine($"BEYOND THE CORRECTING POWER, at k = {collapsed.K}:");
            _output.WriteLine($"  returned NOTHING       : {collapsed.NoDecode + collapsed.CrcRejected} "
                + $"({collapsed.NoDecode} on parity, {collapsed.CrcRejected} on CRC)");
            _output.WriteLine($"  returned a WRONG message: {collapsed.WrongMessage}");
            _output.WriteLine($"  and the sum is the trial count: "
                + $"{collapsed.NoDecode + collapsed.CrcRejected + collapsed.WrongMessage} of {collapsed.Trials}");
        }

        // ---- ASSERTIONS, AFTER THE NUMBERS -------------------------------------------------
        foreach (var row in rows)
        {
            Assert.Equal(
                row.Trials,
                row.Recovered + row.WrongMessage + row.CrcRejected + row.NoDecode);
        }

        // Undamaged in, undamaged out. This is the only row with a bound written in advance,
        // and it is written in advance because it is a tautology of the encoder rather than a
        // measurement of the decoder.
        Assert.Equal(TrialsPerK, rows[0].Recovered);
        Assert.Equal(0, rows[0].WrongMessage);

        // The sweep must actually reach collapse, or it has not measured a correcting power --
        // it has measured the edge of the range somebody chose.
        Assert.True(
            firstNoneRecovered.HasValue,
            $"recovery never reached zero by k = {MaxK}, so this sweep has not found the code's "
            + "limit and the table's rightmost column is the range's edge and not the code's.");

        // And there must be a run of complete recovery to report, or there is no correcting
        // power to state.
        Assert.True(lastAllRecovered >= 1, "not one k above zero recovered on every trial.");
    }

    /// <summary>
    /// The gate, composed in the test project from the two pieces this library already proves,
    /// and used here so that task 4's table does not wait on task 5's seam.
    /// </summary>
    /// <remarks>
    /// <b>Both halves are required and neither is re-implemented.</b> Parity comes from the
    /// decoder's own count; the CRC comes from <see cref="Ft8Payload.TryRead"/>, which unit 206
    /// proved and which is the only CRC check in this tree. <c>Ft8CodewordDecoder</c> composes
    /// exactly the same two pieces in the library, and
    /// <c>Ft8LdpcCodewordGateTests.TheLibrarysGateAgreesWithTheTestProjectsCompositionOverTheCorpus</c>
    /// holds the two against each other rather than letting them drift.
    /// </remarks>
    private static Outcome Judge(LdpcDecodeResult result, ReadOnlySpan<byte> bits, byte[] expected)
    {
        if (!result.ParitySatisfied)
        {
            return Outcome.NoDecode;
        }

        var message = SoftCodeword.MessageFrom(bits);
        if (message is null)
        {
            return Outcome.CrcRejected;
        }

        return message.AsSpan().SequenceEqual(expected) ? Outcome.Recovered : Outcome.WrongMessage;
    }
}
