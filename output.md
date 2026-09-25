READ IN THIS ORDER.

A. MET-INVENTED today over the keyed recordings, against HM-REQ-011's zero: 67 invented over
   473 characters sent, inferred keys, 23 of 23 recordings (share 0.1416).
B. Of the sixty-eight requirements, 5 have a test that proves them and 63 have none. 43 have a
   test that measures something other than what the requirement states.
C. The rest. Section 4 raises 6 items.

```
UNIT:       439 - complete at task 4 of 4, none dropped - 2026-09-25 16:18
            ran by hand, outside the loop
PHASE GOAL: Hamlet meets the CW requirements, measured on the requirements' own metrics
            rather than on character counts.
UNIT GOAL:  Find which requirements any test proves today, and build the four metrics
            the requirements are written in, so a later unit can keep a change on them.
NUMBER:     MET-INVENTED over the keyed recordings: 67 against HM-REQ-011's zero
            (13 sure added, 54 sure wrong, over 473 sent, inferred keys);
            requirements with a test 5 of 68, with none 63, with a test that
            measures something else 43
```

## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both claimed `PROJECT: Hamlet`, and the tree
confirmed it:
- all four `MUST EXIST` files are present;
- neither `CoreHMI.sln` nor `MURC.sln` exists;
- `PROJECT_CARD.md` says `PROJECT: Hamlet`.

The first paste had no gate line and was refused. Nothing in this report is evidence about the
radio.

**Run by hand, outside the loop.** `tools\arbiter\run-phase.bat` halts before it launches
anything. Its arbiter re-decides `MOVE: stop` from stale evidence about the phase transition,
and the instruction does not wait for that fix. Here is what the hand run changed:
- `SESSION.lock` was taken at 15:15:53 through `tools\arbiter\lock.bat take` and released at the
  end the same way.
- Nothing was written to `RUN_LEDGER.md` and nothing under `tools\arbiter\` was touched.
- No box in `PHASE_PLAN.md` was ticked.

`CW_REQUIREMENTS.md` and `CW_SPEC.md` were read before anything else. Commits `1733f862` to
`7fbe7e77` went to `main` and were pushed (see section 2).

**Task 0, the record (`1733f862`).**
- Version went from 1.13.125 to 1.13.126.
- `PROJECT_CARD.md` now reads `PHASE: Hamlet meets the CW requirements` and
  `PHASE_SET: 2026-09-25`.
- `PHASE_STATUS.md` names unit 439 and `CURRENT_STEP: 0`, in both copies.
- `PHASE_OUTCOME.md` gained `## UNIT 439 - STEP 0` in both copies, taken from the instruction's
  decision block, with a `RUN:` line saying it ran by hand.
- `DECISIONS.md` gained HM-DEC-183, and `CLAUDE.md` section 1 gained its row.

**HM-DEC-183 departs from the instruction's text in one clause.** The instruction had it say
that the knowledge rule was ruled "as HM-DEC-175". In this tree HM-DEC-175 is the CW-block
receiver ruling. The knowledge rule is R72, which HM-DEC-181 records as standing. The entry says
that and says why. The rest of its text is the instruction's.

The entry round, which gives the numbers to beat:
- build 0 errors;
- engine carry-forward 178 of 178 in 394 s;
- app carry-forward 275 of 278 in 176 s. The three failures are the dispatcher-loop loss, and
  each type is green alone (4 of 4, 12 of 12);
- captures 51 of 51, adjudicated 13 of 13, named 13 of 13;
- 165 edits over 565 against inferred keys over 23 keyed recordings;
- 17 added named characters (8 single-element) and 56 wrong;
- 17:37 at 19 over 25.

**Task 1, every test (`4626fce9`, 0.1).** `docs/phase-requirements/traceability.md` section 3
lists all 621 CW test methods in 125 files. Each row gives the file, the type, the method, what
it proves, what it measures instead, and what it asserts.

Where the session looked:
- every method in `tests/Hamlet.RadioEngine.Tests/Cw/`, including `Fixtures/`;
- every method in `tests/Hamlet.App.Tests/Cw/`;
- every other test file that names a receive-decoder type (21 files);
- the FT8 projects and `tests/Shared`, which hold no CW test.

Files that name only a CW transmit, band-plan or rig type are outside the decoder's boundary
and are not listed.

The counts:
- 8 methods prove a requirement;
- 612 prove `none`;
- 1 is `unclear`: `TheScorerCountsWhatAHandCountsTests.EachKindIsCountedWhereAHandCountsIt`
  against HM-REQ-082;
- 140 are on a requirement's subject but measure something else;
- 53 sit in the twelve files excluded from compilation.

Five subagents classified the methods from source under `.run-unit/unit439h-rubric.md`. The
session then re-read the asserts of all 8 proving methods before accepting them.

**Task 2, every requirement (`da4f941e`, 0.2 and 0.3).** Section 1 of the same file extends
section T to all 68 requirement ids:
- 5 are proved: 005, 011, 031, 066 and 094;
- 63 have no proving test, 60 of them must-tier;
- 43 have a test that measures something other than what they state, and 38 of those have
  nothing else;
- 25 have no test on their subject at all.

The `none` rows are grouped by section in section 3 below.

Section 2 of the file places every name in the three lists:
- `docs/unit239-failing-set.txt`, 51 names: every one is placed. None proves a requirement, and
  most measure something else against 005, 013, 090, 091 or 092.
- The known-reds block, 7 entries: it names no CW test. Its CW line reads `none`.
- `docs/cw-retired-tests.txt`, 20 names: each was read from history. **None proved a must-tier
  requirement, so there is no finding under 0.3.**

Nothing was retired, repaired or restored.

**Task 3, MET-INVENTED (`53a05598`, 1.1).**
- **Where the metrics live.** `tests/Hamlet.RadioEngine.Tests/Cw/CwMetrics.cs` computes the
  metrics as `CW_SPEC.md` 11 defines them.
- **What a character is.** One symbol, with a prosign counting as one. Characters sent exclude
  spaces.
- **How it aligns.** Characters are aligned with the spaces taken out of both sides, using
  `CwScorer`'s own Levenshtein encoded one symbol to a character.
- **Watched failing first.** `TheMetricsCountWhatAHandCountsTests` went 6 of 7 red against a stub
  that counted nothing. The one that passed expects nought. Against the real count it went 7 of 7
  green.
- **The measurement.** `TheRequirementsAreMeasuredTests.MetInventedOverEveryKeyedRecording` is a
  printer. It uses the baseline's own recordings, keys and scored stretches. The total over the
  real keyed recordings is **67 invented over 473 sent** (13 sure added, 54 sure wrong), with
  inferred keys, on 23 of 23 recordings.

**It is not 17, and that is a finding.** The old 17 is reproduced beside it over the same
stretches: 17 added named characters, 8 of them single-element. The two numbers differ for two
reasons:
- The old count left substitutions out entirely, and MET-INVENTED counts them. That accounts
  for 54 of the gap.
- With spaces set aside, three recordings move an added letter to wrong:
  - `unadjudicated/cw-2026-08-22-031905`: added 2 to 0;
  - `unadjudicated/cw-2026-08-22-032050`: added 2 to 1;
  - `cw-2026-08-18-004507`: added 1 to 0.

**Task 4, the other three and the exit round (`efdad2bc`, `7fbe7e77`, 1.2 to 1.5).**
- **What was added.** MET-CER-SURE, MET-COVERAGE and MET-WBE went into `CwMetrics`, with six
  hand-counted tests.
- **Watched failing first.** The new tests went 6 of 6 red against stubs, then 13 of 13 green.
  `TheRequirementsAreMeasuredTests.TheOtherThreeOverEveryKeyedRecording` measures them.
- **The classes.** The decoder emits only sure and placeholder. A named character below high is
  counted apart and never as sure, and it counted 0. No dim class was made.
- **MET-WBE is scored on boundaries alone,** on the letters-only alignment.
- **How MET-WBE relates to `CwScorer.Kinds`:**
  - On the synthetic set the two agree exactly: 19 inserted and 30 deleted.
  - On the real set MET-WBE counts 42 inserted and 15 deleted, while `Kinds` counts 55 spaces
    added and 5 missing. `Kinds` works in the text alignment, where a letter can be seated
    against a space.
- **1.4, reaching the metrics.** `CwMetrics` is public and static in the engine test assembly,
  as `CwScorer` is. `TheMetricsCountWhatAHandCountsTests` and `TheRequirementsAreMeasuredTests`
  both call all four metrics.
- **The baseline, re-issued.** It is written up in `docs/phase-requirements/metrics-baseline.md`.

The exit round matched entry on every figure:
- build 0 errors;
- engine 178 of 178;
- app 275 of 278, the same dispatcher-loop loss on different victims
  (`TheCarrierHoldsTheButtonsTests`, `TheFavoritesAreUnderTheGreenZoneTests`), each green alone;
- captures 51 of 51, adjudicated 13 of 13, named 13 of 13;
- 165 over 565, 17 added, 8 single;
- 17:37 at 19.

**`git diff` over `src` and `data` from entry `7734ecbe` prints nothing.** The eleven transmit
files print nothing against `7e209cb4`.

**Decisions the session made for itself** (author's, overrulable):
1. **The version is a patch bump, as the instruction says.** HM-DEC-150 says a phase bumps the
   minor, but every phase since 1.13.0 has bumped the patch. This is item 3 of section 4.
2. **HM-DEC-183 keeps the id the instruction gave it.** HM-DEC-182 is absent from the tree
   (item 2).
3. **A test that proves `none` but is on a requirement's subject is named in its own column.**
   That column is how "measures something else" is counted.
4. **The keyed corpus is the 23 real recordings plus the 12 synthetic CQ cases.** They are
   reported apart and never summed, because one has inferred keys and the other exact.

No decision was recorded under CLAUDE.md 12.1.

## 2. What the owner should expect

Nothing in the app changed. Not a line under `src` or `data` moved, so the radio, the CW tab
and the decoder behave exactly as they did this afternoon.

What changed is what the project can measure. Until today "is it better" meant "did a capture
row's character count fall". Now there are the four numbers the requirements are written in:
- how many sure letters were never sent;
- what share of sure letters are wrong;
- how much of what was sent came out sure;
- how many word gaps are misplaced.

Each is counted per recording, with the key's kind beside it. On the real recordings those
numbers are 14 per cent invented, 16 per cent of sure letters wrong, coverage 0.90 and half the
word gaps wrong. The coverage looks good only because the wrong sure letters count toward it:
359 of 473 sent came out sure and right. So the decoder is nowhere near the requirements yet,
and for the first time that can be said in the requirements' own terms.

As for the specification itself, 5 of its 68 requirements have a test behind them today. Most
of the rest have a test nearby that measures something else, and 25 have nothing at all.

What will look wrong but is not:
- **`DecisionLogOrderTests` is red.** It was already red at entry, on a missing HM-DEC-166 row.
  HM-DEC-183 adds HM-DEC-182 to its gap list; see item 2.
- **App carry-forward is 275 of 278.** The three are the known dispatcher-loop loss, and each
  type passes alone.
- **The 0 dB synthetic cases show no invented letters.** They emit nothing sure at all, so
  their MET-CER-SURE has no number.
- **Version 1.13.126 carries no code change.**

**Push:** `git push origin main` after the closing commit; the result is recorded in the
commit that follows it.

## 3. What you should see

**MET-INVENTED over the keyed recordings, per condition.** HM-REQ-011 requires 0. None of the
real recordings was received on a named channel profile or carries an SNR in the 2500 Hz
reference, so none of these groupings is yet a condition in the spec's full sense (item 5).

| set | condition as the spec can state it | key | invented | sure added | sure wrong | sent | share | recordings |
|---|---|---|---|---|---|---|---|---|
| real | sender TX-FARNS (004507, HM-DEC-115) | inferred | 1 | 0 | 1 | 44 | 0.0227 | 1 |
| real | sender TX-ITU (012403, KD0UN) | inferred | 0 | 0 | 0 | 13 | 0.0000 | 1 |
| real | sender TX-TIGHT (013347) | inferred | 0 | 0 | 0 | 6 | 0.0000 | 1 |
| real | sender not stated in CW_SPEC.md | inferred | 66 | 13 | 53 | 410 | 0.1610 | 20 |
| **real** | **all keyed recordings** | **inferred** | **67** | **13** | **54** | **473** | **0.1416** | **23 of 23** |
| synthetic | TX-ITU, 15 dB in the passband | exact | 8 | 6 | 2 | 63 | 0.1270 | 3 |
| synthetic | TX-ITU, 5 dB in the passband | exact | 4 | 3 | 1 | 63 | 0.0635 | 3 |
| synthetic | TX-ITU, 0 dB in the passband | exact | 0 | 0 | 0 | 63 | 0.0000 | 3 |
| synthetic | character gap 5 units, 15 dB | exact | 1 | 0 | 1 | 21 | 0.0476 | 1 |
| synthetic | character gap 5 units, 5 dB | exact | 11 | 4 | 7 | 21 | 0.5238 | 1 |
| synthetic | character gap 5 units, 0 dB | exact | 0 | 0 | 0 | 21 | 0.0000 | 1 |

Every synthetic row has no fading and a shaped noise band that is not shown to be `CH-AWGN`.
Its level is stated in the passband and not restated in the reference. Per recording, the
largest real contributors are:
- 17:37: 14 of 20;
- `032129`: 13 of 38;
- `031838`: 11 of 26;
- `031905`: 10 of 33.

The full per-recording tables are in `docs/phase-requirements/metrics-baseline.md`.

**The other three metrics, totals:**
- **Real, inferred keys.**
  - MET-CER-SURE: 67 wrong or added of 426 sure, 0.1573, against HM-REQ-010's 0.01.
  - MET-COVERAGE: 426 over 473, 0.9006, with 359 of them right, against HM-REQ-012's 0.90.
  - MET-WBE: 57 over 113 words, 0.5044, against HM-REQ-081's 0.05.
- **Synthetic, exact keys.**
  - MET-CER-SURE: 24 of 180, 0.1333.
  - MET-COVERAGE: 180 over 252, 0.7143.
  - MET-WBE: 49 over 84, 0.5833.

**The requirements with no proving test, by section.** This is the map the next steps are aimed
with.

| section | requirements | no proving test | no test on the subject at all | ids |
|---|---|---|---|---|
| A. Honesty of the record (step 2) | 9 | 8 | 5 | 001 002 003 004 006 007 008 009 |
| B. Confidence (step 3) | 6 | 5 | 1 | 010 012 013 014 015 |
| C. Refusal and sensitivity | 4 | 4 | 2 | 020 021 022 023 |
| D. Speed (step 5) | 7 | 6 | 0 | 030 032 033 034 035 036 |
| E. Channel and fading (step 7) | 5 | 5 | 4 | 040 041 042 043 044 |
| F. Sender profiles (step 7) | 5 | 5 | 4 | 050 051 052 053 054 |
| G. Interference (step 7) | 7 | 6 | 2 | 060 061 062 063 064 065 |
| H. Character set (step 6) | 4 | 4 | 3 | 070 071 072 073 |
| I. Word boundaries (step 6) | 5 | 5 | 2 | 080 081 082 083 084 |
| J. Pitch (step 4) | 5 | 4 | 0 | 090 091 092 093 |
| K. Acquisition time and latency (steps 4, 5) | 4 | 4 | 1 | 100 101 102 103 |
| L. Signal measurements reported | 7 | 7 | 1 | 110 111 112 113 114 115 116 |

Next, in order:
1. **Step 2, section A.** It is the emptiest honesty group and depends only on step 0.
   HM-REQ-003 and 004 are inspection, and 001, 002, 005, 006 and 007 need tests naming them.
2. **Step 3.** The metrics exist now. HM-REQ-011 at 67 is the number a change is kept on under
   R78.
3. **Step 7's generator.** E and F have almost nothing to stand on, and no real recording can
   stand for a spec condition.

## 4. What's blocking us

Nothing blocks the next step. Six items for the owner, the one touching the most work first.

**1. A real capture cannot yet stand for a condition the spec defines.** `CW_SPEC.md` section 4
states every condition as a `CH-*` profile, a `TX-*` profile and an SNR in the 2500 Hz reference.
No real recording carries the first or the third. The three that section 10 names by sender are
grouped under that sender; the rest are "sender not stated". So HM-REQ-010, 011, 012 and 081
can be measured on the corpus but not judged at their own conditions. That judgment will come
from the synthetic side once step 7 generates the profiles. Proposed ruling: real captures are
reported by the sender profile the spec names, or "not stated", and never counted toward a
`CH-*` condition. Rejected: assigning a `CH-*` profile to a real capture from its band or time,
which asserts a channel nobody measured.

**2. HM-DEC-182 was ordered and never recorded, and `DecisionLogOrderTests` names it.**
- Work instruction 434 ordered HM-DEC-182, "The tone tracker is open, once an instrument can
  judge it" (R75 and R76), and it is in neither `DECISIONS.md` nor the index.
- This unit recorded HM-DEC-183 as instructed.
- `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` was already red at
  entry: `CLAUDE.md` has no row for HM-DEC-166, which `DECISIONS.md` does have. It now reports
  gaps 105, 136, 166 and 182.

Proposed ruling: HM-DEC-182 is recorded from work instruction 434's block, and HM-DEC-166 gains
its `CLAUDE.md` row. Rejected: adding either to the test's known gaps, because a gap that is a
missing record is exactly what that test exists to catch.

**3. The version scheme and practice disagree.**
- HM-DEC-150 says a phase bumps the minor and resets the patch.
- Every phase since the PSK31 phase opened at 1.13.0 has bumped only the patch, and this
  instruction said to patch-bump, so 1.13.126 opens the requirements phase.

Proposed ruling: either HM-DEC-150 is superseded to say the patch counts units across phases,
or the next phase opens at 1.14.0 and later ones follow. Rejected: renumbering past releases.

**4. Two metric definitions in `CW_SPEC.md` 11 read two ways.**
- MET-CER-SURE is "MET-CER over characters emitted as sure". This unit took the denominator as
  the sure characters emitted, as HM-REQ-010's "one in a hundred sure characters wrong" does,
  rather than characters sent.
- MET-COVERAGE is "sure characters emitted / characters sent" as written. It exceeds one when
  sure characters are added: the 5-unit 5 dB synthetic reads 1.19.

Proposed ruling: MET-CER-SURE's denominator is sure characters emitted, and MET-COVERAGE counts
sure characters that align to a sent character, so it cannot exceed one. Rejected: leaving
coverage as written, because a decoder that adds sure letters scores better on the guard meant
to catch it. Both are author's today and change no number but coverage.

**5. Two green tests assert what a requirement or a ruling contradicts.**
- `CwEmissionGateTests.TheBoundsAreTheRadiosOwn` pins `CwDecoder.SlowestPlausibleWpm` at 6.
  HM-REQ-030 requires 5 WPM.
- `CwSurveyThresholdPinTests.FindingTheToneIsNotClaimingSomebodyIsSending` asserts
  `cw-2026-08-17-134712` holds no keying. HM-DEC-144 ruled it holds `N4L`.

Proposed ruling: each is rewritten to its requirement in the step that owns it, step 5 and
step 4. Rejected: changing them here, which this unit may not do.

**6. Section T of `CW_REQUIREMENTS.md` and the trace differ.**
- T lists `NothingIsInventedAtTheHandover` as the test for 064, and it allows half of what is
  emitted to be invented.
- Eight other T rows name tests that do not assert the requirement's own threshold or range.
- T says none for 011, 066, 094, 005 and 031, and a test proves each.

`CW_REQUIREMENTS.md` is not edited by a session. The traceability file lists the differences,
and T is the owner's to amend or leave.

### Asks still outstanding

None carried in. `WORK_INSTRUCTIONS.md` 439 has no `Asks still outstanding` heading, which
CLAUDE.md 9.6 makes a defect of the order. The queue was reconstructed from the last report,
unit 439 of the archived phase, whose section 4 raised three items. All three concern that
phase's 7.4, are parked with it by HM-DEC-183 and PHASE_PLAN.md 7, and are not carried. The six
items above are this unit's own and are new.
