using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Within reach right now** (work instruction 506 task 3, HM-DEC-211): who on the CQ list would earn him
/// something, and what, in words.
/// </summary>
public sealed class TheWithinReachTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the strip is printed.</param>
    public TheWithinReachTests(ITestOutputHelper output) => _output = output;

    private static DigitalDecodeRow Heard(string message) => new("214100", "-10", "0.2", "1200", message);

    private static Window Realized(CqSnapshot calling)
    {
        var window = new AchievementsWindow
        {
            DataContext = new AchievementsViewModel(TheAchievementsPageTests.TwelveContacts(), "FN00", AchievementPoints.Parse(AchievementPoints.Shipped()))
            {
                Calling = calling,
            },
            Width = 1280,
            Height = 860,
        };

        window.Show();
        TheAchievementsStandingTests.Settle(window);

        return window;
    }

    /// <remarks>
    /// Four callers who would each earn something draw three, doors first; the one who opens a continent is
    /// ringed and says so in words; the heading says when the list was read.
    /// </remarks>
    [AvaloniaFact]
    public void ThreeCallersDrawThreeAndTheDoorIsRinged()
    {
        var read = new DateTime(2026, 9, 30, 14, 32, 0, DateTimeKind.Utc);
        var window = Realized(CqSnapshot.From(
            new[] { Heard("CQ OE8DDX JN76"), Heard("CQ DX J38DX FK92"), Heard("CQ K1ABC FN42"), Heard("CQ ZL1ABC RF72") }, read));

        try
        {
            var strip = TheAchievementsStandingTests.Named<Control>(window, "AchievementsWithinReach");
            var said = TheAchievementsStandingTests.VisibleText(strip).ToList();
            var boxes = strip.GetVisualDescendants().OfType<Border>().Where(b => b.IsEffectivelyVisible && b.Classes.Contains("reach-station")).ToList();
            var screen = (AchievementsViewModel)window.DataContext!;

            _output.WriteLine("strip: " + string.Join(" | ", said));

            Assert.Equal(3, boxes.Count);
            Assert.Contains("from the CQ list read at 14:32 UTC", said);
            Assert.Contains("ZL1ABC", said);

            var door = screen.WithinReach!.Stations.Single(s => s.OpensContinent);
            var doorBox = boxes.Single(b => ReferenceEquals(b.DataContext, door));

            Assert.StartsWith("opens ", door.Earns, StringComparison.Ordinal);
            Assert.Contains(door.Earns, TheAchievementsStandingTests.VisibleText(doorBox));
            Assert.Contains(doorBox.GetVisualDescendants().OfType<Control>(), c => c.IsEffectivelyVisible && c.Classes.Contains("reach-ring"));
            Assert.Same(door, screen.WithinReach.Stations[0]);
            Assert.All(boxes.Where(b => b != doorBox), b => Assert.DoesNotContain(b.GetVisualDescendants().OfType<Control>(), c => c.IsEffectivelyVisible && c.Classes.Contains("reach-ring")));
            Assert.Empty(TheAchievementTilesTests.Clips(window));
        }
        finally
        {
            window.Close();
        }
    }

    /// <remarks>An empty snapshot draws one line and no boxes.</remarks>
    [AvaloniaFact]
    public void AnEmptySnapshotDrawsOneLine()
    {
        foreach (var calling in new[] { CqSnapshot.None, CqSnapshot.From(Array.Empty<DigitalDecodeRow>(), DateTime.UtcNow) })
        {
            var window = Realized(calling);

            try
            {
                var strip = TheAchievementsStandingTests.Named<Control>(window, "AchievementsWithinReach");
                var said = TheAchievementsStandingTests.VisibleText(strip).ToList();

                _output.WriteLine("empty: " + string.Join(" | ", said));

                Assert.DoesNotContain(strip.GetVisualDescendants().OfType<Border>(), b => b.IsEffectivelyVisible && b.Classes.Contains("reach-station"));
                Assert.Contains(((AchievementsViewModel)window.DataContext!).WithinReach!.NobodyLine, said);
            }
            finally
            {
                window.Close();
            }
        }
    }
}
