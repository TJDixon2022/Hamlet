using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A REPLY IS READ FROM ITS FIRST LETTER** (work instruction 533, HM-DEC-237): the owner's recordings of 2026-10-03 on
/// 7.054 MHz, a QSO changing hands between two stations 100 Hz apart.
/// </summary>
/// <remarks>
/// R88 is lifted for the owner's five recordings, the one of 2026-10-02 and the four of 2026-10-03 (the owner,
/// 2026-10-03: *"add them to the next couple rounds"*). No other recording is read. Each is read through the live path as
/// the app wires it, as <see cref="TheOwnersRecordingReadsTests"/> reads the first.
/// </remarks>
public sealed class AReplyIsReadFromItsFirstLetterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading and its marks are printed.</param>
    public AReplyIsReadFromItsFirstLetterTests(ITestOutputHelper output) => _output = output;

    private string Report(string name, double fromSeconds = 0)
    {
        var (text, characters, marks, _, _) = TheOwnersRecordingReadsTests.Read(name);

        _output.WriteLine($"{name}: reads `{text}`");
        _output.WriteLine("characters: " + string.Concat(characters.Select(c => $"{c.Text}[{c.Pattern}]@{c.At.TotalSeconds:0.00} ")));

        foreach (var pitch in marks.Select(m => Math.Round(m.PitchHz / 25) * 25).Distinct().Order())
        {
            var own = marks.Where(m => Math.Abs(m.PitchHz - pitch) <= 12.5 && m.FromSeconds >= fromSeconds).OrderBy(m => m.FromSeconds).ToList();
            CwMark? last = null;
            var line = new List<string>();

            foreach (var m in own)
            {
                var gap = last is null ? double.NaN : (m.FromSeconds - last.ToSeconds) * 1000;

                line.Add(last is not null && gap > 105 ? $"[{gap:0}] {m.FromSeconds:0.00}:{m.LengthMs:0}" : $"{m.FromSeconds:0.00}:{m.LengthMs:0}");
                last = m;
            }

            _output.WriteLine($"  marks near {pitch:0} Hz ({own.Count}): {string.Join(" ", line)}");
        }

        return text;
    }

    /// <summary>Two senders through the filter, the second keyed <paramref name="startSeconds"/> after the first's audio begins.</summary>
    private static float[] Two(string first, double firstHz, string second, double secondHz, double startSeconds)
    {
        var a = AHandIsReadAgainstItselfTests.Keyed(first, _ => new AHandIsReadAgainstItselfTests.Sending(15, 0), 5330, 24, 1, firstHz);
        var b = AHandIsReadAgainstItselfTests.Keyed(second, _ => new AHandIsReadAgainstItselfTests.Sending(15, 0), 5331, 24, 1, secondHz);
        var offset = (int)(startSeconds * Rate);
        var mixed = new float[Math.Max(a.Length, offset + b.Length)];

        for (var i = 0; i < a.Length; i++)
        {
            mixed[i] += a[i];
        }

        for (var i = 0; i < b.Length; i++)
        {
            mixed[offset + i] += b[i];
        }

        return NarrownessReadsTheFiltersBandTests.ThroughTheFilter(mixed);
    }

    private const int Rate = 8000;

    /// <summary>The audio's length in seconds less the lead-in and the tail <see cref="AHandIsReadAgainstItselfTests.Keyed"/> adds: where its keying ends.</summary>
    private static double KeyingEnds(string text, double hz)
        => (AHandIsReadAgainstItselfTests.Keyed(text, _ => new AHandIsReadAgainstItselfTests.Sending(15, 0), 5330, 24, 1, hz).Length / (double)Rate) - 1 - 3;

    private static string Letters(string text) => text.Replace(" ", string.Empty);

    /// <remarks>
    /// Task 1, case 1. The QSO changing hands on 2026-10-03 at 14:40:20. The first station, at 500 Hz, keys its last mark, the
    /// dah closing its `<BT>`, at 11.07 s; the second, at 600 Hz, keys its first, the dit of its W, at 13.57 s, measured on the
    /// 600 Hz lane: nothing of it above the noise before that. Before this unit the terminal printed the first station and
    /// nothing after it, because the first station, silent and let go, kept the better shape and was picked again.
    /// <para>The letters are those of the marks at each pitch, read by hand from their lengths and gaps, and agree with the
    /// web session's element list. One letter is not read by Hamlet: the U at 24.09 s is a dit, a 40 ms gap, then 270 ms of
    /// tone with only a 2 dB notch at 24.29 s where its second gap should be. The dit stands; the two pieces of the tone fail
    /// the single-mark shape, a 2 dB notch not being a key edge, and the letter prints as E. Spaces are ignored.</para>
    /// </remarks>
    [Fact]
    public void TheReplyOnTheOwnersRecordingIsReadFromItsFirstLetter()
    {
        var text = Report("cw-2026-10-03-144020", 13.5);

        Assert.Equal(Letters(Replied), Letters(text));
    }

    /// <summary>What the 14:40:20 recording holds, as measured from its marks (task 1, case 1).</summary>
    internal const string Replied = "ES OK ON PA <BT> WX I N N E T A G I T I U N TEMP";

    /// <remarks>
    /// Task 2. The next recording, 14:40:45, one station at 600 Hz. It opens on the dah and dit of the N closing `IUN`, which
    /// the recording before ends on, and continues `TEMP 57 57 <BT> B TU BOB DE KG8V K`, as the web session measured and as
    /// the marks that stood read by hand. Spaces are ignored: they are reported, not asserted.
    /// </remarks>
    [Fact]
    public void TheNextOverReadsItsLetters()
    {
        var text = Report("cw-2026-10-03-144045");

        Assert.Equal(Letters(NextOver), Letters(text));
    }

    /// <summary>What the 14:40:45 recording holds (task 2).</summary>
    internal const string NextOver = "N TEMP 57 57 <BT> B TU BOB DE KG8V K";

    /// <remarks>
    /// Task 1, case 2. A QSO in miniature: two clean 15 WPM senders 100 Hz apart through the filter with a 1 dB overshoot,
    /// the second starting two seconds before the first finishes. Both are read whole, the first and then the second.
    /// </remarks>
    [Fact]
    public void ASyntheticQsoReadsBothSendersInOrder()
    {
        const string First = "CQ CQ DE W1AW W1AW K";
        const string Second = "W1AW DE K3ZZ K3ZZ K";

        // The second's first mark is 3 s into its own audio.
        var start = KeyingEnds(First, 550) - 2 - 3;
        var r = ThePatternIsTheGateTests.Read(Two(First, 550, Second, 650, start), 650);

        _output.WriteLine($"second starts {start + 3:0.00} s, first ends {KeyingEnds(First, 550):0.00} s; reads `{r.Text}`");

        Assert.Equal(Letters(First + Second), Letters(r.Text));
    }

    /// <remarks>
    /// Task 1, case 3. A sender standing while another prints, which stops before the printed one does and so never takes
    /// the terminal. It prints nothing: a sender's backlog is printed only when the terminal passes to it. Reported, not changed.
    /// </remarks>
    [Fact]
    public void ASenderThatNeverTakesOverPrintsNothing()
    {
        const string Printed = "CQ CQ CQ DE W1AW W1AW W1AW K";
        const string Beside = "TEST TEST";

        var r = ThePatternIsTheGateTests.Read(Two(Printed, 550, Beside, 650, 6), 650);

        _output.WriteLine($"beside ends {6 + KeyingEnds(Beside, 650):0.00} s, printed ends {KeyingEnds(Printed, 550):0.00} s; reads `{r.Text}`");

        Assert.Equal(Letters(Printed), Letters(r.Text));
    }

    /// <remarks>What each of the four recordings of 2026-10-03 reads, with every mark that stood by pitch. Asserts nothing.</remarks>
    [Fact]
    public void WhatTheRecordingsRead()
    {
        foreach (var name in new[] { "cw-2026-10-03-144020", "cw-2026-10-03-144045", "cw-2026-10-03-143951", "cw-2026-10-03-143906" })
        {
            Report(name);
        }
    }
}
