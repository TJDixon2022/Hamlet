using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **RECORD KEEPS LONGER** (work instruction 548, task 3): how much Record keeps is a setting, thirty seconds by default and up
/// to five minutes, kept across restarts, and the tap holds that much.
/// </summary>
public sealed class RecordKeepsLongerTests : IDisposable
{
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Points the settings at a folder of the test's own.</summary>
    public RecordKeepsLongerTests()
    {
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit548-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the settings back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <remarks>Five minutes set comes back after a restart, and the hover says so in words.</remarks>
    [Fact]
    public void FiveMinutesSetComesBackAfterARestart()
    {
        var first = new MainWindowViewModel(new AppSettings(), null) { RecordSeconds = 300 };

        Assert.Equal(300, first.RecordSeconds);

        var again = new MainWindowViewModel(SettingsStore.Load(), null);

        Assert.Equal(300, again.RecordSeconds);
        Assert.Contains("Records the last 5 minutes", again.CaptureTip, StringComparison.Ordinal);
    }

    /// <remarks>A fresh install keeps thirty seconds, as before, and says the last half minute.</remarks>
    [Fact]
    public void AFreshInstallKeepsHalfAMinute()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        Assert.Equal(30, model.RecordSeconds);
        Assert.Contains("Records the last half minute", model.CaptureTip, StringComparison.Ordinal);
    }

    /// <remarks>A decoder made with five minutes keeps five minutes: six minutes fed in, five come out.</remarks>
    [Fact]
    public void TheTapHoldsWhatWasSet()
    {
        const int rate = 8000;
        var decoder = new CwDecoder(rate, 300);
        var chunk = new float[rate];

        for (var s = 0; s < 360; s++)
        {
            decoder.Process(new AudioChunk((long)s * rate, rate, chunk));
        }

        var kept = decoder.Tap.Snapshot();

        Assert.NotNull(kept);
        Assert.Equal(300, decoder.Tap.KeptSeconds);
        Assert.Equal(300 * rate, kept!.Samples.Length);
        Assert.Equal(AudioTap.MaximumSecondsKept, new AudioTap(600).KeptSeconds);
    }
}
