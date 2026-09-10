using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 303 task 4: **a lost audio device is not silent.**
/// </summary>
/// <remarks>
/// <para>**WHAT HAPPENED AT THE RADIO.** A reboot left `settings.json` without its
/// transmit audio device, so pressing CQ did nothing and `transmit_readiness` refused
/// 56 times while the screen said nothing at all. Restoring the device by hand fixed
/// it. **A refusal the operator cannot see is a button that appears broken.**</para>
/// <para>**THE INSTRUCTION ASKS WHAT THE LOAD PATH DOES BEFORE ANYTHING CHANGES**, so
/// the first test here reports rather than asserts a repair, and it is what found the
/// erasing path.</para>
/// </remarks>
public sealed class Unit303LostDeviceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the load path's behaviour is printed.</param>
    public Unit303LostDeviceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the load path does today with a device that is gone.**</summary>
    /// <remarks>
    /// **A REPORT, NOT A REPAIR.** `ChooseEndpoint` returns null where the saved id
    /// names no endpoint, which is right and is what the field's own remarks promise:
    /// nothing is chosen on the operator's behalf. What matters is what happens to
    /// the **saved id** afterwards.
    /// </remarks>
    [Fact]
    public void WhatTheLoadPathDoesWithADeviceThatIsGone()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        _output.WriteLine("saved id      : " + settings.AudioOutputDeviceId);
        _output.WriteLine(
            "endpoints now : " + string.Join(
                ", ", panel.TransmitEndpoints.Select(e => e.Name)));
        _output.WriteLine(
            "selected      : " + (panel.TransmitEndpoint?.Name ?? "(nothing)"));
        _output.WriteLine("saved id after: " + settings.AudioOutputDeviceId);

        // **CONSTRUCTION ALONE KEEPS IT**, because it assigns the backing field.
        Assert.Null(panel.TransmitEndpoint);
        Assert.False(string.IsNullOrEmpty(settings.AudioOutputDeviceId));
    }

    /// <summary>**Opening Settings must not erase a device that is merely absent.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE ERASING PATH AND IT IS WHY THE FILE LOST THE DEVICE.**
    /// The transmit picker is bound two-way to `TransmitEndpoint`. When the control
    /// realizes with a selection that is not among its items, the binding writes
    /// **null** back - and `OnTransmitEndpointChanged` puts that null straight into
    /// `settings.AudioOutputDeviceId` and saves the file.</para>
    /// <para>**SO A SOUND CARD THAT IS ASLEEP FOR ONE BOOT IS FORGOTTEN FOR EVER**,
    /// and the operator is left with a CQ button that does nothing and a settings
    /// screen that shows an empty box, with no way to know a device was ever named.
    /// **A device that is absent is not a device the operator unchose.**</para>
    /// </remarks>
    [AvaloniaFact]
    public void OpeningSettingsDoesNotEraseAnAbsentDevice()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        var window = new SettingsWindow { DataContext = panel };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        try
        {
            _output.WriteLine(
                "after opening Settings, saved id: "
                + (settings.AudioOutputDeviceId ?? "(erased)"));

            Assert.False(
                string.IsNullOrEmpty(settings.AudioOutputDeviceId),
                "opening Settings erased a transmit device that was merely absent, "
                + "so a sound card asleep for one boot is forgotten for ever");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**A null selection does not erase an absent device either.**</summary>
    /// <remarks>
    /// <para>**THE SUSPECTED ERASING PATH, DRIVEN DIRECTLY, AND IT DOES NOT ERASE.**
    /// `OnTransmitEndpointChanged` writes `value?.Id` and saves, so the theory was
    /// that any null selection wipes the saved device. **It does not reach that
    /// handler at all**: where the device is missing the selection is *already* null,
    /// and the generated setter stops on equality before the handler runs.</para>
    /// <para>**SO THE FILE'S LOST DEVICE IS NOT EXPLAINED BY THIS CODE.** That is
    /// reported rather than papered over: a guard was written for this, measured to
    /// guard nothing reachable, and removed.</para>
    /// </remarks>
    [Fact]
    public void ClearingThePickerDoesNotEraseAnAbsentDevice()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        // What any binding, command or code path does when there is no selection.
        panel.TransmitEndpoint = null;

        _output.WriteLine(
            "after the selection went null: "
            + (settings.AudioOutputDeviceId ?? "(erased)"));

        Assert.False(
            string.IsNullOrEmpty(settings.AudioOutputDeviceId),
            "a null selection erased a transmit device that was merely absent");
    }

    /// <summary>**Choosing a real device does replace the saved one.**</summary>
    /// <remarks>
    /// **KEEPING AN ABSENT DEVICE MUST NOT WEDGE THE PICKER.** The guard above holds
    /// only a null selection; picking a device is still exactly as it was.
    /// </remarks>
    [Fact]
    public void ChoosingARealDeviceStillReplacesTheSavedOne()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        panel.TransmitEndpoint = Present()[0];

        _output.WriteLine("after picking Speakers: " + settings.AudioOutputDeviceId);
        _output.WriteLine(
            "warning now: " + (panel.HasTransmitDeviceWarning
                ? panel.TransmitDeviceWarning : "(none)"));

        Assert.Equal(Present()[0].Id, settings.AudioOutputDeviceId);
        Assert.False(panel.HasTransmitDeviceWarning);
    }

    /// <summary>**The operator is told which device is missing, on screen.**</summary>
    /// <remarks>
    /// **NOT ONLY IN A REFUSAL REASON BURIED IN TELEMETRY.** `transmit_readiness`
    /// refused 56 times and the screen said nothing; the button did nothing, the Send
    /// line never changed and the stop control never armed.
    /// </remarks>
    [Fact]
    public void TheOperatorIsToldWhichDeviceIsMissing()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        _output.WriteLine("says: " + panel.TransmitDeviceWarning);

        Assert.True(
            panel.HasTransmitDeviceWarning,
            "a transmit device named in settings and absent from the machine "
            + "produced no statement anywhere the operator can see");

        // **IT NAMES WHAT IS MISSING** rather than saying something went wrong.
        Assert.Contains(
            "a-device-that-is-gone",
            panel.TransmitDeviceWarning,
            StringComparison.Ordinal);
    }

    /// <summary>**The warning is on the realized window, not just in a property.**</summary>
    /// <remarks>
    /// **A VIEW MODEL PROPERTY NOBODY BINDS IS INVISIBLE**, which is the same fault
    /// this task exists to fix wearing different clothes. So the window is built and
    /// the sentence is read off it.
    /// </remarks>
    [AvaloniaFact]
    public void TheWarningIsOnTheRealizedWindow()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        var window = new SettingsWindow { DataContext = panel };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        try
        {
            var shown = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(b => b.Name == "TransmitDeviceMissing");

            _output.WriteLine(
                "on screen: " + (shown?.Text ?? "(the block is not there)"));
            _output.WriteLine(
                "visible  : " + (shown?.IsEffectivelyVisible.ToString() ?? "n/a"));

            Assert.True(shown is not null, "the warning is not on the window");

            Assert.True(
                shown!.IsEffectivelyVisible,
                "the warning is on the window but not visible");

            Assert.Contains(
                "a-device-that-is-gone", shown.Text ?? "", StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**Nothing is substituted.**</summary>
    /// <remarks>
    /// **NAMING THE WRONG SOUND CARD WOULD TRANSMIT INTO NOTHING WHILE LOOKING
    /// FINE**, which is the worst outcome available here and is why the field's own
    /// remarks say the send refuses rather than Hamlet picking.
    /// </remarks>
    [Fact]
    public void NothingIsSubstituted()
    {
        var settings = Saved("{0.0.0.00000000}.{a-device-that-is-gone}");

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        _output.WriteLine(
            "selected: " + (panel.TransmitEndpoint?.Name ?? "(nothing)"));

        Assert.Null(panel.TransmitEndpoint);
    }

    /// <summary>**A device that is present is selected and says nothing.**</summary>
    [Fact]
    public void APresentDeviceIsSelectedAndSaysNothing()
    {
        var settings = Saved(Present()[1].Id);

        var panel = new SettingsViewModel(
            settings, null, null, () => Present());

        _output.WriteLine(
            "selected: " + (panel.TransmitEndpoint?.Name ?? "(nothing)"));
        _output.WriteLine(
            "warning : " + (panel.HasTransmitDeviceWarning
                ? panel.TransmitDeviceWarning : "(none)"));

        Assert.NotNull(panel.TransmitEndpoint);
        Assert.False(panel.HasTransmitDeviceWarning);
    }

    /// <summary>Settings naming one transmit device.</summary>
    private static AppSettings Saved(string id)
        => new() { AudioOutputDeviceId = id, ReconnectOnStartup = false };

    /// <summary>The devices this machine has, for the fake.</summary>
    private static IReadOnlyList<RenderEndpoint> Present()
        => new[]
        {
            new RenderEndpoint(
                "{0.0.0.00000000}.{speakers}", "Speakers", true, 48_000, 2, 16, "shared"),
            new RenderEndpoint(
                "{0.0.0.00000000}.{usb-codec}", "USB Audio CODEC", false, 48_000, 2, 16, "shared"),
        };
}
