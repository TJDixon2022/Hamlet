using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The pitch is chosen by which candidate decodes best, and that only works
/// once every candidate is scored against one noise floor.
/// </summary>
/// <remarks>
/// <para>**THE FAULT THESE PIN, IN ONE SENTENCE.**
/// <see cref="CwProbabilisticResult.LikelihoodRatio"/> takes its idea of noise
/// from the same envelope it is scoring, so it is scale invariant and has no
/// common unit between two pitches. A bin the receiver's filter has already
/// emptied has almost no noise left in it, its residual wobble is scored against
/// a tiny sigma, and it looks like the cleanest keying in the band. Ranking on
/// the bare score picked the station on one of this repository's forty-four
/// captures; with the common floor it picks it on thirty-four.</para>
/// <para>**THEY ARE WRITTEN AGAINST GENERATED AUDIO WITH A BAND IN IT**, because
/// the fault only appears where the band is not flat. A fixture with digital
/// silence outside its tone cannot show it at all, which is HM-OPEN-018's lesson
/// and §12.5's.</para>
/// </remarks>
public sealed class TheQuietestBinNoLongerWinsTests
{
    /// <summary>Where the station sits in every fixture below.</summary>
    private const double StationHz = 500;

    private readonly ITestOutputHelper _output;

    public TheQuietestBinNoLongerWinsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A bin holding nothing scores near nothing once it is stood on the band's
    /// own floor.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE HALF THAT WAS BROKEN.** Without the pedestal an empty bin is
    /// the highest-scoring place in the band, which is exactly backwards, and it
    /// is what put a page of `E`s on the screen at 875 Hz while a net was being
    /// worked at 430 (§0.0).
    /// </remarks>
    [Fact]
    public void AnEmptyBinScoresNearNothing()
    {
        var audio = Signal("CQ CQ DE W1AW K");
        var candidates = CwPitchRanking.Candidates();
        var scores = CwPitchRanking.Score(audio.Samples, audio.SampleRate, candidates);

        Assert.NotNull(scores);

        var station = Nearest(candidates, StationHz);
        var empty = Nearest(candidates, 875);

        _output.WriteLine(
            $"station {candidates[station]:0} Hz: {scores[station]:0.000}");
        _output.WriteLine(
            $"empty   {candidates[empty]:0} Hz: {scores[empty]:0.000}");

        Assert.True(
            scores[empty] < 1.0,
            $"an empty bin at {candidates[empty]:0} Hz scored {scores[empty]:0.000}, "
            + "which is not near nothing");

        Assert.True(
            scores[station] > scores[empty],
            $"the station at {candidates[station]:0} Hz scored {scores[station]:0.000} "
            + $"and an empty bin at {candidates[empty]:0} Hz scored "
            + $"{scores[empty]:0.000}");
    }

    /// <summary>A bin holding a keyed station keeps its structure and wins.</summary>
    /// <remarks>
    /// The pedestal must not buy the empty bins' silence by flattening the
    /// station too. A floor high enough to bury the signal would pass the test
    /// above and be useless.
    /// </remarks>
    [Fact]
    public void TheStationWinsTheBand()
    {
        var audio = Signal("CQ CQ DE W1AW K");
        var candidates = CwPitchRanking.Candidates();
        var ranked = CwPitchRanking.Rank(audio.Samples, audio.SampleRate);

        Assert.True(ranked.Ranked, "nothing was ranked at all");

        _output.WriteLine(
            $"winner {ranked.ToneHz:0} Hz at {ranked.Score:0.000}, "
            + $"runner-up {ranked.RunnerUpHz:0} Hz at {ranked.RunnerUpScore:0.000}");

        Assert.True(
            Math.Abs(ranked.ToneHz - StationHz) <= CwToneTracker.CoarseSpacingHz,
            $"the station is at {StationHz:0} Hz and the ranking chose "
            + $"{ranked.ToneHz:0} Hz");

        _ = candidates;
    }

    /// <summary>
    /// On real audio, and only on real audio, the bare score picks a bin the
    /// receiver emptied and the pedestal picks the station.
    /// </summary>
    /// <remarks>
    /// <para>**A TEST THAT ONLY PROVES THE FIX WORKS CANNOT SHOW THE FIX WAS
    /// NEEDED.** This one goes red if somebody removes the pedestal and keeps
    /// the rest green by loosening it.</para>
    /// <para>**AND IT HAS TO BE A REAL RECORDING, WHICH IS THIS UNIT'S OWN
    /// §12.5 FINDING.** Run against `CwFixtureGenerator`'s shaped band the bare
    /// score picks the station correctly, at 500 Hz scoring 39.45 — so the
    /// generated fixtures cannot show this fault at all, and a session working
    /// only from them would conclude there was nothing to fix. The receiver's
    /// own 500 Hz filter empties the bins outside it far harder than the
    /// generator's band shaping does. `cw-2026-08-28-004844` holds a real net at
    /// 430 Hz, and the bare score there picks 875.</para>
    /// </remarks>
    [Fact]
    public void OnRealAudioTheBareScorePicksAnEmptyBin()
    {
        var audio = Tail(
            WavAudio.Read(Path.Combine(
                CapturedSignalTests.Folder,
                "unadjudicated",
                "cw-2026-08-28-004844.wav")),
            CwProbabilisticStream.WindowSeconds);

        var candidates = CwPitchRanking.Candidates();
        var bare = new double[candidates.Count];

        for (var i = 0; i < candidates.Count; i++)
        {
            bare[i] = CwProbabilisticDecoder
                .DecodeUngated(
                    CwProbabilisticDecoder.Envelope(
                        audio.Samples, audio.SampleRate, candidates[i]),
                    candidates[i])
                .LikelihoodRatio;
        }

        var bareWinner = 0;

        for (var i = 1; i < bare.Length; i++)
        {
            if (bare[i] > bare[bareWinner])
            {
                bareWinner = i;
            }
        }

        var stood = CwPitchRanking.Rank(audio.Samples, audio.SampleRate);

        _output.WriteLine(
            $"bare winner     {candidates[bareWinner]:0} Hz at {bare[bareWinner]:0.00}");
        _output.WriteLine(
            $"pedestal winner {stood.ToneHz:0} Hz at {stood.Score:0.00}");

        // The sheet written at the moment of capture measured the station at
        // 430.0 Hz from the keying the survey admitted.
        const double NetHz = 430;

        Assert.True(
            Math.Abs(candidates[bareWinner] - NetHz) > CwToneTracker.CoarseSpacingHz,
            $"the bare score picked {candidates[bareWinner]:0} Hz, which is the "
            + "station — the pedestal is no longer buying anything here and this "
            + "test has stopped meaning what it says");

        Assert.True(
            Math.Abs(stood.ToneHz - NetHz) <= CwToneTracker.CoarseSpacingHz,
            $"the net is at {NetHz:0} Hz and the ranking chose {stood.ToneHz:0} Hz");
    }

    /// <summary>The last stretch of a recording.</summary>
    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(seconds * audio.SampleRate);
        var from = Math.Max(0, audio.Samples.Length - want);

        return new MonoAudio(audio.SampleRate, audio.Samples[from..]);
    }

    /// <summary>Nothing is ranked from audio too short to hold a character.</summary>
    /// <remarks>
    /// **A PITCH NOBODY MEASURED MUST NOT PRODUCE LETTERS THAT IMPLY IT WAS
    /// MEASURED** (§0.0, HM-DEC-009). Refusing hands the caller back whatever it
    /// already had rather than a number from four hops of noise.
    /// </remarks>
    [Fact]
    public void NothingIsRankedFromTooLittleAudio()
    {
        var audio = Signal("E");
        var scrap = audio.Samples[..Math.Min(200, audio.Samples.Length)];

        var ranked = CwPitchRanking.Rank(scrap, audio.SampleRate);

        Assert.False(
            ranked.Ranked,
            $"a scrap of audio was ranked at {ranked.ToneHz:0} Hz");
    }

    /// <summary>Digital silence is ranked as nothing rather than as a band.</summary>
    /// <remarks>
    /// An all-zero buffer is an absence of measurement and not a quiet band
    /// (HM-DEC-120). The floor is nought there, and a pedestal of nought would
    /// leave every candidate exactly as scale invariant as before.
    /// </remarks>
    [Fact]
    public void DigitalSilenceIsNotRanked()
    {
        var silence = new float[48_000 * 4];

        var ranked = CwPitchRanking.Rank(silence, 48_000);

        Assert.False(
            ranked.Ranked,
            $"an all-zero buffer was ranked at {ranked.ToneHz:0} Hz");
    }

    /// <summary>Standing on a floor adds in power, not in amplitude.</summary>
    /// <remarks>
    /// **ADDING THE FLOOR TO THE MAGNITUDE WOULD DO NOTHING AT ALL.** It shifts
    /// every value by the same amount, leaves the spread untouched, and therefore
    /// leaves the score untouched — which is the scale invariance being removed.
    /// This pins the arithmetic so that mistake cannot be made silently.
    /// </remarks>
    [Fact]
    public void TheFloorIsAddedInPower()
    {
        var stood = CwPitchRanking.StandOn([3.0, 0.0], 4.0);

        Assert.Equal(5.0, stood[0], 9);
        Assert.Equal(4.0, stood[1], 9);
    }

    /// <summary>The candidate set is the tracker's own coarse grid.</summary>
    /// <remarks>
    /// The ranking and the survey answer one question about one set of places.
    /// A second grid would be a second set of places for them to disagree about.
    /// </remarks>
    [Fact]
    public void TheCandidatesAreTheTrackersOwnBins()
    {
        var candidates = CwPitchRanking.Candidates();

        Assert.Equal(25, candidates.Count);
        Assert.Equal(CwToneTracker.MinimumToneHz, candidates[0]);
        Assert.Equal(CwToneTracker.MaximumToneHz, candidates[^1]);
        Assert.Equal(
            CwToneTracker.CoarseSpacingHz, candidates[1] - candidates[0], 9);
    }

    /// <summary>
    /// Generated Morse at <see cref="StationHz"/>, inside a receiver's own
    /// passband.
    /// </summary>
    /// <remarks>
    /// **IT HAS TO BE THE SHAPED BAND AND NOT A BARE TONE.** The fault these
    /// tests pin only exists where the band is not flat: it is the receiver's
    /// filter emptying the bins outside its passband that gives them a noise
    /// floor small enough to score against. A fixture that is tone and digital
    /// silence cannot show it, which is HM-OPEN-018's whole finding.
    /// </remarks>
    private static MonoAudio Signal(string text)
        => CwFixtureGenerator.Generate(new CwFixtureRecipe(
            Name: "pitch-ranking",
            Text: text,
            ToneHz: StationHz,
            SignalToNoiseDb: 18)).Audio;

    /// <summary>The index of the candidate nearest a pitch.</summary>
    private static int Nearest(IReadOnlyList<double> candidates, double hz)
    {
        var best = 0;

        for (var i = 1; i < candidates.Count; i++)
        {
            if (Math.Abs(candidates[i] - hz) < Math.Abs(candidates[best] - hz))
            {
                best = i;
            }
        }

        return best;
    }
}
