using System.Text.RegularExpressions;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The one place where the shapes read out of the pin and the constants the port actually runs on
/// are held against each other.
/// </summary>
/// <remarks>
/// <para>
/// <c>UpstreamSyncSearchInventoryTests</c> is the record of the read and stands on its own;
/// <see cref="Ft8SyncSearch"/> is the port. <b>Between them is the only place a value could drift
/// without anything going red</b> — someone edits a default, the inventory still passes because
/// upstream has not changed, and the port quietly searches a different range or keeps a different
/// number of candidates. These assertions close that gap.
/// </para>
/// <para>
/// <b>This is also where the two application constants are pinned</b>, deliberately, rather than in
/// the inventory: <c>kMin_score</c> and <c>kMax_candidates</c> are the demo program's judgement and
/// not FT8's, so the claim worth asserting is not <i>upstream says 10</i> but <i>this port's default
/// is still whatever upstream's application says</i>.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone stays green.</para>
/// </remarks>
public class Ft8SyncSearchProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SyncSearchProvenanceTests(ITestOutputHelper output) => _output = output;

    [RequiresReferenceCloneFact]
    public void ThePortsSyncGeometryIsThePinsSyncGeometry()
    {
        var constants = ReadSource(@"ft8\constants.h");

        // The port's constant is the 'expected' argument only because xunit's analyser requires a
        // constant there; the claim is symmetric and is that the two agree.
        Assert.Equal(Ft8SyncSearch.SyncGroupLength, ReadMacro(constants, "FT8_LENGTH_SYNC"));
        Assert.Equal(Ft8SyncSearch.SyncGroupCount, ReadMacro(constants, "FT8_NUM_SYNC"));
        Assert.Equal(Ft8SyncSearch.SyncGroupOffset, ReadMacro(constants, "FT8_SYNC_OFFSET"));

        // The Costas array comes from the generated tables file and is never re-transcribed here.
        Assert.Equal(Ft8SyncSearch.SyncGroupLength, Ft8Tables.Ft8CostasPattern.Length);

        _output.WriteLine(
            $"  sync: {Ft8SyncSearch.SyncGroupCount} groups of {Ft8SyncSearch.SyncGroupLength}, "
            + $"{Ft8SyncSearch.SyncGroupOffset} symbols apart - the port's constants, checked "
            + "against the pin's macros.");
    }

    [RequiresReferenceCloneFact]
    public void ThePortsBlockOffsetSweepIsThePinsBlockOffsetSweep()
    {
        var find = ExtractFunctionBody(ReadSource(@"ft8\decode.c"), "ftx_find_candidates");
        var range = Regex.Match(
            find, @"candidate\.time_offset\s*=\s*(-?\d+);\s*candidate\.time_offset\s*<\s*(-?\d+)");
        Assert.True(range.Success, "the time offset sweep is no longer a literal range.");

        Assert.Equal(Ft8SyncSearch.DefaultFirstBlockOffset, int.Parse(range.Groups[1].Value));
        Assert.Equal(Ft8SyncSearch.DefaultLastBlockOffset, int.Parse(range.Groups[2].Value) - 1);

        _output.WriteLine(
            $"  block offsets {Ft8SyncSearch.DefaultFirstBlockOffset} .. "
            + $"{Ft8SyncSearch.DefaultLastBlockOffset} inclusive, which is the pin's half-open range "
            + "written closed.");
    }

    [RequiresReferenceCloneFact]
    public void ThePortsDefaultsForTheTwoApplicationConstantsAreStillTheApplicationsOwn()
    {
        var demo = ReadSource(@"demo\decode_ft8.c");

        var minScore = Regex.Match(demo, @"const\s+int\s+kMin_score\s*=\s*(\d+)\s*;");
        var maxCandidates = Regex.Match(demo, @"const\s+int\s+kMax_candidates\s*=\s*(\d+)\s*;");
        Assert.True(minScore.Success && maxCandidates.Success);

        Assert.Equal(Ft8SyncSearch.DefaultMinimumScore, int.Parse(minScore.Groups[1].Value));
        Assert.Equal(Ft8SyncSearch.DefaultCandidateLimit, int.Parse(maxCandidates.Groups[1].Value));

        // And a default-constructed search really uses them, rather than carrying them as decoration.
        var search = new Ft8SyncSearch();
        Assert.Equal(Ft8SyncSearch.DefaultMinimumScore, search.MinimumScore);
        Assert.Equal(Ft8SyncSearch.DefaultCandidateLimit, search.CandidateLimit);
        Assert.Equal(Ft8SyncSearch.DefaultFirstBlockOffset, search.FirstBlockOffset);
        Assert.Equal(Ft8SyncSearch.DefaultLastBlockOffset, search.LastBlockOffset);

        _output.WriteLine(
            $"  minimum score {search.MinimumScore}, candidate limit {search.CandidateLimit} - both "
            + "the demo application's, both parameters here, both still equal to the pin's.");
    }

    private static int ReadMacro(string source, string macro)
    {
        var match = Regex.Match(source, $@"#define\s+{macro}\s*\((\d+)\)");
        Assert.True(match.Success, $"{macro} is no longer a macro in the source read.");
        return int.Parse(match.Groups[1].Value);
    }

    private static string ExtractFunctionBody(string source, string name)
    {
        var head = Regex.Match(
            source,
            $@"^[A-Za-z_][A-Za-z0-9_ \t\*]*\b{Regex.Escape(name)}\s*\([^;{{]*\)\s*\{{",
            RegexOptions.Multiline);
        Assert.True(head.Success, $"{name} is no longer defined in the source read.");

        var depth = 0;
        var start = head.Index + head.Length - 1;
        for (var i = start; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(i + 1)];
                }
            }
        }

        Assert.Fail($"{name} has no closing brace.");
        return string.Empty;
    }

    private static string ReadSource(string relative)
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail($"{ReferenceClone.Location} exists but could not be read: {detail}.");
        }

        var path = Path.Combine(ReferenceClone.Location, relative);
        Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
        return File.ReadAllText(path);
    }
}
