using System.Text;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// What Hamlet says after setting the receiver up for a block.
/// </summary>
/// <remarks>
/// <para>**A RADIO THAT CHANGES ITSELF SILENTLY IS THE "IS IT BROKEN" CONFUSION
/// RELOCATED RATHER THAN REMOVED** (HM-DEC-056, work instruction 042 task 4).
/// The operator has had settings moved out from under him by machines before,
/// and the thing that makes this different from a rig-control app is that he is
/// told what changed and why, in connected speech rather than as a list of
/// register writes (§0.7, HM-DEC-034).</para>
/// <para>**THREE RULES, AND ALL THREE ARE ABOUT NOT OVERCLAIMING.** Only what
/// actually changed is mentioned, because narrating a control that was already
/// correct teaches him to stop reading. Anything Hamlet could not confirm is
/// said as unconfirmed rather than quietly left out, which is §0.0 in a
/// sentence. Anything his own hand is holding is said as his, because the app
/// taking credit for a switch he set would be the smallest possible lie and
/// still a lie.</para>
/// <para>**THE REASONS ARE THE DATA'S, NOT THIS FILE'S.** Every clause below
/// that explains why comes from the block's own row, so a mode gaining a
/// condition gains its explanation with it and nothing here has to be
/// remembered (§0).</para>
/// </remarks>
public static class ReceiverSetupVoice
{
    /// <summary>Say what the tune-in did.</summary>
    /// <param name="results">What happened to each condition.</param>
    /// <returns>The sentence, or "" where there is nothing worth saying.</returns>
    /// <remarks>
    /// **NOTHING TO SAY IS A REAL ANSWER.** A radio that was already set up
    /// correctly produces silence rather than a reassurance, and a status line
    /// that congratulates itself on every tune-in is one nobody reads by the
    /// third time.
    /// </remarks>
    public static string Say(IReadOnlyList<ConditionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var said = new List<string>();

        var changed = results
            .Where(r => r.Outcome == ConditionOutcome.Changed)
            .Select(Did)
            .ToList();

        if (changed.Count > 0)
        {
            said.Add("I " + Join(changed) + ".");
        }

        foreach (var result in results.Where(
                     r => r.Outcome == ConditionOutcome.LeftToTheOperator))
        {
            said.Add(
                $"Your {result.Condition.Control} is {Words(result.WasText)} and "
                + "I have left it there, because you moved it after I last set it.");
        }

        foreach (var result in results.Where(
                     r => r.Outcome == ConditionOutcome.NotConfirmed))
        {
            said.Add(
                $"I asked for the {result.Condition.Control} to be "
                + $"{result.Condition.WantedText} and the radio did not confirm it, "
                + "so I do not know where it is now.");
        }

        foreach (var result in results.Where(
                     r => r.Outcome == ConditionOutcome.NotRead))
        {
            said.Add(
                $"I could not read the {result.Condition.Control}, so I have not "
                + "touched it.");
        }

        // **AND THE ONES HAMLET CANNOT SET AT ALL.** Saying nothing about them
        // would leave him with a three-kilohertz block seven pixels wide and no
        // idea why, which is what sent him to the front of the radio.
        foreach (var result in results.Where(
                     r => r.Outcome == ConditionOutcome.SpokenOnly))
        {
            said.Add(Cannot(result));
        }

        return string.Join(" ", said);
    }

    private static string Did(ConditionResult result)
    {
        var condition = result.Condition;

        var verb = condition.WantedText.Equals("off", StringComparison.OrdinalIgnoreCase)
            ? $"turned the {condition.Control} off"
            : $"set the {condition.Control} to {condition.WantedText}";

        // **NO COMMA BEFORE THE BECAUSE**, which is not a nicety. With one, the
        // clauses join as "off, because it chops up the tones and turned the auto
        // notch off", and the "and" reads as part of the reason rather than as
        // the next thing Hamlet did. Read it aloud and it comes apart (§0.7).
        return condition.Says.Length > 0
            ? $"{verb} because {condition.Says}"
            : verb;
    }

    private static string Cannot(ConditionResult result)
    {
        var condition = result.Condition;

        var reason = condition.Says.Length > 0
            ? $", because {condition.Says}"
            : "";

        // A control with no cited command at all, against one whose value nobody
        // here has settled. Those are different admissions and they are worth
        // keeping apart: the first is a gap in what Hamlet can reach, and the
        // second is a gap in what Hamlet knows.
        return condition.Field is null
            ? $"Your {condition.Control} wants to be {condition.WantedText}"
              + $"{reason}, and that is one I cannot set from here."
            : $"The {condition.Control} usually wants to be {condition.WantedText} "
              + $"here{reason}, and that is not settled well enough for me to "
              + "change it on your radio.";
    }

    private static string Words(string? text)
        => string.IsNullOrWhiteSpace(text) ? "where it was" : text;

    private static string Join(IReadOnlyList<string> parts)
    {
        if (parts.Count == 1)
        {
            return parts[0];
        }

        var sentence = new StringBuilder();

        for (var i = 0; i < parts.Count; i++)
        {
            if (i > 0)
            {
                // The last joiner keeps its comma, because each clause already
                // carries a reason long enough to need the pause.
                sentence.Append(i == parts.Count - 1 ? ", and " : ", ");
            }

            sentence.Append(parts[i]);
        }

        return sentence.ToString();
    }
}
