# Work instruction 366 - say it out loud: the Olivia send goes through the one sequence

**Step 4 of `PHASE_PLAN.md`, authored by the arbiter.** Unit 365 built Hamlet's own
`OliviaModulator` and proved it at the engine: thirty of thirty loopbacks identical, the signal
inside every tolerance against the mode author's audio, every composed send carrying its RSID burst,
the cap and the patience counted in the variant's characters. **4.1 is met. Nothing has gone out.**
`CanTransmitIn` still names FT8, FT4 and PSK31 and not Olivia, so every Olivia press - the CQ
button, Answer on a row, the card's button, the typed line - reaches the one door and is refused.

**This unit opens that door.** The modulator that exists is put behind the one unslotted sequence
that already keys FT8, FT4 and PSK31, and nothing else in the chain moves. **Six tasks, 0 to 5.
Task 5 is the drop candidate.**

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

The arbiter checked all four against the tree on 2026-09-19, and they held.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: **two invocations, one build each**, with a status write immediately
before each. Never background and poll.

**The engine invocation took 4 m 43 s for 146 tests** (unit 365), inside the 480 s timeout by
about three minutes. **That margin is this unit's to keep.** Report the seconds of both invocations
every time they run. **Nothing this unit adds goes on the engine line.** This unit's one addition to
the list is Olivia's send guard, and it goes on the **app** line (decision BH).

**HM-DEC-165 - a mode that works stays working.** The list carries a send guard and a read guard
for every working mode. **A red after that was green before is a regression.** Name it in sections 1
and 4. If one appears, the repair is this unit's next task before any new criterion. **This unit
changes the send path**, so no app red touching a send is presumed a flake: say what it touches.

**CPU is measured alone.** Every class this unit adds that asserts or reports CPU goes in the
non-parallel `CpuMeasuredAlone` collection.

## 2. The tool facts, as units 359 to 365 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing. Use the
  editor. A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  because warnings are errors.
- **`sed -i` on `output.md`, a `>` redirect anywhere under the root, and `cat >>` onto
  `docs\carry-forward-tests.txt` were refused** as *outside the allowed working directories*. Use
  the file editor for all three.
- **Python did not run for unit 365** - `python file.py` needed approval in three forms - though it
  ran for unit 362 (FT8) and unit 361. **Do not build a task on it.**
- `rm` is refused. A `for` loop over a variable was refused as *simple_expansion*. Do not depend
  on `;`.
- A `grep` pattern containing `\|` was read as several operations and refused; **`grep -E` with the
  same alternation ran.** `grep -o` with a quantifier and `grep -v` in a pipe need approval.
- **`tools\arbiter\validate-output.bat` could not be run by unit 365** - both `cmd /c` and the
  direct call needed approval. If it will not run, check the report's shape by hand against the
  rules the script prints, and say in section 4 that you did.
- These needed approval, which a headless session cannot give: `mkdir`, `cp`, `mv`, `tee`,
  `powershell.exe`, `jq`, `awk`, `git restore --source`, `git checkout <rev> -- <file>`,
  `git stash push`, `git config`, and command substitution. A command that includes one is refused
  whole. **Keep `Passed!|Failed!` in every filter** so the summary line survives.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. `sed -n N,Mp` and
  `tail -n +N | md5sum` in a pipe after `git show` ran.
- Anything outside `C:\Source\HamLet` cannot be listed or read.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). **`tools/status.sh` now writes
  `RULES_AT: HM-DEC-165 (2026-09-19)`** - unit 365 edited it - and reads `WORK_INSTRUCTION` from
  `PHASE_STATUS.md`, which says 365 until you set it.

## 3. Asks still outstanding

Carried per HM-DEC-139. **The last report is `output.md` at `HEAD` (`ab8392c0`), unit 365's.** Its
`## 4. What's blocking us` heading is at **line 264**. **Carry everything from line 265 to the end
verbatim** - its nine items and the queue it carries from the FT8 unit, which carries 364's, 363's
and the older ones. Recover the bytes whichever way the harness allows; unit 365 read them with
`git show ... | sed -n N,Mp` and wrote them back with the file editor, and that path works.

**Check the carried copy.** `git show ab8392c0:output.md | tail -n +265 | md5sum` gives
`fe891bea7e4e2f4a2dc91f1d491ce4ae`. The same `md5sum` over your carried block must give the same
hash. **Report both.**

**Mark nothing in place** except what section 9 says this instruction answers.

---

## 4. Why this unit exists

**This is unit 366, the sixth unit of step 4's pipeline and the ninth of the phase's receive half
before it.** Step 4 stands at **1 of 8 criteria whole** - 4.1 - with 4.2 and 4.4 half-built at the
engine and 4.3, 4.5, 4.6, 4.7 and 4.8 untouched. Every one of the five untouched criteria describes
something a send does, and no send exists. **One gate is between the modulator and all five.**

R28 says *identical to PSK31 above the modem*, and above the modem everything is built: the macros,
the frame, the receipt, the card, Answer, the arming, the sequence, Stop, the record, the power
offer and the ALC sentence. Below the modem the modulator now exists and is proved at both ends.
**What this unit does is join them at the one door and prove the join.**

Of the five untouched criteria this unit takes **4.3, 4.6 and 4.7**, finishes **4.2** and **4.4**,
and leaves **4.5** (move up 500 Hz and switch to 16/500) and **4.8** (the turn indicator within one
block) to the next unit - section 9. They are a new macro and a turn-timing measurement, and neither
is the gate.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own announcement and never
            picked by the operator.
UNIT GOAL:  Make the Olivia press go out. Open the one mode gate, put the modulator's
            audio and its RSID prefix through the same unslotted sequence that keys FT8,
            FT4 and PSK31, at the row's variant and center or at the calling spot for a
            CQ, with PSK31's receipt, PSK31's Stop, the cap and the patience scaled by the
            variant, and the power offer and the ALC as PSK31 has them - and FT8, FT4 and
            PSK31 byte-identical to before.
ADVANCES:   step 4 criteria 4.2 and 4.6 (task 2, must-pass), 4.3 (task 3, must-pass),
            4.4 (task 4, must-pass), 4.7 (task 5, must-pass, the drop candidate). Step 4
            stays partial after this unit whatever happens: 4.5 and 4.8 are not in it.
DRIFT:      0 - unit 365 met 4.1 and the engine halves of 4.2 and 4.4.
```

**Read `PHASE_PLAN.md` at the root in full.** Most of all: step 4 and its entry, R27, R28, R29,
R31, R32, §3.2 and §6's transmit clause.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 18:15 and the arbiter's own reading
after it:

- **HEAD is `ab8392c0`**, unit 365's report commit. Version **1.13.53** in `Directory.Build.props`.
- **The gate.** `CanTransmitIn` is at `MainWindowViewModel.cs:2446` and admits null, `FT8`, `FT4`
  and `PSK31`. It is called at **:15354** in `SendMessage` and by `CanAnswerRowsForTests` at
  **:4408**. `IsPsk31Chosen` is at :2453 and `OliviaLabel` at :2458.
- **The keying sites.** `CivConstants.PttOn` has **one** code line
  (`Ft8TransmitSequence.cs:513`). `_armedSend.Arm(` has **two** (`MainWindowViewModel.cs:15524`
  and **:15723**). The second is the unslotted one.
- **The unslotted send, as PSK31 uses it.** `SendMessage` composes at **:15680**
  (`Psk31Modulator.Compose(wanted, _transmitSampleRate, offsetHz, _settings.TransmitDrivePeak,
  typed ? LongestTypedSeconds : OperatorSend.LongestUnslottedSeconds)`), writes
  `Psk31Events.SendComposed` and `SendStage.Composed` with `UnslottedMode.Psk31.ToString()`
  (:15701), refuses with `no_transmit_path` if `_armedSend` is null, builds `OperatorSend.Now(...)`
  and arms at :15723, then books the receipt. The clear spot is chosen at :16226 by
  `Psk31ClearSpot.Choose`, and a crowded band refuses at :15660 with `no_clear_spot`.
- **The modulator.** `OliviaModulator.Compose(text, variant, centerHz, sampleRate, peak,
  OliviaSendKind kind = Macro)` returns an `UnslottedTransmission` with
  `UnslottedMode.Olivia`, the burst first, `AnnouncedCode` and `AnnouncementSamples` from the burst
  it placed, and `LongestSeconds` from `OliviaTiming.CapSeconds(variant, kind)`. **It throws** where
  the RSID codes cannot be read or carry no burst for the variant. `OliviaModulator.PhaseSeed` is
  365, so the same text makes the same audio.
- **The timing table.** `data\olivia\timing.json`: `cap_macro_characters` **121**,
  `cap_typed_characters` **242**, `patience_characters` **32**, `retire_after_characters` **56**,
  seconds per character 0.68267 / 0.512 / 0.4096. Read through `OliviaTiming`, which has
  `CapSeconds`, `PatienceSeconds` and `RetireWindowSeconds`.
- **The calling table** is `OliviaCallingTable`, `data/bands/olivia-calling.json`, with
  `CallingRowFor(band)` and `DialHzFor(row)`. The panel's sentences are at
  `MainWindowViewModel.cs:1428`, :1441 and :1454.
- **The power offer and the ALC.** `HasPsk31PowerOffer` (**:16435**) is `IsPsk31Chosen && !_psk31PowerSettled`.
  `Psk31AlcReference` (:16414), `HasPsk31AlcReference` (:16417), `Psk31AlcReferenceLine` (:16427),
  and the ALC sentence from :16592. **Every one of them is gated on PSK31 being chosen.** That is
  what 4.7 is about.
- **The tests that assert Olivia refuses.** `TheOliviaSeamTests` (`Assert.False(model.CanAnswerRowsForTests)`
  at :228 and :334; `send_refused` last at :383) and `TheOliviaRowsTests.WithRowsPresentEverySendControlRefusesAndNothingKeys`
  (:256, with at least four refusals at :335). **Both are on the app carry-forward line.** §R12
  governs them - see decision BI.
- **The lists.** `docs\carry-forward-tests.txt`: two invocations, the per-mode guard table at the
  top, and *Olivia send: none yet - added the unit the gate opens.* **That is this unit.**
  `docs\chain-guarding-tests.txt`: thirteen Transmit and Audio classes on the engine project.
- **Carry-forward at the end of unit 365:** engine **146 of 146** (4 m 43 s), app **187 of 188**
  (2 m 19 s). `PttOn` write lines **1**, `Arm(` call lines **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree.**

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload's disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-165 (2026-09-19) and
  `CLAUDE.md` §1 holds CPS-DEC-0165. That is the carried id-scheme split. **Do not repair it.**
- **`PHASE_OUTCOME.md` carries two step 4 entries whose decision letters clash** (unit 365 item 3).
  One, `## UNIT 1 - STEP 4` with `STATE_AFTER: in progress`, is an instruction that produced no
  report and was not what unit 365 built; the other is unit 365's own, letters AO to AY. **The live
  letters are unit 365's, and this instruction's new ones start at AZ so nothing collides.** The
  file is append-only: **do not edit either entry.**
- **`PHASE_PLAN.md` leaves 1.5 and 1.7 unchecked** though step 1 is done. Reported, not edited.
- **`PHASE_STATUS.md`** says `CURRENT_STEP: 4`, step 3 `done`, `WORK_INSTRUCTION: 365`. It is the
  launcher's file. Commit it as the launcher leaves it.
- R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`. The tree has
  `data/rsid/` and `data/bands/`.
- **Untracked in the root:** `.u365-head.md` and `.u365-report.py`, unit 365's scratch, left because
  `rm` is refused. **Do not commit them and do not build on them.**
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
  (the last is also on the chain-guarding list).
- **`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`**
  is on the app line and unit 365 item 7 found its red is **deterministic, not a flake**: the abort
  pair is written twice where the test expects one. It was red before this unit. Decision BF says
  what to do about it. Other known headless flakes:
  `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` (dispatcher loop)
  and `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`. Rerun a red that
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
Olivia tunes to the 8/250 spot for the band. After a certain answer on the calling frequency the
card offers one click - move up 500 Hz and switch to 16/500 - which sends the line saying so,
retunes, changes variant, and the other end's RSID confirms he followed.*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md R32 - Tim, 2026-09-18.** *(a) The RSID burst does not count against the send cap.
The cap measures the macro or the typed line; the announcement is a fixed 2.3 seconds in front of
it. (b) The transmission record says a send was announced - `announced: true` and the RSID code on
every keyboard-mode send. Both were the arbiter's to decide under §6 and it stopped instead; a cap
the plan calls the author's number, and a field on a record, are neither the radio's safety nor a
promise, and are never a stop.*

**PHASE_PLAN.md §3.2 - Speed.** *Every timing rule - the send cap, the turn indicator's patience,
the retire window - scales with the variant's seconds per character, taken from the mode author's
audio ... and never fixed in seconds.*

**PHASE_PLAN.md step 4, the criteria this unit works, verbatim:**
- *4.2 Every send begins with its RSID; the detector reads it back as the variant sent.*
- *4.3 The CQ goes out on the calling center from the cited table for the band on a clear spot; the
  receipt carries no station facts and no Log; a certain answer retires it.*
- *4.4 The cap and the turn indicator's patience are the timing table times stated factors, not
  fixed seconds; a Report at 8/250 is allowed its length.*
- *4.6 Nothing keys at the bench; one `PttOn` site; FT8, FT4 and PSK31 sends byte-identical to
  before except PSK31's new RSID prefix; Stop aborts an Olivia send.*
- *4.7 The power offer, the ALC reference and the ALC sentence work on Olivia sends as on PSK31.*

**PHASE_PLAN.md step 4 entry:** *step 3 done; the loopback chain of the PSK31 phase unchanged,
checked by its guarding tests first.*

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

**PSK31 plan §R10 - one click, one transmission, one keying path.**

**PSK31 plan §R11 - nothing at the radio.** Nothing this unit needs is asked of the operator at the
rig.

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was shut,
that later blocks the unit told to open the door, is the session's to rewrite in its own commit so
it guards the rule and not the shut door - and that is not a ruling, not an ask, and not a stop.*

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file alone,
proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others.*

**PSK31 plan §R15 - the ALC learned from FT8.** The Olivia sends inherit it (4.7); no new ALC rule
is invented.

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never transmits
on a decode.*

**FACT-004.** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio. **No Olivia signal from Hamlet has ever been on the air. The
first one will be Tim's.**

**HM-DEC-139**, **HM-DEC-155**, **HM-DEC-165**, as in sections 1 and 3.

**Standing decisions of earlier instructions, still in force:** D (the cap measures the samples
after the announcement, and the excusal is only the burst `RsidBurst` makes for that code at that
rate), F (the no-slot record carries `announced`, `rsidCode` and `announcementSeconds`), H (format
facts from `data\olivia\format.json` with `pj_mfsk.h` citations - transcription, never a port), I
(**the variant and the center come from the detector or the row, never from the test**), J (CER is
Levenshtein over the reference text's length), AF (one row path), AJ (the retire window is 56
characters of the variant), and unit 365's AP to AY, which built the modulator. **Decision AN is
lifted by this unit for the mode gate.** Every other door stays as it was.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings. They continue
unit 365's letters, so nothing collides with the stale entry in `PHASE_OUTCOME.md`.

- **AZ. One door opens, and nothing else in the chain moves.** Olivia joins `CanTransmitIn`
  (`MainWindowViewModel.cs:2446`). An Olivia send is an `UnslottedTransmission` from
  `OliviaModulator.Compose`, armed at the **existing** unslotted `Arm(` site (:15723) and played by
  the **existing** `Ft8TransmitSequence`. **No new `PttOn` site. No new `Arm(` line. No edit to
  `Ft8TransmitSequence`, `Ft8ArmedSend`, `UnslottedTransmission.Fit`/`Cap`/its announcement
  arithmetic, `OperatorSend`, `RsidBurst`, the abort, or the FT8/FT4/PSK31 branches of
  `SendMessage`.** Reaching the one existing sequence through the one gate is exactly *an audio
  generator and an RSID prefix behind the one sequence* (§6). **Anything wider is not, and needing
  one of those lines is `MOVE: stop`** - stop the task there, commit nothing that touches it, name
  the line and the reason in section 4, and go to reporting.
- **BA. Where a send goes, and it is never the operator's (R27, R28).** An Answer, Report, Confirm
  or typed line addressed to a row goes at **that row's variant and center**, the ones its RSID or
  the blind search gave it. A CQ, or any send with no row, goes at **8/250 on the tab's calling
  center** for the band from `OliviaCallingTable` - not a constant, not a literal. **No control
  anywhere offers a variant**, on screen or in Settings.
- **BB. 4.3's clear spot is PSK31's rule, unchanged.** The CQ's spot is chosen by
  `Psk31ClearSpot.Choose` as :16226 already chooses it, bounded by the **Olivia** listener's lowest
  and highest where it has them, and a crowded band refuses in words with `no_clear_spot`, as PSK31
  does. **The rule is not rewritten for Olivia and its numbers are not moved.** If Olivia's
  bandwidths make PSK31's clearance arithmetic give a spot that overlaps a heard Olivia carrier,
  **that is the finding**: report the numbers, ship the PSK31 rule, and say what the next unit would
  change. Do not invent a second clear-spot rule.
- **BC. 4.3's receipt is PSK31's receipt (R28).** The CQ receipt card is the one the PSK31 CQ books,
  with the mode chip reading Olivia. **It carries no station facts and no Log button**, and a
  certain answer retires it, by the same code and the same certainty gate. **If the receipt needs a
  branch on mode to say Olivia, that is a label, and it is the arbiter's: say Olivia.** Prove the
  "no station facts, no Log" half with the same assertions `TheCqReceiptTests` makes for PSK31.
- **BD. 4.4 at the application, and what the character count means.** The cap comes from
  `OliviaTiming.CapSeconds(variant, kind)` through `OliviaModulator.Compose`'s `kind`, and the app's
  only job is to hand it `Macro` or `TypedLine`. **No cap, patience or window in literal seconds for
  Olivia.** `OperatorSend.LongestUnslottedSeconds` (30), `LongestTypedSeconds` (60) and the FT8 and
  FT4 caps **do not move**, and must be asserted unmoved.
  - **Unit 365 item 1 is answered here and is not reopened.** The count is a **bound on the text**,
    not a promise that a text of that length fits: the air sends whole blocks plus a tail, so 121
    characters at 8/250 takes 84.46 s against an 82.60 s cap and is refused as `LongerThanTheCap`.
    **Refusal is the safe direction and the count stays as `timing.json` has it.** *Rejected:*
    re-deriving the count from whole blocks, which changes a rule already measured and written for
    a difference of one character. **What the unit must add is honesty, not arithmetic:** report
    **the longest text that actually fits at each variant**, measured, for macro and for typed line.
  - **Unit 365 item 6 is answered here.** A typed line at 8/250 may key for **165 s**, which follows
    from R32 and PSK31's own 60 s counted in characters. R32 says a cap is neither the radio's
    safety nor a promise, so it is not a stop and the count is not lowered. **What is required is
    that the operator sees it before he presses:** the typed line's card states the send's seconds
    for the Olivia variant it would go at, as it states them for PSK31. Report the sentence
    verbatim at 8/250.
  - ***A Report at 8/250 is allowed its length*** is proved on the longest Report the app composes -
    the compound-callsign one - **armed and played**, not merely measured.
- **BE. 4.4's patience.** Where the PSK31 turn indicator consults a time, the Olivia turn indicator
  consults `OliviaTiming.PatienceSeconds(variant)`. Unit 365 found the only patience the tree states
  is `Psk31Macros.AnswerSeconds` (7.84 s for the test calls) and that **nothing outside
  `TheCardWaitsOnHimTests` calls it**. **If the trace confirms no application path consumes a
  time-based patience, 4.4's patience half is reported met at the timing table with the place the
  turn is actually decided named, and nothing is invented to have something to scale** (R14). Say
  which it was.
- **BF. 4.6 and Stop.** Nothing keys a real port: every send test runs on a fake wire and a fake
  sound card. `PttOn` code lines stay **1** and `Arm(` call lines stay **2**, counted and reported
  at the end. **Stop aborts an Olivia send by the path it aborts a PSK31 send** - no new abort, no
  new route - proved mid-play on an 8/250 send long enough to be still playing. `TheFt8AndFt4SendsAreByteIdenticalTests`
  and the PSK31 send guards must be green and unedited. **On the doubled abort pair (unit 365 item
  7):** it is red on a tree this unit did not change and it is in the safe direction. **Report it;
  do not chase it** - unless an Olivia send doubles an abort for a reason of this unit's own, which
  is a regression and is this unit's next task.
- **BG. 4.7 - the power offer and the ALC.** `HasPsk31PowerOffer`, `Psk31AlcReference`,
  `Psk31AlcReferenceLine` and the ALC sentence are gated on PSK31 being the chosen mode. **Extend
  the gate to Olivia; do not copy the code and do not fork the sentence** (AF's spirit: one path).
  The learned reference keeps its `Mode` field, so a reference learned on FT8 stays labeled FT8 in
  the sentence. **Half power is offered, nothing is required of the operator, and nothing is set at
  the radio** (R11, HM-DEC-084). If renaming a `Psk31`-prefixed property is what it takes, rename it
  and say so; a rename is not a new door. **No new keying path may appear**, which the counts in BF
  prove.
- **BH. Telemetry and Olivia's send guard (R13, HM-DEC-165).** The PSK31 send-stage events and the
  no-slot record carry `mode` Olivia and the `variant`, with **no text and no callsign**; the
  privacy walk counts every new event. Add **one** app class, `TheOliviaSendTests`, holding
  `AnOliviaCqReachesTheAir`: an Olivia CQ passes `composed`, `armed`, `keyed` and
  `handed_to_the_sound_card`, unkeys ordinarily, and writes one record `announced: true` with code
  69. **That one method goes on the app carry-forward line and into the guard table as *Olivia
  send*.** It is the only change the table takes.
- **BI. The refusal tests are rewritten under §R12, each in its own commit.**
  `TheOliviaSeamTests`' 0.5 send half (:228, :334, :383) and
  `TheOliviaRowsTests.WithRowsPresentEverySendControlRefusesAndNothingKeys` (:256) assert the shut
  door. **Rewrite each to guard what still holds**: no second keying path, `PttOn` 1, `Arm(` 2, no
  other mode's decoder running under Olivia, and nothing composed before a press. **Say which
  assertions changed and why.** Do not delete a class to make a red go away.
- **BJ. The entry and the chain.** Step 4's entry is *the loopback chain of the PSK31 phase
  unchanged, checked by its guarding tests first.* Run `docs\chain-guarding-tests.txt`'s thirteen
  classes by exact name, foreground, **before any change and again after task 2**, and compare the
  reds by name. **A red after that was green before is a regression** and the rule in section 1
  applies.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- **Step 4's entry, first:** decision BJ's before-run of the chain-guarding list, reds by name and
  seconds. Then `ThePsk31CqGoesOutTests` and `TheFt8AndFt4SendsAreByteIdenticalTests` by name. **If
  a class that guards the PSK31 loopback chain is red, stop and report it** - step 4's entry is not
  open.
- Hash all nine Olivia fixtures against `manifest.json`, 9 of 9.
- Report what `PHASE_STATUS.md` and `PHASE_PLAN.md` actually say about step 4, and the two clashing
  step 4 entries in `PHASE_OUTCOME.md` (section 5).
- Append `UNIT 366 - STEP 4` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 365`
  entry. **Touch no earlier entry.**
- Patch-bump the version by one, from 1.13.53.
- Run the carry-forward list, **both invocations**, before any change, with the seconds and the
  per-mode guards' states.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md`, `.u365-head.md`, `.u365-report.py` or anything under
  `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2 writes
a line. Put it in `Unit366Trace`, a class that asserts nothing, in the shape of `Unit365Trace`, not
on the carry-forward line.

1. **The PSK31 press, from click to sound card, with file and line**, for each of: the CQ button,
   Answer on a row, the card's Report, the card's Confirm, and the typed line. Say where each
   chooses its offset, where each composes, where the mode gate refuses Olivia today, which `Arm(`
   site each reaches, and what happens between `composed` and `armed`. **Name every line decision AZ
   would have to touch to make the same press work in Olivia. If any is on AZ's forbidden list, say
   so here, before building anything.**
2. **What an Olivia row hands a send**: where the row's variant and center live after unit 364's row
   path, and whether they reach `SendMessage`. Name the property and the line.
3. **The transmit sample rate** the app composes at, and what `OliviaModulator` and `RsidDetector`
   expect at that rate.
4. **The CQ's spot.** How `Psk31ClearSpot.Choose` is called at :16226, what bounds it takes from a
   listener, and what it would give on the Olivia tab with the two-signal fixture's carriers
   present. **Report the numbers** - this is decision BB's finding if the clearance is too narrow
   for a 250 Hz signal.
5. **The receipt.** Where the CQ receipt is booked, what it shows, what retires it, and what in it
   is PSK31-specific in name or in wording.
6. **The turn indicator.** Where a turn is decided for PSK31, and whether any application path
   consumes a time-based patience (decision BE). Name the file and line, or say plainly that none
   does.
7. **The power offer and the ALC**: every member gated on `IsPsk31Chosen`, by name and line, and
   what 4.7 would have to widen.
8. **The longest Report the app composes** for a compound callsign: its character count, its seconds
   at each Olivia variant, and whether it fits each variant's cap. And **the longest text that
   actually fits** at each variant for a macro and for a typed line (decision BD).

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the gate, and the send through the one sequence (4.2 whole, 4.6)

Build to decisions AZ, BA, BF, BH and BI. **First rewrite the two refusal test classes under §R12,
each in its own commit**, then open the door.

**Tests watched failing first**, in `TheOliviaSendTests` (app):

- **BH's guard**, `AnOliviaCqReachesTheAir`, stage by stage to the sound card, with its record.
- **4.2 whole**: an Olivia CQ, an Answer to an 8/250 row and a Report to a 16/500 row. The rows come
  from the two-signal fixture through the app's own feed, as `TheOliviaRowsTests` makes them. **The
  audio actually handed to the sound card is read by `RsidDetector`, and it reads the variant that
  was sent, at the center that was sent, within 5 Hz** - the detector is never told which
  (decision I). The record says `announced: true` with that code.
- **Decision BA**: the Answer goes at the row's variant and center; the CQ goes at 8/250 on the
  calling center for the band from the table. **No control offers a variant.**
- **Decision AS, still standing**: with the RSID codes unreadable, an Olivia press is refused in
  words and keys nothing - never sent unannounced.
- **4.6**: nothing keys a real port. `PttOn` code lines **1**, `Arm(` call lines **2**, counted at
  the end. **Stop mid-play of an 8/250 send** aborts by PSK31's path, with an ordinary unkey and the
  abort's record. `TheFt8AndFt4SendsAreByteIdenticalTests` green and unedited; the PSK31 send guards
  green.
- **Decision BH's telemetry**: the stage events and the record carry Olivia and the variant, no text
  and no callsign. The privacy walk green.

Add `TheOliviaSendTests.AnOliviaCqReachesTheAir` to the app line and to the guard table in this
task's commit. Then run decision BJ's after-run of the chain-guarding list and the carry-forward
list, both invocations. **Give the seconds and compare every red by name against task 0.**

**If building the send needs a line on AZ's forbidden list, stop this task there**, commit nothing
that touches it, write it in section 4 as `MOVE: stop` material with the line and the reason, and go
to reporting. Do not do tasks 3, 4 or 5.

**Drop candidate:** none. It is 4.2 and 4.6, both must-pass, and HM-DEC-165's guard.

### Task 3 - the CQ at the calling spot, and its receipt (4.3)

Build to decisions BA, BB and BC.

**Tests watched failing first:**

- The CQ's audio center is the band's Olivia calling center **read from the table**, proved on at
  least two bands, and it is not a literal anywhere.
- **A clear spot**: with a carrier sitting on the calling center, the press finds a spot by
  `Psk31ClearSpot`'s rule or refuses in words with `no_clear_spot`, and keys nothing when it
  refuses. Report which happened and the numbers.
- **The receipt** carries no station facts and no Log, asserted as `TheCqReceiptTests` asserts it
  for PSK31, with the mode reading Olivia.
- **A certain answer retires it**, through the same certainty gate, proved on a row that answers the
  CQ.

**Drop candidate:** none. 4.3 is must-pass and the CQ is the first thing Tim will press at step 6.

### Task 4 - the cap and the patience at the press (4.4 whole)

Build to decisions BD and BE.

**Tests watched failing first:**

- For each variant, **the cap the send is held to is `timing.json`'s count times that variant's
  seconds per character**, read and asserted as that product, never as a literal - for a macro and
  for a typed line.
- **The longest compound-callsign Report at 8/250 is armed and plays.**
- **A text one character past what fits at 8/250 is refused** as `LongerThanTheCap`, in the same
  words PSK31's refusal uses, and keys nothing.
- **PSK31's 30 s, the typed line's 60 s, and the FT8 and FT4 caps are unchanged**, each asserted.
- **The typed line's card states the seconds** for the Olivia variant it would go at. Quote the
  sentence at 8/250 in the report.
- **The patience** scaled by `OliviaTiming.PatienceSeconds`, or reported per decision BE with the
  place the turn is decided named.

**Drop candidate:** none. 4.4 is must-pass and the engine half is already built; this is the wiring.

### Task 5 - the power offer and the ALC on an Olivia send (4.7)

Build to decision BG.

**Tests watched failing first:**

- With Olivia chosen, the power offer appears and offers half, exactly as it does with PSK31, and
  settles the same way.
- The ALC reference is read and the ALC sentence is produced for an Olivia send, naming the mode the
  reference was learned on.
- **`PttOn` 1 and `Arm(` 2 still**, counted after this task.

Run the carry-forward list, both invocations, at the end of this task whether or not it is built.

**Drop candidate: this whole task.** Drop it whole and say so plainly; 4.7 then stays *not met* and
is the next unit's beside 4.5 and 4.8. An Olivia send without it still goes out, keys once and
stops - the operator simply is not offered the power he is offered on PSK31. **Do not drop it
half-built.**

---

## 9. Parked - do not touch, do not raise

- **4.5** (the card's *move up 500 Hz and switch to 16/500*, the line, the retune, the variant
  change and the other end's RSID confirming) and **4.8** (the turn indicator within one block of
  the turnover word). **The next unit's.** Do not build a partial 4.5 while wiring the card.
- **Step 5** entirely: logging an Olivia contact, `MODE=OLIVIA` with its submode, the achievements,
  and the ledger that carries no mode (unit 364 item 7).
- **R27's across-tab switch** (unit 364 item 11): an RSID heard under PSK31 or FT8 switches nothing.
  Logged for the plan's author, not built.
- **Unit 365's section 4.** Items 1 and 6 are answered by decision BD and are not reopened. Item 2
  is answered by decision BE. Item 3 is answered in section 5 - the letters. Item 7 is decision BF.
  Items 4, 5, 8 and 9 are findings: **4** (`PHASE_PLAN.md`'s unchecked 1.5 and 1.7) reported not
  edited; **5** (the silence after the burst is not a field in `rsid-codes.json`) logged - `RsidBurst`
  is still not this unit's to edit; **8** is answered by decision BH; **9** is in section 2.
- **The FT8 unit's section 4.** Item 1, the retry at the press, is Tim's own commissioned repair
  offered to him to overrule; carried, not reopened. Item 3, the starter card booked before the arm
  check, is a ruling on work instruction 309's design; carried, not changed, **even though Olivia
  sends now pass through the same booking** - report that they do.
- **Unit 364's section 4.** Item 2's retire factor of 56 stands. Item 6 bears on 4.8. Items 8, 9 and
  10 (the strength dash, the wandering center, two RSID detectors) are logged. The rest are for the
  record.
- **Carried from earlier:** `longestSeconds` on the typed line's record, the refusal sentence quoting
  the whole audio, the *60 s of text* rounding, `RsidDetection` giving the first tone, codes 72 to
  75 and the fldigi commit pin, the dial 1500 Hz below the center (step 0 is closed), the three
  off-list reds, HM-OPEN-090, the id-scheme split, and the screen phase's open asks. **Carried in
  section 4, not worked.**
- **`PHASE_PLAN.md`, `PHASE_OUTCOME.md`'s earlier entries, `tools\`, `.run-unit\` and
  `RUN_LEDGER.md`.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Nothing keys a real port.** Every send test runs on a fake wire and a fake sound card.
- **Do not touch any line on decision AZ's forbidden list.** The answer to needing one is a stop,
  not an edit. **Do not add a second keying path or a second `Arm(` site for any reason.**
- **Do not port `pj_mfsk.h`** and do not add a package (§6).
- **Never give the demodulator or the detector a variant, a center or a start time** from the test
  or from the modulator's inputs (decision I). They find what was sent.
- **No cap, patience or window in literal seconds for Olivia** (§3.2). **Do not move PSK31's, FT8's
  or FT4's caps**, and do not change `timing.json`'s counts (decision BD).
- **Do not let the operator pick a variant**, on screen or in Settings (R27, BA).
- **Never loosen the 5 Hz read-back, the `PttOn` 1, the `Arm(` 2, or a cap test.** A miss ships with
  its number and the criterion is `partial` (§6).
- **Do not edit a fixture, `manifest.json` or `corpus.json`, and write no WAV under `assets\`.**
- **Do not edit an existing test to make a change pass.** The only exception is decision BI's two
  refusal classes, each in its own commit, named in the report.
- **Nothing this unit adds goes on the engine carry-forward line.**
- **Report mismatches. Repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Never commit `SESSION.lock`, `.run-unit\`,
`RUN_LEDGER.md`, `.u365-head.md` or `.u365-report.py`. The report names the branch and whether every
push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 5 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   to 3 done and closed; step 4 was 1 of 8 met at the start of this unit
   (4.1, with 4.2 and 4.4 at the engine only) and is N of 8 after it;
   steps 5 and 6 not started. Step 4 cannot be done on this unit alone -
   4.5 and 4.8 are the next unit's.
B. Step 4's criteria this unit worked. 4.2: sends read back by the
   detector as the variant sent N of N, within N Hz, records announced
   N of N. 4.6: PttOn N (1), Arm( N (2), real ports keyed N (0), Stop
   aborted the 8/250 send mid-play <yes|no>, FT8/FT4 byte-identical
   <green|red>, PSK31 send guards <green|red>. 4.3: CQ center from the
   table on N bands, clear spot <found|refused> with the numbers,
   receipt station facts N (0) and Log <absent|present>, a certain
   answer retired it <yes|no>. 4.4: caps N / N / N s as the product,
   longest 8/250 Report <armed and played|refused>, longest text that
   actually fits per variant N / N / N, patience <scaled|nothing to
   scale, and where the turn is decided>. 4.7: power offer under Olivia
   <yes|no|dropped>, ALC sentence <yes|no|dropped>. Met: <list by id>.
   Not met: <list, with the number>. Entry: chain-guarding reds before
   N, after N, new N (0).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of a step 4 criterion - in
   particular whether the send needed a line on decision AZ's forbidden
   list (stop material), whether any working mode's send or read guard
   went red after green (HM-DEC-165), whether the PttOn or Arm( count
   moved, and the engine and app carry-forward seconds against the 480 s
   timeout.
```

**Then the header:**

```
UNIT:       366 - <complete|stopped> at task N of 5, <task 5 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 4 criteria met 1 of 8 -> N of 8; modes that reach the air 3 -> N
DRIFT:      0
```

**Then a criterion table, 4.1 to 4.8**, with this unit's numbers and state (*met*, *not met*,
*partial*) for 4.2, 4.3, 4.4, 4.6 and 4.7, 4.1 as met by unit 365, and 4.5 and 4.8 as *not worked -
next unit*.

**Section 3 leads with what the operator would see, press by press.** The Olivia tab with the two
rows; the CQ button pressed, and every stage from the press to the unkey with what the screen says
and what the record says; Answer pressed on the 8/250 row; the card's Report on the 16/500 row; a
typed line; and Stop pressed mid-play. Then give:

- the detector's read-back of each send's burst: variant sent, code read, center sent, center read,
  error in Hz
- the CQ's spot: the band, the table's center, the spot chosen, and what was in the way
- the cap table: variant, seconds per character, count, cap in seconds, the longest text that
  actually fits, and the longest keying with the burst included
- the receipt, quoted, and what retired it
- the chain-guarding and carry-forward runs before and after, by name for every red
- the `PttOn` and `Arm(` counts before and after

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004). In
particular, **no Olivia signal from Hamlet has been on the air, and none will be until Tim presses
it at step 6.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: open the mode gate so an Olivia send goes out through the one unslotted sequence with its RSID prefix - the CQ at the cited calling spot with PSK31's receipt, a row's send at the row's variant and center, the cap and the patience at the press, Stop and the power offer as PSK31 has them
MOVE: continue
WHY: Step 4 is 1 of 8 with 4.1 met and the modulator proved at both ends, and the five untouched criteria all describe something a send does while CanTransmitIn still refuses Olivia, so one gate is between the built modulator and all of them. The loop test finds this approach in no entry; the resembling UNIT 1 - STEP 4 entry produced no report and unit 365's report says its instruction was not what ran, so that approach is untested, not tried (ARBITER.md section 8). The only transmit change is routing an existing audio generator and its RSID prefix behind the one existing sequence, which section 6 licenses, fenced by a stop on anything wider.
STATE: partial
DECIDED: author's, overrulable - (AZ) only CanTransmitIn opens; the send is armed at the existing unslotted Arm site and played by the existing Ft8TransmitSequence, with no new PttOn or Arm line and no edit to the sequence, Ft8ArmedSend, Fit, Cap, OperatorSend, RsidBurst, the abort or the other modes' branches - needing one is MOVE: stop. (BA) a send to a row goes at that row's variant and center, a CQ at 8/250 on the band's calling center from the cited table, and no control offers a variant. (BB) 4.3's clear spot is Psk31ClearSpot's rule unchanged, bounded by the Olivia listener; a clearance too narrow for a 250 Hz signal is the finding, not a second rule. (BC) the receipt is PSK31's, the mode chip says Olivia, no station facts and no Log, a certain answer retires it. (BD) unit 365 items 1 and 6 answered - the character counts in timing.json stay as measured, the count is a bound on text and not a promise that a text of that length fits, refusal is the safe direction, and what the unit adds is the measured longest text that fits per variant plus the card stating a typed line's seconds before the press. (BE) the Olivia turn indicator uses OliviaTiming.PatienceSeconds where PSK31's turn consults a time; if no application path consumes one, 4.4's patience half is reported met at the timing table with the place the turn is decided named, and nothing is invented. (BF) Stop aborts by PSK31's path, PttOn stays 1 and Arm( stays 2, and unit 365 item 7's doubled abort pair is reported and not chased unless an Olivia send causes it. (BG) the power offer and the ALC gate widens to Olivia rather than being copied or forked. (BH) the PSK31 send events and record carry Olivia and the variant with no text or callsign, and TheOliviaSendTests.AnOliviaCqReachesTheAir is Olivia's send guard on the app line. (BI) the two Olivia refusal classes are rewritten under R12, each in its own commit. (BJ) the chain-guarding list runs before any change and after task 2, reds compared by name. Decision AN is lifted for the mode gate. Task 5 (4.7) is the drop candidate; tasks 0 to 4 have none. The letters start at AZ because PHASE_OUTCOME.md carries two step 4 entries with clashing AO-AY sets; unit 365's are the live ones and neither entry is edited.
LICENCE: PHASE_PLAN.md step 4 criteria 4.2, 4.3, 4.4, 4.6 and 4.7 and its entry, R27, R28, R29, R31, R32, section 3.2, and section 6 (the transmit clause - an audio generator and an RSID prefix behind the one sequence; a package is needed; a later ruling wins; never loosen a test; a number, a label or a mechanism is the arbiter's; HM-DEC-165); PSK31 plan R10, R11, R12, R13, R14, R15; CLAUDE.md 0.2; HM-DEC-084, HM-DEC-139, HM-DEC-155, HM-DEC-165; ARBITER.md sections 2, 6 and 8
ACCOMPLISHED: Hamlet answers in Olivia. The operator presses CQ on the Olivia tab and a signal goes out on the band's own Olivia calling spot, announced by its variant so the other end's software follows; he presses Answer on a station's row and Hamlet replies at that station's variant and place without ever being asked which; the receipt behaves as PSK31's, the slow variants are allowed the length they need, Stop stops it, and the power offer and the ALC read as they do on PSK31 - with one keying path still, nothing keyed at the bench, and FT8, FT4 and PSK31 exactly as they were.
ADVANCES: step 4 criteria 4.2 and 4.6 (task 2), 4.3 (task 3), 4.4 (task 4) and 4.7 (task 5, the drop candidate), all must-pass; step 4 stays partial, with 4.5 and 4.8 left to the next unit
END-ARBITER-DECISION
```
