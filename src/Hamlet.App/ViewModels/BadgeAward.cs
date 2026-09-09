using System;
using System.Globalization;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **A rank the operator has just reached, at the moment he reaches it.**
/// </summary>
/// <param name="Count">How many contacts are in the log now.</param>
/// <param name="Crossed">The thresholds this rise passed, lowest first.</param>
/// <remarks>
/// <para>**TIM OVERRULED THE AUTHOR ON 2026-09-08**: a badge announces itself in a
/// dialog naming what was earned. Unit 278 built the thresholds, the once-per-rank
/// rule and the silent seeding on a fresh log, and every one of those is untouched —
/// what this adds is something to show.</para>
/// <para>**IT IS A RECORD RATHER THAN A WINDOW, WHICH IS WHAT MAKES IT PROVABLE.**
/// The view model raises one of these and the view decides what a dialog looks like;
/// a test can then assert *what he is told and when* without opening anything, and
/// the one thing that has to be checked on a window — that it never takes focus — is
/// checked on the window.</para>
/// </remarks>
public sealed record BadgeAward(int Count, System.Collections.Generic.IReadOnlyList<int> Crossed)
{
    /// <summary>The belt he is now on.</summary>
    public BeltRank Rank => ContactBelt.For(Count);

    /// <summary>What the dialog's heading says.</summary>
    /// <remarks>
    /// **THE RANK, IN HIS OWN VOCABULARY.** He asked for a belt and this is the belt.
    /// </remarks>
    public string Heading => Rank.Name + " belt";

    /// <summary>
    /// What was earned, naming every threshold this rise passed.
    /// </summary>
    /// <remarks>
    /// <para>**ALL OF THEM, NOT ONLY THE HIGHEST** (unit 278's rule). A quiet evening
    /// on FT8 puts him past two at once, and reporting only the top one would swallow
    /// a milestone he actually reached.</para>
    /// <para>**AND IT IS AN ACKNOWLEDGEMENT RATHER THAN A SCOREBOARD** (§0.7). It says
    /// what happened and stops; it does not congratulate him, tell him to keep going,
    /// or compare him with anybody.</para>
    /// </remarks>
    public string Says
    {
        get
        {
            if (Crossed.Count == 0)
            {
                return "";
            }

            var passed = Crossed.Count == 1
                ? N(Crossed[0]) + " contacts logged."
                : string.Join(" and ", System.Linq.Enumerable.Select(Crossed, N))
                  + " contacts logged.";

            return "That is " + passed;
        }
    }

    /// <summary>What the next rank costs, or "" at the top of the belt.</summary>
    /// <remarks>
    /// **THE SAME SENTENCE THE RING ALREADY HOVERS** (`ContactBelt.Tip`), so the two
    /// cannot come to disagree about how far the next one is (§0).
    /// </remarks>
    public string Next
        => ContactBelt.After(Count) is { } next
            ? N(next.At - Count) + " more and the ring turns " + next.Name + "."
            : "That is the top of the belt.";

    /// <summary>A number with its thousands separated.</summary>
    private static string N(int value)
        => value.ToString("N0", CultureInfo.InvariantCulture);
}
