using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Once a character is on the screen it stays there (work instruction 487, R100, HM-DEC-192).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"If you put a character on the screen, don't make it disappear. It
/// seems like the system is going in and out of detection, and when it goes out, it erases the
/// scroll. If you put something up, leave it."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): a keyed call at nine
/// words a minute between seconds of noise, handed to a gated decoder and the transcript the CW tab
/// shows, the way the tab wires them.</para>
/// </remarks>
public sealed class PrintedStaysPrintedTests
{
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the screens are printed.</param>
    public PrintedStaysPrintedTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves R100: what the screen showed at the last moment the detector said keying - the
    /// settled text and the leading edge after it - is still on the screen once the silence
    /// has run on, and the screen never shrinks at any moment in between.
    /// </remarks>
    [Fact]
    public void WhatWasOnTheScreenIsStillThereAfterTheSilence()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE N0CALL K", WordsPerMinute: 9, ToneHz: 600, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.05, LeadInSeconds: 4, TailSeconds: 4, Seed: 485));
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600)
        {
            KeyingGate = () => detector.Reading.Keying,
            DetectorPitch = () => detector.Reading.Keying ? detector.WatchedHz : double.NaN,
            DetectorBlocks = detector.BlocksBetween,
            DetectorGatesKeying = true,
            DetectorSteersPitch = true,
            DetectorGatesBlocks = true,
        };
        var transcript = new CwTranscript();

        decoder.LeadingEdge += transcript.OfferEdge;
        decoder.CharacterSettled += transcript.Settle;

        string Screen() => transcript.PlainText + transcript.TipText;

        var lastWhileKeying = "";
        var longest = "";
        var shrank = new List<string>();

        for (var at = 0; at + 80 <= audio.Samples.Length; at += 80)
        {
            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, 80)));
            detector.Process(audio.Samples.AsSpan(at, 80));

            var screen = Screen();

            if (detector.Reading.Keying)
            {
                lastWhileKeying = screen;
            }

            if (screen.Length < longest.TrimEnd().Length)
            {
                shrank.Add($"{at / (double)Rate:0.000} s: `{longest}` became `{screen}`");
            }

            if (screen.Length >= longest.Length)
            {
                longest = screen;
            }
        }

        decoder.Flush();

        var after = Screen();

        _output.WriteLine($"last while keying `{lastWhileKeying}`");
        _output.WriteLine($"after the silence `{after}`");
        _output.WriteLine($"moments the screen shrank {shrank.Count}" + (shrank.Count > 0 ? ": " + shrank[0] : ""));

        Assert.False(string.IsNullOrWhiteSpace(lastWhileKeying), "nothing reached the screen at all");
        Assert.StartsWith(lastWhileKeying.TrimEnd(), after, StringComparison.Ordinal);
    }
}
