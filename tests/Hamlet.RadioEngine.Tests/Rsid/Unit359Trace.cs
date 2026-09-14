using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rsid;

/// <summary>
/// Work instruction 359 task 1: **the trace, before anything is built.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit337Measure`. It reads
/// each fixture's header, finds where the RSID burst sits **from the audio** by lining the
/// file's own tone sequences up against the tone energies, measures the PSK31 macros as they
/// are composed today against their caps with the burst in front, hashes today's CQ, and
/// counts the keying and arming sites. **It asserts nothing**, so it cannot become a wall.</para>
/// <para>**EVERY RSID NUMBER IS READ FROM `data/rsid/rsid-codes.json`** (R27). Nothing here
/// types a code, a tone, a spacing or a symbol rate.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit359Trace
{
    private const string Mine = "KC3QIS";
    private const string His = "W1AW";

    /// <summary>The tone-energy table spans two steps either side of the file's sixteen.</summary>
    private const int Margin = 2;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit359Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Print the fixtures, the data, the send path and the counts.</summary>
    [Fact]
    public void TheTraceBeforeAnythingIsBuilt()
    {
        var root = Root();

        using var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "data", "rsid", "rsid-codes.json")));
        var file = data.RootElement;
        var rate = file.GetProperty("symbol_rate_hz").GetDouble();
        var symbols = file.GetProperty("symbols").GetInt32();
        var before = file.GetProperty("silence_symbols_before").GetInt32();
        var first = file.GetProperty("first_tone_offset_symbols").GetInt32();

        var sequences = file.GetProperty("tone_sequences").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.EnumerateArray().Select(t => t.GetInt32()).ToArray());

        _output.WriteLine("== the data");
        _output.WriteLine("keys      : " + string.Join(", ", file.EnumerateObject().Select(p => p.Name)));
        _output.WriteLine("source    : " + file.GetProperty("source").GetString());
        _output.WriteLine($"rate {rate} Hz, {symbols} symbols, {before} silent before, first tone {first} steps");

        foreach (var code in file.GetProperty("codes").EnumerateObject())
        {
            _output.WriteLine(
                $"  {code.Name,-16} {code.Value.GetInt32(),3}  sequence "
                + (sequences.TryGetValue(code.Name, out var s) ? string.Join(" ", s) : "none"));
        }

        _output.WriteLine("");
        _output.WriteLine("== the fixtures");

        using var manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "assets", "fixtures", "olivia", "manifest.json")));

        foreach (var entry in manifest.RootElement.EnumerateArray())
        {
            var name = entry.GetProperty("file").GetString()!;
            var path = Path.Combine(root, "assets", "fixtures", "olivia", name);
            var (format, channels, sampleRate, bits, dataBytes) = Header(path);
            var seconds = dataBytes / (double)(channels * bits / 8) / sampleRate;

            _output.WriteLine(
                $"{name}: format {format}, {channels} ch, {sampleRate} Hz, {bits} bit, {seconds:0.000} s, "
                + $"manifest {entry.GetProperty("seconds").GetDouble()} s, rsid {entry.GetProperty("rsid")}");

            var audio = WavAudio.Read(path);
            var centers = Centers(entry.GetProperty("center_hz"));

            foreach (var center in centers)
            {
                var found = Search(audio, center, rate, symbols, first, sequences, scanSeconds: 12);

                _output.WriteLine(
                    $"   at {center,6:0} Hz best {found.Name,-15} shift {found.Shift,2}  "
                    + $"burst {found.Start:0.000} to {found.End:0.000} s ({found.End - found.Start:0.000} s)  "
                    + $"strongest tone right {found.Matched,2} of {symbols}  energy on sequence {found.Fraction:0.00}  "
                    + $"5 symbols before {found.BeforeDb:0.0} dB, after {found.AfterDb:0.0} dB, "
                    + $"next signal {found.NextOnset:0.000} s");
            }
        }

        _output.WriteLine("");
        _output.WriteLine("== the send path");

        var tones = symbols / rate;
        var withBefore = (symbols + before) / rate;
        var eitherSide = (symbols + (2 * before)) / rate;

        _output.WriteLine(
            $"burst: 15 tones {tones:0.0000} s; with {before} silent before {withBefore:0.0000} s; "
            + $"with {before} either side (SOURCE.md) {eitherSide:0.0000} s");

        var typedLine = LongestTypedLine();

        var sends = new (string Name, string Text, double Cap)[]
        {
            ("CQ", Psk31Macros.Cq(Mine), OperatorSend.LongestUnslottedSeconds),
            ("Answer", Psk31Macros.Answer(His, Mine), OperatorSend.LongestUnslottedSeconds),
            ("Report", Psk31Macros.Report(His, Mine, "Tim", "Trafford PA", "FN00DJ"), OperatorSend.LongestUnslottedSeconds),
            ("Confirm", Psk31Macros.Confirm(His, Mine), OperatorSend.LongestUnslottedSeconds),
            ("Report VP2V/W1AW", Psk31Macros.Report("VP2V/W1AW", Mine, "Tim", "Trafford PA", "FN00DJ"), OperatorSend.LongestUnslottedSeconds),
            ("Typed (" + typedLine.Length + " chars)", Psk31Macros.Typed(His, Mine, typedLine), 60),
        };

        foreach (var sampleRate in new[] { Ft8Composer.DefaultSampleRate, 48_000 })
        {
            _output.WriteLine($"at {sampleRate} Hz");

            foreach (var (name, text, cap) in sends)
            {
                var composed = Psk31Modulator.Compose(text, sampleRate, 1000, Ft8Composer.DefaultDrivePeak, cap);

                _output.WriteLine(
                    $"  {name,-22} {composed.Seconds,7:0.000} s of {cap:0} | + tones {composed.Seconds + tones,7:0.000} "
                    + Over(composed.Seconds + tones, cap)
                    + $" | + tones and silence before {composed.Seconds + withBefore,7:0.000} " + Over(composed.Seconds + withBefore, cap)
                    + $" | + either side {composed.Seconds + eitherSide,7:0.000} " + Over(composed.Seconds + eitherSide, cap));
            }

            var cq = Psk31Modulator.Compose(Psk31Macros.Cq(Mine), sampleRate, 1000, Ft8Composer.DefaultDrivePeak);

            _output.WriteLine(
                $"  CQ samples at 1000 Hz, peak {Ft8Composer.DefaultDrivePeak}: {cq.Samples.Length} samples, sha256 "
                + Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(cq.Samples.AsSpan()))).ToLowerInvariant());
        }

        _output.WriteLine("");
        _output.WriteLine("== the counts");
        _output.WriteLine("CivConstants.PttOn code lines : " + CodeLines(root, "CivConstants.PttOn"));
        _output.WriteLine("_armedSend.Arm( code lines    : " + CodeLines(root, "_armedSend.Arm("));
    }

    private static string Over(double seconds, double cap) => seconds > cap ? "OVER" : "fits";

    /// <summary>The longest framed typed line of plain words that stays within sixty seconds.</summary>
    private static string LongestTypedLine()
    {
        const string Words = "the quick brown fox jumps over the lazy dog ";

        var line = "";

        for (var i = 0; ; i++)
        {
            var next = line + Words[i % Words.Length];

            if (next.Trim().Length > 0 && Psk31Macros.TypedSeconds(His, Mine, next.Trim()) > 60)
            {
                return line.Trim();
            }

            line = next;
        }
    }

    private sealed record Found(
        string Name, int Shift, double Start, double End, int Matched, double Fraction,
        double BeforeDb, double AfterDb, double NextOnset);

    /// <summary>Line every sequence up against the tone energies and keep the best fit.</summary>
    private static Found Search(
        MonoAudio audio, double centerHz, double rate, int symbols, int first,
        IReadOnlyDictionary<string, int[]> sequences, double scanSeconds)
    {
        var x = audio.Samples;
        var sr = audio.SampleRate;
        var perSymbol = sr / rate;
        var window = (int)Math.Round(perSymbol);
        var hop = perSymbol / 8;
        var tonesInTable = 16 + (2 * Margin);
        var lowestStep = first - Margin;
        var scanned = Math.Min(x.Length, (int)(scanSeconds * sr));
        var rows = (int)((scanned - window) / hop) + 1;

        var energy = new double[rows, tonesInTable];

        for (var r = 0; r < rows; r++)
        {
            var at = (int)Math.Round(r * hop);

            for (var t = 0; t < tonesInTable; t++)
            {
                energy[r, t] = Goertzel(x, at, window, centerHz + ((lowestStep + t) * rate), sr);
            }
        }

        var best = new Found("none", 0, 0, 0, -1, 0, 0, 0, 0);
        var bestRow = 0;

        for (var s = 0; s + ((symbols - 1) * 8) < rows; s++)
        {
            foreach (var (name, sequence) in sequences)
            {
                for (var shift = -Margin; shift <= Margin; shift++)
                {
                    var matched = 0;
                    var on = 0.0;
                    var all = 0.0;
                    var offset = Margin + shift;

                    for (var i = 0; i < symbols; i++)
                    {
                        var row = s + (i * 8);
                        var loudest = 0;

                        for (var k = 0; k < 16; k++)
                        {
                            all += energy[row, offset + k];

                            if (energy[row, offset + k] > energy[row, offset + loudest])
                            {
                                loudest = k;
                            }
                        }

                        if (loudest == sequence[i])
                        {
                            matched++;
                        }

                        on += energy[row, offset + sequence[i]];
                    }

                    var fraction = all > 0 ? on / all : 0;

                    if (matched > best.Matched || (matched == best.Matched && fraction > best.Fraction))
                    {
                        best = best with { Name = name, Shift = shift, Matched = matched, Fraction = fraction };
                        bestRow = s;
                    }
                }
            }
        }

        var start = (int)Math.Round(bestRow * hop);
        var end = start + (int)Math.Round(symbols * perSymbol);
        var burst = Rms(x, start, end);
        var span = (int)Math.Round(5 * perSymbol);

        var frame = sr / 50;
        var onset = double.NaN;

        for (var at = end; at + frame <= x.Length; at += frame)
        {
            if (Rms(x, at, at + frame) >= 0.25 * burst)
            {
                onset = at / (double)sr;
                break;
            }
        }

        return best with
        {
            Start = start / (double)sr,
            End = end / (double)sr,
            BeforeDb = Db(Rms(x, Math.Max(0, start - span), start), burst),
            AfterDb = Db(Rms(x, end, Math.Min(x.Length, end + span)), burst),
            NextOnset = onset,
        };
    }

    private static double Db(double level, double reference)
        => level > 0 && reference > 0 ? 20 * Math.Log10(level / reference) : double.NegativeInfinity;

    private static double Rms(float[] x, int from, int to)
    {
        if (to <= from)
        {
            return 0;
        }

        var sum = 0.0;

        for (var i = from; i < to && i < x.Length; i++)
        {
            sum += x[i] * (double)x[i];
        }

        return Math.Sqrt(sum / (to - from));
    }

    private static double Goertzel(float[] x, int start, int n, double hz, int sampleRate)
    {
        var coefficient = 2 * Math.Cos(2 * Math.PI * hz / sampleRate);
        double s1 = 0;
        double s2 = 0;
        var end = Math.Min(x.Length, start + n);

        for (var i = start; i < end; i++)
        {
            var s0 = x[i] + (coefficient * s1) - s2;
            s2 = s1;
            s1 = s0;
        }

        return (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
    }

    private static List<double> Centers(JsonElement center)
        => center.ValueKind switch
        {
            JsonValueKind.Number => [center.GetDouble()],
            JsonValueKind.String => center.GetString()!.Split(',')
                .Select(c => double.Parse(c, CultureInfo.InvariantCulture)).ToList(),
            _ => [1000.0, 2000.0],
        };

    private static (int Format, int Channels, int SampleRate, int Bits, int DataBytes) Header(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var at = 12;
        int format = 0, channels = 0, sampleRate = 0, bits = 0;

        while (at + 8 <= bytes.Length)
        {
            var id = System.Text.Encoding.ASCII.GetString(bytes, at, 4);
            var size = BitConverter.ToInt32(bytes, at + 4);

            if (id == "fmt ")
            {
                format = BitConverter.ToInt16(bytes, at + 8);
                channels = BitConverter.ToInt16(bytes, at + 10);
                sampleRate = BitConverter.ToInt32(bytes, at + 12);
                bits = BitConverter.ToInt16(bytes, at + 22);
            }
            else if (id == "data")
            {
                return (format, channels, sampleRate, bits, size);
            }

            at += 8 + size + (size & 1);
        }

        return (format, channels, sampleRate, bits, 0);
    }

    private static int CodeLines(string root, string needle)
        => Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .SelectMany(File.ReadAllLines)
            .Count(l => l.Contains(needle, StringComparison.Ordinal)
                        && !l.TrimStart().StartsWith("//", StringComparison.Ordinal)
                        && !l.TrimStart().StartsWith("*", StringComparison.Ordinal));

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
