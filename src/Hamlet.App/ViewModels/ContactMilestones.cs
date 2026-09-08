using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **The badges: an acknowledgement that he is making contacts.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**, in his own words: *"just sort of an
/// acknowledgment that you're making contacts."* Nine thresholds, and **every
/// logged contact counts** — work the same station on three bands and that is
/// three, with no judgement made about whether two records are the same
/// station.</para>
/// <para>**NO BADGE IS AWARDED FOR ANYTHING BUT THE COUNT.** Not for a country, not
/// for a band, not for a first contact of the day. Those are worth having and each
/// would need its own rules about what counts; none of them is what he ruled, and a
/// badge nobody asked for is a claim about his operating that he did not make.</para>
/// <para>**EARNED IS DERIVED FROM THE COUNT AND NEVER REMEMBERED, WHICH ALSO
/// ANSWERS WHAT HAPPENS IF THE FILE SHRINKS** (§0.0). A badge says *your log holds a
/// hundred contacts*. If the log holds ninety, Hamlet cannot see a hundred, and a
/// remembered high-water mark would keep asserting one on the strength of a reading
/// nobody can check — on a new machine, or after a file was restored from an old
/// backup, it would be a number with no evidence behind it. So a shrunken log shows
/// fewer badges, and that is the honest picture rather than a bug.</para>
/// <para>**WHAT IS REMEMBERED IS THE ANNOUNCEMENT**, which is a different fact: it
/// stops Hamlet saying the same congratulations twice, and it is allowed to be
/// remembered because it is a statement about what Hamlet has already said rather
/// than about what the log contains.</para>
/// </remarks>
/// <param name="Count">How many records are in the log.</param>
public sealed record ContactMilestones(int Count)
{
    /// <summary>The nine, in one place, and that place is the belt.</summary>
    /// <remarks>
    /// <para>**ONE LIST, NOT SCATTERED** (work instruction 278). A threshold written
    /// in two places is a threshold that disagrees with itself the first time one is
    /// changed, and this list is read by the badge line, the next-badge line and the
    /// announcement alike.</para>
    /// <para>**AND FROM 281 TASK 4 THE LIST IS <see cref="ContactBelt.Ranks"/>**,
    /// rather than the same nine numbers typed out twice. The white rank is the one
    /// that is not a threshold: it is where everybody starts, and *that is 0 contacts
    /// logged* would be a congratulation for having done nothing.</para>
    /// </remarks>
    public static IReadOnlyList<int> Thresholds { get; } =
        ContactBelt.Ranks.Where(r => r.At > 0).Select(r => r.At).ToList();

    /// <summary>Every badge the count has reached, lowest first.</summary>
    public IReadOnlyList<int> Earned
        => Thresholds.Where(t => Count >= t).ToList();

    /// <summary>The highest badge earned, or null.</summary>
    public int? Highest
        => Earned.Count == 0 ? null : Earned[^1];

    /// <summary>The next badge, or null once he has them all.</summary>
    public int? Next
        => Thresholds.FirstOrDefault(t => t > Count) is var t && t > Count ? t : null;

    /// <summary>How many contacts away the next one is, or null.</summary>
    public int? ToGo => Next is { } next ? next - Count : null;

    /// <summary>True where he has passed at least one.</summary>
    public bool HasAny => Earned.Count > 0;

    /// <summary>What the log window says about where he stands.</summary>
    /// <remarks>
    /// **IT IS AN ACKNOWLEDGEMENT AND NOT A SCOREBOARD** (§0.7). It names what he
    /// has passed and what is next, in a sentence, without congratulating him for
    /// being at 3 of 10 on his first evening.
    /// </remarks>
    public string Line
    {
        get
        {
            var passed = Highest is { } high
                ? "You have passed " + N(high) + " contacts. "
                : "";

            if (Next is not { } next || ToGo is not { } toGo)
            {
                return passed + "That is every badge there is.";
            }

            return passed
                + (toGo == 1
                    ? "One more and you reach " + N(next) + "."
                    : N(toGo) + " to go until " + N(next) + ".");
        }
    }

    /// <summary>
    /// **Which badges a rise from one count to another crossed.**
    /// </summary>
    /// <param name="was">What had already been announced, or 0.</param>
    /// <returns>Every threshold above that and at or below the count.</returns>
    /// <remarks>
    /// **A JUMP EARNS EVERY BADGE IT PASSES, NOT ONLY THE HIGHEST.** A quiet
    /// evening on FT8 puts him past two of these at once, and reporting only the
    /// top one would quietly swallow a milestone he actually reached. It is also
    /// the case that a naive comparison against the newest threshold gets wrong,
    /// which is why the test names nine to twenty-six.
    /// </remarks>
    public IReadOnlyList<int> EarnedSince(int was)
        => Thresholds.Where(t => t > was && t <= Count).ToList();

    /// <summary>What Hamlet says, once, when one is passed.</summary>
    /// <param name="was">The highest already announced, or 0.</param>
    /// <returns>One sentence, or "" where nothing was crossed.</returns>
    /// <remarks>
    /// **ONCE, QUIETLY, AND NEVER IN A DIALOG** (work instruction 278). He is often
    /// mid-exchange with fifteen seconds to answer in, and a modal window at that
    /// moment costs him the contact it is congratulating him for.
    /// </remarks>
    public string Announcement(int was)
    {
        var crossed = EarnedSince(was);

        if (crossed.Count == 0)
        {
            return "";
        }

        if (crossed.Count == 1)
        {
            return "That is " + N(crossed[0]) + " contacts logged.";
        }

        // **BOTH ARE NAMED**, because he passed both and being told only about the
        // higher one would lose the smaller.
        var all = string.Join(
            " and ", crossed.Select(t => N(t)));

        return "That is " + all + " contacts logged.";
    }

    /// <summary>A number with its thousands separated.</summary>
    private static string N(int value)
        => value.ToString("N0", CultureInfo.InvariantCulture);
}
