using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// The sheet's `toneHz` line says whether the pitch is proved, a hypothesis or
/// none, on the 23 keyed recordings (HM-REQ-093; work instruction 450, task 2).
/// </summary>
/// <remarks>
/// <para>**EACH RECORDING THROUGH THE SHEET WRITER'S OWN CODE**, replayed as
/// <see cref="TheRestOfTheSheetIsTrueTests"/> replays it, the report taken at the
/// end of the file. Beside each line is what HEAD printed for the same report:
/// decoding is unchanged by this unit (its task 2 compares every recording's text),
/// so the pitch and the old flag are the same, and HEAD's line was a function of
/// them alone: "measured from the keying the survey admitted" wherever a pitch was
/// held, and the not-measured sentences, unchanged, wherever none was.</para>
/// <para>**IT ASSERTS THE WORDING, NOT THE STATE.** Which state a recording ends in
/// is the decoder's; this holds only that a hypothesis is never described as
/// measured from keying now and that each state is named.</para>
/// </remarks>
public sealed class ThePitchLineSaysWhatWasProvedTests
{
    /// <summary>The 23 keyed recordings of the metrics, by stamp.</summary>
    internal static IReadOnlyList<string> Keyed { get; } = new[]
    {
        "cw-2026-09-23-173723", "cw-2026-08-17-013347", "cw-2026-08-17-134712",
        "cw-2026-08-18-003758", "cw-2026-08-24-012403", "cw-2026-08-18-004507",
        "cw-2026-08-22-031838", "cw-2026-08-22-031905", "cw-2026-08-22-031948",
        "cw-2026-08-22-032012", "cw-2026-08-22-032050", "cw-2026-08-22-032113",
        "cw-2026-08-22-032129", "cw-2026-09-24-004108", "cw-2026-09-24-004133",
        "cw-2026-09-24-004205", "cw-2026-09-24-004234", "cw-2026-09-24-004322",
        "cw-2026-09-24-004347", "cw-2026-09-24-004405", "cw-2026-09-24-004427",
        "cw-2026-09-24-004510", "cw-2026-09-24-004550",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public ThePitchLineSaysWhatWasProvedTests(ITestOutputHelper output) => _output = output;

    /// <summary>HM-REQ-093 on the sheet: each state named, a hypothesis never called measured now.</summary>
    [Fact]
    public void EachKeyedRecordingsPitchLineNamesItsState()
    {
        var tally = new Dictionary<CwPitchProof, int>();
        var hypothesisWasMeasured = 0;

        foreach (var stamp in Keyed)
        {
            var (report, line) = PitchLine(stamp);
            var before = report.HasTone && report.PitchWasMeasured
                ? string.Create(CultureInfo.InvariantCulture, $"toneHz     {report.ToneHz:0.0} Hz  (measured from the keying the survey admitted: the centre of the survey bin it was admitted in, not interpolated between bins)")
                : line;

            tally[report.PitchProof] = tally.GetValueOrDefault(report.PitchProof) + 1;

            if (report.HasTone && report.PitchProof == CwPitchProof.Hypothesis)
            {
                hypothesisWasMeasured++;
            }

            _output.WriteLine($"sheet | {stamp} | state {report.PitchProof} | hasTone {report.HasTone}");
            _output.WriteLine($"  before | {before}");
            _output.WriteLine($"  after  | {line}");

            if (report.HasTone && report.PitchProof == CwPitchProof.Hypothesis)
            {
                Assert.Contains("HYPOTHESIS, NOT PROVED NOW", line, StringComparison.Ordinal);
                Assert.DoesNotContain("Measured from that keying", line, StringComparison.Ordinal);
                Assert.DoesNotContain("measured from the keying the survey admitted", line, StringComparison.Ordinal);
            }

            if (report.HasTone && report.PitchProof == CwPitchProof.Proved)
            {
                Assert.Contains("proved:", line, StringComparison.Ordinal);
            }
        }

        _output.WriteLine($"total | {Keyed.Count} keyed recordings | {string.Join(", ", Enum.GetValues<CwPitchProof>().Select(s => $"{s} {tally.GetValueOrDefault(s)}"))} | hypothesis where HEAD's line said measured from keying {hypothesisWasMeasured}");
    }

    private (CwDecodeReport Report, string Line) PitchLine(string stamp)
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

        return (report, sheet.Split(Environment.NewLine).Single(l => l.StartsWith("toneHz ", StringComparison.Ordinal)));
    }
}
