using System.Globalization;
using Avalonia.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 267, task 2: **the licence gate holds from the operator's own
/// click, with his own licence class and his own guard setting, all the way to a
/// slot boundary.**
/// </summary>
/// <remarks>
/// <para>**THE SEAM NOTHING PROVED.** `PHASE_PLAN.md` names licence privileges as
/// one of three things no unit may reason past - *the Settings gate is not
/// bypassable from any send path*. Before this class the gate was proved on one
/// side and the menu on the other, and nothing joined them:
/// <c>OneClickSendsExactlyOneMessageTests.OutOfPrivilegesNothingReachesThePortOrTheSink</c>
/// **constructs the armed send itself**, and the app-side licence tests read a
/// *display* property and never send. `docs/unit267-step-b-trace.md` question 6
/// measured it: <c>RefusedByLicence</c> appeared nowhere in this project at all,
/// and <c>MainWindowViewModel.cs:8346</c>'s branch was asserted by nothing.</para>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT**, which the rule requires named:
/// the panel handing the gate anything other than the operator's own licence class
/// and his own guard setting - a hard-coded class, a <c>GuardEnabled</c> that is
/// always true, a settings read that goes stale. <c>MainWindowViewModel.cs:8221</c>
/// is the one line in `src/` that builds the <c>OperatorSend</c>, and every one of
/// those mistakes **makes the Settings gate bypassable from the one path a click
/// takes, while every engine licence test stays green** - because the engine's
/// tests build their own <c>OperatorSend</c> and would never see it.</para>
/// <para>**NOTHING HERE OPENS ANYTHING** (`SHACK_FACTS.md` FACT-004). No serial
/// port, no <c>WasapiTransmitSink</c>, no sound: the port is <see cref="FakePort"/>
/// and the sink arrives through the substituted factory, exactly as
/// <c>TheSendPathReachesARealRadioTests</c> does it.</para>
/// <para>**AND IT ADDS NO SECOND COPY OF THE RULE.** The refusal lives inside
/// <c>Ft8TransmitSequence.RunAsync</c>, before anything that can key. This reads
/// what that returned; it does not re-decide it.</para>
/// </remarks>
public sealed class TheLicenceGateHoldsFromTheClickTests
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>
    /// FT8's watering hole on 20 m: a General may send data here and a Technician
    /// may not.
    /// </summary>
    /// <remarks>
    /// The same frequency and the same class the engine's own
    /// <c>OutOfPrivilegesNothingReachesThePortOrTheSink</c> uses, so the two levels
    /// are answering about one case rather than about two.
    /// </remarks>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The boundary the decoded rows are hung off.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the outcome, the counts and the sentence are printed.</param>
    public TheLicenceGateHoldsFromTheClickTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **Assertion 1. Out of privileges, the operator's own click sends nothing.**
    /// </summary>
    /// <remarks>
    /// <para>**THE TRAP, WRITTEN DOWN SO NOBODY WALKS INTO IT AGAIN**
    /// (`docs/unit267-step-b-trace.md` question 7). A licence refusal comes back as
    /// <c>Ft8ArmOutcome.Ran</c> at the boundary - <c>Ft8ArmedSend.cs:473</c> returns
    /// <c>Ran</c> for anything the sequence ran, refusal included - and only
    /// <c>TransmitRun.Outcome</c> carries <c>RefusedByLicence</c>. **A test written
    /// against the arm outcome alone would read a refusal as a send.**</para>
    /// <para>`Keyed`, `Sent`, the sink's call count and the bytes on the wire are
    /// all asserted, because *nothing was transmitted* has four separate ways of
    /// being false and only one of them is the outcome word.</para>
    /// </remarks>
    [Fact]
    public async Task OutOfPrivilegesTheClickReachesTheBoundaryAndNothingIsKeyed()
    {
        var (panel, _, factory, port) = Armed(LicenseClass.Technician);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "nothing was armed: " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        _output.WriteLine("licence class     : " + panel.LicenseClass);
        _output.WriteLine("frequency         : " + panel.FrequencyHz);
        _output.WriteLine("boundary outcome  : " + result!.Outcome);
        _output.WriteLine("run outcome       : " + result.Run!.Outcome);
        _output.WriteLine("sent              : " + result.Run.AudioWentOut);
        _output.WriteLine("keyed             : " + result.Run.Keyed);
        _output.WriteLine("sink calls        : " + factory.Sink.TimesCalled);
        _output.WriteLine("bytes at the port : " + BytesAt(port));
        _output.WriteLine("reason            : " + result.Run.Reason);
        _output.WriteLine("citation          : " + result.Run.Citation);

        // THE SEQUENCE RAN AND REFUSED. Both halves, not one.
        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, result.Run.Outcome);

        Assert.False(result.Run.AudioWentOut);
        Assert.False(result.Run.Keyed);

        // NOTHING PLAYED AND NOTHING ON THE WIRE.
        Assert.Equal(0, factory.Sink.TimesCalled);
        Assert.True(factory.Sink.WasNeverTouched);
        Assert.Empty(port.Written);
        Assert.Equal(0, BytesAt(port));
        Assert.True(port.WasNeverWrittenTo);
    }

    /// <summary>
    /// **Assertion 2. And he is told why, in the guard's own words, with the
    /// citation, naming the message that did not go.**
    /// </summary>
    /// <remarks>
    /// <para>A transmission that silently does not happen is the worse fault of the
    /// two: the operator watches an unchanged screen and clicks again. The sentence
    /// is <c>MainWindowViewModel.cs:8346</c>'s branch, which
    /// `docs/unit267-step-b-trace.md` question 6 measured as covered by
    /// **nothing**.</para>
    /// <para>**THE LINE IS POSTED TO THE UI THREAD** at
    /// <c>MainWindowViewModel.cs:8309</c>, and nothing pumps that queue in a test
    /// process, so the queue is run here - the string asserted is the one the
    /// posted job writes and not a stand-in for it.</para>
    /// </remarks>
    [Fact]
    public async Task OutOfPrivilegesTheSendAreaSaysWhyInTheGuardsOwnWordsWithItsCitation()
    {
        var (panel, _, _, _) = Armed(LicenseClass.Technician);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "nothing was armed: " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Dispatcher.UIThread.RunJobs();

        var line = panel.DigitalSendLine;

        _output.WriteLine("the sentence the operator is left looking at:");
        _output.WriteLine("  " + line);

        // IT NAMES THE MESSAGE THAT DID NOT GO.
        Assert.Contains("did not send", line, StringComparison.Ordinal);
        Assert.Contains("W1ABC KC3QIS -10", line, StringComparison.Ordinal);

        // **THE GUARD'S OWN WORDS, NOT A SECOND COPY OF THEM.** Whatever the
        // privileges data says is what the operator reads, so this cannot drift
        // from `data/privileges/us-part97-privileges.json` the way a literal would.
        var run = result!.Run!;

        Assert.NotEmpty(run.Reason);
        Assert.NotEmpty(run.Citation);
        Assert.Contains(run.Reason, line, StringComparison.Ordinal);

        // AND THE CITATION, IN BRACKETS AFTER IT.
        Assert.Contains("(" + run.Citation + ")", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 3. The same click inside privileges goes.**
    /// </summary>
    /// <remarks>
    /// **THIS IS WHAT MAKES ASSERTION 1 A GATE RATHER THAN A DEAD PATH.** Same
    /// panel, same harness, same frequency, same click - only the licence class in
    /// Settings differs. Without it the whole class would pass against a send path
    /// that never worked at all, which is the failure mode a refusal test cannot
    /// see from the inside.
    /// </remarks>
    [Fact]
    public async Task TheSameClickInsidePrivilegesRunsAndKeysTheRadio()
    {
        var (panel, _, factory, port) = Armed(LicenseClass.General);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "nothing was armed: " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Dispatcher.UIThread.RunJobs();

        _output.WriteLine("licence class     : " + panel.LicenseClass);
        _output.WriteLine("boundary outcome  : " + result!.Outcome);
        _output.WriteLine("run outcome       : " + result.Run!.Outcome);
        _output.WriteLine("sent              : " + result.Run.AudioWentOut);
        _output.WriteLine("sink calls        : " + factory.Sink.TimesCalled);
        _output.WriteLine("frames at the port: " + port.Written.Count);
        _output.WriteLine("the sentence      : " + panel.DigitalSendLine);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.Equal(Ft8TransmitOutcome.Played, result.Run.Outcome);
        Assert.True(result.Run.AudioWentOut);

        // ONE TRANSMISSION, KEYED AND UNKEYED.
        Assert.Equal(1, factory.Sink.TimesCalled);
        Assert.Equal(2, port.Written.Count);

        // **AND THE SEND AREA SAYS WHAT WENT AND TO WHOM** - criterion 4's first
        // half, on the same run rather than on a second one. `Addressed` at
        // `MainWindowViewModel.cs:8452` is what puts the callsign in it.
        Assert.Contains("Sent to W1ABC,", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "\"W1ABC KC3QIS -10\"", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 4. Nothing is withheld from the menu while the licence
    /// refuses.**
    /// </summary>
    /// <remarks>
    /// <para>Ruled 2026-09-06: **nothing is forbidden in the menu**, and a refusal
    /// at the gate is not a reason to take an option off it. The licence arrives as
    /// a *note* - <c>MainWindow.axaml.cs:199-202</c> adds
    /// <c>Note(vm.DigitalSendLicenceLine)</c> exactly when
    /// <see cref="MainWindowViewModel.HasDigitalSendLicenceLine"/> is true, and
    /// <c>Note</c> at <c>:245</c> carries no command and is not hit-testable.</para>
    /// <para>**ASSERTED THROUGH THE PANEL, WHICH IS WHERE THE DECISION IS MADE.**
    /// The flyout's own condition and its own text are both read here; building the
    /// <c>MenuFlyout</c> itself needs an Avalonia application and is proved in
    /// <c>TheMenuIsUnderTheMouseTests.OutOfPrivilegesTheMenuSaysSoAndForbidsNothing</c>.</para>
    /// </remarks>
    [Fact]
    public void WhileTheLicenceRefusesTheMenuStillOffersEveryMessageAndSaysWhy()
    {
        var (panel, _, _, _) = Armed(LicenseClass.Technician);

        var row = Add(panel, 8, "KC3QIS W1ABC R-15", snr: "-14");
        var menu = panel.SendMenuFor(row);

        Assert.NotNull(menu);

        foreach (var option in menu!.Options)
        {
            _output.WriteLine(
                option.Text + " | " + option.Label + " | "
                + (option.IsExpected ? "expected" : "-"));
        }

        _output.WriteLine("the note under them: " + panel.DigitalSendLicenceLine);

        // **ALL FIVE, STILL.** Nothing greyed, nothing removed.
        Assert.Equal(5, menu.Options.Count);
        Assert.Contains(menu.Options, o => o.Shape == Ft8SendShape.Grid);
        Assert.Contains(menu.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.Contains(menu.Options, o => o.Shape == Ft8SendShape.RogerAndReport);
        Assert.Contains(menu.Options, o => o.Shape == Ft8SendShape.Acknowledge);
        Assert.Contains(menu.Options, o => o.Shape == Ft8SendShape.Seventy3);

        // AND THE LICENCE IS THERE AS A NOTE, WHICH IS WHAT THE FLYOUT READS.
        Assert.True(panel.HasDigitalSendLicenceLine);
        Assert.NotEmpty(panel.DigitalSendLicenceLine);
    }

    /// <summary>
    /// **What the path does with the guard switched off in Settings. Measured and
    /// recorded, not decided.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 267 task 2's last paragraph: *"Also record, and do not
    /// change: what the path does when <c>RestrictTransmitToPrivileges</c> is
    /// switched off in Settings."* This is that measurement, written as an
    /// assertion so it cannot go stale silently.</para>
    /// <para>**THE BREAKAGE IT WOULD HAVE CAUGHT**, which the rule requires named:
    /// a <c>GuardEnabled</c> that never reaches the gate - hard-coded, or read once
    /// at startup and never again. That fault is invisible to assertions 1 and 3,
    /// which both run with the setting at its default of <c>true</c>. **This is the
    /// only assertion in the tree that moves the switch**, and what proves the
    /// switch arrived is that the *sentence* changes with it: a panel passing a
    /// hard-coded <c>true</c> would leave the operator reading the privileges
    /// refusal, and he reads the guard-off refusal instead.</para>
    /// <para>**THE MEASUREMENT, AND IT IS NOT WHAT THIS TEST FIRST EXPECTED.**
    /// <c>TransmitGuard.Check</c> at <c>TransmitGuard.cs:88-91</c> does return
    /// <c>MayTransmit</c> true with <c>WasOverridden</c> true where the guard is
    /// off - **and this send path refuses anyway**.
    /// <c>Ft8TransmitSequence.Permits</c> at <c>:443</c> accepts one of the gate's
    /// three ways of permitting, because §0.2 says the Settings check is not
    /// bypassable from any send path. **Switching the guard off in Settings does
    /// not make Hamlet transmit outside privileges; it changes which sentence the
    /// operator reads.** The red this was watched at is committed at 5c91f7e with
    /// its verbatim output. Whether that is the right behaviour is not this unit's
    /// to weigh - it is recorded in `output.md` for the owner, and no product code
    /// was touched.</para>
    /// </remarks>
    [Fact]
    public async Task WithTheGuardSwitchedOffInSettingsTheSendPathStillRefusesAndSaysWhy()
    {
        var (panel, settings, factory, port) = Armed(LicenseClass.Technician);

        settings.RestrictTransmitToPrivileges = false;

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "nothing was armed: " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Dispatcher.UIThread.RunJobs();

        _output.WriteLine("guard enabled     : " + settings.RestrictTransmitToPrivileges);
        _output.WriteLine("licence class     : " + panel.LicenseClass);
        _output.WriteLine("run outcome       : " + result!.Run!.Outcome);
        _output.WriteLine("sent              : " + result.Run.AudioWentOut);
        _output.WriteLine("sink calls        : " + factory.Sink.TimesCalled);
        _output.WriteLine("frames at the port: " + port.Written.Count);
        _output.WriteLine("the sentence      : " + panel.DigitalSendLine);

        // **STILL REFUSED, AND STILL NOTHING ON THE WIRE.** The switch does not
        // open a route to the air.
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, result.Run.Outcome);
        Assert.False(result.Run.AudioWentOut);
        Assert.False(result.Run.Keyed);
        Assert.Equal(0, factory.Sink.TimesCalled);
        Assert.Empty(port.Written);

        // **AND THE SWITCH DID REACH THE GATE** - this is a different sentence
        // from assertion 2's, which is what proves the panel passed the operator's
        // own setting rather than a constant.
        Assert.Contains(
            // **`licence` BECAME `license` UNDER §R19** (work instruction 325
            // task 5). In American English the noun is `license`, and this
            // sentence is about the operator's license to transmit. The type
            // name is a code identifier and is outside the rule.
            "the license guard is switched off in Settings",
            panel.DigitalSendLine,
            StringComparison.Ordinal);
        // AND IT STILL CARRIES WHAT THE GUARD SAID WHEN IT WAS LAST ASKED, so
        // switching the guard off does not cost him the reason.
        Assert.Contains(
            "What the guard said when it was last asked",
            panel.DigitalSendLine,
            StringComparison.Ordinal);

        Assert.False(settings.RestrictTransmitToPrivileges);
    }

    /// <summary>How many bytes reached the wire, over every frame.</summary>
    private static int BytesAt(FakePort port) => port.Written.Sum(f => f.Length);

    /// <summary>
    /// A panel on 20 m with the operator's own class, wired to a fake port and a
    /// substituted sink factory, with one transmission's worth of everything ready.
    /// </summary>
    /// <param name="licenseClass">The class the operator has in Settings.</param>
    /// <returns>The panel, its settings, the recording factory and the fake port.</returns>
    private static (
        MainWindowViewModel Panel,
        AppSettings Settings,
        RecordingSinkFactory Factory,
        FakePort Port) Armed(LicenseClass licenseClass)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = licenseClass;
        settings.AudioOutputDeviceId = NamedEndpoint;

        var panel = new MainWindowViewModel(settings, null);

        // 20 m FIRST, THEN THE FREQUENCY - `OnFrequencyHzChanged` clamps to the
        // selected band's map window, so setting the number alone lands somewhere
        // else and the guard answers about a frequency nobody chose.
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var factory = new RecordingSinkFactory();

        panel.TransmitSinkFactory = factory.Make;

        var port = new FakePort();

        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        return (panel, settings, factory, port);
    }

    /// <summary>Puts one decoded row on the panel, the way the decoder would.</summary>
    private static DigitalDecodeRow Add(
        MainWindowViewModel panel, int slot, string message, string snr = "-11")
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        return panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, utc);
    }

    /// <summary>A sink factory that hands back a fake and opens nothing.</summary>
    private sealed class RecordingSinkFactory
    {
        /// <summary>Every endpoint name it was asked for, in order.</summary>
        public List<string> Calls { get; } = [];

        /// <summary>The one fake it hands back.</summary>
        public FakeSink Sink { get; } = new();

        /// <summary>The factory itself.</summary>
        public ITransmitAudioSink Make(string endpoint)
        {
            Calls.Add(endpoint);

            return Sink;
        }
    }
}
