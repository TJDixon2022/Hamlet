using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>A combined codeword is accepted or refused by the port's own parity gate and CRC-14 gate, and by
/// nothing in this library.</b> <c>CLAUDE.md</c> §0.0, at the place unit 247 could fudge it.
/// </summary>
/// <remarks>
/// <para>
/// <b>The specific way soft combining fails is quiet, and the arithmetic is here rather than only in a
/// report.</b> Every combination put to the port's checksum is an independent chance of a false accept
/// at about <b>one in 16 384</b>. Pairing every candidate with every candidate over two slots of 140
/// is <c>19 600</c> submissions a slot pair, about <b>1.2 expected wrong decodes per trial</b> — which
/// would put messages nobody sent in front of the operator inside one rung of the ladder, each one
/// carrying a valid checksum and looking exactly like a decode. <see cref="Ft8DeepCombineSettings"/>
/// bounds it to <c>candidates × MaximumPartners × HistoryDepth</c>, and this file watches the port
/// refuse the wrong pairings that bound admits.
/// </para>
/// <para>
/// <b>The refusal is watched rather than reasoned about.</b> Two hearings of two <em>different</em>
/// transmissions are combined deliberately and put through the same route the loop uses, and what the
/// port says about them is quoted.
/// </para>
/// </remarks>
public class Ft8DeepCombineGateTests(ITestOutputHelper output)
{
    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>
    /// <b>A RIGHT pairing that neither hearing could decode alone comes back through the port as the
    /// message that was sent.</b> The half of the route that has to work for any of this to be worth
    /// doing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The errors are planted at disjoint positions in the two hearings, so their combination is the
    /// transmitted codeword exactly. <b>How many it takes before neither hearing decodes alone is
    /// swept rather than assumed</b>, because these ratios are cleaner than a real candidate's — the
    /// wrong positions carry deliberately weak evidence, which is what makes a hard decision go wrong
    /// there, and belief propagation repairs a great many weak errors. The sweep is printed, and the
    /// row that matters is the first one where both hearings alone are refused.
    /// </para>
    /// </remarks>
    [Fact]
    public void ARightPairingNeitherHearingCouldDecodeAloneComesBackAsTheMessage()
    {
        const string text = "HAMLET 247";

        var truth = TransmittedCodeword(text);
        var random = new Random(247_101);
        var combined = new float[N];
        var reached = 0;

        output.WriteLine("errors each  hearing A alone       hearing B alone       COMBINED");

        foreach (var errors in new[] { 20, 30, 40, 50, 60, 70, 80 })
        {
            var first = ChoosePositions(random, errors, []);
            var second = ChoosePositions(random, errors, first);

            var a = Hearing(random, truth, first);
            var b = Hearing(random, truth, second);

            var alone1 = Ft8CodewordDecoder.Decode(Normalised(a));
            var alone2 = Ft8CodewordDecoder.Decode(Normalised(b));

            var summed = Ft8DeepSoftCombiner.Combine(a, b, Ft8DeepCombineWeighting.Equal, combined);
            var together = Ft8CodewordDecoder.Decode(combined);

            output.WriteLine(
                $"{errors,11}  {alone1.Status,-20}  {alone2.Status,-20}  {together.Status} "
                + $"\"{together.Message.Text}\"  (summed variance {summed:F1})");

            // THE ROW THAT MATTERS: neither hearing alone, and the combination is the message.
            if (!alone1.Decoded && !alone2.Decoded && together.Decoded)
            {
                Assert.Equal(text, together.Message.Text);
                reached++;
            }

            // And wherever the combination did decode, it decoded the message that was sent. A
            // combination that produced some OTHER valid message would be the one thing this step
            // must never do, whatever the rate.
            if (together.Decoded)
            {
                Assert.Equal(text, together.Message.Text);
            }
        }

        Assert.True(
            reached > 0,
            "no planted error count produced a pair that neither hearing could decode alone and the "
                + "combination could, so this test is not evidence that combining reaches anything.");

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{reached} of the swept rows had NEITHER hearing decode alone and the combination decode "
            + "the message that was sent.");
    }

    /// <summary>
    /// <b>A DELIBERATELY WRONG PAIRING IS REFUSED BY THE PORT, IN THE PORT'S OWN WORDS.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two hearings of two different transmissions, each perfectly readable on its own, combined as if
    /// they were the same station repeating itself. The sum reinforces where the two codewords agree
    /// and cancels where they do not, so it lands about half their separation from each.
    /// </para>
    /// <para>
    /// <b>WHAT IS ASSERTED IS §0.0 AND NOT SOMETHING STRONGER, AND THE DIFFERENCE IS THE FINDING.</b>
    /// A wrong pairing must never produce <em>a message nobody sent</em>. It may produce one of the two
    /// messages that went into it, and on this fixture it sometimes does: where the two transmissions
    /// are near-neighbours in the message space — <c>HAMLET 247 A</c> and <c>HAMLET 247 C</c> differ by
    /// one character, so their codewords sit close together — the combination stays inside the
    /// stronger one's basin and belief propagation finishes on it. <b>That is a real transmission
    /// returned, not an invented one</b>, and treating it as a fault would be asserting something the
    /// phase does not require and does not want. Every acceptance is checked against both inputs' text
    /// and the count of messages that were neither is what must be zero.
    /// </para>
    /// <para>
    /// <b>What the port says is the point of the test</b>, so the whole distribution of verdicts is
    /// printed. Nothing in <c>Ft8Sharp.Deep</c> looked at these bits and decided anything.
    /// </para>
    /// </remarks>
    [Fact]
    public void ADeliberatelyWrongPairingIsRefusedByThePortsOwnGates()
    {
        var messages = new[]
        {
            "HAMLET 247 A", "HAMLET 247 B", "HAMLET 247 C", "HAMLET 247 D",
            "CQ TEST K1ABC", "CQ DX W9XYZ", "K1ABC W9XYZ", "W9XYZ K1ABC",
        };

        var codewords = messages.Select(TransmittedCodeword).ToArray();
        var random = new Random(247_102);
        var combined = new float[N];

        var parityNeverSatisfied = 0;
        var checksumFailed = 0;
        var notReadable = 0;
        var returnedAnInput = new List<string>();
        var invented = new List<string>();
        var submissions = 0;
        var closest = int.MaxValue;
        var furthest = 0;

        for (var i = 0; i < codewords.Length; i++)
        {
            for (var j = 0; j < codewords.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var separation = Distance(codewords[i], codewords[j]);
                closest = Math.Min(closest, separation);
                furthest = Math.Max(furthest, separation);

                var a = Hearing(random, codewords[i], ChoosePositions(random, 8, []));
                var b = Hearing(random, codewords[j], ChoosePositions(random, 8, []));

                // Each hearing on its own decodes, so nothing below is an artefact of two bad
                // hearings - it is the pairing and only the pairing.
                Assert.True(Ft8CodewordDecoder.Decode(Normalised(a)).Decoded);
                Assert.True(Ft8CodewordDecoder.Decode(Normalised(b)).Decoded);

                Ft8DeepSoftCombiner.Combine(a, b, Ft8DeepCombineWeighting.Equal, combined);
                var verdict = Ft8CodewordDecoder.Decode(combined);
                submissions++;

                switch (verdict.Status)
                {
                    case Ft8CodewordStatus.ParityNeverSatisfied:
                        parityNeverSatisfied++;
                        break;
                    case Ft8CodewordStatus.ChecksumFailed:
                        checksumFailed++;
                        break;
                    case Ft8CodewordStatus.MessageNotReadable:
                        notReadable++;
                        break;
                    default:
                        var text = verdict.Message.Text;
                        var line =
                            $"\"{messages[i]}\" ({separation} apart) + \"{messages[j]}\" -> \"{text}\"";
                        if (string.Equals(text, messages[i], StringComparison.Ordinal)
                            || string.Equals(text, messages[j], StringComparison.Ordinal))
                        {
                            returnedAnInput.Add(line);
                        }
                        else
                        {
                            invented.Add(line);
                        }

                        break;
                }
            }
        }

        output.WriteLine(
            $"{submissions} deliberately wrong pairings, every input decodable on its own. The two "
            + $"codewords sat {closest} to {furthest} of {N} apart.");
        output.WriteLine("The port's verdicts, in the port's own words:");
        output.WriteLine($"  {Ft8CodewordStatus.ParityNeverSatisfied,-22} {parityNeverSatisfied}");
        output.WriteLine($"  {Ft8CodewordStatus.ChecksumFailed,-22} {checksumFailed}");
        output.WriteLine($"  {Ft8CodewordStatus.MessageNotReadable,-22} {notReadable}");
        output.WriteLine($"  {"Decoded, an input",-22} {returnedAnInput.Count}");
        output.WriteLine($"  {"Decoded, NOBODY SENT IT",-22} {invented.Count}");
        output.WriteLine(string.Empty);

        foreach (var line in returnedAnInput)
        {
            output.WriteLine($"  returned a message that WAS sent: {line}");
        }

        foreach (var line in invented)
        {
            output.WriteLine($"  RETURNED A MESSAGE NOBODY SENT: {line}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "GATE 1 is parity, at Ft8CodewordDecoder.cs:80: \"Until this holds, the bits are the");
        output.WriteLine(
            "decoder's closest approach and not a codeword, so there is nothing here to compute a");
        output.WriteLine(
            "checksum over.\" GATE 2 is the checksum, at :96: \"belief propagation can converge on a");
        output.WriteLine(
            "perfectly valid codeword that is not the one that was sent, and every parity check in");
        output.WriteLine("the code will agree with it. Only the checksum knows.\"");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"Naive expectation for {submissions} submissions: "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(submissions):F4} messages nobody sent.");

        // §0.0. A wrong pairing that returns one of its own two transmissions has returned something
        // that was on the air; a wrong pairing that returns anything else has not, and that is the
        // one thing this step may never do.
        Assert.Empty(invented);
    }

    /// <summary>
    /// <b>The submission budget, multiplied out, so a report quotes arithmetic rather than a hope.</b>
    /// </summary>
    [Fact]
    public void TheSubmissionBudgetIsBoundedAndTheArithmeticIsPinned()
    {
        var settings = Ft8DeepCombineSettings.Default;

        Assert.Equal(1, settings.HistoryDepth);
        Assert.Equal(1, settings.MaximumPartners);
        Assert.Equal(6.25, settings.FrequencyToleranceHz, 6);
        Assert.Equal(0.32, settings.TimeToleranceSeconds, 6);
        Assert.Equal(Ft8DeepCombineWeighting.Equal, settings.Weighting);

        var worstCandidates = new Ft8SlotDecoder().CandidateLimit;

        output.WriteLine("THE SUBMISSION BUDGET, at the defaults.");
        output.WriteLine(
            $"  pairing rule            within {settings.FrequencyToleranceHz:F2} Hz and "
            + $"{settings.TimeToleranceSeconds:F2} s, {settings.MaximumPartners} partner(s) per "
            + $"candidate, {settings.HistoryDepth} slot(s) of history");
        output.WriteLine(string.Empty);
        output.WriteLine("  case                       candidates/slot  submissions/slot pair  over 306 trials  expected wrong");

        foreach (var (name, candidates) in new[]
                 {
                     ("unbounded pairing", -1),
                     ("worst case, the port's candidate limit", worstCandidates),
                     ("observed mean at -21 dB", 13),
                 })
        {
            if (candidates < 0)
            {
                var unbounded = (long)worstCandidates * worstCandidates;
                output.WriteLine(
                    $"  {name,-38} {worstCandidates,6}  {unbounded,20}  {unbounded * 306,15}  "
                    + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(unbounded * 306),14:F1}");
                continue;
            }

            var perPair = settings.SubmissionsPerSlot(candidates);
            output.WriteLine(
                $"  {name,-38} {candidates,6}  {perPair,20}  {(long)perPair * 306,15}  "
                + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts((long)perPair * 306),14:F2}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "The unbounded row is what this rule exists to refuse: about 1.2 expected wrong decodes a");
        output.WriteLine(
            "trial, and 366 across a rung. The bounded rows are upper bounds too, because a submission");
        output.WriteLine(
            "only reaches the CRC-14 if the port's parity gate converged on it first, and most do not.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "THE WORST-CASE ROW IS ABOVE ONE AND IS SAID TO BE, rather than hidden. It is a slot whose");
        output.WriteLine(
            "search returns the port's full candidate limit, on every one of 306 trials; the ladder's");
        output.WriteLine(
            "slots carry one transmission and return about 13. The number a report quotes is the");
        output.WriteLine(
            "submissions actually counted by Ft8DeepCombineCounts over the run, not this bound.");

        // The bound is the whole claim, so it is asserted rather than printed and hoped over.
        Assert.Equal(13, settings.SubmissionsPerSlot(13));
        Assert.Equal(worstCandidates, settings.SubmissionsPerSlot(worstCandidates));
        Assert.Equal(0, settings.SubmissionsPerSlot(0));

        // One submission per candidate per remembered slot is the SMALLEST budget that can produce a
        // combination at all, so this is the floor of the whole approach rather than a setting.
        Assert.Equal(
            settings.SubmissionsPerSlot(worstCandidates),
            worstCandidates * settings.MaximumPartners * settings.HistoryDepth);
        Assert.True(
            Ft8DeepCombineSettings.ExpectedFalseAccepts((long)settings.SubmissionsPerSlot(13) * 306)
                < 0.5,
            "at the candidate count the ladder actually produces, the naive expected number of "
                + "messages nobody sent across a whole 306-trial rung must stay well under one, or "
                + "the budget is not a budget.");
        Assert.True(
            settings.SubmissionsPerSlot(worstCandidates)
                < worstCandidates * worstCandidates / 100,
            "the bounded budget must be at least a hundred times smaller than pairing every candidate "
                + "with every candidate, which is the arithmetic this rule exists to refuse.");
    }

    /// <summary>What the settings refuse, and why the message says so.</summary>
    [Fact]
    public void TheSettingsRefuseAnUnboundedPairing()
    {
        var depth = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(historyDepth: 0));
        var deepDepth = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(
                historyDepth: Ft8DeepCombineSettings.MaximumHistoryDepth + 1));
        var partners = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(maximumPartners: 0));
        var manyPartners = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(
                maximumPartners: Ft8DeepCombineSettings.MaximumPartnersAllowed + 1));
        var frequency = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(frequencyToleranceHz: double.PositiveInfinity));
        var time = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(timeToleranceSeconds: -1.0));

        output.WriteLine(depth.Message);
        output.WriteLine(deepDepth.Message);
        output.WriteLine(partners.Message);
        output.WriteLine(manyPartners.Message);
        output.WriteLine(frequency.Message);
        output.WriteLine(time.Message);
    }

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

    /// <summary>
    /// One hearing of a codeword: confident where it is right, weak where it is wrong, which is what a
    /// hard decision from real tone magnitudes does.
    /// </summary>
    private static float[] Hearing(Random random, byte[] truth, HashSet<int> wrong)
    {
        var ratios = new float[N];
        for (var i = 0; i < N; i++)
        {
            var sign = truth[i] != 0 ? 1.0 : -1.0;
            var magnitude = wrong.Contains(i)
                ? -(0.05 + (0.45 * random.NextDouble()))
                : 0.6 + (0.8 * random.NextDouble());
            ratios[i] = (float)(sign * magnitude);
        }

        return ratios;
    }

    private static HashSet<int> ChoosePositions(Random random, int count, HashSet<int> avoid)
    {
        var chosen = new HashSet<int>(count);
        while (chosen.Count < count)
        {
            var position = random.Next(N);
            if (!avoid.Contains(position))
            {
                chosen.Add(position);
            }
        }

        return chosen;
    }

    private static int Distance(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        var distance = 0;
        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                distance++;
            }
        }

        return distance;
    }

    /// <summary>A copy at the port's own scale, so a single hearing is judged the way the loop does.</summary>
    private static float[] Normalised(float[] ratios)
    {
        var copy = (float[])ratios.Clone();
        Ft8SoftSymbols.Normalise(copy);
        return copy;
    }
}
