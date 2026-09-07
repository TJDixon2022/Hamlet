using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 260, task 4: **Settings can name the device the transmission
/// is played into.**
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE ENUMERATES ANYTHING.** `WasapiTransmitSink.Endpoints()`
/// is the running application's business and is never called from a test
/// (`SHACK_FACTS.md` FACT-004): every list below is handed in through the same
/// seam <c>IAudioDevices</c> already uses for the capture side.</para>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT, AND IT IS THE ONE THAT MATTERS
/// ON THIS SCREEN.** A picker that lands on *something* when the saved id names
/// nothing - which is what the capture-side chooser does on purpose, because
/// listening to the wrong device is a quiet waterfall. **Transmitting into the
/// wrong device puts FT8 out of a laptop speaker while the operator believes he
/// is on the air**, so an id that matches nothing here has to select
/// nothing.</para>
/// </remarks>
public sealed class SettingsNamesTheTransmitEndpointTests
{
    private static readonly RenderEndpoint Radio = new(
        "{0.0.0.00000000}.{usb-codec}", "USB Audio CODEC", false, 48_000, 2, 32, "float32");

    private static readonly RenderEndpoint Speakers = new(
        "{0.0.0.00000000}.{speakers}", "Speakers (Realtek)", true, 48_000, 2, 32, "float32");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the note and the choice are printed.</param>
    public SettingsNamesTheTransmitEndpointTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Choosing an endpoint writes its id into the settings.**</summary>
    [Fact]
    public void ChoosingAnEndpointNamesItInTheSettings()
    {
        var settings = new AppSettings();
        var panel = Panel(settings, Speakers, Radio);

        Assert.True(panel.HasTransmitEndpoints);
        Assert.Equal(2, panel.TransmitEndpoints.Count);
        Assert.Null(panel.TransmitEndpoint);
        Assert.Null(settings.AudioOutputDeviceId);

        panel.TransmitEndpoint = Radio;

        _output.WriteLine("named: " + settings.AudioOutputDeviceId);

        Assert.Equal(Radio.Id, settings.AudioOutputDeviceId);
    }

    /// <summary>**A named endpoint comes back selected when the window reopens.**</summary>
    [Fact]
    public void AnEndpointAlreadyNamedComesBackSelected()
    {
        var settings = new AppSettings { AudioOutputDeviceId = Radio.Id };
        var panel = Panel(settings, Speakers, Radio);

        _output.WriteLine("selected: " + panel.TransmitEndpoint?.Name);

        Assert.Equal(Radio, panel.TransmitEndpoint);
    }

    /// <summary>
    /// **An id that names nothing selects nothing, and never the default.**
    /// </summary>
    /// <remarks>
    /// A device id survives a driver update but not being moved to another USB
    /// socket. **Hamlet does not quietly move the transmission to the speakers**,
    /// even though the speakers are this machine's default endpoint and are right
    /// there in the list.
    /// </remarks>
    [Fact]
    public void AnIdThatNamesNothingSelectsNothingAndNeverTheDefault()
    {
        var settings = new AppSettings
        {
            AudioOutputDeviceId = "{0.0.0.00000000}.{a-radio-that-was-unplugged}",
        };

        var panel = Panel(settings, Speakers, Radio);

        _output.WriteLine("selected: " + (panel.TransmitEndpoint?.Name ?? "nothing"));

        Assert.Null(panel.TransmitEndpoint);
        Assert.DoesNotContain(panel.TransmitEndpoints, e => e == panel.TransmitEndpoint);
    }

    /// <summary>
    /// **No endpoints: the box is disabled and the note says so.**
    /// </summary>
    /// <remarks>
    /// Exactly what <c>HasAudioDevices</c> already does for the capture side. A
    /// machine with no playback device is an ordinary machine, and the note reads
    /// as a fact rather than a fault.
    /// </remarks>
    [Fact]
    public void WithNoEndpointsTheBoxIsDisabledAndTheNoteSaysSo()
    {
        var panel = Panel(new AppSettings());

        _output.WriteLine(panel.TransmitEndpointNote);

        Assert.False(panel.HasTransmitEndpoints);
        Assert.Empty(panel.TransmitEndpoints);
        Assert.Contains(
            "cannot see a playback device",
            panel.TransmitEndpointNote,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **The note says what the device is for, in the operator's words.**
    /// </summary>
    /// <remarks>
    /// The breakage: a picker labelled *Transmit* with no explanation, beside an
    /// *Input* picker, on a screen where choosing the wrong one is silent.
    /// </remarks>
    [Fact]
    public void TheNoteNamesTheRadiosOwnUsbAudioInput()
    {
        var panel = Panel(new AppSettings(), Speakers, Radio);

        _output.WriteLine(panel.TransmitEndpointNote);

        Assert.Contains(
            "radio's own USB audio input",
            panel.TransmitEndpointNote,
            StringComparison.Ordinal);

        // AND IT SAYS WHAT HAPPENS IF HE NAMES NONE.
        Assert.Contains(
            "refuses to transmit", panel.TransmitEndpointNote, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Enumeration that throws leaves an empty list, not a window that will
    /// not open.**
    /// </summary>
    /// <remarks>
    /// A machine with the audio service stopped throws out of the enumerator.
    /// The breakage: a Settings window the operator cannot open at all because
    /// of a device he was not trying to use.
    /// </remarks>
    [Fact]
    public void EnumerationThatThrowsLeavesAnEmptyListRatherThanAFault()
    {
        var panel = new SettingsViewModel(
            new AppSettings(),
            null,
            new NoAudioDevices(),
            () => throw new InvalidOperationException("the audio service is not running"));

        _output.WriteLine(panel.TransmitEndpointNote);

        Assert.Empty(panel.TransmitEndpoints);
        Assert.False(panel.HasTransmitEndpoints);
    }

    private static SettingsViewModel Panel(
        AppSettings settings, params RenderEndpoint[] endpoints)
        => new(settings, null, new NoAudioDevices(), () => endpoints);

    /// <summary>A capture-device source that opens nothing and offers nothing.</summary>
    private sealed class NoAudioDevices : IAudioDevices
    {
        /// <inheritdoc/>
        public IReadOnlyList<AudioDevice> List() => Array.Empty<AudioDevice>();
    }
}
