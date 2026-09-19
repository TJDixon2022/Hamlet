using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 365 task 0: **the measurements before the modulator is built** - the nine
/// hashes, then items (a) to (e).
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit364Trace`. It asserts
/// nothing past the hashes, so it cannot become a wall, and it is not on the carry-forward line.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class Unit365Trace
{
    /// <summary>The callsigns, name, place and grid the app's PSK31 tests already send (decision AR).</summary>
    internal const string Mine = "KC3QIS";
    internal const string His = "W1AW";
    internal const string Name = "Tim";
    internal const string Qth = "Trafford PA";
    internal const string Grid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit365Trace(ITestOutputHelper output) => _output = output;

    /// <summary>The four macros, framed as the app frames them, with the test fields.</summary>
    internal static (string Name, string Text)[] Macros() =>
    [
        ("cq", Psk31Macros.Cq(Mine)),
        ("answer", Psk31Macros.Answer(His, Mine)),
        ("report", Psk31Macros.Report(His, Mine, Name, Qth, Grid)),
        ("confirm", Psk31Macros.Confirm(His, Mine)),
    ];

    /// <summary>PSK31's seconds per character over the four framed macros, the idle and the reference symbol left out (decision AV).</summary>
    internal static double Psk31SecondsPerCharacter()
    {
        var idle = (Psk31Modulator.IdleBitsBefore + Psk31Modulator.IdleBitsAfter + 1) / Psk31Demodulator.Baud;
        var macros = Macros();

        return macros.Sum(m => Psk31Modulator.SecondsFor(m.Text) - idle) / macros.Sum(m => m.Text.Length);
    }

    /// <summary>Task 0 step 3: every fixture the manifest names hashes as it says.</summary>
    [Fact]
    public void TheNineFixturesHash()
    {
        var folder = Path.Combine(OliviaFixtures.Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var files = manifest.RootElement.EnumerateArray().Select(e => e.GetProperty("file").GetString()!).ToArray();

        foreach (var file in files)
        {
            OliviaFixtures.Load(file);
        }

        _output.WriteLine($"hashed {files.Length} of {files.Length}: {string.Join(", ", files)}");
    }

    /// <summary>Item (a): the three clean fixtures, measured by the code task 2 uses.</summary>
    [Fact]
    public void TheAuthorsFixturesMeasured()
    {
        _output.WriteLine("file | code | center | tones | spacing Hz | symbol rate Hz | 99% bandwidth Hz | preamble s | data s | data in symbols");

        foreach (var file in new[] { "olivia-8-250-cq-rsid.wav", "olivia-16-500-qso-rsid.wav", "olivia-32-1000-qso-rsid.wav" })
        {
            var fixture = OliviaFixtures.Load(file);
            var audio = WavAudio.Read(fixture.Path);
            var m = OliviaSignalMeasure.Measure(audio);
            var symbol = OliviaData.Current.Format!.Variant(fixture.Variant!)!.SymbolSeconds;

            _output.WriteLine(
                $"{file} | {m.BurstCode} | {m.BurstCenterHz:0.00} | {m.Tones} | {m.ToneSpacingHz:0.000} | {m.SymbolRateHz:0.000} | "
                + $"{m.OccupiedBandwidthHz:0.00} | {m.PreambleSeconds:0.0000} ({m.PreambleSeconds / symbol:0.00} symbols) | "
                + $"{m.DataSeconds:0.000} | {m.DataSeconds / symbol:0.00} (blocks of 64: {(m.DataSeconds / symbol) / 64:0.000})");
        }
    }

    /// <summary>Items (c), (d) and (e): the mode's switch sites, the PSK31 patience and seconds per character.</summary>
    [Fact]
    public void TheSitesAndThePsk31Numbers()
    {
        // (c) Every line in the source that names UnslottedMode, and every switch in the sequence.
        var src = Path.Combine(OliviaFixtures.Root(), "src");

        foreach (var path in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
        {
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                var sequence = path.EndsWith("Ft8TransmitSequence.cs", StringComparison.Ordinal);

                if (lines[i].Contains("UnslottedMode", StringComparison.Ordinal)
                    || (sequence && (lines[i].Contains("switch", StringComparison.Ordinal) || lines[i].Contains(".Mode", StringComparison.Ordinal))))
                {
                    _output.WriteLine($"(c) {Path.GetRelativePath(src, path)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        // (d) The PSK31 turn patience: R18's stated equivalent of one slot, the Answer macro's time on the air.
        var patience = Psk31Macros.AnswerSeconds(His, Mine);

        _output.WriteLine($"(d) Psk31Macros.AnswerSeconds({His}, {Mine}) = {patience:0.0000} s, \"{Psk31Macros.Answer(His, Mine)}\"");

        // (e) PSK31 seconds per character on the four framed macros, idle left out.
        foreach (var (name, text) in Macros())
        {
            _output.WriteLine($"(e) {name}: {text.Length} characters, {Psk31Modulator.SecondsFor(text):0.0000} s with idle, \"{text}\"");
        }

        var spc = Psk31SecondsPerCharacter();

        _output.WriteLine($"(e) PSK31 seconds per character = {spc:0.000000}; 30 s = {30 / spc:0.00} characters, 60 s = {60 / spc:0.00}, patience = {patience / spc:0.00}");
    }
}
