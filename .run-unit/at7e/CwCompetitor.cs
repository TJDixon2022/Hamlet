namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Somebody else keying inside the same passband as the station being read.
/// </summary>
/// <param name="OffsetHz">
/// How far away they are, signed: positive is above the station Hamlet is
/// reading, negative is below.
/// </param>
/// <param name="RelativeDb">
/// How loud they are against the station being read. Negative is quieter, which
/// is the usual case, and positive means the competitor is the louder of the
/// two.
/// </param>
/// <param name="ToneHz">Where they actually sit, so the figure can be checked.</param>
/// <remarks>
/// <para>**THE SURVEY HAS ALWAYS KNOWN THIS AND NOTHING HAS EVER READ IT.**
/// `CwToneSurvey.Candidates` returns every bin it would admit as keying, with a
/// pitch and a lift for each, and `CwToneTracker.CoarseCandidates` hands that
/// straight out. Until now the only caller in the tree was nothing at all: the
/// verdict says which bin won and the runners-up were dropped on the floor.</para>
/// <para>**IT SAYS WHAT WAS MEASURED AND NOTHING ABOUT WHOSE IT IS** (§0.0), in
/// the same way <see cref="ToneInterference"/> does. Hamlet cannot tell a second
/// operator from the same operator's own image in another bin, and it does not
/// guess: what is useful is the offset and the strength, which are facts, and
/// the reason they matter, which is that anything keying inside the filter is
/// arriving in the same envelope the decoder measures.</para>
/// <para>**AND ITS ABSENCE ASSERTS NOTHING.** No competitor found means the
/// survey did not find one, which is not the same as the frequency being clear,
/// and nothing built on this may say that it is (HM-DEC-009).</para>
/// </remarks>
public readonly record struct CwCompetitor(
    double OffsetHz, double RelativeDb, double ToneHz)
{
    /// <summary>
    /// How far a candidate has to sit from the tracked station before it is
    /// somebody else rather than the same signal.
    /// </summary>
    /// <remarks>
    /// <para>**THIS PROJECT ALREADY HAD A FIGURE FOR EXACTLY THIS QUESTION AND
    /// IT IS A HUNDRED AND TWENTY-FIVE.** `CwToneTracker` has carried it as
    /// `CompetitorSeparationHz` — how far from the tracked note another one has
    /// to be before it counts as a different station rather than the same one
    /// leaking sideways — and the survey carries the same number again as
    /// `NoiseSeparationHz`. It is defined here and read there, rather than
    /// written down a third time (§0).</para>
    /// <para>**THE FIRST DRAFT PUT IT AT FIFTY AND THE TEST CAUGHT IT.** A single
    /// clean station, alone on a generated band, produced a competitor fifty
    /// hertz away and two decibels down: its own image in a neighbouring bin.
    /// That is HM-DEC-127's fault exactly, which is the ruling recording a
    /// station's own image winning a survey outright, and the coarse bank is
    /// twenty-five hertz apart so several bins hear every signal.</para>
    /// <para>**WHAT IT COSTS IS STATED RATHER THAN HIDDEN.** Two operators closer
    /// together than this are not named, because Hamlet genuinely cannot tell
    /// them from one operator leaking, and saying so would be a guess presented
    /// as a decode (§0.0).</para>
    /// </remarks>
    public const double SeparationHz = 125;

    /// <summary>
    /// How far below the tracked station a competitor may sit and still be worth
    /// mentioning.
    /// </summary>
    /// <remarks>
    /// **TWENTY DECIBELS DOWN IS NOT IN THE WAY.** The point of saying anything
    /// is that the operator can reach for a control and improve what he is
    /// reading; a station far under the one he is copying is not what is stopping
    /// him, and naming it teaches him to read past the message (HM-DEC-074's
    /// reasoning about a warning nobody needs).
    /// </remarks>
    public const double QuietestWorthSayingDb = -20;

    /// <summary>Which way to turn, in words rather than in a sign.</summary>
    public string Side => OffsetHz >= 0 ? "above" : "below";

    /// <summary>
    /// What is in the way and the control that changes it, in one sentence.
    /// </summary>
    /// <remarks>
    /// <para>**A DIAGNOSIS IS NOT HELP** (HM-DEC-148). Naming a second station
    /// and stopping there leaves the operator knowing why the screen is wrong and
    /// no better off, which is the fault that ruling was written for: Hamlet held
    /// the answer, printed it in a file, and the operator found it the next day.
    /// So the sentence names the control on the front of this radio.</para>
    /// <para>**THE FILTER FIRST AND THE PASSBAND SHIFT SECOND**, because a
    /// narrower filter helps whichever side the competitor is on and the shift
    /// only helps once there is a filter narrow enough to move. Both are the
    /// operator's to turn; Hamlet writes nothing (HM-DEC-084's third tier is not
    /// entered here at all).</para>
    /// <para>Read-only, and it says so by never offering to do it.</para>
    /// </remarks>
    public string Sentence
        => $"There is somebody else keying about {Math.Abs(OffsetHz):0} hertz "
           + $"{Side} the station you are reading, and they are coming in "
           + $"{Loudness}. Everything inside the filter arrives in the same "
           + "envelope Hamlet measures, so narrowing FILTER will help, and once "
           + "it is narrow the TWIN PBT controls will slide the passband off "
           + "them.";

    private string Loudness
        => RelativeDb switch
        {
            >= 3 => "louder than the one you want",
            >= -3 => "about as strongly",
            >= -10 => $"about {Math.Abs(RelativeDb):0} decibels down",
            _ => $"well under it, about {Math.Abs(RelativeDb):0} decibels down",
        };
}
