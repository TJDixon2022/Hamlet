namespace Hamlet.RadioEngine.Cw;

/// <summary>Where Hamlet believes a contact has got to, if anywhere.</summary>
/// <remarks>
/// THE LOST STATE IS FIRST IN THIS LIST ON PURPOSE. It is the default and the
/// resting position, not an error (HM-DEC-076).
/// </remarks>
public enum ContactFollowState
{
    /// <summary>Hamlet is not following. The default, and never a failure.</summary>
    Lost,

    /// <summary>The operator called and nobody has come back yet.</summary>
    Calling,

    /// <summary>A station answered the operator by name.</summary>
    TheyAnswered,

    /// <summary>Reports are being exchanged.</summary>
    Exchanging,

    /// <summary>Both reports have been sent and it is winding up.</summary>
    SigningOff,
}

/// <summary>What Hamlet can say about where a contact is.</summary>
/// <param name="State">Where it believes it is.</param>
/// <param name="TheirCall">Who the operator is working, or null.</param>
/// <param name="Says">One sentence for the operator.</param>
/// <param name="Evidence">What that belief rests on, or "" when lost.</param>
public sealed record ContactFollow(
    ContactFollowState State, string? TheirCall, string Says, string Evidence);

/// <summary>
/// Following a contact, and admitting when it has stopped following
/// (HM-DEC-076).
/// </summary>
/// <remarks>
/// <para>THE LOST STATE WAS DESIGNED FIRST AND IS THE DEFAULT. A guide that
/// silently keeps guessing after it stopped following is far worse than one that
/// says it is not sure: the first sends somebody confidently to the wrong part
/// of a ritual they have never performed, and the second hands them back the
/// only thing that was ever reliable, which is what the radio is actually
/// hearing. Every path through this returns to lost when evidence runs out
/// (§0.0).</para>
/// <para>EVERY TRANSITION IS JUSTIFIED FROM SOMETHING OBSERVED. What the
/// operator sent, which Hamlet knows exactly because it sent it, and what the
/// decoder resolved cleanly, which is a callsign every character of which came
/// back solid (HM-DEC-073) or a ritual word matched whole. Nothing here infers a
/// stage from the passage of time, from a partial decode, or from what usually
/// happens next.</para>
/// <para>EVIDENCE GOES STALE. A contact that has produced nothing for a while
/// has almost certainly ended or moved on without Hamlet, so the belief expires
/// rather than persisting. Sitting on a stale stage is the exact failure this
/// class exists to avoid.</para>
/// <para>Pure: what was sent, what was heard and a moment in, a belief out. No
/// clock, no radio (§5).</para>
/// </remarks>
public static class ContactTracker
{
    /// <summary>
    /// How long a belief survives with nothing new to support it.
    /// </summary>
    /// <remarks>
    /// Four minutes. A Morse exchange at a relaxed pace has long gaps in it,
    /// so a short window would call itself lost in the middle of an ordinary
    /// contact; much longer and Hamlet would still be claiming to follow a
    /// contact that ended while somebody made tea.
    /// </remarks>
    public static readonly TimeSpan Staleness = TimeSpan.FromMinutes(4);

    /// <summary>What Hamlet says when it is not following.</summary>
    /// <remarks>
    /// It hands back the thing that was always reliable rather than apologizing.
    /// The terminal is showing exactly what the radio heard, and on a contact
    /// Hamlet has lost, that is worth more than a guess about the ritual.
    /// </remarks>
    public const string LostSays =
        "Hamlet is not sure where this contact has got to. What the radio is "
        + "hearing is in the terminal, and that is the reliable thing.";

    /// <summary>Ritual words that say a report is being passed.</summary>
    private static readonly string[] ReportWords = { "RST", "UR", "5NN", "599", "TU" };

    /// <summary>Ritual words that say somebody is winding up.</summary>
    private static readonly string[] ClosingWords = { "73", "SK", "<SK>", "CUL", "GL" };

    /// <summary>
    /// Where the contact is, from what was sent and what was heard.
    /// </summary>
    /// <param name="lastSentStage">
    /// The stage of the last thing the operator actually sent, or null.
    /// </param>
    /// <param name="lastSentUtc">When that went out, or null.</param>
    /// <param name="heard">What the decoder has produced since, or null.</param>
    /// <param name="heardAtUtc">When the last character arrived, or null.</param>
    /// <param name="yourCall">The operator's callsign.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The belief, never null, and lost whenever evidence runs out.</returns>
    public static ContactFollow Follow(
        ContactStage? lastSentStage,
        DateTime? lastSentUtc,
        IReadOnlyList<CwCharacter>? heard,
        DateTime? heardAtUtc,
        string? yourCall,
        DateTime nowUtc)
    {
        // NOTHING SENT AND NOTHING HEARD IS LOST, not "calling". Hamlet has no
        // reason to believe a contact is happening at all.
        if (lastSentStage is null && heardAtUtc is null)
        {
            return Lost();
        }

        var freshest = Newest(lastSentUtc, heardAtUtc);

        // STALE EVIDENCE IS NO EVIDENCE. A contact that has gone quiet for
        // minutes has ended or moved on without Hamlet, and continuing to name
        // a stage would be the silent guessing this exists to prevent.
        if (freshest is null || nowUtc - freshest.Value >= Staleness)
        {
            return Lost();
        }

        var answering = CallsignResolver.AnsweringYou(heard, yourCall);
        var words = Words(heard);

        // THEY CAME BACK TO ME BY NAME, which is the one transition Hamlet can
        // be certain of: his own callsign in the addressed position of a clean
        // decode, with a clean callsign after the DE (HM-DEC-073).
        if (answering is not null)
        {
            if (words.Any(w => ClosingWords.Contains(w, StringComparer.Ordinal)))
            {
                return new ContactFollow(
                    ContactFollowState.SigningOff, answering,
                    $"{answering} is signing off. A 73 back and it is a contact.",
                    $"heard {answering} calling you, and a sign-off word");
            }

            if (words.Any(w => ReportWords.Contains(w, StringComparer.Ordinal)))
            {
                return new ContactFollow(
                    ContactFollowState.Exchanging, answering,
                    $"{answering} is passing a report. Yours goes back next.",
                    $"heard {answering} calling you, and a report word");
            }

            return new ContactFollow(
                ContactFollowState.TheyAnswered, answering,
                $"{answering} came back to you. Answering them is the next thing.",
                $"heard your callsign with {answering} sending it");
        }

        // NOTHING RESOLVED, SO THE ONLY EVIDENCE IS WHAT HE SENT. That is
        // enough to say he called and nobody has come back yet, and it is not
        // enough to say anything about a station, because none was identified.
        if (lastSentStage is ContactStage.Calling && heardAtUtc is null)
        {
            return new ContactFollow(
                ContactFollowState.Calling, null,
                "Your call went out and nothing has come back yet. Listening is "
                + "the whole of the next step.",
                "you called, and nothing has been decoded since");
        }

        // SOMETHING WAS HEARD AND NONE OF IT RESOLVED. This is the case the
        // lost state exists for, and it is the commonest one on a real band.
        return Lost();
    }

    private static ContactFollow Lost()
        => new(ContactFollowState.Lost, null, LostSays, "");

    private static DateTime? Newest(DateTime? a, DateTime? b)
        => a is null ? b : b is null ? a : a > b ? a : b;

    /// <summary>
    /// The whole, solid words in a decode.
    /// </summary>
    /// <remarks>
    /// Solid only, for the same reason a callsign must be (HM-DEC-073). A
    /// half-read "73" is also a half-read anything else, and moving somebody to
    /// the sign-off on it would end a contact that was still going.
    /// </remarks>
    private static IReadOnlyList<string> Words(IReadOnlyList<CwCharacter>? heard)
    {
        if (heard is null || heard.Count == 0)
        {
            return Array.Empty<string>();
        }

        var words = new List<string>();
        var current = new System.Text.StringBuilder();
        var solid = true;

        void Flush()
        {
            if (current.Length > 0 && solid)
            {
                words.Add(current.ToString().ToUpperInvariant());
            }

            current.Clear();
            solid = true;
        }

        foreach (var character in heard)
        {
            if (character.IsWordGap)
            {
                Flush();
                continue;
            }

            if (character.Confidence != CwConfidence.High)
            {
                solid = false;
            }

            current.Append(character.Text);
        }

        Flush();

        return words;
    }
}
