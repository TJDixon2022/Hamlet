using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// What the search does at its edges: the refusals, the bounds, and the property that makes its
/// ranking a ranking.
/// </summary>
/// <remarks>
/// <b>Whether it finds anything is measured elsewhere</b> — <c>Ft8SearchRecoveryTests</c>
/// takes that number over a corpus. This file is the other half: that a request it cannot satisfy
/// comes back cleanly rather than nearly, and that no two candidates it returns are ever left
/// undecided against each other.
/// </remarks>
public class Ft8SyncSearchTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8SyncSearchTests(ITestOutputHelper output) => _output = output;

    private static EncodeCorpus.Entry FirstMessage() => EncodeCorpus.Build()[0];

    private static Ft8Waterfall OneSignalWaterfall(double baseHz = 1000.0, int offset = 0)
    {
        var (slot, _) = SearchFixture.OneSignal(Rate, FirstMessage(), baseHz, offset);
        return new Ft8Monitor().Analyse(slot);
    }

    /// <summary>
    /// The whole point of the type, asserted at the level of its signature: there is nowhere to put
    /// a hint. A search that could be told where to look could not be shown not to have been.
    /// </summary>
    [Fact]
    public void TheSearchHasNoParameterThroughWhichItCouldBeToldWhereToLook()
    {
        var overloads = typeof(Ft8SyncSearch)
            .GetMethods()
            .Where(m => m.Name == nameof(Ft8SyncSearch.Find))
            .ToArray();

        Assert.NotEmpty(overloads);

        foreach (var overload in overloads)
        {
            var parameters = overload.GetParameters();
            _output.WriteLine(
                $"  Find({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))})");

            foreach (var parameter in parameters)
            {
                var name = parameter.Name!.ToLowerInvariant();
                Assert.DoesNotContain("freq", name, StringComparison.Ordinal);
                Assert.DoesNotContain("hertz", name, StringComparison.Ordinal);
                Assert.DoesNotContain("time", name, StringComparison.Ordinal);
                Assert.DoesNotContain("offset", name, StringComparison.Ordinal);
                Assert.DoesNotContain("expect", name, StringComparison.Ordinal);
                Assert.DoesNotContain("hint", name, StringComparison.Ordinal);
                Assert.DoesNotContain("truth", name, StringComparison.Ordinal);
            }
        }

        _output.WriteLine("  the samples or the waterfall, and the extents. Nothing else.");
    }

    /// <summary>A hypothesis with no sync symbol inside the analysis scores zero, not an exception.</summary>
    [Fact]
    public void AHypothesisEntirelyOutsideTheAnalysedBlocksScoresZero()
    {
        var waterfall = OneSignalWaterfall();

        var pastTheEnd = Ft8SyncSearch.ScoreAt(waterfall, waterfall.BlockCount + 1, 0, 100, 0);
        var beforeTheStart = Ft8SyncSearch.ScoreAt(waterfall, -1000, 0, 100, 0);

        Assert.Equal(0, pastTheEnd);
        Assert.Equal(0, beforeTheStart);
        _output.WriteLine($"  {waterfall.BlockCount} blocks analysed; a hypothesis outside them scores 0.");
    }

    /// <summary>Watched refusing: a minimum no hypothesis reaches gives an empty list, cleanly.</summary>
    [Fact]
    public void AMinimumScoreNoHypothesisReachesReturnsAnEmptyList()
    {
        var waterfall = OneSignalWaterfall();

        var reachable = new Ft8SyncSearch().Find(waterfall);
        Assert.NotEmpty(reachable);
        var best = reachable[0].Score;

        var unreachable = new Ft8SyncSearch(minimumScore: best + 1).Find(waterfall);

        Assert.Empty(unreachable);
        _output.WriteLine(
            $"  best score on this slot is {best}; a minimum of {best + 1} returns "
            + $"{unreachable.Count} candidates - empty, not an exception and not a partly filled list.");
    }

    /// <summary>Watched refusing: asking for none gives none.</summary>
    [Fact]
    public void ACandidateLimitOfZeroReturnsAnEmptyList()
    {
        var found = new Ft8SyncSearch(candidateLimit: 0).Find(OneSignalWaterfall());

        Assert.Empty(found);
        _output.WriteLine("  a limit of zero returns zero candidates and does not throw.");
    }

    /// <summary>
    /// Watched refusing: asking for more than exists returns what exists — every hypothesis at or
    /// above the minimum and not one entry more, rather than a list padded to the limit.
    /// </summary>
    [Fact]
    public void MoreCandidatesThanTheSlotCanSupplyReturnsWhatThereIsAndNothingBesides()
    {
        var waterfall = OneSignalWaterfall();
        var search = new Ft8SyncSearch(candidateLimit: 1_000_000);
        var found = search.Find(waterfall);

        // Counted independently, by walking the same hypothesis space and scoring it directly.
        var geometry = waterfall.Geometry;
        var qualifying = 0;
        for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
        {
            for (var freqSub = 0; freqSub < geometry.FrequencyOversampling; freqSub++)
            {
                for (var block = search.FirstBlockOffset; block <= search.LastBlockOffset; block++)
                {
                    for (var bin = 0; bin <= geometry.BinCount - Ft8SyncSearch.ToneCount; bin++)
                    {
                        if (Ft8SyncSearch.ScoreAt(waterfall, block, timeSub, bin, freqSub)
                            >= search.MinimumScore)
                        {
                            qualifying++;
                        }
                    }
                }
            }
        }

        Assert.Equal(qualifying, found.Count);
        Assert.True(found.Count < 1_000_000);
        _output.WriteLine(
            $"  {qualifying} hypotheses reach the minimum of {search.MinimumScore}; asking for "
            + $"1,000,000 returns exactly those {found.Count} and stops.");
    }

    /// <summary>
    /// No candidate below the minimum is ever returned, at any rank, at any limit. This is the one
    /// that would catch a list padded out to its limit with whatever was next.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void NoCandidateBelowTheMinimumIsEverReturned(int minimum)
    {
        var found = new Ft8SyncSearch(candidateLimit: 5000, minimumScore: minimum)
            .Find(OneSignalWaterfall());

        Assert.All(found, candidate => Assert.True(candidate.Score >= minimum));
        _output.WriteLine(
            $"  minimum {minimum,3}: {found.Count,5} candidates, weakest "
            + $"{(found.Count > 0 ? found[^1].Score : 0)}");
    }

    /// <summary>
    /// The ranking is a total order: sorted best first, and <b>no two distinct candidates compare
    /// equal</b>, which is what makes the order a function of the input rather than of the sort.
    /// </summary>
    [Fact]
    public void TheRankingIsATotalOrderWithNoTwoCandidatesLeftUndecided()
    {
        // The minimum is dropped to zero on purpose: the ordering has to hold over the whole list a
        // caller could ask for, not only over the few dozen a clean signal puts above ten.
        var found = new Ft8SyncSearch(candidateLimit: 3000, minimumScore: 0)
            .Find(OneSignalWaterfall());
        Assert.True(found.Count > 100, "this slot should produce plenty of candidates to order.");

        var ties = 0;
        for (var i = 1; i < found.Count; i++)
        {
            Assert.True(
                found[i - 1].CompareTo(found[i]) < 0,
                $"candidates {i - 1} and {i} are not in strictly descending order.");

            if (found[i - 1].Score == found[i].Score)
            {
                ties++;
            }
        }

        _output.WriteLine($"  {found.Count} candidates, {ties} adjacent pairs tied on score alone.");
        _output.WriteLine(
            "  every one of those ties is decided by the block offset, then the time sub-offset,");
        _output.WriteLine(
            "  then the bin offset, then the frequency sub-offset - upstream decides none of them.");
        Assert.True(ties > 0, "if nothing ties, this measurement is not testing what it claims to.");
    }

    /// <summary>Two candidates differing only in position are still ordered, and reflexively so.</summary>
    [Fact]
    public void TheOrderingIsConsistentOnCandidatesThatDifferOnlyInPosition()
    {
        var a = new Ft8Candidate(40, 3, 0, 100, 0);
        var b = new Ft8Candidate(40, 3, 0, 100, 1);
        var c = new Ft8Candidate(40, 3, 1, 100, 0);
        var d = new Ft8Candidate(41, 9, 1, 400, 1);

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.True(a.CompareTo(c) < 0);
        Assert.True(d.CompareTo(a) < 0, "a higher score outranks everything below it.");
        Assert.Equal(0, a.CompareTo(a));

        var sorted = new List<Ft8Candidate> { b, a, d, c };
        sorted.Sort();
        Assert.Equal(new[] { d, a, b, c }, sorted);
        _output.WriteLine("  score descending, then block, time sub, bin, frequency sub - all ascending.");
    }

    /// <summary>Refusals in the constructor and in the scoring primitive, each watched refusing.</summary>
    [Fact]
    public void TheRefusalsAreWatchedRefusing()
    {
        var waterfall = OneSignalWaterfall();
        var geometry = waterfall.Geometry;

        var negativeLimit = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8SyncSearch(candidateLimit: -1));
        var invertedRange = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8SyncSearch(firstBlockOffset: 5, lastBlockOffset: 4));
        var nullWaterfall = Assert.Throws<ArgumentNullException>(
            () => new Ft8SyncSearch().Find((Ft8Waterfall)null!));
        var badTimeSub = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8SyncSearch.ScoreAt(waterfall, 0, geometry.TimeOversampling, 0, 0));
        var badFreqSub = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8SyncSearch.ScoreAt(waterfall, 0, 0, 0, geometry.FrequencyOversampling));
        var binTooHigh = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8SyncSearch.ScoreAt(
                waterfall, 0, 0, geometry.BinCount - Ft8SyncSearch.ToneCount + 1, 0));
        var binTooLow = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8SyncSearch.ScoreAt(waterfall, 0, 0, -1, 0));

        foreach (var refusal in new Exception[]
                 {
                     negativeLimit, invertedRange, nullWaterfall, badTimeSub, badFreqSub, binTooHigh,
                     binTooLow,
                 })
        {
            _output.WriteLine($"  refused: {refusal.Message.Split('\n')[0]}");
        }

        // And the last bin offset that IS allowed is allowed, so the bound is where it says it is.
        var highest = Ft8SyncSearch.ScoreAt(
            waterfall, 0, 0, geometry.BinCount - Ft8SyncSearch.ToneCount, 0);
        _output.WriteLine(
            $"  the highest legal bin offset is {geometry.BinCount - Ft8SyncSearch.ToneCount} and it "
            + $"scores {highest} rather than refusing.");
    }

    /// <summary>
    /// The two entry points agree: handing over the samples and handing over the waterfall built
    /// from them give the same list. <b>Neither is given anything else.</b>
    /// </summary>
    [Fact]
    public void SearchingTheSamplesAndSearchingTheirWaterfallGiveTheSameAnswer()
    {
        var (slot, _) = SearchFixture.OneSignal(Rate, FirstMessage(), 1234.5, 4321);
        var search = new Ft8SyncSearch();

        var fromSamples = search.Find(slot);
        var fromWaterfall = search.Find(new Ft8Monitor().Analyse(slot));

        Assert.Equal(fromWaterfall.Count, fromSamples.Count);
        Assert.Equal(fromWaterfall, fromSamples);
        _output.WriteLine($"  {fromSamples.Count} candidates, identical by both routes.");
    }

    /// <summary>
    /// A whole slot searched in a time worth reporting, because a search that takes minutes would
    /// change what step 5 can afford. Not asserted as a bound - reported as a measurement.
    /// </summary>
    [Fact]
    public void AWholeSlotIsSearchedInATimeWorthReporting()
    {
        var (slot, _) = SearchFixture.OneSignal(Rate, FirstMessage(), 1000.0, 0);
        var search = new Ft8SyncSearch();

        var waterfall = new Ft8Monitor().Analyse(slot);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var found = search.Find(waterfall);
        clock.Stop();

        var geometry = waterfall.Geometry;
        var hypotheses = geometry.TimeOversampling
            * geometry.FrequencyOversampling
            * (search.LastBlockOffset - search.FirstBlockOffset + 1)
            * (geometry.BinCount - Ft8SyncSearch.ToneCount + 1);

        _output.WriteLine($"  {hypotheses} hypotheses scored in {clock.ElapsedMilliseconds} ms");
        _output.WriteLine($"  {found.Count} candidates returned");
        Assert.NotEmpty(found);
    }
}
