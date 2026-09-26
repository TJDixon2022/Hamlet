using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-071: a run of elements sent with no character gap in it that
/// matches a prosign is emitted as one symbol, never split into letters.
/// </summary>
/// <remarks>
/// <para>**THE KEY IS THE PATTERN M.1677-1 GIVES, WRITTEN HERE, AND NOT
/// <see cref="MorseAlphabet"/>'S** (CLAUDE.md 12.5, work instruction 455). Each
/// prosign of `CW_SPEC.md` 6.2 is listed with its elements as the Recommendation
/// and the ARRL list send them, and the generator's keying of the same run is
/// checked against that pattern before anything is decoded, so a fixture that
/// shared the decoder's misunderstanding would fail on its own key.</para>
/// <para>**EACH SEND IS `prosigns-18wpm`'S OWN SHAPE**, `W1AW DE K2ABC ^XX R TU
/// ^XX` at eighteen words a minute over the same quiet band, with the prosign in
/// the middle and at the end, so the detector has acquired before the first one
/// arrives and the second is the sign-off an operator actually hears.</para>
/// <para>**AND ONE SEND THAT MUST NOT MERGE**: the letters `A` and `R` with a
/// real character gap between them, which has to come back as two letters, so
/// nothing here can pass by reading every long run as a prosign.</para>
/// </remarks>
public sealed class TheProsignArrivesAsOneSymbolTests
{
    /// <summary>The speed and band of `prosigns-18wpm`.</summary>
    private const int WordsPerMinute = 18;

    private const double Band = 0.02;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each send's reading is printed.</param>
    public TheProsignArrivesAsOneSymbolTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The prosigns of `CW_SPEC.md` 6.2, each with its M.1677-1 or ARRL pattern.
    /// The error signal is HM-REQ-073's and is not here.
    /// </summary>
    public static IReadOnlyList<(string Name, string Pattern)> Prosigns { get; } = new[]
    {
        ("VE", "...-."),
        ("CT", "-.-.-"),
        ("SK", "...-.-"),
        ("AS", ".-..."),
        ("KN", "-.--."),
        ("BK", "-...-.-"),
        ("CL", "-.-..-.."),
        ("BT", "-...-"),
        ("AR", ".-.-."),
    };

    /// <summary>The same list, for a theory.</summary>
    public static TheoryData<string> Names
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var (name, _) in Prosigns)
            {
                data.Add(name);
            }

            return data;
        }
    }

    /// <summary>
    /// HM-REQ-071: the prosign, sent as one run, comes back as that prosign at
    /// both places it was sent, with nothing added either side of it.
    /// </summary>
    /// <param name="name">The prosign.</param>
    /// <remarks>
    /// **ALL NINE OF `CW_SPEC.md` 6.2, NOT ONLY THE ONES THE TABLE HOLDS.** The
    /// requirement is written against the specification's table, and a prosign
    /// the decoder's table lacks comes back as a placeholder. That is red here on
    /// purpose, and it is 6.1's to fix from cited data (HM-REQ-070), never by a
    /// constant typed in to turn this green.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Names))]
    public void EachProsignArrivesAsOneSymbol(string name)
    {
        var send = Send(name);
        var spans = Spans(send, Message(name), name);

        _output.WriteLine($"HM-REQ-071 | {name} | read `{send.Text}` | at the prosign {string.Join(" and ", spans.Select(s => $"`{s}`"))}");

        Assert.Equal(new[] { $"<{name}>", $"<{name}>" }, spans);
    }

    /// <summary>
    /// HM-REQ-071's other side: `A` and `R` with a real character gap between
    /// them are two letters, so the test above cannot pass by merging runs.
    /// </summary>
    [Fact]
    public void ARealCharacterGapKeepsTheLettersApart()
    {
        var letters = CwDecodeHarness.Decode(new CwSignalRequest(
            "W1AW DE K2ABC AR R TU AR", WordsPerMinute, NoiseAmplitude: Band));

        _output.WriteLine($"HM-REQ-071 | A then R | read `{letters.Text}`");

        Assert.DoesNotContain("<AR>", letters.Text, StringComparison.Ordinal);
        Assert.Equal("W1AW DE K2ABC AR R TU AR", letters.Text);
    }

    /// <summary>
    /// The trace of work instruction 455 task 1, which asserts nothing: each
    /// prosign's pattern, what the table returns for it, what `prosigns-18wpm`
    /// gives back where it carries it, and what a send of it reads as.
    /// </summary>
    [Fact]
    public void EachProsignAsTheTableAndTheDecoderReadIt()
    {
        var fixture = CwFixtures.All.Single(f => f.Name == "prosigns-18wpm");
        var onDisk = CwDecodeHarness.Decode(CwFixtures.Read(fixture));

        _output.WriteLine($"fixture | prosigns-18wpm | sent `{fixture.Sent}` | read `{onDisk.Text}`");
        _output.WriteLine("prosign | pattern | keyed as the pattern | MorseAlphabet | on prosigns-18wpm | on its own send | one symbol or split");

        foreach (var (name, pattern) in Prosigns)
        {
            var table = MorseAlphabet.Lookup(pattern) ?? "not in the table";
            var inFixture = fixture.Sent.Contains("^" + name, StringComparison.Ordinal)
                ? string.Join(" and ", Spans(onDisk, fixture.Sent, name).Select(s => $"`{s}`"))
                : "not in the fixture";
            var send = Send(name);
            var spans = Spans(send, Message(name), name);

            _output.WriteLine(
                $"prosign | {name} | {pattern} | {KeyedAsPattern(name, pattern)} | {table} | {inFixture} | "
                + $"{string.Join(" and ", spans.Select(s => $"`{s}`"))} of `{send.Text}` | {Verdict(spans, name)}");
        }

        var letters = CwDecodeHarness.Decode(new CwSignalRequest(
            "W1AW DE K2ABC AR R TU AR", WordsPerMinute, NoiseAmplitude: Band));

        _output.WriteLine($"gapped | A then R with a character gap | read `{letters.Text}`");
        _output.WriteLine("prosigns in MorseAlphabet: " + string.Join(", ",
            MorseAlphabet.All.Where(p => p.Value.StartsWith('<')).Select(p => $"{p.Value} {p.Key}")));
    }

    /// <summary>
    /// The trace's second half, which asserts nothing: every prosign the real
    /// keyed recordings' keys carry, and every prosign their decodes emit, with
    /// what stood on the other side of the alignment.
    /// </summary>
    [Fact]
    public void EveryProsignTheRealKeysCarry()
    {
        var found = 0;

        foreach (var m in TheRequirementsAreMeasuredTests.Real)
        {
            foreach (var stretch in m.Stretches)
            {
                var steps = stretch.Steps;

                for (var i = 0; i < steps.Count; i++)
                {
                    var key = steps[i].Key;
                    var read = steps[i].Decoded?.Text;

                    if (!IsProsign(key) && !IsProsign(read))
                    {
                        continue;
                    }

                    found++;

                    var from = Math.Max(0, i - 3);
                    var to = Math.Min(steps.Count, i + 4);
                    var keyAround = string.Concat(steps.Skip(from).Take(to - from).Select(s => s.Key ?? "_"));
                    var readAround = string.Concat(steps.Skip(from).Take(to - from).Select(s => s.Decoded?.Text ?? "_"));

                    _output.WriteLine(
                        $"real | {m.Name} | key {key ?? "nothing"} | emitted {read ?? "nothing"} | "
                        + $"{steps[i].Decoded?.Class.ToString() ?? "-"} | key around `{keyAround}` | read around `{readAround}`");
                }
            }
        }

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"real | {found} prosign steps over {TheRequirementsAreMeasuredTests.Real.Count} keyed recordings"));
    }

    private static bool IsProsign(string? text)
        => text is not null && (text.StartsWith('<') || text is "=" or "+");

    /// <summary>The message a prosign is sent in.</summary>
    private static string Message(string name) => $"W1AW DE K2ABC ^{name} R TU ^{name}";

    /// <summary>One prosign's send, decoded.</summary>
    private static CwDecodeResult Send(string name)
        => CwDecodeHarness.Decode(new CwSignalRequest(Message(name), WordsPerMinute, NoiseAmplitude: Band));

    /// <summary>Whether the generator keys `^name` as exactly the pattern, one unit between elements.</summary>
    private static string KeyedAsPattern(string name, string pattern)
    {
        var expected = new List<int>();

        foreach (var element in pattern)
        {
            if (expected.Count > 0)
            {
                expected.Add(1);
            }

            expected.Add(element == '-' ? 3 : 1);
        }

        return MorseCode.KeyPattern("^" + name).SequenceEqual(expected) ? "yes" : "NO";
    }

    /// <summary>
    /// What was emitted at each place the key has the prosign: the aligned
    /// symbol and anything the decode added either side of it.
    /// </summary>
    private static IReadOnlyList<string> Spans(CwDecodeResult result, string key, string name)
    {
        var alignment = CwMetrics.Align(CwMetrics.Symbols(result.Characters), key, CwKeyKind.Exact);
        var steps = alignment.Steps;
        var spans = new List<string>();
        var symbol = $"<{name}>";

        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i].Key != symbol)
            {
                continue;
            }

            var from = i;
            var to = i;

            while (from > 0 && steps[from - 1].Key is null)
            {
                from--;
            }

            while (to + 1 < steps.Count && steps[to + 1].Key is null)
            {
                to++;
            }

            var read = steps.Skip(from).Take(to - from + 1)
                .Where(s => s.Decoded is not null)
                .Select(s => s.Decoded!.Value.Text)
                .ToList();

            spans.Add(read.Count == 0 ? "nothing" : string.Join("|", read));
        }

        return spans;
    }

    private static string Verdict(IReadOnlyList<string> spans, string name)
        => spans.All(s => s == $"<{name}>") ? "one symbol"
            : spans.Any(s => s.Contains('|', StringComparison.Ordinal)) ? "split"
            : "one symbol, not the prosign";
}
