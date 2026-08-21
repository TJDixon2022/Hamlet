using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A recording an independent instrument says contains keying is not read as
/// silence (HM-DEC-091, HM-DEC-114).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE FIRST DECODE FAILURE THIS PROJECT CAN RUN A THOUSAND
/// TIMES.** `cw-2026-08-17-134712` was captured at 7.0119 MHz with a 500 Hz
/// filter and its own sidecar claims 35.2 dB. The decoder emitted nothing at all
/// from it: `107 seen, 0 resolved, 0 emitted`.</para>
/// <para>**THE SECOND OPINION IS WHAT MAKES THAT A DEFECT RATHER THAN AN
/// OPINION** (§12.5). `KeyingEnvelope` shares no code with the decoder, and
/// sweeping this recording it finds a six-second stretch scoring 0.37 at 500 Hz
/// with a 54 ms element, the highest score it has measured on any recording in
/// this repository, higher than the four captures that decoded.</para>
/// <para>**NOTHING HERE ASSERTS WHAT THE STATION SENT.** There is no answer key
/// and adjudicating the recording is Tim's ear, not a test's (§0.0). What is
/// asserted is that something was heard and something was said about it, which
/// is the claim the sidecar's three zeros contradict.</para>
/// </remarks>
public sealed class ARecordingWithKeyingInItIsReadTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public ARecordingWithKeyingInItIsReadTests(ITestOutputHelper output)
        => _output = output;

    private const string Name = "cw-2026-08-17-134712";

    private static MonoAudio Audio() => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, Name + ".wav"));

    /// <summary>Run the decoder over the whole recording, hop by hop.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="startHz">Where the tracker starts looking.</param>
    /// <returns>The decoder, having seen all of it.</returns>
    private static CwDecoder Decode(MonoAudio audio, double startHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return decoder;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091, and it is the measurement everything else in this
    /// question rests on: **there is keying in this recording**, found by an
    /// instrument that shares nothing with the decoder and cannot be talked into
    /// agreeing with it.</para>
    /// <para>Its element length is 54 ms, which is an ordinary hand speed and not
    /// a marginal one.</para>
    /// </remarks>
    [Fact]
    public void AnIndependentInstrumentFindsKeyingInIt()
    {
        var audio = Audio();
        var length = (int)(audio.SampleRate * CwKeyingThresholds.Window.TotalSeconds);
        var best = default(KeyingSighting?);

        for (var start = 0; start + length <= audio.Samples.Length; start += length)
        {
            var slice = new float[length];

            Array.Copy(audio.Samples, start, slice, 0, length);

            var window = KeyingEnvelope.Best(new MonoAudio(audio.SampleRate, slice));

            if (window is { } found
                && (best is null || found.Profile.Score > best.Value.Profile.Score))
            {
                best = found;
            }
        }

        Assert.NotNull(best);

        _output.WriteLine(
            $"best window: {best!.Value.ToneHz:0} Hz, "
            + $"{best.Value.Profile.MedianMs:0} ms, score {best.Value.Profile.Score:0.00}");

        Assert.True(
            best.Value.Profile.Score >= CwKeyingThresholds.KeyingScore,
            $"the meter scored this recording {best.Value.Profile.Score:0.00}");

        Assert.InRange(
            best.Value.Profile.MedianMs,
            CwKeyingThresholds.SlowestChatterMs,
            CwKeyingThresholds.LongestElementMs);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-114 and §0.0: **a recording with keying in it at
    /// 35 dB is read, or it is a defect.** The decoder emitted nothing from this
    /// one, which is silence standing for a signal that was there, and silence is
    /// the one output §0.0 permits only when nothing was heard.</para>
    /// <para>**IT ASSERTS THAT SOMETHING CAME OUT AND NOT WHAT.** The recording
    /// has no adjudicated answer key, so a test that checked the text would be
    /// asserting a transcript nobody has confirmed, and marked or unreadable
    /// characters count here exactly as readable ones do (HM-DEC-048).</para>
    /// </remarks>
    [Fact]
    public void TheDecoderSaysSomethingAboutIt()
    {
        var audio = Audio();
        var decoder = Decode(audio, 600);
        var report = decoder.Report;

        _output.WriteLine(
            $"tone {report.ToneHz:0} Hz, hasTone {report.HasTone}, "
            + $"snr {report.SnrDb:0.0} dB");
        _output.WriteLine(
            $"elements {report.ElementsSeen} seen, {report.ElementsResolved} resolved");
        _output.WriteLine(
            $"characters {report.CharactersEmitted} emitted, "
            + $"{report.CharactersUnsure} unsure");

        // The tone is found and elements are measured, which is why the silence
        // is a defect rather than an empty band: the decoder heard it and had
        // nothing to say.
        Assert.True(report.HasTone, "the tone was never even latched");

        // **THE ELEMENT COUNT USED TO BE A DIFFERENT INSTRUMENT'S.** Fifty was
        // the gate's own edges, and the gate is gone; what the field carries now
        // is elements the working decoder resolved, which is a count of what came
        // out rather than of what went in. Asserting the same number against a
        // different measurement would be a threshold kept for its own sake, and
        // the claim this test makes is the one below.

        Assert.True(
            report.CharactersEmitted > 0,
            $"nothing was emitted from {report.ElementsSeen} elements at "
            + $"{report.SnrDb:0.0} dB, and an independent instrument finds keying here");
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **where the tracker is told to start makes no
    /// difference here**, which kills the fifty-hertz lead before anything is
    /// built on it. The decoder reported 550 Hz and the meter chose 500; started
    /// at 600, 550 or 500 the tracker converges on 500 every time and the outcome
    /// is the same.</para>
    /// <para>A parameter rather than a changed default, so nothing outside a
    /// measurement ever sees anything else.</para>
    /// </remarks>
    /// <param name="startHz">Where the tracker starts looking.</param>
    [Theory]
    [InlineData(600)]
    [InlineData(550)]
    [InlineData(500)]
    public void WhereTheTrackerStartsDoesNotDecideThis(double startHz)
    {
        var decoder = Decode(Audio(), startHz);

        _output.WriteLine(
            $"from {startHz:0} Hz: settled on {decoder.Report.ToneHz:0} Hz, "
            + $"{decoder.Tracker.Retunes} retunes, "
            + $"{decoder.Report.ElementsSeen} elements, "
            + $"{decoder.Report.CharactersEmitted} characters");

        Assert.Equal(500, decoder.Report.ToneHz, 0);
    }
}
