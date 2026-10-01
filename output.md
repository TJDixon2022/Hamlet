```
UNIT: 520 - partial (tasks 1 and 2 landed, task 3 dropped with its gate table) - 2026-10-01
UNIT GOAL: a fist is a sender, the best shape gets the terminal, and a neighbour does not kill a station
NUMBER: a fist scattered by a third scores 0.374 against noise's best 0.173 (was 0.000 against 0.107)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSequenceShape.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 520 and HM-DEC-224 were free.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, ticked no box, and added no ruling to either plan.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**Task 1: a fist is a sender.** Commit `c511c07c`.
- **`CwSequenceShape`: tightness is scored against what a hand does.**
  - A cluster's spread scores four-fifths at a hand's widest (unit 513's 0.25 in log-length) and falls to nought at twice it. No hand makes twice a hand's widest, and that is where unit 513's two speeds mixed in one cluster sit (0.45).
  - Four terms at a hand's widest leave a widest fist about two-fifths of a machine's score: under a machine, and a sender. That reason is from what a keyed tone is, not from a result.
  - **A cluster that has shown few lengths is scored as a hand's.** Two prior lengths at a hand's widest stand in until it shows its own. Without this, a short noise sequence whose two dahs happened to agree scored as a machine, and noise's best rose from 0.107 to 0.239.
  - Unit 519's machine scale stays reachable (`againstAHand: false`), so the test prints before and after.
- **`CwEnvelopeDetector`** keeps the best-scoring noise sequence's whole breakdown, for the tests.

**Task 2: the best shape gets the terminal.** Commit `33eaf797`. In `CwRunReader`:
- **The first pick waits one word gap.** When the first sender qualifies, it waits one of that sender's word gaps (its own word-gap boundary), then prints the best-shaped qualified sender. Its letters are banked meanwhile and print a word late. At the end of the audio nothing waits.
- **A switch only at a pause.** A printed sender silent for its word gap and a dah gives the terminal to a better-shaped qualified sender standing then.
- **No mid-word switches.** A mark is seen only once it has ended, so a letter gap followed by a dah falls short of that silence, and only a gap between words passes it. The existing release stays.

**Task 3: dropped, with the gate table.** Commit `02fa29a8`.
- **The gate table.** Unit 519's case 1: a clean 12 dB sender at 625 Hz, 24 dB carrier keyed at random at 825 Hz. The clean sender's marks, with each per-mark gate off in turn:

  | gate off | past the per-mark gates | stood (of 65) |
  |---|---|---|
  | none (all on) | 31 | 29 |
  | edges | 31 | 29 |
  | narrowness | 31 | 29 |
  | shape | 31 | 29 |
  | key-up | 31 | 29 |
  | promptness | 46 | 29 |
  | one-call | 223 (duplicates) | 29 |
  | the fit | 31 | 29 |
  | the pattern gate | 31 | 31 |

  The pattern gate's level tolerance has no switch of its own; switching off the whole gate stands 31. Flatness is how a bar forms, not a gate, and has no switch.
- **No gate is the cause, and narrowness is not it.** The probe of where the clean marks land, by pitch, finds it:
  - with no carrier, 69 at 625 Hz;
  - with the carrier **steady**, **none** at 625 Hz;
  - with it keyed, 23 or 24 at 625 Hz and 36 or 38 at 825 Hz, whatever the carrier's edges (4 or 10 ms).

  **The apex climb (unit 496) is the cause.** It walks a mark to the louder neighbouring bin, so beside a louder station a 12 dB mark is walked up that station's lobe to 825 Hz. The key-up test there sees a carrier that never keyed with it and refuses the mark. The order's principle applies here: the mark was judged against another sender.
- **Three fixes were built and reverted.** Each split a lone station's marks across its own lobe, making duplicates. The lone clean call read `N0CALAE`, `N0C LL` or `N0CALL N0CALAE K`, and the fists broke:
  1. climb to the bin where the mark rises most over its own gaps;
  2. climb to the louder bin only where the mark rises at least as much;
  3. the same, with the gaps measured past the mark's edges.
- **Case 2 now asserts what task 2 delivered.** The fist prints nothing, the clean sender is chosen, and the shape ranks it higher. Its letters beside the fist are still damaged, by this same climb.

**Records:**
- HM-DEC-224 in `DECISIONS.md`, naming unit 519's measurements and the third half as not built.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 520 - STEP 12`.
- `PHASE_STATUS` (both copies) names 520.
- Version 1.13.204 to 1.13.205.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **A rough fist is a station.** It ranks well above noise and prints. A fist as sloppy as a third either way scores about half what a clean machine does, and twice noise's best.
- **The cleanest station gets the terminal.** Hamlet waits one word after the first station appears, then picks the best, so the first word prints a word late. It switches only at a pause between words, never mid-word.
- **A station 200 Hz from a louder one is still lost.** Hamlet decides each mark's pitch by climbing to the louder neighbouring frequency, and a loud neighbour pulls the quieter station's marks onto itself. The cause is found, but this unit has no fix: each one I tried damaged a lone station, so none was shipped. At 400 Hz apart the quieter station reads whole.
- **Nothing about letters changed.**

## 3. What you should see

**The fist scores against noise**, each alone at 20 WPM and 24 dB:

| sender | unit 519's scale | against a hand | reads |
|---|---|---|---|
| clean | 0.850 | 0.779 | `CQ CQ DE N0CALL N0CALL K` |
| fist, a fifth | 0.068 | 0.693 | `CQ CQ DE N0CALL N0CALL K` |
| fist, a third | 0.000 | 0.374 | `CQ CQ DE N0CALL N0CALLK` |
| noise, 30 s, best | 0.057 | 0.061 | nothing |
| noise, 3 min, best | 0.107 | 0.173 | nothing |

**Case 2's switch:** a clean 10 dB sender at 825 Hz through the filter, beside a 20 dB fist at 600 Hz.
- Before: the fist prints `TESTDEW1AWTESTDEW1AWK`, and the clean sender only `ILK`.
- Now: the fist prints nothing, and the clean sender is chosen. Its shape is 0.454 against the fist's. It prints `RTACK E N■CALAEN■KAEILK`, damaged by task 3's climb.
- Alone through the filter it reads whole.

**Case 4's switch:** a fist printing when a clean sender begins at 8 s. The fist reads whole and its last mark ends at 18.35 s. The clean sender's first letter prints at 19.58 s, 1.2 s after the fist stops.

**The gate table** is in section 1.

**Case 1, clean beside a random carrier, by spacing:**

| carrier away | reads |
|---|---|
| 400 Hz | `CQ CQ DE N0CALL N0CALL K` |
| 200 Hz | `NOAM■HIV5■` |
| 150 Hz | `NOAM■HIV5■` |
| 100 Hz | `NOAM■HEEV■■M` |

`NOAM■HIV5■` is the carrier's reading. Against a hand the carrier scores above nought, so it is no longer barred from printing as it was under unit 519's rule. The clean sender's marks are lost to the climb.

**Two-station case at 200 Hz** (24 dB at 625 and 10 dB at 825): the loud one reads whole.

**Every existing case** was run with unit 515's filter, unit 517's fit and word-gap tests, and these cases, and diffed against HEAD's run. All are identical except:
- **The fading bulletin**, wrong either way. HEAD read `THE G NK D ERO WN U EOD T JU MP TER T AZN M DM OJ M4H E8M`; now `… AZN TG M T2V E6Z E8M`.
- **The W1AW all-gates-off whole-band row**: `MarksLast4s` went from 6 back to 18, as before unit 519. It still reads nothing.
- **Three AGC bulletin rows already broken**, which moved slightly with the first word's wait.
- **`TheRowDescribesTheSenderBeingPrinted`** checks 1300 readings instead of 1522, because the terminal starts a word later. It still passes.

Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

1. **Task 3 is dropped.** The apex climb walks a mark to a louder neighbour's lobe, and a climb that follows this mark's keying split a lone station's marks across its own lobe in all three forms tried. A different approach is the next unit's: fit the station's own lobe shape across the bins, or carry each sequence's own pitch into the climb. It needs the owner's say on which.
2. **The random carrier can print again.** Against a hand it scores above nought, 0.03 to 0.05, and where the clean sender's marks are lost beside it, nothing better stands. Task 3 is what removes it.
3. **The first word prints a word late**, by design.
4. **`DecisionLogOrderTests` gaps check** is red as at HEAD, for HM-DEC-166, 182, 189, 220 and 222. The order check passes.
5. **Unit 518 runs next**, as the order says.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one (item 1). Waiting on the owner. The gate table and the probe are in `TheShapePicksTheSenderTests`, and no change sits in the climb.
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader or the gate.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- Unit 519's ask, whether a better shape may take the terminal before the printed one pauses, was answered by the order (option A) and is dropped.
