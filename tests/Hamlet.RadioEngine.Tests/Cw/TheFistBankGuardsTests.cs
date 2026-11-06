using System.Collections.Concurrent;
using System.Globalization;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE FIST BANK HOLDS** (the owner's ruling on HM-DEC-295, 2026-10-27): the sequence reader ships on, and every row of the
/// bank, dah over dit 1.6, 2.0, 2.5 and 3.0 over its 24 hands, and the fading half over its 48, hold at the figures the reader
/// read at the ship less 5%. It supersedes the guard on the 1.6 and 2.0 rows alone, set by the owner's ruling on HM-DEC-285 at
/// 569 and 624 when the short dah shipped; the reader reads those rows at 990 and 1182. **Restated by HM-DEC-304** (work instruction 599,
/// the owner's ruling of 2026-11-06): the learned reader ships on, and every row holds at what it reads less 5%, the four rows 1212, 1359,
/// 1372 and 1346 (were 990, 1182, 1143 and 1146) and the fading half 2544 (was 1771).
/// </summary>
/// <remarks>Each fist is generated from its recipe and read and scored as the survey reads it (<see cref="TheCwSurveyTests.Fist"/>).</remarks>
public sealed class TheFistBankGuardsTests(ITestOutputHelper output)
{
    /// <summary>Each row's letters right of 1440 at the ship.</summary>
    internal static readonly IReadOnlyDictionary<double, int> Shipped = new Dictionary<double, int> { [1.6] = 1212, [2.0] = 1359, [2.5] = 1372, [3.0] = 1346 };

    /// <summary>The fading half's letters right of 2880 at the ship.</summary>
    internal const int FadingShipped = 2544;

    /// <summary>A figure's floor: less 5%, rounded down.</summary>
    internal static int Floor(int shipped) => (int)Math.Floor(0.95 * shipped);

    [Fact]
    public void TheBankHolds()
    {
        var rows = new ConcurrentBag<(CwFist Fist, int Right)>();

        Parallel.ForEach(CwFistBank.All, new ParallelOptions { MaxDegreeOfParallelism = 3 }, fist => rows.Add((fist, TheCwSurveyTests.Fist(fist).Right ?? 0)));

        foreach (var (ratio, shipped) in Shipped)
        {
            var right = rows.Where(r => r.Fist.Ratio == ratio).Sum(r => r.Right);

            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"dah over dit {ratio:0.0}: {right} of {rows.Count(r => r.Fist.Ratio == ratio) * 60}, floor {Floor(shipped)}, shipped {shipped}"));
            Assert.True(right >= Floor(shipped), string.Create(CultureInfo.InvariantCulture, $"the {ratio:0.0} row read {right}, under its floor of {Floor(shipped)}"));
        }

        var fading = rows.Where(r => r.Fist.Fades).Sum(r => r.Right);

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"the fading half: {fading} of {rows.Count(r => r.Fist.Fades) * 60}, floor {Floor(FadingShipped)}, shipped {FadingShipped}"));
        Assert.True(fading >= Floor(FadingShipped), string.Create(CultureInfo.InvariantCulture, $"the fading half read {fading}, under its floor of {Floor(FadingShipped)}"));
    }
}
