# Work instruction 242 - the scoreboard, and the baseline reproduced rather than inherited

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**This is the first unit of a new phase and it installs the phase.** The previous
phase closed on 2026-09-04 at 21:41 UTC when Tim saw fourteen FT8 messages from
one slot on 14.074. Its files are still at the root and must be archived, not
overwritten.

**The new phase's subject is the 1.5 dB.** `HM-OPEN-067` records it: the 50 per
cent crossing near **-19.5 dB** against a published **-21**, on 306 trials a rung,
with the SNR axis checked against a second instrument sharing no line of code with
the first and agreeing to 0.0098 dB mean. Unit 222 then took it apart and found it
in no single stage - oracle alignment, unquantised magnitudes, physics ratios and
four times the iteration bound each landed inside the as-is interval. **At -21 dB
the hard decisions carry about 31 bit errors against a code that recovers to zero
at 17.** The demodulator is fine. Belief propagation gives up while the answer is
still reachable.

**This unit builds none of that.** It builds the instrument that every later unit
is scored on, and it reproduces the baseline rather than inheriting it.

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    The phase is installed, and a ladder the arbiter can run every unit
              reproduces 4.2 per cent at -21 dB from a cold start.
ADVANCES:     step 0
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report any mismatch.
Report it; do not repair the instruction.

- `PHASE_PLAN.md`, `PHASE_STATUS.md`, `PHASE_OUTCOME.md` and `PROJECT_CARD.md` at
  the root belong to the closing phase. `PHASE_OUTCOME.md` carries about
  forty-one entries which are that phase's memory.
- The replacements are staged at `docs/phase-sensitivity/` and were delivered
  with this instruction.
- `HM-OPEN-067` in `OPEN_ISSUES.md` carries the ladder's figures and the address
  of the loss.
- Units 218, 221 and 222 built and ran ladders. **Find what they left in the
  tree** - the generator, the SNR delivery, the trial harness - and report what
  exists before writing anything new.
- Root version is `1.12.45` after work instruction 241. `Ft8Sharp` is `0.10.7`.

Expected to fail: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, and the 51
inherited CW reds in `docs/unit239-failing-set.txt` which fail at the baseline
`d541fc8` too. Do not chase either.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by any unit.**

**The seam is split, settling the divergence question open since 2026-08-31.**
`Ft8Sharp` stays a faithful MIT port of `ft8_lib`, byte-identical in behaviour,
and **nothing in this phase changes a line of it.** Improvements live in a
sibling, `Ft8Sharp.Deep`. The port's value now is that it cannot drift: every
measurement is taken against something known-identical to upstream.

**There is no WSJT-X on the development machine and no unit may assume one.**
Tim's ruling, 2026-09-04. The only machine that can measure against it has the
radio attached (`SHACK_FACTS.md` FACT-004). **A unit that cannot close without a
real-air comparison says so and stops.** It does not substitute
`decode_ft8.exe`, which is `ft8_lib` and therefore the thing being improved on.

**Tim generates the capture fixtures.** Ruled 2026-09-04. One command at the
shack per batch of captures, committed.

**Nothing is claimed without the scoreboard.** No unit may report an improvement
except as a number on it.

**A wrong decode is counted separately from a missed one, everywhere.** A message
returned that was not sent is the one failure this phase cannot trade against
rate (§0.0).

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only, cited at the point of use.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running.

---

## Tasks

### Task 1 - the phase transition

**Nothing is overwritten. Everything is moved, then replaced, then committed, so
git can undo the whole thing.**

- Create `docs/phase-ft8/` and **move** the closing phase's `PHASE_PLAN.md`,
  `PHASE_STATUS.md` and `PHASE_OUTCOME.md` into it. `PHASE_OUTCOME.md` carries
  forty-one units of memory and **losing it is the failure this task exists to
  avoid.**
- Install the staged files from `docs/phase-sensitivity/` at the root:
  `PHASE_PLAN.md`, `PHASE_STATUS.md`, `PHASE_OUTCOME.md`.
- `PROJECT_CARD.md` gains the new `PHASE` and `PHASE_SET`. **This file is changed
  only by ruling** (§13.3) and the ruling is Tim's approval of `PHASE_PLAN.md` on
  2026-09-04. Append the ruling to `DECISIONS.md` in its own format, next id in
  sequence, naming the phase set and citing the plan.
- **There is no `HEARTBEAT:` line in the new `PHASE_STATUS.md` and none may be
  written by hand.** The launcher writes it. Until it does, the card reading
  *stopped* is the true one.
- Verify: `tools\arbiter\layer-check.bat` and `status-check.bat` if they read
  these files, and report what they say.
- Commit and push before task 2. **If a piece of this cannot be completed, finish
  what can be, record precisely what was not done, and continue to task 2.** A
  half-installed layer is reported and repaired next unit; a stopped loop costs a
  night. The one exception is losing `PHASE_OUTCOME.md`, which is why it is moved
  and committed before anything else happens.

### Task 2 - find what the previous phase already built

- Units 218, 221 and 222 measured ladders. **Report what exists** with file and
  line: the message generator, the signal synthesiser, the noise delivery and its
  SNR calibration, the trial loop, and how a trial's result was scored.
- **Report what the delivered SNR was verified against.** Unit 222 checked the
  axis with a second instrument agreeing to 0.0098 dB mean. Say whether that
  instrument is still in the tree.
- If a usable harness exists, task 3 extends it. **If it does not, say so** - a
  ladder rebuilt from scratch is a different measurement and the baseline
  reproduction in task 4 is what decides whether it is the same one.

### Task 3 - the ladder runs in the loop

- A harness the arbiter can run every unit: synthesized messages at a commanded
  SNR, decoded, scored against the message that went in. **No external decoder,
  because the ground truth is what was transmitted.**
- It takes both `Ft8Sharp` and, once step 1 exists, `Ft8Sharp.Deep`, and reports
  them side by side.
- **Three counts, never two**: decoded correctly, missed, and **returned wrong**.
  A wrong decode is reported on its own line with the message sent and the
  message returned.
- Runs a commanded rung and trial count so a unit can take 306 trials at -21 dB
  or a quick 50 for a smoke check.
- Deterministic from a seed, so a result can be reproduced exactly.
- Time for a 306-trial rung reported, since every later unit pays it.

### Task 4 - the baseline, reproduced

**This is the goal task and it is a measurement, not a build.**

- Run the ladder at **-21 dB, 306 trials**, and report the rate with its Wilson
  interval and the wrong-decode count.
- **The number to reproduce is 13 of 306, 4.2 per cent, 0 wrong**, at a delivered
  -21.001 dB.
- **If it reproduces, the instrument is trusted and committed as the baseline.**
- **If it does not, that is this unit's headline finding - and it is not a stop.**
  Do not adjust the harness until it agrees, and do not halt over it. Record both
  numbers with the delivered SNR each was measured at, **adopt this measurement as
  the baseline with its provenance**, and note that every target in
  `PHASE_PLAN.md` moves by the same offset. The relative gain is what this phase
  measures; the absolute figure is a label on the axis.
- Also run **-19 dB and -20 dB** and report against unit 221's 81.0 and 23.9 per
  cent, so the shape is checked and not only one rung.

### Task 5 - the capture fixture

- A format: a committed text file per real capture, naming the capture, its UTC
  and its **SHA-256**, listing what WSJT-X returned - message, frequency, dt and
  SNR per row.
- A harness that reads it and scores decodes against it, matching on message
  text.
- **A fixture whose capture is absent, or whose hash does not match, fails loudly
  rather than passing quietly.** A stale fixture silently measures the wrong
  thing, and this is the clause that prevents it.
- **One command Tim runs at the shack** to produce a fixture from a capture, in
  one step, no editing. It runs where WSJT-X is, which is not this machine, so
  **it cannot be tested end to end here** - test the reader against a hand-written
  fixture and say plainly that the generator is untested against the real
  program.
- Commit an example fixture, clearly marked as an example and not real air.

### Task 6 - the phase's first outcome entry, and the plan's own escape hatches

**This is the named drop candidate.**

Append this unit's entry to the new `PHASE_OUTCOME.md` through
`tools\arbiter\outcome-append.bat` rather than by hand, so the file's first entry
proves the tool still works against a fresh phase. If the tool refuses, report
what it said and leave the file alone.

**Then read `PHASE_PLAN.md`'s section *the steps are a hypothesis, not a
contract* and say in the report that you have.** It grants the arbiter leave to
reorder, replace, retire and add steps and to move a target that was measured
wrong, all without asking. Every later unit in this phase depends on that being
understood, and a plan followed more literally than it was meant is the failure
this phase is most likely to have.

---

## Parked - do not touch, do not raise

- **Building `Ft8Sharp.Deep`.** Step 1. Not this unit.
- **OSD, subtraction, baseband re-sync, SNR measurement, cross-slot combining.**
  Steps 2 to 6. **No unit may start one before its scoreboard exists.**
- **The decoded text panel.** Work instruction 241 owns it.
- **The CW decoder**, including the 419 dropped chunks in the 21:58 capture and
  the 51 inherited failing cases.
- **`Ft8Sharp.Deep`'s licence** - **already ruled GPL-3.0** by Tim on 2026-09-04.
  Do not raise it.
- **The engine project's missing total**, **the waterfall's late first row**,
  **`ReusableWindow`**, **`ProcessDelayForTests`**, **the tap's owner**, **unit
  237's Extensible conclusion**, **work instruction 231's four tree items**,
  **`validate-output.bat`'s permitted-spellings bug**, **the 101.33 ms pulse
  above 6 kHz**.

---

## What not to do

- **Do not delete `PHASE_OUTCOME.md`.** Move it. Forty-one units of memory.
- **Do not write a `HEARTBEAT:` line.**
- **Do not touch `src/Ft8Sharp/`.** Not a constant, not an arithmetic order. The
  port is the instrument.
- **Do not adjust the ladder to make the baseline reproduce.** If it disagrees,
  that is the finding.
- **Do not report a rate without its wrong-decode count.**
- **Do not assume WSJT-X exists on this machine.**
- **Do not run `Hamlet.App.Tests` unfiltered** - it stops partway. The four
  commands in `docs/full-suite-run.md`. One project at a time, never
  concurrently.

---

## Committing and pushing

Commit and push each task before starting the next. Root version `1.12.45` to
`1.12.46`; `Ft8Sharp` does not move. Name the branch and say whether the push
succeeded.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with three numbers:** the ladder's rate at -21 dB with its
Wilson interval and wrong-decode count, against the 13 of 306 it must reproduce;
the -19 and -20 dB rungs against 81.0 and 23.9 per cent; and the wall-clock time
for a 306-trial rung, since every later unit pays it.

**If the baseline did not reproduce, section 1 leads with that** and section 3
says what the two measurements differ by.

Write `output.md`, then stop. Do not start the next unit.
