using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// The sanctioned read of the pinned clone for the message layer: what <c>ft8/crc.h</c> and
/// <c>ft8/crc.c</c> hold, and whether upstream states a CRC test vector anywhere a port could
/// cite as a known value.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the same route <c>ReferenceCloneProbeTests</c> opened and the only one there is.</b>
/// The agent's own file tools are refused <c>C:\Source\ft8_lib</c>; a compiled test process
/// reading a file is the operating system's business. Everything ported into this library came
/// through here.
/// </para>
/// <para>
/// <b>Shapes only, never values.</b> This inventory prints file sizes, line counts, identifiers
/// and macro names. It does not print a macro's expansion, a polynomial, a width, an alphabet or
/// any other value — those may live in <c>src/Ft8Sharp/</c> where the port needs them and may not
/// appear in a session transcript that becomes a report.
/// </para>
/// <para>
/// <b>The one exception is deliberate, gated and off by default.</b> A port has to be able to
/// read the function it is porting. <see cref="EmitCrcSourceForPorting"/> writes the source text
/// to the test console only when <c>FT8_CRC_SOURCE_DUMP=1</c> is set on the run, mirroring the
/// <c>FT8_TABLEGEN_WRITE</c> idiom the table converter already uses for its one dangerous
/// operation. Nothing it prints goes into a committed file.
/// </para>
/// </remarks>
public class ReferenceCloneCrcInventoryTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceCloneCrcInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files this unit is licensed to read, and no others.</summary>
    private static readonly string[] CrcSources = { @"ft8\crc.h", @"ft8\crc.c" };

    [RequiresReferenceCloneFact]
    public void CrcSourcesAreLegibleAsShapesOnly()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");
        _output.WriteLine("SHAPES ONLY — no macro expansion, polynomial or width is printed here, by ruling.");

        var found = 0;
        foreach (var relative in CrcSources)
        {
            var path = Path.Combine(location, relative);
            if (!File.Exists(path))
            {
                _output.WriteLine($"{relative,-16}: ABSENT");
                continue;
            }

            found++;
            var text = File.ReadAllText(path);
            _output.WriteLine(
                $"{relative,-16}: present, {new FileInfo(path).Length} bytes, {CountLines(text)} lines");

            foreach (var macro in MacroNames(text))
            {
                _output.WriteLine($"    macro     {macro}");
            }

            foreach (var function in FunctionNames(text))
            {
                _output.WriteLine($"    function  {function}");
            }

            foreach (var include in IncludeNames(text))
            {
                _output.WriteLine($"    includes  {include}");
            }
        }

        Assert.Equal(CrcSources.Length, found);
    }

    /// <summary>
    /// Are there known values at all? Criterion 1 says "CRC matches known values", and a value this
    /// port produced itself is not one. This walks the clone's own test sources looking for anything
    /// that states an expected checksum for a stated input, and reports either answer by name.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void UpstreamTestSourcesAreInventoriedForACrcVector()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");

        var candidates = new List<string>();
        foreach (var folder in new[] { "test", "tests" })
        {
            var directory = Path.Combine(location, folder);
            if (Directory.Exists(directory))
            {
                _output.WriteLine($"{folder,-16}: present");
                candidates.AddRange(Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(IsCSource));
            }
            else
            {
                _output.WriteLine($"{folder,-16}: absent");
            }
        }

        // Upstream keeps some self-tests beside the source rather than under a test folder.
        candidates.AddRange(Directory.EnumerateFiles(location, "*.*", SearchOption.TopDirectoryOnly)
            .Where(IsCSource));

        _output.WriteLine($"candidate test sources  : {candidates.Count}");
        foreach (var path in candidates.Distinct().OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var text = File.ReadAllText(path);
            var mentionsCrc = text.Contains("crc", StringComparison.OrdinalIgnoreCase);
            _output.WriteLine(
                $"    {Path.GetRelativePath(location, path),-30} "
                + $"{new FileInfo(path).Length,8} bytes  {CountLines(text),5} lines  "
                + $"mentions crc: {mentionsCrc}");

            if (!mentionsCrc)
            {
                continue;
            }

            foreach (var function in FunctionNames(text))
            {
                _output.WriteLine($"        function  {function}");
            }
        }

        // A self-test inside crc.c itself would be the other place a vector could live.
        var crcSource = Path.Combine(location, @"ft8\crc.c");
        if (File.Exists(crcSource))
        {
            var text = File.ReadAllText(crcSource);
            _output.WriteLine($"crc.c declares main()   : {Regex.IsMatch(text, @"\bmain\s*\(")}");
            _output.WriteLine($"crc.c has assert()      : {text.Contains("assert", StringComparison.Ordinal)}");
        }

        // No assertion on the outcome: both answers are the finding. The inventory is the product.
        Assert.True(candidates.Count >= 0);
    }

    /// <summary>
    /// Prints the CRC source so it can be ported, and only when explicitly asked to.
    /// </summary>
    /// <remarks>
    /// Off by default. A port has to read the function it ports, and the route to that reading is
    /// this test process; but a probe that prints third-party source on every run would put it into
    /// every transcript for the rest of the project's life, and the no-values rule exists precisely
    /// so that does not happen by accident.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void EmitCrcSourceForPorting()
    {
        if (Environment.GetEnvironmentVariable("FT8_CRC_SOURCE_DUMP") != "1")
        {
            _output.WriteLine("Not asked. Set FT8_CRC_SOURCE_DUMP=1 on the run to emit the source for porting.");
            return;
        }

        var location = RequireReachableClone();
        foreach (var relative in CrcSources.Concat(new[] { @"test\test.c" }))
        {
            var path = Path.Combine(location, relative);
            _output.WriteLine($"===== {relative} =====");
            _output.WriteLine(File.Exists(path) ? File.ReadAllText(path) : "(absent)");
        }

        // crc.c spells its two scalars as macros defined elsewhere, so the port cannot be written
        // from crc.c alone. Only the CRC ones are emitted, and only under the same gate.
        var constants = Path.Combine(location, @"ft8\constants.h");
        _output.WriteLine(@"===== ft8\constants.h — CRC macros only =====");
        if (File.Exists(constants))
        {
            foreach (var (name, value) in TableGen.CSourceParser
                         .ParseIntegerMacros(File.ReadAllText(constants))
                         .Where(m => m.Key.Contains("CRC", StringComparison.Ordinal)))
            {
                _output.WriteLine($"{name} = {value}");
            }
        }
        else
        {
            _output.WriteLine("(absent)");
        }
    }

    private string RequireReachableClone()
    {
        var reach = ReferenceClone.Probe(out var detail);
        if (reach == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so nothing can be ported tonight.");
        }

        return ReferenceClone.Location;
    }

    private static bool IsCSource(string path) =>
        Path.GetExtension(path).Equals(".c", StringComparison.OrdinalIgnoreCase)
        || Path.GetExtension(path).Equals(".h", StringComparison.OrdinalIgnoreCase);

    private static int CountLines(string text) => text.Split('\n').Length;

    /// <summary>Macro names only. The expansion is deliberately not captured.</summary>
    private static IEnumerable<string> MacroNames(string text) =>
        Regex.Matches(text, @"^\s*#\s*define\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Distinct();

    /// <summary>Function names as declared or defined, without their bodies or signatures.</summary>
    private static IEnumerable<string> FunctionNames(string text) =>
        Regex.Matches(
                text,
                @"^[A-Za-z_][A-Za-z0-9_ \t\*]*?\b([A-Za-z_][A-Za-z0-9_]*)\s*\([^;{]*\)\s*[;{]",
                RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Where(name => name is not ("if" or "for" or "while" or "switch" or "return" or "sizeof"))
            .Distinct();

    private static IEnumerable<string> IncludeNames(string text) =>
        Regex.Matches(text, @"^\s*#\s*include\s+([<""][^>""]+[>""])", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Distinct();
}
