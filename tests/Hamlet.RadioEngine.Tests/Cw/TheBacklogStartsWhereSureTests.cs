using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE BACKLOG STARTS WHERE HAMLET BECAME SURE** (work instruction 562, HM-DEC-266): the junk before a weak station's call
/// was its backlog, the marks heard before Hamlet was sure of it, printed all at once when it was first picked. The owner,
/// 2026-10-08: yes; a station's backlog starts from the mark at which its shape first reached the green line, 0.4.
/// Built, these went from red to the readings in `docs\cw-scoreboard.md`'s 562 row, but the rule read 285 against 297 and
/// broke the first recording, so it was not shipped. They print what each case reads and assert nothing until the owner
/// rules again.
/// </summary>
public sealed class TheBacklogStartsWhereSureTests(ITestOutputHelper output)
{
    /// <remarks>`143906` prints `QSY DE WB2FU` with nothing before it; at HEAD it opened `IEE I I E NUVEE T`.</remarks>
    [Fact]
    public void TheWeakFastStationOpensOnItsCall()
    {
        var text = TheRecordingsScoreboardTests.ReadLive("cw-2026-10-03-143906").Text;

        output.WriteLine($"143906 reads `{text}`");
    }

    /// <remarks>`121324` prints `NOTA DE KM3STU K KQ4PAK` with no `I` before it.</remarks>
    [Fact]
    public void Km3stuOpensOnHisCall()
    {
        var text = TheRecordingsScoreboardTests.ReadLive("cw-2026-10-08-121324").Text;

        output.WriteLine($"121324 reads `{text}`");
    }

    /// <remarks>A synthetic weak caller at 8 dB strengthening to 14 dB prints its call with no `N EQ` before it.</remarks>
    [Fact]
    public void AWeakCallerOpensOnItsCall()
    {
        var text = AStationPrintsOnceSureTests.AWeakCallerStrengthening(8);

        output.WriteLine($"the weak caller, 8 dB then 14 dB, reads `{text}`");
    }
}
