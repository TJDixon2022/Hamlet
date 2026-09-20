# Work instruction 367 - move up and widen: the one click that takes the QSO off the calling spot

**Step 4 of `PHASE_PLAN.md`, authored by the arbiter.** Unit 366 opened the mode gate and an Olivia
press now goes out: the CQ on the band's cited calling spot, an Answer at the row's own variant and
center, the RSID in front of every send read back within 0.34 Hz, the cap and the patience scaled by
the variant, Stop aborting mid-play, the power offer and the ALC as PSK31 has them. **Step 4 stands
at 6 of 8.** Two criteria are left and neither has been worked: **4.5**, R29's *move up 500 Hz and
switch to 16/500*, and **4.8**, the turn indicator within one block of the turnover word.

**This unit builds 4.5 and measures 4.8.** 4.5 is the last must-pass in the step and the only piece
of the phase's etiquette promise - *the beginner does not have to know to move off the calling
frequency, because one click does it* - that is not built. **Six tasks, 0 to 5. Task 5 (4.8, the
only nice-to-pass left) is the drop candidate.**

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

The arbiter checked all four against the tree on 2026-09-19 after unit 366's report commit, and they
held: both files are present, neither solution exists, and the root is `C:\Source\HamLet`.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: **two invocations, one build each**, with a status write immediately
before each. Never background and poll.

**The engine invocation took 5 m 1 s for 146 tests** at the end of unit 366, inside the 480 s
timeout by about three minutes - a minute less margin than unit 365 had. **That margin is this
unit's to keep. Nothing this unit adds goes on the engine carry-forward line.** Report the seconds
of both invocations every time they run.

**HM-DEC-165 - a mode that works stays working.** The list carries a send guard and a read guard for
every working mode, and since unit 366 Olivia has all three rows: read, modulate and **send**. **A
red after that was green before is a regression.** Name it in sections 1 and 4, and **the repair is
this unit's next task before any new criterion**. This unit touches the send path again - a fifth
thing to send - so no app red touching a send is presumed a flake: say what it touches and which run
the number came from.

**CPU is measured alone.** Every class this unit adds that asserts or reports CPU goes in the
non-parallel `CpuMeasuredAlone` collection.

## 2. The tool facts, as units 359 to 366 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. **Unit 366 wrote every
  commit message into a file under `.run-unit\` with the editor and passed it with `git commit -F`**,
  which worked. Do the same.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing. Use the
  editor. A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  because warnings are errors.
- **`>` redirection is refused anywhere under the root**, including into `.run-unit\`, as is
  `sed -i` on a source file and `echo` into a file. Use the file editor.
- **Python did not run for units 365 or 366** (`python -c` and `python file.py` needed approval),
  though it ran for 361 and 362. **Do not build a task on it.**
- `rm` is refused. A `for` loop over a variable was refused as *simple_expansion*. Do not depend
  on `;`.
- A `grep` pattern containing `\|` was read as several operations and refused; **`grep -E` with the
  same alternation ran.** `grep -o` with a quantifier and `grep -v` in a pipe need approval.
- **`tools\arbiter\validate-output.bat` could not be run by units 365 or 366.** If it will not run,
  check the report's shape by hand against the rules the script prints - the ordering block with A,
  B and C and C's item count; the `UNIT:` line above section 1; the four `##` sections in order with
  their exact names and no fifth; section 3 non-empty; `###` nested under them and nowhere else -
  and say in section 4 that you did.
- These needed approval, which a headless session cannot give: `mkdir`, `cp`, `mv`, `tee`,
  `powershell.exe`, `jq`, `awk`, `git restore --source`, `git checkout <rev> -- <file>`,
  `git stash push`, `git config`, and command substitution. A command that includes one is refused
  whole. **Keep `Passed!|Failed!` in every filter** so the summary line survives.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. `sed -n N,Mp` and
  `tail -n +N | md5sum` in a pipe after `git show` ran.
- Anything outside `C:\Source\HamLet` cannot be listed or read.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `tools/status.sh` writes
  `RULES_AT: HM-DEC-165 (2026-09-19)` and reads `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which
  says 366 until you set it.

## 3. Asks still outstanding

Carried per HM-DEC-139. **The last report is `output.md` at `HEAD` (`efc07610`), unit 366's, 1619
lines.** Its `## 4. What's blocking us` heading is at **line 468**. **Carry everything from line 469
to the end verbatim** - its eight items and the queue it carries from unit 365, which carries the
FT8 unit's, 364's, 363's and the older ones. Recover the bytes whichever way the harness allows;
unit 366 kept `output.md` from `HEAD` in place and replaced only the lines above the queue with the
editor, and that path works and needs no refused command.

**Check the carried copy.** `git show efc07610:output.md | tail -n +469 | md5sum` gives
`685f335dc84fdfdf8da1b686951bb2e4`. The same `md5sum` over your carried block must give the same
hash. **Report both.**

**Mark nothing in place** except what section 9 says this instruction answers.

---

## 4. Why this unit exists

**This is unit 367, the seventh unit of step 4 and the tenth of the phase.** Step 4 stands at **6 of
8 criteria met** - 4.1, 4.2, 4.3, 4.4, 4.6 and 4.7 - with **4.5 the last must-pass** and 4.8 the
last nice-to-pass. Step 5 cannot start until step 4 is done, and step 5's entry is *a loopback
exchange reaches 73*, which is the same conversation 4.5 sits in the middle of.

R29 is the reason 4.5 exists, and it is one of the three things §1 of the plan says makes Olivia
hard for a beginner: *the etiquette of moving off the calling frequency and widening - a macro.* A
newcomer does not know that a QSO answered on the calling spot is supposed to move off it, or that
the answer is to go up and get faster. **One click is Hamlet's answer to that, and it is the only
one of the three §1 names that is still unbuilt.** Everything it needs now exists: the modulator,
the gate, the send at a chosen center and variant, the RSID both ways, the card, the certainty gate
and the cited table.

4.8 is a **measurement, not a build.** Unit 366 established that nothing in the application consumes
a time-based patience - every turn is decided by `Psk31Turn.Read` off the parse, reading no clock -
so what 4.8 asks is how quickly that reading follows the turnover word on an Olivia channel, in
blocks. It is the drop candidate because it is nice-to-pass and because 4.5 is the criterion that
closes the step.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own announcement and never
            picked by the operator.
UNIT GOAL:  Build R29's one click. After a certain answer on the calling spot the card
            offers to move up 500 Hz and switch to 16/500; pressing it sends the line
            saying so at the old place and the old variant, then moves Hamlet's own send
            and listen center up 500 Hz and its variant to 16/500, and the card then says
            what was heard where they moved to - the other end's 16/500 announcement, or
            nothing yet. Then measure how far behind the turnover word the turn indicator
            lands, in blocks.
ADVANCES:   step 4 criterion 4.5 (tasks 2, 3 and 4, must-pass - the last must-pass in the
            step) and 4.8 (task 5, nice-to-pass, the drop candidate). If both are met step
            4 is 8 of 8 and done; if 4.8 is dropped step 4 is 7 of 8 with every must-pass
            met, which is what the plan's §6 calls done on its must-passes.
DRIFT:      0 - unit 366 met 4.2, 4.3, 4.4, 4.6 and 4.7.
```

**Read `PHASE_PLAN.md` at the root in full.** Most of all: step 4 and its entry, R27, R28, **R29**,
R31, R32, §1, §3.2 and §6's transmit clause.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 20:46 and the arbiter's own reading
after it:

- **HEAD is `efc07610`**, unit 366's report commit. Version **1.13.54** in `Directory.Build.props`
  (line 893).
- **The gate is open.** `CanTransmitIn` is at `MainWindowViewModel.cs:2459` and admits null, `FT8`,
  `FT4`, `PSK31` **and Olivia** (:2464). `IsPsk31Chosen` :2467, `OliviaLabel` :2471,
  `IsOliviaChosen` :2481, `CanAnswerRowsForTests` :4423.
- **The keying sites.** `CivConstants.PttOn` has **one** code line
  (`Ft8TransmitSequence.cs:513`). `_armedSend.Arm(` has **two** (`MainWindowViewModel.cs:15551`
  and **:15830**). The second is the unslotted one.
- **Where a send goes and in which variant.** In `SendMessage`, `var at = _psk31SendAtHz;` (about
  :15695) and `var variant = olivia ? _oliviaSendVariant ?? OliviaCallingTable.CallingVariant :
  null;` (about :15698). The offset is chosen at **:15733**:
  `at ?? (olivia ? OliviaCallingOffsetHz() : null) ?? ClearSpotForTheCall()`, refusing with
  `no_clear_spot`. **So a send at a named center in a named variant already has two fields to set
  and needs no new door.** The no-announcement refusal sits above it and runs before the spot is
  chosen. `ClearSpotForTheCall` is at :16422 and calls `Psk31ClearSpot.Choose` at :16436.
- **The two windows are different numbers and this unit turns on the difference.**
  `Psk31ClearSpot`: `ClearHz` **150**, `LowestCallHz` **400**, `HighestCallHz` **2200** - where a
  call to anyone may start. `Psk31CarrierSearch.PassbandLowHz` **200** and `PassbandHighHz` **3000**
  - what Hamlet listens across and what the RSID detector is built with (`HearRsid`, about :3135).
- **The card and its one action.** `Ft8CardActionKind` (`Ft8ContactCard.cs:14`) has exactly
  **`None`, `Send`, `Log`**. `ActionFor` is at `MainWindowViewModel.cs:5700` and `CardActionAsync`
  at :5874, which sets `_psk31Macro = card.Offered` at :5898. The card's turn reading is
  `Ft8ContactCard.Turn` (:205), its state words at :326-:328 and its sentences at :357-:361.
- **What may be offered.** `Psk31Macro` has `None`, `Cq`, `Answer`, `Report`, `Confirm` **and no
  fifth member**. `Psk31Offer.For` returns `None` unless the reading is a **certain** *your turn*
  over a certain message addressed to the operator that hands over - §R1's strict side.
- **The turn.** `Psk31Turn.Read(talk, sending, mine)` at `MainWindowViewModel.cs:4121`, inside the
  card build; there is no clock in it.
- **The timing table.** `OliviaTiming` (`data/olivia/timing.json`) has `CapSeconds(variant, kind)`,
  `PatienceSeconds(variant)`, `RetireWindowSeconds(variant)`, `CapMacroCharacters` 121,
  `CapTypedCharacters` 242, `PatienceCharacters` 32, `RetireAfterCharacters` 56, seconds per
  character 0.68267 / 0.512 / 0.4096.
- **The calling table.** `OliviaCallingTable` (`data/bands/olivia-calling.json`) with
  `CallingVariant = "8/250"`, `CallingRowFor(band)`, `DialHzFor(row)` and `AudioCenterHz`.
- **The RSID feed.** `HearRsid` writes `rsid_heard` **and does nothing else** - unit 359's decision
  B, quoted in its own remarks: *no mode, variant, tab or dial moves, no row appears and nothing is
  sent.* Decision BQ below lifts that one inch and no further.
- **Nothing in `src\` knows what a QSY is.** A case-insensitive search for `qsy`, `move up 500` and
  `MoveUp` finds only CW comments and `FavoritesViewModel.MoveUp`, which is a list control. **4.5 is
  new work, not a widening.**
- **The Olivia test classes that exist.** App: `TheOliviaSendTests`, `TheOliviaRowsTests`,
  `TheOliviaRowsReadLikePsk31Tests`, `TheOliviaRowsKeepUpTests`, `TheOliviaSeamTests`. Engine: the
  demodulator, listener, modulator, blind search, drift, retire, timing, data and below-the-noise
  classes.
- **The lists.** `docs\carry-forward-tests.txt` carries the guard table with Olivia's three rows -
  read (`TheOliviaDemodulatorTests`), modulate (`TheOliviaModulatorTests.EachMacroAndATypedLine
  ComeBackIdentical`) and **send** (`TheOliviaSendTests.AnOliviaCqReachesTheAir`).
  `docs\chain-guarding-tests.txt` holds 17 engine classes and 11 app classes, not the thirteen an
  older instruction said (unit 366 mismatch 3).
- **Carry-forward at the end of unit 366:** app **189 of 189** (2 m 34 s), engine **146 of 146**
  (5 m 1 s). Chain-guarding: engine **98 of 110** (46 s), app **48 of 63** (45 s), the same
  twenty-seven inherited reds by name before and after. `PttOn` write lines **1**, `Arm(` call
  lines **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree.**

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload's disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-165 (2026-09-19) and
  `CLAUDE.md` §1 holds CPS-DEC-0165. That is the carried id-scheme split. **Do not repair it.**
- **`PHASE_STATUS.md` says `CURRENT_STEP: 3` with step 3 `partial`**, though `PHASE_PLAN.md` §8's
  revision of 2026-09-19 evening checks 3.0 to 3.6 and unit 365's decision AO wrote step 3 done. It
  is the launcher's file. **Report what it actually says; change only `WORK_INSTRUCTION` to 367**,
  which `tools/status.sh` reads from there, as unit 366 did.
- **`PHASE_PLAN.md` leaves 1.5, 1.7 and every one of 4.1 to 4.7 unchecked** though units 359 to 366
  met them. **Reported, not edited** (§10).
- **`PHASE_OUTCOME.md` carries two step 4 entries whose decision letters clash** (AO-AY twice). The
  live set is unit 365's; unit 366's ran from AZ to BJ; **this instruction's start at BK so nothing
  collides.** The file is append-only: **do not edit any earlier entry.**
- R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`. The tree has
  `data/rsid/` and `data/bands/`.
- **Untracked in the root and under `.run-unit\`:** `.u365-head.md`, `.u365-report.py` and unit
  366's `commit-366-*.txt` scratch, left because `rm` is refused. **Do not commit them and do not
  build on them.**
- Red and not on the carry-forward list, as they were before unit 366:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- **The twenty-seven chain-guarding reds are inherited and carried as ask 35.** That list holds
  known reds on purpose. Compare by name, do not chase.
- Known headless flakes: `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed` (unit
  366 item 7 - **it touches the power offer, which unit 366 widened**),
  `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` and
  `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`.
  `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`, which
  unit 365 called a deterministic red, was **green in every run of unit 366**. Rerun a red that
  passes alone up to three times and say which run the number came from.
- Uncommitted at authoring: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`, `.run-unit\`,
  and this file.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what they
rejected.**

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode transmission
Hamlet sends begins with the RSID burst naming its mode and variant - Olivia, and PSK31
retroactively. Hamlet listens for RSID across the passband and when one arrives sets the mode,
the variant and the offset itself. The operator never picks a variant. A carrier that never
announced itself gets the blind search of step 2 as a fallback. The tables and codes are
`data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R28 - Identical to PSK31 above the modem.** *The same receipt, conversation card,
Answer, Report, Confirm on certainty, the typed line framed with the callsigns and the hand-back,
the same parser. The mode chip says Olivia, the row says the variant, the calling spot is
Olivia's. Timing rules scale with the variant (§3.2).*

**PHASE_PLAN.md R29 - The established locations, as FT8, FT4 and PSK31 have.** *The calling table
is cited data, `data/olivia-calling.json`, a community convention and labeled as one. Pressing
Olivia tunes to the 8/250 spot for the band. **After a certain answer on the calling frequency the
card offers one click - move up 500 Hz and switch to 16/500 - which sends the line saying so,
retunes, changes variant, and the other end's RSID confirms he followed.*** **This is the criterion
this unit is for; read it twice.**

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md R32 - Tim, 2026-09-18.** *(a) The RSID burst does not count against the send cap.
The cap measures the macro or the typed line; the announcement is a fixed 2.3 seconds in front of
it. (b) The transmission record says a send was announced - `announced: true` and the RSID code on
every keyboard-mode send. Both were the arbiter's to decide under §6 and it stopped instead; a cap
the plan calls the author's number, and a field on a record, are neither the radio's safety nor a
promise, and are never a stop.*

**PHASE_PLAN.md §1, on what makes Olivia hard for a beginner:** *which variant (the signal
announces it - §R27); where (the cited table - §R29); **the etiquette of moving off the calling
frequency and widening (a macro - §R29)**.*

**PHASE_PLAN.md §3.2 - Speed.** *Every timing rule - the send cap, the turn indicator's patience,
the retire window - scales with the variant's seconds per character, taken from the mode author's
audio ... and never fixed in seconds.*

**PHASE_PLAN.md step 4, the criteria this unit works, verbatim:**
- *4.5 After a certain answer on the calling frequency the card offers* move up 500 Hz and switch to
  16/500*; one click sends the line, retunes, changes variant, and the card says whether the other
  end's RSID followed.* **must-pass**
- *4.8 The turn indicator changes within one block of the other station's turnover word.*
  **nice-to-pass**

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *A mode that works stays working - HM-DEC-165, Tim, 2026-09-19. No unit is complete, whatever its
  own criteria say, if a mode that reached the air before it does not reach the air after it, or a
  mode that read the air before it reads less. The carry-forward list carries a send guard and a
  read guard per working mode; a red after is a regression, named, and the next unit's first task
  is the repair.*
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator - a fact stated about the
  radio, a contact or a send. A hint, a label, a number, a layout, a mechanism arithmetic will not
  allow: the arbiter decides, marks it author's and overrulable, and continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on. Never
  loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *A package is needed. `MOVE: stop`.*
- ***Anything touches the transmit chain beyond adding an audio generator and an RSID prefix behind
  the one sequence. `MOVE: stop`.***
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R1 - the certainty gate.** A macro is offered only on a **certain** reading; never on
a guess. **The move is offered under the same gate and on no looser one.**

**PSK31 plan §R10 - one click, one transmission, one keying path.**

**PSK31 plan §R11 - nothing at the radio.** Nothing this unit needs is asked of the operator at the
rig, and **nothing this unit does sends the radio to a new frequency** (decision BK).

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was shut,
that later blocks the unit told to open the door, is the session's to rewrite in its own commit so
it guards the rule and not the shut door - and that is not a ruling, not an ask, and not a stop.*

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file alone,
proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).* **The move
is a stage. It gets its events.**

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others.*

**CLAUDE.md §0.0 - honesty.** A sentence on the screen says what Hamlet actually knows. **The card
never says a station followed when what Hamlet heard was an announcement with no callsign in it**
(decision BP).

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never transmits
on a decode.* **The move is pressed. Nothing about a detection, a parse or a turn reading may cause
a transmission.**

**FACT-004.** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio. **No Olivia signal from Hamlet has ever been on the air. The
first one will be Tim's, at step 6.**

**HM-DEC-139**, **HM-DEC-155**, **HM-DEC-165**, as in sections 1 and 3.

**Standing decisions of earlier instructions, still in force:** D (the cap measures the samples
after the announcement), F (the no-slot record carries `announced`, `rsidCode` and
`announcementSeconds`), H (format facts from `data\olivia\format.json` with `pj_mfsk.h` citations -
transcription, never a port), **I (the variant and the center come from the detector or the row,
never from the test)**, AF (one row path), AJ (the retire window is 56 characters of the variant),
unit 365's AP to AY (the modulator), and unit 366's **AZ** (one door, and the forbidden list),
**BA** (where a send goes, and never the operator's choice), **BB** (the clear-spot rule unchanged),
**BC** (PSK31's receipt), **BD** (the character counts stay as measured), **BF** (Stop by PSK31's
path, `PttOn` 1 and `Arm(` 2), **BH** (the send events and Olivia's send guard). **AZ's forbidden
list binds this unit exactly as it bound unit 366.**

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings. They continue
unit 366's letters.

- **BK. *Move up 500 Hz* moves the audio center, not the dial.** The move is +500 Hz on the offset
  inside the passband Hamlet is already listening across; **no CI-V frequency command is sent, the
  VFO stays exactly where pressing Olivia put it**, and the dial reading on the window does not
  change. That is how two Olivia stations move off a calling spot on one dial, it is what the other
  end's software will follow, and it keeps R11. *Rejected:* moving the VFO, which touches the radio
  mid-QSO, and which the other end cannot see or follow because his software follows an RSID in the
  passband.
  - **The bound is the listening passband, not the calling window.** The moved-to signal must lie
    wholly inside `Psk31CarrierSearch`'s 200-3000 Hz: 16/500 occupies about ±250 Hz, so a new center
    up to about 2750 Hz is allowed. **`Psk31ClearSpot`'s 400-2200 Hz is not the bound** - that is
    where a *call to anyone* may start, and a QSO that has already begun is not a call to anyone.
    On 20 m the calling offset is 1500 Hz and the move lands at 2000 Hz, inside both; **say what the
    offer does on a band whose calling offset would put the move outside the passband** - it is not
    offered, and the card says why rather than offering a move that would put the signal where
    Hamlet cannot hear the answer.
- **BL. When the offer appears, and it is R1's gate and no looser one.** All four, together: Olivia
  is the chosen mode; the card's turn reading is a **certain *your turn*** over a certain message
  addressed to the operator that hands over - the same reading `Psk31Offer.For` demands and not a
  copy of it loosened; the conversation is **at the band's calling center from the cited table**;
  and the variant is **8/250**. **It is offered once per conversation** - once the move has gone
  out, the offer is gone whether or not the other end followed. It appears **beside** the macro
  offer, not instead of it: the same certain answer that offers Report offers the move.
- **BM. It is its own one-click control, and PSK31's card is not touched.** `Ft8CardActionKind`,
  `ActionFor` and `CardActionAsync`'s `Send` and `Log` paths **stay as they are**, and no member is
  added to `Psk31Macro`. The move is a second property and a second command on the card, live only
  under BL's four conditions, so **nothing appears on a PSK31 card and no PSK31 test changes**.
  *Rejected:* a fifth `Psk31Macro` member and a fourth `Ft8CardActionKind`, which put an Olivia-only
  idea into two types every mode shares, for a gain the operator cannot see.
- **BN. What the line says, and where it goes out.** A new Olivia macro, framed as Hamlet frames
  text, in plain American, carrying **both callsigns, the amount and the variant** - the arbiter's
  recommended text, which the unit may shorten and **must quote verbatim in the report**:
  `HIS de MINE  QSY UP 500 TO OLIVIA 16/500  QSY UP 500 TO OLIVIA 16/500  HIS de MINE K`.
  **It goes out at the old place and the old variant** - the calling center at 8/250, with the 8/250
  burst, code 69 - because that is where he is listening. It is a macro for the cap (`OliviaSendKind
  .Macro`), it is composed by `OliviaModulator` like every other send, and it reaches the air
  through the one gate and the one existing `Arm(` site. **It is a fifth thing to send, not a fifth
  way to send.**
- **BO. Nothing moves until the line has actually gone out.** The order is: compose, arm, key, play,
  **ordinary unkey** - and only then does Hamlet's send-and-reply center become +500 and its variant
  16/500. **A refusal, a cap failure, a `no_announcement` or a Stop moves nothing**, and the card
  says so in words. Hamlet is never at a place it did not announce, and never announces a place it
  is not at.
- **BP. What *he followed* may be claimed, and what may not.** The card's follow line has **three
  states**: *waiting*, *an announcement arrived*, and *none arrived*. An announcement counts when it
  is an **Olivia 16/500 code whose center is within the 16/500 occupied bandwidth (about ±250 Hz) of
  the new center**, heard inside the window. **RSID carries no callsign** (§0.0), so the card says
  what was heard where they moved to, and **never that the station personally followed as a
  certainty**; and it never says he refused - after the window it says nothing arrived. **The window
  is the timing table times a stated factor, never seconds** (§3.2): `OliviaTiming.PatienceSeconds
  ("16/500") × 2 = 32.768 s`, asserted as the product. A different whole factor is allowed if the
  unit measures a reason for it - say the reason and the number.
- **BQ. One inch of unit 359's decision B is lifted, and no more.** An Olivia-tab RSID detection may
  be **read** by the follow check. **No detection sets the mode, the tab, the dial, or a variant the
  operator did not press for**, and R27's across-tab switch stays parked. Take the evidence from
  `HearRsid`'s detections or from the Olivia listener's channel-open, whichever the trace shows
  carries the code and the center at the right moment; **name which and why**.
- **BR. The card survives the move.** A station that moves is the same conversation and the same
  card - R28, and 4.5 is unshowable on a card that dies at the QSY. The trace says how a
  conversation is keyed and what ends it; if the move would end the card, **keeping it is part of
  task 4 and not a finding**. Report what that took. If it cannot be kept without a line on AZ's
  forbidden list - it should not be, that list is the transmit chain - **that is `MOVE: stop`**.
- **BS. 4.8 is measured, and nothing is invented to have something to point at.** Unit 366 measured
  that no application path consumes a time-based patience: every turn is `Psk31Turn.Read` off the
  parse at `MainWindowViewModel.cs:4121`, reading no clock. So 4.8 is **the lag, in seconds and in
  blocks**, between the turnover word's last character being accepted on an Olivia channel and the
  card's turn state changing. **One block of that variant or less is met.** If the splitter's rule -
  a message closes on the character *after* the turnover (unit 364 item 6) - makes the lag longer
  than one block, **report the number, leave 4.8 nice-to-pass and not met, and name what would
  change**. **Never loosen it, and never add a clock to the turn** (R14).
- **BT. The entry and the chain (HM-DEC-165).** Run `docs\chain-guarding-tests.txt`'s 17 engine and
  11 app classes by exact name, foreground, **before any change and again after task 3**, and
  compare the reds by name. Run `docs\carry-forward-tests.txt`, both invocations, before the first
  change and after the last. **`PttOn` code lines stay 1 and `Arm(` call lines stay 2**, counted and
  reported at the end. **A red after that was green before is a regression and the repair is the
  next task** (section 1).

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- **Step 4's entry, first** - *the loopback chain of the PSK31 phase unchanged, checked by its
  guarding tests first*: decision BT's before-run of the chain-guarding list, reds by name and
  seconds. Then `ThePsk31CqGoesOutTests`, `TheFt8AndFt4SendsAreByteIdenticalTests` and
  **`TheOliviaSendTests.AnOliviaCqReachesTheAir`** by name. **If a class that guards the PSK31
  loopback chain or Olivia's own send is red, stop and report it** - the entry is not open and the
  repair is task 1.
- Hash all nine Olivia fixtures against `manifest.json`, 9 of 9.
- Report what `PHASE_STATUS.md` and `PHASE_PLAN.md` actually say about step 4, and the two clashing
  step 4 entries in `PHASE_OUTCOME.md` (section 5).
- Append `UNIT 367 - STEP 4` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 366`
  entry. **Touch no earlier entry.**
- Patch-bump the version by one, from 1.13.54.
- Run the carry-forward list, **both invocations**, before any change, with the seconds and the
  per-mode guards' states.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md`, `.u365-head.md`, `.u365-report.py` or anything under
  `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2 writes
a line. Put it in `Unit367Trace`, a class that asserts nothing, in the shape of `Unit366Trace`, not
on either carry-forward line.

1. **The card's life.** How a conversation and its card are keyed - by callsign, by channel, by
   center - where they are built, and **what ends one**. Then the question decision BR turns on:
   **what happens to the card when the same station's signal moves to a new center and a new
   variant** - his old channel retires and a new one opens, so does the card follow him, or does it
   go? Name the file and the line for each answer.
2. **Where the conversation's own center and variant live**, if they live anywhere, and how
   `_psk31SendAtHz` and `_oliviaSendVariant` are set today for an Answer, a Report and a Confirm.
   **Name every line decision BO would set to make the next send go out 500 Hz up at 16/500.**
3. **The offer.** Where `Psk31Offer.For`'s reading is taken, where a card's action label and command
   are built (`ActionFor` :5700, `CardActionAsign` :5874), and **where a second one-click control
   would attach without touching `Ft8CardActionKind`, `Psk31Macro` or PSK31's own card** (decision
   BM).
4. **Does the card know it is on the calling center?** Whether the band's Olivia calling offset is
   reachable where the card is built (`OliviaCallingOffsetHz`), and what it gives on two bands.
5. **The RSID detections.** Where they land today (`HearRsid`, about :3135), whether anything but
   the telemetry line sees them, and whether the Olivia listener's channel-open carries the code and
   the center. **Say which of the two decision BQ should read and why.**
6. **The turn, for 4.8.** Where a turn is decided, what input changes it, and - on the two-signal
   fixture or one made from the shipped audio - **the measured lag in seconds and in blocks** from
   the turnover word's last accepted character to the reading changing. This is 4.8's number; if
   task 5 is dropped, **this measurement still stands and is reported**.
7. **Every `Psk31`-named member on the move's path that is gated on `IsPsk31Chosen`**, by name and
   line - unit 366 item 6's pattern, bounded to this path and not swept wider.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the offer appears at the right moment and nowhere else (4.5, first half)

Build to decisions BL and BM.

**Tests watched failing first**, in a new app class, `TheOliviaMoveUpTests`:

- **It is offered** on a card whose turn is a **certain *your turn*** over his certain answer, under
  Olivia, at the band's calling center from the table, at 8/250.
- **It is not offered** on a guessed *your turn*, on *his turn*, on *he is still sending*, under
  PSK31, at 16/500 or 32/1000, when the conversation is not on the calling center, and **a second
  time after it has been used**. Each of those is its own assertion.
- **The label is one click and says what it does**, quoted verbatim in the report.
- **PSK31's card is unchanged**, proved by `git diff` over `Ft8ContactCard`'s action members and by
  the PSK31 card and offer classes staying green and unedited.
- **Nothing is composed and nothing keys** by the offer merely appearing (§0.2): the transmit
  category is empty until the press.

**Drop candidate:** none. It is the first half of the step's last must-pass.

### Task 3 - one click sends the line, and then moves (4.5, second half)

Build to decisions BN, BO and BK, inside decision AZ's fence.

**Tests watched failing first:**

- **The line goes out at the old place and the old variant**: the calling center, 8/250, the record
  `announced: true` with code **69**, and **the audio actually handed to the sound card read back by
  `RsidDetector`** as `OLIVIA_8_250` at that center within 5 Hz - the detector never told which
  (decision I). **Quote the line's text.**
- **After the ordinary unkey, Hamlet has moved**: the next send to that station goes out at
  **calling center + 500 Hz** at **16/500**, with code **70** read back by the detector within 5 Hz,
  proved by sending one - not by reading a field.
- **A Stop mid-play, a cap refusal and a `no_announcement` refusal each move nothing**: center and
  variant unchanged after, asserted, and the card says in words that nothing moved.
- **The bound** (BK): with a calling offset that would put the moved-to signal outside the 200-3000
  Hz passband, the offer is not made and the card says why. Report the arithmetic on the real bands.
- **Telemetry** (R13): the move writes its own events - offered, sent, and the from and to centers
  and variants - **with no callsign and no text**. The privacy walk green.
- **`PttOn` 1 and `Arm(` 2**, counted after this task, and no line on AZ's forbidden list touched.

Then run decision BT's after-run of the chain-guarding list. **Give the seconds and compare every
red by name against task 0.**

**If sending the line needs a line on AZ's forbidden list, stop this task there**, commit nothing
that touches it, write it in section 4 as `MOVE: stop` material with the line and the reason, and go
to reporting.

**Drop candidate:** none.

### Task 4 - the card says what was heard where they moved to (4.5, last clause)

Build to decisions BP, BQ and BR.

**Tests watched failing first:**

- **Three states, each proved**: waiting inside the window; *an announcement arrived* when a 16/500
  burst is fed at the new center inside the window; *none arrived* after the window ends. **Quote
  all three sentences.**
- **The window is the product** `PatienceSeconds("16/500") × 2`, asserted as the product and never
  as a number of seconds.
- **A 16/500 burst a long way from the new center does not count**, and neither does an 8/250 burst
  at it.
- **The card survives the move** (BR) and is still the same station's card with its history, through
  his old channel retiring and his new one opening.
- **Nothing transmits on a detection** (§0.2), asserted.

**Drop candidate:** none. Without this clause 4.5 is not met.

### Task 5 - the turn indicator against the block (4.8)

Build to decision BS. **This is a measurement, and the number is the deliverable.**

- On an Olivia channel fed a message ending in a turnover word, **the lag from the last accepted
  character of that word to the card's turn reading changing**, in seconds and in blocks, at 8/250
  and at 16/500.
- **Met is one block or less.** If it is more, report the number, say why - the splitter closing a
  message on the following character is the known cause (unit 364 item 6) - and leave 4.8 not met.
  **Do not loosen the test and do not give the turn a clock.**

Run the carry-forward list, both invocations, at the end of this task whether or not it is built.

**Drop candidate: this whole task.** Drop it whole and say so plainly. 4.8 is the step's only
nice-to-pass left; with it dropped, step 4 is 7 of 8 with **every must-pass met**, and the trace's
item 6 measurement still stands in the report. **Do not drop it half-built, and do not drop it to
save time for anything except finishing 4.5.**

---

## 9. Parked - do not touch, do not raise

- **Step 5 entirely**: logging an Olivia contact, `MODE=OLIVIA` with its submode, the achievements,
  and the ledger that carries no mode (unit 364 item 7). **Even though this unit's QSO is the one
  that would be logged.**
- **R27's across-tab switch** (unit 364 item 11): an RSID heard under PSK31 or FT8 switches nothing.
  Decision BQ lifts nothing for it.
- **`Psk31ClearSpot`'s margin against Olivia's width** (unit 366 item 4): the finding stands, the
  rule is not rewritten, and **the move is not routed through the clear-spot rule** - R29 says up
  500 Hz, and up 500 Hz is what it is.
- **The compound-callsign parser** (unit 366 item 1): a line whose callsign carries a slash opens no
  card. It is PSK31's and it is not this unit's. **If it blocks a test, use a plain callsign and say
  so.**
- **The `variant` field on `TransmitRecord`** (unit 366 item 3): still forbidden by AZ. The variant
  travels on the application's own send lines and by the RSID code on the record.
- **The wider `Psk31`-named sweep** (unit 366 item 6): bounded to the move's own path by task 1
  item 7. Do not sweep the rest.
- **Unit 366's three changed sentences** (item 2): shipped, overrulable, carried to Tim. Do not
  revisit them and do not change another shared sentence without saying so in section 4.
- **The doubled abort pair** (unit 365 item 7, unit 366 item 7): report, do not chase, unless an
  Olivia move-send causes it.
- **`PHASE_PLAN.md`, `PHASE_OUTCOME.md`'s earlier entries, `tools\`, `.run-unit\` and
  `RUN_LEDGER.md`.** Reported, not edited.
- **Carried from earlier:** the FT8 unit's retry at the press and its starter card, `longestSeconds`
  on the typed line's record, the *60 s of text* rounding, the silence after the burst not being a
  field, codes 72 to 75 and the fldigi commit pin, the dial 1500 Hz below the center (step 0 is
  closed), the three off-list reds, the id-scheme split, HM-OPEN-090, and the screen phase's open
  asks. **Carried in section 4, not worked.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Nothing keys a real port.** Every send test runs on a fake wire and a fake sound card.
- **Do not touch any line on decision AZ's forbidden list** - `Ft8TransmitSequence`, `Ft8ArmedSend`,
  `UnslottedTransmission.Fit`/`Cap`/its announcement arithmetic, `OperatorSend`, `RsidBurst`, the
  abort, or the FT8/FT4/PSK31 branches of `SendMessage`. The answer to needing one is a stop, not an
  edit. **Do not add a second keying path or a second `Arm(` site for any reason.**
- **Do not send a frequency command to the radio** (BK, R11). The dial does not move.
- **Do not let the operator pick a variant**, on screen or in Settings (R27, BA). **The move offers
  16/500 because R29 says 16/500**, not because a control offers a choice.
- **Do not offer the move on a guess** (R1). A guessed *your turn* offers nothing.
- **Never give the demodulator or the detector a variant, a center or a start time** from the test
  or from the modulator's inputs (decision I).
- **No window, cap, patience or factor in literal seconds for Olivia** (§3.2). **Do not move
  PSK31's, FT8's or FT4's caps**, and do not change `timing.json`'s counts (decision BD).
- **Never loosen the 5 Hz read-back, the `PttOn` 1, the `Arm(` 2, a cap test, or 4.8's one block.**
  A miss ships with its number and the criterion is `partial` (§6).
- **Do not edit a fixture, `manifest.json` or `corpus.json`, and write no WAV under `assets\`.**
- **Do not edit an existing test to make a change pass.** If one guards a door this unit is told to
  open, §R12 applies: rewrite it in its own commit and name the assertions that changed.
- **Nothing this unit adds goes on the engine carry-forward line.**
- **Report mismatches. Repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Write each commit message into a file under
`.run-unit\` with the editor and pass it with `git commit -F`. Never commit `SESSION.lock`,
`.run-unit\`, `RUN_LEDGER.md`, `.u365-head.md` or `.u365-report.py`. The report names the branch and
whether every push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 5 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   to 3 done; step 4 was 6 of 8 met at the start of this unit and is N of
   8 after it, with 4.5 the step's last must-pass and 4.8 its last
   nice-to-pass; steps 5 and 6 not started. Say plainly whether step 4 is
   done, done on its must-passes with 4.8 dropped, or still partial.
B. Step 4's criteria this unit worked. 4.5: the offer appeared under all
   four conditions <yes|no> and was refused on N of N conditions it must
   not appear under; the line quoted, out at the calling center at 8/250,
   code read back OLIVIA_8_250 at N Hz error of 5; after the unkey the
   next send went out at N Hz at 16/500, code read back at N Hz error;
   a Stop, a cap refusal and a no-announcement refusal moved nothing
   <yes|no>; the follow line's three states proved N of 3 with the window
   asserted as PatienceSeconds(16/500) x 2 = N s; the card survived the
   move <yes|no, and what it took>. 4.8: lag from the turnover word to
   the turn reading N s / N blocks at 8/250 and N / N at 16/500, one
   block being N s - <met|not met|dropped>. Met: <list by id>. Not met:
   <list, with the number>. Entry: chain-guarding reds before N, after N,
   new N (0).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of 4.5 or 4.8 - in particular
   whether anything needed a line on decision AZ's forbidden list (stop
   material), whether any working mode's send or read guard went red
   after green (HM-DEC-165), whether the PttOn or Arm( count moved,
   whether any frequency command was sent to the radio (BK), and the
   engine and app carry-forward seconds against the 480 s timeout.
```

**Then the header:**

```
UNIT:       367 - <complete|stopped> at task N of 5, <task 5 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 4 criteria met 6 of 8 -> N of 8; must-passes left in step 4 1 -> N
DRIFT:      0
```

**Then a criterion table, 4.1 to 4.8**, with 4.1 to 4.4, 4.6 and 4.7 as met by units 365 and 366,
and this unit's numbers and state for **4.5** and **4.8**.

**Section 3 leads with the operator's evening, press by press, from the calling spot to the new
one.** He presses Olivia and calls CQ on the cited spot; a station answers certainly; **the card now
offers two things and the report quotes both labels**; he presses the move; the line goes out where
the station is listening; Hamlet moves; his next press goes out 500 Hz up at 16/500; and the card
says what was heard at the new place. Then give:

- the move line's text, verbatim, and its record: macro token, seconds, cap, offset, `announced`,
  `rsidCode`, mode and variant
- the detector's read-back of the move send and of the first send after it: variant sent, code read,
  center sent, center read, error in Hz
- the before-and-after table: center and variant before the press, after the unkey, after a Stop,
  after a refusal
- the follow line's three sentences, quoted, and the window as the product
- 4.8's lag table: variant, block seconds, lag in seconds, lag in blocks
- the chain-guarding and carry-forward runs before and after, by name for every red
- the `PttOn` and `Arm(` counts before and after

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004). In
particular, **no Olivia signal from Hamlet has been on the air, no QSO has been moved off a real
calling frequency, and none will be until Tim presses it at step 6.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: the move-off-and-widen offer on a certain answer - one click sends the line, moves up 500 Hz, switches to 16/500, and the card says whether the other station RSID followed
MOVE: continue
WHY: Step 4 is 6 of 8 with 4.5 the last must-pass and 4.8 the last nice-to-pass, and 4.5 is the one of the three beginner problems PHASE_PLAN section 1 names that is still unbuilt; step 5 cannot start until step 4 closes. The loop test finds no move, no QSY and no widen in any entry, and a search of src finds nothing in the tree that knows what a QSY is, so this is the first unit on it. Everything it needs - the modulator, the open gate, a send at a named center and variant, the RSID both ways, the certainty gate and the cited table - was built by units 365 and 366.
STATE: partial
DECIDED: author's, overrulable - (BK) move up 500 Hz moves the audio center inside the 200-3000 Hz listening passband and sends no frequency command to the radio, the VFO staying where the tab put it; the bound is that passband and not Psk31ClearSpot's 400-2200 calling window, and a move that would fall outside it is not offered and the card says why. (BL) the offer needs all four of Olivia chosen, a certain your turn over his certain answer, the conversation at the cited calling center, and 8/250; it is offered once per conversation and beside the macro offer, never on a guess. (BM) it is its own one-click control, with Ft8CardActionKind, Psk31Macro, ActionFor and CardActionAsync's Send and Log paths unchanged, so PSK31's card does not move. (BN) the line is a new Olivia macro carrying both callsigns, the amount and the variant, quoted verbatim in the report, and it goes out at the old place and the old variant with code 69 because that is where the other station is listening. (BO) nothing moves until the line has gone out and unkeyed ordinarily - a refusal, a cap failure or a Stop moves nothing and the card says so. (BP) the follow line has three states, waiting, an announcement arrived and none arrived; an announcement counts as a 16/500 code within about 250 Hz of the new center inside a window of PatienceSeconds(16/500) times 2, asserted as the product; RSID carries no callsign, so the card says what was heard and never that the station personally followed. (BQ) unit 359's decision B is lifted only so far as letting the follow check read an Olivia-tab detection; no detection sets mode, tab, dial or variant, and R27's across-tab switch stays parked. (BR) the card survives the move, because 4.5 is unshowable on a card that dies at the QSY; keeping it is part of the task, not a finding. (BS) 4.8 is a measurement of the lag in seconds and blocks from the turnover word to the turn reading changing, met at one block or less, with nothing invented and no clock added to the turn if it misses. (BT) the chain-guarding list runs before any change and after task 3, the carry-forward list before the first change and after the last, reds compared by name, PttOn 1 and Arm( 2 counted at the end. Decision AZ's forbidden list binds this unit unchanged. Task 5 (4.8) is the drop candidate; tasks 0 to 4 have none. Unit 366's section 4 asked for no ruling: its items 5 and 6 are taken as task 1 item 7 bounded to the move's path, item 4 is parked with the clear-spot rule unchanged, and the rest are logged. The letters start at BK because unit 366's ran to BJ.
LICENCE: PHASE_PLAN.md step 4 criteria 4.5 and 4.8 and its entry, R27, R28, R29, R31, R32, section 1, section 3.2, and section 6 (the transmit clause - an audio generator and an RSID prefix behind the one sequence; a package is needed; a later ruling wins; never loosen a test; a number, a label or a mechanism is the arbiter's; HM-DEC-165); PSK31 plan R1, R10, R11, R12, R13, R14; CLAUDE.md 0.0 and 0.2; HM-DEC-018, HM-DEC-139, HM-DEC-155, HM-DEC-165; ARBITER.md sections 2, 4 and 6
ACCOMPLISHED: A beginner who answers a call on the Olivia calling spot is not left sitting on it. When the other station has certainly come back, the card offers one thing more - move up 500 Hz and switch to 16/500 - and one click tells him so in his own mode at the place he is listening, then takes Hamlet up and makes it faster, and says what came back from the new place. The calling frequency is left clear for the next caller, the QSO goes on at twice the speed, and the operator never learned the etiquette or touched a variant control - with one keying path still, the dial where he left it, and nothing keyed at the bench.
ADVANCES: step 4 criterion 4.5 (tasks 2, 3 and 4), the last must-pass in the step, and 4.8 (task 5, nice-to-pass, the drop candidate). With 4.5 met step 4 has every must-pass met and step 5's entry opens; with 4.8 also met step 4 is 8 of 8 and done.
END-ARBITER-DECISION
```
