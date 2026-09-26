READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 4 at 4 of 7, step 2 at 3 of 5, step 3 at 3 of 6,
   step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 4, criterion 4.5 (HM-REQ-102 and 103 on the 7.052 opening): acquiring 0.00 to 26.04 s;
   sure characters while acquiring 9 -> 9; opening characters read sure and right 21 -> 21
   of 24; 4.5 ticked. 4.3 ticked at task 0. The from-cold change not kept; level-alone moves
   48 -> 48 (0 of 162 under the refused change); HM-REQ-091 on the real set not met;
   MET-CER-SURE 40 -> 40 (64 under the refused change); MET-INVENTED 40 -> 40 (64);
   coverage 393 -> 393 (313); 4.7 red on 17:37, 032113 and 032129.
C. This report adds the acquiring trace, which shows HM-REQ-102 failing because the decoder
   prints sure letters before the tracker confirms anything (9 on the opening, 245 on the keyed
   23), and one refused tracker change. Section 4 raises 4 items; item 1 stands in the way of
   HM-REQ-102 being met, not in the way of 4.5, which is a measurement and is ticked.

UNIT:       448 - complete at task 3 of 3, none dropped - 2026-09-26 05:16
PHASE GOAL: Hamlet meets every must-tier CW requirement in CW_REQUIREMENTS.md, measured by the metrics CW_SPEC.md defines.
UNIT GOAL:  Measure what the decoder prints on the 7.052 opening while it is still finding the station, before and after one change to where the tracker points its filter before anything is confirmed.
ADVANCED:   yes - 4.3 (task 0, 447's lost tick) and 4.5 (task 2, the measurement) flipped in both PHASE_PLAN.md copies
NUMBER:     sure while acquiring 9 -> 9; opening sure-right 21 -> 21; level-alone moves 48 -> 48; MET-CER-SURE 40 -> 40; MET-INVENTED 40 -> 40
DRIFT:      step 4 1; step 2 2; step 3 0; step 5 1

## 1. What Claude did

**Complete at task 3 of 3. Nothing was dropped: the keyed 23 were traced too.** Claude Code on
this machine, project Hamlet, branch `main`, HEAD at entry `c2cae86d`. Commits: `0804fd31`
(task 0), `3c4e1f75` (task 1), `2c3048d2` (task 2, the refused change as a diff), and the exit
commit. All pushed to `origin/main`. The change was not kept, so it never had a commit of its own
in `src`. Its diff and its test are committed in `.run-unit/unit448-coldmove-notkept.diff`.

**Gate.** Hamlet confirmed: the four MUST EXIST files are present, `CoreHMI.sln` and `MURC.sln`
are absent, and the root is `C:\Source\HamLet`.

**The ids, quoted from `CW_REQUIREMENTS.md`.** Every one matches the instruction's summary. None
is a mismatch.
- HM-REQ-102: "While acquiring, the decoder shall emit no sure character."
- HM-REQ-103: "When a run-up precedes the message, the decoder shall not lose the opening
  characters of the message to acquisition."
- HM-REQ-091: "The decoder shall choose the tracked pitch by keying quality and never by level
  alone or by the operator's configured pitch."
- HM-REQ-090: "The decoder shall acquire and track a keyed tone at any pitch from 300 to 900 Hz
  inclusive."
- HM-REQ-092: "After acquisition, the decoder shall report the pitch it is demodulating at, within
  N Hz of true (MET-PITCH-ERR ≤ N)." N is "TBD, needs ruling (recommended 5 Hz)". Measured here,
  not judged.
- HM-REQ-010, 011 and 012: MET-CER-SURE below 1 %, MET-INVENTED at zero, and MET-COVERAGE ≥ 0.90
  at the sensitivity floor.
- `CW_SPEC.md` §11 defines MET-TACQ ("first element on the air to first sure character, under a
  named run-up") and MET-PITCH-ERR ("estimated − true, hertz, after acquisition").

**"Acquiring" is defined in neither document.** MET-TACQ ends at the first sure character, and
using that would make HM-REQ-102 true by definition. So the instruction's fallback is used:
acquiring runs from the start of the file to the tracker's first keyed verdict, which is
HM-DEC-095's twice-confirmation.
- In code, that verdict is the first hop on which `_lastKeyedHz` is set. The printer reads it by
  reflection.
- `_lastKeyedHz` is never cleared, so the tracker has no "keyed signal after silence" state. Each
  file has one acquiring span, starting at its first sample.

**Checked against the tree, section 2 of the instruction.**
- **447's lost tick: confirmed.** 4.1 and 4.2 were ticked and 4.3 was not, in both copies of
  `PHASE_PLAN.md`.
- **The from-cold move: confirmed.** It is at `CwToneTracker.cs:1014-1025` (comment from 996),
  guarded by `!MidCharacter`, `double.IsNaN(_lastKeyedHz)`, `coarse.Strongest` and the fine
  bank's 15 Hz reach. It runs only when `coarse.Keyed` is null.
- **What `Strongest` ranks by: lift alone.** Lift is a bin's key-down cluster level over the
  median of the baselines of bins at least 125 Hz away. It is taken over every bin, keyed or not,
  and is reported only at 10 dB or more (`CwToneSurvey.Analyze`).
- **Mismatch: `WhatTheTrackerWeighedWhereItMoved` cannot be run over the opening or the keyed 23
  as it stands.** It is hard-wired to the 11 files of `OffAtEntry`, and its "from cold" tally
  counts every move of more than 15 Hz with no keyed verdict. I wrote a new printer rather than
  edit it. My count is every retune while nothing is confirmed: 60 on the 11 files, with at least
  one in each file. 447 counted 48 in 9 of the 11.
- **`TheTrackedPitchIsChosenByKeyingTests` holds two facts:**
  - `NeitherALouderCarrierNorTheConfiguredPitchHoldsTheTracker`, with two cases: 625 Hz keyed
    beside a 525 Hz carrier 6 dB louder, and 675 Hz with the decoder started at 600.
  - `TheInstrumentCasesThroughTheTracker`, a printer.

  **Yes, the carrier case starts cold beside a louder unkeyed carrier.** It asserts only the
  pitch over the last half of the message, so it never looks at the from-cold move.
- **The opening: confirmed.** 003901 to 004234 are spliced as `WhatTheOpeningHeardTests` does it.
  `003919` covers stream 30.00 to 46.20 s, and its file starts at 16.20 s, so 30.54 s is in
  `003919`.
- **Expected failures, as stated:**
  - The named floors: 17:37 38 against 46, `032113` 43 against 45, `032129` 42 against 64.
  - The app line lost 1 at entry and 3 at exit. Each losing type is green alone.
- **Not expected: the first entry run of the app line aborted at the 480 s timeout.** The test
  host hung after 132 had passed (`.run-unit/unit448-cf-app-entry.txt`). One rerun gave 277 of
  278 in 163 s.
- **Known and not mine.** Each is reported once and none was edited:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - R75 names the sender at 625 Hz at 30.54 s. The instrument finds 598.5 to 600.0 Hz from 20 to
    26 s here, and 447 found 599 to 601 at 30 to 34 s.
- **Mismatch with the instruction's premise.** The instruction reasons that the from-cold move
  "decides what the decoder hears while it acquires" on the opening. **On the opening it makes no
  move at all.** The station is on the configured 600 Hz, and the tracker never retunes before
  its first keyed verdict at 26.04 s. So no from-cold change can move 4.5's numbers there, and
  none did.

**Task 0.**
- Version 1.13.134 -> 1.13.135. `PHASE_STATUS.md` names 448 at `CURRENT_STEP: 4` in both copies.
- The runner's and 447's uncommitted writes were committed as they were, with the 448 entry:
  `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `PROJECT_STATUS.md`, `RUN_LEDGER.md`, 447's
  `output.md` and `WORK_INSTRUCTIONS.md`.
- **4.3: `TheTrackedPitchIsChosenByKeyingTests` was 2 of 2 alone in 35 s, so 4.3 is ticked in
  both copies.** Under 447's tick rule, its statement is 447's: HM-REQ-091 is met on the synthetic
  cases and not met on the real set.
- Entry round:
  - build: 0 errors in 16 s;
  - engine carry-forward: 178 of 178 in 373 s;
  - app carry-forward: 277 of 278 in 163 s on the rerun; the loss, `ThePsk31OfferTests`, is 2
    of 2 alone;
  - floors: captures 51 of 51 in 130 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 66 s;
  - metrics: as 447's exit (the table is in section 3);
  - MET-PITCH-ERR, with 447's printer: 69 of 69 measured, **9 files more than 25 Hz off**, 256
    of 1688 windows.

**Task 1: the trace, `WhatTheDecoderDoesWhileAcquiringTests`.** A printer that asserts nothing,
with three facts: the opening, the eleven, and the keyed 23.
- **Its keying score.** At HEAD no bin is ever admitted at a from-cold move, because the move
  runs only when the verdict holds no keying. So "the survey's keying score" needed a reading
  that exists for every bin.
  - `CwToneSurvey.Structures()` and `CwToneTracker.CoarseStructures()` report each bin's lift,
    key-down over key-up contrast, clean marks, and whether the survey admits it. They are
    diagnostic only.
  - The printer's score: of the bins at least 10 dB over the band with at least 8 clean marks,
    the one keyed deepest. A move is by **level alone** if it went anywhere else, or if no bin
    shows keying.
  - Every recording's text at this commit is identical to 447's, all 126 lines.
- **Results at HEAD:**
  - **The opening:** acquiring 0.00 to 26.04 s; first keyed verdict at 600 Hz; 0 cold moves;
    **9 sure characters while acquiring**.
  - **The eleven:** 60 cold moves, **48 by level alone**.
  - **The keyed 23:** 26 cold moves, 23 by level alone. **245 sure characters were printed while
    acquiring.** Many of them are right against the key. For example, `032113` printed 29 sure
    in its first 20.54 s and `032050` printed 36 in its first 24.04 s.
- **The opening's "sent" text is inferred.** It is each file read alone for the part that file
  contributes: `003901` for 0 to 30 s, and `003919` from 30 s. Because 0 to 30 s is the same audio
  as `003901`, that half is its own reference. So the printer also checks the 16.2 to 30 s
  stretch against `003919`'s own reading of the same audio.

**Task 2: the change, not kept.**
- **The test came first:** `TheFilterGoesToTheKeyingFromColdTests`, naming HM-REQ-091 and 102.
  - The case: 700 Hz keyed at 15 dB over the shaped band, an exact key, and a steady 790 Hz
    carrier 6 dB louder, 90 Hz away. The decoder starts cold at 600.
  - **At HEAD it was red.** The cold move went 600 -> 800, the carrier's bin. The filter sat on
    the carrier for 700 of 1007 hops before the 5.04 s verdict, and the carrier printed `TT`
    sure.
- **The method.** While nothing is confirmed and no bin is admitted, the fine bank is pointed at
  the survey's new `DeepestKeying`: the bin keyed deepest among those 10 dB over the band with
  8 clean marks. Where no bin shows that, the filter stays where it is.
  - It replaces the move; it does not remove it.
  - The confirmation, HM-DEC-095's carrier guard, 447's under-10 dB guard and HM-DEC-127 are
    untouched. So are G1, the marks' speed, the dim edge, `RivalMargin`, `MarginLlr` and the
    speed bounds.
  - The instrument is not called.
- **The clause, per R75.** The thing changed is **a clause of neither** HM-DEC-095 nor HM-DEC-127.
  Both concern what the tracker does with a keying candidate, and this move runs only when there
  is none. What records it is the tracker's own "FROM COLD, POINT AT THE LOUDEST THING" comment.
- **The test under the change:**
  - **Its HM-REQ-091 half passed.** The filter went 600 -> 700 at 3.54 s and spent 0 hops on the
    carrier. The text went from `TT IK DE N0CALL N0CALL K` to `IQ CQ DE N0CALL N0CALL K`.
  - **Its HM-REQ-102 half stayed red:** `IQ` was printed sure before the 5.04 s verdict.
- **R78 refused it on every item** (the table is in section 3). Real MET-CER-SURE went 40 of 433
  -> 64 of 377, and coverage 393 -> 313. Files more than 25 Hz off went 9 -> 16.
- **It is out of `src`.** The diff and the test are in `.run-unit/unit448-coldmove-notkept.diff`,
  and the test's source is also in `.run-unit/unit448-TheFilterGoesToTheKeyingFromColdTests.cs.txt`.
  No red test was left in `tests`. It is recorded in `docs/phase-requirements/metrics.md`. No
  second change was built.
- **4.5 is ticked.** HM-REQ-102 and 103 were measured on the opening, at HEAD and with the change
  built, and printed as text.
  - **HM-REQ-102: not met, before and after.**
  - **HM-REQ-103: 21 of 24, before and after.**

**Task 3: the exit round.**
- Build: 0 errors in 8 s.
- Engine carry-forward: 178 of 178 in 377 s.
- App carry-forward: 275 of 278 in 169 s. The losses are `TheWindowHoldsBelowItsMinimumTests`
  (3 of 3 alone) and `TheRecordNamesTheSubModePressedTests` (12 of 12 alone).
- Floors: captures 51 of 51 in 129 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 66 s, as
  at entry.
- Metrics: identical to entry.
- Every type touched, each run alone:

  | type | result | wall time |
  |---|---|---|
  | `CwToneSurveyTests` | 5 of 5 | 5 s |
  | `CwSurveyThresholdPinTests` | 3 of 3 | 8 s |
  | `CwTrackerSwitchTests` | 2 of 2 | 2 s |
  | `TheTrackedPitchIsChosenByKeyingTests` | 2 of 2 | 34 s |
  | `WhatTheDecoderDoesWhileAcquiringTests` | 3 of 3 | 159 s |

- **`src`, file by file, `c2cae86d` to exit:**
  - `src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs`, +60 -2: the `KeyedStructure` record, the
    contrast and mark count out of `Examine`, and the diagnostic `Structures()`.
  - `src/Hamlet.RadioEngine/Cw/CwToneTracker.cs`, +4: the diagnostic `CoarseStructures()`.

  Neither changes a decision: the text of every recording is identical. **None of it keys or
  transmits.** Pushed.
- **Ticks:**
  - 4.3 was ticked at task 0, and 4.5 at task 2.
  - **HM-REQ-091 on the real set: not met.** Level-alone moves on the eleven and the opening were
    48 of 60 before, and still 48 of 60 at exit, since nothing was kept.
  - 4.4, 4.6 and 4.7 are not ticked, and nothing in steps 2, 3 or 5 is. **4.7 stays red** on
    17:37, `032113` and `032129`.
- DRIFT: step 4 1 (not kept), step 2 2, step 3 0, step 5 1.

**Decisions made for myself, in full.**
1. **The printer's keying score is my own construction.** Nothing is admitted at a from-cold
   move, so the survey's own measure, separation, does not exist there.
   - I chose key-down over key-up contrast among bins with the survey's 10 dB lift line and
     8-mark line, and I fixed it in task 1, before the change was built.
   - The change used the same score. So its "level alone 48 -> 0" is true by construction, and
     the instrument and R78 are the independent check.
2. **The survey's first admission is ignored as a pointer.** It was a fluke on the opening
   (850 Hz, 19 dB *below* the band, at 1.60 s). On the eleven it came late or at the configured
   pitch (`032129`: 600 Hz at 4.50 s, where the station is at 500). A change that points only at
   admitted candidates would have lost `032129` outright.
3. **The test lives in its own type.** Its HM-REQ-102 half was red at HEAD for a reason no
   pointing can fix. Putting it inside `TheTrackedPitchIsChosenByKeyingTests` would have turned
   red the type 4.3 was ticked on.
4. **The capture and named floors were not run under the refused change.** R78 had already
   refused it on its metrics.

## 2. What the owner should expect

**No, the decoder does not now listen to the keying station from the first seconds. Nothing the
operator sees has changed in this unit, and the opening reads as it did.** The one change built
did send the filter to a keyed tone instead of a louder steady carrier in the synthetic case: the
opening `TT IK` became `IQ CQ`. On real recordings, though, nearly every loud bin shows keying,
because of the station's own leakage and the receiver's AGC. Pointing at the deepest keying
chased that from survey to survey: 60 moves became 162. It lost the W1AW bulletin `032129`,
whose loudest bin was the station all along, and it raised sure-but-wrong letters from 40 to 64.
It was refused and is not in the build. **What looks wrong but is not:** the 7.052 opening still
prints 9 sure letters, `EII E T NHHK` from 20.6 to 24.7 s, before the tracker has confirmed a
station. The station there is already on the configured 600 Hz, so no tracker change reaches
it. Those letters come from the decoder, which prints sure before the tracker confirms anything,
on 245 characters across the keyed 23, and many of them are right. **The evidence is thin (V-13).**
Every real key is inferred, the opening has no key, and its "sent" text is the files read alone.
**What the synthetic case does not prove (§12.5):** one carrier, one speed and one level, from
the same generator the instrument was proved on. It shows the move can be made by keying. It
does not show that keying depth picks the station on the air, and the real set says it does not.

## 3. What you should see

**The 7.052 opening, 0 to 46.2 s, before and after: identical, because there was no cold move to
change.** `{ }` marks what settled while acquiring (0.00 to 26.04 s). A character read sure and
right against each file alone stands as itself; anything else is in `( )`.

| | HEAD `3c4e1f75` | under the refused change |
|---|---|---|
| text, marked | `{ EII E T NHHK }  EANQNID  EAN(■)IK` | the same |
| acquiring | 0.00 to 26.04 s; first keyed verdict at 600 Hz | the same |
| HM-REQ-102: sure while acquiring | 9: `E@20.64 I@21.07 I@21.49 E@21.82 T@22.22 N@22.56 H@23.33 H@24.01 K@24.68`, all at 600 Hz | the same |
| the same 9 against `003919`'s own reading (`EITEETNXNIK`) | 6 right, 3 wrong (`I@21.49`, `H@23.33`, `H@24.01`) | the same |
| HM-REQ-103: opening sure and right | 21 of 24 (21 sure emitted), inferred; 12 of them after 26.04 s | the same |
| decoder's pitch, 20 to 26 s | 600.0 | the same |
| instrument, 20 to 26 s | 599.4, 599.7, 598.5, 600.0, 599.6 | the same |
| decoder / instrument, 30 to 34 s (447's figures, HEAD) | 600 / 598.7 to 601.0 | the same |

**HM-REQ-102 is not met on the opening, and HM-REQ-103 cannot be judged: 21 of 24 is measured
against an inferred text, and its reference for 0 to 30 s is the same audio.**

**The from-cold moves, the eleven, before -> under the refused change.** Each file shows its
first move with the lift and keying of where it went and of the best-keyed bin (contrast is
key-down over key-up). Every line is in `.run-unit/unit448-acq-eleven-head.txt` and
`-change.txt`.

| file | moves, level alone | first move at HEAD | first move under the change |
|---|---|---|---|
| `014935` | 16, 15 -> 23, 0 | 600 -> 625, loudest 34.8 dB, contrast 10.0; best-keyed 675 at 32.7 dB, 10.6 | 600 -> 675 |
| `014308` | 8, 8 -> 34, 0 | 600 -> 575, 18.6 dB, 7.8; best 600 at 12.9 dB, 12.1 | 600 -> 550 |
| `031838` | 1, 1 -> 2, 0 | 600 -> 500, 56.5 dB, 25.9, 7 marks; best 450 at 42.0 dB, 23.3 | 600 -> 450 |
| `032129` | 1, 1 -> 11, 0 | 600 -> 500, 53.8 dB, 19.1, 7 marks; best 450 at 38.9 dB, 20.6 | 600 -> 450 |
| `012823` | 2, 0 -> 3, 0 | 600 -> 400, 48.2 dB, 13.5; it was the best | the same |
| `005158` | 13, 12 -> 28, 0 | 600 -> 425, 54.4 dB, 10.0; best 525 at 52.8 dB, 10.6 | 600 -> 525 |
| `005218` | 1, 1 -> 25, 0 | 600 -> 575, 58.3 dB, 13.1, 4 marks; best 475 at 52.2 dB, 10.7 | 600 -> 475 |
| `005243` | 1, 1 -> 23, 0 | 600 -> 575, 58.4 dB, 13.1, 4 marks; best 500 at 46.7 dB, 11.2 | 600 -> 500 |
| `002424` | 2, 1 -> 2, 0 | 600 -> 750, 10.3 dB, 13.1; it was the best | the same |
| `002443` | 2, 1 -> 2, 0 | the same as `002424` | the same |
| `002829` | 13, 7 -> 9, 0 | 600 -> 625, 14.8 dB, 13.0; best 600 at 14.4 dB, 14.4 | 600 -> 625 at 7.04 s |
| the opening | 0, 0 -> 0, 0 | none | none |
| **total** | **60, 48 -> 162, 0** | | |

No bin was admitted by the survey at any of them. The instrument puts `031838` and `032129` at
499.9 Hz, so there the loudest bin was right and the deepest keying was 50 Hz off.

**R78, every item a number, before (HEAD) -> under the refused change:**

| item | before | under the change |
|---|---|---|
| MET-CER-SURE, real, inferred | 40 of 433, 0.0924 | 64 of 377, 0.1698 - rises |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 16 of 163, 0.0982 - rises |
| MET-INVENTED, real, inferred | 40 over 473, 0.0846 | 64 over 473, 0.1353 - rises |
| MET-INVENTED, synthetic, exact | 14 over 252 | 16 over 252 - rises |
| coverage, sure and right, real, inferred | 393 over 473, 0.8309 | 313 over 473, 0.6617 - falls |
| coverage, synthetic, exact | 159 over 252, 0.6310 | 147 over 252, 0.5833 - falls |
| MET-WBE, real, inferred | 47 over 113 | 67 over 113 - rises |
| adjudicated | 13 of 13 | 11 of 13 (`003758`, `031948`) |
| V-11, 35 recordings | - | 16 worse |
| level-alone cold moves, eleven and opening | 48 of 60 | 0 of 162 |
| MET-PITCH-ERR: files more than 25 Hz off / windows | 9 / 256 of 1688 | 16 / 486 of 1688 - rises |
| 447's 30 cases through the tracker | medians on the truth | medians hold; 9 cases gain a window more than 25 Hz off, worst 300 Hz |
| opening: sure while acquiring / sure-right | 9 / 21 of 24 | 9 / 21 of 24 |

Per condition, the key's kind, and every MET-PITCH-ERR row that moved are in
`docs/phase-requirements/metrics.md` under unit 448.

**Every recording whose text changed.** None changed in the build, since nothing was kept. Under
the refused change, 42 of 63 recordings' texts changed, and 2 of their callsign lines (all are
in `.run-unit/unit448-text-change.sorted.txt` against `-before`). The ones with a key:

| recording | key | HEAD | under the change |
|---|---|---|---|
| `032129` | inferred | `■ MTMTJ26 PGOPAGATION FORECAST BUAELETIN ARLP034` | `H I 5 E E E IEEII E I SI I ES EEIEE T I I E IETIN EE E S TTEM TTAT` |
| `031948` | inferred | `TM 150 110, AND 110 WITH A MEAN OF 117.W` | `[U] EE II E TTMTT TTTTT TTTTT TTTTT TTTAT ATTTTT 110 WITH A MEAN OF 117.W` |
| `032113` | inferred | `A KET■ A N O INT ERNE T ■ E RSIONS OF 200J6 ...` | `E I E E N E 5N H EEE E ■ I EI■EISISS ...` |
| `012403` | inferred | `DEQ 6Q Q DE KD0UN KD0UN K` | `T G MAEA TTGT TAI 5 K E EQ DE KD0UN KD0UN K` |
| `004234` | inferred | `E C O M <BT> T HANTT TK■ET FOR ON HYPP N AIR QSO WITH W` | `M T <AR> O M <BT> T HANK Y OU FOR ONN HYP N UIR QSO WITH W` |
| `cq-12wpm-5db` | exact | `CQ CQ CQ DE N0CALL N0CALL K` | `DE N0CALL N0CALL K` |
| `cq-18wpm-15db` | exact | `CQ CQ CQ DE N0CALL N0CALL K` | `CE RQ CQ DE N0CALL N0CALL K` |

What the operator would see: nothing new. The build decodes exactly as it did at unit 447's
exit. The trace makes visible that the decoder prints confident letters before it has confirmed
a station, on every file.

## 4. What's blocking us

1. **HM-REQ-102 cannot be met by the tracker, and "acquiring" needs a definition.**
   - Under the fallback definition (up to the tracker's first keyed verdict), the decoder prints
     sure letters while acquiring on 22 of the 23 keyed recordings and on the opening. Many of
     them are right, and `032129`'s whole bulletin is read before a verdict that never comes.
   - Proposed ruling: define acquiring in `CW_SPEC.md` §11, beside MET-TACQ, as the span until
     the decoder's own first confident speed and pitch hold, not the tracker's survey verdict.
     Then measure HM-REQ-102 against that in a decoder unit, not a tracker one.
   - Reasoning: the tracker's verdict is late or absent on real audio (never in 30 s on 7 of
     the 11). Gating sure letters on it would trade HM-REQ-102 for HM-REQ-103 and HM-REQ-012.
   - Rejected: gating the decoder's sure class on the tracker's verdict. It is not built, and it
     would blank most of the openings that read correctly today.
2. **What counts as "keying quality" before anything is confirmed.**
   - The trace shows that on the air the loudest bin is usually the station, and that depth of
     keying does not separate it from its leakage or from AGC pumping.
   - Proposed ruling: a later step 4 unit tries HM-DEC-095's own cross-range ranking before
     confirmation - the loudest among the bins that show keyed structure - with the carrier
     case above as its test.
   - Reasoning: that is the ranking the survey already uses once candidates exist, and it keeps
     the 500 Hz station on `032129` and `031838`.
   - Rejected: re-reading "level alone" so that a keyed-gated loudest counts as keying quality
     in this unit. That would have redefined the metric after seeing the result.
3. **`TheFilterGoesToTheKeyingFromColdTests` is not in the tree.**
   - Proposed ruling: add it as a recorded red, named in `PHASE_PLAN.md` beside the plateau
     test, once item 1 is ruled.
   - Reasoning: it is the only test that watches the from-cold move, and it is red at HEAD on
     HM-REQ-091.
   - Rejected: committing it red now without a ruling on its HM-REQ-102 half, which item 1
     decides.
4. **The app carry-forward line hung once at entry: the host was aborted at 480 s after 132
   passed.**
   - Proposed ruling: none needed now. Report it if it repeats.
   - Reasoning: the rerun was 277 of 278, and the exit run was 275 of 278 with both losing types
     green alone.
   - Rejected: raising the timeout, which would hide a hang.
