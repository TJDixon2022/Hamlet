## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked, and no ruling was added to `PHASE_PLAN.md`.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed, and nothing was written to the radio.

**What the measurement found. It is not quite what the instruction expected.**
- **The dit was right.** The reader measured the sender's dit correctly at every speed; section 3
  has the figures.
- **The boundaries were right.** They were already Morse's own 1:3:7 split at the geometric means:
  √3 ≈ 1.73 dits between a gap inside a letter and one between letters, and √21 ≈ 4.58 dits
  between letters and words.
- **The fault was two other pieces of the reader's gap arithmetic:**
  1. **A sender was forgotten too soon.** An unqualified sender was dropped, and a printed one
     released, after the detector's one-second hold.
     - A word gap at 5 WPM is 1.68 s, so the call's first words were forgotten before the sender
       made its two runs.
     - At 10 WPM the next mark reaches the reader only once it ends, 1.2 s after the last T of TEST,
       so the sender was forgotten and that T went with it.
  2. **Gaps were counted in the marks' dit.** The detector reads a mark short and the gap after it
     long.
     - At 35 WPM the character boundary built on the marks came out at 48 ms, inside the 50 ms gaps
       inside a C, which read as K and E.
- **The E and T splitting you saw was not reproduced** on standard-timing synthetic audio at any
  speed. See section 4.

**The changes, file by file** (all in `82077211`):
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`:**
  - **How long a sender may be silent** is now the longer of two spans: the detector's one-second
    hold, or twice its word-gap boundary (about nine dits) plus its longest mark and the calling
    lag. Nine dits is the same span a lone letter already waits for its confirmation.
  - **A gap is counted in a "gap dit".** That is the marks' dit plus the median of how much longer
    each gap inside a letter ran than that dit. The detector's smear is fixed, measured at 10 to
    16 ms from 5 to 35 WPM, while the dit follows the sender. So this unit follows a change of speed
    as fast as the marks do.
  - **The three boundaries are unchanged:** 1.73 and 4.58 of that unit.
  - Two internal read-only figures were added for the tests' report.
- **`tests/Hamlet.RadioEngine.Tests/Cw/TheGapsBelongToTheSendersOwnDitTests.cs`, new.** Synthetic
  audio made in the test:
  - The call at 5, 10, 18 and 35 WPM.
  - `TEST DE W1AW K` at 5 and 10 WPM.
  - One sender stepping from 10 to 20 WPM.
  - Each case prints the true dit beside both measured dits.
- **Records:**
  - `## UNIT 500 - STEP 12` is in both outcome copies, and both status copies name 500.
  - Version 1.13.186 → 1.13.187.
  - HM-DEC-204, "A sender's gaps are its own dit times one, three and seven", is in
    `DECISIONS.md`.
- **Not changed:** the detector, the marks, the narrowness, the edges, unit 496's bin choice, the
  banking's rule and window, the keyed-mark rule, the two-run rule, the scope, the terminal, the
  layout, the preamp and the buttons.

**Before the dit is known.** A sender is known once it has shown dits and dahs. Until then:
- The unit is the gap inside a letter if the sender has shown one, else a third of its shortest gap
  between runs, else its median mark.
- **Nothing is printed on that estimate.** A sender prints only after two keyed runs with two kinds
  of mark, and at that moment all its unprinted runs are cut again at the unit it has then shown
  (unit 498's re-split).
- **The cost:** the first letter or two show up late, together, once the sender qualifies. They are
  not cut wrongly.

**Following a sender who changes speed.** The marks' dit is read over the sender's last 40 marks.
The gap offset is read over its last 40 gaps inside letters, and it is fixed by the detector rather
than by the speed. So the unit follows the marks and turns over within about twenty marks. On the
10-to-20 WPM case it ended at 65 ms against a true 60.

**Watched failing first.** Four of the seven new cases were red before the change:
- The call at 5 WPM.
- The call at 35 WPM.
- `TEST DE W1AW K` at 5 and 10 WPM.

The first version of the fix, the gaps' own median, then failed the 10-to-20 WPM case, reading
`-B■K` for the answer. That is why the unit is the marks' dit plus an offset.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **Run reader and speed tests: 25 of 25.** That is unit 490 to 498's eighteen cases plus the seven
  new ones.
- **The decoder's run-path tests: 9 of 11.** The reds are `AMarkIsTheEnvelopeOverAThresholdTests`'
  two cases, unchanged from before.
- **The app carry-forward line: 277 of 278.** `TheFavoritesAreUnderTheGreenZoneTests` failed in
  1 ms and passed alone (3 of 3).

## 2. What the owner should expect

1. Rebuild.
2. **W1AW's slow code practice should read as words**, not strings of `E` and `T`. A slow sender is
   now kept through its long word gaps, and the start of each transmission is no longer lost.
3. **Fast sending is unaffected.** 18 and 35 WPM read whole, and 35 now reads its first C
   correctly.
4. **If it still comes out as single letters, look at the dit the reader measures.** Section 3's
   table names it; this unit could not reproduce single-letter output on standard timing, and
   section 4 says what that leaves.

## 3. What you should see

| speed | true dit | measured on the marks | gap dit | before | after |
|---|---|---|---|---|---|
| 5 WPM | 240 ms | 235 ms | 246 ms | `CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 10 WPM | 120 ms | 115 ms | 125 ms | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 18 WPM | 67 ms | 59 ms | 75 ms | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 35 WPM | 34 ms | 28 ms | 42 ms | `KEQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| `TEST DE W1AW K`, 5 WPM | 240 ms | 235 ms | 245 ms | `W1AW K` | `TEST DE W1AW K` |
| `TEST DE W1AW K`, 10 WPM | 120 ms | 115 ms | 125 ms | `TES DE W1AW K` | `TEST DE W1AW K` |
| 10 then 20 WPM | 120 then 60 ms | 55 ms at the end | 65 ms at the end | whole | `CQ CQ DE N0CALL N0CALL K TEST DE W1AW K` |

**The existing cases read exactly as before:**
- The clean call, the call with bursts, the two-station case and the stray dit after the call each
  read `CQ CQ DE N0CALL N0CALL K`.
- The lone dit and lone dah print nothing.
- `TEST DE W1AW K` at 23 WPM reads whole.
- Both noise tests print nothing: 146 marks on thirty seconds of noise, and 115 in the last minute
  of three.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **Your terminal's run of single `E`s and `T`s was not reproduced.** At every speed with standard
  timing, the reader cut letters correctly before this change; what it did was lose the start. If
  the same output returns, the gap dit in the table is the first thing to compare against the
  sender.
- **Farnsworth-style sending is not covered by these cases.** That is characters sent faster than
  the spacing between them. I have not checked whether W1AW's slow practice is sent that way. If it
  is, it needs its own synthetic case.
- **The first letters of a slow sender still appear late**, all at once, when the sender qualifies.
  At 5 WPM that is several seconds.
- **The silence span and the gap unit are the author's**, derived from 1:3:7 and the detector's own
  smear, and overrulable.
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
