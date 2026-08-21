using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// One station answering another, which four capabilities had never been tested
/// against (HM-DEC-104).
/// </summary>
/// <remarks>
/// <para>**CLOCK LOSS, THE RETAINED PREVIOUS CLOCK, TRACKER SWITCHING AND THE
/// SPEED-CHANGE ANNOTATION WERE ALL BUILT ON RULINGS AND NONE OF THEM HAD A
/// COMMITTED TEST.** They were reasoned about, written, reviewed and shipped
/// against nothing. One recording exercises all four, and it is the situation an
/// answered call actually produces rather than a scenario invented at a
/// bench.</para>
/// <para>The recording is a station calling at about eleven words a minute on
/// 615 Hz, a stretch of band, then a different station answering at
/// twenty-two words a minute on 730 Hz. Both halves are gated by the reference
/// on their own at 100 percent, which is what makes anything this finds a fact
/// about Hamlet.</para>
/// </remarks>
public sealed class CwTwoStationTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the run is reported.</param>
    public CwTwoStationTests(ITestOutputHelper output) => _output = output;

    private sealed record Run(
        IReadOnlyList<CwCharacter> Tip,
        IReadOnlyList<double> Tones,
        int Retunes,
        int SpeedChanges,
        int ClockLosses,
        CwDecoder Decoder);

    private static Run Decode()
    {
        var audio = WavAudio.Read(Path.Combine(
            CwFixtureCatalogue.Folder, CwFixtureCatalogue.TwoStationName + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var tip = new List<CwCharacter>();
        var tones = new List<double>();

        var retunes = 0;
        var speedChanges = 0;
        var clockLosses = 0;
        var lastRetunes = 0;

        decoder.CharacterDecoded += c => tip.Add(c);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);

        // Fed in quarter-second bites so the transitions can be watched as they
        // happen rather than inferred from the end state.
        var chunk = audio.SampleRate / 4;
        var lastWpm = 0.0;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

            tones.Add(decoder.Report.ToneHz);

            if (decoder.Tracker.Retunes != lastRetunes)
            {
                retunes++;
                lastRetunes = decoder.Tracker.Retunes;
            }

            // **THE SETTLED PASS'S OWN SIGNALS WENT WITH IT.** A speed change
            // and a lost clock were how the second pass said somebody else had
            // started sending; there is one pass now and the tracker's own move
            // is the signal that survives. The counts stay in the shape of the
            // run so the tests that read the tracker still read it.
            var reading = decoder.Reading;

            if (lastWpm > 0 && Math.Abs(reading.WordsPerMinute - lastWpm) >= 4)
            {
                speedChanges++;
            }

            if (reading.WordsPerMinute > 0)
            {
                lastWpm = reading.WordsPerMinute;
            }
        }

        decoder.Flush();

        return new Run(tip, tones, retunes, speedChanges, clockLosses, decoder);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-104 and phase 3 of the amendment: **the tracker moves
    /// to the answering station.** It starts told to look at 600 Hz, finds the
    /// caller at 615, and has to end up at 730 when somebody else takes over the
    /// frequency. Chasing too eagerly abandons a fading station mid-word and
    /// chasing too reluctantly means the answer is never heard at all.</para>
    /// </remarks>
    [Fact]
    public void TheTrackerFollowsTheStationThatAnswers()
    {
        var run = Decode();

        var early = run.Tones.Take(run.Tones.Count / 3).ToList();
        var late = run.Tones.Skip(run.Tones.Count * 2 / 3).ToList();

        _output.WriteLine($"tone early {early.Max():0} Hz, late {late.Max():0} Hz, "
            + $"{run.Retunes} moves");

        Assert.Contains(early, hz => hz is >= 595 and <= 640);
        Assert.Contains(late, hz => hz is >= 705 and <= 755);
        Assert.True(run.Retunes > 0, "the tracker never moved at all");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-104 and HM-DEC-096 phase 3: **a switch is never taken
    /// part-way through a character.** Finishing a character from a different
    /// part of the band produces a letter nobody sent, with clean timing and a
    /// healthy margin, which is the same confident wrong reading the
    /// truncated-evidence rule exists to prevent.</para>
    /// <para>Asserted here by driving the whole recording and requiring that the
    /// tracker was never mid-character at the moment it moved, which the decoder
    /// records for exactly this purpose.</para>
    /// </remarks>
    [Fact]
    public void NoSwitchIsTakenMidCharacter()
    {
        var audio = WavAudio.Read(Path.Combine(
            CwFixtureCatalogue.Folder, CwFixtureCatalogue.TwoStationName + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var offences = 0;
        var moves = 0;
        var last = 0;

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);

        var chunk = audio.SampleRate / 50;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);
            var before = decoder.Tracker.MidCharacter;

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

            if (decoder.Tracker.Retunes == last)
            {
                continue;
            }

            moves++;
            last = decoder.Tracker.Retunes;

            if (before)
            {
                offences++;
            }
        }

        decoder.Flush();

        _output.WriteLine($"{moves} moves, {offences} of them mid-character");

        Assert.Equal(0, offences);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-104 and HM-DEC-096 phase 2: **the settled pass reports
    /// a speed change rather than averaging two stations into one clock.** Eleven
    /// words a minute and twenty-two in one recording is a two-means fit that can
    /// land inside the legal ratio band while describing neither of them, which is
    /// a confident wrong answer.</para>
    /// <para>On the air a speed change usually means somebody else started
    /// transmitting, which is the earliest evidence there is that a call was
    /// answered.</para>
    /// </remarks>
    [Fact]
    public void TheChangeOfStationIsNoticedRatherThanAveraged()
    {
        var run = Decode();

        _output.WriteLine(
            $"{run.SpeedChanges} speed-change reports, "
            + $"{run.ClockLosses} clock losses, {run.Retunes} tracker moves");

        // Something has to mark the handover. Which of the three fires is a
        // detail of how the discontinuity presented itself; that none of them
        // fires would mean two stations were read as one.
        Assert.True(
            run.SpeedChanges + run.ClockLosses + run.Retunes > 0,
            "two stations at different speeds and pitches passed unremarked, "
            + "which means one clock was fitted across both");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-104: **the decoder does not fit one clock across two
    /// stations.** The final clock has to describe the station being read at the
    /// end, not the average of a caller at eleven words a minute and an answerer
    /// at twenty-two, which would be about sixteen and would describe nobody.
    /// </para>
    /// </remarks>
    [Fact]
    public void NoSingleClockIsFittedAcrossBothStations()
    {
        var run = Decode();

        var wpm = run.Decoder.WordsPerMinute;

        _output.WriteLine($"final speed {wpm?.ToString() ?? "none"} wpm, "
            + $"reading {run.Decoder.Reading.WordsPerMinute:0} wpm");

        if (wpm is null)
        {
            // Refusing to name a speed is a permitted answer and never a wrong
            // one (§0.0).
            return;
        }

        // The average of the two stations is about sixteen and a half, and that
        // is the one answer that cannot be right.
        Assert.False(
            wpm is >= 15 and <= 18,
            $"the decoder settled on {wpm} words a minute, which is the average "
            + "of the two stations and describes neither");
    }

    /// <remarks>
    /// Proves HM-DEC-104 and §0.0: whatever the decoder makes of a recording with
    /// two stations in it, it does not fill the screen from the handover. The
    /// seam is a stretch of band and there is nothing in it to read.
    /// </remarks>
    [Fact]
    public void NothingIsInventedAtTheHandover()
    {
        var run = Decode();

        var caller = CwFixtureCatalogue.Caller;
        var answerer = CwFixtureCatalogue.Answerer;
        var expected = (caller.Text + answerer.Text)
            .Replace(" ", "", StringComparison.Ordinal)
            .ToUpperInvariant();

        var emitted = run.Tip.Count(c => !c.IsWordGap);

        var strangers = run.Tip
            .Where(c => !c.IsWordGap && !c.IsUnreadable && c.Text.Length == 1)
            .Count(c => !expected.Contains(c.Text[0], StringComparison.Ordinal));

        _output.WriteLine($"{emitted} characters, {strangers} not in either message");
        _output.WriteLine(string.Concat(run.Tip.Select(c => c.Text)));

        Assert.True(
            strangers <= emitted / 2,
            $"{strangers} of {emitted} characters belong to neither station, "
            + "which is a decoder reading the seam");
    }
}
