namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Where one sender's gaps divide, measured from the gaps (HM-DEC-115).
/// </summary>
/// <param name="ElementMs">The middle of the gaps inside a character.</param>
/// <param name="CharacterMs">The middle of the gaps between characters.</param>
/// <param name="WordMs">The middle of the gaps between words.</param>
/// <param name="ElementCutMs">Where a gap stops being an element gap.</param>
/// <param name="CharacterCutMs">Where a gap becomes a word gap.</param>
/// <param name="ElementCount">How many gaps landed in the element class.</param>
/// <param name="CharacterCount">How many landed in the character class.</param>
/// <param name="WordCount">How many landed in the word class.</param>
/// <remarks>
/// The centers are carried as well as the boundaries because a Farnsworth sender
/// should be visible rather than merely survived: a surface can say what this
/// station's spacing actually is, and the confidence model can measure how far a
/// gap sat from the center it was assigned to.
/// </remarks>
public readonly record struct CwGapClasses(
    double ElementMs,
    double CharacterMs,
    double WordMs,
    double ElementCutMs,
    double CharacterCutMs,
    int ElementCount = 0,
    int CharacterCount = 0,
    int WordCount = 0)
{
    /// <summary>
    /// How many element gaps long this sender's character gap is.
    /// </summary>
    /// <remarks>
    /// Three on a textbook sender and six on the ARRL bulletin that produced
    /// HM-DEC-115. **It is a description and never a warning**: a sender whose
    /// character gap is a large multiple of the element gap is a normal operator
    /// sending Farnsworth, which is how traffic nets and bulletins are sent.
    /// </remarks>
    public double FarnsworthRatio => ElementMs <= 0 ? 0 : CharacterMs / ElementMs;

    /// <summary>
    /// True when this sender left word gaps long enough to measure (HM-DEC-142).
    /// </summary>
    /// <remarks>
    /// <para>**FALSE IS A MEASUREMENT AND NOT A FAILURE.** A callsign, a contest
    /// exchange or a `V` string carries element gaps and character gaps and no
    /// word gaps at all, so the third class is empty because of what he sent
    /// rather than because the fit could not see it.</para>
    /// <para>Where it is false the character cut is infinite, so no gap can be
    /// classified as a word boundary and the transcript comes out unspaced. That
    /// asserts exactly what was measured: no word boundary anywhere. **The
    /// surface has to say so**, which is the load-bearing half of that ruling.</para>
    /// </remarks>
    public bool WordSpacingMeasured => WordCount > 0;
}

/// <summary>
/// Fits three gap classes to the gaps themselves (HM-DEC-115).
/// </summary>
/// <remarks>
/// <para>**NO DIT MULTIPLE APPEARS ANYWHERE IN HERE, AND THAT IS THE POINT.**
/// HM-DEC-048 ruled that gap classes are clustered from the gaps and never from
/// multiples of the dit, and the code did not do it. Textbook Morse spaces
/// elements one dit apart, characters three and words seven, and **real
/// operators send Farnsworth**: the characters at full speed with the gaps
/// stretched, which is how ARRL bulletins and NTS traffic nets go out and close
/// to the likeliest thing a beginner tunes across.</para>
/// <para>Measured on a strong off-air bulletin at S4, tone 501 Hz: dit 57 ms,
/// element gaps 40, character gaps 190 to 300, word gaps 400 and up. **The
/// element gap is shorter than the dit and the character gap is six times the
/// element gap rather than three.** A decoder using dit multiples gets every
/// character right and puts every space in the wrong place, which is not a
/// transcript, and that is exactly what was on the operator's screen.</para>
/// <para>One implementation, read by both passes, because two copies of a
/// classifier is two classifiers (§0).</para>
/// </remarks>
public static class CwGapFit
{
    /// <summary>The shortest silence anybody could have left, in milliseconds.</summary>
    /// <remarks>
    /// <para>**TWENTY-FIVE, WHICH IS THE SHORTEST DIT THIS RADIO CAN SEND**
    /// (<see cref="CwToneSurvey.ShortestDitMs"/>, forty-eight words a minute, the
    /// fastest its own keyer will go). Nothing below that is a silence anybody
    /// left; it is the gate flapping while the detector finds the signal.</para>
    /// <para>**AND IT IS THE CENTRE THEY SPOIL RATHER THAN THE BOUNDARY.**
    /// Measured on `tightfist-easy`, whose element gaps are 80 milliseconds and
    /// whose character gaps are 162: at the first `S` the fit had the boundary at
    /// 89, which classifies every one of those correctly, and the element class
    /// **centre at 49**, because the window still held gaps of 15, 20, 30 and 35
    /// from before the signal was acquired. Confidence is measured from the
    /// boundary toward the centre, so a gap of 85 scored 4 milliseconds out of
    /// 40 — nought point one — and a character whose pattern was `...` and whose
    /// elements were clean came back as a placeholder. Four seconds later, with
    /// the window full of this fist's own gaps, the same pattern read as `S` at
    /// nought point nine eight.</para>
    /// <para>The letters were never in doubt. What was wrong was the scale the
    /// confidence was measured on, and a scale fitted partly to the detector's
    /// own flapping describes the detector rather than the sender.</para>
    /// </remarks>
    public const double ShortestGapMs = 25;

    /// <summary>How many gaps are needed before they can be clustered.</summary>
    /// <remarks>
    /// Ten. Fewer than that and a single stray gap moves a center, and there is
    /// no honest boundary to be had.
    /// </remarks>
    public const int LeastGaps = 10;

    /// <summary>How far apart neighbouring classes must sit, in log units.</summary>
    /// <remarks>
    /// About half as long again, which is the separation the streaming estimator
    /// already demanded of two classes and the same reasoning: below it, one
    /// spread of gaps is being cut into pieces rather than three groups being
    /// found. A textbook sender clears it easily at one, three and seven dits,
    /// and so does the bulletin at 40, 240 and 600 milliseconds.
    /// </remarks>
    public const double LeastSeparation = 0.405;

    /// <summary>How far above the middle class a lone top member may sit.</summary>
    /// <remarks>
    /// Twice the separation two classes have to clear anyway, so a gap something
    /// over twice as long as the class below it. A word gap at seven dits against
    /// a character gap at three does not come near it; a two second silence
    /// against a 680 millisecond word gap clears it easily.
    /// </remarks>
    public const double LoneOutlier = 2 * LeastSeparation;

    /// <summary>How many lone outliers may be dropped before giving up.</summary>
    /// <remarks>
    /// Three. Each one costs a real measurement, and a window needing more than a
    /// handful has nothing in it worth fitting.
    /// </remarks>
    public const int MostTrims = 3;

    /// <summary>How many refinement passes to make.</summary>
    /// <remarks>
    /// Twenty-four, which is far more than three well separated heaps need and
    /// cheap enough to run whenever a window settles. Fixed rather than
    /// convergence-tested so the cost is knowable (§8).
    /// </remarks>
    public const int Passes = 24;

    /// <summary>
    /// Fit three classes to these gaps, or answer that they do not have three.
    /// </summary>
    /// <param name="gapsMs">The gaps, in milliseconds. Reordered in place.</param>
    /// <param name="count">How many of them to read.</param>
    /// <returns>The classes, or null.</returns>
    /// <remarks>
    /// <para>**NULL IS A REAL ANSWER AND THE CALLER MUST EMIT NOTHING FOR IT.**
    /// Where the gaps do not separate, nobody knows where this sender puts the
    /// spaces, and a guessed boundary is a guess about where the words are. §0.0
    /// prefers silence and already said so.</para>
    /// <para>Fitted in log space, because a gap twice as long as another is the
    /// same distance apart whatever the sender's speed, which is what lets one
    /// rule serve a slow net and a fast contest alike.</para>
    /// </remarks>
    public static CwGapClasses? Fit(double[] gapsMs, int count)
    {
        ArgumentNullException.ThrowIfNull(gapsMs);

        var usable = 0;

        // Shorter than anybody can send is the gate flapping and not a silence
        // (see ShortestGapMs). It is dropped before the fit rather than trimmed
        // after it, because it spoils the class centres and not just their edges.
        for (var i = 0; i < count && i < gapsMs.Length; i++)
        {
            if (gapsMs[i] >= ShortestGapMs)
            {
                gapsMs[usable++] = gapsMs[i];
            }
        }

        if (usable < LeastGaps)
        {
            return null;
        }

        Array.Sort(gapsMs, 0, usable);

        for (var i = 0; i < usable; i++)
        {
            gapsMs[i] = Math.Log(gapsMs[i]);
        }

        // **SEEDED ON PERCENTILES, NOT ON THE ENDS**, and the difference is the
        // whole fit. Seeding at the smallest, middle and largest gap puts the
        // first center on whatever the shortest stray was and leaves it there:
        // the element and character heaps merge into one class, the boundary
        // between them lands inside the element gaps, and the transcript that
        // produces is nothing at all. Measured, not reasoned.
        //
        // A quarter, three quarters, nineteen twentieths. Element gaps are the
        // commonest by far, character gaps next, word gaps rarest, so those
        // three land one in each heap on any ordinary sending.
        double a = 0, b = 0, c = 0;
        int na = 0, nb = 0, nc = 0;

        // Set when the top class comes back empty, which is a sender who left no
        // word gaps rather than a fit that failed (HM-DEC-142).
        var noWordClass = false;

        // **A LONE GAP FAR ABOVE EVERYTHING ELSE IS A PAUSE, NOT A CLASS.** An
        // operator who stops for a couple of seconds between transmissions leaves
        // one silence several times longer than any word gap he sends, and a
        // three-class fit spends its whole top class on it: the word gaps then
        // have to share a class with the character gaps, the boundary between
        // them lands above both, and every space between words disappears.
        // Measured on a looping training signal at twelve words a minute —
        // element gaps 95, character 290, word 680, and one silence of 2000
        // between repeats — the boundary went from 444 to 903 milliseconds and
        // `CQ DE W1AW K` came back as `CQDEW1AW K`.
        //
        // So a top class of exactly one member sitting far clear of the middle
        // one is dropped and the fit taken again without it. It is still a word
        // gap when it is classified, by any boundary this can produce; what it
        // may not do is decide where that boundary goes.
        for (var trim = 0; ; trim++)
        {
            a = gapsMs[usable / 4];
            b = gapsMs[usable * 3 / 4];
            c = gapsMs[Math.Min(usable - 1, usable * 19 / 20)];


            for (var pass = 0; pass < Passes; pass++)
            {
                double sa = 0, sb = 0, sc = 0;
                na = nb = nc = 0;

                for (var i = 0; i < usable; i++)
                {
                    var v = gapsMs[i];
                    var da = Math.Abs(v - a);
                    var db = Math.Abs(v - b);
                    var dc = Math.Abs(v - c);

                    if (da <= db && da <= dc)
                    {
                        sa += v;
                        na++;
                    }
                    else if (db <= dc)
                    {
                        sb += v;
                        nb++;
                    }
                    else
                    {
                        sc += v;
                        nc++;
                    }
                }

                // **AN EMPTY ELEMENT OR CHARACTER CLASS IS THE MEASUREMENT
                // FAILING, AND IT IS STILL A REFUSAL.** Without those two there is
                // no way to tell one character from the next, and a transcript
                // built on that would be confident nonsense.
                if (na == 0 || nb == 0)
                {
                    return null;
                }

                // **AN EMPTY WORD CLASS IS THE SENDER, NOT THE FIT** (HM-DEC-142).
                // He sent a callsign without spaces. Refusing here is what left
                // two hundred and fifty-eight successfully read windows producing
                // an empty transcript on a fixture the reference reads perfectly,
                // and an empty box says nothing was sent — which is a belief
                // formed from the screen that is not true (§0.0).
                //
                // The centres carry on being fitted from the two heaps that do
                // exist; the third is dealt with after the loop, where the word
                // boundary is put out of reach rather than invented.
                if (nc == 0)
                {
                    noWordClass = true;
                    break;
                }

                a = sa / na;
                b = sb / nb;
                c = sc / nc;
            }

            if (noWordClass
                || nc > 1
                || trim >= MostTrims
                || usable - 1 < LeastGaps
                || c - b <= LoneOutlier)
            {
                break;
            }

            // Sorted ascending, so the single member of the top class is the
            // last value left.
            usable--;
        }

        // The two classes that decide where one character ends and the next
        // begins have to stand apart whether or not there is a third. This is
        // the check HM-DEC-142 makes the condition of shipping at all: without it
        // a callsign runs together and reads as confident nonsense, which is
        // worse than the silence it replaces.
        if (b - a < LeastSeparation)
        {
            return null;
        }

        if (noWordClass)
        {
            // **THE WORD BOUNDARY IS PUT OUT OF REACH RATHER THAN INVENTED**
            // (HM-DEC-142). No gap can exceed an infinite cut, so nothing is ever
            // classified as a word gap and the transcript comes out unspaced,
            // which asserts exactly what was measured: no word boundary anywhere.
            // Folding the widest character gaps into a word class instead would
            // place spaces nobody measured, which is the guess HM-DEC-115
            // forbids.
            return new CwGapClasses(
                Math.Exp(a),
                Math.Exp(b),
                0,
                Math.Exp((a + b) / 2),
                double.PositiveInfinity,
                na,
                nb,
                0);
        }

        if (c - b < LeastSeparation)
        {
            return null;
        }

        // A boundary sits between two centers, which in log space is halfway and
        // on the wire is their geometric mean.
        return new CwGapClasses(
            Math.Exp(a),
            Math.Exp(b),
            Math.Exp(c),
            Math.Exp((a + b) / 2),
            Math.Exp((b + c) / 2),
            na,
            nb,
            nc);
    }
}
