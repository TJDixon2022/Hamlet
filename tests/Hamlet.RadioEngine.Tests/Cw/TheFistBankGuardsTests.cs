using System.Collections.Concurrent;
using System.Globalization;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SHORT-DAH ROWS OF THE FIST BANK HOLD** (the owner's ruling on HM-DEC-285, 2026-10-20): <see cref="Hamlet.RadioEngine.Cw.CwRules.ShortDah"/>
/// ships on with its dit floor, and the bank's 1.6 and 2.0 rows, which read 56 and 79 of 1440 before it, read 569 and 624 at the
/// ship. The guard holds each row at that figure less 5%.
/// </summary>
/// <remarks>Each fist is generated from its recipe and read and scored as the survey reads it (<see cref="TheCwSurveyTests.Fist"/>).</remarks>
public sealed class TheFistBankGuardsTests(ITestOutputHelper output)
{
    /// <summary>The 1.6 and 2.0 rows' letters right of 1440 each at the ship.</summary>
    internal static readonly IReadOnlyDictionary<double, int> Shipped = new Dictionary<double, int> { [1.6] = 569, [2.0] = 624 };

    /// <summary>A row's floor: its shipped figure less 5%, rounded down.</summary>
    internal static int Floor(double ratio) => (int)Math.Floor(0.95 * Shipped[ratio]);

    [Fact]
    public void TheShortDahRowsHold()
    {
        var fists = CwFistBank.All.Where(f => Shipped.ContainsKey(f.Ratio)).ToList();
        var rows = new ConcurrentBag<(double Ratio, int Right)>();

        Parallel.ForEach(fists, new ParallelOptions { MaxDegreeOfParallelism = 3 }, fist => rows.Add((fist.Ratio, TheCwSurveyTests.Fist(fist).Right ?? 0)));

        foreach (var ratio in Shipped.Keys)
        {
            var right = rows.Where(r => r.Ratio == ratio).Sum(r => r.Right);

            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"dah over dit {ratio:0.0}: {right} of {fists.Count(f => f.Ratio == ratio) * 60}, floor {Floor(ratio)}, shipped {Shipped[ratio]}"));
            Assert.True(right >= Floor(ratio), string.Create(CultureInfo.InvariantCulture, $"the {ratio:0.0} row read {right}, under its floor of {Floor(ratio)}"));
        }
    }
}
