using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Startup;

/// <summary>
/// The first spot load waits for the radio (HM-DEC-118).
/// </summary>
/// <remarks>
/// <para>**AN EMPTY PANEL ASSERTS NOTHING AND A WRONG-BAND PANEL ASSERTS
/// SOMETHING FALSE**, which is the distinction §0.0 exists to draw. Startup used
/// to load spots from the view model's constructor, before anything was
/// connected, so RBN was filtered and the skimmer watch scoped to whatever band
/// was last remembered (HM-DEC-024, HM-DEC-075).</para>
/// <para>It was seen once, on the training radio, and it self-corrects on the
/// first band change. What it costs meanwhile is a burst of calls to somebody
/// else's service about a band nobody is on, which HM-DEC-024 commits Hamlet not
/// to make.</para>
/// <para>Asserted against the source rather than by driving a window, because
/// the fault is an ordering between a constructor and an event and there is no
/// way to observe that from outside without a radio.</para>
/// </remarks>
public sealed class StartupSpotOrderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the surrounding lines are printed.</param>
    public StartupSpotOrderTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the load happens once and from the reconnect path. Two call sites
    /// would mean the constructor's one is still there beside the new one, which
    /// is the fault unfixed with a second copy for company.
    /// </remarks>
    [Fact]
    public void TheStartupLoadIsCalledOnceAndAfterTheRadioIsAsked()
    {
        var source = File.ReadAllText(Path.Combine(
            Root(), "src", "Hamlet.App", "ViewModels", "MainWindowViewModel.cs"));

        var calls = source.Split("ReloadSpotsAsync(\"startup\")").Length - 1;

        _output.WriteLine($"{calls} startup load call site(s)");

        Assert.Equal(1, calls);

        // And it sits inside the reconnect, which runs from the window's Opened
        // event, rather than in the constructor that builds the view model.
        var reconnect = source.IndexOf(
            "public async Task ReconnectOnStartupAsync", StringComparison.Ordinal);

        var call = source.IndexOf(
            "ReloadSpotsAsync(\"startup\")", StringComparison.Ordinal);

        Assert.True(reconnect > 0, "the reconnect entry point has been renamed");

        Assert.True(
            call > reconnect,
            "the startup spot load is still ahead of the reconnect, so it runs "
            + "before the radio has said where it is");
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
