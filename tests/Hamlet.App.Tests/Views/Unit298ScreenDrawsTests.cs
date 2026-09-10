using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 298: **the screen actually reaches him.**
/// </summary>
/// <remarks>
/// <para>**THIS IS HM-DEC-154's LESSON APPLIED BEFORE IT COSTS ANYTHING.** That
/// ruling was written after a phase built a measurably better decoder and wired none
/// of it in: *a measured gain nobody can see is indistinguishable from none.* A view
/// model full of cards that no window draws is the same fault in a smaller place.
/// </para>
/// <para>**AND THE BINDINGS INSIDE A `DataTemplate` ARE NOT REACHED BY
/// `BindingHealthTests`**, which builds the main window over an empty view model, so
/// no item template is ever realized. Here the window is opened over a real log.
/// </para>
/// </remarks>
public sealed class Unit298ScreenDrawsTests
{
    private const string HisGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drawn screen is printed.</param>
    public Unit298ScreenDrawsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The window draws the tabs, the cards and the challenges.**</summary>
    [AvaloniaFact]
    public void TheAchievementsWindowDrawsTheScreen()
    {
        var complaints = new List<string>();
        var was = Logger.Sink;

        Logger.Sink = new Collector(complaints);

        try
        {
            var window = new AchievementsWindow
            {
                DataContext = new AchievementsViewModel(Full(), HisGrid),
            };

            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var scopes = window.FindControl<TabControl>("AchievementsScopes");
            var challenges = window.FindControl<ItemsControl>("AchievementsChallenges");
            var places = window.FindControl<ItemsControl>("AchievementsPlaces");

            Assert.True(scopes is not null, "the window has no scope tabs");
            Assert.True(challenges is not null, "the window has no challenges list");
            Assert.True(places is not null, "the window has no places list");

            var drawn = window.GetVisualDescendants().OfType<TextBlock>()
                .Select(t => t.Text ?? "")
                .Where(t => t.Trim().Length > 0)
                .ToList();

            foreach (var line in drawn.Take(40))
            {
                _output.WriteLine("  | " + line);
            }

            // **THE THINGS THIS UNIT BUILT ARE ON THE SCREEN**, not merely in a
            // view model somewhere behind it.
            //
            // **THE HEADING IS `Go and try` FROM WORK INSTRUCTION 300**, which named
            // the three sections of the screen it re-presented. The section is the
            // same section and it holds the same cards; only its title is shorter.
            Assert.Contains(drawn, t => t == "Go and try");
            Assert.Contains(drawn, t => t.StartsWith("Furthest", StringComparison.Ordinal));
            Assert.Contains(drawn, t => t.Contains("views open", StringComparison.Ordinal));

            // **AND THE ONE THE INSTRUCTION SAYS MUST NOT SURVIVE IS NOT.**
            Assert.DoesNotContain(drawn, t => t.Contains("of 6.", StringComparison.Ordinal));

            window.Close();
        }
        finally
        {
            Logger.Sink = was;
        }

        var bindings = complaints
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the achievements window has bindings that do not resolve:"
            + Environment.NewLine + string.Join(Environment.NewLine, bindings));
    }

    /// <summary>**An empty log opens the window without drawing a wall.**</summary>
    /// <remarks>
    /// **THE STATE §2 IS ABOUT.** A fresh install must not show a screen full of
    /// things he has not done, and it must not throw either.
    /// </remarks>
    [AvaloniaFact]
    public void AnEmptyLogDrawsNoRecordsAndStillInvites()
    {
        var window = new AchievementsWindow
        {
            DataContext = new AchievementsViewModel(
                Array.Empty<AdifLogRecord>(), HisGrid),
        };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var drawn = window.GetVisualDescendants().OfType<TextBlock>()
            .Select(t => t.Text ?? "")
            .Where(t => t.Trim().Length > 0)
            .ToList();

        foreach (var line in drawn)
        {
            _output.WriteLine("  | " + line);
        }

        Assert.Contains(drawn, t => t.Contains("first contact you log", StringComparison.Ordinal));

        // **NO RECORD CARD ANYWHERE**, because he has opened nothing.
        foreach (var claim in new[] { "Furthest", "Faintest", "Busiest" })
        {
            Assert.DoesNotContain(drawn, t => t.StartsWith(claim, StringComparison.Ordinal));
        }

        // **BUT THE TARGETS ARE HERE** (§3.4), which is the whole point of them.
        Assert.Contains(drawn, t => t == "Go and try");

        window.Close();
    }

    /// <summary>A log with every optional field filled.</summary>
    private static IReadOnlyList<AdifLogRecord> Full()
        => new[]
        {
            Contact("W1ABC", "20m", "FT8", "FN42", -12, -07, "2026-09-10 02:00:00"),
            Contact("EI4GNB", "40m", "FT8", "IO63", -14, -09, "2026-09-10 02:20:00"),
        };

    /// <summary>One sound record.</summary>
    private static AdifLogRecord Contact(
        string call, string band, string mode, string? grid,
        int? sent, int? received, string startedUtc)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = mode,
                GridSquare = grid,
                MyGridSquare = HisGrid,
                ReportSent = sent?.ToString("+00;-00", CultureInfo.InvariantCulture),
                ReportReceived = received?.ToString("+00;-00", CultureInfo.InvariantCulture),
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);

    /// <summary>Keeps every line Avalonia logs while the window is up.</summary>
    private sealed class Collector : ILogSink
    {
        private readonly List<string> _lines;

        public Collector(List<string> lines) => _lines = lines;

        public bool IsEnabled(LogEventLevel level, string area) => true;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
            => _lines.Add("[" + area + "] " + messageTemplate);

        public void Log(
            LogEventLevel level, string area, object? source, string messageTemplate,
            params object?[] propertyValues)
            => _lines.Add("[" + area + "] " + messageTemplate + " "
                          + string.Join(", ", propertyValues));
    }
}
