using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>Everything in the shack command that is reachable on a machine with no WSJT-X on it.</b>
/// </summary>
/// <remarks>
/// <para>
/// The hashing, the row parsing, the fixture writing, the refusal when the decoder is not found and
/// the refusal when it produced nothing - all against decode text <b>committed as a test input</b>
/// under <c>tests/fixtures/ft8/parser-inputs/</c>, which is hand-written, is not WSJT-X's, and says
/// so in its own README.
/// </para>
/// <para>
/// <b>What is NOT here, and this is the honest half:</b> nothing below starts a process. Invoking
/// WSJT-X and getting real rows back cannot be exercised on this machine and is not simulated -
/// <b>Tim's first run at the shack is what exercises it.</b>
/// </para>
/// </remarks>
public class Ft8FixtureGeneratorTests(ITestOutputHelper output) : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-ft8-maker-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private static string Input(string name) => Path.Combine(
        Ft8CaptureFixtures.Root, "parser-inputs", name);

    // ------------------------------------------------------------------ the parser

    [Fact]
    public void TheCommittedDecodeTextParsesIntoRows()
    {
        var rows = WsjtxDecodeLines.Parse(File.ReadAllText(Input("good.decode.txt")), "good.decode.txt");

        Assert.Equal(4, rows.Count);
        Assert.Equal(-5.0, rows[0].SnrDb);
        Assert.Equal(0.48, rows[0].DtSeconds);
        Assert.Equal(1000.0, rows[0].FrequencyHz);
        Assert.Equal("CQ K1ABC FN42", rows[0].Message);
        Assert.Equal("CQ DX VK3ABC QF22", rows[3].Message);
    }

    [Theory]
    [InlineData("short-line.decode.txt", "has 5 fields and the shape this parser accepts has six")]
    [InlineData("bad-number.decode.txt", "its snr field reads \"loud-\" and is not a number")]
    [InlineData("no-tilde.decode.txt", "its fifth field is \":\"")]
    public void AMalformedDecodeLineIsRefusedAndNeverSkipped(string file, string expected)
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => WsjtxDecodeLines.Parse(File.ReadAllText(Input(file)), file));

        Assert.Contains(expected, thrown.Message, StringComparison.Ordinal);
        Assert.Contains("REFUSES RATHER THAN SKIPS", thrown.Message, StringComparison.Ordinal);
        output.WriteLine(thrown.Message);
        output.WriteLine(string.Empty);
    }

    // ------------------------------------------------------------------ nothing back

    [Fact]
    public void ADecoderThatReturnedNothingIsRefusedRatherThanWrittenAsAnEmptyFixture()
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => Ft8FixtureGenerator.RowsFrom(
                File.ReadAllText(Input("nothing.decode.txt")), "capture.wav", "jt9.exe"));

        Assert.Contains("produced no decode lines at all", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("is not a scoreboard", thrown.Message, StringComparison.Ordinal);
        output.WriteLine(thrown.Message);
    }

    // ------------------------------------------------------------------ finding the decoder

    private sealed class NothingInstalled : Ft8FixtureGenerator.IDecoderLookup
    {
        public bool Exists(string path) => false;

        public string? Variable(string name) => null;
    }

    private sealed class OneThingInstalled(string there, string? variable = null)
        : Ft8FixtureGenerator.IDecoderLookup
    {
        public bool Exists(string path) => string.Equals(path, there, StringComparison.Ordinal);

        public string? Variable(string name) =>
            string.Equals(name, Ft8FixtureGenerator.DecoderVariable, StringComparison.Ordinal)
                ? variable
                : null;
    }

    [Fact]
    public void TheDecoderNotBeingFoundIsALoudRefusalThatNamesEveryPlaceItLooked()
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => Ft8FixtureGenerator.LocateDecoder(null, new NothingInstalled()));

        Assert.Contains("was not found, so no fixture was written", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("NOTHING IS SUBSTITUTED FOR IT", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("decode_ft8.exe", thrown.Message, StringComparison.Ordinal);

        foreach (var candidate in Ft8FixtureGenerator.CandidatePaths)
        {
            Assert.Contains(candidate, thrown.Message, StringComparison.Ordinal);
        }

        output.WriteLine(thrown.Message);
    }

    [Fact]
    public void AnExplicitPathThatIsWrongIsNotSearchedAroundForSomethingElse()
    {
        var thrown = Assert.Throws<Ft8FixtureException>(
            () => Ft8FixtureGenerator.LocateDecoder(
                @"D:\nowhere\jt9.exe",
                new OneThingInstalled(Ft8FixtureGenerator.CandidatePaths[0])));

        Assert.Contains("there is nothing there", thrown.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Ft8FixtureGenerator.CandidatePaths[0], thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEnvironmentVariableIsPreferredToTheStandardLocations()
    {
        var configured = @"E:\shack\jt9.exe";

        Assert.Equal(
            configured,
            Ft8FixtureGenerator.LocateDecoder(null, new OneThingInstalled(configured, configured)));
    }

    [Fact]
    public void AStandardLocationIsFoundWhenNothingElseSaysWhere()
    {
        Assert.Equal(
            Ft8FixtureGenerator.CandidatePaths[1],
            Ft8FixtureGenerator.LocateDecoder(
                null, new OneThingInstalled(Ft8FixtureGenerator.CandidatePaths[1])));
    }

    // ------------------------------------------------------------------ building and writing

    private string StageCapture()
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, "ft8-2026-09-04-213015.wav");
        File.Copy(Ft8ExampleFixture.CommittedCapturePath, path);
        return path;
    }

    [Fact]
    public void OneCaptureInOneWholeFixtureOutWithProvenanceSetToARealRun()
    {
        var capture = StageCapture();

        var fixture = Ft8FixtureGenerator.Build(
            capture,
            File.ReadAllText(Input("good.decode.txt")),
            "jt9.exe -8",
            "2026-09-04T21:30:15Z",
            12_000);

        Assert.Equal(Ft8CaptureFixture.ProvenanceWsjtx, fixture.Provenance);
        Assert.True(fixture.IsRealWsjtxRun);
        Assert.Equal(4, fixture.Rows.Count);
        Assert.Equal(Ft8CaptureFixture.HashOf(capture), fixture.Sha256);

        var written = Ft8FixtureGenerator.WriteFixture(fixture, ["written by a test"]);
        output.WriteLine(File.ReadAllText(written));

        // The whole point: it reads back through the reader every later session uses, its capture is
        // beside it, its hash matches, and it is scorable.
        var again = Ft8CaptureFixture.Read(written);
        Assert.Equal(capture, again.RequireCapture());
        again.RequireScorable("Scoring");
        Assert.Equal(fixture.Messages, again.Messages);
    }

    /// <summary><b>The fixture lands beside the capture, same stem, no editing afterwards.</b></summary>
    [Fact]
    public void TheFixtureLandsBesideTheCaptureWithTheSameStem()
    {
        var capture = StageCapture();

        var written = Ft8FixtureGenerator.WriteFixture(
            Ft8FixtureGenerator.Build(
                capture, File.ReadAllText(Input("good.decode.txt")), "jt9.exe", "2026-09-04T21:30:15Z", 12_000));

        Assert.Equal(
            Path.Combine(_folder, "ft8-2026-09-04-213015" + Ft8CaptureFixture.Extension), written);
    }

    /// <summary>
    /// <b>A capture that is not there leaves nothing behind.</b> No half-fixture, no empty file.
    /// </summary>
    [Fact]
    public void AnAbsentCaptureWritesNothingAtAll()
    {
        Directory.CreateDirectory(_folder);
        var capture = Path.Combine(_folder, "not-recorded.wav");

        Assert.Throws<Ft8FixtureException>(
            () => Ft8FixtureGenerator.Build(
                capture, File.ReadAllText(Input("good.decode.txt")), "jt9.exe", "2026-09-04T21:30:15Z", 12_000));

        Assert.Empty(Directory.GetFiles(_folder));
    }

    /// <summary>
    /// <b>A refused parse leaves nothing behind either</b> — the rows are parsed before the file is
    /// opened, so there is no window in which a partial file exists.
    /// </summary>
    [Fact]
    public void ARefusedDecodeLineWritesNothingAtAll()
    {
        var capture = StageCapture();

        Assert.Throws<Ft8FixtureException>(
            () => Ft8FixtureGenerator.Build(
                capture,
                File.ReadAllText(Input("bad-number.decode.txt")),
                "jt9.exe",
                "2026-09-04T21:30:15Z",
                12_000));

        Assert.Equal([capture], Directory.GetFiles(_folder));
    }

    /// <summary>
    /// <b>Nothing partial is left in the destination folder</b>, even under the temporary name the
    /// writer stages through.
    /// </summary>
    [Fact]
    public void NoPartialFileSurvivesASuccessfulWriteEither()
    {
        var capture = StageCapture();

        Ft8FixtureGenerator.WriteFixture(
            Ft8FixtureGenerator.Build(
                capture, File.ReadAllText(Input("good.decode.txt")), "jt9.exe", "2026-09-04T21:30:15Z", 12_000));

        Assert.Empty(Directory.GetFiles(_folder, "*.partial"));
        Assert.Equal(2, Directory.GetFiles(_folder).Length);
    }

    /// <summary>
    /// <b>The generator's own output is what the scorer can score.</b> End to end, on this machine,
    /// with a fixture whose provenance really is <c>wsjtx</c> - built from committed parser input
    /// rather than from a real run, which is why it lives under the temporary folder and is deleted.
    /// </summary>
    [Fact]
    public void WhatTheGeneratorWritesIsWhatTheReaderReadsAndTheScorerScores()
    {
        var capture = StageCapture();

        // The example's own three messages, in the decode-line shape, so the scorer has something to
        // match. This text is a TEST INPUT and is not committed anywhere as a claim.
        var lines = string.Join(
            "\n",
            Ft8CaptureFixture.Read(Ft8ExampleFixture.CommittedFixturePath).Rows
                .Select(r => $"120000 {r.SnrDb:+00.0;-00.0} {r.DtSeconds:+0.00;-0.00} {r.FrequencyHz:0} ~  {r.Message}"));

        var written = Ft8FixtureGenerator.WriteFixture(
            Ft8FixtureGenerator.Build(capture, lines, "test input, not a real run", "2026-09-04T21:30:15Z", 12_000));

        var scores = Ft8Sharp.Tests.Dsp.Ft8LadderHarness.ScoreFixture(Ft8CaptureFixture.Read(written));

        foreach (var line in Ft8Sharp.Tests.Dsp.Ft8LadderHarness.FixtureReport(scores))
        {
            output.WriteLine(line);
        }

        Assert.Equal(3, scores[0].Matched.Count);
        Assert.Empty(scores[0].Missed);
        Assert.Empty(scores[0].ReturnedWrong);
    }
}
