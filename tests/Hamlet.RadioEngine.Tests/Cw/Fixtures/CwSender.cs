using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>One TX-* sender profile as CW_SPEC.md section 10 states it.</summary>
/// <param name="Id">The profile's name, <c>TX-ITU</c> and so on.</param>
/// <param name="Name">Section 10's name column.</param>
/// <param name="Tier">Section 10's tier, ruled 2026-09-25: must, should or later.</param>
/// <param name="Definition">Section 10's definition, quoted.</param>
/// <param name="Refused">Why the profile is not produced, or null where it is.</param>
public sealed record CwSenderProfile(string Id, string Name, string Tier, string Definition, string? Refused)
{
    /// <summary>Whether the generator produces it.</summary>
    public bool Produced => Refused is null;
}

/// <summary>One parameter of a sender's timing, with where its number came from.</summary>
/// <param name="Name">What it is: <c>dah</c>, <c>element gap</c> and so on.</param>
/// <param name="Units">Its length in units of the dit.</param>
/// <param name="Source">Section 10, a named capture, or TX-ITU nominal; and whether it was drawn.</param>
public sealed record CwSenderParameter(string Name, double Units, string Source);

/// <summary>A sender's timing for one case: every length the keying uses, and the draws behind them.</summary>
/// <param name="ProfileId">The TX-* profile.</param>
/// <param name="CharacterWpm">The speed the characters are sent at, PARIS; the dit is 1200 over it.</param>
/// <param name="Seed">The case's seed, which also seeds every draw.</param>
/// <param name="Dah">The dah in units of the dit.</param>
/// <param name="ElementGap">The gap inside a character, in units.</param>
/// <param name="CharacterGap">The gap between characters, in units.</param>
/// <param name="WordGap">The gap between words, in units.</param>
/// <param name="Parameters">Each of the four with its number's source, for the sidecar and senders.md.</param>
public sealed record CwSenderTiming(
    string ProfileId, double CharacterWpm, int Seed,
    double Dah, double ElementGap, double CharacterGap, double WordGap,
    IReadOnlyList<CwSenderParameter> Parameters)
{
    /// <summary>The dit, in milliseconds.</summary>
    public double DitMilliseconds => 1200.0 / CharacterWpm;

    /// <summary>
    /// The overall speed of a text keyed at this timing: the character speed times
    /// the text's length at 1:3:1:3:7 over its length at these gaps, first mark to
    /// last. Equal to the character speed for TX-ITU and below it wherever the gaps
    /// are stretched (CW_SPEC.md 6.4).
    /// </summary>
    /// <param name="text">The text keyed.</param>
    /// <returns>Words a minute.</returns>
    public double OverallWpm(string text)
    {
        var (marks, dahs, element, character, word) = CwSender.Counts(text);
        var nominal = marks + (2 * dahs) + element + (3 * character) + (7 * word);
        var keyed = marks + ((Dah - 1) * dahs) + (ElementGap * element) + (CharacterGap * character) + (WordGap * word);

        return CharacterWpm * nominal / keyed;
    }
}

/// <summary>
/// The TX-* sender profiles, keyed by the fixture generator and carried over the
/// CH-* channels (work instruction 468; PHASE_PLAN.md 7.2; CW_SPEC.md section 10;
/// HM-REQ-050).
/// </summary>
/// <remarks>
/// <para>**A PROFILE CHANGES WHEN THE KEY GOES DOWN AND UP, NEVER WHAT IS SENT.** The
/// key is the text itself, exact by construction (R61, V-13): every character the
/// text holds is keyed and nothing else is.</para>
/// <para>**EVERY NUMBER IS SECTION 10'S, A CAPTURE SECTION 10 NAMES, OR TX-ITU
/// NOMINAL, AND NONE IS INVENTED** (V-04, CLAUDE.md 12.5). A profile whose defining
/// feature section 10 gives no number for is refused by name, as unit 461 refused
/// CH-MDV. Each reading below is the arbiter's under R85 and overrulable; the
/// recipe is <c>docs/phase-requirements/senders.md</c>.</para>
/// <para>**AUDIO SYNTHESIS IN THE TEST FIXTURES, NOT A KEYER** (CLAUDE.md 0.2).
/// Nothing here reaches the radio, and nothing transmit-side is called.</para>
/// </remarks>
public static class CwSender
{
    /// <summary>
    /// TX-TIGHT's lengths: <see cref="CwFixtureRecipe"/>'s defaults, the generator
    /// fitted to the 013347 capture under HM-DEC-101 - dit 105, dah 283, element gap
    /// 65, character gap 130, word gap 280 milliseconds.
    /// </summary>
    private static readonly CwFixtureRecipe Tight = new("013347", "E");

    /// <summary>HM-DEC-115's traffic net (CW_SPEC.md 6.4): 57 ms dit, word gap 500 ms.</summary>
    private const double TrafficNetWordGapUnits = 500.0 / 57.0;

    /// <summary>Section 10's rows in its order, each produced or refused.</summary>
    public static IReadOnlyList<CwSenderProfile> Profiles { get; } = new[]
    {
        new CwSenderProfile("TX-ITU", "Nominal keyer", "must",
            "Exactly 1:3:1:3:7. The KD0UN capture (3.06 / 2.89 / 6.92 units) is a real example.", null),
        new CwSenderProfile("TX-KEYER-W", "Weighted keyer", "must",
            "Electronic keyer with weight and ratio adjusted: ratio 2.5–3.5, gaps ±30 % [verify against WinKeyer documentation].", null),
        new CwSenderProfile("TX-FARNS", "Farnsworth", "must",
            "Character speed above overall speed; character gap 3–7 units at character speed, word gap longer. HM-DEC-115's traffic net is the vendored example.", null),
        new CwSenderProfile("TX-TIGHT", "Tight fist", "must",
            "Element gaps shorter than the dit; character gaps compressed. The 013347 capture (HM-DEC-101).", null),
        new CwSenderProfile("TX-BUG", "Semi-automatic key", "should",
            "Mechanical dits, hand dahs; ratio 3.5–5, dits short and fast, gaps variable.", null),
        new CwSenderProfile("TX-STRAIGHT", "Straight key", "should",
            "Everything hand-timed; ratio and gaps drift; speed wanders.",
            "section 10 gives no number for how far the ratio and the gaps drift or how far the speed wanders, "
            + "and its defining feature is that drift; no range is invented (V-04). The source to vendor is CW_SPEC.md 12 item 6, Gold 1959."),
        new CwSenderProfile("TX-SLOPPY", "Poor sender", "later — honesty rule only",
            "Morse Runner's \"LID\": inconsistent ratios, missing gaps, errors and corrections.",
            "section 10 gives no number for the inconsistency or for how often gaps go missing, and errors and corrections "
            + "change the characters sent, which a sender here never does (the key is the text). The source to vendor is "
            + "CW_SPEC.md 12 item 7, Morse Runner's impairment definitions."),
    };

    /// <summary>The speeds every case is keyed at: <see cref="SyntheticCq.Speeds"/>, 12, 18 and 25 WPM.</summary>
    public static IReadOnlyList<double> Speeds => SyntheticCq.Speeds;

    /// <summary>The tone of every case, the channel cases' and the decoder's starting pitch.</summary>
    public const double PitchHz = 600;

    /// <summary>A profile by name.</summary>
    /// <param name="id">The name.</param>
    /// <returns>The profile.</returns>
    /// <exception cref="NotSupportedException">A refused profile, with the reason.</exception>
    /// <exception cref="ArgumentException">A name section 10 does not have.</exception>
    public static CwSenderProfile Profile(string id)
    {
        var profile = Profiles.SingleOrDefault(p => p.Id == id)
            ?? throw new ArgumentException($"{id} is not a CW_SPEC.md section 10 profile.", nameof(id));

        return profile.Refused is { } why
            ? throw new NotSupportedException($"{id} is refused: {why}")
            : profile;
    }

    /// <summary>
    /// The fixed seed of a case, set before any decode: 468000, plus a hundred times
    /// the profile's row in section 10 (TX-ITU 0 ... TX-BUG 4), plus the speed's index.
    /// </summary>
    /// <param name="id">The profile.</param>
    /// <param name="speedIndex">0, 1 or 2, into <see cref="Speeds"/>.</param>
    /// <returns>The seed.</returns>
    public static int Seed(string id, int speedIndex)
        => 468000 + (100 * Profiles.Select(p => p.Id).ToList().IndexOf(Profile(id).Id)) + speedIndex;

    /// <summary>A profile's timing for one case, with anything drawn drawn from the seed.</summary>
    /// <param name="id">The profile.</param>
    /// <param name="wordsPerMinute">The character speed.</param>
    /// <param name="seed">The case's seed.</param>
    /// <returns>The timing.</returns>
    public static CwSenderTiming Timing(string id, double wordsPerMinute, int seed)
    {
        var profile = Profile(id);
        var draw = Draws(seed);
        const string Nominal = "TX-ITU nominal (section 10 gives no number)";

        CwSenderTiming Of(CwSenderParameter dah, CwSenderParameter element, CwSenderParameter character, CwSenderParameter word)
            => new(profile.Id, wordsPerMinute, seed, dah.Units, element.Units, character.Units, word.Units,
                new[] { dah, element, character, word });

        switch (profile.Id)
        {
            case "TX-ITU":
                return Of(new("dah", 3, "section 10, exactly 1:3:1:3:7"), new("element gap", 1, "section 10, exactly 1:3:1:3:7"),
                    new("character gap", 3, "section 10, exactly 1:3:1:3:7"), new("word gap", 7, "section 10, exactly 1:3:1:3:7"));

            case "TX-KEYER-W":
            {
                // One weight and one ratio per keyer setting: each drawn once per case,
                // uniform over section 10's range, in this order.
                var ratio = Uniform(draw(), 2.5, 3.5);
                var e = Uniform(draw(), 0.7, 1.3);
                var c = Uniform(draw(), 0.7, 1.3);
                var w = Uniform(draw(), 0.7, 1.3);
                const string Gaps = "section 10, gaps ±30 % [verify against WinKeyer documentation], drawn uniform 0.7-1.3 x nominal";

                return Of(new("dah", ratio, "section 10, ratio 2.5-3.5 [verify against WinKeyer documentation], drawn uniform"),
                    new("element gap", 1 * e, Gaps), new("character gap", 3 * c, Gaps), new("word gap", 7 * w, Gaps));
            }

            case "TX-FARNS":
                return Of(new("dah", 3, Nominal + ", at character speed"),
                    new("element gap", 1, Nominal + ", at character speed"),
                    new("character gap", Uniform(draw(), 3, 7), "section 10, 3-7 units at character speed, drawn uniform"),
                    new("word gap", TrafficNetWordGapUnits, "section 10's vendored example, HM-DEC-115's traffic net: 500 ms over a 57 ms dit (CW_SPEC.md 6.4); longer than any character gap drawn"));

            case "TX-TIGHT":
            {
                var dit = Tight.DitMilliseconds;
                const string Capture = "the 013347 capture as HM-DEC-101 fitted the generator to it (CwFixtureRecipe's defaults, dit 105 ms)";

                return Of(new("dah", Tight.DahMilliseconds / dit, Capture + ", 283 ms"),
                    new("element gap", Tight.ElementGapMilliseconds / dit, Capture + ", 65 ms"),
                    new("character gap", Tight.CharacterGapMilliseconds / dit, Capture + ", 130 ms"),
                    new("word gap", Tight.WordGapMilliseconds / dit, Capture + ", 280 ms"));
            }

            case "TX-BUG":
                return Of(new("dah", Uniform(draw(), 3.5, 5), "section 10, ratio 3.5-5, drawn uniform"),
                    new("element gap", 1, Nominal + " - \"gaps variable\""),
                    new("character gap", 3, Nominal + " - \"gaps variable\""),
                    new("word gap", 7, Nominal + " - \"gaps variable\""));

            default:
                throw new NotSupportedException($"{id} has no timing.");
        }
    }

    /// <summary>The generator's recipe for a timing: the five lengths from the dit, the pitch held.</summary>
    /// <param name="timing">The timing.</param>
    /// <param name="text">What is keyed.</param>
    /// <returns>The recipe; its seed and level are the channel's business, not the recipe's.</returns>
    public static CwFixtureRecipe Recipe(CwSenderTiming timing, string text)
    {
        var dit = timing.DitMilliseconds;

        return new CwFixtureRecipe(
            Name: string.Create(CultureInfo.InvariantCulture, $"{timing.ProfileId.ToLowerInvariant()}-{timing.CharacterWpm:0}wpm-{timing.Seed}"),
            Text: CwChannel.Key(text),
            DitMilliseconds: dit,
            DahMilliseconds: timing.Dah * dit,
            ElementGapMilliseconds: timing.ElementGap * dit,
            CharacterGapMilliseconds: timing.CharacterGap * dit,
            WordGapMilliseconds: timing.WordGap * dit,
            SignalToNoiseDb: 0,
            ToneHz: PitchHz,
            DriftHz: 0,
            Seed: timing.Seed);
    }

    /// <summary>Generate one case on a channel.</summary>
    /// <param name="id">The TX-* profile.</param>
    /// <param name="channelId">The CH-* profile; CH-AWGN for HM-REQ-050.</param>
    /// <param name="snrDb">Signal-to-noise in the 2500 Hz reference (CW_SPEC.md 8.1).</param>
    /// <param name="text">What is keyed; the key.</param>
    /// <param name="wordsPerMinute">The character speed.</param>
    /// <param name="seed">Seeds the draws, the band and the paths.</param>
    /// <returns>The audio, the key and the sidecar.</returns>
    public static CwChannelCase Generate(string id, string channelId, double snrDb, string text, double wordsPerMinute, int seed)
    {
        var (parts, _) = Render(id, channelId, snrDb, text, wordsPerMinute, seed);

        return new CwChannelCase(parts.Audio, parts.Key, parts.Sidecar);
    }

    /// <summary>One case taken apart, with the timing it was keyed to.</summary>
    internal static (CwChannelParts Parts, CwSenderTiming Timing) Render(
        string id, string channelId, double snrDb, string text, double wordsPerMinute, int seed)
    {
        var timing = Timing(id, wordsPerMinute, seed);

        return (Keyed(timing, Recipe(timing, text), channelId, snrDb), timing);
    }

    /// <summary>
    /// A case keyed by a recipe with a timing's sidecar: <see cref="Render"/>'s path,
    /// open so a proof can key a recipe the timing does not describe and watch itself fail.
    /// </summary>
    internal static CwChannelParts Keyed(CwSenderTiming timing, CwFixtureRecipe recipe, string channelId, double snrDb)
    {
        var i = CultureInfo.InvariantCulture;
        var profile = Profile(timing.ProfileId);
        var line = string.Create(i,
            $"{profile.Id} ({profile.Name}, {profile.Tier}), {timing.CharacterWpm:0.#} wpm at character speed, dit {timing.DitMilliseconds:0.###} ms, "
            + $"{timing.Dah:0.###}:{timing.ElementGap:0.###}:{timing.CharacterGap:0.###}:{timing.WordGap:0.###}, 5 ms raised-cosine edges");

        var parts = CwChannel.RenderKeyed(channelId, snrDb, recipe, PitchHz, timing.Seed, line);
        var text = new StringBuilder(parts.Sidecar);

        void Line(FormattableString s) => text.AppendLine(s.ToString(i));

        text.AppendLine();
        Line($"txProfile     {profile.Id} - {profile.Name} - tier {profile.Tier} (CW_SPEC.md 10)");
        Line($"txDefinition  {profile.Definition}");
        Line($"txSpeed       {timing.CharacterWpm:0.#} wpm at character speed; overall {timing.OverallWpm(recipe.Text):0.##} wpm over this text");
        Line($"txSeed        {timing.Seed} (draws: xorshift32 from seed ^ 0x7E5D, uniform, in the order listed)");
        Line($"txSnr         {snrDb:0.0} dB in the 2500 Hz reference on {channelId}");

        foreach (var p in timing.Parameters)
        {
            Line($"txParameter   {p.Name,-13} {p.Units:0.####} units, {p.Units * timing.DitMilliseconds:0.###} ms - {p.Source}");
        }

        Line($"txKey         exact by construction (R61, V-13): the text keyed, and nothing else");

        return parts with { Sidecar = text.ToString() };
    }

    /// <summary>What a text keys: marks, dahs among them, element gaps, character gaps and word gaps.</summary>
    /// <param name="text">The text, prosign carets joining letters with element gaps.</param>
    /// <returns>The counts.</returns>
    public static (int Marks, int Dahs, int Element, int Character, int Word) Counts(string text)
    {
        int marks = 0, dahs = 0, element = 0, character = 0;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var w in words)
        {
            var first = true;
            var joining = false;

            foreach (var ch in w)
            {
                if (ch == '^')
                {
                    joining = true;
                    continue;
                }

                var pattern = MorseCode.Spell(ch) ?? "";

                if (!first)
                {
                    if (joining)
                    {
                        element++;
                    }
                    else
                    {
                        character++;
                    }
                }

                marks += pattern.Length;
                dahs += pattern.Count(e => e != '.');
                element += Math.Max(0, pattern.Length - 1);
                first = false;
            }
        }

        return (marks, dahs, element, character, Math.Max(0, words.Length - 1));
    }

    private static double Uniform(double u, double low, double high) => low + ((high - low) * u);

    // xorshift32, the generator's own, seeded apart from the band's and the paths'.
    private static Func<double> Draws(int seed)
    {
        var state = unchecked((uint)seed ^ 0x7E5Du);

        if (state == 0)
        {
            state = 1;
        }

        return () =>
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return ((state & 0xFFFFFF) + 0.5) / 16777216.0;
        };
    }
}
