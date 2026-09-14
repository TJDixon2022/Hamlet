using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 358 task 1: **the Olivia calling table and the RSID codes are in the
/// tree, and they are read.**
/// </summary>
/// <remarks>
/// <para>**EVERY VALUE HERE COMES OUT OF A CITED FILE** (`PHASE_PLAN.md` R27, R29,
/// HM-DEC-054). The numbers below are in the test because a test has to say what it
/// expects; they are nowhere under `src`, which reads them from
/// `data/bands/olivia-calling.json` and `data/rsid/rsid-codes.json`.</para>
/// <para>**A FILE THAT CANNOT BE READ IS A SENTENCE, NOT A GUESS** (§0.0). The last test
/// hands the reader a broken copy of each and asserts that no value comes back and that
/// the sentence names the file.</para>
/// </remarks>
public sealed class TheOliviaDataTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheOliviaDataTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Both files parse, as shipped and as they sit in the tree.**</summary>
    [Fact]
    public void BothFilesParse()
    {
        var shipped = OliviaData.Current;

        _output.WriteLine("problem       : " + (shipped.Problem ?? "(none)"));

        Assert.Null(shipped.Problem);
        Assert.NotNull(shipped.Calling);
        Assert.NotNull(shipped.Rsid);

        _output.WriteLine("calling source: " + shipped.Calling!.Source);
        _output.WriteLine("calling rows  : " + shipped.Calling.Rows.Count);
        _output.WriteLine("rsid source   : " + shipped.Rsid!.Source);
        _output.WriteLine("rsid codes    : " + shipped.Rsid.Codes.Count);

        // **THE CITATIONS TRAVEL WITH THE VALUES**, or a row is a number somebody typed.
        Assert.False(string.IsNullOrWhiteSpace(shipped.Calling.Source));
        Assert.False(string.IsNullOrWhiteSpace(shipped.Calling.About));
        Assert.False(string.IsNullOrWhiteSpace(shipped.Rsid.Source));

        // **EIGHT ROWS, THE FILE'S OWN COUNT**, so a row dropped in parsing shows here.
        Assert.Equal(8, shipped.Calling.Rows.Count);

        // **AND THE FILES IN THE TREE ARE THE ONES EMBEDDED**, so an edit to `data/`
        // that the build did not pick up cannot pass unnoticed.
        var root = RepoRoot();
        var fromTree = OliviaData.Read(
            File.ReadAllText(Path.Combine(root, "data", "bands", "olivia-calling.json")),
            File.ReadAllText(Path.Combine(root, "data", "rsid", "rsid-codes.json")));

        Assert.Null(fromTree.Problem);
        Assert.Equal(shipped.Calling.Rows, fromTree.Calling!.Rows);
        Assert.Equal(shipped.Rsid.Codes, fromTree.Rsid!.Codes);
    }

    /// <summary>**The 8/250 code is 69 and BPSK31 is 1.**</summary>
    [Fact]
    public void TheOlivia8250CodeIs69AndBpsk31Is1()
    {
        var rsid = OliviaData.Current.Rsid;

        Assert.NotNull(rsid);

        _output.WriteLine("OLIVIA_8_250  : " + rsid!.CodeOf("OLIVIA_8_250"));
        _output.WriteLine("BPSK31        : " + rsid.CodeOf("BPSK31"));

        Assert.Equal(69, rsid.CodeOf("OLIVIA_8_250"));
        Assert.Equal(1, rsid.CodeOf("BPSK31"));

        // **A NAME THE FILE DOES NOT CARRY HAS NO CODE**, rather than a nearby one.
        Assert.Null(rsid.CodeOf("OLIVIA_64_2000"));

        // **EACH SEQUENCE IS AS LONG AS THE BURST THE FILE DESCRIBES.**
        Assert.All(
            rsid.ToneSequences.Values,
            sequence => Assert.Equal(rsid.Symbols, sequence.Count));
    }

    /// <summary>**20 m's calling center is 14,073,000, and the dial for it comes from the file.**</summary>
    [Fact]
    public void TwentyMetersCallingCenterIs14073000()
    {
        var calling = OliviaData.Current.Calling;

        Assert.NotNull(calling);

        // **THE BAND AS HAMLET SPELLS IT**, "20 m", against the file's "20m".
        var row = calling!.CallingRowFor("20 m");

        Assert.NotNull(row);

        _output.WriteLine(
            "20 m calling  : " + row!.CenterHz + " center, variant " + row.Variant
            + ", dial " + calling.DialHzFor(row));

        Assert.Equal(14_073_000, row.CenterHz);
        Assert.Equal("8/250", row.Variant);

        // **THE DIAL IS THE FILE'S, NOT ARITHMETIC ON A NUMBER TYPED HERE.** The one row
        // that carries both a center and a dial gives the audio center, and every band's
        // dial is its center less that. On 20 m it must come back as the file's own dial.
        Assert.Equal(14_071_500, calling.DialHzFor(row));

        // **AND EVERY BAND HAMLET HAS AN 8/250 ROW FOR ANSWERS FROM THE FILE.**
        foreach (var band in new[] { "80 m", "40 m", "30 m", "17 m", "15 m", "10 m" })
        {
            var here = calling.CallingRowFor(band);

            _output.WriteLine(
                "  " + band.PadRight(6) + (here is null ? "(none)" : here.CenterHz + " dial " + calling.DialHzFor(here)));

            Assert.NotNull(here);
            Assert.Equal("8/250", here!.Variant);
        }

        // **A BAND WITH NO ROW ANSWERS NULL**, and nothing is filled in.
        Assert.Null(calling.CallingRowFor("160 m"));
    }

    /// <summary>**A malformed copy yields the sentence and no value.**</summary>
    [Fact]
    public void AMalformedCopyYieldsTheSentenceAndNoValue()
    {
        var root = RepoRoot();
        var calling = File.ReadAllText(Path.Combine(root, "data", "bands", "olivia-calling.json"));
        var rsid = File.ReadAllText(Path.Combine(root, "data", "rsid", "rsid-codes.json"));

        // **CUT OFF HALF WAY**, which is what a bad copy looks like.
        var brokenCalling = OliviaData.Read(calling[..(calling.Length / 2)], rsid);

        _output.WriteLine("broken calling: " + brokenCalling.Problem);

        Assert.Null(brokenCalling.Calling);
        Assert.NotNull(brokenCalling.Problem);
        Assert.Contains("olivia-calling.json", brokenCalling.Problem, StringComparison.Ordinal);
        Assert.EndsWith(".", brokenCalling.Problem, StringComparison.Ordinal);

        // **A ROW WITH A CENTER THAT IS NOT A NUMBER IS MALFORMED TOO**, not skipped.
        var wrongType = OliviaData.Read(
            calling.Replace("\"center_hz\": 14073000", "\"center_hz\": \"14.073\"", StringComparison.Ordinal),
            rsid);

        _output.WriteLine("wrong type    : " + wrongType.Problem);

        Assert.Null(wrongType.Calling);
        Assert.NotNull(wrongType.Problem);

        var brokenRsid = OliviaData.Read(calling, rsid.Replace("\"OLIVIA_8_250\": 69", "\"OLIVIA_8_250\": \"sixty-nine\"", StringComparison.Ordinal));

        _output.WriteLine("broken rsid   : " + brokenRsid.Problem);

        Assert.Null(brokenRsid.Rsid);
        Assert.NotNull(brokenRsid.Problem);
        Assert.Contains("rsid-codes.json", brokenRsid.Problem, StringComparison.Ordinal);

        // **A MISSING FILE IS SAID, NOT SUBSTITUTED.**
        var missing = OliviaData.Read(null, null);

        _output.WriteLine("missing       : " + missing.Problem);

        Assert.Null(missing.Calling);
        Assert.Null(missing.Rsid);
        Assert.Contains("olivia-calling.json", missing.Problem!, StringComparison.Ordinal);
        Assert.Contains("rsid-codes.json", missing.Problem!, StringComparison.Ordinal);
    }

    private static string RepoRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
