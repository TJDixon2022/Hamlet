using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>One control Tim can press or hover on the CW tab or the band row.</summary>
/// <param name="Region">`CW tab` or `band row`.</param>
/// <param name="Label">Its name, or its words where it has no name.</param>
/// <param name="Command">What it is bound to.</param>
/// <param name="Tip">Its hover text as the window resolves it, or null.</param>
/// <param name="OnScreen">Whether it is drawn in the state it was found in.</param>
internal sealed record PressableControl(
    string Region, string Label, string Command, string? Tip, bool OnScreen);

/// <summary>
/// **EVERY CONTROL ON THE CW TAB AND THE BAND ROW, FOUND IN THE BUILT WINDOW**
/// (work instruction 422, step 6 criterion 6.4).
/// </summary>
/// <remarks>
/// <para>**WHAT THE TWO PLACES ARE, READ OFF THE TREE.** The CW tab is `CwWorkspace`,
/// the grid `IsCwMode` shows: the send panel and the CW terminal. The band row is
/// what sits above the divider and is the same in every mode: the strip at the
/// header's right end that holds the port and Connect, `BandRow` (the neighborhood
/// card, the sun map and the rig face with its star) and `BandPills`, the band
/// buttons.</para>
/// <para>**PRESSABLE MEANS** a button of any kind, a combo box, a check box, a
/// switch, a spinner, a text box, anything carrying a command, the circled marks
/// that exist to be hovered, and the two drawn controls that take a press: the rig
/// face's star and the neighborhood strip. A template's own parts (a spinner's
/// arrows, a scroll bar) are the control's and are not listed apart from it.</para>
/// </remarks>
internal static class TheControlsTimCanPress
{
    /// <summary>The CW tab.</summary>
    public const string CwTab = "CW tab";

    /// <summary>The band row.</summary>
    public const string BandRow = "band row";

    /// <summary>The fixture: licensed, two saved spots, nothing reaching the network.</summary>
    public static AppSettings Fixture()
    {
        var settings = TheTopRowTests.FixtureSettings();
        var at = new DateTime(2026, 9, 22, 20, 0, 0, DateTimeKind.Utc);

        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 7_030_000, Name = "the QRP spot", Mode = "CW", BandName = "40 m", SavedUtc = at });
        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_050_000, Name = "slow CW on Sunday", Mode = "CW", BandName = "20 m", SavedUtc = at });

        return settings;
    }

    /// <summary>Opens the window on the CW tab, not connected.</summary>
    public static (Window Window, MainWindowViewModel Panel) Open()
    {
        var panel = new MainWindowViewModel(Fixture(), null)
        {
            OperatingMode = "CW",
        };

        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 900 };

        window.Show();
        Settle(window);

        return (window, panel);
    }

    /// <summary>Connects the training radio the way the button does.</summary>
    public static async Task ConnectAsync(Window window, MainWindowViewModel panel)
    {
        panel.SelectedPort = MainWindowViewModel.TrainingRadio;

        if (!panel.IsConnected)
        {
            await panel.ToggleConnectCommand.ExecuteAsync(null);
        }

        Settle(window);
    }

    /// <summary>Disconnects, so no timer outlives the test.</summary>
    public static async Task DisconnectAsync(Window window, MainWindowViewModel panel)
    {
        if (panel.IsConnected)
        {
            await panel.ToggleConnectCommand.ExecuteAsync(null);
        }

        Settle(window);
    }

    /// <summary>Lets the layout settle so every template is realized.</summary>
    public static void Settle(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            window.UpdateLayout();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>Every pressable control in both places, in tree order.</summary>
    public static IReadOnlyList<PressableControl> Find(Window window, MainWindowViewModel panel)
    {
        var commands = CommandNames(panel);
        var found = new List<PressableControl>();

        var connect = window.GetVisualDescendants().OfType<Button>()
            .Single(b => b.Classes.Contains("hm-connect"));

        var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
        var header = (Visual)connect.GetVisualParent()!;
        var bandRow = TheTopRowTests.Named<Control>(window, "BandRow");
        var pills = TheTopRowTests.Named<ItemsControl>(window, "BandPills");

        foreach (var c in Walk(cw))
        {
            found.AddRange(Describe(window, CwTab, c, commands));
        }

        foreach (var root in new[] { header, bandRow, pills })
        {
            foreach (var c in Walk(root))
            {
                found.AddRange(Describe(window, BandRow, c, commands));
            }
        }

        // Two controls with the same words in one place are told apart by what they run.
        var twice = found.GroupBy(c => (c.Region, c.Label)).Where(g => g.Count() > 1)
            .Select(g => g.Key).ToHashSet();

        return found
            .Select(c => twice.Contains((c.Region, c.Label))
                ? c with { Label = c.Label + " (" + c.Command + ")" }
                : c)
            .ToList();
    }

    /// <summary>The tip text, whatever form the window holds it in.</summary>
    public static string? TipOf(Control control)
        => ToolTip.GetTip(control) switch
        {
            null => null,
            string s => s,
            TextBlock t => t.Text,
            object o => o.ToString(),
        };

    private static IEnumerable<Control> Walk(Visual root)
    {
        foreach (var child in root.GetVisualChildren())
        {
            if (child is not Control c)
            {
                continue;
            }

            if (IsPressable(c))
            {
                yield return c;
            }

            // A template's own parts belong to the control and are not listed apart.
            if (c is NumericUpDown or ComboBox or TextBox or ScrollBar
                or HintMarkControl or RigDisplayControl or NeighborhoodMapControl)
            {
                continue;
            }

            foreach (var inner in Walk(c))
            {
                yield return inner;
            }

            if (c is Popup { Child: { } popped })
            {
                if (IsPressable(popped))
                {
                    yield return popped;
                }

                foreach (var inner in Walk(popped))
                {
                    yield return inner;
                }
            }
        }
    }

    private static bool IsPressable(Control c)
        => c is Button or ComboBox or NumericUpDown or TextBox or HintMarkControl
            or RigDisplayControl or NeighborhoodMapControl
            || c.GetType().GetProperty("Command") is { } p
                && typeof(ICommand).IsAssignableFrom(p.PropertyType)
                && p.GetValue(c) is not null;

    private static IEnumerable<PressableControl> Describe(
        Window window, string region, Control c, IReadOnlyDictionary<ICommand, string> commands)
    {
        var on = c.IsEffectivelyVisible;

        switch (c)
        {
            case RigDisplayControl rig:
                yield return new PressableControl(
                    region, "rig face, the frequency digits", "FrequencyHz (the wheel)", TipOf(rig), on);
                yield return new PressableControl(
                    region, "save star", Named(rig.ToggleFavoriteCommand, commands), StarTip(window, rig), on);
                yield break;

            case NeighborhoodMapControl map:
                yield return new PressableControl(
                    region, "neighborhood strip",
                    Named(map.TuneCommand, commands) + ", " + Named(map.SelectCommand, commands),
                    TipOf(map), on);
                yield break;

            case HintMarkControl mark:
                var text = (mark.Text ?? "").Trim();
                yield return new PressableControl(
                    region,
                    "mark " + HintMarkControl.Glyph(mark.Kind) + " "
                        + (mark.Name is { Length: > 0 } n ? n : Quote(text, 40)),
                    "none, it is hovered",
                    TipOf(mark) ?? (text.Length == 0 ? null : HintMarkControl.Word(mark.Kind) + " —  " + text),
                    on && text.Length > 0);
                yield break;

            case ComboBox:
                yield return new PressableControl(region, "port list", "SelectedPort (the choice)", TipOf(c), on);
                yield break;

            case NumericUpDown spin:
                yield return new PressableControl(
                    region, spin.Name is { Length: > 0 } sn ? sn : "spinner",
                    "TransmitDrivePercent (the value)", TipOf(c), on);
                yield break;

            case TextBox:
                yield return new PressableControl(
                    region, "send line", "Transmit.OwnWords.Message (the text)", TipOf(c), on);
                yield break;
        }

        var command = c.GetType().GetProperty("Command")?.GetValue(c) as ICommand;

        yield return new PressableControl(region, LabelOf(c), CommandOf(c, command, commands), TipOf(c), on);
    }

    private static string LabelOf(Control c)
    {
        if (c.Name == "PART_Header" && c.TemplatedParent is CollapsiblePanel panel)
        {
            return "header of " + panel.Title;
        }

        if (c.DataContext is BandButtonViewModel band && c.Classes.Contains("hm-band"))
        {
            return "band " + band.Band.Name;
        }

        if (c.DataContext is FavoriteChip chip)
        {
            var what = c.Classes.Contains("hm-favchip") ? "saved spot " : "forget ";
            return what + chip.Favorite.FrequencyHz.ToString(CultureInfo.InvariantCulture);
        }

        if (c.Name is { Length: > 0 } name)
        {
            return name;
        }

        // A button whose words are bound and empty in this state is named by what it runs.
        return c is ContentControl { Content: string { Length: > 0 } words }
            ? "\"" + words + "\""
            : c.GetType().Name + " with no words";
    }

    private static string CommandOf(
        Control c, ICommand? command, IReadOnlyDictionary<ICommand, string> commands)
    {
        if (c.Name == "PART_Header")
        {
            return "IsExpanded (folds the panel)";
        }

        return command is null ? "none" : Named(command, commands);
    }

    private static string Named(ICommand? command, IReadOnlyDictionary<ICommand, string> commands)
        => command is null
            ? "none"
            : commands.TryGetValue(command, out var name) ? name : command.GetType().Name;

    /// <summary>Every command the window's view models hold, by where they hold it.</summary>
    private static Dictionary<ICommand, string> CommandNames(MainWindowViewModel panel)
    {
        var names = new Dictionary<ICommand, string>(ReferenceEqualityComparer.Instance);

        void Take(object? owner, string prefix)
        {
            if (owner is null)
            {
                return;
            }

            foreach (var p in owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.Name.EndsWith("Command", StringComparison.Ordinal)
                    || !typeof(ICommand).IsAssignableFrom(p.PropertyType)
                    || p.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (p.GetValue(owner) is ICommand cmd)
                {
                    names.TryAdd(cmd, prefix + p.Name);
                }
            }
        }

        Take(panel, "");
        Take(panel.Transmit, "Transmit.");
        Take(panel.Scan, "Scan.");
        Take(panel.AutoCall, "AutoCall.");

        return names;
    }

    /// <summary>
    /// **THE STAR'S HOVER TEXT IS READ WITH THE POINTER ON THE STAR.** The star is drawn
    /// inside the rig face and has no element of its own, so what Tim reads there is
    /// whatever the face says while the pointer is over the star's own target.
    /// </summary>
    private static string? StarTip(Window window, RigDisplayControl rig)
    {
        var field = typeof(RigDisplayControl).GetField("_starRect", BindingFlags.NonPublic | BindingFlags.Instance);

        if (field?.GetValue(rig) is not Rect star || star.Width <= 0)
        {
            return null;
        }

        var at = rig.TranslatePoint(star.Center, window);

        if (at is null)
        {
            return null;
        }

        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, new Point(1, 1));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, at.Value);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var tip = TipOf(rig);

        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, new Point(1, 1));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return tip;
    }

    private static string Quote(string text, int most)
        => "\"" + (text.Length <= most ? text : text[..most] + "…") + "\"";
}

/// <summary>
/// **A FACT THAT ASSERTS NOTHING: WHAT EVERY CONTROL ON THE CW TAB AND THE BAND ROW
/// SAYS ON HOVER** (work instruction 422 task 1, step 6 criterion 6.4).
/// </summary>
/// <remarks>
/// <para>One line per control: where it is, its name, the command it is bound to,
/// its hover text as the built window resolves it, what the command actually does
/// read off its body, and, where a tip exists, whether it is true of the command. A
/// false tip is the reason step 6 exists (R62, CLAUDE.md §0.0).</para>
/// <para>**TWO STATES**, because the connect button's words change with the
/// connection: not connected, then connected to the training radio through the
/// button's own command.</para>
/// </remarks>
public sealed class WhatEveryControlSaysOnHoverTests
{
    /// <summary>
    /// What each command does, read off its body in `MainWindowViewModel`,
    /// `CwTransmitViewModel`, `ScanViewModel`, `AutoCallViewModel` and the controls.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> WhatItDoes = new Dictionary<string, string>
    {
        ["Transmit.PressCommand"] = "KEYS THE RADIO: sends the line in the box as Morse through the one transmit path, after the readiness check and the guard; a line he changed is held for a second press (HM-DEC-059, HM-DEC-079)",
        ["ComposeClearCommand"] = "empties the send line; sends nothing",
        ["ComposeCqCommand"] = "fills the send line with CQ CQ DE <his call> <his call> K; sends nothing",
        ["ComposeRstCommand"] = "fills the send line with RST 599 599; sends nothing",
        ["ComposeSeventyThreeCommand"] = "fills the send line with 73 TU E E; sends nothing",
        ["Transmit.OwnWords.Message (the text)"] = "the line Send puts on the air; typing changes it and sends nothing",
        ["ClearTerminalCommand"] = "wipes the transcript on screen; the decoder keeps listening with the speed and noise floor it has",
        ["IsExpanded (folds the panel)"] = "folds the panel away, or opens it again",
        ["CaptureAudioCommand"] = "keeps the last half minute of audio as a file with its sheet and adds a row to tonight's list",
        ["OpenReceiveHelpCommand"] = "cannot run (CanExecute is false): would expand a panel that is on no screen (HM-OPEN-087)",
        ["DismissReceiveOfferCommand"] = "hides the receive offer for the rest of the session",
        ["SelectBandCommand"] = "selects the band and moves the dial to its CW calling spot; with a radio connected the radio is tuned there; the neighborhood strip redraws for the band",
        ["ToggleConnectCommand"] = "not connected: connects to the radio on the port chosen beside it; connected: disconnects",
        ["SelectedPort (the choice)"] = "chooses the port Connect uses; locked while connected",
        ["Scan.StopCommand"] = "stops the scan now",
        ["AutoCall.StopCommand"] = "stops the transmitter now: keys the stop code itself",
        ["ToggleFavoriteCommand"] = "saves the frequency the dial is on as a favorite, or forgets it when it is already saved",
        ["TuneToFavoriteCommand"] = "tunes to that saved spot",
        ["ForgetFavoriteCommand"] = "forgets that saved spot",
        ["TuneToBestBetCommand"] = "selects the band the ranking puts first, exactly as pressing its band button does",
        ["ToggleUpgradeLadderCommand"] = "shows or hides what the next license class would open on this band",
        ["AcceptLookedUpClassCommand"] = "takes the license class the lookup found in place of the one he set",
        ["KeepMyLicenseClassCommand"] = "keeps the license class he set and stops asking",
        ["AcceptLookedUpGridCommand"] = "takes the looked-up grid in place of the one he typed",
        ["KeepMyGridCommand"] = "keeps the grid he typed and stops asking",
        ["OpenPsk31PowerOfferCommand"] = "opens the RF power offer; writes nothing",
        ["AcceptPsk31PowerCommand"] = "writes the offered RF power to the radio",
        ["DeclinePsk31PowerCommand"] = "closes the offer; writes nothing",
        ["TuneToDotCommand, ShowNeighborhoodCommand"] = "a dot tunes there; the background shows that neighborhood's story; a drag tunes",
        ["TransmitDrivePercent (the value)"] = "sets how loud Hamlet drives the sound card when it transmits, saved for next time",
        ["FrequencyHz (the wheel)"] = "the wheel over a digit tunes that digit",
        ["none, it is hovered"] = "nothing; it holds a sentence for hover",
    };

    /// <summary>
    /// Whether each tip is true of what the control does, read against the body above.
    /// Keyed by label. At entry (unit 421's tree) the save star read the digits' sentence,
    /// FALSE of the star, and the band buttons were true and silent on the press; unit
    /// 422 gave the star its own and put the press in front of each band's.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> IsItTrue = new Dictionary<string, string>
    {
        ["\"Clear\" (ClearTerminalCommand)"] = "true, kept",
        ["\"I hear a station\""] = "true, kept",
        ["ReceiveHelpOfferButton"] = "true, kept: it says the button cannot do anything, and it cannot (6.5's)",
        ["GreenZoneBestBet"] = "true, kept",
        ["rig face, the frequency digits"] = "true of the digits, kept",
        ["save star"] = "true of the star (unit 422); at entry it read the digits' sentence, FALSE of the star",
        ["band"] = "true (unit 422 put the press first); at entry true and silent on the press",
        ["saved spot"] = "true, kept",
        ["forget"] = "true, kept",
        ["mark"] = "true, kept: a mark's sentence is what it is for",
        ["TransmitButton"] = "true (unit 422): says it keys the radio",
        ["\"Clear\" (ComposeClearCommand)"] = "true (unit 422)",
        ["\"CQ\""] = "true (unit 422): fills, sends nothing",
        ["\"RST\""] = "true (unit 422): fills, sends nothing",
        ["\"73\""] = "true (unit 422): fills, sends nothing",
        ["send line"] = "true (unit 422)",
        ["header of"] = "true (unit 422)",
        ["\"No thanks\""] = "true (unit 422)",
        ["\"Stop the scan\""] = "true (unit 422)",
        ["\"STOP TRANSMITTING\""] = "true (unit 422); Escape is handled in MainWindow.axaml.cs",
        ["port list"] = "true (unit 422)",
        ["\"Connect\""] = "true (unit 422), follows the words",
        ["\"Disconnect\""] = "true (unit 422), follows the words",
        ["neighborhood strip"] = "true (unit 422): the mark's own sentence, from one constant",
        ["Button with no words"] = "true (unit 422)",
        ["\"Use the FCC value\""] = "true (unit 422)",
        ["\"Keep mine\""] = "true (unit 422)",
        ["\"Use the looked-up grid\""] = "true (unit 422)",
        ["DigitalTransmitDriveBox"] = "true (unit 422)",
        ["DigitalPsk31PowerLine"] = "true (unit 422)",
        ["DigitalPsk31PowerAccept"] = "true (unit 422)",
        ["DigitalPsk31PowerDecline"] = "true (unit 422)",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the inventory is printed.</param>
    public WhatEveryControlSaysOnHoverTests(ITestOutputHelper output) => _output = output;

    /// <summary>Prints the inventory in both states and asserts nothing.</summary>
    [AvaloniaFact]
    public async Task PrintsEveryControlItsTipAndWhatItDoes()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            Print("NOT CONNECTED, CW TAB", TheControlsTimCanPress.Find(window, panel));

            await TheControlsTimCanPress.ConnectAsync(window, panel);
            _output.WriteLine("");
            _output.WriteLine("connected: " + panel.IsConnected + ", button reads \"" + panel.ConnectButtonText + "\"");
            Print("CONNECTED TO THE TRAINING RADIO, CW TAB", TheControlsTimCanPress.Find(window, panel));
        }
        finally
        {
            await TheControlsTimCanPress.DisconnectAsync(window, panel);
            window.Close();
        }
    }

    private void Print(string state, IReadOnlyList<PressableControl> all)
    {
        _output.WriteLine("== " + state + " ==");

        // 6.4's own list first, then everything else, each place apart.
        foreach (var region in new[] { TheControlsTimCanPress.CwTab, TheControlsTimCanPress.BandRow })
        {
            var here = all.Where(c => c.Region == region).ToList();
            var named = here.Where(IsOnTheNamedList).ToList();
            var rest = here.Where(c => !IsOnTheNamedList(c)).ToList();

            _output.WriteLine("");
            _output.WriteLine("-- " + region + ": 6.4's own list --");
            named.ForEach(Line);
            _output.WriteLine("-- " + region + ": every other control --");
            rest.ForEach(Line);

            var with = here.Count(c => !string.IsNullOrWhiteSpace(c.Tip));
            _output.WriteLine(region + ": " + with + " of " + here.Count + " with a tip, "
                + (here.Count - with) + " without");
        }
    }

    private void Line(PressableControl c)
    {
        var tip = string.IsNullOrWhiteSpace(c.Tip) ? "none" : "\"" + OneLine(c.Tip!) + "\"";
        var does = WhatItDoes.TryGetValue(c.Command, out var d) ? d : "(not read)";
        var truth = string.IsNullOrWhiteSpace(c.Tip) ? "" : " | true? " + Verdict(c.Label);

        _output.WriteLine(
            c.Region + " | " + c.Label + " | " + (c.OnScreen ? "on screen" : "not drawn in this state")
            + " | " + c.Command + " | tip " + tip + truth + " | does: " + does);
    }

    private static string Verdict(string label)
    {
        foreach (var (key, value) in IsItTrue)
        {
            if (label == key || label.StartsWith(key + " ", StringComparison.Ordinal))
            {
                return value;
            }
        }

        return "(not judged)";
    }

    internal static bool IsOnTheNamedList(PressableControl c)
        => c.Label is "TransmitButton" or "save star"
            || c.Command is "ComposeClearCommand" or "ComposeCqCommand" or "ComposeRstCommand"
                or "ComposeSeventyThreeCommand" or "ToggleConnectCommand"
            || c.Label.StartsWith("band ", StringComparison.Ordinal)
            || c.Label.StartsWith("mark ?", StringComparison.Ordinal);

    private static string OneLine(string text)
        => text.Replace("\r", "", StringComparison.Ordinal).Replace("\n", " / ", StringComparison.Ordinal);
}
