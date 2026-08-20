using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Choosing a note by how it is keyed rather than how loud it is (HM-DEC-095).
/// </summary>
/// <remarks>
/// <para>**THESE RUN ON THE THREE RECORDINGS MADE ON THE AIR**, because the fault
/// they cover cannot be reproduced with synthesized Morse. Every synthetic fixture
/// in this repository holds one clean tone and nothing else, so a detector that
/// picks the loudest bin passes all of them and is wrong on every real recording
/// the operator has made.</para>
/// <para>The survey is driven here through its own interface rather than through
/// the tracker, so a failure says whether the measurement is wrong or the wiring
/// is.</para>
/// </remarks>
public sealed class CwToneSurveyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public CwToneSurveyTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Sweep the survey across a recording exactly as the tracker drives it.
    /// </summary>
    /// <param name="name">The recording.</param>
    /// <param name="blockedSeconds">How much of it was the operator himself.</param>
    /// <returns>The best keying found anywhere in it, and the loudest thing that
    /// was not keying.</returns>
    /// <remarks>
    /// **THREE SECONDS OF HISTORY, READ TWICE A SECOND, WHICH IS WHAT THE TRACKER
    /// DOES.** Running one survey over a whole half minute was tried first and it
    /// finds nothing: the station answering the call is on for eight seconds of
    /// thirty, so its element lengths are a quarter of the population and the
    /// clustering that identifies them is drowned by the rest. The window is short
    /// for that reason and the test has to use the real one.
    /// </remarks>
    private static ToneVerdict Survey(string name, out double blockedSeconds)
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder, name + ".wav"));

        var rate = audio.SampleRate;
        var hop = rate / 100;                        // ten milliseconds
        var window = (int)(rate * 0.040);            // forty, about 38 Hz wide

        var bins = new double[25];
        var coefficient = new double[bins.Length];

        for (var i = 0; i < bins.Length; i++)
        {
            bins[i] = 300 + (i * 25);
            coefficient[i] = 2 * Math.Cos(2 * Math.PI * bins[i] / rate);
        }

        var hann = new double[window];
        for (var i = 0; i < window; i++)
        {
            hann[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (window - 1)));
        }

        var survey = new CwToneSurvey(bins, 0.010);
        var guard = new CwTransmitGuard(0.010);
        var db = new double[bins.Length];
        var samples = audio.Samples;
        var frames = 0;

        KeyingCandidate? best = null;
        ToneInterference? loudest = null;

        for (var start = 0; start + window < samples.Length; start += hop)
        {
            double sumSquares = 0;

            for (var i = 0; i < window; i++)
            {
                double raw = samples[start + i];
                sumSquares += raw * raw;
            }

            var blocked = guard.Observe(
                20 * Math.Log10(Math.Sqrt(sumSquares / window) + 1e-12));

            for (var b = 0; b < bins.Length; b++)
            {
                double s1 = 0, s2 = 0;

                for (var i = 0; i < window; i++)
                {
                    var s0 = (samples[start + i] * hann[i]) + (coefficient[b] * s1) - s2;
                    s2 = s1;
                    s1 = s0;
                }

                var power = Math.Max(
                    0, (s1 * s1) + (s2 * s2) - (coefficient[b] * s1 * s2))
                    / ((double)window * window);

                db[b] = 10 * Math.Log10(power + 1e-14);
            }

            survey.Observe(db, blocked);

            // Twice a second, as the tracker reads it.
            if (++frames % 50 != 0)
            {
                continue;
            }

            var verdict = survey.Analyze();

            if (verdict.Keyed is { } keyed
                && (best is not { } sofar || keyed.Separation > sofar.Separation))
            {
                best = keyed;
            }

            if (verdict.Interference is { } noise
                && (loudest is not { } other || noise.LiftDb > other.LiftDb))
            {
                loudest = noise;
            }
        }

        blockedSeconds = guard.BlockedHops * 0.010;
        return new ToneVerdict(best, loudest);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-095, and it is the measurement the whole change rests
    /// on. **Independent analysis of this recording puts a station at 613 Hz
    /// sending dits of about a hundred milliseconds and dahs of about two hundred
    /// and seventy-five**, a ratio near two point eight, which is eleven or twelve
    /// words a minute.</para>
    /// <para>The survey has to find that from the audio alone, on a recording
    /// where the operator himself is transmitting for eighteen of its thirty
    /// seconds. The bin is checked to a coarse bin's width because this is the
    /// coarse stage; the fine stage is what resolves the last few hertz.</para>
    /// </remarks>
    [Fact]
    public void TheStationAnsweringACallIsFoundByItsKeying()
    {
        var verdict = Survey("cw-2026-08-17-013347", out var blocked);

        _output.WriteLine($"blocked {blocked:0.0} s of 30");
        _output.WriteLine("keyed: " + (verdict.Keyed is { } found
            ? $"{found.ToneHz:0} Hz, dit {found.DitMilliseconds:0} ms, "
              + $"dah {found.DahMilliseconds:0} ms, ratio {found.Ratio:0.00}, "
              + $"separation {found.Separation:0.0}, {found.Marks} marks"
            : "none"));

        Assert.NotNull(verdict.Keyed);

        var keyed = verdict.Keyed!.Value;

        Assert.InRange(keyed.ToneHz, 590, 640);
        Assert.InRange(keyed.DitMilliseconds, 80, 125);
        Assert.InRange(keyed.DahMilliseconds, 240, 310);
        Assert.InRange(keyed.Ratio, CwToneSurvey.MinimumRatio, CwToneSurvey.MaximumRatio);
        Assert.InRange(keyed.WordsPerMinute, 9, 15);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-095 and §0.0: **the recording holding no readable
    /// station is not given one.** A detector that answers with its best guess
    /// wherever it is asked is worse than no detector, because a frequency on
    /// screen is a claim that something is there.</para>
    /// <para>This is the assertion that fails first if the separation limit is
    /// ever loosened, which is exactly what should happen.</para>
    /// </remarks>
    [Fact]
    public void NoKeyingIsClaimedWhereNoneWasFound()
    {
        var verdict = Survey("cw-2026-08-17-013622", out _);

        _output.WriteLine("keyed: " + (verdict.Keyed is { } k
            ? $"{k.ToneHz:0} Hz separation {k.Separation:0.0}"
            : "none"));

        Assert.Null(verdict.Keyed);
    }

    /// <remarks>
    /// <para>**THIS REPLACES `ACarrierNeverConvincesTheTrackerItIsAStation`,
    /// WHICH ASSERTED A FALSEHOOD FOR THREE DAYS** (HM-DEC-144). That test read
    /// `cw-2026-08-17-134712.wav` by name and required the tracker never to claim
    /// keying in it, on HM-DEC-095's finding that the strong signal there is a
    /// carrier. It is a station: the gate's own elements across a second and a
    /// half of that recording spell the callsign `N4L`, which is proved in
    /// `TheStationInTheRecordingIsN4LTests`. So the retired test required Hamlet
    /// never to notice a real station, and every change that let it do so failed
    /// the suite.</para>
    /// <para>**NO RECORDING IN THIS REPOSITORY IS ESTABLISHED AS A CARRIER**, so
    /// the property is asserted on audio whose truth is known by construction
    /// instead: a steady tone that never stops, in a shaped band, which is what a
    /// carrier is. **A synthesized fixture is the weaker evidence** (§12.5,
    /// HM-DEC-091) and it is what there is until a real carrier is recorded off
    /// the air and kept.</para>
    /// <para>The claim is made by the tracker rather than by one survey, and it
    /// takes two agreeing surveys, which is the level §0.0 is about because it is
    /// the level that reaches the operator.</para>
    /// </remarks>
    [Fact]
    public void ACarrierNeverConvincesTheTrackerItIsAStation()
    {
        const int rate = 48_000;
        const double toneHz = 620;

        var random = new Random(7300);
        var samples = new float[rate * 30];
        var band = 0.0;

        for (var i = 0; i < samples.Length; i++)
        {
            // A tone that never stops, and a band of noise under it. Nothing here
            // is ever keyed, so nothing here is ever Morse.
            //
            // **THE NOISE IS SHAPED AND WHITE NOISE WOULD NOT DO** (HM-OPEN-018).
            // A receiver hands the decoder what got through its own filter, so
            // energy outside the passband is not there to be chattered on. Fed
            // flat noise instead, the survey claimed keying at 875 Hz beside a
            // 620 Hz carrier, which is a fact about the fixture rather than about
            // this radio.
            var white = (random.NextDouble() - 0.5) * 0.4;

            band = (0.965 * band) + (0.035 * white);

            samples[i] = (float)(
                (0.30 * Math.Sin(2 * Math.PI * toneHz * i / rate)) + (band * 3));
        }

        var tracker = new CwToneTracker(rate, 600);
        var claimed = 0;

        tracker.Process(samples, 0, _ =>
        {
            if (tracker.Verdict.Keyed is not null)
            {
                claimed++;
            }
        });

        _output.WriteLine($"tracked {tracker.ToneHz:0} Hz, "
            + $"keying claimed on {claimed} measurements");

        Assert.False(tracker.HasKeying);
        Assert.Equal(0, claimed);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-095's surviving half on the recording it was written
    /// from: **the loudest thing in this half minute is at 500 Hz**, and the old
    /// detector answered 375, which is neither the strong signal nor a station nor
    /// the operator's own pitch setting, and no reading of the audio produces
    /// it.</para>
    /// <para>**THE NAME OF THIS TEST IS NOW WRONG AND ITS ASSERTIONS ARE NOT**
    /// (HM-DEC-144). That 500 Hz signal is a station sending `N4L`, so calling it
    /// interference is calling somebody's transmission noise. What is asserted
    /// here is only what was measured, a frequency and a strength, and both are
    /// still true. **What the survey should call it is HM-OPEN-054's question**
    /// and is parked, so the test keeps its measurements and this note carries the
    /// correction rather than a rename hiding it.</para>
    /// </remarks>
    [Fact]
    public void TheStrongSignalThatIsNotKeyingIsReportedAsInterference()
    {
        var verdict = Survey("cw-2026-08-17-134712", out _);

        _output.WriteLine("interference: " + (verdict.Interference is { } n
            ? $"{n.ToneHz:0} Hz, {n.LiftDb:0.0} dB over the band, "
              + $"present {n.PresentFraction:P0} of the time"
            : "none"));

        Assert.NotNull(verdict.Interference);

        var noise = verdict.Interference!.Value;

        Assert.InRange(noise.ToneHz, 475, 525);
        Assert.True(
            noise.LiftDb >= CwToneSurvey.InterferenceLiftDb,
            $"the carrier reads {noise.LiftDb:0.0} dB over the band");
    }

    /// <remarks>
    /// Proves HM-DEC-095: a survey with nothing in it says nothing, rather than
    /// answering from an empty history.
    /// </remarks>
    [Fact]
    public void ASurveyWithNoHistorySaysNothing()
    {
        var survey = new CwToneSurvey(new double[] { 500, 600, 700 }, 0.010);

        Assert.False(survey.IsReady);
        Assert.Null(survey.Analyze().Keyed);
        Assert.Null(survey.Analyze().Interference);
    }

    /// <summary>Where the recordings live.</summary>
    internal static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.RadioEngine")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
