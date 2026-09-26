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
/// HM-REQ-035: when the transcript is cleared, the decoder retains its speed,
/// pitch and noise-floor state. Verification row 035 (work instruction 451, task
/// 2; PHASE_PLAN.md 5.5). Measured, not repaired: red is committed red.
/// </summary>
/// <remarks>
/// <para>**THE CLEAR IS THE OPERATOR'S OWN COMMAND**, `ClearTerminalCommand`, on a
/// view model holding the decoder being fed and a transcript filled from it the
/// way the application wires it (`MainWindowViewModel.cs` 11224-11225).</para>
/// <para>**THE SEND IS `cq-18wpm-15db`, EXACT BY CONSTRUCTION**: 18 wpm, 615 Hz,
/// 15 dB over the generator's shaped noise band (V-06), from its key file. The
/// clear is at 12 s, inside the message, and decoding carries on to the end of
/// the file, the same station throughout. Nothing is read back from the decoder
/// to decide where.</para>
/// <para>**THE RESOLUTION IS EXACT, AND THAT IS TIGHTER THAN THE DECODER'S OWN.**
/// The decoder's resolutions are one word a minute for the speed (the rounding in
/// <see cref="CwDecoder.WordsPerMinute"/>), the tracker's 5 Hz fine spacing for
/// the pitch, and 0.005 dB a hop for the held figure over the band's noise
/// (`CwDecoder.cs` 99). The state just before the clear and just after it are
/// read with no audio between them, and the rest of the file is compared hop for
/// hop with a twin decoder fed the same audio and never cleared; the decoder is
/// deterministic, so any difference at all is the clear's.</para>
/// <para>**WHAT THIS DOES NOT PROVE** (12.5): one clean textbook sender; a clear
/// in the middle of a fade or a follow is not it.</para>
/// </remarks>
public sealed class AClearKeepsWhatTheDecoderWorkedOutTests
{
    /// <summary>Where the clear falls, in seconds from the start of the file.</summary>
    private const double ClearAtSeconds = 12.0;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the case is printed.</param>
    public AClearKeepsWhatTheDecoderWorkedOutTests(ITestOutputHelper output) => _output = output;

    /// <summary>What the decoder holds, as the requirement names it.</summary>
    private sealed record State(int? Wpm, double RollingWpm, double ToneHz, CwPitchProof PitchProof, double SnrDb, bool HasTone, bool Reacquiring)
    {
        public override string ToString()
            => string.Create(CultureInfo.InvariantCulture,
                $"speed {Wpm?.ToString(CultureInfo.InvariantCulture) ?? "null"} (rolling {RollingWpm:0.0}), pitch {ToneHz:0.0} {PitchProof}, held SNR {SnrDb:0.000} dB, tone {HasTone}, reacquiring {Reacquiring}");
    }

    /// <summary>HM-REQ-035: the clear leaves speed, pitch and noise floor as they were.</summary>
    [Fact]
    public void HmReq035AClearKeepsTheSpeedThePitchAndTheNoiseFloor()
    {
        var audio = WavAudio.Read(Path.Combine(
            EverySentenceOnTheSheetTests.Root(), "tests", "fixtures", "cw", "synthetic-cq", "cq-18wpm-15db.wav"));

        var model = new MainWindowViewModel(new AppSettings(), null);
        var cleared = new CwDecoder(audio.SampleRate, 600);
        var twin = new CwDecoder(audio.SampleRate, 600);

        typeof(MainWindowViewModel).GetField("_decoder", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(model, cleared);
        cleared.LeadingEdge += model.Transcript.OfferEdge;
        cleared.CharacterSettled += model.Transcript.Settle;

        var hop = cleared.Tracker.HopSamples;
        var clearAt = (long)(ClearAtSeconds * audio.SampleRate);
        var done = false;
        var after = 0;
        var differ = new List<string>();
        State? before = null;
        State? right = null;
        var transcriptBefore = "";

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            var chunk = new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop));

            cleared.Process(chunk);
            twin.Process(chunk);

            if (!done && at + hop >= clearAt)
            {
                before = Of(cleared);
                transcriptBefore = model.Transcript.Tail(40);
                model.ClearTerminalCommand.Execute(null);
                right = Of(cleared);
                done = true;
                continue;
            }

            if (done)
            {
                after++;

                var a = Of(cleared);
                var b = Of(twin);

                if (a != b && differ.Count < 5)
                {
                    differ.Add(string.Create(Invariant, $"{(at + hop) / (double)audio.SampleRate:0.000} s: cleared {a} | twin {b}"));
                }
            }
        }

        Assert.NotNull(before);

        _output.WriteLine($"HM-REQ-035 | the transcript before the clear | \"{transcriptBefore}\" | after | \"{model.Transcript.Tail(40)}\" (the rest of the file)");
        _output.WriteLine($"HM-REQ-035 | just before the clear | {before}");
        _output.WriteLine($"HM-REQ-035 | just after the clear  | {right}");

        var speedNamed = before!.Wpm is not null;
        var same = before == right;
        var twinSame = differ.Count == 0;

        _output.WriteLine($"HM-REQ-035 | a speed named before the clear | {(speedNamed ? "green" : "RED")} | {before.Wpm?.ToString(Invariant) ?? "null"}");
        _output.WriteLine($"HM-REQ-035 | speed, pitch and noise floor across the clear | {(same ? "green" : "RED")} | {(same ? "identical" : "changed")}");
        _output.WriteLine($"HM-REQ-035 | the rest of the file against the twin | {(twinSame ? "green" : "RED")} | {after} hops compared, {(twinSame ? "all identical" : string.Join("; ", differ))}");

        Assert.True(speedNamed, "HM-REQ-035: no speed was named before the clear, so the case does not test what it states");
        Assert.True(same, $"HM-REQ-035 red: before {before}, after {right}");
        Assert.True(twinSame, "HM-REQ-035 red: " + string.Join("; ", differ));
    }

    private static State Of(CwDecoder decoder)
    {
        var report = decoder.Report;

        return new State(decoder.WordsPerMinute, decoder.Reading.WordsPerMinute, report.ToneHz, report.PitchProof, report.SnrDb, report.HasTone, decoder.SpeedIsReacquiring);
    }
}
