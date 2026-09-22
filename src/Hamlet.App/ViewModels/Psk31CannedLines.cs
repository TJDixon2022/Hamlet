using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;

namespace Hamlet.App.ViewModels;

/// <summary>One row of <c>data/psk31/canned.json</c>: a label, and either a macro or a line.</summary>
/// <param name="Label">What the menu item reads.</param>
/// <param name="Macro">Which of Hamlet's own three this row is, or <see cref="Psk31CannedMacro.None"/>.</param>
/// <param name="Text">The words to send, or "" where this row names a macro instead.</param>
/// <remarks>
/// **A ROW IS ONE OR THE OTHER, NEVER BOTH** (work instruction 378 section 6 ruling 1 item 5).
/// Three of Tim's seven lines *are* Hamlet's existing `Answer`, `Report` and `Confirm` macros,
/// and those rows name their macro rather than carrying a second spelling of it: writing an
/// answer beside the one Hamlet already sends is two spellings of one act, which was the whole
/// subject of unit 377.
/// </remarks>
public sealed record Psk31CannedLine(string Label, Psk31CannedMacro Macro, string Text)
{
    /// <summary>True where this row carries words rather than naming a macro.</summary>
    public bool IsText => Macro == Psk31CannedMacro.None;
}

/// <summary>Which of Hamlet's own macros a canned row names, or none.</summary>
/// <remarks>
/// **IT IS NOT <c>Psk31Macro</c> AND DELIBERATELY SO** (§0.1). That enum is the engine's and
/// `Psk31Offer` answers with it; this one is what a word in a file the operator edits may say,
/// and the file may say something that is not a macro at all. Mapping happens in one place, in
/// the reader, so a typo in his file is a refusal here rather than a cast somewhere else.
/// </remarks>
public enum Psk31CannedMacro
{
    /// <summary>The row carries text.</summary>
    None = 0,

    /// <summary><c>Psk31Macros.Answer</c>.</summary>
    Answer,

    /// <summary><c>Psk31Macros.Report</c>.</summary>
    Report,

    /// <summary><c>Psk31Macros.Confirm</c>.</summary>
    Confirm,
}

/// <summary>
/// **The canned lines as they were read: the rows, where they came from, and what was wrong.**
/// </summary>
/// <param name="Lines">The rows in the file's own order, or empty where the file could not be read.</param>
/// <param name="Path">Which file was read - his copy where he has made one, the shipped one otherwise.</param>
/// <param name="Problem">What is wrong with the file, or null where it read whole.</param>
/// <remarks>
/// **A MALFORMED FILE IS REPORTED AND NEVER GUESSED** (§0.0, work instruction 378 section 6
/// ruling 1 item 4). There is no partial read, no skipped row and no silent fallback to the
/// shipped seven when his own copy is broken: <see cref="Lines"/> is empty and
/// <see cref="Problem"/> says what is wrong, and the menu shows that sentence as a note with
/// nothing to click.
/// </remarks>
public sealed record Psk31CannedSet(
    IReadOnlyList<Psk31CannedLine> Lines, string Path, string? Problem)
{
    /// <summary>True where the file read whole and carries at least one row.</summary>
    public bool IsUsable => Problem is null && Lines.Count > 0;
}

/// <summary>What one press on a canned text line carries to the send.</summary>
/// <param name="Station">Whose row it was picked on.</param>
/// <param name="Label">What the menu item read - for the panel's own sentence, never the record.</param>
/// <param name="Text">The words from the file, before Hamlet frames them.</param>
/// <remarks>
/// **THE LABEL NEVER REACHES THE RECORD** (HM-DEC-018 §2.1). It is here because a refusal says
/// which line was refused, and a sentence on the screen is not a line in the file.
/// </remarks>
public sealed record Psk31CannedPress(string Station, string Label, string Text);

/// <summary>What one press on a canned MACRO line carries: the command that already sends it.</summary>
/// <param name="Send">The command that sends this macro today, unchanged.</param>
/// <param name="Parameter">What that command is handed, unchanged - the row, or the card.</param>
/// <remarks>
/// <para>**IT MARKS THE PRESS AND IT DOES NOT RE-ROUTE THE SEND** (work instruction 383 section 6
/// ruling 2 item 3). Criterion 7.2 asks that every canned send write `macro: canned`, and until
/// unit 383 the three macro rows wrote their own token because they are sent by the commands that
/// send them - <c>AnswerPsk31Command</c> and the card's own <c>CardActionCommand</c>. Writing a
/// second answer beside the one Hamlet already sends is two spellings of one act, so what changed
/// is that the press is KNOWN to have come off the list by the time the composer chooses the
/// token. **The send itself is still made by the command in <see cref="Send"/>, handed the
/// parameter in <see cref="Parameter"/>, with §R1's certainty gate exactly where it was.**</para>
/// <para>**AND IT CARRIES NOTHING PERSONAL** (HM-DEC-018 §2.1): no label, no text, no callsign.
/// It is two references to things the menu already built.</para>
/// </remarks>
public sealed record Psk31CannedMacroPress(
    System.Windows.Input.ICommand Send, object? Parameter);

/// <summary>One line on the canned menu: what it reads, and what a click does - or nothing.</summary>
/// <param name="Label">What the item reads.</param>
/// <param name="Command">What a click runs, or null where this is a note or a disabled line.</param>
/// <param name="Parameter">What the command is handed.</param>
/// <param name="Disabled">True where this is a line drawn grey rather than a sentence.</param>
/// <remarks>
/// <para>**A NOTE CARRIES NO COMMAND AND CANNOT BE CLICKED** (the 2026-09-06 ruling, §0.5.1).
/// What Hamlet cannot do right now is ABSENT, with a note beside it saying why, and that is
/// still the rule for every line on this menu but one.</para>
/// <para>**THE ONE EXCEPTION IS R46(b)** (Tim, 2026-09-22; work instruction 387 section 6 ruling
/// 2(a) item 1): *lines that need his callsign are disabled and say why*. The later ruling wins,
/// and it wins **narrowly** - only for the lines that cannot be sent because Hamlet does not know
/// the operator's own callsign. Grey is this project's reserved signal for a control that
/// genuinely cannot be used (§0.5.1, HM-DEC-087) and that is exactly this case: the line is real,
/// it is his, and it will work the moment he fills Settings in. **Every other line keeps the
/// 2026-09-06 rule and the note.**</para>
/// </remarks>
public sealed record Psk31CannedEntry(
    string Label,
    System.Windows.Input.ICommand? Command,
    object? Parameter,
    bool Disabled = false)
{
    /// <summary>True where this entry is a sentence rather than something to click.</summary>
    public bool IsNote => Command is null && !Disabled;

    /// <summary>True where a click would do something.</summary>
    public bool IsLive => Command is not null;
}

/// <summary>
/// **Reads the seven lines a right-click offers, from a file the operator can edit.**
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR'S COPY WINS WHERE HE HAS MADE ONE** (work instruction 378 section 6
/// ruling 1 item 2). <c>%AppData%\Hamlet\canned.json</c> is looked for first - the one folder
/// `AppSettings.DataFolder` already names as the folder Hamlet writes to, beside
/// `settings.json` and `scan-segments.json` - and the file shipped beside the application is
/// the fallback. **Hamlet never writes either one**: it does not create his copy, does not copy
/// the shipped file there and does not repair anything. That is *without a session* satisfied
/// with no new write and no new risk - he copies one file and edits it.</para>
/// <para>**IT IS <c>Content</c> AND NOT AN <c>EmbeddedResource</c>** (ruling 1 item 1). A file
/// the operator is meant to edit must exist as a file beside the application; `bylines.json`'s
/// embedding is the pattern for a file nobody edits, not for this one.</para>
/// <para>**IT READS AND NEVER SENDS** (§0.2). Nothing here composes, arms or keys, and it does
/// not know what a radio is.</para>
/// </remarks>
public static class Psk31CannedLines
{
    /// <summary>Where the shipped file sits, relative to the application.</summary>
    public const string ShippedRelativePath = "data/psk31/canned.json";

    /// <summary>The name the operator's own copy has to carry.</summary>
    public const string OperatorFileName = "canned.json";

    /// <summary>The shipped file, beside the application.</summary>
    public static string ShippedPath
        => System.IO.Path.Combine(
            AppContext.BaseDirectory, "data", "psk31", OperatorFileName);

    /// <summary>The operator's own copy, in the one folder Hamlet writes to.</summary>
    public static string OperatorPath
        => System.IO.Path.Combine(SettingsStore.DataFolder, OperatorFileName);

    /// <summary>Read the lines, his copy first.</summary>
    /// <returns>The rows, the path they came from, and what was wrong with the file.</returns>
    public static Psk31CannedSet Read()
    {
        var his = OperatorPath;

        return File.Exists(his) ? ReadFrom(his) : ReadFrom(ShippedPath);
    }

    /// <summary>Read one named file, for a test and for <see cref="Read"/>.</summary>
    /// <param name="path">Which file.</param>
    /// <returns>The rows, the path, and what was wrong.</returns>
    /// <remarks>
    /// **EVERY REFUSAL NAMES THE FILE AND THE FAULT** (§0.0). *Could not be read* on its own is
    /// a sentence the operator cannot act on; which file, and which row, is what he can.
    /// </remarks>
    public static Psk31CannedSet ReadFrom(string path)
    {
        var none = Array.Empty<Psk31CannedLine>();

        if (!File.Exists(path))
        {
            return new(none, path, "Hamlet could not find " + path + ".");
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (Exception error) when (error is JsonException or IOException or UnauthorizedAccessException)
        {
            return new(none, path, "Hamlet could not read " + path + ": " + error.Message);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("lines", out var rows)
                || rows.ValueKind != JsonValueKind.Array)
            {
                return new(none, path, path + " has no \"lines\" array in it.");
            }

            var read = new List<Psk31CannedLine>();
            var at = 0;

            foreach (var row in rows.EnumerateArray())
            {
                at++;

                if (row.ValueKind != JsonValueKind.Object)
                {
                    return new(none, path, "line " + at + " of " + path + " is not a row.");
                }

                var label = Text(row, "label");
                var macro = Text(row, "macro");
                var text = Text(row, "text");

                if (label.Length == 0)
                {
                    return new(none, path, "line " + at + " of " + path + " has no label.");
                }

                if (macro.Length > 0 && text.Length > 0)
                {
                    return new(none, path,
                        "line " + at + " of " + path + ", \"" + label
                        + "\", carries both a macro and a line of text, and a row is one or the other.");
                }

                if (macro.Length == 0 && text.Length == 0)
                {
                    return new(none, path,
                        "line " + at + " of " + path + ", \"" + label
                        + "\", carries neither a macro nor a line of text.");
                }

                if (macro.Length == 0)
                {
                    read.Add(new(label, Psk31CannedMacro.None, text));

                    continue;
                }

                var named = macro.Trim().ToUpperInvariant() switch
                {
                    "ANSWER" => Psk31CannedMacro.Answer,
                    "REPORT" => Psk31CannedMacro.Report,
                    "CONFIRM" => Psk31CannedMacro.Confirm,
                    _ => Psk31CannedMacro.None,
                };

                if (named == Psk31CannedMacro.None)
                {
                    return new(none, path,
                        "line " + at + " of " + path + ", \"" + label + "\", names the macro \""
                        + macro + "\", and Hamlet has three: answer, report and confirm.");
                }

                read.Add(new(label, named, ""));
            }

            return read.Count == 0
                ? new(none, path, path + " carries no lines at all.")
                : new(read, path, null);
        }
    }

    /// <summary>One string field, trimmed, or "".</summary>
    private static string Text(JsonElement row, string name)
        => row.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()?.Trim() ?? ""
            : "";
}
