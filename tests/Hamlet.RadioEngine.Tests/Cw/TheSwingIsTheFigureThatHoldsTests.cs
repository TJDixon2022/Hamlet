using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Which figure tells a recording with a station in it from one without.
/// </summary>
/// <remarks>
/// <para>**`snrDb` DOES NOT, AND IT HAS COST THIS PROJECT TWO DAYS.** It is a
/// held peak of how far the tracked bin stood above the noise beside it, rising
/// at once and falling about a decibel a second, which is what HM-DEC-090 built
/// it to be so that a station keying for a second and a half inside thirty would
/// not average away to nothing. Read as a figure about a recording it is badly
/// wrong, and it was: a work order was written from a reading of 46.5 on a
/// recording containing no station.</para>
/// <para>**THE SWING BETWEEN QUIET AND LOUD AT THE KEYED PITCH DOES.** It is
/// what <see cref="CwKeyingThresholds.ConfidentSwingDb"/> is measured from, and
/// these tests are the measurement rather than a memory of it.</para>
/// <para>Nothing in `src` reads these. They record a finding so a later session
/// does not have to rediscover it, and so nobody quietly relabels the number
/// back.</para>
/// </remarks>
public sealed class TheSwingIsTheFigureThatHoldsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheSwingIsTheFigureThatHoldsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The recordings that hold somebody keying.</summary>
    private static readonly string[] WithAStation =
    {
        "cw-2026-08-17-013347.wav",
        "cw-2026-08-17-134712.wav",
        "cw-2026-08-18-004507.wav",
        "unadjudicated/cw-2026-08-18-003016.wav",
        "unadjudicated/cw-2026-08-18-003126.wav",
        "unadjudicated/cw-2026-08-18-003758.wav",
    };

    /// <summary>The recordings that hold no keying at any pitch (HM-DEC-090).</summary>
    private static readonly string[] WithNothing =
    {
        "unadjudicated/cw-2026-08-20-014854.wav",
        "unadjudicated/cw-2026-08-20-014935.wav",
    };

    private static MonoAudio Read(string relative) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, relative));

    private double Swing(string relative)
    {
        var best = KeyingEnvelope.Best(Read(relative));
        var swing = best?.Profile.SwingDb ?? 0;

        _output.WriteLine($"{relative,-45} swing {swing,5:0.0} dB");

        return swing;
    }

    private double TonePeak(string relative)
    {
        var audio = Read(relative);
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var peak = decoder.Report.SnrDb;

        _output.WriteLine($"{relative,-45} tonePeak {peak,5:0.0} dB");

        return peak;
    }

    /// <remarks>
    /// <para>Proves the threshold is measured rather than recalled: every
    /// recording holding a station swings past
    /// <see cref="CwKeyingThresholds.ConfidentSwingDb"/> and neither recording
    /// holding nothing comes near it.</para>
    /// </remarks>
    [Fact]
    public void TheSwingSeparatesAStationFromAnEmptyBand()
    {
        var stations = WithAStation.Select(Swing).ToList();
        var empty = WithNothing.Select(Swing).ToList();

        _output.WriteLine("");
        _output.WriteLine(
            $"stations {stations.Min():0.0} to {stations.Max():0.0} dB, "
            + $"empty {empty.Min():0.0} to {empty.Max():0.0} dB, "
            + $"threshold {CwKeyingThresholds.ConfidentSwingDb:0.0}");

        Assert.True(
            stations.Min() >= CwKeyingThresholds.ConfidentSwingDb,
            $"a recording with a station in it swung only {stations.Min():0.0} dB");

        Assert.True(
            empty.Max() < CwKeyingThresholds.ConfidentSwingDb,
            $"a recording with nothing in it swung {empty.Max():0.0} dB");
    }

    /// <remarks>
    /// <para>Proves the fault, so it stays proved: **the held tone peak rates two
    /// recordings holding no keying above the one this decoder reads a callsign
    /// out of.** The number is not deleted and not changed, because it measures
    /// something real. It says what it measures instead, and this is why.</para>
    /// </remarks>
    [Fact]
    public void TheHeldTonePeakRatesSilenceAboveAReadableStation()
    {
        var readable = TonePeak("cw-2026-08-17-013347.wav");
        var nothing = WithNothing.Select(TonePeak).ToList();

        _output.WriteLine("");
        _output.WriteLine(
            $"the recording a callsign comes out of: {readable:0.0} dB; "
            + $"two holding nothing: {string.Join(" and ", nothing.Select(n => $"{n:0.0}"))} dB");

        Assert.True(
            nothing.Min() > readable,
            "the held peak has started separating a station from an empty band, "
            + "which would mean this finding no longer holds and the field's "
            + "label should be revisited");
    }

    /// <remarks>
    /// Proves the field says what it is. A column called `snrDb` that is not one
    /// is the fault this renaming fixes (HM-DEC-091).
    /// </remarks>
    [Fact]
    public void TheRosterColumnSaysWhatItMeasures()
    {
        var columns = CwCaseRoster.Header.Split('\t');

        Assert.Contains("tonePeakDb", columns);
        Assert.DoesNotContain("snrDb", columns);
    }

    /// <remarks>
    /// Proves task 5: the transcript cell says what interval it covers, in the
    /// same words the count beside it uses (HM-DEC-091).
    /// </remarks>
    [Fact]
    public void TheTranscriptCellSaysWhatItCovers()
    {
        var one = new CwCase(
            new DateTime(2026, 8, 20, 1, 48, 54, DateTimeKind.Utc),
            14_028_000, "20 m", "cw-2026-08-20-014854.wav", "",
            800, 14.1, 18, 69, 23, "CQ DE N0CALL K", CwCountsCover.Recording);

        var columns = CwCaseRoster.Row(one).Split('\t');
        var text = columns[Array.IndexOf(CwCaseRoster.Header.Split('\t'), "text")];

        _output.WriteLine(text);

        Assert.StartsWith("CQ DE N0CALL K", text, StringComparison.Ordinal);
        Assert.Contains("the whole session, not this case", text, StringComparison.Ordinal);

        // And a cell with nothing in it needs no clause: there is no interval to
        // qualify and the sentence already says the decoder produced nothing.
        var quiet = CwCaseRoster.Row(one with { Text = "" }).Split('\t');

        Assert.Equal(
            "nothing read",
            quiet[Array.IndexOf(CwCaseRoster.Header.Split('\t'), "text")]);
    }
}
