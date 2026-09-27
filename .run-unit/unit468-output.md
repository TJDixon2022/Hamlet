READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 3 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop with every
   mechanism 9.3 named refused, and this unit worked step 7 against
   R86's words, reported as a mismatch for the owner.
B. Step 7, criterion 7.2 (HM-REQ-050): profiles produced TX-ITU,
   TX-KEYER-W, TX-FARNS, TX-TIGHT, TX-BUG; refused TX-STRAIGHT and
   TX-SLOPPY (section 10 gives no number for the drift or the
   inconsistency that defines them); proof watched red on TX-KEYER-W,
   TX-FARNS, TX-TIGHT and TX-BUG against a nominal sender, then green
   yes (16 of 16); HM-REQ-050 at 15 dB on CH-AWGN, ours: TX-ITU not
   (2 of 3 cases), TX-KEYER-W not (1 of 3), TX-FARNS not (0 of 3),
   TX-TIGHT not (0 of 3), with the worst case (TX-TIGHT 18 WPM)
   coverage 1/21, MET-CER-SURE 1/2 and MET-WBE 6/7; src unchanged
   yes; 7.2 ticked.
C. The findings weighed against A and B: section 4 raises 9 items.
   None is in the way of 7.2, which asks for the measurement and has
   it. None is in the way of 7.3. Item 6 bears on 7.4: 7.4's "states
   what each does not prove" is written for the sender fixtures in
   senders.md, but 7.4 also waits on 7.3's INT-* profiles, so it
   stays open. The R86 mismatch in one line: R86 bars steps 2 to 8
   until 9.4 has a kept change, 9.4 has no route left, and this unit
   worked 7.2 anyway on ARBITER.md section 4 and the launcher's
   instruction, so if the owner holds R86 this unit is fixtures and a
   measurement only.

UNIT:       468 - complete at task 3 of 3, none dropped - 2026-09-27 12:18
PHASE GOAL: Hamlet's CW decoding meets every requirement in CW_REQUIREMENTS.md at the conditions CW_SPEC.md names, each proved by a test that names it.
UNIT GOAL:  Make the test fixtures key every TX-* sender profile that CW_SPEC.md section 10 gives numbers for, with exact keys, and prove each on its own rendered keying. Then measure HM-REQ-050, fist by fist, at 15 dB reference on CH-AWGN, with the decoder untouched.
ADVANCED:   yes - 7.2 ticked: five profiles produced and two refused by name; the proof was watched red on every non-ITU profile and is green; the recipe is written; HM-REQ-050 is measured per profile. Not met is the finding 7.2 asks for.
NUMBER:     HM-REQ-050 not met on 4 of 4 must-tier profiles at 15 dB CH-AWGN (synthetic, exact); met on 0 of 4; worst coverage 1/21, MET-CER-SURE 1/2, MET-WBE 6/7 (TX-TIGHT 18 WPM)
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (carried from 467; step 7 reset on this unit's advance)

## 1. What Claude did

**Complete at task 3 of 3, none dropped.** The drop candidate, the emitted and port-alone
columns and the should-tier cases, was all done. Provenance: Claude Code on QUIVERFULL, project
Hamlet, gate passed (the four MUST EXIST present, `CoreHMI.sln` and `MURC.sln` absent), branch
`main`, HEAD at entry `0f30a782`.

**The tree against section 3.** Everything matched, with three differences:
- `.run-unit\watched.rc` did not exist. At start `watched.cpu` showed deleted. The runner
  rewrote it before task 0's commit, which took it as it was, and it has changed again since.
  I left it unstaged as the runner's.
- `CwFixtureRecipe` has no file of its own. It is in `CwFixtureGenerator.cs`, lines 38 to 57.
- `SESSION.lock` (PID 29664) is the launcher's and was not staged. `.run-unit\fldigi\` was not
  staged, and nothing was fetched.

Both `PHASE_PLAN.md` copies were identical at entry: 7.1 and 7.5 ticked, 7.2 to 7.4 open, 9.4
the only open line of step 9, and R86 at line 342. The port diff against `19109b51` was empty.

**Task 0.** I wrote `## UNIT 468 - STEP 7` into both `PHASE_OUTCOME.md` copies and set both
`PHASE_STATUS.md` copies to 468 with `CURRENT_STEP: 7`. The version went from 1.13.154 to
1.13.155. The entry round matched 467's exit on every figure (table in section 3). The trace
(`.run-unit/unit468-trace.txt`) found:
- the recipe can key any dit, dah and gap lengths, but draws nothing and has no per-element
  jitter;
- character speed against overall speed is not a field, but it follows from stretched gaps;
- `CwChannel.Render` hard-wires TX-ITU keying, so no other sender could reach the channel.

Commit `2ac33910`.

**Task 1.** I built three things, all in `tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/`:
- `CwSender.cs`: the seven section 10 rows, `Timing`, `Recipe`, `Generate`, `Render` and
  `Keyed`, with seeds `468000 + 100 x row + speed index` fixed before any decode.
- `CwChannel.RenderKeyed`: an overload that takes a caller's recipe. `Render` now builds TX-ITU's
  recipe and its sender line and calls it, so the operations are the same ones in the same
  order.
- `TheSenderProfilesAreWhatTheySayTests`: 1 fact and a 15-case theory.

The proof measures the tone before the band is added and never reads the recipe. The text is
used only to label dits and gap kinds.

The final red run failed all 12 non-ITU cases and passed TX-ITU's 3. The green run passed
16 of 16. **Two earlier runs are kept, and each found a fault in the proof, not the sender.**
- The first red run also failed TX-ITU at 25 WPM, reading the word gap as 7.021 units. The
  raised cosine is exactly zero at an on-grid edge, so counting keyed samples read every mark
  one sample short. Runs now start at the silent sample.
- The first green run failed TX-KEYER-W and TX-FARNS at 25 WPM on the word gap, by 0.022 and
  0.023 units against a flat 0.02. That tolerance was tighter than one sample per edge. The
  tolerance is now 2 (1 + G) / D, with a floor of 0.02.

After each fix, the red run was repeated and stayed red on all 12. At task 1: floors 13, 51
and 13; engine 178 of 178; app 278 of 278 on the one rerun (the first run lost
`TheTestsStayOffTheNetworkTests` to the dispatcher loop). Commit `da20f5f8`.

**Task 2.** `TheMustFistsAtFifteenDecibelsFact` asserts nothing. It covers 12 must-tier cases
plus TX-BUG's 3. Each case is read three ways:
- ours, exactly as `TheRequirementsAreMeasuredTests` reads the synthetic set and through the
  same `Measure` and `CwMetrics`;
- the live emitted transcript under `CwSwitchTable.Live`;
- the port alone.

The table went into `senders.md` section 5 and the running line into `metrics.md`. The first
run printed each profile's verdict as "NOT MET on all three cases", which was wrong wording.
It now prints "k of 3 cases met", and the numbers did not change on the rerun. Floors 13, 51
and 13; engine 178 of 178; app 278 of 278 at first run. Commit `2398b076`. **7.2 ticked** in
both copies, commit `ddb65409`.

**Task 3.** The exit round is in section 3's table. Every entry figure is as at entry. The app
line lost `TheChipSaysTheChosenModeTests` to the dispatcher loop on the first run and again on
the rerun. Under DECIDED (7) it was run alone and passed 6 of 6.
`TheSpeedFollowsTheSendersMarkPairsTests` is red at 28 of 31, as expected.

**Decisions I made for myself, each recorded as the arbiter's reading and overrulable:**
1. The case's speed, 12, 18 or 25 WPM, is TX-FARNS's *character* speed, and the overall speed
   falls out below it. That keeps the dit comparable across profiles.
2. One draw per case, not per element. No produced profile has a jitter number.
3. Pitch 600 Hz with no drift, the channel cases' and the decoder's starting pitch.
4. "Emitted under `CwSwitchTable` as it stands" is read as the live path with
   `CwSwitchTable.Live`, because a new condition has no row and `For` would default to
   Arbitrate.
5. The proof's tolerance is the sampling's, as above.

## 2. What the owner should expect

Nothing on the screen changes. Hamlet can now make test signals in the four kinds of "fist" the
specification says it must read: a machine keyer, a weighted electronic keyer, a Farnsworth
traffic sender with long gaps between letters, and a tight hand whose gaps are shorter than
its dots. It can also make a bug. Each was measured off its own audio and found to be keyed the
way the specification describes. On a loud, clean signal:
- **The machine keyer** reads whole at 12 and 18 WPM. At 25 WPM it adds a sure extra letter at
  the end: `CQ CQ CQ DE N0CALL N0CALL KK` for `... K`.
- **The weighted keyer** reads whole only at 18 WPM. At 12 it prints two placeholders inside
  the callsign, `N■0CALL N■CALL`. At 25 it reads the first `C` as a sure `<AR>`.
- **The Farnsworth sender** gets every letter but splits words: `C Q CQ CQ DE N0 C A LL N 0CALL K`
  at 18 WPM. At 25 it loses one `C`, reading `CQ  Q CQ`.
- **The tight fist** does not read at all: `■■■B■■K` for `CQ CQ CQ DE N0CALL N0CALL K` at 18 and
  25 WPM, and `■■■B■■■■<KN>■ ■ALLK` at 12.

What will look wrong but is not: the second reader and the switch change nothing on any of
these signals, because the emitted transcript is ours letter for letter. That is the table as
it stands, not a fault this unit introduced.

## 3. What you should see

No visible change. This unit makes test signals and measures the decoder against them.

**HM-REQ-050, at 15 dB in the 2500 Hz reference on CH-AWGN, ours alone.** Every figure is
synthetic, exact. The key is `CQ CQ CQ DE N0CALL N0CALL K`: 21 characters, 7 words. HM-REQ-013
asks for MET-COVERAGE 21/21, MET-CER-SURE 0 and MET-WBE 0.

| profile | WPM | seed | ours read | MET-COVERAGE | MET-CER-SURE | MET-WBE | HM-REQ-013 |
|---|---|---|---|---|---|---|---|
| TX-ITU | 12 | 468000 | `CQ CQ CQ DE N0CALL N0CALL K` | 21/21 | 0/21 | 0/7 | met |
| TX-ITU | 18 | 468001 | `CQ CQ CQ DE N0CALL N0CALL K` | 21/21 | 0/21 | 0/7 | met |
| TX-ITU | 25 | 468002 | `CQ CQ CQ DE N0CALL N0CALL KK` | 21/21 | 1/22 | 0/7 | **not met** |
| TX-KEYER-W | 12 | 468100 | `CQ CQ CQ DE N■0CALL N■CALL K` | 20/21 | 0/20 | 0/7 | **not met** |
| TX-KEYER-W | 18 | 468101 | `CQ CQ CQ DE N0CALL N0CALL K` | 21/21 | 0/21 | 0/7 | met |
| TX-KEYER-W | 25 | 468102 | `<AR>Q CQ CQ DE N0CALL N0CALL K` | 20/21 | 1/21 | 0/7 | **not met** |
| TX-FARNS | 12 | 468200 | `C Q CQ CQ DE N0CALL N0CALL K` | 21/21 | 0/21 | 1/7 | **not met** |
| TX-FARNS | 18 | 468201 | `C Q CQ CQ DE N0 C A LL N 0CALL K` | 21/21 | 0/21 | 5/7 | **not met** |
| TX-FARNS | 25 | 468202 | `CQ  Q CQ DE N0CALL N0CALL K` | 20/21 | 0/20 | 0/7 | **not met** |
| TX-TIGHT | 12 | 468300 | `■■■B■■■■<KN>■ ■ALLK` | 4/21 | 2/6 | 7/7 | **not met** |
| TX-TIGHT | 18 | 468301 | `■■■B■■K` | 1/21 | 1/2 | 6/7 | **not met** |
| TX-TIGHT | 25 | 468302 | `■■■B■■K` | 1/21 | 1/2 | 6/7 | **not met** |

**Per profile, met only if all three cases are:** TX-ITU not met (2 of 3), TX-KEYER-W not met
(1 of 3), TX-FARNS not met (0 of 3), TX-TIGHT not met (0 of 3). The full printout, with
MET-INVENTED, is `.run-unit/unit468-fists-t2.txt`.

1. **The profile list** (`docs/phase-requirements/senders.md` section 1):
   - Produced:
     - TX-ITU (must): 3 : 1 : 3 : 7.
     - TX-KEYER-W (must): ratio 2.5 to 3.5, each gap at 0.7 to 1.3 of nominal, `[verify]`.
     - TX-FARNS (must): character gap 3 to 7 units at character speed; word gap 8.77 units,
       the traffic net's.
     - TX-TIGHT (must): 013347 as HM-DEC-101 fitted it, 2.695 : 0.619 : 1.238 : 2.667.
     - TX-BUG (should): ratio 3.5 to 5, gaps nominal.
   - Refused:
     - TX-STRAIGHT: no number for the drift of ratio, gaps and speed.
     - TX-SLOPPY: no number for the inconsistency, and errors change the characters sent.
2. **The proof, red** (`.run-unit/unit468-senders-red.txt`, keyed 1:3:1:3:7 whatever the
   profile): 12 failed, 4 passed (TX-ITU's 3 and the list fact). Each profile failed on its own
   definition:
   - TX-KEYER-W: its drawn ratio and all three gaps;
   - TX-FARNS: the character gap, the word gap, the overall speed, and "character speed above
     overall speed" (12.000 against 12.000 at 12 WPM);
   - TX-TIGHT: every length, "element gaps shorter than the dit" (1.0000) and "character gaps
     compressed" (3.0000);
   - TX-BUG: ratio 3.0000 against 3.5 to 5.

   **The proof, green** (`.run-unit/unit468-senders-green.txt`): 16 of 16. At 18 WPM, measured
   against stated:

   | profile | dah/dit | element gap | character gap | word gap | overall WPM |
   |---|---|---|---|---|---|
   | TX-ITU | 2.9974 / 3 | 0.9976 / 1 | 2.9949 / 3 | 6.9900 / 7 | 18.000 / 18.000 |
   | TX-KEYER-W | 3.3459 / 3.3506 | 0.7083 / 0.7115 | 3.2448 / 3.2531 | 6.9297 / 6.9448 | 17.862 / 17.862 |
   | TX-FARNS | 2.9966 / 3 | 0.9968 / 1 | 4.5578 / 4.5672 | 8.7562 / 8.7719 | 16.177 / 16.177 |
   | TX-TIGHT | 2.6933 / 2.6952 | 0.6167 / 0.6190 | 1.2346 / 1.2381 | 2.6614 / 2.6667 | 25.212 / 25.212 |
   | TX-BUG | 3.7741 / 3.7777 | 0.9972 / 1 | 2.9944 / 3 | 6.9886 / 7 | 16.251 / 16.251 |

3. **The should-tier cases, TX-BUG under HM-REQ-052:**
   - 12 WPM met, 21/21, 0/21, 0/7;
   - 18 WPM met, the same;
   - 25 WPM not met: `... KK`, MET-CER-SURE 1/22.

   Not met, 2 of 3.

   Beside ours, the live emitted transcript reads all 15 cases exactly as ours reads them. The
   port alone meets none of the 15: on every case its first `CQ` is lost or cut, it splits
   TX-FARNS into single letters (MET-WBE 13/7 and 14/7), and it reads TX-TIGHT as `QTBK`,
   `TÅBK` and `EEK`.
4. **Commit table, the five at each commit:**

   | commit | what | build | engine line | app line | named | captures | adjudicated |
   |---|---|---|---|---|---|---|---|
   | `2ac33910` | task 0: record, bump, entry, trace | 0 errors | 178/178 | 278/278 | 13/13 | 51/51 | 13/13 |
   | `da20f5f8` | task 1: CwSender, proof, senders.md | 0 errors | 178/178 | 278/278 on the one rerun (first run 277, dispatcher loop) | 13/13 | 51/51 | 13/13 |
   | `2398b076` | task 2: the fact, senders.md, metrics.md | 0 errors | 178/178 | 278/278 first run | 13/13 | 51/51 | 13/13 |
   | `ddb65409` | tick 7.2 (plan lines only; code as `2398b076`) | as `2398b076`, not rerun | as `2398b076` | as `2398b076` | as `2398b076` | as `2398b076` | as `2398b076` |
   | task 3 commit | exit round, output.md | 0 errors | 178/178 | 277/278 on the rerun; `TheChipSaysTheChosenModeTests` alone 6/6 (DECIDED (7)) | 13/13 | 51/51 | 13/13 |

**Every figure, entry beside exit:**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 278 of 278 | 277 of 278 on the rerun, and the lost type alone 6 of 6 (DECIDED (7)) |
| floors: named, captures, adjudicated | 13, 51, 13 | 13, 51, 13 |
| real set, inferred | MET-CER-SURE 33/436, coverage 403/473, MET-WBE 37/113 | the same |
| synthetic set, exact | 14/173, 159/252, 44/84 | the same |
| `TheChannelProfilesAreWhatTheySayTests` | 71 of 71 | 71 of 71 |
| `TheSenderProfilesAreWhatTheySayTests` | did not exist | 16 of 16 |
| `TheArbitrationEarnsItsPlaceTests` | (a) 5/5, (b) harness 1/1, live 2/2 | the same |
| `TheSpeedFollowsTheSendersMarkPairsTests` | red at 28 of 31 (467) | red at 28 of 31, not required |

The exit prints are all empty, printed by `.run-unit/unit468-exit-print.sh`:
- `git diff 0f30a782 -- src`;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`;
- `git diff 7e209cb4` over the eleven transmit files, all present.

`git status` shows `.run-unit/fldigi/` still untracked.

## 4. What's blocking us

Nothing blocks 7.2. Each item below is a plain reading, not a ruling request.

1. **The R86 mismatch.** R86 bars steps 2 to 8 until 9.4 has a kept change. This unit worked
   step 7 anyway. 9.4 has no route left, because every mechanism 9.3 named, (A) to (E) and (W),
   was refused under R78 by units 459, 462 and 463. `ARBITER.md` section 4, the launcher's
   instruction to choose another step when every route is recorded no, and `PHASE_PLAN.md`
   section 5 all direct a move. If the owner holds R86, this unit's work is fixtures and a
   measurement, and nothing the operator reads has changed.
2. **Every profile parameter is the arbiter's reading of `CW_SPEC.md` section 10, overrulable.**
   - TX-KEYER-W: ratio 2.5 to 3.5, and "gaps ±30 %" read as each of the three gaps at 0.7 to
     1.3 of its nominal, drawn independently. **Carries `[verify against WinKeyer
     documentation]`.**
   - TX-FARNS:
     - the case's speed is the character speed;
     - the character gap is drawn from 3 to 7 units;
     - "word gap longer" takes the vendored example's number, HM-DEC-115's 500 ms over a 57 ms
       dit, 8.77 units, in every case;
     - dah and element gap are TX-ITU nominal.
   - TX-TIGHT: HM-DEC-101 records no figure, so this is `CwFixtureRecipe`'s defaults, the
     generator as HM-DEC-101 fitted it to 013347: 283 / 65 / 130 / 280 ms over a 105 ms dit.
     HM-DEC-145's own measurement of the same capture reads dah 2.73, element gap 0.73 and
     character gap 1.49, with no word gap. It agrees on the shape, but its character gap is
     0.25 units wider. The fit was preferred because it gives all four lengths from one source.
   - TX-BUG: ratio 3.5 to 5; the gaps are TX-ITU nominal, marked, because "dits short and
     fast, gaps variable" has no number.
   - TX-STRAIGHT and TX-SLOPPY are refused.
   - Gold 1959 (`CW_SPEC.md` 12 item 6) is unvendored, so every hand-sent range is.
3. **HM-REQ-050 is not met, recorded as a finding for the decoder steps and not chased here:**
   - TX-ITU at 25 WPM: a sure extra `K` at the end. TX-BUG at 25 WPM does the same.
   - TX-KEYER-W at 12 WPM: two placeholders inside `N0CALL`. At 25 WPM: a sure `<AR>` for the
     opening `C`.
   - TX-FARNS: sure word boundaries inside words at 12 and 18 WPM (MET-WBE 1/7 and 5/7). This
     is HM-REQ-054's behaviour, measured only as it fell out. At 25 WPM one `C` is lost.
   - TX-TIGHT: almost everything placeholders at all three speeds, 4, 1 and 1 of 21 sure and
     right.

   The tight fist and the Farnsworth word boundaries bear on steps 3 and 6. The opening and
   closing letters at 25 WPM bear on step 5.
4. **The switch changes nothing on these signals.** The live emitted transcript equals ours on
   all 15 cases, and the port alone is worse on every one. Nothing was switched, and no case
   was added to `CwSwitchTable` (parked).
5. **The draws cover the ranges thinly.** The three KEYER-W ratios fell at 3.33 to 3.37 and the
   three FARNS character gaps at 4.51 to 4.63, with the seeds fixed before any decode and
   nothing re-seeded (V-14). A verdict at the ranges' ends (ratio 2.5, character gap 7) is not
   in these cases.
6. **7.4's "states what each does not prove"** is written for the sender fixtures in
   `senders.md` section 4. 7.4 still waits on 7.3's INT-* profiles, so it was not ticked.
7. **HM-REQ-050's verification row names "synthetic + 013347 and HM-DEC-115 captures".** The
   captures were not measured here. They sit in `TheRequirementsAreMeasuredTests` under
   inferred keys and with no SNR in the 2500 Hz reference, so they are not HM-REQ-013's
   condition.
8. **`CwChannel.Render` now goes through `RenderKeyed`.** The operations are the same ones in
   the same order, and the channel proof is 71 of 71 before and after. No byte comparison
   against the pre-change render was run.
9. **Housekeeping.** Once, `tools/status.sh` was run from the fixtures folder and wrote a
   `PROJECT_STATUS.md` there. It was moved to `.run-unit/unit468-stray-status.tmp` at once and
   never staged. The root file was rewritten from the root.
