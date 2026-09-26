using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// The sheet's `decoderWpm` line and every other surface that shows the speed, on
/// the 23 keyed recordings (HM-REQ-034; work instruction 451, tasks 1 and 2).
/// </summary>
/// <remarks>
/// <para>**EACH RECORDING THROUGH THE APP'S OWN CODE**, replayed as
/// <see cref="ThePitchLineSaysWhatWasProvedTests"/> replays it and read at the end
/// of the file: the sheet from its writer, and the header, the reacquiring pill and
/// the transmit panel's offer from one decode tick of the view model.</para>
/// </remarks>
public sealed class TheSpeedLineSaysWhatWasProvedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheSpeedLineSaysWhatWasProvedTests(ITestOutputHelper output) => _output = output;

    /// <summary>Every speed surface's words, per keyed recording.</summary>
    [Fact]
    public void EachKeyedRecordingsSpeedLinesAsTheyRead()
    {
        foreach (var stamp in ThePitchLineSaysWhatWasProvedTests.Keyed)
        {
            var lines = SpeedLines(stamp);

            _output.WriteLine($"speed | {stamp} | shown {lines.Shown?.ToString() ?? "null"} | reacquiring {lines.Reacquiring}");
            _output.WriteLine($"  sheet   | {lines.Sheet}");
            _output.WriteLine($"  reading | {lines.Reading}");
            _output.WriteLine($"  header  | {lines.Header}");
            _output.WriteLine($"  pill    | {lines.Pill}");
            _output.WriteLine($"  offer   | {lines.Offer}");
        }
    }

    internal sealed record Lines(int? Shown, bool Reacquiring, string Sheet, string Reading, string Header, string Pill, string Offer);

    internal static Lines SpeedLines(string stamp)
    {
        var wav = Directory
            .GetFiles(
                Path.Combine(EverySentenceOnTheSheetTests.Root(), "tests", "fixtures", "cw", "captured"),
                stamp + ".wav",
                SearchOption.AllDirectories)
            .Single();
        var audio = WavAudio.Read(wav);
        var decoder = new CwDecoder(audio.SampleRate);

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
            decoder.Flush();
        }

        var report = decoder.Report;
        var sheet = EverySentenceOnTheSheetTests.Sheet(
            decoder,
            EverySentenceOnTheSheetTests.Meter(audio).Reading,
            audio,
            report,
            MainWindowViewModel.TonePeakRecordLine(audio, report),
            null);
        var rows = sheet.Split(Environment.NewLine);

        var model = new MainWindowViewModel(new AppSettings(), null);

        typeof(MainWindowViewModel).GetField("_decoder", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(model, decoder);
        typeof(MainWindowViewModel).GetMethod("OnDecodeTick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, [null, EventArgs.Empty]);

        return new Lines(
            decoder.WordsPerMinute,
            decoder.SpeedIsReacquiring,
            rows.Single(l => l.StartsWith("decoderWpm ", StringComparison.Ordinal)),
            rows.Single(l => l.StartsWith("reading ", StringComparison.Ordinal)),
            model.TerminalSpeedText,
            model.SpeedReacquiringText,
            model.Transmit.SpeedOffer);
    }
}
