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
/// <param name="Family">Which mode family it belongs to, so the field guide
/// and the band map color it the same way (HM-DEC-032).</param>
public sealed record ModeInfo(
    string Name, string Tagline, string Sound, SignatureKind Signature,
    string Difficulty, string Why, long? LivesAt40mHz, ModeFamily Family);

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
            "The original digital mode, and your brain is the decoder. A century "
            + "old and still the best watts-to-distance deal in radio, which is "
            + "why five watts and a wire gets so much further here than anywhere "
            + "else on the dial.",
            7_030_000, ModeFamily.Cw),
        new ModeInfo("FT8", "Robot handshakes", "15-second warbles",
            SignatureKind.Blocks, "Easy",
            "Works the world on a compromise antenna and a few watts. Watching "
            + "your callsign come back from Japan never gets old, and nobody on "
            + "the other end knows or cares what your station cost.",
            7_074_000, ModeFamily.Digital),
        new ModeInfo("SSB", "Voice", "Duck talk until tuned",
            SignatureKind.Smear, "Easy",
            "Actual conversation with actual humans, no infrastructure "
            + "between you and them.",
            7_188_000, ModeFamily.Phone),
        new ModeInfo("RTTY", "1930s teletype", "Two-tone chatter",
            SignatureKind.Rails, "Medium",
            "Mechanical-era technology still on the air. Sounds like a robot "
            + "bird, and looks like train tracks on the waterfall. Your radio "
            + "decodes this one by itself, on its own screen, with no computer "
            + "involved at all. It will send that decoded text down the USB "
            + "cable too, and there is a catch worth knowing before you go "
            + "looking for it. The radio has one setting for what comes out of "
            + "that port, so choosing the decoded text means the control "
            + "messages stop, and Hamlet would lose the radio entirely for as "
            + "long as it ran. That makes it a thing to do at the radio's own "
            + "screen when you want it, rather than something Hamlet can offer "
            + "you here.",
            7_062_000, ModeFamily.Digital),
        new ModeInfo("PSK31", "Keyboard whisper", "Soft warble",
            SignatureKind.Ribbon, "Medium",
            "Typed chats in a signal 31 Hz wide. A thousand of them could fit "
            + "in the space one voice takes up.",
            7_065_000, ModeFamily.Digital),
        new ModeInfo("JS8", "FT8 that chats", "Same warble, longer",
            SignatureKind.Blocks, "Medium",
            "FT8's machinery with free text on top. It's the campfire version "
            + "of the robot handshake.",
            7_078_000, ModeFamily.Digital),
    };

    /// <summary>
    /// Which family a mode name belongs to (§0.6, HM-DEC-032).
    /// </summary>
    /// <param name="mode">The mode as a source reported it, e.g. "CW", "SSB".</param>
    /// <returns>Its family, or open when nothing in the guide claims it.</returns>
    /// <remarks>
    /// THE FAMILY IS DECLARED ON THE DATA, NEVER ON A CONTROL (§0.6). The guide
    /// already names a family for every mode it describes, so a surface that
    /// wants to color by family reads it from here rather than carrying its own
    /// list. A second copy of the language is a second language.
    /// </remarks>
    public static ModeFamily FamilyFor(string? mode)
    {
        var name = (mode ?? "").Trim();

        if (name.Length == 0)
        {
            return ModeFamily.Open;
        }

        foreach (var described in Modes)
        {
            if (string.Equals(described.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return described.Family;
            }
        }

        // Modes the guide does not describe yet, grouped by what they are. A
        // spot's mode comes from a live feed and the feeds report more names
        // than the guide covers, so an unknown one is open rather than wrong.
        return name.ToUpperInvariant() switch
        {
            "CW" or "CW-R" or "CWR" => ModeFamily.Cw,
            "SSB" or "USB" or "LSB" or "AM" or "FM" or "PHONE" => ModeFamily.Phone,
            "FT8" or "FT4" or "JS8" or "PSK31" or "PSK" or "RTTY" or "DATA"
                or "DIGITAL" or "MFSK" or "OLIVIA" or "JT65" or "JT9" or "WSPR"
                => ModeFamily.Digital,
            _ => ModeFamily.Open,
        };
    }
}
