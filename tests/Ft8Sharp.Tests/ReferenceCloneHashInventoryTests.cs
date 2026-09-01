using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// The sanctioned read of the pinned clone for unit 208: the callsign hashes, the rolling cache that
/// resolves them, and the non-standard-callsign message that carries them.
/// </summary>
/// <remarks>
/// <para>
/// <b>This mirrors <see cref="ReferenceCloneMessageInventoryTests"/> rather than replacing it.</b>
/// The reachability probe, the skip-when-absent attribute and the shapes-only discipline are unit
/// 201's and unit 206's and are reused unchanged — <see cref="RequiresReferenceCloneFactAttribute"/>
/// and <see cref="ReferenceClone"/> are not re-implemented here. What is new is the question:
/// unit 207 inventoried <c>message.c</c> for <em>packing</em>, and this inventories the same file
/// for <em>hashing</em>, which is a different region of it and a different set of scalars.
/// </para>
/// <para>
/// <b>The narrow question nobody has asked.</b> Unit 207 settled that the pin holds no usable
/// message-level known value and that question is closed. This asks a narrower one:
/// <em>does the pin state a hash value for a named callsign anywhere</em> — in a test, a comment, a
/// fixture, an expected-decode list or a <c>#define</c>? That would be the strongest provenance
/// available for the hash, which is the one artifact in this port where a plausible-looking guess is
/// invisible: a hash that is wrong but self-consistent round-trips perfectly through its own cache
/// and is silently deaf on the air.
/// </para>
/// <para>
/// <b>Shapes only, never values.</b> Identifiers, macro names, function names, counts, line counts
/// and byte counts are metadata and are printed. A multiplier, a base, an alphabet, a shift width or
/// a hash value is not. The one exception is <see cref="EmitHashSourceForPorting"/>, off unless
/// <c>FT8_HASH_SOURCE_DUMP=1</c> is set on the run — the same idiom as <c>FT8_MESSAGE_SOURCE_DUMP</c>
/// and <c>FT8_CRC_SOURCE_DUMP</c>. Nothing it prints reaches a committed file.
/// </para>
/// </remarks>
public class ReferenceCloneHashInventoryTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceCloneHashInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 208 is licensed to read, and no others.</summary>
    private static readonly string[] HashSources =
    {
        @"ft8\message.c", @"ft8\message.h", @"ft8\text.c", @"ft8\text.h", @"ft8\constants.h",
    };

    /// <summary>
    /// Where upstream's own <em>implementation</em> of the hash interface lives. <c>message.c</c>
    /// declares the interface and calls it; it does not implement it, so the rolling cache's
    /// eviction and replacement behaviour is not in the library at all. These two are the only
    /// implementations in the pin, and the emitter reads them for the same reason it reads the
    /// library: a cache ported from a guess would be as invisibly wrong as a hash ported from one.
    /// </summary>
    private static readonly string[] CacheSources = { @"demo\decode_ft8.c", @"test\test.c" };

    /// <summary>
    /// The hashing and non-standard-callsign region of the pin, reported as names and shapes.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheHashingRegionIsLegibleAsShapesOnly()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone : {location}");
        _output.WriteLine("SHAPES ONLY — no multiplier, base, alphabet, shift width or hash value is printed.");

        var found = 0;
        foreach (var relative in HashSources)
        {
            var path = Path.Combine(location, relative);
            if (!File.Exists(path))
            {
                _output.WriteLine($"{relative,-18}: ABSENT");
                continue;
            }

            found++;
            var text = File.ReadAllText(path);
            _output.WriteLine(
                $"{relative,-18}: present, {new FileInfo(path).Length} bytes, {CountLines(text)} lines");

            foreach (var macro in MacroNames(text).Where(MentionsHashOrNonstandard))
            {
                _output.WriteLine($"    macro       {macro}");
            }

            foreach (var member in EnumMemberNames(text).Where(MentionsHashOrNonstandard))
            {
                _output.WriteLine($"    enum member {member}");
            }

            foreach (var function in FunctionNames(text).Where(MentionsHashOrNonstandard))
            {
                _output.WriteLine($"    function    {function}");
            }

            foreach (var name in FileScopeVariableNames(text).Where(MentionsHashOrNonstandard))
            {
                _output.WriteLine($"    variable    {name}");
            }
        }

        Assert.Equal(HashSources.Length, found);

        var messageText = File.ReadAllText(Path.Combine(location, @"ft8\message.c"));
        var hashingFunctions = FunctionNames(messageText).Where(MentionsHashOrNonstandard).ToList();
        _output.WriteLine($"functions in message.c whose name mentions hashing or non-standard : {hashingFunctions.Count}");
        foreach (var name in hashingFunctions)
        {
            _output.WriteLine($"    {name}");
        }

        Assert.True(
            hashingFunctions.Count > 0,
            "message.c declares no function whose name mentions hashing, so the hash does not live where "
            + "this unit expects it and that mismatch is reportable.");
    }

    /// <summary>
    /// The narrow question: does the pin state a hash value for a named callsign anywhere? Reports
    /// which, by file and identifier, and whether it is live code or disabled. Neither answer is
    /// asserted on — the inventory is the product, and both answers are useful.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void DoesThePinStateAHashValueForANamedCallsign()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone : {location}");
        _output.WriteLine("NAMES AND SHAPES ONLY — no callsign taken from the clone and no hash value is printed.");

        var scanned = Directory
            .EnumerateFiles(location, "*.*", SearchOption.AllDirectories)
            .Where(p => !p.Contains(@"\.git\", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains(@"\ft4_ft8_public", StringComparison.OrdinalIgnoreCase))
            .Where(IsTextSource)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _output.WriteLine($"candidate sources scanned (ft4_ft8_public excluded) : {scanned.Count}");

        var withHashMention = 0;
        var withStatedValue = 0;
        foreach (var path in scanned)
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (IOException)
            {
                continue;
            }

            if (!text.Contains("hash", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            withHashMention++;
            var relative = Path.GetRelativePath(location, path);

            // A line that mentions hashing AND carries a numeric literal wide enough to be a 10, 12
            // or 22-bit hash, near a callsign-shaped token. Counted and located, never captured.
            var statedValueLines = new List<int>();
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.Contains("hash", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!NumericLiteral.IsMatch(line))
                {
                    continue;
                }

                if (!CallsignShapedToken.IsMatch(line))
                {
                    continue;
                }

                statedValueLines.Add(i + 1);
            }

            var hashMacros = MacroNames(text).Where(MentionsHashOrNonstandard).ToList();
            _output.WriteLine(
                $"    {relative,-30} mentions hash on {CountLinesMentioning(text, "hash")} lines, "
                + $"hash-named macros: {hashMacros.Count}, "
                + $"lines pairing a hash mention with a numeric literal and a callsign-shaped token: {statedValueLines.Count}");

            foreach (var macro in hashMacros)
            {
                _output.WriteLine($"        macro     {macro}");
            }

            if (statedValueLines.Count > 0)
            {
                withStatedValue++;
                foreach (var lineNumber in statedValueLines)
                {
                    var disabled = IsLineInsideDisabledRegion(text, lineNumber);
                    _output.WriteLine(
                        $"        candidate at line {lineNumber,5}  {(disabled ? "DISABLED (#if 0)" : "live code")}");
                }
            }
        }

        _output.WriteLine($"sources mentioning hash                    : {withHashMention}");
        _output.WriteLine($"sources with a candidate stated hash value : {withStatedValue}");
        _output.WriteLine(
            withStatedValue == 0
                ? "ANSWER: the pin states no hash value for a named callsign anywhere."
                : "ANSWER: at least one candidate exists — named above by file and line for a human to judge.");

        Assert.True(scanned.Count > 0, "no source was reachable in the clone, so this measured nothing.");
    }

    /// <summary>
    /// Prints only the hashing and non-standard-callsign region so it can be ported, and only when
    /// explicitly asked to.
    /// </summary>
    /// <remarks>
    /// Off by default and keyed on its own variable, so neither the table rewrite nor unit 207's
    /// message dump turns it on by accident. It emits <em>named functions and their bodies</em>
    /// rather than whole files: <c>message.c</c> is over a thousand lines and a reader who has to
    /// page through all of them to reach the function being ported will skim, which is exactly the
    /// failure mode a faithful port of a hash cannot afford.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void EmitHashSourceForPorting()
    {
        if (Environment.GetEnvironmentVariable("FT8_HASH_SOURCE_DUMP") != "1")
        {
            _output.WriteLine(
                "Not asked. Set FT8_HASH_SOURCE_DUMP=1 on the run to emit the hashing region for porting.");
            return;
        }

        var location = RequireReachableClone();

        var wanted = Environment.GetEnvironmentVariable("FT8_HASH_SOURCE_SYMBOLS");
        var symbols = string.IsNullOrWhiteSpace(wanted)
            ? Array.Empty<string>()
            : wanted.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var file = Environment.GetEnvironmentVariable("FT8_HASH_SOURCE_FILE");
        var all = HashSources.Concat(CacheSources).ToArray();
        var files = string.IsNullOrWhiteSpace(file)
            ? all
            : all.Where(r => r.Contains(file, StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach (var relative in files)
        {
            var path = Path.Combine(location, relative);
            if (!File.Exists(path))
            {
                _output.WriteLine($"===== {relative} ===== (absent)");
                continue;
            }

            var text = File.ReadAllText(path);
            if (symbols.Length == 0)
            {
                _output.WriteLine($"===== {relative} ===== (whole file)");
                _output.WriteLine(text);
                continue;
            }

            foreach (var symbol in symbols)
            {
                var body = ExtractDefinition(text, symbol);
                _output.WriteLine($"===== {relative} :: {symbol} =====");
                _output.WriteLine(body ?? "(not found in this file)");
            }
        }
    }

    /// <summary>
    /// Pulls a named function or file-scope definition out of C source by brace-matching from its
    /// first definition. Crude by design: it is a reading aid for a human porter, and nothing is
    /// asserted on what it returns.
    /// </summary>
    private static string? ExtractDefinition(string text, string symbol)
    {
        // A macro, a typedef or a file-scope struct is not brace-matched from a call site, so take
        // those shapes first and only then fall through to the function-body walk.
        var macro = Regex.Match(text, $@"^\s*#\s*define\s+{Regex.Escape(symbol)}\b[^\n]*", RegexOptions.Multiline);
        if (macro.Success)
        {
            return macro.Value;
        }

        var typedef = Regex.Match(
            text,
            $@"^\s*(?:typedef\s+)?struct\b[^;]*?\b{Regex.Escape(symbol)}\b[^;]*;",
            RegexOptions.Multiline | RegexOptions.Singleline);
        if (typedef.Success && typedef.Value.Contains('{'))
        {
            return typedef.Value;
        }

        foreach (Match match in Regex.Matches(text, $@"^[^\n]*\b{Regex.Escape(symbol)}\s*[\(\[=]", RegexOptions.Multiline))
        {
            var start = match.Index;
            var open = text.IndexOf('{', start);
            var semicolon = text.IndexOf(';', start);
            if (open < 0 || (semicolon >= 0 && semicolon < open))
            {
                // A declaration or a one-line definition. Take the statement.
                if (semicolon < 0)
                {
                    continue;
                }

                return text[start..(semicolon + 1)];
            }

            var depth = 0;
            for (var i = open; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    depth++;
                }
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return PrecedingComment(text, start) + text[start..(i + 1)];
                    }
                }
            }
        }

        return null;
    }

    /// <summary>The comment block immediately above a definition, which is where upstream states intent.</summary>
    private static string PrecedingComment(string text, int start)
    {
        var head = text[..start];
        var close = head.LastIndexOf("*/", StringComparison.Ordinal);
        if (close < 0 || head[(close + 2)..].Trim().Length > 0)
        {
            return string.Empty;
        }

        var open = head.LastIndexOf("/*", StringComparison.Ordinal);
        return open < 0 ? string.Empty : head[open..(close + 2)] + "\n";
    }

    private static bool MentionsHashOrNonstandard(string name) =>
        name.Contains("hash", StringComparison.OrdinalIgnoreCase)
        || name.Contains("nonstd", StringComparison.OrdinalIgnoreCase)
        || name.Contains("nonstandard", StringComparison.OrdinalIgnoreCase)
        || name.Contains("non_std", StringComparison.OrdinalIgnoreCase);

    /// <summary>A numeric literal wide enough to be a 10, 12 or 22-bit hash. Matched, never captured.</summary>
    private static readonly Regex NumericLiteral = new(@"\b(?:0[xX][0-9a-fA-F]{2,}|\d{3,})\b", RegexOptions.Compiled);

    /// <summary>A token shaped like a callsign. Matched, never captured.</summary>
    private static readonly Regex CallsignShapedToken = new(
        @"[""'/][A-Za-z0-9]*\d[A-Za-z]{1,4}[A-Za-z0-9/]*",
        RegexOptions.Compiled);

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so no hashing code may be written tonight.");
        }

        return ReferenceClone.Location;
    }

    private static bool IsTextSource(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".c" or ".h" or ".cpp" or ".hpp" or ".txt" or ".md" or ".py" or ".f90" or ".cmake";
    }

    private static int CountLines(string text) => text.Split('\n').Length;

    private static int CountLinesMentioning(string text, string needle) =>
        text.Split('\n').Count(l => l.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static bool IsLineInsideDisabledRegion(string text, int lineNumber)
    {
        var before = string.Join('\n', text.Split('\n').Take(lineNumber - 1));
        var opens = Regex.Matches(before, @"^\s*#\s*if\s+0\b", RegexOptions.Multiline).Count;
        var closes = Regex.Matches(before, @"^\s*#\s*endif\b", RegexOptions.Multiline).Count;
        return opens > closes;
    }

    private static IEnumerable<string> MacroNames(string text) =>
        Regex.Matches(text, @"^\s*#\s*define\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Distinct();

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

    private static IEnumerable<string> FunctionNames(string text) =>
        Regex.Matches(
                text,
                @"^[A-Za-z_][A-Za-z0-9_ \t\*]*?\b([A-Za-z_][A-Za-z0-9_]*)\s*\([^;{]*\)\s*[;{]",
                RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Where(name => name is not ("if" or "for" or "while" or "switch" or "return" or "sizeof"))
            .Distinct();

    /// <summary>File-scope variable and array names, without their initialisers.</summary>
    private static IEnumerable<string> FileScopeVariableNames(string text) =>
        Regex.Matches(
                text,
                @"^(?:static\s+|const\s+)*[A-Za-z_][A-Za-z0-9_]*\s+\**([A-Za-z_][A-Za-z0-9_]*)\s*(?:\[[^\]]*\])?\s*=",
                RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Distinct();
}
