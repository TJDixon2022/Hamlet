using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Explore;
using Hamlet.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 252, task 5: what the entity table actually resolves on the
/// callsigns this tree already holds.
/// </summary>
/// <remarks>
/// <para>**IT IS THE ONLY HONEST WAY TO KNOW WHETHER *SAY NOTHING* IS QUIET OR
/// DEAFENING.** A table that declines nine calls in ten obeys the ruling
/// perfectly and is useless, and nothing about reading the code would say so. The
/// instruction asks for the share and the ten commonest declines, and it says
/// plainly: measure and report, **do not extend the table on the strength of
/// it.** Nothing here changes a row.</para>
/// <para>**THE CORPUS IS REAL OFF-AIR AUDIO AND NOT THIS PROJECT'S OWN
/// SYNTHESIS.** Karlis Goba's recordings ship with the messages WSJT-X read from
/// them, so these are callsigns that were genuinely on the air on 2019-11-11 —
/// which is the difference between measuring the table and measuring the
/// fixture (§12.5). The tree's own FT8 fixture holds three synthesised rows using
/// the example callsigns `K1ABC` and `W9XYZ`, and those are counted separately
/// because they are not evidence about anything.</para>
/// <para>**IT SKIPS RATHER THAN FAILS WHERE THE CLONE IS ABSENT.** The recordings
/// are not committed, and a machine without them is not a defect.</para>
/// </remarks>
public sealed class WhatTheEntityTableResolvesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the census is printed.</param>
    public WhatTheEntityTableResolvesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The resolve rate over every callsign in the off-air corpus.</summary>
    [Fact]
    public void TheTableIsMeasuredAgainstRealOffAirCallsigns()
    {
        if (!OffAirRecordings.Present)
        {
            _output.WriteLine(
                "The pinned clone is not on this machine, so there is nothing to "
                + "measure. That is not a defect; see the class remarks.");
            return;
        }

        var calls = new List<string>();

        foreach (var path in Directory.GetFiles(
            OffAirRecordings.WavLocation, "*.txt", SearchOption.AllDirectories))
        {
            foreach (var line in File.ReadAllLines(path))
            {
                calls.AddRange(CallsignsIn(line));
            }
        }

        Report("Karlis Goba off-air recordings", calls);

        // **NOT AN ASSERTION ABOUT THE RATE.** The instruction says measure and
        // report; a threshold here would turn a measurement into a target and
        // invite the next session to extend the table until it passed.
        Assert.NotEmpty(calls);
    }

    /// <summary>The tree's own synthesised fixture, counted apart.</summary>
    /// <remarks>
    /// **THREE ROWS AND THEY PROVE NOTHING ABOUT THE TABLE.** `K1ABC` and `W9XYZ`
    /// are the example callsigns the FT8 documentation uses; that they resolve is
    /// a fact about the United States block being large, not about coverage.
    /// </remarks>
    [Fact]
    public void TheTreesOwnFixtureIsCountedApart()
    {
        var path = Path.Combine(
            RepoRoot(), "tests", "fixtures", "ft8", "example",
            "ft8-example-244.fixture.txt");

        if (!File.Exists(path))
        {
            _output.WriteLine("no example fixture at " + path);
            return;
        }

        var calls = new List<string>();

        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith("ROW", StringComparison.Ordinal))
            {
                calls.AddRange(CallsignsIn(line));
            }
        }

        Report("the tree's own synthesised fixture", calls);
    }

    /// <summary>Print the census for one corpus.</summary>
    private void Report(string what, List<string> calls)
    {
        var resolved = new List<string>();
        var declined = new List<string>();

        foreach (var call in calls)
        {
            if (DxccPrefixes.EntityOf(call) is not null)
            {
                resolved.Add(call);
            }
            else
            {
                declined.Add(call);
            }
        }

        var total = calls.Count;
        var share = total == 0 ? 0.0 : 100.0 * resolved.Count / total;

        _output.WriteLine("=== " + what + " ===");
        _output.WriteLine("callsigns seen : " + total
            + "  (distinct " + calls.Distinct(StringComparer.Ordinal).Count() + ")");
        _output.WriteLine("resolved       : " + resolved.Count
            + "  (" + share.ToString("0.0") + "%)");
        _output.WriteLine("declined       : " + declined.Count);

        var commonest = declined
            .GroupBy(c => Stem(c), StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Take(10);

        _output.WriteLine("ten commonest declines, by leading characters:");

        var any = false;

        foreach (var group in commonest)
        {
            any = true;
            _output.WriteLine(
                "   " + group.Key.PadRight(6) + " " + group.Count()
                + "   e.g. " + string.Join(", ", group.Distinct(StringComparer.Ordinal).Take(3)));
        }

        if (!any)
        {
            _output.WriteLine("   none - every callsign resolved");
        }

        _output.WriteLine("");
    }

    /// <summary>The leading letters and digits a decline groups under.</summary>
    /// <remarks>
    /// **GROUPED BY WHAT WOULD HAVE BEEN LOOKED UP**, which is the leading
    /// characters up to and including the first digit run. Grouping by the whole
    /// callsign would make every decline unique and the list useless to whoever
    /// has to decide whether the table is worth extending.
    /// </remarks>
    private static string Stem(string call)
    {
        var match = Regex.Match(call, "^[A-Z0-9]?[A-Z]*[0-9]+");

        return match.Success ? match.Value : call;
    }

    /// <summary>Every callsign-shaped token on a line of expected messages.</summary>
    /// <remarks>
    /// <para>**THE LAST THREE FIELDS ARE THE MESSAGE.** A line reads
    /// `110115   6  0.9 1234 ~  GJ0KYZ RK9AX MO05`, so everything before the `~`
    /// is WSJT-X's own columns and only what follows is what was sent.</para>
    /// <para>**`CQ`, GRIDS AND REPORTS ARE NOT CALLSIGNS.** A grid is two letters
    /// and two digits and would otherwise be counted as a decline, which would
    /// make the table look far worse than it is.</para>
    /// </remarks>
    private static IEnumerable<string> CallsignsIn(string line)
    {
        var at = line.IndexOf('~');
        var body = at >= 0 ? line[(at + 1)..] : line;

        foreach (var token in body.Split(
            ' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = token.Trim().ToUpperInvariant();

            if (t is "CQ" or "DX" or "RRR" or "RR73" or "73" or "QRZ" or "TU")
            {
                continue;
            }

            // A report, with or without its roger.
            if (Regex.IsMatch(t, "^R?[+-][0-9]{1,2}$"))
            {
                continue;
            }

            // A four or six character grid.
            if (Regex.IsMatch(t, "^[A-R]{2}[0-9]{2}([A-X]{2})?$"))
            {
                continue;
            }

            // A callsign has at least one digit and at least one letter.
            if (Regex.IsMatch(t, "^[A-Z0-9/]{3,}$")
                && t.Any(char.IsAsciiDigit)
                && t.Any(char.IsAsciiLetter))
            {
                yield return t;
            }
        }
    }

    /// <summary>The repository root, walking up from the test binary.</summary>
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
