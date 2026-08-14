using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Favorites, which carry the reason (HM-DEC-060).
/// </summary>
/// <remarks>
/// The radio's own memory channels are numbered slots whose meaning you have to
/// remember, which is the problem rather than the answer. Hamlet knows why
/// somebody was on a frequency, because the map already says what lives there.
/// </remarks>
public sealed class FavoriteTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private static Neighborhood At(long hz)
        => NeighborhoodPlan
            .WithEdges(BandPlan.Bands.Single(b => b.Name == "20 m"))
            .Single(n => n.Contains(hz));

    /// <remarks>
    /// SAVING CAPTURES CONTEXT AUTOMATICALLY (HM-DEC-060). Frequency, mode, band
    /// and neighborhood, so a favorite reads for itself with nothing typed. The
    /// operator may rename it and nobody has to.
    /// </remarks>
    [Fact]
    public void SavingCapturesWhereYouWereAndWhatLivesThere()
    {
        var favorite = Favorites.From(14_074_000, "USB", At(14_074_500), Now);

        Assert.Equal(14_074_000, favorite.FrequencyHz);
        Assert.Equal("USB", favorite.Mode);
        Assert.Equal("20 m", favorite.BandName);
        Assert.Contains("FT8", favorite.Neighborhood, StringComparison.Ordinal);
        Assert.Equal(Now, favorite.SavedUtc);

        // And it names itself from the two facts that answer "what was this
        // for": the frequency, and what is there.
        Assert.Contains("14.074", favorite.Name, StringComparison.Ordinal);
        Assert.Contains("FT8", favorite.Name, StringComparison.Ordinal);
    }

    /// <remarks>
    /// NOTHING TYPED AND NOTHING INVENTED (§0.0). Where the map has published no
    /// convention for a stretch, the favorite is the frequency and its band and
    /// says no more, rather than making something up about open ground.
    /// </remarks>
    [Fact]
    public void AStretchNobodyNamedGetsAFrequencyAndABandAndNoMore()
    {
        var open = NeighborhoodPlan
            .WithEdges(BandPlan.Bands.Single(b => b.Name == "20 m"))
            .First(n => n.Family == ModeFamily.Open);

        var named = Favorites.NameFor(open.JumpHz, open);

        Assert.Contains("Open ground", named, StringComparison.OrdinalIgnoreCase);

        // And with no neighborhood at all, just the number and the band.
        var bare = Favorites.NameFor(14_030_000, null);

        Assert.Equal("14.030 on 20 m", bare);
    }

    /// <remarks>
    /// THE STAR IS A TOGGLE AND IT NAMES WHERE YOU ARE. On a saved frequency it
    /// reads the favorite's name; anywhere else it reads the invitation to save.
    /// </remarks>
    [Fact]
    public void TheStarNamesTheFavoriteHereOrInvitesYouToSaveOne()
    {
        var saved = Favorites.From(7_030_000, "CW", null, Now);
        var list = new[] { saved };

        Assert.Equal(saved.Name, Favorites.StarLabel(Favorites.At(list, 7_030_000)));
        Assert.Equal("save this spot", Favorites.StarLabel(Favorites.At(list, 7_040_000)));
    }

    /// <remarks>
    /// EXACT RATHER THAN NEAR. A star that lit up a hundred hertz away would
    /// make un-saving unpredictable, and the operator would learn not to trust
    /// it.
    /// </remarks>
    [Fact]
    public void TheStarLightsOnTheExactFrequencyAndNotNearby()
    {
        var list = new[] { Favorites.From(7_030_000, "CW", null, Now) };

        Assert.NotNull(Favorites.At(list, 7_030_000));
        Assert.Null(Favorites.At(list, 7_030_100));
        Assert.Null(Favorites.At(list, 7_029_900));
    }

    /// <remarks>
    /// Proves the frequency reads the way the app writes it everywhere else, so
    /// a favorite and a card and a status line all say "14.074" rather than
    /// three renderings of one number.
    /// </remarks>
    [Fact]
    public void TheFrequencyReadsTheWayTheAppWritesItEverywhereElse()
    {
        var favorite = Favorites.From(14_074_000, "USB", null, Now);

        Assert.Equal("14.074", favorite.FrequencyLabel);
        Assert.Equal("7.030", Favorites.From(7_030_000, "CW", null, Now).FrequencyLabel);
    }
}
