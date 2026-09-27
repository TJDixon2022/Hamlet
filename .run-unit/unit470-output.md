READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop, and this unit
   worked step 2 against R86's words, reported as a mismatch.
B. Step 2 (HM-REQ-010, with HM-REQ-102 measured): the gate -
   no sure letter while pitch or speed is not proved - refused
   on MET-COVERAGE, every condition with a sure letter (real, sender
   not stated, 0.8317 -> 0.2268); real MET-CER-SURE 33/436 -> 4/114,
   coverage 403/473 -> 110/473; synthetic MET-CER-SURE 14/173 ->
   12/78 (rises), coverage 159/252 -> 66/252; sure letters inside
   acquiring 0 right / 13 wrong of unit 449's 13 (all 13 inside);
   the 7.052 opening's sure letters while acquiring 20 -> 0 (9 -> 0
   over 448's 0 to 26.04 s); 2.4's count 2 of 3; 2.4 open.
C. The findings weighed against A and B: section 4 raises 6 items.
   None is in the way of 2.4 closing. The step closes partial after
   one more unit with no kept change, and item 3 says why this route
   cannot be that change as the proof states stand. The R86 mismatch
   in one line: R86 bars steps 2 to 8 until 9.4 has a kept change,
   9.4 has no route left, and this unit worked step 2 anyway as the
   launcher named it.

UNIT:       470 - complete at task 3 of 3, TX-* and INT-* columns of task 1 dropped - 2026-09-27 16:10
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md at the conditions CW_SPEC.md names, each proved by a test that names it.
UNIT GOAL:  Stop Hamlet printing any letter as sure while it is still finding the station's pitch and speed, as HM-REQ-102 and the owner's 2026-09-26 reading of "acquiring" require, and keep that only if it lowers wrong-sure letters without costing sure-and-right coverage (R78, step 2).
ADVANCED:   no - none - clears a blocker: step 2 criterion 4, count 2 of 3
NUMBER:     MET-CER-SURE real 33/436 -> 33/436, synthetic 14/173 -> 14/173; coverage real 403/473 -> 403/473; gate refused (under the gate: 4/114, 12/78, 110/473)
DRIFT:      step 2 2; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (carried from 469's report; step 2 up one on this unit's no advance)

## 1. What Claude did

**Complete at task 3 of 3.** Dropped: the TX-* and INT-* columns of task 1's trace, the first
drop candidate. The red test used the INT-CARRIER case directly instead. Provenance: Claude Code
on QUIVERFULL, project Hamlet, gate passed (the four MUST EXIST present, `CoreHMI.sln` and
`MURC.sln` absent), branch `main`, HEAD at entry `1f6f20e8`, version 1.13.156 to 1.13.157.

**The tree against section 3.** Everything matched:
- both `PHASE_PLAN.md` copies are identical: 2.1, 2.2, 2.3 and 2.5 ticked, 2.4 open; 7.1, 7.2,
  7.3 and 7.5 ticked; 9.4 step 9's only open line; R86 at line 342;
- `PARKED.md` line 21 carries the RESOLVED line: *"RESOLVED: 2026-09-26 | define acquiring for
  HM-REQ-102 | acquiring runs from the first keyed element at a tracked pitch until both the pitch
  (HM-REQ-093) and the speed (HM-REQ-034) carry proof state proved; while either is hypothesis or
  none, no character is emitted sure. Consistent with HM-REQ-100, which requires proof reachable
  within a five-character run-up. Author's reading of the spec under R85, overrulable."*;
- `CwPitchProof` and `CwSpeedProof` both exist and are reported per decode (`CwDecodeReport`);
- the port diff against `19109b51` is empty.

There was one difference. `.run-unit\watched.rc` did not exist. `watched.cpu` was modified rather
than untracked, and it was committed as it stood at task 0. It has changed again since, and I left
it unstaged as the runner's. `SESSION.lock` and `.run-unit\fldigi\` were not staged, and nothing
was fetched.

**Task 0.** I wrote the record in both `PHASE_OUTCOME.md` copies, set both `PHASE_STATUS.md` copies
to 470 and step 2, bumped the version, and committed the runner's writes as they were. The entry
round matched 469's exit on every figure, and the app line was clean on its first run. Every
recording's text with each character's class is in `.run-unit/unit470-texts-entry.txt`.

**Task 1.** I read the code before any decode (`.run-unit/unit470-trace.txt` Part 1):
- **The class** is assigned in `CwProbabilisticStream.Character` (`CwProbabilisticStream.cs:750`):
  sure where every inner gap fits the unit, otherwise dim or unreadable.
- **Both proof states are live properties, known at the moment of emission.** They are not
  computed after the fact:
  - `CwToneTracker.PitchProof` (`CwToneTracker.cs:478`);
  - `CwDecoder.SpeedProof` (`CwDecoder.cs:696`);
  - the stream sets `Last` and `UnitWasMeasured` before it raises `CharacterSettled`;
  - the tracker has already processed the same hop.

  So `CwDecoder`'s two handlers are where both states are first known together. The stream holds
  no tracker.
- **The arbiter.** Every metric harness builds `CwDecoder` without the second reader, so the
  transcript measured is our class. With the second reader on (the application), an agreeing port
  reading with the higher p, or a port that alone votes, can still emit sure on a span our gate
  dimmed.
- **Departures from the RESOLVED lines** are in section 4, item 4.

`WhatTheSureLettersWerePrintedUnderFact` asserts nothing. It reads both states in the settle
handler through the same harness and the same `CwMetrics` calls. Its totals equal
`TheRequirementsAreMeasuredTests`: 33 of 436 and 14 of 173. Its table is section 3, item 1. I
registered DECIDED (3) in the trace file's Part 3 before any after-figure existed, and committed
it (`bcbf97ba`).

**Task 2.** I wrote `NoSureLetterWhileAcquiringTests`, naming HM-REQ-102 and HM-REQ-010, on two
generated cases whose key is exact by construction. Both are 18 WPM, 15 dB on CH-AWGN, cold from
600 Hz:
- INT-CARRIER(+100 Hz, 0 dB), which reads `CT  EIQ CQ DE N0CALL N0CALL K`;
- the wanted TX-ITU alone, which reads the key exactly.

It was red at HEAD on both, with 67 and 42 characters emitted sure while acquiring
(`.run-unit/unit470-red.txt`). The first offender was `C` at 1.745 s, pitch none, speed hypothesis.
I built the gate in `CwDecoder.WhileAcquiring`, applied to our settled character before the
arbiter and to our leading edge. A sure letter is emitted dim while either state is not proved.
There is no constant, and no proof-state logic changed. The test went green. The R78 table is
section 3. **Refused on MET-COVERAGE.** Several more parts of R78 also fail: synthetic
MET-CER-SURE rises, V-11 is 29 of 35 recordings worse on coverage, the engine line is red
(`CwFixtureTests.TheCleanRecordingsDecodeExactly`), and `TheArbitrationEarnsItsPlaceTests` part
(b) is red. The change and its test are `.run-unit/unit470-gate.patch`, and the test file's copy is
`.run-unit/unit470-NoSureLetterWhileAcquiringTests.cs.txt`. `src` and `tests` are restored to HEAD,
and the refusal is under `## Unit 470` in `metrics.md` (`e35fa35c`). No second form was tried.

**Task 3.** The exit round is in section 3, item 6. Every figure is as at entry, and both texts
are byte-identical to entry.

**Decisions I made for myself, in full:**
- **The gate covers our leading edge as well as the settled text.** A letter shown sure on the
  edge and then settled dim would be the guess HM-REQ-102 forbids. No metric reads the edge, so
  this changes no figure.
- **The red test's second case is the wanted alone rather than a TX-* sender case.** It is the
  "cold TX-ITU start" the instruction names, rendered by unit 469's `CwInterference.Render` with
  no interferer, so both cases share one generator and one seed.
- **"Inside acquiring" in the trace is read at emission**, the settle moment, as DECIDED (3)
  says, not at the letter's audio time `At`. The two differ by the decision delay.
- **The 7.052 opening's right and wrong labels use unit 448's reference**: each of `003901` and
  `003919` read alone. That reference is the decoder's own reading, so "right" there is weak.
  HM-REQ-102 does not depend on it.
- **I left the new test out of both carry-forward lines**, per section 8. The refused test is not
  committed under `tests`.

## 2. What the owner should expect

Nothing on the screen changes. The rule would show letters dim, as guesses, while Hamlet is still
proving the station's pitch and speed. On the 23 real recordings it would have dimmed 293 letters
Hamlet reads right to catch 29 it reads wrong, and it would leave 7 of those recordings with no
sure letter at all. On a clean test signal it dims letters in the middle of words, because the
pitch counts as proved only just after each survey, and the survey runs every half second. The
test CQ at 18 WPM would read `(C)Q (C)Q CQ D(E) (N)(0)CALL (N)(0)(C)(A)(L)L K` instead of
`CQ CQ CQ DE N0CALL N0CALL K`, with the letters in brackets shown dim. The W1AW bulletin
`PREDICTED 10.7 K NTIMETER FLAX IS 125, 125T` would go almost wholly dim. So the rule is kept on
file and not in Hamlet. Step 2 now has one more attempt before it closes as partial.

**What will look wrong but is not:** the app line lost a different test to the headless
dispatcher fault at tasks 1 and 2. Each rerun, or the lost test run alone, passed.

## 3. What you should see

**The R78 table** (before is HEAD `bcbf97ba`, under is the gate; key's kind beside each):

| part of R78 | before | under the gate | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 33 of 436, 0.0757 | 4 of 114, 0.0351 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 12 of 78, 0.1538 | **rises** |
| MET-INVENTED, real, inferred | 33 over 473, 0.0698 | 4 over 473, 0.0085 | falls |
| MET-INVENTED, synthetic, exact | 14 over 252 | 12 over 252 | falls |
| sure-and-right coverage, real, inferred | 403 over 473, 0.8520 | 110 over 473, 0.2326 | **falls - refuses** |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 66 over 252, 0.2619 | **falls - refuses** |
| MET-WBE, real / synthetic | 37 over 113 / 44 over 84 | the same | holds |
| dim precision (HM-REQ-014) | no dim letter in either set | real 293 right of 322 dim, 0.9099; synthetic 93 of 95, 0.9789 | reported |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | 29 worse on sure-and-right, none worse on wrong, added or boundaries | **fails** |
| floors: named, captures | 13 of 13, 51 of 51 | 13 of 13, 51 of 51 | hold |
| engine line | 178 of 178 | 176 of 178, `TheCleanRecordingsDecodeExactly` red on clean-12wpm and clean-18wpm | **fails** |
| app line | 278 of 278 | 278 of 278 | holds |
| `TheArbitrationEarnsItsPlaceTests` | (a) 5/5, (b) harness 1/1, live 2/2 | (a) 5/5; (b) harness red, live red | **fails** |

Part (b) goes red because the rule's choice moves off the switch table on 4 harness rows and on
the live row (Arbitrate becomes OursAlone or PortAlone), and character gap 5 at 15 dB becomes worse
than the port on 010, 013 and 012.

Per condition, MET-CER-SURE and coverage before -> under:

| condition | key | MET-CER-SURE | MET-COVERAGE |
|---|---|---|---|
| real, sender not stated (20) | inferred | 33/374 -> 4/97 | 0.8317 -> 0.2268 |
| real TX-FARNS | inferred | 0/43 -> 0/11 | 0.9773 -> 0.2500 |
| real TX-ITU | inferred | 0/13 -> 0/5 | 1.0000 -> 0.3846 |
| real TX-TIGHT | inferred | 0/6 -> 0/1 | 1.0000 -> 0.1667 |
| synth TX-ITU 15 dB | exact | 1/64 -> 1/30 | 1.0000 -> 0.4603 |
| synth TX-ITU 5 dB | exact | 1/63 -> 1/23 | 0.9841 -> 0.3492 |
| synth char gap 5, 15 dB | exact | 1/21 -> 1/9 | 0.9524 -> 0.3810 |
| synth char gap 5, 5 dB | exact | 11/25 -> 9/16 | 0.6667 -> 0.3333 |
| synth, both 0 dB | exact | none sure | 0 -> 0 |

MET-WBE is unchanged on every condition.

1. **Task 1's acquiring table at HEAD** (sure letters in the scored stretches, by the proof states
   at emission):

   | set | key | inside: right | inside: wrong or added | outside: right | outside: wrong or added |
   |---|---|---|---|---|---|
   | real, sender not stated | inferred | 248 | 29 | 93 | 4 |
   | real TX-FARNS / TX-ITU / TX-TIGHT | inferred | 32 / 8 / 5 | 0 / 0 / 0 | 11 / 5 / 1 | 0 / 0 / 0 |
   | **real, all** | inferred | **293** | **29** | 110 | 4 |
   | synth TX-ITU 15 / 5 dB | exact | 34 / 40 | 0 / 0 | 29 / 22 | 1 / 1 |
   | synth char gap 5, 15 / 5 dB | exact | 12 / 7 | 0 / 2 | 8 / 7 | 1 / 9 |
   | **synthetic, all** | exact | **93** | **2** | 66 | 12 |

   Which state was not proved, real: pitch alone 58 right / 4 wrong; speed alone 10 / 3; neither
   225 / 22. **Unit 449's 13: all 13 found, all sure and inside acquiring**, every one with pitch
   none and speed hypothesis and before the first keyed verdict (`032012` 3, `032050` 4, `032113`
   1, `032129` 5). Unit 448's acquiring, before the first keyed verdict, holds 121 real sure
   letters (108 right, 13 wrong), and 119 of them are inside the resolution's.
2. **The 7.052 opening, 0 to 46.2 s** (sure as itself, dim in parentheses, braces round what was
   emitted while acquiring; first keyed verdict 26.04 s):
   - before: `{EII ETNHHK        EANQNI}D          {EAN(■)IK     }`. That is 20 sure while acquiring,
     all right against 448's reference, and 9 of them before 26.04 s, as unit 448 measured.
   - under the gate: `{(E)(I)(I) (E)(T)(N)(H)(H)(K)        (E)(A)(N)(Q)(N)(I)}D          {(E)(A)(N)(■)(I)(K)     }`.
     That is 0 sure while acquiring. HM-REQ-102 is met on the opening, and HM-REQ-103 falls from
     21 to 1 of 24.
3. **Recordings whose classes changed under the gate.** No letter changed on any recording
   (`.run-unit/unit470-letters.sh`: letters identical, classes aside). Over all 35 whole texts,
   685 sure became dim and 330 stayed sure. 31 recordings changed:
   - the 8 synthetic cases at 5 and 15 dB;
   - all 23 real recordings.

   Whole texts, before and under, are in `.run-unit/unit470-texts-entry.txt` and
   `unit470-text-gate.txt`. Samples:
   - `cq-18wpm-15db`: `CQ CQ CQ DE N0CALL N0CALL K` -> `(C)Q (C)Q CQ D(E) (N)(0)CALL (N)(0)(C)(A)(L)L K`
   - `cq-12wpm-15db`: -> `(C)(Q) (C)(Q) (C)(Q) (D)(E) (N)(0)(C)(A)L(L) (N)(0)(C)(A)L(L) (K)`
   - `031905`: `PREDICTED 10.7 K NTIMETER FLAX IS 125, 125T` -> all dim but one `E`
   - `004507`: `... ARRL D O T N E T <BT>  E ACH STATION HANDLING THIS MESSAGE PE` -> `... (<BT>)  (E) (A)(C)(H) ST(A)(T)(I)ON (H)(A)(N)(D)(L)(I)(N)G THI(S) (M)(E)(S)(S)(A)GE PE`
   - `004322`: `P O N S ORED AMERICA 25 9 OPERATION X ALL LOGS WILA` -> `(P) (O) N (S) ORED AMERICA 2(5) (9) (O)PERATION X ALL (L)OGS WILA`
   - `032129`: the whole ARRL bulletin opening dim.
4. **First sure character, before -> under the gate, in seconds** ("-" means none):

   | recording | before | under | recording | before | under |
   |---|---|---|---|---|---|
   | 7.052 opening | 20.64 | 33.48 | `004108` | 0.90 | 5.59 |
   | `173723` | 0.88 | 8.18 | `004133` | 0.92 | 3.35 |
   | `013347` | 0.32 | 20.69 | `004205` | 1.87 | 9.01 |
   | `134712` | 14.57 | - | `004234` | 0.12 | 3.57 |
   | `003758` | 2.92 | 13.06 | `004322` | 1.73 | 3.23 |
   | `012403` | 14.41 | 25.53 | `004347` | 0.84 | 0.84 |
   | `004507` | 0.05 | 14.70 | `004405` | 1.74 | 3.87 |
   | `031838` | 1.86 | - | `004427` | 2.48 | 2.48 |
   | `031905` | 3.72 | 16.69 | `004510` | 2.10 | 3.94 |
   | `031948` | 2.18 | - | `004550` | 0.23 | 3.68 |
   | `032012` | 1.77 | - | cq 12 WPM 15 / 5 dB | 2.11 / 2.11 | 19.11 / 19.11 |
   | `032050` | 2.30 | - | cq 18 WPM 15 / 5 dB | 1.75 / 2.81 | 2.81 / 5.07 |
   | `032113` | 1.69 | - | cq 25 WPM 15 / 5 dB | 1.54 / 1.54 | 2.31 / 2.31 |
   | `032129` | 3.20 | - | cq 18 WPM char 5, 15 / 5 dB | 1.75 / 1.75 | 5.35 / 5.34 |

   The three 0 dB passband cases and char 5 at 0 dB have no sure character before or after.
   This stands in for MET-TACQ (5.4). It is reported, and it is not a keep gate.
5. **The commit table**, with the five results at each commit:

   | commit | what | build | engine line | app line | named | captures | adjudicated |
   |---|---|---|---|---|---|---|---|
   | `ab98456b` | task 0: record, 1.13.157, runner's writes, entry | 0 errors | 178/178 | 278/278 first run | 13/13 | 51/51 | 13/13 |
   | `bcbf97ba` | task 1: the fact and the trace, rule registered | 0 errors | 178/178 | 277/278 twice (dispatcher loop, a different type each run); `Unit376TheTopBandTests` alone 5/5 (DECIDED (7)) | 13/13 | 51/51 | 13/13 |
   | `e35fa35c` | task 2: refused; metrics.md, patch, printouts | 0 errors | 178/178 | 278/278 on the one rerun (first 276, dispatcher loop) | 13/13 | 51/51 | 13/13 |
   | task 3 commit | exit round, output.md | 0 errors | 178/178 | 278/278 first run | 13/13 | 51/51 | 13/13 |
6. **Every figure, entry beside exit:**

   | check | entry | exit |
   |---|---|---|
   | build | 0 errors | 0 errors |
   | engine line | 178 of 178 | 178 of 178 |
   | app line | 278 of 278 | 278 of 278 |
   | floors: named, captures, adjudicated | 13, 51, 13 | 13, 51, 13 |
   | real set, inferred | MET-CER-SURE 33/436, coverage 403/473, MET-WBE 37/113 | the same |
   | synthetic set, exact | 14/173, 159/252, 44/84 | the same |
   | `TheArbitrationEarnsItsPlaceTests` | (a) 5/5, (b) harness 1/1, live 2/2 | the same |
   | `TheChannelProfilesAreWhatTheySayTests` | 71 of 71 | 71 of 71 |
   | `TheSenderProfilesAreWhatTheySayTests` | 16 of 16 | 16 of 16 |
   | `TheInterferenceProfilesAreWhatTheySayTests` | 21 of 21 | 21 of 21 |
   | `TheSpeedFollowsTheSendersMarkPairsTests` | red at 28 of 31 (469) | red at 28 of 31, not required |
   | every recording's text and classes | `unit470-texts-entry.txt` | byte-identical (ours and port) |

   The exit prints are all empty (`.run-unit/unit470-exit-print.sh`):
   - `git diff 1f6f20e8 -- src`;
   - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`;
   - `git diff 7e209cb4` over the eleven transmit files, all present.

   `git status` shows `.run-unit/fldigi/` still untracked.

## 4. What's blocking us

Nothing blocks 2.4. Each item is a plain reading, not a ruling request.

1. **The R86 mismatch.** R86 bars steps 2 to 8 until 9.4 has a kept change. 9.4 has no route
   left, since (A) to (E) and (W) were all refused. This unit worked step 2 as the launcher named
   it, as units 468 and 469 worked outside step 9. If the owner holds R86, this unit is a
   measurement only, and nothing the operator reads has changed.
2. **DECIDED (2) and (3) are the arbiter's, and overrulable.**
   - (2): 449's exclusion of an acquisition gate rested on its being parked. The RESOLVED line of
     `9a329cdb` answered that.
   - (3): the gate reads the tree's two proof states at emission, and dims a sure letter while
     either is not proved.

   This unit read "at emission" as the settle moment, and applied the gate before the arbiter and
   to our leading edge too.
3. **Refused: HM-REQ-102 and HM-REQ-012 pull against each other on this corpus.**
   - The resolution's acquiring holds 293 of the 403 sure-and-right real letters for 29 of the 33
     wrong or added. The synthetic figures are 93 of 159 for 2 of 14.
   - On 7 of the 23 real recordings no character is ever emitted with both states proved.
   - On a clean generated TX-ITU the pitch is proved only just after a confirming survey, so
     "acquiring" recurs inside words (`D(E)`, `ST(A)(T)(I)ON`).

   As the proof states stand, no form of this gate can pass 2.2's coverage line. That is a finding
   about the requirements and the proof states, not a request. The narrower forms the trace
   suggests are not tried (V-14):
   - speed only: 10 right for 3 wrong;
   - the 448 span only: 108 right for 13 wrong;
   - a grace period.
4. **The proof states against their RESOLVED lines** (reported, not repaired):
   - **Pitch.** The tree says proved while the latest survey's keyed verdict is at the reported
     pitch. It does not require that a whole character has resolved at it, as the RESOLVED line
     says, so on that clause it proves sooner than the line. It also drops to hypothesis between
     confirming surveys while the station is still keying. The RESOLVED line says hypothesis only
     where "keying is not admitted or no character has resolved". Whether "admits keying" means the
     latest survey or the station's keying still being admitted is where the tree and this unit's
     result part. That reading, not the gate, is what makes acquiring recur mid-word.
   - **Speed.** The tree has "the tone stops" (six surveys) and "the pitch changes" (a follow of
     half the filter). It has no test of the marks' unit within 10 percent of the path's
     (HM-REQ-031). "Measured" is the estimator's dit or the marks' overrule, not the grid's winner.
5. **With the second reader on, the gate is only on our reading.** An agreeing port reading with
   the higher p, or a port that alone votes, can still reach the screen sure while acquiring
   (`CwArbiter.Decide`). The switch and vote tables are parked, so this is recorded only.
6. **2.4's count after this unit is 2 of 3**:
   - 449 kept a change;
   - 460 kept none;
   - 470 kept none.

   The next step-2 unit with no kept change closes step 2 partial, and its trace goes to
   `PARKED.md`.
