using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **A CHANGE THE RADIO ANNOUNCES LEAVES THE WINDOW EXACTLY AS THE SAME CHANGE CHOSEN IN THE APP
/// DOES** (work instruction 481, step 11 criterion 11.6, R95).
/// </summary>
/// <remarks>
/// <para>Tim, R95: *"A frequency change the radio announces leaves the window exactly as the same
/// change chosen in the app does - every panel's position and size, and every sentence. Unit 423
/// proved the privilege path; the announced path was never driven."*</para>
/// <para>**TWO WINDOWS, THE SAME SIZE, THE SAME SETTINGS AND LICENCE FIXTURE**
/// (<see cref="TheTopRowTuned.Open(double, double, LicenseClass, string)"/>), each told the same
/// starting place by the radio. Then window A is taken to the target the way the operator takes it
/// there in Hamlet - <c>TuneToCommand</c>, the one door a favorite, a story, a spot, a map dot and a
/// tape marker all go through, with a band chip first where the band changes - and the radio's
/// read-back of frequency and mode arrives after it. Window B is taken there by the radio alone:
/// frequency and mode arrive through <c>ApplyRigState</c>, the door the rig monitor posts every poll
/// and every transceive report through, and the one <c>TuningDoesNotSnapBackTests</c> drives.</para>
/// <para>**THE TRAINING RADIO IS NOT THE RIG HERE, AND THAT IS MEASURED RATHER THAN CHOSEN.** It
/// synthesizes Morse only and refuses any other mode, so it cannot announce USB-D; the state it
/// would have posted is built here from the same fields it posts (<c>RigField.Frequency</c>, and
/// <c>RigField.Mode</c> and <c>RigField.DataMode</c> as the IC-7300's poll reads them).</para>
/// <para>**NEITHER WINDOW HAS HEARD A MODE BEFORE THE CHANGE** (author's, overrulable, R85). On the
/// app path the mode is Hamlet's own mode-follow write, which HM-DEC-056 does not treat as the
/// operator's hand; a window primed with CW would read the read-back of that write as his hand,
/// which the app path never does. So both start knowing the frequency only, and the stand-down is
/// left to its own rule.</para>
/// <para>**AFTER LAYOUT SETTLES, EVERYTHING IS COMPARED**: every visible panel's bounds to the pixel,
/// every sentence in the neighborhood panel string for string with the width it was drawn at, the
/// tab, the mode chip on the rig face and the digital strip, and the privilege tone and color.
/// **NOTHING IS KEYED AND NO PORT IS OPENED.**</para>
/// </remarks>
public sealed class TheAnnouncedChangeMatchesTheChosenChangeTests
{
    /// <summary>The target: 14.0754 MHz, an FT8-adjacent data frequency on 20 m (plan §7's banked case).</summary>
    public const long Target = 14_075_400;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where both readings and every difference are printed.</param>
    public TheAnnouncedChangeMatchesTheChosenChangeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>From 14.050 in CW on 20 m to 14.0754 in USB-D: the announced window matches the chosen one.</summary>
    [AvaloniaFact]
    public void TheRadioTakingTheDialTo14_0754InUsbDLeavesTheWindowAsChoosingItDoes()
        => AssertTheSame(Drive("14.050 CW -> 14.0754 USB-D", "20 m", 14_050_000, "20 m", Target, CivMode.Usb, true));

    /// <summary>From 7.074 in USB-D on 40 m, across the band, to 14.0754 in USB-D.</summary>
    /// <remarks>Driven because the first pair was green at HEAD (work instruction 481 task 2).</remarks>
    [AvaloniaFact]
    public void FromFortyMetresTheAnnouncedBandChangeMatchesTheChosenOne()
        => AssertTheSame(Drive("7.074 USB-D -> 14.0754 USB-D", "40 m", 7_074_000, "20 m", Target, CivMode.Usb, true));

    /// <summary>From 14.0754 in USB-D back to 14.050 in CW.</summary>
    /// <remarks>Driven because the first pair was green at HEAD (work instruction 481 task 2).</remarks>
    [AvaloniaFact]
    public void BackToMorseTheAnnouncedChangeMatchesTheChosenOne()
        => AssertTheSame(Drive("14.0754 USB-D -> 14.050 CW", "20 m", Target, "20 m", 14_050_000, CivMode.Cw, false));

    /// <summary>
    /// **A TRACE THAT ASSERTS NOTHING**: the first pair again, with the announcing radio having
    /// reported CW before the change, as a real radio's poll always has.
    /// </summary>
    /// <remarks>
    /// The chosen window still starts knowing no mode, because on the app path the mode that
    /// arrives is Hamlet's own write. What this prints is what HM-DEC-056's stand-down does to the
    /// window when the radio's mode changes under Hamlet; whether R95 reaches that is a question for
    /// the owner, not for this unit (work instruction 481 section 4 of the report).
    /// </remarks>
    [AvaloniaFact]
    public void TraceTheAnnouncedChangeWhenTheRadioHadSaidCwFirst()
    {
        foreach (var size in new[] { TheTopRowTuned.DefaultSize, (1920.0, 1040.0) })
        {
            Drive(
                $"14.050 CW -> 14.0754 USB-D, the radio having said CW, {size.Item1} x {size.Item2}",
                "20 m", 14_050_000, "20 m", Target, CivMode.Usb, true, CivMode.Cw, size);
        }
    }

    /// <summary>Drives both windows through one change and returns every difference.</summary>
    private List<string> Drive(
        string label, string fromBand, long fromHz, string toBand, long toHz, CivMode toMode, bool toData,
        CivMode? announcedHeardFirst = null, (double Width, double Height)? size = null)
    {
        var (width, height) = size ?? TheTopRowTuned.DefaultSize;
        var (chosen, chosenPanel) = TheTopRowTuned.Open(width, height, LicenseClass.General);
        var (announced, announcedPanel) = TheTopRowTuned.Open(width, height, LicenseClass.General);

        try
        {
            foreach (var (window, panel) in new[] { (chosen, chosenPanel), (announced, announcedPanel) })
            {
                panel.SelectBandCommand.Execute(panel.Bands.First(b => b.Band.Name == fromBand));
                TheTopRowTuned.Settle(window);
                panel.TuneToCommand.Execute(fromHz);
                panel.ApplyRigState(
                    panel == announcedPanel && announcedHeardFirst is { } first
                        ? Report(fromHz, first, false)
                        : Report(fromHz, null, null));
                TheTopRowTuned.Settle(window);
            }

            var before = Read(chosen, chosenPanel);
            var beforeB = Read(announced, announcedPanel);
            var start = Differences(before, beforeB);

            _output.WriteLine($"==== {label}: at the start the two windows differ in {start.Count} line(s)");

            foreach (var line in start)
            {
                _output.WriteLine("   " + line);
            }

            // **A: CHOSEN IN THE APP.** The band chip where the band changes, then the tune every
            // favorite, story, spot and map dot goes through; then the radio's read-back of where it
            // is and what mode Hamlet's mode-follow put it in.
            if (toBand != chosenPanel.SelectedBand.Band.Name)
            {
                chosenPanel.SelectBandCommand.Execute(chosenPanel.Bands.First(b => b.Band.Name == toBand));
                TheTopRowTuned.Settle(chosen);
            }

            chosenPanel.TuneToCommand.Execute(toHz);
            TheTopRowTuned.Settle(chosen);
            chosenPanel.ApplyRigState(Report(toHz, toMode, toData));
            TheTopRowTuned.Settle(chosen);

            // **B: ANNOUNCED BY THE RADIO.** Nothing pressed: the frequency and the mode arrive as
            // the rig monitor posts them.
            announcedPanel.ApplyRigState(Report(toHz, toMode, toData));
            TheTopRowTuned.Settle(announced);

            var a = Read(chosen, chosenPanel);
            var b = Read(announced, announcedPanel);

            _output.WriteLine($"==== {label}: chosen (A)");
            Print(a);
            _output.WriteLine($"==== {label}: announced (B)");
            Print(b);

            var differences = Differences(a, b).Select(d => label + ": " + d).ToList();

            _output.WriteLine($"==== {label}: {differences.Count} difference(s)");

            foreach (var line in differences)
            {
                _output.WriteLine("   " + line);
            }

            return differences;
        }
        finally
        {
            chosen.Close();
            announced.Close();
        }
    }

    private static void AssertTheSame(List<string> differences)
        => Assert.True(differences.Count == 0, string.Join(Environment.NewLine, differences));

    /// <summary>What the radio posts: the frequency, and the mode with its data flag where one is given.</summary>
    private static RigState Report(long hz, CivMode? mode, bool? data)
    {
        var now = DateTime.UtcNow;
        var values = new List<RigValue>
        {
            RigValue.Known(RigField.Frequency, hz, CivValues.FrequencyText(hz), now, "CI-V 00 transceive"),
        };

        if (mode is { } m)
        {
            values.Add(RigValue.Known(RigField.Mode, (int)m, m.ToString().ToUpperInvariant(), now, "CI-V 04"));
        }

        if (data is { } d)
        {
            values.Add(RigValue.Known(RigField.DataMode, d ? 1 : 0, d ? "on" : "off", now, "CI-V 1A 06"));
        }

        return RigState.Empty.With(values);
    }

    /// <summary>
    /// One window, read as ordered <c>key = value</c> lines: every visible panel's bounds, every
    /// sentence in the neighborhood panel, the tab, the mode chips and the privilege tone and color.
    /// </summary>
    private static List<(string Key, string Value)> Read(Window window, MainWindowViewModel panel)
    {
        var lines = new List<(string, string)>();
        var top = TheTopRowTuned.Read(window, panel);

        lines.Add(("dial", panel.FrequencyHz.ToString(CultureInfo.InvariantCulture)));
        lines.Add(("band", panel.SelectedBand.Band.Name));
        lines.Add(("tab", panel.OperatingMode));
        lines.Add(("mode chip (rig face)", panel.RigModeText));
        lines.Add(("digital mode chips", string.Join(", ", panel.DigitalModeChips.Select(c => $"{c.Label} lit {c.IsLit} chosen {c.IsChosen}"))));
        lines.Add(("privilege tone", top.Tone.ToString()));
        lines.Add(("privilege color", top.Background));
        lines.Add(("privilege headline", panel.PrivilegeStatus.Headline));
        lines.Add(("privilege detail", panel.PrivilegeStatus.Detail));
        lines.Add(("mode line", panel.ModeLineText));
        lines.Add(("panel the neighborhood card", TheTopRowTuned.Box(top.Card)));
        lines.Add(("panel the sun map", TheTopRowTuned.Box(top.Map)));
        lines.Add(("panel the radio panel", TheTopRowTuned.Box(top.Rig)));

        var seen = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var box in window.GetVisualDescendants().OfType<CollapsiblePanel>())
        {
            if (!box.IsEffectivelyVisible)
            {
                continue;
            }

            var name = string.IsNullOrEmpty(box.Title) ? "(untitled)" : box.Title;
            seen[name] = seen.TryGetValue(name, out var n) ? n + 1 : 1;
            lines.Add(($"panel {name} #{seen[name]}", TheTopRowTuned.Box(TheTopRowTuned.Where(box, window))));
        }

        // **AND EVERY NAMED CONTROL ON SCREEN**, so a panel that is not a collapsible one - the
        // status bar, the tab strip, the rig face's own parts - cannot move unseen.
        var named = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var control in window.GetVisualDescendants().OfType<Control>())
        {
            if (control.Name is not { Length: > 0 } id || !control.IsEffectivelyVisible)
            {
                continue;
            }

            named[id] = named.TryGetValue(id, out var k) ? k + 1 : 1;
            lines.Add(($"named {id} #{named[id]}", TheTopRowTuned.Box(TheTopRowTuned.Where(control, window))));
        }

        lines.Add(("mode-follow stood down", panel.ModeFollowSuspended.ToString()));
        lines.Add(("status line", panel.StatusOnScreen));

        var card = window.GetVisualDescendants().OfType<Control>().First(c => c.Name == "TopRowCardScroller");
        var i = 0;

        foreach (var text in card.GetVisualDescendants().OfType<TextBlock>())
        {
            if (!text.IsEffectivelyVisible || string.IsNullOrEmpty(text.Text))
            {
                continue;
            }

            i++;
            lines.Add(($"sentence {i:00} {(text.Name is { Length: > 0 } nm ? nm : "(unnamed)")}",
                "\"" + text.Text + "\" drawn " + TheTopRowTuned.Px(text.Bounds.Width) + " x " + TheTopRowTuned.Px(text.Bounds.Height)));
        }

        return lines;
    }

    private static List<string> Differences(List<(string Key, string Value)> a, List<(string Key, string Value)> b)
    {
        var misses = new List<string>();
        var bByKey = b.GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First().Value, StringComparer.Ordinal);
        var aKeys = new HashSet<string>(a.Select(x => x.Key), StringComparer.Ordinal);

        foreach (var (key, value) in a)
        {
            if (!bByKey.TryGetValue(key, out var other))
            {
                misses.Add($"{key}: chosen {value}; announced has none");
            }
            else if (!string.Equals(value, other, StringComparison.Ordinal))
            {
                misses.Add($"{key}: chosen {value}; announced {other}");
            }
        }

        foreach (var (key, value) in b)
        {
            if (!aKeys.Contains(key))
            {
                misses.Add($"{key}: chosen has none; announced {value}");
            }
        }

        return misses;
    }

    private void Print(List<(string Key, string Value)> lines)
    {
        foreach (var (key, value) in lines)
        {
            _output.WriteLine($"   {key} = {value}");
        }
    }
}
