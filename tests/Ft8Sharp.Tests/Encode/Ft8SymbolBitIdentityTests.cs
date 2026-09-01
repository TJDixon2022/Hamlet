using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Step 3's second exit criterion: the symbol sequence is bit-identical to <c>ft8_lib</c>'s for the
/// same message. Leg C, and the one comparison in this phase that agreeing with ourselves cannot
/// fake.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this and nothing else settles it.</b> The port's tones can be the right length, entirely
/// inside the eight-tone alphabet, with all three Costas blocks at their measured indices, and
/// still be wrong in two ways nothing inside this library can see: the Gray map run backwards, and
/// the codeword bit walk restarted at each sync block instead of continuing across it. Both are
/// expression-anchored readings taken from inside upstream's own function body. Only upstream's own
/// output decides them.
/// </para>
/// <para>
/// <b>Nothing upstream produces is committed.</b> The tones are read at run time, compared, and
/// dropped. Whether they matched is recorded; what they were is not.
/// </para>
/// <para>
/// <b>It skips when the oracle cannot answer, and a skip here is not good news.</b> On a machine
/// with no clone and no build that is expected and correct. On a machine that has them, a skip
/// means the oracle is broken and the reason says how — which is exactly what happened when this
/// was written.
/// </para>
/// </remarks>
public class Ft8SymbolBitIdentityTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SymbolBitIdentityTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Every symbol of every message this library can put into words, against upstream's own tones.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void EverySymbolOfEveryMessageIsIdenticalToUpstreams()
    {
        var corpus = EncodeCorpus.Build();
        var comparable = corpus.Where(entry => entry.Text is not null).ToList();

        _output.WriteLine($"corpus            : {corpus.Count} messages");
        _output.WriteLine($"with a text form  : {comparable.Count}");
        _output.WriteLine($"telemetry omitted : {corpus.Count - comparable.Count} (no text form exists)");
        _output.WriteLine(string.Empty);

        var compared = 0;
        var matching = 0;
        var failures = new List<string>();

        foreach (var entry in comparable)
        {
            var run = Ft8Oracle.Generate(entry.Text!);
            if (run.ExitCode != 0)
            {
                failures.Add(
                    $"[{entry.Label}] upstream exited {run.ExitCode} (0x{run.ExitCode:X8}) rather "
                    + "than encoding the message");
                continue;
            }

            if (!Ft8Oracle.TryReadTones(run.StandardOutput, Ft8SymbolEncoder.SymbolCount, out var theirs))
            {
                failures.Add(
                    $"[{entry.Label}] upstream ran and printed no tone sequence this parser could read");
                continue;
            }

            var ours = Ft8SymbolEncoder.Encode(entry.Message);
            var result = SymbolComparison.Compare(ours, theirs);
            compared++;

            if (result.Identical)
            {
                matching++;
                _output.WriteLine($"  match   [{entry.Kind}] {entry.Label}: {result.Compared} symbols");
                continue;
            }

            _output.WriteLine($"  DIFFER  [{entry.Kind}] {entry.Label}: {result.Explanation}");
            failures.Add(
                $"[{entry.Label}] ({entry.Kind}) message {comparable.IndexOf(entry)} of "
                + $"{comparable.Count}: {result.Explanation}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"messages compared : {compared}");
        _output.WriteLine($"matching symbol for symbol: {matching}");

        Assert.True(
            failures.Count == 0,
            "the symbol sequence is not bit-identical to ft8_lib's:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, failures));

        Assert.Equal(comparable.Count, compared);
        Assert.Equal(compared, matching);
    }

    /// <summary>
    /// Unit 208's carried-forward debt, named and measured on its own: a message whose callsign
    /// travels as a hash rather than in full.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A comparison covering only standard messages with basecalls in them passes whatever the hash
    /// does, because no hash will have been on the wire. This leg is reported separately for that
    /// reason and must not be folded into the corpus total.
    /// </para>
    /// <para>
    /// <b>What counts is a hash on the wire on BOTH sides.</b> The corpus carries hashed entries of
    /// two kinds and only one of them upstream can be asked for — see the remarks on
    /// <see cref="EncodeCorpus"/>. The entries with no text form are named here as not covered rather
    /// than left out silently, because a leg quietly dropped reads exactly like a leg that passed.
    /// </para>
    /// </remarks>
    [RequiresWorkingOracleFact]
    public void AMessageWhoseCallsignTravelsAsAHashIsCompared()
    {
        var hashed = EncodeCorpus.Build().Where(e => e.CarriesHashedCallsign).ToList();
        var comparable = hashed.Where(e => e.Text is not null).ToList();

        _output.WriteLine($"entries carrying a hash on the wire : {hashed.Count}");
        _output.WriteLine($"of those, upstream can be asked for : {comparable.Count}");
        foreach (var entry in hashed.Where(e => e.Text is null))
        {
            _output.WriteLine(
                $"  NOT COVERED [{entry.Kind}] {entry.Label}: no text form makes upstream produce "
                + "this wire format, so it cannot be compared and is not counted as covered");
        }

        _output.WriteLine(string.Empty);

        Assert.True(
            comparable.Count > 0,
            "no message whose callsign travels as a hash can be put to upstream at all, so this leg "
            + "is NOT COVERED and the hash still stands on two legs");

        foreach (var entry in comparable)
        {
            var run = Ft8Oracle.Generate(entry.Text!);
            Assert.Equal(0, run.ExitCode);

            var read = Ft8Oracle.TryReadTones(run.StandardOutput, Ft8SymbolEncoder.SymbolCount, out var theirs);
            Assert.True(
                read,
                $"[{entry.Label}] upstream would not give a tone sequence for a message carrying a "
                + "hashed callsign, so this leg is NOT COVERED and the hash still stands on two legs");

            var ours = Ft8SymbolEncoder.Encode(entry.Message);
            var result = SymbolComparison.Compare(ours, theirs);

            _output.WriteLine($"  [{entry.Kind}] {entry.Label}: {result.Explanation}");
            Assert.True(result.Identical, $"[{entry.Label}] the hashed-callsign leg differs: {result.Explanation}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"{comparable.Count} messages compared with a callsign genuinely on the wire as a hash, "
            + "and every symbol of every one of them is identical to upstream's. Unit 208's debt is "
            + "settled for this form.");
    }

    /// <summary>
    /// The comparison watched refusing. A comparison that has never failed is not a comparison.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the same reasoning that makes step 1's boundary test show itself refusing a
    /// reference rather than merely passing. It deliberately needs no oracle: the comparator is the
    /// thing under test, and on a machine where the oracle is absent or broken this is the only
    /// evidence that the comparison would have caught anything at all.
    /// </para>
    /// <para>
    /// One symbol is altered, at a position chosen to be a data symbol rather than a sync symbol,
    /// and the comparator is required to name that exact position — not to report a count.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheComparisonNamesThePositionWhenOneSymbolIsAltered()
    {
        var entry = EncodeCorpus.Build().First();
        var ours = Ft8SymbolEncoder.Encode(entry.Message);

        // A data symbol, not a sync symbol: the first position after the opening Costas block.
        const int altered = 7;
        Assert.False(Ft8SymbolEncoder.IsSyncSymbol(altered), "position 7 was expected to carry data");

        var theirs = (byte[])ours.Clone();
        theirs[altered] = (byte)((theirs[altered] + 1) % Ft8SymbolEncoder.ToneCount);

        var result = SymbolComparison.Compare(ours, theirs);

        _output.WriteLine($"altered position : {altered}");
        _output.WriteLine($"comparator says  : {result.Explanation}");

        Assert.False(result.Identical, "the comparator passed a sequence that had been altered");
        Assert.Equal(altered, result.FirstDifference);
        Assert.Equal(1, result.DifferenceCount);
        Assert.Contains("data symbol", result.Explanation, StringComparison.Ordinal);
    }

    /// <summary>
    /// And refusing inside a sync block, because the two faults are different and the comparator is
    /// required to tell a reader which one they are looking at.
    /// </summary>
    [Fact]
    public void TheComparisonSaysWhenTheDifferenceIsInsideASyncBlock()
    {
        var entry = EncodeCorpus.Build().First();
        var ours = Ft8SymbolEncoder.Encode(entry.Message);

        const int altered = 38;
        Assert.True(Ft8SymbolEncoder.IsSyncSymbol(altered), "position 38 was expected to be a sync symbol");

        var theirs = (byte[])ours.Clone();
        theirs[altered] = (byte)((theirs[altered] + 1) % Ft8SymbolEncoder.ToneCount);

        var result = SymbolComparison.Compare(ours, theirs);

        _output.WriteLine($"altered position : {altered}");
        _output.WriteLine($"comparator says  : {result.Explanation}");

        Assert.False(result.Identical);
        Assert.Equal(altered, result.FirstDifference);
        Assert.Contains("sync block 1", result.Explanation, StringComparison.Ordinal);
    }

    /// <summary>
    /// A sequence of the wrong length is refused rather than compared over the shorter of the two,
    /// which would report agreement over a prefix as agreement.
    /// </summary>
    [Fact]
    public void TheComparisonRefusesTwoSequencesOfDifferentLengths()
    {
        var entry = EncodeCorpus.Build().First();
        var ours = Ft8SymbolEncoder.Encode(entry.Message);
        var truncated = ours[..^1];

        var result = SymbolComparison.Compare(ours, truncated);

        _output.WriteLine($"comparator says : {result.Explanation}");

        Assert.False(result.Identical);
        Assert.Equal(0, result.Compared);
    }

    /// <summary>
    /// And it agrees when it should, so the refusals above are not a comparator that refuses
    /// everything.
    /// </summary>
    [Fact]
    public void TheComparisonAgreesWithAnUnalteredSequence()
    {
        foreach (var entry in EncodeCorpus.Build())
        {
            var ours = Ft8SymbolEncoder.Encode(entry.Message);
            var again = Ft8SymbolEncoder.Encode(entry.Message);

            var result = SymbolComparison.Compare(ours, again);
            Assert.True(result.Identical, $"[{entry.Label}] {result.Explanation}");
            Assert.Equal(Ft8SymbolEncoder.SymbolCount, result.Compared);
        }
    }

    /// <summary>
    /// The tone parser watched refusing, because a parser that scavenges digits out of any line
    /// would read a frequency or a duration as tones and then report an agreement it never made.
    /// </summary>
    [Fact]
    public void TheToneParserRefusesLinesThatAreNotToneSequences()
    {
        const int n = 79;

        Assert.False(Ft8Oracle.TryReadTones("Generate a 15-second WAV file encoding a message.", n, out _));
        Assert.False(Ft8Oracle.TryReadTones(string.Empty, n, out _));

        // The right count of numbers, but one is outside the eight-tone alphabet.
        var almost = string.Join(' ', Enumerable.Repeat("3", n - 1).Append("8"));
        Assert.False(Ft8Oracle.TryReadTones(almost, n, out _));

        // The right values, but too few of them.
        var tooFew = string.Join(' ', Enumerable.Repeat("3", n - 1));
        Assert.False(Ft8Oracle.TryReadTones(tooFew, n, out _));

        // And one it must accept, so the refusals above are not a parser that refuses everything.
        var good = string.Join(' ', Enumerable.Repeat("5", n));
        Assert.True(Ft8Oracle.TryReadTones($"header line\n{good}\ntrailer", n, out var tones));
        Assert.Equal(n, tones.Length);

        // The run-together form upstream actually uses, watched refusing on the same three faults
        // before it is trusted to accept anything. It was added when the generator was first made to
        // survive its own waveform and print, and a second accepted form is a second way to read
        // agreement that was never measured unless it is held to the same standard as the first.
        var run = new string('5', n);
        Assert.False(Ft8Oracle.TryReadTones($"FT8 tones: {run[..^1]}", n, out _));
        Assert.False(Ft8Oracle.TryReadTones($"FT8 tones: {run}5", n, out _));
        Assert.False(Ft8Oracle.TryReadTones($"FT8 tones: {run[..^1]}8", n, out _));

        // A long run of decimal digits that is not tones — the right length, wrong alphabet.
        Assert.False(Ft8Oracle.TryReadTones($"checksum: {new string('9', n)}", n, out _));

        Assert.True(Ft8Oracle.TryReadTones($"label: {run}", n, out var packed));
        Assert.Equal(n, packed.Length);
        Assert.All(packed, tone => Assert.Equal(5, tone));
    }

    /// <summary>
    /// The hex reader watched refusing, since it is what upgrades criterion 1 from a syndrome check
    /// against our own tables to a byte-for-byte comparison against upstream's own bits.
    /// </summary>
    /// <remarks>
    /// A reader that accepted anything after a label would compare our packed message against
    /// whatever happened to sit on that line, and report a match nobody measured.
    /// </remarks>
    [Fact]
    public void TheHexReaderRefusesLinesThatCarryNoHexRun()
    {
        Assert.False(Ft8Oracle.TryReadHexAfterLabel("nothing labelled here", "payload", out _));
        Assert.False(Ft8Oracle.TryReadHexAfterLabel("payload: not hex at all", "payload", out _));
        Assert.False(Ft8Oracle.TryReadHexAfterLabel(string.Empty, "payload", out _));

        // A label followed by hex, with the colon upstream puts between them stepped over rather
        // than read as a field and mistaken for the end of the run.
        Assert.True(Ft8Oracle.TryReadHexAfterLabel("Packed data: 00 11 AB cd", "data", out var bytes));
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0xAB, bytes[2]);
        Assert.Equal(0xCD, bytes[3]);
    }
}
