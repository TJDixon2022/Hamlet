using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 265, task 5: **Settings carries the transmit drive, beside
/// the endpoint picker, and it will not accept a level the composer would
/// refuse.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** The conservative default and
/// the applied scale are what protect the band and they are task 2's; this is
/// the control on top of them. Without it the level is honoured from
/// `%AppData%\Hamlet\settings.json` and nowhere else, so changing it means
/// editing JSON by hand - which an operator setting a drive against an ALC meter
/// will do once and then stop doing.</para>
/// <para>**NOTHING HERE ENUMERATES ANYTHING AND NOTHING OPENS A DEVICE**
/// (`SHACK_FACTS.md` FACT-004), the same rule
/// <see cref="SettingsNamesTheTransmitEndpointTests"/> keeps. **And no figure
/// here says anything about the IC-7300**: what its USB modulation input expects
/// is not in this repository, which is exactly what the note on the screen has
/// to tell the operator.</para>
/// </remarks>
public sealed class SettingsCarriesTheTransmitDriveTests
{
    private static readonly RenderEndpoint Radio = new(
        "{0.0.0.00000000}.{usb-codec}", "USB Audio CODEC", false, 48_000, 2, 32, "float32");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the note and the level are printed.</param>
    public SettingsCarriesTheTransmitDriveTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The box opens showing the level that is actually in force.**</summary>
    [Fact]
    public void TheBoxShowsTheLevelTheSettingsFileHolds()
    {
        var panel = Panel(new AppSettings());

        _output.WriteLine("percent shown : " + panel.TransmitDrivePercent);
        _output.WriteLine("peak in file  : " + Ft8Composer.DefaultDrivePeak);

        Assert.Equal(Ft8Composer.DefaultDrivePeak * 100.0, panel.TransmitDrivePercent, 4);

        var already = Panel(new AppSettings { TransmitDrivePeak = 0.4f });

        Assert.Equal(40.0, already.TransmitDrivePercent, 4);
    }

    /// <summary>**Setting a level writes it into the settings.**</summary>
    [Fact]
    public void ChangingTheDriveWritesItIntoTheSettings()
    {
        var settings = new AppSettings();
        var panel = Panel(settings);

        panel.TransmitDrivePercent = 40.0;

        _output.WriteLine("peak now : " + settings.TransmitDrivePeak);

        Assert.Equal(0.4f, settings.TransmitDrivePeak, 4);
    }

    /// <summary>
    /// **It will not accept a value the composer would refuse.**
    /// </summary>
    /// <remarks>
    /// The question is asked of <c>Ft8Composer.DriveIsUsable</c> rather than
    /// answered again on this screen, so a level the box accepted could never be
    /// one the send path then refused with a sentence at the moment the operator
    /// pressed send. **The setting keeps the last usable level** rather than
    /// storing one that would be rejected.
    /// </remarks>
    [Theory]
    [InlineData(0.0)]
    [InlineData(-10.0)]
    [InlineData(101.0)]
    public void ALevelTheComposerWouldRefuseIsNotWrittenToTheSettings(double percent)
    {
        var settings = new AppSettings { TransmitDrivePeak = 0.3f };
        var panel = Panel(settings);

        panel.TransmitDrivePercent = percent;

        _output.WriteLine("asked for : " + percent + " %");
        _output.WriteLine("peak now  : " + settings.TransmitDrivePeak);
        _output.WriteLine("note      : " + panel.TransmitDriveNote);

        Assert.Equal(0.3f, settings.TransmitDrivePeak, 4);
        Assert.Contains("not a transmit level", panel.TransmitDriveNote, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The note shows the level in dBFS and says it is a starting point set
    /// against the radio's own ALC.**
    /// </summary>
    /// <remarks>
    /// The breakage: a number on a screen that reads as a specification. FACT-004
    /// - what the IC-7300's USB modulation input expects is not in this
    /// repository, so the one thing the screen must not do is let a default look
    /// like the right answer.
    /// </remarks>
    [Fact]
    public void TheNoteSaysItIsAStartingPointToBeSetAgainstTheRadiosAlc()
    {
        var panel = Panel(new AppSettings());

        _output.WriteLine(panel.TransmitDriveNote);

        // THE LEVEL, IN THE UNIT IT IS TALKED ABOUT IN.
        Assert.Contains("-12.0 dBFS", panel.TransmitDriveNote, StringComparison.Ordinal);

        // AND THAT IT IS A STARTING POINT, NOT A SPECIFICATION.
        Assert.Contains(
            "starting point, not a specification",
            panel.TransmitDriveNote,
            StringComparison.Ordinal);

        // AND WHAT HE SETS IT AGAINST.
        Assert.Contains("ALC", panel.TransmitDriveNote, StringComparison.Ordinal);
    }

    private static SettingsViewModel Panel(AppSettings settings)
        => new(settings, null, new NoDevices(), () => new[] { Radio });

    /// <summary>A capture-device source that opens nothing and offers nothing.</summary>
    private sealed class NoDevices : IAudioDevices
    {
        /// <inheritdoc/>
        public IReadOnlyList<AudioDevice> List() => [];
    }
}
