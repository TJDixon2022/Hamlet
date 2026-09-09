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
    /// <summary>
    /// The mode worked for the first time, or null where this is a belt.
    /// </summary>
    /// <remarks>
    /// <para>**ONE NOTICE FOR BOTH KINDS OF ACHIEVEMENT** (work instruction 287
    /// task 3: reuse this, do not build a second notice). A second window would be
    /// a second place for the one property that matters here to be got wrong, which
    /// is that it never takes his focus, and the first one to drift would be the one
    /// nobody was watching.</para>
    /// <para>**SO EVERY DIFFERENCE BETWEEN A BELT AND A MODE FIRST IS FOUR STRINGS
    /// AND A COLOR**, decided here where a test can read them, rather than anywhere
    /// a window has to be opened to find out.</para>
    /// </remarks>
    public ModeFirstRow? First { get; init; }

    /// <summary>
    /// A group of the achievements screen that has just opened, or null.
    /// </summary>
    /// <remarks>
    /// <para>**THE THIRD KIND THROUGH THE SAME WINDOW** (work instruction 298 task
    /// 6: reuse unit 286's notice, do not build a second one). A belt, a mode first
    /// and a group opening are three ladders read off one log, and every one of them
    /// has to keep the one property that matters: **it never takes his focus**. A
    /// second window would be a second place for that to be got wrong, and the first
    /// to drift would be the one nobody was watching.</para>
    /// <para>**§3.2 MADE VISIBLE.** *One contact in, three or four possibilities
    /// out.* This carries what opened and how many cards came with it, which is that
    /// sentence turned into something he can see happen.</para>
    /// </remarks>
    public AchievementOpening? Opened { get; init; }

    /// <summary>The belt he is now on.</summary>
    public BeltRank Rank => ContactBelt.For(Count);

    /// <summary>What goes in the ring: the count, or the mode.</summary>
    /// <remarks>
    /// **THE RING SHOWS WHAT THE ACHIEVEMENT IS COUNTED IN.** A belt is counted in
    /// contacts and a mode first is not counted at all, so putting the running total
    /// in it would answer a question nobody asked.
    /// </remarks>
    public string Ring => Opened is { } opening
        ? opening.Cards.ToString("N0", CultureInfo.InvariantCulture)
        : First is null
            ? Count.ToString("N0", CultureInfo.InvariantCulture)
            : First.Name;

    /// <summary>The ring's ink.</summary>
    /// <remarks>
    /// **A MODE FIRST WEARS THE CARD'S OWN EARNED INK**, so the notice and the row
    /// it just filled in are the same green rather than two greens (§0).
    /// </remarks>
    public string Ink => First?.Ink ?? Rank.Ink;

    /// <summary>What the dialog's heading says.</summary>
    /// <remarks>
    /// **THE RANK, IN HIS OWN VOCABULARY.** He asked for a belt and this is the belt.
    /// </remarks>
    public string Heading => Opened is { } opening
        ? opening.Title + " is open"
        : First is null
            ? Rank.Name + " belt"
            : "first " + First.Name + " contact";

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
            if (Opened is { } opening)
            {
                // **WHAT OPENED AND HOW MUCH CAME WITH IT** (§3.2). The count is the
                // point: an unlock that revealed one card is a filled slot, and one
                // that revealed several is the screen growing.
                return opening.Cards == 1
                    ? "That opens " + opening.Title + ", with one new record in it."
                    : "That opens " + opening.Title + ", with "
                      + opening.Cards.ToString("N0", CultureInfo.InvariantCulture)
                      + " new records in it.";
            }

            if (First is not null)
            {
                // **THE STATION IS NAMED WHERE THE RECORD CARRIES ONE**, because
                // that is the thing he will remember about it, and left out where
                // it does not rather than filled in with something plausible
                // (§0.0). Every record Hamlet writes carries a callsign, so the
                // second branch is about a file another logger wrote.
                return First.Station.Length > 0
                    ? First.Station + " is your first contact on " + First.Name + "."
                    : "That is your first contact on " + First.Name + ".";
            }

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
    {
        get
        {
            if (Opened is not null)
            {
                return "It is on the achievements screen from now on.";
            }

            if (First is not null)
            {
                // **WHERE IT WENT, RATHER THAN WHAT IS LEFT.** Five modes to go
                // would be a scoreboard, and most of the five are not his to have
                // done anyway.
                return "It is on the achievements screen from now on.";
            }

            return ContactBelt.After(Count) is { } next
                ? N(next.At - Count) + " more and the ring turns " + next.Name + "."
                : "That is the top of the belt.";
        }
    }

    /// <summary>A number with its thousands separated.</summary>
    private static string N(int value)
        => value.ToString("N0", CultureInfo.InvariantCulture);
}
