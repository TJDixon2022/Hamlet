using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 281, task 2: advice waits behind a mark, and a fault does not.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: text only where he intentionally hovers, with
/// one exception — **a fault speaks unasked.**</para>
/// <para>**THE POINT OF THE TEST IS THAT NOTHING WAS DELETED.** Moving a sentence
/// behind a mark and deleting it look identical on the screen, and only one of them
/// is allowed (§0.0, HM-DEC-092). This asserts that each sentence Settings stopped
/// showing is still in the window, one hover away.</para>
/// </remarks>
public sealed class AdviceWaitsToBeAskedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltip inventory is printed.</param>
    public AdviceWaitsToBeAskedTests(ITestOutputHelper output) => _output = output;

    /// <summary>A machine with nothing to record from.</summary>
    private sealed class NoDevices : IAudioDevices
    {
        public IReadOnlyList<AudioDevice> List() => Array.Empty<AudioDevice>();
    }

    /// <summary>**Every sentence Settings stopped showing is still one hover away.**</summary>
    [AvaloniaFact]
    public void TheAdviceSettingsStoppedShowingIsStillThere()
    {
        var window = Settings(out _);
        var tips = Tips(window);

        _output.WriteLine(tips.Count + " marks on the Settings window:");

        foreach (var tip in tips.OrderByDescending(t => t.Length))
        {
            _output.WriteLine("  " + tip.Length.ToString().PadLeft(4) + "  " + tip);
        }

        // A phrase from each sentence that left the screen. Phrases rather than
        // whole sentences, because two of them are composed at run time and carry
        // a live number in the middle.
        var moved = new[]
        {
            "never written to telemetry and never uploaded",
            "Listening is never restricted by any of this",
            "It never restricts tuning or receiving",
            "which Hamlet chooses for you when it",
            "what carries FT8 out of the computer",
            "Set it to match the radio and the decoder starts in the right place",
            "Hamlet has never heard you copy anything",
            "It tries once and then leaves it to you",
            "getting it wrong is why a beginner hears nothing",
            "Delete one and the scanner will not go there",
            "refreshing pauses while the window is out of sight",
            "Explore ▸ Refresh spots",
            "whether that person is probably still on that frequency",
            "often well over an hour",
            "Weaker evidence they are still calling",
            "sit on one frequency for the whole event",
            "owed knowing who is calling",
            "no machine identifier are ever written",
            "then the oldest are deleted",
        };

        var missing = moved
            .Where(phrase => !tips.Any(t => t.Contains(phrase, StringComparison.Ordinal)))
            .ToList();

        Assert.True(
            missing.Count == 0,
            "these sentences left the screen and are not behind any mark, so they "
            + "were deleted rather than moved: " + string.Join(" | ", missing));
    }

    /// <summary>**The mark carries the kind in words, never in the glyph alone.**</summary>
    /// <remarks>
    /// §0.6. A shape is a color's cousin: somebody who does not recognise it has no
    /// second way in, so the tooltip leads with what kind of thing this is.
    /// </remarks>
    [AvaloniaFact]
    public void EveryKindIsToldApartWithoutTheGlyph()
    {
        var kinds = new[] { HintKind.Tip, HintKind.Measurement, HintKind.Boundary };

        Assert.Equal(3, kinds.Select(HintMarkControl.Glyph).Distinct().Count());
        Assert.Equal(3, kinds.Select(HintMarkControl.Word).Distinct().Count());

        var mark = new HintMarkControl { Kind = HintKind.Boundary, Text = "the edge" };

        mark.Measure(new Avalonia.Size(100, 100));

        var tip = ToolTip.GetTip(mark) as string;

        Assert.NotNull(tip);
        Assert.StartsWith(HintMarkControl.Word(HintKind.Boundary), tip, StringComparison.Ordinal);
        Assert.EndsWith("the edge", tip, StringComparison.Ordinal);
    }

    /// <summary>**A mark holding nothing is not a hover target at all.**</summary>
    [AvaloniaFact]
    public void AnEmptyMarkIsNotDrawnAndHoldsNoTooltip()
    {
        var mark = new HintMarkControl { Kind = HintKind.Tip, Text = "   " };

        mark.Measure(new Avalonia.Size(100, 100));

        Assert.Equal(default, mark.DesiredSize);
        Assert.Null(ToolTip.GetTip(mark));
    }

    /// <summary>**A fault stays in words on the screen and does not go behind a mark.**</summary>
    /// <remarks>
    /// The single exception to the ruling. `AudioDeviceNote` used to carry both a
    /// fault and a tip in one property; with the device list empty the fault is what
    /// it says, and it must be readable without hovering anything.
    /// </remarks>
    [AvaloniaFact]
    public void AFaultIsReadWithoutHovering()
    {
        var window = Settings(out var panel, new NoDevices());

        Assert.True(panel.HasAudioDeviceFault, "the fixture was meant to have no capture device");

        var onScreen = HowMuchTheApplicationSaysTests.Shown(window).ToList();

        Assert.Contains(
            onScreen,
            t => t.Contains("cannot see a recording device", StringComparison.Ordinal));

        // And the tip half is not also on the screen, or the split bought nothing.
        Assert.DoesNotContain(
            onScreen,
            t => t.Contains("which Hamlet chooses for you", StringComparison.Ordinal));
    }

    /// <summary>Every tooltip a mark on this window is holding.</summary>
    /// <param name="window">The realized window.</param>
    /// <returns>The tooltips, in tree order.</returns>
    private static List<string> Tips(Window window)
        => window.GetVisualDescendants()
            .OfType<HintMarkControl>()
            .Select(m => ToolTip.GetTip(m) as string)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t!)
            .ToList();

    /// <summary>The Settings window, realized.</summary>
    /// <param name="panel">The view model behind it.</param>
    /// <param name="devices">The capture devices to offer, or null for this machine.</param>
    /// <returns>The shown window.</returns>
    private static Window Settings(out SettingsViewModel panel, IAudioDevices? devices = null)
    {
        panel = new SettingsViewModel(new AppSettings(), null, devices);

        var window = new SettingsWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
    }
}
