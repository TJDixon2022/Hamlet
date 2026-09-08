using System.ComponentModel;
using System.Globalization;
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
public sealed record DigitalDecodeRow(
    string Utc, string Snr, string Dt, string Hz, string Message,
    string ObserverGrid = "",
    DateTime SlotStartUtc = default,
    string Contact = "",
    long HeardOnHz = 0,
    bool IsSent = false)
    : INotifyPropertyChanged
{
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
    /// **ONE PLACE BUILDS A SENT ROW**, so the empty cells cannot be filled in by
    /// a second call site in a hurry. The `Utc` cell is formatted exactly as a
    /// decoded row's is, because the two are read down one column.
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
    public string Shown
        => RepeatCount > 1
            ? Message + " x" + RepeatCount.ToString(CultureInfo.InvariantCulture)
            : Message;

    /// <summary>The time, and whether he sent it, beneath the message.</summary>
    /// <remarks>
    /// <para>**REMOVING WORDS IS NOT REMOVING FACTS** (Tim's ruling, 2026-09-08,
    /// and the standing §0.0 rule). The conversation now carries its direction in
    /// the alignment - his own on the right, what he heard on the left - which is
    /// what a reader takes in without reading anything. **The word stays anyway**,
    /// because alignment alone is a shape and §0.6 does not let a shape be the only
    /// carrier of a meaning.</para>
    /// <para>**AND THE TIME STAYS.** It is the one thing on the row that lets him
    /// see he answered a slot late, which is the whole reason the conversation was
    /// built.</para>
    /// </remarks>
    public string Caption
        => IsSent ? Utc + " · sent" : Utc;

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
    public double RowOpacity => HasWorkedBefore ? 0.55 : 1.0;

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
        => Ft8Vocabulary.Split(Message);

    /// <summary>Who the message is addressed to, or "".</summary>
    /// <remarks>
    /// **`Addressee` AND `Sender` RATHER THAN `To` AND `From`.** The record
    /// already has a static `From(Ft8Decode)` factory, and a property of the
    /// same name does not compile. The pair is renamed together so they stay
    /// symmetrical.
    /// </remarks>
    public string Addressee => Fields?.To ?? "";

    /// <summary>Who sent it, or "".</summary>
    public string Sender => Fields?.From ?? "";

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
        => DxccPrefixes.EntityOf(Sender) is { } entity
            ? $"Who sent it. {Sender} is a callsign from {Sentence(entity)}"
            : "Who sent it.";

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
    /// </remarks>
    public string PayloadHelp => Ft8Vocabulary.Explain(Fields, ObserverGrid) ?? "";

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
