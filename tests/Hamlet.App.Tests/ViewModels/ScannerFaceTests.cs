using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// What the scanner puts on screen (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// <para>Most of this phase is verified at the screen rather than here, which is
/// why it waited for Tim. What can be proved by test is proved by test anyway,
/// and these are the claims §0.0 and §0.2.1 make about the words rather than
/// about the pixels.</para>
/// </remarks>
public sealed class ScannerFaceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public ScannerFaceTests(ITestOutputHelper output) => _output = output;

    private static ScanDwell Dwell(long hz, string heard, double score = 0.95)
    {
        var dwell = new ScanDwell(hz);

        foreach (var c in heard)
        {
            dwell.Take(c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 20, 18,
                    TimeSpan.Zero)
                : new CwCharacter(
                    c.ToString(),
                    score >= 0.7 ? CwConfidence.High : CwConfidence.Low,
                    score, ".-", 20, 18, TimeSpan.Zero));
        }

        return dwell;
    }

    private static ScanDwellRow Row(ScanDwell dwell)
    {
        var stopped = dwell.Decide(dwell.Seconds) == DwellAction.Stay;

        return new ScanDwellRow(
            dwell.FrequencyHz,
            $"{dwell.FrequencyHz / 1_000_000.0:0.000} MHz",
            dwell.Verdict.Sentence,
            stopped,
            stopped ? dwell.Verdict.Confidence : null);
    }

    /// <remarks>
    /// <para>Proves §0.2.1 and §0.5: **a place the scan listened to and found
    /// nobody at still says so.** A scan whose record holds only its stops
    /// cannot be told from one that never ran, and the frequencies it passed
    /// over are half of what it measured. Hiding detail is fine and hiding
    /// information is not.</para>
    /// </remarks>
    [Fact]
    public void APlaceThatHeldNobodyStillSaysWhereItWasAndWhatWasHeard()
    {
        var row = Row(Dwell(7_030_000, "XZ QJ WY"));

        _output.WriteLine($"{row.Label}: {row.Sentence}");

        Assert.False(row.Stopped);
        Assert.Contains("7.030", row.Label, StringComparison.Ordinal);
        Assert.NotEqual("", row.Sentence);

        // And it makes no claim about how sure it is, because there is nothing
        // to be sure of.
        Assert.False(row.HasSureness);
    }

    /// <remarks>
    /// <para>**PROVES THE CARRY-THROUGH ONTO THE SCREEN.** The engine's verdict
    /// already reads "not at all sure" for a call assembled from dim letters,
    /// and a screen that re-derived its own word from the same number would be a
    /// second place for the two to disagree. A stop drawn identically whatever
    /// it rests on is a guess presented as a decode (§0.0, HM-DEC-108).</para>
    /// </remarks>
    [Fact]
    public void ADimCallAndASolidOneAreNotDrawnTheSame()
    {
        var solid = Row(Dwell(7_030_000, "CQ DE W1AW", score: 0.95));
        var dim = Row(Dwell(7_031_000, "CQ DE W1AW", score: 0.3));

        _output.WriteLine($"solid: {solid.SurenessText}  ({solid.Sureness:0.00})");
        _output.WriteLine($"dim  : {dim.SurenessText}  ({dim.Sureness:0.00})");

        Assert.True(solid.Stopped);
        Assert.True(dim.Stopped);

        Assert.Equal("sure", solid.SurenessText);
        Assert.Equal("not at all sure", dim.SurenessText);

        // **AND THE COLOUR FOLLOWS THE WORDS.** This is a row the operator taps
        // to move his dial, so a maybe-CQ drawn in the same green as a clean one
        // is a guess wearing a decode's clothes. The words carry it and the hue
        // carries it too, which is §0.6's rule in the direction it is usually
        // stated the other way round.
        Assert.True(solid.IsSolid);
        Assert.False(dim.IsSolid);
    }

    /// <remarks>
    /// <para>Proves the row carries **the frequency the dwell listened at**, not
    /// the bin a candidate was ranked in. A tap tunes to this number, so a row
    /// that carried anything else would send the operator somewhere Hamlet never
    /// heard.</para>
    /// </remarks>
    [Fact]
    public void AResultCarriesTheFrequencyItWasHeardOn()
    {
        var row = Row(Dwell(7_028_500, "CQ CQ DE W1AW"));

        _output.WriteLine($"{row.FrequencyHz} Hz, drawn as {row.Label}, "
            + $"tune says '{row.TuneLabel}'");

        Assert.Equal(7_028_500, row.FrequencyHz);
        Assert.Contains("7.029", row.Label, StringComparison.Ordinal);
        Assert.Contains(row.Label, row.TuneLabel, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the payoff and §0.2.1 together: **tapping a result tunes
    /// there and stops the scan.** A scan that carried on moving the dial while
    /// the operator listened to what he just found is that section's own
    /// practical test failing — he could not tell where his radio had been left
    /// or why.</para>
    /// <para>There is no radio here, so what is proved is the face's own
    /// contract: the frequency the operator picked reaches the app's tuning path,
    /// and the scanner is asked to stop. What the dial does about it is
    /// `BandScanner`'s and is proved against a rig elsewhere.</para>
    /// </remarks>
    [Fact]
    public async Task TappingAResultTunesToItAndStopsTheScan()
    {
        long? tuned = null;
        var said = "";

        var scan = new ScanViewModel(
            line => said = line,
            tune: hz => tuned = hz);

        await scan.TuneToDwellCommand.ExecuteAsync(
            Row(Dwell(7_028_500, "CQ CQ DE W1AW")));

        _output.WriteLine($"tuned to {tuned}, said '{said}'");

        Assert.Equal(7_028_500, tuned);
        Assert.False(scan.IsScanning);
        Assert.Contains("7.029", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the guard rather than an expectation: a row with nowhere to go
    /// moves nothing. Silence is the right answer for a destination that is not
    /// one, and moving the dial anyway would be the worst available reading of
    /// §0.2.1.
    /// </remarks>
    [Fact]
    public async Task ARowWithNowhereToGoMovesNothing()
    {
        long? tuned = null;
        var scan = new ScanViewModel(_ => { }, tune: hz => tuned = hz);

        await scan.TuneToDwellCommand.ExecuteAsync(null);
        await scan.TuneToDwellCommand.ExecuteAsync(
            new ScanDwellRow(0, "", "", Stopped: false, Sureness: null));

        Assert.Null(tuned);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-073 survives onto the screen: **a callsign-shaped
    /// token stops the scan and is never printed as a callsign.** The engine
    /// carries no name on that verdict, and this is the check that nothing in
    /// the display puts one back.</para>
    /// </remarks>
    [Fact]
    public void ACallsignShapedStopNeverPrintsTheCallsign()
    {
        var row = Row(Dwell(7_030_000, "TU VA3VRR FB"));

        _output.WriteLine($"{row.Label}: {row.Sentence}");

        Assert.True(row.Stopped);
        Assert.DoesNotContain("VA3VRR", row.Sentence, StringComparison.Ordinal);
        Assert.DoesNotContain("VA3VRR", row.Label, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves §0.5: **a collapsed scanner still carries its news.** A shut panel
    /// that goes silent about a scan moving the dial is the prime directive
    /// broken by omission.
    /// </remarks>
    [Fact]
    public void TheCollapsedScannerStillSaysWhatIsHappening()
    {
        var scan = new ScanViewModel(_ => { });

        Assert.Equal("not scanning", scan.Summary);

        scan.WhereNow = "listening at 7.030 MHz";
        scan.IsScanning = true;

        _output.WriteLine(scan.Summary);

        Assert.Contains("scanning", scan.Summary, StringComparison.Ordinal);
        Assert.Contains("7.030", scan.Summary, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the stop is safe to press whatever is happening.**
    /// It awaits nothing, so it cannot queue behind the tune it is stopping, and
    /// it is harmless when nothing is running. A stop that throws when nothing
    /// is scanning is a stop nobody will reach for in a hurry.</para>
    /// </remarks>
    [Fact]
    public void TheStopIsSafeWithNoRadioAndNoScan()
    {
        var scan = new ScanViewModel(_ => { });

        scan.StopNow();
        scan.StopCommand.Execute(null);
        scan.StopNow();

        Assert.False(scan.IsScanning);
    }

    /// <remarks>
    /// Proves §0.2.1: **a scan cannot be started before there is a radio to
    /// abort against.** The engine refuses too, and this keeps the offer off the
    /// screen rather than letting the operator press something that will only
    /// tell him no.
    /// </remarks>
    [Fact]
    public void NoScanIsOfferedWithoutARadio()
    {
        var scan = new ScanViewModel(_ => { });

        Assert.False(scan.CanStart);

        scan.Attach(null, null, null, null);

        Assert.False(scan.CanStart);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **a scan file that cannot be read is refused where
    /// the operator can see it, and never quietly replaced with Hamlet's own
    /// list.** Substituting the default would run the scan over a stretch he did
    /// not choose, which is the one thing the file exists to prevent, and it
    /// would do it silently.</para>
    /// <para>The engine already throws on a bad file. What this proves is that
    /// the refusal reaches a surface rather than being swallowed on the way, and
    /// it is proved against a real unreadable file rather than argued about
    /// (§12.5).</para>
    /// </remarks>
    [Fact]
    public async Task AScanFileThatCannotBeReadIsRefusedWhereHeCanSeeIt()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-face-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, "scan-segments.json");

        File.WriteAllText(path, "{ this is not json at all");

        try
        {
            var said = new List<string>();
            var scan = new ScanViewModel(said.Add, segmentsPath: path);

            await scan.StartCommand.ExecuteAsync(null);

            _output.WriteLine($"refusal : {scan.Refusal}");
            _output.WriteLine($"status  : {string.Join(" | ", said)}");

            // NOT SCANNING, and saying why in both places the operator looks.
            Assert.False(scan.IsScanning);

            // **AND IT NAMED THE FILE RATHER THAN CARRYING ON.** The engine's
            // own default is twenty cited stretches, so a silent fallback would
            // have looked like a working scan.
            Assert.Contains("scan file", scan.Refusal, StringComparison.Ordinal);
            Assert.Contains(scan.Refusal, said);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
