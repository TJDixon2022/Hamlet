# Work instruction 365 - say it: Hamlet's own Olivia modulator, and an Olivia send through the one sequence

**Step 4 of `PHASE_PLAN.md`, authored by the arbiter.** Steps 0 to 3 are done and closed: Tim's
plan revision of 2026-09-19, evening (`cf62bd9d`), checked 2.3, 2.7 and 3.0 to 3.6 on units 363 and
364's reports and wrote *Step 4 is next*. The FT8 repair (work instruction 362, reused number,
`a3719b98` to `a7f81c48`) came before this unit and reported no regression. **This is the first unit
on step 4.** It builds Hamlet's own Olivia modulator and proves it against Hamlet's own demodulator and
the mode author's audio (4.1). It opens the one mode gate for Olivia so a send goes through the same
unslotted sequence with its RSID in front (4.2, 4.6). It scales the cap and the turn indicator's
patience by the variant (4.4). **Five tasks, 0 to 4. Task 4 is the drop candidate.**

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

**The engine invocation took 4 m 46 s for 142 tests** (unit 364). That is inside the 480 s timeout
by about three minutes. **That margin is this unit's to keep.** Report the seconds of both
invocations every time they run. **Nothing this unit adds goes on the engine line.** The Olivia send
guard goes on the app line (decision AW). The modulator's classes run by name beside the list.

**HM-DEC-165 - a mode that works stays working.** The list carries a send guard and a read guard
for every working mode. **A red after that was green before is a regression.** Name it in sections 1
and 4. If one appears, the repair is this unit's next task before any new criterion.

**CPU is measured alone.** Every class this unit adds that asserts or reports CPU goes in the
non-parallel `CpuMeasuredAlone` collection.

## 2. The tool facts, as units 359 to 364 and 362 (FT8) measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing. Use the
  editor.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`), because
  warnings are errors.
- **`sed -i` on `output.md`, a `>` redirect into the root, and `cat >>` onto
  `docs\carry-forward-tests.txt` were refused** as *outside the allowed working directories*. Use
  the file editor for all three.
- `rm` is refused, and so is a `for` loop over `$f`. Do not depend on `;`.
- **Python ran for unit 362 (FT8) and needed approval for earlier units.** Do not build a task on
  it.
- These needed approval, which a headless session cannot give: `mkdir`, `cp`, `mv`, `tee`,
  `powershell.exe`, `jq`, `awk`, `git restore --source`, `git checkout <rev> -- <file>`,
  `git stash push`, `git config`, command substitution, `grep -o` with a quantifier, and `grep -v`
  in a pipe. A command that includes one is refused whole. `grep -E` and `tail` after
  `dotnet test` ran. **Keep `Passed!|Failed!` in every filter** so the summary line survives.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran.
- `git show ... > file` is blocked as redirection. `sed -n N,Mp` and `tail -n +N | md5sum` in a
  pipe after `git show` ran.
- Anything outside `C:\Source\HamLet` cannot be listed or read.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `tools/status.sh` writes
  `RULES_AT: HM-DEC-161` and reads `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.

## 3. Asks still outstanding

Carried per HM-DEC-139. **`output.md` is not in the tree at `HEAD`.** Tim's plan commit `cf62bd9d`
removed it. The last report is `a7f81c48:output.md`, the FT8 unit's. **Carry its
`## 4. What's blocking us` verbatim, from line 202 to its end.** That includes its seven items and
the queue it carries from unit 364, which in turn carries 363's. Recover the bytes whichever way the
harness allows: `git show a7f81c48:output.md` read in pieces and written with the file editor, if
nothing shorter is permitted. **Check the carried copy:** `git show a7f81c48:output.md | tail -n +202
| md5sum` gives `5750364d4622527068ce9bc441b9abc4`. The same `md5sum` over your carried block must
give the same hash. Report both.

**Mark nothing in place.** Section 9 says which of the FT8 unit's seven items and unit 364's twelve
this instruction takes up.

---

## 4. Why this unit exists

**Hamlet reads Olivia and cannot say a word in it.** Rows are on the screen. Answer, the card and
the typed line all reach `SendMessage` and are refused at the mode gate, `CanTransmitIn`
(`MainWindowViewModel.cs:2446`), which names FT8, FT4 and PSK31 and not Olivia. That was decision AN
and step 0's 0.5, on purpose. **Step 4 opens that door.** R28 says *identical to PSK31 above the
modem*. So everything above the modem is already built: the macros, the typed line and its frame,
the cap, the arming, the sequence, Stop and the record. What does not exist is the modem itself, a
modulator. This unit builds that modulator, proves it against both ends, puts it behind the one
sequence with its RSID in front, and scales the timing rules.

Of step 4's seven must-passes, this unit takes 4.1, 4.2, 4.6 and 4.4. **4.3, 4.5 and 4.7 are the
next unit's** (section 9). They all stand on a send that goes out, and until now there was none.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal itself and never picked by
            the operator.
UNIT GOAL:  Give Hamlet its own Olivia voice: a modulator for 8/250, 16/500 and 32/1000
            that Hamlet's demodulator reads back letter for letter and that matches the
            mode author's audio in spacing, rate, preamble and bandwidth; and an Olivia
            press that goes out through the same one sequence as PSK31, announced by its
            variant's RSID, stoppable, with its cap and turn patience scaled by the
            variant - and FT8, FT4 and PSK31 exactly as they were.
ADVANCES:   step 4 criteria 4.1 (task 2, must-pass), 4.2 and 4.6 (task 3, must-pass),
            4.4 (task 4, must-pass, the drop candidate). Step 4 stays partial after this
            unit whatever happens: 4.3, 4.5, 4.7 and 4.8 are not in it.
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full.** Most of all: step 4, R27, R28, R29, R32, §3.2, and
§6's transmit clause.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 17:04 and the arbiter's own reading
after it:

- **HEAD is `cf62bd9d`** (`phase: olivia plan rev4 - steps 2 and 3 checked, HM-DEC-165`), a second
  commit of that name on top of `55ee028b`. Version **1.13.52** in `Directory.Build.props`.
- **`output.md` does not exist at `HEAD`**. The last report is `a7f81c48:output.md`, with its
  section 4 from line 202.
- **`PHASE_PLAN.md` at `HEAD` checks 2.3, 2.7 and 3.0 to 3.6.** It still leaves **1.5 and 1.7
  unchecked**, though the record has 1.5 met (unit 360) and 1.7 measured (unit 359). Its §6 opens
  with HM-DEC-165.
- **`PHASE_STATUS.md` still says step 3 `partial`**, `CURRENT_STEP: 3`, `WORK_INSTRUCTION: 358`.
  The plan's revision supersedes it: step 3 is done (§6, *a later ruling wins*). It is the
  launcher's file. Commit it as the launcher leaves it.
- **`PHASE_OUTCOME.md`**: the `UNIT 3 - STEP 3` entry says `STATE_AFTER: partial`. That was judged
  from unit 364's task-1 draft; its final report met all seven. The last entry is `UNIT 362 -
  CARRIED REPAIR`. Append-only.
- **The gate.** `CanTransmitIn` at `MainWindowViewModel.cs:2446` admits null, FT8, FT4 and PSK31.
  It is called at :15354 in `SendMessage` and by `CanAnswerRowsForTests` at :4408.
- **The keying sites.** `CivConstants.PttOn` has one code line (`Ft8TransmitSequence.cs:513`).
  `_armedSend.Arm(` has two lines (`MainWindowViewModel.cs:15524` and :15723).
- **The unslotted send.** `UnslottedTransmission` (`src\Hamlet.RadioEngine\Transmit\`) carries
  `AnnouncedCode`, `AnnouncementSamples`, `Cap` (`LongestSeconds` or
  `OperatorSend.LongestUnslottedSeconds`, 30) and `Fit`. `Fit` holds the text to the cap and the
  announcement to `RsidBurst`'s own length. `UnslottedMode` has a `Psk31` member. **Nothing in
  `Ft8TransmitSequence` switches on `UnslottedMode`**. It is used at `Psk31Modulator.cs:145`/:154
  and `MainWindowViewModel.cs:15701`. `Psk31Modulator.Compose(text, sampleRate, offsetHz, peak,
  longestSeconds)` puts `RsidBurst.Samples` in front and sets the announcement. The typed line's
  cap is `MainWindowViewModel.LongestTypedSeconds = 60` (:15879), and it is chosen at :15681.
- **The format.** `data\olivia\format.json` has seven variant rows, with tones, bandwidth, spacing,
  symbol seconds and first-tone offset, and cites `pj_mfsk.h` for the Gray code and for the
  raised-cosine symbol shape at half-symbol separation (:121, :227, :804). `data\olivia\timing.json`
  is measured, with `retire_after_characters: 56`, and is read through `OliviaTiming`. The RSID
  codes are `data\rsid\rsid-codes.json`: `OLIVIA_8_250` 69, `OLIVIA_16_500` 70,
  `OLIVIA_32_1000` 71.
- **The PSK31 turn indicator** is `Psk31Turn`. **The arbiter did not find a time-based patience in
  it** (`grep atience` finds nothing in `src`). Trace item 5 says what 4.4's *turn indicator's
  patience* is in the tree.
- **The chain-guarding list** is `docs\chain-guarding-tests.txt`: ten Transmit classes and three
  Audio classes on the engine project. Known reds on it are compared by name, never chased.
- **Carry-forward at the end of the FT8 unit:** engine 143 of 143, app 188 of 188 on the second
  run. The per-mode guard table is at the top of the list: *Olivia send: none yet - added the unit
  Olivia first transmits.* That is this unit.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree.**

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload's disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-161 (2026-09-11), and
  `CLAUDE.md` §1 holds CPS-DEC-0165.
- `PHASE_PLAN.md`'s unchecked 1.5 and 1.7. The stale `PHASE_STATUS.md` step 3 and unit number. The
  mislabeled `UNIT 1/2/3` harness entries in `PHASE_OUTCOME.md`. **Do not edit any of them.**
- R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`. The tree has
  `data/rsid/` and `data/bands/`.
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
  (the last one is also on the chain-guarding list).
- Headless flakes: `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`,
  `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` (dispatcher
  loop), and `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`. Rerun a
  red that passes alone up to three times, and say which run the number came from. **This unit
  changes the send path**, so no app red touching a send is presumed a flake. Say what it touches.
- Uncommitted at authoring: `PHASE_STATUS.md`, `.run-unit\reload.txt`, and this file.

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
every keyboard-mode send. ... a cap the plan calls the author's number, and a field on a record,
are neither the radio's safety nor a promise, and are never a stop.*

**PHASE_PLAN.md §3.2 - Speed.** *Every timing rule - the send cap, the turn indicator's patience,
the retire window - scales with the variant's seconds per character, taken from the mode author's
audio ... and never fixed in seconds.*

**PHASE_PLAN.md step 4, the criteria this unit works, verbatim:**
- *4.1 Loopback: each macro and a typed line, modulated by Hamlet at each variant, decodes through
  Hamlet's demodulator identical; and the modulated signal's tone spacing, symbol rate, preamble
  length and occupied bandwidth match the mode author's fixture for that variant within stated
  tolerances.*
- *4.2 Every send begins with its RSID; the detector reads it back as the variant sent.*
- *4.4 The cap and the turn indicator's patience are the timing table times stated factors, not
  fixed seconds; a Report at 8/250 is allowed its length.*
- *4.6 Nothing keys at the bench; one `PttOn` site; FT8, FT4 and PSK31 sends byte-identical to
  before except PSK31's new RSID prefix; Stop aborts an Olivia send.*

**PHASE_PLAN.md step 4 entry:** *step 3 done; the loopback chain of the PSK31 phase unchanged,
checked by its guarding tests first.*

**PHASE_PLAN.md step 2's reference clause, still binding.** *The reference is fldigi's
`pj_mfsk.h`, read at the pinned clone and never ported wholesale (PSK31 plan R5); the structure may
be followed, the code is Hamlet's.*

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *A mode that works stays working - HM-DEC-165, Tim, 2026-09-19. No unit is complete, whatever its
  own criteria say, if a mode that reached the air before it does not reach the air after it, or a
  mode that read the air before it reads less. The carry-forward list carries a send guard and a
  read guard per working mode; a red after is a regression, named, and the next unit's first task
  is the repair.*
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past
  the budget; a decision that changes what the product promises the operator - a fact stated
  about the radio, a contact or a send. A hint, a label, a number, a layout, a mechanism
  arithmetic will not allow: the arbiter decides, marks it author's and overrulable, and
  continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on. Never
  loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *Reading `pj_mfsk.h` tempts a port. Read the structure; write Hamlet's own. If a unit cannot
  proceed without copying, `MOVE: stop` and say what it would copy.*
- *A package is needed. `MOVE: stop`.*
- *Anything touches the transmit chain beyond adding an audio generator and an RSID prefix behind
  the one sequence. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R10 - one click, one transmission, one keying path.** Carried by §2 of the Olivia
plan: *one click one transmission and the one keying path.*

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was
shut, that later blocks the unit told to open the door, is the session's to rewrite in its own
commit so it guards the rule and not the shut door - and that is not a ruling, not an ask, and
not a stop.* **This applies to every test that asserts an Olivia send is refused.** Those are
`TheOliviaSeamTests`' 0.5 send half and `TheOliviaRowsTests`' decision-AN refusals. Rewrite each in
its own commit to guard what still holds: no second keying path, `PttOn` 1, `Arm(` 2, and no other
mode's decoder running under Olivia. Say which assertions changed and why.

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file
alone, proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others.*

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never
transmits on a decode.*

**FACT-004** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio.

**HM-DEC-139**, **HM-DEC-155**, **HM-DEC-165**, as in sections 1 and 3.

**Standing decisions of earlier instructions, still in force:** H (format constants from
`data\olivia\format.json` with `pj_mfsk.h` citations, transcription of a format's facts and not a
port), I (variant and center from the detector, never from the test), J (CER is Levenshtein over
the reference text's length, line endings unified, case exact), M (rates derive from
`format.json`), V (a clean CER off 0.0000, or a noise file that emits a character, after a receiver
change is a regression and the change comes out), D (the cap measures the samples after the
announcement, and the excusal is only the burst `RsidBurst` makes for that code at that rate), F
(the no-slot record carries `announced`, `rsidCode` and `announcementSeconds`), AF (one row path).
**Decision AN is lifted by this unit** for the mode gate only. Every other door stays as it was.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings. They are in
the decision block at the end.

- **AO. One door opens, and nothing else in the chain moves.** Olivia is added to `CanTransmitIn`.
  `UnslottedMode` gains an `Olivia` member, which is a label the record carries. The sequence does
  not switch on it. An Olivia send is an `UnslottedTransmission` armed at the **existing**
  unslotted `Arm(` site and played by the **existing** `Ft8TransmitSequence`. **No new `PttOn`
  site. No new `Arm(` line. No edit to `Ft8TransmitSequence`, `Ft8ArmedSend`,
  `UnslottedTransmission.Fit`/`Cap`/`AnnouncementIsTheBurst`, `OperatorSend`, `RsidBurst`, the
  abort, or the FT8/FT4/PSK31 branches of `SendMessage`.** If the Olivia send cannot be built
  without one of those edits, **stop that task, say which line and why, and write `MOVE: stop` in
  section 4.** That is §6's transmit clause, not the unit's to decide. Reaching the existing
  sequence through the one gate is exactly *an audio generator and an RSID prefix behind the one
  sequence*. Nothing wider is.
- **AP. `OliviaModulator` is Hamlet's own, in the engine.** It lives in `src\Hamlet.RadioEngine\Olivia\`.
  The format comes from `format.json` through `OliviaData`, and the rates derive from it (H, M): the
  scrambling, the shift, the character-to-Walsh map, the Gray code, the symbol shape and its
  half-symbol separation. **If a fact the modulator needs is missing from `format.json`**, it may
  be added **with its `pj_mfsk.h` line citation**, as decision H allowed. Examples are the shape's
  window, or the start and stop tones and the idle between blocks that fldigi sends. Adding a fact
  is transcription. Copying a routine is a port. The modulator is the demodulator's inverse
  written in Hamlet's words. **If it cannot be written without copying `pj_mfsk.h` code, stop and
  say what it would copy (§6).** `Compose(text, variant, sampleRate, centerHz, peak, longestSeconds)`
  returns an `UnslottedTransmission` shaped as `Psk31Modulator.Compose`'s is: burst first, from
  `RsidBurst` with the variant's code, then `AnnouncedCode` and `AnnouncementSamples` set from the
  burst it actually put there (decision D). A text is padded to whole blocks as the mode requires.
- **AQ. 4.1's proof, and its tolerances.** These are the arbiter's numbers.
  - **Loopback.** For each of 8/250, 16/500 and 32/1000, each of the four PSK31 macros, filled as
    the app fills them for a stated test call, and one typed line with its frame are modulated at
    the transmit rate the trace finds. The audio is resampled if the demodulator needs it. **The
    variant and center are found by the RSID detector on that audio** (decision I). The text is
    read by `OliviaDemodulator`. **CER 0.0000, identical text**, fifteen rows printed.
  - **Against the mode author.** The fixtures to compare are `olivia-8-250-cq-rsid.wav`,
    `olivia-16-500-qso-rsid.wav` and the clean 32/1000 file, each hash-checked. Measure both
    signals the same way, with one Hamlet routine in the test for both. **Tone spacing** must be
    within **0.5 %**. **Symbol rate** must be within **0.5 %**. **Preamble length** is the audio
    between the RSID burst's end and the first data symbol, including any start tones. It must be
    within **one symbol period**. **Occupied bandwidth** is the 99 % power bandwidth over the data
    section. It must be within **5 %**. A miss is shipped with its number and 4.1 is `partial`
    (§6). **Never widen a tolerance after measuring.** If the author's fixture has something
    Hamlet's audio lacks, such as start or stop tones, that is the finding. AP says how to close it.
- **AR. The variant and the place of a send are never the operator's (R27).** An Answer, Report,
  Confirm or typed line to a row goes **at that row's variant and center**, the ones its RSID or the
  blind search gave. A CQ, or any send with no row, goes at **8/250 on the tab's calling center**,
  the audio center step 0 tuned to (R29). **4.3's clear-spot and receipt rules are not built
  here.** Nothing on screen asks for a variant.
- **AS. Every Olivia send is announced, and says so (4.2, R32).** The burst for 69, 70 or 71 goes
  in front of every Olivia send. It is outside the cap by decision D's existing arithmetic, which
  does not change. The no-slot record carries `announced: true` and the code (decision F), and its
  mode says Olivia. **If the codes cannot be read, the send is refused, not sent unannounced.**
  This differs from PSK31, whose fallback predates R27's *always*. The arbiter decides it: an
  unannounced Olivia send would be one the other end cannot choose a decoder for.
- **AT. 4.4 - the cap and the patience in characters, times the variant's seconds per character.**
  - **The cap.** An Olivia send's `LongestSeconds` is `timing.json`'s seconds per character for
    its variant times a **character allowance**. The allowance is the **same number of characters
    PSK31's cap already allows**. For macros: 30 s divided by PSK31's seconds per character,
    measured by `Psk31Modulator.SecondsFor` over the four macros at their longest fill, rounded
    down to a whole character. For the typed line: the same from 60 s. **Both allowances go in
    `timing.json`** beside `retire_after_characters`, with that derivation stated, and are read
    through `OliviaTiming`. **Neither is a seconds literal in code.** `OperatorSend.LongestUnslottedSeconds`,
    `LongestTypedSeconds` and every FT8, FT4 and PSK31 cap do not move. The card's *too long to
    send* reads the same figure for an Olivia typed line.
  - **The patience.** It is whatever trace item 5 finds 4.4 names, scaled the same way: PSK31's
    figure in PSK31 characters, times the variant's seconds per character. **If the tree has no
    time-based patience for the turn indicator, 4.4's second half is reported as having nothing to
    scale**, with where the turn is decided. Nothing is invented to have something to scale (R14).
  - **"A Report at 8/250 is allowed its length"** is proved on the longest Report the app composes:
    the compound-callsign one that unit 360 measured for PSK31. At 8/250 it fits and is armed.
- **AU. Stop aborts an Olivia send by the path it aborts PSK31.** No new abort. 4.6's *Stop aborts*
  is proved on a fake wire and a fake sound card with an 8/250 send long enough to be mid-play.
- **AV. Telemetry (R13).** The PSK31 send-stage events and the no-slot record, with `mode` Olivia
  and the `variant`. Add one `olivia_send_composed` only if the trace shows the PSK31 composition
  event cannot carry the variant as it stands, and say which. **No text, no callsign.** The privacy
  walk counts any new event.
- **AW. Olivia's send guard (HM-DEC-165).** A new app class, `TheOliviaSendTests`, holds
  `AnOliviaCqReachesTheAir`. It runs on the application's own path with a fake wire and card: an
  Olivia CQ passes `composed`, `read_back` if the path has it, `armed`, `keyed` and
  `handed_to_the_sound_card`, then unkeys ordinarily, and writes one record announced with 69. That
  one method goes on the **app** line and into the guard table as *Olivia send*. This is the only
  change the table takes.
- **AX. The entry is checked on the chain's own guards.** The step 4 entry says *the loopback chain
  of the PSK31 phase unchanged, checked by its guarding tests first*. Run
  `docs\chain-guarding-tests.txt`'s thirteen classes, each by exact name and foreground, **before
  any change and again after task 3**. Compare the reds by name. **A red after that was green
  before is a regression**, and the rule above applies. Report both runs' reds by name.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_PLAN.md` has 3.0 to 3.6 checked and `PHASE_STATUS.md` still says step 3 `partial`.
  Report both.
- **Step 4's entry, first:** decision AX's before-run of the chain-guarding list, with the reds by
  name and the seconds. Then `ThePsk31CqGoesOutTests` and `TheFt8AndFt4SendsAreByteIdenticalTests`
  by name. If a class that guards the PSK31 loopback is red, stop and report it. Step 4's entry is
  not open.
- Hash all nine Olivia fixtures against the manifest, 9 of 9.
- Append `UNIT 365 - STEP 4` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 364`
  entry. Touch no earlier entry.
- Patch-bump the version by one, from 1.13.52.
- Run the carry-forward list, both invocations, before any change, and give the seconds and the
  per-mode guards' states.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md` or anything under `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line. Put it in `Unit365Trace`, a class that asserts nothing, in the shape of
`Unit364Trace`, not on the carry-forward line.

1. **The PSK31 send, press to sound card, with file and line.** Cover the CQ, Answer, the card's
   Report and Confirm, and the typed line. Say where each composes, where the mode gate refuses
   Olivia today, which `Arm(` site each reaches, and what `SendMessage` does between `read_back` and
   `armed` for PSK31. **Name every line decision AO would have to touch.** If any is on its
   forbidden list, say so here, before building.
2. **The transmit rate** the PSK31 path composes at, and what `OliviaDemodulator` expects.
3. **The mode author's signals, measured.** For each of the three clean fixtures: the RSID burst's
   end, what comes before the first data symbol (start tones, silence, their length), the tone
   spacing, the symbol rate, the 99 % bandwidth, and what follows the last block. **This is what
   decision AQ's tolerances are checked against, and what AP's modulator must reproduce.**
4. **What `format.json` lacks** for a modulator, fact by fact, each with the `pj_mfsk.h` line that
   states it.
5. **The turn indicator's patience.** What 4.4 would scale, where it lives, and its figure for
   PSK31. Or say that nothing time-based exists.
6. **PSK31's seconds per character** over the four macros at their longest fill, by
   `Psk31Modulator.SecondsFor`, and the two allowances decision AT derives from it. Give the
   resulting cap in seconds at each Olivia variant, and **the longest keying that allows at 8/250**,
   burst included, stated plainly.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the modulator (4.1)

Build `OliviaModulator` to decisions AP and AQ, in the engine.

**Tests watched failing first**, in a new `TheOliviaModulatorTests` (engine, `CpuMeasuredAlone`),
every fixture hash-checked first:

- **The loopback**: fifteen rows, each at CER 0.0000, with variant and center found by the detector.
- **Against the author**: for each variant, the four measurements beside the fixture's and the
  tolerance, each asserted.
- **The burst**: the detector reads 69, 70 and 71 at the center sent, within 5 Hz.
- **Noise-safe**: silence modulated as no text gives no characters through the demodulator.

**Not on the carry-forward line** (section 1). Run it by name, and give its seconds.

If a row decodes but not identically, that is a finding. Report the text, the blocks and the CER.
4.1 is not met, and no tolerance moves.

**Drop candidate:** none. Everything after it stands on the modulator.

### Task 3 - an Olivia send through the one sequence (4.2, 4.6)

Wire the send to decisions AO, AR, AS, AU, AV and AW. First rewrite the refusal tests under §R12,
each in its own commit.

**Tests watched failing first**, in `TheOliviaSendTests` (app):

- **AW's guard**, `AnOliviaCqReachesTheAir`.
- **4.2**: an Olivia CQ, an Answer to an 8/250 row and a Report to a 16/500 row. The two rows come
  from the two-signal fixture through the app's feed, as `TheOliviaRowsTests` makes them. **The
  audio handed to the sound card is read by the RSID detector as the variant sent, at the center
  sent**, and the record says `announced: true` with that code.
- **Decision AR**: the Answer goes at the row's variant and center. The CQ goes at 8/250 on the
  calling center. No control offers a variant.
- **4.6**: nothing keys a real port (fake wire). `PttOn` code lines stay 1 and `Arm(` lines stay 2,
  counted at the end. **Stop mid-play of an 8/250 send** aborts it by the same path PSK31 uses, with
  an ordinary unkey and the abort's record. `TheFt8AndFt4SendsAreByteIdenticalTests` and the PSK31
  send guards are unchanged and green.
- **Decision AS's refusal**: with the RSID codes unreadable, an Olivia press is refused in words
  and keys nothing.
- **Decision AV**: the stage events and the record carry Olivia and the variant, no text and no
  callsign. The privacy walk is green.

Add `TheOliviaSendTests.AnOliviaCqReachesTheAir` to the app line and to the guard table, in this
task's commit (AW). Then run decision AX's after-run of the chain-guarding list and the carry-forward
list, both invocations. Give the seconds and compare reds by name against task 0.

**If building the send needs a line on AO's forbidden list, stop this task there.** Commit nothing
that touches it. Write it in section 4 as `MOVE: stop` material with the line and the reason, and go
to reporting. Do not do task 4.

**Drop candidate:** none. It is 4.2 and 4.6, both must-pass, and HM-DEC-165's guard.

### Task 4 - the cap and the patience, scaled (4.4)

**Only if tasks 2 and 3 are reported with their numbers.** Wire it to decision AT.

**Tests watched failing first**, in `TheOliviaSendTests` or its own class:

- For each of the three variants: **the cap is the timing table's seconds per character times the
  stated allowance**, read from `timing.json` and asserted as that product, not a literal. Do the
  same for the typed line's.
- **The longest Report at 8/250 is armed and plays** (trace item 1's compound-callsign Report).
  **A text one character over the 8/250 allowance is refused** as `LongerThanTheCap`, with the same
  words PSK31's refusal uses.
- **The PSK31, FT8 and FT4 caps are unchanged**: 30, 60 and the slot's, each asserted.
- **The patience**, scaled the same way, or reported as having nothing to scale (AT).

Run the carry-forward list, both invocations, at the end.

**Drop candidate: this whole task.** Drop it whole and say so. An Olivia send then keeps PSK31's
30 s and 60 s caps, and a long 8/250 Report is refused honestly by `LongerThanTheCap`. The next unit
takes 4.4. Do not drop it half-built.

---

## 9. Parked - do not touch, do not raise

- **4.3** (the CQ on a clear spot at the calling center, the receipt without station facts or Log,
  a certain answer retiring it), **4.5** (move up 500 Hz and switch to 16/500), **4.7** (the power
  offer and the ALC on Olivia sends), **4.8** (the turnover within one block). These are the next
  unit's. Decision AR's CQ goes at the calling center with no clear-spot rule, and is not reported
  as 4.3.
- **Logging an Olivia contact, and the ledger with no mode** (unit 364 item 7). Step 5.
- **R27's across-tab switch** (unit 364 item 11). Logged for the plan's author, not built.
- **The FT8 unit's section 4.** Item 1, the retry at the press, is Tim's own commissioned repair,
  offered to him to overrule. It asks nothing that stops this unit, and he wrote *Step 4 is next*
  after it. It is carried, not reopened. Item 3, the starter card booked before the arm check, is a
  ruling on work instruction 309's design. It is carried, not changed, even though Olivia sends now
  pass through the same booking. Items 2, 4, 5, 6 and 7 are findings. Item 7 is answered by decision
  AW.
- **Unit 364's section 4.** Item 2's retire factor of 56 stands; it is in `timing.json`. Item 3 is
  this unit's gate, decision AO. Item 6, *an over's last line is not read until the next
  character*, bears on 4.8, not here. Items 8 to 10 (the strength dash, the wandering center, two
  RSID detectors) are logged. Items 1, 4, 5 and 12 are for the record.
- **Carried from earlier:** `longestSeconds` on the typed line's record (unit 360 item 1), the
  refusal sentence quoting the whole audio, the *60 s of text* rounding, `RsidDetection` giving the
  first tone, codes 72 to 75 and the fldigi commit pin, the dial 1500 Hz below the center (step 0
  closed), the flaky Stop tests, HM-OPEN-090, the three off-list reds, and the screen phase's open
  asks. Carried in section 4, not worked.
- **`PHASE_PLAN.md`'s unchecked 1.5 and 1.7, `PHASE_STATUS.md`'s stale step 3 and unit number, and
  the mislabeled `PHASE_OUTCOME.md` entries.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Nothing keys a real port.** Every send test runs on a fake wire and a fake sound card.
- **Do not touch any line on decision AO's forbidden list.** The answer to needing one is a stop,
  not an edit.
- **Do not port `pj_mfsk.h`.** Facts go in `format.json` with citations. Routines are Hamlet's (AP).
- **Never give the demodulator or the detector a variant, a center or a start time** from the test
  or the modulator's inputs (decision I). They find what was sent.
- **No cap, patience or window in literal seconds for Olivia** (§3.2, AT). Do not move PSK31's, FT8's
  or FT4's.
- **Never loosen CER 0.0000, the 0.5 %, the one symbol, the 5 %, the 5 Hz, or a cap test.** A miss
  ships with its number (§6).
- **Do not let the operator pick a variant**, on screen or in Settings (R27, AR).
- **Do not edit a fixture, `manifest.json` or `corpus.json`, and write no WAV under `assets\`.**
- **Do not edit an existing test to make a change pass.** The only exceptions are the Olivia refusal
  assertions under §R12, each in its own commit, said in the report.
- **Nothing on the engine carry-forward line** (section 1).
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md`, `PHASE_PLAN.md` or earlier
  `PHASE_OUTCOME.md` entries.** Add no package.
- **Report mismatches. Repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Never commit `SESSION.lock` or `.run-unit\`. The
report names the branch and whether every push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 4 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   to 3 done and closed (Tim's plan revision of 2026-09-19, evening);
   step 4 was not started at the start of this unit and is <state>
   after it; steps 5 and 6 not started. Step 4 cannot be done on this
   unit alone - 4.3, 4.5, 4.7 and 4.8 are the next unit's.
B. Step 4's criteria this unit worked - 4.1: loopback rows N of 15 at
   CER 0.0000; against the author, per variant, spacing N % (0.5),
   symbol rate N % (0.5), preamble N symbols (1), bandwidth N % (5).
   4.2: sends read back by the detector as the variant sent N of N,
   records announced N of N. 4.6: PttOn N (1), Arm( N (2), real ports
   keyed N (0), Stop aborted the 8/250 send <yes|no>, FT8/FT4 byte-
   identical <green|red>, PSK31 send guards <green|red>. 4.4: allowances
   N and N characters, caps N / N / N s, longest 8/250 Report <armed|
   refused>, patience <scaled|nothing to scale>, or dropped. Met: <list
   by id>. Not met: <list, with the number>. Entry: chain-guarding reds
   before N, after N, new N (0).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of a step 4 criterion - in
   particular whether building the send needed a line on decision AO's
   forbidden list (stop material), whether the modulator needed code
   copied from pj_mfsk.h (stop material), whether any working mode's
   guard went red after green (HM-DEC-165), and the engine and app
   carry-forward seconds against the 480 s timeout.
```

**Then the header:**

```
UNIT:       365 - <complete|stopped> at task N of 4, <task 4 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 4 criteria met 0 of 8 -> N of 8; modes that reach the air 3 -> N
DRIFT:      0
```

**Then a criterion table, 4.1 to 4.8.** Give 4.1, 4.2, 4.4 and 4.6 with this unit's numbers and
state (*met*, *not met*, *partial*). List 4.3, 4.5, 4.7 and 4.8 as *not worked - next unit*.

**Section 3 leads with what the operator would see.** Show the Olivia tab with the two rows, an
Answer pressed on the 8/250 row, and what the screen and the record say, stage by stage, from the
press to the unkey. Show the same for a CQ. Then give:

- the fifteen-row loopback table
- the author-comparison table: variant, measure, author, Hamlet, difference, tolerance
- the detector's read-back of each send's burst
- the cap table: variant, seconds per character, allowance, cap, longest keying with the burst
- the chain-guarding and carry-forward before and after, by name for every red
- the `PttOn` and `Arm(` counts

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004). In
particular, **no Olivia signal from Hamlet has been on the air**. The first one will be Tim's.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: Hamlet own Olivia modulator from format.json proved by loopback through the demodulator and against the mode author fixtures, and an Olivia send through the one unslotted sequence with its RSID prefix by opening only the mode gate, cap and patience as characters times the timing table
MOVE: continue
WHY: Steps 0 to 3 are done and closed by Tim's plan revision of 2026-09-19, which names step 4 next; the FT8 repair that paused the phase reported no regression. The loop test finds no Olivia modulator or Olivia send in any entry, so this is the first unit on step 4, not a retry. Its only transmit change is the audio generator and RSID prefix behind the one sequence that section 6 licenses, fenced by a stop on anything wider.
STATE: not started
DECIDED: author's, overrulable - (AO) only CanTransmitIn opens for Olivia and UnslottedMode gains an Olivia label; an Olivia send uses the existing unslotted Arm site and Ft8TransmitSequence, with no new PttOn or Arm line and no edit to the sequence, Ft8ArmedSend, Fit, Cap, OperatorSend, RsidBurst, the abort or the other modes' branches - needing one is MOVE: stop. (AP) OliviaModulator is Hamlet's own in the engine, from format.json; missing format facts may be added with pj_mfsk.h citations; copying a routine is stop material. (AQ) 4.1 is fifteen loopback rows at CER 0.0000 with variant and center found by the detector, and against the author's three clean fixtures spacing and symbol rate within 0.5 percent, preamble within one symbol, 99 percent bandwidth within 5 percent, never widened. (AR) a send to a row goes at the row's variant and center; a CQ or rowless send at 8/250 on the tab's calling center; no variant control; 4.3's clear spot not built. (AS) every Olivia send carries its 69, 70 or 71 burst and says so on the record; unreadable codes refuse the send. (AT) the Olivia cap is the variant's seconds per character times an allowance in characters equal to what PSK31's 30 s and 60 s caps allow at PSK31's measured rate, stored in timing.json; the patience scaled the same way or reported as nothing to scale; no other mode's cap moves. (AU) Stop aborts by PSK31's path. (AV) the PSK31 send events and record with mode Olivia and the variant, no text or callsign. (AW) TheOliviaSendTests.AnOliviaCqReachesTheAir is Olivia's send guard on the app line. (AX) the chain-guarding list is run before any change and after task 3, reds compared by name. Decision AN lifted for the mode gate only; the Olivia refusal tests are rewritten under R12. Task 4 (4.4) is the drop candidate; tasks 0 to 3 have none. The FT8 unit's section 4 item 1 invites the owner to overrule a repair he commissioned and asks no decision that stops this unit; it is carried, as are the rest, and unit 364's items 3 and 7 are taken as AO and parked to step 5.
LICENCE: PHASE_PLAN.md step 4 criteria 4.1, 4.2, 4.4 and 4.6 and its entry, the revision of 2026-09-19 evening, R27, R28, R29, R31, R32, section 3.2, and section 6 (the transmit clause, the port clause, a package is needed, a later ruling wins, never loosen a test, HM-DEC-165); PSK31 plan R5, R10, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155, HM-DEC-165; ARBITER.md sections 2 and 6
ACCOMPLISHED: Hamlet speaks Olivia - its own signal, which its own receiver reads back letter for letter and which matches the mode author's in every measure - and an Olivia CQ or answer goes out through the same single keying path as PSK31, announced by its variant so the other end's software switches to it, stoppable, and allowed the length the slow variants need, with FT8, FT4 and PSK31 exactly as they were
ADVANCES: step 4 criteria 4.1 (task 2), 4.2 and 4.6 (task 3), 4.4 (task 4, the drop candidate), all must-pass; step 4 stays partial, with 4.3, 4.5, 4.7 and 4.8 left to the next unit
END-ARBITER-DECISION
```
