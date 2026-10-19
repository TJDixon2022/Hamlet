using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SHADOW RULE REACHES THE GRID, DOWN TO ONE LANE'S SPACING** (work instruction 573, HM-DEC-277). A sender 50 Hz under
/// W1AW printed in every live session for two weeks, measured once as energy locked to W1AW's keying; the shadow rule judged only
/// the bank's lanes, and only against stations more than 50 Hz away. These hold a synthetic station and a tone keyed in lockstep
/// with it, 15 dB under it, at 550, 575 and 650 Hz, as the shadow was measured live. Synthetic, generated as the synthetic CQs
/// are; not a recording.
/// </summary>
public sealed class TheShadowReachesTheGridTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The station: 600 Hz, 18 WPM, 20 dB over the band.</summary>
    internal const double StationHz = 600;

    /// <summary>How far under the station its shadow is keyed: 15 dB.</summary>
    internal const double ShadowDb = -15;

    /// <summary>The station's recipe: the synthetic CQ at 18 WPM, at 20 dB, on its own seed.</summary>
    internal static CwFixtureRecipe Station { get; } = SyntheticCq.Recipe(18, 20, 20261019) with { Name = "shadow-station", ToneHz = StationHz };

    /// <summary>The station and its shadow at <paramref name="shadowHz"/>: the same text, speed and lead-in, so the two key in lockstep.</summary>
    /// <param name="shadowHz">Where the shadow sits.</param>
    /// <returns>The audio.</returns>
    internal static MonoAudio Fixture(double shadowHz)
        => CwFixtureGenerator.Together(Station, Station with { Name = "shadow", ToneHz = shadowHz }, ShadowDb).Audio;

    /// <summary>What the chain as shipped prints, the letters nearer the shadow's pitch than the station's, and the station's letters right.</summary>
    internal static (string Text, string AtShadow, int Right, int OutOf) Read(double shadowHz)
    {
        var audio = Fixture(shadowHz);
        var (letters, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate);
        var atShadow = string.Concat(letters.Where(l => Math.Abs(l.PitchHz - shadowHz) < Math.Abs(l.PitchHz - StationHz)).Select(l => l.Text));
        var station = string.Join(' ', letters.Where(l => Math.Abs(l.PitchHz - shadowHz) >= Math.Abs(l.PitchHz - StationHz)).Select(l => l.Text));

        return (text, atShadow, TheRecordingsScoreboardTests.Right(station, SyntheticCq.Text), TheRecordingsScoreboardTests.Letters(SyntheticCq.Text).Length);
    }

    /// <remarks>
    /// Work instruction 573, task 2: the trace. The station alone, and with each shadow: what prints, each letter's pitch, and
    /// the shadows the rule holds back with their share. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheFixturesTraced()
    {
        foreach (var (shadowHz, judged) in new double?[] { null, 550, 575, 650 }.SelectMany(h => new[] { (h, false), (h, true) }))
        {
            var audio = shadowHz is { } hz ? Fixture(hz) : CwFixtureGenerator.Generate(Station).Audio;
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<string>();
            var chunk = audio.SampleRate / 100;

            chain.Detector.SetPassband(600, 500);
            chain.Detector.ShadowJudgesGrid = judged;
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add(string.Create(Inv, $"{c.Text}@{run.Average(m => m.PitchHz):0}"));

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();

            output.WriteLine((shadowHz is null ? "the station alone" : string.Create(Inv, $"shadow at {shadowHz:0} Hz")) + (judged ? ", the grid judged to 25 Hz:" : ", as at HEAD:"));
            output.WriteLine("  letters: " + string.Join(' ', letters));
            output.WriteLine("  shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Select(h => string.Create(Inv, $"{h.AtSeconds:0.0} s {h.Finder} {h.PitchHz:0} Hz of {h.OfPitchHz:0} Hz share {h.Share:0.00}"))));
        }
    }

    /// <remarks>
    /// Work instruction 573, task 2: with the shadow at 550, 575 and 650 Hz, nothing prints at the shadow's pitch. The order asked
    /// too that the 600 Hz station read whole; alone it reads 21 of 21, and beside its shadow it loses letters of its opening, 20,
    /// 19 and 18 of 21, with the grid judged or not, its own pitch's letters broken where the two tones beat. That half is reported
    /// and not asserted, since no shadow rule reaches it.
    /// </remarks>
    [Theory]
    [InlineData(550)]
    [InlineData(575)]
    [InlineData(650)]
    public void TheShadowPrintsNothing(double shadowHz)
    {
        var (text, atShadow, right, outOf) = Read(shadowHz);

        output.WriteLine(string.Create(Inv, $"shadow at {shadowHz:0} Hz: reads `{text}`; at the shadow `{atShadow}`; the station {right} of {outOf}"));
        Assert.True(atShadow.Length == 0, $"the shadow at {shadowHz:0} Hz printed `{atShadow}`");
    }

    /// <remarks>
    /// Work instruction 573, sections 3 and 4: with the grid judged, W1AW's piece 4 and the one old capture whose strays rose
    /// when the handover shipped, `cw-2026-08-31-003229`. What prints, each letter's pitch and contrast with the sequences
    /// standing as it printed, the handover's trace, and every shadow held by the lanes and the grid. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheRecordingsTraced()
    {
        foreach (var (recording, judged) in new[] { "w1aw-2026-10-07/piece-04", "unadjudicated/cw-2026-08-31-003229" }.SelectMany(r => new[] { (r, false), (r, true) }))
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<string>();
            var trace = new List<string>();
            var chunk = audio.SampleRate / 100;
            var now = 0.0;
            var full = recording.Contains("003229", StringComparison.Ordinal);

            chain.Detector.SetPassband(pitch, width);
            chain.Detector.ShadowJudgesGrid = judged;
            chain.Detector.HandoverTrace = trace;
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add(full
                ? string.Create(Inv, $"{run[^1].ToSeconds:0.0} s {c.Text}@{run.Average(m => m.PitchHz):0} {run.Average(m => m.ContrastDb):0.0} dB [{chain.Detector.SequencesNow(now)}]")
                : string.Create(Inv, $"{c.Text}@{run.Average(m => m.PitchHz):0}"));

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
                now = (at + chunk) / (double)audio.SampleRate;
            }

            chain.Decoder.Flush();

            output.WriteLine(string.Create(Inv, $"{recording}, {pitch:0} Hz, filter {width:0} Hz, {(judged ? "the grid judged to 25 Hz" : "as at HEAD")}:"));
            output.WriteLine("  letters: " + string.Join(full ? "\n    " : " ", letters));
            output.WriteLine("  handover: " + string.Join("; ", trace));
            output.WriteLine("  grid shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Where(h => h.Finder == "grid").Select(h => string.Create(Inv, $"{h.AtSeconds:0.0} s {h.PitchHz:0} Hz of {h.OfPitchHz:0} Hz share {h.Share:0.00}"))));
            output.WriteLine("  lane shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Where(h => h.Finder == "lane").Select(h => string.Create(Inv, $"{h.AtSeconds:0.0} s {h.PitchHz:0} Hz of {h.OfPitchHz:0} Hz share {h.Share:0.00}"))));
        }
    }
}
