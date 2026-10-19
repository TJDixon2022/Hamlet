# Work instruction 573 - The shadow rule reaches the grid, down to one lane's spacing

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
  - `PROJECT_CARD.md` says Hamlet, and `Hamlet.sln` is at the root.
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwShadow.cs` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, three commits (task 1, task 2, the measure and record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted. The fixture is synthetic.
  - A stray shell call during the work invoked Python, which the rules bar. It was stopped, ran nothing, and changed nothing.
- **Task 1, the rule reaches the grid** (`87ea48ce`):
  - **`CwShadow.ShadowReachHz`, 25 Hz.** The handover's same-station distance, 50 Hz, is unchanged. How the two sit together in the code:
    - The handover still asks `HandoverHz` whether the grid holds a lane's station.
    - The shadow rule asks `ShadowReachHz` how far a stronger station must be.
    - The lanes are judged in `LaneIsShadow` and the grid's sequences in `GridIsShadow`, so nothing is judged twice.
  - **The grid path:** the pattern gate now reports the sequence an offered mark went to (`LastOfferedId`). Before a grid sequence's stood marks are handed to the sender gate, the sequence is judged by `CwShadow.IsShadowOf`, over its last ten marks, against every stronger station Hamlet holds:
    - the pattern gate's other standing sequences and candidates;
    - the bank's standing lanes.

    It is judged again each time a mark of it stands. A shadow's marks are not handed on, so the sequence is never picked. A held one judged a shadow is let go by the sender gate's own release, and sets the time a lane picked within `LaneLeadSeconds` hands on no backlog.
  - **The record:** the capture sheets' `bank` line ends `; shadows 550 Hz 0.90 of 600 Hz, ...`, the shadows held in the last minute by lanes and grid alike, or `none`. Each `cw_listen` row gains `shadows`.
  - **One choice inside the rule** (section 4): inside 50 Hz the stronger station must also be louder by level.
- **Task 2, the fixture and the trace** (`db983cc0`): `TheShadowReachesTheGridTests`.
  - **The fixture**, generated as the synthetic CQs are: a 600 Hz station at 18 WPM and 20 dB, and the same keying at 550, 575 or 650 Hz, 15 dB under it.
  - **The traces:** the fixture alone and with each shadow, W1AW piece-04, and `cw-2026-08-31-003229`, each with the grid judged and not.
- **The measure: the ship failed**, on two conditions:
  - **W1AW** fell from 2111 to **2079**: 2115 of 2151 right, wrong 20 to 36, 0 invented.
  - **The fixtures' 600 Hz station** did not read whole beside its shadow, before or after.

  So `CwShadow.JudgesTheGrid` is **off**, and the reach is off with it. Off reads exactly as at HEAD.
- **Records:**
  - Version 1.13.256 to 1.13.257.
  - HM-DEC-277 in `DECISIONS.md` and its `CLAUDE.md` §1 row.
  - `CW_SPEC.md` §2.
  - A 573 row in each scoreboard table.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey rewritten by its own run, with a new `Rerun` line.
- **Recorded under §12.1:** nothing. HM-DEC-277 records the work order's own outcome.

## 2. What the owner should expect

- **The ghost beside W1AW is not caught by this, and nothing changed on screen.** The grid path is off.
- **Why it ships off:**
  - With the grid judged, W1AW's bulletin lost 32 points. Most of the loss is piece 1's slow stretch, `15 13 AND 10 WPM`, which read `■E ■S T AND 1T M`.
  - The twelve-plus-three held at 297, and the four 5 dB CQs held at 13, 14, 19 and 19 of 21.
- **What the fixture showed:** a tone keyed in lockstep 15 dB under a station never printed at its own pitch, at 550, 575 or 650 Hz, with the rule or without it. No grid sequence there ever stood. Beside it, the station lost letters of its opening call (20, 19 and 18 of 21, against 21 of 21 alone), the same both ways. That damage is at the station's own pitch where the two tones beat, and no shadow rule reaches it.
- **Tonight's sheet will still say something new.** The `bank` line now ends with `shadows ...`, and each `cw_listen` row has a `shadows` field: every shadow held in the last minute, with pitch, share and the station it shadows.
  - With the grid path off, only the lanes judge, still to 50 Hz, so a sender at exactly 550 Hz beside W1AW will **not** appear there.
  - Lanes further out will, as they did on piece-04: 350 to 525 Hz and 650 to 850 Hz.
- **Suite:**
  - Build clean, 0 warnings.
  - App line 278 of 278. Survey line 1 of 1, every group unchanged.
  - **The engine line passed 146 of 146 and then its test host crashed.** The one retry crashed the same way after 145 passed. Per HM-DEC-276 rule 3 it is reported as **not measured whole**, with no further retry. The retry was not fully alone: idle `dotnet` build-server processes were still up.
  - The two scoreboard row checks and `DecisionLogOrderTests` passed after the records were written.
- **Pushed to `main`.** Left uncommitted, as found: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, the `.run-unit\reports` files and the `cases-*.txt` files.
- **What will look wrong but is not:**
  - `TheShadowReachesTheGridTests.TheShadowPrintsNothing` asserts only that nothing prints at the shadow. The station-whole half is reported in its output rather than asserted, because it fails at HEAD too.
  - The new tests are not on the carry-forward line.

## 3. What you should see

- **The three fixtures, before and after** (before = HEAD, after = the grid judged to 25 Hz):

  | shadow | before | after | at the shadow |
  |---|---|---|---|
  | none | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 | the same | - |
  | 550 Hz | `NQ CQ CQ DE N0CALL N0CALL K`, 20 of 21 | the same | nothing, both |
  | 575 Hz | `IS EIA CQ CQ DEN0CALL N0CALL K`, 19 of 21 | the same | nothing, both |
  | 650 Hz | `NQ CQ DE N0CALL N0CALL K`, 18 of 21 | the same | nothing, both |

  The letters `IS EIA` print at 599 to 604 Hz, the station's own pitch. Built first on contrast alone, the grid rule held the station's own 600 Hz sequence as a "shadow" of its skirt at 575 or 625 Hz, and the station alone read `CQ CN ... TNALL`. That is why the level condition is there.
- **W1AW piece-04, grid sequences held back within 25 to 50 Hz of W1AW: none.**
  - The grid path held no sequence at all.
  - All 706 printed letters sat at 598 to 603 Hz, both ways.
  - No grid sequence within 25 to 50 Hz was ever picked.
- **`cw-2026-08-31-003229`:**
  - **As shipped**, `S EA I E THS NE A` prints from the bank's lane at **850 Hz**, held from 8.1 s to 22.1 s (100 ms dit, then 60 ms) at 13.4 to 19.8 dB contrast.
  - **What else stood:** the only stronger thing standing anywhere was a grid sequence at **584 Hz**, of one length and shape 0.11, not a station. The rest were sequences of one or two 20 ms marks.
  - **With the reach at 25 Hz**, the 850 Hz lane is held as a shadow of an **800 Hz lane**, 50 Hz away, at shares 0.67 to 0.70 (15.0 s and 22.0 s), and the capture prints `ZES`.
  - **The grid path holds nothing here.** The rule as extended does hold it, through the lanes' reach.
- **The survey's groups (score), before and after:**

  | Group | Before (572) | After (573) |
  |---|---|---|
  | Synthetic fixtures | 44 | 44 |
  | Receiver fixtures, easy | 75 | 75 |
  | Receiver fixtures, working | -4 | -4 |
  | Receiver fixtures, edge | 0 | 0 |
  | Receiver fixtures, ungraded | 63 | 63 |
  | Synthetic CQs, 15 dB | 72 | 72 |
  | Synthetic CQs, 5 dB | 46 | 46 |
  | Synthetic CQs, 0 dB | 0 | 0 |
  | The boards | 297 and 2111 | 297 and 2111 |
  | Strays: highest, summed | 3.3, 21.9 | 3.3, 21.9 |

- **The scoreboards' rows:**
  - Twelve-plus-three: `| 573 | 2026-10-19 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | …`. Judged, it also read 297, with the carrier at one seed, nothing in a silence, and the first recording whole.
  - W1AW: `| 573 | 2026-10-19 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added | …`. Judged: 2079.
- **The ship conditions, judged:**

  | Condition | Result |
  |---|---|
  | Both boards | **fail**: W1AW 2079 against 2111, wrong 20 to 36 (`docs\cw-scoreboard.md`, `w1aw-2026-10-07/piece-01` at 198.1 to 240.3 s) |
  | Hard limits | carrier at 1 seed, nothing in a silence, first recording whole: hold. Strays with it on: not measured, since the survey ran once, as shipped |
  | Weak-end guard | 13, 14, 19, 19 of 21: holds |
  | The three fixtures | **fail**: nothing at the shadow, but the station reads 20, 19 and 18 of 21 (`TheShadowReachesTheGridTests`) |
  | W1AW piece-04 | no grid sequence within 25 to 50 Hz picked: holds |

## 4. What's blocking us

- **Choice taken: inside 50 Hz the stronger station must also be louder by level.** On contrast alone the grid rule took the station's own sequence for a shadow of its skirt. Past 50 Hz the rule is unchanged, and no constant (30 ms, two thirds) moved.
- **Choice taken: the reach goes off with the switch.** Off means the lanes are judged to 50 Hz, exactly as at HEAD. The order named only the grid path, but the 25 Hz reach on the lanes alone changes what prints (on `003229` it cut 11 letters to 3). Keeping it on would have been a second setting with its boards unmeasured.
- **Choice taken: the fixture test asserts only nothing-at-the-shadow.** The station-whole half fails at HEAD as well, and is reported, not asserted.
- **The ghost at 550 Hz is still not reproduced.** A lockstep tone 15 dB down never stands as a grid sequence on replay. What printed live is not this.
- **The fixtures' station losing its opening beside a lockstep tone** is a separate fault at the station's own pitch, where two tones 25 to 50 Hz apart beat.
- **The W1AW loss with the grid judged** is in the slow stretch of piece 1. It was not traced further.
- **The engine line's test host crashes** after its last test, twice, with no reason in the log. Unit 572 ran the same line clean.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
