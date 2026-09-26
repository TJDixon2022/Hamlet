READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at 3 of 5, step 3 at 1 of 6 by the plan's
   checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 2, criterion 2.5, with HM-REQ-080/081 on 17:37 and HM-REQ-010/011 as guards: the change
   not kept, because 17:37's boundaries wrong rose instead of falling to 5 or fewer and V-11 failed on
   three recordings. 17:37 boundaries wrong 7 -> 7 (8 under the change); MET-WBE 52 -> 52 over 113
   (43 under the change); MET-CER-SURE 0.1116 -> 0.1116 (0.1082 under the change); 17:37's floor red
   at 38; 2.5 not ticked, because no change was kept and 17:37 was not re-banked.
C. Section 4 raises 2 items; item 1 (whether 2.5 can be met on 17:37 by any honest gap rule) is in
   the way of 2.5. This report adds the first gap-by-gap trace of 17:37, measured at f14b2453 and
   at HEAD. It shows the 5 boundaries before G1 came from a backwards gap reading whose 455 ms
   threshold happened to take out two letter spaces of 310 and 320 ms. It also records a rule that
   improves the real set but fails 17:37.

UNIT:       444 - complete at task 3 of 3, none dropped - 2026-09-25 23:58
PHASE GOAL: The CW decoder meets CW_REQUIREMENTS.md, measured requirement by requirement, and at the end Tim at the radio says it reads.
UNIT GOAL:  Get 17:37's CQ to read DE and WB6RED as the words sent, keeping G1's letters, so that its named floor turns green honestly and step 2's exit holds.
ADVANCED:   no - no criterion flipped in PHASE_PLAN.md; the one change built failed R78 on 17:37 itself.
NUMBER:     17:37 boundaries wrong 7 -> 7; MET-WBE 52 -> 52; MET-CER-SURE 0.1116 -> 0.1116
DRIFT:      2 consecutive step-2 units without a kept change (was 1)

## 1. What Claude did

**Exit state: complete, at task 3 of 3, none dropped.** This ran on QUIVERFULL in Claude Code, on
project Hamlet, confirmed by the six-item gate, on branch `main`. Every commit was pushed to
`origin/main`: `8e08b146` (task 0), `cc1f9d0b` (task 1), `5a5643e7` (task 2), plus a closing
commit with this report.

**Task 0, the entry.**
- `PHASE_OUTCOME.md` gained `## UNIT 444 - STEP 2` in both copies, from the decision block, plus an
  `ENTRY:` line.
- `PHASE_STATUS.md` names 444 with `CURRENT_STEP: 2` in both copies. The docs copy still said 3.
- The version went from 1.13.130 to 1.13.131.
- The runner's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
  `WORK_INSTRUCTIONS.md` were committed as the runner wrote them. My only edits to those files were
  the 444 lines.

Entry round, one type per invocation:
- Build: 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 278 of 278.
- Captures: 51 of 51.
- Adjudicated: 13 of 13.
- Named: 12 of 13. 17:37 is red: banked 46, reads 38.

Metrics, real recordings, inferred keys:
- MET-CER-SURE: 47 of 421, 0.1116.
- MET-INVENTED: 47 over 473, 0.0994, of which 4 added and 43 substituted.
- Sure-and-right coverage: 374 over 473, 0.7907.
- MET-WBE: 52 over 113, 0.4602.
- **17:37 alone:** MET-WBE 7 over 6 words (6 inserted, 1 deleted), 1.1667. It reads
  `CQ CQ CQ DEWB6 RE D W B 7E E I`.

**The named floor.** It is `TheNumberCannotBeGamedTests.EachKeyedRecordingIsReadAtAll`, in
`tests/Hamlet.RadioEngine.Tests/Cw/TheNumberCannotBeGamedTests.cs`, row
`{ TheSeventeenThirtySevenCaptureTests.Name, 46 }` in `NamedFloors` at line 56. It counts the named
characters over the **whole recording**, not just the scored region. A named character is one that
is neither a word gap nor a placeholder, and it is counted only at or above the span bar.

**Task 1, the trace.** I added a printer,
`HowSeventeenThirtySevensGapsAreCalledTests.EveryGapInTheCqAndHowItWasCalled`. For every gap between
marks from 17:37's first `C` to the end, it prints:
- the time, and the length in ms and in units;
- the read that decided it, with that read's unit and its source (held measured gaps, or textbook
  1/3/7);
- the letter, word and relabel thresholds in force;
- what the read's window measured, and whether G1's condition held there;
- the path's call and the decoder's call;
- the key's call;
- the marks either side.

The key's call comes from lining the envelope's marks, as dit or dah, up with the key's elements.
All 61 marks matched. The key's second `WB6RED` runs past the audio.

It ran twice, and **both columns are measured**. The second run had `src/Hamlet.RadioEngine/Cw`
checked out at `f14b2453`, and `src` was restored afterwards. The same 60 gaps line up one for one.
Changed under G1:
- **Nine element gaps** inside `B`, `6` and `W` went from letter to element. The key agrees with G1
  on all nine. These are G1's letters.
- **Two letter spaces became word spaces:**
  - 6|R at 24.030 s: 310 ms, 4.43 u.
  - W|B at 26.830 s: 320 ms, 4.57 u.
  - On both, the key agrees with the old call. These two are the whole of 5 -> 7.

**Cause:** on 17:37, G1's condition is made by one key-up of 10 to 15 ms (0.14 to 0.21 u, 1 of 44
gaps). That is a dropout inside a mark, and it takes the shortest gap cluster to itself. G1 refuses
the window, no character gap is measured in it, and the stream stays on a spacing whose word
threshold sits below those two letter spaces: 303 ms (4.33 u) at 24.030 s and 254 ms (3.63 u) at
26.830 s. Before G1, the refused reading itself was held (10/828/250 ms). Its backwards relabel
boundary of 455 ms (6.5 u) took the path's spaces out.

**Other recordings.** Comparing boundaries wrong per recording, f14b2453 -> HEAD, **only 17:37
rose**. So the cause reaches no other keyed recording whose MET-WBE rose under G1, because there is
none. `031838` fell 5 -> 1, `032012` 3 -> 2 and `032129` 7 -> 5. Real MET-WBE went 57 -> 52.

**Task 2, one change.** The rule came from the trace and was set before any numbers: **the gap
clustering in `CwUnitEstimator.MeasureGaps` and `MeasureCharacterGap` leaves out key-ups shorter than
half a unit.** The trace puts the dropouts at 0.14 to 0.40 u and the shortest element gap the key
confirms at 0.57 u. G1, the marks' speed, `RivalMargin` and `MarginLlr` were not touched.

| part of R78 | before | under the change | verdict |
|---|---|---|---|
| MET-WBE, 17:37 | 7 | 8 | **rises; 5 or fewer needed** |
| MET-WBE, real | 52 over 113 | 43 over 113 | falls |
| MET-CER-SURE, real | 47 of 421, 0.1116 | 46 of 425, 0.1082 | falls |
| MET-INVENTED, real | 47 over 473 | 46 over 473 | falls |
| sure-and-right, real | 374 | 379 | rises |
| adjudicated | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | 17:37 7 -> 8, `031905` 2 -> 3, `cq-18wpm-5db-char5` 10 -> 14 boundaries | **fails** |
| 17:37 wrong-or-added / sure-right | 3 / 17 | 3 / 17 | G1's letters stay removed |
| capture rows | 51 of 51 | 49 of 51 (`004234` 36 -> 34, `004427` 42 -> 41) | reported |

**Not kept: 17:37's boundaries rise and V-11 fails.** The diff is
`.run-unit/unit444-wbe-notkept.diff`, and `src` carries none of it. It is recorded in
`docs/phase-requirements/metrics.md` per condition, with the key's kind beside each number. 17:37
was not re-banked.

**Task 3, the exit round.**
- Build: 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 276 of 278. Both losses are `ThePowerIsOfferedTests` ("You've caused dispatcher
  loop"). That type passes 3 of 3 alone, so this is the recorded dispatcher-loop loss.
- Captures: 51 of 51.
- Adjudicated: 13 of 13.
- Named: 12 of 13. 17:37 is red at 38.
- Metrics: identical to entry.
- `HowSeventeenThirtySevensGapsAreCalledTests`: 1 of 1.
- `TheFiveToEightDecibelPlateauHolds` was not run. It is in neither line.

**`src` at exit against entry `1f6a5789`: no file changed.** Nothing that keys or transmits was
touched. The one tree change outside the record is the new printer in `tests/`.

**2.5 is not ticked**, because no change was kept and 17:37 was not re-banked. The commits of units
442 and 443 all exited with 17:37 red, and so did every commit of this unit. **2.4 is not ticked.**
DRIFT for step 2 is 2: 441 kept a change, and 442 and 444 did not.

**Mismatches with the instruction, reported and not repaired:**
- **V-11 is in `CW_REQUIREMENTS.md` (line 254), not `CW_SPEC.md`.** It reads: "No change may make
  an earlier capture or the synthetic corpus go red to make a newer one green."
- **HM-REQ-010:** "On every must-tier condition at or above the sensitivity floor, the decoder shall
  keep MET-CER-SURE below 1 %."
- **HM-REQ-011:** "On every condition at or above the sensitivity floor, the decoder shall keep
  MET-INVENTED at zero."
- **HM-REQ-080:** "On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder shall
  place every word boundary where the sender placed it (MET-WBE = 0)."
- **HM-REQ-081:** "On every must-tier condition at the sensitivity floor, the decoder shall keep
  MET-WBE at or below 5 % of words."
- **HM-REQ-082:** "The decoder shall score word-boundary errors separately from character errors."
- None of the five names 17:37's condition. That condition is real HF with no CH-* profile, and its
  sender is not stated in `CW_SPEC.md`.
- **The instruction's facts that held:**
  - G1 and 14f515bd are in HEAD's ancestry.
  - G1 is in `MeasureGaps` as described.
  - `metrics.md` gives both texts.
  - The counts 5 before G1 and 7 after were confirmed by measurement at both commits.
- **Known and not mine, reported once:**
  - `PHASE_OUTCOME.md`'s header has stale step titles.
  - `CW_SPEC.md` §11 still has the old MET-COVERAGE text.
  - On RULES_AT: `CLAUDE.md` holds HM-DEC-165 in its log (line 376). A search of `CLAUDE.md` finds no
    CPS-DEC-0183.

**Decisions I made myself:**
1. I measured the before-G1 half rather than reasoning it, since the unit had time.
2. I also ran the metrics at f14b2453, to get the per-recording comparison.
3. The key's call on a gap comes from an edit alignment of marks (dit or dah) to the key's
   elements, not from the letter alignment.
4. The printer reads the stream's held spacing by reflection, so the same file runs at both commits.
5. I added the character-gap column to the trace before any rule was chosen.
6. The half-unit line was set from the trace alone, and I did not try a second value or a second rule.

**`validate-output.bat` was not run: my permissions refused the call.** I checked its rules by hand:
- The ordering block is at lines 1 to 14, with `READ IN THIS ORDER.` and lines starting `A.`, `B.`
  and `C.`.
- Line C carries `Section 4 raises 2 items`.
- The `UNIT:` line has no brackets.
- There are exactly the four headings, and section 3 is not empty.

## 2. What the owner should expect

No: the operator still reads `CQ CQ CQ DEWB6 RE D W B`. Nothing the decoder prints changed, because
the one change was not kept. Letters are unchanged, and no sure letter was turned wrong. The evidence
is an inferred key only (V-13). Floors are as recorded: 17:37 is red at 38, and everything else is
green.

What is new is why the "5 before G1" was 5. The pre-G1 decoder held a backwards gap reading, where
the character gap was longer than the word gap. By accident, that put a 455 ms threshold above
this sender's 310 and 320 ms letter spaces. This sender's letter spaces (135 to 320 ms) and word
spaces (245 to 485 ms) overlap. `DE|W` is a 245 ms word space, shorter than six letter spaces
around it, so no duration rule can place it.

**What will look wrong but is not:**
- The app line reads 276 of 278 at exit. The two losses are the recorded dispatcher loop, and that
  type is green alone.
- The rejected rule improved the real set: MET-WBE 52 -> 43, and `004234` read `THANK YOU FOR`
  instead of `T HANTT TK■ET FOR`. It is still out, by R78 and V-11.

## 3. What you should see

**17:37, as the operator reads it** (key `CQ CQ CQ DE WB6RED WB6RED`, inferred):

| when | text | boundaries wrong |
|---|---|---|
| before G1 (f14b2453) | `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` | 5 |
| at entry (1f6a5789) | `CQ CQ CQ DEWB6 RE D W B 7E E I` | 7 |
| at exit (src unchanged) | `CQ CQ CQ DEWB6 RE D W B 7E E I` | 7 |
| under the not-kept rule | `CQ CQ CQ DEWB 6 RE D W B 7E E I` | 8 |

**No other recording's text changed at exit.** Under the rule while it was built, 16 texts changed.
The full diff is in `.run-unit/unit444-text-diff.txt`. The ones an operator would notice:

| recording | at HEAD | under the rule |
|---|---|---|
| `004234` | `C O M <BT> T HANTT TK■ET FOR ON` | `C OM <BT> THANK YOU FOR ON` |
| `032113` | `A KET■ A N O INT ERNE T ■ E RSIONS OF` | `A KET■ ANO INTERNET ■ERSIONS OF` |
| `032050` | `CAN BE FOTA ND IN` | `CAN BE FOTAND IN` |
| `004405` | `A N T HONY LUSCRE` | `ANTHONY LUSCRE` |
| `004347` | `W ILL BE UPLOADED` | `WILL BE UPLOADED` |
| `004427` | `QSL TN6TBRE C E` | `QSL TN6BRE C E` |
| `031905` | `IAIEI TANI   WLUX` | `IAIEI TA NI   WLUX` |
| `013637` | `S KY LI TE ... ALL DAY JUST AWE SO` | `S KY L I TE ... ALL D AY  JU S T A WE H O` |
| `cq-18wpm-5db-char5` | `C QC Q C Q TEE T ■KTDUUEUE N 0 C A L L K` | `C QCQCQDEN0 NC A L L N 0 C A L L K` |

Where to look:
- The gap-by-gap line-up is `.run-unit/unit444-lineup.txt`.
- The trace summary is `.run-unit/unit444-trace-summary.md`.
- The per-recording V-11 table is `.run-unit/unit444-v11-change.txt`.

## 4. What's blocking us

1. **Rule whether 2.5 is to be met on 17:37 by a boundary count of 5 or fewer, or whether 17:37's
   named floor may be re-banked at 38 with its boundaries at 7.**
   - Reasoning: the trace shows the pre-G1 count of 5 was produced by a backwards gap reading, not
     by a rule that measured this sender. The two boundaries G1 "lost" are letter spaces of 310 and
     320 ms, while this sender also sends word spaces of 245 and 330 ms.
   - Under R72, any duration rule that restores both must place a boundary between 320 and 330 ms
     in the reads that decide them. That is tuning to one recording.
   - While 443's DECIDED (3) stands, no unit can tick 2.5.
   - Rejected: building a second rule to reach 5, which the instruction forbids and V-11 would test
     on a knife edge. Also rejected: re-banking here, which 443's ruling forbids.
2. **Consider the half-unit dropout rule (`.run-unit/unit444-wbe-notkept.diff`) as a candidate for
   step 6's word-boundary work.**
   - Reasoning: on the real set it takes MET-WBE from 52 to 43 over 113, MET-CER-SURE from 0.1116
     to 0.1082 and MET-INVENTED from 47 to 46, and it reads `004234`'s `THANK YOU FOR`.
   - It failed here only on 17:37 (7 -> 8), `031905` (2 -> 3) and the synthetic character-gap-5 case
     at 5 dB (10 -> 14).
   - Rejected: keeping it now, because R78 and V-11 refuse it and step 6 is parked for this unit.
