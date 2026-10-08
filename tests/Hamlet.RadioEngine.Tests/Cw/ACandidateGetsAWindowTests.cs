using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A CANDIDATE GETS A WINDOW BEFORE IT STANDS** (work instruction 558, task 1, HM-DEC-262): the weak fast station and a weak
/// POTA call 200 Hz off the pitch, read through the app's chain, where candidates get windows by default since work
/// instruction 560.
/// </summary>
public sealed class ACandidateGetsAWindowTests(ITestOutputHelper output)
{
    private const int Rate = 8000;

    /// <remarks>Case 1: `cw-2026-10-03-143906`, about 26 WPM at 11 dB; a plain read gives `QSY ... DE WB2FU`. Asserts nothing.</remarks>
    [Fact]
    public void TheWeakFastStation()
    {
        var read = TheRecordingsScoreboardTests.ReadLive("cw-2026-10-03-143906");

        output.WriteLine($"143906 reads `{read.Text}`");
    }

    /// <remarks>
    /// Case 2: KM3STU's case, a 22 WPM CQ at 13 dB with its tone at 800 Hz, 200 Hz off the radio's 600 Hz pitch, through the
    /// 500 Hz filter with the bench's AGC, read at that pitch and filter. Asserts nothing; the reading is the result.
    /// </remarks>
    [Fact]
    public void AWeakPotaCallOffThePitch()
    {
        const string Sent = "CQ POTA DE KM3STU KM3STU K";
        var keyed = AHandIsReadAgainstItselfTests.Keyed(Sent, _ => new AHandIsReadAgainstItselfTests.Sending(22, 0), 5580, 13, 1, 800);
        var read = TheRecordingsScoreboardTests.ReadLive(NarrownessReadsTheFiltersBandTests.ThroughTheFilter(keyed), Rate, 600, 500);
        var first = read.Letters.Count > 0 ? read.Letters[0].Seconds : double.NaN;

        output.WriteLine($"`{Sent}` at 22 WPM, 13 dB, 800 Hz: reads `{read.Text}`; first letter printed ends at {first:0.00} s; the call's keying runs from 3.00 s to {(keyed.Length / (double)Rate) - 4:0.00} s");
    }
}
