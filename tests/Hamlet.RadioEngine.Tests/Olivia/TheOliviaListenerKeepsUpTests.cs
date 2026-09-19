using System;
using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 363 task 4: **the listener keeps up with the air** - its CPU over the audio's
/// seconds, fed a quarter-second at a time (step 3 criterion 3.6, measured on the engine).
/// </summary>
/// <remarks>
/// <para>**THE RATIO IS PROCESS CPU OVER AUDIO SECONDS**, around the feed alone - the file read
/// first - with the RSID detector, the streaming search and every channel's reader inside it.
/// **The longest single Add is printed beside it**, in wall seconds, because a listener that
/// averages under 1.0 and stalls for seconds once is not keeping up.</para>
/// <para>**MEASURED ON THE ENGINE**: the next unit measures it again with the rows drawn.</para>
/// <para>**RUN ALONE, BECAUSE THE CPU IS THE PROCESS'S** (<see cref="CpuMeasuredAlone"/>).
/// **COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaListenerKeepsUpTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each ratio is printed.</param>
    public TheOliviaListenerKeepsUpTests(ITestOutputHelper output) => _output = output;

    /// <summary>**3.6: on the two-signal file the listener's ratio is under 1.0.**</summary>
    [Fact]
    public void TheTwoSignalFileIsReadFasterThanItArrives()
    {
        var (ratio, _) = Ratio("olivia-two-signals-rsid.wav");

        Assert.True(ratio < 1.0, $"ratio {ratio:0.000} against 1.0");
    }

    /// <summary>The same ratio on the 131 s 16/500 file and on the noise, printed and not asserted.</summary>
    /// <param name="file">The fixture.</param>
    [Theory]
    [InlineData("olivia-16-500-qso-rsid.wav")]
    [InlineData("olivia-noise-only-30s.wav")]
    public void TheRatioElsewhereIsPrinted(string file) => Ratio(file);

    private (double Ratio, double WorstSeconds) Ratio(string file)
    {
        var audio = WavAudio.Read(OliviaFixtures.Load(file).Path);
        var listener = new OliviaListener(
            OliviaData.Current.Format!, OliviaData.Current.Rsid!, audio.SampleRate, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var piece = audio.SampleRate / 4;
        var worst = 0.0;
        var worstAt = 0.0;
        var process = Process.GetCurrentProcess();
        var before = process.TotalProcessorTime;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            var started = Stopwatch.GetTimestamp();

            listener.Add(audio.Samples.AsSpan(at, Math.Min(piece, audio.Samples.Length - at)));

            var took = Stopwatch.GetElapsedTime(started).TotalSeconds;

            if (took > worst)
            {
                worst = took;
                worstAt = at / (double)audio.SampleRate;
            }
        }

        listener.Flush();
        process.Refresh();

        var cpu = (process.TotalProcessorTime - before).TotalSeconds;
        var seconds = audio.Samples.Length / (double)audio.SampleRate;
        var ratio = cpu / seconds;

        _output.WriteLine(
            $"{file}: {seconds:0.00} s of audio in {piece}-sample pieces, cpu {cpu:0.000} s, ratio {ratio:0.000} (against 1.0); "
            + $"longest single Add {worst:0.000} s wall, on the piece at {worstAt:0.00} s; channels {listener.Channels.Count}");

        return (ratio, worst);
    }
}
