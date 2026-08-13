using Hamlet.App.Settings;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.Settings;

/// <summary>
/// The per-source switches (HM-DEC-022) and the two sources that ship off
/// (HM-DEC-024).
/// </summary>
public sealed class SourceSettingsTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_folder, "settings.json");

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    /// <remarks>
    /// Proves POTA and RBN ship on: they are public feeds that ask nothing of
    /// the operator beyond ordinary politeness, and a newcomer should see
    /// spots the first time they open the app.
    /// </remarks>
    [Theory]
    [InlineData(PotaActivitySource.SourceName)]
    [InlineData(RbnActivitySource.SourceName)]
    public void LiveFeeds_ShipOn(string source)
        => Assert.True(new AppSettings().IsSourceEnabled(source));

    /// <remarks>
    /// Proves SOTA ships off. Its API's terms require the developer to be
    /// registered with the SOTA Reflector's API-consumers group and to have
    /// had AI-written software approved, and Hamlet does not enter into that
    /// on the operator's behalf (HM-DEC-024).
    /// </remarks>
    [Fact]
    public void Sota_ShipsOff()
        => Assert.False(new AppSettings().IsSourceEnabled(SotaActivitySource.SourceName));

    /// <remarks>
    /// Proves the sample feed ships off now that live feeds work. Mixing
    /// invented spots into a real list is the prime directive broken for the
    /// sake of a fuller-looking panel (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void SampleFeed_ShipsOff()
        => Assert.False(new AppSettings().IsSourceEnabled(FakeActivitySource.SourceName));

    /// <remarks>
    /// Proves an unknown source name falls back to the default rather than
    /// throwing, so a settings file written by an older build still loads.
    /// </remarks>
    [Fact]
    public void UnknownSource_FallsBackToTheDefault()
        => Assert.True(new AppSettings().IsSourceEnabled("Something New"));

    /// <remarks>
    /// Proves the switches survive a restart, through the real loader.
    /// </remarks>
    [Fact]
    public void Switches_RoundTripThroughSettingsJson()
    {
        var written = new AppSettings();
        written.SetSourceEnabled(PotaActivitySource.SourceName, false);
        written.SetSourceEnabled(SotaActivitySource.SourceName, true);

        SettingsStore.SaveTo(written, SettingsPath);
        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.False(read.IsSourceEnabled(PotaActivitySource.SourceName));
        Assert.True(read.IsSourceEnabled(SotaActivitySource.SourceName));

        // Untouched sources keep their shipped defaults.
        Assert.True(read.IsSourceEnabled(RbnActivitySource.SourceName));
    }
}
