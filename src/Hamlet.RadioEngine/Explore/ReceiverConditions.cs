using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One thing the receive side has to be, for a mode to work.</summary>
/// <param name="Control">
/// What the operator would call it: "noise blanker", "scope span".
/// </param>
/// <param name="Field">
/// The rig field it lives in, or null where Hamlet has no cited command for it.
/// </param>
/// <param name="Wanted">The value wanted, or null where none can be written.</param>
/// <param name="WantedText">That value in words.</param>
/// <param name="Says">
/// The reason as one clause, for the sentence the operator reads after a
/// tune-in. It follows a "because", so it is a phrase and not a sentence.
/// </param>
/// <param name="Because">
/// The same reason at length, for wherever there is room to explain properly.
/// </param>
/// <param name="Confirmed">
/// Whether this may be written to a radio. False where the value is stated so
/// it can be spoken and has not been established (§12.4).
/// </param>
/// <param name="Confirm">Who settles it, where it is unconfirmed.</param>
/// <param name="Condition">
/// What the value depends on, or empty where it is a constant. `overflow` means
/// the front end's own reading decides; `band` means the frequency does.
/// </param>
/// <remarks>
/// <para>**A CONDITION CARRIES ITS REASON BECAUSE THE OPERATOR IS GOING TO BE
/// TOLD IT** (work instruction 042, tasks 2 and 4). Hamlet changing settings
/// silently is the same confusion relocated rather than removed; what makes this
/// not a rig-control panel is that he states an intent and hears what followed
/// from it (HM-DEC-050).</para>
/// <para>**AND A CONDITION WITH NO FIELD IS NOT A DEFECT.** The scope span is a
/// real requirement of this mode and there is no cited byte for it in §4, so it
/// is stated, spoken, and not written. Saying nothing about it would leave the
/// operator with a three-kilohertz block seven pixels wide and no idea why.</para>
/// </remarks>
public sealed record ReceiverCondition(
    string Control,
    RigField? Field,
    int? Wanted,
    string WantedText,
    string Says,
    string Because,
    bool Confirmed = true,
    string Confirm = "",
    string Condition = "")
{
    /// <summary>
    /// What this row depends on, or empty where its value is a constant.
    /// </summary>
    /// <remarks>
    /// <para>**TWO OF CW'S SETTINGS ARE RULES RATHER THAN VALUES** (Tim's ruling
    /// of 2026-08-29). The attenuator is off unless the front end reads
    /// overloading, and the preamp is off at 40 m and below. Writing either as a
    /// constant is wrong half the time, and on 2026-08-29 it was wrong in both
    /// directions on one evening: 20 dB on while a station faded to nothing, and
    /// off while the front end read overloading at S9 plus 10. **Hamlet read the
    /// answer on both evenings and said nothing.**</para>
    /// <para>**A CONDITION THAT CANNOT BE RESOLVED IS NOT WRITTEN.** Where the
    /// reading it depends on is unknown, the row is spoken and no byte goes out,
    /// because a rule applied without its input is a constant wearing a rule's
    /// clothes (§0.0).</para>
    /// </remarks>
    public string Condition { get; init; } = Condition;

    /// <summary>True where this row's value depends on a live reading.</summary>
    public bool IsConditional => Condition.Length > 0;

    /// <summary>Whether Hamlet may set this itself.</summary>
    /// <remarks>
    /// Both halves are required and they fail for different reasons. No field
    /// means no cited command (§4, HM-DEC-084). Unconfirmed means the value
    /// itself has not been established, and writing it would be a guess wearing
    /// a byte (§0.0, §12.4).
    /// </remarks>
    public bool CanBeWritten => Field is not null && Wanted is not null && Confirmed;
}

/// <summary>
/// What each mode needs of the receiver in front of it.
/// </summary>
/// <remarks>
/// <para>**THIS EXISTS BECAUSE THE OPERATOR WAS TOLD TO PRESS BUTTONS ON HIS
/// RADIO THREE TIMES IN ONE AFTERNOON** (work instruction 042). His words:
/// *"You control what needs to be set to make each mode work. I do not want to
/// touch the radio."*</para>
/// <para>**KEYED BY MODE, NOT BY BLOCK.** A noise blanker chops the tones of an
/// FT8 signal on 80 m for the same reason it chops them on 20 m, so the fact
/// belongs to the mode; written onto each block it would be twelve copies of one
/// sentence, and copies drift (§0). What the block supplies is what is about the
/// block, which is its width.</para>
/// <para>**A MODE THIS FILE CANNOT SPEAK FOR STATES NOTHING**, and that produces
/// no claim and no write rather than a gap to be filled in later by whoever is
/// passing.</para>
/// </remarks>
public static class ReceiverConditions
{
    /// <summary>Resource name of the embedded conditions file.</summary>
    public const string ResourceName =
        "Hamlet.RadioEngine.Data.Bands.mode-receiver-conditions.json";

    private static readonly Lazy<ConditionData> Shared = new(LoadEmbedded);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Every mode label the file states conditions for.</summary>
    public static IEnumerable<string> Modes => Shared.Value.ByMode.Keys;

    /// <summary>What the file deliberately does not cover.</summary>
    public static IReadOnlyList<NeighborhoodUnknown> Unknowns => Shared.Value.Unknowns;

    /// <summary>
    /// What the receiver has to be, to work the mode that lives in this block.
    /// </summary>
    /// <param name="hood">The block, or null where the dial is off the map.</param>
    /// <returns>
    /// The conditions, or an empty list where the block states none.
    /// </returns>
    /// <remarks>
    /// <para>**THE SPAN IS DERIVED HERE AND NOT STORED** (§0). It is the block's
    /// own width, the same number the passband comes from: a scope showing two
    /// hundred kilohertz draws a three-kilohertz block in about seven pixels, so
    /// a crowded band and an empty one look identical.</para>
    /// <para>It has no field, because §4 carries no CI-V command for the span.
    /// So it is spoken and not written, which is the honest half of a promise
    /// Hamlet cannot yet keep whole.</para>
    /// </remarks>
    public static IReadOnlyList<ReceiverCondition> ForBlock(Neighborhood? hood)
    {
        if (hood is null)
        {
            return Array.Empty<ReceiverCondition>();
        }

        var key = hood.ShortName.Trim().ToUpperInvariant();

        if (!Shared.Value.ByMode.TryGetValue(key, out var stated))
        {
            return Array.Empty<ReceiverCondition>();
        }

        var all = new List<ReceiverCondition>(stated);

        if (hood.PassbandHz is { } wideHz)
        {
            all.Add(new ReceiverCondition(
                "scope span",
                Field: null,
                Wanted: null,
                Describe(wideHz),
                "a scope showing a couple of hundred kilohertz draws the whole "
                + "block about seven pixels wide",
                "The block is only a few kilohertz across, so a scope set to show "
                + "a couple of hundred kilohertz draws the whole of it about "
                + "seven pixels wide. A band full of stations and a band with "
                + "nobody on it then look exactly the same, which is the one "
                + "thing a waterfall is there to tell you apart."));
        }

        return all;
    }

    private static string Describe(long hertz)
        => hertz >= 1000
            ? $"{hertz / 1000.0:0.#} kHz across"
            : $"{hertz} Hz across";

    /// <summary>Parse conditions from JSON.</summary>
    /// <param name="json">The file.</param>
    /// <returns>The parsed data.</returns>
    /// <exception cref="InvalidDataException">The file is unusable.</exception>
    public static ConditionData Parse(string json)
    {
        var dto = JsonSerializer.Deserialize<ConditionFile>(json, JsonOptions)
                  ?? throw new InvalidDataException("receiver-conditions file is empty");

        if (dto.Modes is null || dto.Modes.Length == 0)
        {
            throw new InvalidDataException("receiver-conditions file states no modes");
        }

        var byMode = new Dictionary<string, IReadOnlyList<ReceiverCondition>>(
            StringComparer.Ordinal);

        // Two passes, because a mode may say it wants what another one wants and
        // that is a fact worth stating once rather than a row to copy.
        foreach (var mode in dto.Modes.Where(m => m.SameAs is null))
        {
            byMode[Key(mode.Mode)] = (mode.Conditions ?? Array.Empty<ConditionDto>())
                .Select(Convert)
                .ToList();
        }

        foreach (var mode in dto.Modes.Where(m => m.SameAs is not null))
        {
            if (!byMode.TryGetValue(Key(mode.SameAs), out var same))
            {
                throw new InvalidDataException(
                    $"{mode.Mode} says it is the same as {mode.SameAs}, "
                    + "which this file does not state");
            }

            byMode[Key(mode.Mode)] = same;
        }

        return new ConditionData(
            byMode,
            (dto.Unknowns ?? Array.Empty<UnknownDto>())
                .Select(u => new NeighborhoodUnknown(u.Topic ?? "", u.Reason ?? ""))
                .ToList());
    }

    private static string Key(string? mode)
        => (mode ?? "").Trim().ToUpperInvariant();

    private static ReceiverCondition Convert(ConditionDto dto)
        => new(
            dto.Control ?? "",
            ParseField(dto.Field),
            dto.Wanted,
            dto.WantedText ?? "",
            dto.Says ?? "",
            dto.Because ?? "",
            dto.Confirmed,
            dto.Confirm ?? "",
            dto.Condition ?? "");

    private static RigField? ParseField(string? name)
        => Enum.TryParse<RigField>(name, ignoreCase: true, out var field)
            ? field
            : null;

    private static ConditionData LoadEmbedded()
    {
        var assembly = typeof(ReceiverConditions).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded conditions data '{ResourceName}' is missing; "
                + "the build did not include "
                + "data/bands/mode-receiver-conditions.json");

        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    /// <summary>The parsed file.</summary>
    /// <param name="ByMode">Conditions by mode label.</param>
    /// <param name="Unknowns">What the file will not say.</param>
    public sealed record ConditionData(
        IReadOnlyDictionary<string, IReadOnlyList<ReceiverCondition>> ByMode,
        IReadOnlyList<NeighborhoodUnknown> Unknowns);

    private sealed class ConditionFile
    {
        public ModeDto[]? Modes { get; set; }

        public UnknownDto[]? Unknowns { get; set; }
    }

    private sealed class ModeDto
    {
        public string? Mode { get; set; }

        public string? SameAs { get; set; }

        public ConditionDto[]? Conditions { get; set; }
    }

    private sealed class ConditionDto
    {
        public string? Control { get; set; }

        public string? Field { get; set; }

        public int? Wanted { get; set; }

        public string? WantedText { get; set; }

        public string? Says { get; set; }

        public string? Because { get; set; }

        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        public string? Confirm { get; set; }

        /// <summary>What the row's value depends on, where it is a rule.</summary>
        public string? Condition { get; set; }
    }

    private sealed class UnknownDto
    {
        public string? Topic { get; set; }

        public string? Reason { get; set; }
    }
}
