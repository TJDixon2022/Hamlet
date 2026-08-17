using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// The fence a scan stays inside, and where its numbers come from
/// (HM-DEC-107 phase 8, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**THE SHIPPED DEFAULT IS GENERATED AND NOT TRANSCRIBED.** §0.2.1 wants
/// the scanned stretch in a file the operator edits, and §0 wants anything
/// derivable from a source of truth derived rather than copied. Both are met the
/// same way: the default is built from the cited Morse rows of
/// <c>data/bands/us-neighborhoods.json</c>, so a correction there reaches the
/// scanner without anybody making it twice.</para>
/// </remarks>
public sealed class ScanSegmentsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the segments are printed.</param>
    public ScanSegmentsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// <para>Proves §0 and §0.2.1: **every default segment carries the citation
    /// its neighborhood row carried.** A frequency a scanner will drive the
    /// operator's radio to, with no source behind it, is the prime directive
    /// broken in the data layer where it is hardest to see (§0.0).</para>
    /// </remarks>
    [Fact]
    public void EverySegmentInTheDefaultSaysWhereItsNumbersCameFrom()
    {
        var segments = ScanSegments.Default;

        foreach (var segment in segments.All)
        {
            _output.WriteLine($"{segment.Band,-6} {segment.LowHz}-{segment.HighHz}  "
                + $"{segment.Name}  [{segment.Cite}]");
        }

        Assert.NotEmpty(segments.All);

        Assert.All(segments.All, s => Assert.False(
            string.IsNullOrWhiteSpace(s.Cite),
            $"the segment '{s.Name}' on {s.Band} has no source behind it"));

        Assert.All(segments.All, s => Assert.True(
            s.HighHz > s.LowHz, $"the segment '{s.Name}' is backwards or empty"));
    }

    /// <remarks>
    /// Proves §0: **the default is generated from the neighborhood data rather
    /// than being a second copy of it.** Every segment matches a Morse row in
    /// that file exactly, so the two cannot drift apart.
    /// </remarks>
    [Fact]
    public void TheDefaultIsTheNeighborhoodDataAndNotACopyOfIt()
    {
        var data = NeighborhoodData.Current;

        var morse = data.Bands
            .SelectMany(b => data.ForBand(b))
            .Where(h => h.Family == ModeFamily.Cw)
            .Select(h => (h.LowHz, h.HighHz))
            .ToHashSet();

        _output.WriteLine($"{morse.Count} Morse rows, "
            + $"{ScanSegments.Default.All.Count} segments");

        Assert.Equal(morse.Count, ScanSegments.Default.All.Count);

        Assert.All(
            ScanSegments.Default.All,
            s => Assert.Contains((s.LowHz, s.HighHz), morse));
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 8: **the Morse scanner scans Morse.** A dwell
    /// scored by a Morse decoder has nothing to say about a stretch of FT8, so
    /// pointing the radio there would waste the operator's evening measuring
    /// something this cannot measure.
    /// </remarks>
    [Fact]
    public void NothingButTheMorseStretchesIsInTheDefault()
    {
        var data = NeighborhoodData.Current;

        var notMorse = data.Bands
            .SelectMany(b => data.ForBand(b))
            .Where(h => h.Family != ModeFamily.Cw)
            .Select(h => (h.LowHz, h.HighHz))
            .ToHashSet();

        Assert.All(
            ScanSegments.Default.All,
            s => Assert.DoesNotContain((s.LowHz, s.HighHz), notMorse));
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the operator's file wins outright and is not
    /// merged.** Merging would mean a segment he deleted comes back on the next
    /// release, which is the app overruling him about where his own radio may
    /// go.</para>
    /// </remarks>
    [Fact]
    public void TheOperatorsOwnFileReplacesTheDefaultRatherThanAddingToIt()
    {
        var mine = ScanSegments.Parse(
            """
            {
              "segments": [
                { "band": "30 m", "name": "the only place I scan",
                  "lowHz": 10100000, "highHz": 10130000, "cite": "my own choice" }
              ]
            }
            """,
            "the operator's file");

        _output.WriteLine($"{mine.All.Count} segment from {mine.Origin}");

        Assert.Single(mine.All);
        Assert.Equal("the only place I scan", mine.All[0].Name);
    }

    /// <remarks>
    /// Proves §0.2.1: a row switched off is kept and skipped, so the operator can
    /// park a stretch without losing what he wrote about it.
    /// </remarks>
    [Fact]
    public void ASegmentSwitchedOffIsKeptAndSkipped()
    {
        var mixed = ScanSegments.Parse(
            """
            {
              "segments": [
                { "band": "40 m", "name": "on", "lowHz": 7000000,
                  "highHz": 7050000, "cite": "test" },
                { "band": "20 m", "name": "off", "lowHz": 14000000,
                  "highHz": 14060000, "cite": "test", "enabled": false }
              ]
            }
            """);

        Assert.Equal(2, mixed.All.Count);
        Assert.Single(mixed.Enabled);
        Assert.Equal("on", mixed.Enabled[0].Name);
    }

    /// <remarks>
    /// <para>Proves §0.2.1 and §0.0: **a file that cannot be read is refused
    /// loudly and never quietly replaced with the default.** Silently
    /// substituting would run the scan over a stretch the operator did not
    /// choose, which is the one thing the file exists to prevent.</para>
    /// </remarks>
    [Theory]
    [InlineData("{ not json at all", "not readable JSON")]
    [InlineData("{ \"segments\": [] }", "names no segments")]
    [InlineData(
        "{ \"segments\": [ { \"name\": \"backwards\", \"lowHz\": 7050000, "
        + "\"highHz\": 7000000 } ] }",
        "backwards or empty")]
    public void AFileThatCannotBeReadIsRefusedRatherThanReplaced(
        string json, string expected)
    {
        var error = Assert.Throws<InvalidDataException>(() => ScanSegments.Parse(json));

        _output.WriteLine(error.Message);

        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the file Hamlet writes is written once and never
    /// again.** A file the operator has edited is his, and a release that
    /// refreshed it would silently undo his decisions about where his own radio
    /// may go.</para>
    /// </remarks>
    [Fact]
    public void TheFileIsWrittenOnceAndAnEditedOneIsNeverOverwritten()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-scan-" + Guid.NewGuid().ToString("N"));

        var path = Path.Combine(folder, "scan-segments.json");

        try
        {
            ScanSegments.WriteDefaultIfMissing(path);

            Assert.True(File.Exists(path));

            var written = File.ReadAllText(path);

            // He edits it down to one stretch of his own.
            File.WriteAllText(
                path,
                """
                { "segments": [ { "band": "30 m", "name": "mine",
                  "lowHz": 10100000, "highHz": 10130000, "cite": "mine" } ] }
                """);

            ScanSegments.WriteDefaultIfMissing(path);

            var after = ScanSegments.LoadOrDefault(path);

            _output.WriteLine($"{after.All.Count} segment survived, "
                + $"against {written.Length} bytes Hamlet first wrote");

            Assert.Single(after.All);
            Assert.Equal("mine", after.All[0].Name);
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }

    /// <remarks>
    /// Proves the round trip: what Hamlet writes is what Hamlet reads, so the
    /// operator's first edit starts from something that works (§5).
    /// </remarks>
    [Fact]
    public void WhatHamletWritesIsWhatHamletReads()
    {
        var back = ScanSegments.Parse(ScanSegments.Default.ToJson(), "the round trip");

        Assert.Equal(ScanSegments.Default.All.Count, back.All.Count);

        for (var i = 0; i < back.All.Count; i++)
        {
            Assert.Equal(ScanSegments.Default.All[i], back.All[i]);
        }
    }

    /// <remarks>
    /// Proves §0.2.1: a missing file is the ordinary first-run case and is not an
    /// error, so the scanner has a fence before the operator has written
    /// anything.
    /// </remarks>
    [Fact]
    public void AMissingFileIsTheFirstRunAndNotAFault()
    {
        var path = Path.Combine(
            Path.GetTempPath(), "hamlet-scan-missing-" + Guid.NewGuid().ToString("N"),
            "nothing.json");

        var segments = ScanSegments.LoadOrDefault(path);

        Assert.Equal(ScanSegments.Default.All.Count, segments.All.Count);
        Assert.Contains("us-neighborhoods.json", segments.Origin, StringComparison.Ordinal);
    }
}
