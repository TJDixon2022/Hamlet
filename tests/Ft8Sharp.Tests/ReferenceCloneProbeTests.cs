using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// Asks one question: can a process started by <c>dotnet test</c> read the pinned
/// <c>ft8_lib</c> clone at <c>C:\Source\ft8_lib</c>?
/// </summary>
/// <remarks>
/// <para>
/// The agent's own file tools cannot — both a direct read of that path and a
/// <c>git -C</c> against it are refused by the session's working-directory sandbox.
/// Those refusals are checks on the agent's tools; a compiled program reading a file
/// is the operating system's business, and nothing had established which of the two
/// governs a test process. Two of step 1's exit criteria — a checked-in converter
/// that reads <c>ft8/constants.c</c>, and LDPC parity verified against it — are
/// unreachable until this is answered, so it is answered here rather than assumed.
/// </para>
/// <para>
/// <b>Shapes only, never values.</b> The tables have exactly one legal route into
/// this repository: a checked-in converter that writes them to disk. A reader that
/// prints table contents into a session transcript is a second route wearing a
/// tool's clothes. This probe prints names, C identifiers, types, dimensions and
/// element counts, and never one element of one table.
/// </para>
/// <para>
/// <b>Absent is a skip; present-and-unreadable is a failure.</b> Reference material
/// is never committed, so a fresh clone must stay green without it — see
/// <see cref="RequiresReferenceCloneFactAttribute"/>. But a clone that is there and
/// cannot be read is the finding this probe exists to catch, and it is loud.
/// </para>
/// </remarks>
public class ReferenceCloneProbeTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceCloneProbeTests(ITestOutputHelper output) => _output = output;

    [RequiresReferenceCloneFact]
    public void TestProcessCanReachThePinnedReferenceClone()
    {
        var location = ReferenceClone.Location;
        var reach = ReferenceClone.Probe(out var detail);
        _output.WriteLine($"clone path              : {location}");
        _output.WriteLine($"reachability            : {reach} ({detail})");

        RefuseToPassOnAnUnreadableClone(reach, detail);

        foreach (var relative in new[] { @"ft8\constants.c", @"ft8\constants.h" })
        {
            var path = Path.Combine(location, relative);
            if (File.Exists(path))
            {
                _output.WriteLine(
                    $"{relative,-24}: present, {new FileInfo(path).Length} bytes, {CountLines(path)} lines");
            }
            else
            {
                _output.WriteLine($"{relative,-24}: ABSENT");
            }
        }

        var head = ReferenceClone.ResolveHead(location, out var howRead);
        _output.WriteLine($"HEAD read via           : {howRead}");
        _output.WriteLine($"HEAD                    : {(head.Length == 0 ? "(unreadable)" : head)}");
        _output.WriteLine($"pin                     : {ReferenceClone.PinnedCommit}");
        _output.WriteLine(
            $"HEAD == pin             : {string.Equals(head, ReferenceClone.PinnedCommit, StringComparison.OrdinalIgnoreCase)}");

        // Presence only. Not enumerated, not opened, not one byte read: the folder is
        // Fortran of uncertain provenance and no route to a table may go through it.
        var forbidden = Path.Combine(location, "ft4_ft8_public");
        _output.WriteLine(
            $"ft4_ft8_public present  : {Directory.Exists(forbidden)} (presence only — not enumerated)");

        // Whether ft8_lib could be built on this machine at all. Existence and size only.
        foreach (var buildFile in new[] { "Makefile", "CMakeLists.txt" })
        {
            var path = Path.Combine(location, buildFile);
            _output.WriteLine(File.Exists(path)
                ? $"{buildFile,-24}: present, {new FileInfo(path).Length} bytes"
                : $"{buildFile,-24}: absent");
        }

        var constants = Path.Combine(location, @"ft8\constants.c");
        Assert.True(
            File.Exists(constants),
            $"{location} is readable but {constants} is not there. The tables have to come from "
            + "that file and from no other, so a clone without it cannot serve as the reference.");

        Assert.True(
            string.Equals(head, ReferenceClone.PinnedCommit, StringComparison.OrdinalIgnoreCase),
            $"The clone at {location} is at '{(head.Length == 0 ? "(unreadable)" : head)}' and the pin is "
            + $"'{ReferenceClone.PinnedCommit}'. Everything ported is ported from the pin, so provenance "
            + "cannot be recorded against a clone sitting somewhere else. Read via: " + howRead);
    }

    [RequiresReferenceCloneFact]
    public void ConstantsInventoryIsLegibleAsShapesOnly()
    {
        var reach = ReferenceClone.Probe(out var detail);
        RefuseToPassOnAnUnreadableClone(reach, detail);

        var constants = Path.Combine(ReferenceClone.Location, @"ft8\constants.c");
        Assert.True(File.Exists(constants), $"{constants} is not there.");

        string text;
        try
        {
            text = File.ReadAllText(constants);
        }
        catch (Exception ex)
        {
            Assert.Fail(
                $"{constants} exists but could not be opened by the test process: "
                + $"{ex.GetType().Name}: {ex.Message}. A file that can be seen and not read is the "
                + "finding this probe is for, and it is not a skip.");
            return;
        }

        var tables = InventoryArrays(text);
        _output.WriteLine($"source                  : {constants}");
        _output.WriteLine($"array definitions found : {tables.Count}");
        _output.WriteLine("SHAPES ONLY — no table value is printed here, by ruling.");
        _output.WriteLine($"{"C identifier",-30} {"type",-20} {"dimensions",-28} elements");
        foreach (var t in tables)
        {
            _output.WriteLine($"{t.Name,-30} {t.Type,-20} {t.Dimensions,-28} {t.ElementCount}");
        }

        Assert.True(
            tables.Count > 0,
            $"{constants} was read ({text.Length} bytes) but no array definition was recognised in it. "
            + "Either the file is not what the port expects, or the shape reader needs work — either "
            + "way the converter that follows cannot be authored against it as it stands.");
    }

    /// <summary>
    /// A clone that is present and refuses to be read is the whole point of this probe and may
    /// never be quietly treated as absent.
    /// </summary>
    private static void RefuseToPassOnAnUnreadableClone(ReferenceClone.Reach reach, string detail)
    {
        if (reach == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "This is not the same as the clone being absent and must not be skipped — it means "
                + "nothing in this loop can reach the reference implementation.");
        }
    }

    private static int CountLines(string path)
    {
        using var reader = new StreamReader(path);
        var lines = 0;
        while (reader.ReadLine() is not null)
        {
            lines++;
        }

        return lines;
    }

    private sealed record ArrayShape(string Type, string Name, string Dimensions, int ElementCount);

    /// <summary>
    /// Matches a C array definition with an initialiser: a type, an identifier, one or more
    /// bracketed dimensions, and an opening brace.
    /// </summary>
    private static readonly Regex ArrayDefinition = new(
        @"(?<type>[A-Za-z_][A-Za-z0-9_ \t\*]*?)\s*(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?<dims>(?:\[[^\]]*\]\s*)+)=\s*\{",
        RegexOptions.Compiled);

    /// <summary>
    /// Reports the shape of every table in the file and the content of none of them. Element
    /// counts are arithmetic over the file rather than a reading of it — no value is retained,
    /// returned or printed.
    /// </summary>
    private static List<ArrayShape> InventoryArrays(string source)
    {
        var text = StripCommentsAndLiterals(source);
        var shapes = new List<ArrayShape>();

        foreach (Match match in ArrayDefinition.Matches(text))
        {
            var brace = match.Index + match.Length - 1;
            var end = FindMatchingBrace(text, brace);
            if (end < 0)
            {
                continue;
            }

            var body = text.Substring(brace + 1, end - brace - 1);
            var leaves = body
                .Replace('{', ',')
                .Replace('}', ',')
                .Split(',')
                .Count(part => part.Trim().Length > 0);

            var type = match.Groups["type"].Value.Trim();
            var dims = Regex.Replace(match.Groups["dims"].Value.Trim(), @"\s+", string.Empty);
            shapes.Add(new ArrayShape(
                type.Length == 0 ? "(none stated)" : type,
                match.Groups["name"].Value,
                dims,
                leaves));
        }

        return shapes;
    }

    private static int FindMatchingBrace(string text, int open)
    {
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
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Blanks comments and string and character literals so a comma inside one is not counted as
    /// an element separator. Newlines are preserved so nothing shifts onto another line.
    /// </summary>
    private static string StripCommentsAndLiterals(string source)
    {
        var text = new StringBuilder(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n')
                {
                    i++;
                }

                text.Append('\n');
                continue;
            }

            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < source.Length && !(source[i] == '*' && source[i + 1] == '/'))
                {
                    text.Append(source[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                i++;
                text.Append(' ');
                continue;
            }

            if (source[i] == '"' || source[i] == '\'')
            {
                var quote = source[i];
                i++;
                while (i < source.Length && source[i] != quote)
                {
                    i += source[i] == '\\' ? 2 : 1;
                }

                text.Append('0');
                continue;
            }

            text.Append(source[i]);
        }

        return text.ToString();
    }
}

/// <summary>
/// A fact that skips itself when the pinned reference clone is not on this machine, and runs
/// anyway when the clone is present but refuses to be read.
/// </summary>
/// <remarks>
/// Reference material is never committed — ~21 MB of someone else's off-air recordings and a
/// third party's source do not enter a repository headed for publication — so a fresh clone has
/// to stay green without it. xunit v2 has no dynamic skip in the version this project pins, and
/// adding a package for one is a dependency bought for a sentence; setting <c>Skip</c> from a
/// derived attribute is the idiom that does not.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresReferenceCloneFactAttribute : FactAttribute
{
    public RequiresReferenceCloneFactAttribute()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.Absent)
        {
            Skip = $"The pinned ft8_lib clone is not on this machine at {ReferenceClone.Location}. "
                + $"It is never committed, so this is expected on a fresh clone. {detail}";
        }
    }
}

/// <summary>The pinned upstream clone, and what can be learned about it without reading its data.</summary>
internal static class ReferenceClone
{
    /// <summary>Where the clone lives, outside the tree and never committed.</summary>
    private const string DefaultLocation = @"C:\Source\ft8_lib";

    /// <summary>The commit everything ported is ported from.</summary>
    public const string PinnedCommit = "9fec6ca39886edbf96f4f5e71edc76da5074e871";

    /// <summary>
    /// The clone to probe. Overridable so the skip path can be <em>watched</em> on a machine where
    /// the clone is present — <c>dotnet test -e FT8_LIB_PATH=&lt;nowhere&gt;</c> — rather than
    /// asserted in a comment.
    /// </summary>
    public static string Location =>
        Environment.GetEnvironmentVariable("FT8_LIB_PATH") is { Length: > 0 } configured
            ? configured
            : DefaultLocation;

    public enum Reach
    {
        Reachable,
        Absent,
        PresentButUnreadable,
    }

    /// <summary>
    /// Distinguishes "not there" from "there and refused". <see cref="Directory.Exists(string)"/>
    /// answers false to both, which is exactly the confusion this probe must not make.
    /// </summary>
    public static Reach Probe(out string detail)
    {
        try
        {
            using var entries = Directory.EnumerateFileSystemEntries(Location).GetEnumerator();
            var any = entries.MoveNext();
            detail = any ? "directory enumerated" : "directory enumerated, empty";
            return Reach.Reachable;
        }
        catch (DirectoryNotFoundException ex)
        {
            detail = $"{ex.GetType().Name}: {ex.Message}";
            return Reach.Absent;
        }
        catch (Exception ex)
        {
            detail = $"{ex.GetType().Name}: {ex.Message}";
            return Reach.PresentButUnreadable;
        }
    }

    /// <summary>
    /// Reads the clone's checked-out commit the way git stores it: <c>.git/HEAD</c>, then the ref
    /// it names, then <c>packed-refs</c> if that ref has no loose file.
    /// </summary>
    public static string ResolveHead(string clonePath, out string howRead)
    {
        try
        {
            var gitPath = Path.Combine(clonePath, ".git");
            if (File.Exists(gitPath))
            {
                // A worktree or submodule: .git is a file naming the real git directory.
                var pointer = File.ReadAllText(gitPath).Trim();
                const string prefix = "gitdir:";
                if (pointer.StartsWith(prefix, StringComparison.Ordinal))
                {
                    gitPath = Path.GetFullPath(Path.Combine(clonePath, pointer[prefix.Length..].Trim()));
                }
            }

            var headFile = Path.Combine(gitPath, "HEAD");
            if (!File.Exists(headFile))
            {
                howRead = $"no HEAD file at {headFile}";
                return string.Empty;
            }

            var head = File.ReadAllText(headFile).Trim();
            if (!head.StartsWith("ref:", StringComparison.Ordinal))
            {
                howRead = $"{headFile} (detached HEAD, commit written in place)";
                return head;
            }

            var refName = head[4..].Trim();
            var loose = Path.Combine(gitPath, refName.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(loose))
            {
                howRead = $"{headFile} -> {refName} -> {loose}";
                return File.ReadAllText(loose).Trim();
            }

            var packed = Path.Combine(gitPath, "packed-refs");
            if (File.Exists(packed))
            {
                foreach (var line in File.ReadLines(packed))
                {
                    var parts = line.Split(' ', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length == 2 && parts[1] == refName)
                    {
                        howRead = $"{headFile} -> {refName} -> {packed}";
                        return parts[0];
                    }
                }
            }

            howRead = $"{headFile} names {refName}, which has neither a loose file nor a packed-refs entry";
            return string.Empty;
        }
        catch (Exception ex)
        {
            howRead = $"{ex.GetType().Name}: {ex.Message}";
            return string.Empty;
        }
    }
}
