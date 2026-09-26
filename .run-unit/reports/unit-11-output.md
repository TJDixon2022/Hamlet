READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at 3 of 5 and open, step 3 at 3 of 6, step 4 at
   4 of 7, step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 2, criterion 2.4 (HM-REQ-010): 40 sure-wrong letters at HEAD traced, largest open group
   "read after the tracker left the station for a quieter keyed candidate" with 9 (against 6
   right); the change kept; MET-CER-SURE real 40 of 433 -> 33 of 436, synthetic 14 of 173 ->
   14 of 173; MET-INVENTED 40 -> 33; coverage 393 -> 403; 2.4 not ticked, count reset to 0;
   2.5 red on 17:37, 032113 and 032129.
C. This report adds a trace of the forty with the instrument's pitch beside each, and one kept
   tracker change that stops the filter leaving a station for a keyed tone 10 dB or more below
   it. Section 4 raises 3 items; none is in the way of a criterion in B.

UNIT:       449 - complete at task 3 of 3, dropped the 440/441 matching (the named drop candidate) - 2026-09-26 06:45
PHASE GOAL: Hamlet meets every must-tier CW requirement, measured by the metrics CW_SPEC.md defines.
UNIT GOAL:  Fewer letters printed sure and wrong: trace the forty left at HEAD with the instrument's pitch beside each, build one change against the largest cause no unit has tried, and keep it only under R78; if refused, close step 2 partial.
ADVANCED:   no - no criterion flipped; the change was kept, so 2.4 does not flip, and HM-REQ-010's number moved and step 2's count went to 0
NUMBER:     MET-CER-SURE real 40 of 433 -> 33 of 436; synthetic 14 of 173 -> 14 of 173; MET-INVENTED 40 -> 33; coverage 393 -> 403; largest group 9
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1

## 1. What Claude did

**Complete at task 3 of 3.** The named drop candidate was dropped: matching the 40 against 440's
54 and 441's 43. Time went to task 2 instead. Claude Code on this machine, project Hamlet, branch
`main`, HEAD at entry `6b872543`. Commits: `0dfdd3a6` (task 0), `e8c5ad58` (task 1), `573a3c5e`
(the change and its test, on their own), `779999af` (task 2's record), and the exit commit. All
are pushed to `origin/main`.

**Gate.** Hamlet confirmed. The four MUST EXIST files are present, `CoreHMI.sln` and `MURC.sln`
are absent, and the root is `C:\Source\HamLet`.

**The ids, quoted from `CW_REQUIREMENTS.md`:**
- HM-REQ-010: "On every must-tier condition at or above the sensitivity floor, the decoder shall
  keep MET-CER-SURE below 1 %." Matches.
- HM-REQ-011: "... shall keep MET-INVENTED at zero." Matches.
- HM-REQ-012: "... shall emit at least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90)."
  Matches. R82 reads coverage as sure-and-right over sent.
- HM-REQ-014: "... characters the decoder emits as dim shall be correct at least 70 % of the
  time." Matches.
- **Mismatch: HM-REQ-015 is not a dim rate.** Its text is "The decoder shall make each
  character's confidence class available at the moment the character is emitted." I report
  dim over named letters emitted as "the dim rate", under my own definition.
- **V-11 is in `CW_REQUIREMENTS.md` (line 254), not `CW_SPEC.md`:** "No change may make an
  earlier capture or the synthetic corpus go red to make a newer one green." It matches in
  substance.

**Checked against the tree (§2 of the instruction):**
- **Confirmed as stated:**
  - HEAD was `6b872543`.
  - In both copies of `PHASE_PLAN.md`, 2.1 to 2.3 are ticked and 2.4 and 2.5 are not.
  - The printer, `CwMetrics.cs` and `Cw/Instruments/CwPitchInstrument.cs` (under `tests`) are
    where stated.
  - Step 2's count of units with no kept change is 2 of 3: 442 (unit 2) and 444 (unit 6), with
    no step-2 unit since.
- **Mismatch, as the instruction asked me to report it:** `PARKED.md`'s header says it is
  "Written by run-phase.bat, never by a session", while 2.4 sends the trace there. The change
  was kept, so no line was appended and the file is untouched.
- **Expected failures, as stated:**
  - The named floors: 17:37 38 against 46, `032113` 43 against 45, `032129` 42 against 64.
  - The app line lost 5 at entry and 3 at exit. Every losing type is green alone.
  - **No host hang this time,** so 448's item 4 did not repeat.
- **Known and not mine, each reported once and none edited:**
  - `PHASE_OUTCOME.md`'s header still has the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.

**Task 0.**
- Version 1.13.135 -> 1.13.136. `PHASE_STATUS.md` names 449 at `CURRENT_STEP: 2` in both copies.
- The runner's `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `PARKED.md`, `RUN_LEDGER.md` and
  `WORK_INSTRUCTIONS.md` were committed as they were, with the entry.
- **Entry round, identical to 448's exit:**
  - build: 0 errors in 15 s;
  - engine carry-forward: 178 of 178 in 378 s;
  - app carry-forward: 273 of 278 in 167 s;
  - floors: captures 51 of 51 in 130 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 66 s;
  - metrics: as the instruction's table;
  - MET-PITCH-ERR: 9 files more than 25 Hz off, 256 of 1688 windows, in 253 s.

**Task 1: the trace.** I wrote `WhatTheFortySureWrongLettersRestOnTests` as a **sibling** rather
than extending 440's printer. That one traces only substitutions on the real set; this one needs
the added letters, the synthetic set, and the tracker hop by hop.
- It prints every sure wrong or added letter with:
  - span, the speed in force, and the speed the window's marks and the letter's own marks imply;
  - the decoder's mix pitch, the tracker's pitch and the instrument's pitch;
  - any retune inside the letter or within 1 s of it;
  - marks and inner gaps in units, with 445's verdict;
  - `MarginLlr` and the rival reading;
  - whether it came before the first keyed verdict (a fact only);
  - the excluded routes whose own test covers it.
- It asserts only that the count is MET-CER-SURE's own 40. Its output is
  `.run-unit/unit449-trace-head.txt`.
- **The rival reading is a new `src` field, for the record only.** `RivalMargin` computed the
  rival and threw it away. `CwProbabilisticDecoder.Rival` now keeps it, and it reaches
  `CwCharacter.RivalReading`. Nothing decides on it, and all 126 recording texts were identical
  before and after.
- **Findings:**
  - 14 of the 40 were mixed more than 25 Hz off the instrument's pitch, against 20 of 393 right
    letters.
  - 13 came before the first keyed verdict.
  - 14 are covered by no excluded route's test.
  - **The largest group by cause, which no excluded route addresses: letters read after the
    tracker left the station for a quieter keyed candidate.** Its signal, mixed 100 Hz or more
    from the pitch the decoder held longest in the file, covers 9 wrong or added against 6 right.
  - On `031905` the survey confirmed a 300 Hz candidate at 12.50 s. Its key-down was 21.9 dB
    below the 500 Hz station's (−49.4 against −27.5 dB). The tracker moved at 13.04 s and read 8
    wrong and 1 right there. HM-DEC-127's floor refuses only at 25 dB.
  - On `032050` it went to 325 Hz, 14.6 dB of lift below. There no station had ever been
    confirmed, so HM-DEC-127 had nothing to compare against.

**Task 2: the change, kept.**
- **Test first:** `TheTrackerStaysWithTheStationItReadsTests`, naming HM-REQ-010.
  - The case: 500 Hz at 15 dB over the shaped band, exact key, two overs with 7.5 s of band noise
    between them, and a station 15 dB quieter at 300 Hz sending `VVV` throughout.
  - **Red at HEAD:** 500 -> 300 at 15.04 s for a candidate 15.2 dB below, `VVV VVV VVV` printed
    sure, the second `N0CALL` lost. Sure added 6, wrong 7.
- **The method.** In `CwToneTracker.ReadSurvey`, HM-DEC-127's floor is now
  `CwToneSurvey.InterferenceLiftDb` (10 dB) instead of `FilterRejectionDb` (25 dB).
  - **Where the threshold comes from.** It is the survey's own line for "band noise having a good
    moment". The trace's refused moves sit at 21.9 dB (key-down) and 14.6 dB (lift). HM-DEC-127's
    own measurement puts legitimate displacements within 1.5 dB.
  - **The clause it works against, per R75:** HM-DEC-127's "There is nothing in between".
  - **Within §3's limits.** It is none of the excluded routes, it does not gate the sure class on
    acquisition or on the verdict, and the instrument is not called.
- **Kept on every R78 item** (the table is in section 3). Only `031905`'s text changed.
- 2.4 is not ticked, and step 2's count goes to 0. The before and after are in `metrics.md`.
  No floor was re-banked.

**Task 3: the exit round.**
- Build: 0 errors in 16 s.
- Engine carry-forward: 178 of 178 in 375 s.
- App carry-forward: 275 of 278 in 166 s. `TheTestsStayOffTheNetworkTests` is 5 of 5 alone and
  `Unit376TheTopBandTests` 5 of 5 alone.
- Floors: captures 51 of 51 in 129 s, adjudicated 13 of 13 in 32 s, named 10 of 13 in 66 s, the
  same three as at entry.
- Metrics: as in section 3's "after" column.
- **Every type touched, each run alone:**

  | type | result | wall time |
  |---|---|---|
  | `TheTrackerStaysWithTheStationItReadsTests` | 1 of 1 | 2 s |
  | `CwTrackerSwitchTests` | 2 of 2 | 2 s |
  | `TheTrackedPitchIsChosenByKeyingTests` | 2 of 2 | 34 s |
  | `CwSurveyThresholdPinTests` | 3 of 3 | 8 s |
  | `CwToneSurveyTests` | 5 of 5 | 5 s |
  | `TheOperatorIsToldAboutASecondStationTests` | 5 of 5 | 8 s |
  | `TheTrackerSwitchTraceTests` | 10 of 10 | 142 s |
  | `TheStationStillKeyingTraceTests` | 10 of 10 | 155 s |
  | `WhatTheFortySureWrongLettersRestOnTests` | 2 of 2 | 156 s |

  `ThePeakAgainstASecondSignalTests` and `TheQuietestBinNoLongerWinsTests` also reference the
  tracker, but the test project's `<Compile Remove>` leaves them out, so they did not run.
- **`src`, file by file, `6b872543` to exit:**
  - `CwToneTracker.cs`, +13 −1: the floor and its comment.
  - `CwProbabilisticDecoder.cs`, +67 −4: `Rival`, and `RivalMargin` split to report its rival.
  - `CwCharacter.cs`, +9: `RivalReading`.
  - `CwProbabilisticStream.cs`, +1: carries `RivalReading`.

  **None of it keys or transmits.**
- **Ticks:** 2.4 is not ticked, and 2.5 is not ticked. Nothing in steps 3, 4 or 5 is ticked.
- DRIFT: step 2 0 (kept), step 3 0, step 4 1, step 5 1.

**Decisions made for myself, in full.**
1. **The test's assertion was narrowed after its first run under the change.**
   - As first written, it asserted no sure letter wrong or added against the key at all. It was
     red at HEAD (13) and still red under the change (3).
   - The 3 are not the group's cause: `EQ` for `CQ` from cold (acquisition, parked), and `I E`
     printed sure on band noise at 500 Hz during the break.
   - It now asserts the group's cause: no sure `V` from the quieter station, and the second over
     read as `N0CALL K`. The full counts still print beside it.
   - Rebuilt with the change set aside, it is red at HEAD (`.run-unit/unit449-test-head2.txt`).
2. **The group's signal, "mixed 100 Hz or more from the pitch held longest in the file", is
   computed over the whole file.** So it describes the group; the decoder cannot use it as it
   runs. What the change uses is the key-down levels the survey already compares.
3. **I added the `src` diagnostic `RivalReading`** so the trace could print the rival reading.
   It changes no decision.
4. **The floor does not reach `032050`,** where no station was ever confirmed. Extending it to
   an unconfirmed tone would have been a second change.

## 2. What the owner should expect

**Yes, fewer letters are now printed as certain that were not what was sent: 33 on the real
recordings where there were 40, and no new ones anywhere.** All seven are on one W1AW bulletin,
`031905`. At 13 s the tracker used to leave the station at 500 Hz for a much quieter keyed tone
at 300 Hz. For four seconds it printed confident garbage in place of the solar-flux line, then
came back. It now stays on the station, and `10.7  IAIEI TANI   WLUX` reads
`10.7 K NTIMETER FLAX`. Ten more letters are right, and nothing reads worse on any other
recording, real or synthetic. **What reads worse, if anything:** the same line now ends
`125, 125T`, with a trailing `T` outside the scored stretch. Also, a genuinely different station
10 dB or more quieter than the one being read will not take the filter while the tracker
remembers the louder one. At 25 dB this was already so; it is now so from 10 dB (section 4, item
2). **The evidence is thin (V-13).** It is one real recording with an inferred key, and the
synthetic case is from the same generator the instrument was proved on. **What the synthetic
case does not prove (§12.5):** one level gap, one speed, one pair of pitches, with a silent break
where the real station never went silent. **Step 2 stays open.** MET-CER-SURE is 0.0757 on the
real set against HM-REQ-010's one in a hundred, and 0.0809 on the synthetic set. 2.5 is still
held red by the named floors under 443's DECIDED (3).

## 3. What you should see

**The group table: the 40 sure wrong or added at HEAD (real, inferred keys), by what the decoder
computes, with right letters beside wrong ones.** Where an excluded route's test covers letters,
the route is named.

| group (signal) | wrong or added | of them, no excluded route covers | right with the same signal | excluded route |
|---|---|---|---|---|
| a single element (E or T) | 17 | 6 | 64 | none; covers too many right letters, not a lever |
| envelope's whole marks differ in number from the pattern's | 10 | 5 | 49 | none; not a lever |
| **mixed 100 Hz or more from the pitch the decoder held longest in the file** (the tracker left the station for a quieter keyed candidate) | **9** | **5** | **6** | **none: the cause is the tracker's move, which no route touched** |
| alone between two word gaps | 6 | 3 | 41 | none |
| tracker retuned inside the letter or within 1 s | 6 | 2 | 129 | none |
| evidence per hop under 2 | 6 | 2 | 9 | none |
| before the tracker's first keyed verdict | 13 | - | 108 | 448, from cold (acquisition, parked) |
| window's or own marks over 1.25 times the path's unit | 11 | - | 70 | 441, marks' speed |
| rival margin under 1 nat | 7 | - | 16 | 442, rival margin |
| a key-up under half a unit inside the letter | 5 | - | 80 | 444, gap duration |
| speed at the grid's edge | 3 | - | 0 | 446, speed bounds |
| worst mark or gap past the farthest right letter | 0 | - | 0 | 445, mark-shape edge |

Letters can show more than one signal. Against the instrument, 14 of the 40 were mixed more than
25 Hz off the pitch it measured, against 20 of 393 right letters. Synthetic, exact keys: 14, none
more than 25 Hz off, none before the first verdict, 9 covered by no route. The largest signal
there is a retune within 1 s (12 wrong against 108 right), which is not a lever.

**Three of the group's letters as text.** `031905` is W1AW's bulletin, and the key is inferred.
The letters are 14.980 s (`N`), 15.150 s (`T`) and 16.000 s (`E`) of `CENTIMETER`. Before the
change, the decoder was mixing at 300 Hz while the instrument measured the station at 499.9 Hz.

| | text |
|---|---|
| as sent (key) | `PREDICTED 10.7 CENTIMETER FLUX IS 125, 125` |
| read before | `PREDICTED 10.7  IAIEI TANI   WLUX IS 125,` (`N` read `I`, `T` read `E`, `E` read `T`, all sure) |
| read after | `PREDICTED 10.7 K NTIMETER FLAX IS 125, 125T` (all three sure and right; `CE` read `K` and `U` read `A`, still sure and wrong) |

**R78, every item a number** (before is HEAD `e8c5ad58`; after is the change, identical at exit):

| part of R78 | before | after | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 40 of 433 (35 substituted, 5 added), 0.0924 | 33 of 436 (28 substituted, 5 added), 0.0757 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 14 of 173, 0.0809 | does not rise |
| MET-INVENTED, real, inferred | 40 over 473, 0.0846 | 33 over 473, 0.0698 | does not rise |
| MET-INVENTED, synthetic, exact | 14 over 252 | 14 over 252 | does not rise |
| sure-and-right coverage, real, inferred | 393 over 473, 0.8309 | 403 over 473, 0.8520 | does not fall |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252, 0.6310 | does not fall |
| MET-WBE, real, inferred | 47 over 113, 0.4159 | 46 over 113, 0.4071 | does not rise |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings, four metrics | - | 0 worse; `031905` wrong-or-added 9 -> 2, right 20 -> 30, boundaries wrong 2 -> 1 | holds |
| **MET-PITCH-ERR, files more than 25 Hz off** | 9; 256 of 1688 windows | 9; 248 of 1688 windows (`031905` 9 of 29 -> 1 of 29) | does not rise |
| HM-REQ-014 dim precision, real and synthetic | no dim letter in the scored stretches | no dim letter | reported, not judged |
| dim rate (my definition; see section 1) | 0 of 433 real, 0 of 173 synthetic | 0 of 436, 0 of 173 | reported, not judged |
| capture rows; named floors | 51 of 51; 10 of 13 | 51 of 51; 10 of 13, the same three | as at entry |

Per condition, with the key's kind:
- Real, sender not stated (20 recordings), inferred: MET-CER-SURE 40 of 371, 0.1078 -> 33 of 374,
  0.0882.
- Real TX-FARNS, TX-ITU and TX-TIGHT, inferred: 0 before and after.
- Every synthetic condition, exact: unchanged.

**Every recording whose text changed.** Of 126 recording texts, one changed:

| recording | key | before | after |
|---|---|---|---|
| `unadjudicated/cw-2026-08-22-031905` | inferred (`… PREDICTED 10.7 CENTIMETER FLUX IS 125, 125`) | `■ PREDICTED 10.7  IAIEI TANI   WLUX IS 125,` | `■ PREDICTED 10.7 K NTIMETER FLAX IS 125, 125T` |

After the change the trace leaves 33:
- 13 came before the first keyed verdict.
- 10 are covered by no excluded route, and the largest signal among them is single-element
  letters, 4 wrong against 35 right.
- The move group is down to 1, on `032050`.

## 4. What's blocking us

1. **Ruling: the HM-DEC-127 floor is the survey's 10 dB, not the filter's 25 dB.**
   - **Reasoning.** HM-DEC-127 recorded that there was "nothing in between" a legitimate
     displacement (within 1.5 dB) and an image (35 dB below). `031905` has one at 21.9 dB, and
     the tracker followed it into eight wrong sure letters. The survey's own 10 dB line refuses it
     and is kept under R78 on every item.
   - **Rejected:** keeping 25 dB (it leaves the group), and a floor fitted between 1.5 and 21.9 dB
     (fitted to one recording).
   - The code carries it now. The owner may want HM-DEC-127's entry superseded in the decision
     log rather than only in the tracker's comment. It is not in the way of B.
2. **Ruling: a genuinely different station 10 dB or more below the one last read is not followed
   while that level is remembered.**
   - **Reasoning.** `_readingDb` is never cleared, so after a loud station stops, a quieter one
     that answers is refused until something louder confirms. This held at 25 dB before. At 10 dB
     it reaches more cases, and nothing in the corpus shows one.
   - **Rejected:** clearing `_readingDb` when the station stops. That is a second change, and 444's
     rule applies to it.
   - Not in the way of B.
3. **Ruling: what the instruction calls HM-REQ-015's "dim rate".**
   - **Reasoning.** HM-REQ-015 in `CW_REQUIREMENTS.md` is class-at-emission, not a rate. I
     reported dim over named letters emitted under my own definition.
   - **Rejected:** measuring HM-REQ-015 as written, which is a timing property with no metric in
     §11.
   - Not in the way of B.
