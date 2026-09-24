READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0, 1, 2 done;
   3 partial on 3.4 and 3.6, 3.7 now ticked; 4 and 5 not started; 6 partial on 6.3,
   6.4, 6.5; 7 partial on 7.1 to 7.4 and 7.6.
B. Step 3, criterion 3.7 clause by clause: the bar is the raw SpanLogLikelihoodRatio
   at 13.0, justified from task 1's print alone. The table below gives old, above-bar
   and below-bar counts for all 51 rows, with the 13 keyed floors beside them. At the
   re-measure commit the adjudicated readings and keyed totals print identical to
   entry, and no character above the bar was lost. Then 3.6: the trace, and task 3's
   change kept, with all keyed edits 167 to 165 and added letters 17 before and 17
   after. 3.6 is held rather than ticked. Then 3.5 green at exit, and 3.4's count
   is 0 of 3 after a kept change.
C. The rest. Section 4 raises 8 items. None of them is in the way of 3.7. The
   first is the owner's call on whether 3.6 is met.

```
UNIT:       421 - complete at task 4 of 4, none dropped - 2026-09-24 15:09
PHASE GOAL: Hamlet decodes an on-air CQ call into the text that was actually sent, measured as edits against a key
UNIT GOAL:  make every floor count only letters the decoder was sure of, above a span bar taken from the stray letters' own trace, so stray E and T can be removed without a real letter losing its guard
ADVANCED:   yes - 3.7 met and ticked, and a first stray-letter change kept at 167 to 165 edits
NUMBER:     span bar raw SpanLogLikelihoodRatio = 13.0; rows with below-bar characters 18 of 51; all keyed 167 over 565 -> 165 over 565, inferred keys
DRIFT:      0
```

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** Machine QUIVERFULL, project Hamlet (gate confirmed), branch `main`, entry `23457c4b`. Commits, all pushed: `bd0242a0` task 0, `19562a18` task 1 (the trace, alone), `df26a5d7` task 2 (the re-measure, alone), `a913f927` task 3 (the change, alone), `00668552` the verdict that kept it, plus the exit commit carrying this report. Every push returned 0.

**Task 0, the record.** HM-DEC-176 is in `DECISIONS.md`, and its row is at the top of `CLAUDE.md` section 1. P17 is marked answered by R71, and P18 is added in the instruction's words. `PHASE_OUTCOME.md` has UNIT 421 - STEP 3, `PHASE_STATUS.md` names unit 421 with CURRENT_STEP 3 (it already read 3), and the version went 1.13.107 to 1.13.108.

The entry round:
- Build: clean with warnings as errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 194 passed, then the host hung and was killed at 480 s. The 20 names that never reported were each run alone, one type per invocation, and all passed.
- Captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- Baseline, benchmark and space printers: green.

**The numbers to beat, all against inferred keys:**

| set | edits over characters |
|---|---|
| all keyed | 167 over 565 |
| baseline | 22 over 46 |
| 17:37 | 19 over 25 |
| outside | 110 over 363 |
| the ten | 35 over 156 |

**Section 5: checking the instruction against the tree.** I repaired none of these mismatches, only reported them.
1. `CwCharacter` does carry the three span figures. `SpanLogLikelihoodRatio` defaults to NaN. `SpanMarginForRecord` is the ratio over the hops, and it is **0, not NaN, when the hop count is 0** (only word gaps have 0 hops).
2. **R71's 153202 figures (4.9, 5.0, 14.0, 6.8) are the raw `SpanLogLikelihoodRatio`.** That is the only span figure the capture sidecar prints: its `spanLlr` line prints `Text:raw/MarginLlr/share` at one decimal (`MainWindowViewModel.SpanRatioLine`). **`cw-2026-09-24-153202` is not in the tree and is not one of the 51 rows.**
3. The 51 rows and their three numbers match the instruction.
4. The 13 keyed floors are in `TheNumberCannotBeGamedTests.NamedFloors`.
5. `CharacterMargin = 1.0` is at line 323. **It gates on `CwProbabilisticCharacter.SpanMargin`, the per-hop span figure, the same number that becomes `SpanMarginForRecord`.** It does not gate on `Score`. `Score` is the window's likelihood ratio per hop, and a separate gate, `Gate = 1.40` at line 791, applies to it. Every settled letter has cleared both gates.
6. At entry (unit 420's exit), 49 of the 51 rows sat exactly at their named floor. The other two were one above: `031838` at 43 against 42, and `001952` at 57 against 56.

**Every place a floor is counted.** Two counters assert:
- `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` (named and elements)
- `TheNumberCannotBeGamedTests.EachKeyedRecordingIsReadAtAll` (named)

These read `Floors` only for its names and assert nothing: `TheGateBarSweepTests`, `TheStationStillKeyingTraceTests`, `TheTrackerSwitchTraceTests`, `TheTwoPitchesTableTests`, `TheReworkNumbersPrinterTests`, and `WhereTheSpaceIsDecidedTests` (which prints a "named at entry" figure). I left them unchanged.

**NaN spans: 0 on every row, 0 of 1,836 named in all.** Only one place in the tree builds a `CwCharacter`, `CwProbabilisticStream.Character`, and it always sets the span.

**Keyed and unkeyed rows:**
- **22 of the 51 rows are keyed:** the 12 adjudicated readings and the ten keyed captures of the locked-on run. `004535` has a key file that names no stretch.
- **29 rows are unkeyed. R71 and task 3 say 40,** which is a mismatch.
- The three adjudicated recordings in the baseline are rows and keyed floors both. The ten are rows only.
- 17:37 is not a row. It is a keyed floor and in the baseline only.

**Task 1, the trace.** `WhatTheStrayLettersRestOnTests` asserts nothing and was committed alone. Its own alignment reproduces 167 over 565. Table 1 lists 42 added or wrong single-element characters, each with its three span figures and the two gates it cleared. Table 2 prints span distributions, quarter-decade heaps, per-recording edges and a ladder over all 1,836 named characters on the 51 rows and 17:37.

**Task 2, the re-measure (one commit, `df26a5d7`).** Both asserting counters now count characters at or above `TheCapturesThatDecodeKeepDecodingTests.SpanBar = 13.0`, and element floors count the elements inside those characters. Each row also prints the characters it has below the bar. 18 rows and 4 keyed floors were re-stated. The decoder was not touched in tasks 1 and 2.

**Task 3, the change (`a913f927`), kept.** `CwProbabilisticDecoder.Judged` no longer emits a character of one element whose raw span is under `StrayElementSpan = 13.0`.

**Task 4, the exit round:**
- `Hamlet.sln` builds non-incrementally with warnings as errors, 0 warnings.
- Engine carry-forward: 178 of 178.
- App carry-forward: 275 of 278. Three tests were lost to the dispatcher loop ("You've caused dispatcher loop"): two in `ThePsk31ConversationCardTests`, one in `BindingHealthTests`. Alone, they passed 8 of 8 and 1 of 1.
- Captures 51 of 51 under the bar, adjudicated 13 of 13, keyed floors 13 of 13, clean synthetics 2 of 2.
- Green: `TheBaselineIsScoredTests`, `TheBenchmarkIsKeyedTests`, `WhereTheSpaceIsDecidedTests`, `WhatTheStrayLettersRestOnTests`.
- Also green against the change: `EachCharacterAnswersForItselfTests` 6 of 6, `TheProbabilisticDecoderTests` 11 of 11, `NothingActsOnTheAdmissionVerdictTests`, `WhatAFlatMarginDoesToShortCharactersTests`, and `TheSeventeenThirtySevenCaptureTests` 5 of 5. `NoSenderIsSplitInTwoTests` and `EveryElementCarriesItsOwnPitchTests` are excluded from compilation by the test project, so nothing ran for them.
- `src/Hamlet.App` shows nothing against entry. The transmit files show nothing against `7e209cb4`. `data` shows nothing.
- I did not run `ModeFollowsTheMapAgainTests`. It is on no line of this unit's round, and I did not touch it (P16).

**Decisions I made myself, in full:**
- **The bar's figure and value.** It is the raw `SpanLogLikelihoodRatio` at 13.0. My reasons, from the print alone, are in section 3.
- **3.2's third test is read on the scored regions.** The three adjudicated regions print character for character identical before and after (013347 `VA3VRR` 0 edits, 134712 3 characters 0 edits, 003758 3 edits over 12). What changed is the full settled texts of 134712 and 003758, which lose stray `E`s outside those regions. Both texts are printed in section 3.
- **3.6 is held, not ticked,** even though every clause holds on the letter. Section 4 has the question.
- **The placeholder column of `Floors` is left as it was set.** It is printed and asserted on nothing, and on no row did the count move.

## 2. What the owner should expect

Every CW floor now counts only letters the decoder stood behind: named characters whose own span, the raw evidence over their marks, is 13 or more. On the 23 recordings with a key, the weakest letter the key calls right stands at 30.8, so no real letter lost its guard. Of the 1,790 named characters on the 51 capture rows, 63 were under the bar, and every one was a lone `E`. Each row's floor was re-stated at the count it read above the bar, with the decoder unchanged. The first change against the strays then took exactly those 63 `E`s off the screen and nothing else. On the recordings with no key, that is stray `E`s gone: 11 from `001952`, 10 each from `134712`'s tail after `N4L` and from `002016`, and fewer elsewhere. The correctness number moved 167 to 165 edits. **What will look wrong but is not:** four keyed floors and 18 capture floors went down (for example 134712 from 21 to 11). Only below-bar `E`s left them, and no row's above-bar count fell. **Also expect this:** not one of the 17 letters the keys call added inside a scored stretch was under the bar. So on the keyed recordings, the stray `E` and `T` you would score as extra are still on the screen. The ones that came off are `E`s outside what was keyed.

## 3. What you should see

**The bar: raw `SpanLogLikelihoodRatio` = 13.0.** No character any inferred key aligns as right sits below it.

These are the reasons, taken from task 1's print alone:
1. **The hard ceiling.** The lowest right character stands at raw 30.8, a lone `E` on 17:37 at 25.100 s. The bar must be under it.
2. **The empty stretch.** On the raw figure, no key-aligned right character stands anywhere from the corpus minimum of 3.28 up to 30.8. The key-aligned characters in that stretch are:

   | characters | label | raw span |
   |---|---|---|
   | three `E` on `031838` | wrong | 4.42, 5.02, 5.50 |
   | one longer character | added | 21.04 |
   | one `T` | wrong | 26.04 |

   13.0 is the midpoint on a log scale between the top of that low `E` cluster and the lowest right character (square root of 5.50 × 30.8 = 13.02), rounded down.
3. **Why raw and not per hop.**
   - Across keyed recordings, the median right character spreads the same on both figures: 12.47 to 1 between the lowest and highest recording. So per hop is not more comparable across recordings on this corpus, although `CwCharacter`'s remarks say it is the only comparable form.
   - Per hop, right single elements sit above right longer letters (medians 14.66 against 10.06), which lifts exactly the class the bar is for. Right characters also run down to 1.376, next to the emission gate itself, with no empty stretch under them.
   - Raw puts right single elements below longer letters (297 against 817), and leaves the empty stretch described above.
   - R71's own 153202 figures are raw.
4. **The third figure fails.** Per hop over the recording's own median is wrecked by soup: on 013347 the `V` of `VA3VRR` is 5×10⁻⁸ of its recording's median.

**The 51 rows at the re-measure commit, `df26a5d7`.** Every row's named, element and placeholder counts are identical to entry, so above-bar = named − below-bar exactly.

| # | row | old floor | named | above bar | below bar | elements, old floor → new | keyed floor, old → new |
|---|---|---|---|---|---|---|---|
| 1 | 013347 | 57 | 57 | 57 | 0 | 106 → 106 | 57 → 57 |
| 2 | 134712 | 21 | 21 | **11** | **10** | 41 → 31 | 21 → **11** |
| 3 | 004507 | 49 | 49 | 49 | 0 | 117 → 117 | 49 → 49 |
| 4 | 012403 | 21 | 21 | **19** | **2** | 62 → 60 | 21 → **19** |
| 5 | 031838 | 42 | 43 | **40** | **3** | 93 → 91 | 43 → **40** |
| 6 | 031905 | 36 | 36 | 36 | 0 | 108 → 108 | 36 → 36 |
| 7 | 031948 | 31 | 31 | 31 | 0 | 111 → 111 | 31 → 31 |
| 8 | 032012 | 43 | 43 | 43 | 0 | 119 → 119 | 43 → 43 |
| 9 | 032050 | 44 | 44 | 44 | 0 | 105 → 105 | 44 → 44 |
| 10 | 032113 | 47 | 47 | 47 | 0 | 102 → 102 | 47 → 47 |
| 11 | 032129 | 65 | 65 | 65 | 0 | 114 → 114 | 65 → 65 |
| 12 | 013622 | 51 | 51 | **49** | **2** | 80 → 78 | |
| 13 | 003016 | 54 | 54 | 54 | 0 | 146 → 146 | |
| 14 | 003126 | 48 | 48 | 48 | 0 | 131 → 131 | |
| 15 | 003758 | 44 | 44 | **43** | **1** | 93 → 92 | 44 → **43** |
| 16 | 001520 | 1 | 1 | 1 | 0 | 1 → 1 | |
| 17 | 001831 | 44 | 44 | **43** | **1** | 108 → 107 | |
| 18 | 001952 | 56 | 57 | **46** | **11** | 113 → 103 | |
| 19 | 002016 | 44 | 44 | **34** | **10** | 84 → 74 | |
| 20 | 011552 | 22 | 22 | 22 | 0 | 74 → 74 | |
| 21 | 012748 | 2 | 2 | 2 | 0 | 3 → 3 | |
| 22 | 012823 | 26 | 26 | **23** | **3** | 40 → 37 | |
| 23 | 012922 | 45 | 45 | **43** | **2** | 106 → 104 | |
| 24 | 013010 | 48 | 48 | 48 | 0 | 122 → 122 | |
| 25 | 013150 | 51 | 51 | 51 | 0 | 123 → 123 | |
| 26 | 013303 | 44 | 44 | 44 | 0 | 127 → 127 | |
| 27 | 013402 | 56 | 56 | 56 | 0 | 150 → 150 | |
| 28 | 013520 | 55 | 55 | 55 | 0 | 147 → 147 | |
| 29 | 013637 | 60 | 60 | 60 | 0 | 157 → 157 | |
| 30 | 021410 | 36 | 36 | 36 | 0 | 88 → 88 | |
| 31 | 021629 | 27 | 27 | 27 | 0 | 65 → 65 | |
| 32 | 021825 | 25 | 25 | **19** | **6** | 49 → 43 | |
| 33 | 125941 | 0 | 0 | 0 | 0 | 0 → 0 | |
| 34 | 014854 | 0 | 0 | 0 | 0 | 0 → 0 | |
| 35 | 014935 | 0 | 0 | 0 | 0 | 0 → 0 | |
| 36 | 014113 | 0 | 0 | 0 | 0 | 0 → 0 | |
| 37 | 014308 | 0 | 0 | 0 | 0 | 0 → 0 | |
| 38 | 003901 | 9 | 9 | 9 | 0 | 20 → 20 | |
| 39 | 003919 | 27 | 27 | **25** | **2** | 54 → 52 | |
| 40 | 004027 | 40 | 40 | **39** | **1** | 119 → 118 | |
| 41 | 004108, keyed | 32 | 32 | 32 | 0 | 107 → 107 | |
| 42 | 004133, keyed | 30 | 30 | **28** | **2** | 87 → 85 | |
| 43 | 004205, keyed | 34 | 34 | 34 | 0 | 96 → 96 | |
| 44 | 004234, keyed | 37 | 37 | **36** | **1** | 96 → 95 | |
| 45 | 004322, keyed | 39 | 39 | 39 | 0 | 112 → 112 | |
| 46 | 004347, keyed | 40 | 40 | 40 | 0 | 115 → 115 | |
| 47 | 004405, keyed | 36 | 36 | **35** | **1** | 106 → 105 | |
| 48 | 004427, keyed | 43 | 43 | **42** | **1** | 112 → 111 | |
| 49 | 004510, keyed | 38 | 38 | **34** | **4** | 104 → 100 | |
| 50 | 004535 | 47 | 47 | 47 | 0 | 128 → 128 | |
| 51 | 004550, keyed | 41 | 41 | 41 | 0 | 123 → 123 | |
| | **17:37, not a row** | | 46 | 46 | 0 | | 46 → 46 |

Totals: 1,790 named, 1,727 above the bar, 63 below, every one of them a single-dot `E`, on 18 rows. The 12 adjudicated rows' count floors are still retired in favor of their anchors (Tim, 2026-08-25). Their element floors still assert.

**At the re-measure commit** the adjudicated readings, the baseline rows and the benchmark rows print character for character identical to entry, and `src/Hamlet.RadioEngine/Cw` shows nothing against `23457c4b`. All keyed is 167 over 565, baseline 22 over 46, 17:37 19 over 25, outside 110 over 363, and the ten 35 over 156, all against inferred keys. **No character above the bar was lost.** On every row, the above-bar count equals the entry named count less the characters below the bar, and the decoder read exactly what it read at entry.

**3.6's trace, per keyed recording.** Named characters on each recording are split right / wrong (single-element) / added (single-element) / outside every scored stretch, at entry:

| recording | named | right | wrong (single-element) | added (single-element) | outside |
|---|---|---|---|---|---|
| 17:37 | 46 | 13 | 7 (6) | 8 (6) | 18 |
| 013347 | 57 | 6 | 0 | 0 | 51 |
| 134712 | 21 | 3 | 0 | 0 | 18 |
| 003758 | 44 | 8 | 3 (3) | 0 | 33 |
| 012403 | 21 | 13 | 0 | 0 | 8 |
| 004507 | 49 | 42 | 0 | 1 (0) | 6 |
| 031838 | 43 | 8 | 14 (11) | 1 (1) | 20 |
| 031905 | 36 | 20 | 8 (1) | 2 (0) | 6 |
| 031948 | 31 | 26 | 1 (0) | 0 | 4 |
| 032012 | 43 | 40 | 1 (1) | 2 (0) | 0 |
| 032050 | 44 | 31 | 4 (1) | 2 (1) | 7 |
| 032113 | 47 | 20 | 2 (0) | 0 | 25 |
| 032129 | 65 | 15 | 13 (9) | 0 | 37 |
| 004234 | 37 | 7 | 2 (2) | 0 | 28 |
| 004108, 004133, 004205, 004322, 004347, 004405, 004427, 004510, 004550 | 32, 30, 34, 39, 40, 36, 43, 38, 41 | 2, 4, 8, 29, 23, 6, 9, 11, 8 | 2, 0, 0, 3, 0, 0, 0, 0, 0 | 0, 0, 0, 0, 1 (0), 0, 0, 0, 0 | 28, 26, 26, 7, 16, 30, 34, 27, 33 |
| **all keyed** | **917** | **352** | **60 (34)** | **17 (8)** | **488** |

Every stray was admitted by `CharacterMargin` on its per-hop span and by the window `Gate` on `Score`. The raw spans of the eight added single-element letters run from 33.4 to 159.2: the six on 17:37, the `T` on 031838 at 21.355 s, and the `T` on 032050 at 11.195 s. **All eight stand above the lowest right character on all three figures, so no bar that keeps every right letter can reach them.**

**Task 3's change, `a913f927`, kept under 3.2's four tests:**
1. **All keyed edits fall:** 167 to 165 over 565. `031838` goes 21 to 19 over 35, where `TEAHEEEA MEAN` becomes `TEAH A MEAN`. No recording rose. Baseline 22, the ten 35.
2. **No above-bar keyed floor breaks:** 13 of 13.
3. **The three adjudicated readings.** Their scored regions are identical. Here are the full texts, before and after:
   ```
   134712 before:           E           I  E E E EE E  I■5   NT    N4LZT K  EE E
   134712 after:                       I        I■5   NT    N4LZT K
   003758 before: ... EAN EANQNIK   E    EAN E
   003758 after:  ... EAN EANQNIK        EAN E
   013347:        unchanged
   ```
4. **No row's above-bar count falls:** 51 of 51 identical, and exactly the 63 below-bar `E`s were removed.

**17:37:** 19 edits over 25 before and after, inferred key. **Added letters on the keyed recordings: 17 before, 17 after; single-element 8 before, 8 after.** Wrong letters went 60 to 56.

**Removed on the unkeyed rows,** as `E`, raw span:

| row | removed |
|---|---|
| 001952 | 11: 4.30, 9.80, 8.04, 11.21, 5.20, 8.90, 6.80, 9.06, 8.22, 4.96, 6.89 |
| 002016 | 10: 9.60, 12.45, 11.13, 4.51, 3.28, 10.41, 12.08, 5.38, 8.24, 4.71 |
| 021825 | 6: 5.54, 8.26, 4.03, 11.46, 12.71, 7.16 |
| 012823 | 3: 6.30, 10.67, 10.92 |
| 013622 | 2: 10.03, 7.99 |
| 012922 | 2: 9.65, 6.89 |
| 003919 | 2: 9.86, 8.78 |
| 001831 | 1: 10.91 |
| 004027 | 1: 6.42 |

The keyed rows lost 25: 10 on 134712, 2 on 012403, 3 on 031838, 1 on 003758, 2 on 004133, 1 on 004234, 1 on 004405, 1 on 004427 and 4 on 004510. Of those, 22 sat outside every scored stretch, and the 3 on 031838 were inside one. Every figure is in `.run-unit/unit421-captures-t2.txt`.

**3.5** is green at exit. **3.4's count** is 0 of 3, because this unit kept a change. The phase's running total is now **217 to 165 edits over 565 characters, against inferred keys** (`baseline.md`).

## 4. What's blocking us

Nothing here halts the phase, and nothing is in the way of 3.7, which is met.

1. **Is 3.6 met?** Proposed ruling: 3.6 stays open until a change takes an added letter off a scored stretch. **Reasoning:**
   - Every clause is met on the letter: the trace, the change judged under 3.2, the total falling 167 to 165, and added letters reported.
   - But the purpose was the stray letters the key calls extra, and those are 17 before and 17 after.
   - The trace shows no bar that keeps every right letter can reach them, since all eight added single elements stand at raw 33 or higher, over the right `E` at 30.8. So a further change must tell a stray from a real letter by something other than span.

   **Rejected:** ticking 3.6 on the two edits, which came from wrong letters rather than added ones. Not blocking.

2. **R71 counts 40 unkeyed captures. The tree has 29 unkeyed rows**, with 22 of 51 keyed: 12 adjudicated and the ten. Proposed ruling: none needed. The count in R71 is prose and asserts nothing. Recorded so the next author does not plan on 40. Not blocking.

3. **`CwCharacter.SpanMarginForRecord`'s remarks say it is the only span form comparable across recordings.** The trace finds the per-hop and raw figures spread alike across recordings on right characters (12.47 to 1). Per hop also flatters single elements. Proposed ruling: leave the remark until a unit touching `CwCharacter` rewrites it with the trace beside it (12.6). Not blocking.

Carried per HM-DEC-139, as the instruction states them:

4. **P12** stays parked: the `captured` and `broadcast` clock lines.
5. **The `clipping` and `inputFloor` question** stays parked.
6. **P14, P15 and P16** stay parked.
7. **Unit 420's stale `unknowns` entry for `CW`** in `mode-receiver-conditions.json` is logged as P18 in task 0, in its own words, and not touched.
8. **P17 is answered by R71.** It leaves the carried list.
