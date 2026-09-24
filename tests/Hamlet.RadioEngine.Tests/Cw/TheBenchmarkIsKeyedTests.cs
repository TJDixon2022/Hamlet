using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The locked-on run of 2026-09-24 keyed by differencing consecutive transcripts and
/// scored (work instruction 412, task 3; PHASE_PLAN.md 1.6, R61, R63).
/// </summary>
/// <remarks>
/// <para>**EVERY KEY HERE IS INFERRED AND IS READ FROM ITS KEY FILE**, never typed
/// again (§0): each `.key.md` beside a capture names its scored stretches and the
/// key for each, and says how it was built. A capture whose key file names no
/// stretch is keyed with nothing to score.</para>
/// <para>**THE KEY FILES ARE CHECKED AGAINST THE SIDECARS**: each previous
/// capture's `text` must be a prefix of the next one's, and every stretch must be
/// a substring of what was added between them, so a key cannot drift from the
/// decode it was inferred from.</para>
/// <para>**TWO SCORES PER STRETCH.** *Live* is the stretch as the application read
/// it that night, scored whole against its key: what Tim watched, fixed in the
/// sidecar and moved by nothing in this tree. *Bench* is this WAV replayed through
/// the decoder at HEAD, cold, with the key aligned into its best-fitting stretch
/// (<see cref="CwScorer.Within"/>): the number a later change moves. Where a
/// capture follows the previous one by more than thirty seconds, part of what was
/// added was heard before the WAV begins, and its key file says so.</para>
/// <para>A printer. It asserts only that the key files agree with the sidecars.</para>
/// </remarks>
public sealed class TheBenchmarkIsKeyedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheBenchmarkIsKeyedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The locked-on run, each capture with the one it is differenced against.</summary>
    public static IReadOnlyList<(string Name, string Previous)> Run { get; } = new[]
    {
        ("004108", "004027"),
        ("004133", "004108"),
        ("004205", "004133"),
        ("004234", "004205"),
        ("004322", "004234"),
        ("004347", "004322"),
        ("004405", "004347"),
        ("004427", "004405"),
        ("004510", "004427"),
        ("004535", "004510"),
        ("004550", "004535"),
    }.Select(p => (Full(p.Item1), Full(p.Item2))).ToList();

    /// <summary>One scored stretch of a capture and its key.</summary>
    /// <param name="Region">The stretch as the application read it.</param>
    /// <param name="Key">What was inferred to have been sent.</param>
    public sealed record Stretch(string Region, string Key);

    private static string Full(string time) => $"unadjudicated/cw-2026-09-24-{time}";

    /// <summary>The sidecar's cumulative `text` line.</summary>
    /// <param name="name">The capture.</param>
    /// <returns>The text, leading and trailing spaces kept.</returns>
    internal static string Text(string name)
        => File.ReadAllLines(Path.Combine(CapturedSignalTests.Folder, name + ".txt"))
            .Single(line => line.StartsWith("text       ", StringComparison.Ordinal))["text       ".Length..];

    /// <summary>The stretches a key file names, in order.</summary>
    /// <param name="name">The capture.</param>
    /// <returns>Each stretch with its key.</returns>
    internal static IReadOnlyList<Stretch> Stretches(string name)
    {
        var lines = File.ReadAllLines(Path.Combine(CapturedSignalTests.Folder, name + ".key.md"));
        var regions = lines.Select(l => Regex.Match(l, @"^\s+stretch\s+`(.*)`\s*$"))
            .Where(m => m.Success).Select(m => m.Groups[1].Value).ToList();
        var keys = lines.Select(l => Regex.Match(l, @"^\s+key\s+`(.*)`\s*$"))
            .Where(m => m.Success).Select(m => m.Groups[1].Value).ToList();

        Assert.Equal(regions.Count, keys.Count);

        return regions.Zip(keys, (r, k) => new Stretch(r, k)).ToList();
    }

    /// <remarks>
    /// Proves 1.6: every capture of the locked-on run carries a key file whose
    /// stretches are what the application decoded in that capture, and each is
    /// scored with its three parts and its guard, live and on the bench, tabled
    /// with totals beside the baseline.
    /// </remarks>
    [Fact]
    public void EachKeyIsScoredLiveAndOnTheBench()
    {
        _output.WriteLine(
            "key | capture | stretch | live edits | scored length | key | live guard | bench edits | bench guard | bench region");

        var live = new List<CwScore>();
        var bench = new List<CwScore>();
        var keyed = 0;

        foreach (var (name, previous) in Run)
        {
            var before = Text(previous);
            var text = Text(name);

            Assert.StartsWith(before, text, StringComparison.Ordinal);

            var added = text[before.Length..];
            var stretches = Stretches(name);
            var reading = stretches.Count > 0 ? TheBaselineIsScoredTests.Read(name) : null;

            if (stretches.Count == 0)
            {
                _output.WriteLine($"key | {name} | none scored | | | | | | | added `{added}`");
                continue;
            }

            keyed++;

            foreach (var stretch in stretches)
            {
                Assert.Contains(stretch.Region, added, StringComparison.Ordinal);

                var l = CwScorer.Whole(stretch.Region, stretch.Key, CwKeyKind.Inferred);
                var b = CwScorer.Within(reading!, stretch.Key, CwKeyKind.Inferred);

                live.Add(l);
                bench.Add(b);

                _output.WriteLine(
                    $"key | {name} | `{stretch.Region}` | {l.Edits} | {l.ScoredLength} | `{stretch.Key}` | "
                    + $"{l.Guard} | {b.Edits} | {b.Guard} | `{b.Region}`");
            }
        }

        _output.WriteLine(
            $"total live | {live.Sum(s => s.Edits)} edits over {live.Sum(s => s.ScoredLength)} characters "
            + $"against inferred keys, {live.Count} stretches, {keyed} of {Run.Count} captures scored, "
            + $"{live.Sum(s => s.Unsure)} unsure per {live.Sum(s => s.Named)} named");
        _output.WriteLine(
            $"total bench | {bench.Sum(s => s.Edits)} edits over {bench.Sum(s => s.ScoredLength)} characters "
            + $"against inferred keys, {bench.Count} stretches, "
            + $"{bench.Sum(s => s.Unsure)} unsure per {bench.Sum(s => s.Named)} named");
    }
}
