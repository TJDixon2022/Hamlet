using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 269, task 2: **the Transmit drive is set where he is
/// looking - under the waterfall on the Digital tab, not behind a modal
/// dialog.**
/// </summary>
/// <remarks>
/// <para>**WHY IT IS UNDER THE WATERFALL.** `PHASE_PLAN.md` step D's first exit
/// criterion reads *"Tim sets the Transmit drive control and reads the dBFS and
/// clip count under the waterfall"*. Before this unit the only Transmit drive
/// control in Hamlet was `TransmitDriveBox` in `SettingsWindow.axaml:262`,
/// reached through <c>MainWindowViewModel.OpenSettingsAsync</c>, which shows the
/// settings window with <c>await window.ShowDialog(desktop.MainWindow)</c> at
/// <c>MainWindowViewModel.cs:4059</c> - **a second top-level window over the
/// waterfall, over the decode table and over the always-pressable
/// `DigitalStopButton`**, while the slot boundary keeps being driven behind it
/// by the 250 ms `_decodeTimer` (`MainWindowViewModel.cs:3272`, `:4898`,
/// `:7618`). Setting a drive between two fifteen-second slots therefore cost one
/// window opened and four controls crossed, with the band and the Stop button
/// hidden for the duration. It now costs no windows and one control.</para>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT: TWO CONTROLS OVER ONE SETTING
/// DRIFTING APART.** This unit puts a second control over a setting one control
/// already writes. The two ways that goes wrong are both silent. First, units: a
/// tab control that writes a *percentage* into <c>AppSettings.TransmitDrivePeak</c>,
/// which holds a *peak*, makes `25` mean 2500 % of full scale - a value
/// <c>Ft8Composer.DriveIsUsable</c> refuses, so the next transmission is refused
/// with a sentence the operator cannot account for. Second, persistence: a tab
/// control that changes the level in memory and never calls
/// <c>SettingsStore.Save</c> loses the level he set against his ALC the next time
/// Hamlet launches, and he sets it again without ever learning why. **Neither
/// would fail anything in the tree before tonight**, because before tonight
/// there was only one control.</para>
/// <para>**NOTHING IS DUPLICATED.** The percent-to-peak conversion, the
/// <c>Ft8Composer.DriveIsUsable</c> question and the note sentence are shared
/// with <see cref="SettingsViewModel"/> through <c>TransmitDrive</c>
/// rather than copied, so the two controls cannot come to different answers
/// about what a level is or what it works out to in dBFS. The committed strings
/// on the Settings screen are byte-for-byte what they were.</para>
/// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
/// (`SHACK_FACTS.md` FACT-004). No render endpoint is enumerated or opened and no
/// serial port exists. **No figure here is advice about the IC-7300** - what its
/// USB modulation input expects is not in this repository, which is what the note
/// on the screen has to keep saying.</para>
/// </remarks>
public sealed class TheDriveIsSetWhereHeIsLookingTests
{
    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every control and figure found is printed.</param>
    public TheDriveIsSetWhereHeIsLookingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **The control opens showing the level that is actually in force**, and it
    /// is under the waterfall with the Stop button still beside it.
    /// </summary>
    /// <remarks>
    /// Two settings objects, because a control that renders the default would
    /// pass on the default alone: one fresh <see cref="AppSettings"/>, and one
    /// already holding a level an operator chose.
    /// </remarks>
    [AvaloniaFact]
    public void TheControlUnderTheWaterfallOpensShowingTheLevelInForce()
    {
        var (window, _, settings) = Scene();

        var box = DriveBox(window);

        _output.WriteLine("peak in settings : " + settings.TransmitDrivePeak.ToString("F6"));
        _output.WriteLine("control shows    : " + box.Value + " %");

        Assert.Equal(
            (decimal)(Ft8Composer.DefaultDrivePeak * 100.0f),
            box.Value!.Value,
            2);

        // **IT IS UNDER THE WATERFALL AND NOT SOMEWHERE ELSE ON THE WINDOW.**
        // The criterion names a place, so the place is asserted: inside the
        // reserved Send area, which is Grid.Row 1 directly beneath
        // `DigitalWaterfallPanel` in `DigitalLeftColumn`.
        var reserved = Named<Border>(window, "DigitalSendReserved");

        Assert.True(
            box.GetVisualAncestors().Contains(reserved),
            "the drive control is on the window but not inside DigitalSendReserved, "
            + "which is the area under the waterfall.");

        // **AND THE STOP BUTTON IS STILL THERE AND STILL ON THE VISIBLE AREA.**
        // A new control in the Send area that pushes the always-pressable Stop
        // off the screen has broken the first thing no unit may reason past.
        var stop = Named<Button>(window, "DigitalStopButton");
        var stopAt = stop.TranslatePoint(default, window)!.Value;

        _output.WriteLine("DigitalStopButton at : " + stopAt + "  size " + stop.Bounds.Size);

        Assert.True(stop.IsVisible && stop.IsEffectivelyEnabled, "the Stop button is not usable.");
        Assert.True(
            stopAt.Y >= 0 && stopAt.Y + stop.Bounds.Height <= window.Bounds.Height,
            "the Stop button was pushed off the visible area, at y " + stopAt.Y
            + " on a window " + window.Bounds.Height + " high.");

        window.Close();

        // ---- AND A LEVEL SOMEBODY ALREADY CHOSE.
        var (second, _, _) = Scene(s => s.TransmitDrivePeak = 0.4f);

        var already = DriveBox(second);

        _output.WriteLine("with 0.4 in settings, control shows : " + already.Value + " %");

        Assert.Equal(40.0m, already.Value!.Value, 2);

        second.Close();
    }

    /// <summary>
    /// **Moving the control writes `AppSettings.TransmitDrivePeak`** - the same
    /// field, in the same units, that the Settings screen writes.
    /// </summary>
    /// <remarks>
    /// **Percent on the screen, peak in the file**, exactly as
    /// <c>SettingsViewModel.cs:330</c> does it. This is the units half of the
    /// breakage in the class remarks: a control that wrote 40 where 0.4 belongs
    /// would pass anything that only checked *the number moved*.
    /// </remarks>
    [AvaloniaFact]
    public void MovingTheControlWritesThePeakIntoTheSettings()
    {
        var (window, _, settings) = Scene();

        var box = DriveBox(window);

        box.Value = 40.0m;

        Pump(window);

        _output.WriteLine("asked for : 40 % on the screen");
        _output.WriteLine("peak now  : " + settings.TransmitDrivePeak.ToString("F6"));

        Assert.Equal(0.4f, settings.TransmitDrivePeak, 4);

        window.Close();
    }

    /// <summary>
    /// **A level the composer would refuse is not written, and the operator is
    /// told in `Ft8Composer`'s own words.**
    /// </summary>
    /// <remarks>
    /// <para>The question is asked of <c>Ft8Composer.DriveIsUsable</c> rather
    /// than answered a second time on this screen, so a level the tab accepted
    /// could never be one the send path then refuses with a sentence at the
    /// moment the operator presses send. **The setting keeps the last usable
    /// level** rather than storing one that would be rejected.</para>
    /// <para>**IT GOES THROUGH THE VIEW MODEL AND NOT THROUGH THE SPINNER**, and
    /// that is the stronger test: the spinner's own <c>Minimum</c> and
    /// <c>Maximum</c> are 1 and 100, the same two ends the Settings spinner
    /// carries, so a refused level cannot be typed into it - which means the
    /// bounds on the control and the composer's refusal would agree by accident
    /// even if the refusal were missing entirely. The expected sentence is read
    /// **off the composer here**, not written out again, so this cannot pass by
    /// matching a copy of a string that has since changed.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ALevelTheComposerWouldRefuseIsNotWrittenAndItSaysSoInTheComposersWords()
    {
        var (window, panel, settings) = Scene(s => s.TransmitDrivePeak = 0.3f);

        var note = Named<TextBlock>(window, "DigitalTransmitDriveNote");

        foreach (var refused in new[] { 0.0, -10.0, 101.0 })
        {
            panel.TransmitDrivePercent = refused;

            Pump(window);

            _output.WriteLine("asked for : " + refused + " %");
            _output.WriteLine("peak now  : " + settings.TransmitDrivePeak.ToString("F6"));
            _output.WriteLine("note      : " + note.Text);

            Assert.Equal(0.3f, settings.TransmitDrivePeak, 4);

            // **THE COMPOSER'S OWN SENTENCE, ASKED OF THE COMPOSER.** The note
            // upper-cases its first letter, so the tail from the second
            // character is what is looked for.
            Ft8Composer.DriveIsUsable((float)(refused / 100.0), out var why);

            Assert.Contains(why[1..], note.Text ?? "", StringComparison.Ordinal);
            Assert.Contains(
                "not a transmit level", note.Text ?? "", StringComparison.Ordinal);

            // And it names the level that is still in force, so he is not left
            // guessing what his radio is being driven at.
            Assert.Contains("still using 30 %", note.Text ?? "", StringComparison.Ordinal);
        }

        window.Close();
    }

    /// <summary>
    /// **The two views cannot disagree**: a settings panel built after the tab's
    /// control moved the level shows the new one.
    /// </summary>
    /// <remarks>
    /// They are two views over one <see cref="AppSettings"/> instance -
    /// <c>MainWindowViewModel.cs:4057</c> hands its own <c>_settings</c> to the
    /// settings view model - so this is the property that fails the moment either
    /// control starts keeping a level of its own.
    /// </remarks>
    [AvaloniaFact]
    public void AfterTheTabMovesItTheSettingsScreenShowsTheSameLevel()
    {
        var (window, _, settings) = Scene();

        DriveBox(window).Value = 40.0m;

        Pump(window);

        var settingsPanel = new SettingsViewModel(
            settings, null, new NoDevices(), Array.Empty<RenderEndpoint>);

        _output.WriteLine("tab wrote peak      : " + settings.TransmitDrivePeak.ToString("F6"));
        _output.WriteLine("settings screen says: " + settingsPanel.TransmitDrivePercent + " %");

        Assert.Equal(40.0, settingsPanel.TransmitDrivePercent, 4);

        window.Close();
    }

    /// <summary>
    /// **The dBFS is on screen before anything is transmitted, and it changes as
    /// the control moves.**
    /// </summary>
    /// <remarks>
    /// <para>This is the half of step D's first criterion that nothing in the
    /// tree could do before tonight. The only level Hamlet showed anywhere was
    /// inside <c>MainWindowViewModel.DigitalSendLine</c>, and that line is
    /// <c>NothingHasBeenSent</c> until a transmission has already gone out - so an
    /// operator setting a drive against his radio's ALC had to transmit once to
    /// find out what he had set.</para>
    /// <para>Nothing here sends anything. The window is opened, the number is
    /// read, the control is moved and the number is read again.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheLevelInDbfsIsOnScreenBeforeAnythingIsSent()
    {
        var (window, panel, _) = Scene();

        var note = Named<TextBlock>(window, "DigitalTransmitDriveNote");

        _output.WriteLine("nothing sent yet, the send line reads:");
        _output.WriteLine("  " + panel.DigitalSendLine);
        _output.WriteLine("and the drive note under the waterfall reads:");
        _output.WriteLine("  " + note.Text);

        // Nothing has been transmitted, which is the whole point of this one.
        Assert.Equal(MainWindowViewModel.NothingHasBeenSent, panel.DigitalSendLine);

        // -12.0 dBFS, from the default drive of 0.25.
        Assert.Contains("-12.0 dBFS", note.Text ?? "", StringComparison.Ordinal);

        // **AND IT MOVES WITH THE CONTROL**, so he can find the level before he
        // keys the radio rather than after.
        DriveBox(window).Value = 40.0m;

        Pump(window);

        _output.WriteLine("after moving the control to 40 %:");
        _output.WriteLine("  " + note.Text);

        // 20 * log10(0.4) = -7.96 dBFS, which formats as -8.0.
        Assert.Contains("-8.0 dBFS", note.Text ?? "", StringComparison.Ordinal);
        Assert.DoesNotContain("-12.0 dBFS", note.Text ?? "", StringComparison.Ordinal);

        window.Close();
    }

    /// <summary>The realized drive control under the waterfall.</summary>
    /// <param name="window">The shown window.</param>
    /// <returns>The control, or a failure naming what is missing.</returns>
    private static NumericUpDown DriveBox(MainWindow window)
        => Named<NumericUpDown>(window, "DigitalTransmitDriveBox");

    /// <summary>One realized control, found on the window by its own name.</summary>
    /// <typeparam name="T">What kind of control it should be.</typeparam>
    /// <param name="window">The shown window.</param>
    /// <param name="name">Its <c>x:Name</c>.</param>
    /// <returns>The control.</returns>
    private static T Named<T>(MainWindow window, string name)
        where T : Control
    {
        Pump(window);

        var found = window.GetVisualDescendants().OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "there is no " + typeof(T).Name + " called \"" + name
            + "\" on the realized window.");

        return found!;
    }

    /// <summary>The real window on the Digital tab, on 20 m, with no radio.</summary>
    /// <param name="arrange">Anything a test wants set before the window opens.</param>
    /// <returns>The window, its view model and the settings behind both.</returns>
    /// <remarks>
    /// **NO PORT AND NO SINK.** Nothing here transmits, so neither is built; the
    /// view model's <c>_armedSend</c> stays null, which is this machine's real
    /// state (FACT-004).
    /// </remarks>
    private static (MainWindow Window, MainWindowViewModel Panel, AppSettings Settings)
        Scene(Action<AppSettings>? arrange = null)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        arrange?.Invoke(settings);

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        // **TALL ENOUGH THAT THE SEND AREA IS ON THE SCREEN**, the size unit 268
        // measured it needs: at the default height the reserved area sits below
        // the fold, which is a fact about the window and not about the controls
        // in it.
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        Pump(window);

        return (window, panel, settings);
    }

    /// <summary>Runs the dispatcher queue and lays the window out.</summary>
    /// <param name="window">The window.</param>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>A capture-device source that opens nothing and offers nothing.</summary>
    private sealed class NoDevices : IAudioDevices
    {
        /// <inheritdoc/>
        public IReadOnlyList<AudioDevice> List() => [];
    }
}
