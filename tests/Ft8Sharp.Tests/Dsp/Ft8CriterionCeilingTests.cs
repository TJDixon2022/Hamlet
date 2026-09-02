using Ft8Sharp.Message;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE CEILING ON CRITERION 3, computed before the residue is judged rather than discovered at
/// the end of it.</b> How many of upstream's 1298 expected lines this library could match with a
/// perfect receiver — that is, how many of them its message layer can represent at all.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is worth a task of its own.</b> This library cannot match an expected line it cannot
/// <em>say</em>. If a line names a station by a hash the list's own writer could not resolve, or is
/// of a shape no type this library builds accepts, then no improvement to the search, the
/// extraction or the correcting power could ever turn it into a match — the limit is arithmetic and
/// the DSP is irrelevant to it. <b>760 of 1298 and 760 of a ceiling are different reports about the
/// same night</b>, and both are stated here so neither can stand alone.
/// </para>
/// <para>
/// <b>A ceiling below 1298 is not an excuse and is not used as one.</b> It is a measurement of where
/// the remaining work lives: what it says is that the receive-side shortfall is smaller than the raw
/// fraction suggests, and that a named part of the rest sits in a decision the owner holds rather
/// than in any code this phase can change.
/// </para>
/// <para>
/// <b>The hashed lines are counted apart and the reason is not a technicality.</b> A line printed
/// <c>&lt;...&gt;</c> has lost the callsign in the <em>list</em>. Nobody can re-pack it — not this
/// library, not upstream, not the thing that wrote the list. Folding those into "lines this library
/// refuses" would blame this port for somebody else's missing information, and folding them the
/// other way would hide a real cost. They get their own row.
/// </para>
/// </remarks>
public class Ft8CriterionCeilingTests
{
    private readonly ITestOutputHelper _output;

    public Ft8CriterionCeilingTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Every expected line put through this library's own packers, with the reason named for every
    /// one that does not come back.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheHighestScoreCriterionThreeCouldReachIsComputedBeforeTheResidueIsJudged()
    {
        var recordings = ReferenceRecordings.WithExpectedLists();
        Assert.NotEmpty(recordings);

        var counts = new Dictionary<ExpectedMessagePacker.PackFailure, int>();
        var lines = 0;
        var examples = new Dictionary<ExpectedMessagePacker.PackFailure, List<string>>();

        foreach (var recording in recordings)
        {
            foreach (var text in recording.ExpectedMessages())
            {
                lines++;
                var outcome = ExpectedMessagePacker.TryPack(text, out _);
                counts.TryGetValue(outcome, out var current);
                counts[outcome] = current + 1;

                if (outcome == ExpectedMessagePacker.PackFailure.None)
                {
                    continue;
                }

                if (!examples.TryGetValue(outcome, out var sample))
                {
                    sample = new List<string>();
                    examples[outcome] = sample;
                }

                if (sample.Count < 8)
                {
                    sample.Add($"{recording.Name,-22} {text}");
                }
            }
        }

        var representable = Count(counts, ExpectedMessagePacker.PackFailure.None);
        var hashed = Count(counts, ExpectedMessagePacker.PackFailure.HashedCallsignLostInTheList);
        var noShape = Count(counts, ExpectedMessagePacker.PackFailure.NoShapeThisLibraryBuildsAcceptsIt);
        var noRoundTrip = Count(counts, ExpectedMessagePacker.PackFailure.PackedButDidNotRoundTrip);

        _output.WriteLine($"{"outcome",-46} {"lines",6} {"share",8}");
        Row("REPRESENTABLE - packs and round-trips", representable, lines);
        Row("the LIST lost the callsign to a hash (<...>)", hashed, lines);
        Row("no shape this library builds accepts it", noShape, lines);
        Row("packed, and came back as different text", noRoundTrip, lines);
        Row("TOTAL", representable + hashed + noShape + noRoundTrip, lines);

        _output.WriteLine(string.Empty);
        foreach (var (outcome, sample) in examples.OrderBy(p => p.Key.ToString(), StringComparer.Ordinal))
        {
            _output.WriteLine($"  the first few of: {outcome}");
            foreach (var line in sample)
            {
                _output.WriteLine($"    {line}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE CEILING, IN ONE SENTENCE. The highest score criterion 3 could reach with a");
        _output.WriteLine($"  perfect receiver, given this library's message layer as it stands tonight, is");
        _output.WriteLine($"  {representable} OF {lines} - {100.0 * representable / lines:F1} PER CENT.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  AND LAST NIGHT'S RESULT AGAINST IT, BOTH READINGS, NEITHER ALONE:");
        _output.WriteLine($"    760 of {lines} expected lines               = {100.0 * 760 / lines:F1} per cent");
        _output.WriteLine($"    760 of a ceiling of {representable}                = "
            + $"{100.0 * 760 / Math.Max(1, representable):F1} per cent");
        _output.WriteLine($"    the gap the receiver can still close     = {representable - 760} lines");
        _output.WriteLine($"    the gap no receiver can close            = {lines - representable} lines");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  A CEILING BELOW 1298 IS NOT AN EXCUSE AND IS NOT USED AS ONE. It says where the");
        _output.WriteLine("  remaining work lives, and it says that one named part of the shortfall sits in a");
        _output.WriteLine("  decision the owner holds - whether a decoder may print a station it cannot name -");
        _output.WriteLine("  rather than in any code this phase is allowed to change.");

        // Exhaustive by construction: every line lands in exactly one of the four.
        Assert.Equal(lines, representable + hashed + noShape + noRoundTrip);
        Assert.True(representable > 0, "this library could not represent a single expected line.");

        void Row(string label, int count, int total) =>
            _output.WriteLine($"{label,-46} {count,6} {100.0 * count / Math.Max(1, total),7:F1}%");
    }

    /// <summary>
    /// <b>The ceiling is a property of the message layer and it is proved on the corpus rather than
    /// asserted from the recordings.</b> Every message type this library builds packs and reads back
    /// through the same instrument the ceiling is measured with.
    /// </summary>
    /// <remarks>
    /// <b>Without this the ceiling could be an artifact of a weak packer.</b> A ceiling measured by
    /// an instrument that quietly refuses good text would read low for the wrong reason, and the
    /// report would then blame the message layer for the instrument. This runs step 2's own closing
    /// corpus through it and requires every text form to come back.
    /// </remarks>
    [Fact]
    public void TheInstrumentTheCeilingIsMeasuredWithReadsBackEveryTypeThisLibraryBuilds()
    {
        var texts = new List<string>();
        var message = new byte[Ft8Payload.MessageBytes];

        // Standard messages: a CQ, a lettered CQ, a directed call with a grid, one with a report,
        // one with each of the three tokens, and both suffix forms.
        foreach (var text in new[]
        {
            "CQ K1ABC FN42",
            "CQ DX K1ABC FN42",
            "CQ TEST K1ABC FN42",
            "K1ABC W9XYZ FN42",
            "K1ABC W9XYZ -11",
            "K1ABC W9XYZ R-09",
            "K1ABC W9XYZ RRR",
            "K1ABC W9XYZ RR73",
            "K1ABC W9XYZ 73",
            "K1ABC/R W9XYZ/R FN42",
            "K1ABC/P W9XYZ/P FN42",
            "DE K1ABC FN42",
            "QRZ K1ABC FN42",
        })
        {
            texts.Add(text);
        }

        // Free text and telemetry, which are the two shapes that are not fields at all.
        // Thirteen characters is free text's whole width, so both of these sit inside it. A
        // fourteenth character is not a defect of anything and is not offered here as one.
        texts.Add("HELLO WORLD");
        texts.Add("TNX FER QSO 7");
        texts.Add("0123456789ABCDEF01");

        var readBack = 0;
        foreach (var text in texts)
        {
            var outcome = ExpectedMessagePacker.TryPack(text, out message);
            _output.WriteLine($"  {(outcome == ExpectedMessagePacker.PackFailure.None ? "ok    " : "REFUSED")} "
                + $"{text,-24} {(outcome == ExpectedMessagePacker.PackFailure.None ? string.Empty : outcome.ToString())}");

            if (outcome == ExpectedMessagePacker.PackFailure.None)
            {
                readBack++;
                Assert.Equal(text, Ft8MessageDecoder.Decode(message).Text.Trim());
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {readBack} of {texts.Count} text forms pack and read back as themselves.");
        _output.WriteLine("  So a line the ceiling counts as unrepresentable is a real limit of the message");
        _output.WriteLine("  layer and not an artifact of the instrument measuring it.");

        Assert.Equal(texts.Count, readBack);
    }

    private static int Count(
        IReadOnlyDictionary<ExpectedMessagePacker.PackFailure, int> counts,
        ExpectedMessagePacker.PackFailure outcome) =>
        counts.TryGetValue(outcome, out var value) ? value : 0;
}
