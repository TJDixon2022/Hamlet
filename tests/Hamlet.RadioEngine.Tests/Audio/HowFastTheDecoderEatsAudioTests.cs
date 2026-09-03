using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 238, task 1.2: how much audio `CwDecoder.Process` can eat
/// per wall-clock second at 48 kHz.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT A RATCHET.** It asserts almost nothing.
/// The number it prints is the input to a decision about where the audio path
/// needs a queue, and a threshold here would turn a machine's speed into a
/// build failure on somebody else's laptop.</para>
/// <para>**AND IT IS AN INDICATION RATHER THAN THE FINDING** (`SHACK_FACTS.md`
/// FACT-004). There are two computers and only the shack machine has a radio.
/// A throughput number taken here says what this processor does with this audio;
/// it does not say what the shack machine does, and the arrival ratio that
/// matters is the counter reading the operator takes there.</para>
/// <para>**WHY 960-SAMPLE CHUNKS.** That is what `BufferedAudioSource` hands the
/// decoder in the application (HM-DEC-119), and `Process` is measurably a
/// different decoder at a different chunk size — the same ruling records 650 Hz
/// tracked at 240 samples against 500 Hz at 960 on one capture. Measuring at a
/// size the application never uses would be measuring a decoder the operator
/// never runs.</para>
/// <para>**THE AUDIO IS REAL OFF-AIR CW** (HM-DEC-091). Generated tone is
/// cheaper to decode than a band with noise and fading in it, so a throughput
/// figure taken on synthesized audio would flatter the decoder in exactly the
/// direction that matters here.</para>
/// </remarks>
public sealed class HowFastTheDecoderEatsAudioTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public HowFastTheDecoderEatsAudioTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The chunk the application actually delivers.</summary>
    private const int ChunkSamples = 960;

    /// <summary>How much audio to push through, in seconds.</summary>
    /// <remarks>
    /// Sixty, which the work instruction sets. The fixture is thirty seconds, so
    /// it is fed twice — the decoder is not reset between passes, because a
    /// decoder that has been running is the one the operator has.
    /// </remarks>
    private const int Seconds = 60;

    /// <summary>What `Process` costs per wall-clock second at 48 kHz.</summary>
    [Fact]
    public void ProcessThroughputAtFortyEightKilohertz()
    {
        var audio = WavAudio.Read(CapturePath("cw-2026-08-18-003016"));

        Assert.Equal(48_000, audio.SampleRate);

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var total = audio.SampleRate * Seconds;
        var pushed = 0L;
        var at = 0;

        // Warm the paths so the figure is steady-state rather than first-call
        // JIT. One second of audio, not counted.
        for (var w = 0; w + ChunkSamples <= audio.SampleRate; w += ChunkSamples)
        {
            decoder.Process(new AudioChunk(
                w, audio.SampleRate, audio.Samples.AsSpan(w, ChunkSamples)));
        }

        var clock = Stopwatch.StartNew();

        while (pushed < total)
        {
            if (at + ChunkSamples > audio.Samples.Length)
            {
                at = 0;
            }

            decoder.Process(new AudioChunk(
                pushed, audio.SampleRate, audio.Samples.AsSpan(at, ChunkSamples)));

            at += ChunkSamples;
            pushed += ChunkSamples;
        }

        clock.Stop();

        var elapsed = clock.Elapsed.TotalSeconds;
        var perSecond = pushed / elapsed;
        var ratio = perSecond / audio.SampleRate;

        _output.WriteLine("CwDecoder.Process throughput, 48 kHz, 960-sample chunks");
        _output.WriteLine("  audio pushed      : " + pushed + " samples ("
            + (pushed / (double)audio.SampleRate).ToString("0.0") + " s)");
        _output.WriteLine("  wall clock        : " + elapsed.ToString("0.00") + " s");
        _output.WriteLine("  samples / second  : " + perSecond.ToString("0"));
        _output.WriteLine("  real time is      : " + audio.SampleRate);
        _output.WriteLine("  ratio             : " + ratio.ToString("0.00") + "x real time");
        _output.WriteLine("");
        _output.WriteLine("  FACT-004: this is the development machine and it has no radio.");
        _output.WriteLine("  This figure is an indication about a different processor, not");
        _output.WriteLine("  the finding. The finding is the arrival counter on the shack");
        _output.WriteLine("  machine, and only the operator can take it.");

        // The only assertion: the harness actually ran. A throughput figure of
        // nought means the loop did not execute, which is the one way this
        // measurement could lie quietly.
        Assert.True(pushed >= total, "the harness did not push the audio it claims to have");
        Assert.True(elapsed > 0, "no wall clock elapsed, so the ratio is meaningless");
    }

    /// <summary>Where a captured fixture lives.</summary>
    private static string CapturePath(string capture)
    {
        var here = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(here))
        {
            var folder = Path.Combine(here, "tests", "fixtures", "cw", "captured");

            if (Directory.Exists(folder))
            {
                return Directory
                    .GetFiles(folder, capture + ".wav", SearchOption.AllDirectories)
                    .Single();
            }

            here = Path.GetDirectoryName(here.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        throw new DirectoryNotFoundException(
            "no captured fixtures folder above " + AppContext.BaseDirectory);
    }
}
