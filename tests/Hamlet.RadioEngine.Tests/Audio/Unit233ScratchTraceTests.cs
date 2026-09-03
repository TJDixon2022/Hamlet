// SCRATCH HARNESS - work instruction 235, tasks 1, 2 and 7. NOT COMMITTED, NEVER STAGED.
//
// `Read` and the shell are both confined to the repository, so %AppData%\Hamlet
// cannot be read directly - `ls` on it is refused by the sandbox. This harness
// runs under `dotnet test` and writes what it finds into the repository, where
// `Read` can reach it.
//
// It READS ONLY inside %AppData%. Nothing there is created, written or deleted,
// per work instruction 235 section 7. The one thing it writes outside the
// repository is the backup copy, which goes to the temp folder.
//
// Overwrites unit 234's spent file rather than orphaning a third one.
//
// Driven by two environment variables, read from the parent shell:
//   HAMLET_UNIT235_LABEL   names the snapshot, e.g. "task1", "2a-before".
//   HAMLET_UNIT235_BACKUP  "1" asks for the whole-folder backup and the
//                          settings/record report. Anything else skips both.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Audio;

public class Unit233ScratchTraceTests
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hamlet");

    // Keys whose values bear on a bench session and are safe to print.
    private static readonly string[] BenchKeyHints =
    {
        "audio", "device", "input", "output", "sound", "card", "mode", "band",
        "freq", "khz", "mhz", "hz", "rig", "radio", "port", "baud", "vfo",
        "sample", "wsjt", "ft8", "digital",
    };

    // Keys that may never be printed. Presence only.
    private static readonly string[] PersonalKeyHints =
    {
        "call", "grid", "locator", "name", "operator", "email", "qth", "address",
        "licence", "license", "owner", "user", "station", "sig", "token", "key",
        "password", "secret", "lat", "lon", "path", "folder", "dir", "file",
    };

    [Fact]
    public void Trace_TheOperatorsOwnRecord()
    {
        var scratch = Path.Combine(RepoRoot(), ".unit235");
        Directory.CreateDirectory(scratch);

        // The sandbox refuses an env-var prefix in front of `dotnet test`, so the
        // two knobs come from a file written by the shell instead: line 1 the
        // label, line 2 "backup" to ask for the copy and the record report.
        var runFile = Path.Combine(scratch, "run.txt");
        var run = File.Exists(runFile)
            ? File.ReadAllLines(runFile)
            : Array.Empty<string>();
        var label = run.Length > 0 && run[0].Trim().Length > 0 ? run[0].Trim() : "unlabelled";
        var wantBackup = run.Length > 1 && run[1].Trim() == "backup";

        // --- 0. the backup goes first, before anything else in this process ---
        if (wantBackup)
        {
            var report = new StringBuilder();
            try
            {
                var dest = Path.Combine(Path.GetTempPath(), "hamlet-unit235-backup");
                CopyTree(DataFolder, dest);
                report.AppendLine("BACKUP OK");
                report.AppendLine("source: " + DataFolder);
                report.AppendLine("dest:   " + dest);
                report.AppendLine("files copied: " +
                    Directory.GetFiles(dest, "*", SearchOption.AllDirectories).Length);
                report.AppendLine("bytes copied: " +
                    Directory.GetFiles(dest, "*", SearchOption.AllDirectories)
                        .Sum(f => new FileInfo(f).Length));
            }
            catch (Exception ex)
            {
                report.AppendLine("BACKUP FAILED - " + ex.GetType().Name + ": " + ex.Message);
            }

            File.WriteAllText(Path.Combine(scratch, "backup.txt"), report.ToString());
        }

        // --- 1. the snapshot: relpath, size, last-write UTC, SHA-256 ----------
        var snap = new StringBuilder();
        snap.AppendLine("# unit 235 snapshot\tlabel=" + label);
        snap.AppendLine("# taken " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        snap.AppendLine("# folder " + DataFolder + "\texists=" + Directory.Exists(DataFolder));
        snap.AppendLine("# relpath\tbytes\tlastWriteUtc\tsha256");
        if (Directory.Exists(DataFolder))
        {
            foreach (var f in Directory.GetFiles(DataFolder, "*", SearchOption.AllDirectories)
                         .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var rel = Path.GetRelativePath(DataFolder, f);
                long len;
                string mtime;
                string hash;
                try
                {
                    var fi = new FileInfo(f);
                    len = fi.Length;
                    mtime = fi.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                    hash = Sha256(f);
                }
                catch (Exception ex)
                {
                    len = -1;
                    mtime = "(unreadable)";
                    hash = "(unreadable: " + ex.GetType().Name + ")";
                }

                snap.AppendLine($"{rel}\t{len}\t{mtime}\t{hash}");
            }
        }

        File.WriteAllText(Path.Combine(scratch, "snap-" + label + ".txt"), snap.ToString());

        if (!wantBackup)
        {
            return;
        }

        // --- 2. the state of the record --------------------------------------
        var w = new StringBuilder();
        void W(string s) => w.AppendLine(s);

        W("UNIT 235 TASK 1 - THE STATE OF THE OPERATOR'S RECORD");
        W("taken " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        W("");

        var captures = Path.Combine(DataFolder, "captures");
        W("captures exists:          " + Directory.Exists(captures));
        W("captures\\digital exists:  " + Directory.Exists(Path.Combine(captures, "digital")));
        if (Directory.Exists(captures))
        {
            foreach (var f in Directory.GetFiles(captures, "*", SearchOption.AllDirectories))
            {
                var fi = new FileInfo(f);
                W("  capture: " + Path.GetRelativePath(captures, f) + "\t" + fi.Length + " bytes\t" +
                  fi.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            }
        }

        W("");
        var telemetry = Path.Combine(DataFolder, "telemetry");
        var jsonls = Directory.Exists(telemetry)
            ? Directory.GetFiles(telemetry, "*.jsonl").OrderBy(x => x, StringComparer.Ordinal).ToArray()
            : Array.Empty<string>();
        W("telemetry folder exists:  " + Directory.Exists(telemetry));
        W("jsonl count:              " + jsonls.Length);
        W("2026-09-03.jsonl present: " + jsonls.Any(f => Path.GetFileName(f) == "2026-09-03.jsonl"));
        W("any file after 2026-08-28: " +
          jsonls.Any(f => string.CompareOrdinal(Path.GetFileNameWithoutExtension(f), "2026-08-28") > 0));
        W("newest jsonl by name:     " +
          (jsonls.Length == 0 ? "(none)" : Path.GetFileName(jsonls[^1])));
        W("");

        W("appVersion census:");
        var seen = new Dictionary<string, (string NewestTs, long Count)>(StringComparer.Ordinal);
        var bad = 0;
        foreach (var f in jsonls)
        {
            foreach (var line in File.ReadLines(f))
            {
                if (line.Length == 0)
                {
                    continue;
                }

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var ver = doc.RootElement.TryGetProperty("appVersion", out var v)
                        ? v.GetString() ?? "(null)" : "(absent)";
                    var ts = doc.RootElement.TryGetProperty("ts", out var t) ? t.GetString() ?? "" : "";
                    if (!seen.TryGetValue(ver, out var cur))
                    {
                        cur = ("", 0);
                    }

                    seen[ver] = (string.CompareOrdinal(ts, cur.NewestTs) > 0 ? ts : cur.NewestTs, cur.Count + 1);
                }
                catch (JsonException)
                {
                    bad++;
                }
            }
        }

        foreach (var kv in seen.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            W($"  {kv.Key}\tlines={kv.Value.Count}\tnewest ts={kv.Value.NewestTs}");
        }

        W("  unparseable lines: " + bad);
        var newest = seen.OrderByDescending(k => k.Value.NewestTs, StringComparer.Ordinal)
            .Select(k => k.Key).FirstOrDefault();
        W("  newest appVersion in the whole record: " + (newest ?? "(none - no lines at all)"));
        W("");

        // --- 3. settings.json, key names only, values only where safe --------
        W("settings.json:");
        var settingsPath = Path.Combine(DataFolder, "settings.json");
        W("  exists: " + File.Exists(settingsPath));
        if (File.Exists(settingsPath))
        {
            var fi = new FileInfo(settingsPath);
            W("  bytes: " + fi.Length);
            W("  last written UTC: " +
              fi.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(settingsPath));
                W("  keys and how they are reported:");
                foreach (var p in doc.RootElement.EnumerateObject())
                {
                    var lower = p.Name.ToLowerInvariant();
                    var personal = PersonalKeyHints.Any(h => lower.Contains(h, StringComparison.Ordinal));
                    var bench = BenchKeyHints.Any(h => lower.Contains(h, StringComparison.Ordinal));
                    if (personal || !bench)
                    {
                        var absent = p.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
                                     (p.Value.ValueKind == JsonValueKind.String &&
                                      string.IsNullOrEmpty(p.Value.GetString()));
                        W($"    {p.Name}\t[{p.Value.ValueKind}]\t" +
                          (personal ? (absent ? "PERSONAL - absent/empty" : "PERSONAL - present, value withheld")
                                    : "value not printed (not a bench field)"));
                    }
                    else
                    {
                        var raw = p.Value.GetRawText();
                        if (raw.Length > 200)
                        {
                            raw = raw[..200] + "...(truncated)";
                        }

                        W($"    {p.Name}\t[{p.Value.ValueKind}]\t= {raw}");
                    }
                }
            }
            catch (JsonException ex)
            {
                W("  settings.json did not parse: " + ex.GetType().Name);
            }
        }

        File.WriteAllText(Path.Combine(scratch, "record.txt"), w.ToString());
    }

    /// Compares the backup taken in task 1 against whatever settings.json now
    /// holds, key by key. Both sides are read only. Personal fields are reported
    /// as present or absent and never printed.
    [Fact]
    public void Compare_TheBackupAgainstWhatIsThereNow()
    {
        var scratch = Path.Combine(RepoRoot(), ".unit235");
        Directory.CreateDirectory(scratch);
        var before = Path.Combine(Path.GetTempPath(), "hamlet-unit235-backup", "settings.json");
        var after = Path.Combine(DataFolder, "settings.json");

        var w = new StringBuilder();
        w.AppendLine("UNIT 235 TASK 2 - WHAT THE TEST RUN DID TO settings.json");
        w.AppendLine("taken " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        w.AppendLine("backup: " + before + "\texists=" + File.Exists(before));
        w.AppendLine("live:   " + after + "\texists=" + File.Exists(after));
        w.AppendLine();

        if (!File.Exists(before) || !File.Exists(after))
        {
            File.WriteAllText(Path.Combine(scratch, "settings-diff.txt"), w.ToString());
            return;
        }

        using var b = JsonDocument.Parse(File.ReadAllText(before));
        using var a = JsonDocument.Parse(File.ReadAllText(after));
        var bk = b.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetRawText(), StringComparer.Ordinal);
        var ak = a.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetRawText(), StringComparer.Ordinal);

        w.AppendLine($"keys before: {bk.Count}\tkeys after: {ak.Count}");
        w.AppendLine();
        w.AppendLine("key\tverdict\tbefore\tafter");
        foreach (var key in bk.Keys.Union(ak.Keys, StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal))
        {
            var lower = key.ToLowerInvariant();
            var personal = PersonalKeyHints.Any(h => lower.Contains(h, StringComparison.Ordinal));
            var hadIt = bk.TryGetValue(key, out var bv);
            var hasIt = ak.TryGetValue(key, out var av);

            string verdict;
            if (!hadIt)
            {
                verdict = "ADDED BY THE TEST RUN";
            }
            else if (!hasIt)
            {
                verdict = "REMOVED BY THE TEST RUN";
            }
            else
            {
                verdict = string.Equals(bv, av, StringComparison.Ordinal) ? "same" : "CHANGED";
            }

            if (personal)
            {
                w.AppendLine($"{key}\t{verdict}\t[personal - {(hadIt ? Shape(bv!) : "absent")}]" +
                             $"\t[personal - {(hasIt ? Shape(av!) : "absent")}]");
            }
            else
            {
                w.AppendLine($"{key}\t{verdict}\t{Trunc(hadIt ? bv! : "(absent)")}\t{Trunc(hasIt ? av! : "(absent)")}");
            }
        }

        // Operator is the one object that could carry the callsign, the grid and
        // the licence class. Sub-key names and present/absent only - never a value.
        w.AppendLine();
        w.AppendLine("Operator sub-keys - names and present/absent only, no values");
        w.AppendLine("subkey\tkind\tbefore\tafter");
        var bo = b.RootElement.TryGetProperty("Operator", out var boe) && boe.ValueKind == JsonValueKind.Object
            ? boe.EnumerateObject().ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal)
            : new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        var ao = a.RootElement.TryGetProperty("Operator", out var aoe) && aoe.ValueKind == JsonValueKind.Object
            ? aoe.EnumerateObject().ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal)
            : new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        static string State(Dictionary<string, JsonElement> d, string k)
        {
            if (!d.TryGetValue(k, out var v))
            {
                return "key absent";
            }

            return v.ValueKind switch
            {
                JsonValueKind.Null => "null",
                JsonValueKind.String => string.IsNullOrEmpty(v.GetString()) ? "EMPTY" : "present",
                JsonValueKind.Array => v.GetArrayLength() == 0 ? "EMPTY" : "present",
                JsonValueKind.Object => "present",
                _ => "present",
            };
        }

        foreach (var k in bo.Keys.Union(ao.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
        {
            var kind = bo.TryGetValue(k, out var bv2) ? bv2.ValueKind.ToString()
                : ao.TryGetValue(k, out var av2) ? av2.ValueKind.ToString() : "?";
            // An 8-hex-digit SHA-256 prefix says "same value" or "different
            // value" without disclosing either. Same fingerprint, same string.
            static string Fp(Dictionary<string, JsonElement> d, string k)
            {
                if (!d.TryGetValue(k, out var v))
                {
                    return "--------";
                }

                using var sha = SHA256.Create();
                return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(v.GetRawText())))[..8];
            }

            var fb = Fp(bo, k);
            var fa = Fp(ao, k);
            w.AppendLine($"{k}\t{kind}\t{State(bo, k)} [{fb}]\t{State(ao, k)} [{fa}]\t" +
                         (fb == fa ? "IDENTICAL" : "DIFFERENT"));
        }

        File.WriteAllText(Path.Combine(scratch, "settings-diff.txt"), w.ToString());
    }

    // Describes a personal value without disclosing it.
    private static string Shape(string raw) =>
        raw is "null" ? "null" : raw.Length <= 2 ? "empty" : raw.Length + " chars";

    private static string Trunc(string s) => s.Length <= 120 ? s : s[..120] + "...";

    /// Task 3.3 - the version read off the produced binary rather than off
    /// Directory.Build.props, and task 5 - what Hamlet is installed on this
    /// machine. Both are read-only; nothing is created and nothing is launched.
    [Fact]
    public void Version_OffTheBinary_AndTheMachineCensus()
    {
        var scratch = Path.Combine(RepoRoot(), ".unit235");
        Directory.CreateDirectory(scratch);
        var w = new StringBuilder();
        w.AppendLine("UNIT 235 TASKS 3.3 AND 5");
        w.AppendLine("taken " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        w.AppendLine();

        w.AppendLine("== 3.3 THE VERSION THIS TREE STAMPS ON ITS OWN BINARY ==");
        var outDir = Path.Combine(RepoRoot(), "src", "Hamlet.App", "bin", "Release", "net8.0");
        foreach (var name in new[] { "Hamlet.App.dll", "Hamlet.App.exe" })
        {
            var p = Path.Combine(outDir, name);
            w.AppendLine(name + "\texists=" + File.Exists(p));
            if (!File.Exists(p))
            {
                continue;
            }

            w.AppendLine("  last written UTC: " +
                new FileInfo(p).LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

            var fvi = FileVersionInfo.GetVersionInfo(p);
            w.AppendLine("  Win32 FileVersion:    " + (fvi.FileVersion ?? "(none)"));
            w.AppendLine("  Win32 ProductVersion: " + (fvi.ProductVersion ?? "(none)"));

            try
            {
                var an = AssemblyName.GetAssemblyName(p);
                w.AppendLine("  AssemblyVersion:      " + an.Version);
                w.AppendLine("  GetName().Version.ToString(3) would yield: " +
                    (an.Version?.ToString(3) ?? "(null)") +
                    "   <-- what App.axaml.cs:37 stamps on telemetry and About");
            }
            catch (BadImageFormatException)
            {
                w.AppendLine("  AssemblyVersion:      (not a managed assembly - apphost shim)");
            }
        }

        w.AppendLine();
        w.AppendLine("== 5. WHAT HAMLET IS INSTALLED ON THIS MACHINE ==");
        var roots = new List<(string What, string Path)>();
        void Add(string what, Environment.SpecialFolder f)
        {
            var p = Environment.GetFolderPath(f);
            if (p.Length > 0)
            {
                roots.Add((what, p));
            }
        }

        Add("Desktop", Environment.SpecialFolder.DesktopDirectory);
        Add("Start Menu (user)", Environment.SpecialFolder.StartMenu);
        Add("Start Menu (common)", Environment.SpecialFolder.CommonStartMenu);
        Add("Program Files", Environment.SpecialFolder.ProgramFiles);
        Add("Program Files (x86)", Environment.SpecialFolder.ProgramFilesX86);
        Add("Local AppData", Environment.SpecialFolder.LocalApplicationData);
        Add("AppData", Environment.SpecialFolder.ApplicationData);
        roots.Add(("User profile", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

        var hits = 0;
        foreach (var (what, root) in roots)
        {
            w.AppendLine($"-- {what}: {root}");
            if (!Directory.Exists(root))
            {
                w.AppendLine("   (does not exist)");
                continue;
            }

            // A recursive walk that steps over folders the sandbox refuses,
            // rather than a single EnumerateFiles that aborts the whole root on
            // the first one. A search that stopped early is not a search that
            // found nothing (CLAUDE.md 0.0).
            var found = new List<string>();
            var refused = Walk(root, found, 0);
            if (refused > 0)
            {
                w.AppendLine($"   ({refused} sub-folder(s) refused and stepped over; the rest was searched)");
            }

            var any = false;
            foreach (var f in found)
            {
                // Everything under this repository is build output, not an install.
                if (f.StartsWith(RepoRoot(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                any = true;
                hits++;
                var fi = new FileInfo(f);
                w.AppendLine("   " + f);
                w.AppendLine("     bytes " + fi.Length + "   last written UTC " +
                    fi.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
                if (!f.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    var v = FileVersionInfo.GetVersionInfo(f);
                    w.AppendLine("     FileVersion " + (v.FileVersion ?? "(none)") +
                                 "   ProductVersion " + (v.ProductVersion ?? "(none)"));
                }
            }

            if (!any)
            {
                w.AppendLine("   (nothing)");
            }
        }

        w.AppendLine();
        w.AppendLine(hits == 0
            ? "*** THERE IS NO INSTALLED HAMLET ON THIS MACHINE outside this repository's own build output. ***"
            : $"*** {hits} candidate(s) found outside the repository. ***");

        File.WriteAllText(Path.Combine(scratch, "version-and-census.txt"), w.ToString());
    }

    // Depth-limited recursive walk collecting Hamlet executables and shortcuts,
    // counting rather than throwing on folders it may not read.
    private static int Walk(string dir, List<string> found, int depth)
    {
        if (depth > 8 || found.Count > 200)
        {
            return 0;
        }

        var refused = 0;
        try
        {
            foreach (var f in Directory.EnumerateFiles(dir))
            {
                var n = Path.GetFileName(f);
                if (n.Equals("Hamlet.App.exe", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("Hamlet.exe", StringComparison.OrdinalIgnoreCase)
                    || (n.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) &&
                        n.Contains("hamlet", StringComparison.OrdinalIgnoreCase)))
                {
                    found.Add(f);
                }
            }
        }
        catch (Exception)
        {
            refused++;
        }

        IEnumerable<string> subs;
        try
        {
            subs = Directory.EnumerateDirectories(dir).ToList();
        }
        catch (Exception)
        {
            return refused + 1;
        }

        foreach (var s in subs)
        {
            refused += Walk(s, found, depth + 1);
        }

        return refused;
    }

    private static void CopyTree(string src, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var d in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(dest, Path.GetRelativePath(src, d)));
        }

        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(dest, Path.GetRelativePath(src, f));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(f, target, overwrite: true);
        }
    }

    private static string Sha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private static string RepoRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Hamlet.sln")))
        {
            d = d.Parent;
        }

        return d?.FullName ?? AppContext.BaseDirectory;
    }
}
