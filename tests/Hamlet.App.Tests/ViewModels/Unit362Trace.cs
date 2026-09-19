using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 362 (the FT8 regression) task 0: **an FT8 send at the bench, stage by stage,
/// against the operator's record.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION.** Each press is made on the application's own
/// send path with a fake wire and a fake card (`FakePort`, `FakeSink`), and the record it writes is
/// read back from a real `JsonlTelemetry` file, so the stages printed are the stages the file would
/// hold. **It asserts nothing**, so it cannot become a wall.</para>
/// <para>**NOTHING HERE OPENS A PORT OR KEYS ANYTHING, AND NOTHING MEASURED HERE IS EVIDENCE ABOUT
/// THE RADIO** (FACT-004).</para>
/// </remarks>
public sealed class Unit362Trace : IDisposable
{
    private const string Mine = "KC3QIS";
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit362-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the stages are printed.</param>
    public Unit362Trace(ITestOutputHelper output)
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

    /// <summary>Press FT8 four ways and print what the record says each time.</summary>
    [Fact]
    public async Task AnFt8SendStageByStage()
    {
        await Press("the CQ button", armed: true, p => p.SendCallToAnyoneCommand.Execute(null));
        await Press("a 16-character reply", armed: true, p => p.SendMessageCommand.Execute("W1AW " + Mine + " FN00"));
        await Press("no armed send (no port)", armed: false, p => p.SendMessageCommand.Execute("W1AW " + Mine + " FN00"));
        await Press("an unreadable message", armed: true, p => p.SendMessageCommand.Execute("VP2MAA " + Mine + " FN00DJ"));
    }

    private async Task Press(string what, bool armed, Action<MainWindowViewModel> press)
    {
        var name = what.Replace(' ', '-');
        var folder = Path.Combine(_folder, name);

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "362", _ => true))
        {
            var settings = new AppSettings();

            settings.Operator.Callsign = Mine;
            settings.Operator.GridSquare = "FN00";
            settings.Operator.LicenseClass = LicenseClass.General;
            settings.AudioOutputDeviceId = "{0.0.0.00000000}.{a-fake-endpoint}";

            var panel = new MainWindowViewModel(settings, telemetry) { OperatingMode = "Digital" };

            panel.UseModeForTests(DigitalMode.Ft8);
            panel.SelectedBand = panel.Bands.First(b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
            panel.FrequencyHz = Ft8On20m;

            var port = new FakePort();
            var sink = new FakeSink();

            panel.TransmitSinkFactory = _ => sink;

            if (armed)
            {
                panel.UseRigPortForTests(port);
                panel.BuildTheArmedSend(port);
            }

            press(panel);

            var slot = panel.ArmedForSlotUtc;
            string outcome = "not armed, so no boundary";

            if (slot is { } s)
            {
                var result = await panel.AtSlotBoundaryAsync(s);

                outcome = "boundary " + s.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                          + ", audio went out " + (result?.Run?.AudioWentOut.ToString() ?? "no run")
                          + ", " + (result?.Run?.CameOutOfTransmit.ToString() ?? "-");
            }

            _output.WriteLine("-- " + what);
            _output.WriteLine("   screen : " + panel.DigitalSendLine);
            _output.WriteLine("   result : " + outcome + "; frames on the wire " + port.Written.Count);
        }

        foreach (var line in Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines))
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            var evt = root.TryGetProperty("event", out var e) ? e.ToString() : "?";

            if (evt is "send_stage" or "operator_action" or "send_refused" or "ft8_transmission"
                || evt.Contains("refus", StringComparison.Ordinal))
            {
                var data = root.TryGetProperty("data", out var d) ? d.ToString() : "";

                _output.WriteLine("   " + evt + " " + (data.Length > 160 ? data[..160] : data));
            }
        }
    }
}
