# Work instruction 359 - hear the announcement: RSID in, RSID out

**Step 1 of `PHASE_PLAN.md`, authored by the arbiter.** Step 0 is done and closed (unit 358,
`STATE_AFTER: done`). Steps 2 to 5 follow; step 6 is Tim's. **Seven tasks, 0 to 6.**

**This is 359's second authoring, and the first one never ran.** The instruction written at
about 11:50 on 2026-09-14 was never executed. The run halted at 11:58 because it *could not
take the session lock* (`RUN_LEDGER.md`, last line). No commit follows `1444962f`, and
`src\Hamlet.RadioEngine\Rsid\` still holds only `RsidCodes.cs`. The outcome entry `UNIT 2 -
STEP 1` says `FATE: executed`, but its `COST` is unit 358's to the cent, and its `STATE_WHY`
describes unit 358's report. **This approach has not been tried**, so aiming at it again is
not a loop (`ARBITER.md` §8, *a `never ran` entry is not a tried approach*). The tree claims
below were measured again, and four were corrected.

**Status.** `tools/status.sh`, real clock, after every commit and every task.

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

The arbiter checked all four on 2026-09-14 after 12:08, and they held.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: two invocations, one build each, with a status write immediately
before each. Never background and poll.

## 2. The tool fact

- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused. `rm` is refused.
- A `for` loop over `$f` is refused (*simple_expansion*).
- `mkdir`, `cp` and `powershell.exe` need approval, which a headless session cannot give, so
  write files with the editor.
- `-m` more than once works.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  since warnings are errors. Anchor on the comment's first line.
- Unit 358 measured a `git` command with a `--format="%h ..."` argument needing approval. So
  did a `ls` or `git -C` aimed at `C:\Source\fldigi`, outside the root.
- `tools/status.sh` writes `RULES_AT: HM-DEC-161`.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 358's `## 4. What's blocking us` verbatim, from its
first line to its end** (`1444962f:output.md`), the way unit 358 carried unit 357's. That
includes its nested queues and its reference to `4c55deac:output.md`. `output.md` is deleted
in the working tree (the launcher's doing), so read the committed copy. This unit answers
none of them.

---

## 4. Why this unit exists

**Hamlet reads 0 of the 8 RSID bursts in the mode author's fixtures, and 0 of PSK31's sends
announce themselves.** Unit 358 hashed the nine fixtures 9 of 9. The tree has no RSID code
beyond `RsidCodes`, which reads the data file.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own RSID and never
            picked by the operator.
UNIT GOAL:  Hamlet hears an RSID burst anywhere in the passband and reads the mode,
            variant and center from it on every fixture; Hamlet's own burst reads back;
            every PSK31 send begins with its BPSK31 burst, FT8 and FT4 untouched.
ADVANCES:   step 1 criteria 1.1, 1.2, 1.3 (task 2), 1.4 (task 3), 1.5 (task 4),
            1.6 (tasks 4 and 5). 1.7 is task 6, the drop candidate.
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full.** Step 1 is the one this unit is for. Its entry
is step 0 done, plus `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav` hashing as the
manifest says, **checked first.**

**Why the detector comes before the prefix.** Criterion 1.5 is proved by loopback through
the detector, so task 4 can only be proved once tasks 2 and 3 work. Task 4 is also the only
task that touches the send path, so it goes after the ones that cannot key anything.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even
when the work succeeded.

What this instruction believes, from the reload of 2026-09-14 12:08 and the arbiter's own
reading after it:

- **`data\rsid\rsid-codes.json`:**
  - Top-level keys, and only these: `source`, `symbol_rate_hz` 10.7666015625, `symbols` 15,
    `silence_symbols_before` 5, `first_tone_offset_symbols` -7, `codes`, `tone_sequences`.
  - Codes: BPSK31 1, OLIVIA_8_250 69, 16_500 70, 32_1000 71, 8_500 72, 16_1000 73, 4_500 74,
    4_250 75.
  - `tone_sequences` exist for BPSK31, 8/250 and 16/500, and by the unit-358 reading for
    32/1000 too. **Confirm the fourth.**
  - **There is no `Squares` or `indices` table**, though `assets\reference\SOURCE.md` says
    they are in the file. `source` is pinned to *master 2026-09-14*, not a commit.
- **`assets\fixtures\olivia\manifest.json`:** nine files.
  - Seven carry RSID: `olivia-8-250-cq-rsid`, `olivia-16-500-qso-rsid`,
    `olivia-32-1000-qso-rsid`, `olivia-16-500-qso-snr-10db`, `olivia-16-500-qso-snr-16db`,
    `olivia-two-signals-rsid` and `psk31-cq-rsid`. That makes 8 bursts, two of them in the
    two-signal file.
  - Every single-signal file is centered at 1000 Hz. The two-signal file has 8/250 at 1000
    and 16/500 at 2000.
  - `olivia-8-250-qso-norsid` and `olivia-noise-only-30s` carry no RSID.
- `src\Hamlet.RadioEngine\Rsid\RsidCodes.cs` is the only file in `Rsid\`.
  `Olivia\OliviaData.cs` reads the Olivia data.
- **The PSK31 send path:**
  - `Psk31Modulator.Compose` returns an `UnslottedTransmission`.
  - `MainWindowViewModel.SendPsk31` hands it to `Ft8ArmedSend.NowAsync`, which runs
    `Ft8TransmitSequence`, the one `CivConstants.PttOn` write (`Ft8TransmitSequence.cs:513`).
  - `SendTypedPsk31` goes through the same door.
  - The two `_armedSend.Arm(` lines are at `MainWindowViewModel.cs:15038` and `:15235`.
- **The cap:** `UnslottedTransmission.Seconds` is the length of all its samples, held to
  `Cap`.
  - A macro's cap is `Ft8TransmitSequence.LongestUnslottedSeconds` = 30
    (`Ft8TransmitSequence.cs:133`). *The first authoring put it on `OperatorSend`; it is not
    there.*
  - A typed line's cap is `MainWindowViewModel.LongestTypedSeconds` = 60 (`:15391`).
  - `Ft8ContactCard` computes *too long to send* against the 60.
- **Guards:** `TheFt8AndFt4SendsAreByteIdenticalTests`, `TheUnslottedSendTests`,
  `ThePsk31ModulatorTests`, `TheStopIsAlwaysOnScreenTests`, and `TheOliviaSeamTests`
  (`tests\Hamlet.App.Tests\ViewModels\`).
- **The PSK31 phase's four-signal recording** is `assets\fixtures\psk31-four-signals.wav`,
  with a `-48k` twin beside it.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload reads *CPS-DEC-0164* as the highest ruling in `CLAUDE.md` §1. The table's
  highest row is **HM-DEC-164**. `PROJECT_STATUS.md` `RULES_AT` still says HM-DEC-161.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`. The
  tree has `data/rsid/` and `data/bands/` (unit 358's instruction).
- These are uncommitted: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
  `WORK_INSTRUCTIONS.md` are modified, `output.md` is deleted, and 15 files under `.run-unit\`
  are changed. Those are the launcher's and the arbiter's writes. `SESSION.lock` may be
  untracked.
- The `UNIT 2 - STEP 1` entry in `PHASE_OUTCOME.md` carries `FATE: executed` for a run that
  never happened (the preamble above). **Report it; do not edit the entry.** The file is
  append-only.
- `docs\carry-forward-tests.txt` as read does not name `TheOliviaSeamTests`, though unit 358
  reported app 143 to 144. Say which is true.
- Two tests are red and neither is on the carry-forward list:
  - `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`, because of the two
    `_armedSend.Arm(` lines since `87485625`.
  - `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`.
- The app carry-forward list is flaky run to run (unit 357 item 1). Rerun a red that passes
  alone up to three times, and say in the report which run the number came from.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what
they rejected.**

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode
transmission Hamlet sends begins with the RSID burst naming its mode and variant - Olivia,
and PSK31 retroactively. Hamlet listens for RSID across the passband and when one arrives
sets the mode, the variant and the offset itself. The operator never picks a variant. A
carrier that never announced itself gets the blind search of step 2 as a fallback. The
tables and codes are `data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R30, in part.** *The fixtures under `assets/fixtures/olivia/` were made by the
web thread from the mode author's own transmitter ... with RSID bursts from fldigi's own
encoder. They are independent of anything Hamlet thinks Olivia is, which is the lesson of
the PSK31 phase.*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and
continue.*

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on.
  Never loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *Reading `pj_mfsk.h` tempts a port. Read the structure; write Hamlet's own. If a unit cannot
  proceed without copying, `MOVE: stop` and say what it would copy.*
- *A package is needed. `MOVE: stop`.*
- *Anything touches the transmit chain beyond adding an audio generator and an RSID prefix
  behind the one sequence. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R5 - The reference implementation.** *`fldigi`, which is GPL-3 - the same
licence as Hamlet. Cloned outside the tree at `C:\Source\fldigi`, pinned to one commit that
the step-1 unit records in its report, never committed, never ported wholesale ... What is
written for Hamlet is Hamlet's.* This unit is the RSID step, and **it records the commit.**

**PSK31 plan §R10 - the transmit chain may carry a send that has no slot.** *A send with no
slot is not held to a slot fit; it is held to a stated maximum length so a continuous carrier
cannot run on ... and it is not more than thirty seconds ... The gate, the single `PttOn` use
site, the `finally` that unkeys or aborts, and `StopNow` stay one code path shared by all
three modes. FT8 and FT4 must stay byte-identical, proved by the tests that guard them today,
run filtered. The transmission record says which mode went out ... Rejected: a second
sequence (a second `PttOn` site), an invented PSK31 slot, shortened macros, and splitting a
macro across slots (one click, several keyings).*

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was
shut, that later blocks the unit told to open the door, is the session's to rewrite in its own
commit so it guards the rule and not the shut door - and that is not a ruling, not an ask, and
not a stop.*

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step
adds writes an event ... that lets a person diagnose that stage from the file alone, proved by
assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit
writes the tests its criteria need and no others.*

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has
a same-thread, no-await abort available. One operator action, one transmission ... It never
transmits on a timer, on a decode, or because a contact "should" continue.* **A detected RSID
never causes a send.** *The dummy-load requirement is withdrawn in full ... not to be
referenced.*

**CLAUDE.md §2.1.3.** *Recorded off-air audio may contain callsigns and message content ...
fixtures committed to the public repo are reviewed by Tim first.* This unit commits no audio.

**FACT-004** *There are two computers, and only one of them has a radio on it.* **FACT-006**
*The development machine has no contact log and never will.* Nothing measured here is evidence
about the radio.

**HM-DEC-139**, **HM-DEC-155**, as in sections 1 and 3.

**The arbiter's own decisions for this unit.** These are author's decisions, overrulable, and
not rulings; they are in the decision block at the end.

- **A. The burst is part of the transmission and counts inside the existing cap, which does
  not move.** The typed line's *too long to send* estimate includes it, so the card and the
  gate agree. The cap is §R10's transmit-safety bound, so this unit neither raises it nor
  exempts the burst from it. **If task 1 finds that any of the four macros, at its composed
  length plus the burst, would exceed 30 s, task 4 is not built.** Report the numbers in
  section 4 for the next arbiter to take to Tim.
- **B. In this step `rsid_heard` is fed live under the Olivia tab only, and a detection
  changes no mode, variant, tab or dial.** R27's *sets the mode ... itself* is the rows of
  step 3. This step proves the reading.
- **C. The detector and the generator work from the `tone_sequences` in the file, and step 1
  needs only those.** Every fixture's code (1, 69, 70) has a sequence in the file. 1.4 names
  *the shipped burst's tone sequence from `rsid-codes.json`*. Codes 72 to 75 have no
  sequence, and the file has no table to derive one.
  - **Do not derive a sequence in code.**
  - If task 1 finds `rsid.cxx` readable at a pinned commit, the unit may add the missing
    table to the data file, with its citation and the commit (R27). Otherwise 72 to 75 stay
    undetected, which is a finding and not a partial: no criterion names them.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check that `PHASE_STATUS.md` has step 0 `done`.
- Hash `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav` first, then the other seven, against
  the manifest. If any fails, stop: step 1's entry is not met.
- Append `UNIT 359 - STEP 1` to `PHASE_OUTCOME.md`, in the shape of the `UNIT 358` entry.
- Patch-bump to 1.13.46.
- Run the carry-forward list, both invocations, before any change.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand.
- **Do not commit** `SESSION.lock`, `RUN_LEDGER.md`, anything under `.run-unit\`, or the
  deletion of `output.md`. Those are the launcher's, and the unit writes a new `output.md` at
  its end.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line.

1. **The fixtures.** For each of the nine: the sample rate, the channel count, the length, and
   where the RSID burst starts and ends, **measured from the audio**, not assumed from 2.32 s.
2. **The reference.** Is `C:\Source\fldigi` present? Record its commit hash (§R5 wants it), and
   whether `src\rsid\rsid.cxx` and `rsid_defs.cxx` are there.
   - **If the clone is absent, or reading it needs approval, that is a finding, not a stop.**
     The data file and `SOURCE.md` describe the burst.
3. **The data.** Confirm section 5's reading of `rsid-codes.json`, and say whether its contents
   are enough to detect and generate the three fixture codes without a literal in code
   (decision C).
4. **The send path, as it is.**
   - Where the PSK31 samples are made, and at what rate.
   - Where the cap is checked.
   - What the transmission record carries today.
   - **The seconds of each of the four macros and of a 60-second typed line as composed
     today, and each plus the burst's measured length against its cap.**
   - A hash of today's CQ samples at a stated offset. This is task 4's *before*.
   - The count of `PttOn` writes and of `_armedSend.Arm(` lines.
5. **The Olivia tab.** What audio reaches anything under it today (unit 358: the tick hands a
   running capture the tap's stream and nothing else).

**Before-numbers:** bursts read right 0 of 8; false detections on the two no-RSID files 0;
PSK31 sends announced 0 of 5 (four macros, the typed line).

**If item 4 shows a macro over its cap with the burst, decision A applies:** task 4 is not
built, and the unit carries on with tasks 2, 3, 5 and 6.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the detector (1.1, 1.2, 1.3)

`RsidDetector` in `src\Hamlet.RadioEngine\Rsid\`, **Hamlet's own code**:
- Read `rsid.cxx` for structure if it is readable (§R5, §6). Take every code, tone sequence,
  spacing and symbol rate from `rsid-codes.json`.
- It searches the passband the waterfall shows. For each burst it yields the code, the mode
  and variant named from the file, the center in Hz, a quality, and the time.
- It hashes each fixture against the manifest before it uses it (the 2.5 rule, met early for
  step 2).

**Test watched failing first:** `TheRsidDetectorTests`, engine.
- The six single-burst RSID fixtures each yield **exactly one** detection, with the right
  code and variant and the center within 5 Hz of 1000.
- `olivia-8-250-qso-norsid` and `olivia-noise-only-30s` yield **none**.
- `olivia-two-signals-rsid` yields **two**: 69 (8/250) at 1000 and 70 (16/500) at 2000, each
  within 5 Hz.
- The -16 dB fixture is also asserted on its own, so 1.3 is named.

**Report per fixture:** the expected code and center, what was detected, the error in Hz, and
the quality. **Never loosen 5 Hz.** A miss by a little ships, with the number and `partial`
(§6).

**Drop candidate:** none.

### Task 3 - Hamlet's own burst (1.4)

A generator in `Rsid\`. Given a code from the file, a center and a sample rate, it produces the
burst's samples: 15 tones plus the silence the file names.

**Test watched failing first:** `TheRsidBurstTests`, engine.
- The generator's tone sequence for each code that has a sequence equals `tone_sequences` in
  the file.
- **Loopback:** each of those codes, at three centers across the passband, reads back through
  task 2's detector as that code at that center, within 5 Hz.
- **Against the shipped burst:** in `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav`, the
  strongest tone in each of the 15 symbols matches Hamlet's sequence for that code, 15 of 15.
- The length is the file's symbol count over its symbol rate, to the sample.

**Drop candidate:** none.

### Task 4 - every PSK31 send begins with its announcement (1.5, `rsid_sent` of 1.6)

**Only if task 1 item 4 found every macro within its cap with the burst** (decision A).

Task 3's BPSK31 burst goes **in front of the PSK31 text audio, in the composition**. It is
centered on the send's own audio offset. It rides the same `UnslottedTransmission` through the
same `SendMessage` door, the same arming and the same `Ft8TransmitSequence`. Composing it in one
place is what puts it on the four macros and the typed line alike.
- The transmission record says the send was announced, and with which code.
- An `rsid_sent` event carries the code, variant and center, and no callsign or text.

**Test watched failing first:** `ThePsk31SendIsAnnouncedTests`, in the engine or app project,
wherever the composition is.
- **Loopback:** a composed CQ, through the detector, yields code 1 at the send offset within
  5 Hz.
- The samples after the burst are **identical to task 1's before-hash** of the same CQ.
- A typed line and each of the other three macros begin with the burst.
- The record and `rsid_sent` say so, with nothing personal.
- The typed line's *too long to send* estimate includes the burst.
- The count of `PttOn` writes, and of `_armedSend.Arm(` lines, is **unchanged from task 1**.

These run filtered, green and **unedited**: `TheFt8AndFt4SendsAreByteIdenticalTests`,
`TheUnslottedSendTests` and `TheStopIsAlwaysOnScreenTests`. A PSK31 test that asserted the audio
begins with the text is the unit's to rewrite, in its own commit, to guard the rule (§R12).

**The stop, in the plan's words.** If this needs anything beyond composing audio ahead of the
text, **do not build it**. That includes:
- a change to `Ft8TransmitSequence`, the gate, `Stop`, or the cap's value;
- a second keying, or any second path.

Write in section 4 what it would need, name it `MOVE: stop` material, and carry on with tasks 5
and 6.

**Drop candidate:** none. *Not built under decision A* is not *dropped*, and the report must say
which.

### Task 5 - `rsid_heard`, live under Olivia (1.6)

- The detector writes `rsid_heard`: code, mode, variant, center, quality. **No callsign, no
  text.**
- Under the Olivia tab, the tap's audio (at the rate task 1 found) feeds the detector, alongside
  the capture unit 358 left.
- **A detection changes nothing else** (decision B). No mode, variant, tab or dial moves; no row
  appears; nothing is sent.

**Test watched failing first:** extend `TheOliviaSeamTests`.
- A fixture fed through the tap under Olivia yields its `rsid_heard` with the fields and nothing
  personal.
- The same audio under FT8 yields no RSID event.
- The dial, mode and tab are unchanged after a detection.
- `BindingHealthTests` and `VoiceTests` stay green.

**Then put this unit's guards on the carry-forward list:**
- `TheRsidDetectorTests` and `TheRsidBurstTests` on the engine invocation;
- `ThePsk31SendIsAnnouncedTests` on whichever invocation holds it;
- `TheOliviaSeamTests` on the app invocation, if task 1 found it absent.

They keep step 2's entry and 4.2 honest. Run the list, both invocations, at the end.

**Drop candidate:** none.

### Task 6 - the detector keeps up (1.7, nice-to-pass)

Time the detector over `assets\fixtures\psk31-four-signals.wav` (name which of the two files
was used, and its rate) and over `olivia-two-signals-rsid.wav`. Report the ratio of CPU time to
audio time for each.

**Test:** none needed beyond a reported measurement. Put it in the detector test's output.

**Drop candidate: this whole task.** Drop it whole, and say that it was dropped.

---

## 9. Parked - do not touch, do not raise

- **The Olivia demodulator, the blind search, the timing table's measurement** - step 2.
- **A detection that sets the mode, variant or offset, and rows per station** - step 3
  (decision B).
- **Listening for RSID under the PSK31, FT8 or FT4 tabs** - step 3's Hamlet-wide listening.
- **An Olivia modulator or any Olivia send** - step 4.
- **Codes 72 to 75**, beyond decision C's one permitted data addition.
- **The dial 1500 Hz below the center** (unit 358 section 4 item 1). Step 0 is closed, and only
  Tim reopens it.
- **Three of unit 358's items (3 to 5)**, each already filed or step 2's:
  - HM-OPEN-090, `cq_pressed` saying `Ft8`;
  - `SOURCE.md`'s date pin: record fldigi's commit in the report, and do not edit `SOURCE.md`;
  - `timing.json` unparsed.
- **The mislabeled `UNIT 2` outcome entry.** Report it in section 1, and leave the harness
  alone.
- **The screen phase's open asks**: the card at 1100 x 780, CQ below the window, the flaky stop
  tests, the typed line not in the conversation, the ALC never polled, the box's missing
  sender. They are carried in section 4, not worked.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** It
  is what killed three sessions.
- **No literal RSID code, tone, spacing or symbol rate in code.** The file is the citation
  (R27), and a second copy is how the two drift.
- **Do not change `Ft8TransmitSequence`, the gate, `Stop` or the cap's value, and do not add a
  `PttOn` or `Arm` site.** §6 and §R10: the prefix is the one transmit change this step is
  licensed for.
- **Do not port `rsid.cxx` wholesale, and add no package.** §R5 and §6. Either one is a
  `MOVE: stop`.
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md` or earlier `PHASE_OUTCOME.md`
  entries.**
- **Report mismatches; repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Never commit `SESSION.lock` or `.run-unit\`.
The report names the branch and whether every push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or not
built under decision A. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner
should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0 done
   and closed; step 1 <state after this unit>; steps 2-6 not started.
B. Step 1's criteria 1.1 to 1.7 - each met or not, with its number: bursts
   read right N of 8, worst center error X Hz, false detections N on 2 files,
   the -16 dB burst read yes|no, Hamlet's burst read back N of M, shipped
   bursts matched N of 30 symbols, PSK31 sends announced N of 5, FT8/FT4
   byte-identical yes|no, rsid_heard/rsid_sent asserted yes|no, real-time
   ratio or dropped.
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of a criterion in B - in particular
   whether task 4 was not built under decision A, or stopped at the
   transmit chain, and whether the fldigi commit was recorded.
```

**Then the header:**

```
UNIT:       359 - <complete|stopped> at task N of 7, <which dropped or not built> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     bursts read right 0 -> N of 8; PSK31 sends announced 0 -> N of 5
DRIFT:      0
```

**Then a criterion table, 1.1 to 1.7**, as unit 358's was.

**Section 3 leads with the detection table**, eight lines, one per burst:
- the fixture;
- the expected code and center;
- the detected code and center;
- the error in Hz;
- the quality.

After it come the two no-RSID files with their detection counts, then the PSK31 CQ loopback
line, giving the code and center read back from Hamlet's own send. Then the `rsid_heard` and
`rsid_sent` events as written. **Section 1 records the fldigi commit** (§R5), task 1's findings
and the section 5 mismatches.

**Every appearance claim is computed, not seen. Nothing here is evidence about the radio**
(FACT-004).

---

```
ARBITER-DECISION
STEP: 1
APPROACH: Hamlet's own RSID detector across the passband reading codes and tone sequences from the ported data file, proved on the mode author's fixtures, and an RSID burst generator composed in front of every PSK31 send's audio behind the one transmit sequence
MOVE: continue
WHY: Step 0 is done and closed and step 1 is next in a one-way pipeline. The loop test matches the UNIT 2 entry, but that run never executed - it halted on the session lock, carries unit 358's cost, and left no commit - so the approach is untried, not looped. The only transmit change is the RSID prefix section 6 licenses, fenced by a stop.
STATE: not started
DECIDED: author's, overrulable - (A) the burst counts inside the existing no-slot caps, 30 s for a macro and 60 s for a typed line, which do not move; if any macro would exceed its cap with the burst, task 4 is not built and the numbers go to Tim as a section R10 question. (B) rsid_heard is fed live under the Olivia tab only, and a detection changes no mode, variant, tab or dial in this step. (C) detection and generation work from the tone_sequences in rsid-codes.json, which cover every fixture's code; codes 72 to 75 have no sequence and no table, and none is derived in code - the table may be added to the data file with a pinned citation, or they stay undetected as a finding. The UNIT 2 outcome entry's FATE executed is read as never ran. Unit 358's section 4 item 1 (dial 1500 Hz below the center) is logged, not reopened: step 0 is closed.
LICENCE: PHASE_PLAN.md R27, R30, R31 and section 6 (the RSID-prefix clause, a done step is closed); PSK31 plan R5, R10, R12, R13, R14; CLAUDE.md 0.2 and 2.1; HM-DEC-139, HM-DEC-155; ARBITER.md section 6 (a number or a mechanism is the arbiter's) and section 8 (a never-ran entry is not a tried approach)
ACCOMPLISHED: Hamlet hears an RSID announcement anywhere in the passband and names the mode, variant and spot from it on the mode author's own audio, and every PSK31 transmission it makes announces itself the same way, with FT8 and FT4 unchanged
ADVANCES: step 1 criteria 1.1, 1.2, 1.3, 1.4, 1.5, 1.6; 1.7 is task 6, the drop candidate
END-ARBITER-DECISION
```
