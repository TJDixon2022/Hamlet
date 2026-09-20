using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 369 task 2, `PHASE_PLAN.md` criteria 0.3 and 0.4: **a send
/// that cannot go names the true fault.**
/// </summary>
/// <remarks>
/// <para>**THE REFUSAL NAMED THE WRONG FAULT AND COST FIVE DAYS.** On 2026-09-19
/// the operator's FT8 sends stopped and the record said `send_refused, stage arm,
/// reason transmit_device_would_not_open`. **The device did not fail to open. No
/// device had been chosen**, and Hamlet knew which - the two cases are decided four
/// lines apart in `BuildTheArmedSend`. He found the cause himself: *"the settings
/// lost the listing setting."*</para>
/// <para>**SO THE TWO FAULTS ARE TWO REFUSALS.** No device chosen says so and says
/// where to fix it, with the word *Settings* opening the picker. A device that is
/// chosen and will not open keeps its own reason and **carries the three facts that
/// make it diagnosable** (§0.0.1): which device, what rate was asked of it, and
/// what the operating system said.</para>
/// <para>**NOTHING HERE OPENS A DEVICE OR KEYS A RADIO** (FACT-004, FACT-006): the
/// wire is `FakePort` and the sink factory throws on purpose.</para>
/// </remarks>
public sealed class TheRefusalNamesTheFaultTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Device = "{0.0.0.00000000}.{the-usb-audio-codec}";
    private const long Ft8On20m = 14_074_000;

    /// <summary>What a real `WasapiTransmitSink` says when the name is stale.</summary>
    private const string OsError =
        "there is no active render endpoint called \"" + Device + "\".";

    private readonly ITestOutputHelper _output;

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-refusal-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentence and the record are printed.</param>
    public TheRefusalNamesTheFaultTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(_folder);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    // ------------------------------------------------------------------
    // 0.3 - no device chosen is its own refusal, and its own sentence.
    // ------------------------------------------------------------------

    /// <summary>
    /// **No transmit device chosen yields `no_transmit_device` and the sentence.**
    /// </summary>
    [Fact]
    public void NoDeviceChosenYieldsItsOwnReasonAndItsOwnSentence()
    {
        var panel = Refused(named: false, sinkThrows: null);

        _output.WriteLine("screen: " + panel.DigitalSendLine);
        _output.WriteLine("sentence: " + panel.TransmitRefusalSentence);

        var lines = Lines();
        var refused = Assert.Single(Events(lines, "send_refused"));

        Assert.Equal("arm", refused.GetProperty("stage").GetString());
        Assert.Equal("no_transmit_device", refused.GetProperty("reason").GetString());

        // **THE SENTENCE, WORD FOR WORD** (criterion 0.3).
        Assert.Equal(
            "No transmit device is chosen. Open Settings and pick the radio's "
            + "sound card.",
            panel.TransmitRefusalSentence);

        // **AND IT IS WHAT IS ON THE PANEL**, not a thing the view model knows and
        // the screen does not say.
        Assert.Equal(panel.TransmitRefusalSentence, panel.DigitalSendLine);

        // **IT DOES NOT SAY THE DEVICE WOULD NOT OPEN**, which is the false
        // sentence this unit exists to remove.
        Assert.DoesNotContain(
            "would not open", panel.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "could not be opened",
            panel.DigitalSendLine,
            StringComparison.OrdinalIgnoreCase);

        Assert.Null(panel.ArmedForSlotUtc);
    }

    /// <summary>
    /// **The word *Settings* in the sentence opens the picker.**
    /// </summary>
    /// <remarks>
    /// <para>**THE THREE PIECES ARE THE SENTENCE, AND THE MIDDLE ONE IS THE
    /// COMMAND'S LABEL.** The view puts them side by side with the middle one as a
    /// button, so this asserts both that they reassemble into the sentence exactly
    /// and that the command behind the middle one is the one that opens
    /// Settings.</para>
    /// <para>**THE COMMAND IS NOT EXECUTED HERE.** `OpenSettingsAsync` wants a
    /// desktop lifetime and a main window, and neither exists in a headless run; it
    /// returns without doing anything, which would prove nothing. What is asserted
    /// is that the link carries that command and that it can be pressed - the
    /// window it opens is `BindingHealthTests`' ground.</para>
    /// </remarks>
    [Fact]
    public void TheWordSettingsCarriesTheCommandThatOpensThePicker()
    {
        var panel = Refused(named: false, sinkThrows: null);

        Assert.True(panel.TransmitRefusalOffersSettings);

        Assert.Equal("Settings", panel.TransmitRefusalSettingsWord);

        Assert.Equal(
            panel.TransmitRefusalSentence,
            panel.TransmitRefusalBeforeTheSettingsWord
            + panel.TransmitRefusalSettingsWord
            + panel.TransmitRefusalAfterTheSettingsWord);

        Assert.NotNull(panel.OpenSettingsCommand);
        Assert.True(panel.OpenSettingsCommand.CanExecute(null));
    }

    /// <summary>
    /// **A refusal that Settings cannot fix does not offer Settings.**
    /// </summary>
    /// <remarks>
    /// A device that is plugged out is not fixed by opening the picker, and advice
    /// that does not work is worse than none (§0.0). The link belongs to the one
    /// fault it repairs.
    /// </remarks>
    [Theory]
    [InlineData(true, "transmit_device_would_not_open")]
    [InlineData(false, "no_transmit_device")]
    public void OnlyTheNoDeviceRefusalOffersTheSettingsLink(bool named, string reason)
    {
        var panel = Refused(named, named ? new InvalidOperationException(OsError) : null);

        Assert.Equal(reason == "no_transmit_device", panel.TransmitRefusalOffersSettings);
    }

    // ------------------------------------------------------------------
    // 0.4 - a device that will not open names itself, its rate and the error.
    // ------------------------------------------------------------------

    /// <summary>
    /// **A chosen device that refuses keeps its own reason and carries the device
    /// name, the rate asked for and the OS error - in the event and on the panel.**
    /// </summary>
    [Fact]
    public void AChosenDeviceThatWillNotOpenCarriesNameRateAndError()
    {
        var panel = Refused(named: true, new InvalidOperationException(OsError));

        _output.WriteLine("screen: " + panel.DigitalSendLine);

        var lines = Lines();
        var refused = Assert.Single(Events(lines, "send_refused"));

        Assert.Equal("arm", refused.GetProperty("stage").GetString());
        Assert.Equal(
            "transmit_device_would_not_open", refused.GetProperty("reason").GetString());

        // **IN THE EVENT** (criterion 0.4).
        Assert.Equal(Device, refused.GetProperty("device").GetString());
        Assert.Equal(
            Ft8Composer.DefaultSampleRate, refused.GetProperty("rateAsked").GetInt32());
        Assert.Equal(OsError, refused.GetProperty("osError").GetString());

        // **AND ON EVERY CONNECT-TIME EVENT TOO**, which is where the refusal is
        // decided. There is more than one: the press rebuilds the path before it
        // gives up, which is work instruction 362's repair, and each attempt is
        // written down. **All of them carry the three facts** - an attempt that
        // named the device and one that did not would make the file read as two
        // different faults.
        var paths = Events(lines, "transmit_path")
            .Where(e => e.GetProperty("reason").GetString()
                        == "transmit_device_would_not_open")
            .ToList();

        Assert.NotEmpty(paths);

        foreach (var path in paths)
        {
            Assert.Equal(Device, path.GetProperty("device").GetString());
            Assert.Equal(
                Ft8Composer.DefaultSampleRate, path.GetProperty("rateAsked").GetInt32());
            Assert.Equal(OsError, path.GetProperty("osError").GetString());
        }

        // **AND ON THE PANEL**, all three, in words.
        Assert.Contains(Device, panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            Ft8Composer.DefaultSampleRate.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            panel.DigitalSendLine,
            StringComparison.Ordinal);
        Assert.Contains(OsError, panel.DigitalSendLine, StringComparison.Ordinal);

        Assert.Null(panel.ArmedForSlotUtc);
    }

    /// <summary>
    /// **The refusals that have no device to name do not pretend to have one.**
    /// </summary>
    /// <remarks>
    /// An absent key and an empty one are different pictures (§0.0). `no_radio`,
    /// `no_serial_port` and `no_transmit_device` each genuinely have no device, and
    /// a `"device": ""` on those events would be a fact stated about nothing.
    /// </remarks>
    [Theory]
    [InlineData(false, "no_transmit_device")]
    [InlineData(true, "transmit_device_would_not_open")]
    public void OnlyADeviceRefusalCarriesADeviceName(bool named, string reason)
    {
        Refused(named, named ? new InvalidOperationException(OsError) : null);

        var refused = Assert.Single(Events(Lines(), "send_refused"));

        Assert.Equal(reason, refused.GetProperty("reason").GetString());

        Assert.Equal(
            named, refused.TryGetProperty("device", out _));
    }

    // ------------------------------------------------------------------
    // The harness.
    // ------------------------------------------------------------------

    /// <summary>Connect, press CQ, and hand back the panel that refused.</summary>
    /// <param name="named">Whether a transmit device is named in Settings.</param>
    /// <param name="sinkThrows">What the sink factory throws, or null.</param>
    private MainWindowViewModel Refused(bool named, Exception? sinkThrows)
    {
        using var telemetry = new JsonlTelemetry(_folder, "369", _ => true);

        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = named ? Device : null;

        var panel = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
        };

        panel.UseModeForTests(DigitalMode.Ft8);
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;
        panel.UseWorkedBeforeForTests(
            new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));

        panel.TransmitSinkFactory = _ => throw (sinkThrows
            ?? new InvalidOperationException("no sink was asked for"));

        var port = new FakePort();

        panel.UseRigPortForTests(port);
        panel.BuildTheArmedSend(port);

        panel.SendMessageCommand.Execute("W1AW " + Mine + " FN00");

        Assert.Empty(port.Written);

        return panel;
    }

    private List<string> Lines()
        => Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    /// <summary>The `data` payloads of every event of that name.</summary>
    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();
}
