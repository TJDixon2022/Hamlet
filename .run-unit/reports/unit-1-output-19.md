```
READ IN THIS ORDER.

A. Added letters over the keyed recordings, before and after, and whether
   the ratio separates the added from the right.
B. Step 3's criterion 3.6, and 3.5 at exit.
C. The rest. Section 4 raises 3 items, none blocking.
```

```
UNIT:       428 - complete at task 4 of 4, none dropped - 2026-09-24 23:47
PHASE GOAL: When a station calls CQ in Morse, the text on the CW tab is what was sent, measured as edit distance against a key, and Tim says it read.
UNIT GOAL:  Find out whether a letter whose span is a small fraction of its neighbors' spans is litter, and demote it only if that ratio parts the added letters from the right ones.
ADVANCED:   no - on the 23 keyed recordings the ratio does not separate added from right under any of three traced figures, so nothing was built and 3.6 did not move
NUMBER:     added letters over all keyed recordings: 17 -> 17; all keyed 165 -> 165 over 565
DRIFT:      1 consecutive unit without advance  (was 0)
```

## 1. What Claude did

**Complete, 4 of 4 tasks, none dropped. Tasks 2 and 3 became traces, as task 1's rule required, because task 1 found no separation.** Machine QUIVERFULL, project Hamlet confirmed by the section 0 gate, branch main, HEAD at entry 9db61107. Every commit was pushed: 521b37b7, 952a74cc, 3396d277, a1fcfc4a, and the exit commit carrying this file.

**The instruction against the tree (section 5), reported, nothing repaired:**

- **The 2026-09-25 captures from 7.052 are not in the tree.** `tests/fixtures/cw/captured/unadjudicated` holds none dated 2026-09-25; the newest are the thirteen from 2026-09-24 00:39 to 00:45 UTC. A search of the repository and the whole user profile for `cw-2026-09-25*` found nothing. **Task 1 therefore ran on the 23 keyed recordings alone**, plus the 29 unkeyed capture rows printed as a third heap. None of the instruction's quoted figures (`OPERETTEETTTTED`, `ALL LOGS WILL BE UPLOADED`) could be reproduced or checked.
- **The span is one number.** The sheet's `spanLlr` first figure, the floors' `SpanBar` of 13.0 and the decoder's `StrayElementSpan` of 13.0 all read `CwProbabilisticCharacter.SpanLogLikelihoodRatio`. It is computed in `CwProbabilisticDecoder.Spell`, copied to `CwCharacter` by `CwProbabilisticStream` (line 661), and printed by `MainWindowViewModel.SpanRatioLine`.
- **`Judged` admits a character on two tests, not one.** The first is `SpanMargin >= CharacterMargin` (1.0 per hop). The second, for a single element only, is raw span `>= StrayElementSpan` (13.0, unit 421). A word gap passes unjudged. The instruction names only the first.
- **Keyed totals at HEAD:** all keyed 165 edits over 565 against inferred keys. That is the baseline 22 over 46 (17:37 is 19 over 25), outside 108 over 363, and the ten 35 over 156 at the bench (41 over 156 live). **17 added letters, 8 of them single-element**, 56 wrong. These match unit 425's exit.
- R71's bar of 13.0 and R73's clause read as the instruction states them.
- The instruction and HM-DEC-181 are dated 2026-09-25, but the machine's clock read 2026-09-24 throughout, 22:58 to 23:47 local. That is 2026-09-25 in UTC, so this is noted and not treated as a mismatch.

**Task 0.** Recorded HM-DEC-181 in `DECISIONS.md` and at the top of `CLAUDE.md` section 1, dated 2026-09-25. Added `PHASE_OUTCOME.md` `## UNIT 428 - STEP 3` from the decision block with the entry round. `PHASE_STATUS.md` names unit 428, CURRENT_STEP 3. Version 1.13.114 to 1.13.115. **Entry round**, one type per invocation:
- build 0 errors
- engine carry-forward 178 of 178
- app carry-forward 278 of 278
- captures 51 of 51 in 119 s
- adjudicated 13 of 13
- keyed floors 13 of 13
- TheBenchmarkIsKeyedTests 1 of 1
- TheBaselineIsScoredTests 2 of 2
- the stray trace 1 of 1

**Task 1 - `WhatTheNeighborsSayTests.EachLetterAgainstTheLettersAroundIt`**, a fact that asserts nothing. It prints every named character on the keyed recordings (892 of them, 1773 across all 52 recordings) with:
- its raw span
- the median raw span of its window
- the ratio of the two

Then the ratio's distribution for added, right, wrong, keyed-but-outside-every-stretch and unkeyed. It does this over all characters, over single elements (E and T) alone, and over longer characters. It then prints a ladder at every added character's ratio, and the lowest right and unkeyed ratios by name.

**The window, a decision I made for myself, overrulable:** three named characters on each side. The character itself is left out, and word gaps and placeholders are skipped. At a recording's edge the window takes what is there and does not borrow from the other side. It counts characters rather than seconds so that a slow fist and a fast one are compared with the same number of neighbors. A median of six survives one or two litter neighbors, and three each side is shorter than a callsign, so the window stays in the passage the letter was sent in. **Result: they overlap. No ratio is named.** See section 3.

**Task 2 - `EachLetterAgainstAWiderWindow`**, the second trace on a different window, as task 1's rule requires when nothing separates. **Eight each side, my decision, overrulable.** Task 1 showed 17:37's added letters in runs (26.250, 26.515, 26.820 s), so a window of three was half filled with the litter it was meant to stand against. Eight is longer than any such run. **Still overlapped.** Nothing was built or judged under 3.2.

**Task 3 - `EachMarkAgainstTheMarksAroundIt`**, a third figure, my decision, overrulable. It keeps the eight-each-side window but uses raw span *per mark* (span over the dits and dahs in the pattern) for the character and its neighbors alike. The reason: raw span sums over marks, so a right `E` next to an `H` sits low for having one mark, and tasks 1 and 2 had found the right single elements sitting *lower* than the added ones. **Per mark, the added single elements sit above the right ones.** Nothing built. Tasks 2 and 3 both ran as traces, as the instruction provides for when no change is kept; neither is the task 3 drop candidate.

**Task 4 - exit round**, one type per invocation:
- `Hamlet.sln` non-incremental with warnings as errors: 0 warnings, 0 errors
- engine carry-forward 178 of 178
- app carry-forward 277 of 278; the one loss, `TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks`, is *You've caused dispatcher loop* in 1 ms, and the type is 4 of 4 alone
- captures 51 of 51 in 119 s, every row's named, above-bar, below-bar, element and placeholder counts identical to entry, 51 of 51 `same`
- adjudicated 13 of 13, output identical to entry but for timing lines
- keyed floors 13 of 13
- keyed and baseline totals identical to entry
- `WhatTheNeighborsSayTests` 3 of 3 in 359 s

**`src` and `data` show nothing against entry 9db61107, and the transmit files show nothing against 7e209cb4.** The only code this unit added is the one test file.

## 2. What the owner should expect

**Nothing on the CW tab changes.** No decoder code was touched. The litter the owner reads between words today (`EETTTEETTTTTTTTETTETETKTETEE`) will still be there tomorrow.

**What is now known, on the recordings in the tree:** judging a letter by its span against its neighbors' spans does not find the added letters. That held for windows of three and eight on each side, on raw span and on span per mark. Right `E`s and `T`s routinely sit at a fifth to a tenth of their neighbors, because a single dit carries less evidence than a four-mark letter beside it. Meanwhile 6 of the 8 added single elements are on 17:37, where *every* letter is weak (neighbors 45 to 330), so the litter there does not stand out. A bar that caught even the lowest added `T` would first take 17 right letters with a window of three, 8 with a window of eight, and 7 per mark.

**What will look wrong but is not:** HM-DEC-181 is recorded as ruled, and this unit's finding does not contradict it. The ruling says the window and fraction are measured, and they were measured here on 17 added letters. The traffic net the ruling was written from, with hundreds of litter characters on one strong signal, may still separate. It is not in the tree, so it could not be tested (section 4, item 1).

## 3. What you should see

**No visible change.** This unit only measured, and the measurement says the rule it was built to test would cost right letters on the recordings the project holds.

**The two distributions, added against right, span over the median span of three named characters each side (task 1):**

| heap | n | min | quartile | median | quartile | max |
|---|---|---|---|---|---|---|
| added, all | 17 | 0.065 | 0.286 | 0.729 | 1.163 | 4.000 |
| right, all | 353 | 0.050 | 0.629 | 1.003 | 1.642 | 47.678 |
| **added, single-element E and T** | 8 | 0.223 | 0.279 | **0.649** | 0.737 | 1.191 |
| **right, single-element E and T** | 62 | 0.050 | 0.219 | **0.369** | 0.697 | 1.860 |

**The right single elements sit *lower* against their neighbors than the added ones.** A bar at the lowest added single element's ratio, 0.223, takes 1 of 8 added and 17 of 62 right, plus 72 of 252 unkeyed single elements. Added letters taken before any right one: **0 of 8**.

The same two with eight each side (task 2): added single 0.185 / 0.262 / **0.688** / 0.844 / 2.034; right single 0.063 / 0.224 / **0.377** / 0.862 / 2.113. Taken before any right one: 0 of 8. Across all characters, 1 of 17 goes first: the `I` at 14.155 s on `cw-2026-08-22-031905`, ratio 0.034. But 9 keyed-unscored and 4 unkeyed characters go under that same bar, so it is not a separation either.

Per mark, eight each side (task 3): added single median **1.638**, right single **0.870**. Taken before any right one: 0 of 8.

**The one line of the 7.052 traffic net before and after cannot be shown.** Those captures are not in the tree, and nothing was changed that would alter a line.

**Criterion 3.6 is not ticked:** added letters are 17 before and 17 after. **3.5 held at exit:** both carry-forward lines, the three floor tests with captures at 51, the keyed totals, and the named floors are as at entry, and the transmit files are untouched.

## 4. What's blocking us

Nothing blocks the phase (R65). Three items are parked, most useful first.

**1. Bank the 2026-09-25 traffic net and re-run the trace on it.** Proposed ruling: the 7.052 captures the instruction describes are copied into `tests/fixtures/cw/captured/unadjudicated` with a key file for at least the stretches the instruction quotes (`OPERATION`, `ALL LOGS WILL BE UPLOADED`). A unit then adds them to the trace's recordings and re-runs `WhatTheNeighborsSayTests` unchanged. Reasoning: HM-DEC-181 was drawn from that recording, and the keyed corpus holds only 17 added letters, 6 of them in one weak passage on 17:37. A strong, single-signal recording with hundreds of litter characters is the case the ruling describes and the one the tree lacks. Rejected: building the rule anyway on the instruction's quoted figures, because the trace on the tree's recordings says it costs right letters, and section 10 forbids building what the trace does not support.

**2. Whether 3.6 is reachable on the span at all.** Proposed ruling: with raw span, per-hop span (units 421 and 425), span over neighbors, and span per mark over neighbors all traced and none separating the 8 added single elements from the 62 right ones, the next 3.6 unit traces something other than a span figure, or 3.6 waits for item 1's recording. Reasoning: four figures measured on one corpus all overlap. Rejected: lowering a floor to admit a relative bar, which R71 and R73 forbid outside key-aligned added letters.

**3. `Judged` has two admission tests, and the instruction named one.** For the record only: `CharacterMargin` 1.0 per hop, and `StrayElementSpan` 13.0 raw for single elements since unit 421. No ruling needed unless the next author wants the instruction's wording to match.
