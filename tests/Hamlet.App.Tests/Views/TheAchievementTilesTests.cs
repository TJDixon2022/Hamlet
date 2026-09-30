using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **The rank trail and the eight tiles** (work instruction 506 task 2, HM-DEC-211).
/// </summary>
/// <remarks>
/// **EVERYTHING HERE IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** Every number asserted comes off the
/// scores the page was built from, never a figure typed here.
/// </remarks>
public sealed class TheAchievementTilesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheAchievementTilesTests(ITestOutputHelper output) => _output = output;

    private static List<Button> Tiles(Window window)
        => TheAchievementsStandingTests.Named<ItemsControl>(window, "AchievementsBadges").GetVisualDescendants().OfType<Button>()
            .Where(b => b.IsEffectivelyVisible && b.Classes.Contains("achievement-tile"))
            .ToList();

    /// <remarks>
    /// Eight tiles, each the button that opens its kind; the level chip is the file's level in words; the bar
    /// is the count over the next level's count; a kind with nothing earned is locked with what opens it, and
    /// only such a kind.
    /// </remarks>
    [AvaloniaFact]
    public void EightTilesEachOpeningItsKind()
    {
        foreach (var (records, label) in new (IReadOnlyList<AdifLogRecord>, string)[]
        {
            (TheAchievementsPageTests.TwelveContacts(), "twelve contacts"),
            (Array.Empty<AdifLogRecord>(), "empty log"),
        })
        {
            var window = TheAchievementsStandingTests.Realized(records, AchievementPoints.Parse(AchievementPoints.Shipped()), 1280);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                var tiles = Tiles(window);

                Assert.Equal(8, tiles.Count);

                foreach (var tile in tiles)
                {
                    var badge = (AchievementBadge)tile.DataContext!;
                    var said = TheAchievementsStandingTests.VisibleText(tile).ToList();
                    var bars = tile.GetVisualDescendants().OfType<BadgeProgressControl>().Where(b => b.IsEffectivelyVisible).ToList();

                    _output.WriteLine(label + " " + badge.Kind.PadRight(12) + (badge.IsLocked ? "LOCKED " : "") + string.Join(" | ", said)
                        + (bars.Count == 1 ? " | bar " + bars[0].Fraction.ToString("0.000", CultureInfo.InvariantCulture) : ""));

                    Assert.Same(screen.OpenCategoryCommand, tile.Command);
                    Assert.Equal(badge.Kind, tile.CommandParameter);
                    Assert.Contains(badge.Name, said);

                    if (badge.Score.Worked == 0)
                    {
                        Assert.True(badge.IsLocked);
                        Assert.Contains("Locked", said);
                        Assert.Contains("To open it:", said);
                        Assert.Contains(badge.ToOpenLine, said);
                        Assert.Empty(bars);
                        continue;
                    }

                    Assert.False(badge.IsLocked);
                    Assert.DoesNotContain("Locked", said);
                    Assert.Contains(badge.CountText, said);
                    Assert.Contains(badge.CountWords, said);

                    // **THE LEVEL IS THE FILE'S, IN WORDS, OR NO CHIP BELOW THE FIRST.**
                    if (badge.Score.Level >= 1)
                    {
                        Assert.Contains(badge.Score.LevelName, said);
                    }
                    else
                    {
                        Assert.DoesNotContain(said, s => s is "Bronze" or "Silver" or "Gold" or "Platinum");
                    }

                    // **THE BAR RUNS TO THE NEXT LEVEL OF HIS FILE.**
                    if (badge.Score.NextLevelAt is { } next)
                    {
                        Assert.Single(bars);
                        Assert.Equal(Math.Min(1.0, (double)badge.Score.Worked / next), bars[0].Fraction, 6);
                        Assert.Contains(badge.BarGapLine, said);
                    }

                    if (badge.HasNextCard)
                    {
                        Assert.Contains(badge.NextIsDoor ? "Opens a set:" : "Next:", said);
                        Assert.Contains(badge.NextCard, said);
                    }
                }

                // **EACH TILE OPENS ITS KIND.**
                foreach (var kind in AchievementKinds.All)
                {
                    var tile = Tiles(window).Single(t => (string?)t.CommandParameter == kind);
                    var at = tile.TranslatePoint(new Point(tile.Bounds.Width / 2, tile.Bounds.Height / 2), window)!.Value;

                    window.MouseMove(at);
                    window.MouseDown(at, MouseButton.Left);
                    window.MouseUp(at, MouseButton.Left);
                    TheAchievementsStandingTests.Settle(window);

                    Assert.Equal(kind, screen.Category?.Kind);

                    screen.BackCommand.Execute(null);
                    TheAchievementsStandingTests.Settle(window);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <remarks>
    /// The trail draws exactly one locked rank and nothing after it, with where it opens; more than four
    /// passed ranks draws the last three and says how many came before.
    /// </remarks>
    [AvaloniaFact]
    public void TheTrailDrawsOneLockedRank()
    {
        var twelve = TheAchievementsPageTests.TwelveContacts();
        var total = new AchievementScores(new AchievementLog(twelve, "FN00"), AchievementPoints.Parse(AchievementPoints.Shipped())).Total!.Value;

        foreach (var (points, label) in new[]
        {
            (AchievementPoints.Parse(AchievementPoints.Shipped()), "the shipped file"),
            (TheAchievementsStandingTests.WithRanks(new long[] { 1, 2, 3, 4, 5, 6, total + 10, total + 20 }), "six ranks passed"),
        })
        {
            var window = TheAchievementsStandingTests.Realized(twelve, points, 1280);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                var trail = TheAchievementsStandingTests.Named<Control>(window, "AchievementsRankTrail");
                var said = TheAchievementsStandingTests.VisibleText(trail).ToList();
                var locked = trail.GetVisualDescendants().OfType<Control>().Count(c => c.IsEffectivelyVisible && c.Classes.Contains("rank-locked"));
                var scores = screen.Page!.Scores;

                _output.WriteLine(label + ": rank " + scores.Rank + "; trail " + string.Join(" | ", said) + "; locked steps " + locked);

                Assert.Equal(1, locked);
                Assert.Contains("you are here", said);
                Assert.Contains("opens at " + scores.NextRankAt!.Value.ToString("#,0", CultureInfo.InvariantCulture) + " points", said);
                Assert.Contains(scores.NextRankName, said);
                Assert.DoesNotContain(scores.Points.RankName(scores.Rank + 2), said);

                if (scores.Rank - 1 > AchievementRankTrail.PassedMost)
                {
                    Assert.Contains(said, s => s.EndsWith("ranks before", StringComparison.Ordinal));
                    Assert.DoesNotContain(scores.Points.RankName(1), said);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <remarks>No string on the opening page is a denominator of the world, and nothing on it clips at 1280, 1400 and 1920 wide.</remarks>
    [AvaloniaFact]
    public void NoDenominatorAndNothingClips()
    {
        foreach (var width in new[] { 1280.0, 1400.0, 1920.0 })
        {
            foreach (var records in new IReadOnlyList<AdifLogRecord>[] { TheAchievementsPageTests.TwelveContacts(), Array.Empty<AdifLogRecord>() })
            {
                var window = TheAchievementsStandingTests.Realized(records, AchievementPoints.Parse(AchievementPoints.Shipped()), width);

                try
                {
                    var page = TheAchievementsStandingTests.Named<Control>(window, "AchievementsPage");
                    var said = TheAchievementsStandingTests.VisibleText(page).ToList();
                    var clips = Clips(window);

                    _output.WriteLine(width.ToString("0", CultureInfo.InvariantCulture) + " x " + window.Bounds.Height.ToString("0", CultureInfo.InvariantCulture) + ", " + records.Count + " records: "
                        + said.Count + " runs, " + (clips.Count == 0 ? "nothing clips" : string.Join("; ", clips)));

                    Assert.DoesNotContain(said, s => Regex.IsMatch(s, @"\d\s+of\s+\d"));
                    Assert.DoesNotContain(said, s => s.Contains("of 340", StringComparison.Ordinal));
                    Assert.Empty(clips);
                }
                finally
                {
                    window.Close();
                }
            }
        }
    }

    /// <summary>
    /// **A sentence on an unearned card that wraps between words and breaks none** (work instruction 506 task
    /// 4): the next stamp stands in a column the picture draws its sentences over two lines in, so there a run
    /// may wrap, but only where it is on a card not yet earned, and every word it holds fits the run's width
    /// on its own. Anything else that wraps is still a fault.
    /// </summary>
    internal static bool WrapsBetweenWordsInAStamp(TextBlock text)
    {
        if (text.TextWrapping == Avalonia.Media.TextWrapping.NoWrap
            || !text.GetVisualAncestors().OfType<Border>().Any(b => b.Classes.Contains("category-card") && b.DataContext is AchievementCategoryCard { Earned: false }))
        {
            return false;
        }

        var typeface = new Avalonia.Media.Typeface(text.FontFamily, text.FontStyle, text.FontWeight);

        return (text.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(word => new Avalonia.Media.TextFormatting.TextLayout(word, typeface, text.FontSize, null).Width <= text.Bounds.Width + 0.5);
    }

    /// <summary>Every visible run that would clip or wrap, in the page-wide test's words.</summary>
    internal static List<string> Clips(Window window)
    {
        var clips = new List<string>();

        foreach (var text in window.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0))
        {
            if (text.TextWrapping != Avalonia.Media.TextWrapping.NoWrap)
            {
                continue;
            }

            var needs = new Avalonia.Media.TextFormatting.TextLayout(
                text.Text ?? "", new Avalonia.Media.Typeface(text.FontFamily, text.FontStyle, text.FontWeight), text.FontSize, null).Width;

            // A run inside a Viewbox is scaled to its box, so it is measured against the Viewbox's own size.
            if (text.GetVisualAncestors().OfType<Viewbox>().Any())
            {
                continue;
            }

            if (needs > text.Bounds.Width + 0.5)
            {
                clips.Add("[" + text.Text + "] needs " + needs.ToString("0.0", CultureInfo.InvariantCulture) + " px and its slot is " + text.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture));
                continue;
            }

            foreach (var box in text.GetVisualAncestors().OfType<Control>())
            {
                var left = text.TranslatePoint(new Point(0, 0), box)?.X ?? 0;

                if (left < -0.5 || left + needs > box.Bounds.Width + 0.5)
                {
                    clips.Add("[" + text.Text + "] runs from " + left.ToString("0.0", CultureInfo.InvariantCulture) + " to " + (left + needs).ToString("0.0", CultureInfo.InvariantCulture)
                        + " inside a " + box.GetType().Name + " " + box.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture) + " wide");
                    break;
                }
            }
        }

        return clips;
    }
}
