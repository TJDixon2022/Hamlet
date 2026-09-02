using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// <b>Step 5, criterion 2: a candidate failing CRC is never returned as a decode, however
/// tempting the partial.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHICH READING OF THE CRITERION THIS STANDS ON, SAID PLAINLY.</b> The criterion says
/// <em>a candidate</em>, and there are no candidates in this path. Soft symbol extraction does
/// not exist in this library: nothing turns a place in a waterfall into log-likelihood ratios,
/// so nothing that has ever been near a radio reaches this gate. <b>The gate is proven at the
/// codeword entry point, and the criterion is re-taken end to end when the next unit connects a
/// candidate to it.</b> That is the honest statement and this file does not claim more.
/// </para>
/// <para>
/// <b>The tempting case is the second test and it is the one the criterion is actually
/// about.</b> Belief propagation converging on a perfectly valid codeword that nobody sent is
/// not a hypothetical: every one of the code's 83 parity checks agrees with it, the decoder
/// reports a clean correction, and the 91 bits underneath look exactly like a payload. Only the
/// checksum knows the difference. So a codeword is built with its checksum bits deliberately
/// wrong -- valid parity, wrong CRC -- and the gate has to return nothing.
/// </para>
/// </remarks>
public class Ft8LdpcCodewordGateTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcCodewordGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>Trials in each of the two noise families.</summary>
    private const int NoiseTrials = 5_000;

    private const int UniformSeed = 21_552;
    private const int GaussianSeed = 21_554;

    /// <summary>
    /// COUNT 1 -- a clean codeword decodes to its own message, over the whole corpus.
    /// </summary>
    /// <remarks>
    /// Two comparisons, because they answer different questions. <b>The bits</b>: the 77 bits
    /// that come back through the decoder are the 77 bits that went in. <b>The gate</b>: what
    /// <see cref="Ft8CodewordDecoder"/> says about those ratios is exactly what
    /// <see cref="Ft8MessageDecoder"/> says about the original message -- same type, same status,
    /// same text. That second form covers the corpus entries that are sound messages this
    /// library cannot put into words, and it covers them by <em>agreeing about the refusal</em>
    /// rather than by being excused from the count.
    /// </remarks>
    [Fact]
    public void ACleanCodewordDecodesToItsOwnMessageOverTheWholeCorpus()
    {
        var corpus = EncodeCorpus.Build();

        var bitsRecovered = 0;
        var gateAgreedWithStepTwo = 0;
        var becameText = 0;
        var soundButUnreadable = new List<string>();

        foreach (var entry in corpus)
        {
            var ratios = SoftCodeword.RatiosFor(SoftCodeword.CodewordBitsFor(entry.Message));

            // The bits, through the decoder alone.
            var bits = new byte[Ft8Tables.LdpcN];
            var correction = LdpcDecoder.Decode(ratios, bits);
            var recovered = correction.ParitySatisfied ? SoftCodeword.MessageFrom(bits) : null;
            if (recovered is not null && recovered.AsSpan().SequenceEqual(entry.Message))
            {
                bitsRecovered++;
            }

            // The gate, against what step 2 makes of the message that went in.
            var gated = Ft8CodewordDecoder.Decode(ratios);
            var expected = Ft8MessageDecoder.Decode(entry.Message);

            if (gated.Message.Type == expected.Type
                && gated.Message.Status == expected.Status
                && gated.Message.Text == expected.Text)
            {
                gateAgreedWithStepTwo++;
            }

            if (gated.Decoded)
            {
                becameText++;
            }
            else
            {
                soundButUnreadable.Add($"{entry.Label} [{entry.Kind}] -> {gated.Status} / {gated.Message.Status}");
            }

            Assert.NotEqual(Ft8CodewordStatus.ParityNeverSatisfied, gated.Status);
            Assert.NotEqual(Ft8CodewordStatus.ChecksumFailed, gated.Status);
        }

        _output.WriteLine($"corpus messages                                      : {corpus.Count}");
        _output.WriteLine($"77 bits recovered exactly                            : {bitsRecovered} of {corpus.Count}");
        _output.WriteLine($"gate agrees with step 2 about the same message       : "
            + $"{gateAgreedWithStepTwo} of {corpus.Count}");
        _output.WriteLine($"passed BOTH gates and became text                    : {becameText} of {corpus.Count}");
        _output.WriteLine($"passed both gates and are sound but not readable here: {soundButUnreadable.Count}");
        foreach (var line in soundButUnreadable)
        {
            _output.WriteLine($"    {line}");
        }

        Assert.Equal(corpus.Count, bitsRecovered);
        Assert.Equal(corpus.Count, gateAgreedWithStepTwo);
    }

    /// <summary>
    /// COUNT 2 -- <b>THE TEMPTING CASE.</b> A codeword whose payload checksum bits have been
    /// altered is a perfectly valid codeword with a wrong checksum, and the gate returns nothing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The construction is the point. The checksum bits are altered <em>before</em> the parity is
    /// computed, so what reaches the decoder is a genuine member of the code: belief propagation
    /// finds it in one iteration with zero unsatisfied checks and would hand it over without a
    /// murmur if parity were the only gate. <b>That is exactly what converging to the wrong
    /// codeword looks like from inside</b>, and it is reproduced here deliberately rather than
    /// waited for.
    /// </para>
    /// <para>
    /// Two families are swept: every one of the 14 checksum bits, and every one of the 77 message
    /// bits. Both produce a valid codeword whose stored checksum is not the checksum of its own
    /// payload, and neither may return anything.
    /// </para>
    /// </remarks>
    [Fact]
    public void ACodewordWithAlteredChecksumBitsIsValidParityAndReturnsNothing()
    {
        var corpus = EncodeCorpus.Build();

        var checksumTrials = 0;
        var messageTrials = 0;
        var returnedSomething = 0;
        var parityWasSatisfied = 0;
        var refusedOnChecksum = 0;

        foreach (var entry in corpus)
        {
            var clean = new byte[Ft8Payload.PayloadBytes];
            Ft8Payload.Create(entry.Message, clean);

            // Bits 77..90 are the checksum; bits 0..76 are the message.
            foreach (var bit in Enumerable.Range(Ft8Payload.MessageBits, Ft8Payload.CrcBits)
                         .Concat(Enumerable.Range(0, Ft8Payload.MessageBits)))
            {
                var isChecksumBit = bit >= Ft8Payload.MessageBits;
                if (isChecksumBit)
                {
                    checksumTrials++;
                }
                else
                {
                    messageTrials++;
                }

                var altered = (byte[])clean.Clone();
                altered[bit / 8] ^= (byte)(0x80u >> (bit % 8));

                var codeword = new byte[LdpcEncoder.CodewordBytes];
                LdpcEncoder.Encode(altered, codeword);
                var ratios = SoftCodeword.RatiosFor(
                    LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN));

                var result = Ft8CodewordDecoder.Decode(ratios);

                if (result.Correction.ParitySatisfied)
                {
                    parityWasSatisfied++;
                }

                if (result.Status == Ft8CodewordStatus.ChecksumFailed)
                {
                    refusedOnChecksum++;
                }

                if (result.Decoded || result.Message.Text.Length > 0)
                {
                    returnedSomething++;
                }
            }
        }

        var trials = checksumTrials + messageTrials;

        _output.WriteLine($"corpus messages                                  : {corpus.Count}");
        _output.WriteLine($"checksum-bit alterations tried                   : {checksumTrials}");
        _output.WriteLine($"message-bit alterations tried                    : {messageTrials}");
        _output.WriteLine($"total tried                                      : {trials}");
        _output.WriteLine($"of which PARITY WAS FULLY SATISFIED              : {parityWasSatisfied} "
            + "  <-- every one is a genuine codeword the LDPC gate cannot fault");
        _output.WriteLine($"refused at the checksum gate                     : {refusedOnChecksum}");
        _output.WriteLine($"RETURNED ANYTHING AT ALL                         : {returnedSomething}");

        Assert.Equal(trials, parityWasSatisfied);
        Assert.Equal(trials, refusedOnChecksum);
        Assert.Equal(0, returnedSomething);
    }

    /// <summary>
    /// COUNT 3 -- random ratios, uniformly drawn, seeded: how many returned a message.
    /// </summary>
    /// <remarks>
    /// <b>The count is reported, not a pass.</b> A random hard decision satisfies all 83 checks
    /// with probability 2^-83, so this is expected to be zero and the value of running it is that
    /// it was run rather than argued. The distribution of the outcomes is printed too, because
    /// "0 returned" over a set that never even reached parity says something different from "0
    /// returned" over a set that reached it and was stopped by the checksum.
    /// </remarks>
    [Fact]
    public void RandomRatiosReturnAMessageThisManyTimes()
    {
        var random = new Random(UniformSeed);
        var ratios = new float[Ft8Tables.LdpcN];

        var returned = 0;
        var byStatus = new Dictionary<Ft8CodewordStatus, int>();
        var worstUnsatisfied = int.MaxValue;

        for (var trial = 0; trial < NoiseTrials; trial++)
        {
            for (var i = 0; i < ratios.Length; i++)
            {
                ratios[i] = (float)((random.NextDouble() * 2.0 - 1.0) * SoftCodeword.ConfidentMagnitude);
            }

            var result = Ft8CodewordDecoder.Decode(ratios);
            byStatus[result.Status] = byStatus.GetValueOrDefault(result.Status) + 1;
            worstUnsatisfied = Math.Min(worstUnsatisfied, result.Correction.UnsatisfiedChecks);

            if (result.Decoded || result.Message.Text.Length > 0)
            {
                returned++;
            }
        }

        Report("uniform on [-A, +A]", UniformSeed, returned, byStatus, worstUnsatisfied);
        Assert.Equal(0, returned);
    }

    /// <summary>
    /// COUNT 4 -- ratios from noise alone, with no codeword under them at all.
    /// </summary>
    /// <remarks>
    /// <b>The same question, and it is not the same array.</b> Count 3's ratios are uniform;
    /// these are Gaussian, from the seeded generator unit 213 built and this project's only noise
    /// source. A real channel delivers something Gaussian rather than something uniform, so this
    /// is the closer of the two to what the next unit's extraction will hand over when it is
    /// pointed at a piece of empty band.
    /// </remarks>
    [Fact]
    public void RatiosFromNoiseAloneReturnAMessageThisManyTimes()
    {
        var noise = new GaussianNoise(GaussianSeed);

        var returned = 0;
        var byStatus = new Dictionary<Ft8CodewordStatus, int>();
        var worstUnsatisfied = int.MaxValue;

        for (var trial = 0; trial < NoiseTrials; trial++)
        {
            var ratios = noise.Block(Ft8Tables.LdpcN, SoftCodeword.ConfidentMagnitude);

            var result = Ft8CodewordDecoder.Decode(ratios);
            byStatus[result.Status] = byStatus.GetValueOrDefault(result.Status) + 1;
            worstUnsatisfied = Math.Min(worstUnsatisfied, result.Correction.UnsatisfiedChecks);

            if (result.Decoded || result.Message.Text.Length > 0)
            {
                returned++;
            }
        }

        Report("Gaussian, no codeword under it", GaussianSeed, returned, byStatus, worstUnsatisfied);
        Assert.Equal(0, returned);
    }

    private void Report(
        string family,
        int seed,
        int returned,
        Dictionary<Ft8CodewordStatus, int> byStatus,
        int worstUnsatisfied)
    {
        _output.WriteLine($"family                       : {family}");
        _output.WriteLine($"trials                       : {NoiseTrials}, seed {seed}");
        _output.WriteLine($"RETURNED A MESSAGE           : {returned} of {NoiseTrials}");
        _output.WriteLine($"closest any trial came       : {worstUnsatisfied} of {Ft8Tables.LdpcM} "
            + "checks still unsatisfied");
        foreach (var (status, count) in byStatus.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
        {
            _output.WriteLine($"  {status,-22} : {count}");
        }
    }
}
