READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 5 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6,
   6 3 of 6, 7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop, and
   this unit worked step 2 against R86's words, reported as a mismatch.
B. Step 2 (HM-REQ-010, with HM-REQ-102 measured): the acquiring
   stretch re-read at the proved pitch and speed before it settles -
   refused on MET-CER-SURE (real, sender not stated: 33/374 -> 33/374,
   does not fall; synthetic character gap 5 at 5 dB: 11/25 -> 12/26,
   rises); letters unsettled at proof 33 real / 39 synthetic, wrong to
   right 0 / 0, right to wrong 1 / 1; real MET-CER-SURE 33/436 ->
   33/436, coverage 403/473 -> 403/473; synthetic MET-CER-SURE 14/173
   -> 14/173, coverage 159/252 -> 159/252; 2.4 ticked, the trace in
   PARKED.md, step 2 closed partial with MET-CER-SURE at 33/436 real
   (inferred) and 14/173 synthetic (exact) against one in a hundred.
C. The findings weighed against A and B: section 4 raises 7 items. None
   is in the way of 2.4, which is ticked. Item 4 says that a closed step
   2 leaves HM-REQ-010 unmet, as 2.4's own words foresee. The R86
   mismatch in one line: R86 bars steps 2 to 8 until 9.4 has a kept
   change, 9.4 has no route left, and this unit worked step 2 anyway as
   the launcher named it.

UNIT:       471 - complete at task 4 of 4, decode-time and live-against-settled lines dropped - 2026-09-27 18:12
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md at the conditions CW_SPEC.md names, each proved by a test that names it.
UNIT GOAL:  Read again, at the pitch and speed once both are proved, the letters Hamlet guessed while it was still finding a station, before they settle, and keep that only if confidently wrong letters fall with nothing else worse (R78); otherwise close step 2 partial as 2.4 is written.
ADVANCED:   yes - step 2 criterion 4, ticked: the re-read was refused, the third consecutive step-2 unit with no kept change, so the trace went to PARKED.md and 2.4 is ticked in both plans
NUMBER:     MET-CER-SURE real 33/436 -> 33/436, synthetic 14/173 -> 14/173; coverage real 403 -> 403; re-read refused
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (carried from 470's report; step 2 reset from 2 on this unit's advance)

## 1. What Claude did

**Complete at task 4 of 4.** I dropped two lines of task 2's table: the decode time, and the live
against settled word boundaries. They are the instruction's first two drop candidates, in its
order. Nothing that is never shed was dropped. Provenance: Claude Code on QUIVERFULL, project
Hamlet, gate passed (the four MUST EXIST present, `CoreHMI.sln` and `MURC.sln` absent), branch
`main`, HEAD at entry `a2e07f6d`, version 1.13.157 to 1.13.158.

**The tree against section 3.** Everything matched except three things:
- both `PHASE_PLAN.md` copies were identical at entry: 2.1, 2.2, 2.3 and 2.5 ticked, 2.4 open,
  9.4 step 9's only open line, R86 at line 342;
- the port diff against `19109b51` is empty, and `.run-unit/unit470-gate.patch` exists and was not
  applied;
- **mismatch:** `.run-unit\watched.rc` did not exist. `watched.cpu` was the runner's deleted file
  at launch and was rewritten during the session. It was committed as it stood at each commit;
- **mismatch:** `PARKED.md`'s header reads *"Written by run-phase.bat, never by a session, and
  append-only"*. The words are broken across two lines in the file ("never by" / "a session").
  This is why DECIDED (6) reports a mismatch (section 4, item 3);
- **mismatch:** the watched-red output `.run-unit\unit471-red.txt` does not exist, because no red
  case exists (task 2 below).

`SESSION.lock` and `.run-unit\fldigi\` were not staged, and nothing was fetched.

**Task 0.** I wrote the record in both `PHASE_OUTCOME.md` copies, set both `PHASE_STATUS.md` copies
to 471 and step 2, bumped the version, and committed the runner's writes as they were. Every entry
figure matched 470's exit. The app line lost 3 tests to the dispatcher loop, then passed 278 of 278
on the one rerun. The texts with classes are byte-identical to `unit470-texts-entry.txt`
(`78f69b94`).

**Task 1.** I read the code before any decode (`.run-unit/unit471-trace.txt` Part 1):
- **The scored path is the stream.** The harness builds `CwDecoder` without the second reader, so
  `Arbitrated` passes our settled character through unchanged. The CW tab uses the same
  `CwProbabilisticStream`, with `CwArbiter` after it. A change in the stream or in `CwDecoder.Step`
  acts the same on both.
- **The window.** 12 s (2400 hops of 5 ms), mixed down hop by hop at the pitch in force. Every
  0.5 s the whole window is decoded again. A letter settles once it ends more than
  `DecisionDelaySeconds` = 1.0 s behind the newest hop. That is 10 dits at 12 WPM, 15 at 18 and
  20.8 at 25, so a letter settles 1.0 to 1.5 s after its audio. Nothing behind `_settledThrough`
  is announced again. The stream keeps no raw audio.
- **The proof states** are live properties, read after each read. `SpeedProof` reads the stream's
  last read, so the read at which the speed is proved has already decoded its whole window, the
  unsettled letters included, at the proved speed. **Only the pitch the window was mixed at can
  differ.** A letter that settles while acquiring (470's sense) settled at a read without both
  proved. It can have been unsettled at a proof only if both flickered on at an earlier read.
- **Re-reading** needs the raw audio. `CwProbabilisticDecoder.Decode(envelope, pitch, wpm, gaps)`
  and the integrator's own window and taper are public.

`WhatTheAcquiringLettersReadAtTheProvedValuesFact` (HM-REQ-010, HM-REQ-102) asserts nothing. It
reads both states after every read. For each letter emitted while acquiring, it finds the first
read after the letter's audio with both proved. It then re-reads that read's window from the raw
audio at the proved pitch and speed, with the read's gaps and the stream's own class, both read
by reflection so nothing under `src` changed. Its totals equal `TheRequirementsAreMeasuredTests`:
33 of 436 and 14 of 173. The tables are section 3, item 1. I registered DECIDED (3) and (4) in
Part 3 of the trace before any after-figure existed (`d0a1fc36`).

**Task 2.** The first table was not empty, so task 2 ran.

**The test first.** I searched 33 generated exact-key cases for a red case. They were cold TX-ITU
at 12, 18 and 25 WPM with four seeds each, INT-CARRIER(±100, 0) and INT-ADJ(±100, 0, 25) at each
speed, and the HM-REQ-062 carriers at 18 WPM. 126 letters were unsettled at proof, and **not one
wrong letter read right: 0.** So there is no red case, and the change was judged on R78 alone, as
the instruction directs. `TheAcquiringStretchIsReadAtTheProvedValuesTests` holds the search and a
test that no settled letter moves. That test passes at HEAD, so it was not watched red.

**The build.** I built the re-read exactly as registered:
- `CwProbabilisticStream` keeps the window's raw audio in a ring;
- `CwDecoder.Step` calls `ReadAgainAtProvedValues(Tracker.ToneHz, Reading.WordsPerMinute)` at
  each read where both states are proved and were not both proved at the read before;
- the stream re-mixes the window at that pitch and decodes it at that speed with the read's gaps;
- it pins every letter ending after the settled mark and by the previous read's newest hop, and
  those settle in place of the ordinary read's;
- nothing settled is touched, and no constant is added.

The test went green under it.

**The verdict.** **Refused on MET-CER-SURE.** It does not fall on the real set, 33 of 436 on
every real condition. On synthetic character gap 5 at 5 dB it rises, 11 of 25 to 12 of 26, and
MET-INVENTED rises there too, 11 to 12 over 21. The floors also fail: captures 44 of 51, named
11 of 13, engine line 176 of 178. The change and its test are `.run-unit/unit471-reread.patch`,
and the test file's copy is `.run-unit/unit471-TheAcquiringStretchIsReadAtTheProvedValuesTests.cs.txt`.
`src` and `tests` are restored to HEAD, and the refusal is under `## Unit 471` in `metrics.md`
(`0fa6f02c`). No second form was tried.

**Task 3.** I wrote `## Step 2 - closed partial at unit 471` in `metrics.md`, appended one line to
`PARKED.md` and ticked 2.4 in both plans, changing nothing else. The copies are still identical
(`d5bf90e1`).

**Task 4.** Every exit figure equals its entry figure, and the texts are byte-identical (section
3, item 6).

**Decisions I made for myself, in full:**
- **"First proved" is read per letter:** the first read after the letter's audio at which both
  states are proved. That covers each return to proved after a flicker, not only the first proof
  in a recording. Only 2 of the 33 real reachable letters and 3 of the 39 synthetic ones sit at a
  recording's very first proof. The build acts at every return to proved, so it matches the trace.
- **An unsettled letter was "emitted while acquiring"** if its audio arrived by the last read
  without both proved. It was offered on the leading edge then, and the stream cannot know at
  proof time how a letter will later settle.
- **The leading edge is left as the ordinary read offers it.** The rule is about what settles, and
  the edge is provisional on both paths.
- **The PARKED.md line's unit field is `unit 10`**, the runner's iteration in
  `.run-unit/launch.stamp`, in the file's own numbering. The launch time is `not recorded`,
  because `PHASE_OUTCOME.md` carries no launch line for this unit.
- **The "before" side of dim precision and the 7.052 opening** was measured with 470's printer at
  HEAD after `src` was restored. `src` is the same as at 470's HEAD.
- **Commit `0fa6f02c` was not given its own five-test round.** Its `src` and `tests` are
  byte-identical to `d0a1fc36`'s, which exited green, and it was rebuilt with 0 errors. The exit
  round ran on `d5bf90e1`'s tree.

## 2. What the owner should expect

Nothing on the screen changes. The work on confidently wrong letters is closed for this phase.
This unit tried reading a station's opening letters again once Hamlet has proved the pitch and
speed. Every letter that can still be re-read at that moment was already right, and the wrong
ones settle too early to reach, or come from recordings where Hamlet never proves both. So it
fixed nothing, and it made one synthetic case slightly worse. 33 of the 436 letters Hamlet
prints as certain on the real recordings are still wrong, 7.6 in a hundred against the
requirement's one. On the synthetic signals it is 14 of 173, 8.1 in a hundred. Where those 33
come from, and every route tried against them, is written down in `metrics.md` and `PARKED.md`.

**What will look wrong but is not:**
- Step 2 now shows every box ticked while HM-REQ-010 is still unmet. That is 2.4's "closes
  partial" working as written (section 4, item 4).
- The app line lost tests to the headless dispatcher fault at tasks 0 and 1, and under the
  refused change. Each passed on its one rerun.

## 3. What you should see

**The R78 table** (before is the entry round, `src` as at `a2e07f6d` and unchanged to `d0a1fc36`;
under is the re-read; key's kind beside each):

| part of R78 | before | under the re-read | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 33 of 436, 0.0757 | 33 of 436, 0.0757 | **does not fall - refuses** |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 14 of 173; TX-ITU 5 dB falls, character gap 5 at 5 dB **rises** | **refuses** |
| MET-INVENTED, real, inferred | 33 over 473, 0.0698 | 33 over 473 | holds |
| MET-INVENTED, synthetic, exact | 14 over 252 | 14 over 252; character gap 5 at 5 dB 11 -> 12 over 21 | **rises on a condition** |
| sure-and-right coverage, real, inferred | 403 over 473, 0.8520 | 403 over 473 | holds |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252 | holds |
| MET-WBE, real / synthetic | 37 over 113 / 44 over 84 | the same | holds |
| dim precision (HM-REQ-014) | no dim letter in either set | no dim letter | no number |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings, four metrics | - | 2 rows move: `cq-18wpm-5db-char5` one more sure added, `cq-25wpm-5db` one fewer | reported |
| capture rows (V-11's guard) | 51 of 51 | 44 of 51: `004405`, `001831`, `013303`, `013150`, `003758`, `003126`, `013347` | **fails** |
| named floors | 13 of 13 | 11 of 13: `003758` (43), `013347` (57) | **fails** |
| engine line | 178 of 178 | 176 of 178 (`013303`, `013150` capture rows) | **fails** |
| app line | 278 of 278 | 277, then 278 of 278 on the one rerun | holds |
| `TheArbitrationEarnsItsPlaceTests` | (a) 5/5, (b) harness 1/1, live 2/2 | the same | holds |
| HM-REQ-102, 7.052 opening, sure while acquiring | 20 (9 before the 26.04 s verdict) | 20 (9) | unchanged |

Per condition, MET-CER-SURE and coverage, before -> under:

| condition | key | MET-CER-SURE | MET-COVERAGE |
|---|---|---|---|
| real, sender not stated (20) | inferred | 33/374 -> 33/374 | 0.8317 -> 0.8317 |
| real TX-FARNS / TX-ITU / TX-TIGHT | inferred | 0/43, 0/13, 0/6 -> the same | 0.9773, 1.0000, 1.0000 -> the same |
| synth TX-ITU 15 dB | exact | 1/64 -> 1/64 | 1.0000 -> 1.0000 |
| synth TX-ITU 5 dB | exact | 1/63 -> 0/62 | 0.9841 -> 0.9841 |
| synth char gap 5, 15 dB | exact | 1/21 -> 1/21 | 0.9524 -> 0.9524 |
| synth char gap 5, 5 dB | exact | 11/25 -> **12/26** | 0.6667 -> 0.6667 |
| synth, both 0 dB | exact | none sure | 0 -> 0 |

1. **Task 1's reading of the code:** the scored path is the stream through `CwDecoder` with no
   arbiter, and the CW tab adds the arbiter after it. The window is 12 s, read every 0.5 s. A
   letter settles 1.0 s behind the newest audio (10 to 21 dits at 12 to 25 WPM). Proof arrives at
   reads, 3.0 to 27.0 s into a recording, and flickers 1 to 8 times. 7 of the 23 real recordings
   and the 0 dB synthetic cases never reach both proved.
2. **Task 1's two tables** (letters emitted while acquiring, scored stretches):

   | set | key | unsettled at the first proof after them | wrong->right | right->wrong | right->right | wrong->wrong | sure->dim | dim->sure | added | lost |
   |---|---|---|---|---|---|---|---|---|---|---|
   | real, all | inferred | 33 (sender not stated 26, TX-FARNS 6, TX-ITU 1) | 0 | 1 | 32 | 0 | 0 | 0 | 0 | 1 |
   | synthetic, all | exact | 39 (TX-ITU 15 dB 12, 5 dB 17; char gap 5 15 dB 5, 5 dB 5) | 0 | 1 | 36 | 2 | 0 | 0 | 0 | 0 |

   | set | key | settled before proof | sure right | sure wrong or added | never proved after them |
   |---|---|---|---|---|---|
   | real, all | inferred | 289 | 260 | 29 | 214 |
   | synthetic, all | exact | 56 | 56 | 0 | 3 |

   The real right-to-wrong is `012403`'s `0`, lost. The synthetic one is `cq-18wpm-5db`'s `C`, read
   `6`, which HEAD's own edge also read at that moment. The two wrong-to-wrong are
   `cq-18wpm-5db-char5`'s added `T` and `K`. **Unit 449's 13: all in the second table**, all in
   `032012`, `032050`, `032113` and `032129`, which never reach both proved. **Unit 470's 29: all
   in the second table.** 24 of them are never proved after them. 5 settled before a proof:
   `003758`'s `E E T` (proof at 14.00 s; a retracting re-read would give `4`), `031905`'s `K` and
   `004347`'s added `O`.
3. **The 7.052 opening, 0 to 46.2 s** (sure as itself, dim in parentheses, braces round what was
   emitted while acquiring), before and under the re-read, identical:
   `{EII ETNHHK        EANQNI}D          {EAN(■)IK     }`. That is 20 sure letters while
   acquiring, all right against 448's reference, and 21 of 24 sure and right.
4. **Recordings whose text or classes changed under the re-read: 23 of 35.** Most moved lines
   keep their letter and class and only carry the re-read's p. The text changes are:
   - letters added: `004133` `...T UT...` -> `...DT UT...`; `004234` `N AIR` -> `EN AIR`;
     `004550` `A3HB` -> `AA3HB`; `cq-18wpm-5db-char5` gains a sure `T`;
   - letters dropped: `013347` an `R`; `003758` a `K`; `004405` an `8`; `cq-25wpm-5db` the added `K`;
   - word spaces moved: `003758`, `004550`.

   Before and after, whole: `.run-unit/unit471-texts-entry.txt` and `unit471-text-change.txt`,
   with every moved line in `unit471-textdiff-entry-change.txt`.
5. **Task 3.** The closing table (`metrics.md`, `## Step 2 - closed partial at unit 471`):

   | figure | real, inferred | synthetic, exact |
   |---|---|---|
   | unit 439's baseline | 67 of 426 (54 substituted, 13 added); coverage 359/473 | 24 of 180; 156/252 |
   | 449's kept change | 40/433 -> 33/436; coverage 393 -> 403 | 14/173 both |
   | today | 33/436, 0.0757; 403/473 | 14/173, 0.0809; 159/252 |
   | HM-REQ-010, below 0.01 | not met (sender not stated 0.0882; the other three real conditions 0, none a requirement condition) | not met on any condition with a sure letter |

   The `PARKED.md` line as written:
   `PARKED: 2.4 | unit 10 launched not recorded | 2026-09-27 17:47 | drift | Step 2 closed partial
   after three consecutive units with no kept change (460, 470 and 471), with MET-CER-SURE
   standing at 33 of 436 sure on the real set (inferred keys) and 14 of 173 on the synthetic set
   (exact keys) against HM-REQ-010's one in a hundred, and the trace is held in
   docs/phase-requirements/metrics.md's step-2 sections, .run-unit/unit470-trace.txt,
   .run-unit/unit471-trace.txt, and the facts WhatTheSureLettersWerePrintedUnderFact and
   WhatTheAcquiringLettersReadAtTheProvedValuesFact.`
6. **The commit table, and every figure at exit:**

   | commit | what | build | engine line | app line | named | captures | adjudicated |
   |---|---|---|---|---|---|---|---|
   | `78f69b94` | task 0: record, 1.13.158, runner's writes, entry | 0 errors | 178/178 | 278/278 on the rerun (275 first, dispatcher loop) | 13/13 | 51/51 | 13/13 |
   | `d0a1fc36` | task 1: the fact and the trace, rule registered | 0 errors | 178/178 | 278/278 on the rerun (276 first, dispatcher loop) | 13/13 | 51/51 | 13/13 |
   | `0fa6f02c` | task 2: refused; metrics.md, patch, printouts | 0 errors | `src`/`tests` identical to `d0a1fc36`, not re-run | as `d0a1fc36` | as `d0a1fc36` | as `d0a1fc36` | as `d0a1fc36` |
   | `d5bf90e1` | task 3: closing section, PARKED line, 2.4 tick | 0 errors | 178/178 | 278/278 first run | 13/13 | 51/51 | 13/13 |
   | task 4 commit | exit printouts, output.md | as `d5bf90e1` (no `src` or `tests` change) | | | | | |

   | check | entry | exit |
   |---|---|---|
   | build | 0 errors | 0 errors |
   | engine / app line | 178 of 178 / 278 of 278 (rerun) | 178 of 178 / 278 of 278 (first run) |
   | floors: named, captures, adjudicated | 13, 51, 13 | 13, 51, 13 |
   | real set, inferred | MET-CER-SURE 33/436, MET-INVENTED 33/473, coverage 403/473, MET-WBE 37/113 | the same |
   | synthetic set, exact | 14/173, 14/252, 159/252, 44/84 | the same |
   | `TheArbitrationEarnsItsPlaceTests` | (a) 5/5, (b) harness 1/1, live 2/2 | the same |
   | `TheSpeedFollowsTheSendersMarkPairsTests` | red at 28 of 31 (469, 470) | red at 28 of 31, not required |
   | every recording's text and classes | `unit471-texts-entry.txt` | byte-identical, ours and port |

   The exit prints (`.run-unit/unit471-exit-print.sh`) are empty for all three diffs:
   - `git diff a2e07f6d -- src`;
   - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`;
   - `git diff 7e209cb4` over the eleven transmit files, all 11 present.

   `git diff a2e07f6d -- tests` is the fact alone. `git status` shows `.run-unit/fldigi/` still
   untracked.

## 4. What's blocking us

Nothing blocks. Each item is a plain reading, not a ruling request.

1. **The R86 mismatch.** R86 bars steps 2 to 8 until 9.4 has a kept change. 9.4 has no route
   left, since (A) to (E) and (W) were all refused. This unit worked step 2 as the launcher named
   it, as units 468 to 470 worked outside step 9.
2. **DECIDED (3) and (4) are the arbiter's, and overrulable.** "Re-read" was read as: the window
   mixed down again from the raw audio at the proved pitch, decoded at the proved speed with the
   read's gaps, and settled in place of the ordinary read over what arrived while acquiring and is
   unsettled. "The proved values" were read as `Tracker.ToneHz` and `Reading.WordsPerMinute` at the
   read where both states are proved. "First proved" was read per letter (section 1).
3. **`PARKED.md`'s header against 449 DECIDED (3).** The header says the file is *"Written by
   run-phase.bat, never by a session, and append-only"*. This session appended one line, as 449
   DECIDED (3) and task 3 direct. The line is in the file's form, but its unit field is the
   runner's iteration number, `unit 10`, and its launch time reads `not recorded`.
4. **Ticking 2.4 leaves every line of step 2 ticked**, so the launcher reads step 2 as closed while
   HM-REQ-010 is not met: real 33 of 436 against one in a hundred, synthetic 14 of 173. That is
   2.4's own "closes partial" set against the owner's checkbox rule of 2026-09-23. It is a
   finding.
5. **Why the re-read cannot reach the wrong letters, as the tree stands.** The speed half is met
   already: the read that proves the speed has decoded the unsettled letters at that speed. Only
   the mix-down pitch can differ, and on this corpus that changes almost nothing. The wrong letters
   settle before a proof arrives (the window is 1.0 s behind the audio), or sit in the 7 real
   recordings that never reach both proved. A longer delay or a hold on settling would reach 5 of
   the 29. Both are V-14's new constants and are not tried.
6. **A side result:** synthetic TX-ITU at 5 dB fell to 0 of 62 under the refused change, below
   HM-REQ-010's line, while character gap 5 at 5 dB rose. Recorded only.
7. **2.4's state after this unit: ticked.** Step 2's count of consecutive units with no kept
   change is 3 of 3 (460, 470, 471), and step 2 is closed partial.
