using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 323 task 4: **power is offered at half, and the ALC becomes a sentence.**
/// </summary>
/// <remarks>
/// <para>**§R11, TIM 2026-09-11: THE OPERATOR SETS NOTHING AT THE RADIO.** The drive is what
/// FT8 already composes at; RF power is offered as a percentage beside it and **never
/// written silently** (HM-DEC-084); and during a send an ALC reading past the zone becomes a
/// sentence a person with no shack years can act on.</para>
/// <para>**ON THIS MACHINE THERE IS NO RADIO** (`SHACK_FACTS.md` FACT-004, FACT-006). The
/// offer's write is proved to reach `CivWrites.RfPower` on the rig interface and stop there,
/// and the ALC sentence is proved from a reading handed in. **Every appearance claim is
/// computed** - the window is built headless and its visual tree walked, which says a control
/// exists and is visible and says nothing about what it looks like.</para>
/// </remarks>
public sealed class ThePowerIsOfferedTests : IDisposable
{
    private const long On20m = 14_070_000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public ThePowerIsOfferedTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-power-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
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

    /// <summary>**The offer is on the PSK31 panel, at half, and no USB MOD Level is anywhere.**</summary>
    [AvaloniaFact]
    public void TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel()
    {
        var (window, model, _) = Scene();

        var offer = window.FindControl<TextBlock>("DigitalPsk31PowerOffer");
        var accept = window.FindControl<Button>("DigitalPsk31PowerAccept");

        Assert.NotNull(offer);
        Assert.NotNull(accept);

        _output.WriteLine("offer  : " + offer!.Text);
        _output.WriteLine("button : " + accept!.Content);

        Assert.True(offer.IsEffectivelyVisible);
        Assert.True(accept.IsEffectivelyVisible);

        // **HALF, SAID AS A PERCENTAGE**, on the face rather than in a tooltip.
        Assert.Equal(50, MainWindowViewModel.Psk31PowerPercent);
        Assert.Contains("50%", offer.Text ?? "", StringComparison.Ordinal);

        // **AND NOTHING ON THE SCREEN IS THE USB MOD LEVEL** (§R11: the operator is never
        // asked to set a level, read a meter, or know what ALC is). Every visible piece of
        // text on the whole window, not only this panel.
        var drawn = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible)
            .Select(t => t.Text ?? "")
            .ToList();

        foreach (var word in new[] { "USB MOD", "MOD Level", "ALC" })
        {
            Assert.DoesNotContain(
                drawn, t => t.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>**Accepting writes RF power exactly once; declining writes nothing.**</summary>
    [AvaloniaFact]
    public async Task AcceptingWritesOnceAndDecliningWritesNothing()
    {
        var (_, accepted, acceptedRig) = Scene();

        await accepted.AcceptPsk31PowerCommand.ExecuteAsync(null);

        foreach (var (write, value) in acceptedRig.Written)
        {
            _output.WriteLine("wrote " + write.Field + " = " + value);
        }

        // **ONE WRITE, AND IT IS `CivWrites.RfPower`.**
        var one = Assert.Single(acceptedRig.Written);

        Assert.Same(CivWrites.RfPower, one.Write);
        Assert.Equal(RigField.RfPower, one.Write.Field);

        // **HALF OF THE RANGE THE WRITE ITSELF DECLARES**, `0000 to 0255`.
        Assert.Equal(128, one.Value);

        // **AND THE OFFER IS ANSWERED, SO IT IS NOT ASKED AGAIN.**
        Assert.False(accepted.HasPsk31PowerOffer);

        var (_, declined, declinedRig) = Scene();

        declined.DeclinePsk31PowerCommand.Execute(null);

        _output.WriteLine("after declining: " + declinedRig.Written.Count + " write(s)");

        Assert.Empty(declinedRig.Written);
        Assert.False(declined.HasPsk31PowerOffer);
    }

    /// <summary>**An ALC past the zone becomes a sentence and a line; one inside becomes neither.**</summary>
    [Fact]
    public void AnAlcPastTheZoneBecomesASentenceAndOneInsideItDoesNot()
    {
        foreach (var (reading, past) in new[] { (200.0, true), (60.0, false) })
        {
            List<string> lines;
            string sentence;

            using (var telemetry = new JsonlTelemetry(_folder, "323", _ => true))
            {
                var model = Panel(telemetry);

                model.Psk31AlcForTests = RigValue.Known(
                    RigField.Alc, reading, reading.ToString("0", System.Globalization.CultureInfo.InvariantCulture),
                    DateTime.UtcNow, "handed in");

                model.ReadTheAlcForTests();

                sentence = model.Psk31AlcLine;
            }

            lines = Directory.GetFiles(_folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("\"event\":\"psk31_send_alc\"", StringComparison.Ordinal))
                .ToList();

            var latest = lines[^1];

            _output.WriteLine("ALC " + reading + ": [" + sentence + "]");
            _output.WriteLine("        " + latest);

            // **THE READING IS IN THE RECORD EITHER WAY, WITH ITS AGE AND THE ZONE**
            // (§R13, HM-DEC-111). A good reading is as loggable as a bad one.
            Assert.Contains("\"measured\":true", latest, StringComparison.Ordinal);
            Assert.Contains(
                "\"pastTheZone\":" + (past ? "true" : "false"),
                latest,
                StringComparison.Ordinal);
            Assert.Contains("\"ageMs\":", latest, StringComparison.Ordinal);
            Assert.Contains("\"zone\":128", latest, StringComparison.Ordinal);

            if (past)
            {
                // **A SENTENCE, AND THE ONE THING TO DO ABOUT IT** - no jargon, no meter.
                Assert.Contains("Turn the transmit drive", sentence, StringComparison.Ordinal);
                Assert.DoesNotContain("ALC", sentence, StringComparison.Ordinal);
            }
            else
            {
                Assert.Equal("", sentence);
            }
        }
    }

    /// <summary>One setting Hamlet asked the radio to change.</summary>
    private sealed record Wrote(CivWrite Write, int Value);

    /// <summary>A rig that records what it was asked to set and does nothing else.</summary>
    /// <remarks>
    /// **NO PORT IS EVER OPENED IN THIS PROJECT** (FACT-004). This is the far end of
    /// `MainWindowViewModel.WriteSettingAsync`, and it exists so a test can say the offer's
    /// write reached `CivWrites.RfPower` and stopped there.
    /// </remarks>
    private sealed class WritingRig : IRig
    {
        private readonly List<Wrote> _written = [];

        public IReadOnlyList<Wrote> Written => _written;

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "recording fake", false, false, false, false, HfBands.Names);

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(On20m);

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(
                new[] { RigValue.Unknown(field, "this fake answers nothing") });

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, byte? filterSlot = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("this fake does not do modes"));

        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            _written.Add(new Wrote(write, value));

            return Task.FromResult(RigWriteResult.NotSupported("this fake writes nothing"));
        }

        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "nothing in this file keys a transmitter (CLAUDE.md 0.2)");

        public void AbortCw()
        {
        }

        /// <summary>Keeps the compiler from warning that the events are unused.</summary>
        internal void NobodyRaisesThese()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
        }
    }

    private static (MainWindow Window, MainWindowViewModel Model, WritingRig Rig) Scene()
    {
        var model = Panel();
        var rig = new WritingRig();

        model.UseRigForTests(rig);

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1600 };

        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return (window, model, rig);
    }

    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        model.SelectedBand = model.Bands.First(
            b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }
}
