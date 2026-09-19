using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>The collection whose tests measure process CPU and so run with nothing beside them.</summary>
/// <remarks>
/// The app assembly already runs no two tests at once (`TestParallelism.cs`); this names the rule the
/// engine tests' collection of the same name states, for a class that asserts CPU.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class CpuMeasuredAlone
{
    /// <summary>The collection's name.</summary>
    public const string Name = "CPU measured alone";
}

/// <summary>
/// Work instruction 364 task 5: **the real-time ratio with the rows drawn** (step 3 criterion 3.6,
/// nice-to-pass), beside unit 363's 0.154 for the listener alone.
/// </summary>
/// <remarks>
/// <para>**THE WHOLE TICK, NOT THE LISTENER**: the two-signal file handed to the real tick through the
/// tap a quarter-second at a time - the capture check, the RSID detector the tab already ran, the
/// Olivia listener, the mapping, the PSK31 row path and the row events - at the fixture's own 8 kHz and
/// at a sound card's 48 kHz through the resampler, then the four blocks of quiet every rows test
/// feeds. Process CPU around the feed alone, over the audio's seconds.</para>
/// <para>**NOT ON THE CARRY-FORWARD LINE** (the instruction). **COMPUTED, NOT SEEN** (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaRowsKeepUpTests
{
    private const int TailBlocks = 4;

    private const double TailRms = 0.001;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the ratio is printed.</param>
    public TheOliviaRowsKeepUpTests(ITestOutputHelper output) => _output = output;

    /// <summary>**3.6: the two-signal file through the app with the rows drawn, under 1.0 of real time.**</summary>
    /// <param name="deviceRate">The rate the tap hands over: the fixture's own, and a sound card's.</param>
    [Theory]
    [InlineData(8_000)]
    [InlineData(48_000)]
    public void TheTwoSignalFileIsReadAndDrawnFasterThanItArrives(int deviceRate)
    {
        var audio = Fixture("olivia-two-signals-rsid.wav");
        var format = OliviaData.Current.Format!;
        var tail = Noise((int)(TailBlocks * format.SymbolsPerBlock * format.Variants.Max(v => v.SymbolSeconds) * audio.SampleRate));
        var all = audio.Samples.Concat(tail).ToArray();
        var hold = deviceRate / audio.SampleRate;
        var model = Listening();
        var piece = new float[deviceRate / 4];
        var filled = 0;
        var worst = 0.0;
        var worstAt = 0.0;
        var fed = 0L;
        var process = Process.GetCurrentProcess();

        process.Refresh();

        var before = process.TotalProcessorTime;
        var fileCpu = double.NaN;

        foreach (var sample in all)
        {
            for (var i = 0; i < hold; i++)
            {
                piece[filled++] = sample;

                if (filled == piece.Length)
                {
                    var started = Stopwatch.GetTimestamp();

                    model.TapForTests!.Take(piece, deviceRate);
                    model.LookForASlotForTests();

                    var took = Stopwatch.GetElapsedTime(started).TotalSeconds;

                    fed += piece.Length;
                    filled = 0;

                    if (took > worst)
                    {
                        worst = took;
                        worstAt = fed / (double)deviceRate;
                    }

                    if (double.IsNaN(fileCpu) && fed >= (long)audio.Samples.Length * hold)
                    {
                        process.Refresh();
                        fileCpu = (process.TotalProcessorTime - before).TotalSeconds;
                    }
                }
            }
        }

        process.Refresh();

        var cpu = (process.TotalProcessorTime - before).TotalSeconds;
        var seconds = fed / (double)deviceRate;
        var ratio = cpu / seconds;
        var rows = model.DigitalDecodes.Where(r => r.HasVariant).ToList();

        _output.WriteLine(
            $"device {deviceRate} Hz: {seconds:0.00} s of audio ({audio.Samples.Length / (double)audio.SampleRate:0.00} s file + quiet), "
            + $"cpu {cpu:0.000} s, ratio {ratio:0.000} (1.0); the file alone {fileCpu:0.000} s cpu, "
            + $"{fileCpu / (audio.Samples.Length / (double)audio.SampleRate):0.000}; longest single tick {worst:0.000} s wall, "
            + $"the piece ending at {worstAt:0.00} s; rows drawn {rows.Count}: {string.Join(", ", rows.Select(r => r.Variant + " at " + r.Hz))}");

        Assert.Equal(2, rows.Count);
        Assert.True(ratio < 1.0, $"ratio {ratio:0.000} is not under 1.0");
    }

    private static float[] Noise(int count)
    {
        var random = new Random(364);
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            samples[i] = (float)(TailRms * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        return samples;
    }

    private static MainWindowViewModel Listening()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "K1ABC";
        settings.Operator.GridSquare = "FN42";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.ChooseDigitalModeCommand.Execute("Olivia");

        return model;
    }

    private static MonoAudio Fixture(string file)
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        var folder = Path.Combine(at!.FullName, "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return WavAudio.Read(path);
    }
}
