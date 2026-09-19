# Work instruction 362 - the blind variant search: a carrier that never announced itself, found and read

**Step 2 of `PHASE_PLAN.md`, authored by the arbiter.** Steps 0 and 1 are done and closed. Step
2 stands at **five of its seven criteria met** - 2.1, 2.2, 2.4, 2.5 and 2.6, checked in the plan
on 2026-09-19 - and unit 361 built the demodulator they stand on. **Two are left: 2.3, the blind
variant search, which is the last must-pass between this phase and step 3's entry, and 2.7, the
drift fixture, which is nice-to-pass.** This unit takes both. **Five tasks, 0 to 4; task 4 is the
drop candidate.**

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
way its top comment says: two invocations, one build each, with a status write immediately
before each. Never background and poll.

**This unit's tests are slow, and that is a number, not a reason to background them.** Unit 361
measured the RSID detector at about ten seconds of CPU on the 131-second files and the
demodulator at about four, and the engine invocation grew by about a minute. **The blind search
will trial-decode a 172-second file more than once.** Report the seconds; do not hide them
behind a background run, and do not cut a fixture short to make a number look better.

## 2. The tool facts, as units 359, 360 and 361 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- **A `sed` substitution with backslashes in the pattern matched nothing and reported nothing**
  (unit 361). It fails silently. Use the editor.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`), since
  warnings are errors. Anchor on the comment's first line.
- `;` is refused. `rm` is refused. A `for` loop over `$f` is refused (*simple_expansion*).
- **Python scripts written to the scratchpad and run as `python file.py` ran** (unit 361, against
  unit 360's report that Python did not run). `python -c` needed approval and was not tried.
- `mkdir`, `cp`, `powershell.exe`, `jq`, `awk`, `git restore --source`,
  `git checkout <rev> -- <file>` and `git stash push` needed approval, which a headless session
  cannot give. Write files with the editor.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. Run directly
  (not through `sh`) inside an `&&` chain, it needed approval.
- `git show ... > file` is blocked. `sed -n` in a pipe after `git show` ran;
  `tail -n +N file | md5sum` ran.
- Anything outside `C:\Source\HamLet` - including `C:\Source\fldigi` - cannot be listed or read.
  Jalocha's headers are inside the root at `assets\reference\jalocha\`.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `WORKING` and `claude` are
  not allowed words. `tools/status.sh` writes `RULES_AT: HM-DEC-161` and reads
  `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 361's `## 4. What's blocking us` verbatim, from its first
line to its end**, its nested queues and the reference to `4c55deac:output.md` included, the way
unit 361 carried unit 360's.

**`output.md` is not in the working tree.** Commit `483a5af3` deleted it when the plan was
revised. Unit 361's report is at **`6d9cf1e6:output.md`** - read the committed copy.

**Mark one item in place, and delete nothing:**
- unit 361 item 1 (the -16 dB fixture is not read at CER 0.10):
  *ANSWERED by `PHASE_PLAN.md` §8, the revision of 2026-09-19 - the -16 dB ceiling was the plan
  author's own error, set below the mode's published sensitivity for 16/500, and is withdrawn;
  2.2 now asks for the number measured and reported with no ceiling, which unit 361 measured at
  0.9203, and 2.2 is checked met. Work instruction 362 task 2 makes the test say what the
  criterion now says.*

This unit answers none of the others.

---

## 4. Why this unit exists

**Step 2 is five of seven. Criterion 2.3 is the only must-pass left in it, and step 3's entry is
shut until it is met.** Hamlet can read an Olivia signal that announced itself; it cannot yet
find one that did not. R27 puts the blind search in the plan for exactly that operator: a carrier
sitting on the waterfall with no RSID in front of it, which today Hamlet hears as nothing.

Unit 361 proved the hard half by accident: handed the manifest's variant and center, its
demodulator read `olivia-8-250-qso-norsid.wav` at **CER 0.0000**. **So what is unbuilt is the
finding, not the reading** - measuring the center, the tone spacing and the occupied bandwidth off
the audio alone, choosing the variant from them, and refusing to choose anything when there is only
noise.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal itself and never picked by
            the operator, which for an unannounced carrier means measured off the air.
UNIT GOAL:  A blind variant search that finds the no-RSID 8/250 carrier in the mode
            author's audio - center and variant from the audio alone, within a stated
            time - hands it to unit 361's demodulator for a decode at CER 0.05 or under,
            and finds nothing at all in pure noise; and a carrier drifting 20 Hz a minute
            that the demodulator holds.
ADVANCES:   step 2 criterion 2.3 (task 3, must-pass) and 2.7 (task 4, nice-to-pass);
            2.3 is the last must-pass shut between step 2 and step 3's entry.
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full**, step 2 and §3.3 above all, and read §8's revision
record: 2.2 was corrected on 2026-09-19 and that correction is task 2's whole reason.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 10:53 and the arbiter's own reading
after it:

- **HEAD is `483a5af3`** (`phase: olivia plan rev3 - 2.2 corrected, 3.0 added`). Version
  **1.13.48** in `Directory.Build.props`.
- **`output.md` is absent from the working tree**, deleted by `483a5af3`. Unit 361's report is
  `6d9cf1e6:output.md`.
- **`PHASE_OUTCOME.md` ends with the `UNIT 361 - STEP 2` entry written before that unit ran.** It
  carries no `FATE` and no `STATE_AFTER`; the launcher may append one. Append-only either way.
- **The fixtures** are nine files under `assets\fixtures\olivia\` with `manifest.json`. This
  unit's are `olivia-8-250-qso-norsid.wav` (172.06 s, variant `8/250`, `center_hz` 1000,
  `rsid: false`, *the blind-search case*, 251 characters of text), `olivia-noise-only-30s.wav`
  (30 s, variant and center `null`), and for task 4 the clean `olivia-16-500-qso-rsid.wav`
  (131.38 s) and `olivia-8-250-cq-rsid.wav` (28.98 s).
- **`src\Hamlet.RadioEngine\Olivia\`** holds `OliviaCallingTable.cs`, `OliviaData.cs`,
  `OliviaDemodulator.cs`, `OliviaFormat.cs` and `OliviaTiming.cs`.
- **`OliviaDemodulator`** takes `(OliviaFormat, OliviaVariant, double centerHz, int sampleRate,
  ITelemetry?, double threshold = SyncThreshold)` and `Decode(MonoAudio, double startSeconds)`
  returns `OliviaDecoding(Text, BlocksDecoded, BlocksRejected, CharactersOut, FrequencyOffsetHz,
  SymbolPhase, BlockPhase, SyncSnr, FirstBlockSeconds, LastBlockSeconds, BlockSnrs,
  LastBlockCharacters)`. Its constants: `FramesPerSymbol` 8, `PaddingFactor` 4, `Passes` 3,
  `SyncThreshold` 4.0.
- **`OliviaFormat`** exposes `Variants` - seven rows - and `Variant(name)`; `OliviaVariant` is
  `(Name, Tones, BandwidthHz, BitsPerSymbol, ToneSpacingHz, SymbolSeconds, FirstToneOffsetHz)`.
  `data\olivia\format.json` and `data\olivia\timing.json` ship embedded, and `timing.json` now
  says `source: measured`.
- **The tests** are in `tests\Hamlet.RadioEngine.Tests\Olivia\`: `OliviaFixtures.cs` (with
  `Load`, `CharacterErrorRate`, `Distance`, `Root`), `TheOliviaDataTests.cs`,
  `TheOliviaDemodulatorTests.cs`, `Unit360Trace.cs`, `Unit361Trace.cs`.
- **Carry-forward at the end of unit 361:** engine 119 of 119, app 166 of 166, first runs. The
  engine line carries `TheOliviaDemodulatorTests` **by type and method, four of its five names**.
- `CivConstants.PttOn` code lines: **1** (`Ft8TransmitSequence.cs:513`; two further mentions are
  in comments). `_armedSend.Arm(` lines: **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree**, so
  §6's *real off-air audio appears* branch does not fire.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- **`TheOliviaDemodulatorTests.TheMinusSixteenDecibelFixtureDecodes` is red** (CER 0.9203 against
  the 0.10 it asserts) and is off the carry-forward list for that reason. **Task 2 is about it.**
- The reload's one disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-161 (2026-09-11);
  `CLAUDE.md` §1 holds CPS-DEC-0164.
- **`PHASE_STATUS.md` says step 2 `not started` and `WORK_INSTRUCTION: 358`**, while
  `PHASE_PLAN.md` checks five of step 2's criteria. It is the launcher's file. Report it; commit
  it as the launcher leaves it; **do not edit `PHASE_PLAN.md`.**
- `PHASE_PLAN.md` step 1 shows 1.5 and 1.7 unchecked though step 1 is `done` in the record.
- `PHASE_OUTCOME.md`'s harness entries are headed `UNIT 1` and `UNIT 2`, and `UNIT 2 - STEP 1`
  reads `FATE: executed` for a run that never happened.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`; the
  tree has `data/rsid/` and `data/bands/`.
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- The app carry-forward list is flaky run to run. Rerun a red that passes alone up to three
  times, and say which run the number came from. **This unit changes no app code, so an app red
  is a flake or older than this unit** - say which.
- Uncommitted at authoring: `.run-unit\reload.txt`, `PHASE_STATUS.md`, and this file.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what they
rejected.**

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode transmission
Hamlet sends begins with the RSID burst naming its mode and variant - Olivia, and PSK31
retroactively. Hamlet listens for RSID across the passband and when one arrives sets the mode,
the variant and the offset itself. The operator never picks a variant. **A carrier that never
announced itself gets the blind search of step 2 as a fallback.** The tables and codes are
`data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R30 - Synthetic fixtures first, the capture button from step 0.** *Tim has no
time to generate fldigi audio. So the fixtures under `assets/fixtures/olivia/` were made by the
web thread from **the mode author's own transmitter** - Pawel Jalocha's `pj_mfsk.h` as shipped in
fldigi, compiled and driven exactly as fldigi drives it - with RSID bursts from fldigi's own
encoder. They are independent of anything Hamlet thinks Olivia is, which is the lesson of the
PSK31 phase. `assets/reference/SOURCE.md` says how.*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md §3.3 - The error correction.** *Olivia's decoder either has a block or does not;
there are no garbled letters, only missing ones. R9 - a character not sure of is not shown - is
the mode's own behavior.*

**PHASE_PLAN.md §8, the revision of 2026-09-19.** *2.2 corrected - the -16 dB ceiling was the
author's, below the mode's own sensitivity; 3.0 added so the below-noise claim is proved on 8/250
at -14 dB; 2.1, 2.4, 2.5, 2.6 checked as unit 361 proved them.* **2.2 now reads:** *the -10 dB
fixture decodes at or under 0.05; the -16 dB fixture's CER is measured and reported with no
ceiling.*

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past
  the budget; a decision that changes what the product promises the operator - a fact stated
  about the radio, a contact or a send. A hint, a label, a number, a layout, a mechanism
  arithmetic will not allow: the arbiter decides, marks it author's and overrulable, and
  continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on. Never
  loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *A fixture will not decode at all. That is a finding about the demodulator, not the fixture -
  the fixtures are the mode author's. Report the spectrum measured against the manifest, mark
  `partial`, and name what the next unit tries.*
- *Reading `pj_mfsk.h` tempts a port. Read the structure; write Hamlet's own. If a unit cannot
  proceed without copying, `MOVE: stop` and say what it would copy.*
- *A package is needed. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R5 - The reference implementation.** *`fldigi`, which is GPL-3 - the same licence
as Hamlet ... never ported wholesale ... What is written for Hamlet is Hamlet's.*

**PSK31 plan §R9 - Squelch and the honest character.** *A character the demodulator was not sure
of is not shown - no `?`, no dimmed maybe. Silence.*

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was
shut, that later blocks the unit told to open the door, is the session's to rewrite in its own
commit so it guards the rule and not the shut door - and that is not a ruling, not an ask, and
not a stop.*

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file
alone, proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others; it does not add guards for doors it is not building,
pins against changes it is not making, or tests of a test.*

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never
transmits on a decode.* **This unit adds a receiver and touches nothing that keys.**

**FACT-004** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio.

**HM-DEC-139**, **HM-DEC-155**, as in sections 1 and 3.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings; they are in
the decision block at the end.

- **O. The search is its own class and is never told the answer.** `OliviaBlindSearch` under
  `src\Hamlet.RadioEngine\Olivia\`, given audio, its rate and a passband, hands back zero or more
  candidates - center, variant, a confidence, and the seconds of audio it needed. **It is given no
  center, no variant and no start time.** The manifest's `variant` and `center_hz` check what it
  found; they are never an input, as decision I said of the demodulator. Engine only: it is not
  wired into the Olivia panel, which keeps saying nothing decodes yet (decision L stands, and
  0.5 holds).
- **P. The variant is measured, then confirmed, and a tie is broken by the decode.** Take the tone
  spacing and the occupied band off the spectrum; match the pair against `format.json`'s rows;
  rank the rows that fit and confirm by trial decode with unit 361's demodulator, the winner being
  the one whose blocks stand furthest out of the noise on the measure `SyncThreshold` already
  uses. **A candidate no better than noise is not a detection.** The rows that must be separated
  are the phase's three - 8/250, 16/500, 32/1000. Whether the other four rows in `format.json`
  separate on the same two measurements is **reported as a finding, not built** (§R14).
- **Q. The stated time is stated in two numbers.** 2.3 says *within a stated time* and sets no
  ceiling, so the unit states, for the no-RSID fixture: **the seconds of audio from the start of
  the file that the search consumed before it named the center and the variant**, and **the CPU
  seconds it spent doing so**. Both go in the report. The decode that follows the search still
  meets 2.5's twenty-second CPU ceiling, which is reported separately from the search's.
- **R. A search that fires on noise is not a search.** `olivia-noise-only-30s.wav`, put through
  the blind search over the same passband, yields **no candidate**. This is part of 2.3's proof,
  not a new criterion: a search that always finds something has found nothing.
- **S. The drift fixture is made in the test and added to no folder.** 2.7's file is derived in
  memory from a shipped fixture whose hash was checked first, by a linear frequency ramp of
  **20 Hz per minute** across the whole file, and the recipe is stated in the report so anyone can
  make the same audio. **Nothing is written into `assets\fixtures\olivia\` and `manifest.json` is
  not edited** - the nine files and their hashes are the mode author's, and the entry check counts
  nine.
- **T. What *holds* means for 2.7.** The criterion says a drifting carrier *holds* and sets no
  number. **It is met when the drifted file decodes at CER 0.05 or under** - the below-the-noise
  ceiling rather than the clean one, because drift costs something - **and the demodulator reports
  the offset it tracked across the file.** 2.7 is nice-to-pass: if it misses, the number ships and
  2.7 is reported not met.
- **U. The -16 dB test now asserts what the corrected 2.2 asserts.** The plan's revision of
  2026-09-19 withdrew the 0.10 ceiling; `TheMinusSixteenDecibelFixtureDecodes` still asserts it,
  is red, and is off the carry-forward list for being red. It is rewritten **in its own commit**
  (§R12) to measure the CER, print it, and assert only what 2.2 and 2.5 now ask - that the file is
  read inside the CPU ceiling and that every character shown came from an accepted block (§3.3,
  §R9) - and its name goes onto the engine carry-forward line. **This is not loosening a test
  under §6**: the ceiling it guards is one the owner has withdrawn, and *a later ruling wins* is
  the clause. If the unit judges otherwise, it says so in section 4 and leaves the test alone.
- **V. Per-block tracking is allowed, and unit 361's numbers are re-measured after it.** If 2.7
  needs the frequency offset and the timing tracked per block rather than once per recording
  (unit 361 section 4 item 2), building that inside `OliviaDemodulator` is this unit's to do.
  **Every number in unit 361's decode table is then re-measured and printed beside the old one**:
  the three clean CERs, the -10 dB CER, the -16 dB CER, the noise character counts, the CPU per
  file. **A clean CER that moves off 0.0000, or a noise file that emits a character, is a
  regression and the change comes out.**

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_STATUS.md` has steps 0 and 1 `done`, and report what it says about step 2.
- **This unit's ground, first:** hash `olivia-8-250-qso-norsid.wav` against the manifest, then run
  the **RSID detector** over it and report that it finds **nothing** - that absence is what makes
  it the blind case and what this whole unit stands on. Then hash the other eight, 9 of 9. If any
  hash fails, stop.
- Append `UNIT 362 - STEP 2` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 361`
  entry. Touch no earlier entry.
- Patch-bump the version by one (unit 361 left 1.13.48).
- Run the carry-forward list, both invocations, before any change.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md` or anything under `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line. A class that asserts nothing, in the shape of `Unit361Trace`.

1. **`olivia-8-250-qso-norsid.wav`, measured as if nothing were known about it**: the spectrum
   after the first second - lowest and highest tone, occupied band at 20 dB, mean tone spacing,
   the symbol period measured from the tone changes, and where in the file the tones begin.
   **Print the manifest's `8/250` and `1000` beside it as the check, not the input.**
2. **The same measurements on `olivia-noise-only-30s.wav`**, so the report says in numbers what
   the search sees when there is nothing there. This is the floor decision R is judged against.
3. **`format.json`'s seven rows as a table** - tones, bandwidth, spacing, symbol seconds, first
   tone offset - and **which rows those two measurements separate and which they do not**. Name
   every pair that measurement alone cannot tell apart; that list is decision P's finding.
4. **What one trial decode costs and what a wrong variant looks like:** run
   `OliviaDemodulator.Decode` over the no-RSID file at each of 8/250, 16/500 and 32/1000 at the
   center item 1 measured, and print for each the CER, the blocks decoded and rejected, the sync
   S/N and the CPU. **A wrong variant must look obviously wrong on one of those numbers** - say
   which one, because that is what task 3 ranks on.
5. **Where the drift will bite:** how `Decode` chooses its frequency offset today - one offset for
   the whole recording - and the lines that decide it; and what 20 Hz per minute comes to in tone
   spacings across `olivia-16-500-qso-rsid.wav` and `olivia-8-250-cq-rsid.wav`.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the -16 dB test says what the criterion says (decision U, §R12)

**Its own commit**, as §R12 asks.

- Rewrite `TheMinusSixteenDecibelFixtureDecodes` so it measures the CER, prints it, and asserts
  only what corrected 2.2 and 2.5 ask. **Do not delete it** and do not weaken it into a test that
  cannot fail: it still fails if the file will not be read at all, if it runs past the CPU
  ceiling, or if a character is shown that no accepted block produced.
- Put its name on the engine line of `docs\carry-forward-tests.txt`, and **correct that file's
  `WHAT UNIT 361 ADDED` paragraph**, which says the name is left off because it is red.
- Say in section 1 what the assertion now is, in one sentence, and what it would take to fail it.

**Drop candidate:** none. It is small, and it clears the phase's only self-made red.

### Task 3 - the blind variant search (2.3)

Build `OliviaBlindSearch` under `src\Hamlet.RadioEngine\Olivia\`, to decisions O, P, Q and R. No
tone count, spacing, symbol length or bandwidth is a literal in code - they come from
`format.json`, as decisions H and M already bind the demodulator.

**Tests watched failing first**, in a new `TheOliviaBlindSearchTests` (engine):

- **2.3** `olivia-8-250-qso-norsid.wav`, hash checked first, put through the blind search over the
  passband with **no variant and no center given**, yields **one candidate: variant `8/250`,
  center within 5 Hz of the manifest's 1000 Hz**; and the decode that follows the search reads
  the manifest text at **CER 0.05 or under**. Report the CER to four places.
- **Decision Q** The seconds of audio the search consumed before it named the variant and the
  center, and its CPU seconds, are printed for that file; the decode after it is reported against
  2.5's twenty seconds separately.
- **Decision R** `olivia-noise-only-30s.wav` through the same search over the same passband yields
  **no candidate**.
- **§R13** The search writes its own event - the measurements it made, the candidates it weighed
  and the one it chose, with `mode: olivia` and the variant - in the category the PSK31 receive
  events use. **No decoded text and no callsign in it.** Proved by assertion.

Then run the carry-forward list, both invocations, with `TheOliviaBlindSearchTests` added to the
engine line in this task's commit - **and say what it cost in seconds**, as unit 361 did.

**If the search finds the carrier but names the wrong variant**, that is the finding: report the
measurements, the ranking, and what separated the rows wrongly, mark 2.3 not met, and name what
the next unit tries. **Do not tell the search the answer to make it pass**, and do not loosen the
5 Hz or the 0.05.

**Drop candidate:** none. This is the unit.

### Task 4 - the carrier that drifts (2.7)

**Only if task 3 is reported with its numbers.** Decisions S, T and V.

**Tests watched failing first**, in `TheOliviaDemodulatorTests` or a class of its own:

- A copy of a shipped clean fixture, made in the test with a 20 Hz-per-minute linear ramp
  (decision S), decodes at **CER 0.05 or under** (decision T), with the offset the demodulator
  tracked reported across the file.
- **Decision V's re-measurement table** if anything inside `OliviaDemodulator` changed: unit 361's
  eight rows measured again and printed beside the old numbers.

**Drop candidate: this whole task.** Drop it whole and say so; 2.7 is nice-to-pass, and step 3's
entry does not wait on it. **Do not drop it half-built** - a drifting fixture with no decode
number proves nothing.

---

## 9. Parked - do not touch, do not raise

- **Chasing a better -16 dB number.** 2.2 is met as corrected, by the measured figure. Tuning
  aimed only at that file is not this unit's; if decision V's tracking moves it, report the new
  number and move on.
- **Criterion 3.0's new fixture from the reference generator, the two-signal fixture, rows per
  station, wiring anything into the Olivia panel, a detection that sets the mode, variant or
  dial** - step 3.
- **An Olivia modulator, any Olivia send, the turn timing that undercounts Hamlet's own answer by
  the burst** (unit 359 item 4) - step 4.
- **`longestSeconds` on the typed line's record, the refusal sentence quoting the whole audio, the
  card's *60 s of text* rounding** (unit 360 items 1, 2, 4) - transmit-side findings, carried in
  section 4, not worked.
- **`RsidDetection` giving the first tone rather than the burst's end** (unit 361 item 4) - the
  derivation works and the helper exists.
- **Codes 72 to 75 and the fldigi commit pin** (unit 359 item 3); **the dial 1500 Hz below the
  center** (unit 358 item 1, step 0 closed); **the flaky Stop tests, HM-OPEN-090, the three
  off-list reds, the screen phase's open asks.** Carried in section 4, not worked.
- **`PHASE_PLAN.md`'s unchecked 1.5 and 1.7, `PHASE_STATUS.md`'s stale step 2 and unit number, and
  the mislabeled `PHASE_OUTCOME.md` entries.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not port `pj_mfsk.h`, `pj_fht.h` or `pj_gray.h`, and add no package.** Read the structure;
  write Hamlet's own. Anything that cannot be written without copying is `MOVE: stop` material in
  section 4, not built.
- **No literal tone count, spacing, symbol length, rate, bandwidth, scrambling code, shift or
  mapping in code.** They come from `format.json` (decisions H, M and O).
- **Never give the blind search a variant, a center or a start time**, in the code or in a test
  (decision O). A search told where to look has proved nothing.
- **Do not edit a fixture, `manifest.json`, or write a WAV into `assets\fixtures\olivia\`**
  (decision S).
- **Never loosen the 0.05 CER, the 5 Hz center or the 20 s CPU ceiling.** A miss ships with its
  number (§6).
- **Touch nothing on the transmit side**: not `Ft8TransmitSequence`, `UnslottedTransmission`,
  `Psk31Modulator`, `RsidBurst`, `PttOn` or any `Arm` site. `PttOn` 1 and `Arm(` 2 at the end, as
  at the start.
- **Do not wire anything into the app** (decision L).
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md`, `PHASE_PLAN.md` or earlier
  `PHASE_OUTCOME.md` entries.**
- **Report mismatches; repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Task 2's §R12 rewrite is its own commit. Never
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

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   and 1 done and closed; step 2 was five of seven met at the start of
   this unit (2.1, 2.2, 2.4, 2.5, 2.6) and is <state after this unit>;
   steps 3-6 not started, and step 3's entry opens only when 2.3 is met.
B. Step 2's two open criteria - 2.3 blind search: candidates found N,
   variant named <name> against 8/250, center N Hz against 1000 within
   5 Hz, CER N (0.05), audio seconds before the naming N, search CPU N s,
   noise-only candidates N (0); 2.7 drift: CER N (0.05) or dropped, offset
   tracked N to N Hz. Met: <list by id>. The five already met: re-measured
   <yes, with the numbers|not re-measured, nothing in the demodulator
   changed>.
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of 2.3 or 2.7 - in particular whether
   the search named a wrong variant and on which measurement, whether
   anything in pj_mfsk.h could not be written without copying (stop
   material), and whether task 2 left the -16 dB test asserting anything
   the corrected 2.2 does not ask for.
```

**Then the header:**

```
UNIT:       362 - <complete|stopped> at task N of 5, <task 4 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 2 criteria met 5 of 7 -> N of 7; the unannounced carrier found blind 0 -> <1|0>
DRIFT:      0
```

**Then a criterion table, 2.1 to 2.7** - 2.3 and 2.7 with this unit's numbers, and 2.1, 2.2, 2.4,
2.5 and 2.6 either re-measured with their numbers or marked *not re-measured, unchanged*, with
which it is stated plainly.

**Section 3 leads with the blind-search table**: for the no-RSID fixture and the noise-only
fixture, the measured tone spacing and occupied band, the candidate rows those admitted, the trial
decode of each candidate with its sync S/N, the one chosen, the center against 1000 Hz, the audio
seconds and CPU seconds the search needed, the CER of the decode that followed against 0.05. Then
the decoded text beside the manifest text if it is not exact. Then the search's §R13 event as
written, to show it carries no text or callsign. Then the drift table if task 4 was built - CER,
the tracked offset across the file, and the recipe that made the audio. Then decision V's
re-measurement of unit 361's eight rows if anything in the demodulator moved.

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004).

---

```
ARBITER-DECISION
STEP: 2
APPROACH: blind variant search for a carrier that never announced itself - the center and the variant found from occupied bandwidth and tone spacing, confirmed by trial decode against the noise floor, plus a drift fixture made from the shipped one
MOVE: continue
WHY: Step 2 is five of seven with 2.3 the only must-pass left, and step 3's entry is shut until it is met; the loop test finds no blind search in any entry, and unit 361 already read this very fixture at CER 0.0000 when handed the variant, so what is unbuilt is the finding and not the reading.
STATE: partial
DECIDED: author's, overrulable - (O) OliviaBlindSearch is its own engine class, given audio and a passband and never a center, a variant or a start time; the manifest checks what it found. (P) the variant is chosen by measured tone spacing and occupied band matched against format.json rows, ranked and confirmed by trial decode on the sync S/N the demodulator already uses, with a candidate no better than noise refused; only the phase's three rows must separate, and the other four are a reported finding. (Q) 2.3's stated time is stated as two numbers - audio seconds consumed before the naming, and search CPU seconds - with no ceiling set, the decode still measured against 2.5's twenty. (R) the noise-only fixture must yield no candidate; that is part of 2.3's proof. (S) 2.7's drifted audio is made in the test from a hash-checked shipped file by a 20 Hz-per-minute ramp, written into no folder and never into the manifest. (T) 2.7 holds means CER 0.05 or under with the tracked offset reported. (U) the -16 dB test is rewritten in its own commit to assert what the corrected 2.2 asks - read inside the CPU ceiling, every character from an accepted block, the CER measured and printed with no ceiling - and joins the carry-forward list, because the plan withdrew that ceiling on 2026-09-19 and a later ruling wins. (V) per-block frequency and timing tracking is allowed for 2.7, and unit 361's eight decode rows are re-measured beside their old numbers, a clean CER off 0.0000 being a regression that comes out. Task 4 is the drop candidate; tasks 0 to 3 have none. Unit 361's section 4 asked for no ruling; its item 1 is answered by the plan's own revision and marked in place.
LICENCE: PHASE_PLAN.md step 2 criteria 2.3 and 2.7, R27 (the blind search as the fallback), R30, R31, section 3.3, section 8 (the revision of 2026-09-19) and section 6 (a later ruling wins; a number or a mechanism is the arbiter's; a fixture that will not decode; the port clause); PSK31 plan R5, R9, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155; ARBITER.md section 6
ACCOMPLISHED: Hamlet finds an Olivia station that never announced itself - reading its variant and its place off the air the way an operator reads a waterfall - and reads it, while hearing nothing at all in noise; which is the last thing step 2 owed and the gate to hearing everyone
ADVANCES: step 2 criterion 2.3, the last must-pass shut between step 2 and step 3's entry (task 3), and criterion 2.7 (task 4)
END-ARBITER-DECISION
```
