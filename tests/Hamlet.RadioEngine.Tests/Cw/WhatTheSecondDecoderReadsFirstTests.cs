using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A first look at the second decoder (work instruction 456, task 4): what
/// <see cref="FldigiCwDecoder"/> prints on the synthetic set and on three keyed
/// captures, beside ours and beside the key. Proves no requirement.
/// </summary>
/// <remarks>
/// <para>**A PRINTER THAT ASSERTS NOTHING, AND NOT A SCORE.** HM-REQ-123 is
/// 9.2's, through the same scorer and metrics; nothing here counts or tables.</para>
/// <para>**THE PITCH IS THE INSTRUMENT'S**: the median of
/// <see cref="CwPitchInstrument"/>'s windows over the recording, not our
/// tracker's, given to the port as fldigi's waterfall cursor would give it.
/// The 48000 Hz captures reach the port through <see cref="FldigiRateAdapter"/>,
/// which is Hamlet's and not fldigi's.</para>
/// <para>**THE THREE CAPTURES**: 17:37, whose key is inferred and which holds
/// a red floor; `cw-2026-08-17-013347`, the adjudicated VA3VRR; and the first
/// of the ten benchmark recordings with a keyed stretch.</para>
/// </remarks>
public sealed class WhatTheSecondDecoderReadsFirstTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where each block is printed.</param>
    public WhatTheSecondDecoderReadsFirstTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One block per recording: the key, ours, the port's, and the pitch given.</summary>
    [Fact]
    public void BesideOursAndTheKey()
    {
        foreach (var recipe in SyntheticCq.All)
        {
            var path = Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav");
            Block("synthetic/" + recipe.Name, WavAudio.Read(path), SyntheticCq.Text, OursSynthetic(path));
        }

        var ten = WhatTheStrayLettersRestOnTests.KeyedRecordings.First(k => k.Set == "the ten").Name;
        var captures = new (string Name, string Key)[]
        {
            (TheSeventeenThirtySevenCaptureTests.Name, TheSeventeenThirtySevenCaptureTests.InferredKey + " (inferred)"),
            ("cw-2026-08-17-013347", "VA3VRR (adjudicated, a stretch)"),
            (ten, string.Join(" | ", TheBenchmarkIsKeyedTests.Stretches(ten).Select(s => s.Key)) + " (inferred stretches)"),
        };

        foreach (var (name, key) in captures)
        {
            var ours = CwReading.Of(TheSeventeenThirtySevenCaptureTests.Settle(name)).Text;
            Block(name, WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav")), key, ours);
        }
    }

    private void Block(string name, MonoAudio audio, string key, string ours)
    {
        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);

        _output.WriteLine($"== {name} | {audio.SampleRate} Hz | {audio.Samples.Length / (double)audio.SampleRate:0.0} s");
        _output.WriteLine($"key    | {key}");
        _output.WriteLine($"ours   | {ours}");

        if (windows.Count == 0)
        {
            _output.WriteLine("fldigi | not run: the pitch instrument found no keyed window");
            return;
        }

        var pitch = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
        var decoder = new FldigiCwDecoder(pitch);
        decoder.rx_process(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate));

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"fldigi | {decoder.Text}"));
        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"given  | {pitch:0.00} Hz, the instrument's median over {windows.Count} windows; fldigi's receive speed at the end {decoder.ReceiveSpeed} WPM"));
    }

    private static string OursSynthetic(string path)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return CwReading.Of(settled).Text;
    }
}
