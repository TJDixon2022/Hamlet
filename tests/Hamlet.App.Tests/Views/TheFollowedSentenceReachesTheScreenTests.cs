using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// What the terminal says while it refills a window it emptied to follow
/// somebody.
/// </summary>
/// <remarks>
/// <para>**A TERMINAL THAT GOES QUIET WITHOUT SAYING WHY IS ITS OWN CONFIDENT
/// WRONG ANSWER** (§0.0). Following somebody empties twelve seconds of held audio,
/// and twelve seconds of nothing with no explanation reads as a dead band at the
/// one moment it certainly is not one.</para>
/// <para>**PROVED ON THE REAL WINDOW AND NOT ON A PROPERTY.** A property that
/// returns the right string and an element that is not on the screen look the
/// same to every test that reads a view model, and that is exactly how the
/// capture press disappeared for a day.</para>
/// </remarks>
public sealed class TheFollowedSentenceReachesTheScreenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drawn text is printed.</param>
    public TheFollowedSentenceReachesTheScreenTests(ITestOutputHelper output)
        => _output = output;

    private static IEnumerable<string> AdvisoryTexts(Window window)
        => window.GetVisualDescendants()
            .OfType<GlossaryTextControl>()
            .Select(c => c.Text ?? "")
            .Where(t => t.Length > 0);

    /// <remarks>
    /// Proves the sentence is drawn where the operator is already looking, and
    /// that it goes when text resumes.
    /// </remarks>
    [AvaloniaFact]
    public void ItIsDrawnWhileRefillingAndGoesWhenTextResumes()
    {
        // There is no layout store to protect any more (Tim, 2026-08-27).

            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            window.Show();

            // **THE TERMINAL IS THE CW WORKSPACE NOW**, permanent rather
            // than a widget somebody has to fetch (Tim, 2026-08-27), so there
            // is nothing to add before looking for it.

            model.ListeningAfresh = true;

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var drawn = AdvisoryTexts(window).ToList();
            var said = drawn.Any(t => t.Contains(
                "has moved across to them", StringComparison.Ordinal));

            foreach (var text in drawn)
            {
                _output.WriteLine("drawn: " + text);
            }

            // And it goes when there is text again.
            model.ListeningAfresh = false;

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var afterwards = AdvisoryTexts(window).Any(t => t.Contains(
                "has moved across to them", StringComparison.Ordinal));

            window.Close();

            Assert.True(
                said,
                "the terminal goes quiet for twelve seconds after following "
                + "somebody and does not say why, which reads as a dead band at "
                + "the moment somebody answered");

            Assert.False(afterwards, "the sentence outstayed the silence");
    }
}
