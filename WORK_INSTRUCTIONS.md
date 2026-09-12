# Work instruction 327 - hear him through the silence, and mark him so it shows

**Seed under `--seed`, one iteration.** Steps 0 to 5 are `done`. Step 6 is Tim at the
radio and only he closes it. This unit is carried repair on what he saw at the radio on
the nights of 2026-09-11 and 12. **Six tasks, each small. Status before every `dotnet`
command.**

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt`, run
exactly as its top comment says - two invocations, one build each. Never background and
poll. **Write `PROJECT_STATUS.md` immediately before every `dotnet test` and `dotnet
build`.** The watchdog kills at twelve minutes of silence.

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Single backslashes; check what landed. **Commands joined by `;` are
refused. `rm` is refused; a redirect into the repository is refused.** A file that must
go is emptied with a one-line comment and raised. **Multi-line commit messages: use
`-m` more than once on one `git commit` line**, which is what 326 could not find.

---

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 326's queue. **Every item comes back in section 4,
verbatim where unresolved.** Touched here:

- **Unit 326 item 8** - a logged PSK31 contact carries no grid. **Task 5.**
- **Unit 326 item 9** - the three press-test types back on the carry-forward list.
  **Task 6, yes.**
- **Unit 325 item 6 / unit 326 item 10** - the two panels share one collapse flag and
  two `Views` tests are red for it. **Task 6.**
- **Unit 324 item 4** - why a 62 dB carrier failed the keying-shape test. **Task 1
  answers half of it**: it was idling.
- **The ALC margin of 15** - built, carried, Tim's to overrule.
- **`validate-output.bat` could not be invoked** two units running. Try it; report the
  exact command and result.
- **Three files emptied because `rm` is refused**: `Psk31Listening.cs` was deleted by 322;
  `commit-msg-326.txt` and unit 323's item 51. **Task 6 lists them for Tim to delete by
  hand**, in one line.
- **`HM-DEC-161` versus `CPS-DEC-0161`** - two id schemes. Report; do not repair.

All others as unit 326 carried them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  A PSK31 station stays on the list while he is idling, so Tim can
            read him and answer him. The achievement mark becomes something
            Tim can see from across the desk and click for the reason. The
            card's empty right column carries the contact's facts.
ADVANCES:   No step. Carried repair before step 6.
DRIFT:      0 carried. Expect 1.
```

**From `%AppData%\Hamlet\telemetry\2026-09-12.jsonl`, read by the web thread:**

- **7.070, 00:54 to 00:57, version 1.13.10.** Tim pressed CQ and **a PSK31 signal left
  the radio** - 11.46 s at 1747 Hz, radio confirmed transmitting, 31% power. Then a
  station at **2073 Hz**, strong, **squelch open at quality 0.99** - as clean as the
  fixture - appeared six times in two minutes and was retired each time as `SignalGone`
  after 1 to 5 seconds, zero characters. **206 seconds, 12 carriers, 0 lines.**
- **7.070, 02:12 to 03:05, version 1.13.11.** Fifty minutes. **One line parsed.**
- **03:05 to 03:08, version 1.13.12.** Zero carriers.

**Why, and this is specific.** A PSK31 station that is idling - between words, or
waiting for a reply - sends continuous phase reversals. **An idling PSK31 signal has no
energy at its carrier frequency**; it splits into two lines 15.6 Hz either side. A search
that looks for a spectral peak at the center sees the station vanish every time he stops
typing and reappear when he types. That is the pattern in the file, to the second:
appear, a few seconds, gone, appear at the same offset. The demodulator, which measures
keying shape, rates the same signal 0.99 throughout. **The search retires a live station
for idling, which is unit 324's silence fault one layer down.** And zero characters
during idle is correct - there are none - so the station at 2073 Hz may well have been
someone answering Tim's CQ and waiting.

**And what Tim saw on the FT8 list, 2026-09-12, version 1.13.12:** the quill 325 built
is a dark glyph the size of a bullet, all alike, no green, no orange, nothing turning.
Tim: *"ugly and useless."* And clicking it does nothing: *"no achievement possibility
click response."* He ruled two nights ago that a subtle mark is invisible to him; this
is that mark.

**And the card**: *"the real estate to the right of the map is valuable"* - it is empty -
and *"remove the info from below the map, I really never noticed it."*

---

## 5. Verify this instruction against the tree

**Names from units 314-326's reports.** Check; **report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible.

- `Psk31CarrierSearch` - its window, `SignalHalfWidthHz`, `CandidateRatio`,
  `DynamicRangeDb`, the candidate bar, and the retire rule 324 built (`retirePasses` 8,
  `retireSeconds` 1.024 - **which is silence-shaped again; read it**). Per-carrier
  `Psk31Demodulator` and its `SquelchQuality`, how often it is sampled, and
  `psk31_squelch`/`psk31_reading` events.
- The quill: `AchievementMarkControl.cs` (the SHA pin that moved twice - **§R14, no pin**),
  `NudgeIsDoor`, `RowLift`, the orbit ring, the colors 325 chose (**the record says
  amber; Tim ruled orange - report the hex and whether it reads orange**), the hover text
  `NudgeTip`/`NudgeWords`, and what a click on the mark does today (nothing).
- The map popup 310 built - its shape, dismiss X, how it opens - as the model for the
  quill popup.
- The conversation card: the map row, the caption under it (`… is in Austria, grid JN76.
  That is 4,500 miles from you.`), `OperatorLocation.DescribeCompass`, the distance, the
  message count and *show the N messages*, the state word.
- `Ft8ContactLogEntry` and how the RST reached it in 326; the PSK31 card's grid.
- `docs\carry-forward-tests.txt` (26 types); `ThePressingOfCqTests`, `TheCqReceiptTests`,
  `ThePsk31CqGoesOutTests`; the two `Views` reds in `TheMenuIsUnderTheMouseTests`; the
  panels' shared expanded flag.
- Fixtures: the four-signal file; **make one more** in task 1 from the reference
  convention: a single station that types for 10 s, idles for 8 s, types for 10 s.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R1-§R19.** In particular **§R9** a character not sure of is not shown;
**§R12** a session fixes its own tests; **§R13** telemetry on every stage; **§R14** tests
prove criteria, nothing beyond; **§R16** two quills, no cap - counter green and still,
**door orange and turning**; **§R19** American.

**Tim, 2026-09-10 and 11, on the mark:** *"It's a tiny dot lost in the sea of the tray"*;
*"ugly and useless"*; a subtle mark is invisible to him. **§0.6** - color is never the
only carrier. **§0.5 / HM-DEC-012** - family color is text only; **the quill is not
family color, it is its own, and a disc filled green or orange is a mark, not a bar.**
**`ACHIEVEMENTS_PHILOSOPHY.md` §3.1** - a door is never named; **§3.5** - the teaching is
the product; **§4** - worked, never confirmed; nothing shames.

**Tim, 2026-09-12, the card:** facts beside the map, once, and the caption under the map
removed. **§0.0** - a fact on the card is a fact Hamlet knows. **HM-DEC-111** - a
reading carries its age.

**§0.2** - nothing here touches what keys. **HM-DEC-018, §2.1** - nothing personal in
an event. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**, **the dummy load
withdrawn in full.**

---

## 7. Status cadence

`PROJECT_STATUS.md`: **after every task, at least every ten minutes, and immediately
before every `dotnet test` and `dotnet build`.**

---

## 8. The tasks

Six. Each names the test to watch failing first and one drop candidate. **Drop from the
back; task 1 is never dropped.**

### Task 1 - a station stays on the list while he is idling

Append `UNIT 327` to `PHASE_OUTCOME.md` under step 6 as carried repair; patch-bump; run
the carry-forward list as its comment says, status first.

**The rule.** A held carrier is kept **by what its demodulator says** - squelch open, or
quality above the candidate bar within the last N seconds - **and is retired only when
both the demodulator and the search have lost it** for the stated passes. An idling
station keeps its demodulator's squelch open (quality 0.99 in the file) and is therefore
kept. **State the numbers and why.** Also: the search's peak detector treats the two idle
sidebands at ±15.6 Hz as one carrier at their center, so an idling station is *found*,
not only kept - state how.

**The fixture.** From `assets/reference-modem.py`'s convention: one station at 1000 Hz
that types the CQ line for ~10 s, **idles 8 s** (continuous zeros), types it again.
8 kHz, SNR +10 dB. Hash into `manifest-step2.json`.

**Telemetry.** `psk31_carrier_retired` reasons gain `SignalGone` only when both agree;
add `psk31_carrier_idling` when a held carrier goes idle and `psk31_carrier_typing` when
characters resume, so the file shows the idle gap as an idle gap.

**Test watched failing first:** `ThePsk31StationIdlesTests`, engine and app. Watch it
fail, then green: the idle fixture yields **one** carrier for its whole length, retired
once at the end; the two CQ lines both decode at or under 0.05; an idling event and a
typing event bracket the gap; the four-signal fixture still yields four; the noise-only
fixture still yields none.

**Drop candidate:** none. **Not droppable.**

---

### Task 2 - first-character latency, measured

The 1621 Hz station on 7.070 gave two characters in nine seconds at quality 0.92. From
the clean fixture and the idle fixture: **how long from a carrier appearing to its first
character**, in seconds, and what dominates it - the bit-clock settle, the varicode
separator wait, the squelch. Report the number; **if it is over three seconds, state
what would bring it under and do not build it here.**

**Test watched failing first:** extend `ThePsk31DemodulatorTests` by one that asserts the
latency is reported and is under a ceiling the unit states from the measurement.

**Drop candidate:** the ceiling. Keep the measurement.

---

### Task 3 - the mark is a thing you can see, and clicking it tells you why

**The mark.** On every list row that qualifies (§R16, no cap): a mark **the full height
of the row**, not a bullet. If the quill shape does not survive at row height, **a filled
disc does**, and the shape difference for a door is the ring. **Counter: green
`#3B6D11`, still. Door: orange - report the hex - with the ring, turning.** Report what
325 shipped (size in pixels, color) beside what this ships. A worked, faded station:
no mark, as now.

**The click.** Clicking the mark opens a popup with a dismiss X, in the shape of the map
popup from unit 310, that says **why this station is interesting and what working him
would earn**, in the voice the hovers use. For a counter: the entity and the count it
moves - *Costa Rica · a new country · you have worked 0 in Central America*. For a door:
*A first contact in a new area · working him opens a set of cards you have not seen yet*
- **naming nothing** (§3.1). One more line for either: *A QSO first. The card comes with
it.* Counts say **worked**, never confirmed (§4). The hover stays as its one line.

**Telemetry.** `nudge_opened` with the kind (counter/door) and nothing else.

**Test watched failing first:** `TheMarkIsSeenAndClickedTests`, app. Watch it fail, then
green: a qualifying row's mark is row-height; a counter is green and still; a door is
orange, ringed and turning; a worked row has none; clicking opens the popup; the popup
names the entity for a counter and names nothing for a door; the word *confirmed* appears
nowhere; the X closes it; `BindingHealthTests` green.

**Drop candidate:** the turning. Keep ring plus orange.

---

### Task 4 - the card's right column, and no caption under the map

Beside the map, top to bottom, **only what Hamlet knows for certain**:

- **4,500 miles · northeast** - distance and `DescribeCompass`, moved up from the caption
- **Grid JN76** - and country if the header does not carry it
- **His time: 07:02** - local solar time from his longitude, labeled as such, not a time
  zone lookup
- **Last heard: 73 · 10 s ago** - what he sent and when (HM-DEC-111)
- **9 messages** - with *show the messages* here
- **New country** / **Would open a new area** - the mark's reason, and the task 3 popup
  opens from it

**The caption under the map is removed.** Everything it said is now above. The card gets
shorter by that line. A fact Hamlet does not have - no grid, no last message - is
**absent, not dashed**.

**Test watched failing first:** `TheCardsRightColumnTests`, app. Watch it fail, then
green: the six facts render from a full card; a card with no grid renders without the
grid line and without the time; no caption is bound under the map; `VoiceTests` green;
`BindingHealthTests` green.

**Drop candidate:** *His time*. Keep the other five.

---

### Task 5 - the logged PSK31 contact carries his grid

Unit 326 item 8. The parser read it; the card shows it; `Ft8ContactLogEntry` reads grids
from FT8 fields and drops it. Hand it to the entry the way the RST was handed. `GRIDSQUARE`
appears in the ADIF for a PSK31 contact whose grid was read; **absent when it was not**,
never invented.

**Test watched failing first:** extend `ThePsk31LogsWithRstTests` by one: a contact with
a read grid exports `GRIDSQUARE`; one without exports none; FT8 records byte-identical.

**Drop candidate:** the whole task.

---

### Task 6 - housekeeping, one line each

- The three press-test types back on `docs\carry-forward-tests.txt`, in the two-invocation
  form. Report the run time before and after.
- The two digital panels get their own collapse flags; the two `Views` reds in
  `TheMenuIsUnderTheMouseTests` are run and reported, fixed if the flag was the cause,
  left and named if not.
- **The files this environment cannot delete**, listed in section 2 of the report in one
  line for Tim to remove by hand: `commit-msg-326.txt`, unit 323's item 51, any other.
- **The door color**: if 325's is amber, make it the orange Tim ruled.

**Test watched failing first:** none new; the `Views` tests and the carry-forward list.
**Drop candidate:** the whole task.

---

## 9. Parked

- **Step 6.** Tim's.
- **Real off-air audio.** Task 1 answers the idle half of unit 324's item 4; the other
  half - why a 62 dB carrier scored 0.6 on keying shape at appearance - still wants a
  recording.
- **A gone-quiet for the PSK31 card.** Unruled.
- **Any change to what keys. Any package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Status before every `dotnet`
  command.**
- **Do not retire a carrier the demodulator still rates readable.**
- **Do not show a character or a row Hamlet is not sure of.**
- **Do not make the mark subtle.** Row height, two colors, shape difference.
- **Do not name a door's area anywhere.**
- **Do not invent a fact for the card.** Absent, not dashed.
- **Do not add a SHA pin to any control.** §R14.
- **Do not touch anything that keys, arms, composes or plays.**
- **Do not put a callsign, grid or text in any event.**
- **Do not edit the phase files** beyond the outcome append.
- **Do not add a package.**
- **Do not chase the known reds** beyond the two named in task 6.
- **Do not repair this instruction.** Report mismatches; keep working.
- **Write American.**

---

## 11. Committing and pushing

Commit per task; `-m` more than once for a multi-line message; push at the end. Nothing
left uncommitted.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 5 done,
   6 not started - unchanged by this unit.
B. No step criterion moves; this is carried repair before step 6.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       327 - <complete|stopped> at task N of 6, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     carriers kept through an 8 s idle 0 -> 1; first-character latency <n> s;
            mark height <before> -> <after> px
DRIFT:      <n> consecutive units without advance  (was 0)
```

**Section 2 must tell Tim, in plain words: that a station who stops typing stays on the
list now; what the mark looks like and what happens when he clicks it; what the card's
right column shows; and the one-line list of files to delete by hand.**

**Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 6
APPROACH: keep a PSK31 carrier by its demodulator's verdict so an idling station stays on the list, measure first-character latency, make the achievement mark row-height in two colors with a popup on click, fill the card's right column with the contact's facts and drop the caption, log the PSK31 grid, and do the housekeeping
MOVE: continue
WHY: the owner's telemetry from two nights on 7.070 shows a clean station retired six times for idling and 206 seconds with zero lines read, and his own eyes found the mark unreadable and unclickable and the card's column empty; step 6 is his and cannot start until he can read a station
STATE: not started
DECIDED: the keep-and-retire numbers, the latency ceiling, the mark's exact size and the orange hex are the unit's to state; the idle fixture is made from the reference convention
LICENCE: PHASE_PLAN.md R9, R12, R13, R14, R16, R19; Tim 2026-09-11 and 12 on the mark and the card; CLAUDE.md 0.0, 0.5, 0.6; ACHIEVEMENTS_PHILOSOPHY.md 3.1, 3.5, 4
ACCOMPLISHED: a station who pauses stays on Tim's list until he actually leaves, the achievement mark is something Tim can see and click for the reason, and the card says what Hamlet knows beside the map
ADVANCES: none - carried repair before step 6
END-ARBITER-DECISION
```
