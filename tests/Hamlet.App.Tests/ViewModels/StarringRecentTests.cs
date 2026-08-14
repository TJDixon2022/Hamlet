using System.Collections.ObjectModel;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Turning a place the operator has been into a favorite, from the manage
/// window (HM-DEC-072).
/// </summary>
/// <remarks>
/// This is how most favorites will actually be born: somebody was somewhere
/// good, did not think to save it, and realizes the following evening that they
/// want it. So the favorite it produces has to be indistinguishable from one
/// made at the star, and the app has to own the save rather than the window
/// building a favorite of its own.
/// </remarks>
public sealed class StarringRecentTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private static RecentStation Visit(long hz, string station = "")
        => RecentStations.From(hz, station, "CW", null, Now);

    /// <remarks>
    /// Proves HM-DEC-072: the window hands the entry back rather than saving it
    /// itself, so there is one save path and a favorite made here carries what a
    /// direct save carries.
    /// </remarks>
    [Fact]
    public void StarringFromTheWindowSavesThroughTheApp()
    {
        var favorites = new ObservableCollection<Favorite>();
        var starred = new List<RecentStation>();

        var window = new FavoritesViewModel(
            favorites,
            () => { },
            new[] { Visit(7_030_000, "W1AW"), Visit(14_074_000) },
            entry =>
            {
                starred.Add(entry);
                favorites.Add(RecentStations.ToFavorite(entry, null, Now));
            });

        Assert.True(window.HasRecent);
        Assert.Equal(2, window.Recent.Count);

        window.StarCommand.Execute(window.Recent[0]);

        Assert.Single(starred);
        Assert.Equal(7_030_000, starred[0].FrequencyHz);
        Assert.Equal(
            Favorites.From(7_030_000, "CW", null, Now), favorites[0]);
    }

    /// <remarks>
    /// Proves HM-DEC-072: a place already saved shows as saved and cannot be
    /// starred twice. Two favorites on one frequency would make the star under
    /// the display unpredictable about which one it was un-saving.
    /// </remarks>
    [Fact]
    public void APlaceAlreadySavedIsShownAsSavedAndCannotBeStarredAgain()
    {
        var favorites = new ObservableCollection<Favorite>
        {
            Favorites.From(7_030_000, "CW", null, Now),
        };

        var calls = 0;

        var window = new FavoritesViewModel(
            favorites, () => { }, new[] { Visit(7_030_000) }, _ => calls++);

        Assert.True(window.Recent[0].IsSaved);
        Assert.Equal("★ saved", window.Recent[0].StarLabel);

        window.StarCommand.Execute(window.Recent[0]);

        Assert.Equal(0, calls);
        Assert.Single(favorites);
    }

    /// <remarks>
    /// Proves HM-DEC-072: the row flips to saved once it has been, so the button
    /// does not sit there inviting a second press that does nothing.
    /// </remarks>
    [Fact]
    public void TheRowSaysSavedAfterItHasBeen()
    {
        var favorites = new ObservableCollection<Favorite>();

        var window = new FavoritesViewModel(
            favorites,
            () => { },
            new[] { Visit(7_030_000) },
            entry => favorites.Add(RecentStations.ToFavorite(entry, null, Now)));

        Assert.Equal("☆ save", window.Recent[0].StarLabel);

        window.StarCommand.Execute(window.Recent[0]);

        Assert.True(window.Recent[0].IsSaved);
        Assert.Equal("★ saved", window.Recent[0].StarLabel);
    }

    /// <remarks>
    /// Proves HM-DEC-072: a row for a place with no identified station reads as
    /// a place in the window too. The honesty rule holds on every surface that
    /// shows these, not only on the dropdown.
    /// </remarks>
    [Fact]
    public void ARowWithNoStationReadsAsAPlace()
    {
        var window = new FavoritesViewModel(
            new ObservableCollection<Favorite>(), () => { },
            new[] { Visit(7_030_000) }, _ => { });

        Assert.False(window.Recent[0].Entry.IsIdentified);
        Assert.Equal("7.030 on 40 m", window.Recent[0].Label);
    }

    /// <remarks>
    /// Proves HM-DEC-072: with nowhere to show, the section is absent rather
    /// than an empty box, which is the same rule the dropdowns follow.
    /// </remarks>
    [Fact]
    public void WithNowhereToShowTheSectionIsAbsent()
    {
        var window = new FavoritesViewModel(
            new ObservableCollection<Favorite>(), () => { });

        Assert.False(window.HasRecent);
        Assert.Empty(window.Recent);
    }
}
