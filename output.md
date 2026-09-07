READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air. It transmitted on a live
   antenna on 2026-09-07, and since unit 271 every composed message round-trips
   through the decoder.
B. THE STEP AND ITS EXIT CRITERIA. Step E's bench half. It closes no criterion and
   does not claim to.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A: a
   completed contact can be written down in one click, the file is ADIF a logger
   will read, and a station already worked is obvious before he calls it. **Section
   4 has no ask** — three things are stated rather than asked, and the queue is
   carried outbound unchanged.

UNIT:       274 — complete at task 5 of 5 — 2026-09-07 19:13
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  A completed contact can be logged in one click, the entry says what was observed and what was typed, the file is ADIF a logger will read, and a station already worked is obvious before he calls it.
ADVANCED:   yes — the log exists and the first contact is uploadable the day it lands. No step's criterion moved and the instruction says so itself.
NUMBER:     265 bytes an entry, and the already-worked check runs **once** rather than per decode. There was no such figure before; task 5 is the first measurement of it.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, five of five.** The named drop candidate was attempted. Machine
`QUIVERFULL`, `C:\Source\HamLet`, branch `main`, all pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**
`SESSION.lock` absent, one session, tree clean at `3232dc2` with unit 273 fully in.

### Shell refusals, recorded verbatim

**None.** The same two tool facts as last unit and no new ones: this shell will not
carry a quoted heredoc containing an apostrophe, and it collapses a doubled
backslash inside one. Both were worked around with script files and the
file-editing tools.

### Task by task

**1 — what the ledger knows.** Reading only, committed as
`docs/unit274-what-the-ledger-knows.md` with file and line for each fact. Its two
findings shaped everything after it.

**2 — the ADIF writer.** `AdifLog` in the engine, from the fetched specification.

**3 and 4 — the dialog and the mark.** Committed together at `da2b903`, because the
mark is what a written log is *for* and the write is what puts a station in it.

**5 — what the log costs.** Measured, nothing optimised.

### Decisions made on this session's own authority, reproduced in full

**One: a grid is a property of the sender and a report is a property of the pair,
so they read different lists.** The first draft read both off `HeardToUs` and lost
the grid on a complete exchange — because **that exchange opens with his CQ**,
which is how nearly every contact opens, and a CQ is addressed to nobody in
particular. `CQ IK4LZH JN54` says where IK4LZH is whoever he was calling. A report
cannot be read the same way: `-12` in a message to W1ABC is what he heard **W1ABC**
at, and putting it in this entry would be a stranger's number in the operator's
log. The grid now reads `Heard`; the reports still read `HeardToUs` and `Sent`.

**Two: compound calls are different stations for the worked-before mark.** The
instruction says to say what I did and to treat them as different where the answer
is not certain. `W4/YV7AXM` and `YV7AXM` are the same licensee and arguably not the
same contact: one is a Venezuelan station at home, the other that licensee in
Florida, which is a different DXCC entity and a different contact to most award
programmes. **The mark under-claims rather than over-claims**, so he is never told
he has worked somebody he has not.

**Three: `INotifyPropertyChanged` came back to `DigitalDecodeRow`.** Unit 252
removed it when the dimming it existed for went, on the reasoning that an event
nobody raises is a promise the type cannot keep. Something raises one again: a row
is drawn before the log is consulted and again after he logs a contact, and without
it the mark would appear only on rows arriving afterwards.

### What the tests found that reading would not have

**Three real defects, all in code I had just written.**

- **A note containing `<EOH>` cut the file in half.** The reader found the header
  terminator by searching the text, and the operator's own words contained one, so
  `100% <> :: <EOH> <EOR>` came back as two records, one of them nonsense. A
  terminator inside a value is ordinary text; only a parser that skips values by
  their declared length can tell the two apart. The pre-scan is gone.
- **An end time with no start silently lost the end.** `TIME_OFF` has no date of
  its own — the record's date is `QSO_DATE`, from the start — so an end time
  written without one is a time of day belonging to no day, and a reader would have
  to invent the date. The writer refuses it now.
- **The grid, above.**

## 2. What the owner should expect

**He can write a contact down, and he can see who he has already worked.**

- **Right-click a row on the mine side and there is a `Log this contact...` item**,
  under a rule below the send options. It is on rows addressed to him and on
  nothing else — a Log item on a CQ would offer to write down a contact that has
  not happened.
- **The dialog opens already filled** with everything Hamlet heard: the station,
  his grid, both reports, both times, the band, the frequency, the mode, and the
  operator's own callsign and grid.
- **Every field says where it came from.** A field Hamlet heard is marked *heard by
  Hamlet*; one it did not says **"Hamlet did not hear this"** rather than sitting
  empty, and it is left out of the file rather than written blank.
- **The observed fields cannot be typed over.** There is a notes box, it is his,
  and it goes to the ADIF `COMMENT` field and nowhere else.
- **Save writes one record. Cancel writes nothing** — the dialog never touches a
  file at all.
- **The file is `%AppData%\Hamlet\contacts.adi`**, beside `settings.json`, ADIF
  from the first contact, and it appends rather than rewriting.
- **A station already in the log carries a small green `worked`** on both lists,
  and hovering it says when and on what band.
- **Nothing is hidden or disabled by the mark.** Working somebody twice is his
  choice on another band or another day.

**What will look wrong and is not:**

- **A portable call is not marked as worked.** `W4/YV7AXM` after logging `YV7AXM`
  shows nothing, deliberately — see section 1.
- **The frequency in the dialog is where the dial is now.** If he has retuned since
  the contact, that is not the frequency he worked the station on. Hamlet does not
  know the contact's own dial; the row carries its slot and not the tuning. **The
  dialog says so on the field** — *where the dial is now* — rather than recording a
  band he may not have worked.
- **A contact he answered on another program has no `RST_SENT`.** The ledger books
  what actually went out through Hamlet's own send path, and an offer is not a
  transmission.

**Build:** clean, 0 warnings, 0 errors, whole solution, twelve times.

**Tests:** filtered and foregrounded, all belonging to this instruction. Engine
side `TheAdifLogRoundTripsTests` and `TheLogEntryIsWhatWasHeardTests` **22 of 22**;
app side `TheLogDialogAndTheWorkedMarkTests` and
`WhatTheLogCostsAfterAnEveningTests` **15 of 15**. No suite was run and nothing was
backgrounded. **Every test that touches the log redirects `SettingsStore.DataFolder`
to a temporary folder**, the seam unit 235 added, so none of them can append to his
real log.

**Not run, and you should know which:** the decoded row's own view tests —
`TheDecodedColumnsLineUpTests` in particular — because the left row's grid gained
an `Auto` column for the mark and its message moved from column 4 to column 5. Both
the header and the row moved together, so the origins should still line up, but
*should* is not *does*. They live in the `Views` namespace the standing rule keeps a
unit out of.

**Pushed to `main`:** `7424ab9`, `6e7009d`, `da2b903`, `51dddb6`. Version
**1.12.130 → 1.12.134**. `Ft8Sharp` did not move.

## 3. What you should see

**1. One ADIF record, quoted whole**, with the header the file opens with. This is
what the writer actually produced this session:

```
Hamlet contact log. ADI format, ADIF Specification 3.1.4, released 2022-12-06,
https://www.adif.org/314/ADIF_314.htm, retrieved 2026-09-07.
Fields Hamlet did not observe are absent rather than empty.

<ADIF_VER:5>3.1.4
<PROGRAMID:6>Hamlet
<PROGRAMVERSION:8>1.12.132
<EOH>
<CALL:6>IK4LZH
<STATION_CALLSIGN:6>KC3QIS
<QSO_DATE:8>20260907
<TIME_ON:6>214130
<TIME_OFF:6>214300
<BAND:3>20m
<MODE:3>FT8
<FREQ:9>14.074000
<RST_SENT:3>-09
<RST_RCVD:3>-12
<GRIDSQUARE:4>JN54
<MY_GRIDSQUARE:6>FN00DJ
<COMMENT:21>First one into Italy.
<EOR>
```

**The round trip: every field came back as it went in.** The test compares the
whole record rather than field by field, so a field the test forgot to name cannot
slip through. A separate test walks the written text **without the reader**,
checking every declared length against the character that follows it — so a writer
and a reader wrong the same way cannot both pass (§12.5).

**Eight note shapes survive intact**, including `<EOR> in the middle of a note` and
`<CALL:5>FAKE1 pretending to be a field`. Nothing is escaped, and that is the
format rather than an omission: ADI has no escape character, and **the length
prefix is what makes an angle bracket inside a note ordinary text.**

**2. An incomplete contact's record**, for a station answered somewhere other than
Hamlet:

```
<CALL:6>IK4LZH
<STATION_CALLSIGN:6>KC3QIS
<QSO_DATE:8>20260907
<TIME_ON:6>214130
<TIME_OFF:6>214200
<RST_RCVD:3>-12
<EOR>
```

**`RST_SENT` is absent, not `<RST_SENT:0>`** — which would assert an empty report
was exchanged, a thing that did not happen. `RST_RCVD` is still there, so the
absence reads as the missing fact rather than the writer having given up on the
pair. `BAND`, `MODE`, `FREQ` and `MY_GRIDSQUARE` are absent for the same reason:
nobody handed them in.

**3. A decoded row for a station already in the log**, and the mark on hover:

```
CQ IK4LZH JN54        worked
                      -> "You worked IK4LZH before, on 2026-09-07, on 20m."

CQ W1ABC FN31         (no mark)
CQ W4/YV7AXM EL96     (no mark, after logging YV7AXM — see section 1)
```

**And what the log costs**, measured at a hundred entries:

```
100 entries          26,572 bytes      265 bytes an entry
a thousand           about 259 KB

reading the whole log, 20 times      median 0.217 ms, slowest 0.291 ms
fourteen decodes placed, 10 slots    median 0.037 ms, slowest 0.043 ms
                                     against a slot of 15,000 ms
```

**Per decode or once: ONCE.** The log is read into a dictionary the first time a
row is placed and kept, and re-read only when a contact is logged — the one thing
in the application that changes the file. The per-decode cost is a case-insensitive
dictionary lookup on the sender's callsign.

**That claim is asserted, not only timed.** A timing can be fast for the wrong
reason and would go on passing after somebody put a read back in, so the second
test **deletes the log file** after the first row and shows the mark still landing
on the next one. If the file is gone and the answer is the same, nothing read it.

## 4. What's blocking us

**Nothing blocks the next unit, and there is no ask.** Three things stated.

**1. The frequency in a log entry is the dial at logging time, not at contact
time.**

Stated, and handled on screen rather than hidden. The row carries its slot and does
not carry the tuning; **Hamlet does not know what dial a contact happened on**, and
inventing one would be §0.0 exactly. So the dialog shows the frequency it is about
to write, labelled *where the dial is now*, and he can see it before he saves.

**If it should be otherwise, the fix is that a row remembers its own dial**, which
is a change to what a decode carries rather than to the log — and that is a
different unit's work.

**2. One detail of the ADIF specification is not fully cited, and it is marked.**

`FREQ`'s own field definition could not be retrieved: the spec is one very large
page and four separate fetches truncated before the field-definition table. **The
tag names are all cited**; what is not is `FREQ`'s unit. It is written in
**megahertz**, which is what the Band enumeration on the same page states its own
edges in, and `BAND` is written beside it and **is** fully cited — so every record
carries its own cross-check and a unit error would be visible on the first import
rather than silent. It is in the file's remarks as a marked assumption (§12.4)
rather than glossed.

**3. The decoded rows' view tests were not run and the left grid changed shape.**

Named rather than left to be discovered. The left row and its header both gained an
`Auto` column for the mark, moving the message from column 4 to column 5 — together,
so `TheDecodedColumnsLineUpTests` should still hold. It is in the `Views` namespace
whose stall unit 230 documented.

### Asks still outstanding

Carried outbound per HM-DEC-139.

1. **Two issues of one work-instruction number.** Raised by unit 271, and unit 252
   before it. **The author's error: an executed order must never be amended, only
   succeeded.** No unit action; recorded so the phase record's collision is
   explained when either is cited. **Unchanged this unit.**
2. **`PM95` reads *southern Japan***, raised by unit 271 as the compass qualifier's
   weakest reading. **Not a defect**; the table is where to argue with it.
   **Unchanged this unit.**
3. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05, step 6's two unmet exit
   criteria from the closed sensitivity phase. By HM-DEC-140 they live in
   `OPEN_ISSUES.md` and not on this queue; named once so the next session stops
   rediscovering them. **Unchanged this unit.**
4. **The message column does not fit.** Raised by unit 273, 2026-09-07. Waiting on
   your choice between five options with the numbers behind each, in unit 273's
   section 4. **The change is not in the tree**: nothing was shrunk or wrapped, and
   the left list clips today. **This unit did not touch it** — the `Auto` column
   the mark added takes no width on a row it has nothing to say about, so the
   figures unit 273 measured are unchanged for an unmarked row and narrower by the
   width of the word *worked* for a marked one.
5. **`CLAUDE.md` §1 stops indexing at HM-DEC-152.** Raised by unit 273. Rulings 153
   to 159 are in `DECISIONS.md` and not in the index, and adding one would fail
   `DecisionLogOrderTests`'s contiguity assertion on six rulings nobody in this
   sequence wrote. **Unchanged this unit.**

**Nothing was added to the queue by this unit**, and nothing was dropped.
