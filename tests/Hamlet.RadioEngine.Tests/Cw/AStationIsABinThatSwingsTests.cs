using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Swing finds the stations the average missed, and finds nothing in silence.
/// </summary>
/// <remarks>
/// <para>**THE ACCEPTANCE FILE IS `cw-2026-08-31-003229`** (work instruction 055).
/// A station called CQ, and Hamlet's survey admitted no keying at all, so the
/// squelch turned all forty-three characters into blocks. An independent decoder
/// reads it at 583.5 Hz.</para>
/// <para>**THIS SEES MORE AND REQUIRES NOTHING LESS** (HM-DEC-120). The silence
/// cases bound the threshold from below, and the margin between what a
/// station-free recording produces and what a station produces is measured here
/// rather than assumed.</para>
/// </remarks>
public sealed class AStationIsABinThatSwingsTests
{
    private readonly ITestOutputHelper _output;

    public AStationIsABinThatSwingsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The acceptance file, and where the station is.</summary>
    /// <remarks>
    /// **THE BENCH READS 583.5 Hz.** A 12.5 Hz grid puts that between 575 and
    /// 587.5, so either is the same station and anything else is not.
    /// </remarks>
    [Fact]
    public void TheRefusedCqIsFoundNearFiveEightyThree()
    {
        var audio = Capture("cw-2026-08-31-003229");
        var ranked = CwSwingSurvey.Rank(audio.Samples, audio.SampleRate);

        Assert.NotEmpty(ranked);

        _output.WriteLine(
            "top four: "
            + string.Join("  ", ranked.Take(4).Select(
                c => $"{c.Hz:0} Hz swing {c.SwingDb:0.0} keyed {c.KeyedDb:0.0}")));

        Assert.InRange(ranked[0].Hz, 570, 600);
    }

    /// <summary>The noise pick is demoted below the keyed carriers.</summary>
    /// <remarks>
    /// **HAMLET CHOSE 510 Hz AND EMITTED FORTY-EIGHT `E`s FROM IT.** The bench
    /// found the strongest keyed carrier at 773.4 Hz. The test is that 510 is no
    /// longer the winner; which of the real carriers wins is not asserted, because
    /// no adjudication says which station that file is about.
    /// </remarks>
    [Fact]
    public void TheNoisePickNoLongerWins()
    {
        var audio = Capture("cw-2026-08-31-002443");
        var ranked = CwSwingSurvey.Rank(audio.Samples, audio.SampleRate);

        Assert.NotEmpty(ranked);

        _output.WriteLine(
            "top four: "
            + string.Join("  ", ranked.Take(4).Select(
                c => $"{c.Hz:0} Hz swing {c.SwingDb:0.0} keyed {c.KeyedDb:0.0}")));

        Assert.False(
            Math.Abs(ranked[0].Hz - 510) < 12.5,
            $"the winner is {ranked[0].Hz:0} Hz, which is still the noise pick");
    }

    /// <summary>
    /// The margin between silence and a station, which is what bounds the
    /// threshold.
    /// </summary>
    /// <remarks>
    /// **THE THRESHOLD MUST SIT ABOVE ANYTHING A STATION-FREE RECORDING
    /// PRODUCES** (work instruction 055, task 2, which requires the margin be
    /// reported explicitly). Two silences are tried: digital zero, which is what
    /// a muted receiver gives, and shaped noise, which is what an empty band
    /// gives.
    /// </remarks>
    [Fact]
    public void TheMarginBetweenSilenceAndAStation()
    {
        var quiet = new float[48_000 * 20];

        var digital = CwSwingSurvey.Rank(quiet, 48_000);
        var digitalTop = digital.Count == 0 ? 0 : digital[0].SwingDb;

        var random = new Random(20260831);
        var noise = new float[48_000 * 20];

        for (var i = 0; i < noise.Length; i++)
        {
            noise[i] = (float)((random.NextDouble() * 2) - 1) * 0.02f;
        }

        var band = CwSwingSurvey.Rank(noise, 48_000);
        var bandTop = band.Count == 0 ? 0 : band[0].SwingDb;

        var station = Capture("cw-2026-08-31-003229");
        var found = CwSwingSurvey.Rank(station.Samples, station.SampleRate);
        var stationTop = found.Count == 0 ? 0 : found[0].SwingDb;

        _output.WriteLine($"digital silence : {digitalTop:0.0} dB");
        _output.WriteLine($"band noise      : {bandTop:0.0} dB");
        _output.WriteLine($"the CQ at 003229: {stationTop:0.0} dB");
        _output.WriteLine("");
        _output.WriteLine(
            $"margin over band noise: {stationTop - bandTop:0.0} dB");

        Assert.True(
            stationTop > bandTop,
            $"a station swings {stationTop:0.0} dB and empty band noise swings "
            + $"{bandTop:0.0}, so swing cannot separate them");
    }

    /// <summary>Every capture of the evening, ranked, for the record.</summary>
    /// <remarks>
    /// No assertion beyond each producing a candidate: this prints the table the
    /// report carries, and the classes it covers are the four the unit exists for.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-08-31-003212")]
    [InlineData("cw-2026-08-31-003229")]
    [InlineData("cw-2026-08-31-002424")]
    [InlineData("cw-2026-08-31-002443")]
    [InlineData("cw-2026-08-31-002829")]
    [InlineData("cw-2026-08-31-003408")]
    [InlineData("cw-2026-08-31-003419")]
    public void EveryCaptureOfTheEveningRanks(string name)
    {
        var audio = Capture(name);
        var ranked = CwSwingSurvey.Rank(audio.Samples, audio.SampleRate);

        Assert.NotEmpty(ranked);

        _output.WriteLine(
            $"{name}: {ranked[0].Hz:0} Hz, swing {ranked[0].SwingDb:0.0} dB, "
            + $"keyed {ranked[0].KeyedDb:0.0} dB");
    }

    /// <summary>One of tonight's captures.</summary>
    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            CwFixtures.Folder, "captured", "unadjudicated", name + ".wav"));
}
