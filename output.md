# Unit 571 - A shadow is told by its timing

## 1. What Claude did

**Gate and surface:** Claude Code on the development computer, branch `main`. The prompt claimed `PROJECT: Hamlet`. The tree confirmed it: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio.

**Run by hand:**
- `SESSION.lock` was taken and released through `tools\arbiter\lock.bat`.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
- Nothing keys or transmits, and nothing was written to the radio. No recording was added, removed or promoted.
- Version 1.13.254 to 1.13.255. HM-DEC-275 was used; HM-DEC-269 stays held for unit 565.

**Task 1, the shadow rule** (`13dd9b35`):
- **The rule is `CwShadow.IsShadowOf`.** A lane's sequence is a shadow of a station that is stronger by contrast and more than 50 Hz away, where `ShadowShare` of its last ten marks start within `ShadowStartMs` of that station's marks.
- **The constants:** `ShadowStartMs` is 30 ms and `ShadowShare` is two thirds.
- **What a lane is judged against:** the grid's standing stations and candidates, and the bank's other standing lanes.
- **Your two rules are in:**
  - A pitch is judged for shadow before the pick. A shadow pitch is never a candidate, and a held lane at one is let go.
  - A lane picked within 3 s of a shadow being let go hands on no backlog.
- **The constants and the figures beside them, handover on:**

  | ShadowStartMs, ShadowShare | Board | W1AW | Carrier prints at |
  |---|---|---|---|
  | **30 ms, two thirds (as built)** | **297** | **2111** | 5195 only |
  | 20 ms, two thirds | 297 | 2111 | 5195 only |
  | 40 ms, two thirds | 288 | 2111 | 5195 only |
  | 30 ms, a half | 297 | 2111 | 5195 only |
  | 30 ms, three quarters | 297 | 2111 | 5195 and 5209 |

  The side variants' boards and carrier seeds were all measured. The run stopped for low memory only after the last of them, so nothing is reported as not measured.

**Task 2, the measure** (`f42a2cc2`): every ship condition held except one.
- **Failed: the strays on the 65 old captures, 21.9 against 21.6.**
- Most of the rise is `cw-2026-08-31-003229`. It reads nothing with the handover off and `S EA I E THS NE A` with it on.
- So the handover stays off, and the shadow rule stays in the tree.
- The survey was rerun with the handover off, as shipped. It reads row for row as at HEAD, with a new `rerun` line.

**Not committed:** the ship code was written but not committed, because the handover did not ship. That is:
- the `Weak stations` row and its `WeakStations` setting;
- the sheet and `cw_listen` fields;
- the `Weak station at <pitch> Hz` story line;
- a public `LaneHandover` and `WeakStationPitchHz` on the detector;
- the guard `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter`.

The patch is kept outside the tree, in the session scratchpad, as `unit571-ship-unshipped.patch`.

**Records:**
- `DECISIONS.md` HM-DEC-275, *A shadow is told by its timing*, with your ruling quoted.
- The `CLAUDE.md` §1 row.
- `PHASE_OUTCOME.md` (both copies) `## UNIT 571 - STEP 12`. `PHASE_STATUS.md` (both copies) names 571.
- `CW_SPEC.md` §2.
- A 571 row in both scoreboard tables, at the figures that shipped: the board as at HEAD.
- Nothing was recorded under §12.1.

**Lines:**
- Engine carry-forward line: 153 of 154 in 6 min 24 s. The one red is `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`, as at HEAD. Both scoreboard guards pass.
- App line: 278 of 278 in 2 min 10 s.
- Survey line: passed, 2 min 23 s off and 6 min 56 s on.

## 2. What the owner should expect

**Weak stations do not yet read in the app.** The handover is still off.

With it on, it now does almost everything we asked:
- The four 5 dB CQs read 13, 14, 16 and 19 of 21.
- The carrier keyed at random prints at one seed of twenty, 5195, as at HEAD.
- Both boards hold: 297 and 2111, with wrong and invented unchanged.
- Your 221745 reads `TONITE` again.
- The survey's 5 dB CQs go from 0 to 46 and `two-station` from nothing to 20 of 27.

What it cost is about three tenths of a stray letter per ten seconds across the 65 old captures. Most of that is one August capture, `cw-2026-08-31-003229`: it prints `S EA I E THS NE A` where it printed nothing. The bar was "does not rise", so it does not ship.

**The shadow rule in two sentences:** a lane whose marks start, two times in three, within 30 ms of a louder station's marks more than 50 Hz away is that station's echo and not a station. It is judged before a lane is picked, so dropping one never sets off a fresh pick at the same station.

**The `Weak stations` setting is not on screen.** It was built for the scan's settings popover, under AGC in CW, and waits on the handover shipping. Nothing on screen has moved.

**The next W1AW session's sheet:** with the handover off, nothing in the app changes and the sheet carries nothing new. On piece 4 the rule held lanes at 350 to 525 Hz and 650 to 850 Hz as shadows of W1AW at 600 Hz. **550 Hz is exactly 50 Hz from W1AW, so the rule cannot reach the live 550 Hz sender**, because it only judges stations more than 50 Hz away.

**What looks wrong but is not:**
- The RSID real-time test is red on the engine line, as at HEAD.
- `docs\cw-survey-2026-10-09.md` changed only its `rerun` line.

Everything is pushed to `main`.

## 3. What you should see

**The carrier at twenty seeds, handover on at 30 ms and two thirds:** only 5195 prints, `NTTTTNTE TEAETOTTNEAIT TAN`. Seeds 5193, 5194 and 5196 to 5212 print nothing; 5209 is silent.

**The four 5 dB CQs, handover on:**

| Fixture | Read | Of 21 |
|---|---|---|
| `cq-12wpm-5db` | `FQ N QCQ DE N0 IL N0N L I K` | 13 |
| `cq-18wpm-5db` | `RQKQCQ DE N0C DL N0K DL K` | 14 |
| `cq-18wpm-5db-char5` | `RQCQN DEN0KALLN0KALLK` | 16 |
| `cq-25wpm-5db` | `NQ CQ CQ DE N0CAL I N0CALL K` | 19 |

**`two-station`:**
- Handover off: nothing.
- Handover on: `FQ CG DE N■ TLL K ■CALL DE W1XYZ K`, 20 of 27.
- No shares were recorded: no lane there is ever judged against a station more than 50 Hz away, so neither station is a shadow of the other.

**W1AW piece-04, the lanes held back as shadows of W1AW at 600 Hz:**

| Lane | Times held | Largest share |
|---|---|---|
| 350 Hz | 517 (5.6 to 277.8 s) | 1.00 |
| 375 Hz | 439 | 1.00 |
| 400 Hz | 593 | 1.00 |
| 425 Hz | 660 | 1.00 |
| 450 Hz | 803 | 1.00 |
| 475 Hz | 931 | 1.00 |
| 500 Hz | 889 | 1.00 |
| 525 Hz | 575 | 0.90 |
| 650 Hz | 6 (71.7 to 224.7 s) | 0.90 |
| 675 Hz | 564 | 0.90 |
| 700 Hz | 1003 | 1.00 |
| 725 Hz | 849 | 1.00 |
| 750 Hz | 743 | 1.00 |
| 775 Hz | 526 | 1.00 |
| 800 Hz | 498 | 1.00 |
| 825 Hz | 422 | 1.00 |
| 850 Hz | 444 | 1.00 |

No lane from 550 to 625 Hz is judged.

**The survey's summary:** "Off" is the tree as shipped, the same as HEAD. "On" is the handover on at 30 ms and two thirds.

| Group | Off (shipped) | On |
|---|---|---|
| Synthetic fixtures | 44 | 44 |
| Receiver fixtures: easy | 67 | 75 |
| Receiver fixtures: working | -4 | -4 |
| Receiver fixtures: edge | 0 | 0 |
| Receiver fixtures: ungraded | 44 | 63 |
| Synthetic CQs, 15 dB | 62 | 72 |
| Synthetic CQs, 5 dB | 0 | 46 |
| Synthetic CQs, 0 dB | 0 | 0 |
| The twelve-plus-three | 297 | 297 |
| W1AW pieces | 2111 | 2111 |
| Strays per 10 s, 65 old captures | 21.6 | **21.9** |

Captures whose strays moved with the handover on:

| Capture | Off | On |
|---|---|---|
| `cw-2026-08-31-003229` | 0.0 | 1.0 |
| `cw-2026-09-24-004550` | 0.0 | 0.3 |
| `cw-2026-08-31-003408` | 1.0 | 0.7 |
| `cw-2026-09-23-173723` | 0.7 | 0.0 |

**The scoreboard rows:**

| unit | date | right | wrong | invented | score | printed in silence | spaces right |
|---|---|---|---|---|---|---|---|
| 571 | 2026-10-13 | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |

| unit | date | score | right | wrong | invented | spaces right |
|---|---|---|---|---|---|---|
| 571 | 2026-10-13 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

## 4. What's blocking us

- **The strays, 21.9 against 21.6,** are the one condition the handover fails, and most of it is `cw-2026-08-31-003229`. That capture is unadjudicated, so nobody yet knows whether `S EA I E THS NE A` is junk or a weak station read badly.
- **The live 550 Hz sender is outside the shadow rule:** it is exactly 50 Hz from W1AW.
- **Reds:** `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` on the engine line, as at HEAD.

**Ask, this unit: what to do about the strays.** Proposed ruling: the next unit traces `cw-2026-08-31-003229` with the handover on (which lane, what pitch, what it stood against) before any rule is changed, and ships on that trace.

| Option | For | Against |
|---|---|---|
| A. Trace 003229 first, then rule (proposed) | Tells junk from a weak real station before a number is chased; one file holds most of the rise | One more unit before weak stations read |
| B. Ship the handover on now, strays at 21.9 | The 5 dB CQs, `two-station` and the easy and ungraded fixtures read today, and every hard limit holds | Breaks a bar the order set, by 0.3 |
| C. Leave it off and move on | Nothing changes | The weak end stays unread |

The industry answer is A. A stray figure measured on an unadjudicated capture cannot tell a weak station from junk, and §0.0 needs that answered before Hamlet prints it.

### Asks still outstanding

- **Unit 571, 2026-10-13:** what to do about the strays (above). Waiting on the owner. The rule sits in the tree behind `CwEnvelopeDetector.LaneHandover`, off. The ship code is in the session scratchpad, not the tree.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by this unit's shadow rule; still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.

Unit 570's ask (the carrier at seed 5209) was answered by your ruling of 2026-10-13, option A, and is dropped.
