using Hamlet.RadioEngine.Contacts;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Controls;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>One line of the Digital tab's decoded table.</summary>
/// <param name="Utc">When the slot opened, as `hhmmss`.</param>
/// <param name="Snr">
/// The signal-to-noise ratio, **or <see cref="NoMeasurement"/>, which is what it
/// is today**. See the remarks.
/// </param>
/// <param name="Dt">How far into the slot the transmission started, in seconds.</param>
/// <param name="Hz">The lowest of the eight tones, in the audio passband.</param>
/// <param name="Message">The text, exactly as it was sent.</param>
/// <remarks>
/// <para>**FORMATTED HERE RATHER THAN IN THE MARKUP**, so what a reader sees can
/// be asserted by a test that never opens a window.</para>
/// <para>**THE SNR COLUMN CARRIED A DASH FROM WORK INSTRUCTION 037 UNTIL UNIT
/// 251** (§0.0). The columns were committed on the assumption that `snr` is what
/// the decoder produces; it is not. `Ft8Sharp` returns a Costas sync score — how
/// far the expected tone stood above the average of the eight — which is not
/// decibels, is not calibrated against anything, and would have been read as a
/// measurement the moment it appeared under that heading. A dash said nothing
/// was measured, which was true.</para>
/// <para>**SINCE UNIT 251 SOMETHING MEASURES ONE.**
/// `Ft8Sharp.Deep.Ft8DeepSignalToNoise` reads the power in the tone that was
/// transmitted against the seven tones that were not, at the same instant, and
/// carries the per-bin ratio to the 2500 Hz reference bandwidth by a derived
/// 26.0206 dB. Over 510 synthesized messages, five rungs and two placements, it
/// agreed with the ratio actually delivered to a **mean absolute error of
/// 0.26 dB and a 95th percentile of 0.62 dB** — `docs/unit251-snr-trace.md` §6.
/// `PHASE_PLAN.md` step 2 says the column keeps its dash if agreement is worse
/// than 2 dB. **It is not, so the column shows a number.**</para>
/// <para>**AND THE DASH DID NOT GO AWAY** — it moved. A message whose ratio
/// could not be measured still shows <see cref="NoMeasurement"/>, because a
/// floored or guessed decibel figure is indistinguishable on the screen from a
/// measured weak one, which is the fault §0.0 exists for.</para>
/// <para>**WHOLE DECIBELS, AND THE REASON IS THE 95th AND NOT THE MEAN.** A
/// tenth of a decibel in this column would say the difference between a station
/// at -13.2 and one at -13.4 means something. It does not: one message in twenty
/// is 0.62 dB or further out, so the first decimal is noise being drawn as
/// signal. Whole decibels is the coarsest unit that still separates the stations
/// an operator has to choose between, it is what this mode is quoted in
/// everywhere else, and it fits the 48-pixel monospace column with room for the
/// sign.</para>
/// </remarks>
/// <remarks>
/// <para>**THREE THINGS ON THIS ROW CHANGE AFTER IT ARRIVES**, and the list is
/// here because it has been wrong twice. <see cref="WorkedBefore"/> and the
/// <see cref="RowOpacity"/> and <see cref="WorkedTip"/> derived from it are
/// written when the log is read; <see cref="RepeatCount"/> is written when the
/// same message arrives again. Everything else is fixed at construction.</para>
/// <para>**THE HISTORY, BECAUSE THE REMARK THAT USED TO SIT HERE WAS FALSE BY THE
/// TIME ANYBODY READ IT.** Unit 252 removed a `RowOpacity` derived from an
/// `IsDimmed` flag, because Tim's ruling of 2026-09-06 took a filtered-out row off
/// the table entirely rather than dimming it, and this remark then said nothing on
/// the row ever changes. Unit 274 gave it a mutable `WorkedBefore`, unit 277 a
/// mutable `RepeatCount`, and the remark stayed. **A file that states a rule its
/// own code breaks is worse than either answer** (HM-DEC-159), because the next
/// session reads the rule, believes it, and writes against it.</para>
/// <para>**AND `RowOpacity` IS BACK FOR A DIFFERENT REASON THAN IT LEFT.** It went
/// because a row the filter did not want should not be on the table at all; it
/// returns because a station he has already worked should still be on the table
/// and should recede (Tim's ruling, 2026-09-08). Those are opposite situations and
/// the second does not reopen the first.</para>
/// </remarks>
/// <param name="ObserverGrid">
/// The operator's own Maidenhead locator from Settings, or "" where he has not
/// set one. **It is on the row because the tooltip needs it and the tooltip is
/// built from the row**, and `Ft8Vocabulary` is a static table with no route to
/// settings. It is set in one place — `MainWindowViewModel.PlaceRow` — so there
/// is no second copy of the operator's grid anywhere; `OperatorProfile.GridSquare`
/// stays the only one. It is used to measure a distance from and never to name a
/// place (Tim's ruling, 2026-09-05).
/// </param>
/// <param name="SlotStartUtc">
/// The boundary of the slot this row's message was in, in true UTC.
/// **The `Utc` cell is `HHmmss` for a reader and cannot be counted with**; this
/// is the moment the contact ledger measures slots from. It is set by
/// <see cref="From(Ft8Decode)"/> off `Ft8Decode.SlotStartUtc` and by nothing
/// else.
/// </param>
/// <param name="HeardOnHz">
/// **The dial this message was heard on**, in hertz, or 0 where it is not
/// recorded.
/// <para>**A LOG ENTRY MUST NOT RECORD A BAND HE DID NOT WORK.** Unit 274's
/// dialog wrote the frequency the dial was on when he right-clicked, said so on
/// the field, and named the fix as another unit's work. This is it: the row
/// carries the tuning it was decoded at, so the log records where the contact
/// happened rather than where the radio has since been turned.</para>
/// <para>**ZERO MEANS NOT RECORDED, NOT ZERO HERTZ.** Anything decoded before
/// this change carries no dial, and the log leaves `FREQ` and `BAND` out
/// entirely rather than guessing one - the rule every other field already
/// follows (§0.0).</para>
/// <para>It is set in one place, `MainWindowViewModel.PlaceRow`, beside
/// <paramref name="ObserverGrid"/> and <paramref name="Contact"/>.</para>
/// </param>
/// <param name="Contact">
/// **Where the contact with this row's sender stands, as text a reader sees.**
/// One of `PHASE_PLAN.md`'s four - *waiting on him*, *your move*, *complete* or
/// *gone quiet* - with the count of slots beside it, or "" where the message
/// has no sender to hold a contact with.
/// **Formatted in the engine and carried here as a string**, for the same reason
/// the rest of this row is formatted rather than templated: what a reader sees
/// can then be asserted by a test that never opens a window.
/// It is set in one place, `MainWindowViewModel.PlaceRow`, beside
/// <paramref name="ObserverGrid"/>.
/// </param>
/// <param name="IsSent">
/// **True where this row is a message the operator transmitted**, rather than one
/// the decoder read off the air.
/// <para>**THIS REVERSES UNIT 273'S INSTRUCTION**, which said a sent message is
/// not a decode and does not belong in a decoded list. It was right about what a
/// sent message is and wrong about what the panel is for: **a conversation with
/// one side missing is unreadable.** On 2026-09-08 a station came back to him
/// three times at -09 and the panel showed three identical rows, with his own
/// three transmissions nowhere, so nothing on the screen said he was answering
/// late (Tim's ruling, 2026-09-08).</para>
/// <para>**A SENT ROW CARRIES NO `Snr` AND NO `Dt`, AND THEY ARE EMPTY RATHER
/// THAN ZERO** (§0.0). Nothing measured a signal-to-noise ratio for a message
/// this station transmitted, and nothing measured how late into the slot it
/// arrived, because it did not arrive. A zero in either cell is a measurement
/// that was never taken, drawn as one that was.</para>
/// <para>**IT IS NOT IN <c>DigitalDecodes</c>.** The master table is what the
/// decoder read, and its counts and its hidden arithmetic are about the band. A
/// sent row lives only on the For you side, which is a conversation rather than
/// a table of decodes.</para>
/// </param>
/// <param name="IsTextOnly">
/// **True where this row is free text and no FT8 field may be read out of it** (work
/// instruction 315 task 3). A PSK31 row is a conversation arriving a character at a time,
/// not an FT8 message: at the moment it reads `CQ CQ CQ` it is three words, and handed to
/// `Ft8Vocabulary.Split` the first would be called an addressee. <see cref="Fields"/> stays
/// null for it, and so do the payload and its hover.
/// <para>**SINCE WORK INSTRUCTION 316 ITS STATION COMES FROM <paramref name="Reading"/>
/// INSTEAD** - the PSK31 parser's reading of the row's latest complete message - so the
/// sender, the addressee, the CQ filter, the operator's own side, the fade, the country and
/// the quill work on it through the same members an FT8 row supplies from its fields.</para>
/// </param>
/// <param name="Reading">
/// **What the latest complete message on this row says**, or null where no message on it has
/// finished yet (`Psk31MessageSplitter`). Only a text-only row carries one.
/// </param>
public sealed partial record DigitalDecodeRow(
    string Utc, string Snr, string Dt, string Hz, string Message,
    string ObserverGrid = "",
    DateTime SlotStartUtc = default,
    string Contact = "",
    long HeardOnHz = 0,
    bool IsSent = false,
    bool IsTextOnly = false,
    Hamlet.RadioEngine.Psk31.Psk31Exchange? Reading = null)
    : INotifyPropertyChanged
{
    /// <summary>The word that says how sure Hamlet is of this row's station, or "".</summary>
    /// <remarks>
    /// <para>**A WORD AND NOT A COLOUR** (§R1, §0.6). *guess* where the parse is not certain but
    /// names a speaker; *unknown* where it names none; nothing where it is certain, nothing on a
    /// row with no finished message, and nothing on an FT8 row.</para>
    /// <para>**THE WORDING IS A SESSION'S** (work instruction 316 task 4), not a ruling.</para>
    /// </remarks>
    public string ReadingWord
        => Reading is null ? ""
            : Reading.Speaker is null ? "unknown"
            : Reading.IsCertain ? ""
            : "guess";

    /// <summary>True where <see cref="ReadingWord"/> has something to say.</summary>
    public bool HasReadingWord => ReadingWord.Length > 0;

    /// <summary>True where this row's station is a guess or unknown rather than read for certain.</summary>
    public bool IsGuess => Reading is { IsCertain: false };

    /// <summary>What a sent row puts in a cell nothing measured.</summary>
    /// <remarks>
    /// **EMPTY, AND NOT <see cref="NoMeasurement"/>.** The dash means *this was
    /// measured and the measurement failed*, which is a claim about a reading
    /// that was attempted. A transmitted message was never a candidate for one:
    /// there is no attempt to report on, so there is nothing to say.
    /// </remarks>
    public const string NotMeasured = "";

    /// <summary>
    /// A row for a message this station transmitted, in the slot it went out in.
    /// </summary>
    /// <param name="message">The text, exactly as it went on the air.</param>
    /// <param name="slotStartUtc">The boundary of the slot it occupied, corrected.</param>
    /// <returns>The row.</returns>
    /// <remarks>
    /// <para>**ONE PLACE BUILDS A SENT ROW**, so the empty cells cannot be filled in
    /// by a second call site in a hurry. The `Utc` cell is formatted exactly as a
    /// decoded row's is, because the two are read down one column.</para>
    /// <para>**THE GRID IS NOT ITS TO FILL AND THE CALLER MUST** (work instruction
    /// 281 task 6). This is a static builder and the operator's grid lives in
    /// settings, so it passes the empty string and `MainWindowViewModel.KeepSentRow`
    /// puts the real one on through `WithOperatorGrid`. **It was the blank that
    /// shipped**, for six units, because nothing said the caller owed it one.</para>
    /// </remarks>
    public static DigitalDecodeRow Sent(string message, DateTime slotStartUtc)
        => new(
            slotStartUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            NotMeasured,
            NotMeasured,
            NotMeasured,
            message ?? "",
            ObserverGrid: "",
            SlotStartUtc: slotStartUtc,
            Contact: "",
            HeardOnHz: 0,
            IsSent: true);

    private int _repeatCount = 1;

    /// <summary>How many times this identical message arrived in a row.</summary>
    /// <remarks>
    /// <para>**THREE COPIES OF ONE MESSAGE MEAN HE IS NOT BEING HEARD, AND THREE
    /// ROWS HIDE IT** (Tim's ruling, 2026-09-08). A station that sends the same
    /// report again has not received an answer it recognised, and read as three
    /// separate lines that reads as three pieces of news rather than one fact
    /// repeated.</para>
    /// <para>**ONLY WHERE NOTHING CAME BETWEEN.** A repeat that arrives after he
    /// transmitted is a different fact from one that arrives before, and it is the
    /// single most diagnostic thing in the exchange this unit was written for: it
    /// says the station did not hear his answer. Folding it back into a row above
    /// his transmission would destroy exactly what the panel exists to show, so
    /// counting stops at anything in between.</para>
    /// <para>**IT IS THE ONE OTHER MUTABLE THING HERE**, for
    /// <see cref="WorkedBefore"/>'s reason: the row is already on the screen when
    /// the second copy arrives, and it has to say so without being replaced.</para>
    /// </remarks>
    public int RepeatCount
    {
        get => _repeatCount;
        set
        {
            if (_repeatCount == value)
            {
                return;
            }

            _repeatCount = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepeatCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Shown)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasRepeats)));
        }
    }

    /// <summary>True where this message arrived more than once running.</summary>
    public bool HasRepeats => RepeatCount > 1;

    /// <summary>The message as the conversation draws it, with any repeat count.</summary>
    /// <remarks>
    /// **THE COUNT IS ON THE MESSAGE AND NOT IN A COLUMN OF ITS OWN**, because it
    /// is a fact about that message rather than about the slot: `-09 x3` is one
    /// station saying one thing three times. <see cref="Message"/> is left exactly
    /// as it was sent, so everything that reasons about the text still sees the
    /// text.
    /// </remarks>
    public string Shown => Message;

    /// <summary>The time beneath the message, and how often it was heard.</summary>
    /// <remarks>
    /// <para>**`sent` CAME OUT ON 2026-09-08** (Tim, work instruction 281 task 3:
    /// *the alignment says it*). His own messages sit on the right in an amber-edged
    /// bubble and what he heard sits on the left, and that is read without reading.
    /// **Unit 280 kept the word citing §0.6 and the citation was wrong**: that rule
    /// is about colour, and its own practical test is whether the screen still reads
    /// in grayscale. Alignment is exactly what does survive grayscale.</para>
    /// <para>**THE FACT IS ONE HOVER AWAY** (<see cref="DirectionTip"/>), not
    /// deleted.</para>
    /// <para>**AND THE TIME STAYS.** It is the one thing on the row that lets him
    /// see he answered a slot late, which is the whole reason the conversation was
    /// built.</para>
    /// </remarks>
    public string Caption
    {
        get
        {
            // **A GUESSED STATION SAYS SO UNDER ITS MESSAGE TOO** (work instruction 316 task 4,
            // §R1). His side draws a row as a bubble and this caption rather than as the
            // table's cells, so the word is here as well as beside the sender there.
            var when = HasReadingWord ? Utc + " · " + ReadingWord : Utc;

            // **THE FOLD MOVED UNDER THE MESSAGE ON 2026-09-08** (Tim: show, do
            // not tell). `x2` sat inside the message text, where it read as part
            // of what the station transmitted - and the message is the one string
            // on this row that must be exactly what went out. **Where the fold
            // stops has not changed**, only where it is shown.
            return RepeatCount switch
            {
                <= 1 => when,
                2 => when + " · heard twice",
                3 => when + " · heard three times",
                _ => when + " · heard "
                    + RepeatCount.ToString(CultureInfo.InvariantCulture) + " times",
            };
        }
    }

    /// <summary>Which way this message went, for the caption's own hover.</summary>
    /// <remarks>
    /// **MOVED, NOT DELETED.** The word `sent` left the caption on 2026-09-08 and
    /// this is where it went, with the slot time beside it (§0.0, HM-DEC-092).
    /// </remarks>
    public string DirectionTip
        => IsSent
            ? "You sent this at " + Utc + " UTC."
            : "Hamlet heard this at " + Utc + " UTC.";

    /// <summary>What the row says it is, for a reader who cannot see colour.</summary>
    /// <remarks>
    /// **COLOUR IS NEVER THE ONLY CARRIER** (§0.6). The two kinds of row must be
    /// tellable apart at a glance and also in grayscale, so the mark is a word
    /// and the colour is a second signal saying the same thing.
    /// </remarks>
    public string Direction => IsSent ? "sent" : "";

    /// <summary>True where the direction mark should be drawn.</summary>
    public bool HasDirection => Direction.Length > 0;

    private string _workedBefore = "";

    /// <summary>What the log says about this sender, or "".</summary>
    /// <remarks>
    /// <para>**MARKED, NOT HIDDEN AND NOT DISABLED** (Tim's ruling, 2026-09-07).
    /// Working somebody twice is his choice, on another band or another day, and
    /// nothing about this row changes except that it says so.</para>
    /// <para>**THE ONE MUTABLE THING ON THIS RECORD, AND IT IS SET IN ONE PLACE.**
    /// A row is born before the log is consulted and again after a contact is
    /// written, so this is assigned by `MainWindowViewModel.RefreshWorkedBefore`
    /// rather than being part of what the row is. It raises its own change so a
    /// row already on screen picks up the mark the moment the log gains an entry.
    /// </para>
    /// </remarks>
    public string WorkedBefore
    {
        get => _workedBefore;
        set
        {
            if (_workedBefore == value)
            {
                return;
            }

            _workedBefore = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkedBefore)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasWorkedBefore)));

            // **THE ROW FADES THE MOMENT THE LOG GAINS THE ENTRY** (Tim,
            // 2026-09-08). He logs a contact and that station's other rows from
            // the same evening recede at once, rather than only the rows that
            // arrive after it.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RowOpacity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkedTip)));
        }
    }

    /// <summary>True where the log already holds this station.</summary>
    public bool HasWorkedBefore => _workedBefore.Length > 0;

    /// <summary>How strongly the row is drawn.</summary>
    /// <remarks>
    /// <para>**A STATION HE HAS WORKED FADES RATHER THAN WEARING A LABEL** (Tim's
    /// ruling, 2026-09-08, replacing unit 274's green `worked` beside the
    /// callsign). The word cost a column on the narrower of the two lists and said
    /// in five characters what the whole row can say by receding.</para>
    /// <para>**FADED IS NOT DISABLED, AND THE DIFFERENCE IS DELIBERATE IN THE
    /// MECHANISM** (§0.5.1, HM-DEC-087). Grey is this project's reserved signal
    /// for a control that genuinely cannot be used, and greying the row would have
    /// borrowed exactly that signal for a row he is free to work again. Opacity
    /// **keeps every colour the row already has** - the addressee, the sender and
    /// the payload stay their own family colours, softer - where greying would
    /// replace them with the disabled palette. So a faded row reads as *already
    /// dealt with* and a grey control still reads as *you cannot press this*, and
    /// the two do not collide.</para>
    /// <para>**NOTHING ABOUT THE ROW IS FORBIDDEN.** It keeps the full right-click
    /// menu with the same items, it is still clickable, and working the same
    /// station again on another band or another day is his choice.</para>
    /// <para>**0.55 RATHER THAN FAINTER.** It has to be obvious at a glance across
    /// a list of fourteen rows a slot, and it has to stay readable: this is the
    /// list he reads callsigns off, and a callsign he has to lean in for is worse
    /// than a label he has to ignore.</para>
    /// </remarks>
    public double RowOpacity => HasWorkedBefore || HeardNotReadable ? 0.55 : 1.0;

    /// <summary>**True where Hamlet can hear this carrier and cannot yet read it.**</summary>
    /// <remarks>
    /// <para>**AN EMPTY LIST AND A BAND FULL OF SIGNALS HAMLET CANNOT READ MUST NOT LOOK
    /// THE SAME** (§0.0, HM-DEC-092, work instruction 324 task 3). The search is sure this
    /// is a PSK31 carrier - that is why it is on the list at all - and the demodulator's
    /// squelch has not opened on it, so there is nothing to print. Before unit 324 that
    /// was a row with a blank message, which reads as *a station that has not said
    /// anything yet*, and on the operator's own evening it was a row that appeared and
    /// vanished two seconds later.</para>
    /// <para>**IT BORROWS UNIT 279'S 0.55 RATHER THAN INVENTING A SECOND FADE**, and for
    /// the same reason: recede, do not grey. Grey is reserved for what cannot be used
    /// (§0.5.1), and there is nothing wrong with this row - it is the truth about a signal
    /// on the air. **And it says so in words as well as by fading**, because color and
    /// weight may never be the only carriers of meaning (§0.6).</para>
    /// <para>**IT IS THE AUTHOR'S CHOICE AND IT IS MARKED AS ONE.** The owner may rule the
    /// row out entirely, or rule that it should not be dimmed; nothing else depends on
    /// it.</para>
    /// </remarks>
    public bool HeardNotReadable { get; init; }

    /// <summary>What working this station would open, if anything.</summary>
    /// <remarks>
    /// <para>**THE OTHER END OF UNIT 279 AXIS.** A worked station dims to 0.55; a
    /// marked one lifts. **Nothing between them moves** - an unmarked station is not
    /// a lesser station (§3.7), and the person he most wants to work may be an
    /// ordinary domestic contact.</para>
    /// <para>**SET BY THE PANEL, NOT DERIVED HERE.** The mark is sticky per station
    /// and capped, which is bookkeeping across the whole list rather than a fact
    /// about one row.</para>
    /// </remarks>
    public NudgeKind Nudge
    {
        get => _nudge;
        set
        {
            if (_nudge == value)
            {
                return;
            }

            _nudge = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nudge)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNudged)));
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(nameof(NudgeIsDoor)));
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(nameof(NudgeForm)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RowLift)));
        }
    }

    private NudgeKind _nudge;

    /// <summary>True where this row carries the quill.</summary>
    public bool IsNudged => _nudge != NudgeKind.None;

    /// <summary>True where what would open is an area he has never opened.</summary>
    /// <remarks>
    /// **THE ORBIT RING SAYS IT AND NO WORD DOES** (Tim, 2026-09-10). §3.1 is
    /// *absent, not dimmed*: the CQ list must not be what tells him an area exists.
    /// </remarks>
    public bool NudgeIsDoor => _nudge == NudgeKind.Door;

    /// <summary>Which of the two quills this row carries (§R16).</summary>
    /// <remarks>
    /// <para>**TWO KINDS, TOLD APART BY SHAPE AND BY COLOR** (Tim, 2026-09-11). A
    /// counter - a new country, state or grid with nothing opening behind it - is
    /// the still quill in decode green. A door - a first contact that opens a
    /// whole set - is the quill with the orbit ring, turning, in the palette's
    /// amber. Before this both drew the same lit ring and *a new country* and *a
    /// whole set opens* were one picture.</para>
    /// <para>**AN UNMARKED ROW ANSWERS `Tray` AND NOTHING DRAWS IT**, because the
    /// control is not visible at all where <see cref="IsNudged"/> is false. The
    /// value is never read in that state; the switch is total because a partial
    /// one would have to invent a fourth case.</para>
    /// </remarks>
    public AchievementMarkForm NudgeForm => _nudge switch
    {
        NudgeKind.Door => AchievementMarkForm.Door,
        NudgeKind.Visible => AchievementMarkForm.Counter,
        _ => AchievementMarkForm.Tray,
    };

    /// <summary>How far a marked row lifts above the rest.</summary>
    /// <remarks>
    /// **A LIFT PLUS THE QUILL, NEVER A LIFT ALONE** (Tim, 2026-09-10). On a night
    /// when the whole slot is new a relative lift marks nothing, and *it is a tiny
    /// dot lost in the sea of the tray* is the failure this avoids. The quill is
    /// absolute, already means *achievement* in this application, and reads with the
    /// colour taken away (§0.6).
    /// </remarks>
    public double RowLift => IsNudged ? 1.0 : 0.0;

    /// <summary>What the mark says on a deliberate look, or "".</summary>
    /// <remarks>
    /// **A VISIBLE CARD MAY BE NAMED AND A DOOR MAY NOT** (Tim, 2026-09-10, and
    /// §3.1). **Nothing here says *confirmed*** - Hamlet has contacts and no
    /// confirmations (§4) - and nothing promises the contact will succeed.
    /// </remarks>
    public string NudgeTip { get; set; } = "";

    /// <summary>**Why this station is interesting, for the press on the mark.**</summary>
    /// <remarks>
    /// **THE HOVER IS THE HEADLINE AND THIS IS THE PARAGRAPH** (work instruction 327 task
    /// 3). <see cref="NudgeTip"/> is unchanged and still one line; this is what a deliberate
    /// press gets. **It names the entity for a counter and names nothing for a door**
    /// (§3.1), and the word *confirmed* is not in it (§4).
    /// </remarks>
    public string NudgeReasonLine { get; set; } = "";

    /// <summary>What working him would earn, for the press on the mark.</summary>
    /// <remarks>
    /// **THE SAME LINE FOR BOTH KINDS** (<see cref="NudgeWords.Earns"/>). The QSO is the
    /// achievement and the card is what comes with it; nothing here promises the contact
    /// will succeed.
    /// </remarks>
    public string NudgeEarnsLine { get; set; } = "";

    /// <summary>Called when the mark is pressed, so the shell can write it down.</summary>
    /// <remarks>
    /// <para>**THE ROW DOES NOT KNOW WHAT TELEMETRY IS** (§0.1's shape, one level in). It
    /// is handed something to call; whether that writes a line, counts a press or does
    /// nothing is the panel's business, and a row built by a test has no sink and needs
    /// none.</para>
    /// <para>**IT CARRIES THE KIND AND NOTHING ELSE** (HM-DEC-018, §2.1). Not the callsign,
    /// not the entity, not the continent - `counter` or `door`.</para>
    /// </remarks>
    public Action<NudgeKind>? NudgeOpened { get; set; }

    /// <summary>True while the reason for this row's mark is on screen.</summary>
    /// <remarks>
    /// **IT LIVES ON THE ROW AND NOT ON THE WINDOW**, for the reason the conversation
    /// card's own map does: the list holds as many marked rows as the band offers, and one
    /// shared *a reason is open* would open the wrong one the moment there are two.
    /// </remarks>
    public bool NudgeIsOpen
    {
        get => _nudgeIsOpen;
        set
        {
            if (_nudgeIsOpen == value)
            {
                return;
            }

            _nudgeIsOpen = value;

            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(nameof(NudgeIsOpen)));
        }
    }

    private bool _nudgeIsOpen;

    /// <summary>Open the reason for this row's mark.</summary>
    /// <remarks>
    /// <para>**IT TRANSMITS NOTHING** (§0.2). The mark was already a nudge and never an
    /// arming; pressing it is the operator reading, and nothing on this path touches,
    /// shortens or pre-arms any route to the transmitter.</para>
    /// <para>**AND IT DOES NOTHING ON AN UNMARKED ROW.** The control is not drawn there, but
    /// a command that would have worked if it were is a different thing from one that
    /// refuses (§0.5.1, HM-DEC-087), and only the second survives somebody arriving by
    /// keyboard.</para>
    /// </remarks>
    public ICommand OpenTheNudgeCommand => _openTheNudge ??= new RelayCommand(OpenTheNudge);

    private ICommand? _openTheNudge;

    private void OpenTheNudge()
    {
        if (!IsNudged)
        {
            return;
        }

        NudgeIsOpen = true;

        NudgeOpened?.Invoke(_nudge);
    }

    /// <summary>Put the reason away.</summary>
    /// <remarks>
    /// **THE DISMISS X**, the same one the enlarged map carries. Nothing is lost by closing
    /// it: the mark is still on the row and the hover still says its one line.
    /// </remarks>
    public ICommand CloseTheNudgeCommand
        => _closeTheNudge ??= new RelayCommand(() => NudgeIsOpen = false);

    private ICommand? _closeTheNudge;

    /// <summary>The hover text, or null where there is none.</summary>
    /// <remarks>
    /// **UNIT 274'S OWN WORDING, MOVED ONTO THE ROW** rather than onto a marker
    /// that no longer exists. **Null and not the empty string**, because Avalonia
    /// draws an empty tooltip for `""` and a blank box following the pointer down
    /// a list of unworked stations would be worse than the label this replaced.
    /// </remarks>
    public string? WorkedTip => HasWorkedBefore ? _workedBefore : null;

    /// <inheritdoc/>
    /// <remarks>
    /// **IT CAME BACK FOR THE WORKED-BEFORE MARK** (work instruction 274 task 4).
    /// Unit 252 took `INotifyPropertyChanged` off this record when the dimming it
    /// existed for was removed, on the reasoning that an event nobody raises is a
    /// promise the type cannot keep. Something raises one again: a row is drawn
    /// before the log is consulted, and again after he logs a contact, and without
    /// this the mark would appear only on rows that arrive afterwards.
    /// </remarks>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>What the `snr` cell says for a message whose ratio was not measured.</summary>
    /// <remarks>
    /// **NOT "while nothing measures one" ANY MORE.** Something does, since unit
    /// 251. This is what a message whose symbol sequence could not be recovered,
    /// or whose frame ran off the end of the slot, shows instead of a number.
    /// </remarks>
    public const string NoMeasurement = "—";

    /// <summary>
    /// Puts a measured ratio into the `snr` cell, or <see cref="NoMeasurement"/>.
    /// </summary>
    /// <param name="decibels">The ratio, or null where none was measured.</param>
    /// <returns>Whole decibels with an explicit sign, or the dash.</returns>
    /// <remarks>
    /// <para>**WHOLE DECIBELS, WITH THE SIGN ALWAYS SHOWN.** The remarks on this
    /// record say why the precision stops there. The sign is always drawn
    /// because most FT8 reports are negative and a bare `3` in a column of
    /// `-13`s reads as a missing minus rather than as a strong station.</para>
    /// <para>**AWAY FROM ZERO AT THE HALF**, which is what
    /// <see cref="MidpointRounding.AwayFromZero"/> gives and what
    /// <see cref="Math.Round(double)"/> does not: banker's rounding would send
    /// -13.5 and -14.5 to the same cell and nothing on the screen would say so.</para>
    /// </remarks>
    public static string FormatSnr(double? decibels)
    {
        if (decibels is not { } measured || double.IsNaN(measured) || double.IsInfinity(measured))
        {
            return NoMeasurement;
        }

        var whole = (int)Math.Round(measured, MidpointRounding.AwayFromZero);
        return whole.ToString("+0;-0;+0", CultureInfo.InvariantCulture);
    }

    /// <summary>The message split into its three fields, or null.</summary>
    /// <remarks>
    /// **NULL FOR ANYTHING THAT IS NOT PLAINLY THREE FIELDS.** Free text,
    /// telemetry and non-standard callsign forms are drawn as they arrived, with
    /// no colouring and no field tooltips, because labelling the wrong half of a
    /// message is worse than labelling none of it.
    /// </remarks>
    public Hamlet.RadioEngine.Contacts.Ft8MessageFields? Fields
        => IsTextOnly ? null : Ft8Vocabulary.Split(Message);

    /// <summary>Who the message is addressed to, or "".</summary>
    /// <remarks>
    /// **`Addressee` AND `Sender` RATHER THAN `To` AND `From`.** The record
    /// already has a static `From(Ft8Decode)` factory, and a property of the
    /// same name does not compile. The pair is renamed together so they stay
    /// symmetrical.
    /// </remarks>
    /// <remarks>
    /// **ON A PSK31 ROW IT IS THE LATEST MESSAGE'S ADDRESSEE** (work instruction 316 task 4),
    /// spelled the way an FT8 to-field spells a call to anyone - see <see cref="ReadAddressee"/> -
    /// so the CQ filter and <see cref="AddresseeHelp"/> answer it without changing.
    /// </remarks>
    public string Addressee
        => IsTextOnly ? ReadAddressee : Fields?.To ?? "";

    /// <summary>Who sent it, or "".</summary>
    /// <remarks>
    /// **ON A PSK31 ROW IT IS THE SPEAKER OF THE LATEST COMPLETE MESSAGE**, or nothing where no
    /// message has finished or the parser could not name one. The fade, the country in
    /// <see cref="SenderHelp"/> and the quill all ask this member, which is how they run on a
    /// PSK31 row with no change to their own code.
    /// </remarks>
    public string Sender
        => IsTextOnly ? Reading?.Speaker ?? "" : Fields?.From ?? "";

    /// <summary>True where a PSK31 row has a sender to pick out beside its text.</summary>
    public bool ShowsReadSender => IsTextOnly && Sender.Length > 0;

    /// <summary>A PSK31 row's addressee, spelled the way the CQ filter already reads one.</summary>
    /// <remarks>
    /// <para>**`ANY` IS HANDED OVER AS `CQ` AND `DX` AS `CQ DX`**, which is how an FT8 row's
    /// to-field spells a call to anyone, so `Ft8MessageSplit.IsCallToAnyone` - asked by the CQ
    /// filter - and <see cref="AddresseeHelp"/> both answer it as they always have.</para>
    /// <para>**NOTHING WHERE THERE IS NO SPEAKER.** An addressee nobody is saying is not a
    /// station calling anybody, and a CQ from nobody the text names is not a CQ the filter may
    /// show (§0.0).</para>
    /// </remarks>
    private string ReadAddressee
        => Reading is not { Speaker: not null } reading
            ? ""
            : reading.Addressee switch
            {
                Hamlet.RadioEngine.Psk31.Psk31ExchangeParser.Anyone => DxccPrefixes.Ft8CallToAnyone,
                Hamlet.RadioEngine.Psk31.Psk31ExchangeParser.Dx =>
                    DxccPrefixes.Ft8CallToAnyone + " " + Hamlet.RadioEngine.Psk31.Psk31ExchangeParser.Dx,
                null => "",
                var call => call,
            };

    /// <summary>The payload, or "".</summary>
    public string Payload => Fields?.Payload ?? "";

    /// <summary>The whole message, shown only where it has no three parts.</summary>
    /// <remarks>
    /// **THE TWO ARE EXCLUSIVE**, so a message never appears twice: either the
    /// three coloured fields are shown, or this is.
    /// </remarks>
    public string Unsplit => Fields is null ? Message : "";

    /// <summary>True where the message has three fields to colour.</summary>
    public bool HasFields => Fields is not null;

    /// <summary>Hover text naming the addressee field.</summary>
    /// <remarks>
    /// **STRUCTURE, NOT MEANING.** Saying which field is the addressee is a fact
    /// about the message format. It does not say who the station is, where they
    /// are, or why they are calling.
    /// </remarks>
    public string AddresseeHelp
        => string.Equals(Addressee, "CQ", StringComparison.OrdinalIgnoreCase)
            || Addressee.StartsWith("CQ ", StringComparison.OrdinalIgnoreCase)
            ? "Who this is addressed to. CQ means anyone."
            : "Who this is addressed to.";

    /// <summary>Hover text naming the sender field, with the entity where certain.</summary>
    /// <remarks>
    /// <para>**THE COUNTRY ONLY WHERE IT IS CERTAIN** (Tim's ruling, 2026-09-05:
    /// *say nothing if we don't know*). Where `DxccPrefixes` declines — a shared
    /// prefix, a form it does not handle, a station at sea — the line is exactly
    /// what unit 241 left and carries no hedge in place of the answer.</para>
    /// <para>**IT IS A FACT ABOUT THE LICENCE AND NEVER A LOCATION.**
    /// HM-DEC-038 forbids deriving a distance or a position from a prefix and
    /// that stands: this says which entity issued the callsign, and how far away
    /// the station is comes from a grid square or from nowhere.</para>
    /// </remarks>
    public string SenderHelp
    {
        get
        {
            // **HE KNOWS WHO SENT IT** (Tim, 2026-09-08, work instruction 281 task
            // 5). Observed on his own CQ: Hamlet told him his own callsign is from
            // the United States and offered to work out how far away he is. A
            // message he sent gets no sender tooltip at all.
            if (IsSent)
            {
                return "";
            }

            return DxccPrefixes.EntityOf(Sender) is { } entity
                ? $"Who sent it. {Sender} is a callsign from "
                  + Sentence(EntitySpoken.Of(entity))
                : "Who sent it.";
        }
    }

    /// <summary>An entity name finished as a sentence.</summary>
    /// <remarks>
    /// **THE ARRL'S OWN NAMES ABBREVIATE, AND SOME END IN A STOP.** `Canary Is.`
    /// and `Rodrigues I.` are its wording and the wording is kept, because a name
    /// tidied is a name that no longer matches the source it is cited from.
    /// Appending a second full stop put `Canary Is..` on the screen, so the stop
    /// is added only where there is not one already.
    /// </remarks>
    private static string Sentence(string entity)
        => entity.EndsWith('.') ? entity : entity + ".";

    /// <summary>
    /// Hover text for the payload, from the closed table, or "" for silence.
    /// </summary>
    /// <remarks>
    /// <para>**AN EMPTY STRING AND NOT A FALLBACK SENTENCE.** Avalonia shows no
    /// tooltip for an empty tip, which is exactly what Tim's ruling asks for:
    /// anything off the list gets nothing, not "unrecognised".</para>
    /// <para>**THE WHOLE MESSAGE GOES IN, NOT THE PAYLOAD ALONE** (unit 251 task
    /// 4). A report is a report from one named station to another, and until now
    /// this handed `Explain` three characters with no way of knowing whose report
    /// it was — so it said *hears you*, about a contact the operator was not in.</para>
    /// <para>**AND A MESSAGE HE SENT IS NOT EXPLAINED BACK TO HIM** (Tim,
    /// 2026-09-08, work instruction 281 task 5). Every sentence this table produces
    /// is about the sender: who they are, where they are, what they are asking for.
    /// On his own transmission the sender is him, and he composed it, saw it before
    /// it went, and clicked once to send it. <see cref="DirectionTip"/> still says
    /// when it went out.</para>
    /// </remarks>
    public string PayloadHelp
        => IsSent ? "" : Ft8Vocabulary.Explain(Fields, ObserverGrid) ?? "";

    /// <summary>True where the payload is on the list and has hover text.</summary>
    public bool HasPayloadHelp => PayloadHelp.Length > 0;

    /// <summary>Puts a decode into the table's five columns.</summary>
    /// <param name="decode">What came out of the slot.</param>
    /// <returns>The row.</returns>
    /// <exception cref="ArgumentNullException">The decode is null.</exception>
    public static DigitalDecodeRow From(Ft8Decode decode)
    {
        ArgumentNullException.ThrowIfNull(decode);

        return new DigitalDecodeRow(
            decode.SlotStartUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            FormatSnr(decode.SignalToNoiseDb),
            decode.OffsetSeconds.ToString("0.0", CultureInfo.InvariantCulture),
            decode.FrequencyHz.ToString("0", CultureInfo.InvariantCulture),
            decode.Message,
            ObserverGrid: "",
            SlotStartUtc: decode.SlotStartUtc);
    }
}
