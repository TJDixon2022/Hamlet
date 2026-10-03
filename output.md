```
UNIT: 529 - tasks 1 and 2 done; task 3 measured, nothing further changed - 2026-10-02
UNIT GOAL: look at a sender through a window that fits it
NUMBER: the T of BEST's top wobbles 1.5 dB through its own window against 3.4 to 3.7 dB on the grid
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwPatternGate.cs` and `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's one recording. HM-DEC-233 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- R88 stayed lifted for `cw-2026-10-02-200157.wav` alone. No other recording, fixture or telemetry was read.
- Nothing keys, transmits or writes to the radio.

**Task 1: a standing sender through its own window.** Commit `3371d63c`.
- **Measured first** (`TheTopOfADahThroughTwoWindows`):
  - The T of BEST, found on the new envelope, runs 11.570 to 11.775 s; its top was read 20 ms in from each edge.
  - The order's 11.48 to 11.69 s window reaches into the gap before the dah, which alone reads 15 dB of range.
  - On the 650 and 675 Hz bins through the 10 ms window, the top wobbled 3.4 and 3.7 dB top to bottom (0.8 dB standard deviation).
  - Through its own window at 662.5 Hz it wobbled 1.5 dB (0.4).
- **The window, `CwSenderLane`:**
  - It mixes at the sender's pitch, measured on its own samples, and low-passes with a fourth-order Butterworth. It is causal.
  - **The cutoff (my choice, with its reason):** set where the filter's 10 to 90 per cent rise is a quarter of the dit, so a dit's top stays flat over its middle half.
  - On the recording the dit read 74 ms, giving a 19.5 Hz cutoff, a 19.8 ms rise, and a **23.0 ms delay** taken off every mark's times.
  - It retunes at each mark the sender takes.
- **Where it opens:** only on the standing sender the terminal prints.
  - A noise sequence stood in one bench case, and a narrow low-pass turns noise into dit-long humps. Noise never prints, so noise reads as before.
  - The shared bench helper (`ThePatternIsTheGateTests.Read`) never told the detector what it prints; it now does, as the app does.
- **Its marks:**
  - A mark is a stretch over half the sender's amplitude less one flat-top wobble, timed where it crosses half its own top.
  - Each meets the same tests a bin's bar meets: key-up, edges (read over two rises), own height, narrowness, and the shape score, whose flatness term is the flatness test.
  - Narrowness is also read on the mark's own samples, comparing the pitch with 150 Hz either side. This keeps out one of the recording's clicks, which the low-pass stretched into a 56 ms, 7.6 dB hump.
  - The run-and-bar method doesn't fit this envelope. A bar must sit wholly above its neighbouring runs, and the slower rise put the last hops of a rise inside the top's range, so whole dits were refused.
- **The grid and the reader:**
  - The grid, including its fitted rectangles, leaves the sender's pitch alone while the window is open.
  - The reader now judges a silence from what has been called (`CwMarkBatch.LateSeconds`), since the window calls a mark later than a bin does.

**Task 2: a pause is not a word.** Commit `dae23143`.
- **The pause rule:** a gap longer than three of the sender's word gaps is a pause and is left out of its word cluster. The word gap is taken as 7/3 of the sender's own letter centre.
- **Settling:** the clusters settle with equal spreads until each side has shown three gaps. A lone pause has no spread of its own, and before this it drew the boundary to itself.
- **The five-dit floor:**
  - It retires where the word cluster holds three gaps; there the line is the boundary between the sender's own letter and word clusters.
  - Before that, the floor and √(7/3) of the letter centre stand.
- **The line for this sender comes out at 508 ms**, between 430 and 580.
- **Retiring the floor everywhere broke the SKCC straight key** (`S KCC`, `SKCCDE`), so, as the order said, the floor stands until the word cluster is trusted.

**Task 3: measured, no change.**
- The G of GP now reads whole.
- The weak 25-35-25 WPM row now reads whole.
- Both came from task 1. The two faults left are in section 4.

**Records:**
- HM-DEC-233 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 529 - STEP 12`.
- `PHASE_STATUS` (both copies) names 529.
- Version 1.13.213 to 1.13.214.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **Your recording now reads:**
  - sent: `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
  - now: `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP`
  - Every dah is a dah. The 73 after the pause is there. The callsign holds together.
- **What changed:** once Hamlet has a station it's printing, it listens to that station through a window tuned to its exact note and narrowed to its speed. So a dah that looked wobbly stays whole, and a weak signal stands further out of the noise. On the bench, a 20 WPM call 8 dB over the noise through your filter now reads `N0CALL` where it read `N0CEELL`.
- **A long letter gap no longer splits a callsign** once the sender has shown three of its word gaps. Before that, five dits still makes a word, so a very slow-spaced hand can still split a word in its first seconds.
- **Two faults are still on screen:**
  - The 7 before V shows as `M S`.
  - The closing DEWA doesn't print.

## 3. What you should see

**The wobble, the T of BEST's top (11.590 to 11.755 s):**

| how it is read | standard deviation | top to bottom |
|---|---|---|
| 650 Hz bin, 10 ms window (today's path) | 0.79 dB | 3.44 dB |
| 675 Hz bin, 10 ms window | 0.79 dB | 3.69 dB |
| its own window, 662.5 Hz | 0.41 dB | 1.48 dB |

The 7 after the pause, 14.9 to 17.0 s:
- **before:** seven candidates, none of them its own (25–50 ms bits at 550 to 800 Hz);
- **after:** all five of its elements, at 211, 213, 79, 78 and 77 ms, 662.9 Hz.

**The recording's text:**

| | text |
|---|---|
| sent | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| before 529 | `F ER C H AT<BT> BESEMSV E Y <SK> KC4 Z MPDEWA` |
| after task 1 | `F ER C H AT<BT> BEST7V 73 <SK> KC4ZGP` |
| after task 2 (now) | `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP` |

Task 2 drew the 7's inner line differently, so the 7 before V reads `M S` again.

**The strength table, 20 WPM through the 500 Hz filter, 1 dB AGC:**

| dB | before | now |
|---|---|---|
| 8 | `CTU NIG DE N0CEELL N0CALL K` | `CTU NIG DE N0CALL N0CALL K` |
| 10 | `CTU CQ DE N0CALL N0CALL K` | same |
| 12, 16, 24 | whole | whole |

The opening `CTU` comes before the window opens; it's the grid's.

**The between-bin cases** at 612.5, 637.5 and 662.5 Hz: whole, 70 stood, as before.

**The existing cases:**
- **Noise:** 30 s and 3 minutes, nothing printed, as before.
- **Changed for the better:** the weak 25-35-25 WPM row at 1 dB now reads whole (was `… AND I SFE`).
- **Changed in a red diagnostic:** in the random carrier's gate table, the key-up-off row reads `NOAM TTN I T AEEN E` (was `… AE E N`).
- **Everything else** in sets a, b1, b3, b4 and b5 reads as before.
- **The known reds:**
  - the call at 8 dB;
  - Farnsworth and fast at 10 dB;
  - a letter from noise (blocks);
  - the random carrier at 725 and 775 Hz;
  - the clean sender at the edge;
  - the radio strength table at 8 and 10 dB;
  - `TheOwnersRecordingReads`.

## 4. What's blocking us

1. **DEWA is lost.**
   - The reader lets a sender go when its shape falls under 0.1. This hand's letter gaps (2 to 5.7 dits, with no jump to its words) score nought for tightness.
   - Scoring them on task 2's split brings DEWA back, but the straight key then reads `SKCCDE` in four of six conditions, because the first pick moves.
   - Ruling asked: *the shape's letter-gap tightness is scored on the sender's own letter cluster, split as the word line is, and the straight key's first pick is fixed separately.* Rejected: keeping it as it is, which loses a sender's closing words whenever its letter gaps spread past a hand's widest.
2. **The 7 before V reads `M S`.** Its 120 ms inner gap is over the reader's letter line of 100 to 114 ms, which leans toward the tight element gaps (70 to 82 ms). The cause is in the reader, not the envelope.
3. **`F ER` at the start:** the first gaps are judged before any letter cluster exists.
4. **Bench helpers that don't wire the printed pitch** (`TheLetterGapHoldsTests`, `WhichGateTurnsAwayW1awTests`, and others) never open the window, so they test the grid alone.
5. **Pre-existing app reds** outside the carry-forward line were not touched:
   - the scope's tone-line tests;
   - two `centre` spellings;
   - `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 529, 2026-10-02:** the shape's letter-gap tightness (item 1 above). No change for it sits in the tree.
- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
