# Work instruction 361 - hear one: Hamlet's own Olivia demodulator, read on the mode author's clean audio and below the noise

**Step 2 of `PHASE_PLAN.md`, authored by the arbiter.** Steps 0 and 1 are done and closed: the
separate reading of unit 360's report returned `done` on step 1, every clause of 1.5 measured.
Step 2 has not started, and its entry is open (unit 360 measured the clean 16/500 fixture's RSID
at 1000.32 Hz, code 70). **This unit builds the demodulator for a named variant at a named
offset and proves it on the three clean fixtures, the noise-only fixture and the two below-noise
fixtures. Six tasks, 0 to 5; task 5 is the drop candidate.** The blind search (2.3) and the
drift fixture (2.7) are the next unit's, and parked here.

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

The arbiter checked all four on 2026-09-18 after 22:14, and they held.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: two invocations, one build each, with a status write immediately
before each. Never background and poll. **A fixture decode that runs long is a finding, not a
reason to run in the background:** each clean fixture must decode in under twenty seconds of
CPU (2.5), so a test that takes minutes is itself the number to report.

## 2. The tool facts, as units 359 and 360 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- `;` is refused. `rm` is refused. A `for` loop over `$f` is refused (*simple_expansion*).
- `mkdir`, `cp`, `powershell.exe`, `python -c`, `jq`, `awk`, `git restore --source`,
  `git checkout <rev> -- <file>` and `git stash push` needed approval, which a headless session
  cannot give. Write files with the editor.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. Run directly
  (not through `sh`) inside an `&&` chain, it needed approval.
- `git show ... > file` is blocked. `sed -n` in a pipe after `git show` ran;
  `tail -n +N file | md5sum` ran.
- Anything outside `C:\Source\HamLet` - including `C:\Source\fldigi` - cannot be listed or read.
  **Jalocha's headers are inside the root** at `assets\reference\jalocha\`, which is all this
  unit needs.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  since warnings are errors. Anchor on the comment's first line.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `WORKING` and `claude`
  are not allowed words. `tools/status.sh` writes `RULES_AT: HM-DEC-161` and reads
  `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 360's `## 4. What's blocking us` verbatim, from its first
line to its end** (`dcffd02c:output.md`), its nested queues and the reference to
`4c55deac:output.md` included, the way unit 360 carried unit 359's. If `output.md` is deleted
in the working tree (the launcher's doing), read the committed copy.

**Mark one item in place, and delete nothing:**
- unit 358 item 5 (`timing.json` not parsed by a machine), inside the carried queue:
  *ANSWERED by work instruction 360 task 4 - parsed by System.Text.Json, agrees with the
  manifest except 8/250 no-RSID per-character rounded up (0.686 against 0.685).*

This unit answers none of the others.

---

## 4. Why this unit exists

**Step 2 stands at zero of seven criteria, and nothing in Hamlet reads Olivia text today.** The
step 1 detector names the variant and the center from the burst; nothing hears what follows it.
Everything after this in the phase - rows, the parser, sending, logging, Tim at the radio -
stands on a demodulator that reads the mode author's own audio.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own RSID and never
            picked by the operator.
UNIT GOAL:  Hamlet's own OliviaDemodulator, given the variant and the center the RSID
            detector read, turns the mode author's clean 8/250, 16/500 and 32/1000
            audio into its text at CER 0.01 or under, the -10 and -16 dB files at 0.05
            and 0.10, and pure noise into nothing - each in under 20 s of CPU.
ADVANCES:   step 2 criteria 2.1, 2.4 and 2.5 (task 3), 2.2 (task 4), 2.6 (task 5).
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full**, §3.3 and step 2 above all. Step 2's reference is
`pj_mfsk.h`, read for structure; the code is Hamlet's (PSK31 plan §R5, below).

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even when
the work succeeded.

What this instruction believes, from the reload of 2026-09-18 22:13 and the arbiter's own
reading after it:

- **HEAD is `dcffd02c`** (`docs(unit360): the report ...`). Version 1.13.47 in
  `Directory.Build.props`.
- **The fixtures** are nine files under `assets\fixtures\olivia\` with `manifest.json`. Each
  manifest entry carries `file`, `sha256`, `seconds`, `variant`, `center_hz` (1000 for the
  single-signal files), `rsid`, `note` and `text`. The texts use `\n` between lines. The files
  for this unit: `olivia-8-250-cq-rsid.wav` (a CQ, 38 characters, 28.98 s),
  `olivia-16-500-qso-rsid.wav` (131.38 s), `olivia-32-1000-qso-rsid.wav` (106.8 s),
  `olivia-16-500-qso-snr-10db.wav`, `olivia-16-500-qso-snr-16db.wav` and
  `olivia-noise-only-30s.wav`.
- **`assets\reference\jalocha\`** holds `pj_mfsk.h` (2367 lines), `pj_fht.h`, `pj_gray.h`,
  `pj_fft.h`, `pj_cmpx.h`, `pj_fifo.h`, `pj_lowpass3.h` and `pj_struc.h`. Unit 360's structure
  map (its section 1, task 4) names the receiver's parts: `MFSK_Demodulator` `:702`-`:1047`,
  `MFSK_Encoder` `:1058`-`:1218` (scrambling code `:1076`, shift 13 per character for Olivia),
  `MFSK_SoftDecoder` `:1219`-`:1431`, `MFSK_Receiver` `:1929`-`:2367`.
- **`src\Hamlet.RadioEngine\Olivia\`** holds `OliviaCallingTable.cs` and `OliviaData.cs` and no
  demodulator. `OliviaData.Current` reads the embedded data files once. The RSID detector lives
  under `src\Hamlet.RadioEngine\Rsid\`.
- **`data\olivia\timing.json`** says `source: estimated` and has a row per variant; unit 360
  parsed it by machine and found it agrees with the manifest (8/250 no-RSID rounded up, 0.686
  against 0.685).
- **`assets\fixtures\captured\`** exists and holds only `README.md`, which is about PSK31. **No
  real Olivia audio is in the tree**, so §6's *real off-air audio appears* branch does not fire.
- **Carry-forward** at the end of unit 360: engine 111 of 111; app 165 of 166 in the combined
  run, the red passing alone on the first rerun.
- `CivConstants.PttOn` code lines: 1. `_armedSend.Arm(` lines: 2.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload reads *CPS-DEC-0164* as the highest ruling in `CLAUDE.md` §1; the table's highest
  row is HM-DEC-164. `PROJECT_STATUS.md` `RULES_AT` still says HM-DEC-161.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`; the
  tree has `data/rsid/` and `data/bands/`.
- `PHASE_PLAN.md` step 1 shows 1.5 and 1.7 unchecked; the outcome record has step 1 `done`. Report
  it; do not edit the plan.
- `PHASE_OUTCOME.md`'s harness entries are headed `UNIT 1` and `UNIT 2`; `UNIT 2 - STEP 1` reads
  `FATE: executed` for a run that never happened. Append-only; do not edit.
- `PHASE_STATUS.md` still says `WORK_INSTRUCTION: 358`. The launcher's file.
- Uncommitted at authoring: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, files under `.run-unit\`,
  `SESSION.lock` (untracked), and this file.
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` (two `Arm(` lines),
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- The app carry-forward list is flaky run to run (`TheStopIsAlwaysOnScreenTests` and
  `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning` most of all).
  Rerun a red that passes alone up to three times, and say which run the number came from.
  **This unit changes no app code, so an app red is a flake or older than this unit** - say which.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what they
rejected.**

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode transmission
Hamlet sends begins with the RSID burst naming its mode and variant - Olivia, and PSK31
retroactively. Hamlet listens for RSID across the passband and when one arrives sets the mode,
the variant and the offset itself. The operator never picks a variant. A carrier that never
announced itself gets the blind search of step 2 as a fallback. The tables and codes are
`data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R30 - Synthetic fixtures first, the capture button from step 0.** *Tim has no
time to generate fldigi audio. So the fixtures under `assets/fixtures/olivia/` were made by the
web thread from **the mode author's own transmitter** - Pawel Jalocha's `pj_mfsk.h` as shipped
in fldigi, compiled and driven exactly as fldigi drives it - with RSID bursts from fldigi's own
encoder. They are independent of anything Hamlet thinks Olivia is, which is the lesson of the
PSK31 phase. `assets/reference/SOURCE.md` says how. The Capture button is on the Olivia panel
from step 0 so the first evening's air becomes the fixture for the next unit.*

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md §3.3 - The error correction.** *Olivia's decoder either has a block or does not;
there are no garbled letters, only missing ones. R9 - a character not sure of is not shown - is
the mode's own behavior.*

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past
  the budget; a decision that changes what the product promises the operator - a fact stated
  about the radio, a contact or a send. A hint, a label, a number, a layout, a mechanism
  arithmetic will not allow: the arbiter decides, marks it author's and overrulable, and
  continues.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on.
  Never loosen a test.*
- *A done step is closed. Only Tim reopens it.*
- *A fixture will not decode at all. That is a finding about the demodulator, not the fixture -
  the fixtures are the mode author's. Report the spectrum measured against the manifest, mark
  `partial`, and name what the next unit tries.*
- *Reading `pj_mfsk.h` tempts a port. Read the structure; write Hamlet's own. If a unit cannot
  proceed without copying, `MOVE: stop` and say what it would copy.*
- *A package is needed. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R5 - The reference implementation.** *`fldigi`, which is GPL-3 - the same licence
as Hamlet. Cloned outside the tree at `C:\Source\fldigi`, pinned to one commit that the step-1
unit records in its report, never committed, never ported wholesale - the same rule the FT8
phase set for `ft8_lib`. What is read from it: the varicode table (which is a published
standard and may be transcribed with its source cited), the raised-cosine shaping and the
demodulator structure. What is written for Hamlet is Hamlet's.*

**PSK31 plan §R9 - Squelch and the honest character.** *A character the demodulator was not
sure of is not shown - no `?`, no dimmed maybe. Silence.*

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
transmits on a timer, on a decode, or because a contact "should" continue.* This unit adds a
receiver and touches nothing that keys.

**FACT-004** *There are two computers, and only one of them has a radio on it.* **FACT-006** *The
development machine has no contact log and never will.* Nothing measured here is evidence about
the radio.

**HM-DEC-139**, **HM-DEC-155**, as in sections 1 and 3.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings; they are in the
decision block at the end.

- **H. The format's constants are cited data; the algorithm is Hamlet's.** Unit 360 found that
  no *algorithm* in `pj_mfsk.h` needs copying, and that four things must match bit for bit
  because they *are* the format: the 64-bit Olivia scrambling code, the per-character shift of
  13, the character-to-Walsh-index mapping, and the Gray code; plus each variant's parameters
  (tones, bandwidth, bits per symbol, symbols per block). They go in one cited data file,
  **`data\olivia\format.json`**, each value with its `pj_mfsk.h` line citation and the
  `SOURCE.md` pin, read through `OliviaData` at startup the way `rsid-codes.json` is, a
  malformed file reported and not guessed. **This is not a port under §6**: it is the pattern
  R27 already set for the RSID tables and §R5 set for the varicode table - facts of a published
  format, transcribed with their source. Tone detection, sync, de-interleave, descrambling and
  the FHT correlation are written as Hamlet's own. **If the unit finds a part it cannot write
  without copying code**, it does not build it: it names the lines in section 4 as `MOVE: stop`
  material and goes on with what it can.
- **I. The variant and the center come from the RSID, not from the test.** Each fixture test
  runs the step 1 detector first and hands its variant and center to the demodulator; the
  demodulator's own interface takes a named variant and a named offset, as the plan words it.
  The manifest's `variant` and `center_hz` are used only to check the detection. Decoding
  begins after the burst.
- **J. CER is stated once and never loosened.** CER = Levenshtein edit distance between the
  decoded text and the manifest `text`, divided by the manifest text's length. Before comparing,
  CR LF and lone CR become LF in both; case is compared exactly; nothing else is normalized.
  Idle and null characters the mode sends between messages are not text and are not shown
  (§R9). Characters decoded before sync are counted as insertions, not trimmed. The helper that
  computes it lives once, in the test project, and every fixture test uses it.
- **K. Telemetry, §R13.** The demodulator writes an event per stage it adds - sync found or lost
  (variant, center, frequency offset found, block phase, S/N measure), and a per-run summary
  (blocks decoded, blocks rejected, characters out) - with `mode: olivia` and the variant, in the
  category the PSK31 receive events use. **No decoded text and no callsign in any event.**
  Proved by assertion against one clean fixture.
- **L. The engine only.** The demodulator is built and proved in `Hamlet.RadioEngine` and its
  tests. **It is not wired into the Olivia panel in this unit**; the panel keeps saying nothing
  decodes yet, and criterion 0.5's *under Olivia no other mode's decoder runs* holds. Rows per
  station are step 3.
- **M. The rate is stated.** The demodulator runs at the rate Hamlet's receive path hands the
  RSID detector, reached by Hamlet's own resampler; the unit states the rate and the samples per
  symbol for each variant in its trace, and no rate, tone spacing or symbol length is a literal
  in code - they derive from `format.json`.
- **N. The timing table (2.6) is measured by the demodulator.** Seconds per character for each
  variant is measured from the demodulator's own block times across each clean fixture (first
  block to last, over characters decoded), compared to the manifest arithmetic, and written into
  `data\olivia\timing.json` with `source` changed from `estimated` to `measured` and the method
  stated. The 8/250 row is measured on the clean CQ file; the no-RSID file's figure is reported
  beside it, not used, because it is the blind search's.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_STATUS.md` has steps 0 and 1 `done`.
- **Step 2's entry, first:** hash `olivia-16-500-qso-rsid.wav` against the manifest, then run
  the RSID detector over it and report the detection (unit 360: code 70 at 1000.32 Hz). Then
  hash the other eight, 9 of 9. If any fails, stop.
- Append `UNIT 361 - STEP 2` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 360`
  entry. Touch no earlier entry.
- Patch-bump the version by one (unit 360 left 1.13.47).
- Run the carry-forward list, both invocations, before any change.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do
  not commit** `SESSION.lock`, `RUN_LEDGER.md`, anything under `.run-unit\`, or the deletion of
  `output.md`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line. A trace class that asserts nothing, in the shape of `Unit360Trace`.

1. **For each of 8/250, 16/500 and 32/1000, from `pj_mfsk.h` and the fixtures**: tones, tone
   spacing, symbol length in seconds, bits per symbol, symbols per block, characters per block,
   seconds per block, and samples per symbol at the rate decision M names. Where the header and
   the audio disagree, the audio wins and the disagreement is reported.
2. **The spectrum of each clean fixture after its burst**, measured: occupied band, lowest and
   highest tone against `center_hz` and the variant's bandwidth. This is the ground §6's *will
   not decode at all* branch reports against.
3. **The four format constants of decision H**, each with its line in `pj_mfsk.h` and the line
   in `SOURCE.md` that pins the header.
4. **Where the demodulator plugs in:** the RSID detector's result type and what it gives
   (variant, center, time of the burst's end), `OliviaData`'s loading and error path, the
   resampler the RSID path uses, and the telemetry category the PSK31 receive events use.
5. **The CPU of the RSID detector alone** over each fixture, so task 3's 20-second figure has a
   floor to be read against.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the format as cited data

Build decision H's data file and its reading. No demodulator yet.

**Test watched failing first**, in a new `TheOliviaDemodulatorTests` (engine) or the existing
`OliviaData` tests, whichever already reads the data files:
- `format.json` is read at startup and gives each variant's parameters and the four constants.
- A malformed file is reported in words, not guessed - the same shape as `rsid-codes.json`'s.

**Drop candidate:** none.

### Task 3 - hear one: the clean fixtures and the noise (2.1, 2.4, 2.5)

Build `OliviaDemodulator` under `src\Hamlet.RadioEngine\Olivia\`: tone detection per symbol,
block phase and frequency-offset sync, de-interleave, descramble, the Walsh-function correlation
decode, characters out only from blocks the decoder is sure of (§3.3, §R9). Decisions I, J, K, L
and M.

**Tests watched failing first**, in `TheOliviaDemodulatorTests` (engine):
- **2.1** Each of `olivia-8-250-cq-rsid.wav`, `olivia-16-500-qso-rsid.wav` and
  `olivia-32-1000-qso-rsid.wav`, variant and center from the detector, decodes to its manifest
  text at **CER 0.01 or under**. Report each CER to four places and the decoded text beside the
  manifest's for any file that is not exact.
- **2.4** `olivia-noise-only-30s.wav`, run at each of the three variants at 1000 Hz, emits
  **zero characters**.
- **2.5** Each file's hash matches the manifest before it is used; each decodes in **under 20 s
  of CPU**, measured as process CPU and reported per file.
- **§R13** The events of decision K fire on the clean 16/500 file, and none carries text or a
  callsign.

Then run the carry-forward list, both invocations, with `TheOliviaDemodulatorTests` added to the
engine line of `docs\carry-forward-tests.txt` in this task's commit - say so.

**If a clean fixture will not decode at all**, that is §6's branch: report the spectrum from
task 1 item 2 against the manifest, mark the criterion not met, name what the next unit tries,
and go on to task 4 only if at least one clean fixture decodes. **If a CER misses 0.01 by a
little**, ship, report the number, and do not loosen the test.

**Drop candidate:** none.

### Task 4 - below the noise (2.2)

**Only if task 3 decodes the clean 16/500 file.**

**Tests watched failing first**, same class:
- `olivia-16-500-qso-snr-10db.wav` decodes at **CER 0.05 or under**.
- `olivia-16-500-qso-snr-16db.wav` decodes at **CER 0.10 or under**.
- Both variant and center from the detector (1.3 already proved the -16 dB burst is heard);
  both under 20 s of CPU, reported.

Tuning the sync threshold or integration for these is the unit's; **a number chosen here is
the author's and is stated with the file it was chosen on**. Show that the clean and noise-only
results of task 3 did not move after any tuning.

**Drop candidate:** none. If it cannot be met, it ships with its numbers and 2.2 is reported not
met.

### Task 5 - the timing table (2.6)

**Only if tasks 3 and 4 are green, or task 4 is reported with its numbers.** Decision N.

**Test watched failing first:** `timing.json` reads `source: measured` and each variant's seconds
per character is within 2% of the figure the demodulator measures on its clean fixture, and the
table is what `OliviaData` hands out. Report the three measured figures beside the manifest
arithmetic and the file's old estimates.

**Drop candidate: this whole task.** Drop it whole and say so; 2.6 is then the next unit's with
the blind search.

---

## 9. Parked - do not touch, do not raise

- **The blind variant search (2.3) and the drift fixture (2.7)** - the next unit, on this
  unit's demodulator.
- **Wiring the demodulator into the Olivia panel, rows per station, a detection that sets the
  mode, variant or offset** - step 3.
- **An Olivia modulator, any Olivia send, the turn timing that undercounts Hamlet's own answer
  by the burst** (unit 359 item 4) - step 4.
- **`longestSeconds` on the typed line's record, the refusal sentence quoting the whole audio,
  the card's *60 s of text* rounding** (unit 360 items 1, 2, 4) - transmit-side findings, carried
  in section 4, not worked.
- **Codes 72 to 75 and the fldigi commit pin** (unit 359 item 3). The clone is outside the root;
  `SOURCE.md`'s pin is what the data file cites.
- **The dial 1500 Hz below the center** (unit 358 item 1). Step 0 is closed.
- **The flaky Stop tests, HM-OPEN-090, the three off-list reds, the screen phase's open asks.**
  Carried in section 4, not worked.
- **`PHASE_PLAN.md`'s unchecked 1.5 and 1.7 and the mislabeled outcome entries.** Reported, not
  edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not port `pj_mfsk.h`, `pj_fht.h` or `pj_gray.h`, and add no package.** Read the structure;
  write Hamlet's own; the constants go in `format.json` with citations (decision H). Anything
  else that cannot be written without copying is `MOVE: stop` material in section 4, not built.
- **No literal tone count, spacing, symbol length, rate, scrambling code, shift or mapping in
  code.** They come from `format.json` (decisions H and M).
- **Touch nothing on the transmit side**: not `Ft8TransmitSequence`, `UnslottedTransmission`,
  `Psk31Modulator`, `RsidBurst`, `PttOn` or any `Arm` site. `PttOn` 1 and `Arm(` 2 at the end,
  as at the start.
- **Do not wire the demodulator into the app** (decision L).
- **Never loosen a CER ceiling or a CPU ceiling**, and never edit a fixture or the manifest.
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
task 5 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   and 1 done and closed; step 2 <state after this unit>; steps 3-6 not
   started, step 3's entry not open until 2.3 is met.
B. Step 2's criteria - 2.1 clean CER 8/250 N, 16/500 N, 32/1000 N
   (ceiling 0.01, variant from RSID yes|no); 2.2 -10 dB N (0.05),
   -16 dB N (0.10); 2.3 not attempted (parked); 2.4 noise-only characters
   N (0); 2.5 hashes 9 of 9, CPU per file N s (ceiling 20); 2.6 measured
   yes|no|dropped; 2.7 not attempted (parked). Met: <list by id>.
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of a criterion in B - in particular
   whether anything in pj_mfsk.h could not be written without copying
   (stop material), and whether any fixture would not decode at all.
```

**Then the header:**

```
UNIT:       361 - <complete|stopped> at task N of 6, <task 5 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     Olivia fixtures decoded at or under their ceiling 0 -> N of 5 (three clean, two below the noise); noise-only characters N
DRIFT:      0
```

**Then a criterion table, 2.1 to 2.7**, each with this unit's number or *parked*.

**Section 3 leads with the decode table**: per fixture, the variant and center the detector gave,
CER to four places against its ceiling, characters decoded against the manifest's, blocks decoded
and rejected, CPU seconds, met or not. Then each decoded text beside its manifest text for any
fixture that is not exact. Then one decision-K event as written, to show it carries no text or
callsign. Then the timing table before and after, if task 5 was built.

**Every appearance claim is computed, not seen. Nothing here is evidence about the radio**
(FACT-004).

---

```
ARBITER-DECISION
STEP: 2
APPROACH: Hamlet's own Olivia demodulator for a named variant and offset from the RSID detection - FFT tone detection, block sync, de-interleave, descramble, Walsh correlation decode - with the format constants as cited data, proved on the clean, noise-only and below-noise fixtures
MOVE: continue
WHY: Step 1 is done and closed and step 2 is next in a one-way pipeline; its entry was measured open by unit 360, the loop test finds no Olivia demodulator in any entry, and unit 360 read pj_mfsk.h and found no algorithm that must be copied.
STATE: not started
DECIDED: author's, overrulable - (H) the scrambling code, the shift of 13, the character-to-Walsh mapping, the Gray code and each variant's parameters go in data/olivia/format.json with pj_mfsk.h line citations, read through OliviaData, which is transcription of a format's facts as R27 and PSK31 R5 did, not a port; anything else that cannot be written without copying is stop material, not built. (I) variant and center come from the step 1 detector, never from the test. (J) CER is Levenshtein over the manifest text's length, line endings unified, case exact, pre-sync characters counted. (K) sync and per-run summary events with mode olivia and the variant, no text, no callsign. (L) engine only, the panel is not wired. (M) rate and samples per symbol derive from format.json. (N) timing.json becomes measured from the demodulator's block times. Task 5 (2.6) is the drop candidate; 2.3 blind search and 2.7 drift are parked to the next unit.
LICENCE: PHASE_PLAN.md step 2, R27, R30, R31, section 3.3 and section 6 (a number or a mechanism is the arbiter's; a fixture that will not decode; the port clause); PSK31 plan R5, R9, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155; ARBITER.md section 6
ACCOMPLISHED: Hamlet reads Olivia text off the mode author's own audio at all three variants, in the clear and below the noise, with the variant taken from the signal's announcement, and hears nothing in pure noise - the receiver every later step stands on
ADVANCES: step 2 criteria 2.1, 2.4 and 2.5 (task 3), 2.2 (task 4) and 2.6 (task 5)
END-ARBITER-DECISION
```
