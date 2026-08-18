using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// The controls HM-DEC-134 and HM-DEC-060 rule into existence are on a screen and
/// wired to something.
/// </summary>
/// <remarks>
/// <para>**A RULING THAT WAS BUILT AND CANNOT BE REACHED IS A RULING THAT DID NOT
/// SHIP**, and this repository has done it twice: HM-DEC-072's manage window was
/// ruled, built and unreachable, and the calling cycle was on no preset at all.
/// So the question "can he get at it" is asked by a test rather than by somebody
/// remembering to look.</para>
/// <para>**AND THE MAIN WINDOW'S OWN BINDING SWEEP DOES NOT COVER THESE.** A
/// dropdown realizes its item template only when it is opened, so the button
/// inside the recent list's rows is invisible to `BindingHealthTests` however
/// carefully that test runs the loop — which is exactly the shape of HM-DEC-087,
/// a control bound to nothing looking identical to a disabled one.</para>
/// </remarks>
public sealed class RemovalReachableTests
{
    private static AppSettings WithTwoPlaces()
    {
        var settings = new AppSettings();

        settings.Recent.Add(new SavedRecentStation
        {
            FrequencyHz = 7_030_100,
            Mode = "CW",
            BandName = "40 m",
            VisitedUtc = new DateTime(2026, 8, 18, 20, 31, 0, DateTimeKind.Utc),
            Visits = 2,
        });

        settings.Recent.Add(new SavedRecentStation
        {
            FrequencyHz = 7_047_000,
            Mode = "CW",
            BandName = "40 m",
            VisitedUtc = new DateTime(2026, 8, 18, 20, 30, 0, DateTimeKind.Utc),
        });

        return settings;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-134: the way to forget where you are is a real
    /// control on the strip, bound to a real command, visible exactly when
    /// there is something there to forget (§0.5.1) — and pressing it takes the
    /// place out of the list.</para>
    /// <para>**IT IS BESIDE THE DROPDOWN AND NOT INSIDE IT, AND THIS TEST IS WHY.**
    /// The first build put the button in the dropdown's row template, where its
    /// containers live in a popup with its own visual root: the test could not
    /// reach the button at all, and a control a test cannot reach is a control
    /// whose deadness nothing can report (HM-DEC-087).</para>
    /// </remarks>
    [AvaloniaFact]
    public void ForgettingWhereYouAreIsALiveControlOnTheStrip()
    {
        var complaints = new Complaints();
        var was = Logger.Sink;
        Logger.Sink = complaints;

        var layouts = Hamlet.App.Layout.LayoutStore.Path;
        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var model = new MainWindowViewModel(WithTwoPlaces(), null);
            var window = new MainWindow { DataContext = model };

            window.Show();
            Pump();

            var button = window.GetVisualDescendants()
                .OfType<Button>()
                .Single(b => b.Content as string == "forget this place");

            Assert.NotNull(button.Command);

            // Nowhere near either remembered place, so there is nothing to
            // offer and the control is absent rather than grey. Inside the band
            // on purpose: the dial is clamped to the picture (HM-DEC-055), so a
            // frequency on another band would not move it at all.
            model.FrequencyHz = 7_150_000;
            Pump();
            Assert.False(model.IsSomewhereRemembered);
            Assert.False(button.IsVisible);

            // A hundred hertz off one of them is the same place (HM-DEC-072),
            // so the offer stands where the operator would expect it to.
            model.FrequencyHz = 7_030_000;
            Pump();
            Assert.True(model.IsSomewhereRemembered);
            Assert.True(button.IsVisible);

            button.Command!.Execute(button.CommandParameter);
            Pump();

            Assert.Single(model.Recent);
            Assert.DoesNotContain(model.Recent, e => e.FrequencyHz == 7_030_100);
            Assert.False(button.IsVisible);

            window.Close();
        }
        finally
        {
            Logger.Sink = was;

            try
            {
                File.Delete(Hamlet.App.Layout.LayoutStore.Path);
            }
            catch (IOException)
            {
                // A leftover temporary file is not a failing test.
            }

            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }

        var bindings = complaints.Lines
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the recent controls have bindings that do not resolve:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, bindings));
    }

    /// <remarks>
    /// Proves HM-DEC-060 and HM-DEC-072: the manage window builds against the
    /// real view model with every binding resolving, so the door the Radio menu
    /// opens leads somewhere. That submenu was ruled and never built once
    /// already, which is why this is measured rather than assumed.
    /// </remarks>
    [AvaloniaFact]
    public void TheManageWindowBuildsWithoutOneComplaint()
    {
        var complaints = new Complaints();
        var was = Logger.Sink;
        Logger.Sink = complaints;
        var removed = 0;

        try
        {
            var model = new FavoritesViewModel(
                new System.Collections.ObjectModel.ObservableCollection<
                    Hamlet.RadioEngine.Explore.Favorite>(),
                () => { },
                new[]
                {
                    Hamlet.RadioEngine.Explore.RecentStations.From(
                        7_030_100, "", "CW", null,
                        new DateTime(2026, 8, 18, 20, 31, 0, DateTimeKind.Utc),
                        Hamlet.RadioEngine.Explore.StationSource.None),
                },
                _ => { },
                forget => removed++);

            var window = new FavoritesWindow { DataContext = model };

            window.Show();
            Pump();

            // **THE SIBLING'S REMOVE, WHICH RECENT DID NOT INHERIT UNTIL NOW**
            // (HM-DEC-060, HM-DEC-134). Asserted by pressing it rather than by
            // finding it, because a button that is present and bound to nothing
            // is the fault this whole file exists for.
            var forget = window.GetVisualDescendants()
                .OfType<Button>()
                .Single(b => b.Content as string == "forget");

            Assert.NotNull(forget.Command);
            forget.Command!.Execute(forget.CommandParameter);
            Pump();

            Assert.Empty(model.Recent);
            Assert.Equal(1, removed);

            window.Close();
        }
        finally
        {
            Logger.Sink = was;
        }

        var bindings = complaints.Lines
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the manage window has bindings that do not resolve:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, bindings));
    }

    private static void Pump()
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }
    }
}
