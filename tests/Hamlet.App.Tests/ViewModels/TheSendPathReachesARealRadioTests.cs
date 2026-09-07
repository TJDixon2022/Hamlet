using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 260, task 3: **the send path is given the port and the named
/// endpoint it has never had - and is given them only when both are really
/// there.**
/// </summary>
/// <remarks>
/// <para>**THIS IS WHERE AN UNASKED TRANSMISSION WOULD COME FROM**, and these
/// assertions exist to make that impossible rather than unlikely. Until this unit
/// <c>_armedSend</c> was assigned by exactly one line in the whole tree and that
/// line was <c>UseArmedSendForTests</c> (`docs/unit260-route-trace.md` question
/// 5), so every existing test of the send path proved the path and not the
/// wiring. **That is the gap these close.**</para>
/// <para>**NOTHING HERE OPENS ANYTHING** (`SHACK_FACTS.md` FACT-004, work
/// instruction 260 *What not to do* 1). No <c>SystemSerialPort</c> and no
/// <c>WasapiTransmitSink</c> is constructed by any test in this project: the
/// port is a fake and the sink arrives through the substituted factory. **A
/// device opened here would measure this machine and say nothing about the
/// radio.**</para>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT.** Building the armed send
/// unconditionally at connect - which is the obvious way to write it - reaches
/// the sink factory with an empty name on a machine with no such endpoint. With
/// the real factory in place that is a <c>WasapiTransmitSink</c> constructor
/// throwing where an operator would see it. **Watched failing exactly that way
/// before the guards went in**, and quoted in `output.md`.</para>
/// </remarks>
public sealed class TheSendPathReachesARealRadioTests
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the refusals are printed.</param>
    public TheSendPathReachesARealRadioTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **No port and no endpoint: nothing is armed and the sink factory is never
    /// called.**
    /// </summary>
    /// <remarks>
    /// Assertion 1. The refusal keeps the exact wording it had before this unit,
    /// because this is the case that has not changed: on a machine with no radio
    /// and nothing named, the operator reads the same sentence he always did.
    /// </remarks>
    [Fact]
    public void WithNoPortAndNoEndpointNothingIsArmedAndTheSinkIsNeverAskedFor()
    {
        var (panel, _, factory) = Panel();

        panel.BuildTheArmedSend(null);

        _output.WriteLine("sink factory calls: " + factory.Calls.Count);

        Assert.Empty(factory.Calls);
        Assert.False(panel.HasSomethingToTransmitThrough);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Contains("sent nothing", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "no radio is connected and no transmit audio device is named in Settings",
            panel.DigitalSendLine,
            StringComparison.Ordinal);
        Assert.Null(panel.ArmedForSlotUtc);
    }

    /// <summary>
    /// **A named endpoint on its own arms nothing, and says why.**
    /// </summary>
    /// <remarks>
    /// Assertion 1's other half. **An endpoint is not a radio.** The sink factory
    /// is still never called, because building a sink for a transmission that has
    /// no wire to key would open a device for nothing.
    /// </remarks>
    [Fact]
    public void AnEndpointWithoutARadioArmsNothingAndTheSinkIsStillNeverAskedFor()
    {
        var (panel, settings, factory) = Panel();

        settings.AudioOutputDeviceId = NamedEndpoint;

        panel.BuildTheArmedSend(null);

        Assert.Empty(factory.Calls);
        Assert.False(panel.HasSomethingToTransmitThrough);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Contains(
            "no radio with a serial port is connected",
            panel.DigitalSendLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **The training radio has no port, so it can arm nothing.**
    /// </summary>
    /// <remarks>
    /// Assertion 2. **A simulator must never become a route to a keying frame**,
    /// and the way it cannot is that it has nothing to key with:
    /// <c>CreateRig</c> returns no port for the training entry, so the connect
    /// path reaches <c>BuildTheArmedSend(null)</c> however the endpoint is set.
    /// The breakage this would have caught: keeping *a* port field and filling it
    /// from whichever rig connected, which on the training radio would key
    /// nothing at all while the Send area said a message had gone out.
    /// </remarks>
    [Fact]
    public void TheTrainingRadioBringsNoPortAndThereforeArmsNothing()
    {
        var (rig, port) = MainWindowViewModel.CreateRig(MainWindowViewModel.TrainingRadio);

        _output.WriteLine("training rig      : " + rig.GetType().Name);
        _output.WriteLine("training rig port : " + (port is null ? "none" : port.PortName));

        Assert.True(rig.IsSimulated);
        Assert.Null(port);

        var (panel, settings, factory) = Panel();

        settings.AudioOutputDeviceId = NamedEndpoint;

        panel.BuildTheArmedSend(port);

        Assert.Empty(factory.Calls);
        Assert.False(panel.HasSomethingToTransmitThrough);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Contains(
            "simulator and has no port", panel.DigitalSendLine, StringComparison.Ordinal);

        (rig as IDisposable)?.Dispose();
    }

    /// <summary>
    /// **A port and an endpoint: one transmission is armed, through the one
    /// command that arms.**
    /// </summary>
    /// <remarks>
    /// Assertion 3. The sink comes from the endpoint's name and from nowhere
    /// else, and the click goes through <c>SendMessageCommand</c> - which is the
    /// only line in `src/` that calls <c>Ft8ArmedSend.Arm</c>.
    /// </remarks>
    [Fact]
    public async Task WithBothHalvesOneClickArmsExactlyOneTransmission()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;

        panel.BuildTheArmedSend(port);

        _output.WriteLine("sink factory asked for: "
            + string.Join(", ", factory.Calls));

        Assert.Equal(NamedEndpoint, Assert.Single(factory.Calls));
        Assert.True(panel.HasSomethingToTransmitThrough);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        _output.WriteLine("outcome        : " + result!.Outcome);
        _output.WriteLine("sent           : " + result.Run?.Sent);
        _output.WriteLine("sink calls     : " + factory.Sink.TimesCalled);
        _output.WriteLine("frames written : " + port.Written.Count);
        _output.WriteLine(panel.DigitalSendLine);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(result.Run!.Sent);

        // **ONE TRANSMISSION, THROUGH THE SINK THIS CODE BUILT.**
        Assert.Equal(1, factory.Sink.TimesCalled);

        // KEYED AND UNKEYED, ON THE PORT THIS CODE KEPT.
        Assert.Equal(2, port.Written.Count);
    }

    /// <summary>
    /// **One click is one message across two boundaries.**
    /// </summary>
    /// <remarks>
    /// <para>Assertion 4, re-asserted against an <c>Ft8ArmedSend</c> this code
    /// built rather than one a test handed over. Unit 259 proved the same
    /// sentence against a hand-injected one, which proved the path and not the
    /// wiring.</para>
    /// <para>**THE RED WAS ALREADY WATCHED AND IS NOT RE-WATCHED HERE.** Unit 259
    /// quoted the failure - one click and three boundaries putting three
    /// transmissions on the wire - and the fix lives inside
    /// <c>Ft8ArmedSend.AtBoundaryAsync</c>, which work instruction 260 *What not
    /// to do* 3 forbids this unit from touching. Reproducing it would mean
    /// editing that file.</para>
    /// </remarks>
    [Fact]
    public async Task OneClickIsOneMessageAcrossTwoBoundaries()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;

        panel.BuildTheArmedSend(port);
        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, panel.DigitalSendLine);

        var first = await panel.AtSlotBoundaryAsync(slot!.Value);
        var second = await panel.AtSlotBoundaryAsync(slot.Value.AddSeconds(15));

        _output.WriteLine("first boundary  : " + first!.Outcome);
        _output.WriteLine("second boundary : " + second!.Outcome);
        _output.WriteLine("sink calls      : " + factory.Sink.TimesCalled);
        _output.WriteLine("frames written  : " + port.Written.Count);

        Assert.Equal(Ft8ArmOutcome.Ran, first.Outcome);
        Assert.Equal(Ft8ArmOutcome.NothingArmed, second.Outcome);

        // **TWO BOUNDARIES, ONE TRANSMISSION.**
        Assert.Equal(1, factory.Sink.TimesCalled);
        Assert.Equal(2, port.Written.Count);
    }

    /// <summary>
    /// **A stale endpoint name leaves nothing armed and says so out loud.**
    /// </summary>
    /// <remarks>
    /// <para>A device id survives a driver update but not being moved to another
    /// USB socket, and <c>WasapiTransmitSink</c> throws where the endpoint it
    /// names is not there. **A click must not put an exception in front of an
    /// operator**, so the throw is caught where the sink is built - at connect -
    /// and the reserved Send area says what happened.</para>
    /// <para>The breakage this would have caught: an unhandled exception raised
    /// from a context menu while the operator was answering a CQ.</para>
    /// </remarks>
    [Fact]
    public void AStaleEndpointNameIsCaughtAtConnectAndSaidInTheSendArea()
    {
        var (panel, settings, factory) = Panel();

        settings.AudioOutputDeviceId = NamedEndpoint;

        factory.Throws = new InvalidOperationException(
            "there is no active render endpoint called \"" + NamedEndpoint + "\".");

        panel.BuildTheArmedSend(new FakePort());

        _output.WriteLine("send area at connect: " + panel.DigitalSendLine);

        Assert.False(panel.HasSomethingToTransmitThrough);
        Assert.Contains(
            "could not be opened", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "no active render endpoint", panel.DigitalSendLine, StringComparison.Ordinal);

        // AND THE CLICK REFUSES WITH THE SAME WORDS RATHER THAN THROWING.
        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine("send area on the click: " + panel.DigitalSendLine);

        Assert.Contains("sent nothing", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "could not be opened", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Losing the radio clears the armed send, so nothing survives a
    /// disconnect.**
    /// </summary>
    /// <remarks>
    /// The disconnect nulls the port and the armed send in the same `finally`
    /// that nulls the rig; this asserts the guarantee that makes that safe -
    /// building with no port clears whatever was there. The breakage: a radio
    /// unplugged mid-evening leaving a sender pointing at a port that is gone.
    /// </remarks>
    [Fact]
    public void LosingThePortClearsWhateverWasArmed()
    {
        var (panel, settings, _) = Panel();

        settings.AudioOutputDeviceId = NamedEndpoint;

        panel.BuildTheArmedSend(new FakePort());

        Assert.True(panel.HasSomethingToTransmitThrough);

        panel.BuildTheArmedSend(null);

        Assert.False(panel.HasSomethingToTransmitThrough);
        Assert.Null(panel.ArmedForSlotUtc);
    }

    /// <summary>A panel on 20 m, a licence that permits, and a recording factory.</summary>
    private static (MainWindowViewModel Panel, AppSettings Settings, RecordingSinkFactory Factory)
        Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var panel = new MainWindowViewModel(settings, null);

        // 20 m FIRST, THEN THE FREQUENCY - `OnFrequencyHzChanged` clamps to the
        // selected band's map window.
        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var factory = new RecordingSinkFactory();

        panel.TransmitSinkFactory = factory.Make;

        return (panel, settings, factory);
    }

    /// <summary>
    /// A sink factory that says what it was asked for and hands back a fake.
    /// </summary>
    /// <remarks>
    /// **THE DEFAULT FACTORY IS NEVER INVOKED BY ANY TEST IN THIS UNIT**, and
    /// this is what proves it: every panel these tests build has this in place of
    /// `name => new WasapiTransmitSink(name)`.
    /// </remarks>
    private sealed class RecordingSinkFactory
    {
        /// <summary>Every endpoint name it was asked for, in order.</summary>
        public List<string> Calls { get; } = [];

        /// <summary>The one fake it hands back.</summary>
        public FakeSink Sink { get; } = new();

        /// <summary>Throw this instead of handing a sink back.</summary>
        public Exception? Throws { get; set; }

        /// <summary>The factory itself.</summary>
        public ITransmitAudioSink Make(string endpoint)
        {
            Calls.Add(endpoint);

            return Throws is null ? Sink : throw Throws;
        }
    }
}
