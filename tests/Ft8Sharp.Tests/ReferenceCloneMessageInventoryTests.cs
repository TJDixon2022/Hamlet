using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// The sanctioned read of the pinned clone for unit 207: what <c>ft8/message.c</c>, <c>ft8/text.c</c>
/// and <c>ft8/text.h</c> hold, and whether upstream states a <em>message-level</em> known value
/// anywhere — a message string paired with a stated packed value, a stated 77-bit pattern, or a
/// stated symbol sequence.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nobody has asked the pin that second question before.</b> Unit 206 inventoried
/// <c>test/test.c</c> for a <em>CRC</em> vector and found one, disabled and stale. A message-level
/// vector is a different artifact and it is the strongest evidence available that this port's
/// packing agrees with upstream's rather than merely with its own unpacker. Both answers are the
/// finding; neither is asserted on.
/// </para>
/// <para>
/// <b>This extends the inventory rather than duplicating it.</b> The reachability probe, the
/// skip-when-absent attribute and the shapes-only discipline all come from
/// <see cref="ReferenceCloneProbeTests"/> and <see cref="ReferenceCloneCrcInventoryTests"/>.
/// </para>
/// <para>
/// <b>Shapes only, never values.</b> Sizes, line counts, identifiers, macro names and enumerator
/// names are metadata and are printed. A macro's expansion, a field width, an alphabet, a message
/// string or a packed value is not. The one exception is
/// <see cref="EmitMessageSourceForPorting"/>, which is off unless
/// <c>FT8_MESSAGE_SOURCE_DUMP=1</c> is set on the run — the same idiom as
/// <c>FT8_TABLEGEN_WRITE</c> and <c>FT8_CRC_SOURCE_DUMP</c>. Nothing it prints reaches a
/// committed file.
/// </para>
/// </remarks>
public class ReferenceCloneMessageInventoryTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceCloneMessageInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 207 is licensed to read, and no others.</summary>
    private static readonly string[] MessageSources =
    {
        @"ft8\message.c", @"ft8\message.h", @"ft8\text.c", @"ft8\text.h",
    };

    /// <summary>
    /// Work instruction 206 named <c>ft8/pack.c</c> and <c>ft8/unpack.c</c>. Unit 206 measured that
    /// neither is in the pin. This confirms or refutes that on its own reading rather than
    /// inheriting it.
    /// </summary>
    private static readonly string[] FilesUnit206ReportedAbsent = { @"ft8\pack.c", @"ft8\unpack.c" };

    [RequiresReferenceCloneFact]
    public void MessageAndTextSourcesAreLegibleAsShapesOnly()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");
        _output.WriteLine("SHAPES ONLY — no macro expansion, field width, alphabet or message text is printed.");

        _output.WriteLine("known item 1 — the files work instruction 206 named:");
        foreach (var relative in FilesUnit206ReportedAbsent)
        {
            var path = Path.Combine(location, relative);
            _output.WriteLine($"    {relative,-16}: {(File.Exists(path) ? "PRESENT" : "absent")}");
            Assert.False(
                File.Exists(path),
                $"{relative} is in the pin after all. Unit 206 measured it absent and this instruction "
                + "carries that as known item 1; a mismatch here is reportable.");
        }

        var found = 0;
        foreach (var relative in MessageSources)
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
                _output.WriteLine($"    macro       {macro}");
            }

            foreach (var member in EnumMemberNames(text))
            {
                _output.WriteLine($"    enum member {member}");
            }

            foreach (var function in FunctionNames(text))
            {
                _output.WriteLine($"    function    {function}");
            }

            foreach (var include in IncludeNames(text))
            {
                _output.WriteLine($"    includes    {include}");
            }
        }

        Assert.Equal(MessageSources.Length, found);

        // Where packing actually lives, stated as a count of identifiers rather than as a claim.
        var messageText = File.ReadAllText(Path.Combine(location, @"ft8\message.c"));
        var packingFunctions = FunctionNames(messageText)
            .Where(n => n.Contains("pack", StringComparison.OrdinalIgnoreCase))
            .ToList();
        _output.WriteLine($"pack/unpack functions in message.c : {packingFunctions.Count}");
        foreach (var name in packingFunctions)
        {
            _output.WriteLine($"    {name}");
        }

        Assert.True(
            packingFunctions.Count > 0,
            "message.c declares no function whose name mentions packing, so known item 1's claim that "
            + "packing lives there is not confirmed by this reading.");
    }

    /// <summary>
    /// Does the pin hold a <em>message-level</em> known value? Reports which, by file and identifier,
    /// and whether it is live code or disabled. Neither answer is asserted on: the inventory is the
    /// product.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void UpstreamSourcesAreInventoriedForAMessageVector()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");
        _output.WriteLine("NAMES AND SHAPES ONLY — no vector's text, packed value or symbol sequence is printed.");

        var candidates = new List<string>();
        foreach (var folder in new[] { "test", "tests" })
        {
            var directory = Path.Combine(location, folder);
            if (Directory.Exists(directory))
            {
                candidates.AddRange(Directory
                    .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(IsCSource));
            }
        }

        candidates.AddRange(Directory
            .EnumerateFiles(location, "*.*", SearchOption.TopDirectoryOnly)
            .Where(IsCSource));

        var ft8Folder = Path.Combine(location, "ft8");
        if (Directory.Exists(ft8Folder))
        {
            candidates.AddRange(Directory.EnumerateFiles(ft8Folder, "*.*").Where(IsCSource));
        }

        var scanned = candidates.Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _output.WriteLine($"candidate C sources     : {scanned.Count}");

        var withVectorShape = 0;
        foreach (var path in scanned)
        {
            var text = File.ReadAllText(path);
            var relative = Path.GetRelativePath(location, path);

            // The three shapes a message-level known value could take. Counted, never captured.
            var messageLiterals = MessageLikeLiteral.Matches(text).Count;
            var packedArrays = PackedByteArray.Matches(text).Count;
            var toneArrays = ToneOrSymbolArray.Matches(text).Count;
            var mentionsPack = text.Contains("pack", StringComparison.OrdinalIgnoreCase);

            if (messageLiterals == 0 && packedArrays == 0 && toneArrays == 0 && !mentionsPack)
            {
                continue;
            }

            withVectorShape++;
            _output.WriteLine(
                $"    {relative,-28} {new FileInfo(path).Length,7} bytes {CountLines(text),5} lines  "
                + $"message-shaped literals: {messageLiterals}  packed arrays: {packedArrays}  "
                + $"tone/symbol arrays: {toneArrays}  mentions pack: {mentionsPack}");

            foreach (var function in FunctionNames(text))
            {
                var live = IsInsideDisabledRegion(text, function);
                _output.WriteLine($"        function  {function,-32} {(live ? "DISABLED (#if 0 / commented)" : "live code")}");
            }

            // Whether the file pairs an expected value with an input at all: assert-shaped lines.
            var asserts = Regex.Matches(text, @"\bassert\s*\(", RegexOptions.None).Count;
            var comparisons = Regex.Matches(text, @"\b(memcmp|strcmp|strncmp)\s*\(", RegexOptions.None).Count;
            _output.WriteLine($"        assert() calls: {asserts}   memcmp/strcmp calls: {comparisons}");
        }

        _output.WriteLine($"sources with any vector shape : {withVectorShape}");

        // No assertion on the outcome. Both answers are reportable and the report says which.
        Assert.True(scanned.Count > 0, "no C source was reachable in the clone, so this measured nothing.");
    }

    /// <summary>
    /// Prints the message-layer source so it can be ported, and only when explicitly asked to.
    /// </summary>
    /// <remarks>
    /// Off by default, keyed on its own variable so that neither the CRC dump nor the table rewrite
    /// turns it on by accident. A port has to read the functions it ports; a probe that printed
    /// third-party source on every run would put it into every transcript for the rest of the
    /// project's life.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void EmitMessageSourceForPorting()
    {
        if (Environment.GetEnvironmentVariable("FT8_MESSAGE_SOURCE_DUMP") != "1")
        {
            _output.WriteLine(
                "Not asked. Set FT8_MESSAGE_SOURCE_DUMP=1 on the run to emit the source for porting.");
            return;
        }

        var location = RequireReachableClone();

        // One file at a time when asked for one: message.c alone is 1156 lines, and a reader that
        // has to page through all of them to reach the function it is porting will skim.
        var only = Environment.GetEnvironmentVariable("FT8_MESSAGE_SOURCE_FILE");

        // Unit 209 extends this list by name rather than building a second emitter: the symbol
        // assembly lives in ft8/encode.c, which is where encode174 was already ported from, and
        // the sequence geometry is declared in ft8/constants.h. Still gated, still off by default.
        var wanted = MessageSources.Concat(new[]
        {
            @"ft8\constants.h", @"test\test.c", @"ft8\encode.c", @"ft8\encode.h",
        });
        if (only is { Length: > 0 })
        {
            wanted = wanted.Where(r => r.Contains(only, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var relative in wanted)
        {
            var path = Path.Combine(location, relative);
            _output.WriteLine($"===== {relative} =====");
            _output.WriteLine(File.Exists(path) ? File.ReadAllText(path) : "(absent)");
        }
    }

    /// <summary>A string literal long enough and shaped like an on-air message rather than a format string.</summary>
    private static readonly Regex MessageLikeLiteral = new(
        @"""[A-Z0-9][A-Z0-9 /+\-?]{6,}""",
        RegexOptions.Compiled);

    /// <summary>A braced byte array of the size a packed payload would be.</summary>
    private static readonly Regex PackedByteArray = new(
        @"\b(?:uint8_t|unsigned char|char)\s+[A-Za-z_][A-Za-z0-9_]*\s*\[\s*\d*\s*\]\s*=\s*\{\s*0[xX]",
        RegexOptions.Compiled);

    /// <summary>A braced array named like a tone or symbol sequence.</summary>
    private static readonly Regex ToneOrSymbolArray = new(
        @"\b[A-Za-z_][A-Za-z0-9_]*(?:tone|symbol|itone)[A-Za-z0-9_]*\s*\[[^\]]*\]\s*=\s*\{",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Whether the first definition of a named function sits inside an <c>#if 0</c> block. Crude by
    /// design — it reports a shape for a human to judge and nothing depends on it.
    /// </summary>
    private static bool IsInsideDisabledRegion(string text, string functionName)
    {
        var at = text.IndexOf(functionName, StringComparison.Ordinal);
        if (at < 0)
        {
            return false;
        }

        var before = text[..at];
        var opens = Regex.Matches(before, @"^\s*#\s*if\s+0\b", RegexOptions.Multiline).Count;
        var closes = Regex.Matches(before, @"^\s*#\s*endif\b", RegexOptions.Multiline).Count;
        return opens > closes;
    }

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
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

    /// <summary>
    /// Enumerator names only. An enumerator's assigned value would be a field width or a type code
    /// and is not printed here — <c>UpstreamMessageProvenanceTests</c> asserts those against this
    /// library's constants by machine without either side reaching a transcript.
    /// </summary>
    private static IEnumerable<string> EnumMemberNames(string text)
    {
        foreach (Match block in Regex.Matches(text, @"enum\b[^{]*\{(?<body>[^}]*)\}", RegexOptions.Singleline))
        {
            foreach (var part in block.Groups["body"].Value.Split(','))
            {
                var name = Regex.Match(part, @"[A-Za-z_][A-Za-z0-9_]*");
                if (name.Success && !part.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    yield return name.Value;
                }
            }
        }
    }

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
