using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 418 task 1: the whole capture sheet regenerated through the
/// writer's own code, every line printed beside what the code that produced it did.
/// </summary>
/// <remarks>
/// <para>**A FACT THAT ASSERTS NOTHING.** It builds a view model, puts a decoder
/// that heard only this file behind it and the keying meter's last reading over this
/// file beside it, and calls the sheet writer, <c>CaptureNotes</c>, exactly as the
/// press does. Nothing is written into the capture folder and no capture in the
/// tree is edited.</para>
/// <para>**WHAT THE REGENERATED SHEET CANNOT SHOW.** No radio is attached, so the
/// rig block and the frequency, band and broadcast lines take their unread
/// branches, and no evening's counters exist, so <c>inThis</c> takes its underived
/// one. Those lines are judged from the code and the saved sheets, not from this
/// print.</para>
/// </remarks>
public sealed class EverySentenceOnTheSheetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sheets are printed.</param>
    public EverySentenceOnTheSheetTests(ITestOutputHelper output) => _output = output;

    /// <summary>Three sheets, each line beside what produced it.</summary>
    [Fact]
    public void EachLineIsPrintedBesideWhatTheCodeDid()
    {
        var root = Root();
        var captured = Path.Combine(root, "tests", "fixtures", "cw", "captured");

        string[] stamps =
        [
            "cw-2026-09-23-173723",
            "cw-2026-08-22-014113",
            "cw-2026-08-17-013347",
        ];

        _output.WriteLine("== what the code says, the same for every sheet");
        _output.WriteLine(
            $"keying sweep: KeyingEnvelope.LowestToneHz {KeyingEnvelope.LowestToneHz}, "
            + $"HighestToneHz {KeyingEnvelope.HighestToneHz}, ToneStepHz {KeyingEnvelope.ToneStepHz}; "
            + $"CwToneTracker.MinimumToneHz {CwToneTracker.MinimumToneHz}, "
            + $"MaximumToneHz {CwToneTracker.MaximumToneHz}");
        _output.WriteLine(
            $"keying window: CwKeyingThresholds.Window {CwKeyingThresholds.Window.TotalSeconds} s; "
            + $"the meter looks every {Constant<TimeSpan>(typeof(MainWindowViewModel), "KeyingMeterEvery")} "
            + $"and turns to no keying after {CwKeyingThresholds.QuietWindowsBeforeNoKeying} quiet windows");
        _output.WriteLine(
            $"level meter: one level every {Constant<double>(typeof(AudioTap), "LevelSeconds")} s; "
            + $"floor falls at {Constant<double>(typeof(AudioTap), "FloorFallAlpha")} and rises at "
            + $"{Constant<double>(typeof(AudioTap), "FloorRiseAlpha")} a level");
        _output.WriteLine(
            $"survey bins: coarse {Constant<double>(typeof(CwToneTracker), "CoarseSpacingHz")} Hz, "
            + $"fine {Constant<double>(typeof(CwToneTracker), "FineSpacingHz")} Hz");
        _output.WriteLine(
            $"reading: the window is {CwProbabilisticStream.WindowSeconds} s, read again every "
            + $"{CwProbabilisticStream.ReadEverySeconds} s");

        foreach (var stamp in stamps)
        {
            var wav = Directory
                .GetFiles(captured, stamp + ".wav", SearchOption.AllDirectories)
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
            var meter = Meter(audio);

            var clock = Stopwatch.StartNew();
            var tonePeak = MainWindowViewModel.TonePeakRecordLine(audio, report);
            clock.Stop();

            var sheet = Sheet(decoder, meter.Reading, audio, report, tonePeak);

            _output.WriteLine("");
            _output.WriteLine($"== {stamp}, the sheet as the writer composes it today");

            foreach (var line in sheet.Split(Environment.NewLine))
            {
                _output.WriteLine("  | " + line);
            }

            _output.WriteLine($"== {stamp}, beside the code");

            var peak = audio.Samples.Max(s => Math.Abs((double)s));
            var atFullScale = audio.Samples.Count(s => Math.Abs((double)s) >= 0.999);
            var level = (int)(audio.SampleRate * 0.2);
            var blocks = Enumerable.Range(0, audio.Samples.Length / level)
                .Select(b => Rms(audio.Samples, b * level, level))
                .ToArray();

            _output.WriteLine(
                $"clipping: the sheet's figure is the tap's last {0.2} s level window, "
                + $"clipped {report.Level.Clipping}; over the whole recording the peak is "
                + $"{Db(peak):0.0} dBFS and {atFullScale} samples sit at or past 0.999");
            _output.WriteLine(
                $"meterPeak: {report.Level.PeakDb:0.0} dBFS over that last window; inputPeak over "
                + $"the whole recording {AudioTap.PeakOf(audio):0.0}");
            _output.WriteLine(
                $"inputFloor: the tap's running floor at the end {report.Level.FloorDb:0.0} dBFS; "
                + $"the recording's own quietest {0.2} s block {blocks.Min():0.0} dBFS, "
                + $"median block {blocks.OrderBy(b => b).ElementAt(blocks.Length / 2):0.0}, "
                + $"first block {blocks[0]:0.0}");
            _output.WriteLine(
                $"toneHz: {report.ToneHz.ToString("0.000", CultureInfo.InvariantCulture)}, "
                + $"measured {report.PitchWasMeasured}, hasTone {report.HasTone}; "
                + $"remainder on the 5 Hz fine grid {Math.IEEERemainder(report.ToneHz, 5):0.000}");
            _output.WriteLine(
                $"tonePeak: measured over this file in {clock.ElapsedMilliseconds} ms, which is how "
                + "long the press now waits before the sheet is composed");
            _output.WriteLine(
                $"competing: HasKeying {report.HasKeying}, competitor "
                + $"{(report.Competitor is { } c ? $"{c.ToneHz:0}" : "none")}, interference "
                + $"{(report.Interference is { } i ? $"{i.ToneHz:0} Hz lift {i.LiftDb:0.0} present {i.PresentFraction:0.00}" : "none")}");

            if (report.PitchWasMeasured)
            {
                var profile = KeyingEnvelope.Measure(audio, report.ToneHz);
                var chatter = profile.RunsMs.Where(r => r < KeyingEnvelope.ShortestElementMs).Sum();
                var span = audio.Samples.Length * 1000.0 / audio.SampleRate;

                _output.WriteLine(
                    $"duty: {profile.Duty * 100:0.0}% of the file above the envelope's threshold at "
                    + $"{report.ToneHz:0.0} Hz; element-length runs {profile.ElementShare * 100:0.0}%, "
                    + $"runs shorter than an element {chatter / span * 100:0.0}%, "
                    + $"{profile.RunsMs.Count} runs");
            }

            _output.WriteLine(
                $"keying: the meter's last reading {meter.Reading.Verdict} at {meter.Reading.ToneHz:0} Hz, "
                + $"held {meter.Reading.Held}, over the {CwKeyingThresholds.Window.TotalSeconds} s window "
                + "ending at the file's last whole second; the meter reads the decoder's own tap "
                + "(CwKeyingMeter.Update(AudioTap)) and sweeps the tracker's own range");
            _output.WriteLine(
                $"reading: {decoder.Reading.WordsPerMinute:0.0} WPM, ratio {decoder.Reading.LikelihoodRatio:0.0}; "
                + $"decoderWpm {decoder.WordsPerMinute?.ToString(CultureInfo.InvariantCulture) ?? "none"}");

            var saved = Path.ChangeExtension(wav, ".txt");

            if (File.Exists(saved))
            {
                _output.WriteLine($"== {stamp}, the saved sheet's signal lines, as the evening wrote them");

                foreach (var line in File.ReadLines(saved).TakeWhile(l => !l.StartsWith("Frequency ", StringComparison.Ordinal)))
                {
                    _output.WriteLine("  : " + line);
                }
            }
        }
    }

    /// <summary>The sheet exactly as the press composes it.</summary>
    internal static string Sheet(
        CwDecoder decoder, KeyingReading keying, MonoAudio audio, CwDecodeReport report, string tonePeak)
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        Field("_decoder").SetValue(model, decoder);
        Field("_keyingReading").SetValue(model, keying);

        var writer = typeof(MainWindowViewModel).GetMethod(
            "CaptureNotes", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(writer);

        return (string)writer!.Invoke(
            model, [audio, decoder.Tap.SamplesSeen, report, tonePeak])!;
    }

    /// <summary>The keying meter over this file, one six-second window a second.</summary>
    internal static CwKeyingMeter Meter(MonoAudio audio)
    {
        var meter = new CwKeyingMeter();
        var window = (int)(CwKeyingThresholds.Window.TotalSeconds * audio.SampleRate);

        for (var end = window; end <= audio.Samples.Length; end += audio.SampleRate)
        {
            meter.Update(new MonoAudio(audio.SampleRate, audio.Samples[(end - window)..end]));
        }

        return meter;
    }

    private static FieldInfo Field(string name)
    {
        var field = typeof(MainWindowViewModel).GetField(
            name, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);

        return field!;
    }

    private static T Constant<T>(Type type, string name)
    {
        var field = type.GetField(
            name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(field);

        return field!.IsLiteral ? (T)field.GetRawConstantValue()! : (T)field.GetValue(null)!;
    }

    private static double Rms(float[] samples, int start, int count)
    {
        var sum = 0.0;

        for (var i = start; i < start + count; i++)
        {
            sum += samples[i] * (double)samples[i];
        }

        return Db(Math.Sqrt(sum / count));
    }

    private static double Db(double magnitude) => 20 * Math.Log10(Math.Max(magnitude, 1e-12));

    internal static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
