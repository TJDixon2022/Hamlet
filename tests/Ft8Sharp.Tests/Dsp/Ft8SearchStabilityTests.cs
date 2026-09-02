using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Step 4's third exit criterion, measured: <b>candidate ranking is stable across runs.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This is not the FFT's determinism and not the waterfall's.</b> Unit 213 measured the transform
/// as bit-identical on a reused plan and the waterfall as byte-identical on a reused monitor, and its
/// report said in as many words that this was the foundation and not the criterion. The criterion is
/// about the <em>ranking</em>: whether the list of places, in the order it comes back, is a function
/// of the audio.
/// </para>
/// <para>
/// <b>Asserted on the values, not on the count.</b> Two lists of the same length with two candidates
/// swapped is exactly the failure a count would hide, so every comparison below is element for
/// element over the whole list — the score, all four position fields and the position in the list.
/// </para>
/// <para>
/// <b>The one that actually catches an unstable sort</b> is
/// <see cref="TheOrderDoesNotDependOnTheOrderTheHypothesesWereGeneratedIn"/>. Two runs of the same
/// code over the same data will agree even when the sort is unstable, because the input order is the
/// same both times. Generating the hypotheses in a different order and requiring the same answer is
/// the test that does not let that through — and it is why the ranking was required to be a total
/// order in the first place.
/// </para>
/// </remarks>
public class Ft8SearchStabilityTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8SearchStabilityTests(ITestOutputHelper output) => _output = output;

    private static float[] SingleSignalSlot()
    {
        var (slot, _) = SearchFixture.OneSignal(Rate, EncodeCorpus.Build()[3], 1417.1875, 3701);
        return slot;
    }

    private static float[] TwentySignalSlot()
    {
        var (audio, _, _) = Ft8SearchPassbandTests.BuildNoisyPassbandSlot(-10.0, seed: 214_060);
        return audio;
    }

    public static TheoryData<string> Slots => new() { "twenty signals in noise", "one signal" };

    private static float[] SlotFor(string which) =>
        which == "one signal" ? SingleSignalSlot() : TwentySignalSlot();

    /// <summary>
    /// Compares two lists whole: same length, and then every field of every element at every
    /// position. Reports what it compared rather than only that it passed.
    /// </summary>
    private void AssertIdentical(
        string what,
        IReadOnlyList<Ft8Candidate> left,
        IReadOnlyList<Ft8Candidate> right)
    {
        Assert.Equal(left.Count, right.Count);

        var fields = 0;
        for (var i = 0; i < left.Count; i++)
        {
            Assert.Equal(left[i].Score, right[i].Score);
            Assert.Equal(left[i].BlockOffset, right[i].BlockOffset);
            Assert.Equal(left[i].TimeSubOffset, right[i].TimeSubOffset);
            Assert.Equal(left[i].BinOffset, right[i].BinOffset);
            Assert.Equal(left[i].FrequencySubOffset, right[i].FrequencySubOffset);
            fields += 5;
        }

        _output.WriteLine(
            $"  {what}: {left.Count} candidates, {fields} field comparisons, all equal - "
            + "and the comparison is on the values at each position, not on the count.");
    }

    /// <summary>Two searches over the same samples return the same list, element for element.</summary>
    [Theory]
    [MemberData(nameof(Slots))]
    public void TwoSearchesOverTheSameSamplesReturnTheSameList(string which)
    {
        var slot = SlotFor(which);

        var first = new Ft8SyncSearch().Find(new Ft8Monitor().Analyse(slot));
        var second = new Ft8SyncSearch().Find(new Ft8Monitor().Analyse(slot));

        Assert.NotEmpty(first);
        AssertIdentical($"{which}, two independent runs", first, second);
    }

    /// <summary>A fresh monitor and one reused after <c>Reset()</c> give the same list.</summary>
    [Theory]
    [MemberData(nameof(Slots))]
    public void AFreshMonitorAndAReusedOneGiveTheSameList(string which)
    {
        var slot = SlotFor(which);
        var search = new Ft8SyncSearch();

        var fresh = search.Find(new Ft8Monitor().Analyse(slot));

        var reused = new Ft8Monitor();
        reused.Analyse(SingleSignalSlot());
        reused.Reset();
        var afterReset = search.Find(reused.Analyse(slot));

        Assert.NotEmpty(fresh);
        AssertIdentical($"{which}, fresh monitor against one reused after Reset()", fresh, afterReset);
    }

    /// <summary>
    /// <b>The one that catches an unstable sort.</b> The whole hypothesis space is enumerated in a
    /// seeded-shuffled order and in a reversed order, scored with the same public primitive the
    /// search uses, filtered by the same minimum and sorted by the same total order — and the answer
    /// has to be the search's own, element for element.
    /// </summary>
    /// <remarks>
    /// Two runs of the same code over the same data agree even when the sort is unstable, because
    /// the generation order is the same both times. This changes it.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Slots))]
    public void TheOrderDoesNotDependOnTheOrderTheHypothesesWereGeneratedIn(string which)
    {
        var slot = SlotFor(which);
        var search = new Ft8SyncSearch();
        var waterfall = new Ft8Monitor().Analyse(slot);

        var fromTheSearch = search.Find(waterfall);
        Assert.NotEmpty(fromTheSearch);

        var geometry = waterfall.Geometry;
        var hypotheses = new List<(int Block, int TimeSub, int Bin, int FreqSub)>();
        for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
        {
            for (var freqSub = 0; freqSub < geometry.FrequencyOversampling; freqSub++)
            {
                for (var block = search.FirstBlockOffset; block <= search.LastBlockOffset; block++)
                {
                    for (var bin = 0; bin <= geometry.BinCount - Ft8SyncSearch.ToneCount; bin++)
                    {
                        hypotheses.Add((block, timeSub, bin, freqSub));
                    }
                }
            }
        }

        _output.WriteLine($"  {hypotheses.Count} hypotheses, re-enumerated two other ways");

        var reversed = Enumerable.Reverse(hypotheses).ToList();

        // Seeded, so the shuffle is itself reproducible. The point is a DIFFERENT order, not an
        // unknown one.
        var shuffled = hypotheses.ToList();
        var random = new Random(214_061);
        for (var i = shuffled.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        Assert.NotEqual(hypotheses[0], shuffled[0]);

        foreach (var (label, order) in new[] { ("reversed", reversed), ("seeded shuffle", shuffled) })
        {
            var built = new List<Ft8Candidate>();
            foreach (var (block, timeSub, bin, freqSub) in order)
            {
                var score = Ft8SyncSearch.ScoreAt(waterfall, block, timeSub, bin, freqSub);
                if (score >= search.MinimumScore)
                {
                    built.Add(new Ft8Candidate(score, block, timeSub, bin, freqSub));
                }
            }

            built.Sort();
            if (built.Count > search.CandidateLimit)
            {
                built.RemoveRange(search.CandidateLimit, built.Count - search.CandidateLimit);
            }

            AssertIdentical($"{which}, generated in {label} order", fromTheSearch, built);
        }
    }

    /// <summary>
    /// Nothing about the answer depends on how many candidates were asked for: the first N of a long
    /// list are the whole of a list of length N. A limit that changed the ORDER of what it kept
    /// would be a ranking that depends on the question rather than on the audio.
    /// </summary>
    [Fact]
    public void ShorteningTheListTruncatesItRatherThanReorderingIt()
    {
        var waterfall = new Ft8Monitor().Analyse(TwentySignalSlot());
        var full = new Ft8SyncSearch(candidateLimit: 500).Find(waterfall);

        foreach (var limit in new[] { 1, 5, 20, 60, 140 })
        {
            var shorter = new Ft8SyncSearch(candidateLimit: limit).Find(waterfall);
            AssertIdentical($"limit {limit} against the first {limit} of {full.Count}",
                full.Take(shorter.Count).ToList(), shorter);
        }
    }

    /// <summary>
    /// The search holds no state between calls: the same instance used many times over different
    /// audio still answers each one the way a fresh instance would.
    /// </summary>
    [Fact]
    public void OneSearchInstanceUsedManyTimesAnswersLikeAFreshOneEveryTime()
    {
        var reused = new Ft8SyncSearch();
        var single = new Ft8Monitor().Analyse(SingleSignalSlot());
        var twenty = new Ft8Monitor().Analyse(TwentySignalSlot());

        var singleFresh = new Ft8SyncSearch().Find(single);
        var twentyFresh = new Ft8SyncSearch().Find(twenty);

        // Alternated deliberately: a search that carried anything over would show it here.
        for (var round = 0; round < 3; round++)
        {
            AssertIdentical($"round {round}, one signal", singleFresh, reused.Find(single));
            AssertIdentical($"round {round}, twenty signals", twentyFresh, reused.Find(twenty));
        }
    }
}
