using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 362 tasks 0 and 1: **the blind case's ground, and the trace before the search
/// is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit361Trace`. It prints what
/// the no-RSID file and the noise look like to something that knows nothing about them, the
/// format's seven rows, what a trial decode costs at a right and a wrong variant, and where drift
/// will bite. **It asserts nothing**, so it cannot become a wall.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit362Trace
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit362Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Task 0: the no-RSID file hashes as its manifest says, and the RSID detector hears nothing in it.</summary>
    [Fact]
    public void TheBlindFileAnnouncesNothing()
    {
        var fixture = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var audio = WavAudio.Read(fixture.Path);
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var heard = RsidDetector.Detect(OliviaData.Current.Rsid!, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

        _output.WriteLine(
            $"{Path.GetFileName(fixture.Path)}: hash matches the manifest; {audio.SampleRate} Hz, "
            + $"{audio.Samples.Length / (double)audio.SampleRate:0.00} s; RSID detections {heard.Count} "
            + $"over {Psk31CarrierSearch.PassbandLowHz}-{Psk31CarrierSearch.PassbandHighHz} Hz; detector cpu {cpu:0.000} s");

        foreach (var d in heard)
        {
            _output.WriteLine($"   detection: {d.Name} code {d.Code} at {d.CenterHz:0.00} Hz, {d.StartSeconds:0.000} s");
        }
    }
}
