using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The per-band activity indicator and its hover evidence (HM-DEC-031).
/// </summary>
public sealed class BandActivityTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<CwBand> Bands => BandPlan.Bands;

    private static CwBand Band(string name) => Bands.First(b => b.Name == name);

    private static ActivitySpot Spot(string bandName, int ageMinutes = 1, string mode = "CW")
    {
        var band = Band(bandName);
        return new ActivitySpot(
            "someone is calling",
            band.LowHz + 5_000,
            mode,
            "test",
            Now.AddMinutes(-ageMinutes),
            18);
    }

    private static IReadOnlyList<ActivitySpot> Spots(string bandName, int count, string mode = "CW")
        => Enumerable.Range(0, count).Select(_ => Spot(bandName, mode: mode)).ToList();

    private static SourceStatus Ok(string name, string? scopedTo = null)
        => new(name, SourceState.Ok, 1, Now, null) { ScopedToBand = scopedTo };

    private static SourceStatus Down(string name, string? scopedTo = null)
        => new(name, SourceState.Degraded, 0, Now.AddMinutes(-20), "no answer")
        { ScopedToBand = scopedTo };

    private static SourceStatus Off(string name, string? scopedTo = null)
        => new(name, SourceState.Disabled, 0, null, null) { ScopedToBand = scopedTo };

    private static BandActivityReading For(
        string bandName,
        IReadOnlyList<ActivitySpot> spots,
        IReadOnlyList<SourceStatus> statuses)
        => BandActivity.Summarize(Bands, spots, statuses, Now).Single(r => r.BandName == bandName);

    /// <remarks>
    /// Proves the scale is relative: the busiest band fills the indicator and
    /// the others are drawn against it. An absolute scale would be a number
    /// nobody can calibrate — "34 spots" means nothing without knowing whether
    /// 34 is a lot tonight.
    /// </remarks>
    [Fact]
    public void Scale_IsRelativeToTheBusiestBandRightNow()
    {
        var spots = Spots("40 m", 40).Concat(Spots("20 m", 10)).Concat(Spots("15 m", 2)).ToList();
        var statuses = new[] { Ok("POTA") };

        var readings = BandActivity.Summarize(Bands, spots, statuses, Now);

        var forty = readings.Single(r => r.BandName == "40 m");
        var twenty = readings.Single(r => r.BandName == "20 m");
        var fifteen = readings.Single(r => r.BandName == "15 m");

        Assert.Equal(BandActivity.MaxPips, forty.Pips);
        Assert.True(twenty.Pips < forty.Pips, $"20 m ({twenty.Pips}) under 40 m ({forty.Pips})");
        Assert.True(fifteen.Pips < twenty.Pips);
        Assert.True(fifteen.Pips >= 1, "a band with anything on it keeps a pip");
    }

    /// <remarks>
    /// Proves the scale spreads bands across the indicator instead of piling
    /// them into the bottom bucket. Found by the test above: band activity is
    /// heavily tailed, and a linear scale across four pips gave a band with a
    /// quarter of the traffic the same single pip as one with a twentieth.
    /// </remarks>
    [Fact]
    public void Scale_SeparatesBandsRatherThanPilingThemAtTheBottom()
    {
        Assert.Equal(BandActivity.MaxPips, BandActivity.PipsFor(40, 40));
        Assert.Equal(2, BandActivity.PipsFor(10, 40));
        Assert.Equal(1, BandActivity.PipsFor(2, 40));
        Assert.Equal(0, BandActivity.PipsFor(0, 40));

        // Monotonic: more traffic never draws fewer pips.
        var previous = 0;
        for (var n = 0; n <= 100; n++)
        {
            var pips = BandActivity.PipsFor(n, 100);
            Assert.True(pips >= previous, $"{n} spots drew fewer pips than {n - 1}");
            previous = pips;
        }
    }

    /// <remarks>
    /// Proves the same counts scale differently once the busiest band changes.
    /// The indicator answers "which of these is worth a look right now", so it
    /// has to move when the answer moves.
    /// </remarks>
    [Fact]
    public void Scale_MovesWithTheBusiestBand()
    {
        var statuses = new[] { Ok("POTA") };

        var alone = For("20 m", Spots("20 m", 10), statuses);
        var alongside = For("20 m", Spots("20 m", 10).Concat(Spots("40 m", 60)).ToList(), statuses);

        Assert.Equal(BandActivity.MaxPips, alone.Pips);
        Assert.True(alongside.Pips < alone.Pips);
    }

    /// <remarks>
    /// Proves the distinction the whole feature turns on: a band nobody is
    /// watching and a band being watched in silence are different claims, and
    /// they differ in state, in wording, and in what the indicator draws.
    /// </remarks>
    [Fact]
    public void NoData_And_NothingHeard_AreDifferentClaims()
    {
        var watched = For("40 m", Array.Empty<ActivitySpot>(), new[] { Ok("POTA") });
        var unwatched = For("40 m", Array.Empty<ActivitySpot>(), new[] { Off("POTA") });

        Assert.Equal(BandActivityState.NothingHeard, watched.State);
        Assert.Equal(BandActivityState.NoData, unwatched.State);

        Assert.False(watched.IsUnknown);
        Assert.True(unwatched.IsUnknown);

        Assert.NotEqual(watched.Claim, unwatched.Claim);
        Assert.Contains("nothing heard", watched.Claim, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no data", unwatched.Claim, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves every source being switched off reads as "no enabled source",
    /// which is the operator's own doing and fixed in Settings.
    /// </remarks>
    [Fact]
    public void AllSourcesOff_SaysNoEnabledSource()
    {
        var reading = For("40 m", Array.Empty<ActivitySpot>(), new[] { Off("POTA"), Off("RBN") });

        Assert.Equal(BandActivityState.NoData, reading.State);
        Assert.Contains("No enabled source", reading.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the hedge is earned. With every covering source healthy and
    /// reporting zero, the tooltip may say "likely closed rather than
    /// unwatched" — hedged, and naming the possibility it cannot rule out.
    /// </remarks>
    [Fact]
    public void NothingHeard_WithHealthySources_HedgesExplicitly()
    {
        var reading = For(
            "40 m", Array.Empty<ActivitySpot>(), new[] { Ok("POTA"), Ok("RBN", "40 m") });

        Assert.Contains("POTA and RBN are both answering", reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("likely closed rather than unwatched", reading.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the hedge is withdrawn the moment a source goes down. A gap in
    /// the watch is not evidence about the band, so the sentence that draws a
    /// conclusion from silence must not survive it (HM-DEC-022).
    /// </remarks>
    [Fact]
    public void NothingHeard_WithASourceDown_DropsTheHedge()
    {
        var reading = For("40 m", Array.Empty<ActivitySpot>(), new[] { Ok("POTA"), Down("RBN", "40 m") });

        Assert.DoesNotContain("likely closed", reading.Evidence, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RBN", reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("isn't answering", reading.Evidence, StringComparison.Ordinal);
        Assert.Equal(ConditionsConfidence.Thin, reading.Confidence);
    }

    /// <remarks>
    /// Proves the crux of the honesty problem. RBN is filtered to the band on
    /// screen, so it has nothing to say about any other band. Crediting its
    /// silence about 17 m as an observation would manufacture confidence from
    /// a source that was never pointed there.
    /// </remarks>
    [Fact]
    public void BandScopedSource_DoesNotVouchForBandsItCannotSee()
    {
        var statuses = new[] { Ok("POTA"), Ok("RBN", scopedTo: "40 m") };

        var onScreen = For("40 m", Array.Empty<ActivitySpot>(), statuses);
        var elsewhere = For("17 m", Array.Empty<ActivitySpot>(), statuses);

        // RBN counts on the band it is watching...
        Assert.Contains("RBN", onScreen.Evidence, StringComparison.Ordinal);

        // ...and is silent about the ones it is not.
        Assert.DoesNotContain("RBN", elsewhere.Evidence, StringComparison.Ordinal);
        Assert.Contains("POTA", elsewhere.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves that when the only source covering a band is scoped elsewhere,
    /// the band reads as no data rather than as an empty band.
    /// </remarks>
    [Fact]
    public void BandWithOnlyAScopedSourceElsewhere_HasNoData()
    {
        var reading = For(
            "17 m", Array.Empty<ActivitySpot>(), new[] { Ok("RBN", scopedTo: "40 m") });

        Assert.Equal(BandActivityState.NoData, reading.State);
        Assert.Contains("No enabled source", reading.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a busy band is called busy with its counts and its sources, in
    /// the voice HM-DEC-025 established.
    /// </remarks>
    [Fact]
    public void Busy_BandCarriesItsCountsAndSources()
    {
        var spots = Spots("40 m", 34, "CW").Concat(Spots("40 m", 12, "SSB")).ToList();
        var reading = For("40 m", spots, new[] { Ok("POTA"), Ok("RBN", "40 m") });

        Assert.Equal(BandActivityState.Heard, reading.State);
        Assert.Equal("busy.", reading.Claim);
        Assert.Contains(
            $"46 signals in the last {(int)BandActivity.Window.TotalMinutes} minutes",
            reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("34 of them CW", reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("From POTA and RBN.", reading.Evidence, StringComparison.Ordinal);
        Assert.Equal(ConditionsConfidence.Sound, reading.Confidence);
    }

    /// <remarks>
    /// Proves a thin sample softens its own wording rather than being reported
    /// as a finding. Two signals is evidence that two people were heard.
    /// </remarks>
    [Fact]
    public void ThinSample_SaysItIsTooLittleToBeSure()
    {
        var reading = For("17 m", Spots("17 m", 2), new[] { Ok("RBN", scopedTo: "17 m") });

        Assert.Equal("quiet.", reading.Claim);
        Assert.Contains(
            $"2 signals in the last {(int)BandActivity.Window.TotalMinutes} minutes",
            reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("too little to be sure", reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("From RBN.", reading.Evidence, StringComparison.Ordinal);
        Assert.Equal(ConditionsConfidence.Thin, reading.Confidence);
    }

    /// <remarks>
    /// Proves a degraded source is named in the tooltip rather than quietly
    /// reducing the numbers it feeds (HM-DEC-022).
    /// </remarks>
    [Fact]
    public void DegradedSource_IsNamedInTheTooltip()
    {
        var reading = For("40 m", Spots("40 m", 30), new[] { Ok("RBN", "40 m"), Down("POTA") });

        Assert.Contains("POTA", reading.Evidence, StringComparison.Ordinal);
        Assert.Contains("isn't answering", reading.Evidence, StringComparison.Ordinal);
        Assert.Equal(ConditionsConfidence.Thin, reading.Confidence);
    }

    /// <remarks>
    /// Proves the window is real: older spots are not counted as current
    /// activity, so the button describes now rather than the last hour.
    /// </remarks>
    [Fact]
    public void OnlyCountsWhatIsInsideTheWindow()
    {
        // Comfortably outside whatever the window currently is (HM-DEC-045).
        var stale = (int)BandActivity.Window.TotalMinutes + 30;
        var old = Enumerable.Range(0, 30).Select(_ => Spot("40 m", ageMinutes: stale)).ToList();
        var reading = For("40 m", old, new[] { Ok("POTA") });

        Assert.Equal(0, reading.SpotCount);
        Assert.Equal(BandActivityState.NothingHeard, reading.State);
    }

    /// <remarks>
    /// THE CONSTRAINT THIS FEATURE LIVES UNDER. Spot counts are a proxy for
    /// activity, never for propagation: RBN counts say where skimmers are,
    /// POTA says where activators went. No tooltip may assert what the
    /// ionosphere is doing. The one permitted sentence is hedged in its own
    /// words and only reachable with every covering source healthy.
    /// </remarks>
    [Fact]
    public void NoTooltip_AssertsPropagation()
    {
        var spotSets = new[]
        {
            Array.Empty<ActivitySpot>(),
            Spots("40 m", 1),
            Spots("40 m", 3),
            Spots("40 m", 40).Concat(Spots("20 m", 8)).ToList(),
        };

        var statusSets = new[]
        {
            new[] { Ok("POTA"), Ok("RBN", "40 m") },
            new[] { Ok("POTA"), Down("RBN", "40 m") },
            new[] { Down("POTA"), Down("RBN", "40 m") },
            new[] { Off("POTA"), Off("RBN", "40 m") },
        };

        var banned = new[]
        {
            "is closed", "is open", "band is dead", "propagation",
            "the ionosphere", "will not hear", "cannot hear", "you can work",
        };

        foreach (var spots in spotSets)
        {
            foreach (var statuses in statusSets)
            {
                foreach (var reading in BandActivity.Summarize(Bands, spots, statuses, Now))
                {
                    foreach (var phrase in banned)
                    {
                        Assert.DoesNotContain(
                            phrase, reading.Tooltip, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }
    }

    /// <remarks>
    /// Proves the tooltip reads as one sentence, named band first, so a row of
    /// them can be compared at a glance.
    /// </remarks>
    [Fact]
    public void Tooltip_LeadsWithTheBandAndThenTheClaim()
    {
        var reading = For("40 m", Spots("40 m", 30), new[] { Ok("POTA") });

        Assert.StartsWith("40 m · busy.", reading.Tooltip, StringComparison.Ordinal);
        Assert.Contains(reading.Evidence, reading.Tooltip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves determinism (§5): no clock is read, so the same inputs give the
    /// same readings every call.
    /// </remarks>
    [Fact]
    public void Summarize_IsDeterministic()
    {
        var spots = Spots("40 m", 12).Concat(Spots("20 m", 3)).ToList();
        var statuses = new[] { Ok("POTA"), Ok("RBN", "40 m") };

        var a = BandActivity.Summarize(Bands, spots, statuses, Now);
        var b = BandActivity.Summarize(Bands, spots, statuses, Now);

        Assert.Equal(a, b);
    }

    /// <remarks>
    /// Proves every band on display gets a reading, so no button is left
    /// silently blank.
    /// </remarks>
    [Fact]
    public void EveryBand_GetsAReading()
    {
        var readings = BandActivity.Summarize(
            Bands, Spots("40 m", 5), new[] { Ok("POTA") }, Now);

        Assert.Equal(Bands.Count, readings.Count);
        Assert.All(readings, r => Assert.False(string.IsNullOrWhiteSpace(r.Tooltip)));
        Assert.Equal(Bands.Select(b => b.Name), readings.Select(r => r.BandName));
    }
}
