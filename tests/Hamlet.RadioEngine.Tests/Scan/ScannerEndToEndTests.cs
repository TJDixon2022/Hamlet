using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// The survey, the dwell and the safety envelope run as one thing (HM-DEC-107,
/// phase 4 of the cleanup order).
/// </summary>
/// <remarks>
/// <para>**EVERY PIECE WAS TESTED AND THE PIECES HAD NEVER MET.** The survey was
/// proved on generated sweeps, the dwell on hand-built characters, and the
/// envelope on a stub radio that answered instantly. This is the first thing
/// that puts a training radio, its synthesized spectrum and the real decoder in
/// one place and turns the handle.</para>
/// <para>**AND NONE OF IT IS EVIDENCE ABOUT THE RADIO** (HM-DEC-093). The
/// training radio places its signals by reading the neighborhood plan, so it
/// teaches the real band, and it is still a program pretending. What these prove
/// is that the parts fit together, not that a scan finds anybody.</para>
/// </remarks>
public sealed class ScannerEndToEndTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the run is printed.</param>
    public ScannerEndToEndTests(ITestOutputHelper output) => _output = output;

    private static CwBand FortyMeters => HfBands.Bands.Single(b => b.Name == "40 m");

    private static async Task<RigStateMonitor> ReadyAsync(IRig rig)
    {
        var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));

        return monitor;
    }

    private static ScanSegments FenceAround(CwBand band)
        => ScanSegments.Parse(
            $$"""
            {
              "segments": [
                { "band": "{{band.Name}}", "name": "the Morse end",
                  "lowHz": {{band.CwLowHz}}, "highHz": {{band.CwHighHz}},
                  "cite": "derived from the band under test" }
              ]
            }
            """,
            "the end-to-end fence");

    /// <remarks>
    /// <para>**THE CLAIM THE WHOLE SCANNER RESTS ON, AGAINST A LIVE-ISH SOURCE.**
    /// The engine measured 0.955 against 0.006 on sweeps built by hand; this puts
    /// the same survey behind the training radio's own synthesizer, which places
    /// its signals by reading the neighborhood plan rather than by being told
    /// where to put them.</para>
    /// <para>What is asserted is the ordering and not the numbers. A carrier is
    /// the loudest thing on many bands and it is always there, so anything
    /// ranking by strength tours the birdies and never reaches a person.</para>
    /// </remarks>
    [Fact]
    public void TheSurveyPrefersSomebodySendingToTheLoudestThingOnTheBand()
    {
        var band = FortyMeters;
        var survey = new ScopeBinSurvey();

        using var spectrum = new TrainingSpectrumSource(band, seed: 20260818);

        spectrum.FrameReady += (in SpectrumFrame frame) => survey.Observe(frame);

        // Thirty seconds of sweeps, pumped rather than waited for, so the run is
        // instant and identical every time (§5).
        for (var i = 0; i < TrainingSpectrumSource.FramesPerSecond * 30; i++)
        {
            spectrum.PumpOnce(
                TimeSpan.FromSeconds(i / (double)TrainingSpectrumSource.FramesPerSecond));
        }

        var ranked = survey.Ranked();
        var steady = survey.Steady();

        foreach (var bin in ranked.Take(6))
        {
            _output.WriteLine($"{bin.CenterHz / 1e6:0.000} MHz  score {bin.Score:0.000}  "
                + $"presence {bin.Presence:P0}  swing {bin.Variability:0.0}  "
                + $"{(bin.LooksSteady ? "steady" : "moving")}");
        }

        _output.WriteLine($"{ranked.Count} candidates, {steady.Count} of them steady");

        Assert.NotEmpty(ranked);

        // NOTHING STEADY MAY OUTRANK EVERYTHING THAT MOVES. Where the band holds
        // both, the people come first.
        if (ranked.Any(b => b.LooksSteady) && ranked.Any(b => !b.LooksSteady))
        {
            var bestSteady = ranked.First(b => b.LooksSteady).Score;
            var bestMoving = ranked.First(b => !b.LooksSteady).Score;

            _output.WriteLine($"best moving {bestMoving:0.000} against "
                + $"best steady {bestSteady:0.000}");

            Assert.True(
                bestMoving > bestSteady,
                "a carrier outranked every operator on the band, which is the "
                + "one thing the ranking exists to prevent");
        }
    }

    /// <remarks>
    /// <para>Proves the dwell reaches the real decoder and the verdict carries
    /// its confidence out (HM-DEC-107 phase 7). The audio is the training
    /// source's own Morse, which is what the app feeds the decoder when a
    /// training radio is connected.</para>
    /// </remarks>
    [Fact]
    public void ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence()
    {
        var decoder = new CwDecoder(8_000, 600);
        var dwell = new ScanDwell(FortyMeters.JumpHz);

        decoder.CharacterSettled += dwell.Take;

        using var audio = new TrainingAudioSource(
            MorseCode.CqCall("N0CALL"), wordsPerMinute: 18, toneHz: 600);

        decoder.Listen(audio);
        audio.Start();

        // Twenty seconds of audio, pumped a tenth of a second at a time so the
        // run is deterministic and instant (§5).
        for (var i = 0; i < 200; i++)
        {
            audio.PumpOnce(audio.SampleRate / 10);
        }

        decoder.Flush();

        _output.WriteLine(dwell.Describe());
        _output.WriteLine($"{dwell.Heard.Count} characters reached the dwell");

        Assert.NotEmpty(dwell.Heard);

        // THE VERDICT IS WHATEVER IT IS, and what matters is that it carries a
        // reason and a confidence rather than being a bare halt.
        Assert.NotEqual("", dwell.Verdict.Sentence);

        if (dwell.Verdict.Stop)
        {
            Assert.InRange(dwell.Verdict.Confidence, 0.0, 1.0);
            Assert.NotEqual("", dwell.Verdict.Evidence);
        }
    }

    /// <remarks>
    /// <para>Proves §0.2.1 on the route that matters most: **the dial goes back
    /// when the operator presses stop**, against a radio that actually moves
    /// rather than a stub that records the request.</para>
    /// </remarks>
    [Fact]
    public async Task TheDialComesBackWhenTheOperatorStops()
    {
        var band = FortyMeters;
        var rig = new TrainingRig(band.JumpHz);

        await rig.ConnectAsync();

        using var monitor = await ReadyAsync(rig);

        var home = new MemoryHome();
        var scanner = new BandScanner(rig, monitor, home);

        var outcome = await scanner.RunAsync(
            new[] { band.CwLowHz + 5_000, band.CwLowHz + 9_000 },
            FenceAround(band),
            (hz, seconds, token) =>
            {
                scanner.Stop();
                return Task.FromResult(new ScanDwell(hz, seconds));
            });

        var where = await rig.GetFrequencyHzAsync();

        _output.WriteLine(outcome.Sentence);
        _output.WriteLine($"the dial is at {where / 1e6:0.000} MHz, "
            + $"and it started at {band.JumpHz / 1e6:0.000}");

        Assert.Equal(ScanStopCause.OperatorStopped, outcome.Cause);
        Assert.Equal(band.JumpHz, where);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **a hand on the knob ends the scan and the dial stays
    /// where he put it.** The training radio raises the same event the real one
    /// does when somebody turns it, so this exercises the abort rather than
    /// simulating the abort.</para>
    /// </remarks>
    [Fact]
    public async Task AHandOnTheKnobEndsTheScanAndLeavesItWhereHePutIt()
    {
        var band = FortyMeters;
        var rig = new TrainingRig(band.JumpHz);

        await rig.ConnectAsync();

        using var monitor = await ReadyAsync(rig);

        var home = new MemoryHome();
        var scanner = new BandScanner(rig, monitor, home);
        var his = band.CwLowHz + 33_333;

        var outcome = await scanner.RunAsync(
            new[] { band.CwLowHz + 5_000, band.CwLowHz + 9_000 },
            FenceAround(band),
            (hz, seconds, token) =>
            {
                rig.SimulateKnobTurn(his);
                return Task.FromResult(new ScanDwell(hz, seconds));
            });

        var where = await rig.GetFrequencyHzAsync();

        _output.WriteLine(outcome.Sentence);
        _output.WriteLine($"the dial is at {where / 1e6:0.000} MHz");

        Assert.Equal(ScanStopCause.DialTouched, outcome.Cause);
        Assert.Equal(his, where);

        // AND THE NOTE IS CLEARED. He is where he wants to be, so a later connect
        // must not drag him off it.
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2.1's crash-safe half against a radio: **the app dying
    /// mid-scan leaves the note on disk, and the next connect puts the dial
    /// back.** This is the exit route that cannot be tested by stopping
    /// politely, so the scan is abandoned rather than ended.</para>
    /// </remarks>
    [Fact]
    public async Task TheDialComesBackOnTheNextConnectAfterTheAppDiesMidScan()
    {
        var band = FortyMeters;
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-e2e-" + Guid.NewGuid().ToString("N"));

        var notePath = Path.Combine(folder, "scan-home");

        try
        {
            // The session that went away: it wrote the note and never came back.
            var dying = new FileScanHome(notePath);
            dying.Remember(band.JumpHz);

            // A fresh app, a fresh radio, parked where the scan left it.
            var stranded = band.CwLowHz + 12_000;
            var rig = new TrainingRig(stranded);

            await rig.ConnectAsync();

            using var monitor = await ReadyAsync(rig);

            var scanner = new BandScanner(rig, monitor, new FileScanHome(notePath));
            var restored = await scanner.RestoreHomeAsync();

            var where = await rig.GetFrequencyHzAsync();

            _output.WriteLine($"found at {stranded / 1e6:0.000} MHz, "
                + $"restored to {where / 1e6:0.000} MHz");

            Assert.Equal(band.JumpHz, restored);
            Assert.Equal(band.JumpHz, where);
            Assert.False(File.Exists(notePath));
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }

    /// <remarks>
    /// <para>Proves §0.5 and §0.0.1 end to end: **a scan that found nobody still
    /// says where it listened.** A record holding only the stops cannot be told
    /// from one that never ran, and the places it passed over are half of what it
    /// measured.</para>
    /// </remarks>
    [Fact]
    public async Task AScanThatFoundNobodyStillSaysEverywhereItListened()
    {
        var band = FortyMeters;
        var rig = new TrainingRig(band.JumpHz);

        await rig.ConnectAsync();

        using var monitor = await ReadyAsync(rig);

        var wanted = new[]
        {
            band.CwLowHz + 5_000, band.CwLowHz + 9_000, band.CwLowHz + 13_000,
        };

        var scanner = new BandScanner(rig, monitor, new MemoryHome());

        var outcome = await scanner.RunAsync(
            wanted,
            FenceAround(band),
            (hz, seconds, token) => Task.FromResult(new ScanDwell(hz, seconds)));

        foreach (var dwell in outcome.Dwells)
        {
            _output.WriteLine(dwell.Describe());
        }

        Assert.Equal(ScanStopCause.Finished, outcome.Cause);
        Assert.Equal(wanted.Length, outcome.Dwells.Count);

        Assert.All(outcome.Dwells, d => Assert.NotEqual("", d.Describe()));

        // AND THE DIAL IS BACK. Finding nobody is not a reason to leave the
        // operator's radio wherever the scan gave up.
        Assert.True(outcome.HomeRestored);
        Assert.Equal(band.JumpHz, await rig.GetFrequencyHzAsync());
    }

    /// <summary>The note, in memory, so a test does not touch the disk.</summary>
    private sealed class MemoryHome : IScanHome
    {
        public long? Pending { get; private set; }

        public void Remember(long frequencyHz) => Pending = frequencyHz;

        public void Clear() => Pending = null;
    }
}
