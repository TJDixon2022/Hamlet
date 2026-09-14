using System.Text.Json;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>One row of the Olivia calling table.</summary>
/// <param name="Band">The band as the file spells it, e.g. "20m".</param>
/// <param name="CenterHz">The center of the signal, in hertz.</param>
/// <param name="DialHz">The USB dial the file gives for it, or null where it gives none.</param>
/// <param name="Variant">The variant the row is for, as the file writes it.</param>
public sealed record OliviaCallingRow(string Band, long CenterHz, long? DialHz, string Variant);

/// <summary>
/// **Where Olivia is called, read from <c>data/bands/olivia-calling.json</c>.**
/// </summary>
/// <remarks>
/// <para>**A COMMUNITY CONVENTION AND LABELED AS ONE** (HM-DEC-054, `PHASE_PLAN.md` R29).
/// The file cites the Olivia community's own page and says in its first line that it is not
/// a band plan. <see cref="About"/> carries that sentence so a surface showing a row can
/// show what kind of fact it is.</para>
/// <para>**THERE IS NO FREQUENCY LITERAL IN THIS FILE**, the rule
/// <see cref="Bands.DigitalCallingFrequencies"/> keeps and for the same reason (§0,
/// §0.2.1).</para>
/// <para>**THE DIAL IS THE FILE'S TOO.** A row gives the center of the signal, and a radio
/// in USB tuned to the center puts the signal at zero hertz of audio, where nothing can hear
/// it. The file states that its dial is the USB dial for a fixed audio center and gives one
/// row with both numbers; <see cref="AudioCenterHz"/> is the difference on that row, and
/// every band's dial is its center less it. **This reading is work instruction 358's author
/// decision and is overrulable**: the instruction says the tab tunes to the calling center,
/// and this is the dial that puts the calling center in the middle of the passband.</para>
/// </remarks>
public sealed class OliviaCallingTable
{
    /// <summary>The file's name, as a sentence about it names it.</summary>
    public const string FilePath = "data/bands/olivia-calling.json";

    /// <summary>The variant a call starts on, which is the row the tab tunes to.</summary>
    /// <remarks>
    /// **R29 AND THE FILE BOTH SAY IT**: *pressing Olivia tunes to the 8/250 spot for the
    /// band*, and the file's convention reads *call on 8/250 at the calling center*. It is a
    /// variant name matched against the file's own rows, not a frequency.
    /// </remarks>
    public const string CallingVariant = "8/250";

    private OliviaCallingTable(
        string about,
        string source,
        string convention,
        IReadOnlyList<OliviaCallingRow> rows,
        long? audioCenterHz)
    {
        About = about;
        Source = source;
        Convention = convention;
        Rows = rows;
        AudioCenterHz = audioCenterHz;
    }

    /// <summary>What the file says it is, in its own words.</summary>
    public string About { get; }

    /// <summary>Where the rows were read from, as the file states it.</summary>
    public string Source { get; }

    /// <summary>The operating convention the file states, or "" where it states none.</summary>
    public string Convention { get; }

    /// <summary>Every row, in the file's order.</summary>
    public IReadOnlyList<OliviaCallingRow> Rows { get; }

    /// <summary>
    /// The audio center the file's dials assume, or null where no row gives a dial.
    /// </summary>
    public long? AudioCenterHz { get; }

    /// <summary>The row a call starts on for a band, or null where the file has none.</summary>
    /// <param name="bandName">The band as Hamlet spells it, e.g. "20 m".</param>
    /// <returns>The 8/250 row for the band, or null.</returns>
    /// <remarks>
    /// **"20 m" AND "20m" ARE THE SAME BAND.** Hamlet's band names carry a space and the
    /// file's do not; the match ignores spacing and case and nothing else.
    /// </remarks>
    public OliviaCallingRow? CallingRowFor(string? bandName)
    {
        if (string.IsNullOrWhiteSpace(bandName))
        {
            return null;
        }

        var wanted = Squeeze(bandName);

        return Rows.FirstOrDefault(
            row => Squeeze(row.Band) == wanted
                   && string.Equals(row.Variant.Trim(), CallingVariant, StringComparison.Ordinal));
    }

    /// <summary>The USB dial that puts a row's center in the middle of the passband.</summary>
    /// <param name="row">A row of this table.</param>
    /// <returns>The dial in hertz, or null where the file gives no way to say.</returns>
    public long? DialHzFor(OliviaCallingRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return row.DialHz ?? (AudioCenterHz is { } audio ? row.CenterHz - audio : null);
    }

    /// <summary>Parse the table from the file's text.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The table.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    /// <remarks>
    /// **STRICT, BECAUSE A ROW SKIPPED IS A BAND SILENTLY WITHOUT A SPOT.** A row with a
    /// center that is not a whole number fails the whole file rather than dropping out.
    /// </remarks>
    public static OliviaCallingTable Parse(string json)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                "it is not readable JSON (line " + (error.LineNumber + 1) + ")", error);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("it does not hold a table");
            }

            var source = Text(root, "source");

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException("it does not say where its rows came from");
            }

            var about = Text(root, "_about");

            if (string.IsNullOrWhiteSpace(about))
            {
                throw new InvalidDataException("it does not say what kind of table it is");
            }

            if (!root.TryGetProperty("rows", out var rowsElement)
                || rowsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("it carries no rows");
            }

            var rows = new List<OliviaCallingRow>();

            foreach (var element in rowsElement.EnumerateArray())
            {
                var number = rows.Count + 1;

                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("row " + number + " is not a row");
                }

                var band = Text(element, "band");

                if (string.IsNullOrWhiteSpace(band))
                {
                    throw new InvalidDataException("row " + number + " names no band");
                }

                var variant = Text(element, "variant");

                if (string.IsNullOrWhiteSpace(variant))
                {
                    throw new InvalidDataException("the " + band + " row names no variant");
                }

                var center = Hertz(element, "center_hz")
                    ?? throw new InvalidDataException(
                        "the " + band + " row's center is missing or not a whole number of hertz");

                long? dial = null;

                if (element.TryGetProperty("dial_hz", out _))
                {
                    dial = Hertz(element, "dial_hz")
                        ?? throw new InvalidDataException(
                            "the " + band + " row's dial is not a whole number of hertz");
                }

                rows.Add(new OliviaCallingRow(band, center, dial, variant));
            }

            if (rows.Count == 0)
            {
                throw new InvalidDataException("it carries no rows");
            }

            // **EVERY ROW THAT GIVES BOTH NUMBERS MUST AGREE ON THE AUDIO CENTER**, or the
            // file contradicts itself and no dial derived from it can be trusted.
            var offsets = rows
                .Where(row => row.DialHz is not null)
                .Select(row => row.CenterHz - row.DialHz!.Value)
                .Distinct()
                .ToList();

            if (offsets.Count > 1)
            {
                throw new InvalidDataException("its rows disagree about the audio center their dials assume");
            }

            return new OliviaCallingTable(
                about,
                source,
                Text(root, "convention"),
                rows,
                offsets.Count == 1 ? offsets[0] : null);
        }
    }

    private static string Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static long? Hertz(JsonElement element, string name)
        => element.TryGetProperty(name, out var value)
           && value.ValueKind == JsonValueKind.Number
           && value.TryGetInt64(out var hz)
           && hz > 0
            ? hz
            : null;

    private static string Squeeze(string band)
        => string.Concat(band.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
}
