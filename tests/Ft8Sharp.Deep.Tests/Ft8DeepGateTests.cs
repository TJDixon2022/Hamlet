using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The port accepts or refuses, never this library. <c>CLAUDE.md</c> §0.0, and it is the criterion
/// step 2 cannot trade.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The specific way this step fails is quiet, so it is written down here.</b> Every codeword put
/// to the CRC-14 is an independent chance of a false accept at about one in 16 384. A slot's 140
/// candidates times an order-2 search of 4187 re-encodings is 586 180 codewords; submitting all of
/// them would be expected to put about 36 messages nobody sent in front of the operator, every slot,
/// each one carrying a valid checksum and looking exactly like a decode. <b>So exactly one codeword
/// per candidate is submitted - the single best by soft distance - and the expected false accepts
/// fall to 140 in 16 384, about 0.009 a slot.</b>
/// </para>
/// <para>
/// <b>And the refusal is watched rather than reasoned about.</b>
/// <see cref="AWrongOsdCodewordIsRefusedByThePortsOwnChecksum"/> makes the OSD stage genuinely fail -
/// more errors in the basis than its order can cover - takes the valid-but-wrong codeword it returns,
/// and puts it through the same <see cref="Ft8DeepOrderedStatistics.Saturate"/> route the loop uses.
/// The port's parity gate passes it, because it is a codeword; the port's CRC-14 gate refuses it,
/// because its checksum is not the checksum of its own payload.
/// </para>
/// </remarks>
public class Ft8DeepGateTests(ITestOutputHelper output)
{
    private const int N = Ft8DeepOrderedStatistics.CodewordBits;

    private static byte[] TransmittedCodeword(string text)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        var packed = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, packed);

        var bits = new byte[N];
        for (var i = 0; i < N; i++)
        {
            bits[i] = (byte)((packed[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    private static float[] RatiosFor(ReadOnlySpan<byte> codeword)
    {
        var ratios = new float[N];
        for (var i = 0; i < N; i++)
        {
            ratios[i] = codeword[i] != 0 ? 4.0f : -4.0f;
        }

        return ratios;
    }

    /// <summary>
    /// <b>A codeword OSD gets RIGHT comes back through the port as the message that was sent</b> -
    /// which is the half of the route that has to work for any of this to be worth doing.
    /// </summary>
    [Fact]
    public void ARightOsdCodewordComesBackThroughThePortAsTheMessage()
    {
        const string text = "HAMLET 246";

        var truth = TransmittedCodeword(text);
        var ratios = RatiosFor(truth);

        // Two errors inside the basis, which order 2 covers.
        ratios[3] = -ratios[3];
        ratios[41] = -ratios[41];

        var recovered = new byte[N];
        var osd = new Ft8DeepOrderedStatistics();
        var found = osd.Decode(ratios, 2, recovered);

        Assert.Equal(truth, recovered);

        var gateRatios = new float[N];
        Ft8DeepOrderedStatistics.Saturate(recovered, gateRatios);
        var gated = Ft8CodewordDecoder.Decode(gateRatios);

        output.WriteLine($"OSD soft distance {found.SoftDistance:F3} in {found.Reencodings} re-encodings");
        output.WriteLine($"the port's verdict  {gated.Status}");
        output.WriteLine($"the port's text     \"{gated.Message.Text}\"");
        output.WriteLine($"belief propagation spent {gated.Correction.Iterations} iteration(s)");

        Assert.Equal(Ft8CodewordStatus.Decoded, gated.Status);
        Assert.Equal(text, gated.Message.Text, StringComparer.Ordinal);
        Assert.Equal(1, gated.Correction.Iterations);
    }

    /// <summary>
    /// <b>A codeword OSD gets WRONG is refused by the port's own checksum, in the port's own
    /// words.</b>
    /// </summary>
    /// <remarks>
    /// This is the case §0.0 is about, and it is the reason nothing in <c>Ft8Sharp.Deep</c> is allowed
    /// to say a message is real. The codeword below is a <em>perfectly valid codeword of the code</em>
    /// - parity holds on every one of the 83 checks - and it is still not what anybody sent. Only the
    /// checksum knows, and only the port asks it.
    /// </remarks>
    [Fact]
    public void AWrongOsdCodewordIsRefusedByThePortsOwnChecksum()
    {
        var truth = TransmittedCodeword("HAMLET 246");
        var ratios = RatiosFor(truth);

        // Five errors inside the basis against an order-1 search: OSD cannot reach the transmitted
        // codeword and will return the best thing it can, which is a codeword nobody sent.
        foreach (var position in new[] { 3, 17, 41, 62, 88 })
        {
            ratios[position] = -ratios[position];
        }

        var recovered = new byte[N];
        var osd = new Ft8DeepOrderedStatistics();
        var found = osd.Decode(ratios, 1, recovered);

        var wrongBy = 0;
        for (var i = 0; i < N; i++)
        {
            if (recovered[i] != truth[i])
            {
                wrongBy++;
            }
        }

        Assert.NotEqual(0, wrongBy);

        // It IS a codeword: re-encoding its own first 91 bits gives it back whole.
        var payload = new byte[LdpcEncoder.PayloadBytes];
        for (var i = 0; i < Ft8DeepOrderedStatistics.BasisBits; i++)
        {
            if (recovered[i] != 0)
            {
                payload[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }

        var reencoded = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, reencoded);
        for (var i = 0; i < N; i++)
        {
            Assert.Equal((byte)((reencoded[i / 8] >> (7 - (i % 8))) & 1), recovered[i]);
        }

        var gateRatios = new float[N];
        Ft8DeepOrderedStatistics.Saturate(recovered, gateRatios);
        var gated = Ft8CodewordDecoder.Decode(gateRatios);

        output.WriteLine(
            $"OSD returned a codeword {wrongBy} bits from the one that was sent, at soft distance "
            + $"{found.SoftDistance:F3} after {found.Reencodings} re-encodings.");
        output.WriteLine("It IS a codeword: re-encoding its own first 91 bits returns it whole.");
        output.WriteLine($"parity satisfied     {gated.Correction.ParitySatisfied}");
        output.WriteLine($"unsatisfied checks   {gated.Correction.UnsatisfiedChecks}");
        output.WriteLine($"THE PORT'S VERDICT   {gated.Status}");
        output.WriteLine($"the port's text      \"{gated.Message.Text}\"");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "Ft8CodewordStatus.ChecksumFailed, in the port's own words: \"The bits formed a valid");
        output.WriteLine(
            "codeword and its checksum disagreed with its payload. Almost always a codeword that was");
        output.WriteLine("never sent, and the one this gate exists for.\"");

        Assert.True(gated.Correction.ParitySatisfied);
        Assert.Equal(Ft8CodewordStatus.ChecksumFailed, gated.Status);
        Assert.False(gated.Decoded);
        Assert.Equal(string.Empty, gated.Message.Text);
    }

    /// <summary>
    /// <b>The de-duplication key for an OSD decode is the codeword OSD already has</b>, not a re-run
    /// of <c>LdpcDecoder.Decode</c> over the original ratios.
    /// </summary>
    /// <remarks>
    /// The port recovers its key by re-running belief propagation over the same ratios, which works
    /// because the port only ever gets there when belief propagation already succeeded. <b>For a
    /// candidate OSD rescued it has not succeeded and will not</b> - that is the definition of the
    /// candidates OSD is offered - so the re-run returns the decoder's closest approach rather than
    /// the message, and two different rescued candidates carrying the same message would compare
    /// unequal and be returned twice. This test watches that happen, so the reason the key is taken
    /// from OSD's own codeword is on the record as a measurement.
    /// </remarks>
    [Fact]
    public void ThePortsWayOfRecoveringTheKeyDoesNotWorkForACandidateOsdRescued()
    {
        // The shape of a candidate OSD is offered: far more hard-decision errors than belief
        // propagation can repair, but nearly all of them in the unreliable positions, so the most
        // reliable basis carries only one. That is unit 246 task 1's measured shape, made exactly.
        var truth = TransmittedCodeword("HAMLET 246");
        var ratios = new float[N];
        for (var i = 0; i < N; i++)
        {
            var magnitude = i < Ft8DeepOrderedStatistics.BasisBits ? 4.0f : 0.5f;
            ratios[i] = truth[i] != 0 ? magnitude : -magnitude;
        }

        var planted = 0;
        for (var i = Ft8DeepOrderedStatistics.BasisBits; i < N; i += 3)
        {
            ratios[i] = -ratios[i];
            planted++;
        }

        ratios[41] = -ratios[41];
        planted++;

        output.WriteLine(
            $"{planted} hard-decision errors planted, {planted - 1} of them below the basis and 1 "
            + "inside it.");

        // Belief propagation on these ratios: this is what the port would re-run for its key.
        var fromBeliefPropagation = new byte[N];
        var correction = LdpcDecoder.Decode(ratios, fromBeliefPropagation, LdpcDecoder.DefaultMaxIterations);

        var recovered = new byte[N];
        var osd = new Ft8DeepOrderedStatistics();
        osd.Decode(ratios, 1, recovered);

        Assert.Equal(truth, recovered);

        var keyDisagreements = 0;
        for (var i = 0; i < Ft8Payload.MessageBits; i++)
        {
            if (fromBeliefPropagation[i] != recovered[i])
            {
                keyDisagreements++;
            }
        }

        output.WriteLine($"belief propagation parity satisfied {correction.ParitySatisfied}");
        output.WriteLine($"iterations spent                    {correction.Iterations}");
        output.WriteLine($"unsatisfied checks                  {correction.UnsatisfiedChecks}");
        output.WriteLine(
            $"key bits where the port's re-run disagrees with OSD's codeword: {keyDisagreements} of "
            + $"{Ft8Payload.MessageBits}");

        Assert.False(
            correction.ParitySatisfied,
            "belief propagation reached a codeword on these ratios, so this is no longer an example "
                + "of a candidate OSD had to rescue and it does not show what it was written to show.");
        Assert.NotEqual(0, keyDisagreements);
    }
}
