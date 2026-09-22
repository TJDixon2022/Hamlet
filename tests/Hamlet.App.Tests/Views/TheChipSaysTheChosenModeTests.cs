using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Criterion 7.5 and Tim's *none of it screams click me*: the chip filled is the mode he chose,
/// and every chip is a button that says where it goes** - work instruction 390 task 1.
/// </summary>
/// <remarks>
/// <para>**WHY 7.5 WAS TICKED AND STILL WRONG ON HIS SCREEN.** Unit 378 asserted
/// `DigitalModeChip.IsChosen` and the send line, both right; but the strip's FILL was drawn from
/// `IsLit` - the dial is inside that mode's block on the map - and Olivia's 20 m dial, 14.0715, is
/// inside PSK31's block. So under Olivia the PSK31 chip was filled and the Olivia chip was an
/// outline: two chips indicated, the filled one wrong. This asserts the fill itself.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). The one send driven here goes over
/// `FakePort` and `FakeSink`.</para>
/// </remarks>
public sealed class TheChipSaysTheChosenModeTests : IDisposable
{
    private const string Mine = "KC3QIS";

    /// <summary>Olivia's cited 20 m dial - inside PSK31's block, the case Tim saw.</summary>
    private const long OliviasDialOn20m = 14_071_500;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every chip is printed.</param>
    public TheChipSaysTheChosenModeTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-chip-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the temporary folder.</summary>
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

    /// <summary>
    /// **Under each of FT8, FT4, PSK31 and Olivia exactly one chip is filled and it is the chosen
    /// one** - with the dial sitting where Olivia's cited row puts it, inside PSK31's block.
    /// </summary>
    /// <param name="mode">The chip pressed.</param>
    [AvaloniaTheory]
    [InlineData("FT8")]
    [InlineData("FT4")]
    [InlineData("PSK31")]
    [InlineData("Olivia")]
    public void ExactlyOneChipIsFilledAndItIsTheChosenOne(string mode)
    {
        var model = Panel(mode);

        foreach (var chip in model.DigitalModeChips)
        {
            _output.WriteLine(
                mode + ": " + chip.Label.PadRight(7) + " filled " + chip.IsFilled + ", chosen "
                + chip.IsChosen + ", dial in its block " + chip.IsLit + ", hover [" + chip.Hover + "]");
        }

        var filled = model.DigitalModeChips.Where(c => c.IsFilled).ToList();

        Assert.Equal(mode, Assert.Single(filled).Label);
        Assert.Equal(mode, model.ChosenDigitalMode);

        // **TIM'S CASE BY NAME**: under Olivia, on Olivia's dial, the PSK31 chip is not filled -
        // and the Olivia chip, on its own cited dial, does not claim the dial is elsewhere.
        if (mode == "Olivia")
        {
            Assert.False(model.DigitalModeChips.Single(c => c.Label == "PSK31").IsFilled);

            var olivia = model.DigitalModeChips.Single(c => c.Label == "Olivia");

            Assert.True(olivia.IsFilledHere);
            Assert.DoesNotContain("not there", olivia.Hover, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// **Each chip is a button with the hand cursor and a hover naming where it goes, from the
    /// cited row; the filled one is the chosen one on the screen; and the label reads *tune to:*.**
    /// </summary>
    [AvaloniaFact]
    public void EveryChipIsAButtonThatSaysWhereItGoesAndTheLabelSaysTuneTo()
    {
        var window = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;

            model.ChooseDigitalModeCommand.Execute("Olivia");
            model.FrequencyHz = OliviasDialOn20m;

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var strip = TheTopRowTests.Named<Border>(window, "DigitalModeStrip");
            var label = strip.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.IsEffectivelyVisible);

            _output.WriteLine("the strip's label reads [" + label.Text + "]");

            Assert.Equal("tune to:", label.Text);

            var chips = TheTopRowTests.Named<ItemsControl>(window, "DigitalModeChipStrip")
                .GetVisualDescendants().OfType<Button>().ToList();

            Assert.Equal(DigitalModeChip.Labels.Count, chips.Count);

            var misses = new List<string>();
            var filled = new List<string>();

            foreach (var button in chips)
            {
                var chip = (DigitalModeChip)button.DataContext!;
                var tip = ToolTip.GetTip(button) as string ?? "";
                var fill = button.GetVisualDescendants().OfType<Border>()
                    .Any(b => b.IsEffectivelyVisible && ReferenceEquals(b.Background, ModePalette.DigitalFill));

                _output.WriteLine(
                    chip.Label.PadRight(7) + " cursor " + button.Cursor + ", enabled " + button.IsEffectivelyEnabled
                    + ", filled on screen " + fill + ", hover [" + tip + "]");

                if (fill)
                {
                    filled.Add(chip.Label);
                }

                if (button.Command is null || !button.IsEffectivelyEnabled)
                {
                    misses.Add(chip.Label + ": not a live button");
                }

                if (button.Cursor?.ToString() != new Cursor(StandardCursorType.Hand).ToString())
                {
                    misses.Add(chip.Label + ": the cursor is not the hand");
                }

                if (!tip.Contains(chip.Label, StringComparison.Ordinal))
                {
                    misses.Add(chip.Label + ": the hover [" + tip + "] does not name the mode");
                }

                var where = WhereItGoes(chip.Label);

                if (where is not null && !tip.StartsWith(where + " · ", StringComparison.Ordinal))
                {
                    misses.Add(chip.Label + ": the hover [" + tip + "] does not open with the cited " + where);
                }
            }

            Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
            Assert.Equal("Olivia", Assert.Single(filled));

            // **THE ONE TIM QUOTED**, spelled out rather than only derived.
            Assert.StartsWith(
                "14.070 · PSK31",
                ToolTip.GetTip(chips.Single(b => ((DigitalModeChip)b.DataContext!).Label == "PSK31")) as string,
                StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**The send line names the chosen mode after a fed send**: *N s of Olivia*.</summary>
    [AvaloniaFact]
    public void TheSendLineNamesTheChosenModeAfterAFedSend()
    {
        var olivia = WhatTheSendLineSaysAfterACq("Olivia");
        var psk31 = WhatTheSendLineSaysAfterACq("PSK31");

        _output.WriteLine("Olivia: [" + olivia + "]");
        _output.WriteLine("PSK31 : [" + psk31 + "]");

        Assert.Matches(@" - \d+(\.\d)? s of Olivia\.$", olivia);
        Assert.DoesNotContain("PSK31", olivia, StringComparison.Ordinal);
        Assert.Matches(@" - \d+(\.\d)? s of PSK31\.$", psk31);
    }

    // -------------------------------------------------------------------------

    /// <summary>The frequency a chip's press goes to on 20 m, from the cited rows, or null.</summary>
    private static string? WhereItGoes(string label)
    {
        if (label == "Olivia")
        {
            return "14.073";
        }

        return DigitalCallingFrequencies.Find("20 m", label) is { } block
            ? (block.JumpHz / 1_000_000.0).ToString("0.000#", CultureInfo.InvariantCulture)
            : null;
    }

    private string WhatTheSendLineSaysAfterACq(string mode)
    {
        using var telemetry = new JsonlTelemetry(_folder, "390-" + mode, _ => true);

        var model = Panel(mode, telemetry);

        model.TapForTests = new Hamlet.RadioEngine.Audio.AudioTap();

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        model.SendCallToAnyoneCommand.Execute(null);

        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, sink.TimesCalled);

        return model.DigitalSendLine;
    }

    private static MainWindowViewModel Panel(string mode, JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(
            b => b.Band.LowHz <= OliviasDialOn20m && b.Band.HighHz >= OliviasDialOn20m);
        model.FrequencyHz = OliviasDialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }
}
