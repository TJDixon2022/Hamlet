READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 4 at 2 of 7, step 2 at 3 of 5, step 3 at 3 of 6,
   step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 4, criterion 4.1 (HM-REQ-092 measured, N TBD): the instrument's bin 0.5 Hz, 30 of 30
   cases within one bin, cost 24411 us per 1 s instrument hop (118 us per 5 ms decoder hop),
   4.1 ticked; 4.2 69 captures measured, 11 more than 25 Hz off; the tracker change kept,
   against HM-DEC-095's clause that two agreeing surveys rest on six seconds of evidence and
   noise does not repeat in the same bin; MET-CER-SURE 45 -> 40; MET-INVENTED 45 -> 40;
   coverage 374 -> 393; 4.7 red on 17:37.
C. This report adds the instrument, the MET-PITCH-ERR table over the tree, what the tracker weighed
   at 30.54 s, and one kept tracker change. Section 4 raises 3 items; none is in the way of a
   criterion in B. They are: whether 4.3 stands on a test the real set contradicts, whether two
   sub-bin or sign-flipped MET-PITCH-ERR moves count as "rises", and whether a group other than
   the largest could be the one built.

UNIT:       447 - complete at task 4 of 4, none dropped - 2026-09-26 03:46
PHASE GOAL: Hamlet meets every must-tier CW requirement in CW_REQUIREMENTS.md, measured against its own metrics.
UNIT GOAL:  Build a pitch ruler that owes nothing to the tracker, prove it on known tones, read MET-PITCH-ERR over every capture with it, then make one tracker change that puts the decoder on the note the sender is keying.
ADVANCED:   yes - 4.1, 4.2 and 4.3 flipped in PHASE_PLAN.md (4.3 on a synthetic test; see section 4)
NUMBER:     instrument bin 0.5 Hz, worst case 0.024 Hz; >25 Hz cases 11 -> 9; MET-CER-SURE 45 -> 40; MET-INVENTED 45 -> 40
DRIFT:      step 4 0; step 2 2; step 3 0; step 5 1

## 1. What Claude did

**Complete at task 4 of 4, none dropped.** Claude Code on QUIVERFULL, project Hamlet, branch
`main`, HEAD at entry `ae2d6b00`. Commits: `41311294` (task 0), `9b283b20` (task 1), `0bcf2996`
(task 2), `b4884813` (the tracker change, on its own), `c2cae86d` (task 3's record), and the exit
commit. All pushed to `origin/main`.

**Gate.** Hamlet confirmed: the four MUST EXIST files present, `CoreHMI.sln` and `MURC.sln` absent,
root `C:\Source\HamLet`.

**The ids, quoted from the documents.**
- HM-REQ-092: "After acquisition, the decoder shall report the pitch it is demodulating at, within
  N Hz of true (MET-PITCH-ERR ≤ N)." N is "TBD, needs ruling (recommended 5 Hz)". Matches.
- HM-REQ-090: "The decoder shall acquire and track a keyed tone at any pitch from 300 to 900 Hz
  inclusive." Matches.
- HM-REQ-091: "The decoder shall choose the tracked pitch by keying quality and never by level
  alone or by the operator's configured pitch." Matches.
- HM-REQ-102: "While acquiring, the decoder shall emit no sure character." HM-REQ-103: "When a
  run-up precedes the message, the decoder shall not lose the opening characters of the message to
  acquisition." Both match.
- HM-REQ-010, 011 and 012 match as the instruction states them. `CW_SPEC.md` §11: "MET-PITCH-ERR |
  Pitch error | estimated − true, hertz, after acquisition." V-11 as stated.

**Checked against the tree, section 2 of the instruction.**
- `CwToneTracker.cs`, `CwToneSurvey.cs` and `CwPitchChoice.cs` are all present. **Mismatch on the
  bin:**
  - The tracker's coarse bank is 25 Hz (`CoarseSpacingHz`), 300 to 900.
  - A fine bank 5 Hz apart (`FineSpacingHz`, ±15 Hz reach) is what it reports from, with a 1 Hz
    refine step (`RefineStepHz`).
  - The survey works on whatever bins it is given: the coarse 25 Hz bank and the fine 5 Hz bank.
  - "The plan says 25 Hz" describes the coarse stage only.
  - To find a pitch, the tracker calls its own `Goertzel` and `Coefficient`, a Hann taper
    (`BuildHann`, `BuildGateHann`, `Taper`) and `CwToneSurvey.Analyze`. There is no FFT.
- **30.54 s is in `cw-2026-09-24-003919`.** The splice puts `003919` at stream 30.00 to 46.20 s,
  after 13.8 s of it overlapped `003901`.
- `cw-2026-09-24-135641` and `-152135` are not in the tree. Confirmed; not added.
- The generator is `CwFixtureGenerator` (`tests/.../Cw/Fixtures`). It keys a tone at a stated pitch
  over a shaped noise band (350 to 870 Hz, out-of-band 30 dB down, never digital silence).
  `SyntheticCq.Recipe` gives PARIS timing.
- Known and not mine, reported once each, none edited:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165 while `CLAUDE.md` §1 holds CPS-DEC-0183.
- Expected failures at entry, as stated: 17:37's named floor red (46 banked, reads 38); the app
  line lost 1 at entry and 5 at exit, each losing type green alone.

**Task 0, the entry.**
- Version 1.13.133 -> 1.13.134. `PHASE_STATUS.md` names 447 at `CURRENT_STEP: 4` in both copies.
- Build 0 errors in 17 s. Engine carry-forward 178 of 178 in 375 s. App carry-forward 277 of 278
  in 154 s; the loss, `TheCarrierHoldsTheButtonsTests`, is 8 of 8 alone.
- Floors: captures 51 of 51 in 132 s; adjudicated 13 of 13 in 33 s; named 12 of 13 in 69 s.
- Metrics identical to 446's exit.
- The runner's `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were committed as they
  were, with the 447 entry added.

**Task 1, the instrument (4.1).** `CwPitchInstrument`, in
`tests/Hamlet.RadioEngine.Tests/Cw/Instruments/`.
- **Under the tests, because only a measurement uses it.** A type under `src` would sit in the
  product for something to call, and R76 and §12.5 need it out of the decode path.
- **What it references:** `Math`, `float[]`, `double[]`, `List<T>`, `Array.Sort`, value tuples and
  its own `PitchWindow` record. No `using` line at all. `ItSharesNoCodeWithTheTracker` reads its
  source and finds no code line naming `CwToneTracker`, `CwToneSurvey`, `CwPitchChoice`, Goertzel,
  `Hamlet.RadioEngine.Cw` or `.Audio`, `CwDecoder`, `CwProbabilistic`, `CwCompetitor`,
  `ToneReading` or `ToneVerdict`. It reads no tracker state.
- **Method, stage 1: where keying is.**
  - A radix-2 FFT and Hann written in the file, on frames of the next power of two above 30 ms,
    a quarter-frame apart.
  - Each bin from 300 to 900 Hz is taken relative to the frame's median bin and smoothed over
    three frames. Frames 30 dB under the median frame count as the receiver muted and are
    dropped.
  - In each 2 s window, stepped 1 s, the bin that swings hardest (90th over 20th percentile) is
    where somebody is keying, if all of these hold:
    - its swing is at least 12 dB;
    - its 90th percentile stands at least 8 dB over the median bin;
    - at least 0.3 s is keyed.
  - Its frames above the midpoint are the marks. None of this decision comes from the tracker.
- **Method, stage 2: what the note is.**
  - A direct Fourier sum over only the marks' samples, under a 2 s Hann.
  - Evaluated every 0.25 Hz across one coarse bin plus 2 Hz either side, then every 0.02 Hz
    across ±0.25 Hz, then a parabola through the best three.
  - **Bin 0.5 Hz**, one over the window.
- **The cases.** 30 in all, every one built by the generator at 15 dB over its noise band, exact
  keys, the generator's 3 Hz swing set to 0:
  - nine pitches (300, 400, 523, 600, 625, 640, 675, 750 and 900 Hz) at 12, 20 and 30 WPM;
  - 625 Hz at 20 WPM and 5 dB;
  - 625 Hz keyed under a continuous 525 Hz carrier 6 dB louder;
  - 615 Hz sliding 20 Hz per 30 s over a 35 s message. For this, `CwFixtureRecipe` gained
    `DriftHzPerSecond`, default 0, guarded so every existing recipe renders sample for sample as
    before.
- **The test.** `ThePitchInstrumentIsProvedTests.EveryCaseLandsWithinOneOfItsOwnBinsOfTheTruth`,
  naming HM-REQ-092 and 4.1.
  - It failed first against a truth 1 Hz wrong: 0 of 30 inside, worst 1.024 Hz.
  - Then it passed: 30 of 30 inside, worst 0.024 Hz.
  - A first build with a flat ±20 Hz fine reach missed 900 Hz and three off-bin cases (−5 to
    −13.5 Hz). The reach became one coarse bin plus 2 Hz before the pass.
- **Cost.** On `cw-2026-09-24-003919`, 48 kHz, 30 s, median of three runs: 707.9 ms, which is
  **24411 us per 1 s instrument hop** and 118.0 us per 5 ms decoder hop.
- 30 of 30 cases admitted. 4.1 ticked in task 1's commit.
- **Decision made for myself, reported in full.** The instrument was revised in task 2, after the
  tick, and re-proved on the same 30 cases (30 of 30, worst 0.024 Hz); the figures above are the
  revised instrument's. The first real run found "keying" at 780 to 896 Hz, at and past the
  receiver's filter edge, swinging with the station being read. That is the radio's AGC pulling
  the band's noise down on key-down while a bin outside the filter does not move. The revision
  added one rule: a bin counts only if its keyed level stands 8 dB over the median bin. A tone
  keyed at 5 dB in the passband stands about 15 dB over it. The pre-revision cost was 31944 us
  per hop.

**Task 2, MET-PITCH-ERR over the tree (4.2).** `WhatPitchTheDecoderIsOnTests`, a printer that
asserts nothing.
- **What it covers:** every WAV under `tests/fixtures/cw/captured`, 69 of them. They hold the 51
  capture rows, the 23 keyed recordings and all 14 files of the 7.052 session.
- **What it compares:** the decoder is driven cold from 600 Hz, hop by hop. The tracker's `ToneHz`
  is sampled on every hop and its median taken over each 2 s instrument window where keying was
  found. MET-PITCH-ERR is tracker less instrument; a file's figure is the median over its windows.
- **What it found:** all 69 measured; keying found in all 69. **11 files more than 25 Hz off**,
  named in section 3. 61 files have at least one window more than 25 Hz off, 304 of 1688 windows.
- `135641` and `152135` are absent and unmeasured.
- 4.2 ticked.
- One definitional choice: the error is taken over every hop in a keyed window, before
  acquisition as well as after. `CW_SPEC.md` §11 says "after acquisition". The move log below
  shows what that choice includes.

**Task 3, what the tracker weighed, and one change.**
- **At 30.54 s.** The existing printer `WhatTheOpeningHeardTests.WhyTheMixMoved("stream", 28, 38)`
  shows the tracker weighed the survey's keying score and the confirmation rule:
  - 525 Hz was admitted at 30.04 and 30.54 s: separation 4.57 and 4.60 against the 4.0 limit,
    ratio 3.59 and 3.33, 18 marks, lift 2.4 and 2.5 dB.
  - It was confirmed twice. Nothing had yet been confirmed, so `_readingDb` was NaN and
    HM-DEC-127's abandon rule had nothing to protect.
  - The tracker switched 600 -> 525.
  - The instrument puts the keying at 598.7 to 601.0 Hz across 30 to 34 s, with swings of 34 to
    37 dB.
  - **This does not agree with R75's "the sender's actual 625 Hz"** for that stretch. The
    instrument finds 625 to 628 Hz only from 76 s on.
- **The groups.** A new fact, `WhatTheTrackerWeighedWhereItMoved`, logs every move of more than
  15 Hz on the 11 files:
  - **A:** 51 moves on a read with no keyed verdict. 48 are the from-cold "point at the loudest"
    move (`CwToneTracker.cs` 1014 to 1025), in 9 of the 11 files, made before anything is
    confirmed.
  - **B:** all 8 moves made on a keyed verdict went to a candidate under 10 dB of lift, from +2.7
    down to −35.1 dB, in 5 files.
  - The opening's 525 at 2.5 dB is group B.
  - **Decision made for myself:** I built against B, not the largest group A. Reasons:
    - A happens before acquisition, and MET-PITCH-ERR is defined after it.
    - A is the pre-confirmation fallback, whose removal the tracker's own comments record as
      costing openings.
    - B holds the opening, which is what R83 and this unit's ACCOMPLISHED line aim at.
  - Raised in section 4.
- **The change, `b4884813`.** In `ReadSurvey`, after HM-DEC-095's twice-confirmation, a candidate
  outside the fine bank's reach whose measured lift is under `CwToneSurvey.InterferenceLiftDb`
  (10 dB, the survey's own line for "band noise having a good moment") is not moved to.
  - **It works against HM-DEC-095's clause** "Two agreeing surveys half a second apart rest on
    six seconds of evidence, and noise does not repeat itself in the same bin". The two surveys
    share 2.5 of their 3 s, and on the opening the noise did repeat.
  - The choice is still made by keying structure.
  - The instrument is not called by the tracker.
  - G1, the marks' speed, the dim edge, `RivalMargin`, `MarginLlr` and the speed bounds are
    untouched.
- **Judged under R78: kept.** The full table is in `docs/phase-requirements/metrics.md` under
  unit 447. Every item as a number:
  - MET-CER-SURE, real, inferred: 45 of 419 -> 40 of 433. Synthetic, exact: 14 of 173 unchanged.
  - MET-INVENTED, real: 45 -> 40 over 473. Synthetic: 14 -> 14.
  - Sure-and-right coverage, real: 374 -> 393 over 473. Synthetic: 159 -> 159.
  - MET-WBE, real: 52 -> 47 over 113.
  - Adjudicated 13 of 13.
  - V-11: 35 recordings, 0 worse; the one that moved is `032129`.
  - MET-PITCH-ERR: 11 -> 9 files more than 25 Hz off, 304 -> 256 windows.
  - Task 1's cases through the tracker are unchanged.
- **Floors.** Captures 51 of 51. Named floors: `032113` 45 -> 43 and `032129` 64 -> 42, reported
  with text, not re-banked.
- **4.3.** `TheTrackedPitchIsChosenByKeyingTests` names HM-REQ-091 and is green before and after
  the change, so 4.3 is ticked. Whether HM-REQ-091 is met: **on these two synthetic cases yes; on
  the real set no.** Group A's 48 moves to the loudest bin are choice by level alone.

**Task 4, the exit round.**
- Build 0 errors in 15 s.
- Engine carry-forward 178 of 178 in 376 s.
- App carry-forward 273 of 278 in 168 s. The 5 losses are `TheRecordNamesTheSubModePressedTests`
  (4), 12 of 12 alone, and `BindingHealthTests` (1), 1 of 1 alone.
- Captures 51 of 51 in 130 s. Adjudicated 13 of 13 in 31 s.
- Named 10 of 13 in 66 s: 17:37 at 38, `032113` at 43, `032129` at 42.
- Metrics as under the change.
- Every type touched: TOUCHED_RESULTS.
- **`src` changed in one file, `src/Hamlet.RadioEngine/Cw/CwToneTracker.cs`**, +30 lines, one
  guard in `ReadSurvey`. It changes which pitch the receive filter points at. Nothing in it keys
  or transmits. Pushed.
- **Ticks:** 4.1 (task 1), 4.2 (task 2) and 4.3 (exit) ticked. 4.4 to 4.7 not ticked, nor anything
  in steps 2, 3 or 5. 4.7 stays red on 17:37, and now also on `032113` and `032129`.
- DRIFT: step 4 0, the change kept; step 2 2; step 3 0; step 5 1.

## 2. What the owner should expect

**The decoder now stays on the note being keyed in more of the places it used to wander from, and
the operator reads more of what was sent where that happened.** On the 7.052 opening the decoder
no longer jumps from 600 to 525 Hz at 30.54 s. From 30 to 34 s the instrument puts the keyer at
599 to 601 Hz, and the decoder now sits at 600 there, about 1 Hz off, where it used to be 74 to
76 Hz off. The stretch from 30 s now reads `EANQNID EAN■IK`, the same run of letters `003919` reads
decoded on its own, where it read `UIEH EE E E T I <HH> EA N ■IK`. On the W1AW bulletin `032129`
the tail of E and I junk becomes `FORECAST BUAELETIN ARLP034`, and that one recording carries every
metric movement: against its inferred key, wrong-or-added 10 -> 5 and right 15 -> 34.

**What reads worse, or looks wrong but is not:**
- `032129` and `032113` now fail their named-character floors: 64 -> 42 and 45 -> 43. The
  characters lost are junk E and I; the scored stretches are better or unchanged. R78 reports
  this and does not reject it.
- On the opening from 45 to 49 s the decoder now sits at 625 instead of 575. The instrument says
  600 there, so it is 25 Hz off on the other side.
- On `002424` and `002443` the decoder no longer moves to 475 and stays at the configured 600. The
  instrument says 562, so the error goes from −87 to +38 Hz. The decoder is still off there, just
  by less and the other way.

**The evidence is thin on the real side (V-13).** Every real metric movement is one recording
against an inferred key, and the opening has no key at all. **The synthetic cases prove the
ruler, not the decoder.** They show the instrument lands within 0.024 Hz on tones it was built
against. They do not show it reads real audio right: it needed one real-audio fix already (the
AGC edge), and it still finds "keying" in the four recordings the floors call empty. Its reading
of 600 rather than 625 at the opening's 30 to 34 s is a measurement nobody has checked by ear.

## 3. What you should see

**The instrument (4.1): bin 0.5 Hz, 30 of 30 within one bin, 24411 us per 1 s hop (118 us per 5 ms
decoder hop).** The truth, the estimate (median of windows) and the worst error, in Hz; every case
is inside one bin:

| case | truth | estimate | worst error |
|---|---|---|---|
| 300, 12 / 20 / 30 WPM | 300 | 300.00 / 300.00 / 300.00 | +0.002 / −0.003 / −0.003 |
| 400, 12 / 20 / 30 | 400 | 400.00 each | −0.006 / +0.004 / −0.015 |
| 523, 12 / 20 / 30 | 523 | 523.00 each | −0.010 / +0.006 / −0.006 |
| 600, 12 / 20 / 30 | 600 | 600.00 each | +0.012 / −0.006 / −0.010 |
| 625, 12 / 20 / 30 | 625 | 625.00 each | +0.006 / −0.007 / +0.011 |
| 640, 12 / 20 / 30 | 640 | 640.00 each | +0.014 / −0.011 / −0.005 |
| 675, 12 / 20 / 30 | 675 | 675.00 each | −0.008 / +0.006 / +0.006 |
| 750, 12 / 20 / 30 | 750 | 750.00 each | +0.003 / −0.004 / +0.015 |
| 900, 12 / 20 / 30 | 900 | 900.00 each | −0.003 / +0.003 / −0.002 |
| 625, 20 WPM, 5 dB | 625 | 625.00 | +0.022 |
| 625 keyed, 525 carrier +6 dB | 625 | 625.00 | +0.007 |
| 615 sliding +20 Hz / 30 s | 615 + 0.667 t | 615.88 to 638.87 | −0.024 |

**MET-PITCH-ERR over the tree at entry, the `>25` cases first.** This is tracker less instrument,
the median over the file's keyed windows, then windows more than 25 Hz off:

| file | tracker | instrument | MET-PITCH-ERR | windows >25 |
|---|---|---|---|---|
| 08-20-014935 | 625.0 | 580.0 | +45.0 | 8 of 10 |
| 08-22-014308 | 575.0 | 607.2 | −32.2 | 16 of 22 |
| 08-22-031838 | 525.0 | 499.9 | +25.1 | 21 of 29 |
| 08-22-032129 | 650.0 | 499.9 | +150.1 | 17 of 29 |
| 08-25-012823 | 450.0 | 500.3 | −49.2 | 18 of 27 |
| 08-28-005158 | 575.0 | 607.4 | −34.1 | 12 of 17 |
| 08-28-005218 | 750.0 | 607.8 | +150.7 | 28 of 29 |
| 08-28-005243 | 575.0 | 607.2 | −32.1 | 15 of 29 |
| 08-31-002424 | 475.0 | 561.8 | −86.8 | 6 of 6 |
| 08-31-002443 | 475.0 | 561.3 | −86.3 | 7 of 7 |
| 08-31-002829 | 800.0 | 612.5 | +183.6 | 17 of 29 |

- The other 58 are within 25 Hz on their median, 50 of them with at least one window more than
  25 Hz off. The full table is `.run-unit/unit447-pitch-table-head.txt`.
- The opening's files, one by one: `003901` +0.4, `003919` +0.3, `004027` −0.3, `004108` −1.3,
  `004133` −1.7, `004205` −1.6, `004234` −1.8.
- `135641` and `152135`: not in the tree, unmeasured.
- After the change, 9 files are more than 25 Hz off; `032129` (+0.1) and `002829` (+7.5) leave the
  list.

**The 7.052 opening (003901 to 004234 spliced; 30.54 s is in 003919):**

| | before | after |
|---|---|---|
| text, 30 to 46.2 s | `UIEH EE E E T I <HH> EA N ■IK` | `EANQNID EAN■IK` |
| decoder's pitch at 30 to 34 s | 525 | 600 |
| instrument, 30 to 34 s | 598.7, 599.7, 601.0 | the same |
| MET-PITCH-ERR, 30 to 34 s | −73.7, −74.7, −76.0 | +1.3, +0.3, −1.0 |
| first sure character | `E` at 20.64 s, at 600 Hz | the same |

The characters from 0 to 30 s (`EII E T NHHK`, all sure, at 600) are unchanged. HM-REQ-102 and 103
are printed here and not judged; that is 4.5.

**Every recording whose text changed, with its key:**
- `032129`, inferred: `■ MTMTJ26 PGOPAGATION E EE [E]IIEE I [E] EE [I]EEEE I HE EE I E E S E E E E
  IEEE E I SE IE I E E EEI` -> `■ MTMTJ26 PGOPAGATION FORECAST BUAELETIN ARLP034`
- `032113`, inferred, the change outside its scored stretch: `... 200J6 I I I TT E EI EEEEI I E E`
  -> `... 200J6 I I I TT E E I I I WE T I`
- `003919`, no key: `EITEETNXNIK EANQNID EANQNIK` -> `EITEETNXNIK EANQNID EANQNIK E`

What the operator would see at the radio: on a call like the 7.052 opening, the decoder stays on
the station it hears keying instead of jumping to a quiet tone 75 Hz away, so the letters in that
stretch come from the station.

## 4. What's blocking us

Nothing blocks a criterion in B. Three items need a ruling for the record:

1. **4.3 is ticked on a synthetic test while the real set shows HM-REQ-091 unmet.**
   - Proposed ruling: 4.3's "the report states whether it is met" is satisfied by stating
     "no, on the real set". The tick stands.
   - Reasoning: the plan line asks for a test naming the requirement and a statement, not that the
     requirement be met.
   - Rejected: leaving 4.3 unticked, which the instruction's own test for it ("a test naming
     HM-REQ-091 written and green") does not support.
2. **Whether R78's "rises on none" was met.** Two MET-PITCH-ERR movements need a reading:
   - `005051`'s median moved +4.2 -> +4.4 Hz, inside the instrument's 0.5 Hz bin, while its
     windows more than 25 Hz off fell 5 -> 3.
   - Three opening windows at 45 to 49 s moved from −24.2/−24.9/−25.0 to +25.8/+25.1/+25.0.
   - Proposed ruling: neither is a rise, the first being below the ruler's bin and the second a
     sign change of the same size within 1.6 Hz.
   - Rejected: refusing the change, which would give back the 30 to 34 s fix and `032129`'s text
     over sub-bin and same-size movements.
3. **The largest group was not the one built.**
   - Group A is the from-cold move to the loudest bin, before anything is confirmed: 48 moves in
     9 of 11 files, and choice by level alone under HM-REQ-091.
   - Proposed ruling: a later step 4 unit takes group A, since that is where HM-REQ-091 actually
     fails.
   - Reasoning: A is before acquisition, which MET-PITCH-ERR excludes, and the tracker's own
     comments record that removing it costs openings. B holds the opening R83 aims at.
   - Rejected: building against A this unit, as one change per unit.
