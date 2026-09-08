using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hamlet.App.ViewModels;

/// <summary>One rank on the belt: what it is called, when it starts, and its ink.</summary>
/// <param name="Name">The colour's name, in words, for the hover.</param>
/// <param name="At">The contact count at which it is reached.</param>
/// <param name="Ink">The ring's colour, as a hex string.</param>
public sealed record BeltRank(string Name, int At, string Ink);

/// <summary>
/// **The count wears a rank he can see from across the room.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: a coloured ring that rises with the count,
/// martial-arts order, **and gold at ten thousand — go for the gold.**</para>
/// <para>**IT IS A FLOURISH ON TOP OF A NUMBER AND NEVER THE THING ITSELF** (§0.6).
/// The count is large, the progress bar is drawn in a neutral ink, and both read in
/// grayscale exactly as well as in colour. What grayscale loses is the rank, which
/// is the decoration. **Nothing is knowable only from the hue** — a reader who
/// cannot tell brown from red still knows how many contacts he has made and how far
/// the next rank is, which is everything the ring is about.</para>
/// <para>**THE RING IS A BORDER AND NEVER A FILL** (HM-DEC-012). A filled disc in a
/// status bar reads as a status light, which is a different claim entirely, and a
/// column of filled shapes reads as stripes rather than as structure.</para>
/// <para>**ONE LIST, ONE PLACE** (work instruction 278's rule, and 281 task 4's).
/// <see cref="ContactMilestones.Thresholds"/> is derived from this table rather than
/// written beside it: a threshold in two places disagrees with itself the first time
/// one of them is changed. The white rank is the one that is not a threshold — it is
/// where everybody starts, and announcing *that is 0 contacts logged* would be a
/// congratulation for having done nothing.</para>
/// <para>**AND WHITE IS DRAWN AS A LIGHT WARM GREY RATHER THAN AS WHITE**, because
/// the panels behind it are white on warm paper (HM-DEC-012) and a white ring on a
/// white ground is no ring at all. The rank is still called white, which is what the
/// hover says and what a martial artist would call it.</para>
/// </remarks>
public static class ContactBelt
{
    /// <summary>The ten, in order, lowest first.</summary>
    public static IReadOnlyList<BeltRank> Ranks { get; } = new[]
    {
        new BeltRank("white", 0, "#D3CDBE"),
        new BeltRank("yellow", 10, "#D9A400"),
        new BeltRank("orange", 25, "#CC6414"),
        new BeltRank("green", 50, "#2E7D4F"),
        new BeltRank("blue", 100, "#2A6BA8"),
        new BeltRank("purple", 500, "#6B4E9E"),
        new BeltRank("brown", 1000, "#7A5230"),
        new BeltRank("red", 2000, "#B03030"),
        new BeltRank("black", 5000, "#2B2B2B"),
        new BeltRank("gold", 10000, "#C9A227"),
    };

    /// <summary>The rank a count has reached. Never null: everybody starts white.</summary>
    /// <param name="count">How many contacts are in the log.</param>
    /// <returns>The highest rank at or below the count.</returns>
    public static BeltRank For(int count)
        => Ranks.Last(r => r.At <= Math.Max(count, 0));

    /// <summary>The rank above this count, or null at the top.</summary>
    /// <param name="count">How many contacts are in the log.</param>
    /// <returns>The next rank, or null once the gold is his.</returns>
    public static BeltRank? After(int count)
        => Ranks.FirstOrDefault(r => r.At > count);

    /// <summary>
    /// How far between this rank and the next, from 0 to 1.
    /// </summary>
    /// <param name="count">How many contacts are in the log.</param>
    /// <returns>The fraction, or 1 at the top of the belt.</returns>
    /// <remarks>
    /// **THE BAR IS THE GRAYSCALE CARRIER OF PROGRESS** (§0.6), which is why it is
    /// a measured fraction rather than a decoration that fills as it likes. At the
    /// gold it is full, because there is nowhere further to go and an empty bar
    /// there would read as no progress at all.
    /// </remarks>
    public static double Progress(int count)
    {
        if (After(count) is not { } next)
        {
            return 1.0;
        }

        var here = For(count).At;
        var span = next.At - here;

        return span <= 0 ? 0.0 : Math.Clamp((count - here) / (double)span, 0.0, 1.0);
    }

    /// <summary>What the ring says when he hovers it.</summary>
    /// <param name="count">How many contacts are in the log.</param>
    /// <returns>The rank he holds, and what the next one costs.</returns>
    /// <remarks>
    /// **THE WORDS ARE HERE AND NOT ON THE SCREEN** (Tim, 2026-09-08). What was
    /// beside the number — *contacts*, and *6 to 10* — is in this sentence, and the
    /// number and the ring carry it unhovered.
    /// </remarks>
    public static string Tip(int count)
    {
        var rank = For(count);

        var held = "You are on the " + rank.Name + " belt, at "
            + N(count) + (count == 1 ? " contact" : " contacts") + ".";

        if (After(count) is not { } next)
        {
            return held + " That is the top of the belt.";
        }

        var toGo = next.At - count;

        return held + " " + (toGo == 1 ? "One more contact" : N(toGo) + " more contacts")
            + " and the ring turns " + next.Name + " at " + N(next.At) + ".";
    }

    /// <summary>A number with its thousands separated.</summary>
    private static string N(int value)
        => value.ToString("N0", CultureInfo.InvariantCulture);
}
