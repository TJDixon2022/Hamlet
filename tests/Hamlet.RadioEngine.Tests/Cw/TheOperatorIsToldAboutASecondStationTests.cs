using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A second station in the same passband is named, with the control that
/// changes it, and silence is never reported as a clear frequency.
/// </summary>
/// <remarks>
/// <para>**THE SURVEY HAS ALWAYS KNOWN THIS AND NOTHING READ IT.**
/// `CwToneSurvey.Candidates` returns every admitted keying bin with a pitch and
/// a lift, `CwToneTracker.CoarseCandidates` hands them out, and until this unit
/// the only caller in the tree was none. So this is plumbing rather than
/// detection, and the tests are about what is said rather than about what is
/// found.</para>
/// <para>**THE TWO THAT MATTER MOST ARE THE NEGATIVE ONES** (§0.0). A station
/// alone must produce no mention at all, because advice about a knob that is
/// already right is noise; and no competitor found must never be phrased as the
/// frequency being clear, because the survey wants three seconds and eight clean
/// marks before it admits anything and a station that has just started is absent
/// here and present on the air (HM-DEC-009).</para>
/// </remarks>
public sealed class TheOperatorIsToldAboutASecondStationTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheOperatorIsToldAboutASecondStationTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The strongest competitor seen at any moment while the recording played.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">Where to start looking.</param>
    /// <returns>The competitor, or null if none was ever found.</returns>
    /// <remarks>
    /// **A COMPETITOR IS A LIVE FACT AND READING IT AT THE END FINDS NOTHING.**
    /// The survey keeps three seconds of rolling history, so once a recording has
    /// been played to its end that history holds the trailing silence and admits
    /// no bins at all — not even the station the whole file was about. The first
    /// draft of these tests read the report after the audio finished and
    /// concluded the feature did not work.
    /// <para>The panel and the capture sheet both ask this question while
    /// somebody is listening, so the tests ask it the same way.</para>
    /// </remarks>
    private static CwCompetitor? Sweep(MonoAudio audio, double toneHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, toneHz);
        var hop = decoder.Tracker.HopSamples;

        CwCompetitor? loudest = null;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.Competitor is not { } found)
            {
                continue;
            }

            if (loudest is not { } best || found.RelativeDb > best.RelativeDb)
            {
                loudest = found;
            }
        }

        return loudest;
    }

    /// <remarks>
    /// Proves the whole path from the survey's runners-up to a fact a panel can
    /// show: two stations in one passband, and the offset and relative strength
    /// come back measured.
    /// </remarks>
    [Fact]
    public void ASecondStationInThePassbandIsFoundAndMeasured()
    {
        var seen = Sweep(
            CwTwoInOnePassband.Audio(200, 0), CwTwoInOnePassband.WantedToneHz);

        _output.WriteLine(
            seen is { } found
                ? $"{found.OffsetHz:+0;-0} Hz at {found.RelativeDb:+0.0;-0.0} dB "
                  + $"({found.ToneHz:0} Hz)"
                : "nothing found at any moment");

        Assert.NotNull(seen);

        var competitor = seen!.Value;

        Assert.True(
            Math.Abs(competitor.OffsetHz) >= CwCompetitor.SeparationHz,
            $"a competitor {competitor.OffsetHz:0} Hz away is the same signal, "
            + "not somebody else.");

        Assert.True(
            competitor.RelativeDb >= CwCompetitor.QuietestWorthSayingDb,
            $"{competitor.RelativeDb:0.0} dB down is not in anybody's way.");
    }

    /// <remarks>
    /// Proves the sentence does the job HM-DEC-148 requires of it: it names the
    /// control on the front of the radio rather than stopping at the diagnosis,
    /// and it never offers to turn anything itself.
    /// </remarks>
    [Fact]
    public void TheSentenceNamesTheControlAndOffersToChangeNothing()
    {
        var competitor = new CwCompetitor(180, -4, 780);
        var sentence = competitor.Sentence;

        _output.WriteLine(sentence);

        Assert.Contains("180 hertz", sentence, StringComparison.Ordinal);
        Assert.Contains("above", sentence, StringComparison.Ordinal);
        Assert.Contains("FILTER", sentence, StringComparison.Ordinal);
        Assert.Contains("TWIN PBT", sentence, StringComparison.Ordinal);

        // Read-only, and it says so by never offering (HM-DEC-084, HM-DEC-148).
        // **THESE ARE WHOLE PHRASES ON PURPOSE.** The first draft looked for
        // "let me" case-insensitively and found it inside "Hamlet measures",
        // which is a test failing on its own carelessness rather than on the
        // sentence.
        foreach (var offer in new[]
        {
            "Hamlet will set", "shall I", "press here", "click here",
            "Let me ", "I will change",
        })
        {
            Assert.DoesNotContain(
                offer, sentence, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves the side is named in words rather than left as a sign, and that it
    /// follows the measurement rather than a convention somebody has to remember.
    /// </remarks>
    [Fact]
    public void SomebodyBelowIsSaidToBeBelow()
    {
        Assert.Equal("above", new CwCompetitor(120, -3, 720).Side);
        Assert.Equal("below", new CwCompetitor(-120, -3, 480).Side);

        Assert.Contains(
            "below", new CwCompetitor(-120, -3, 480).Sentence,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// **Proves the first negative.** A station on its own says nothing, because
    /// advice about a knob that is already in the right place is noise and it
    /// teaches the operator to read past the panel.
    /// </remarks>
    [Fact]
    public void AStationOnItsOwnProducesNoMentionAtAll()
    {
        var seen = Sweep(
            CwTwoInOnePassband.Alone(), CwTwoInOnePassband.WantedToneHz);

        _output.WriteLine(
            seen is { } found
                ? $"found {found.OffsetHz:+0;-0} Hz at {found.RelativeDb:+0.0;-0.0} dB"
                : "nothing found at any moment, which is right");

        Assert.Null(seen);
    }

    /// <remarks>
    /// **Proves the second negative, which is the one §0.0 turns on.** Audio with
    /// no station in it produces no competitor, and the absence must never be
    /// rendered as a claim that the frequency is clear.
    /// </remarks>
    [Fact]
    public void AnEmptyBandIsNotReportedAsClear()
    {
        var seen = Sweep(
            WavAudio.Read(
                Path.Combine(
                    CapturedSignalTests.Folder,
                    "unadjudicated",
                    "cw-2026-08-20-014935.wav")),
            600);

        _output.WriteLine(
            seen is { } found
                ? $"found {found.OffsetHz:+0;-0} Hz at {found.RelativeDb:+0.0;-0.0} dB"
                : "nothing found at any moment");

        // Nothing is keyed anywhere in this recording, so there is nothing for a
        // competitor to be a competitor to.
        Assert.Null(seen);
    }
}
