READ IN THIS ORDER.

A. The parity totals: none. fldigi's source could not be fetched, so there is no second decoder
   and nothing to table. Ours at entry, for the comparison when it comes: real, inferred keys,
   MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, sure-and-right coverage 403 over 473,
   MET-WBE 37 over 113; synthetic, exact keys, 14 of 173, 14 over 252, 159 over 252, 44 over 84.
B. Step 9 (0 of 6 by the plan, unchanged): 9.1 not started, since the port needs the source and
   `git clone` of github.com/w1hkj/fldigi was refused in this session; 9.2 the table, 9.3 the wins
   and 9.5 the recordings it reads that we do not are all behind 9.1.
C. This report adds the entry round at step 9, with every floor and metric as 453 left them, and
   one request in section 4 that unblocks all of B: put fldigi's source under `.run-unit\fldigi\`,
   or allow the clone. Nothing else here bears on A or B.

UNIT:       454 - stopped at task 1 of 4, none dropped (tasks 1 to 4 blocked by the refused clone) - 2026-09-26 13:07
PHASE GOAL: Hamlet's CW decoder meets every must-tier requirement in CW_REQUIREMENTS.md, shown by tests that name them, ending with Tim reading real CW off the air.
UNIT GOAL:  Put a faithful port of fldigi's CW receiver beside ours, read every recording with both through the same scorer, and say where fldigi does better and, from its source, why.
ADVANCED:   no - the port was not built because the source could not be fetched; nothing in step 9 moved
NUMBER:     MET-CER-SURE ours 33 of 436 real, 14 of 173 synthetic; fldigi not measured; recordings fldigi wins not measured, ours wins not measured, tie not measured
DRIFT:      step 9 1 (was 0); step 2 0; step 3 0; step 4 1; step 5 1; step 6 1, carried from 453

## 1. What Claude did

**Stopped at task 1 of 4.** Task 0 is done. Tasks 1, 2, 3 and 4 were not started. The work
instruction's §3 says that if `git clone` is refused, the session records the denial, asks for the
files, and stops at task 1. It was refused, so that is what I did. Task 3 was the named drop
candidate. Tasks 1, 2 and 4 are **not** drop candidates: they were stopped by the block, not by a
sizing choice of mine.

**Provenance.** Claude Code on the Windows development machine (QUIVERFULL). The prompt claimed
Hamlet, and the gate confirmed it: `SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`,
`CW_REQUIREMENTS.md` and `CW_SPEC.md` are present, `CoreHMI.sln` and `MURC.sln` are absent, and the
root is `C:\Source\HamLet`. Branch `main`. Nothing in this report is evidence about the radio.
`CW_REQUIREMENTS.md` has section M (v1.1, 2026-09-26), as the instruction requires.

**The denial.** Command: `git clone --depth 1 https://github.com/w1hkj/fldigi.git
C:/Source/HamLet/.run-unit/fldigi`. The session's permission layer answered "This command requires
approval", and this session cannot grant approval. I treated that as a denial and did not try any
other route: no curl, no archive download, no web fetch. `.run-unit\fldigi\` does not exist.

**Task 0, commit `2daa8ad1`, pushed to `origin/main`:**
- `PHASE_OUTCOME.md` has `## UNIT 454 - STEP 9`, with the arbiter block and an ENTRY line, in both
  copies.
- `PHASE_STATUS.md` names 454 with `CURRENT_STEP: 9`, in both copies.
- Version bumped from 1.13.140 to 1.13.141.
- The runner's uncommitted writes (`PHASE_STATUS.md`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`) were
  committed as they were, following 453's precedent.

**Entry round.** All figures are at HEAD `d81d8e0a` plus the record edits; `src` was not changed.

| check | entry |
|---|---|
| build, warnings as errors | 0 errors, 17 s |
| engine carry-forward line | 178 of 178, 388 s |
| app carry-forward line | first run 274 of 278, 173 s: 4 lost in 1 ms each to Avalonia headless "You've caused dispatcher loop" (`ThePsk31ConversationCardTests` x2, `TheFavoritesAreChipsTests` x2). Rerun once: 278 of 278, 168 s. No hang. |
| captures floor | 51 of 51, 131 s |
| adjudicated floor | 13 of 13, 34 s |
| named floors | 10 of 13, 67 s: 17:37 38 of 46, `032113` 43 of 45, `032129` 42 of 64 (red as at 453) |
| MET-CER-SURE | real, inferred: 33 of 436, 0.0757. Synthetic, exact: 14 of 173, 0.0809 |
| MET-INVENTED | real: 33 over 473. Synthetic: 14 over 252 |
| coverage (R82) | real: 403 over 473, 0.8520. Synthetic: 159 over 252, 0.6310 |
| MET-WBE | real: 37 (29 inserted, 8 deleted) over 113. Synthetic: 44 (13 inserted, 31 deleted) over 84 |
| metrics run | 64 s |

Each figure equals 453's exit.

**Verified against the tree (§3), as far as it reaches without the source:**
- `CwDecodeHarness.Decode(MonoAudio, expectedToneHz)` builds a decoder from `audio.SampleRate`,
  pumps a `BufferedAudioSource` through `Listen`, then calls `Flush`, and collects
  `CharacterSettled`. The second decoder has to be driven through the same `MonoAudio` and pump
  for the comparison to be fair.
- fldigi's internal rate and block size are **not verified**, because the source is not here. The
  port's input stage is named in the unit that has the source.

**Decisions I made for myself:**
1. I reran the app carry-forward line once after the dispatcher-loop failures, and recorded both
   runs. No source had changed, and 453 saw the same intermittent loss at its exit.
2. I did not continue to other tasks. Task 2 needs task 1's port. Task 4's exit round would repeat
   task 0's, with `src` untouched.

Nothing was recorded under `CLAUDE.md` §12.1. Nothing in this unit keys or transmits.

**Instruction check.** The prompt and the work instruction both carry the status cadence, and the
work instruction states the task count (4).

## 2. What the owner should expect

Nothing about CW has changed today. Hamlet reads every recording exactly as it did after
yesterday's run. It still gets about one letter in thirteen wrong while showing it as sure, and on
the W1AW bulletin it still prints `PGOPAGATION FORECAST BUAELETIN`. How far that is from a mature
decoder is still unmeasured, because the fldigi source could not be downloaded from inside this
session. Once the source is in the folder named in section 4, the next session can port it and
produce the side-by-side table. **What will look wrong but is not:**
- The app test line lost 4 tests on its first run and passed 278 of 278 on the rerun. That is a
  known intermittent fault in the test host, not a regression.
- The three named floors are red, the same three at the same values as 453 left them.

## 3. What you should see

**No visible change. This unit produced no parity table and no second decoder.** The question it
was commissioned to answer, how ours compares with fldigi on the four metrics, is **not answered**:
fldigi 0 of 0 measured, against ours at 33 of 436 sure-wrong on the real recordings and 14 of 173
on the synthetic set. Nothing changed in the application.

## 4. What's blocking us

1. **Ruling proposed: the owner places fldigi's source under `C:\Source\HamLet\.run-unit\fldigi\`.**
   - **Simplest way:** run
     `git clone https://github.com/w1hkj/fldigi.git C:\Source\HamLet\.run-unit\fldigi` from any
     shell. That brings the commit hash for the attribution. The alternative is allowing that one
     command in the session's permissions.
   - **Minimum set, if you copy files instead:** `src/cw_rtty/cw.cxx`, `src/include/cw.h`, and
     the DSP helpers they include (the FFT filter, the moving average and the sliding FFT), with
     the commit hash written beside them.
   - **Reasoning:** the work instruction's §3 routes a refused clone to exactly this request, and
     forbids routing around it.
   - **Rejected:** fetching the source by another route (curl, web fetch, archive). The clone
     refusal is a denial, and a second route around it is what §3 and §6 forbid.
   - This blocks tasks 1 to 4 and all of step 9.
2. **CW reading under R85:** none needed this unit, because no CW question arose before the block.

**Asks still outstanding**
- From unit 453, 2026-09-26: whether 6.5 stands ticked on three spans measured and three not
  measurable here. It waits on the owner's ruling. The tick is in both copies of `PHASE_PLAN.md`,
  and the measurement is in `docs/phase-requirements/metrics.md`. R85 may settle it as a CW
  question read from the documents. No ruling is on file.
- From unit 453, 2026-09-26: whether HM-REQ-084 keeps `ABOVE` on 013637, which no decoder working
  from the audio alone can print as one word under R72. It waits on the owner's ruling. The
  requirement stands unedited in `CW_REQUIREMENTS.md` §I. No ruling is on file.
