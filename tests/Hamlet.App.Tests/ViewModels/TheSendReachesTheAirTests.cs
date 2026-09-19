using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 362 (the FT8 regression) task 1: **a send reaches the air, and a send that
/// does not says why in the record.**
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR'S RECORD OF 2026-09-19** held two FT8 sends that stopped at `read_back`
/// with nothing after them. At the bench the one path that leaves exactly that record is a press
/// with no armed send, and the return there wrote to the screen and not to the file. **A click
/// that does not transmit says so** (§0.2), in the record as well as on the screen (R13).</para>
/// <para>**EVERY EARLY RETURN BETWEEN READ-BACK AND ARMING IS DRIVEN**, and each must leave
/// `send_refused` with a stable reason. The working sends are driven too, FT8, FT4 and PSK31, each
/// to the sound card, so the repair is proved not to have broken the mode beside it.</para>
/// <para>**NOTHING HERE OPENS A PORT OR KEYS A RADIO** (FACT-004): the wire is `FakePort` and the
/// card is `FakeSink`.</para>
/// </remarks>
public sealed class TheSendReachesTheAirTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Device = "{0.0.0.00000000}.{a-fake-endpoint}";
    private const long Ft8On20m = 14_074_000;
    private const long Ft4On20m = 14_080_000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-air-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public TheSendReachesTheAirTests(ITestOutputHelper output)
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
        }
    }

    /// <summary>**An FT8 CQ passes composed, read-back and armed, and plays at the boundary with its record written.**</summary>
    [Fact]
    public Task AnFt8CqReachesTheAir() => ReachesTheAir(DigitalMode.Ft8, Ft8On20m);

    /// <summary>**The same for FT4.**</summary>
    [Fact]
    public Task AnFt4CqReachesTheAir() => ReachesTheAir(DigitalMode.Ft4, Ft4On20m);

    /// <summary>**A PSK31 CQ still carries its burst, and its records say it was announced.**</summary>
    [Fact]
    public void APsk31CqStillCarriesItsBurstAndSaysSo()
    {
        FakeSink sink;

        using (var telemetry = new JsonlTelemetry(_folder, "362", _ => true))
        {
            var settings = Settings(named: true);
            var model = new MainWindowViewModel(settings, telemetry) { OperatingMode = "Digital" };

            model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
            model.FrequencyHz = Ft8On20m;
            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
            model.ChooseDigitalModeCommand.Execute("PSK31");

            var port = new FakePort();

            sink = new FakeSink();
            model.UseRigPortForTests(port);
            model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

            model.SendCallToAnyoneCommand.Execute(null);

            for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
            {
                System.Threading.Thread.Sleep(10);
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var lines = Lines();

        Assert.Equal(1, sink.TimesCalled);
        Assert.True(Assert.Single(Events(lines, "psk31_send_composed")).GetProperty("announced").GetBoolean());
        Assert.True(Assert.Single(Events(lines, TransmitRecord.EventName)).GetProperty("announced").GetBoolean());
        Assert.Empty(Events(lines, "send_refused"));
    }

    /// <summary>**A message that would not read back leaves `send_refused`, not silence.**</summary>
    [Fact]
    public void AnUnreadableMessageSaysWhyInTheRecord()
    {
        var (panel, port, _) = Refused(connectWith: new FakePort(), named: true, "VP2MAA " + Mine + " FN00DJ");

        var refused = Assert.Single(Events(Lines(), "send_refused"));

        Assert.Equal("read_back", refused.GetProperty("stage").GetString());
        Assert.Equal("unreadable_on_the_air", refused.GetProperty("reason").GetString());
        Assert.Null(panel.ArmedForSlotUtc);
        Assert.Empty(port!.Written);
    }

    /// <summary>
    /// **Each way of having nothing armed leaves `send_refused` naming which**, at the press, and a
    /// line at connect saying the transmit path was not built and why.
    /// </summary>
    /// <param name="withPort">Whether a radio with a serial port is connected.</param>
    /// <param name="named">Whether a transmit audio device is named in Settings.</param>
    /// <param name="reason">The stable reason the record must carry.</param>
    [Theory]
    [InlineData(false, false, "no_radio")]
    [InlineData(false, true, "no_serial_port")]
    [InlineData(true, false, "no_transmit_device")]
    public void NothingArmedSaysWhyInTheRecord(bool withPort, bool named, string reason)
    {
        var (panel, port, _) = Refused(withPort ? new FakePort() : null, named, "W1AW " + Mine + " FN00");

        var lines = Lines();
        var refused = Assert.Single(Events(lines, "send_refused"));

        Assert.Equal("arm", refused.GetProperty("stage").GetString());
        Assert.Equal(reason, refused.GetProperty("reason").GetString());
        Assert.Contains(Events(lines, "transmit_path"), e => e.GetProperty("reason").GetString() == reason);
        Assert.Null(panel.ArmedForSlotUtc);
        Assert.True(port is null || port.Written.Count == 0);
    }

    /// <summary>
    /// **The repair: a transmit device named after the radio connected is used at the next press**,
    /// rather than the send stopping silently until the radio is connected again.
    /// </summary>
    [Fact]
    public async Task ADeviceNamedAfterTheRadioConnectedIsUsedAtThePress()
    {
        using var telemetry = new JsonlTelemetry(_folder, "362", _ => true);

        var settings = Settings(named: false);
        var panel = Panel(settings, telemetry, DigitalMode.Ft8, Ft8On20m);
        var port = new FakePort();
        var sink = new FakeSink();

        panel.TransmitSinkFactory = _ => sink;
        panel.UseRigPortForTests(port);
        panel.BuildTheArmedSend(port);

        Assert.False(panel.HasSomethingToTransmitThrough);

        settings.AudioOutputDeviceId = Device;
        panel.SendCallToAnyoneCommand.Execute(null);

        var slot = panel.ArmedForSlotUtc;

        _output.WriteLine("screen: " + panel.DigitalSendLine);
        Assert.True(slot is not null, panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot.Value);

        Assert.True(result!.Run!.AudioWentOut, result.Run.Reason);
    }

    private async Task ReachesTheAir(DigitalMode mode, long frequencyHz)
    {
        Ft8BoundaryResult? result;

        using (var telemetry = new JsonlTelemetry(_folder, "362", _ => true))
        {
            var panel = Panel(Settings(named: true), telemetry, mode, frequencyHz);
            var port = new FakePort();
            var sink = new FakeSink();

            panel.TransmitSinkFactory = _ => sink;
            panel.UseRigPortForTests(port);
            panel.BuildTheArmedSend(port);

            panel.SendCallToAnyoneCommand.Execute(null);

            var slot = panel.ArmedForSlotUtc;

            _output.WriteLine(mode + " screen: " + panel.DigitalSendLine);
            Assert.True(slot is not null, panel.DigitalSendLine);

            result = await panel.AtSlotBoundaryAsync(slot.Value);
        }

        var lines = Lines();
        var stages = Events(lines, SendStage.EventName).Select(e => e.GetProperty("stage").GetString()).ToList();

        _output.WriteLine(mode + " stages: " + string.Join(", ", stages));

        Assert.True(result!.Run!.AudioWentOut, result.Run.Reason);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, result.Run.CameOutOfTransmit);

        foreach (var stage in new[] { SendStage.Composed, SendStage.ReadBack, SendStage.Armed, SendStage.Keyed, SendStage.HandedToTheSoundCard })
        {
            Assert.Contains(stage, stages);
        }

        Assert.Single(Events(lines, TransmitRecord.EventName));
        Assert.Empty(Events(lines, "send_refused"));
    }

    private (MainWindowViewModel Panel, FakePort? Port, FakeSink Sink) Refused(FakePort? connectWith, bool named, string message)
    {
        using var telemetry = new JsonlTelemetry(_folder, "362", _ => true);

        var panel = Panel(Settings(named), telemetry, DigitalMode.Ft8, Ft8On20m);
        var sink = new FakeSink();

        panel.TransmitSinkFactory = _ => sink;
        panel.UseRigPortForTests(connectWith);
        panel.BuildTheArmedSend(connectWith);

        panel.SendMessageCommand.Execute(message);

        _output.WriteLine("screen: " + panel.DigitalSendLine);

        return (panel, connectWith, sink);
    }

    private static AppSettings Settings(bool named)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = named ? Device : null;

        return settings;
    }

    private static MainWindowViewModel Panel(AppSettings settings, JsonlTelemetry telemetry, DigitalMode mode, long frequencyHz)
    {
        var panel = new MainWindowViewModel(settings, telemetry) { OperatingMode = "Digital" };

        panel.UseModeForTests(mode);
        panel.SelectedBand = panel.Bands.First(b => b.Band.LowHz <= frequencyHz && b.Band.HighHz >= frequencyHz);
        panel.FrequencyHz = frequencyHz;

        return panel;
    }

    private List<string> Lines()
    {
        var lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

        foreach (var line in lines.Where(l => l.Contains("send_", StringComparison.Ordinal)
                                              || l.Contains("transmit_path", StringComparison.Ordinal)
                                              || l.Contains(TransmitRecord.EventName, StringComparison.Ordinal)))
        {
            _output.WriteLine(line.Length > 240 ? line[..240] : line);
        }

        return lines;
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();
}
