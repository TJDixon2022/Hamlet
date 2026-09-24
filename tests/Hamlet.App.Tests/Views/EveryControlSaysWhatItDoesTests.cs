using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **EVERY CONTROL ON THE CW TAB AND THE BAND ROW SAYS, ON HOVER, WHAT IT DOES**
/// (work instruction 422 task 2, step 6 criterion 6.4; HM-DEC-080, §0.5.1).
/// </summary>
/// <remarks>
/// <para>**EACH CONTROL IS NAMED HERE, ONE BY ONE**, so a failure says which control
/// has nothing to say. The names are the inventory
/// `WhatEveryControlSaysOnHoverTests` printed, found in the built window and never
/// by reading the markup as text.</para>
/// <para>**THE LIST IS CLOSED BOTH WAYS.** A named control that is no longer there
/// fails, and so does a control that appears on the CW tab or the band row without
/// being named here, so a button added later without hover text cannot slip past by
/// not being on the list.</para>
/// <para>**A MARK THAT HOLDS NO SENTENCE IS NOT DRAWN AND CANNOT BE HOVERED**
/// (`HintMarkControl` measures to nothing), so in a state where it holds nothing it is
/// reported and not failed. Every other control fails on a missing or blank tip
/// whether or not the state draws it, because its tip is its own and not the
/// state's.</para>
/// </remarks>
public sealed class EveryControlSaysWhatItDoesTests
{
    /// <summary>The CW tab, in the order the window holds it.</summary>
    internal static readonly string[] OnTheCwTab =
    {
        "TransmitButton",
        "\"Clear\" (ComposeClearCommand)",
        "\"CQ\"",
        "\"RST\"",
        "\"73\"",
        "mark ? \"CQ tells the band you are looking for a …\"",
        "send line",
        "header of CW terminal",
        "\"Clear\" (ClearTerminalCommand)",
        "mark ? \"what the radio is hearing, as it arrives\"",
        "mark ⊣ \"Press this whenever you can hear a stati…\"",
        "\"I hear a station\"",
        "ReceiveHelpOfferButton",
        "\"No thanks\"",
        "mark ⊣ \"a dimmed character is one Hamlet is not …\"",
    };

    /// <summary>The band row, in the order the window holds it.</summary>
    internal static readonly string[] OnTheBandRow =
    {
        "\"Stop the scan\"",
        "\"STOP TRANSMITTING\"",
        "port list",
        "connect button",
        "header of Neighborhood map",
        "mark ? \"hover a dot to see who it is · click a d…\"",
        "mark ? MapLegendMark",
        "mark ⊣ GreenZoneRuleOfThumbMark",
        "neighborhood strip",
        "GreenZoneBestBet",
        "Button with no words",
        "saved spot 7030000",
        "forget 7030000",
        "saved spot 14050000",
        "forget 14050000",
        "\"Use the FCC value\"",
        "\"Keep mine\" (KeepMyLicenseClassCommand)",
        "\"Use the looked-up grid\"",
        "\"Keep mine\" (KeepMyGridCommand)",
        "rig face, the frequency digits",
        "save star",
        "mark # LinkCheckMark",
        "DigitalTransmitDriveBox",
        "mark ? DigitalTransmitDriveTip",
        "DigitalPsk31PowerLine",
        "DigitalPsk31PowerAccept",
        "DigitalPsk31PowerDecline",
        "band 80 m",
        "band 40 m",
        "band 30 m",
        "band 20 m",
        "band 17 m",
        "band 15 m",
        "band 10 m",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each control and its tip are printed.</param>
    public EveryControlSaysWhatItDoesTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Not connected: every named control has hover text.**</summary>
    [AvaloniaFact]
    public void NotConnectedEveryNamedControlSaysWhatItDoes()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            Judge("not connected", TheControlsTimCanPress.Find(window, panel));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Connected: every named control has hover text, and the connect button's
    /// follows its words.** Connected through the button's own command to the training
    /// radio, the way `TheBarSaysWhatHamletCouldNotDoTests` does.
    /// </summary>
    [AvaloniaFact]
    public async Task ConnectedEveryNamedControlSaysWhatItDoes()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            var before = Connect(TheControlsTimCanPress.Find(window, panel));

            await TheControlsTimCanPress.ConnectAsync(window, panel);
            Assert.True(panel.IsConnected, "the training radio did not connect");

            var all = TheControlsTimCanPress.Find(window, panel);
            var after = Connect(all);

            _output.WriteLine("not connected: " + Show(before.Tip));
            _output.WriteLine("connected    : " + Show(after.Tip));

            Judge("connected", all);

            Assert.True(
                !string.IsNullOrWhiteSpace(after.Tip) && after.Tip != before.Tip,
                "the connect button reads \"" + panel.ConnectButtonText + "\" and its tip did not "
                + "change with its words: " + Show(after.Tip));
        }
        finally
        {
            await TheControlsTimCanPress.DisconnectAsync(window, panel);
            window.Close();
        }
    }

    /// <summary>
    /// **The star says what the star does, not what the digits do.** Its hover text is
    /// read with the pointer on the star's own target in the rig face.
    /// </summary>
    [AvaloniaFact]
    public void TheStarSaysItSavesAndNotHowToTune()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            var all = TheControlsTimCanPress.Find(window, panel);
            var star = all.Single(c => c.Label == "save star");
            var digits = all.Single(c => c.Label == "rig face, the frequency digits");

            _output.WriteLine("star  : " + Show(star.Tip));
            _output.WriteLine("digits: " + Show(digits.Tip));

            Assert.True(
                !string.IsNullOrWhiteSpace(star.Tip) && star.Tip != digits.Tip,
                "with the pointer on the save star the face says " + Show(star.Tip)
                + ", which is the digits' sentence and says nothing about saving");
        }
        finally
        {
            window.Close();
        }
    }

    private void Judge(string state, IReadOnlyList<PressableControl> all)
    {
        var misses = new List<string>();

        Check(TheControlsTimCanPress.CwTab, OnTheCwTab);
        Check(TheControlsTimCanPress.BandRow, OnTheBandRow);

        Assert.True(
            misses.Count == 0,
            state + ": " + misses.Count + " control(s) say nothing on hover or are not where the list says:"
            + Environment.NewLine + string.Join(Environment.NewLine, misses));

        void Check(string region, string[] names)
        {
            var here = all.Where(c => c.Region == region).ToList();
            var keys = here.Select(Key).ToList();

            foreach (var name in names)
            {
                var found = here.Where(c => Key(c) == name).ToList();

                if (found.Count == 0)
                {
                    misses.Add(region + " | " + name + " | not found in the window");
                    continue;
                }

                foreach (var c in found)
                {
                    _output.WriteLine(state + " | " + region + " | " + name + " | " + Show(c.Tip));

                    if (!string.IsNullOrWhiteSpace(c.Tip))
                    {
                        continue;
                    }

                    if (name.StartsWith("mark ", StringComparison.Ordinal) && !c.OnScreen)
                    {
                        _output.WriteLine("  (a mark holding no sentence in this state is not drawn and cannot be hovered)");
                        continue;
                    }

                    misses.Add(region + " | " + name + " | no tip | runs " + c.Command);
                }
            }

            foreach (var extra in keys.Distinct().Where(k => !names.Contains(k)))
            {
                misses.Add(region + " | " + extra + " | on screen and not named by this test");
            }
        }
    }

    private static string Key(PressableControl c)
        => c.Command == "ToggleConnectCommand" ? "connect button" : c.Label;

    private static PressableControl Connect(IReadOnlyList<PressableControl> all)
        => all.Single(c => c.Command == "ToggleConnectCommand");

    private static string Show(string? tip)
        => string.IsNullOrWhiteSpace(tip) ? "none" : "\"" + tip.Replace("\n", " / ", StringComparison.Ordinal) + "\"";
}
