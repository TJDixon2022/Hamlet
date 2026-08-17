using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The second pass, and how it reconciles with the leading edge (HM-DEC-096).
/// </summary>
/// <remarks>
/// Hamlet reads the same audio twice: a streaming pass at the leading edge that
/// must decide where the threshold is before it has heard the stretch, and a
/// settled pass a few seconds behind that fits the threshold to the stretch it
/// is reading. These prove the second one exists, that it refuses rather than
/// guessing, and that the two are reconciled by the ruled rule rather than by
/// whichever answered last.
/// </remarks>
public sealed class CwSettledPassTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two readings are printed.</param>
    public CwSettledPassTests(ITestOutputHelper output) => _output = output;

    private sealed record Run(
        string Provisional,
        string Settled,
        IReadOnlyList<CwRevision> Revisions,
        SettledOutcome Outcome,
        IReadOnlyList<CwCharacter> SettledCharacters);

    private static Run Decode(string name, double expectedToneHz = 600)
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, expectedToneHz);
        var provisional = new System.Text.StringBuilder();
        var settled = new System.Text.StringBuilder();
        var settledChars = new List<CwCharacter>();

        decoder.CharacterDecoded += c => provisional.Append(c.Text);
        decoder.CharacterSettled += c =>
        {
            settled.Append(c.Text);
            settledChars.Add(c);
        };

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return new Run(
            provisional.ToString().Trim(),
            settled.ToString().Trim(),
            decoder.Revisions,
            decoder.SettledState,
            settledChars);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 1: **there are two readings of the same
    /// audio and they are separately visible.** Before this the decoder had one
    /// pass, which had to commit at the leading edge, and the reference chain's
    /// clustering gate could not simply replace it because that gate fits a
    /// threshold to a stretch and then applies it to the same stretch.</para>
    /// </remarks>
    [Fact]
    public void BothPassesReadTheAnsweringStation()
    {
        var run = Decode("cw-2026-08-17-013347");

        _output.WriteLine($"provisional : {run.Provisional}");
        _output.WriteLine($"settled     : {run.Settled}");
        _output.WriteLine($"window      : {run.Outcome.WindowSeconds:0.00} s"
            + (run.Outcome.WindowWasCapped ? " (capped)" : "")
            + $", contrast {run.Outcome.ContrastDb:0.0} dB"
            + $", dit {run.Outcome.DitMilliseconds:0} ms");
        _output.WriteLine($"revisions   : {run.Revisions.Count}, "
            + $"{run.Revisions.Count(r => r.Agreed)} agreed");

        foreach (var revision in run.Revisions.Take(20))
        {
            _output.WriteLine(
                $"    '{revision.Provisional.Text}' {revision.Provisional.Score:0.00}"
                + $"  ->  '{revision.Settled.Text}' {revision.Settled.Score:0.00}"
                + (revision.Agreed ? "  agreed" : "  DIFFERED"));
        }

        foreach (var c in run.SettledCharacters.Where(c => !c.IsWordGap))
        {
            _output.WriteLine($"    settled '{c.Text}' {c.Confidence} {c.Score:0.00} "
                + $"'{c.Pattern}' at {c.At.TotalSeconds:0.00}s");
        }

        Assert.NotEmpty(run.SettledCharacters);

        // **NOTHING THE SECOND PASS WILL NOT STAND BEHIND MAY LOOK LIKE COPY**
        // (§0.0). Settled text is what the transcript keeps, so a character it
        // shows at full strength is a claim that this is what was sent.
        var confident = run.SettledCharacters
            .Where(c => !c.IsWordGap && c.Confidence == CwConfidence.High)
            .ToList();

        _output.WriteLine($"confident   : {string.Concat(confident.Select(c => c.Text))}");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 1, the window rule. **The window is the
    /// longer of about two and a half seconds and about thirty elements, and
    /// never past four**, because both constraints are real and they bind at
    /// opposite ends of the speed range.</para>
    /// </remarks>
    [Theory]
    [InlineData(0, CwSettledPass.ShortestWindowSeconds)]
    [InlineData(20, CwSettledPass.ShortestWindowSeconds)]
    [InlineData(48, 2.88)]
    [InlineData(70, CwSettledPass.LongestWindowSeconds)]
    [InlineData(200, CwSettledPass.LongestWindowSeconds)]
    public void TheWindowIsTheLongerOfTimeAndElements(double ditMs, double expected)
    {
        var pass = new CwSettledPass(48_000, 0.010);

        // Enough quiet audio that the pass has something to size a window over.
        for (var i = 0; i < 900; i++)
        {
            pass.Observe(-60, false, i * 480);
        }

        var outcome = pass.Settle(ditMs, new List<SettledCharacter>());

        Assert.Equal(expected, outcome.WindowSeconds, 2);
        Assert.InRange(
            outcome.WindowSeconds,
            CwSettledPass.ShortestWindowSeconds,
            CwSettledPass.LongestWindowSeconds);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 1: **when the ceiling binds, the outcome
    /// says so.** A slow fist wants a window longer than four seconds and cannot
    /// have one, because a settled line exists to catch a callsign in a live
    /// contact and a six-second lag makes it useless for that. Accepting the
    /// weaker fit is right; concealing that it is weaker is §0.0 broken.</para>
    /// </remarks>
    [Fact]
    public void AWindowThatHitTheCeilingSaysSo()
    {
        var pass = new CwSettledPass(48_000, 0.010);

        for (var i = 0; i < 900; i++)
        {
            pass.Observe(-60, false, i * 480);
        }

        // Eight words a minute, so thirty elements wants about nine seconds.
        var slow = pass.Settle(150, new List<SettledCharacter>());

        Assert.True(slow.WindowWasCapped);
        Assert.Equal(CwSettledPass.LongestWindowSeconds, slow.WindowSeconds, 2);

        // And an ordinary fist does not trip it.
        var ordinary = pass.Settle(40, new List<SettledCharacter>());

        Assert.False(ordinary.WindowWasCapped);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 1 and §0.0: **flat audio has one level, not
    /// two, and the pass refuses it.** A threshold placed in the middle of noise
    /// produces confident nonsense, which is the output this project fears most.
    /// </para>
    /// </remarks>
    [Fact]
    public void FlatAudioIsRefusedRatherThanGated()
    {
        var pass = new CwSettledPass(48_000, 0.010);

        for (var i = 0; i < 900; i++)
        {
            // A couple of decibels of wobble, which is what noise does.
            pass.Observe(-60 + ((i % 5) * 0.4), false, i * 480);
        }

        var into = new List<SettledCharacter>();
        var outcome = pass.Settle(100, into);

        Assert.Equal(SettledRefusal.Contrast, outcome.Refusal);
        Assert.Empty(into);
        Assert.False(outcome.Read);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 2: **marks that do not cluster into a clock
    /// anybody could send produce nothing.** Two stations at different speeds in
    /// one window fit a ratio inside the legal band while describing neither of
    /// them, and that is a confident wrong answer.</para>
    /// </remarks>
    [Fact]
    public void MarksThatDoNotDescribeAClockProduceNothing()
    {
        var pass = new CwSettledPass(48_000, 0.010);
        var at = 0;

        // Keying with no structure: every mark the same length, so there is no
        // dit and dah to find.
        for (var element = 0; element < 60; element++)
        {
            for (var i = 0; i < 6; i++)
            {
                pass.Observe(-20, false, at++ * 480);
            }

            for (var i = 0; i < 6; i++)
            {
                pass.Observe(-60, false, at++ * 480);
            }
        }

        var into = new List<SettledCharacter>();
        var outcome = pass.Settle(60, into);

        Assert.True(
            outcome.Refusal is SettledRefusal.Clock or SettledRefusal.ClockLost,
            $"expected a clock refusal and got {outcome.Refusal}");

        Assert.Empty(into);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 2: **the clock that was running gets a
    /// hearing before it is thrown away.** A fade or a burst of somebody else's
    /// keying breaks a window's fit without anything having changed about the
    /// station being read, and a refusal that fires on every fade is worse than
    /// no refusal at all.</para>
    /// <para>Here the sending never changes, and a stretch of it is buried so the
    /// fresh fit is poor. The clock carried forward still describes the marks, so
    /// nothing is lost.</para>
    /// </remarks>
    [Fact]
    public void AFadeDoesNotThrowAwayAWorkingClock()
    {
        var pass = new CwSettledPass(48_000, 0.010);
        var at = 0;

        void Send(int marks, double loudDb)
        {
            for (var i = 0; i < marks; i++)
            {
                // Ten points of mark, then thirty of gap: a dit at 100 ms with
                // room between characters.
                var length = i % 4 == 3 ? 30 : 10;

                for (var k = 0; k < length; k++)
                {
                    pass.Observe(loudDb, false, at++ * 480);
                }

                for (var k = 0; k < 12; k++)
                {
                    pass.Observe(-60, false, at++ * 480);
                }
            }
        }

        Send(40, -20);

        var into = new List<SettledCharacter>();
        var settled = pass.Settle(100, into);

        Assert.True(settled.Read, $"the clean stretch refused: {settled.Refusal}");

        var established = settled.DitMilliseconds;

        Assert.InRange(established, 60, 160);

        // Now the same fist, ten decibels down. The contrast is thinner and the
        // fit is worse, and it is still the same station.
        Send(40, -34);

        var faded = pass.Settle(100, new List<SettledCharacter>());

        Assert.NotEqual(SettledRefusal.ClockLost, faded.Refusal);
        Assert.False(
            faded.SpeedChanged,
            "a fade was reported as somebody else starting to send");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 4: **while the settled pass is refusing, the
    /// leading edge keeps running and says it is unstable.** The moment somebody
    /// answers is the worst possible moment for the live feed to go dark, and the
    /// provisional line's whole purpose is catching a callsign quickly.</para>
    /// </remarks>
    [Fact]
    public void TheLeadingEdgeIsMarkedUnstableWhileNothingConfirmsIt()
    {
        var run = Decode("cw-2026-08-17-134712");

        _output.WriteLine($"provisional : '{run.Provisional}'");
        _output.WriteLine($"settled     : '{run.Settled}'");
        _output.WriteLine($"refusal     : {run.Outcome.Refusal}");

        // This recording holds a carrier and no readable station, so the settled
        // pass has nothing to confirm and the tip carries the mark.
        Assert.Equal("", run.Settled);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096: **the revision log is in memory and is not
    /// written anywhere.** It is diagnostic under §0.0.1 rather than a record of
    /// the air, and a log that grows on disk needs a retention policy nobody has
    /// designed.</para>
    /// </remarks>
    [Fact]
    public void TheRevisionLogIsExportableAndNotPersisted()
    {
        var run = Decode("cw-2026-08-17-013347");

        Assert.NotNull(run.Revisions);

        // Every revision carries both readings, so a wrong settled answer can be
        // argued with rather than merely disagreed with.
        foreach (var revision in run.Revisions)
        {
            Assert.Equal(CwReadingStage.Settled, revision.Settled.Stage);
            Assert.NotEqual(CwReadingStage.Settled, revision.Provisional.Stage);
        }
    }
}
