namespace Hamlet.RadioEngine.Explore;

/// <summary>Waterfall fingerprint archetypes the app can draw and, in
/// phase 3, recognize.</summary>
public enum SignatureKind
{
    /// <summary>On/off keying: dots and dashes (CW).</summary>
    Dots,

    /// <summary>Synchronized rectangular bursts (FT8/JS8).</summary>
    Blocks,

    /// <summary>A wide voice smear (SSB).</summary>
    Smear,

    /// <summary>Two parallel rails (RTTY).</summary>
    Rails,

    /// <summary>One hair-thin ribbon (PSK31).</summary>
    Ribbon,
}

/// <summary>One field-guide entry: a mode's three identities — how it
/// sounds, how it looks on the waterfall, and why it's worth your evening.</summary>
/// <param name="Name">Mode name.</param>
/// <param name="Tagline">Three-word character.</param>
/// <param name="Sound">What the ear hears, plainly.</param>
/// <param name="Signature">Waterfall fingerprint archetype.</param>
/// <param name="Difficulty">Honest one-word entry bar.</param>
/// <param name="Why">One sentence of why it's cool. No jargon.</param>
/// <param name="LivesAt40mHz">Where it lives on 40 m, for "take me there";
/// null when it has no 40 m home.</param>
public sealed record ModeInfo(
    string Name, string Tagline, string Sound, SignatureKind Signature,
    string Difficulty, string Why, long? LivesAt40mHz);

/// <summary>
/// The field guide to the modes. Editorial content, the demystifying kind
/// (HM-DEC-016): plain language over precision, honesty over salesmanship.
/// </summary>
public static class ModeGuide
{
    /// <summary>All field-guide entries, newcomer-friendliest ordering.</summary>
    public static IReadOnlyList<ModeInfo> Modes { get; } = new[]
    {
        new ModeInfo("CW", "Morse code", "Musical beeps", SignatureKind.Dots,
            "Learnable",
            "The original digital mode — your brain is the decoder. A century "
            + "old and still the best watts-to-distance deal in radio.",
            7_030_000),
        new ModeInfo("FT8", "Robot handshakes", "15-second warbles",
            SignatureKind.Blocks, "Easy",
            "Works the world on a wet-noodle antenna. Watching your callsign "
            + "come back from Japan never gets old.",
            7_074_000),
        new ModeInfo("SSB", "Voice", "Duck talk until tuned",
            SignatureKind.Smear, "Easy",
            "Actual conversation with actual humans, no infrastructure "
            + "between you and them.",
            7_188_000),
        new ModeInfo("RTTY", "1930s teletype", "Two-tone chatter",
            SignatureKind.Rails, "Medium",
            "Mechanical-era technology still on the air. Sounds like a robot "
            + "bird; looks like train tracks on the waterfall.",
            7_062_000),
        new ModeInfo("PSK31", "Keyboard whisper", "Soft warble",
            SignatureKind.Ribbon, "Medium",
            "Typed chats in a signal 31 Hz wide — a thousand of them could "
            + "fit where one voice sits.",
            7_065_000),
        new ModeInfo("JS8", "FT8 that chats", "Same warble, longer",
            SignatureKind.Blocks, "Medium",
            "FT8's machinery, but free text — the campfire version of the "
            + "robot handshake.",
            7_078_000),
    };
}
