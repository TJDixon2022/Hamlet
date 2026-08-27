using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What the probabilistic decoder made of a stretch of audio.</summary>
/// <param name="LikelihoodRatio">
/// How much better the best reading explains the audio than "this is all noise",
/// per hop. **The null hypothesis is explicitly modelled and competes**, so an
/// empty band scores near nought here rather than being caught by a guard
/// (HM-DEC-120).
/// </param>
/// <param name="WordsPerMinute">
/// The speed hypothesis that won, which is not a measurement of anything. Nothing
/// here fits a speed from run lengths; a dozen speeds are tried and the audio
/// picks.
/// </param>
/// <param name="Text">What it read, or "" when the ratio is below the gate.</param>
/// <param name="ToneHz">The pitch it was given.</param>
/// <param name="EndsInsideCharacter">
/// True when the winning path's last segment is a mark or the gap between two
/// marks of one character, rather than the gap between characters or between
/// words. **This is the question HM-DEC-096 phase 3's interlock asks**, answered
/// by the path itself rather than inferred from anything: the decoder already
/// chose where every element and every character begins and ends, and the last
/// segment of that choice is what the newest audio is inside of. It is false
/// whenever the gate is closed, because nothing is being read and there is no
/// character to be part of the way through.
/// </param>
/// <param name="Characters">
/// The same reading, one entry per character, each carrying the hop it ended at.
/// Empty when the gate is closed. **The streaming path needs the times**: a
/// character old enough to be behind the decision delay is settled and one inside
/// it may still be revised, which is the whole point of deciding late.
/// </param>
public readonly record struct CwProbabilisticResult(
    double LikelihoodRatio,
    double WordsPerMinute,
    string Text,
    double ToneHz,
    IReadOnlyList<CwProbabilisticCharacter> Characters,
    bool EndsInsideCharacter = false)
{
    /// <summary>
    /// True when the winning speed sits at either end of the search, so it may
    /// be a limit rather than a measurement.
    /// </summary>
    /// <remarks>
    /// **A HYPOTHESIS AT THE EDGE OF A RANGE WINS BY DEFAULT RATHER THAN ON
    /// EVIDENCE** — there is nothing beyond it to lose to. On 2026-08-25 two
    /// operators measured 30.9 and 30.8 words a minute and Hamlet reported 32 for
    /// both, which was the top of its grid, and nothing on the sheet said so. A
    /// number that cannot be told from a ceiling is not a measurement (§0.0).
    /// </remarks>
    public bool SpeedIsAtTheEdge
        => Characters.Count > 0
           && (WordsPerMinute <= CwProbabilisticDecoder.SlowestWpm + 1e-9
               || WordsPerMinute >= CwProbabilisticDecoder.FastestWpm - 1e-9);

    /// <summary>Nothing measured.</summary>
    public static CwProbabilisticResult None { get; }
        = new(0, 0, "", 0, Array.Empty<CwProbabilisticCharacter>());
}

/// <summary>One character the decoder read, and where it ended.</summary>
/// <param name="Text">The letter, or a space for a word gap.</param>
/// <param name="Pattern">The dits and dahs behind it, or "" for a word gap.</param>
/// <param name="EndHop">Which hop of the window it ended at.</param>
/// <param name="MarginLlr">
/// How much better the winning reading was than the nearest alternative
/// arriving at the same place. Recorded and read by nothing.
/// </param>
/// <param name="SpanHops">
/// How many hops the character spans, from the start of its first mark to the
/// end of its last, so the ratio can be read per hop.
/// </param>
/// <param name="SpanLogLikelihoodRatio">
/// How much better this character's own span is explained by the keying the path
/// chose than by the key having been up throughout it.
/// </param>
/// <remarks>
/// <para>**A CHARACTER READ FROM A SIGNAL AND A CHARACTER MINTED FROM NOISE ARE
/// SEPARABLE BY THIS NUMBER, AND UNTIL NOW NOTHING RECORDED IT.** The window's
/// own likelihood ratio is an average over the whole window, so one letter read
/// out of a fade and one letter assembled out of the gaps between two other
/// stations carry the same figure, and the sidecar beside a wrong decode could
/// not say which it was holding.</para>
/// <para>**IT IS THE DATA TERM ONLY, AND THE GAPS INSIDE THE CHARACTER CANCEL
/// EXACTLY.** Both hypotheses say the key is up during an element gap, so those
/// hops contribute nothing to the difference and the whole quantity reduces to
/// the marks: for each one, the summed per-hop log-likelihood that the key was
/// down, less the summed log-likelihood that it was up. The Gaussian length
/// penalty is deliberately left out — it scores how well a segment's duration
/// matched the speed hypothesis, which is a statement about the clock rather
/// than about whether there was a signal there at all.</para>
/// <para>**LARGE AND POSITIVE MEANS THE MARKS STOOD ABOVE THE NOISE.** Near
/// zero, or negative, means the path found a letter in audio that all-key-up
/// explains as well or better, which is exactly HM-DEC-007's case: a wrong
/// decode with the evidence attached is a regression test.</para>
/// </remarks>
public readonly record struct CwProbabilisticCharacter(
    string Text,
    string Pattern,
    int EndHop,
    double SpanLogLikelihoodRatio = 0,
    int SpanHops = 0,
    double MarginLlr = double.NaN)
{
    /// <summary>
    /// How much better the winning reading was than the nearest alternative
    /// arriving at the same place.
    /// </summary>
    /// <remarks>
    /// <para>**NOTHING READS THIS AND THAT IS DELIBERATE.** It is recorded so
    /// that tomorrow's thresholds come from a real distribution rather than from
    /// a guess, which is the mistake every constant in this file has had to be
    /// walked back from at least once.</para>
    /// <para>**WHY THE QUANTITY BESIDE IT IS NOT ENOUGH.**
    /// <see cref="SpanLogLikelihoodRatio"/> scores a character against the key
    /// never having gone down, and on audio that is never silent the null is
    /// wrong: measured on the pile-up of 2026-08-26, characters carved out of
    /// continuous tone scored eight thousand to twenty-nine thousand against
    /// silence while the plausible tail scored forty-one to four hundred — the
    /// soup outscoring the copy a hundred to one. Against a *second-best
    /// reading* rather than against silence, a letter carved out of a continuous
    /// tone has an alternative that fits about as well, and the margin collapses
    /// toward nought.</para>
    /// <para>This tree has its own reason to want it: unit 1.11.10 recorded that
    /// the short-character bias needs a per-character expectation, and a margin
    /// against the runner-up is one that does not care how many elements a
    /// character has.</para>
    /// <para><see cref="double.NaN"/> where the path had no alternative to
    /// compare against, which is not the same as a margin of nought.</para>
    /// </remarks>
    public double MarginLlr { get; init; } = MarginLlr;
    /// <summary>
    /// The character's own evidence per hop, in the units
    /// <see cref="CwProbabilisticResult.LikelihoodRatio"/> is measured in.
    /// </summary>
    /// <remarks>
    /// <para>**AN ABSOLUTE SPAN RATIO IS MEANINGLESS ACROSS RECORDINGS AND THE
    /// CORPUS SAYS SO LOUDLY.** Unit 001 measured the medians: a character read
    /// correctly on `cw-2026-08-18-004507` scores about three thousand, and a
    /// character on `cw-2026-08-17-013347` scores eleven **billion**. The
    /// quantity is a sum of per-hop log-likelihoods and the per-hop difference
    /// scales with the squared ratio of signal amplitude to the noise estimate,
    /// which is taken from each recording's own envelope. A threshold in these
    /// units would be a threshold on how quiet the band was.</para>
    /// <para>**DIVIDING BY THE SPAN PUTS IT IN THE GATE'S OWN UNITS.** The window
    /// ratio is the whole window's margin over all-key-up divided by its hop
    /// count; this is one character's margin over all-key-up divided by its hop
    /// count. Same reference, same arithmetic, one character instead of a
    /// window, so the outer guard and the inner test can be read against each
    /// other rather than against two different scales.</para>
    /// <para>The whole span is the divisor and not just the marks. A character
    /// whose element gaps are long relative to its marks really does carry less
    /// evidence per hop, and the window ratio divides by its silence too.</para>
    /// </remarks>
    public double SpanMargin
        => SpanHops <= 0 ? 0 : SpanLogLikelihoodRatio / SpanHops;
}

/// <summary>
/// A segmental Viterbi CW decoder that never forms a threshold.
/// </summary>
/// <remarks>
/// <para>**THE OLD DECODER'S ARCHITECTURE WAS THE FAULT.** It thresholded the
/// envelope into hard key-down and key-up runs, fitted a speed by clustering
/// those run lengths, and picked its analysis width from the fitted speed. Every
/// stage depended on the one before and the evidence was discarded at the first
/// step, so nothing downstream could recover from a wrong commit. And it was a
/// loop with positive feedback: chatter shortens the fitted dit, a short dit
/// reads as a fast fist, a fast fist widens the bandwidth, more noise crosses the
/// threshold. Measured on this repository's own recordings, senders working near
/// fourteen words a minute fitted at twenty-two to fifty-six.</para>
/// <para>**THE THREE IDEAS, WHICH ARE THE WHOLE OF IT.** Every hop produces two
/// numbers, the log-likelihood the key is down and the log-likelihood it is up,
/// and nothing commits. Speed is an outer hypothesis rather than a measurement,
/// so the loop cannot exist. And element boundaries and character boundaries are
/// chosen together and late, by dynamic programming over whole elements, rather
/// than one gap at a time against a threshold.</para>
/// <para>These are E. L. Bell's ideas from 1977, reduced to something small.
/// Ported line for line from `tools/reference-decoder/reference_decoder.py`,
/// which is in this repository so that the port has an implementation to be
/// checked against rather than a description.</para>
/// <para>**AND SILENCE FALLS OUT RATHER THAN BEING BOLTED ON.** "The whole
/// stretch is noise" is a competing hypothesis with a score of its own, so on a
/// recording holding no station it wins and there is nothing to emit. That is
/// HM-DEC-120 by construction; it is still tested rather than assumed.</para>
/// </remarks>
public static class CwProbabilisticDecoder
{
    /// <summary>
    /// The log-likelihood ratio per hop below which nothing is emitted from a
    /// window at all.
    /// </summary>
    /// <remarks>
    /// <para>**RE-EXPRESSED 2026-08-24, IN THE UNITS OF THE CORRECTED SCALE.**
    /// Fifteen was calibrated when the noise scale was 0.455 sigma, which
    /// inflated every quadratic term about four and eight tenths times. Unit
    /// 1.11.4 corrected the scale by identity and the model got better at
    /// reading while this number stayed where it was, so a bar meant for one set
    /// of units was applied to another: four recordings that read the day before
    /// read nothing.</para>
    /// <para>**IT IS THE OUTER SILENCE GUARD AND NOT THE EMIT DECISION.** That
    /// belongs to the character and has since unit 1.11.3
    /// (<see cref="CharacterMargin"/>). This asks only whether there is anything
    /// in this window at all.</para>
    /// <para>**WHERE THE NUMBER COMES FROM.** Every window ratio the streaming
    /// path produces across the corpus, measured 2026-08-24 on the instrument
    /// that actually gates. The guard is applied per window, so a whole-file
    /// figure is the wrong measurement for it — unit 1.11.4's published table
    /// was whole-file and is not what a recording meets.</para>
    /// <list type="table">
    /// <item><description>`cw-2026-08-20-014854`, holding nothing: 55 windows,
    /// **highest 0.840**</description></item>
    /// <item><description>`cw-2026-08-20-014935`, holding nothing: 55 windows,
    /// highest 0.115</description></item>
    /// <item><description>`cw-2026-08-24-012403`, holding `DE KD0UN KD0UN K`:
    /// **highest 1.684**, three quarters of its windows under 1.156</description></item>
    /// <item><description>`cw-2026-08-18-004507`, the ARRL bulletin: lowest
    /// 4.271</description></item>
    /// <item><description>`cw-2026-08-17-134712`, holding `N4L`: highest
    /// 13.226</description></item>
    /// </list>
    /// <para>**THE GAP IS 0.840 TO 1.684 AND THIS SITS IN IT.** Above every
    /// window either empty capture produces, so the property HM-DEC-120 protects
    /// is held with room; below the best windows of the weakest recording that
    /// holds a station, so the signal this work was commissioned to recover is
    /// admitted.</para>
    /// <para>**IT IS 1.40 RATHER THAN THE GAP'S MIDPOINT, AND AN EXISTING
    /// ASSERTION IS WHY.** `ARecordingWithNoStationInItSaysNothing` requires an
    /// empty band to score under half the guard, which is a standing claim that
    /// the separation is comfortable rather than merely correct. On
    /// `cw-2026-08-20-014854`'s whole-file ratio of 0.65 that needs a guard above
    /// 1.30. The midpoint, 1.25, held the silence property and failed that
    /// assertion; 1.40 satisfies both and reads the same 84.2 % of
    /// `cw-2026-08-24-012403` with two fewer characters invented. **Choosing
    /// inside a measured gap to respect a constraint somebody already ruled on is
    /// not fitting a number to a fixture.**</para>
    /// <para>**THE GAP IS NARROW AND THAT IS SAID RATHER THAN HIDDEN.** Two to
    /// one, against the five hundred to one the old units flattered. A recording
    /// holding no station whose noise ran a little hotter than
    /// `cw-2026-08-20-014854`'s would cross it. What stands behind the guard is
    /// the per-character margin, which is where the emit decision lives.</para>
    /// </remarks>
    public const double Gate = 1.40;

    /// <summary>How wide the envelope's own filter is, in hertz.</summary>
    /// <remarks>
    /// Sixty. **It is not chosen from any speed and nothing measured decides it**,
    /// which is the point: the old decoder's bandwidth came from its own fitted
    /// speed and that was the loop. A dit at forty words a minute is thirty
    /// milliseconds, so sixty hertz passes every element anybody sends.
    /// </remarks>
    public const double BandwidthHz = 60.0;

    /// <summary>
    /// How much evidence per hop a character must carry, over the key never
    /// having gone down across its own span, before it prints as a letter.
    /// </summary>
    /// <remarks>
    /// <para>**THE EMIT DECISION IS PER CHARACTER, AND THE WINDOW RATIO IS ONLY
    /// AN OUTER SILENCE GUARD.** <see cref="Gate"/> asks whether a whole window
    /// averaged better than silence, which is a question about a stretch of band
    /// and not about a letter. Measured on this repository's own corpus it is
    /// **anti-correlated with correctness**: `cw-2026-08-17-134712`, which
    /// carries an adjudicated `N4L`, scores 4.64, while `cw-2026-08-20-014854`,
    /// which an independent sweep says holds no keying at all, scores 7.98. The
    /// empty band beats the station.</para>
    /// <para>**NOUGHT WAS THE ONE VALUE THAT WAS NOT A TUNED THRESHOLD, AND
    /// NOUGHT WAS NOT ENOUGH.** The quantity is a character's own evidence
    /// measured against the key never having gone down across its span, so nought
    /// is the point where the two explanations are equally good, and below it
    /// silence explains that stretch of audio *better* than the letter the path
    /// chose. That reasoning still holds and it is why nought was ruled. What it
    /// misses is that a letter which beats silence by a whisker is not thereby a
    /// letter somebody sent: on a calling frequency most of the file is silence,
    /// and characters scoring nine tenths and one and eight tenths were reaching
    /// the screen as `E` beside real characters scoring in the thousands.</para>
    /// <para>**ONE, MEASURED AGAINST THE ONLY CONSTRAINT THAT BINDS IT**
    /// (<see cref="WeakestAdjudicatedCharacterMargin"/>). The weakest character on
    /// any recording holding words anybody has adjudicated or corroborated is
    /// 1.083, on `cw-2026-08-24-012403`. One sits below it with eight per cent of
    /// room and is the largest round number that does. Measured over the whole
    /// corpus it removes eighty characters, **and every one of them comes from a
    /// recording holding no adjudicated words**: the pileups, the low-duty
    /// captures, and the trailing run of `E`s on `cw-2026-08-17-134712` that
    /// follows the `N4L` rather than belonging to it. The four recordings holding
    /// content anybody has checked lose nothing.</para>
    /// <para>**A HIGHER FLOOR IS NOT AVAILABLE ON THIS QUANTITY IN ANY
    /// NORMALISATION, AND THE REASON IS WORTH MORE THAN THE NUMBER.** On
    /// `cw-2026-08-17-013347` the adjudicated `VA3VRR` scores between 1.5 and 6.5
    /// while the ninety-six characters of soup preceding it score between ten
    /// million and three hundred million. **The soup outranks the callsign by
    /// seven to eight orders of magnitude on the very quantity a gate would sort
    /// by**, so on that recording every threshold removes the station before it
    /// removes a single stranger. Dividing by the window's own likelihood ratio
    /// was measured and makes it worse rather than better: it cuts `VA3VRR` whole
    /// at a normalised floor of 0.1 and keeps every character of the soup.
    /// Dividing by a per-element-count median was measured and moves the binding
    /// constraint not at all, `cw-2026-08-24-012403`'s weakest character being
    /// binding in that form too at 0.1008. **Span evidence is not monotone in
    /// correctness across recordings**, which is the same anti-correlation the
    /// paragraph below records between recordings, found again inside one.</para>
    /// <para>**WHAT THE FLOOR THEREFORE IS AND IS NOT.** It is a floor under the
    /// weakest thing anybody has confirmed, and it removes characters that even a
    /// generous reading cannot separate from silence. It is not a soup filter, and
    /// nothing here claims that what survives it was sent.</para>
    /// <para>**AND IT MARKS RATHER THAN DELETES.** A character below the floor
    /// renders as HM-DEC-048's placeholder, because something did sound there and
    /// a shorter tidy word is a worse lie than a visible gap. What changes on a
    /// quiet frequency is that the operator sees the decoder failing to read
    /// rather than a page of confident `E`s.</para>
    /// <para>**A HIGHER MARGIN WAS DERIVED, TRIED, AND MEASURED WRONG.** Read on
    /// whole files there is a clean gap: `cw-2026-08-18-004507` reads with its
    /// weakest character at 49.8 while `cw-2026-08-20-014854`, which holds no
    /// keying at all, tops out at 42.5. Forty-six sits in that gap and silences
    /// both empty captures on their own characters. **It does not survive the
    /// streaming path**, which is what production runs: there the same capture's
    /// weakest real character is **3.1**, and forty-six marks letters of
    /// `HANDLING` and costs `VA3VRR` on `cw-2026-08-17-013347`, an adjudicated
    /// reading. A margin taken through one instrument is not a fact about
    /// another (HM-DEC-119's own lesson).</para>
    /// <para>**WHY THE TWO DISAGREE**: the whole-file read estimates its noise
    /// scale once over the entire recording, and the streaming path re-estimates
    /// it every window from twelve seconds of audio, so the same character is
    /// scored against two different noise floors.</para>
    /// <para>**AND ON THE STREAMING PATH THERE IS NO GAP TO DERIVE FROM**, because
    /// both empty captures emit nothing there at all: the window guard refuses
    /// every one of their windows, so they contribute no characters to compare
    /// against. That is this unit's finding for task 5 and is reported rather
    /// than papered over with a number.</para>
    /// </remarks>
    public const double CharacterMargin = 1.0;

    /// <summary>
    /// The weakest character on any recording holding words somebody has
    /// adjudicated or corroborated, in the units <see cref="CharacterMargin"/>
    /// is measured in.
    /// </summary>
    /// <remarks>
    /// <para>**IT IS 1.047, AND IT IS THE ONLY THING THAT DECIDES HOW HIGH THE
    /// CHARACTER FLOOR MAY GO.** Measured 2026-08-25 over every capture in the
    /// tree: `cw-2026-08-24-012403`, which holds `DE KD0UN KD0UN K`, produces
    /// nineteen characters and its weakest carries 1.047. Its whole window sits
    /// at 1.43 to 1.69 against a <see cref="Gate"/> of 1.40, so the recording
    /// barely clears the outer silence guard at all and every character in it is
    /// weak. Next weakest across the protected set is `cw-2026-08-17-013347`'s
    /// `VA3VRR` at 1.480, then `cw-2026-08-18-004507`'s bulletin at 1.635.</para>
    /// <para>**THE TWO DRIVE PATHS DISAGREE ABOUT THAT CHARACTER BY THREE AND A
    /// HALF PER CENT, AND THE LOWER READING IS THE ONE RECORDED.** Fed the
    /// recording hop by hop it scores 1.047; fed the same recording through
    /// `Listen` and a buffered source it scores 1.083. A constant has to be safe
    /// on whichever path production takes, so the number here is the smaller.
    /// **That the two disagree at all is a finding rather than a rounding**, and
    /// it is not confined to a margin: on `cw-2026-08-22-032113` the two paths
    /// track different notes, 650 Hz against 500 Hz.</para>
    /// <para>**THE CHARACTER THAT BINDS IT IS PROBABLY NOT A REAL ONE, AND THAT
    /// IS SAID RATHER THAN USED.** It is a lone `E` sitting between the second
    /// `KD0UN` and the closing `K`, where the reading `DE KD0UN KD0UN K` has no
    /// letter at all. Nobody has adjudicated it, so it is treated as real and
    /// the floor stays under it (§12.5). If it is ever ruled a stranger, the
    /// binding constraint becomes `VA3VRR` at 1.480 and this floor can rise.</para>
    /// <para>A floor above this number costs a real word, which is the one thing
    /// a gate against soup may not do. It is recorded rather than left implicit
    /// so that the next session raising the floor has to argue with a
    /// measurement rather than with a comment.</para>
    /// </remarks>
    public const double WeakestAdjudicatedCharacterMargin = 1.047;

    /// <summary>
    /// How wide the envelope's integrator is, in hertz of equivalent noise
    /// bandwidth.
    /// </summary>
    /// <remarks>
    /// <para>**FORTY-FIVE, AND IT IS WHAT A HANN OF THE BOXCAR'S OWN MAIN-LOBE
    /// WIDTH COMES TO.** The number is not chosen from a speed and nothing about
    /// a fist decides it, which is the same reasoning
    /// <see cref="BandwidthHz"/> carried: the old decoder took its bandwidth
    /// from its own fitted speed and that was the loop this architecture exists
    /// to break.</para>
    /// <para>**WHY IT IS NARROWER THAN THE SIXTY IT REPLACES, WITHOUT ANYBODY
    /// CHOOSING TO NARROW IT.** A Hann taper has a main lobe twice the width of
    /// a boxcar of the same length, so matching the boxcar's main lobe means
    /// doubling the length, and a Hann of length N has an equivalent noise
    /// bandwidth of 1.5 fs/N against the boxcar's fs/N. Doubling the length and
    /// multiplying by one and a half lands on three quarters of sixty. **The
    /// main lobe is what carries the wanted station**, so it is the figure held
    /// constant and the noise bandwidth is what follows.</para>
    /// <para>**MEASURED RATHER THAN ARGUED, AND THE ORDERED MEASUREMENT RETURNED
    /// A TIE.** The trade at 60, 45, 30 and 20 hertz is in
    /// `ANALYSIS-cw-integrator-bandwidth-2026-08-23.md`. Across the whole swept
    /// grid — five offsets from 40 hertz out, three levels down from equal —
    /// **every width reads the wanted station whole**, so rejection did not
    /// choose this number and nothing about the two-station case did.</para>
    /// <para>**WHAT NARROWING COSTS, ALSO MEASURED.** Sensitivity: nothing, down
    /// to nought decibels at every width. A fast fist: nothing, to thirty-five
    /// words a minute at every width, including a seventy-five millisecond
    /// integrator on a thirty-four millisecond dit, because a segmental decoder
    /// scores a span rather than thresholding a level and a smeared envelope
    /// keeps its timing. **The gate's own margin: real, and the binding one.**
    /// The empty band on `cw-2026-08-20-014854` climbs 6.6, 8.0, 9.3, 10.0
    /// against a gate of fifteen. Silence holds at every width, so HM-DEC-120 is
    /// not traded, but the room under the gate halves. And the corpus: 013347
    /// falls from eighty-three characters to forty-nine at twenty hertz with its
    /// E-share rising, which is plainly worse.</para>
    /// <para>**THIRTY IS THE LIVE ALTERNATIVE AND IT IS A TRADE RATHER THAN A
    /// DEDUCTION** (§12.1). Below about thirty hertz of separation the narrower
    /// filters win outright, and thirty would buy that at no measured cost to a
    /// fast fist. Those rows are not in the swept grid; they were added by the
    /// session that wrote them, and fitting a production constant to a fixture
    /// the same session invented is the shape of the failure §12.5 exists to
    /// stop. So the principled figure stands and the trade is handed back.</para>
    /// </remarks>
    public const double IntegratorBandwidthHz = 45.0;

    /// <summary>What shape the envelope's integrator is, for the record.</summary>
    /// <remarks>
    /// **A TABLE THAT NAMES ITS OWN INSTRUMENT CANNOT BE MISFILED** (§0.0.1). The
    /// front end is being measured before and after a change to it, and two
    /// tables that look alike and were taken through different filters are worth
    /// less than one table, because nobody can tell afterwards which was which.
    /// </remarks>
    public const string IntegratorName = "Hann";

    /// <summary>
    /// The quarter point of a Rayleigh envelope, in units of its own scale.
    /// </summary>
    /// <remarks>
    /// **AN IDENTITY, NOT A FACTOR**, and it is 0.758528. The scale this
    /// replaced was the quarter point times six tenths, which works out at
    /// **0.455 sigma**: two and a fifth times too small, so every quadratic term
    /// was inflated about four and eight tenths times. Nothing here is to be
    /// re-tuned. If it is wrong then the derivation is wrong.
    /// </remarks>
    public const double RayleighQuarterPoint = 0.758527616440932;

    /// <summary>
    /// How much audio the noise scale and the keyed level are estimated over,
    /// in seconds.
    /// </summary>
    /// <remarks>
    /// <para>**THE SAME SPAN ON BOTH PATHS, WHICH IS THE WHOLE POINT.** The
    /// offline read handed <c>LogLikelihoods</c> a whole recording and the
    /// streaming path handed it a twelve second window, so one character was
    /// scored against two different noise floors and a margin measured on one
    /// path was not a fact about the other. Unit 1.11.3 found a clean gap at 46
    /// on whole files that cost `VA3VRR` in streaming, which is that fault
    /// exactly (HM-DEC-119's own lesson).</para>
    /// <para>**PROVISIONAL, AND MEASURED RATHER THAN ASSUMED.** Two and a half
    /// seconds holds roughly twenty elements at eighteen words a minute, which
    /// is enough for a quarter point to mean something and short enough to
    /// follow a fade. What one and a half and four seconds do to the same corpus
    /// is reported beside it.</para>
    /// </remarks>
    public const double NoiseSpanSeconds = 2.5;

    /// <summary>How often the envelope is sampled, in milliseconds.</summary>
    public const double HopMilliseconds = 5.0;

    /// <summary>The slowest speed hypothesis tried.</summary>
    /// <remarks>
    /// **EIGHT, BECAUSE A GRID THAT STOPS AT TEN CANNOT FIT A TEN.** A hypothesis
    /// at the very edge of the range wins by default rather than on evidence:
    /// there is nothing below it to lose to, so a sender slower than the floor is
    /// fitted at the floor whatever he is actually doing. The operator this
    /// application is for works people sending eight to twelve on a straight key,
    /// which is the slowest thing on the band and the easiest to copy by ear.
    /// </remarks>
    public const double SlowestWpm = 8;

    /// <summary>The fastest speed hypothesis tried.</summary>
    /// <remarks>
    /// <para>**FORTY, BECAUSE A MACHINE SENDER IS THE EASIEST THING ON THE BAND
    /// AND HAMLET COULD NOT FIT ONE.** A station running thirty-five or forty is
    /// almost always a program sending perfect timing, which is the least
    /// demanding audio a decoder ever sees, and the old ceiling of thirty-two put
    /// it outside the grid.</para>
    /// <para>**THE REMARKS SAID FORTY AND THE CONSTANT SAID THIRTY-TWO FOR TWO
    /// DAYS** (HM-OPEN-058, logged 2026-08-23 and parked in every unit since).
    /// What settled it is a pair of live captures rather than the contradiction:
    /// on the evening of 2026-08-25 two First Class CW Operators' Club members
    /// measured 30.9 and 30.8 words a minute and Hamlet reported **32 for both**,
    /// which is the top of its own search. One notch faster and the grid could
    /// not have followed them, and the failure would have looked like a decoder
    /// fault rather than a range limit.</para>
    /// <para>**AND A WINNER AT EITHER END IS NOW SAID OUT LOUD**, so a range
    /// limit is never again mistaken for a measurement
    /// (<see cref="CwProbabilisticResult.SpeedIsAtTheEdge"/>).</para>
    /// </remarks>
    public const double FastestWpm = 40;

    /// <summary>How far apart the speed hypotheses sit.</summary>
    /// <remarks>
    /// <para>**ONE, BECAUSE HALF THE SPEEDS PEOPLE SEND AT WERE NOT ON THE
    /// GRID.** Ordinary operators work at thirteen, fifteen, seventeen, nineteen
    /// and twenty-one words a minute, and a step of two reached none of the odd
    /// ones. A hypothesis a quarter short of the truth stretches every gap
    /// measured against it.</para>
    /// <para>**IT IS NOT WHY CHARACTERS BREAK, AND THAT WAS MEASURED**: on
    /// `cw-2026-08-18-004507` the likelihood is 32.3 to 32.4 at every speed from
    /// eleven to thirty-two, so the objective is flat in speed and which
    /// hypothesis wins is nearly arbitrary; elements per character stays between
    /// 2.33 and 2.50 across that whole range.</para>
    /// <para>**AND A STEP OF ONE WAS BUILT AND MEASURED AND DOES NOT SHIP.** With
    /// a flat objective, more hypotheses is more ways to be wrong: the sensitivity
    /// fixture, which sends at eighteen, was won by nine words a minute and the
    /// sweep began inventing 0.22 of the message at eighteen decibels where it had
    /// invented nothing. It also costs 22.7 per cent of real time against 13.5.
    /// **HM-DEC-120 is not traded for reaching the odd speeds**, so the step stays
    /// at two until the objective can tell speeds apart.</para>
    /// </remarks>
    public const double WpmStep = 2;

    /// <summary>One element kind the model knows about.</summary>
    /// <param name="Units">How many dit-lengths it is expected to last.</param>
    /// <param name="IsKeyDown">Whether the key is down for it.</param>
    /// <param name="Token">What it contributes to a character, or "".</param>
    private readonly record struct Kind(int Units, bool IsKeyDown, string Token);

    /// <summary>
    /// Dit, dah, the gap inside a character, the gap between characters, and the
    /// gap between words.
    /// </summary>
    private static readonly Kind[] Kinds =
    {
        new(1, true, "."),
        new(3, true, "-"),
        new(1, false, ""),
        new(3, false, "|"),
        new(7, false, " "),
    };

    /// <summary>How far from its expected length a segment may stray, as a share.</summary>
    /// <remarks>
    /// Less than half and more than twice, which is deliberately loose: a real
    /// fist sends a dah anywhere from two and a half to four and a quarter dits
    /// (HM-DEC-144, HM-DEC-145), and the Gaussian penalty below does the work of
    /// preferring the middle rather than a bound doing it.
    /// </remarks>
    private const double ShortestShare = 0.45;

    private const double LongestShare = 2.2;

    /// <summary>
    /// How wide the penalty on a segment's length is, as a share of the log
    /// ratio between what arrived and what was expected.
    /// </summary>
    /// <remarks>
    /// <para>**THE SCATTER IS SCORED AS A RATIO AND NOT AS A DIFFERENCE**, ruled
    /// by Tim on 2026-08-22. Timing error in a hand-sent fist is multiplicative:
    /// a sender who runs a fifth long runs a fifth long on dits, dahs and gaps
    /// alike, which is a property of hands rather than of textbooks. So the cost
    /// is `ln(span / want) / 0.35`, and **both crossovers land at the geometric
    /// mean, 1.73 units** — between a dit and a dah, and between the gap inside a
    /// character and the gap between two.</para>
    /// <para>**WHAT IT REPLACED, AND WHY THAT WAS WRONG.** The cost used to be
    /// `(span − want) / (want × 0.35)`, a share of each kind's own expected
    /// length, so the gap between characters was allowed three times the scatter
    /// of the gap inside one and the word gap seven times. **The two costs
    /// crossed at one and a half units rather than at two**: at a gap of exactly
    /// two units the element reading cost 4.08 and the character reading 0.45,
    /// with an identical evidence term, so nothing argued back. Every gap longer
    /// than one and a half dits was called a character gap, and a decoder that
    /// breaks between the elements of one letter emits E, T and I.</para>
    /// <para>**MEASURED, AND BOTH HALVES ARE TRUE.** It reads `2 MOVIES A DAY`
    /// where it read `2 IOVI ES`, `EACH` as one word, keeps `N4LQ K` on the
    /// capture HM-DEC-144 adjudicated as `N4L` and brings `VRR VA` out of the one
    /// HM-DEC-145 adjudicated as `VA3VRR`. **And elements per character is
    /// unmoved in aggregate.**</para>
    /// <para>**ONE OTHER MODEL WAS BUILT AND MEASURED AND REJECTED**: scaling the
    /// scatter by the dit rather than by the segment, which moves both crossovers
    /// to two units and costs five of seven recordings their text, because the
    /// dahs of a real fist arrive at two to two and a half units and then read as
    /// dits.</para>
    /// <para>`tools/reference-decoder/reference_decoder.py` carries the same
    /// change, so `ItReadsWhatTheReferenceReads` still means what it meant.</para>
    /// </remarks>
    private const double LengthToleranceShare = 0.35;

    /// <summary>How many samples a Hann integrator of a given width spans.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="bandwidthHz">The wanted equivalent noise bandwidth.</param>
    /// <returns>The window length in samples, always odd.</returns>
    /// <remarks>
    /// **A HANN OF LENGTH N HAS AN EQUIVALENT NOISE BANDWIDTH OF 1.5 fs/N**, so
    /// the length follows from the width rather than the other way round. Odd, so
    /// the taper has a single centre sample and the centred and trailing forms
    /// differ by exactly half a window rather than by half a window and a half a
    /// sample.
    /// </remarks>
    public static int IntegratorWindow(int sampleRate, double bandwidthHz)
    {
        var length = Math.Max(3, (int)Math.Round(1.5 * sampleRate / bandwidthHz));

        return length % 2 == 0 ? length + 1 : length;
    }

    /// <summary>The integrator's taper, one weight per sample.</summary>
    /// <param name="length">How many samples it spans.</param>
    /// <returns>The weights.</returns>
    /// <remarks>
    /// <para>**A BOXCAR'S FIRST SIDELOBE IS THIRTEEN DECIBELS DOWN AND A HANN'S
    /// IS THIRTY-ONE**, for one multiply per sample. A station a hundred hertz
    /// away entered the boxcar's envelope at minus sixteen decibels, which is
    /// attenuated rather than rejected; through this it enters at minus
    /// forty-two.</para>
    /// <para>**THE COST IS TIME AND IT IS REAL.** Matching the main lobe doubles
    /// the window, so the integrator spans thirty-three milliseconds where the
    /// boxcar spanned seventeen. At thirty words a minute a dit is forty
    /// milliseconds, and an integrator most of a dit long rounds the top of every
    /// short mark. That is measured rather than argued (task 4).</para>
    /// <para>The weights are not normalised here; the caller divides by their
    /// sum, which is what makes a magnitude comparable between two window
    /// shapes.</para>
    /// </remarks>
    public static double[] IntegratorTaper(int length)
    {
        var taper = new double[Math.Max(1, length)];

        if (taper.Length == 1)
        {
            taper[0] = 1;

            return taper;
        }

        for (var n = 0; n < taper.Length; n++)
        {
            taper[n] = 0.5 * (1 - Math.Cos(2 * Math.PI * n / (taper.Length - 1)));
        }

        return taper;
    }

    /// <summary>Read a stretch of audio at a known pitch.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">Where the station is, from the tone tracker.</param>
    /// <returns>What it read, and how much better than noise that reading is.</returns>
    public static CwProbabilisticResult Decode(MonoAudio audio, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var envelope = Envelope(audio.Samples, audio.SampleRate, toneHz);

        return Decode(envelope, toneHz);
    }

    /// <summary>Read an envelope that has already been taken.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// Separate so the streaming path can keep one rolling envelope rather than
    /// re-mixing the same audio for every window.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope, double toneHz)
        => Decode(envelope, toneHz, atWordsPerMinute: null);

    /// <summary>Read an envelope, optionally at one imposed speed.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="atWordsPerMinute">
    /// One speed to read at, or null to search the grid.
    /// </param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// **THE IMPOSED SPEED IS FOR ASKING QUESTIONS, NOT FOR DECODING.** Nothing
    /// in the application passes it. It exists so a measurement can separate two
    /// faults that look alike: a speed the grid cannot reach, and a gap model that
    /// breaks characters wherever the speed lands.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope, double toneHz, double? atWordsPerMinute)
        => Decode(envelope, toneHz, atWordsPerMinute, gapMilliseconds: null);

    /// <summary>Read an envelope, at one speed and with this sender's own gaps.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="atWordsPerMinute">One speed to read at, or null to search.</param>
    /// <param name="gapMilliseconds">
    /// How long this sender's gap inside a character, between characters and
    /// between words actually are, or null to take them as one, three and seven
    /// units.
    /// </param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// **THE GAP LENGTHS COME FROM THE GAPS OR THEY COME FROM THE UNIT, AND THE
    /// SECOND COUPLES TWO FAILURES INTO ONE.** With the expected lengths taken as
    /// multiples of the unit, the cost of reading a gap as a letter break crosses
    /// the cost of reading it as an element gap at the geometric mean of one and
    /// three units, so **a unit that is wrong moves every letter boundary with
    /// it**. Handing the measured lengths in puts that crossing at the geometric
    /// mean of two things the sender actually did, which on every capture here
    /// lands in an empty stretch of that sender's own gap distribution.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope,
        double toneHz,
        double? atWordsPerMinute,
        IReadOnlyList<double>? gapMilliseconds)
        => Decode(
            envelope, toneHz, atWordsPerMinute, gapMilliseconds, ungated: false);

    /// <summary>Read an envelope, returning what the path spelled whatever it scored.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <returns>What the path spelled, with no window gate applied.</returns>
    /// <remarks>
    /// **FOR MEASUREMENT, AND NOTHING IN THE APPLICATION CALLS IT.** The question
    /// "what would this audio have emitted if the gate had let it through" cannot
    /// be asked of a decoder that returns an empty list when the gate refuses, and
    /// it is exactly the question a gate's calibration turns on (§0.0.1).
    /// </remarks>
    public static CwProbabilisticResult DecodeUngated(
        IReadOnlyList<double> envelope, double toneHz)
        => Decode(envelope, toneHz, null, null, ungated: true);

    /// <summary>Read an envelope, with the gate and the estimation span open.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="ungated">True to return what the path spelled whatever it scored.</param>
    /// <param name="noiseSpanSeconds">What the noise scale is estimated over.</param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// **FOR MEASUREMENT, AND NOTHING IN THE APPLICATION CALLS IT.** Two
    /// questions this unit has to answer cannot be asked any other way: what an
    /// empty band's characters would score if the guard let them through, and
    /// how much every figure moves when the estimation span moves.
    /// </remarks>
    public static CwProbabilisticResult DecodeForMeasurement(
        IReadOnlyList<double> envelope,
        double toneHz,
        bool ungated,
        double noiseSpanSeconds)
        => Decode(
            envelope, toneHz, null, null, ungated,
            jointly: false, noiseSpanSeconds);

    /// <summary>Read an envelope, with the joint cutter deciding the cuts.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="atWordsPerMinute">A speed to hold, or null to fit one.</param>
    /// <param name="gapMilliseconds">This sender's three gap classes, or null.</param>
    /// <param name="jointly">Whether <see cref="CwJointCutter"/> decides the cuts.</param>
    /// <returns>What it read.</returns>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope,
        double toneHz,
        double? atWordsPerMinute,
        IReadOnlyList<double>? gapMilliseconds,
        bool jointly)
        => Decode(
            envelope, toneHz, atWordsPerMinute, gapMilliseconds,
            ungated: false, jointly);

    private static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope,
        double toneHz,
        double? atWordsPerMinute,
        IReadOnlyList<double>? gapMilliseconds,
        bool ungated,
        bool jointly = false,
        double noiseSpanSeconds = NoiseSpanSeconds)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (envelope.Count < 8)
        {
            return CwProbabilisticResult.None;
        }

        var (keyDown, keyUp) = LogLikelihoods(envelope, noiseSpanSeconds);
        var nothingAtAll = 0.0;

        foreach (var value in keyUp)
        {
            nothingAtAll += value;
        }

        var bestScore = double.NegativeInfinity;
        var bestWpm = 0.0;
        var bestLastKind = -1;
        IReadOnlyList<CwProbabilisticCharacter> bestCharacters =
            Array.Empty<CwProbabilisticCharacter>();

        var from = atWordsPerMinute ?? SlowestWpm;
        var to = atWordsPerMinute ?? FastestWpm;

        for (var wpm = from; wpm <= to + 1e-9; wpm += WpmStep)
        {
            var (score, characters, lastKind) =
                DecodeAt(
                    envelope.Count, wpm, keyDown, keyUp, gapMilliseconds,
                    jointly);

            if (score > bestScore)
            {
                bestScore = score;
                bestWpm = wpm;
                bestCharacters = characters;
                bestLastKind = lastKind;
            }
        }

        // **WHERE THE PATH ENDS IS WHERE THE AUDIO IS.** Kinds 0 and 1 are the
        // mark, kind 2 is the gap inside a character; 3 and 4 are the gaps
        // between characters and between words, which is where the tracker is
        // free to move.
        var insideCharacter = bestLastKind is >= 0 and <= 2;

        var ratio = (bestScore - nothingAtAll) / envelope.Count;

        if (ungated)
        {
            return new CwProbabilisticResult(
                ratio, bestWpm, string.Concat(bestCharacters.Select(c => c.Text)),
                toneHz, bestCharacters, insideCharacter);
        }

        if (ratio < Gate)
        {
            // **THE NULL HYPOTHESIS WON FOR THE WHOLE STRETCH.** This is now the
            // outer silence guard and nothing else: it asks whether there is
            // anything here at all, and the decision about each letter is made
            // below, on that letter's own evidence (§0.0, HM-DEC-120).
            return new CwProbabilisticResult(
                ratio, bestWpm, "", toneHz, Array.Empty<CwProbabilisticCharacter>());
        }

        // **AND NOW EACH CHARACTER ANSWERS FOR ITSELF.** A window that averaged
        // well can still contain letters the path assembled out of the gaps, and
        // the window ratio cannot tell them apart because every character in a
        // window carries the same one. A character that cannot clear its own
        // margin is marked rather than dropped: something was heard there and
        // could not be resolved, which is exactly what the placeholder is for
        // (§0.0, HM-DEC-048).
        var judged = Marked(bestCharacters);

        return new CwProbabilisticResult(
            ratio,
            bestWpm,
            string.Concat(judged.Select(c => c.Text)),
            toneHz,
            judged,
            insideCharacter);
    }

    /// <summary>
    /// Quadrature mixdown to the tone, smoothed, and sampled every hop.
    /// </summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>One magnitude per hop.</returns>
    /// <remarks>
    /// A boxcar over the quadrature arms, which is what a filter of this
    /// bandwidth amounts to. Running sums, so the whole thing is one pass
    /// whatever the window length.
    /// </remarks>
    public static double[] Envelope(
        IReadOnlyList<float> samples, int sampleRate, double toneHz)
        => Envelope(samples, sampleRate, toneHz, IntegratorBandwidthHz);

    /// <summary>Read a stretch of audio at a known pitch and a stated width.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <param name="bandwidthHz">The integrator's equivalent noise bandwidth.</param>
    /// <returns>One magnitude per hop.</returns>
    /// <remarks>
    /// <para>**THE WIDTH IS A PARAMETER HERE AND A CONSTANT IN PRODUCTION.** It
    /// is open so the trade between rejecting a competing station and rounding
    /// the top of a fast dit can be swept and tabulated; nothing in the
    /// application passes anything but
    /// <see cref="IntegratorBandwidthHz"/>.</para>
    /// <para>**NO PREFIX SUMS, BECAUSE A TAPER IS NOT A RUNNING SUM.** The
    /// boxcar this replaced could be two subtractions per hop; a weighted window
    /// is a multiply-accumulate over its own length. It runs once per hop rather
    /// than once per sample, so the cost is the window length times the hop
    /// count and not the sample count squared.</para>
    /// </remarks>
    public static double[] Envelope(
        IReadOnlyList<float> samples,
        int sampleRate,
        double toneHz,
        double bandwidthHz)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var count = samples.Count;
        var window = IntegratorWindow(sampleRate, bandwidthHz);
        var taper = IntegratorTaper(window);
        var weight = taper.Sum();
        var step = Math.Max(1, (int)(sampleRate * HopMilliseconds / 1000.0));

        var mixedI = new double[count];
        var mixedQ = new double[count];
        var omega = -2 * Math.PI * toneHz / sampleRate;

        for (var i = 0; i < count; i++)
        {
            var angle = omega * i;

            mixedI[i] = samples[i] * Math.Cos(angle);
            mixedQ[i] = samples[i] * Math.Sin(angle);
        }

        // **THE SAME CENTRING THE REFERENCE USES.** The window is laid over the
        // sample at the centre, zero outside the recording, which is what numpy's
        // `same` convolution does and what the port has to match for its output
        // to be comparable at all. The taper's own centre sits on that sample.
        var lead = (window - 1) / 2;
        var envelope = new double[(count + step - 1) / step];

        for (var out_ = 0; out_ < envelope.Length; out_++)
        {
            var centre = out_ * step;
            var first = centre - lead;

            double i = 0;
            double q = 0;

            for (var n = 0; n < window; n++)
            {
                var at = first + n;

                if (at < 0 || at >= count)
                {
                    continue;
                }

                i += mixedI[at] * taper[n];
                q += mixedQ[at] * taper[n];
            }

            envelope[out_] = Math.Sqrt((i * i) + (q * q)) / weight;
        }

        return envelope;
    }

    /// <summary>
    /// Per-hop log-likelihood that the key is down, and that it is up.
    /// </summary>
    /// <param name="envelope">The envelope.</param>
    /// <returns>The two streams.</returns>
    /// <remarks>
    /// **NO THRESHOLD IS FORMED ANYWHERE.** The noise scale comes from the lower
    /// quartile of the envelope and the signal amplitude from its upper tail, and
    /// every hop is scored against both hypotheses. Bell does this properly with a
    /// tracked noise power feeding Kalman recursions; this is the cheap version
    /// and it is where a later session should improve it.
    /// </remarks>
    public static (double[] KeyDown, double[] KeyUp) LogLikelihoods(
        IReadOnlyList<double> envelope)
        => LogLikelihoods(envelope, NoiseSpanSeconds);

    /// <summary>Per-hop log-likelihoods, over a stated estimation span.</summary>
    /// <param name="envelope">The envelope.</param>
    /// <param name="noiseSpanSeconds">
    /// How much audio the noise scale and the keyed level are taken over.
    /// </param>
    /// <returns>The two streams.</returns>
    /// <remarks>
    /// **THE SPAN IS OPEN HERE AND A CONSTANT IN PRODUCTION.** It is a parameter
    /// so the sensitivity of every number in this unit to it can be measured and
    /// reported rather than assumed; nothing in the application passes anything
    /// but <see cref="NoiseSpanSeconds"/>.
    /// </remarks>
    public static (double[] KeyDown, double[] KeyUp) LogLikelihoods(
        IReadOnlyList<double> envelope, double noiseSpanSeconds)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var count = envelope.Count;
        var keyDown = new double[count];
        var keyUp = new double[count];

        if (count == 0)
        {
            return (keyDown, keyUp);
        }

        var span = Math.Max(
            8, (int)(noiseSpanSeconds * 1000.0 / HopMilliseconds));

        // How often the estimate is re-taken: an eighth of the span, so it
        // follows a fade without being re-sorted at every hop.
        var step = Math.Max(1, span / 8);

        var scratch = new double[Math.Min(span, count)];

        var sigma = 0.0;
        var amplitude = 0.0;
        var estimatedAt = int.MinValue;

        for (var i = 0; i < count; i++)
        {
            if (estimatedAt == int.MinValue || i - estimatedAt >= step)
            {
                Estimate(envelope, i, span, scratch, out sigma, out amplitude);
                estimatedAt = i;
            }

            // **DIGITAL SILENCE IS AN ABSENCE OF MEASUREMENT, NOT A QUIET BAND**
            // (§0.0, HM-DEC-009). Where a quarter of the span is exactly nought
            // the quarter point is nought, sigma falls to its floor, and every
            // log-likelihood is then a ratio of two arbitrary numbers: the model
            // is being asked what the noise looks like in audio that has no
            // noise. Scored that way, three seconds of an all-zero buffer
            // produced characters.
            //
            // Both hypotheses are given the same score over such a span, so
            // neither can win it and the window contributes nothing either way.
            // Nothing is read from it rather than noise being read against a
            // clamped scale.
            if (double.IsNaN(sigma))
            {
                keyUp[i] = 0;
                keyDown[i] = 0;

                continue;
            }

            var e = Math.Max(envelope[i], 1e-12);
            var logSigma = Math.Log(sigma);
            var variance = 2 * sigma * sigma;

            // **KEY UP IS RAYLEIGH, NOT GAUSSIAN.** An envelope magnitude taken
            // from a quadrature pair of Gaussian noise is Rayleigh distributed,
            // and its log density carries a log-of-the-magnitude term. Leaving
            // that out is what let noise score as evidence: without it the
            // key-up hypothesis is under-credited in the upper tail, which is
            // exactly where noise peaks live, so a loud noise peak looked more
            // like a mark than like noise.
            keyUp[i] = Math.Log(e) - (2 * logSigma) - (e * e / variance);

            // Key down is the Gaussian approximation to a Rician envelope, which
            // is what it always was. What changes is that it is now a proper log
            // density, so the difference between the two is a log-likelihood
            // ratio rather than a difference between two differently normalised
            // numbers.
            var off = e - amplitude;

            keyDown[i] = -HalfLogTwoPi - logSigma - (off * off / variance);
        }

        return (keyDown, keyUp);
    }

    /// <summary>
    /// Per-hop log-likelihoods with the key-up state fitted from the observed
    /// inter-mark level rather than pinned to the noise scale.
    /// </summary>
    /// <param name="envelope">The envelope.</param>
    /// <param name="noiseSpanSeconds">What the estimates are taken over.</param>
    /// <returns>The two streams.</returns>
    /// <remarks>
    /// <para>**WHAT THIS CHANGES AND WHY** (work instruction 035, task 3). The
    /// shipped model scores key-up as a Rayleigh at the noise scale. On the
    /// captures the operator can hear, the observed key-up state sits 15 to 37
    /// decibels above the band beside the station — it is not noise, and the
    /// model is being asked to explain it as noise.</para>
    /// <para>**BOTH STATES ARE FITTED, WHICH IS THE PUBLISHED SHAPE.**
    /// `cwdecoder.py` in this repository fits two means to the decibel envelope
    /// per window; RSCW places its threshold where the mean distance to the
    /// samples above equals the mean distance to those below. Neither assumes
    /// either state.</para>
    /// <para>**HOW IT BEHAVES WHEN THERE IS NO STATION, WHICH IS THE CASE THAT
    /// PROTECTS HM-DEC-120.** On audio holding nothing the two fitted locations
    /// collapse toward each other, so every hop scores nearly alike under both
    /// hypotheses and their difference — the likelihood ratio — goes toward
    /// **zero**, which is further below the gate rather than above it. The
    /// collapse makes the model *less* willing to read, not more, and that is a
    /// property of fitting both states rather than a guard bolted on.</para>
    /// <para>**THE SPREAD IS FLOORED AT THE NOISE SCALE.** Two locations fitted
    /// to a handful of hops can land arbitrarily close together, and a vanishing
    /// width would then make every hop infinitely surprising under one of them.
    /// The noise scale is a physical lower bound on how tightly either state can
    /// really be known, so neither width goes below it.</para>
    /// </remarks>
    public static (double[] KeyDown, double[] KeyUp) FittedLogLikelihoods(
        IReadOnlyList<double> envelope, double noiseSpanSeconds)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var count = envelope.Count;
        var keyDown = new double[count];
        var keyUp = new double[count];

        if (count == 0)
        {
            return (keyDown, keyUp);
        }

        var span = Math.Max(8, (int)(noiseSpanSeconds * 1000.0 / HopMilliseconds));
        var step = Math.Max(1, span / 8);
        var scratch = new double[Math.Min(span, count)];

        var sigma = 0.0;
        var amplitude = 0.0;
        var estimatedAt = int.MinValue;

        var upLevel = 0.0;
        var upWidth = 0.0;
        var downWidth = 0.0;

        for (var i = 0; i < count; i++)
        {
            if (estimatedAt == int.MinValue || i - estimatedAt >= step)
            {
                Estimate(envelope, i, span, scratch, out sigma, out amplitude);

                if (!double.IsNaN(sigma))
                {
                    FitTwoStates(
                        envelope, i, span, sigma,
                        out upLevel, out upWidth, out downWidth);
                }

                estimatedAt = i;
            }

            if (double.IsNaN(sigma))
            {
                keyUp[i] = 0;
                keyDown[i] = 0;

                continue;
            }

            var e = Math.Max(envelope[i], 1e-12);

            var offUp = e - upLevel;
            keyUp[i] = -HalfLogTwoPi - Math.Log(upWidth)
                - (offUp * offUp / (2 * upWidth * upWidth));

            var offDown = e - amplitude;
            keyDown[i] = -HalfLogTwoPi - Math.Log(downWidth)
                - (offDown * offDown / (2 * downWidth * downWidth));
        }

        return (keyDown, keyUp);
    }

    /// <summary>Two levels fitted to the envelope around one hop.</summary>
    /// <param name="envelope">The envelope.</param>
    /// <param name="at">Which hop the span is centred on.</param>
    /// <param name="span">How many hops it covers.</param>
    /// <param name="sigma">The noise scale, which floors both widths.</param>
    /// <param name="upLevel">Where the inter-mark state actually sits.</param>
    /// <param name="upWidth">How tightly, never below the noise scale.</param>
    /// <param name="downWidth">The same for the keyed state.</param>
    /// <remarks>
    /// **LOCAL IN TIME, ON THE SPAN THE NOISE SCALE ALREADY USES.** A key-up
    /// level averaged over a whole recording is HM-DEC-090's own fault arriving
    /// again: that ruling turned two whole-file averages into held peaks because
    /// a figure taken across a station's silence is not a figure about the
    /// station.
    /// </remarks>
    private static void FitTwoStates(
        IReadOnlyList<double> envelope,
        int at,
        int span,
        double sigma,
        out double upLevel,
        out double upWidth,
        out double downWidth)
    {
        var count = envelope.Count;
        var half = span / 2;
        var from = Math.Clamp(at - half, 0, Math.Max(0, count - span));
        var take = Math.Min(span, count - from);

        var low = double.MaxValue;
        var high = double.MinValue;

        for (var n = 0; n < take; n++)
        {
            var v = envelope[from + n];
            low = Math.Min(low, v);
            high = Math.Max(high, v);
        }

        var cut = (low + high) / 2;
        var upMean = low;
        var downMean = high;

        for (var pass = 0; pass < 12; pass++)
        {
            double lo = 0, hi = 0;
            int loN = 0, hiN = 0;

            for (var n = 0; n < take; n++)
            {
                var v = envelope[from + n];

                if (v >= cut)
                {
                    hi += v;
                    hiN++;
                }
                else
                {
                    lo += v;
                    loN++;
                }
            }

            if (loN == 0 || hiN == 0)
            {
                break;
            }

            upMean = lo / loN;
            downMean = hi / hiN;

            var next = (upMean + downMean) / 2;

            if (Math.Abs(next - cut) < 1e-12)
            {
                break;
            }

            cut = next;
        }

        double upVar = 0, downVar = 0;
        int upN = 0, downN = 0;

        for (var n = 0; n < take; n++)
        {
            var v = envelope[from + n];

            if (v >= cut)
            {
                downVar += (v - downMean) * (v - downMean);
                downN++;
            }
            else
            {
                upVar += (v - upMean) * (v - upMean);
                upN++;
            }
        }

        upLevel = upMean;
        upWidth = Math.Max(upN > 1 ? Math.Sqrt(upVar / upN) : sigma, sigma);
        downWidth = Math.Max(downN > 1 ? Math.Sqrt(downVar / downN) : sigma, sigma);
    }

    private static readonly double HalfLogTwoPi = 0.5 * Math.Log(2 * Math.PI);

    /// <summary>
    /// The noise scale and the keyed level, over the span around one hop.
    /// </summary>
    /// <param name="envelope">The envelope.</param>
    /// <param name="at">Which hop the span is centred on.</param>
    /// <param name="span">How many hops it covers.</param>
    /// <param name="scratch">Working room, reused so nothing allocates per hop.</param>
    /// <param name="sigma">The Rayleigh scale.</param>
    /// <param name="amplitude">The keyed level.</param>
    /// <remarks>
    /// **SIGMA COMES FROM THE QUARTER POINT BY AN IDENTITY.** For a Rayleigh
    /// envelope the cumulative distribution is one minus the exponential of
    /// minus e squared over two sigma squared, so at the quarter point that
    /// exponential is three quarters and the magnitude is sigma times the root
    /// of twice the natural log of four thirds. See
    /// <see cref="RayleighQuarterPoint"/>. There is nothing in it to tune.
    /// </remarks>
    private static void Estimate(
        IReadOnlyList<double> envelope,
        int at,
        int span,
        double[] scratch,
        out double sigma,
        out double amplitude)
    {
        var count = envelope.Count;
        var half = span / 2;
        var from = Math.Clamp(at - half, 0, Math.Max(0, count - scratch.Length));
        var take = Math.Min(scratch.Length, count - from);

        for (var n = 0; n < take; n++)
        {
            scratch[n] = envelope[from + n];
        }

        Array.Sort(scratch, 0, take);

        var quarter = PercentileOf(scratch, take, 25);

        // **NO ESTIMATE RATHER THAN A CLAMPED ONE, AND ONLY WHERE THERE IS
        // NOTHING AT ALL.** A span whose largest magnitude is nought is entirely
        // digital silence, which a receiver never delivers: it is a muted codec,
        // a gap spliced into a recording, or a fixture built without a noise
        // floor (HM-OPEN-018). There is no noise to estimate, so no estimate is
        // returned and the caller reads nothing from the span. Returning a floor
        // instead is what put `cw-2026-08-23-001520` at ten to the sixteenth and
        // let three seconds of an all-zero buffer emit characters.
        //
        // **THE TEST IS THE QUARTER POINT, AND THE SPECIFICATION'S LITERAL FORM
        // WAS TRIED AND IS WORSE.** Refusing only where the whole span is silent
        // takes `clean-12wpm`, `clean-18wpm` and `prosigns-18wpm` from nine, nine
        // and sixteen to **nought**: over a wholly silent span both hypotheses
        // score the same, so a mark costs no more than a gap and the length
        // penalty alone decides, which on fixtures made of tone and exact silence
        // is most of the recording. The quarter point costs those two fixtures
        // two and three characters instead, and they are the ones HM-OPEN-018
        // already records as encoding a physical impossibility.
        if (quarter <= 0)
        {
            sigma = double.NaN;
            amplitude = double.NaN;

            return;
        }

        sigma = quarter / RayleighQuarterPoint;
        amplitude = Math.Max(PercentileOf(scratch, take, 97), sigma * 1.05);
    }

    /// <summary>One value out of the first N of a sorted buffer.</summary>
    /// <param name="sorted">The buffer, sorted over its first entries.</param>
    /// <param name="count">How many of them are real.</param>
    /// <param name="percent">Which percentile.</param>
    /// <returns>The value.</returns>
    private static double PercentileOf(double[] sorted, int count, double percent)
    {
        if (count <= 0)
        {
            return 0;
        }

        var at = (percent / 100.0) * (count - 1);
        var below = (int)at;
        var above = Math.Min(below + 1, count - 1);
        var share = at - below;

        return (sorted[below] * (1 - share)) + (sorted[above] * share);
    }

    /// <summary>One value out of a sorted set, interpolating between neighbours.</summary>
    /// <param name="sorted">The values, in order.</param>
    /// <param name="percent">Which percentile.</param>
    /// <returns>The value.</returns>
    private static double Percentile(double[] sorted, double percent)
    {
        if (sorted.Length == 0)
        {
            return 0;
        }

        var at = percent / 100.0 * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }

    /// <summary>
    /// The segmental Viterbi at one speed hypothesis.
    /// </summary>
    /// <param name="count">How many hops there are.</param>
    /// <param name="wpm">The speed being tried.</param>
    /// <param name="keyDown">Per-hop log-likelihood the key is down.</param>
    /// <param name="keyUp">Per-hop log-likelihood the key is up.</param>
    /// <param name="gapMilliseconds">
    /// This sender's own three gap lengths, or null to expect one, three and
    /// seven units.
    /// </param>
    /// <param name="jointly">Whether the joint cutter decides the cuts.</param>
    /// <returns>The best total score and what it spells.</returns>
    /// <remarks>
    /// **EVERY PATH IS A CHAIN OF WHOLE ELEMENTS THAT MUST ALTERNATE.** A
    /// segment's score is the summed per-hop likelihood over its span plus a
    /// Gaussian penalty on how far its length sits from the one, three or seven
    /// units the hypothesis expects. Cumulative sums make a span's score two
    /// subtractions, so the whole thing is one pass over hops times durations
    /// times kinds.
    /// </remarks>
    private static (
        double Score,
        IReadOnlyList<CwProbabilisticCharacter> Characters,
        int LastKind)
        DecodeAt(
        int count,
        double wpm,
        double[] keyDown,
        double[] keyUp,
        IReadOnlyList<double>? gapMilliseconds = null,
        bool jointly = false)
    {
        var unit = 1200.0 / wpm / HopMilliseconds;

        // **THIS SENDER'S OWN GAPS, IN HOPS**, when they were measured. The kinds
        // keep their order — the gap inside a character, then between characters,
        // then between words — and only what each one expects to last changes.
        var gapHops = gapMilliseconds is { Count: 3 }
            ? new[]
            {
                gapMilliseconds[0] / HopMilliseconds,
                gapMilliseconds[1] / HopMilliseconds,
                gapMilliseconds[2] / HopMilliseconds,
            }
            : null;

        var downTo = new double[count + 1];
        var upTo = new double[count + 1];

        for (var i = 0; i < count; i++)
        {
            downTo[i + 1] = downTo[i] + keyDown[i];
            upTo[i + 1] = upTo[i] + keyUp[i];
        }

        var best = new double[count + 1];
        var fromHop = new int[count + 1];
        var kindAt = new int[count + 1];
        var wasDown = new bool[count + 1];

        // **THE RUNNER-UP, KEPT SO A CHARACTER CAN SAY HOW CLOSE THE ARGUMENT
        // WAS.** Nothing reads it yet; see
        // <see cref="CwProbabilisticCharacter.MarginLlr"/> for why it is worth
        // recording before anything is decided on it.
        var second = new double[count + 1];

        Array.Fill(second, double.NegativeInfinity);
        Array.Fill(best, double.NegativeInfinity);
        Array.Fill(fromHop, -1);
        best[0] = 0;

        for (var i = 1; i <= count; i++)
        {
            for (var k = 0; k < Kinds.Length; k++)
            {
                var kind = Kinds[k];
                var want = gapHops is not null && !kind.IsKeyDown
                    ? gapHops[k - 2]
                    : kind.Units * unit;
                var shortest = Math.Max(1, (int)(want * ShortestShare));
                var longest = Math.Max(shortest + 1, (int)(want * LongestShare));
                var ceiling = Math.Min(longest, i);

                for (var span = shortest; span <= ceiling; span++)
                {
                    var j = i - span;

                    if (double.IsNegativeInfinity(best[j]))
                    {
                        continue;
                    }

                    // Elements must alternate: a mark cannot follow a mark.
                    if (j > 0 && wasDown[j] == kind.IsKeyDown)
                    {
                        continue;
                    }

                    var evidence = kind.IsKeyDown
                        ? downTo[i] - downTo[j]
                        : upTo[i] - upTo[j];

                    // Guarded against a zero span, which the shortest-span floor
                    // already makes impossible and which would be an infinity if
                    // it ever were not.
                    var off = Math.Log(Math.Max(span, 1e-9) / want)
                        / LengthToleranceShare;
                    var score = best[j] + evidence - (0.5 * off * off);

                    if (score > best[i])
                    {
                        second[i] = best[i];
                        best[i] = score;
                        fromHop[i] = j;
                        kindAt[i] = k;
                        wasDown[i] = kind.IsKeyDown;
                    }
                    else if (score > second[i])
                    {
                        second[i] = score;
                    }
                }
            }
        }

        return (
            best[count],
            Spell(
                count, fromHop, kindAt, downTo, upTo, best, second, unit, gapHops,
                jointly),
            kindAt[count]);
    }

    /// <summary>
    /// Replace every character that cannot clear its own margin with the
    /// unresolved placeholder.
    /// </summary>
    /// <param name="characters">What the path spelled.</param>
    /// <returns>The same list, with the weak ones marked.</returns>
    /// <remarks>
    /// <para>**MARKED, NOT DROPPED.** Dropping it would close the gap and hand
    /// the reader a shorter word that looks like a clean decode; the whole point
    /// of the third confidence state is that the operator can see Hamlet
    /// struggling at a particular letter rather than being handed a tidied
    /// result (HM-DEC-048).</para>
    /// <para>A word gap carries no marks and has no evidence of its own to
    /// clear, so it is left alone. Its own span ratio is nought by construction
    /// and testing it would delete every space.</para>
    /// </remarks>
    private static IReadOnlyList<CwProbabilisticCharacter> Marked(
        IReadOnlyList<CwProbabilisticCharacter> characters)
    {
        var marked = new List<CwProbabilisticCharacter>(characters.Count);

        foreach (var character in characters)
        {
            var isWordGap = character.Pattern.Length == 0;

            marked.Add(isWordGap || character.SpanMargin >= CharacterMargin
                ? character
                : character with { Text = "#" });
        }

        return marked;
    }

    /// <summary>The element stream the first pass produced, marks and gaps.</summary>
    private static List<CwElement> ElementsOf(
        int count, int[] fromHop, int[] kindAt)
    {
        var walk = new List<CwElement>();
        var at = count;

        while (at > 0 && fromHop[at] >= 0)
        {
            walk.Add(new CwElement(Kinds[kindAt[at]].IsKeyDown, fromHop[at], at));
            at = fromHop[at];
        }

        walk.Reverse();

        return walk;
    }

    /// <summary>Walk the winning path back and turn it into letters.</summary>
    /// <param name="count">How many hops there were.</param>
    /// <param name="fromHop">Where each hop's best segment started.</param>
    /// <param name="kindAt">Which kind that segment was.</param>
    /// <param name="downTo">Cumulative key-down log-likelihood, hop by hop.</param>
    /// <param name="upTo">Cumulative key-up log-likelihood, hop by hop.</param>
    /// <param name="best">The winning score at each hop.</param>
    /// <param name="second">The runner-up score at each hop.</param>
    /// <param name="unit">The fitted clock, in hops per unit.</param>
    /// <param name="gapHops">This sender's own gap lengths, or null.</param>
    /// <param name="jointly">
    /// Whether <see cref="CwJointCutter"/> decides the character boundaries.
    /// **A parameter and never a static** — xUnit runs test classes in parallel
    /// and a mutable static read by the decode path is read by whichever test is
    /// running at the time, which is how the first build of this measured itself
    /// as having changed nothing.
    /// </param>
    /// <returns>The text.</returns>
    /// <remarks>
    /// **EACH CHARACTER'S OWN SPAN IS SCORED AGAINST ALL-KEY-UP ON THE WAY
    /// PAST**, which the cumulative sums make two subtractions per mark. See
    /// <see cref="CwProbabilisticCharacter.SpanLogLikelihoodRatio"/> for why the
    /// element gaps inside a character contribute nothing and why the length
    /// penalty is left out.
    /// </remarks>
    private static IReadOnlyList<CwProbabilisticCharacter> Spell(
        int count, int[] fromHop, int[] kindAt, double[] downTo, double[] upTo,
        double[] best, double[] second, double unit, double[]? gapHops,
        bool jointly)
    {
        if (jointly)
        {
            return SpellJointly(
                count, fromHop, kindAt, downTo, upTo, unit, gapHops);
        }

        // How much better the winning path was than the nearest alternative
        // arriving at the same hop; see `CwProbabilisticCharacter.MarginLlr`.
        static double Margin(double[] best, double[] second, int at)
            => double.IsNegativeInfinity(second[at]) || double.IsNegativeInfinity(best[at])
                ? double.NaN
                : best[at] - second[at];

        var path = new List<(int Kind, int StartHop, int EndHop)>();
        var at = count;

        while (at > 0 && fromHop[at] >= 0)
        {
            path.Add((kindAt[at], fromHop[at], at));
            at = fromHop[at];
        }

        path.Reverse();

        var pattern = new System.Text.StringBuilder();
        var characters = new List<CwProbabilisticCharacter>();

        // The running total for the character being spelled, marks only.
        var spanRatio = 0.0;

        // Where its first mark began, so the span can be divided by its length.
        var spanFrom = -1;

        foreach (var (k, startHop, endHop) in path)
        {
            var kind = Kinds[k];

            if (kind.IsKeyDown)
            {
                pattern.Append(kind.Token);

                if (spanFrom < 0)
                {
                    spanFrom = startHop;
                }

                spanRatio += downTo[endHop] - downTo[startHop]
                    - (upTo[endHop] - upTo[startHop]);

                continue;
            }

            if (kind.Token.Length == 0)
            {
                continue;
            }

            // **THE CHARACTER ENDED WHEN THE KEY WENT UP**, not when the gap
            // after it finished, and the difference is not cosmetic: a letter and
            // the word gap behind it would otherwise carry the same moment, and
            // a streaming reader that settles by time cannot tell them apart.
            if (pattern.Length > 0)
            {
                var spelled = pattern.ToString();

                characters.Add(new CwProbabilisticCharacter(
                    MorseAlphabet.Lookup(spelled) ?? "#", spelled, startHop,
                    spanRatio,
                    spanFrom < 0 ? 0 : startHop - spanFrom,
                    Margin(best, second, startHop)));

                pattern.Clear();
                spanRatio = 0;
                spanFrom = -1;
            }

            if (kind.Token == " ")
            {
                characters.Add(new CwProbabilisticCharacter(" ", "", endHop));
            }
        }

        if (pattern.Length > 0)
        {
            var spelled = pattern.ToString();

            characters.Add(new CwProbabilisticCharacter(
                MorseAlphabet.Lookup(spelled) ?? "#", spelled, count, spanRatio,
                spanFrom < 0 ? 0 : count - spanFrom,
                Margin(best, second, count)));
        }

        return characters;
    }

    /// <summary>
    /// The same path, cut into characters by <see cref="CwJointCutter"/>.
    /// </summary>
    /// <remarks>
    /// The first pass still decides where the key went down and up; what changes
    /// is only where those elements are divided into letters, and that decision
    /// is now made together with what the letters are.
    /// </remarks>
    private static IReadOnlyList<CwProbabilisticCharacter> SpellJointly(
        int count, int[] fromHop, int[] kindAt,
        double[] downTo, double[] upTo, double unit, double[]? gapHops)
    {
        var elements = ElementsOf(count, fromHop, kindAt);
        var cut = CwJointCutter.Cut(elements, unit, gapHops);
        var characters = new List<CwProbabilisticCharacter>();

        var marks = elements.Where(e => e.IsMark).ToList();

        foreach (var c in cut)
        {
            // The evidence for the span, on the same scale the old path used, so
            // `spanLlr` keeps meaning what it meant on every sheet already
            // written (HM-DEC-091: one source).
            var first = marks[c.FirstMark];
            var last = marks[c.FirstMark + c.MarkCount - 1];

            var ratio = 0.0;

            for (var m = c.FirstMark; m < c.FirstMark + c.MarkCount; m++)
            {
                ratio += downTo[marks[m].EndHop] - downTo[marks[m].StartHop]
                    - (upTo[marks[m].EndHop] - upTo[marks[m].StartHop]);
            }

            characters.Add(new CwProbabilisticCharacter(
                c.Text, c.Pattern, last.EndHop, ratio,
                last.EndHop - first.StartHop, c.Margin));

            if (c.EndsWord)
            {
                characters.Add(new CwProbabilisticCharacter(" ", "", last.EndHop));
            }
        }

        return characters;
    }
}
