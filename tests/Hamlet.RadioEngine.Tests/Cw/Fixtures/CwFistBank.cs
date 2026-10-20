using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>One fist of the bank: a hand's dah over its dit, its speed, its signal in the passband, and whether it fades.</summary>
/// <param name="Ratio">The dah over the dit.</param>
/// <param name="Wpm">The speed, PARIS; the dit is 1200 ms over it.</param>
/// <param name="Db">The signal over the noise in the receiver's 520 Hz passband, as the generator measures it.</param>
/// <param name="Fades">Whether it fades by the working fixtures' 12 dB at 0.7 Hz.</param>
public sealed record CwFist(double Ratio, double Wpm, double Db, bool Fades)
{
    /// <summary>Its name: <c>fist-r{ratio}-{wpm}wpm-{db}db-{fade|flat}</c>.</summary>
    public string Name => string.Create(CultureInfo.InvariantCulture, $"fist-r{Ratio:0.0}-{Wpm:0}wpm-{Db:0}db-{(Fades ? "fade" : "flat")}");

    /// <summary>Its seed: a hash of its name, so it never changes while the name does not.</summary>
    public int Seed
    {
        get
        {
            // FNV-1a over the name's characters, folded to a positive int.
            var hash = 2166136261u;

            foreach (var c in Name)
            {
                hash = (hash ^ c) * 16777619u;
            }

            return (int)(hash & 0x7FFFFFFF);
        }
    }

    /// <summary>The recipe line the survey prints: everything the fist is made from.</summary>
    public string Recipe => string.Create(
        CultureInfo.InvariantCulture,
        $"dah {Ratio:0.0} dits, {Wpm:0} WPM (dit {1200 / Wpm:0} ms), {Db:0} dB in the passband, {(Fades ? $"{CwFixtureCatalogue.QsbDepthDb:0} dB of QSB at {CwFixtureCatalogue.QsbHz:0.0} Hz" : "no fade")}, {CwFistBank.ToneHz:0} Hz drifting {CwFistBank.DriftHz:0} Hz, seed {Seed}");
}

/// <summary>
/// **THE FIST BANK** (work instruction 580, HM-DEC-284): a yardstick with a truth for every hand on the band, generated at test
/// time from each fist's recipe and seed, so nothing binary is added to the tree and the recipe is the truth.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-20:** *"we've had no improvement for many units … step back and reanalyze."* The first fist generated
/// with a dah of 2.0 dits read 0 of 60, and had read nothing for the whole project. From here on every rule ships against a truth.</para>
/// <para>**EVERY FIST IS A HAND:** its dits and dahs each spread ±15%, its gaps inside letters one dit ±20%, its letter gaps 2 to
/// 3.5 dits and its word gaps 4 to 6, each drawn from the fist's seed, all sending one ragchew exchange, rendered over the band
/// by <see cref="CwFixtureGenerator.GenerateFromEdges"/> at 615 Hz with 3 Hz of drift.</para>
/// </remarks>
public static class CwFistBank
{
    /// <summary>What every fist sends: 60 letters, a call, a report, a name and a QTH.</summary>
    public const string Text = "CQ CQ DE N0CALL N0CALL K GM KEN NICE TO MEET U RPRT UR 559 QTH IS CHICOPEE MA K";

    /// <summary>The note.</summary>
    public const double ToneHz = 615;

    /// <summary>How far the note wanders.</summary>
    public const double DriftHz = 3;

    /// <summary>The dahs over the dits of the grid.</summary>
    public static readonly double[] Ratios = [1.6, 2.0, 2.5, 3.0];

    /// <summary>The speeds of the grid.</summary>
    public static readonly double[] Speeds = [10, 15, 20, 25];

    /// <summary>The signals of the grid, in the passband.</summary>
    public static readonly double[] Signals = [5, 10, 20];

    /// <summary>The ninety-six fists: every ratio, speed, signal, flat and fading.</summary>
    public static IReadOnlyList<CwFist> All { get; } =
        (from ratio in Ratios from wpm in Speeds from db in Signals from fades in new[] { false, true } select new CwFist(ratio, wpm, db, fades)).ToList();

    /// <summary>A fist's key-down and key-up times, alternating from the first key-down, drawn from its seed.</summary>
    public static double[] Edges(CwFist fist)
    {
        var random = new Random(fist.Seed);
        var dit = 1.2 / fist.Wpm;
        var edges = new List<double>();
        var at = CwFixtureGenerator.MessageStartSeconds;

        double Spread(double ideal, double s) => ideal * (1 + (s * ((2 * random.NextDouble()) - 1)));
        double Between(double low, double high) => dit * (low + ((high - low) * random.NextDouble()));

        var words = Text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var code = Morse[words[w][c]];

                for (var e = 0; e < code.Length; e++)
                {
                    edges.Add(at);
                    at += Spread(code[e] == '-' ? fist.Ratio * dit : dit, 0.15);
                    edges.Add(at);

                    if (e + 1 < code.Length)
                    {
                        at += Spread(dit, 0.2);
                    }
                }

                if (c + 1 < words[w].Length)
                {
                    at += Between(2, 3.5);
                }
                else if (w + 1 < words.Length)
                {
                    at += Between(4, 6);
                }
            }
        }

        return edges.ToArray();
    }

    /// <summary>A fist's audio: its keying over the band, at its signal and fade.</summary>
    public static MonoAudio Audio(CwFist fist)
    {
        var recipe = new CwFixtureRecipe(
            fist.Name, Text, DitMilliseconds: 1200 / fist.Wpm, DahMilliseconds: fist.Ratio * 1200 / fist.Wpm,
            SignalToNoiseDb: fist.Db, ToneHz: ToneHz, DriftHz: DriftHz,
            QsbHz: fist.Fades ? CwFixtureCatalogue.QsbHz : 0, QsbDepthDb: fist.Fades ? CwFixtureCatalogue.QsbDepthDb : 0,
            Seed: fist.Seed);

        return CwFixtureGenerator.GenerateFromEdges(recipe, Edges(fist));
    }

    /// <summary>The International Morse code for the letters and figures the text holds.</summary>
    private static readonly IReadOnlyDictionary<char, string> Morse = new Dictionary<char, string>
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.", ['G'] = "--.",
        ['H'] = "....", ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..", ['M'] = "--", ['N'] = "-.",
        ['O'] = "---", ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-", ['U'] = "..-",
        ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-", ['Y'] = "-.--", ['Z'] = "--..",
        ['0'] = "-----", ['1'] = ".----", ['2'] = "..---", ['3'] = "...--", ['4'] = "....-", ['5'] = ".....",
        ['6'] = "-....", ['7'] = "--...", ['8'] = "---..", ['9'] = "----.",
    };
}
