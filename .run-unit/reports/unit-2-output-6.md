```
READ IN THIS ORDER.

A. The phase goal, Hamlet reads a CQ call correctly, and where each step
   stands: 0 done, 1 done, 2 done, 3 partial with P6 parked, 4 not
   started, 5 Tim's, 6 partial.
B. Step 1: 1.1 the set keyed and scored, 1.2 three by three and the
   recipes, 1.3 inferring-a-key.md, 1.4 what synthetics do not prove,
   1.5 the exit round - each met or not, with the number.
C. The synthetic table, weighed against A and B: what the decoder reads
   exactly and where it breaks. Section 4 raises 2 items, none in the way
   of a criterion in B.
```

**A.** Step 1 went from partial to **done** in this unit. Unit 412 had met 1.6, and this unit
met 1.1 to 1.5. The other steps are as they were: 0 and 2 done, 3 partial with P6 parked, 4
not started, 5 Tim's, 6 partial. **The decoder is unchanged.** All keyed recordings score
217 edits over 565 characters against inferred keys, at entry and at exit.

**B.** All five criteria are met and ticked in `PHASE_PLAN.md`:
- **1.1 met.** Twelve CQ calls, each with an exact key beside it, scored at HEAD and tabled
  in `baseline.md`. The three-by-three grid scores **102 edits over 243 characters against
  exact keys, 1 unsure per 134 named**.
- **1.2 met.** 12, 18 and 25 wpm by 15, 5 and 0 dB. Every recipe is in its key file and in
  the table below. `TheSyntheticCqRebuildsTests` holds every WAV byte for byte to its
  recipe, 13 of 13.
- **1.3 met.** `docs/phase-correctness/inferring-a-key.md` is written, with 17:37 worked in
  six steps.
- **1.4 met.** `docs/phase-correctness/synthetic-cq.md` is written and cited from all twelve
  key files. The rule that no synthetic case is ever sole evidence is stated beside the
  table in `baseline.md`.
- **1.5 met.** Both carry-forward lines are green (178 of 178 and 278 of 278). The three
  floor tests are green: captures 51 of 51 with every row identical to entry, adjudicated
  13 of 13, and the clean synthetics 2 of 2.

**C.** Against keys nobody had to guess:
- **At 15 and 5 dB the decoder reads a textbook CQ** with at most one edit in four of the
  six cases.
- **At 0 dB it prints nothing at any speed.** It goes silent, not wrong.
- **Two cases break at a strong signal.** At 18 wpm and 15 dB it splits `N0CALL` into
  single elements (13 edits). With the character gap at five units, it reads every
  character gap as a word gap (15 edits, 14 of them spaces). That second case is step 3's
  fault, reproduced on an exact key.

Section 4 raises 2 items. Both are parked, and neither is in the way of a criterion in B.

```
UNIT:       414 - complete at task 5 of 5, none dropped - 2026-09-24 00:50
PHASE GOAL: Hamlet reads a CQ call off the air correctly, measured as edits against a key over a scored region with a guard against going quiet, and at the end Tim says it read
UNIT GOAL:  Build CQ calls whose keys are exact because the generator made them, at three speeds by three signal strengths, score each at HEAD outside the 3.2 total, and write the rule for keying a call off the air and what the synthetics cannot prove
ADVANCED:   yes - 1.1, 1.2, 1.3, 1.4 and 1.5 met and ticked, step 1 done
NUMBER:     synthetic cases scored: 12, edits over characters against exact keys: 165 over 324 - the 3 by 3 grid 102 over 243, the five-unit row 63 over 81
DRIFT:      0
```

## 1. What Claude did

**Complete at task 5 of 5, none dropped.**
- **Where:** QUIVERFULL, branch `main`, project Hamlet. The gate confirmed it:
  `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln`
  absent.
- **Evidence:** nothing in this report is evidence about the radio.
- **Commits:** entry HEAD `5f8d48b0`. Task commits `2952581c`, `9662d294`, `87a602ec`,
  `3b37354b` and `f84b4683`, then the exit commit, each pushed with rc 0.

**The synthetic table.**
- **Text:** every case sends `CQ CQ CQ DE N0CALL N0CALL K`. `N0CALL` is already in the
  tree's fixtures and has a digit of five elements and an `L` of four.
- **Key:** exact by construction.
- **Scored region:** the whole decode against the whole key, ends trimmed.
- **Decode:** `CwDecoder`, fed hop by hop from 600 Hz, the floors' path.
- **Recipe:** in each case's key file in `tests/fixtures/cw/synthetic-cq/`. Tone 615 Hz
  drifting 3 Hz, no QSB, no preamble.
- **Spacing:** textbook 1 : 3 : 1 : 3 : 7 from the dit, 1200.0 / wpm ms, except the three
  `char5` cases, whose character gap is 5 units.

| case | wpm | char gap | SNR in passband | seed | edits | scored length | key | unsure per named | `Within` | decode |
|---|---|---|---|---|---|---|---|---|---|---|
| cq-12wpm-15db | 12 | 3 | 15 dB | 20260924 | 0 | 27 | exact | 0 per 21 | 0 | `CQ CQ CQ DE N0CALL N0CALL K` |
| cq-12wpm-5db | 12 | 3 | 5 dB | 20260925 | 5 | 27 | exact | 0 per 23 | 1 | `CQ CQ CQ DE N0CALL N0CALL T E A` |
| cq-12wpm-0db | 12 | 3 | 0 dB | 20260926 | 27 | 27 | exact | nothing named | 27 | empty |
| cq-18wpm-15db | 18 | 3 | 15 dB | 20260927 | 13 | 27 | exact | 0 per 26 | 4 | `CQ CQ CQ DE N0CALL N0C E T E T E ELT K` |
| cq-18wpm-5db | 18 | 3 | 5 dB | 20260928 | 1 | 27 | exact | 1 per 20 | 1 | `■Q CQ CQ DE N0CALL N0CALL K` |
| cq-18wpm-0db | 18 | 3 | 0 dB | 20260929 | 27 | 27 | exact | nothing named | 27 | empty |
| cq-25wpm-15db | 25 | 3 | 15 dB | 20260930 | 1 | 27 | exact | 0 per 22 | 0 | `CQ CQ CQ DE N0CALL N0CALL KK` |
| cq-25wpm-5db | 25 | 3 | 5 dB | 20260931 | 1 | 27 | exact | 0 per 22 | 0 | `CQ CQ CQ DE N0CALL N0CALL KK` |
| cq-25wpm-0db | 25 | 3 | 0 dB | 20260932 | 27 | 27 | exact | nothing named | 27 | empty |
| **grid** | | | | | **102** | **243** | **exact** | **1 per 134** | | |
| cq-18wpm-15db-char5 | 18 | 5 | 15 dB | 20260933 | 15 | 27 | exact | 0 per 21 | 15 | `C Q C Q C Q D E N 0 C A E L N 0 C A L L K` |
| cq-18wpm-5db-char5 | 18 | 5 | 5 dB | 20260934 | 21 | 27 | exact | 1 per 25 | 17 | `C Q C Q C Q TEE T ■KTDUUEUE N 0 C A L L K` |
| cq-18wpm-0db-char5 | 18 | 5 | 0 dB | 20260935 | 27 | 27 | exact | nothing named | 27 | empty |
| **five-unit row** | | | | | **63** | **81** | **exact** | **1 per 46** | | |

**All twelve are outside every total**, including the 217 that 3.2 judges. They are tabled in
`baseline.md` under *Synthetic, exact keys*, with 1.4's rule beside them.

**Task 0, the record.**
- Version 1.13.100 to 1.13.101. `PHASE_STATUS.md` names 414 and step 1. `PHASE_OUTCOME.md`
  has its `UNIT 414 - STEP 1` entry.
- **Section 5 against the tree:**
  - The recipe and `Generate` are as stated, and the sidecar names `wpm` and `snrDb`.
  - `CwKeyKind` already has `Exact`. It is in the test project's `CwScorer.cs`, not in
    `src`, so nothing was added to it.
  - 17:37's region rule is in `baseline.md` and `CwScorer.FromFirst`. **Mismatch:** a
    third copy, `TheSeventeenThirtySevenCaptureTests.ScoredRegion`, opens at the first
    `CQ CQ` rather than the first `CQ`. Reported, not repaired.
  - HM-DEC-101's gate still exists as
    `CwFixtureCommitTests.TheReferenceHasScoredThisFixture`. It runs nothing at test time.
    It checks for a committed `reference` line, which `tools/score-fixtures/score-fixtures.py`
    writes by running `cwdecoder.py`, and it covers only the catalogue in
    `tests/fixtures/cw/receiver`.
  - `inferring-a-key.md` did not exist.
- **Mismatch in the instruction's section 3:** it says all three of unit 413's asks are
  parked in `PARKED.md`. Only P6 is there. "3.5 against 3.2's own-commit rule" and "which
  total 3.2 judges" are not. Reported, not repaired or raised.
- **Entry round**, one build, then `--no-build`:

| type | result | time |
|---|---|---|
| engine carry-forward | 178 of 178 | 378 s |
| app carry-forward | 278 of 278, none lost to the dispatcher loop | 162 s |
| captures | 51 of 51 | 121 s |
| adjudicated | 13 of 13 | 29 s |
| clean synthetics | 2 of 2 | 2 s |
| `TheNumberCannotBeGamedTests` | 13 of 13 | 62 s |
| `TheBaselineIsScoredTests` | 33 over 46, outside 124 over 363, inferred | |
| `TheBenchmarkIsKeyedTests` | bench 60 over 156, live 41 over 156, inferred | |

**Task 1, the trace.** `WhatTheGeneratorMakesTests` asserts nothing and ran in 6 s. It
measures each corner two ways: directly, and taken apart into tone and noise. The noise is
the same recipe rendered with the tone 400 dB down, so it has the same seed and length.
- **cq-12wpm-15db:**
  - SNR: 15.09 dB directly and 14.99 dB apart, against 15.0.
  - Pitch: 618 Hz, against 615 drifting 3.
  - Runs: 73 marks and 72 gaps, each against its own.
  - Measured at half amplitude: dit 95.0, dah 295.0, gaps 105.0 / 305.0 / 705.0 ms.
    That is the recipe's 100 / 300 / 100 / 300 / 700 exactly as the 5 ms raised-cosine
    edges predict. Worst 0.3 ms.
  - Decode: 0 edits over 27 against an exact key.
- **cq-25wpm-0db:**
  - Measured directly, the envelope finds 243 marks, the noise's as well as the tone's.
  - Taken apart: SNR -0.01 dB against 0.0. 73 and 72 runs. Gaps 53.0 / 148.9 / 341.1 ms
    against 48 / 144 / 336, worst 0.3 ms.
  - Decode: empty, 27 edits over 27 against an exact key.
- **No generator defect.** At 25 wpm the edges put the rendered gaps at 1.10, 3.10 and
  7.11 dits of the recipe's.

**Task 2, the set (1.1, 1.2).**
- **Files:** nine cases, each with a WAV, a sidecar and a `.key.md`. The key file states
  that the key is exact by construction, the full recipe as expressions, the scored region,
  and what the case does not prove.
- **`TheSyntheticCqRebuildsTests`:** 10 of 10 in 1 s, first run given 600 s. It checks
  that each WAV is byte-identical and each key file is what its recipe writes. It writes the
  set only when `HAMLET_WRITE_SYNTHETIC_CQ=1`.
- **`TheSyntheticCqIsScoredTests`:** one fact, 6 s on its first run, given 600 s. It
  asserts no edit count, only that each row's error kinds add up to its edits.

**Task 3, the rule (1.3, 1.4).**
- **`inferring-a-key.md`** covers:
  - What may be inferred: the CQ form, a call read alike twice or more, and 1.6's
    differencing, which it cites.
  - What may not: unreadable audio, a call seen once, an unrepeated number, report or
    name, and anything that "must have been".
  - How the scored region is chosen: 17:37's rule, and scoring less rather than
    guessing more.
  - The worked example: 17:37 from its key file. The form is found. `WB6RED` is
    established by `ETWB6RED` and `W B 6 RE D`. No `K` is keyed because none was read.
    The soup is left unscored. It scores 29 edits over 25 against an inferred key, and
    the region is neither cut back nor widened.
- **`synthetic-cq.md`** covers what the set does not prove:
  - textbook spacing, the decoder's own fallback at `CwUnitEstimator.cs` 216 in
    `MeasureGaps`;
  - generated noise;
  - one tone and no second station;
  - machine-perfect keying;
  - a steady signal;
  - one call;
  - no reference score.

**Task 4, the five-unit row.** Three cases at 18 wpm with the character gap at five units,
scored and tabled as above. The nine grid WAVs rewrote byte-identical. Rebuild type 13 of 13.
**WAV size: 4,472,784 bytes for all twelve, under 10 MB.**

**Task 5, the exit round (1.5).** `Hamlet.sln` builds with warnings as errors.

| type | result | time |
|---|---|---|
| engine carry-forward | 178 of 178 | 376 s |
| app carry-forward | 278 of 278, none lost to the dispatcher loop | 164 s |
| captures | 51 of 51, every row identical to entry | 125 s |
| adjudicated | 13 of 13, identical to entry | |
| clean synthetics | 2 of 2 | |
| `TheNumberCannotBeGamedTests` | 13 of 13, identical to entry | |
| `TheBaselineIsScoredTests` | 33 over 46 and 124 over 363, identical to entry | |
| `TheBenchmarkIsKeyedTests` | bench 60 over 156, identical to entry | |
| `WhatTheGeneratorMakesTests` | 2 of 2 | |
| `TheSyntheticCqRebuildsTests` | 13 of 13 | |
| `TheSyntheticCqIsScoredTests` | 1 of 1, 102 over 243 and 63 over 81, as at task 4 | |

- `git diff` over the eleven transmit files against `7e209cb4` prints nothing.
- **`git diff 5f8d48b0 -- src` prints nothing.**
- 1.1 to 1.5 are ticked, and step 1 is marked done in `PHASE_STATUS.md` and `PHASE_OUTCOME.md`.

**Decisions made for myself**, author's and overrulable. No `DECISIONS.md` entry was written.
- **The levels.** I kept the catalogue's 15, 5 and 0 dB, each confirmed as rendered by
  task 1's measurement. None was chosen from a decode.
- **The measurement.** The weak corner was measured by taking the file apart into tone and
  noise, because a direct envelope cannot find edges at 0 dB.
- **The scored region.** Whole-decode scoring trims the gaps at the decode's two ends and
  nothing else.
- **The writer.** It is a fact gated on an environment variable, not a tool.
- **P8.** I parked the missing reference score as P8 rather than calling it a mismatch
  that blocks 1.1.

## 2. What the owner should expect

**What changes for you.** For the first time, some of the decoder's scores rest on keys
nobody had to guess. The computer built these CQ calls itself, so it knows every letter it
sent. When a score says "exact", nobody inferred the letters from the decode or from what a
CQ call usually says. That matters because every earlier number in this phase rests on keys
worked out from the decode, and nobody here reads Morse to check them.

**What the calls show.**
- At 15 and 5 dB the decoder mostly reads a clean call.
- At 0 dB it prints nothing at any speed. So it loses a clean CQ somewhere between 5 and
  0 dB of signal over the noise in the receiver's passband.
- When letters are spaced five dits apart instead of three, it breaks every word into
  single letters, even on a strong signal. That is the same fault seen on the air.

**What these cases cannot tell you:** whether the decoder reads a real operator on a real
band. The spacing, the noise, the single tone and the perfect keying are all the computer's.

**What will look wrong but is not:**
- `tests/fixtures/cw/synthetic-cq` adds 4.5 MB of WAVs.
- The three 0 dB cases score 27 edits over 27 because the decode is empty. That is a
  measurement, not a broken test.
- None of the twelve has a reference score yet (P8).

**Build and tests:** the build is green. Every test named above passes, none fails, and
nothing was lost to the dispatcher loop. Everything was pushed to `main`.

## 3. What you should see

**No visible change.** The CW tab reads exactly as it did yesterday, because no decoder line
changed.

What this unit adds is a way to measure the decoder:
- Twelve CQ calls with exact keys. The five-dit-spaced one reproduces, on a known key, the
  word-breaking fault you have seen on the CW tab.
- A written rule for keying a call heard on the air.

Both give the next spacing repair something to be measured against. It still cannot be kept
on the synthetic cases alone.

## 4. What's blocking us

**Nothing blocks this phase.** Both items below are parked in
`docs/phase-correctness/PARKED.md` under R65, and neither is in the way of a criterion in step 1.

1. **P7: whether the synthetic cases ever join the total 3.2 judges.**
   - **Ruling, if wanted:** they stay outside every total. A synthetic case can support a
     change but never carry it.
   - **Reasoning:** 1.4 forbids a synthetic case from being the sole evidence. A synthetic
     row inside the 217 would let a change that improves only synthetics pass 3.2's first
     test.
   - **Rejected:** adding them to the 217. That breaks 1.4. Also rejected: a second total
     that 3.2 reads. That is a new rule for 3.2, and 3.2 is Tim's.
2. **P8: the synthetic cases carry no reference score.**
   - **Ruling, if wanted:** run `tools/score-fixtures` over `tests/fixtures/cw/synthetic-cq`
     before any synthetic case is used as evidence for a change, with the gate extended to
     that folder.
   - **Reasoning:** HM-DEC-101 and CLAUDE.md 12.5 require a reference read before a
     generated fixture judges Hamlet. The gate runs in Python, which cannot run in a loop
     session. Today these cases judge nothing: no floor, no assertion on a score.
   - **Rejected:** scoring them in C# as a stand-in reference. That is not the validated
     chain.
   - **Note:** the catalogue records that the reference could not read textbook spacing
     at 18 wpm, a dit-to-dah ratio measured at 2.45 against its 2.5 floor. So it may refuse
     some of these cases.

**Asks still outstanding:** none carried as blocking. The instruction's section 3 says unit
413's three asks are all parked and not this unit's to raise. As section 1 reports, only P6
is in `PARKED.md`.
