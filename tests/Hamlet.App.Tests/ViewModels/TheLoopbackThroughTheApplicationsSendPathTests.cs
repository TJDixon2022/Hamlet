using System.Diagnostics;
using Ft8Sharp.Dsp;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using NAudio.CoreAudioApi;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE LOOPBACK, DRIVEN BY THE APPLICATION'S OWN SEND PATH.** Work instruction
/// 262, task 5.
/// </summary>
/// <remarks>
/// <para>**WHAT UNIT 256 PROVED AND WHAT IT DID NOT.** Its loopback - compose,
/// play, capture, resample, decode - composed at the endpoint's rate **inside its
/// own test**, calling <c>Ft8Composer.ComposeSignal(text, sink.EndpointSampleRate)</c>
/// directly. It never touched a caller, and the caller is where the defect lived:
/// <c>MainWindowViewModel</c> composed at 12000 Hz and every send through a real
/// endpoint threw after the radio was keyed. **This is the same chain with the
/// application driving it** - the click, <c>BuildTheArmedSend</c>, the slot
/// boundary, and a real <c>WasapiTransmitSink</c>.</para>
/// <para>**IT MAKES REAL SOUND ON A REAL COMPUTER.** One transmission, 12.64
/// seconds of FT8 tones out of a sound card, and the run says which endpoint and
/// at what peak. One message, not three.</para>
/// <para>**NOTHING HERE OPENS A SERIAL PORT OR KEYS ANYTHING** (`SHACK_FACTS.md`
/// FACT-004). The device in this test is a sound card and the port is
/// <see cref="FakePort"/>. A real port beside a real sink would be a real
/// transmission and this machine has no radio to make one on. **Nothing measured
/// here says anything about the IC-7300.**</para>
/// <para>**A MACHINE WITH NO RENDER ENDPOINT IS A NORMAL MACHINE.** Where there
/// is none the test says so and stops, which is unit 256's precedent and is a
/// fact about the machine rather than a failure.</para>
/// </remarks>
public sealed class TheLoopbackThroughTheApplicationsSendPathTests
{
    /// <summary>How long to let the capture settle before the click.</summary>
    private static readonly TimeSpan PreRoll = TimeSpan.FromSeconds(1);

    /// <summary>How long to keep capturing after the transmission returns.</summary>
    private static readonly TimeSpan PostRoll = TimeSpan.FromSeconds(1);

    /// <summary>One FT8 slot, which is what the decoder's geometry describes.</summary>
    private const double SlotSeconds = 15.0;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the run is reported.</param>
    public TheLoopbackThroughTheApplicationsSendPathTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **One click, out of this machine's sound card, and back through Hamlet's
    /// own decoder as the same message.**
    /// </summary>
    [Fact]
    public async Task AMessageTheApplicationSentLeavesThisMachineAndComesBack()
    {
        var endpoint = Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine("NOT ATTEMPTED: " + why + " (SHACK_FACTS.md FACT-004)");
            _output.WriteLine(
                "That is a fact about this machine and not a failure. The wiring is "
                + "proved against the faithful fake by "
                + nameof(TheSendPathComposesAtTheEndpointsRateTests) + ".");

            return;
        }

        _output.WriteLine("chosen because   : " + why);
        _output.WriteLine("endpoint         : " + endpoint.Name);
        _output.WriteLine("endpoint declares: " + endpoint.SampleRate + " Hz");

        const string Message = "CQ KC3QIS FN00";

        var (panel, settings) = Panel();
        var port = new FakePort();

        // **THE REAL SINK, THROUGH THE APPLICATION'S OWN FACTORY.** This is the
        // one test in this project that lets a `WasapiTransmitSink` be built, and
        // it builds it the way the application does - from the endpoint named in
        // Settings.
        WasapiTransmitSink? sink = null;

        panel.TransmitSinkFactory = name =>
        {
            sink = new WasapiTransmitSink(name);

            return sink;
        };

        settings.AudioOutputDeviceId = endpoint.Id;

        try
        {
            panel.BuildTheArmedSend(port);

            Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);
            Assert.NotNull(sink);

            _output.WriteLine("sink opened      : " + sink!.DeviceName);
            _output.WriteLine("sink will accept : " + sink.EndpointSampleRate + " Hz");

            var tap = new AudioTap();
            var mono = Array.Empty<float>();

            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDevice(endpoint.Id);
            using var capture = new NAudio.Wave.WasapiLoopbackCapture(device);
            using var stopped = new ManualResetEventSlim(false);

            capture.DataAvailable += (_, e) =>
            {
                if (e.BytesRecorded <= 0)
                {
                    // An idle endpoint delivers empty packets; they are not
                    // silence to be recorded, they are nothing arriving.
                    return;
                }

                // **THE ENGINE'S OWN DOWNMIX, NOT A SECOND ONE.** The same
                // conversion the receive path uses on every buffer the radio
                // delivers.
                var frames = WasapiAudioSource.Downmix(
                    e.Buffer, e.BytesRecorded, capture.WaveFormat, ref mono);

                if (frames > 0)
                {
                    tap.Take(mono.AsSpan(0, frames), capture.WaveFormat.SampleRate);
                }
            };

            capture.RecordingStopped += (_, _) => stopped.Set();

            var clock = Stopwatch.StartNew();

            capture.StartRecording();
            await Task.Delay(PreRoll);

            // **THE CLICK.** Everything from here is the application's.
            panel.SendMessageCommand.Execute(Message);

            var slot = panel.ArmedForSlotUtc;

            Assert.True(slot is not null, panel.DigitalSendLine);

            var result = await panel.AtSlotBoundaryAsync(slot!.Value);

            await Task.Delay(PostRoll);
            capture.StopRecording();
            stopped.Wait(TimeSpan.FromSeconds(5));
            clock.Stop();

            var run = result!.Run;
            var sent = result.Send!.Transmission;
            var played = run?.Played ?? new PlayedAudio(0, TimeSpan.Zero);

            _output.WriteLine(string.Empty);
            _output.WriteLine("message          : " + sent.Text);
            _output.WriteLine("composed at      : " + sent.SampleRate + " Hz");
            _output.WriteLine("samples composed : " + sent.Samples.Length);
            _output.WriteLine("rate asked       : " + sink.RateAsked + " Hz");
            _output.WriteLine("rate got         : " + sink.RateGot + " Hz");
            _output.WriteLine("outcome          : " + run?.Outcome);
            _output.WriteLine("reason           : " + run?.Reason);
            _output.WriteLine("keyed            : " + run?.Keyed);
            _output.WriteLine("came out of tx   : " + run?.CameOutOfTransmit);
            _output.WriteLine("samples played   : " + played.SamplesPlayed);
            _output.WriteLine("play took        : " + played.Took.TotalSeconds.ToString("F2") + " s");
            _output.WriteLine("peak written     : " + sink.PeakWritten.ToString("F4"));
            _output.WriteLine("rms written      : " + sink.RmsWritten.ToString("F4"));
            _output.WriteLine("clipped samples  : " + sink.ClippedSamples);
            _output.WriteLine("frames on wire   : " + port.Written.Count);
            _output.WriteLine("wall clock       : " + clock.Elapsed.TotalSeconds.ToString("F2") + " s");

            // **THE SEND PATH RAN WHOLE.**
            Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
            Assert.True(run!.AudioWentOut, run.Reason);
            Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);

            // **AND IT ASKED FOR THE RATE THE ENDPOINT DECLARES**, which is the
            // whole of what this unit changed.
            Assert.Equal(endpoint.SampleRate, sent.SampleRate);
            Assert.Equal(sink.RateGot, sink.RateAsked);
            Assert.Equal(sent.Samples.Length, played.SamplesPlayed);

            var captured = tap.Snapshot();

            Assert.True(
                captured is not null && captured.Samples.Length > 0,
                "the loopback capture started and delivered nothing while the endpoint was "
                + $"rendering - {tap.SamplesSeen} samples reached the tap");

            _output.WriteLine("captured at      : " + captured!.SampleRate + " Hz");
            _output.WriteLine("captured samples : " + captured.Samples.Length);

            // **THE CAPTURE, PLACED AT THE START OF ONE SLOT.** 12.64 s of signal
            // with nothing before it; the rest of the slot is silence, which is the
            // shape the composer's own padded slot has.
            var window = new float[(int)Math.Round(SlotSeconds * captured.SampleRate)];
            Array.Copy(
                captured.Samples, window, Math.Min(captured.Samples.Length, window.Length));

            var onTheGrid = Ft8Resample.ToFt8Rate(new MonoAudio(captured.SampleRate, window));
            var texts = new Ft8SlotDecoder().Decode(onTheGrid.Samples).Texts;

            _output.WriteLine(string.Empty);
            _output.WriteLine("expected         : " + sent.ReadsBackAs);
            _output.WriteLine(
                "decoder returned : "
                + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));

            // **§0.0. A decode that did not happen is reported as one that did not
            // happen**, with what did come back.
            Assert.True(
                texts.Contains(sent.ReadsBackAs, StringComparer.Ordinal),
                $"the message \"{sent.Text}\" was clicked, went out of {sink.DeviceName} at "
                + $"{sent.SampleRate} Hz and the decoder returned "
                + $"{(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")} "
                + $"from the captured audio, which peaked at {sink.PeakWritten:F4} on the way out.");
        }
        finally
        {
            sink?.Dispose();
        }
    }

    /// <summary>
    /// The endpoint this test will make sound on, and why it was chosen.
    /// </summary>
    /// <remarks>
    /// **CHOSEN, NOT DEFAULTED TO**, on unit 256's reasoning: this plays 12.64
    /// seconds of FT8 tones out of a computer somebody owns and may be asleep
    /// near, so the endpoint is picked deliberately and named in the output. A
    /// display-audio endpoint is preferred because it is a monitor's audio path
    /// rather than the machine's speakers, and because it is not the default -
    /// which means a sink that quietly fell back to the default would show up as a
    /// different name here rather than as a passing test.
    /// </remarks>
    private static RenderEndpoint? Preferred(out string why)
    {
        var endpoints = WasapiTransmitSink.Endpoints();

        if (endpoints.Count == 0)
        {
            why = "this machine has no active render endpoint at all";

            return null;
        }

        var quiet = endpoints.FirstOrDefault(
            e => e.Name.Contains("Display Audio", StringComparison.OrdinalIgnoreCase));

        if (quiet is not null)
        {
            why = "a display-audio endpoint - a monitor's audio path rather than the "
                + $"machine's speakers - chosen from {endpoints.Count} active render endpoints, "
                + $"and it is {(quiet.IsDefault ? "also" : "not")} the default";

            return quiet;
        }

        var chosen = endpoints.FirstOrDefault(e => !e.IsDefault) ?? endpoints[0];

        why = "no display-audio endpoint on this machine, so "
            + $"{(chosen.IsDefault ? "the default" : "the first endpoint that is not the default")} "
            + $"was taken from {endpoints.Count} active render endpoints - THIS MAY BE AUDIBLE";

        return chosen;
    }

    /// <summary>A panel on 20 m with a licence that permits.</summary>
    private static (MainWindowViewModel Panel, AppSettings Settings) Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var panel = new MainWindowViewModel(settings, null);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        return (panel, settings);
    }
}
