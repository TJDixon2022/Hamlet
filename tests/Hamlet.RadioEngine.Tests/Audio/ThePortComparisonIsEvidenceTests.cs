using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 249, task 4: the comparison is evidence, off by default, and
/// never a second list.
/// </summary>
/// <remarks>
/// <para>**TWO LISTS ON SCREEN THAT DISAGREE WOULD HAND THE OPERATOR AN
/// ADJUDICATION THIS APPLICATION EXISTS TO MAKE FOR HIM** (§0.0). So what the
/// port made of a slot goes to the record and stays there, and the messages that
/// come back are Deep's whether the comparison ran or not.</para>
/// <para>**AND THE LADDER IS STILL THE EVIDENCE.** One slot compared two ways
/// settles nothing; unit 248's 306 trials at each level is what supports a claim
/// about either decoder. This is the convenience for an evening somebody wants
/// to look.</para>
/// </remarks>
public sealed class ThePortComparisonIsEvidenceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public ThePortComparisonIsEvidenceTests(ITestOutputHelper output)
        => _output = output;

    private static DateTime EndedAt { get; } =
        new(2026, 9, 3, 21, 6, 30, DateTimeKind.Utc);

    private static ClockOffset Measured { get; } =
        new(0, new DateTime(2026, 9, 3, 21, 0, 0, DateTimeKind.Utc));

    /// <summary>Off by default, and off records nothing rather than zeroes.</summary>
    [Fact]
    public void ItIsOffByDefaultAndSaysSoRatherThanRecordingZeroes()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        Assert.NotEmpty(heard.Slots);

        foreach (var slot in heard.Slots)
        {
            _output.WriteLine(slot.SlotStartUtc.ToString("HH:mm:ss")
                + "  comparison: "
                + (slot.PortComparison is null ? "not run" : slot.PortComparison.ToString()));

            // **NULL AND NOT A ZEROED RECORD.** "Nobody asked" and "the port
            // found nothing" are opposite facts, and a zero here would read as
            // the second.
            Assert.Null(slot.PortComparison);
        }
    }

    /// <summary>
    /// On, it records both counts and still returns only Deep's messages.
    /// </summary>
    [Fact]
    public void OnItRecordsBothCountsAndShowsOnlyOneList()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var without = Ft8Reader.Read(audio, EndedAt, Measured);

        var startedWith = Stopwatch.GetTimestamp();
        var with = Ft8Reader.Read(
            audio, EndedAt, Measured, compareWithThePort: true);
        var withMs = (Stopwatch.GetTimestamp() - startedWith) * 1000.0
            / Stopwatch.Frequency;

        Assert.NotEmpty(with.Slots);

        var slot = with.Slots[0];

        Assert.NotNull(slot.PortComparison);

        _output.WriteLine("Deep on this slot : "
            + slot.CandidateCount + " candidates, "
            + slot.BecameTextCount + " text");
        _output.WriteLine("port on this slot : " + slot.PortComparison);
        _output.WriteLine("");
        _output.WriteLine("messages returned, comparison off : "
            + without.Decodes.Count);
        _output.WriteLine("messages returned, comparison on  : "
            + with.Decodes.Count);
        _output.WriteLine("");
        _output.WriteLine("the whole read with it on : "
            + withMs.ToString("0") + " ms");
        _output.WriteLine("the port's share of that  : "
            + slot.PortComparison.Value.Milliseconds.ToString("0") + " ms a slot");
        _output.WriteLine("budget                    : 15000 ms a slot");

        // **THE MESSAGES ARE THE SAME EITHER WAY.** Turning the comparison on
        // adds a count to the record and changes nothing the operator sees.
        Assert.Equal(
            without.Decodes.Select(d => d.Message).ToList(),
            with.Decodes.Select(d => d.Message).ToList());

        // The port's own counts are populated, so the comparison really ran.
        Assert.True(
            slot.PortComparison.Value.CandidateCount > 0,
            "the port recorded no candidates, so the comparison did not run");

        Assert.True(
            slot.PortComparison.Value.Milliseconds > 0,
            "the port's decode was not timed");
    }

    /// <summary>The sidecar carries it, and says "not run" when it did not.</summary>
    [Fact]
    public void TheSidecarCarriesTheComparisonOrSaysItDidNotRun()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var off = Sheet(audio, compare: false);
        var on = Sheet(audio, compare: true);

        var offLine = Line(off);
        var onLine = Line(on);

        _output.WriteLine("comparison off : " + offLine);
        _output.WriteLine("comparison on  : " + onLine);

        Assert.NotNull(offLine);
        Assert.NotNull(onLine);

        Assert.Contains("not run", offLine, StringComparison.Ordinal);

        Assert.DoesNotContain("not run", onLine, StringComparison.Ordinal);
        Assert.Contains("candidates", onLine, StringComparison.Ordinal);
        Assert.Contains("message(s)", onLine, StringComparison.Ordinal);
    }

    private static string? Line(string sheet)
        => sheet.Split('\n')
            .FirstOrDefault(l => l.StartsWith("portComparison", StringComparison.Ordinal))
            ?.Trim();

    private static string Sheet(MonoAudio audio, bool compare)
    {
        var heard = Ft8Reader.Read(
            audio, EndedAt, Measured, compareWithThePort: compare);

        return DigitalCaptureSheet.Compose(
            EndedAt,
            audio.Duration.TotalSeconds,
            audio.SampleRate,
            RigState.Empty,
            Measured,
            EndedAt,
            "20 m FT8",
            null,
            census: heard.Slots,
            refusal: heard.Refusal);
    }

    private static MonoAudio? Fixture()
    {
        var root = Path.Combine(Root(), "tests", "fixtures", "ft8");

        if (!Directory.Exists(root))
        {
            return null;
        }

        var files = Directory
            .EnumerateFiles(root, "*.wav", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        var file = files.FirstOrDefault(
            p => p.Contains(
                $"{Path.DirectorySeparatorChar}captured{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            ?? files.FirstOrDefault();

        return file is null ? null : WavAudio.Read(file);
    }

    private static string Root()
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
