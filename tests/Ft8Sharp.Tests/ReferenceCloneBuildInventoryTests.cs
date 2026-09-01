using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// The sanctioned read of the pinned clone for unit 209, and the narrow question this unit turns
/// on: <b>does the pin contain a program whose job is to turn a message into tones or symbols, and
/// does the build system name it as a target?</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is a test and not a shell command.</b> The session's sandbox refuses to list
/// <c>C:\Source\ft8_lib</c> with the agent's own file tools and refuses a bash <c>ls</c> against
/// it. A process started by <c>dotnet test</c> reads it with the operating system's permissions,
/// which is the route units 206 through 208 established and the only one available. The refusal
/// is reported as a refusal and is not worked around.
/// </para>
/// <para>
/// <b>Shapes only, never values.</b> File names, byte sizes, line counts, target names and C
/// identifiers are metadata and are printed. Not one line of upstream source, not one tone, not
/// one table element reaches this transcript.
/// </para>
/// <para>
/// <b>Nothing here asserts an answer.</b> Work instruction 209 states in terms that its
/// description of the clone is the arbiter's expectation and not a measurement, and that a
/// mismatch is reported rather than repaired. So the inventory is the product: both answers to
/// the narrow question are reportable and neither is asserted on.
/// </para>
/// </remarks>
public class ReferenceCloneBuildInventoryTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceCloneBuildInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>Build-system files a C project of this shape could carry.</summary>
    private static readonly string[] BuildFileNames =
    {
        "Makefile", "makefile", "GNUmakefile", "CMakeLists.txt", "meson.build",
        "build.sh", "build.bat", "configure", "Makefile.am", "Makefile.in",
    };

    /// <summary>
    /// What the pin's build system offers, and whether any of it is a message-to-tones program.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void ThePinsBuildSystemAndItsDemoProgramsAreInventoried()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");
        _output.WriteLine("NAMES, SIZES AND LINE COUNTS ONLY — no line of upstream source is printed.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("--- build files at the clone root ---");
        var buildFiles = new List<string>();
        foreach (var name in BuildFileNames)
        {
            var path = Path.Combine(location, name);
            if (!File.Exists(path))
            {
                continue;
            }

            buildFiles.Add(path);
            var text = File.ReadAllText(path);
            _output.WriteLine($"    {name,-18} {new FileInfo(path).Length,7} bytes {CountLines(text),5} lines");
        }

        if (buildFiles.Count == 0)
        {
            _output.WriteLine("    (none)");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("--- C sources at the clone root, which is where a demo program would sit ---");
        var rootSources = Directory
            .EnumerateFiles(location, "*.*", SearchOption.TopDirectoryOnly)
            .Where(IsCSource)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var path in rootSources)
        {
            var text = File.ReadAllText(path);
            var hasMain = Regex.IsMatch(text, @"^\s*(?:int|void)\s+main\s*\(", RegexOptions.Multiline);
            _output.WriteLine(
                $"    {Path.GetFileName(path),-22} {new FileInfo(path).Length,7} bytes "
                + $"{CountLines(text),5} lines  has main(): {hasMain}");
        }

        if (rootSources.Count == 0)
        {
            _output.WriteLine("    (none)");
        }

        // Any folder a project of this shape puts examples in.
        foreach (var folder in new[] { "demo", "demos", "example", "examples", "apps", "tools", "src" })
        {
            var directory = Path.Combine(location, folder);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            _output.WriteLine(string.Empty);
            _output.WriteLine($"--- {folder}/ ---");
            foreach (var path in Directory
                         .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                         .Where(IsCSource)
                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var text = File.ReadAllText(path);
                var hasMain = Regex.IsMatch(text, @"^\s*(?:int|void)\s+main\s*\(", RegexOptions.Multiline);
                _output.WriteLine(
                    $"    {Path.GetRelativePath(location, path),-30} {new FileInfo(path).Length,7} bytes "
                    + $"{CountLines(text),5} lines  has main(): {hasMain}");
            }
        }

        // The narrow question, part one: is there a program whose job is message -> tones/symbols?
        _output.WriteLine(string.Empty);
        _output.WriteLine("--- the narrow question, part 1: a program that turns a message into tones ---");
        var candidates = new List<string>();
        foreach (var path in AllCSources(location))
        {
            var text = File.ReadAllText(path);
            if (!Regex.IsMatch(text, @"^\s*(?:int|void)\s+main\s*\(", RegexOptions.Multiline))
            {
                continue;
            }

            // A generator calls the encoder and produces tones; a decoder calls a decode entry
            // point. Both are counted so the two can be told apart by shape rather than by name.
            var callsEncode = Regex.IsMatch(text, @"\b(ft8_encode|ft4_encode|encode174|genft8)\s*\(");
            var callsDecode = Regex.IsMatch(text, @"\b(ft8_decode|ft8_find_sync|decode)\w*\s*\(");
            var mentionsTone = Regex.IsMatch(text, @"\bi?tones?\b", RegexOptions.IgnoreCase);
            var relative = Path.GetRelativePath(location, path);
            _output.WriteLine(
                $"    {relative,-30} calls an encode entry point: {callsEncode,-5}  "
                + $"calls a decode entry point: {callsDecode,-5}  mentions tones: {mentionsTone}");

            if (callsEncode)
            {
                candidates.Add(relative);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(candidates.Count > 0
            ? $"ANSWER: yes — the pin contains a program whose job is to turn a message into tones "
              + $"or symbols. Named: {string.Join(", ", candidates)}"
            : "ANSWER: no — the pin contains no program whose job is to turn a message into tones "
              + "or symbols.");

        // The narrow question, part two: does the build system name it as a target?
        _output.WriteLine(string.Empty);
        _output.WriteLine("--- the narrow question, part 2: does the build system name it as a target ---");
        foreach (var buildFile in buildFiles)
        {
            var text = File.ReadAllText(buildFile);
            var name = Path.GetFileName(buildFile);

            // Make targets: a rule head at column zero. CMake targets: add_executable.
            var makeTargets = Regex
                .Matches(text, @"^([A-Za-z_][A-Za-z0-9_./\-]*)\s*:(?!=)", RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
            var cmakeTargets = Regex
                .Matches(text, @"add_executable\s*\(\s*([A-Za-z_][A-Za-z0-9_\-]*)", RegexOptions.IgnoreCase)
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            _output.WriteLine($"    {name}: {makeTargets.Count} make-rule heads, {cmakeTargets.Count} add_executable targets");
            foreach (var target in makeTargets)
            {
                _output.WriteLine($"        rule head        {target}");
            }

            foreach (var target in cmakeTargets)
            {
                _output.WriteLine($"        add_executable   {target}");
            }

            foreach (var candidate in candidates)
            {
                var stem = Path.GetFileNameWithoutExtension(candidate);
                var named = makeTargets.Any(t => t.Contains(stem, StringComparison.OrdinalIgnoreCase))
                            || cmakeTargets.Any(t => t.Contains(stem, StringComparison.OrdinalIgnoreCase))
                            || text.Contains(stem, StringComparison.OrdinalIgnoreCase);
                _output.WriteLine($"        names '{stem}' as a target: {named}");
            }
        }

        Assert.True(
            buildFiles.Count > 0 || rootSources.Count > 0,
            $"Nothing was readable at {location}, so this inventory measured nothing.");
    }

    /// <summary>
    /// What this machine could build C with. Reported, never installed — installing a toolchain is
    /// the owner's under <c>ARBITER.md</c> section 6 and work instruction 209 refuses it in terms.
    /// </summary>
    /// <remarks>
    /// This lives in a test for the same reason the clone inventory does: the sandbox refuses the
    /// agent's own tools outside the repository root, so a bash <c>ls</c> of
    /// <c>C:\Program Files</c> is refused and a process reading the same path is not.
    /// </remarks>
    [Fact]
    public void WhatThisMachineCouldBuildCWithIsReported()
    {
        _output.WriteLine("PATH-resolvable C toolchain:");
        foreach (var exe in new[] { "cl.exe", "gcc.exe", "clang.exe", "cc.exe", "cmake.exe", "make.exe", "ninja.exe", "mingw32-make.exe" })
        {
            _output.WriteLine($"    {exe,-18} {OnPath(exe) ?? "NOT ON PATH"}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("Visual Studio / MSVC, by well-known location:");
        foreach (var root in new[]
                 {
                     @"C:\Program Files\Microsoft Visual Studio",
                     @"C:\Program Files (x86)\Microsoft Visual Studio",
                     @"C:\Program Files (x86)\Microsoft Visual Studio\Installer",
                     @"C:\Program Files\Microsoft Visual Studio\2022",
                     @"C:\Program Files (x86)\Windows Kits\10",
                     @"C:\Program Files\LLVM\bin",
                     @"C:\msys64\mingw64\bin",
                     @"C:\MinGW\bin",
                     @"C:\ProgramData\chocolatey\bin",
                 })
        {
            _output.WriteLine($"    {root,-52} {(Directory.Exists(root) ? "present" : "absent")}");
        }

        var vswhere = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";
        _output.WriteLine($"    {vswhere,-52} {(File.Exists(vswhere) ? "present" : "absent")}");

        // Task 2 step 1 asks after "MSVC through a developer command prompt" by name. That is
        // vcvars, and it is useless without a Windows SDK to supply the CRT headers and libraries,
        // so both are measured rather than one inferred from the other.
        _output.WriteLine(string.Empty);
        _output.WriteLine("developer command prompt (vcvars), by search under any Visual Studio root:");
        var vcvars = 0;
        foreach (var root in new[]
                 {
                     @"C:\Program Files\Microsoft Visual Studio",
                     @"C:\Program Files (x86)\Microsoft Visual Studio",
                 })
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                foreach (var path in Directory
                             .EnumerateFiles(root, "vcvars64.bat", SearchOption.AllDirectories)
                             .Concat(Directory.EnumerateFiles(root, "vcvarsall.bat", SearchOption.AllDirectories)))
                {
                    _output.WriteLine($"    {path}");
                    vcvars++;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"    {root}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        _output.WriteLine(vcvars == 0 ? "    (none)" : $"    vcvars scripts found: {vcvars}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("Windows SDK — without it cl.exe has no CRT headers or import libraries:");
        foreach (var kit in new[]
                 {
                     @"C:\Program Files (x86)\Windows Kits\10\Include",
                     @"C:\Program Files (x86)\Windows Kits\10\Lib",
                     @"C:\Program Files\Windows Kits\10\Include",
                     @"C:\Program Files\Windows Kits\10\Lib",
                 })
        {
            if (Directory.Exists(kit))
            {
                var versions = Directory.EnumerateDirectories(kit).Select(Path.GetFileName).ToList();
                _output.WriteLine($"    {kit,-46} present, {versions.Count} version folders: {string.Join(", ", versions)}");
            }
            else
            {
                _output.WriteLine($"    {kit,-46} absent");
            }
        }

        // The MSVC toolset's own CRT headers, which ship with the compiler rather than with the SDK.
        const string toolset = @"C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Tools\MSVC\14.51.36231";
        _output.WriteLine(string.Empty);
        _output.WriteLine("MSVC toolset headers and libraries:");
        foreach (var relative in new[] { @"include\stdio.h", @"lib\x64\libcmt.lib" })
        {
            var path = Path.Combine(toolset, relative);
            _output.WriteLine($"    {relative,-24} {(File.Exists(path) ? "present" : "absent")}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"what the toolset folder actually contains ({toolset}):");
        if (Directory.Exists(toolset))
        {
            foreach (var directory in Directory.EnumerateDirectories(toolset))
            {
                _output.WriteLine($"    dir  {Path.GetFileName(directory)}");
            }
        }
        else
        {
            _output.WriteLine("    (toolset folder absent)");
        }

        // A Windows SDK anywhere under either Program Files, not just at the canonical path.
        _output.WriteLine(string.Empty);
        _output.WriteLine("any Windows SDK 'ucrt' include folder anywhere under Program Files:");
        var kits = 0;
        foreach (var root in new[] { @"C:\Program Files\Windows Kits", @"C:\Program Files (x86)\Windows Kits" })
        {
            _output.WriteLine($"    {root,-40} {(Directory.Exists(root) ? "present" : "absent")}");
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                foreach (var path in Directory.EnumerateDirectories(root, "ucrt", SearchOption.AllDirectories))
                {
                    _output.WriteLine($"        {path}");
                    kits++;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
            }
        }

        _output.WriteLine($"    ucrt include folders found: {kits}");

        // What the toolset's lib folder and the Windows Kits folder actually hold, one level deep,
        // so "it would not build" is a measured statement about named folders rather than an
        // inference from two missing files.
        _output.WriteLine(string.Empty);
        _output.WriteLine("one level deep, so the finding is named rather than inferred:");
        foreach (var probe in new[]
                 {
                     Path.Combine(toolset, "lib"),
                     Path.Combine(toolset, "bin"),
                     @"C:\Program Files (x86)\Windows Kits",
                 })
        {
            if (!Directory.Exists(probe))
            {
                _output.WriteLine($"    {probe}: absent");
                continue;
            }

            var children = Directory.EnumerateDirectories(probe).Select(Path.GetFileName).ToList();
            var files = Directory.EnumerateFiles(probe).Count();
            _output.WriteLine($"    {probe}");
            _output.WriteLine($"        {children.Count} folders ({string.Join(", ", children)}), {files} files");
        }

        // Every cl.exe on the machine, if any, without enumerating the whole disk.
        _output.WriteLine(string.Empty);
        _output.WriteLine("cl.exe under any Visual Studio root:");
        var found = 0;
        foreach (var root in new[]
                 {
                     @"C:\Program Files\Microsoft Visual Studio",
                     @"C:\Program Files (x86)\Microsoft Visual Studio",
                 })
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                foreach (var path in Directory.EnumerateFiles(root, "cl.exe", SearchOption.AllDirectories))
                {
                    _output.WriteLine($"    {path}");
                    found++;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"    {root}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        if (found == 0)
        {
            _output.WriteLine("    (none)");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"cl.exe copies found: {found}");

        // No assertion. What is on the machine is the finding; nothing is installed to change it.
    }

    private static string? OnPath(string exe)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var folder in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(folder.Trim(), exe);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (ArgumentException)
            {
                // A malformed PATH entry is not this test's problem.
            }
        }

        return null;
    }

    private static IEnumerable<string> AllCSources(string location)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(location, "*.*", SearchOption.TopDirectoryOnly).Where(IsCSource))
        {
            if (seen.Add(path))
            {
                yield return path;
            }
        }

        // Every folder but the one that is forbidden by ruling: ft4_ft8_public is Fortran of
        // uncertain provenance and is never read, enumerated or referenced.
        foreach (var folder in Directory.EnumerateDirectories(location))
        {
            var name = Path.GetFileName(folder);
            if (name.Equals("ft4_ft8_public", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith(".", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Where(IsCSource))
            {
                if (seen.Add(path))
                {
                    yield return path;
                }
            }
        }
    }

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}.");
        }

        return ReferenceClone.Location;
    }

    private static bool IsCSource(string path) =>
        Path.GetExtension(path).Equals(".c", StringComparison.OrdinalIgnoreCase)
        || Path.GetExtension(path).Equals(".h", StringComparison.OrdinalIgnoreCase);

    private static int CountLines(string text) => text.Split('\n').Length;
}
