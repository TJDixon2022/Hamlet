using System.Text.Json;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **THE SCAN SHOWS ITS REASONING** (work instruction 543, task 3, HM-DEC-247): the line under the terminal header says what
/// the scan is doing in the owner's terms, and scan.json keeps every survey, every peak it considered and why it was listed
/// or skipped, so a station the owner sees on the waterfall and the scan skips has a written reason.
/// </summary>
public sealed class TheScanShowsItsReasoningTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines and the survey are printed.</param>
    public TheScanShowsItsReasoningTests(ITestOutputHelper output) => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <remarks>
    /// On the span of task 1: the line watches the span with its clock and its count, visits each station by its place on
    /// the list, and advances to the next span.
    /// </remarks>
    [Fact]
    public async Task TheLineSaysWatchingVisitingAndAdvancing()
    {
        using var world = await SpanWorld.Ready(WhyClearStationsAreSkippedTests.Home, WhyClearStationsAreSkippedTests.ThreeClear);
        var lines = new List<string>();
        var (_, summary) = await world.Run(_folder, new CwScanSettings(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30)), s => s.Said += lines.Add);
        var distinct = lines.Distinct().ToList();

        _output.WriteLine(string.Join(Environment.NewLine, distinct.Take(40)));

        Assert.Contains("watching 7.000–7.020 · 0:03 · 3 stations", distinct);
        Assert.Contains("visiting 1 of 3 · 7.0043 · landing", distinct);
        Assert.Contains("visiting 2 of 3 · 7.0091 · empty", distinct);
        Assert.Contains("visiting 3 of 3 · 7.0150 · empty", distinct);
        Assert.Contains("advancing to 7.020–7.040", distinct);
        Assert.True(distinct.IndexOf("advancing to 7.020–7.040") > distinct.IndexOf("visiting 3 of 3 · 7.0150 · empty"));
    }

    /// <remarks>
    /// On the whole-segment scope of the catch tests, with the call, the noise and the carrier: a station heard keying says
    /// its light as the ear has it, and one with no shape says negative, each with its clock.
    /// </remarks>
    [Fact]
    public async Task TheLineSaysTheLightOrNegativeOnAVisit()
    {
        using var world = await TheCatchScanTests.ScanWorld.Ready(keepsSending: true);
        var lines = new List<string>();

        await world.Run(_folder, TheCatchScanTests.Short with { Length = TimeSpan.FromSeconds(140) }, s => s.Said += lines.Add);

        var distinct = lines.Distinct().ToList();

        _output.WriteLine(string.Join(Environment.NewLine, distinct.Where(l => !l.StartsWith("watching", StringComparison.Ordinal)).Take(30)));

        Assert.Contains(distinct, l => l.StartsWith("visiting 1 of 3 · 7.0100 · ", StringComparison.Ordinal)
            && (l.Contains("shape found", StringComparison.Ordinal) || l.Contains("reading", StringComparison.Ordinal)));
        Assert.Contains("visiting 3 of 3 · 7.0500 · negative · 0:12", distinct);
    }

    /// <remarks>
    /// scan.json keeps each survey: the span, the sweeps, and every peak considered with its level, width at half its
    /// height and sweeps stood, listed or skipped and why. A phone-wide signal is skipped as too wide, two stations 150 Hz
    /// apart are one, noise blips do not repeat, and each catch names the survey it came from.
    /// </remarks>
    [Fact]
    public async Task ScanJsonKeepsEverySurveyAndWhy()
    {
        ScopeStation[] stations =
        [
            new("moderate", 7_004_000, [30, 18, 6], 0.55),
            new("neighbour", 7_004_150, [20, 12, 4], 0.55),
            new("phone-wide", 7_012_000, Enumerable.Repeat((byte)40, 30).ToArray(), 1.0),
        ];

        using var world = await SpanWorld.Ready(WhyClearStationsAreSkippedTests.Home, stations, seed: 5433);
        var (scan, summary) = await world.Run(_folder, new CwScanSettings(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30)));
        var json = File.ReadAllText(Path.Combine(scan.ScanFolder!, "scan.json"));
        using var document = JsonDocument.Parse(json);
        var survey = document.RootElement.GetProperty("surveys")[0];
        var considered = survey.GetProperty("considered").EnumerateArray().ToList();

        _output.WriteLine(JsonSerializer.Serialize(
            new
            {
                index = survey.GetProperty("index").GetInt32(),
                lowHz = survey.GetProperty("lowHz").GetInt64(),
                highHz = survey.GetProperty("highHz").GetInt64(),
                sweeps = survey.GetProperty("sweeps").GetInt32(),
                listed = survey.GetProperty("listed").GetInt32(),
                considered = considered.Where(c => c.GetProperty("seen").GetInt32() > 1 || c.GetProperty("verdict").GetString() != "not-repeating").Select(c => c.Clone()).ToList(),
                blipsNotRepeating = considered.Count(c => c.GetProperty("seen").GetInt32() == 1),
            },
            new JsonSerializerOptions { WriteIndented = true }));

        Assert.Equal(7_000_000, survey.GetProperty("lowHz").GetInt64());
        Assert.Equal(7_020_000, survey.GetProperty("highHz").GetInt64());
        Assert.InRange(survey.GetProperty("sweeps").GetInt32(), 12, 14);
        Assert.Equal(1, survey.GetProperty("listed").GetInt32());

        string VerdictNear(long hz) => considered.OrderBy(c => Math.Abs(c.GetProperty("frequencyHz").GetInt64() - hz)).First().GetProperty("verdict").GetString()!;

        Assert.Equal("listed", VerdictNear(7_004_000));
        Assert.Contains(considered, c => c.GetProperty("verdict").GetString() == "merged" && c.GetProperty("why").GetString()!.Contains("7.00", StringComparison.Ordinal));
        Assert.Equal("too-wide", VerdictNear(7_012_000));
        Assert.Contains(considered, c => c.GetProperty("verdict").GetString() == "not-repeating");
        Assert.All(considered, c =>
        {
            Assert.True(c.GetProperty("widthHz").GetDouble() > 0);
            Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("why").GetString()));
        });

        var first = summary.Catches.First();

        Assert.Equal(1, first.Survey);
        Assert.InRange(first.SignalHz, 6_999_000, 7_005_000);
    }
}
