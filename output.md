## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `docs\carry-forward-tests.txt`, `docs\cw-scoreboard.md` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 13:45:54 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. No recording added; nothing keys or transmits. Version 1.13.247 to **1.13.248**. Ruling **HM-DEC-267**, the number the order gave. Nothing was recorded under §12.1.

**Task 1, the dispatcher-loop failure found and gone** (`09205f24`).
- **What throws it:** Avalonia 11.3.0's `Dispatcher.ResetForUnitTests`, read in its source. As each headless test starts, it runs every leftover job on the dispatcher and queues any timer that has come due after each one. If that takes more than five seconds it throws `You've caused dispatcher loop` on whichever test is starting.
- **How it was found:** a temporary assembly hook logged each test's start and end and the jobs left in the dispatcher's queue, the queue read for diagnosis only, and not committed.
- **Leak 1, the view model's timers:** every `MainWindowViewModel` a test builds starts its timers, because the `DispatcherTimer` constructor that takes a handler starts it. Nothing stopped them: scope at 50 ms, dwell and decode at 250 ms, age at 1 s.
  - Plain `[Fact]` tests build view models off the headless dispatcher, so their ticks queued there unrun.
  - After `ThePsk31TelemetryTests.NoiseProducesCandidatesThatNeverCrossAndNoCarriers`, the queue held `DispatcherTimer.FireTick` only, up to 200 of them. The next two headless tests, `TheFavoritesAreUnderTheGreenZoneTests`, each spent five seconds failing. The gap in the trace was 13 s.
- **Leak 2, the view model's settled snapshot:** posted after a five-second `Task.Delay` that nothing could cancel, so it landed on whichever test's dispatcher was current. Up to 97 `WriteSettledSnapshot` jobs were queued at once.
  - With the timers fixed and this not yet fixed, one run in five still failed (run 4: 275 of 278).
- **The fix, at the source:**
  - `src\Hamlet.App\Controls\UiTimers.cs` records every timer the app makes: the view model's nine, the six controls' and the two windows'. It runs the snapshot's delay as a cancellable post.
  - An assembly hook, `tests\Hamlet.App.Tests\TimersStopWithTheirTest.cs`, stops and cancels them all when each test ends. On a headless test that is on the dispatcher, just before Avalonia's own drain.
  - Nothing is retried and nothing is run one by one.
  - In the app, the only change is that the snapshot's delay goes through `UiTimers.PostAfter`. Nothing in the app calls `StopAll`.
- **What it leaves:** Avalonia's own one-shot press-and-hold timers, 300 ms, which fire once each, and a few transmit continuations that post once.
- **Proved:** the app carry-forward line, five runs in a row, 278 of 278 each. Runs one and two came before a comment-only change to the hook.

**Task 2, the engine line run whole once:** completed under its 480 s timeout, in 386 s, at 153 of 154.
- **The one red:** `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`. It divides the whole process's CPU time by the audio's length while other test classes run beside it: 1.579 of real time there, 0.079 alone, where it passes.
- **A finding only;** not fixed.

**Records:**
- `PHASE_OUTCOME.md`, both copies: `## UNIT 563 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 563.
- `Directory.Build.props`: 1.13.248.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-267.
- `docs\cw-scoreboard.md`: a 563 row in each table, unchanged.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** both boards as at HEAD. **Decision-log and voice tests:** 7 of 7.

## 2. What the owner should expect

**Nothing about reading changed.** Rebuild if you like, but Hamlet reads exactly as before:
- **297** on your twelve plus KM3STU's three, **234** without his three.
- **2111** on W1AW.
- Every stretch is letter for letter the same.

**What was leaking:** every time a test built the app's main screen without a window, its timers kept ticking after the test ended, along with a snapshot it writes five seconds after it opens. Those leftovers piled up behind tests that don't run a screen, and the next test that did had to work through all of them before it could start. Past five seconds Avalonia gave up, and that test failed. That's why it was a different test each time.

**Now each test stops its own timers when it ends:**
- The app's test line ran clean five times in a row.
- It's about 20 seconds faster, and the reports won't need to explain away a red any more.
- In the app itself, the screen's timers run as before.

**The engine's test line, run whole for the first time in weeks,** finishes in about six and a half minutes. It has one red of the same kind: a speed check on the RSID detector that counts the other tests' work as its own. It passes alone.

Pushed to `main`.

## 3. What you should see

**The app carry-forward line, five runs in a row** (with the fix):

| run | count | time |
|---|---|---|
| 1 | 278 of 278 | 2 m 14 s |
| 2 | 278 of 278 | 2 m 14 s |
| 3 | 278 of 278 | 2 m 11 s |
| 4 | 278 of 278 | 2 m 8 s |
| 5 | 278 of 278 | 2 m 10 s |

**Before the fix, and with half of it:**
- At HEAD: 276 of 278 and 277 of 278 last unit, each about 2 m 26 s to 2 m 32 s.
- With the timers stopped and the snapshot not yet cancellable: 278, 278, 278, 275, 278.

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 563 task 1 | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 563 task 1 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

Every stretch reads as at HEAD. Noise prints nothing, the carrier at seed 5195 as before, and the first recording whole.

**The engine line, run whole once:**

| | |
|---|---|
| completed | yes, in 386 s, under its 480 s timeout |
| count | 153 of 154 |
| red | `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`: `psk31-four-signals.wav` cpu/audio 1.579 (wall 6.09 s for 38.59 s of audio) |
| alone | passes: cpu/audio 0.079 |

## 4. What's blocking us

- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** reads the whole process's CPU while other test classes run beside it, so the engine line run whole is red on it. It needs its own CPU measure or a collection of its own; not fixed here.
- **Two reply tests stay red at HEAD:** `AReplyIsReadFromItsFirstLetterTests`' synthetic QSO (`TAW`) and `144020` (`IAN`).

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
