using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A REPLY BEGINS WHERE THE OTHER ENDS** (work instruction 556, task 1, HM-DEC-260). Live at every section break of W1AW on
/// 2026-10-07 the terminal printed a block of junk: W1AW paused a second or two, was released, the terminal went to a sender
/// that had stood beside it, and that sender printed every letter it had accumulated since it stood, minutes of them. A
/// sender given the terminal now prints only what it sent since the printed one last keyed, less one word gap, and nothing
/// of its past where it stood longer than <see cref="Hamlet.RadioEngine.Cw.CwSenderGate.ReplyOverlapSeconds"/> while the
/// printed one kept keying.
/// </summary>
/// <remarks>Synthetic: 8 kHz, through the filter, read through the app's chain at a 600 Hz pitch and a 500 Hz filter.</remarks>
public sealed class AReplyBeginsWhereTheOtherEndsTests(ITestOutputHelper output)
{
    private const int Rate = 8000;

    private const string Half = "QST DE W1AW QST DE W1AW THE SPEEDS WILL RUN FROM TEN TO THIRTY FIVE WORDS A MINUTE AND THE TEXT IS FROM QST PAGE FIFTY ONE";

    /// <remarks>
    /// A strong station at 550 Hz, 25 WPM, about a minute, a two-second pause, about a minute more; and a second sender at 750
    /// Hz, 18 WPM and 10 dB weaker, keying beside it the whole time. Nothing of the second sender's past prints at the pause:
    /// no letter it keyed while the first was still sending reaches the terminal, and the first's second half prints.
    /// </remarks>
    [Fact]
    public void NothingStoodBesideAStationPrintsAtItsPause()
    {
        var first = AHandIsReadAgainstItselfTests.Keyed(Half, _ => new AHandIsReadAgainstItselfTests.Sending(25, 0), 5561, 24, 1, 550);
        var second = AHandIsReadAgainstItselfTests.Keyed(Half, _ => new AHandIsReadAgainstItselfTests.Sending(25, 0), 5562, 24, 1, 550);

        // Each keying runs from 3 s into its audio to 4 s before its end; the first is cut 1 s after it ends and the second
        // begins 1 s before it starts, so the station pauses 2 s.
        var cutA = first.Length - (3 * Rate);
        var fromB = 2 * Rate;
        var station = new float[cutA + second.Length - fromB];

        Array.Copy(first, station, cutA);
        Array.Copy(second, fromB, station, cutA, second.Length - fromB);

        var pauseFrom = (first.Length / (double)Rate) - 4;
        var pauseTo = pauseFrom + 2;
        var besideText = string.Concat(Enumerable.Repeat("CQ TEST DE K3ZZ K3ZZ ", 40));
        var beside = AHandIsReadAgainstItselfTests.Keyed(besideText, _ => new AHandIsReadAgainstItselfTests.Sending(18, 0), 5563, 14, 1, 750);
        var mixed = new float[station.Length];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = station[i] + (i < beside.Length ? beside[i] : 0);
        }

        var (letters, text) = TheRecordingsScoreboardTests.ReadLive(NarrownessReadsTheFiltersBandTests.ThroughTheFilter(mixed), Rate, 600, 500);
        var besideLetters = letters.Where(l => Math.Abs(l.PitchHz - 750) <= 40).ToList();
        var pastAtThePause = besideLetters.Where(l => l.Seconds < pauseFrom).ToList();
        var afterPause = string.Concat(letters.Where(l => Math.Abs(l.PitchHz - 550) <= 40 && l.Seconds > pauseTo).Select(l => l.Text));

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"pause {pauseFrom:0.0} to {pauseTo:0.0} s of {mixed.Length / (double)Rate:0.0} s"));
        output.WriteLine($"reads `{text}`");
        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"the second sender's letters printed: {besideLetters.Count}, keyed before the pause {pastAtThePause.Count}; first printed at {(besideLetters.Count > 0 ? besideLetters.Min(l => l.Seconds) : double.NaN):0.0} s: `{string.Concat(besideLetters.Select(l => l.Text))}`"));
        output.WriteLine($"the first after the pause: `{afterPause}`");

        Assert.Empty(pastAtThePause);
        Assert.Contains(Half.Replace(" ", string.Empty)[..20], afterPause);
    }
}
