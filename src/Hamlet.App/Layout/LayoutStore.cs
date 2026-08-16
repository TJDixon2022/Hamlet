using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.App.Settings;

namespace Hamlet.App.Layout;

/// <summary>What the operator has arranged and saved (HM-DEC-086).</summary>
/// <param name="Current">
/// What is on the canvas right now, so closing and opening the app puts
/// everything back where it was.
/// </param>
/// <param name="Saved">Arrangements the operator named and kept.</param>
/// <param name="StartedFrom">
/// Which preset the current arrangement began as, or "". A label rather than a
/// link: rearranging does not change the preset, and this only says where it came
/// from.
/// </param>
public sealed record LayoutBook(
    CanvasLayout? Current = null,
    IReadOnlyList<CanvasLayout>? Saved = null,
    string StartedFrom = "")
{
    /// <summary>The arrangements the operator saved, never null.</summary>
    /// <remarks>
    /// **NOT WRITTEN TO THE FILE.** It is a convenience over
    /// <see cref="Saved"/>, and a serializer left to itself will happily write a
    /// second copy of every saved layout under this name and then ignore it on
    /// the way back in (HM-DEC-089).
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyList<CanvasLayout> Kept => Saved ?? Array.Empty<CanvasLayout>();
}

/// <summary>
/// Reads and writes the canvas, beside the operator profile (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>Its own file rather than a corner of `settings.json`, because a layout
/// is a document the operator authored and the settings file is a bag of
/// switches. Somebody who wants to keep an arrangement, mail it to a friend or
/// put it back after an experiment can do all three with one file, and a
/// corrupted layout cannot take the callsign down with it.</para>
/// <para>**NEVER THROWS** (§8). A layout that cannot be read means starting from
/// the furnished preset, which is a good place to be. A layout that cannot be
/// written loses an arrangement and nothing else.</para>
/// </remarks>
public static class LayoutStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// %AppData%\Hamlet\layouts.json, unless something has redirected it.
    /// </summary>
    /// <remarks>
    /// Settable so a test can point it at a temporary file (HM-DEC-089). The
    /// headless window test builds the real view model, which loads and saves
    /// the canvas, and a test that reads and writes the operator's own
    /// arrangement is a test that can both flake and destroy.
    /// </remarks>
    public static string Path { get; set; } =
        System.IO.Path.Combine(SettingsStore.DataFolder, "layouts.json");

    /// <summary>What was saved, or an empty book.</summary>
    /// <returns>The book. Never null and never throws.</returns>
    public static LayoutBook Load() => LoadFrom(Path);

    /// <summary>Keep it.</summary>
    /// <param name="book">What to write.</param>
    public static void Save(LayoutBook book) => SaveTo(book, Path);

    /// <summary>Read from an explicit path, so the real load is the tested one.</summary>
    /// <param name="path">Where.</param>
    /// <returns>The book, or an empty one.</returns>
    public static LayoutBook LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return new LayoutBook();
            }

            return JsonSerializer.Deserialize<LayoutBook>(
                File.ReadAllText(path), Options) ?? new LayoutBook();
        }
        catch (Exception)
        {
            return new LayoutBook();
        }
    }

    /// <summary>Write to an explicit path. Never throws.</summary>
    /// <param name="book">What to write.</param>
    /// <param name="path">Where.</param>
    public static void SaveTo(LayoutBook book, string path)
    {
        try
        {
            var folder = System.IO.Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(book, Options));
        }
        catch (Exception)
        {
            // An arrangement is worth keeping and not worth crashing over.
        }
    }
}
