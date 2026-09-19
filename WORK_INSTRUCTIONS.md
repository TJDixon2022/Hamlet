# Work instruction 363 - hear everyone: the Olivia listener, and the below-noise claim proved at 8/250

**Step 3 of `PHASE_PLAN.md`, authored by the arbiter.** Steps 0, 1 and 2 are done and closed. Step 2
closed with unit 362: seven of seven, the blind search finding the no-RSID 8/250 carrier and the
drift fixture holding, and the state session read the report as `done`. **Step 3 is not started,
with none of its seven criteria met.** This unit takes the engine half of step 3: **3.0**, the
below-noise decode at 8/250 and -14 dB, which is a must-pass the plan added on 2026-09-19; **the
engine half of 3.1**, an `OliviaListener` that hands out one channel per station the way
`Psk31Listener` does for PSK31, proved on the two-signal fixture; and **3.6**'s real-time ratio,
measured on that listener. The rows on screen, the parser, the retire rule and the row telemetry
(3.2 to 3.5) are the next unit's and they stand on this one. **Five tasks, 0 to 4; task 4 is the
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

**These tests are slow, and that is a number, not a reason to background them.** Unit 362 measured
the engine invocation at 1 m 23 s for 124 tests. A listener fed three fixtures in quarter-second
pieces will add to that. Report the seconds; do not cut a fixture short to make a number better.

**CPU is measured alone.** Unit 362 found that process CPU counts the classes running beside a
test, and put its classes in the non-parallel `CpuMeasuredAlone` collection. **Every class this unit
adds that asserts or reports CPU goes in that collection.**

## 2. The tool facts, as units 359 to 362 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing (unit
  361). Use the editor.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`), since
  warnings are errors. Anchor on the comment's first line.
- `rm` is refused. A `for` loop over `$f` is refused. **A command joined with `;` ran for unit 362**
  where earlier units were refused; do not depend on it.
- **Python is unreliable**: it ran for unit 361 from the scratchpad, and needed approval for unit
  362 from the root. Do not build a task on it. `python -c` needs approval.
- `mkdir`, `cp`, `mv`, `tee`, `powershell.exe`, `jq`, `awk`, `git restore --source`,
  `git checkout <rev> -- <file>`, `git stash push`, `git check-ignore`, command substitution, and
  `grep -v "^\s*$"` or `sed -n '/a/,/b/p'` piped after `dotnet test` needed approval, which a
  headless session cannot give. `grep -E` and `tail` after `dotnet test` ran. Write files with the
  editor.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. Run directly
  (not through `sh`) inside an `&&` chain, it needed approval.
- `git show ... > file` and `git show ... | tail >> file` are blocked as redirection. `sed -n N,Mp`
  in a pipe after `git show` ran; `tail -n +N file | md5sum` ran. **Unit 362 carried its queue in
  with the file editor and checked it by `md5sum`**; do the same.
- Anything outside `C:\Source\HamLet` cannot be listed or read. Jalocha's headers are inside the
  root at `assets\reference\jalocha\`.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `WORKING` and `claude` are
  not allowed words. `tools/status.sh` writes `RULES_AT: HM-DEC-161` and reads `WORK_INSTRUCTION`
  from `PHASE_STATUS.md`, which still says 358.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 362's `## 4. What's blocking us` verbatim, from its first line
to its end**, its nested queues and the reference to `4c55deac:output.md` included, the way unit
362 carried unit 361's. Unit 362's report is `output.md` in the working tree, committed in
`60ec790a`; check the carried copy against it by `md5sum`.

**Mark nothing in place.** This instruction answers none of the carried items; section 9 says
which of unit 362's seven it takes up and how.

---

## 4. Why this unit exists

**Step 2 is done: Hamlet can read one Olivia station, named or unnamed. It cannot yet read two at
once, or read one while it is arriving, and nothing it reads reaches the screen.** Step 3 is where
Olivia starts to look like PSK31 to the operator - a row per station - and every row stands on an
engine object that is fed the passband as it arrives and hands back one channel per station. For
PSK31 that object is `Psk31Listener`. **Olivia has no such object**: `OliviaDemodulator.Decode`
reads a whole recording at once, and unit 361 item 2 and unit 362 item 4 both said step 3 would
need it fed as a stream. That is task 3.

**And the phase's headline claim is not yet proved.** §1 says Olivia *reads around -13 dB where
PSK31 falls apart at -10*. The plan's revision of 2026-09-19 withdrew the -16 dB ceiling at 16/500,
which was set below the mode's own sensitivity, and added 3.0 so the below-noise claim is proved at
the variant that can do it: 8/250 at -14 dB. That is task 2.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal itself and never picked by
            the operator.
UNIT GOAL:  Prove Olivia reads below the noise - an 8/250 QSO at -14 dB in 2500 Hz, RSID in
            front, decoded at CER 0.10 or under with the variant from its RSID - and build
            the engine's Olivia listener: audio fed as it arrives, one channel per station
            heard by RSID or found blind, each with its own demodulator and text as blocks
            arrive, the two-signal fixture yielding two channels with nothing of one in the
            other, at a real-time ratio reported.
ADVANCES:   step 3 criterion 3.0 (task 2, must-pass); the engine half of 3.1 (task 3,
            must-pass - the rows are the next unit's); 3.6 measured on the listener (task 4,
            nice-to-pass).
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full** - step 3, §3.2 (every timing rule scales with the
variant), §3.3, R28 and §8's revision record above all.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 11:44 and the arbiter's own reading
after it:

- **HEAD is `60ec790a`** (`docs(unit362): the report ...`). Version **1.13.49** in
  `Directory.Build.props`.
- **`output.md` is unit 362's report**, in the working tree and committed.
- **`PHASE_STATUS.md` says steps 0, 1 and 2 `done`, step 3 `not started`, `CURRENT_STEP: 3`**, and
  still `WORK_INSTRUCTION: 358`.
- **`PHASE_OUTCOME.md` ends with a `UNIT 1 - STEP 2` entry** carrying unit 362's approach,
  `FATE: executed` and `STATE_AFTER: done`. Append-only.
- **The fixtures** are nine files under `assets\fixtures\olivia\` with `manifest.json`. This unit's:
  - `olivia-two-signals-rsid.wav` - 28.98 s, `8/250 + 16/500`, centers `"1000,2000"`, both with
    RSID, *KC3QIS 8/250 at 1000 Hz and EI4GNB 16/500 at 2000 Hz*; its `text` is the two stations'
    CQs joined by ` | `, the 8/250 station's first.
  - `olivia-8-250-cq-rsid.wav` - 28.98 s, 8/250 at 1000, RSID in front.
  - `olivia-8-250-qso-norsid.wav` - 172.06 s, 8/250 at 1000, no RSID, the four-line QSO text.
  - `olivia-noise-only-30s.wav`, and the three clean and two noisy files for the regression rows.
- **`assets\reference\olivia-fixture-generator.cpp`** takes `tones bandwidth centerHz outfile text`
  and writes a clean 8000 Hz WAV: **no RSID and no noise**. `assets\reference\SOURCE.md` says
  *nothing here is built by a session*, and that the RSID bursts came from a port of fldigi's
  `cRsId`, not from this program.
- **`src\Hamlet.RadioEngine\Olivia\`** holds `OliviaBlindSearch.cs`, `OliviaCallingTable.cs`,
  `OliviaData.cs`, `OliviaDemodulator.cs`, `OliviaFormat.cs`, `OliviaTiming.cs`.
  `OliviaDemodulator.Decode(MonoAudio, double startSeconds)` reads a whole recording, now with
  per-block offset tracking (`TrackTones` 2, `TrackSmoothing` 2) and an `OffsetTrack` on its result.
  `OliviaBlindSearch.Search(MonoAudio, lowestHz, highestHz)` returns `OliviaSearch`, re-reading from
  the start each time it takes more audio (unit 362 item 4).
- **`RsidDetector`** has a streaming `Feed(ReadOnlySpan<float>)` and `Flush()` beside the static
  `Detect`.
- **`Psk31Listener`** (`src\Hamlet.RadioEngine\Psk31\Psk31Listener.cs`) is the shape this unit
  follows: `Add(samples)`, `Channels` as `Psk31Channel(Id, OffsetHz, StrengthDb, Text, Readable)`,
  a replay of `ReplaySeconds` to a new channel, `States` for telemetry, *knows nothing about tabs,
  rows or radios*. `MainWindowViewModel` holds one and draws its channels.
- **Carry-forward at the end of unit 362:** engine 124 of 124, app 166 of 166. The engine line
  carries `TheOliviaDemodulatorTests` by name, five of its six (not
  `TheTimingTableIsMeasuredByTheDemodulator`), and `TheOliviaBlindSearchTests` and
  `TheOliviaDriftTests` by class.
- `CivConstants.PttOn` code lines: **1** (`Ft8TransmitSequence.cs:513`). `_armedSend.Arm(` lines:
  **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree**, so
  §6's *real off-air audio appears* branch does not fire.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- `PHASE_PLAN.md` shows **2.3 and 2.7 unchecked**, and **1.5 and 1.7 unchecked**, though steps 1
  and 2 are `done` in the record. The arbiter writes only this file; **do not edit `PHASE_PLAN.md`.**
- The reload's one disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-161 (2026-09-11);
  `CLAUDE.md` §1 holds CPS-DEC-0164.
- `PHASE_STATUS.md` says `WORK_INSTRUCTION: 358`. It is the launcher's file; commit it as the
  launcher leaves it.
- `PHASE_OUTCOME.md`'s harness entries are headed `UNIT 1` and `UNIT 2`, and `UNIT 2 - STEP 1`
  reads `FATE: executed` for a run that never happened.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`; the
  tree has `data/rsid/` and `data/bands/`.
- `PHASE_PLAN.md` 2.3 says *tone spacing and symbol rate*; the search measures spacing and
  occupied band (unit 362 item 1). Logged; not this unit's.
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- The app carry-forward list is flaky run to run. Rerun a red that passes alone up to three
  times, and say which run the number came from. **This unit changes no app code, so an app red
  is a flake or older than this unit** - say which.
- `.unit362-carry.tmp` sits in the root, ignored by git, left by unit 362 because `rm` is refused.
- Uncommitted at authoring: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`, everything
  under `.run-unit\`, and this file.

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

**PHASE_PLAN.md R30 - Synthetic fixtures first, the capture button from step 0.** *Tim has no
time to generate fldigi audio. So the fixtures under `assets/fixtures/olivia/` were made by the
web thread from **the mode author's own transmitter** - Pawel Jalocha's `pj_mfsk.h` as shipped in
fldigi, compiled and driven exactly as fldigi drives it - with RSID bursts from fldigi's own
encoder. They are independent of anything Hamlet thinks Olivia is, which is the lesson of the
PSK31 phase. `assets/reference/SOURCE.md` says how.*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md §3.2 - Speed.** *Every timing rule - the send cap, the turn indicator's patience,
the retire window - scales with the variant's seconds per character, taken from the mode author's
audio ... and never fixed in seconds.*

**PHASE_PLAN.md §3.3 - The error correction.** *Olivia's decoder either has a block or does not;
there are no garbled letters, only missing ones. R9 - a character not sure of is not shown - is
the mode's own behavior.*

**PHASE_PLAN.md §8, the revision of 2026-09-19.** *2.2 corrected - the -16 dB ceiling was the
author's, below the mode's own sensitivity; **3.0 added so the below-noise claim is proved on
8/250 at -14 dB**.* **3.0 reads:** *A new fixture from the reference generator - 8/250 at -14 dB in
2500 Hz, RSID in front, the QSO text - is made by the unit and decodes at or under 0.10; the
below-noise claim proved at the variant that can do it.*

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

**Standing decisions of earlier instructions, still in force:** H (format constants from
`data\olivia\format.json` with citations), I (variant and center from the detector, never from the
test), J (CER is Levenshtein over the manifest text's length, line endings unified, case exact),
L (engine only, the panel is not wired), M (rates and samples per symbol derive from
`format.json`), O (the blind search is never told the answer), V (a clean CER off 0.0000, or a
noise file that emits a character, after a demodulator change is a regression and the change
comes out).

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings; they are in
the decision block at the end.

- **W. 3.0's fixture is made in the test from the mode author's own audio, and the generator is
  not compiled.** The plan asks for a fixture *from the reference generator* and *made by the
  unit*. The reference generator's output for exactly this variant, center and text is already in
  the tree and hashed - `olivia-8-250-qso-norsid.wav` is Jalocha's transmitter sending the QSO at
  8/250 and 1000 Hz. The generator itself makes no RSID and no noise, `SOURCE.md` says nothing
  there is built by a session, and compiling it would need a C++ toolchain this headless session
  cannot be shown to have and may not install (§6, *a package is needed*). **So the unit makes the
  fixture in memory, as unit 362 made 2.7's:**
  1. hash-check `olivia-8-250-cq-rsid.wav` and `olivia-8-250-qso-norsid.wav` first;
  2. take the **RSID burst from the front of `olivia-8-250-cq-rsid.wav`** - the fldigi-port burst
     the web thread made, from the first sample to the burst's end plus its trailing silence, the
     end located by the detector and `rsid-codes.json`'s symbol count and rate, not by a literal -
     and put it in front of the whole of `olivia-8-250-qso-norsid.wav`, both at their shipped
     levels. Every sample of signal is then the web thread's, none of it Hamlet's;
  3. add **white Gaussian noise from a seeded generator** (the seed stated in the test and the
     report), scaled so the Olivia signal's power, measured over the QSO part alone, stands
     **-14 dB against the noise power in 2500 Hz** - the noise's total variance times 2500 over
     the Nyquist 4000 - and **measure the SNR back** by the method unit 361 used to read the -10
     dB file at -10.48 dB, printing both;
  4. write it into no folder; `manifest.json` is not edited and the entry check still counts nine.
  **One realization of noise is one draw.** The test asserts on the stated seed and **prints the
  CER on four further seeds, not asserted**, so the report shows whether 0.10 was met by the mode
  or by luck. To overrule, say *compile the generator*.
- **X. 3.0's variant comes from the made fixture's RSID.** The detector reads the burst in the
  noisy audio and names 8/250 at 1000 within 5 Hz, and the demodulator is built from that
  detection (decision I). **The 0.10 is not loosened**; a miss ships its number and 3.0 is
  `partial` (§6). If the burst itself is not detected at -14 dB, that is the finding - step 1's
  1.3 read it at -16 on 16/500 - and the decode is still run at the blind search's answer and
  reported, 3.0 not met.
- **Y. `OliviaListener` is the engine's one Olivia receiver, shaped like `Psk31Listener`.** Under
  `src\Hamlet.RadioEngine\Olivia\`. **Samples go in as they arrive; channels come out.** It knows
  nothing about tabs, rows or radios. Inside it: the streaming `RsidDetector` across the passband;
  the blind search for carriers that announced nothing; and **one demodulator per channel**, built
  at the variant and center it was found at. A channel carries at least an id stable for its life,
  the variant, the center (tracked), how it was found (`rsid` or `blind`), the text so far and
  the blocks decoded and rejected. **Text appears as blocks are accepted**, never a character
  before its block (§3.3, §R9). A new channel is given the audio from its burst's end, or from the
  point the search consumed, so a station's first block is not lost - the Olivia analogue of
  `ReplaySeconds`, derived from the variant's block length, not a literal.
- **Z. The listener reads a stream, and does not re-read the recording.** Feeding it must not cost
  a whole-history decode per block, nor a whole-history search per step: the running state the
  demodulator and the search need - offset track, symbol and block phase, averaged spectrum - is
  kept and advanced. How is the unit's; `pj_mfsk.h`'s receiver structure may be read and not
  ported. **The regression gate: each shipped file fed to the listener in quarter-second pieces
  reads what `Decode` read over the whole file** - the three clean files at CER 0.0000, the -10 dB
  file at 0.05 or under, the noise-only file with no channel and no character - printed beside
  unit 362's numbers. A clean file off 0.0000 through the listener is a regression in the listener
  and is not shipped as met.
- **AA. One station, one channel.** A carrier announced by RSID is not opened a second time by
  the blind search, and a blind-found carrier that later sends an RSID at the same place becomes
  that announced channel rather than a second one. *The same place* is within half the narrower
  variant's bandwidth. A new RSID at an occupied place naming a **different** variant ends the old
  channel and opens a new one - the station said it changed; the card's handling of that is step
  4's (R29). **No channel is retired for going quiet in this unit**: channels stay listed, and the
  retire window of 3.4 is the next unit's, scaled by the timing table (§3.2).
- **AB. 3.1's engine half, and what *nothing of one in the other* means.** The two-signal fixture
  fed to the listener yields **exactly two channels: 8/250 within 5 Hz of 1000, 16/500 within 5 Hz
  of 2000**, each found by `rsid`; each channel's text against its own half of the manifest `text`
  (split at ` | `) at **CER 0.05 or under**; and **the other station's callsign appears nowhere in
  a channel's text**, nor any character no accepted block of that channel produced. The rows are
  not drawn in this unit, so **3.1 is reported *engine half met*, not met**; the next unit draws
  the rows from these channels and checks it.
- **AC. Unit 362 item 2 is this unit's only if it bites.** A block that clears the threshold with
  wrong characters in it is the risk the two-signal test exists to catch. **If a channel shows a
  character that is not its own station's**, building the per-character gate unit 362 item 2
  proposes is inside this unit, under decision V's re-measurement rule. If it does not bite,
  item 2 stays logged, not chased.
- **AD. The listener's own event** (§R13): one when a channel opens - `mode: olivia`, the variant,
  the center, how found, the audio seconds at which it opened - and a periodic or per-block state
  in the shape of `Psk31Listener.States`, in the category the Olivia receive events already use.
  **No decoded text and no callsign.** Proved by assertion on the two-signal fixture.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_STATUS.md` has steps 0, 1 and 2 `done` and step 3 `not started`, and report it.
- **Step 3's entry, first:** hash `olivia-two-signals-rsid.wav` against the manifest, then run the
  RSID detector over it and report **two detections, 8/250 at 1000 and 16/500 at 2000**, with
  their centers. Then hash the other eight, 9 of 9. If any hash fails, or the detector does not
  find both, stop.
- Append `UNIT 363 - STEP 3` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 362`
  entry. Touch no earlier entry.
- Patch-bump the version by one (unit 362 left 1.13.49).
- Run the carry-forward list, both invocations, before any change.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md` or anything under `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line. A class that asserts nothing, `Unit363Trace`, in the shape of `Unit362Trace`.

1. **The -14 dB fixture, made by decision W's recipe, measured before any change:** where the
   burst slice ends, in samples and seconds, and how that was located; the SNR set and the SNR
   measured back; **the same measure on the shipped -10 dB file** beside it, as the check on the
   method; the detector's reading on the made audio; then `OliviaDemodulator.Decode` at that
   detection - CER, blocks decoded and rejected, sync S/N, CPU - on the stated seed and the four
   others. **This is the number 3.0 starts from.**
2. **The two-signal fixture through what exists today:** each detection handed to its own
   `OliviaDemodulator.Decode` over the whole file - CER of each against its half of the text,
   blocks decoded and rejected, and **any character from the other station in each** - and
   `OliviaBlindSearch` over it, told nothing, with what it names. This is what the listener must at
   least equal.
3. **What in `Decode` is whole-recording today, with the lines:** the offset search and track, the
   symbol phase, the block phase, the passes, and anything else that looks at audio not yet
   arrived. For each, what a stream must hold instead. **And the same for `OliviaBlindSearch`**
   (unit 362 item 4).
4. **`Psk31Listener`'s shape as the app uses it**, with file and line: how it is fed and in what
   pieces, what a channel carries, how a new channel is replayed, what `States` gives telemetry,
   and where `MainWindowViewModel` reads `Channels` - so `OliviaListener` fits the place the next
   unit will plug it into.
5. **What the variants' blocks cost in time:** block length in seconds at 8/250, 16/500 and
   32/1000 from `format.json`, and the seconds from a burst's end to the first accepted block on
   the three clean files. This bounds the replay decision Y derives.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - below the noise at 8/250 (3.0)

**Tests watched failing first**, in a new `TheOliviaBelowTheNoiseTests` (engine, in
`CpuMeasuredAlone`):

- **3.0** The fixture made by decision W, on the stated seed: the detector names **8/250 within 5 Hz
  of 1000**, and the demodulator built from that detection reads the manifest QSO text at **CER
  0.10 or under**. Print the CER to four places, the SNR set and measured back, the blocks decoded
  and rejected, and the CPU against 2.5's twenty seconds.
- **Four further seeds**, printed and not asserted, with their CERs.
- Every character shown came from an accepted block (§3.3, §R9), asserted as unit 362's -16 dB test
  asserts it.

"Watched failing first" here means against a stub that returns no text, since the demodulator
exists; say so.

**If 3.0 misses**, the demodulator may be improved - unit 361 item 1's list (sync decided per block
from the neighbors' likelihoods, soft combining) and unit 362 item 2's per-character gate are the
candidates - **under decision V: unit 362's eight re-measured rows printed again beside their
numbers, a clean CER off 0.0000 or a noise character being a regression that comes out.** If it
still misses, ship the number, 3.0 `partial`, and name what the next unit tries. **Do not loosen
the 0.10, do not change the SNR, and do not pick a seed.**

Add `TheOliviaBelowTheNoiseTests` to the engine carry-forward line in this task's commit.

**Drop candidate:** none. It is a must-pass and it is the phase's headline claim.

### Task 3 - the Olivia listener (3.1, engine half)

Build `OliviaListener` under `src\Hamlet.RadioEngine\Olivia\`, to decisions Y, Z, AA, AB and AD.
Streaming changes inside `OliviaDemodulator`, `OliviaBlindSearch` or `RsidDetector` that the
listener needs are inside this task, **with `Decode` and `Search` keeping their current results**
- their existing tests are the guard, and they stay green unedited.

**Tests watched failing first**, in a new `TheOliviaListenerTests` (engine, in `CpuMeasuredAlone`),
every file hash-checked first and fed in quarter-second pieces:

- **3.1, engine half** `olivia-two-signals-rsid.wav` yields exactly two channels, as decision AB
  says: variants and centers, each text at 0.05 or under against its own half, no character of the
  other station in either.
- **Decision Z's regression rows**: the three clean files through the listener, one channel each at
  the manifest variant and center, CER 0.0000; the -10 dB file at 0.05 or under;
  `olivia-8-250-qso-norsid.wav` one channel found `blind`, 8/250 within 5 Hz of 1000, CER 0.05 or
  under; `olivia-noise-only-30s.wav` **no channel and no character**. Printed beside unit 362's
  whole-file numbers.
- **Decision AA**: on the no-RSID file and on an RSID file, one station is one channel - asserted
  by channel count across the whole feed, not only at the end.
- **§R13, decision AD**: the channel-open event and the state on the two-signal fixture, with
  `mode: olivia` and the variant; **no decoded text and no callsign in any of it**, asserted the way
  unit 362 asserted its search event.

Add `TheOliviaListenerTests` to the engine carry-forward line in this task's commit, run the list,
both invocations, **and say what it cost in seconds.**

**If the two-signal fixture gives one channel, three, or a channel with the other's text**, that is
the finding: report the detections, the channels opened and when, and the offending characters,
mark 3.1's engine half not met, and name what the next unit tries. **Do not tell the listener where
the stations are.**

**Drop candidate:** none. This is the unit's structure; step 3's rows stand on it.

### Task 4 - the real-time ratio (3.6)

**Only if task 3 is reported with its numbers.**

- The listener's CPU over the two-signal fixture's audio seconds, fed in quarter-second pieces, in
  `CpuMeasuredAlone`: **the ratio, reported against 1.0**, asserted under 1.0. The same ratio
  printed for the 131 s 16/500 file and the noise-only file.
- The per-piece worst case - the longest single `Add` - printed, because a listener that averages
  under 1.0 and stalls for four seconds once a block is not keeping up.

**3.6 is reported *measured on the engine*.** The next unit re-measures it with the rows drawn.

**Drop candidate: this whole task.** Drop it whole and say so; 3.6 is nice-to-pass. Do not drop it
half-built.

---

## 9. Parked - do not touch, do not raise

- **Rows on screen, `MainWindowViewModel`, the Olivia panel, the parser on Olivia rows, the CQ
  filter, worked-fade, `EntityOf`, the quill, the hover (3.2, 3.3), the retire window (3.4), the
  row telemetry (3.5)** - the next unit's, on this unit's listener. **Wire nothing into the app**
  (decision L); the panel keeps saying nothing decodes yet.
- **A detection that sets the tab, the mode, the variant or the dial** - R27's *sets the mode ...
  itself* is the app half, with the rows.
- **Compiling `olivia-fixture-generator.cpp`, installing a compiler, or writing a WAV into
  `assets\fixtures\`** (decision W).
- **Chasing the -16 dB 16/500 number, or the blind search below the noise** (unit 362 item 5). If
  task 2's changes move either, report the new number and move on.
- **An Olivia modulator, any Olivia send, the move-off-and-widen macro, the turn timing that
  undercounts Hamlet's own answer by the burst** (unit 359 item 4) - step 4.
- **Unit 362's section 4:** item 1 (2.3's wording) is logged - the plan's author's; item 2 is
  decision AC; item 3 (`TheOliviaDemodulatorTests` not in `CpuMeasuredAlone`) is logged - **if it
  turns a carry-forward run red, putting that class in the collection is inside this unit, one
  attribute, said in the report**; item 4 is decision Z; item 5 is parked above; items 6 and 7 are
  logged.
- **`longestSeconds` on the typed line's record, the refusal sentence quoting the whole audio, the
  card's *60 s of text* rounding** (unit 360 items 1, 2, 4); **`RsidDetection` giving the first
  tone** (unit 361 item 4); **codes 72 to 75 and the fldigi commit pin** (unit 359 item 3); **the
  dial 1500 Hz below the center** (unit 358 item 1, step 0 closed); **the flaky Stop tests,
  HM-OPEN-090, the three off-list reds, the screen phase's open asks.** Carried in section 4, not
  worked.
- **`PHASE_PLAN.md`'s unchecked 1.5, 1.7, 2.3 and 2.7, `PHASE_STATUS.md`'s stale unit number, and
  the mislabeled `PHASE_OUTCOME.md` entries.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not port `pj_mfsk.h`, `pj_fht.h` or `pj_gray.h`, and add no package.** Read the structure;
  write Hamlet's own. Anything that cannot be written without copying is `MOVE: stop` material in
  section 4, not built.
- **No literal tone count, spacing, symbol length, block length, rate, bandwidth, scrambling code,
  shift or mapping in code.** They come from `format.json` (decisions H and M); the replay and the
  *same place* width derive from them (decisions Y and AA).
- **Never give the listener, the search or the demodulator a variant, a center or a start time
  from the manifest**, in code or in a test (decisions I and O). The manifest checks; it never
  feeds.
- **Do not edit a fixture or `manifest.json`, and write no WAV anywhere under `assets\`**
  (decision W).
- **Never loosen the 0.10, the 0.05, the 5 Hz, the 1.0 ratio or the 20 s CPU ceiling, and never
  choose the noise seed after seeing its result.** A miss ships with its number (§6).
- **Do not edit an existing test to make a streaming change pass.** `Decode` and `Search` keep
  their results; if one of their tests goes red, the change is wrong.
- **Touch nothing on the transmit side**: not `Ft8TransmitSequence`, `UnslottedTransmission`,
  `Psk31Modulator`, `RsidBurst`, `PttOn` or any `Arm` site. `PttOn` 1 and `Arm(` 2 at the end, as
  at the start.
- **Do not wire anything into the app** (decision L).
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md`, `PHASE_PLAN.md` or earlier
  `PHASE_OUTCOME.md` entries.**
- **Report mismatches; repair nothing outside the task. Write American.**

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

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0,
   1 and 2 done and closed; step 3 was not started, none of seven met, at
   the start of this unit and is <state after this unit>; steps 4-6 not
   started, and step 4's entry opens only when step 3 is done.
B. Step 3's criteria this unit worked - 3.0 below the noise: SNR set -14,
   measured N dB; RSID read <variant at N Hz|not read>; CER N (0.10) on
   seed N, and N, N, N, N on the four others. 3.1 engine half: channels N
   (2), 8/250 at N Hz and 16/500 at N Hz (5 Hz), CERs N and N (0.05),
   characters of the other station N and N (0). 3.6: ratio N (1.0) on the
   engine, worst piece N s, or dropped. Met: <list by id>; engine half
   met: <3.1 or none>. Not worked, the next unit's: 3.2, 3.3, 3.4, 3.5,
   and the rows half of 3.1. Step 2's rows through the listener: clean
   CERs N / N / N (0.0000), -10 dB N, noise channels N (0).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of 3.0 or 3.1 - in particular
   whether 3.0 was met on every seed or only the stated one, whether a
   channel ever showed the other station's characters and what was built
   about it (decision AC), whether anything in pj_mfsk.h could not be
   written without copying (stop material), and whether the listener is
   ready for the next unit to draw rows from as it stands.
```

**Then the header:**

```
UNIT:       363 - <complete|stopped> at task N of 5, <task 4 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 3 criteria met 0 of 7 -> N of 7; stations read at once 1 -> N
DRIFT:      0
```

**Then a criterion table, 3.0 to 3.6** - 3.0, 3.1 and 3.6 with this unit's numbers and their
state (*met*, *engine half met*, *measured on the engine*, *not met*), and 3.2 to 3.5 marked *not
worked, the next unit's*.

**Section 3 leads with the 3.0 table**: the recipe (slice end, seed, SNR set and measured back,
the -10 dB file's measure beside it), the RSID reading, and for each of the five seeds the CER,
blocks decoded and rejected, and sync S/N, the asserted seed marked. Then the decoded text beside
the manifest text if it is not exact. **Then the listener table**: for the two-signal fixture, each
channel - id, how found, variant, center against the manifest, when it opened in audio seconds,
CER against its half, the other station's characters - and then decision Z's regression rows beside
unit 362's numbers. Then the listener's §R13 events as written, to show they carry no text or
callsign. Then the real-time table if task 4 was built. Then, if anything inside the demodulator
changed, decision V's eight rows re-measured.

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004).

---

```
ARBITER-DECISION
STEP: 3
APPROACH: Olivia listener - one streaming demodulator per carrier heard by RSID or found blind, channels per station as Psk31Listener gives them, and the -14 dB 8/250 fixture made in the test from the mode author's shipped audio with a seeded noise
MOVE: continue
WHY: Step 2 is done and closed, and step 3 is next in a one-way pipeline with its entry - the two-signal fixture's RSIDs - proved by step 1's 1.2. Its rows need an engine object that reads a stream and gives a channel per station, which does not exist, and its 3.0 is the phase's below-noise claim; the loop test finds neither in any entry.
STATE: not started
DECIDED: author's, overrulable - (W) 3.0's fixture is made in memory from the hash-checked shipped audio - the fldigi-port RSID burst sliced from olivia-8-250-cq-rsid.wav in front of the whole of olivia-8-250-qso-norsid.wav, which is Jalocha's generator's output for exactly this variant, center and text - with seeded white Gaussian noise at -14 dB in 2500 Hz measured back; the generator is not compiled, no WAV is written, the asserted seed is stated and four more are printed; overrule by saying compile the generator. (X) the variant comes from the made fixture's RSID and the 0.10 is not loosened. (Y) OliviaListener is an engine class shaped like Psk31Listener - samples in, one channel per station out, found by RSID or blind search, each with its own demodulator, text only as blocks are accepted, a replay derived from the block length. (Z) it reads a stream without re-reading the recording, gated by each shipped file through the listener reading what Decode reads. (AA) one station is one channel within half the narrower bandwidth, a different variant announced at an occupied place ends the old channel, and no channel retires in this unit. (AB) 3.1 is proved at the engine as two channels with their own text at 0.05 and no callsign of the other, and reported engine half met, not met, until the next unit draws the rows. (AC) unit 362 item 2's per-character gate is built only if a channel shows another station's characters. (AD) the listener writes a channel-open event and states with mode olivia and no text or callsign. Task 4 (3.6) is the drop candidate; tasks 0 to 3 have none. Unit 362's section 4 asked for no ruling; items 2 and 4 are taken as decisions AC and Z, item 3 only if it turns a run red, the rest logged.
LICENCE: PHASE_PLAN.md step 3 criteria 3.0, 3.1 and 3.6, R27, R28, R30, R31, section 3.2, section 3.3, section 8 (the revision of 2026-09-19 adding 3.0) and section 6 (a number or a mechanism is the arbiter's; a package is needed; the port clause; never loosen a test); PSK31 plan R5, R9, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155; ARBITER.md sections 2 and 6
ACCOMPLISHED: Hamlet reads Olivia from under the noise - an 8/250 QSO a listener cannot hear, read to its text - and hears two Olivia stations at once as it hears PSK31 stations, each on its own line of text as it arrives and nothing of one in the other, which is the engine every Olivia row on the screen will stand on
ADVANCES: step 3 criterion 3.0 (task 2, must-pass); the engine half of criterion 3.1 (task 3, must-pass - the rows are the next unit's); criterion 3.6 measured on the engine (task 4, nice-to-pass)
END-ARBITER-DECISION
```
