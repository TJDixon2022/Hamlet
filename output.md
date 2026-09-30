## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; R109 was appended to both `PHASE_PLAN.md` copies in the owner's words.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed, and nothing was written to the radio.

**The changes, file by file** (all in `aeb848ec`):
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`, change one: the gaps between letters are the
  sender's own.**
  - **The gap inside a letter** stays as unit 500 left it: the gap dit, with the boundary at √3.
  - **The letter gap** is the mean of the lowest cluster of the sender's gaps between runs. The
    gaps are sorted and walked up from the shortest until two neighbours differ by √(7/3), which
    is half of three-to-seven in log length.
  - **The word boundary** is that letter gap × √(7/3). Farnsworth stretching spreads PARIS's
    nineteen spacing units evenly, so the letter and word gaps still sit at three and seven of one
    stretched unit.
  - **Why the lowest cluster:** a pause longer than a word gap sits above the word cluster and
    cannot pull the letter gap. A split at the widest ratio would let it.
  - **Before the sender has shown three gaps between runs**, the reader keeps 1:3:7 on the gap dit,
    and **a sender is not printed until it has shown them**. Printed earlier, a Farnsworth call's
    first two letters were spaced on the dit and read `C Q`.
  - **The silence span** now follows the measured word boundary.
  - **A sender not yet measured is kept at least 3.66 s**, the slowest Farnsworth word gap (18 WPM
    letters at 5 WPM overall, from PARIS), plus the wait for its next mark. Before this, a 5 WPM
    Farnsworth sender was forgotten between its letters and never made two runs.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`, change two: a lone letter must belong to something
  (R109).**
  - **A one-mark letter prints only when a letter of two marks or more** from the same sender stands
    within twice the word boundary, before or after it.
  - **Another one-mark letter no longer confirms it.**
  - **Three or more one-mark letters in a row are dropped together.**
  - A banked letter has not been printed, so dropping it takes nothing off the screen (R100).
- **`tests/Hamlet.RadioEngine.Tests/Cw/FarnsworthAndLoneLettersTests.cs`, new.** The Farnsworth
  audio is built in the test: each letter at 18 WPM, followed by the gap PARIS gives. One stretched
  unit is (60/overall − 31 × 1.2/18) / 19 s; a letter gap is three units and a word gap seven.
- **Records:**
  - `## UNIT 501 - STEP 12` is in both outcome copies, and both status copies name 501.
  - Version 1.13.187 → 1.13.188.
  - R109 is in both plans.
  - HM-DEC-205, "Farnsworth gaps are the sender's own, and a lone letter must belong to something",
    is in `DECISIONS.md`.
- **Not changed:** the detector, the marks, the narrowness, the edges, unit 496's bin choice, the
  keyed-mark rule, the two-run rule, the scope, the terminal, the layout, the preamp and the
  buttons.

**Watched failing first, at HEAD's reader:**
- The Farnsworth call at 5 WPM overall read **nothing**.
- The Farnsworth call at 10 and 13 read `C Q C Q D N 0 C A L L N 0 C A L L K`.
- Farnsworth `TEST DE W1AW K` at 5 read **nothing**.
- The call followed by `T E T T E` read `CQ CQ DE N0CALL N0CALL K T E T T`, which is the
  owner's symptom.
- `DE DE` read `DE D`.
- `T E T T E` alone already printed nothing at HEAD, because a sender of lone marks never makes two
  runs of two marks and so is never printed. That is why the call-first case was added.

**Found and not repaired: a crash in the detector.**
- `CwEnvelopeDetector.cs` line 1160 walks a mark back to where its tone rose. In the first seconds
  of audio it can step to hop −1, and `Level(-1)` throws `IndexOutOfRangeException`.
- It happened on the call-then-`T E T T E` case with seed 5035. The detector was outside this
  unit's scope, so the case was moved to seed 5036, and the test's remarks say so.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **Run reader, speed and Farnsworth tests: 33 of 33.**
- **The decoder's run-path tests: 9 of 11.** The reds are `AMarkIsTheEnvelopeOverAThresholdTests`'
  two cases, unchanged from before.
- **The app carry-forward line: 276 of 278.** `ThePsk31OfferTests` and
  `TheMainWindowBindsWithoutOneComplaint` failed in 1 ms and passed alone (3 of 3).

## 2. What the owner should expect

1. Rebuild.
2. **W1AW's slow code practice should read as words.** Its letters come at 18 WPM with long spaces
   between them, and the reader now measures those spaces from the sender instead of assuming them
   from the dit.
3. **Strings of `E` and `T` should be gone.** A lone letter now needs a real letter beside it, and
   three lone letters in a row are thrown out.
4. `TEST`, `DE` and every real `E` and `T` inside a word still print.
5. **The first letters of a sender appear a little late.** Nothing prints until the sender has
   shown three gaps between letters; at 5 WPM Farnsworth that is several seconds.
6. **If it still comes out wrong, compare the gap table in section 3** with what you hear.

## 3. What you should see

**Farnsworth, 18 WPM letters, the true gaps beside the measured ones:**

| sent | overall | letter gap, true / measured | word gap, true / measured | before | after |
|---|---|---|---|---|---|
| `CQ CQ DE N0CALL N0CALL K` | 5 WPM | 1568 / 1577 ms | 3660 / 3667 ms | nothing | `CQ CQ DE N0CALL N0CALL K` |
| `CQ CQ DE N0CALL N0CALL K` | 10 WPM | 621 / 628 ms | 1449 / 1455 ms | `C Q C Q D N 0 C A L L N 0 C A L L K` | `CQ CQ DE N0CALL N0CALL K` |
| `CQ CQ DE N0CALL N0CALL K` | 13 WPM | 402 / 410 ms | 939 / 948 ms | `C Q C Q D N 0 C A L L N 0 C A L L K` | `CQ CQ DE N0CALL N0CALL K` |
| `TEST DE W1AW K` | 5 WPM | 1568 / 1576 ms | 3660 / 3668 ms | nothing | `TEST DE W1AW K` |

**Lone letters:**

| case | before | after |
|---|---|---|
| `T E T T E` alone, 18 WPM | nothing | nothing |
| the call, then `T E T T E`, 18 WPM | `CQ CQ DE N0CALL N0CALL K T E T T` | `CQ CQ DE N0CALL N0CALL K` |
| `TEST DE W1AW K`, 18 WPM | `TEST DE W1AW K` | `TEST DE W1AW K` |
| `DE DE`, 18 WPM | `DE D` | `DE DE` |

**The existing cases read exactly as at HEAD:**
- The calls at 5, 10, 18 and 35 WPM read whole.
- The 10-to-20 WPM sender reads whole.
- The clean call, the call with bursts, the two-station case and the stray dit each read
  `CQ CQ DE N0CALL N0CALL K`.
- `TEST DE W1AW K` at 5, 10 and 23 WPM reads whole.
- The lone dit and lone dah print nothing.
- Both noise tests print nothing: 146 marks on thirty seconds of noise, and 115 in the last minute
  of three.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The detector can crash on real audio.** `CwEnvelopeDetector.cs` line 1160 reads the hop before
  hop zero and throws `IndexOutOfRangeException` early in the audio (seed 5035 in the new test). A
  guard there is a one-line detector change and wants the owner's go-ahead, since the detector was
  out of scope.
- **A sender that never shows three gaps between runs is never printed.** For example, a two-letter
  word sent once and nothing else.
- **A sender not yet measured is kept 3.66 s rather than one second** before it is forgotten. The
  noise tests are unchanged by it, but noise gets longer to accumulate.
- **The cluster walk, the three gaps and the forgetting floor are the author's**, derived from
  PARIS and 1:3:7, and overrulable.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests`.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `TheLicenceCardAnswersForTheTabAndNotForMorseAlways`.
  - `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
