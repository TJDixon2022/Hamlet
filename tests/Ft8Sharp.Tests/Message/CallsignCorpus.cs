using System.Text;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// A seeded generator of realistically-shaped callsigns, across every shape the callsign field and
/// the hash admit.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every callsign this produces is this project's own datum.</b> Nothing here is taken out of the
/// pinned clone, so a call from this generator, and a hash of one, may be named in a report. That is
/// the whole reason the generator exists rather than a list.
/// </para>
/// <para>
/// <b>Seeded, and the seed is stated wherever a count is reported.</b> A corpus whose membership
/// depends on when it ran is not a measurement, and step 4 has <em>ranking is stable across runs</em>
/// waiting on the same habit.
/// </para>
/// <para>
/// <b>Ten shapes, of which four are standard basecalls and six are not.</b> The standard four are
/// unit 207's, kept deliberately identical so the two units' corpora are comparable. The other six
/// are what the hash exists for: portable and reciprocal prefixes, stroked suffixes that are not
/// <c>/P</c> or <c>/R</c>, special-event calls, and calls at both ends of the three-to-eleven
/// character range the field admits — including calls long enough that the hash stops reading them.
/// </para>
/// </remarks>
internal static class CallsignCorpus
{
    /// <summary>The number of distinct shapes <see cref="Generate"/> can produce.</summary>
    internal const int ShapeCount = 10;

    /// <summary>The number of those shapes that are standard basecalls.</summary>
    internal const int StandardShapeCount = 4;

    /// <summary>
    /// Generates one callsign of the given shape.
    /// </summary>
    /// <param name="random">The seeded source.</param>
    /// <param name="shape">A shape index, taken modulo <see cref="ShapeCount"/>.</param>
    /// <param name="standard">
    /// Whether the shape is one the 28-bit field can pack as a basecall rather than as a hash. Note
    /// that this says what the <em>shape</em> is, not what the field will do with this particular
    /// call: a shape-0 call is a basecall by construction, but a non-standard shape may still
    /// happen to be spelled like one.
    /// </param>
    internal static string Generate(Random random, int shape, out bool standard)
    {
        standard = shape % ShapeCount < StandardShapeCount;
        var text = new StringBuilder();

        switch (shape % ShapeCount)
        {
            case 0:
                // Area digit third: two leading alphanumerics, a digit, then up to three letters.
                text.Append(Alphanumeric(random));
                text.Append(Alphanumeric(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(0, 4));
                break;

            case 1:
                // Area digit second: one alphanumeric, a digit, then one to three letters.
                text.Append(Alphanumeric(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(1, 4));
                break;

            case 2:
                // The Swaziland prefix work-around.
                text.Append("3DA0");
                AppendLetters(text, random, random.Next(1, 4));
                break;

            case 3:
                // The Guinea prefix work-around.
                text.Append("3X");
                text.Append(Letter(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(0, 3));
                break;

            case 4:
                // A DX prefix on the front of an ordinary call: the commonest non-standard shape
                // actually heard, and the one the header comment of the pin's own message type uses.
                AppendPrefix(text, random);
                text.Append('/');
                AppendBasecall(text, random);
                break;

            case 5:
                // A stroked suffix that is neither /P nor /R, so the 28-bit field cannot carry it.
                AppendBasecall(text, random);
                text.Append('/');
                text.Append(random.Next(4) switch
                {
                    0 => "M",
                    1 => "MM",
                    2 => "AM",
                    _ => ((char)('0' + random.Next(10))).ToString(),
                });
                break;

            case 6:
                // A special-event call: a long alphanumeric run with a digit in it and no shape a
                // basecall would recognise.
                text.Append(Letter(random));
                text.Append(Letter(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(4, 7));
                break;

            case 7:
                // The shortest thing the field admits at all.
                text.Append(Letter(random));
                text.Append(Digit(random));
                text.Append(Digit(random));
                break;

            case 8:
                // Exactly eleven characters — the last length the hash reads in full.
                AppendPrefix(text, random);
                text.Append('/');
                while (text.Length < 11)
                {
                    text.Append(Alphanumeric(random));
                }

                break;

            default:
                // Longer than the hash reads. Two of these that agree in their first eleven
                // characters hash identically, on the air and here, and that is not a defect.
                AppendPrefix(text, random);
                text.Append('/');
                AppendBasecall(text, random);
                text.Append('/');
                text.Append(Letter(random));
                text.Append(Letter(random));
                break;
        }

        return text.ToString();
    }

    /// <summary>A whole seeded corpus of distinct callsigns, cycling through every shape.</summary>
    /// <param name="seed">The seed, which the report states.</param>
    /// <param name="count">How many distinct calls are wanted.</param>
    /// <remarks>
    /// Distinct by construction: shapes overlap, so a generator that did not de-duplicate would
    /// report a collision between a call and itself.
    /// </remarks>
    internal static List<string> Distinct(int seed, int count)
    {
        var random = new Random(seed);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var calls = new List<string>(count);

        // Bounded, so that a shape set which cannot produce the requested number of distinct calls
        // ends the loop rather than the process.
        var attempts = 0;
        var limit = count * 20L;
        while (calls.Count < count && attempts < limit)
        {
            attempts++;
            var call = Generate(random, attempts % ShapeCount, out _);
            if (seen.Add(call))
            {
                calls.Add(call);
            }
        }

        return calls;
    }

    private static void AppendBasecall(StringBuilder text, Random random)
    {
        text.Append(Alphanumeric(random));
        text.Append(Alphanumeric(random));
        text.Append(Digit(random));
        AppendLetters(text, random, random.Next(1, 4));
    }

    private static void AppendPrefix(StringBuilder text, Random random)
    {
        text.Append(Letter(random));
        if (random.Next(2) == 0)
        {
            text.Append(Alphanumeric(random));
        }

        text.Append(Digit(random));
    }

    private static char Alphanumeric(Random random)
    {
        var n = random.Next(36);
        return n < 10 ? (char)('0' + n) : (char)('A' + n - 10);
    }

    private static char Digit(Random random) => (char)('0' + random.Next(10));

    private static char Letter(Random random) => (char)('A' + random.Next(26));

    private static void AppendLetters(StringBuilder text, Random random, int count)
    {
        for (var i = 0; i < count; i++)
        {
            text.Append(Letter(random));
        }
    }
}
