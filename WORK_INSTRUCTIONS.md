# Work instruction 364 - hear everyone and read: Olivia rows on the screen

**Step 3 of `PHASE_PLAN.md`, authored by the arbiter.** Steps 0, 1 and 2 are done and closed.
**Step 3 is `partial`**: unit 363 met **3.0** (8/250 at -14 dB, CER 0.0000 on five seeds), met **the
engine half of 3.1** (`OliviaListener` gives the two-signal fixture as two channels, each its own
text at 0.0000, nothing of one in the other), and measured **3.6** on the engine at 0.154. The state
session read it `partial`: *3.1 has only its engine half with no rows drawn, and 3.2, 3.3, 3.4 and
3.5 have not been worked.* **This unit takes all of what is left**: the listener wired into the app
under the Olivia tab and drawn as rows through the PSK31 row path (the rows half of 3.1), the row
telemetry (3.5), the parser and the row features on Olivia rows with no change to their code (3.2,
3.3), the retire window (3.4), and 3.6 re-measured with the rows drawn. **Six tasks, 0 to 5; task 5
is the drop candidate.**

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

**The engine invocation now takes 4 m 40 s for 134 tests** (unit 363), inside the 480 s timeout
with about three minutes to spare. **That margin is this unit's to keep.** Decision AL takes the
one name on the line that asserts nothing off it; every engine class this unit adds that feeds a
whole fixture through the listener costs about as much as `TheOliviaListenerTests` did. Report the
seconds of both invocations each time they run. **If the engine invocation would pass 400 s, say
so before adding a name and put the new class on the app line or run it by name beside the list,
stated** - never raise the timeout and never background it.

**CPU is measured alone.** Every class this unit adds that asserts or reports CPU goes in the
non-parallel `CpuMeasuredAlone` collection.

## 2. The tool facts, as units 359 to 363 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. Put multi-line edits in
  script files or use the editor.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing (unit
  361). Use the editor.
- A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`), since
  warnings are errors. Anchor on the comment's first line.
- **`sed -i` on `output.md` and `cat >> docs\carry-forward-tests.txt` were refused as "outside the
  allowed working directories"** though both are in the root (unit 363). Edit both with the file
  editor.
- `rm` is refused. A `for` loop over `$f` is refused. A command joined with `;` ran for unit 362;
  do not depend on it.
- **Python is unreliable** - it ran for unit 361 and needed approval for unit 362. Do not build a
  task on it.
- `mkdir`, `cp`, `mv`, `tee`, `powershell.exe`, `jq`, `awk`, `git restore --source`,
  `git checkout <rev> -- <file>`, `git stash push`, `git check-ignore`, command substitution, and
  `grep -v` in a pipe after `dotnet test` needed approval, which a headless session cannot give. A
  command that included one was refused whole (unit 363). `grep -E` and `tail` after `dotnet test`
  ran. **Do not filter the carry-forward output so hard that the summary line is lost** - unit 363
  ran the engine line twice for that reason.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran.
- `git show ... > file` is blocked as redirection. `sed -n N,Mp` in a pipe after `git show` ran;
  `tail -n +N file | md5sum` ran.
- Anything outside `C:\Source\HamLet` cannot be listed or read.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `WORKING` and `claude` are
  not allowed words. `tools/status.sh` writes `RULES_AT: HM-DEC-161` and reads `WORK_INSTRUCTION`
  from `PHASE_STATUS.md`, which still says 358.

## 3. Asks still outstanding

Carried per HM-DEC-139. **Carry unit 363's `## 4. What's blocking us` verbatim, from its first line
to its end**, its nested queues and the reference to `4c55deac:output.md` included. Unit 363 did it
without retyping: it removed the old report's first sections in place with the file editor, so the
carried text is the original bytes. Do the same, and check the carried copy by `md5sum` against
`output.md` at `75571f5d` (unit 363's report commit).

**Mark nothing in place.** This instruction answers none of the carried items; section 9 says
which of unit 363's nine it takes up and how.

---

## 4. Why this unit exists

**Hamlet's engine reads two Olivia stations at once, from under the noise, and the operator sees
none of it.** The Olivia panel still says nothing decodes yet. Step 3 is where Olivia starts to look
like PSK31 to the operator - R28, *identical to PSK31 above the modem*: a row per station, the
variant on the row, the same parser reading it, the same CQ filter, worked-fade, entity, quill and
hover, and rows that stay after the station ends. Every one of those already exists for PSK31 rows.
**This unit's job is to put Olivia channels into that path without forking it, and to prove the
path did not change to take them.**

Step 4 - Olivia's own send - opens only when step 3 is `done`. Of step 3's seven criteria, 3.1's
rows half, 3.2, 3.3, 3.4 and 3.5 are the five must-passes still open. All five are in this unit.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal itself and never picked by
            the operator.
UNIT GOAL:  Put Olivia on the screen as PSK31 is on it: the engine's Olivia listener fed
            under the Olivia tab, a row per station through the PSK31 row path with the
            variant on the row, the PSK31 parser and the row features reading Olivia rows
            with no change to their code, rows that retire on a window scaled by the
            variant's timing table and stay listed as ended, and the row telemetry with
            mode olivia - with nothing new able to reach a send.
ADVANCES:   step 3 criteria 3.1 rows half (task 2, must-pass), 3.5 (task 2, must-pass),
            3.2 and 3.3 (task 3, must-pass), 3.4 (task 4, must-pass); 3.6 re-measured
            with the rows drawn (task 5, nice-to-pass). If all five are met, step 3 is
            done and step 4's entry opens.
DRIFT:      0
```

**Read `PHASE_PLAN.md` at the root in full** - step 3, R27, R28, §3.2 (every timing rule scales
with the variant) and step 0's 0.5 above all.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 12:44 and the arbiter's own reading
after it:

- **HEAD is `eb714fa4`** (`chore(unit363): status - task 4 of 5 ...`), the report at `75571f5d`.
  Version **1.13.50** in `Directory.Build.props`.
- **`output.md` is unit 363's report**, committed.
- **`PHASE_STATUS.md` says steps 0, 1 and 2 `done`, step 3 `partial`, `CURRENT_STEP: 3`**, and
  still `WORK_INSTRUCTION: 358`.
- **`PHASE_OUTCOME.md` ends with a `UNIT 2 - STEP 3` entry** carrying unit 363's approach, `FATE:
  executed` and `STATE_AFTER: partial`. Append-only.
- **`src\Hamlet.RadioEngine\Olivia\`** holds `OliviaBlindSearch.cs`, `OliviaCallingTable.cs`,
  `OliviaData.cs`, `OliviaDemodulator.cs`, `OliviaFormat.cs`, `OliviaListener.cs`,
  `OliviaSearchStream.cs`, `OliviaStream.cs`, `OliviaTiming.cs`. `OliviaListener` has `Add(samples)`,
  `Flush()`, `Channels` as `OliviaChannel(Id, Variant, CenterHz, Found, OpenedSeconds, Text,
  BlocksDecoded, BlocksRejected, Ended)`, `States` as `OliviaChannelState(...)`, `SamplesSeen`,
  `SampleRate`, `ReplaySeconds` (6.144 s, derived), and writes its own `olivia_channel` and
  `olivia_block` events in category `Psk31`. **Nothing retires.** It is wired into nothing.
- **The PSK31 row path** (unit 363's trace, lines as at `60ec790a`): `MainWindowViewModel` makes
  `_psk31 = new Psk31Listener(Psk31Resampler.TargetSampleRate)` at :2896, feeds it at :2958 from
  the audio tap through the resampler, and draws `Channels` through `ShowPsk31Channels` at :3500 /
  :3524, with `ShowPsk31ChannelsForTests` at :3509. The app writes the row events from `States` at
  :3179, :3444 and :3975 and drains `Watch` at :3386-3422. PSK31 retires a channel after
  `Psk31CarrierSearch.RetireAfterPasses` passes, 1.02 s (`Psk31Listener.cs:82`, :218).
- **The transcript corpus** is `assets\fixtures\psk31-transcripts\corpus.json`, read by
  `tests\Hamlet.RadioEngine.Tests\Psk31\Psk31Corpus.cs`, `ThePsk31ExchangeParserTests` and the app's
  `ThePsk31TelemetryTests`. The parser is `src\Hamlet.RadioEngine\Psk31\Psk31ExchangeParser.cs`.
- **The timing table** is `data\olivia\timing.json`, measured by the demodulator (unit 361): about
  0.685 s a character at 8/250.
- **Carry-forward at the end of unit 363:** engine 134 of 134 in 4 m 40 s, app 165 of 166 (the
  flaky Stop test, green alone on its second rerun). `TheOliviaSeamTests` is on the app line;
  `TheOliviaBelowTheNoiseTests` and `TheOliviaListenerTests` are on the engine line by class;
  `TheOliviaListenerKeepsUpTests` is not on it.
- `CivConstants.PttOn` code lines: **1** (`Ft8TransmitSequence.cs:513`). `_armedSend.Arm(` lines:
  **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree**, so
  §6's *real off-air audio appears* branch does not fire.

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- `PHASE_PLAN.md` shows **1.5, 1.7, 2.3, 2.7 and 3.0 unchecked**, though the record has them met.
  **Do not edit `PHASE_PLAN.md`.**
- The reload's disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-161 (2026-09-11);
  `CLAUDE.md` §1 holds CPS-DEC-0164.
- `PHASE_STATUS.md` says `WORK_INSTRUCTION: 358`. It is the launcher's file; commit it as the
  launcher leaves it.
- `PHASE_OUTCOME.md`'s harness entries are headed `UNIT 1` and `UNIT 2`, and `UNIT 2 - STEP 1`
  reads `FATE: executed` for a run that never happened.
- `PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`; the
  tree has `data/rsid/` and `data/bands/`.
- Red and not on the carry-forward list:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` is
  flaky. Rerun a red that passes alone up to three times, and say which run the number came from.
  **This unit does change app code**, so an app red is not presumed a flake: say what it touches.
- `.unit362-carry.tmp` sits in the root, ignored by git.
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

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md §3.2 - Speed.** *Every timing rule - the send cap, the turn indicator's patience,
the retire window - scales with the variant's seconds per character, taken from the mode author's
audio ... and never fixed in seconds.*

**PHASE_PLAN.md §3.3 - The error correction.** *Olivia's decoder either has a block or does not;
there are no garbled letters, only missing ones. R9 - a character not sure of is not shown - is
the mode's own behavior.*

**PHASE_PLAN.md step 0, criterion 0.5 (done, closed, and still binding).** *Under Olivia no other
mode's decoder runs and no path reaches the send chain.* **Drawing rows must not open one.**

**PHASE_PLAN.md step 3, the criteria this unit works, verbatim:**
- *3.1 The two-signal fixture yields two rows, each with its variant and its own text at or under
  0.05, nothing of one in the other.*
- *3.2 The transcript corpus fed through Olivia rows yields the same verdicts as through PSK31
  rows - no parser change.*
- *3.3 The CQ filter, worked-fade, `EntityOf` with its `CQ` guard, the quill and the hover run on
  Olivia rows with no change to their code.*
- *3.4 A row is retired when its signal goes and stays on the list marked ended; the retire window
  is the variant's timing table times a stated factor.*
- *3.5 Telemetry: the PSK31 row events with `mode: olivia` and the variant; nothing personal.*
- *3.6 Real-time ratio on the two-signal fixture under 1.0, reported.* (nice-to-pass)

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
- *A package is needed. `MOVE: stop`.*
- *Anything touches the transmit chain beyond adding an audio generator and an RSID prefix behind
  the one sequence. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R9 - Squelch and the honest character.** *A character the demodulator was not sure
of is not shown - no `?`, no dimmed maybe. Silence.*

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was
shut, that later blocks the unit told to open the door, is the session's to rewrite in its own
commit so it guards the rule and not the shut door - and that is not a ruling, not an ask, and
not a stop.* **This applies to `TheOliviaSeamTests`' "nothing decodes yet" assertion** (decision
AM): rewrite it in its own commit to guard what 0.5 guards, not the sentence.

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file
alone, proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).*

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others; it does not add guards for doors it is not building,
pins against changes it is not making, or tests of a test.*

**CLAUDE.md §0.2 - Transmit safety, absolute.** *Every code path that keys the transmitter has a
same-thread, no-await abort available. One operator action, one transmission ... It never
transmits on a decode.* **This unit adds rows and touches nothing that keys.**

**FACT-004** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio.

**HM-DEC-139**, **HM-DEC-155**, as in sections 1 and 3.

**Standing decisions of earlier instructions, still in force:** H (format constants from
`data\olivia\format.json`), I (variant and center from the detector, never from the test), J (CER
is Levenshtein over the manifest text's length, line endings unified, case exact), M (rates derive
from `format.json`), O (the blind search is never told the answer), V (a clean CER off 0.0000, or
a noise file that emits a character, after a receiver change is a regression and the change comes
out), Y and Z (the listener's shape and streaming), AA (one station, one channel). **Decision L -
*engine only, the panel is not wired* - is lifted by this unit**, for the receive side only.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings; they are in
the decision block at the end.

- **AE. The listener is fed under the Olivia tab, and only there.** One `OliviaListener`, made and
  fed where `MainWindowViewModel` makes and feeds `Psk31Listener`, from the same audio tap. **Under
  Olivia the PSK31 listener is not fed, and under every other tab the Olivia listener is not fed**
  (0.5's *no other mode's decoder runs*, both ways). Its rate is whatever the trace finds
  `OliviaListener` needs against what `Psk31Resampler` gives; a resampler the listener needs is
  inside this unit, a package is not (§6). **R27's app half across tabs - an RSID heard under
  PSK31 or FT8 switching the tab to Olivia - is not a step 3 criterion and is not built here**; it
  is logged in the report for the plan's author, not chased. Under the Olivia tab the variant is
  the channel's, from its RSID or the blind search, and the operator picks nothing.
- **AF. One row path, not two.** Olivia channels are drawn by the code that draws PSK31 channels,
  through a mapping from `OliviaChannel` to whatever `ShowPsk31Channels` takes. **The row gains the
  variant** (R28, *the row says the variant*) - a field and its text, shown only on Olivia rows;
  that and the mapping are the only changes the row path may take. **No copy of the row code, the
  parser, the CQ filter, worked-fade, `EntityOf`, the quill or the hover.** 3.3's *no change to their
  code* is proved by `git diff 75571f5d -- <each file>` showing nothing in those members, printed in
  the report; if the variant field needs a line in one of them, say which line and why, and 3.3 is
  reported with that line named.
- **AG. The row's text is the channel's accepted text, and nothing else.** No character before its
  block (§3.3, §R9). The row's center is **the center as of the last accepted block**, not the
  tracker's latest (unit 363 item 4). The text's lag behind the air - about three blocks (unit 363
  item 3) - is **measured from the row's point of view and reported, with no ceiling**; nothing in
  step 3 sets one, and 4.8's turnover patience is step 4's.
- **AH. 3.1's rows are proved through the app's own feed.** The two-signal fixture, hash-checked,
  fed through the path the app uses (as `ThePsk31HearsEveryoneTests` feeds PSK31), yields **exactly
  two rows**: 8/250 within 5 Hz of 1000 and 16/500 within 5 Hz of 2000, each row showing its variant,
  each row's text at **CER 0.05 or under** against its half of the manifest `text` (split at ` | `),
  and **the other station's callsign nowhere in a row**. Two rows at most at any tick, asserted
  across the whole feed. The listener is never told where the stations are.
- **AI. 3.2 feeds the corpus text through Olivia rows.** The corpus is text, not audio, so it is
  fed at the row: for each transcript in `corpus.json`, an Olivia row carrying that text (through
  the same test seam `ShowPsk31ChannelsForTests` gives PSK31, an Olivia twin of it if needed) and a
  PSK31 row carrying the same text, and **the parser's verdicts compared, every field, every
  transcript: zero differences**, the count printed. `Psk31ExchangeParser.cs` has no diff against
  `75571f5d`. If a transcript's verdict differs, that is the finding; the parser is not changed to
  make it agree.
- **AJ. 3.4 - the retire window is 24 times the variant's seconds per character, measured from
  the end of the channel's last accepted block.** At 8/250 that is about 16.4 s, eight blocks; at
  16/500 and 32/1000 proportionally less, never under the reading lag. **The factor 24 is the
  arbiter's number**: long enough that a station pausing between lines is not retired, short
  enough that a row goes within an over. The trace measures the longest gap between accepted blocks
  on every shipped fixture; **if any fixture's channel would retire mid-transmission at 24, raise
  the factor to the smallest whole number that holds on every fixture, state it and say why** -
  that is a number, not a ruling. The factor lives in data or a named constant beside the timing
  table, read from it, not a seconds literal. **A retired row stays on the list marked ended**, as
  PSK31 rows do, and a retired channel's tracker stops, so its center stops wandering (unit 363
  item 4). The retire is the engine's (`OliviaListener`), the *ended* mark the row's; a new RSID or
  a blind find at the place reopens as a new channel under decision AA.
- **AK. 3.5 - the PSK31 row events, with `mode: olivia` and the variant.** The events the app
  writes for PSK31 rows from `States` and `Watch` are written for Olivia rows from
  `OliviaListener.States`, **the same event names and fields**, plus `mode: olivia` and `variant`,
  plus the retire (AJ) where PSK31 writes its end. **No decoded text and no callsign**, asserted as
  unit 363 asserted the listener's events. The listener's own `olivia_channel` and `olivia_block`
  stay as they are; do not write a row event twice from two places.
- **AL. The engine carry-forward line loses the name that asserts nothing.** Unit 363 item 2:
  `TheOliviaBelowTheNoiseTests` is on the line by class, which runs `TheFurtherSeedsArePrinted`
  (about 85 s, asserts nothing - the list's own rule keeps such names off). **Replace it with the
  type-and-method form `TheOliviaBelowTheNoiseTests.TheQsoIsReadBelowTheNoise`**, in task 0's second
  commit, after the before-run, and report the new time.
- **AM. The panel stops saying nothing decodes.** Once rows are drawn the sentence is false. It
  goes; the panel shows the rows where PSK31's panel shows its rows, and **no other wording or
  layout changes**. `TheOliviaSeamTests`' assertion of that sentence is rewritten under §R12 in its
  own commit, to guard 0.5 - no other decoder runs, no path to a send - rather than the sentence.
- **AN. No row reaches a send.** Olivia rows get whatever the PSK31 row carries on screen, but
  **Answer, Report, Confirm, the CQ press and the typed line under the Olivia tab still refuse and
  key nothing**, exactly as 0.5 proved at step 0. Step 4 opens them. Asserted: a click on each under
  Olivia with two rows present sends nothing and writes the refusal step 0 writes; `PttOn` 1 and
  `Arm(` 2 at the end, as at the start. **If drawing the rows through the PSK31 path cannot be done
  without a send path appearing under Olivia, stop that task, say what opened it, and `MOVE: stop`
  in section 4** - that is §6's transmit clause, not the unit's to decide.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- Check `PHASE_STATUS.md` has steps 0, 1 and 2 `done` and step 3 `partial`, and report it.
- **Step 3's entry, first:** hash `olivia-two-signals-rsid.wav` against the manifest, then run the
  RSID detector over it and report **two detections, 8/250 at 1000 and 16/500 at 2000**, with
  their centers. Then the other eight, 9 of 9. If any hash fails, or the detector does not find
  both, stop.
- Append `UNIT 364 - STEP 3` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 363`
  entry. Touch no earlier entry.
- Patch-bump the version by one (unit 363 left 1.13.50).
- Run the carry-forward list, both invocations, before any change, and give the seconds.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md` or anything under `.run-unit\`.
- **Then decision AL**, in its own commit, with the engine line run once more and its seconds.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2
writes a line. A class that asserts nothing, `Unit364Trace`, in the shape of `Unit363Trace`, not on
the carry-forward line.

1. **The rate.** What `OliviaListener` expects, what the audio tap and `Psk31Resampler` give, and
   what feeding one from the other takes.
2. **The row path, with file and line:** what `ShowPsk31Channels` takes and builds, what a row
   carries, where the parser, the CQ filter, worked-fade, `EntityOf`, the quill and the hover read
   it, and **where a click on a row or the card reaches a send and how 0.5's refusal stops it under
   Olivia today**. This is where decision AN's risk lives; name it.
3. **The PSK31 row events**: each event name the app writes for a row, from where, with its fields -
   the list decision AK copies.
4. **The gaps.** On every shipped Olivia fixture through the listener: the longest gap between the
   ends of two accepted blocks on one channel, in seconds and in the variant's characters, and the
   seconds from the last accepted block to the file's end. **This is what decision AJ's 24 is
   checked against.**
5. **The lag, from the row's side:** on the two-signal fixture and the 16/500 QSO, for each block,
   when it ended in the audio and when its text reached the channel. First, median and worst.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - the rows (3.1 rows half, 3.5)

Wire the listener to decisions AE, AF, AG, AK, AM and AN.

**Tests watched failing first**, in a new `TheOliviaRowsTests` (app), every file hash-checked first:

- **3.1** decision AH on the two-signal fixture: two rows, variants and centers, each text at 0.05
  or under against its own half, the other callsign in neither, two rows at most at any tick.
- **The noise-only file** through the same path: **no row and no character**. The no-RSID 8/250
  file: one row, `8/250`, found blind.
- **3.5** decision AK: the row events on the two-signal fixture with `mode: olivia` and the
  variant, no text and no callsign in any of them.
- **Decision AN**: with the two rows present under Olivia, every send control refuses and keys
  nothing.
- **Decision AE**: under the Olivia tab the PSK31 listener is fed nothing; under PSK31 the Olivia
  listener is fed nothing.

`TheOliviaSeamTests` rewritten under §R12 in its own commit (decision AM). Add `TheOliviaRowsTests`
to the app carry-forward line in this task's commit, run the list, both invocations, and give the
seconds.

**If the two-signal fixture gives one row, three, or a row with the other's text through the app
where the engine gave two clean channels**, that is a finding about the wiring: report the ticks,
the pieces fed and the channels at each, and 3.1 is not met.

**Drop candidate:** none. Everything after it stands on the rows.

### Task 3 - the parser and the row features on Olivia rows (3.2, 3.3)

**Tests watched failing first**, in a new `TheOliviaRowsReadLikePsk31Tests` (app or engine, where the
seams are - say which):

- **3.2** decision AI: every transcript in `corpus.json` through an Olivia row and a PSK31 row,
  verdicts compared field by field, **zero differences**, the count of transcripts printed.
- **3.3** each of the CQ filter, worked-fade, `EntityOf` with its `CQ` guard, the quill and the
  whole-message hover exercised on an Olivia row, **with the same outcome as on the PSK31 row
  carrying the same text** - the CQ filter keeps a CQ row and drops a non-CQ one; a worked call's
  row fades; `EntityOf` resolves the station and refuses `CQ` as a call; the quill and the hover
  show what they show for PSK31.
- **The `git diff 75571f5d` of the parser and the five features' files**, printed in the report
  (decision AF).

Add the class to its carry-forward line in this task's commit.

**Drop candidate:** none. Both are must-pass.

### Task 4 - the retire (3.4)

To decision AJ, in `OliviaListener` and the row.

**Tests watched failing first**, in `TheOliviaRowsTests` or its own class:

- **3.4** on the two-signal fixture fed to its end and then silence (or noise at the file's
  level, stated) for longer than the window: **each row is retired within the window of its own
  variant after its last accepted block, and stays on the list marked ended**, with its text. The
  16/500 row, whose station stops first, is ended while the 8/250 row is still open, if the
  fixture's timing allows it - say what it allows.
- **No row retires mid-transmission** on any shipped fixture at the stated factor (trace item 4).
- **The ended channel's center does not move** after it ends.
- **The retire event** (decision AK) carries the variant, the window in seconds and the factor, no
  text.
- **The window is derived**: for each of the three variants, the test reads the factor and the
  timing table and asserts the window is their product, not a literal.

**Drop candidate:** none. It is a must-pass.

### Task 5 - the real-time ratio with the rows drawn (3.6)

**Only if tasks 2 to 4 are reported with their numbers.**

- The two-signal fixture through the app's feed path with rows drawn, in `CpuMeasuredAlone`: **the
  ratio, reported against 1.0**, asserted under 1.0, beside unit 363's engine 0.154. The longest
  single tick printed.
- Not on the carry-forward line.

**Drop candidate: this whole task.** Drop it whole and say so; 3.6 is nice-to-pass and was
measured on the engine. Do not drop it half-built.

---

## 9. Parked - do not touch, do not raise

- **R27's app half across tabs** - an RSID heard under another tab switching to Olivia (decision
  AE). Logged in the report for the plan's author; not built.
- **Any Olivia send: the modulator, the macros, the typed line, the move-off-and-widen macro, the
  turn timing** - step 4. Decision AN keeps every send control refusing.
- **Shortening the reading lag** (unit 363 item 3) - measured and reported (decision AG), not
  changed; trading `TrackSmoothing` against 2.7's drift is not this unit's.
- **The per-character gate** (unit 362 item 2, unit 363 item 5) - logged. **If a row ever shows a
  character not its own station's**, that is the finding; report it and do not ship 3.1 as met.
- **The streaming reader against `Decode` below the noise** (unit 363 item 6), the -16 dB file
  through the listener, the blind search below the noise - logged.
- **The listener's `Flush()` and its own events against the PSK31 split** (unit 363 item 7) -
  decision AK settles the events; `Flush()` stays, uncalled by the app.
- **Unit 363's section 4:** item 1 logged; item 2 is decision AL; item 3 is decision AG; item 4 is
  decisions AG and AJ; item 5 parked above; items 6 and 7 as above; items 8 and 9 logged.
- **`longestSeconds` on the typed line's record, the refusal sentence quoting the whole audio, the
  card's *60 s of text* rounding** (unit 360 items 1, 2, 4); **`RsidDetection` giving the first
  tone** (unit 361 item 4); **codes 72 to 75 and the fldigi commit pin** (unit 359 item 3); **the
  dial 1500 Hz below the center** (unit 358 item 1, step 0 closed); **the flaky Stop tests,
  HM-OPEN-090, the three off-list reds, the screen phase's open asks.** Carried in section 4, not
  worked.
- **`PHASE_PLAN.md`'s unchecked 1.5, 1.7, 2.3, 2.7 and 3.0, `PHASE_STATUS.md`'s stale unit number,
  and the mislabeled `PHASE_OUTCOME.md` entries.** Reported, not edited.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not copy the row path, the parser or any of 3.3's features for Olivia** (decision AF). One
  path; a variant field and a mapping.
- **Do not change `Psk31ExchangeParser` or any of 3.3's features to make an Olivia row agree.** A
  difference is the finding.
- **No retire window, lag or replay in literal seconds** (§3.2, decision AJ). Factors and the timing
  table.
- **Never give the listener a variant, a center or a start time from the manifest**, in code or in a
  test (decisions I and O).
- **Never loosen the 0.05, the 5 Hz, the 1.0 ratio, or a retire test.** A miss ships with its number
  (§6).
- **Do not edit a fixture, `manifest.json` or `corpus.json`, and write no WAV under `assets\`.**
- **Touch nothing on the transmit side**: not `Ft8TransmitSequence`, `UnslottedTransmission`,
  `Psk31Modulator`, `RsidBurst`, `PttOn`, any `Arm` site, or the refusal that stops a send under
  Olivia. `PttOn` 1 and `Arm(` 2 at the end, as at the start. Decision AN's stop is the only answer
  to a send path appearing.
- **Do not edit an existing test to make a change pass**, except `TheOliviaSeamTests` under §R12
  and decision AM, in its own commit, said in the report.
- **Do not touch `tools\`, `.run-unit\`, `RUN_LEDGER.md`, `PHASE_PLAN.md` or earlier
  `PHASE_OUTCOME.md` entries.** Add no package.
- **Report mismatches; repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Never commit `SESSION.lock` or `.run-unit\`. The
report names the branch and whether every push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 5 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0,
   1 and 2 done and closed; step 3 was partial at the start of this unit
   - 3.0 met, 3.1 engine half, 3.6 measured on the engine - and is
   <state after this unit>; steps 4-6 not started, and step 4's entry
   opens only when step 3 is done, which it <is|is not> on this report.
B. Step 3's criteria this unit worked - 3.1 rows: rows N (2), 8/250 at N
   Hz and 16/500 at N Hz (5 Hz), CERs N and N (0.05), the other callsign
   N and N (0), most rows at once N. 3.2: transcripts N, verdict
   differences N (0), parser diff <none|lines>. 3.3: CQ filter, worked-
   fade, EntityOf and CQ guard, quill, hover - <same|differs> each, code
   diff <none|lines named>. 3.4: factor N (24 or raised, why), windows N
   / N / N s, rows ended N of N, mid-transmission retires N (0). 3.5:
   events N, mode olivia and variant on each, text or callsign N (0).
   3.6: ratio N (1.0) with rows, or dropped. Met: <list by id>. Not met:
   <list, with the number>. Send controls under Olivia with rows: keyed
   N (0); PttOn N (1), Arm( N (2).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of a step 3 criterion - in
   particular whether drawing rows opened any path toward a send
   (decision AN, stop material), whether any row showed a character not
   its own station's, whether any feature's code had to change for
   Olivia rows, the lag measured from the row, and the engine and app
   carry-forward seconds against the 480 s timeout.
```

**Then the header:**

```
UNIT:       364 - <complete|stopped> at task N of 6, <task 5 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 3 criteria met 1 of 7 -> N of 7; Olivia rows on screen 0 -> N
DRIFT:      0
```

**Then a criterion table, 3.0 to 3.6** - 3.0 as unit 363 met it, and 3.1 to 3.6 with this unit's
numbers and their state (*met*, *not met*, *measured*).

**Section 3 leads with what the operator would see**: the Olivia tab with the two-signal fixture
fed, the two rows as drawn - each row's text, variant, center and state - and then the same after
the retire window, both rows marked ended. Then the 3.2 comparison count and any difference, the
3.3 table (feature, PSK31 row, Olivia row, code diff), the retire table (variant, seconds per
character, factor, window, longest gap on any fixture), the row events as written, the lag table
from the trace, the send-control refusals, and the real-time table if task 5 was built.

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004).

---

```
ARBITER-DECISION
STEP: 3
APPROACH: Olivia listener wired under the Olivia tab and its channels drawn as rows through the one PSK31 row path with the variant on the row - parser and row features unchanged, retire window as a factor on the timing table, the PSK31 row events with mode olivia - and every send control still refusing
MOVE: continue
WHY: Step 3 is partial with 3.0 met and the listener built; the five must-passes left - 3.1's rows, 3.2, 3.3, 3.4, 3.5 - all stand on putting that listener's channels on the screen, and the loop test finds no rows approach in any entry. This is the first unit on them, not a retry.
STATE: partial
DECIDED: author's, overrulable - (AE) the listener is fed under the Olivia tab only, from the PSK31 audio tap, PSK31's listener not fed under Olivia nor Olivia's elsewhere; R27's across-tab switch is not a step 3 criterion and is logged, not built. (AF) one row path - a mapping and a variant field, no copy of the row code, parser, CQ filter, worked-fade, EntityOf, quill or hover, proved by git diff. (AG) a row's text is the channel's accepted text, its center as of the last accepted block, the lag measured and reported with no ceiling. (AH) 3.1 proved through the app's own feed on the two-signal fixture. (AI) 3.2 fed at the row - every corpus.json transcript through an Olivia row and a PSK31 row, zero verdict differences. (AJ) the retire window is 24 times the variant's seconds per character from the last accepted block's end, raised to the smallest whole factor that retires nothing mid-transmission on any shipped fixture if 24 does, stated; ended rows stay listed and stop tracking. (AK) the PSK31 row events with mode olivia and the variant, no text, no callsign. (AL) the engine carry-forward line takes TheOliviaBelowTheNoiseTests.TheQsoIsReadBelowTheNoise by method, dropping the name that asserts nothing. (AM) the panel's nothing-decodes sentence goes and TheOliviaSeamTests is rewritten under R12 to guard 0.5. (AN) every send control under Olivia still refuses with rows present; a send path appearing is stop material. Decision L lifted for the receive side. Task 5 (3.6) is the drop candidate; tasks 0 to 4 have none. Unit 363's section 4 asked for no ruling; items 2, 3 and 4 are taken as decisions AL, AG and AJ, the rest logged.
LICENCE: PHASE_PLAN.md step 3 criteria 3.1 to 3.6, step 0 criterion 0.5, R27, R28, R31, section 3.2, section 3.3 and section 6 (a number, a label or a mechanism is the arbiter's; the transmit clause; never loosen a test; a package is needed); PSK31 plan R9, R12, R13, R14; CLAUDE.md 0.2; HM-DEC-139, HM-DEC-155; ARBITER.md sections 2 and 6
ACCOMPLISHED: The operator presses Olivia and sees the stations on the air as rows, as PSK31 shows them - each row naming its variant, reading only its own station, understood by the same parser, filtered, faded and resolved the same way, and marked ended when the station goes - with nothing on the Olivia tab yet able to transmit; which closes hearing everyone and opens Hamlet's own Olivia send
ADVANCES: step 3 criteria 3.1 rows half (task 2), 3.5 (task 2), 3.2 and 3.3 (task 3), 3.4 (task 4), all must-pass; 3.6 re-measured with rows (task 5, nice-to-pass)
END-ARBITER-DECISION
```
