using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Why upstream's generator does or does not run on this machine, measured rather than inferred.
/// </summary>
/// <remarks>
/// <para>
/// A non-zero exit code is not a diagnosis. This separates a bad build from a good binary meeting a
/// platform limit, and it does so from two independent directions: the executable's own PE header,
/// which states the stack the linker asked Windows for, and the generator's source in the pin,
/// which states how much stack it wants.
/// </para>
/// <para>
/// <b>Shapes and sizes only.</b> Nothing here reproduces a line of upstream C or a value it
/// computes. A stack size is a property of a build, not a datum of the algorithm.
/// </para>
/// </remarks>
public class Ft8OracleDiagnosisTests
{
    private readonly ITestOutputHelper _output;

    public Ft8OracleDiagnosisTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The stack Windows will give the generator's main thread, read out of its PE optional header.
    /// </summary>
    /// <remarks>
    /// Windows takes the reserve from the image, not from the parent, so a process cannot be handed
    /// a bigger stack at launch. If the number here is the linker's 1 MB default and the generator
    /// wants more, no amount of running it differently will help — which is exactly the distinction
    /// this test exists to make.
    /// </remarks>
    [RequiresOracleFact]
    public void TheGeneratorsStackReserveIsReadFromItsOwnHeader()
    {
        var reserve = ReadStackReserve(Ft8Oracle.ExecutablePath, out var detail);

        _output.WriteLine($"image        : {Ft8Oracle.ExecutablePath}");
        _output.WriteLine($"how read     : {detail}");
        _output.WriteLine($"stack reserve: {reserve} bytes ({reserve / 1024.0 / 1024.0:F2} MB)");

        Assert.True(reserve > 0, $"could not read a stack reserve out of the image: {detail}");
    }

    /// <summary>
    /// How much stack the generator asks for, from the pin: the arrays it puts on the stack, sized
    /// from the sample rate and the transmission's length.
    /// </summary>
    /// <remarks>
    /// The generator is written for gcc and clang on systems whose default stack is 8 MB. Windows
    /// links for 1 MB. That difference is a property of the platform and the build, not of the
    /// port, and it is the reason this unit could not close criterion 2.
    /// </remarks>
    [RequiresOracleFact]
    public void TheGeneratorPutsItsWholeSignalOnTheStack()
    {
        var source = Path.Combine(ReferenceClone.Location, "demo", "gen_ft8.c");
        Assert.True(File.Exists(source), $"the pin has no generator source at {source}");

        var text = File.ReadAllText(source);

        // A C99 variable length array: a declaration whose extent is an expression rather than a
        // constant. Shape is reported; the declaration itself is not reproduced.
        var arrays = Regex.Matches(text, @"\b(float|double|int|uint8_t|int16_t)\s+\w+\s*\[[^\]]+\]\s*;");
        _output.WriteLine($"stack array declarations found: {arrays.Count}");
        foreach (Match match in arrays)
        {
            var line = text.Take(match.Index).Count(c => c == '\n') + 1;
            var inside = match.Value[(match.Value.IndexOf('[') + 1)..match.Value.LastIndexOf(']')].Trim();
            var isConstant = int.TryParse(inside, out _);
            _output.WriteLine(
                $"  line {line,4}: element type declared, extent is "
                + $"{(isConstant ? "a constant" : "an expression — a variable length array")}, "
                + $"{inside.Length} characters of extent");
        }

        Assert.True(arrays.Count > 0, "the generator declares no stack arrays, so the diagnosis is wrong");
    }

    /// <summary>
    /// Whether the generator ever prints the tone sequence, which decides whether a direct
    /// comparison is possible at all once it can be made to run.
    /// </summary>
    /// <remarks>
    /// This is the question task 3 exists to answer and the one the next unit most needs settled: a
    /// generator that prints its tones gives the clean channel to step 3's second criterion, and one
    /// that writes only a WAV leaves demodulation as the only route.
    /// </remarks>
    [RequiresOracleFact]
    public void WhetherTheGeneratorEverPrintsItsTones()
    {
        var source = Path.Combine(ReferenceClone.Location, "demo", "gen_ft8.c");
        var lines = File.ReadAllLines(source);

        var prints = 0;
        var printsInAToneLoop = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("printf", StringComparison.Ordinal))
            {
                continue;
            }

            prints++;

            // A print inside, or immediately after, a loop whose bound is the channel symbol count
            // is a print of the tones. The window is small on purpose — a wide one would match
            // anything.
            var from = Math.Max(0, i - 3);
            var window = string.Join('\n', lines[from..(i + 1)]);
            if (Regex.IsMatch(window, @"\bfor\b[^\n]*\b(tones?|symbol)", RegexOptions.IgnoreCase))
            {
                printsInAToneLoop++;
            }
        }

        // The narrow window above can miss a print whose loop is written differently, so the same
        // question is asked a second way: where do the prints sit relative to the tone buffer?
        var toneLines = new List<int>();
        var printLines = new List<int>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], @"\btone", RegexOptions.IgnoreCase))
            {
                toneLines.Add(i + 1);
            }

            if (lines[i].Contains("printf", StringComparison.Ordinal))
            {
                printLines.Add(i + 1);
            }
        }

        _output.WriteLine($"lines mentioning a tone : {string.Join(", ", toneLines)}");
        _output.WriteLine($"lines calling printf    : {string.Join(", ", printLines)}");

        var printsWithinTwoLinesOfATone = printLines
            .Count(p => toneLines.Any(t => Math.Abs(t - p) <= 2));
        _output.WriteLine($"prints within two lines of a tone mention: {printsWithinTwoLinesOfATone}");

        // And a third way, because the answer decides whether the direct channel to criterion 2
        // exists at all. Each line near the tone buffer is described by which constructs it holds,
        // never by its text: a print of an integer conversion inside a loop over the tone buffer is
        // a print of the tones and nothing else is.
        _output.WriteLine(string.Empty);
        _output.WriteLine("line | for | printf | tone | %d | %f | comment");
        for (var i = 145; i < Math.Min(lines.Length, 195); i++)
        {
            var line = lines[i];
            var flags = new[]
            {
                Regex.IsMatch(line, @"\bfor\b") ? "for" : "   ",
                line.Contains("printf", StringComparison.Ordinal) ? "printf" : "      ",
                Regex.IsMatch(line, @"\btone", RegexOptions.IgnoreCase) ? "tone" : "    ",
                line.Contains("%d", StringComparison.Ordinal) ? "%d" : "  ",
                Regex.IsMatch(line, @"%[\d\.]*f") ? "%f" : "  ",
                line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? "comment" : "       ",
            };

            if (flags.Any(f => f.Trim().Length > 0))
            {
                _output.WriteLine($"{i + 1,4} | {string.Join(" | ", flags)}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"printf calls in the generator          : {prints}");
        _output.WriteLine($"of those, matched by the narrow window : {printsInAToneLoop}");

        // The reading that decides it. A label print, then a loop, then an integer conversion
        // inside that loop, all within a few lines of the tone buffer, is a print of the tones.
        var toneLoopPrint = printLines.Any(p =>
            Regex.IsMatch(lines[p - 1], @"%d")
            && toneLines.Any(t => Math.Abs(t - p) <= 1)
            && Enumerable.Range(Math.Max(1, p - 4), 4).Any(q => Regex.IsMatch(lines[q - 1], @"\bfor\b")));

        _output.WriteLine(
            toneLoopPrint
                ? "The tones ARE printed: a label, a loop, and an integer conversion per tone. "
                  + "The direct channel to criterion 2 exists once the generator can be made to run."
                : "No print of the tones was found; the WAV would be the only route.");

        // And the ordering, which is the load-bearing part. The tones are printed before the
        // waveform buffer is declared, so a run that dies allocating that buffer has already
        // produced the tones — they are lost in an unflushed stdio buffer rather than never
        // computed. That is what makes this a build problem and not an oracle that cannot answer.
        var lastTonePrint = printLines.LastOrDefault(p => toneLines.Any(t => Math.Abs(t - p) <= 1));
        var signalArray = Regex.Matches(
            string.Join('\n', lines),
            @"\b(float|double|int16_t)\s+\w+\s*\[[^\]]+\]\s*;");
        var lastArrayLine = signalArray.Count == 0
            ? -1
            : string.Join('\n', lines).Take(signalArray[^1].Index).Count(c => c == '\n') + 1;

        _output.WriteLine(string.Empty);
        _output.WriteLine($"last print of a tone     : line {lastTonePrint}");
        _output.WriteLine($"last stack array declared: line {lastArrayLine}");
        _output.WriteLine(
            lastTonePrint > 0 && lastArrayLine > lastTonePrint
                ? "The tones are printed BEFORE the waveform buffer is declared."
                : "The tones are printed after the waveform buffer, so a crash there loses them.");

        Assert.True(prints > 0, "the generator prints nothing at all, which contradicts its own usage text");
        Assert.True(
            toneLoopPrint,
            "the generator was expected to print its tone sequence, and no print of it was found — "
            + "if this fails after a re-pin, the direct route to criterion 2 has gone and task 7's "
            + "WAV demodulation becomes the only one.");
    }

    /// <summary>
    /// Reads <c>SizeOfStackReserve</c> out of a PE image's optional header.
    /// </summary>
    /// <remarks>
    /// Walked by hand rather than with a package: this project's test assembly is allowed
    /// dependencies, but one added to read eight bytes at a known offset is a dependency bought for
    /// a sentence.
    /// </remarks>
    private static ulong ReadStackReserve(string imagePath, out string detail)
    {
        try
        {
            using var stream = File.OpenRead(imagePath);
            using var reader = new BinaryReader(stream);

            if (reader.ReadUInt16() != 0x5A4D)
            {
                detail = "not a PE image: no MZ signature";
                return 0;
            }

            stream.Position = 0x3C;
            var peOffset = reader.ReadUInt32();
            stream.Position = peOffset;
            if (reader.ReadUInt32() != 0x00004550)
            {
                detail = "not a PE image: no PE signature at e_lfanew";
                return 0;
            }

            // COFF header is 20 bytes; the optional header's magic follows it.
            stream.Position = peOffset + 4 + 20;
            var magic = reader.ReadUInt16();

            if (magic == 0x20B)
            {
                // PE32+: SizeOfStackReserve is 8 bytes at offset 72 of the optional header.
                stream.Position = peOffset + 4 + 20 + 72;
                detail = "PE32+ optional header, SizeOfStackReserve at offset 72";
                return reader.ReadUInt64();
            }

            if (magic == 0x10B)
            {
                // PE32: 4 bytes at offset 72.
                stream.Position = peOffset + 4 + 20 + 72;
                detail = "PE32 optional header, SizeOfStackReserve at offset 72";
                return reader.ReadUInt32();
            }

            detail = $"unrecognised optional header magic 0x{magic:X}";
            return 0;
        }
        catch (Exception ex)
        {
            detail = $"{ex.GetType().Name}: {ex.Message}";
            return 0;
        }
    }
}
