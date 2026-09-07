using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 262: **the application composes at the rate the transmit
/// endpoint actually declares, not at the decoder's 12000 Hz.**
/// </summary>
/// <remarks>
/// <para>**THIS DRIVES THE APPLICATION'S OWN SEND PATH AND NOT A SEQUENCE A TEST
/// ASSEMBLED.** The click goes through <c>SendMessageCommand</c>, the armed send
/// comes from <c>BuildTheArmedSend</c>, and the sink arrives through the
/// substitutable <c>TransmitSinkFactory</c>. That route is the whole point: unit
/// 256 proved a loopback that composed at the endpoint's rate **inside its own
/// test**, never touching the caller, and the caller is where the defect lived.</para>
/// <para>**THE BREAKAGE THESE CAUGHT.** Before this unit the send path composed
/// with no rate argument at <c>MainWindowViewModel.cs:8120</c>, which is
/// <c>Ft8Waveform.DefaultSampleRate</c> - 12000 Hz. Every one of this machine's
/// four active render endpoints declares 48000 Hz. So on a real click the radio
/// keyed at <c>Ft8TransmitSequence.cs:283</c>, the sink threw at
/// <c>WasapiTransmitSink.cs:301</c>, the abort took the radio back out of
/// transmit and nothing went on the air - **every time, on every endpoint.**
/// Watched failing exactly that way before the fix went in, and quoted in
/// `output.md`.</para>
/// <para>**AND NO TEST COULD HAVE CAUGHT IT**, because both transmit fakes took
/// any rate they were handed. The fake was made faithful in the same unit -
/// <c>FakeSink.DeclaredSampleRate</c> - and that is what licenses these
/// assertions existing at all.</para>
/// <para>**NOTHING HERE OPENS ANYTHING** (`SHACK_FACTS.md` FACT-004). No serial
/// port and no <c>WasapiTransmitSink</c>: the port is a fake and the sink arrives
/// through the substituted factory. Nothing measured here says anything about the
/// radio.</para>
/// </remarks>
public sealed class TheSendPathComposesAtTheEndpointsRateTests
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>
    /// What every active render endpoint on this machine declares, measured in
    /// task 1. It is also the shared-mode mix rate of ordinary hardware.
    /// </summary>
    private const int OrdinaryEndpointRate = 48_000;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rates are printed.</param>
    public TheSendPathComposesAtTheEndpointsRateTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A click, an endpoint speaking 48000 Hz, and a whole transmission goes
    /// out.**
    /// </summary>
    /// <remarks>
    /// The assertion this unit exists for. **Whole** is the operative word: the
    /// sink is asked for the rate it declares, it plays every sample it was given,
    /// the radio keys and comes back out, and the Send area says the message went.
    /// </remarks>
    [Fact]
    public async Task AtAnEndpointSpeaking48000TheWholeTransmissionGoesOut()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        factory.Sink.DeclaredSampleRate = OrdinaryEndpointRate;

        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        _output.WriteLine("endpoint declares : " + OrdinaryEndpointRate + " Hz");
        _output.WriteLine("rate asked for    : " + factory.Sink.RateAskedFor + " Hz");
        _output.WriteLine("outcome           : " + result!.Outcome);
        _output.WriteLine("run outcome       : " + result.Run?.Outcome);
        _output.WriteLine("sent              : " + result.Run?.Sent);
        _output.WriteLine("keyed             : " + result.Run?.Keyed);
        _output.WriteLine("came out of tx    : " + result.Run?.CameOutOfTransmit);
        _output.WriteLine("samples offered   : " + result.Run?.SamplesOffered);
        _output.WriteLine("samples played    : " + result.Run?.Played?.SamplesPlayed);
        _output.WriteLine("seconds offered   : " + result.Run?.SecondsOffered);
        _output.WriteLine("frames written    : " + port.Written.Count);

        // **WHAT THE OPERATOR IS TOLD.** `WentLine` puts `run.Reason` into the
        // reserved Send area verbatim; it is printed off the run rather than off
        // `DigitalSendLine` because the write is posted to the UI thread and
        // there is no dispatcher pumping in a headless test.
        _output.WriteLine("run reason        : " + result.Run?.Reason);
        _output.WriteLine(
            "operator would see: Hamlet did not send \"W1ABC KC3QIS -10\": "
            + result.Run?.Reason);

        // **THE RATE ASKED FOR IS THE RATE THE ENDPOINT DECLARES.**
        Assert.Equal(OrdinaryEndpointRate, factory.Sink.RateAskedFor);

        // AND A WHOLE TRANSMISSION WENT OUT.
        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);
        Assert.True(result.Run.Keyed);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, result.Run.CameOutOfTransmit);
        Assert.Equal(
            result.Run.SamplesOffered, result.Run.Played!.Value.SamplesPlayed);

        // 12.64 s AT 48000 Hz, WHICH IS THE SAME 12.64 s IT WAS AT 12000.
        Assert.Equal(12.64, result.Run.SecondsOffered, 3);
        Assert.Equal(
            (int)Math.Round(12.64 * OrdinaryEndpointRate), result.Run.SamplesOffered);

        // **AND NOTHING WAS EXPLAINED AWAY**, because there was nothing to
        // explain. `WentLine` puts `run.Reason` in front of the operator only
        // where the run did not send, and here it is empty.
        Assert.Equal(string.Empty, result.Run.Reason);

        // The reserved Send area is asserted at the click rather than at the
        // boundary: `AtSlotBoundaryAsync` posts its line to the UI thread, and
        // nothing pumps a dispatcher in a headless test.
        Assert.Contains("Sending to W1ABC", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **An endpoint whose rate FT8 cannot be built at is refused when it is
    /// chosen, and the radio is never keyed.**
    /// </summary>
    /// <remarks>
    /// <para>**THE REFUSAL HAS TO ARRIVE BEFORE THE KEYING FRAME.** The keying
    /// write at <c>Ft8TransmitSequence.cs:283</c> precedes the sink call at
    /// <c>:287</c>, so a rate that cannot work must be caught where the sink is
    /// built or the radio keys before anybody finds out.</para>
    /// <para>**NO ENDPOINT ON THIS MACHINE DECLARES SUCH A RATE** - all four
    /// declare 48000 Hz and <c>Ft8Composer.RateIsUsable</c> accepts it. The
    /// operator's machine is not this one, so the branch is written and proved
    /// against a constructed rate rather than left unreached and unwritten.</para>
    /// </remarks>
    [Fact]
    public void AnEndpointFt8CannotBeBuiltAtIsRefusedWhenItIsChosen()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;

        // A CHANNEL SYMBOL IS 0.16 s, SO 8001 x 0.16 IS NOT A WHOLE NUMBER OF
        // SAMPLES AND THE PORT REFUSES THE RATE.
        factory.Sink.DeclaredSampleRate = 8001;

        panel.BuildTheArmedSend(port);

        _output.WriteLine("send area at connect : " + panel.DigitalSendLine);

        // NOTHING IS ARMED AT ALL, SO THE CLICK CANNOT REACH A KEYING FRAME.
        Assert.False(panel.HasSomethingToTransmitThrough);

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine("send area on click   : " + panel.DigitalSendLine);
        _output.WriteLine("frames written       : " + port.Written.Count);
        _output.WriteLine("sink calls           : " + factory.Sink.TimesCalled);

        Assert.Null(panel.ArmedForSlotUtc);

        // **THE RADIO WAS NEVER KEYED AND THE SINK WAS NEVER TOUCHED.**
        Assert.True(port.WasNeverWrittenTo);
        Assert.True(factory.Sink.WasNeverTouched);

        // AND HE IS TOLD WHICH DEVICE, WHAT IT DECLARED, AND TO GO AND CHANGE IT.
        Assert.Contains(NamedEndpoint, panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains("8001", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "another transmit audio device", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The rate travels all the way into the composed transmission**, not just
    /// into the call.
    /// </summary>
    /// <remarks>
    /// <c>Ft8Transmission.SampleRate</c> is what
    /// <c>Ft8TransmitSequence</c> passes to the sink and what the telemetry record
    /// carries, so a rate that reached the sink by some other route would be a
    /// second site to drift.
    /// </remarks>
    [Fact]
    public async Task TheComposedTransmissionCarriesTheEndpointsRate()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        factory.Sink.DeclaredSampleRate = OrdinaryEndpointRate;

        panel.BuildTheArmedSend(port);
        panel.SendMessageCommand.Execute("CQ KC3QIS FN00");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);
        var sent = result!.Send!.Transmission;

        _output.WriteLine("transmission text : " + sent.Text);
        _output.WriteLine("transmission rate : " + sent.SampleRate + " Hz");
        _output.WriteLine("samples           : " + sent.Samples.Length);
        _output.WriteLine("slot seconds      : " + sent.SlotSeconds);

        Assert.Equal(OrdinaryEndpointRate, sent.SampleRate);
        Assert.Equal(OrdinaryEndpointRate, factory.Sink.RateAskedFor);
        Assert.Equal(12.64, sent.SlotSeconds, 3);
    }

    /// <summary>
    /// **A sink that declares 12000 Hz still gets 12000 Hz**, so the fix is not a
    /// second hard-coded number.
    /// </summary>
    /// <remarks>
    /// The failure this forbids: replacing one constant with another. What is
    /// wanted is *the rate the endpoint declares*, and the only way to see the
    /// difference is to ask two endpoints that declare different rates.
    /// </remarks>
    [Fact]
    public async Task AnEndpointSpeaking12000GetsTwelveThousand()
    {
        var (panel, settings, factory) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        factory.Sink.DeclaredSampleRate = 12_000;

        panel.BuildTheArmedSend(port);
        panel.SendMessageCommand.Execute("W1ABC KC3QIS RR73");

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        _output.WriteLine("endpoint declares : 12000 Hz");
        _output.WriteLine("rate asked for    : " + factory.Sink.RateAskedFor + " Hz");
        _output.WriteLine("sent              : " + result!.Run?.Sent);

        Assert.Equal(12_000, factory.Sink.RateAskedFor);
        Assert.True(result.Run!.Sent, result.Run.Reason);
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

    /// <summary>A sink factory that says what it was asked for and hands back a fake.</summary>
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
