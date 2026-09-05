using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>The four ways a capture fixture must refuse, each watched.</b>
/// </summary>
/// <remarks>
/// <para>
/// <c>PHASE_PLAN.md</c> step 0: <em>a fixture whose named capture is absent, or whose hash does not
/// match, fails loudly rather than passing quietly. A stale fixture silently measures the wrong
/// thing.</em> <b>A refusal that is not tested is a refusal that will not happen</b>, and the failure
/// mode this exit exists to prevent is not a wrong number - it is a skip, a warning, a zero-row
/// result or a silently empty list.
/// </para>
/// <para>
/// <b>Every fixture in this file is built on disk under <see cref="Path.GetTempPath"/> and deleted.</b>
/// Nothing here writes into the repository, and in particular nothing here writes a fixture whose
/// provenance is <c>wsjtx</c> anywhere a later session could find it.
/// </para>
/// </remarks>
public class Ft8CaptureFixtureTests(ITestOutputHelper output) : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-ft8-fixture-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    // ---------------------------------------------------------------- refusal 1

    [Fact]
    public void RefusalOne_TheNamedCaptureIsAbsent()
    {
        var fixture = Ft8CaptureFixture.Read(Write("absent", Text(capture: "not-here.wav")));

        // The fixture itself parses cleanly. It is the capture that is missing, and the two are
        // deliberately different failures at different moments.
        Assert.Equal("not-here.wav", fixture.CaptureName);

        var thrown = Assert.Throws<Ft8FixtureException>(() => fixture.RequireCapture());

        Assert.Contains("not-here.wav", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("is not at", thrown.Message, StringComparison.Ordinal);
        output.WriteLine("REFUSAL 1 - the named capture is absent:");
        output.WriteLine(thrown.Message);
    }

    /// <summary>
    /// <b>Zero fixtures is not the same thing as a fixture whose capture is gone</b>, and this is the
    /// half of that pair that must stay a clean pass.
    /// </summary>
    /// <remarks>
    /// <c>SHACK_FACTS.md</c> FACT-004: the radio lives on a different computer, so
    /// <c>tests/fixtures/ft8/captured/</c> holding no fixtures is the expected state here. A unit that
    /// made this red would have made every session on this machine red for a fact about where the
    /// radio is.
    /// </remarks>
    [Fact]
    public void NoFixturesAtAllIsACleanPass()
    {
        var captured = Ft8CaptureFixtures.PathsIn(Ft8CaptureFixtures.CapturedFolder);

        // Whatever is there, every one of them is readable and its capture is beside it. Today that
        // is none of them, and the assertion below is what starts biting the moment Tim commits one.
        foreach (var path in captured)
        {
            var fixture = Ft8CaptureFixture.Read(path);
            fixture.RequireCapture();
            Assert.Equal(Ft8CaptureFixture.ProvenanceWsjtx, fixture.Provenance);
        }

        output.WriteLine(
            $"{captured.Count} committed fixture(s) in tests/fixtures/ft8/{Ft8CaptureFixtures.CapturedFolder}/. "
            + "Zero is FACT-004's expected state on this machine and is not a defect.");

        // And the folder that is never empty: the worked example is always there.
        Assert.NotEmpty(Ft8CaptureFixtures.PathsIn(Ft8CaptureFixtures.ExampleFolder));
    }

    // ---------------------------------------------------------------- refusal 2

    [Fact]
    public void RefusalTwo_TheCaptureIsThereAndItsHashDoesNotMatch()
    {
        var wav = File.ReadAllBytes(Ft8ExampleFixture.CommittedCapturePath);
        var real = Ft8CaptureFixture.HashOfBytes(wav);
        var stale = new string('0', 64);

        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(Path.Combine(_folder, "capture.wav"), wav);

        var fixture = Ft8CaptureFixture.Read(Write("stale", Text(sha: stale)));
        var thrown = Assert.Throws<Ft8FixtureException>(() => fixture.RequireCapture());

        Assert.Contains(real, thrown.Message, StringComparison.Ordinal);
        Assert.Contains(stale, thrown.Message, StringComparison.Ordinal);
        Assert.Contains("NOT THE AUDIO", thrown.Message, StringComparison.Ordinal);
        output.WriteLine("REFUSAL 2 - the capture is present and its SHA-256 does not match:");
        output.WriteLine(thrown.Message);
    }

    [Fact]
    public void AMatchingHashIsNotRefused()
    {
        var wav = File.ReadAllBytes(Ft8ExampleFixture.CommittedCapturePath);
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(Path.Combine(_folder, "capture.wav"), wav);

        var fixture = Ft8CaptureFixture.Read(
            Write("good", Text(sha: Ft8CaptureFixture.HashOfBytes(wav))));

        Assert.Equal(Path.Combine(_folder, "capture.wav"), fixture.RequireCapture());
    }

    /// <summary>One byte changed in the middle of the audio is caught. A hash that never fires is not a check.</summary>
    [Fact]
    public void OneChangedByteIsCaught()
    {
        var wav = File.ReadAllBytes(Ft8ExampleFixture.CommittedCapturePath);
        var recorded = Ft8CaptureFixture.HashOfBytes(wav);

        wav[wav.Length / 2] ^= 0x01;

        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(Path.Combine(_folder, "capture.wav"), wav);

        var fixture = Ft8CaptureFixture.Read(Write("onebyte", Text(sha: recorded)));
        var thrown = Assert.Throws<Ft8FixtureException>(() => fixture.RequireCapture());

        Assert.Contains("NOT THE AUDIO", thrown.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- refusal 3

    public static TheoryData<string, string, string> Malformations()
    {
        var data = new TheoryData<string, string, string>();

        data.Add("a row with four fields instead of five", Text(rows: "ROW  5.0  0.48  1000"), "needs five");
        data.Add("a row whose snr is not a number", Text(rows: "ROW  loud  0.48  1000  CQ K1ABC FN42"), "is not a number");
        data.Add("a row whose dt is not a number", Text(rows: "ROW  5.0  soon  1000  CQ K1ABC FN42"), "is not a number");
        data.Add("a row whose frequency is not a number", Text(rows: "ROW  5.0  0.48  high  CQ K1ABC FN42"), "is not a number");
        // An empty message lands on the FIELD COUNT rather than on the empty-message guard: a
        // whitespace-only remainder is not a fifth field, so the row is short before anyone looks at
        // what the message says. Both are refusals and both name the line, so the distinction costs
        // nothing - but it is stated here rather than left as a surprise, and the empty-message guard
        // in Ft8CaptureFixture is belt-and-braces for a future split rule rather than a live path.
        data.Add("a row whose message is nothing but whitespace", Text(rows: "ROW  5.0  0.48  1000    "), "needs five");
        data.Add("no rows at all", Text(rows: string.Empty), "carries no ROW lines");
        data.Add("a missing header key", Text().Replace("sampleRate  12000\n", string.Empty, StringComparison.Ordinal), "there is no \"sampleRate\" header");
        data.Add("a repeated header key", Text() + "\nutc         2020-01-01T00:00:00Z\n", "repeats the header key");
        data.Add("an unknown header key", Text() + "\nconfidence  high\n", "is not one of");
        data.Add("a line that is neither header nor row", Text() + "\nlooksfine\n", "neither a comment");
        data.Add("a truncated sha256", Text(sha: "abc123"), "64 lower-case hexadecimal");
        data.Add("an upper-case sha256", Text(sha: new string('A', 64)), "64 lower-case hexadecimal");
        data.Add("a local time in utc", Text(utc: "2026-09-04 17:30:15"), "not yyyy-MM-ddTHH:mm:ssZ");
        data.Add("a future format version", Text(format: "2"), "knows only format 1");
        data.Add("a capture name carrying a path", Text(capture: "../elsewhere/capture.wav"), "carries a path");
        data.Add("an unrecognised provenance", Text(provenance: "probably-wsjtx"), "the only values this reader accepts");
        data.Add("an empty generator", Text(generator: string.Empty), "there is no \"generator\" header");

        return data;
    }

    [Theory]
    [MemberData(nameof(Malformations))]
    public void RefusalThree_AMalformedFixtureIsRefusedAndNamed(string what, string text, string expected)
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => Ft8CaptureFixture.Parse(text, "example.fixture.txt"));

        Assert.Contains(expected, thrown.Message, StringComparison.Ordinal);
        output.WriteLine($"REFUSAL 3 - {what}:");
        output.WriteLine(thrown.Message);
        output.WriteLine(string.Empty);
    }

    /// <summary>
    /// <b>The message a fixture claims goes through the ladder's own normalisation, not a second copy
    /// of it.</b> Two or more spaces cut the trailing annotation; nothing else is stripped, and
    /// <c>RR73</c> and <c>RRR</c> stay different messages.
    /// </summary>
    [Fact]
    public void TheMessageIsNormalisedTheSameWayUpstreamsListsAre()
    {
        var fixture = Ft8CaptureFixture.Parse(
            Text(rows: "ROW  5.0  0.48  1000  CQ K1ABC FN42   United States"), "n.fixture.txt");

        Assert.Equal("CQ K1ABC FN42", fixture.Rows[0].Message);

        var seventy3 = Ft8CaptureFixture.Parse(Text(rows: "ROW  0  0  1000  K1ABC W9XYZ RR73"), "n");
        var rrr = Ft8CaptureFixture.Parse(Text(rows: "ROW  0  0  1000  K1ABC W9XYZ RRR"), "n");
        Assert.NotEqual(seventy3.Rows[0].Message, rrr.Rows[0].Message);
    }

    // ---------------------------------------------------------------- refusal 4

    [Fact]
    public void RefusalFour_ScoringAgainstSomethingThatIsNotAWsjtxRun()
    {
        var fixture = Ft8CaptureFixture.Read(
            Write("example", Text(provenance: Ft8CaptureFixture.ProvenanceExample)));

        var thrown = Assert.Throws<Ft8FixtureException>(
            () => fixture.RequireScorable("Scoring Ft8Sharp against this fixture"));

        Assert.Contains("provenance is \"example\"", thrown.Message, StringComparison.Ordinal);
        output.WriteLine("REFUSAL 4 - the provenance is not a real WSJT-X run and the caller asked to score:");
        output.WriteLine(thrown.Message);
    }

    /// <summary><b>Reading an example fixture is fine.</b> Only scoring against one is refused.</summary>
    [Fact]
    public void ReadingAnExampleFixtureIsNotRefused()
    {
        var fixture = Ft8CaptureFixture.Read(
            Write("readable", Text(provenance: Ft8CaptureFixture.ProvenanceExample)));

        Assert.Equal(3, fixture.Rows.Count);
        Assert.False(fixture.IsRealWsjtxRun);
    }

    [Fact]
    public void AWsjtxFixtureIsScorable()
    {
        var fixture = Ft8CaptureFixture.Parse(
            Text(provenance: Ft8CaptureFixture.ProvenanceWsjtx), "w.fixture.txt");

        Assert.True(fixture.IsRealWsjtxRun);
        fixture.RequireScorable("Scoring");
    }

    // ---------------------------------------------------------------- round trip

    [Fact]
    public void WhatIsWrittenIsWhatIsRead()
    {
        var original = Ft8CaptureFixture.Parse(Text(), "r.fixture.txt");
        var again = Ft8CaptureFixture.Parse(original.ToFileText(["round trip"]), "r.fixture.txt");

        Assert.Equal(original.CaptureName, again.CaptureName);
        Assert.Equal(original.Utc, again.Utc);
        Assert.Equal(original.Sha256, again.Sha256);
        Assert.Equal(original.SampleRate, again.SampleRate);
        Assert.Equal(original.Provenance, again.Provenance);
        Assert.Equal(original.Generator, again.Generator);
        Assert.Equal(original.Messages, again.Messages);
    }

    [Fact]
    public void AFixtureFileThatIsNotThereIsNotAnEmptyFixture()
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => Ft8CaptureFixture.Read(Path.Combine(_folder, "nothing.fixture.txt")));

        Assert.Contains("never an empty fixture", thrown.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>A whole valid fixture, with one field at a time replaced by the caller.</summary>
    private static string Text(
        string format = "1",
        string capture = "capture.wav",
        string utc = "2026-09-04T21:30:15Z",
        string? sha = null,
        string sampleRate = "12000",
        string provenance = Ft8CaptureFixture.ProvenanceExample,
        string generator = "Ft8CaptureFixtureTests",
        string? rows = null) =>
        $"# a fixture built in a test\n\n"
        + $"format      {format}\n"
        + $"capture     {capture}\n"
        + $"utc         {utc}\n"
        + $"sha256      {sha ?? new string('a', 64)}\n"
        + $"sampleRate  {sampleRate}\n"
        + $"provenance  {provenance}\n"
        + $"generator   {generator}\n\n"
        + (rows ?? "ROW  5.0  0.48  1000  CQ K1ABC FN42\n"
            + "ROW  -3.0  0.64  1500  CQ W9XYZ\n"
            + "ROW  -12.5  0.80  2000  K1ABC W9XYZ -11")
        + "\n";

    private string Write(string stem, string text)
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, stem + Ft8CaptureFixture.Extension);
        File.WriteAllText(path, text);
        return path;
    }
}
