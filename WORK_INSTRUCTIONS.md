# Work instruction 360 - hear the announcement, closed: the record says so, the burst is outside the cap

**Step 1 of `PHASE_PLAN.md`, authored by the arbiter.** Step 0 is done and closed. Step 1 is
`partial`: unit 359 met 1.1 to 1.4, 1.6 and 1.7 with measured numbers, and the separate reading
of its report returned `partial` on **1.5 alone** - the transmission record does not say the send
was announced. Tim's R32 of 2026-09-18 settles both of unit 359's questions, and 1.5 was
reworded to carry them. **This unit closes 1.5 and nothing else. Five tasks, 0 to 4; task 4 is
the drop candidate.**

**Status.** `tools/status.sh`, real clock, after every commit and every task, and immediately
before each `dotnet test` invocation.

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

The arbiter checked all four on 2026-09-18 after 21:48, and they held.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: two invocations, one build each, with a status write immediately
before each. Never background and poll.

## 2. The tool facts, as unit 359 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused. `rm` is refused. A `for` loop over `$f` is refused (*simple_expansion*).
- `mkdir`, `cp` and `powershell.exe` need approval, which a headless session cannot give, so
  write files with the editor.
- `( ... || true)`, a `grep -v "^\s*$"` in a pipe, and an anchored `grep -n` on one file each
  needed approval. `&&` chains, `| tail`, `| grep -E` and several `-m` on one commit worked.
- `git show ... > file` is blocked; write the report with the editor.
- Anything outside `C:\Source\HamLet` - including `C:\Source\fldigi` - cannot be listed or read.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  since warnings are errors. Anchor on the comment's first line.
- `tools/status.sh` writes `RULES_AT: HM-DEC-161`.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 359's `## 4. What's blocking us` verbatim, from its first
line to its end** (`db3edfa1:output.md`), the way unit 359 carried unit 358's - its nested queues
and the reference to `4c55deac:output.md` included. `output.md` is deleted in the working tree
(the launcher's doing), so read the committed copy.

**Mark two of its items in place, and delete nothing:**
- unit 359 item 1 (the compound-callsign report over the cap): *ANSWERED by R32 (a), Tim
  2026-09-18 - the burst does not count against the cap; built in work instruction 360 task 2.*
- unit 359 item 2 (the record does not say announced): *ANSWERED by R32 (b), Tim 2026-09-18;
  built in work instruction 360 task 3.*

This unit answers none of the others.

---

## 4. Why this unit exists

**Step 1 stands at six of seven criteria.** The one open, 1.5, is short of two things: the
`ft8_transmission` record for a PSK31 send carries no announcement (0 of 5 kinds of send), and
the burst counts inside the 30 s macro cap, so a report to a compound callsign (28.192 s of
text, 30.050 s with the burst) is refused where it went before.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own RSID and never
            picked by the operator.
UNIT GOAL:  Every PSK31 send's transmission record says announced: true with the RSID
            code, and the cap measures the macro or the typed line and not the 1.86 s
            burst in front of it - so step 1 closes and step 2's entry is open.
ADVANCES:   step 1 criterion 1.5 (tasks 2 and 3), the last one open.
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full**, R32 and step 1 above all. Step 2 - the Olivia
demodulator - cannot begin until step 1 is done (`PHASE_PLAN.md` step 2 entry), and this is
what stands between them.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even when
the work succeeded.

What this instruction believes, from the reload of 2026-09-18 21:48 and the arbiter's own
reading after it:

- **HEAD is `7bb6b253`** (`phase: olivia plan rev2 - R32`).
- **`UnslottedTransmission`** (`src\Hamlet.RadioEngine\Transmit\UnslottedTransmission.cs`):
  `Seconds` is all the samples over the rate; `AnnouncedCode` is an init property (`:62`);
  `Fit` compares `Seconds` to `Cap` (`:81`-`:85`). So today the burst is inside the cap.
- **`OperatorSend.LongestUnslottedSeconds = 30`** at `Ft8TransmitSequence.cs:133`, inside
  `public sealed record OperatorSend`. `MainWindowViewModel.LongestTypedSeconds = 60` at
  `:15489`.
- **`Psk31Modulator.Compose`** (`Psk31Modulator.cs:148`) puts the burst in front of the text
  and sets `AnnouncedCode`; `SentSecondsFor` (`:111`) is the text plus the burst, and drives the
  typed line's *too long to send* through `Psk31Macros.TypedSeconds` (`Psk31Macros.cs:155`) and
  `MainWindowViewModel.cs:15563`.
- **The record:** `Ft8TransmitSequence.Recorded` (`:637`) builds the no-slot `TransmitRecord`
  from `send.Unslotted`'s mode, fit and seconds, and nothing of `AnnouncedCode`.
  `TransmitRecord` is `src\Hamlet.RadioEngine\Telemetry\TransmitRecord.cs:70`.
- `psk31_send_composed` already carries `announced` and `rsidCode`; `rsid_sent` is written after
  the keying frame.
- **Guards:** `TheFt8AndFt4SendsAreByteIdenticalTests` and `TheUnslottedSendTests` (engine,
  `Transmit\`), `ThePsk31ModulatorTests` (engine, `Psk31\`), `ThePsk31SendIsAnnouncedTests` and
  `ThePsk31TransmitTelemetryTests` (app), `TheStopIsAlwaysOnScreenTests` (app, `Views\`).
- **Carry-forward** at the end of unit 359: app 165 of 165, engine 105 of 105.
- `CivConstants.PttOn` code lines: 1. `_armedSend.Arm(` lines: 2.
- `assets\fixtures\captured\` does not exist. Jalocha's headers, `pj_mfsk.h` among them, are
  in the tree at `assets\reference\jalocha\`.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload reads *CPS-DEC-0164* as the highest ruling in `CLAUDE.md` §1; the table's highest
  row is HM-DEC-164. `PROJECT_STATUS.md` `RULES_AT` still says HM-DEC-161.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`; the
  tree has `data/rsid/` and `data/bands/`.
- `PHASE_PLAN.md` step 1 shows **1.7 unchecked**, though unit 359 measured it (0.075 of real
  time on both files) and the plan's revision record says met criteria were checked. Report it;
  do not edit the plan.
- `PHASE_OUTCOME.md`'s `UNIT 2 - STEP 1` entry reads `FATE: executed` for a run that never
  happened, and the harness writes some entries as `UNIT 1`. Append-only; do not edit.
- `PHASE_STATUS.md` still says `WORK_INSTRUCTION: 358`. The launcher's file.
- Uncommitted at authoring: `PHASE_STATUS.md`, `.run-unit\reload.txt`, and this file.
- Two tests are red and neither is on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` (two `Arm(` lines) and
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`.
- The app carry-forward list is flaky run to run, `TheStopIsAlwaysOnScreenTests` on an FT8 send
  most of all. Rerun a red that passes alone up to three times, and say which run the number
  came from.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what they
rejected.**

**PHASE_PLAN.md R32 - Tim, 2026-09-18, on unit 359's two questions.** *(a) **The RSID burst
does not count against the send cap.** The cap measures the macro or the typed line; the
announcement is a fixed 2.3 seconds in front of it. (b) **The transmission record says a send
was announced** - `announced: true` and the RSID code on every keyboard-mode send. Both were the
arbiter's to decide under §6 and it stopped instead; **a cap the plan calls the author's number,
and a field on a record, are neither the radio's safety nor a promise, and are never a stop.***

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode
transmission Hamlet sends begins with the RSID burst naming its mode and variant - Olivia, and
PSK31 retroactively. Hamlet listens for RSID across the passband and when one arrives sets the
mode, the variant and the offset itself. The operator never picks a variant. A carrier that
never announced itself gets the blind search of step 2 as a fallback. The tables and codes are
`data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past
  the budget; a decision that changes what the product promises the operator - a fact stated
  about the radio, a contact or a send. A hint, a label, a number, a layout, a mechanism
  arithmetic will not allow: the arbiter decides, marks it author's and overrulable, and
  continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on.
  Never loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *A package is needed. `MOVE: stop`.*
- *Anything touches the transmit chain beyond adding an audio generator and an RSID prefix
  behind the one sequence. `MOVE: stop`.* **R32 is the later ruling**: holding the prefix
  outside the cap and naming it on the record are part of the RSID prefix, and licensed.
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R10 - the transmit chain may carry a send that has no slot.** *A send with no slot
is not held to a slot fit; it is held to a stated maximum length so a continuous carrier cannot
run on ... and it is not more than thirty seconds ... The gate, the single `PttOn` use site, the
`finally` that unkeys or aborts, and `StopNow` stay one code path shared by all three modes.
FT8 and FT4 must stay byte-identical, proved by the tests that guard them today, run filtered.
The transmission record says which mode went out ... Rejected: a second sequence (a second
`PttOn` site), an invented PSK31 slot, shortened macros, and splitting a macro across slots (one
click, several keyings).* R32 (a) reads the thirty as the text's; the value does not move.

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was
shut, that later blocks the unit told to open the door, is the session's to rewrite in its own
commit so it guards the rule and not the shut door - and that is not a ruling, not an ask, and
not a stop.*

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event ... that lets a person diagnose that stage from the file alone, proved by
assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others.*

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never
transmits on a timer, on a decode, or because a contact "should" continue.* *The dummy-load
requirement is withdrawn in full ... not to be referenced.*

**FACT-004** *There are two computers, and only one of them has a radio on it.* **FACT-006** *The
development machine has no contact log and never will.* Nothing measured here is evidence about
the radio.

**HM-DEC-139**, **HM-DEC-155**, as in sections 1 and 3.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings; they are in the
decision block at the end.

- **D. The cap measures the text; the announcement is excused by its measured length, and only
  that.** `UnslottedTransmission` learns how many of its samples are the announcement, set only
  by `Compose` from the burst it actually put there. `Fit` measures the samples after the
  announcement against `Cap`. **The excusal is bounded:** an announcement longer than
  `RsidBurst`'s own length for that code at that rate, or one claimed with no `AnnouncedCode`,
  is `LongerThanTheCap` - so no path can buy carrier time by calling it an announcement, and the
  longest keying is the cap plus one burst. The cap's *value* does not move.
- **E. The card and the gate agree.** The typed line's *too long to send* measures the framed
  text alone against 60 s, as the gate now does. Whether `SentSecondsFor` survives as the
  on-air total for telemetry, or goes, is the unit's call; a file emptied is listed.
- **F. The record.** The no-slot `ft8_transmission` gains `announced` (true or false),
  `rsidCode` (the code, or absent when false) and `announcementSeconds`. `audioSeconds` stays
  the whole audio, because that is what keyed. The slotted branch is untouched and
  `TheFt8AndFt4SendsAreByteIdenticalTests` pins it. Carrying the fields through `Recorded` and
  `TransmitRecord` is the one change `Ft8TransmitSequence` takes; the gate, `PttOn`, the
  `finally`, `StopNow` and the cap's value do not change.
- **G. The burst keeps its shape**: the file's 5 silent symbols, then the 15 tones, 1.8576 s at
  12 kHz. Unit 359 item 1's option C (dropping the leading silence) is moot under R32 and is not
  built.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_STATUS.md` has step 0 `done` and step 1 `partial`.
- Hash the nine fixtures against `assets\fixtures\olivia\manifest.json`, `psk31-cq-rsid.wav`
  first. If any fails, stop.
- Append `UNIT 360 - STEP 1` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 359`
  entry. Touch no earlier entry.
- Patch-bump the version by one (unit 359 left 1.13.46).
- Run the carry-forward list, both invocations, before any change.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do
  not commit** `SESSION.lock`, `RUN_LEDGER.md`, anything under `.run-unit\`, or the deletion of
  `output.md`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line.

1. **Every place a no-slot send's length is judged**: `Fit`, `Ft8ArmedSend.Arm`, the sequence's
   own refusal, the typed line's *too long to send*, `psk31_send_composed`'s `withinCap`, and
   anything else found. For each, what it measures today.
2. **The seconds, text and total, of each send** - CQ, Answer, Report to W1AW, Confirm, Report
   to VP2V/W1AW, and the longest typed line - against its cap. Unit 359's table is the *before*;
   say whether it still holds.
3. **What `ft8_transmission` carries today for a PSK31 send**, as written to a telemetry file,
   and for an FT8 send.
4. **Every test that asserts the burst counts inside the cap**, or that the typed line's limit
   includes it. Each is a §R12 rewrite in task 2's own commit.
5. The count of `PttOn` code lines and `_armedSend.Arm(` lines.

**Before-numbers:** PSK31 records saying announced 0 of 5; the compound-callsign report refused;
the longest typed line's text about 58.2 s.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the burst is outside the cap (1.5, R32 a)

Build decisions D and E.

**Test watched failing first:** extend `TheUnslottedSendTests` (engine) and
`ThePsk31SendIsAnnouncedTests` (app).
- A composed Report to VP2V/W1AW, 28.19 s of text, **fits** and is armed; its audio is text plus
  burst.
- A text at the cap to within one sample, with its burst, fits; one sample over, refused.
- An announcement claimed longer than the burst, or with no code, is `LongerThanTheCap`.
- A typed line whose framed text is just under 60 s is **not** *too long to send* and goes; just
  over, the card says so and the press is refused, and the card and the gate agree on both.
- Unannounced audio (codes unreadable) is measured whole, exactly as before.

**Run filtered, green and unedited:** `TheFt8AndFt4SendsAreByteIdenticalTests`,
`TheStopIsAlwaysOnScreenTests`. Rewrite under §R12, in their own commit, only the tests task 1
item 4 named.

**The stop.** If this needs a change to the gate, `PttOn`, the `finally`, `StopNow`, the cap's
value, or a second keying or path, **do not build it**; write in section 4 what it would need as
`MOVE: stop` material, and go on to task 3.

**Drop candidate:** none.

### Task 3 - the record says it was announced (1.5, R32 b)

Build decision F.

**Test watched failing first:** extend `ThePsk31SendIsAnnouncedTests` (or
`ThePsk31TransmitTelemetryTests`, wherever the written record is read).
- Each of the five kinds of PSK31 send - CQ, Answer, Report, Confirm, typed line - pressed on the
  fake radio writes an `ft8_transmission` with `announced: true`, `rsidCode: 1` and
  `announcementSeconds` equal to the burst's length at that rate. **5 of 5.**
- With the codes unreadable, a PSK31 send writes `announced: false` and no code.
- An FT8 send's `ft8_transmission` is field for field what it was: no new key.
- Nothing personal in any of it: no callsign, grid, name, place or text.
- `PttOn` 1 and `Arm(` 2, unchanged from task 1.

Then run the carry-forward list, both invocations. The list gains nothing unless a new test class
was made; if one was, add it and say so.

**Drop candidate:** none.

### Task 4 - step 2's ground, measured and nothing built

**Only if tasks 2 and 3 are green.** A read and a measurement for the next arbiter; no production
code, no test assertions.

1. **Step 2's entry check:** the clean 16/500 fixture's RSID is detected (it was in unit 359;
   say it still is).
2. **`assets\reference\jalocha\pj_mfsk.h`, read for structure** (PSK31 plan §R5): the parts an
   Olivia receiver has - tone detection, symbol and block sync, the Walsh-function decode, the
   scrambler and interleave - named with their line ranges, and **what in them could not be
   written as Hamlet's own without copying**, if anything (§6's port clause).
3. **`data\olivia\timing.json`** parsed by a machine, and whether it agrees with the manifest's
   seconds and characters for each variant (unit 358 item 5).

Report it in section 1 under its own heading.

**Drop candidate: this whole task.** Drop it whole and say so.

---

## 9. Parked - do not touch, do not raise

- **The Olivia demodulator, the blind search, the timing table's measurement** - step 2. Task 4
  reads; it builds nothing.
- **A detection that sets the mode, variant or offset, and rows per station** - step 3.
- **Turn timing that undercounts Hamlet's own answer by the burst** (unit 359 item 4) - step 4's
  timing work.
- **An Olivia modulator or any Olivia send** - step 4.
- **Codes 72 to 75, and the fldigi commit pin** (unit 359 item 3). The clone is outside the
  root; the Jalocha headers step 2 needs are inside it.
- **The dial 1500 Hz below the center** (unit 358 item 1). Step 0 is closed.
- **The flaky Stop tests, HM-OPEN-090, `SOURCE.md`'s date pin, the screen phase's open asks.**
  Carried in section 4, not worked.
- **`PHASE_PLAN.md`'s unchecked 1.7 and the mislabeled outcome entries.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not change the cap's value, the gate, `PttOn`, the `finally` or `StopNow`, and add no
  `PttOn` or `Arm` site.** §R10, §6; R32 licenses the measurement and the field, not these.
- **No literal RSID code, tone, spacing, symbol rate or burst length in code.** The burst's
  length is `RsidBurst`'s, from the file (R27).
- **Do not port `pj_mfsk.h` or `rsid.cxx`, and add no package.** Either is a `MOVE: stop`.
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md`, `PHASE_PLAN.md` or earlier
  `PHASE_OUTCOME.md` entries.**
- **Report mismatches; repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. §R12 rewrites go in their own commit. Never
commit `SESSION.lock` or `.run-unit\`. The report names the branch and whether every push
succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 4 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0 done
   and closed; step 1 <state after this unit - done only if 1.5 is met>;
   steps 2-6 not started, step 2's entry <open|not open>.
B. Step 1's one open criterion, 1.5, in its R32 wording - PSK31 records
   saying announced N of 5 with code 1; the compound-callsign report
   fits yes|no at N s of text; the typed-line card and gate agree yes|no;
   FT8/FT4 byte-identical yes|no; PttOn 1 and Arm( 2 unchanged yes|no.
   1.1-1.4, 1.6, 1.7 still green on the carry-forward list yes|no.
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of 1.5 or of step 2's entry - in
   particular whether task 2 stopped at the transmit chain, and whether
   task 4 found anything in pj_mfsk.h that cannot be written without
   copying.
```

**Then the header:**

```
UNIT:       360 - <complete|stopped> at task N of 5, <task 4 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     PSK31 records saying announced 0 -> N of 5; compound-callsign report refused -> <fits|refused>
DRIFT:      0
```

**Then a criterion table, 1.1 to 1.7**, 1.5 with this unit's numbers and the others with the
carry-forward run that shows them still green.

**Section 3 leads with the five `ft8_transmission` lines** for the five kinds of PSK31 send, as
written, then one FT8 line beside them showing no new key, then the cap table: each send's text
seconds, burst seconds and total against its cap, fits or refused. Then task 4's findings, if
built.

**Every appearance claim is computed, not seen. Nothing here is evidence about the radio**
(FACT-004).

---

```
ARBITER-DECISION
STEP: 1
APPROACH: carry the RSID code into the no-slot transmission record and hold the cap to the text alone, burst outside it, bounded by the burst's own measured length
MOVE: continue
WHY: Step 1 is partial on 1.5 alone, and both halves unit 359 fenced off are now licensed by Tim's R32; the loop test finds this approach in no entry, and step 2 cannot start until step 1 is done.
STATE: partial
DECIDED: author's, overrulable - (D) the cap measures the samples after the announcement, and the excusal is only the burst RsidBurst made for that code at that rate; anything longer, or claimed with no code, is LongerThanTheCap, and the cap's value does not move. (E) the typed line's too-long-to-send measures the framed text alone, so the card and the gate agree. (F) the no-slot ft8_transmission gains announced, rsidCode and announcementSeconds, audioSeconds stays the whole audio, and the slotted branch is untouched. (G) the burst keeps the file's leading silence; unit 359 item 1 option C is moot. Task 4, a read-only measurement of step 2's ground in pj_mfsk.h and timing.json, is the drop candidate. PHASE_PLAN.md's unchecked 1.7 is reported, not edited.
LICENCE: PHASE_PLAN.md R32 (a) and (b), R27, R31 and section 6 (a later ruling wins; a number or a mechanism is the arbiter's); PSK31 plan R5, R10, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155; ARBITER.md section 6
ACCOMPLISHED: Every PSK31 transmission Hamlet makes says in its own record that it announced itself and with which code, and the announcement no longer eats into the send's length, so the compound-callsign report goes again - which closes hearing the announcement and opens the Olivia demodulator
ADVANCES: step 1 criterion 1.5, the last open criterion of step 1
END-ARBITER-DECISION
```
