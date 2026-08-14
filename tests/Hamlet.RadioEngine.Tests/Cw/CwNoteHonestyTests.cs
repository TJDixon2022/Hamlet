using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The honesty constraint on what the terminal says about a poor decode
/// (HM-DEC-048).
/// </summary>
/// <remarks>
/// The same sweep the band character passages get (HM-DEC-031), pointed at the
/// decoder's notes. These describe what the decoder measured, and a note that
/// wandered into diagnosing the band, the antenna or the other operator's
/// equipment would be a claim Hamlet has no instrument for. Warmth is allowed
/// and never buys a claim (§0.7).
/// </remarks>
public sealed class CwNoteHonestyTests
{
    /// <summary>
    /// What a note may never say.
    /// </summary>
    /// <remarks>
    /// <para>Carried from the band character sweep and extended for the things
    /// a decoder would be uniquely tempted to blame: somebody's antenna,
    /// somebody's radio, somebody's fist.</para>
    /// <para>Note what is NOT banned. "That is the band doing it, not you and
    /// not your radio" declines to blame the operator, which is the opposite of
    /// a claim: it asserts nothing about whether conditions are good, bad,
    /// opening or closing, and the person this is written for has spent six
    /// years assuming every problem is theirs. Banning the reassurance along
    /// with the diagnosis would be enforcing coldness rather than honesty.</para>
    /// </remarks>
    private static readonly string[] Banned =
    {
        // Propagation and the state of the band, which Hamlet cannot see.
        "conditions are", "propagation", "the ionosphere", "band is dead",
        "band is open", "band is closed", "is wide open", "band is poor",
        "the band is", "tonight", "this evening",

        // Equipment, which Hamlet is not measuring and cannot inspect.
        "your antenna", "the antenna", "check your", "your equipment",
        "their equipment", "his equipment", "her equipment", "is faulty",
        "something is wrong", "your radio is", "your rig", "a better antenna",

        // The other operator, who is not on trial.
        "bad fist", "poor fist", "sloppy", "careless", "sending badly",

        // Promises about what will happen, which are not measurements.
        "you will hear", "you can work", "guaranteed", "will improve",
        "will get better",
    };

    /// <remarks>
    /// THE SWEEP. Every note the decoder can produce, checked against every
    /// banned phrase. A note is a measurement wearing plain words, and the
    /// moment it becomes a diagnosis it is asserting something the app has no
    /// way to know (§0.0).
    /// </remarks>
    [Fact]
    public void NoNoteEverDiagnosesAnythingItCannotMeasure()
    {
        var offenders = new List<string>();

        foreach (var note in Enum.GetValues<CwNote>())
        {
            var text = CwNotes.Text(note);

            foreach (var phrase in Banned)
            {
                if (text.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    offenders.Add($"{note} says \"{phrase}\"");
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <remarks>
    /// Proves the sweep is reading something. A banned-phrase test over an empty
    /// set of strings passes forever and proves nothing, which is the failure
    /// mode of every test shaped like this one.
    /// </remarks>
    [Fact]
    public void TheSweepIsActuallyReadingTheNotes()
    {
        var written = Enum.GetValues<CwNote>()
            .Where(n => n != CwNote.None)
            .Select(CwNotes.Text)
            .ToList();

        Assert.Equal(4, written.Count);
        Assert.All(written, t => Assert.True(t.Length > 80, $"too short: {t}"));

        // And it can see a phrase it would have to reject.
        Assert.Contains("conditions are", "conditions are poor", StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves silence is silence. "Nothing worth saying" has to produce nothing
    /// at all, because a note that appeared when the decode was going fine would
    /// train the operator to stop reading them.
    /// </remarks>
    [Fact]
    public void NothingWorthSayingSaysNothing()
        => Assert.Equal("", CwNotes.Text(CwNote.None));

    /// <remarks>
    /// THE VOICE STANDARD, on the one surface where a cold sentence would do the
    /// most damage (§0.7, HM-DEC-040). At most one em dash in a passage, and the
    /// notes as written have none. A stack of clipped fragments reads as
    /// machine-written, and the person reading this has had enough of being told
    /// things by machines.
    /// </remarks>
    [Fact]
    public void TheNotesAreWrittenInTheProjectVoice()
    {
        foreach (var note in Enum.GetValues<CwNote>())
        {
            var text = CwNotes.Text(note);

            Assert.True(
                text.Count(c => c == '—') <= 1,
                $"{note} carries more than one em dash");

            if (text.Length == 0)
            {
                continue;
            }

            // Connected speech rather than a status code: more than one
            // sentence, and no shouting.
            Assert.True(text.Count(c => c == '.') >= 2, $"{note} is a single fact");
            Assert.DoesNotContain("!", text, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves the note that does the most emotional work still says the thing
    /// Tim asked it to say. A newcomer watching letters come and go has no way
    /// to tell a fading signal from a decoder that does not work, and being told
    /// once that it is not their fault is the whole point of the sentence.
    /// </remarks>
    [Fact]
    public void TheFadingNoteStillDeclinesToBlameTheOperator()
    {
        var text = CwNotes.Text(CwNote.Fading);

        Assert.Contains("rising and falling", text, StringComparison.Ordinal);
        Assert.Contains("not you", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the speed note tells the operator the thing that actually fixes
    /// it. Asking somebody to slow down is the single most useful thing a
    /// beginner can learn to do on CW, and the note that mentions the problem
    /// without mentioning the remedy would be describing a wall.
    /// </remarks>
    [Fact]
    public void TheSpeedNoteSaysHowToAskForSlower()
    {
        var text = CwNotes.Text(CwNote.TooFast);

        Assert.Contains("QRS", text, StringComparison.Ordinal);
        Assert.Contains("normal", text, StringComparison.OrdinalIgnoreCase);
    }
}
