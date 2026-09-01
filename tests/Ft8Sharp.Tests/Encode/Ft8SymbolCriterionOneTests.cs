using Ft8Sharp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Ldpc;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Step 3's first exit criterion, re-taken from message text rather than inherited from step 1's
/// basis proof: real messages, every type this library builds, through the packer, the CRC, the
/// 91-bit payload, the LDPC encoder and all 83 parity checks.
/// </summary>
/// <remarks>
/// <para>
/// <b>Which reading of "matches the reference" this stands on, stated so a reader does not assume
/// the stronger one.</b> There are two. The stronger is a byte-for-byte comparison of the codeword
/// against the one <c>ft8_lib</c> produces for the same message; that needs the reference built,
/// and unit 209 could not build it — there is no C toolchain on this machine. <b>What runs here is
/// the weaker reading: the syndrome check against the checked-in parity tables</b>, computed by
/// <c>LdpcCheck</c>, which is an independent implementation in the test project that shares no
/// code with <c>LdpcEncoder</c>. Every one of the 83 checks must be satisfied.
/// </para>
/// <para>
/// <b>Why it is still worth taking.</b> Step 1 proved the parity tables by linearity over the 91
/// basis payloads — a proof about the tables and about every payload, and a strong one. What it did
/// not do is exercise the chain a real message travels: pack, CRC, payload, encode. That chain is
/// what this runs, and its value is that a defect anywhere in the message layer shows up as a
/// codeword that does not satisfy its own parity.
/// </para>
/// <para>
/// <b>Step 1's proof is not touched.</b> <c>Ft8LdpcParityTests</c>, <c>BasisProof</c> and
/// <c>Payloads</c> are load-bearing and are left exactly as they are. This is added beside them.
/// </para>
/// </remarks>
public class Ft8SymbolCriterionOneTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SymbolCriterionOneTests(ITestOutputHelper output) => _output = output;

    /// <summary>The seed, stated so the run is repeatable and the count means something.</summary>
    private const int Seed = 20901;

    [Fact]
    public void EveryRealMessageProducesACodewordThatClearsAllEightyThreeChecks()
    {
        var messages = BuildCorpus();
        var byKind = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var failures = 0;
        var firstFailure = string.Empty;
        var checksRun = 0;

        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        foreach (var (kind, message) in messages)
        {
            byKind[kind] = byKind.GetValueOrDefault(kind) + 1;

            Ft8Payload.Create(message, payload);
            LdpcEncoder.Encode(payload, codeword);

            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);

            // Both readings of the parity tables, over every one of the 83 checks.
            var fromNm = LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);
            var fromMn = LdpcCheck.SyndromeFromMn(bits, Ft8Tables.LdpcMn);
            checksRun += fromNm.Length + fromMn.Length;

            var failing = LdpcCheck.FailingCount(fromNm);
            if (failing != 0 || LdpcCheck.FailingCount(fromMn) != 0)
            {
                failures++;
                if (firstFailure.Length == 0)
                {
                    firstFailure =
                        $"a {kind} message left {failing} of {Ft8Tables.LdpcM} checks unsatisfied; "
                        + $"first failing check {LdpcCheck.FailingChecks(fromNm).FirstOrDefault()}";
                }
            }

            // And the codeword lays out into a whole transmission, which is the tie between this
            // criterion and the one this unit exists for.
            Assert.Equal(Ft8SymbolEncoder.SymbolCount, Ft8SymbolEncoder.Encode(message).Length);
        }

        _output.WriteLine($"seed                                          : {Seed}");
        _output.WriteLine($"real messages through pack, CRC, payload, encode : {messages.Count}");
        foreach (var (kind, count) in byKind)
        {
            _output.WriteLine($"    {kind,-34} : {count}");
        }

        _output.WriteLine($"parity checks run (both table readings)       : {checksRun}");
        _output.WriteLine($"messages failing any of the {Ft8Tables.LdpcM} checks        : {failures}");
        _output.WriteLine(
            "READING STOOD ON: the syndrome check against the checked-in parity tables, computed "
            + "by the independent LdpcCheck. NOT a byte-for-byte comparison against ft8_lib's own "
            + "codeword. Unit 209 recorded that as 'the reference could not be built on this "
            + "machine', which unit 210 found to be no longer the whole truth: the reference IS "
            + "built here and it will not run, so the stronger reading is still out of reach for a "
            + "different reason. Ft8OracleDiagnosisTests holds the measurement.");

        Assert.True(failures == 0, firstFailure);

        // Every type this library builds has to be in it, and in quantity — a criterion taken over
        // a corpus that happens to be all one type is not the criterion.
        Assert.True(messages.Count > 1000, $"the corpus is only {messages.Count} messages.");
        Assert.Equal(6, byKind.Count);
        Assert.All(byKind, kind => Assert.True(
            kind.Value >= 100,
            $"only {kind.Value} messages of kind '{kind.Key}' packed, which is too few to say the "
            + "type was covered."));
    }

    /// <summary>
    /// The parity check is watched refusing, because a check that has never failed says nothing
    /// about the run where it passed.
    /// </summary>
    [Fact]
    public void TheParityCheckIsWatchedRefusingACorruptedCodeword()
    {
        var message = EncodeCorpus.Build()[0].Message;
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        var clean = LdpcCheck.FailingCount(
            LdpcCheck.SyndromeFromNm(
                LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN),
                Ft8Tables.LdpcNm,
                Ft8Tables.LdpcNumRows));
        Assert.Equal(0, clean);

        // Flip one bit of the codeword. A column-weight-three code must see exactly three checks
        // fail, which is a sharper statement than "some check failed".
        var disturbed = new List<int>();
        for (var bit = 0; bit < Ft8Tables.LdpcN; bit++)
        {
            var corrupted = (byte[])codeword.Clone();
            corrupted[bit / 8] ^= (byte)(0x80 >> (bit % 8));
            var failing = LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(
                    LdpcCheck.UnpackMsbFirst(corrupted, Ft8Tables.LdpcN),
                    Ft8Tables.LdpcNm,
                    Ft8Tables.LdpcNumRows));
            disturbed.Add(failing);
        }

        _output.WriteLine($"every one of the {Ft8Tables.LdpcN} single-bit flips was caught");
        _output.WriteLine($"checks disturbed per flip: min {disturbed.Min()}, max {disturbed.Max()}");
        Assert.All(disturbed, failing => Assert.True(failing > 0));
        Assert.Equal(Ft8Tables.LdpcMnRowWidth, disturbed.Min());
        Assert.Equal(Ft8Tables.LdpcMnRowWidth, disturbed.Max());
    }

    /// <summary>
    /// Real messages of every type this library builds. Generated from a seed so the count is
    /// repeatable and so no expected value is committed anywhere.
    /// </summary>
    private static List<(string Kind, byte[] Message)> BuildCorpus()
    {
        var random = new Random(Seed);
        var corpus = new List<(string, byte[])>();
        var calls = CallsignCorpus.Distinct(Seed, 400).Where(c => c.Length >= 3).ToList();

        string[] reports = { "-15", "-11", "-01", "+03", "R-09", "R+05", "RRR", "RR73", "73" };
        string[] grids = { "FN42", "IO91", "JN58", "KM72", "PM95", "EM12", "DM04" };
        string[] letteredCq = { "CQ", "CQ DX", "CQ NA", "CQ POTA" };

        // Standard messages: CQs with and without a grid, and exchanges with every report token.
        // Generated callsigns are drawn from the full range of shapes, so a good share of them are
        // non-standard and are refused by this message type; the counts below are attempts rather
        // than messages, and the test reports what actually packed.
        for (var i = 0; i < 900; i++)
        {
            var to = letteredCq[random.Next(letteredCq.Length)];
            var de = calls[random.Next(calls.Count)];
            var extra = random.Next(4) == 0 ? string.Empty : grids[random.Next(grids.Length)];
            Add(corpus, "standard, CQ", m => Ft8StandardMessage.TryPack(to, de, extra, m));
        }

        for (var i = 0; i < 1400; i++)
        {
            var to = calls[random.Next(calls.Count)];
            var de = calls[random.Next(calls.Count)];
            var extra = random.Next(3) == 0
                ? grids[random.Next(grids.Length)]
                : reports[random.Next(reports.Length)];
            Add(corpus, "standard, exchange", m => Ft8StandardMessage.TryPack(to, de, extra, m));
        }

        // Free text, at every width the field carries.
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 +-./?";
        for (var i = 0; i < 200; i++)
        {
            var length = 1 + random.Next(13);
            var text = new string(Enumerable
                .Range(0, length)
                .Select(_ => alphabet[random.Next(alphabet.Length)])
                .ToArray())
                .Trim();
            if (text.Length == 0)
            {
                text = "CQ";
            }

            Add(corpus, "free text", m => Ft8FreeText.TryPackText(text, m));
        }

        // Telemetry.
        for (var i = 0; i < 200; i++)
        {
            var bytes = new byte[9];
            random.NextBytes(bytes);
            bytes[0] &= 0x0F; // the field is 71 bits, not 72
            Add(corpus, "telemetry", m => Ft8FreeText.TryPackTelemetry(bytes, m));
        }

        // Non-standard callsigns, both legs: the call spelled out, and a companion hashed through
        // a warm cache. The second is unit 208's carried-forward item.
        for (var i = 0; i < 200; i++)
        {
            var call = $"{(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}{random.Next(10)}/"
                       + calls[random.Next(calls.Count)];
            if (call.Length > 11)
            {
                continue;
            }

            Add(corpus, "non-standard, call in full", m => Ft8NonstandardMessage.TryPack("CQ", call, string.Empty, null, m));

            var cache = new Ft8CallsignCache();
            cache.Save(call);
            var companion = calls[random.Next(calls.Count)];
            Add(
                corpus,
                "non-standard, hashed companion",
                m => Ft8NonstandardMessage.TryPack(call, companion, string.Empty, cache, m));
        }

        return corpus;
    }

    /// <summary>Adds a message if it packed, and silently skips one the library refuses.</summary>
    /// <remarks>
    /// A refusal is not a failure here — it is the packer doing its job on a combination this
    /// message type cannot carry, and unit 207's refusal tests are what assert that behaviour. What
    /// this corpus needs is messages that <em>did</em> pack, and it says how many it got.
    /// </remarks>
    private static void Add(
        List<(string, byte[])> corpus,
        string kind,
        Func<byte[], Ft8PackResult> pack)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        if (pack(message) == Ft8PackResult.Ok)
        {
            corpus.Add((kind, message));
        }
    }
}
