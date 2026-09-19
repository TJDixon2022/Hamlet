using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 360 task 4: **step 2's ground, measured and nothing built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit359Trace`. It detects the
/// clean 16/500 fixture's burst - step 2's entry check - and parses `data/olivia/timing.json` by
/// machine against the manifest's seconds and texts (unit 358 item 5). **It asserts nothing**, so
/// it cannot become a wall.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit360Trace
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit360Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Print the entry check and the timing table against the manifest.</summary>
    [Fact]
    public void StepTwosGround()
    {
        var root = Root();
        var fixtures = Path.Combine(root, "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixtures, "manifest.json")));
        var entries = manifest.RootElement.EnumerateArray().ToDictionary(e => e.GetProperty("file").GetString()!);

        _output.WriteLine("== step 2's entry: the clean 16/500 burst");

        var codes = OliviaData.Current.Rsid!;
        var clean = WavAudio.Read(Path.Combine(fixtures, "olivia-16-500-qso-rsid.wav"));

        foreach (var d in RsidDetector.Detect(codes, clean, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz))
        {
            _output.WriteLine($"  {d.Name} ({d.Code}) at {d.CenterHz:0.00} Hz, tones right {d.TonesRight}");
        }

        _output.WriteLine("");
        _output.WriteLine("== data/olivia/timing.json, parsed, against the manifest");

        using var timing = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "data", "olivia", "timing.json")));
        var t = timing.RootElement;

        _output.WriteLine("keys   : " + string.Join(", ", t.EnumerateObject().Select(p => p.Name)));
        _output.WriteLine("source : " + t.GetProperty("source").GetString());

        foreach (var variant in t.GetProperty("variants").EnumerateArray())
        {
            var name = variant.GetProperty("variant").GetString();

            // **WORK INSTRUCTION 361 MEASURED THIS FILE** (decision N, and PSK31 plan R12): the
            // estimate's fixed time and its readings went with the estimate, so they are read where
            // they are still present and said to be absent where they are not.
            var fixedSeconds = variant.TryGetProperty("fixed_seconds", out var f) ? f.ToString() : "(not in the measured file)";

            _output.WriteLine(
                $"{name}: {variant.GetProperty("seconds_per_character")} s per character, fixed {fixedSeconds}");

            if (!variant.TryGetProperty("readings", out var readings))
            {
                _output.WriteLine("  no readings: the file is measured, not estimated from the manifest");

                continue;
            }

            foreach (var reading in readings.EnumerateArray())
            {
                var file = reading.GetProperty("file").GetString()!;
                var entry = entries[file];
                var seconds = entry.GetProperty("seconds").GetDouble();
                var characters = entry.GetProperty("text").GetString()!.Length;
                var rsid = entry.GetProperty("rsid").GetBoolean();
                var rsidSeconds = reading.GetProperty("rsid_seconds").GetDouble();
                var perCharacter = Math.Round((seconds - rsidSeconds) / characters, 3);

                var agrees =
                    reading.GetProperty("seconds").GetDouble() == seconds
                    && reading.GetProperty("characters").GetInt32() == characters
                    && (rsidSeconds > 0) == rsid
                    && reading.GetProperty("seconds_per_character").GetDouble() == perCharacter
                    && entry.GetProperty("variant").GetString() == name;

                _output.WriteLine(
                    $"  {file}: seconds {reading.GetProperty("seconds")} / manifest {seconds}; characters "
                    + $"{reading.GetProperty("characters")} / {characters}; rsid {rsidSeconds} / {rsid}; per character "
                    + $"{reading.GetProperty("seconds_per_character")} / {perCharacter:0.000} - {(agrees ? "agrees" : "DISAGREES")}");
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"the file's 2.32 s against RsidBurst: tones and 5 silent before {RsidBurst.Seconds(codes):0.0000} s, "
            + $"5 silent either side {(codes.SilenceSymbolsBefore * 2 + codes.Symbols) / codes.SymbolRateHz:0.0000} s");
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
