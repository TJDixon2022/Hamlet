using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// **The eight kinds of achievement, named once.**
/// </summary>
/// <remarks>
/// **THE KEYS ARE THE POINTS FILE'S OWN SPELLING**, because the file is the owner's to
/// edit and a key he has to guess at is a key he will get wrong. `hall_of_fame` and
/// `total_miles` carry their underscores here for exactly that reason.
/// </remarks>
public static class AchievementKinds
{
    /// <summary>Firsts that happen once.</summary>
    public const string HallOfFame = "hall_of_fame";

    /// <summary>A first in each of the seven.</summary>
    public const string Continents = "continents";

    /// <summary>One card per DXCC entity.</summary>
    public const string Countries = "countries";

    /// <summary>The fifty, plus DC.</summary>
    public const string States = "states";

    /// <summary>Four-character squares.</summary>
    public const string Grids = "grids";

    /// <summary>Grid to grid, added up.</summary>
    public const string TotalMiles = "total_miles";

    /// <summary>A first on each band.</summary>
    public const string Bands = "bands";

    /// <summary>A first in each mode.</summary>
    public const string Modes = "modes";

    /// <summary>**The eight, in the order the page draws them.**</summary>
    /// <remarks>
    /// The order is the approved mockup's, read left to right and top to bottom:
    /// Hall of Fame, Continents, Countries, States, then Grids, Total Miles, Bands,
    /// Modes.
    /// </remarks>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        HallOfFame, Continents, Countries, States, Grids, TotalMiles, Bands, Modes,
    };
}

/// <summary>
/// **The owner's points file, read and never written by code.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"we're moving away from the achievements all count the
/// same… getting all continents is hard… 10 million is hard… the points don't matter."*
/// The numbers are his judgement of difficulty from his own station, they are arbitrary by
/// his own ruling, and **they are therefore not code's to hold** (§0: where something can
/// come from a source of truth, it comes from the source of truth).</para>
/// <para>**NO POINT VALUE IS HARD-CODED ANYWHERE IN HAMLET.** Every figure this type
/// exposes came out of the file. Where the file does not say, the answer is **absent** and
/// not a default: a screen showing a score Hamlet made up is a screen showing a guess as a
/// measurement (§0.0).</para>
/// <para>**A MISSING OR MALFORMED FILE IS A REPORTED STATE AND NOT A CRASH.**
/// <see cref="Read"/> never throws. <see cref="Loaded"/> is false, <see cref="Problem"/>
/// says what happened in one line, and the page shows the scores as absent with that line
/// under them - which is different from every kind scoring nought.</para>
/// </remarks>
public sealed class AchievementPoints
{
    private readonly Dictionary<string, int> _continentPer = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _bandSpecial = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _modeSpecial = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _stateSpecial = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _hallOfFame = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _per = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _all = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, IReadOnlyList<(long At, int Points)>> _milestones
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, IReadOnlyList<long>> _levels
        = new(StringComparer.OrdinalIgnoreCase);

    private AchievementPoints()
    {
    }

    /// <summary>True where a file was read and understood.</summary>
    public bool Loaded { get; private init; }

    /// <summary>What went wrong, in one line, or "".</summary>
    /// <remarks>
    /// **IT NAMES THE FILE AND THE FAULT AND ASKS FOR NOTHING.** The owner is the only
    /// person who can fix it, and he fixes it by opening the file - so the line says where
    /// the file is.
    /// </remarks>
    public string Problem { get; private init; } = "";

    /// <summary>The `_about` line the file carries, or "".</summary>
    public string About { get; private init; } = "";

    /// <summary>A short hash of the bytes read, for the telemetry record.</summary>
    /// <remarks>
    /// **THE HASH AND NEVER THE CONTENTS** (§R13). It says *this is the file that was
    /// loaded* so two runs can be told apart, and it carries no callsign, no entity name
    /// and no point value.
    /// </remarks>
    public string Hash { get; private init; } = "";

    /// <summary>The rank thresholds, ascending, or empty.</summary>
    public IReadOnlyList<long> Ranks { get; private init; } = Array.Empty<long>();

    /// <summary>How many kinds the file carried a section for.</summary>
    public int Kinds { get; private set; }

    /// <summary>Nothing was read. Every score is absent.</summary>
    /// <param name="problem">What to say on the page.</param>
    /// <returns>The absent state.</returns>
    public static AchievementPoints Absent(string problem)
        => new() { Loaded = false, Problem = problem };

    /// <summary>
    /// **The shipping copy's text**, for seeding the owner's file the first time.
    /// </summary>
    /// <remarks>
    /// **IT IS NOT WHAT THE SCORES ARE COMPUTED FROM.** The scores come from the file on
    /// disk, always, so that editing that file changes the screen - which is the whole
    /// point of it being a file. This is the text Hamlet writes there once, when there is
    /// nothing there to read.
    /// </remarks>
    /// <returns>The shipped JSON, or "" where the resource is missing.</returns>
    public static string Shipped()
    {
        using var stream = typeof(AchievementPoints).Assembly
            .GetManifestResourceStream(
                "Hamlet.RadioEngine.Data.Achievements.achievement-points.json");

        if (stream is null)
        {
            return "";
        }

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// **Read the owner's file, seeding it from the shipping copy the first time.**
    /// </summary>
    /// <param name="path">Where the owner's file is.</param>
    /// <returns>The points, loaded or absent.</returns>
    /// <remarks>
    /// <para>**IT SEEDS ONLY WHERE THERE IS NOTHING THERE, AND NEVER OVER AN EXISTING
    /// FILE** - not even a broken one. A malformed file is his edit and Hamlet replacing it
    /// would throw away work he was in the middle of; the page says it could not be read
    /// and the file is left exactly as he left it.</para>
    /// <para>**AND A FAILED SEED IS A REPORTED STATE.** Where the folder cannot be written
    /// the scores are absent with the reason, rather than the screen quietly falling back
    /// to the shipped numbers and telling him his edits did nothing.</para>
    /// </remarks>
    public static AchievementPoints ReadOrSeed(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                var shipped = Shipped();

                if (shipped.Length == 0)
                {
                    return Absent(
                        "The points file could not be read: there is no file at " + path
                        + " and no shipped copy to seed it from.");
                }

                var folder = Path.GetDirectoryName(path);

                if (folder is { Length: > 0 })
                {
                    Directory.CreateDirectory(folder);
                }

                File.WriteAllText(path, shipped);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Absent(
                "The points file could not be written: " + path + " - " + ex.Message);
        }

        return Read(path);
    }

    /// <summary>
    /// **Read the file, or come back absent with a line saying why.** Never throws.
    /// </summary>
    /// <param name="path">Where the owner's file is.</param>
    /// <returns>The points, loaded or absent.</returns>
    public static AchievementPoints Read(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return Absent(
                    "The points file could not be read: there is no file at " + path + ".");
            }

            return Parse(File.ReadAllText(path), path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Absent(
                "The points file could not be read: " + path + " - " + ex.Message);
        }
    }

    /// <summary>
    /// **Parse the file's text.** The real read and the tested read are the same code (§5).
    /// </summary>
    /// <param name="json">The file's contents.</param>
    /// <param name="where">The path, for the line a fault produces.</param>
    /// <returns>The points, loaded or absent.</returns>
    public static AchievementPoints Parse(string json, string where = "the points file")
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return Absent(
                    "The points file could not be read: " + where
                    + " is not a JSON object.");
            }

            var points = new AchievementPoints
            {
                Loaded = true,
                About = Text(root, "_about"),
                Hash = Fingerprint(json),
                Ranks = Numbers(root, "ranks"),
            };

            var kinds = 0;

            foreach (var kind in AchievementKinds.All)
            {
                if (!root.TryGetProperty(kind, out var section)
                    || section.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                kinds++;
                points.ReadSection(kind, section);
            }

            if (root.TryGetProperty("levels", out var levels)
                && levels.ValueKind == JsonValueKind.Object)
            {
                foreach (var kind in AchievementKinds.All)
                {
                    if (levels.TryGetProperty(kind, out var one))
                    {
                        points._levels[kind] = Numbers(one);
                    }
                }
            }

            if (kinds == 0)
            {
                return Absent(
                    "The points file could not be read: " + where
                    + " names none of the eight kinds.");
            }

            points.Kinds = kinds;

            return points;
        }
        catch (JsonException ex)
        {
            return Absent(
                "The points file could not be read: " + where + " is not valid JSON - "
                + ex.Message);
        }
    }

    /// <summary>What one of a kind is worth, or null where the file does not say.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <returns>The points, or null.</returns>
    public int? Per(string kind)
        => _per.TryGetValue(kind, out var value) ? value : null;

    /// <summary>What one continent is worth, or null.</summary>
    /// <param name="code">The continent code - `EU`, `AF`.</param>
    /// <returns>The points, or null.</returns>
    public int? PerContinent(string? code)
        => code is not null && _continentPer.TryGetValue(code, out var value) ? value : null;

    /// <summary>What the *all of them* bonus is worth for a kind, or null.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <returns>The bonus, or null.</returns>
    public int? AllBonus(string kind)
        => _all.TryGetValue(kind, out var value) ? value : null;

    /// <summary>What a named special is worth, or null.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <param name="name">The band, mode or state the file names.</param>
    /// <returns>The points, or null.</returns>
    public int? Special(string kind, string? name)
    {
        if (name is null)
        {
            return null;
        }

        var table = kind switch
        {
            AchievementKinds.Bands => _bandSpecial,
            AchievementKinds.Modes => _modeSpecial,
            AchievementKinds.States => _stateSpecial,
            AchievementKinds.HallOfFame => _hallOfFame,
            _ => null,
        };

        return table is not null && table.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>The milestones for a kind, ascending. Empty where the file names none.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <returns>The pairs of count and points.</returns>
    public IReadOnlyList<(long At, int Points)> Milestones(string kind)
        => _milestones.TryGetValue(kind, out var rows)
            ? rows
            : Array.Empty<(long, int)>();

    /// <summary>The level thresholds for a kind, ascending. Empty where none.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <returns>The thresholds.</returns>
    public IReadOnlyList<long> Levels(string kind)
        => _levels.TryGetValue(kind, out var rows) ? rows : Array.Empty<long>();

    /// <summary>Every Hall of Fame first the file names, with its points.</summary>
    public IReadOnlyDictionary<string, int> HallOfFame => _hallOfFame;

    private void ReadSection(string kind, JsonElement section)
    {
        if (section.TryGetProperty("per", out var per))
        {
            if (per.ValueKind == JsonValueKind.Number && per.TryGetInt32(out var flat))
            {
                _per[kind] = flat;
            }
            else if (per.ValueKind == JsonValueKind.Object)
            {
                // **THE CONTINENTS' `per` IS A TABLE AND EVERY OTHER KIND'S IS A
                // NUMBER**, because Antarctica is not worth what North America is. The
                // shape says which, so the file needs no type field.
                foreach (var entry in per.EnumerateObject())
                {
                    if (entry.Value.TryGetInt32(out var value))
                    {
                        _continentPer[entry.Name] = value;
                    }
                }
            }
        }

        if (section.TryGetProperty("all", out var all) && all.TryGetInt32(out var bonus))
        {
            _all[kind] = bonus;
        }

        if (section.TryGetProperty("special", out var special)
            && special.ValueKind == JsonValueKind.Object)
        {
            var table = kind switch
            {
                AchievementKinds.Bands => _bandSpecial,
                AchievementKinds.Modes => _modeSpecial,
                AchievementKinds.States => _stateSpecial,
                _ => null,
            };

            if (table is not null)
            {
                foreach (var entry in special.EnumerateObject())
                {
                    if (entry.Value.TryGetInt32(out var value))
                    {
                        table[entry.Name] = value;
                    }
                }
            }
        }

        // **THE HALL OF FAME IS A FLAT TABLE OF NAMED FIRSTS**, because each one is its
        // own card and there is nothing to count.
        if (kind == AchievementKinds.HallOfFame)
        {
            foreach (var entry in section.EnumerateObject())
            {
                if (!entry.Name.StartsWith('_') && entry.Value.TryGetInt32(out var value))
                {
                    _hallOfFame[entry.Name] = value;
                }
            }
        }

        // **MILESTONES AND TIERS ARE ONE SHAPE UNDER TWO NAMES.** Countries and grids
        // call them milestones and Total Miles calls them tiers; both are *at this count,
        // this many points*, so both are read here and nothing downstream cares which
        // word the file used.
        foreach (var name in new[] { "milestones", "tiers" })
        {
            if (!section.TryGetProperty(name, out var rows)
                || rows.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var read = new List<(long At, int Points)>();

            foreach (var entry in rows.EnumerateObject())
            {
                if (long.TryParse(
                        entry.Name, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out var at)
                    && entry.Value.TryGetInt32(out var value))
                {
                    read.Add((at, value));
                }
            }

            _milestones[kind] = read.OrderBy(r => r.At).ToList();
        }
    }

    private static string Text(JsonElement root, string name)
        => root.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static IReadOnlyList<long> Numbers(JsonElement root, string name)
        => root.TryGetProperty(name, out var value)
            ? Numbers(value)
            : Array.Empty<long>();

    private static IReadOnlyList<long> Numbers(JsonElement array)
    {
        if (array.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<long>();
        }

        var read = new List<long>();

        foreach (var one in array.EnumerateArray())
        {
            if (one.TryGetInt64(out var value))
            {
                read.Add(value);
            }
        }

        read.Sort();

        return read;
    }

    /// <summary>Eight hex characters of SHA-256 over the bytes read.</summary>
    private static string Fingerprint(string json)
        => Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(json)))[..8].ToLowerInvariant();
}
