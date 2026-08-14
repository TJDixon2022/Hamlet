namespace Hamlet.RadioEngine.Cw;

/// <summary>Where a contact has got to.</summary>
/// <remarks>
/// A contact has a shape, close to a ritual, and everybody knows it except the
/// person who has never made one (HM-DEC-043). Naming the stages is what lets
/// Hamlet offer the one thing anybody would say next rather than the whole
/// ritual at once (HM-DEC-059).
/// </remarks>
public enum ContactStage
{
    /// <summary>Nothing is happening. The thing to do is call.</summary>
    Calling,

    /// <summary>Somebody is calling, and the thing to do is answer them.</summary>
    Answering,

    /// <summary>Answered, and the thing to do is send the report.</summary>
    Exchanging,

    /// <summary>Report sent, and the thing to do is confirm theirs.</summary>
    Confirming,

    /// <summary>Done, and the thing to do is sign off.</summary>
    SigningOff,
}

/// <summary>One thing Hamlet is offering to send, right now.</summary>
/// <param name="Stage">Which part of the ritual this is.</param>
/// <param name="Label">What the button says, e.g. "Call CQ".</param>
/// <param name="Message">Exactly what would go out.</param>
/// <param name="Meaning">The same thing in plain English.</param>
/// <param name="Note">
/// Why it is like that, for somebody who has not seen it before, or "".
/// </param>
public sealed record SendOption(
    ContactStage Stage, string Label, string Message, string Meaning, string Note)
{
    /// <summary>How many keyer messages this will take.</summary>
    public int Pieces => CwMessage.PieceCount(Message);
}

/// <summary>
/// What Hamlet offers to send, and when (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>CONTEXTUAL, NOT A MENU OF EVERYTHING. Calling CQ is one button when
/// nothing is happening; answering is a different button when a station is
/// calling; the exchange, the confirmation and the sign-off each appear when
/// they are the next thing anybody would say. The operator is never presented
/// with the whole ritual at once and asked to pick, because the terror is not
/// the radio, it is not knowing what to say, and a wall of choices is the same
/// problem wearing a different coat (HM-DEC-043).</para>
/// <para>Every message here is the same shape the worked example already
/// teaches, in the operator's own callsign, so the thing they read in the
/// contact panel and the thing the button sends are recognizably one thing.
/// </para>
/// <para>Pure: a stage and some facts in, options out. No radio, no clock, so
/// every case is testable without either (§5).</para>
/// </remarks>
public static class ContactScript
{
    /// <summary>What to offer at this point in a contact.</summary>
    /// <param name="stage">Where the contact has got to.</param>
    /// <param name="yourCall">The operator's callsign.</param>
    /// <param name="theirCall">
    /// Who they are talking to, or null when nobody yet.
    /// </param>
    /// <param name="report">The signal report to send, e.g. "579".</param>
    /// <param name="qth">Where the operator is, as they would send it.</param>
    /// <returns>The options, in the order the buttons appear.</returns>
    public static IReadOnlyList<SendOption> Offer(
        ContactStage stage,
        string yourCall,
        string? theirCall = null,
        string report = "579",
        string? qth = null)
    {
        var you = Normalize(yourCall, ContactShapeCall);
        var them = Normalize(theirCall, "");
        var where = CwMessage.Clean(qth);

        return stage switch
        {
            ContactStage.Calling => Calling(you),
            ContactStage.Answering => Answering(you, them),
            ContactStage.Exchanging => Exchanging(you, them, report, where),
            ContactStage.Confirming => Confirming(you, them),
            _ => SigningOff(you, them),
        };
    }

    /// <summary>
    /// The stage a contact has reached, from what has been sent and heard.
    /// </summary>
    /// <param name="theyAreCalling">True when a station is calling and nobody has answered.</param>
    /// <param name="youAnswered">True once the operator has answered them.</param>
    /// <param name="youSentReport">True once the operator has sent their report.</param>
    /// <param name="theySentReport">True once the other station's report has arrived.</param>
    /// <returns>Where the contact is.</returns>
    /// <remarks>
    /// Deliberately driven by what actually happened rather than by a wizard
    /// step counter, so an operator who does something out of order is followed
    /// rather than corrected. Nobody is grading this (HM-DEC-043).
    /// </remarks>
    public static ContactStage StageOf(
        bool theyAreCalling, bool youAnswered, bool youSentReport, bool theySentReport)
    {
        if (youSentReport && theySentReport)
        {
            return ContactStage.SigningOff;
        }

        if (youSentReport)
        {
            return ContactStage.Confirming;
        }

        if (youAnswered)
        {
            return ContactStage.Exchanging;
        }

        return theyAreCalling ? ContactStage.Answering : ContactStage.Calling;
    }

    private const string ContactShapeCall = "KC3QIS";

    private static IReadOnlyList<SendOption> Calling(string you) => new[]
    {
        new SendOption(
            ContactStage.Calling,
            "Call CQ",
            $"CQ CQ CQ DE {you} {you} K",
            "Calling anyone, this is me, over.",
            "The callsign goes twice because the first one is often half-missed "
            + "while somebody is still tuning you in. K means go ahead."),

        new SendOption(
            ContactStage.Calling,
            "Call CQ and ask for slow",
            $"CQ CQ DE {you} {you} QRS PSE K",
            "Calling anyone, please send slowly, over.",
            "QRS means send more slowly, and PSE is please. Asking for it in the "
            + "call itself is normal and welcome, and it saves you asking after "
            + "somebody has already started."),
    };

    private static IReadOnlyList<SendOption> Answering(string you, string them)
    {
        var to = them.Length > 0 ? them : "the station";

        return new[]
        {
            new SendOption(
                ContactStage.Answering,
                them.Length > 0 ? $"Answer {them}" : "Answer them",
                them.Length > 0 ? $"{them} DE {you} {you} K" : $"DE {you} {you} K",
                $"Calling {to}, this is me, over.",
                "Their call first and yours second, which is the pattern the whole "
                + "hobby uses: who it is for, then who it is from."),

            new SendOption(
                ContactStage.Answering,
                "Answer and ask for slow",
                them.Length > 0
                    ? $"{them} DE {you} QRS PSE K"
                    : $"DE {you} QRS PSE K",
                "This is me, please send more slowly, over.",
                "There is nothing embarrassing in this. Most operators are pleased "
                + "to slow down and a good many of them learned the same way."),
        };
    }

    private static IReadOnlyList<SendOption> Exchanging(
        string you, string them, string report, string qth)
    {
        var to = them.Length > 0 ? $"{them} DE {you}" : $"DE {you}";
        var place = qth.Length > 0 ? $" QTH {qth}" : "";

        return new[]
        {
            new SendOption(
                ContactStage.Exchanging,
                "Send my report",
                $"{to} TU UR RST {report} {report}{place} BK",
                $"Thanks. Your signal is {report} here."
                + (qth.Length > 0 ? $" I am in {qth}. Back to you." : " Back to you."),
                "UR is your and TU is thank you. The report goes twice for the same "
                + "reason the callsign did. BK is a quicker handover than K between "
                + "two stations already talking."),

            new SendOption(
                ContactStage.Exchanging,
                "Say I am new",
                $"{to} TU UR RST {report} {report} QRS PSE ES NEW OP BK",
                "Thanks, your report, please send slowly, I am a new operator.",
                "ES is and. Saying you are new is a real and welcome thing to send, "
                + "and most operators will slow right down and enjoy it."),
        };
    }

    private static IReadOnlyList<SendOption> Confirming(string you, string them)
    {
        var to = them.Length > 0 ? $"{them} DE {you}" : $"DE {you}";

        return new[]
        {
            new SendOption(
                ContactStage.Confirming,
                "Confirm theirs",
                $"{to} R TU FB QSL BK",
                "Understood, thank you, that is fine, received. Back to you.",
                "R means received and understood, FB is fine business, and QSL is "
                + "an acknowledgement. Three ways of saying the same thing, which "
                + "is very Morse."),

            new SendOption(
                ContactStage.Confirming,
                "Ask them to repeat",
                $"{to} PSE AGN QRS BK",
                "Please say that again, more slowly. Back to you.",
                "AGN is again. Asking for a repeat is completely ordinary, and "
                + "every operator on the band has done it this week."),
        };
    }

    private static IReadOnlyList<SendOption> SigningOff(string you, string them)
    {
        var to = them.Length > 0 ? $"{them} DE {you}" : $"DE {you}";

        return new[]
        {
            new SendOption(
                ContactStage.SigningOff,
                "Sign off",
                $"{to} TU 73 SK",
                "Thanks, best regards, this is the end of the contact.",
                "73 is best regards, and never 73s, since the number is already "
                + "plural. SK ends the contact rather than the transmission, so it "
                + "is the sign-off you use once."),

            new SendOption(
                ContactStage.SigningOff,
                "Sign off and thank them",
                $"{to} TU FER MY 1ST QSO 73 SK",
                "Thanks for my first contact, best regards.",
                "FER is for and QSO is a contact. Worth sending once. The other "
                + "operator will be delighted and will very likely say so."),
        };
    }

    private static string Normalize(string? call, string fallback)
    {
        var clean = CwMessage.Clean(call);
        return clean.Length == 0 ? fallback : clean;
    }
}
