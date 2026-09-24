READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3, 6 and 7 partial; 4 and
   5 not started. This unit moved no step: no decoder change was built, and no number moved.
B. Step 3's criterion 3.6: clause one is delivered. The trace names every added and wrong
   single-element character, what admitted it, and every figure the instruction asked for.
   Clause two is not met this unit. No measure, alone or paired with another, separates the 8
   added letters from the right, wrong and unkeyed single-element ones, so no change was built
   and the four tests of 3.2 had nothing to judge. Total edits 165 -> 165, added letters
   17 -> 17. 3.4's count of units with no kept change is now 1. 3.5 holds at exit.
C. The rest. Section 4 raises 3 items. The first stands in the way of 3.6 clause two. None stands
   in the way of 3.4.

```
UNIT:       425 - complete at task 4 of 4, none dropped - 2026-09-24 19:42
PHASE GOAL: Hamlet prints a CQ call as it was sent, judged by edits against a key over the scored stretch, with nothing kept that costs a floor
UNIT GOAL:  Find a measure other than span that tells a stray E or T the key says was never sent from a real letter, and build one change on it that removes strays and no real letter
ADVANCED:   no - the trace found no measure that separates the 8 added letters from right, wrong or unkeyed ones, so nothing was built and nothing moved
NUMBER:     all keyed edits over 565 against inferred keys: 165 -> 165; added letters 17 -> 17
DRIFT:      1 consecutive unit without advance, was 0
```

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** The run was on QUIVERFULL, in C:\Source\HamLet. The
gate confirmed Hamlet, and all work was on branch `main`. Tasks 2 and 3 built and judged nothing.
That is the instruction's own route: task 2 says to build nothing and go to task 4 when task 1
names no separating measure. So they are not a drop.

- **Task 0, `f33c8c09`.** HM-DEC-178 is in `DECISIONS.md` and has a row at the top of `CLAUDE.md`
  §1. P19 is marked answered. `PHASE_OUTCOME.md` has UNIT 425 - STEP 3 with the entry round.
  `PHASE_STATUS.md` names unit 425 at CURRENT_STEP 3. The version went from 1.13.111 to
  1.13.112. Entry round results:
  - build: 0 errors
  - engine carry-forward: 178 of 178
  - app carry-forward: 276 of 278. The 2 misses, both in `TheStopIsAlwaysOnScreenTests`, were the
    dispatcher loop, and the type is 5 of 5 alone.
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13
  - keyed and baseline types: green
  - `WhatTheStrayLettersRestOnTests`: green
  - `DecisionLogOrderTests`: red on HM-DEC-166 only (P25), as expected.
- **Task 1, `d1b9347d`.** I added `WhatTheStrayLettersRestOnTests.WhatSeparatesAStrayFromALetter`,
  which asserts nothing. It decodes each recording exactly as the floors do, and after every
  5 ms hop it reads the stream's mixing pitch and its newest envelope value. For every
  single-element named character it prints:
  - the key's verdict
  - the raw span and the window Gate score
  - the gaps before and after, and its key-down length, in units of the read's speed
  - whether it stands alone between word gaps
  - its pitch against the sender's pitch, which is the median over the recording's named
    characters
  - its energy, which is the median envelope over its hops, against the median of its three
    named neighbors on each side.

  It then prints the edge for each measure and for each pair of measures. Its own check reads
  165 edits over 565, identical to entry.
- **Task 4, this commit.** The exit round, below.

**Section 5 checks. Nothing was repaired.**
- **The table in section 4 matches the entry run exactly**: all eight rows, times, spans, Gate
  scores and per-hop margins.
- **The totals match.** 165 over 565, 17:37 at 19 over 25, and 17 added letters, 8 of them
  single-element, all agree with `baseline.md` row 421 and the entry run. Wrong letters are 56,
  31 of them single-element. The subtotals are:
  - baseline: 22 over 46
  - outside: 108 over 363, 9 recordings
  - bench: 35 over 156
  - live: 41 over 156
- **The floors hold.** All 13 keyed floors and all 51 capture rows are at or above their floors
  on counts at or above the span bar of 13. Every keyed floor equals its count. The floor
  numbers live in two places:
  - `TheNumberCannotBeGamedTests.NamedFloors`: `tests/Hamlet.RadioEngine.Tests/Cw/TheNumberCannotBeGamedTests.cs:50`
  - `TheCapturesThatDecodeKeepDecodingTests.Floors`: `tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs:113`
- **Capture rows with no key: 29**, the same as R73 says. That is 52 recordings, meaning the 51
  rows plus 17:37, less the 23 keyed.
- **`DecisionLogOrderTests`** is red at entry and at exit, on the same assertion: the gaps
  expected are 105 and 136, and 166 is also missing (P25). The order test passes with the
  HM-DEC-178 row.

**Exit round (3.5).**
- `Hamlet.sln` builds non-incremental with warnings as errors: 0 warnings, 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 276 of 278. The two misses were dispatcher loops, and both types are green
  alone: `TheRecordNamesTheSubModePressedTests` 12 of 12 and `TheWindowHoldsBelowItsMinimumTests`
  3 of 3.
- Captures: 51 of 51, **wall time 124 s**.
- Adjudicated 13 of 13, keyed floors 13 of 13.
- The touched type, `WhatTheStrayLettersRestOnTests`: 2 of 2 in 247 s.
- Of unit 421's seven Cw types, five are green: `EachCharacterAnswersForItselfTests` 6 of 6,
  `NothingActsOnTheAdmissionVerdictTests` 1 of 1, `TheProbabilisticDecoderTests` 11 of 11,
  `TheSeventeenThirtySevenCaptureTests` 5 of 5, and `WhatAFlatMarginDoesToShortCharactersTests`
  1 of 1. The other two, `EveryElementCarriesItsOwnPitchTests` and `NoSenderIsSplitInTwoTests`,
  match no test. They are `Compile Remove`d in the engine test project, as they were at unit 421,
  so they ran nothing (section 4, item 3).
- Nothing that was green at entry is red.
- The transmit files show nothing against `7e209cb4`, and `src` and `data` show nothing against
  entry `b12cbbe4`.

**Decisions I made myself.**
- **How each figure is defined.**
  - The unit is the one taken from the speed of the read that settled the character.
  - A gap is measured to the neighboring *settled* characters. So a character the path read
    and the margin dropped is inside that gap.
  - The sender pitch is the median mixing pitch over the recording's named characters, which
    needs no key.
  - Energy is a median, so a multi-element neighbor's element gaps do not dilute it.
- **The side each measure's strays fall on** is taken from the medians of the added and right
  heaps, not from any bar.
- **Pairs.** A pair is tested as the box that holds all eight added letters. The instruction
  asked for single measures; I added pairs so that "none separates" covers two measures
  combined as well.
- **I wrote a new engine script, `unit425-cwtypes.sh`.** The `types.sh` copied from unit 424 is
  an app-project script. It ran the seven Cw types against the app project and found none. The
  rerun overwrote those empty outputs.
- **DRIFT is written as 1.** The work instruction's template shows `DRIFT: 0`, but CLAUDE_CODE.md
  §8 says to increment it on `ADVANCED: no`. I followed §8.

## 2. What the owner should expect

No stray letters stopped printing, no real letter went, and the 17:37 CQ call reads exactly as
before: `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` where `CQ CQ CQ DE WB6RED WB6RED` was sent. The
unit looked for something other than signal strength that marks a stray E or T, and did not
find it. The eight strays are as loud as the letters beside them. Five of them are followed by
a gap about the length of the space inside a letter. That suggests they are pieces of real
letters cut apart, not noise, but I have not proved it (see item 1 in section 4). Many wrong
letters, and many single letters on the recordings nobody has keyed, look the same. Any rule
that removed the strays would remove those too. What will look wrong but is not: a new test
prints a long table and passes. It asserts nothing, by design.

## 3. What you should see

**No visible change.** This unit only adds a trace. The decoder, the screen and every number
are as they were.

**1. The measure chosen: none.** Across the whole tree there are 498 single-element named
characters: 8 added, 62 right, 31 wrong, 145 on keyed recordings but outside every scored
stretch, and 252 on the 29 unkeyed rows. None is under the span bar. For each measure below, the
side the added letters fall on, and how many of every other heap stand on that side of the
least stray added one:

| measure | added side | added median, right median | least stray added | nearest right | right | wrong | keyed unscored | unkeyed | added taken cleanly |
|---|---|---|---|---|---|---|---|---|---|
| window Gate score | low | 1.801, 5.979 | 6.876 | 1.428 | 39 | 19 | 55 | 201 | 0 of 8 |
| raw span | low | 88.2, 323.3 | 159.2 | 30.8 | 19 | 15 | 41 | 149 | 0 of 8 |
| gap before, units | high | 4.53, 3.83 | 1.56 | 31.1 | 62 | 30 | 135 | 236 | 0 of 8 |
| gap after, units | low | 1.84, 3.80 | 4.68 | 2.18 | 41 | 22 | 67 | 135 | 0 of 8 |
| key-down, units | high | 2.20, 1.63 | 0.71 | 4.2 | 56 | 24 | 88 | 217 | 0 of 8 |
| key-down over its nominal element | low | 0.85, 0.975 | 2.17 | 0.37 | 62 | 31 | 145 | 252 | 0 of 8 |
| alone between word gaps | none of the 8 is alone | 0, 0 | 0 | 0 | 52 | 29 | 104 | 212 | 0 of 8 |
| off the sender pitch, Hz | high | 10, 0 | 0 | 200 | 62 | 31 | 145 | 252 | 0 of 8 |
| energy over neighbors' median | high | 1.014, 1.006 | 0.986 | 1.475 | 42 | 19 | 90 | 152 | 0 of 8 |

**No measure takes even one added letter** without some other single-element character on its
side. The window Gate score, which the instruction's reading pointed at, takes 3 of 8 at its
lowest step. At that step it also takes 2 right, 2 wrong, 4 keyed unscored and 25 unkeyed.

**The gap after comes closest.** Five of the eight are followed by 1.84 units or less, and no
right single-element letter is under 2.18. But at that bar there are also 7 wrong, 16 keyed
unscored and 22 unkeyed. That breaks R73, which forbids removing wrong letters, and it breaks
the unkeyed floors.

**Pairs.** The best box, raw span with energy, holds all eight along with 8 right, 10 wrong,
14 keyed unscored and 74 unkeyed. No pair is clean.

**The eight**, with their figures. Gaps and key-down are in units.

| recording | at | char | Gate | raw span | gap before | gap after | key-down | alone | pitch, sender | energy |
|---|---|---|---|---|---|---|---|---|---|---|
| 173723 | 21.170 s | E | 2.488 | 86.2 | 3.90 | 3.47 | 0.99 | no | 575, 585 | 0.993 |
| 173723 | 22.540 s | T | 1.801 | 156.0 | 4.53 | 1.84 | 2.41 | no | 585, 585 | 1.011 |
| 173723 | 22.720 s | E | 1.801 | 34.6 | 1.84 | 2.98 | 0.71 | no | 585, 585 | 1.048 |
| 173723 | 26.250 s | E | 1.568 | 33.4 | 4.96 | 1.56 | 0.85 | no | 600, 585 | 0.986 |
| 173723 | 26.515 s | T | 1.568 | 88.2 | 1.56 | 1.56 | 2.20 | no | 600, 585 | 1.028 |
| 173723 | 26.820 s | T | 1.568 | 138.1 | 1.56 | 4.68 | 2.76 | no | 600, 585 | 1.111 |
| 031838 | 21.355 s | T | 2.131 | 159.2 | 13.83 | -6.50 | 6.50 | no | 525, 525 | 1.012 |
| 032050 | 11.195 s | T | 6.876 | 74.1 | 5.53 | 1.84 | 2.13 | no | 500, 500 | 1.014 |

All 498 rows are in `.run-unit/unit425-t1-separate.txt`, with the unkeyed rows among them.

**2. Characters removed: none.** No change was built.

**3. The four tests: not run as a judgment**, because there was no change to judge. As
numbers at exit against entry:
- (1) keyed edits 165 -> 165
- (2) keyed floors 13 of 13, unchanged
- (3) adjudicated 13 of 13, unchanged
- (4) captures 51 of 51, above-bar counts unchanged, since `src` did not change
- added letters 17 -> 17, 8 single-element -> 8
- wrong letters 56 -> 56, 31 single-element -> 31.

**4. 17:37, before and after, identical:** settled `T EABNIREDWBZ WB6RED CQ CQ CQ DEWTEETEEERE D
ETTTB 7E E I`. The scored region `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` is 19 edits from `CQ CQ CQ
DE WB6RED WB6RED`.

## 4. What's blocking us

1. **3.6 clause two may need a join, and a join moves the unkeyed floors. This is in the way of
   3.6.**
   - **Proposed ruling, the owner's:** the next unit on 3.6 traces whether each added single
     element is an element of a neighboring real letter, split at an element gap. If it is, the
     route is a join and not a condition on emission. A join that merges single elements into one
     letter leaves the above-bar element count unchanged and lowers the above-bar named count. A
     ruling would be needed on whether that counts as a floor lowered on an unkeyed row.
   - **Reasoning:** the eight sit at 0.99 to 1.11 of their neighbors' energy, so they are made of
     real keying. Five are followed by 1.84 units or less. On 17:37, E T T at 26.250, 26.515 and
     26.820 s are separated by 1.56 units, about an element gap. Dit-dah-dah with element gaps is
     W, the letter the key has there in `WB6RED`. That is an inference from the trace, not a
     measurement of the path.
   - **What else the trace shows:** the wrong and unkeyed single elements share this shape, so
     under R73 and R71 as written no change can remove the eight.
   - **Rejected:** building the gap-after condition anyway, because it costs 7 wrong and 22
     unkeyed characters; widening R73 to wrong letters, because that is not mine to propose past
     stating it.
2. **One of the eight overlaps its neighbor.** `cw-2026-08-22-031838` has a T at 21.355 s whose
   gap after is -6.50 units: the next settled character starts before this one ends. That
   suggests two consecutive reads of the window each settled a reading of the same keying, a
   settling fault and not a stray. Not traced further.
   - **Proposed:** a unit on 3.6 traces it first, since a fault in the settle step would be
     fixed without any condition.
   - Not blocking.
3. **Two of the Cw types the instruction names for the exit round compile out.**
   `EveryElementCarriesItsOwnPitchTests` and `NoSenderIsSplitInTwoTests` are `Compile Remove`d
   in `tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj` (lines 43 and 45), so
   naming them runs nothing, at unit 421 and now.
   - **Proposed:** the next instruction drops them from the list, or a ruling restores them.
   - Not blocking.
