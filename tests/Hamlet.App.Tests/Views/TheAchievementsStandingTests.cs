using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
/// **Your standing: the ring, the gap, three facts and what was just unlocked** (work instruction 506
/// task 1, HM-DEC-211).
/// </summary>
/// <remarks>
/// **EVERYTHING HERE IS COMPUTED ON THE HEADLESS HOST, NOT SEEN.** The fraction is asserted against the
/// file's own arithmetic, never against a number typed here.
/// </remarks>
public sealed class TheAchievementsStandingTests
{
    private const string MyGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheAchievementsStandingTests(ITestOutputHelper output) => _output = output;

    /// <summary>The shipped file with its `ranks` and `rank_names` replaced.</summary>
    internal static AchievementPoints WithRanks(IEnumerable<long> ranks, IEnumerable<string>? names = null)
    {
        var json = AchievementPoints.Shipped();

        json = Regex.Replace(json, @"""ranks""\s*:\s*\[[^\]]*\]", @"""ranks"": [" + string.Join(", ", ranks.Select(r => r.ToString(CultureInfo.InvariantCulture))) + "]");
        json = Regex.Replace(json, @"""rank_names""\s*:\s*\[[^\]]*\]", @"""rank_names"": [" + string.Join(", ", (names ?? Array.Empty<string>()).Select(n => "\"" + n + "\"")) + "]");

        return AchievementPoints.Parse(json);
    }

    private static double Expected(AchievementScores scores)
        => scores.NextRankAt is { } next && scores.RankStartsAt is { } start
            ? (double)(scores.Total!.Value - start) / (next - start)
            : 1;

    /// <remarks>
    /// The ring's fraction is the file's arithmetic on three logs - empty, the twelve contacts, and the twelve
    /// contacts against ranks set one point past their total - drawn by the window's ring; the rank is the
    /// number where the file names none.
    /// </remarks>
    [AvaloniaFact]
    public void TheRingIsTheFilesArithmetic()
    {
        var twelve = TheAchievementsPageTests.TwelveContacts();
        var total = new AchievementScores(new AchievementLog(twelve, MyGrid), AchievementPoints.Parse(AchievementPoints.Shipped())).Total!.Value;

        foreach (var (records, points, label) in new (IReadOnlyList<AdifLogRecord>, AchievementPoints, string)[]
        {
            (Array.Empty<AdifLogRecord>(), AchievementPoints.Parse(AchievementPoints.Shipped()), "empty log"),
            (twelve, AchievementPoints.Parse(AchievementPoints.Shipped()), "twelve contacts"),
            (twelve, WithRanks(new long[] { total - 10, total + 1, total + 100 }), "a point short of a rank"),
        })
        {
            var window = Realized(records, points, 1280);
            var screen = (AchievementsViewModel)window.DataContext!;

            try
            {
                var scores = screen.Page!.Scores;
                var ring = Named<StandingRingControl>(window, "AchievementsStandingRing");
                var said = VisibleText(Named<Control>(window, "AchievementsStanding")).ToList();

                _output.WriteLine(label + ": total " + scores.Total + ", rank " + scores.Rank + " began at " + scores.RankStartsAt + ", next at " + scores.NextRankAt
                    + "; ring " + ring.Fraction.ToString("0.000", CultureInfo.InvariantCulture) + "; says " + string.Join(" | ", said));

                Assert.True(ring.IsEffectivelyVisible, label + ": no ring drawn");
                Assert.Equal(Expected(scores), ring.Fraction, 6);
                Assert.Contains("Rank", said);
                Assert.Contains(scores.Rank.ToString(CultureInfo.InvariantCulture), said);
                Assert.Contains(AchievementStanding.Points(scores.Total!.Value), said);

                if (scores.ToNextRank is { } gap)
                {
                    Assert.Contains(AchievementStanding.Points(gap) + " to " + scores.NextRankName + ".", said);
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <remarks>Where the file names the rank, the name is drawn and not `Rank` and the number.</remarks>
    [AvaloniaFact]
    public void TheRankNameFollowsTheFile()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), WithRanks(new long[] { 25, 100, 250, 500, 1000 }, new[] { "Listener", "Novice", "Operator", "Regular", "Elmer", "Legend" }), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            var said = VisibleText(Named<Control>(window, "AchievementsStanding")).ToList();
            var name = screen.Page!.Scores.RankName;

            _output.WriteLine("named: " + string.Join(" | ", said));

            Assert.NotEqual("Rank " + screen.Page.Scores.Rank, name);
            Assert.Contains(name, said);
            Assert.DoesNotContain("Rank", said);
        }
        finally
        {
            window.Close();
        }
    }

    /// <remarks>An unreadable file draws no ring and no number, says the problem, and the rest of the page still draws.</remarks>
    [AvaloniaFact]
    public void AnUnreadableFileDrawsNoNumber()
    {
        var window = Realized(TheAchievementsPageTests.TwelveContacts(), AchievementPoints.Absent("the points file could not be read"), 1280);

        try
        {
            var standing = Named<Control>(window, "AchievementsStanding");
            var said = VisibleText(standing).ToList();

            _output.WriteLine("unreadable: " + string.Join(" | ", said));

            Assert.False(Named<StandingRingControl>(window, "AchievementsStandingRing").IsEffectivelyVisible);
            Assert.Contains("the points file could not be read", said);
            Assert.DoesNotContain(said, s => s.EndsWith(" points", StringComparison.Ordinal) || s.Contains("points to", StringComparison.Ordinal));
            Assert.True(Named<ItemsControl>(window, "AchievementsBadges").IsEffectivelyVisible, "the rest of the page did not draw");
        }
        finally
        {
            window.Close();
        }
    }

    /// <remarks>
    /// Just unlocked is the most recent earned card by its contact's date. On the twelve contacts that is a
    /// record with no grid, so it is a plain panel and not a button; with that record left out, it is the
    /// button, and pressing it opens the popup with that path.
    /// </remarks>
    [AvaloniaFact]
    public void JustUnlockedOpensItsPath()
    {
        var twelve = TheAchievementsPageTests.TwelveContacts();
        var plain = Realized(twelve, AchievementPoints.Parse(AchievementPoints.Shipped()), 1280);

        try
        {
            var latest = ((AchievementsViewModel)plain.DataContext!).Standing!.JustUnlocked!;

            _output.WriteLine("twelve contacts, just unlocked " + latest.Title + " " + latest.Callsign + ", map " + latest.HasMap);

            Assert.False(latest.HasMap);
            Assert.False(Named<Button>(plain, "AchievementsJustUnlocked").IsEffectivelyVisible);
            Assert.Contains(latest.Title, VisibleText(Named<Border>(plain, "AchievementsJustUnlockedNoMap")));
        }
        finally
        {
            plain.Close();
        }

        var window = Realized(twelve.Where(r => r.Contact.Call != latestNoGridCall(twelve)).ToList(), AchievementPoints.Parse(AchievementPoints.Shipped()), 1280);
        var screen = (AchievementsViewModel)window.DataContext!;

        try
        {
            var latest = screen.Earned.OrderByDescending(e => e.Card.EarnedUtc ?? DateTime.MinValue).First().Card;
            var button = Named<Button>(window, "AchievementsJustUnlocked");

            _output.WriteLine("just unlocked: " + latest.Title + " " + latest.Callsign + " " + latest.EarnedUtc + "; drawn " + string.Join(" | ", VisibleText(button)));

            Assert.Same(latest, screen.Standing!.JustUnlocked);
            Assert.Contains(latest.Title, VisibleText(button));

            var at = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

            window.MouseMove(at);
            window.MouseDown(at, MouseButton.Left);
            window.MouseUp(at, MouseButton.Left);
            Settle(window);

            Assert.True(screen.MapIsOpen, "pressing just unlocked opened no map");
            Assert.Same(latest.Globe, screen.OpenedMap);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>The callsign of the latest earned card's contact where it has no map, found through the view model.</summary>
    private static string latestNoGridCall(IReadOnlyList<AdifLogRecord> records)
        => new AchievementsViewModel(records, MyGrid, AchievementPoints.Parse(AchievementPoints.Shipped())).Standing!.JustUnlocked!.Callsign;

    internal static Window Realized(IReadOnlyList<AdifLogRecord> records, AchievementPoints points, double width, double height = 860)
    {
        var window = new AchievementsWindow
        {
            DataContext = new AchievementsViewModel(records, MyGrid, points),
            Width = width,
            Height = height,
        };

        window.Show();
        Settle(window);

        return window;
    }

    internal static void Settle(Window window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    internal static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    internal static IEnumerable<string> VisibleText(Control root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0)
            .Select(t => t.Text!);
}
