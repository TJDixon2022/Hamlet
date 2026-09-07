using System.Globalization;
using System.Text;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>One line of a scene corpus: a decode, as the decoder returned it.</summary>
/// <param name="Slot">Which numbered slot of the scene it came out of.</param>
/// <param name="FrequencyHz">Where the decoder found it, in whole hertz.</param>
/// <param name="Message">The text, exactly as the decoder returned it.</param>
public sealed record Ft8SceneLine(int Slot, int FrequencyHz, string Message);

/// <summary>
/// The band scene on disk, and the arithmetic for reading it back.
/// </summary>
/// <remarks>
/// <para>**THE CORPUS IS WHAT CAME OUT OF THE DECODER AND NOT WHAT WENT IN.**
/// The scene is composed by <c>Ft8Composer</c>, summed slot by slot, and read
/// back once through <c>Ft8DeepSlotDecoder</c>; these lines are that decoder's
/// answer. A message that did not decode is not in this file, and the generator
/// counts and names it rather than writing it in by hand.</para>
/// <para>**IT IS SYNTHESIZED AND IT IS NOT A CAPTURE.** It lives in
/// <c>tests/fixtures/ft8/scenes/</c> and never in <c>tests/fixtures/ft8/captured/</c>,
/// it carries no <c>provenance: wsjtx</c>, and it may never be scored against
/// the decoder's accuracy. Its own README says so at length.</para>
/// <para>**THIS READER IS TEST-ONLY AND LIVES IN THE TEST PROJECT.** Nothing in
/// <c>src/</c> knows the file exists; the ledger under test is fed messages and
/// slot boundaries and has no idea where they came from.</para>
/// </remarks>
public sealed class Ft8SceneCorpus
{
    /// <summary>Where the committed scene lives, relative to the repository root.</summary>
    public const string RelativePath =
        "tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt";

    /// <summary>The first line of the file, which names the format.</summary>
    public const string FormatMarker = "# hamlet-ft8-scene v1";

    private Ft8SceneCorpus(
        string operatorCallsign,
        DateTime slotZeroUtc,
        IReadOnlyList<Ft8SceneLine> lines)
    {
        OperatorCallsign = operatorCallsign;
        SlotZeroUtc = slotZeroUtc;
        Lines = lines;
    }

    /// <summary>Whose station the scene is written from.</summary>
    public string OperatorCallsign { get; }

    /// <summary>The UTC boundary slot 0 opened on.</summary>
    public DateTime SlotZeroUtc { get; }

    /// <summary>Every decode, in slot order and then in the decoder's own order.</summary>
    public IReadOnlyList<Ft8SceneLine> Lines { get; }

    /// <summary>How many numbered slots the scene runs for.</summary>
    public int SlotCount => Lines.Count == 0 ? 0 : Lines.Max(line => line.Slot) + 1;

    /// <summary>The UTC boundary a numbered slot opened on.</summary>
    /// <param name="slot">The slot number, counting from zero.</param>
    /// <returns>The boundary, in true UTC.</returns>
    /// <remarks>
    /// **THE SLOT PERIOD IS <c>Ft8Slots.SlotSeconds</c> AND IS NOT WRITTEN AGAIN
    /// HERE.** A second copy of fifteen is a second thing to be wrong.
    /// </remarks>
    public DateTime SlotUtc(int slot)
        => SlotZeroUtc.AddSeconds(slot * RadioEngine.Audio.Ft8Slots.SlotSeconds);

    /// <summary>Reads the committed scene.</summary>
    /// <param name="path">The file.</param>
    /// <returns>The scene.</returns>
    /// <exception cref="InvalidOperationException">The file is not a scene corpus.</exception>
    public static Ft8SceneCorpus Read(string path)
    {
        var text = File.ReadAllText(path);
        return Parse(text);
    }

    /// <summary>Reads a scene out of the text of one.</summary>
    /// <param name="text">The whole file.</param>
    /// <returns>The scene.</returns>
    /// <exception cref="InvalidOperationException">The text is not a scene corpus.</exception>
    public static Ft8SceneCorpus Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        if (lines.Length == 0 || !string.Equals(lines[0].Trim(), FormatMarker, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "this is not a Hamlet FT8 scene corpus: its first line is not \""
                + FormatMarker + "\"");
        }

        var callsign = "";
        var slotZero = default(DateTime);
        var found = new List<Ft8SceneLine>();

        foreach (var raw in lines)
        {
            var line = raw.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("operator:", StringComparison.Ordinal))
            {
                callsign = line["operator:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("slot0utc:", StringComparison.Ordinal))
            {
                slotZero = DateTime.Parse(
                    line["slot0utc:".Length..].Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                continue;
            }

            var parts = line.Split('|', 3);

            if (parts.Length != 3)
            {
                throw new InvalidOperationException(
                    "a scene line must be \"slot | hz | message\" and this is: " + line);
            }

            found.Add(new Ft8SceneLine(
                int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
                parts[2].Trim()));
        }

        if (callsign.Length == 0)
        {
            throw new InvalidOperationException("the scene names no operator");
        }

        return new Ft8SceneCorpus(callsign, slotZero, found);
    }

    /// <summary>Writes a scene in the format <see cref="Parse"/> reads.</summary>
    /// <param name="operatorCallsign">Whose station it is written from.</param>
    /// <param name="slotZeroUtc">The boundary slot 0 opened on.</param>
    /// <param name="lines">The decodes, in slot order.</param>
    /// <param name="regeneratedBy">
    /// Which test regenerates this file. **It defaults to the band scene's own
    /// generator**, so every caller written before there was a second scene keeps
    /// its bytes exactly - a scene file naming the wrong generator sends the next
    /// reader to a test that does not write it.
    /// </param>
    /// <returns>The whole file, with a trailing newline.</returns>
    public static string Write(
        string operatorCallsign,
        DateTime slotZeroUtc,
        IReadOnlyList<Ft8SceneLine> lines,
        string regeneratedBy = "TheBandSceneIsWhatHamletsDecoderReadTests")
    {
        ArgumentNullException.ThrowIfNull(lines);

        var text = new StringBuilder();

        text.Append(FormatMarker).Append('\n');
        text.Append("# Synthesized by Hamlet's own encoder and read back once through its own\n");
        text.Append("# decoder. NOT a WSJT-X capture and never to be scored against the\n");
        text.Append("# decoder's accuracy - see README.md beside this file.\n");
        text.Append("# Regenerated by ").Append(regeneratedBy).Append(".\n");
        text.Append("operator: ").Append(operatorCallsign).Append('\n');
        text.Append("slot0utc: ")
            .Append(slotZeroUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            .Append('\n');
        text.Append("# slot | hz | message\n");

        foreach (var line in lines)
        {
            text.Append(line.Slot.ToString(CultureInfo.InvariantCulture).PadLeft(4))
                .Append(" | ")
                .Append(line.FrequencyHz.ToString(CultureInfo.InvariantCulture).PadLeft(5))
                .Append(" | ")
                .Append(line.Message)
                .Append('\n');
        }

        return text.ToString();
    }

    /// <summary>The repository root, found by walking up to the solution file.</summary>
    /// <returns>The directory holding <c>Hamlet.sln</c>.</returns>
    /// <exception cref="InvalidOperationException">There is no solution above the binary.</exception>
    public static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    /// <summary>The committed scene's full path on this machine.</summary>
    public static string PathInTree
        => Path.Combine(Root(), RelativePath.Replace('/', Path.DirectorySeparatorChar));
}
