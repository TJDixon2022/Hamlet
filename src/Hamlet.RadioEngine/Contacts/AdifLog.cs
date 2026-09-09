using System.Globalization;
using System.Text;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>One contact, as the fields ADIF keeps it in.</summary>
/// <remarks>
/// <para>**EVERY FIELD IS NULLABLE AND NULL MEANS NOT OBSERVED** (§0.0, and the
/// instruction's own rule). A field Hamlet did not observe is **absent from the
/// record**, not written empty and not written with a plausible value. A log
/// outlives everything else in this project: an entry carrying a report Hamlet
/// never heard is a false record of what happened on the air, and there is nothing
/// later that can tell it from a true one.</para>
/// <para>**THE OPERATOR'S NOTES ARE A FIELD LIKE ANY OTHER AND OVERWRITE
/// NOTHING.** They go in `COMMENT`; no typed value is ever put in an observed
/// field.</para>
/// </remarks>
public sealed record AdifContact
{
    /// <summary>The station worked. ADIF `CALL`.</summary>
    public string? Call { get; init; }

    /// <summary>The operator's own callsign. ADIF `STATION_CALLSIGN`.</summary>
    public string? StationCallsign { get; init; }

    /// <summary>When the contact started, in UTC. ADIF `QSO_DATE` and `TIME_ON`.</summary>
    public DateTime? StartedUtc { get; init; }

    /// <summary>When it ended, in UTC. ADIF `TIME_OFF`.</summary>
    public DateTime? EndedUtc { get; init; }

    /// <summary>The band, as ADIF's own enumeration spells it. ADIF `BAND`.</summary>
    public string? Band { get; init; }

    /// <summary>The dial frequency in megahertz. ADIF `FREQ`.</summary>
    public double? FrequencyMhz { get; init; }

    /// <summary>The mode. ADIF `MODE`.</summary>
    public string? Mode { get; init; }

    /// <summary>The submode, where the mode has one. ADIF `SUBMODE`.</summary>
    /// <remarks>
    /// <para>**NULL IS THE ORDINARY ANSWER AND NOT A GAP.** Most modes stand on
    /// their own — `FT8` is a Mode and takes no submode — so a record with
    /// `MODE=FT8` and nothing here is complete rather than missing something. The
    /// field is null on every record Hamlet wrote before work instruction 291 and
    /// on every FT8 record it will ever write.</para>
    /// <para>**IT NAMES THE MODE WHERE `MODE` CANNOT.** `MODE=MFSK` on its own is
    /// *some kind of multi-frequency shift keying*; the pair `MODE=MFSK,
    /// SUBMODE=FT4` is FT4 and is what every other logger has a row for. See
    /// <see cref="ContactModes"/>, which holds the pairs, and never set this
    /// without setting <see cref="Mode"/> from the same source.</para>
    /// </remarks>
    public string? Submode { get; init; }

    /// <summary>The report the operator sent. ADIF `RST_SENT`.</summary>
    public string? ReportSent { get; init; }

    /// <summary>The report the other station sent. ADIF `RST_RCVD`.</summary>
    public string? ReportReceived { get; init; }

    /// <summary>The other station's locator. ADIF `GRIDSQUARE`.</summary>
    public string? GridSquare { get; init; }

    /// <summary>The operator's own locator. ADIF `MY_GRIDSQUARE`.</summary>
    public string? MyGridSquare { get; init; }

    /// <summary>What the operator typed. ADIF `COMMENT`.</summary>
    public string? Comment { get; init; }
}

/// <summary>One record as the file actually holds it, damage included.</summary>
/// <remarks>
/// <para>**A LOG THAT QUIETLY DROPS A RECORD IS WORSE THAN ONE THAT SHOWS A BAD
/// ONE** (work instruction 278). The plain reader cannot tell a caller the
/// difference between a field Hamlet never observed and a field it wrote and cannot
/// read back: both arrive as null. A window drawing them alike tells the operator
/// his radio did not know the band when the truth is that the file is damaged.</para>
/// <para>**IT CARRIES NO OPINION ABOUT WHAT TO DO.** Nothing here hides, drops or
/// repairs a record. It reports, and what to draw is the view's business.</para>
/// </remarks>
/// <param name="Contact">Everything that could be read, with the rest left null.</param>
/// <param name="Faults">
/// What could not be read, in the operator's words rather than the parser's, empty
/// where the record is sound.
/// </param>
/// <param name="Terminated">
/// **False where the record has no `&lt;EOR&gt;`**, which means the file was cut off
/// while it was being written. Such a record is returned rather than dropped, and
/// <see cref="AdifLog.Read"/> still leaves it out, because every caller of that
/// wants contacts the operator finished writing.
/// </param>
public sealed record AdifLogRecord(
    AdifContact Contact, IReadOnlyList<string> Faults, bool Terminated)
{
    /// <summary>True where the record is whole and every field was readable.</summary>
    public bool IsSound => Terminated && Faults.Count == 0;
}

/// <summary>
/// Reads and writes the ADI data format, so a contact Hamlet logged opens in
/// somebody else's logger.
/// </summary>
/// <remarks>
/// <para>**THE SOURCE, CITED, BECAUSE THE TAG NAMES ARE NOT THIS FILE'S TO
/// INVENT.** Everything below is read from the **ADIF Specification, version
/// 3.1.4, released 6 December 2022**, at
/// `https://www.adif.org/314/ADIF_314.htm`, retrieved 2026-09-07. Writing tag
/// names from memory is the pattern this project has spent a fortnight learning
/// not to trust, and a logger rejects a wrong one without saying why.</para>
/// <para>**WHAT THE SPECIFICATION SAYS, QUOTED.** A field is
/// `&lt;FIELDNAME:length[:datatype]&gt;value`. A QSO record is terminated by
/// `&lt;EOR&gt;`; a header is terminated by `&lt;EOH&gt;`. `QSO_DATE` is "8 Digits
/// representing a UTC date in YYYYMMDD format". `TIME_ON` is "6 Digits
/// representing a UTC time in HHMMSS format or 4 Digits representing a time in
/// HHMM format" — six is written here, because a slot boundary has seconds and
/// throwing them away would lose a fact the ledger holds. `String` is "a sequence
/// of Characters", ASCII 32 to 126. The Band enumeration gives `20m` for
/// "14.0" to "14.35" MHz. `FT8` is a Mode and takes no submode.</para>
/// <para>**`SUBMODE` IS THE COMPANION `MODE` NEEDS FOR THREE OF THE SIX, AND ITS
/// NAME IS A READING RATHER THAN A FETCH** (work instruction 291 task 1). It
/// carries the specific mode where `MODE` carries only the family: `MODE=MFSK`
/// alone is *some kind of multi-frequency shift keying*, and it is the pair
/// `MODE=MFSK, SUBMODE=FT4` that names FT4. **`MODE=FT4` is not valid ADIF** and a
/// logger has no row for it. Where a mode stands on its own the field is absent,
/// which is every FT8 record.</para>
/// <para>**WHAT COULD NOT BE DONE, SAID PLAINLY (§12.4).** This session could not
/// reach `https://www.adif.org/314/ADIF_314.htm` — the shell refused the fetch, so
/// there was no page to truncate — and there is no pinned copy under `data/vendor/`.
/// The tag name `SUBMODE` and the pairing `MFSK`/`FT4` therefore rest on **unit
/// 287's in-tree reading of the same edition**, recorded at
/// <see cref="ContactModes.Cite"/> as retrieved 2026-09-08 and quoted at
/// `ContactModes.cs:98-104`. **That is a reading carried forward, not a fetch made
/// here**, and it is marked the way `FREQ` below is marked rather than written with
/// the confidence of something this session verified. It was not recalled from
/// memory, which is the one thing forbidden.</para>
/// <para>**THE LENGTH IS THE ESCAPE, AND THAT IS WHY NOTHING IS ESCAPED HERE.**
/// ADI has no escape character: a reader takes exactly `length` characters after
/// the `&gt;`. So an angle bracket, a colon or a newline inside the operator's
/// notes survives untouched, and quoting them would corrupt them. The round-trip
/// test is what proves it rather than this paragraph.</para>
/// <para>**ONE THING IS NOT FULLY CITED AND IT IS MARKED RATHER THAN GLOSSED.**
/// `FREQ`'s own field definition could not be retrieved — the specification is one
/// very large page and the fetch truncated before the field-definition table. It
/// is written in **megahertz**, which is what the Band enumeration on the same page
/// states its own edges in. `BAND` is written beside it and **is** fully cited, so
/// a reader has a cross-check on the face of every record, and a unit error would
/// be visible the first time the file is imported rather than silent. Said out
/// loud because §12.4 would rather have a marked assumption than a confident
/// one.</para>
/// </remarks>
public static class AdifLog
{
    /// <summary>A band name as the ADIF Band enumeration spells it.</summary>
    /// <param name="displayName">Hamlet's own display name, e.g. `20 m`.</param>
    /// <returns>The enumeration value, e.g. `20m`, or null for nothing.</returns>
    /// <remarks>
    /// <para>**HAMLET SAYS `20 m` AND ADIF SAYS `20m`, AND UNIT 274 WROTE THE
    /// FIRST ONE INTO THE FILE.** `HfBands` names bands for the screen —
    /// `HfBands.cs:76` — and the log took that name straight through. The
    /// specification's Band enumeration gives **`20m`** for "14.0" to "14.35" MHz,
    /// quoted from the fetch of 2026-09-07, and a logger reading `20 m` has no row
    /// for it.</para>
    /// <para>**WHY THE ROUND TRIP DID NOT CATCH IT.** Unit 274's test wrote a
    /// hand-made `"20m"` and read back `"20m"`, so the writer and the reader agreed
    /// perfectly about a value the application never produces. That is §12.5 in
    /// miniature: a fixture built from the same assumption as the code proves
    /// nothing about the code. What caught it was work instruction 275 task 3,
    /// which pushed the app's **own** band name through for the first time.</para>
    /// <para>**IT REMOVES SPACE AND NOTHING ELSE.** This is a format conversion
    /// rather than a translation: every ADIF band value is a number and a unit run
    /// together, and every `HfBands` name is the same two with a space between
    /// them. Nothing is renamed, mapped or guessed, and a name this cannot
    /// recognise comes back null so the field is absent rather than wrong.</para>
    /// </remarks>
    public static string? BandValueFor(string? displayName)
    {
        var name = (displayName ?? "").Replace(" ", "").Trim().ToLowerInvariant();

        // A band value is digits then `m` or `cm`. Anything else is not one, and
        // an unrecognised name is left out rather than written wrong.
        var digits = 0;

        while (digits < name.Length && char.IsAsciiDigit(name[digits]))
        {
            digits++;
        }

        var unit = name[digits..];

        return digits > 0 && unit is "m" or "cm" ? name : null;
    }

    /// <summary>The specification this file was written from.</summary>
    public const string Specification =
        "ADIF Specification 3.1.4, released 2022-12-06, "
        + "https://www.adif.org/314/ADIF_314.htm, retrieved 2026-09-07";

    /// <summary>What Hamlet calls itself in the header.</summary>
    private const string ProgramId = "Hamlet";

    /// <summary>The ADIF version this file claims to be.</summary>
    private const string AdifVersion = "3.1.4";

    /// <summary>The header a new log file opens with.</summary>
    /// <param name="programVersion">Hamlet's own version, for `PROGRAMVERSION`.</param>
    /// <returns>The header text, terminated by its `&lt;EOH&gt;`.</returns>
    /// <remarks>
    /// **THE TEXT BEFORE THE FIRST FIELD IS A COMMENT AND IS ALLOWED.** ADI files
    /// conventionally open with a line of prose before the header fields; a reader
    /// takes everything before the first `&lt;` as a comment. It is used here to
    /// say what wrote the file, because somebody opening it in a text editor in
    /// five years should not have to guess.
    /// </remarks>
    public static string Header(string programVersion)
    {
        var text = new StringBuilder();

        text.Append("Hamlet contact log. ADI format, ")
            .Append(Specification)
            .Append('.')
            .Append('\n')
            .Append("Fields Hamlet did not observe are absent rather than empty.")
            .Append('\n')
            .Append('\n');

        Field(text, "ADIF_VER", AdifVersion);
        Field(text, "PROGRAMID", ProgramId);
        Field(text, "PROGRAMVERSION", programVersion);
        text.Append("<EOH>").Append('\n');

        return text.ToString();
    }

    /// <summary>One contact as an ADI record.</summary>
    /// <param name="contact">The contact.</param>
    /// <returns>The record, terminated by its `&lt;EOR&gt;`.</returns>
    /// <exception cref="ArgumentNullException">The contact is null.</exception>
    /// <remarks>
    /// **THE ORDER IS THIS FILE'S AND MEANS NOTHING TO A READER.** ADI is a tagged
    /// format and a reader takes fields in whatever order it finds them; they are
    /// written in the order a person would read them so the file is legible in an
    /// editor.
    /// </remarks>
    public static string Record(AdifContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        var text = new StringBuilder();

        Field(text, "CALL", contact.Call);
        Field(text, "STATION_CALLSIGN", contact.StationCallsign);

        if (contact.StartedUtc is { } started)
        {
            Field(text, "QSO_DATE", started.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            Field(text, "TIME_ON", started.ToString("HHmmss", CultureInfo.InvariantCulture));
        }

        // **NO DATE, NO END TIME.** `TIME_OFF` has no date of its own: the
        // record's date is `QSO_DATE`, which comes from the start. An end time
        // written without one is a time of day belonging to no day, and a reader
        // would have to invent the date to make sense of it — which is the
        // guess §0.0 forbids, made by somebody else's program on our behalf.
        //
        // The ledger cannot produce this (the end is the last message and the
        // start the first, so one implies the other); it is refused here rather
        // than relied on not to happen.
        if (contact.EndedUtc is { } ended && contact.StartedUtc is not null)
        {
            Field(text, "TIME_OFF", ended.ToString("HHmmss", CultureInfo.InvariantCulture));
        }

        Field(text, "BAND", contact.Band);
        Field(text, "MODE", contact.Mode);

        // **BESIDE `MODE` BECAUSE A PERSON READS THEM AS A PAIR.** The order means
        // nothing to a reader (see the remark above); it means a great deal to
        // somebody opening the file in an editor to check what he worked.
        // `Field` writes nothing at all where there is no submode, which is every
        // FT8 record and most others.
        Field(text, "SUBMODE", contact.Submode);

        if (contact.FrequencyMhz is { } mhz)
        {
            // Six decimal places is one hertz at HF, which is the resolution the
            // radio reports the dial at. Trailing zeros are kept rather than
            // trimmed: a fixed shape is easier to read down a column.
            Field(text, "FREQ", mhz.ToString("0.000000", CultureInfo.InvariantCulture));
        }

        Field(text, "RST_SENT", contact.ReportSent);
        Field(text, "RST_RCVD", contact.ReportReceived);
        Field(text, "GRIDSQUARE", contact.GridSquare);
        Field(text, "MY_GRIDSQUARE", contact.MyGridSquare);
        Field(text, "COMMENT", contact.Comment);

        text.Append("<EOR>").Append('\n');

        return text.ToString();
    }

    /// <summary>Read every QSO record out of an ADI file's text.</summary>
    /// <param name="text">The file's whole content.</param>
    /// <returns>One contact per record, in the order they appear.</returns>
    /// <remarks>
    /// <para>**THIS EXISTS FOR THE ROUND TRIP AND IT IS NOT A GENERAL READER.** It
    /// reads what <see cref="Record"/> writes, so the test can assert that every
    /// field came back as it went in — which is the only thing that catches a
    /// length off by one, and a length off by one is what makes another program
    /// reject the file without saying why.</para>
    /// <para>**ANYTHING BEFORE `&lt;EOH&gt;` IS THE HEADER AND IS SKIPPED.** A file
    /// with no header is read from the top, which is what the format allows.</para>
    /// </remarks>
    public static IReadOnlyList<AdifContact> Read(string? text)
    {
        var records = ReadRecords(text);
        var contacts = new List<AdifContact>(records.Count);

        foreach (var record in records)
        {
            // **THE UNTERMINATED TAIL IS NOT A RECORD TO THIS READER.** It was
            // dropped silently before <see cref="ReadRecords"/> existed and it is
            // dropped deliberately here, because every caller of this method wants
            // contacts the operator finished writing. The one that wants to see the
            // damage asks for it by name.
            if (record.Terminated)
            {
                contacts.Add(record.Contact);
            }
        }

        return contacts;
    }

    /// <summary>
    /// **Read every record, and say what could not be read**, for a reader that
    /// has to show damage rather than inherit it.
    /// </summary>
    /// <param name="text">The file's whole content.</param>
    /// <returns>One entry per record, in file order, faults and all.</returns>
    /// <remarks>
    /// <para>**ONE PARSER, TWO DOORS.** <see cref="Read"/> is written in terms of
    /// this and there is no second scan of the text anywhere in this file. A second
    /// ADI parser is exactly the thing work instruction 278 forbids, and it would
    /// disagree with this one the first time either was touched.</para>
    /// <para>**WHY IT EXISTS.** The plain reader cannot tell a caller the
    /// difference between a field Hamlet never observed and a field it wrote and
    /// cannot read back. Both arrive as null, and a log window drawing them the
    /// same way tells the operator his radio did not know the band when in fact the
    /// file is damaged. **A log that quietly drops a record is worse than one that
    /// shows a bad one**, and the same is true of a field.</para>
    /// <para>**IT NEVER THROWS AND NEVER STOPS.** A fault is recorded against the
    /// record it was found in and the scan carries on, so one bad byte costs one
    /// field rather than the rest of the file.</para>
    /// </remarks>
    public static IReadOnlyList<AdifLogRecord> ReadRecords(string? text)
    {
        // **THE HEADER IS FOUND BY PARSING AND NEVER BY SEARCHING FOR `<EOH>`.**
        // The first draft did `IndexOf("<EOH>")` and a note reading
        // `100% <> :: <EOH> <EOR>` cut the file in half at the operator's own
        // words: the terminator inside a value is ordinary text, and only a
        // parser that skips values by their declared length can tell the two
        // apart. It came back as two records, one of them nonsense.
        var body = text ?? "";

        var records = new List<AdifLogRecord>();
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var faults = new List<string>();
        var started = false;
        var i = 0;

        while (i < body.Length)
        {
            var open = body.IndexOf('<', i);

            if (open < 0)
            {
                break;
            }

            var close = body.IndexOf('>', open);

            if (close < 0)
            {
                // **A TAG THAT NEVER CLOSES ENDS THE FILE**, and the record it was
                // in is unterminated. Saying so is the whole point of this method:
                // before it, this was where a record vanished.
                faults.Add(
                    "the file ends in the middle of a tag, so the rest of this "
                    + "record was never written");
                started = true;
                break;
            }

            var tag = body[(open + 1)..close];

            if (string.Equals(tag, "EOR", StringComparison.OrdinalIgnoreCase))
            {
                records.Add(new AdifLogRecord(From(fields), faults.ToArray(), true));
                fields.Clear();
                faults.Clear();
                started = false;
                i = close + 1;
                continue;
            }

            // **THE HEADER ENDS HERE AND ITS FIELDS ARE NOT A CONTACT.**
            // `ADIF_VER` and `PROGRAMID` are above it; kept, they would come back
            // as a first record with a version number in it.
            if (string.Equals(tag, "EOH", StringComparison.OrdinalIgnoreCase))
            {
                fields.Clear();
                faults.Clear();
                started = false;
                i = close + 1;
                continue;
            }

            started = true;

            var parts = tag.Split(':');

            if (parts.Length < 2 || !int.TryParse(
                    parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var length))
            {
                // **THE FIELD IS NAMED, BECAUSE WHICH ONE IS MISSING IS THE POINT.**
                // A record that lost its band reads very differently from one that
                // lost a note.
                faults.Add(
                    "a field written as \"" + tag + "\" has no readable length, so "
                    + "its value could not be found and the field was left out");
                i = close + 1;
                continue;
            }

            // **EXACTLY `length` CHARACTERS, WHICH IS THE WHOLE FORMAT.** Nothing
            // is searched for and nothing is unescaped: the count is what makes an
            // angle bracket inside a note ordinary text.
            var from = close + 1;
            var take = Math.Min(length, body.Length - from);

            if (take < length)
            {
                faults.Add(
                    "the field \"" + parts[0] + "\" says it is " + length
                    + " characters and the file ends after " + take
                    + ", so it was cut off");
            }

            fields[parts[0]] = body.Substring(from, take);
            i = from + take;
        }

        // **THE TAIL IS EMITTED RATHER THAN DROPPED.** A file truncated mid-record
        // used to lose that record with no trace anywhere, which is the silent skip
        // this method was written to end.
        if (started || fields.Count > 0)
        {
            faults.Add(
                "this record has no end marker, so the file was cut off while it "
                + "was being written");

            records.Add(new AdifLogRecord(From(fields), faults.ToArray(), false));
        }

        return records;
    }

    /// <summary>Turn one record's fields into a contact.</summary>
    private static AdifContact From(IReadOnlyDictionary<string, string> fields)
    {
        return new AdifContact
        {
            Call = Get(fields, "CALL"),
            StationCallsign = Get(fields, "STATION_CALLSIGN"),
            StartedUtc = Moment(Get(fields, "QSO_DATE"), Get(fields, "TIME_ON")),
            EndedUtc = Moment(Get(fields, "QSO_DATE"), Get(fields, "TIME_OFF")),
            Band = Get(fields, "BAND"),
            Mode = Get(fields, "MODE"),
            Submode = Get(fields, "SUBMODE"),
            FrequencyMhz = Get(fields, "FREQ") is { } f
                && double.TryParse(
                    f, NumberStyles.Float, CultureInfo.InvariantCulture, out var mhz)
                ? mhz
                : null,
            ReportSent = Get(fields, "RST_SENT"),
            ReportReceived = Get(fields, "RST_RCVD"),
            GridSquare = Get(fields, "GRIDSQUARE"),
            MyGridSquare = Get(fields, "MY_GRIDSQUARE"),
            Comment = Get(fields, "COMMENT"),
        };
    }

    private static string? Get(IReadOnlyDictionary<string, string> fields, string name)
        => fields.TryGetValue(name, out var value) ? value : null;

    /// <summary>A date and a time back into a moment, or null.</summary>
    private static DateTime? Moment(string? date, string? time)
    {
        if (date is null || time is null)
        {
            return null;
        }

        var formats = time.Length == 4 ? "yyyyMMddHHmm" : "yyyyMMddHHmmss";

        return DateTime.TryParseExact(
            date + time, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var moment)
            ? moment
            : null;
    }

    /// <summary>Write one field, or nothing at all where there is no value.</summary>
    /// <remarks>
    /// <para>**NO VALUE MEANS NO FIELD** (§0.0). Not `&lt;RST_SENT:0&gt;`, which
    /// asserts an empty report was exchanged, and not a plausible `59`. The
    /// absence is the honest answer and a logger reads it as one.</para>
    /// <para>**THE LENGTH IS IN BYTES.** For the ASCII the specification's `String`
    /// type permits, a character is a byte; the count is taken from the UTF-8
    /// encoding so that a note carrying something outside ASCII still declares a
    /// length a reader can trust, rather than being silently stripped of the
    /// operator's own words. That such a note is outside what `String` defines is
    /// said here rather than fixed by mangling it.</para>
    /// </remarks>
    private static void Field(StringBuilder text, string name, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        text.Append('<')
            .Append(name)
            .Append(':')
            .Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture))
            .Append('>')
            .Append(value)
            .Append('\n');
    }
}
