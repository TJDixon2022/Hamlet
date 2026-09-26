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

    /// <summary>
    /// HM-REQ-034 on every speed surface: each state named, a hypothesis never
    /// presented as measured, and no number shown where HEAD withheld one.
    /// </summary>
    /// <remarks>
    /// **IT ASSERTS THE WORDING, NOT THE STATE.** Which state a recording ends in is
    /// the decoder's. HEAD's words for every recording are
    /// `.run-unit/unit451-app-speedlines-before.txt`, printed by this type before
    /// `src` changed; decoding is unchanged, so the number is the same.
    /// </remarks>
    [Fact]
    public void EachKeyedRecordingsSpeedLinesAsTheyRead()
    {
        var tally = new Dictionary<CwSpeedProof, int>();
        var hypothesisShown = 0;

        foreach (var stamp in ThePitchLineSaysWhatWasProvedTests.Keyed)
        {
            var lines = SpeedLines(stamp);

            tally[lines.State] = tally.GetValueOrDefault(lines.State) + 1;

            _output.WriteLine($"speed | {stamp} | shown {lines.Shown?.ToString() ?? "null"} | reacquiring {lines.Reacquiring} | state {lines.State}");
            _output.WriteLine($"  sheet   | {lines.Sheet}");
            _output.WriteLine($"  reading | {lines.Reading}");
            _output.WriteLine($"  header  | {lines.Header}");
            _output.WriteLine($"  pill    | {lines.Pill}");
            _output.WriteLine($"  offer   | {lines.Offer}");

            if (lines.Shown is { } wpm)
            {
                if (lines.State == CwSpeedProof.Proved)
                {
                    Assert.StartsWith($"decoderWpm {wpm}  (proved:", lines.Sheet, StringComparison.Ordinal);
                    Assert.Equal($"{wpm} WPM", lines.Header);
                    Assert.StartsWith("They are sending at about", lines.Offer, StringComparison.Ordinal);
                    Assert.Contains("; the speed is proved", lines.Reading, StringComparison.Ordinal);
                }
                else
                {
                    hypothesisShown++;
                    Assert.Equal(CwSpeedProof.Hypothesis, lines.State);
                    Assert.StartsWith($"decoderWpm {wpm}  HYPOTHESIS, NOT PROVED NOW (", lines.Sheet, StringComparison.Ordinal);
                    Assert.Equal($"{wpm} WPM, not proved", lines.Header);
                    Assert.DoesNotContain("They are sending at", lines.Offer, StringComparison.Ordinal);
                    Assert.Contains("not proved", lines.Offer, StringComparison.Ordinal);
                    Assert.Contains("HYPOTHESIS, NOT PROVED NOW", lines.Reading, StringComparison.Ordinal);
                }
            }
            else
            {
                // No number anywhere HEAD withheld one (work instruction 451, 3 (a)).
                Assert.NotEqual(CwSpeedProof.Proved, lines.State);
                Assert.Equal("", lines.Header);
                Assert.Equal("", lines.Offer);
                Assert.StartsWith($"decoderWpm {(lines.State == CwSpeedProof.Hypothesis ? "hypothesis" : "none")}, ", lines.Sheet, StringComparison.Ordinal);
            }
        }

        _output.WriteLine($"total | {ThePitchLineSaysWhatWasProvedTests.Keyed.Count} keyed recordings at the end of the file | {string.Join(", ", Enum.GetValues<CwSpeedProof>().Select(s => $"{s} {tally.GetValueOrDefault(s)}"))} | a hypothesis where HEAD showed the number bare {hypothesisShown}");
    }

    internal sealed record Lines(int? Shown, bool Reacquiring, CwSpeedProof State, string Sheet, string Reading, string Header, string Pill, string Offer);

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
            report.SpeedProof,
            rows.Single(l => l.StartsWith("decoderWpm ", StringComparison.Ordinal)),
            rows.Single(l => l.StartsWith("reading ", StringComparison.Ordinal)),
            model.TerminalSpeedText,
            model.SpeedReacquiringText,
            model.Transmit.SpeedOffer);
    }
}
