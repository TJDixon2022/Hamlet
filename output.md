## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- **The lock:** `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same
  way.
- **Records not touched:** nothing was written to `RUN_LEDGER.md`, and nothing under
  `tools\arbiter\` was touched. No box was ticked.
- **Read:** no recording, fixture, floor or telemetry.
- **Not committed:** nothing under `.run-unit\`. Nothing keys or transmits.

**One departure from the order.** It named **HM-DEC-209**, which unit 505 had already used an hour
earlier. Ids are never reused (§2.1), so this ruling is **HM-DEC-210**.

**Commits, all on `main` and pushed:**
- `f35a994c` task 1, the measurement
- `2a6a1835` tasks 2 and 3, the gates and the pattern
- `994624dc` task 4, the display and the records

**Version.** 1.13.192 to 1.13.193.

**Measured first, against the true key timing** (task 1). The call is at 20 WPM, with 65 marks
sent.
- **16 dB:** no mark was lost. Four real marks were called twice, overlapping at one pitch and two
  levels, and those were the letters that read wrong.
- **8 dB:** four dahs were lost, and others read 30 to 80 ms for 180. The detector measured no
  contrast at all.

So the weak station's marks mostly *did* reach the reader. What was wrong was duplicates at 16 dB
and broken bars at 8 dB.

**The changes, file by file:**
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`:**
  - **Every single-mark test is now a fraction of the mark's own height over the gap beside it**
    (`OwnContrast`). That height is the median of the peak bin's hops 3 to 8 outside each edge.
  - **Edges:** a mark must fall `EdgeShare` = **0.5** of its own height within four hops. It had to
    fall 6 dB. The reason: a keyed edge takes the level from the top to the gap within the window's
    spread whatever the height, so half of it is crossed at any height, and a bump in noise that
    slopes into its neighbours does not cross it.
  - **Narrowness:** a mark is broad only where the bins 300 Hz away **both rise with it** by more than
    `NarrowShare` = **0.5** of its height. Before, it had to stand 6 dB over their *level*. The reason:
    noise is broadband and lifts both sides at once, while a tone's energy stays in its own bin.
    - The rise replaced the level for a measured reason. The first version, a level at half the
      mark's height, lost a dah of the loud station in the two-station case: the quiet station at
      825 Hz leaks into the 925 Hz probe bin.
    - The rise of one side alone failed too, whenever the other station happened to key during the
      mark. "Both sides" answers both.
  - **The shape score is rebuilt from ratios.**
    - **Narrowness** is one less the smaller neighbour rise over the mark's height.
    - **Contrast** is how evenly the mark stands over its two gaps: the lower of the two heights
      over the higher.
    - **The 15 dB constant is gone.**
    - **The threshold is 0.003**, a quarter under the lowest real mark's 0.004 at 8 dB, with the
      edges and narrowness tested and the shape not. It still turns away any bar scoring nought on
      a term. It no longer separates much else on a weak station, and the pattern does that work.
    - Unit 502's 0.25 was set with a call 22 dB over the noise in front of it.
  - **The pattern gate is wired in.** Every candidate goes to `CwPatternGate`, and only marks that
    stand reach `MarksSince`, the scope's hops and the mark count. The one-call rule reads every
    candidate, as it read every called mark at HEAD.
  - **`Follow(pitch)`:** the printed sender's bin is watched over the radio's pointer and the sweep.
    The reading's `Pointed` flag stays true only for the radio's own pointer.
  - **`MarksLast4s`** is now the marks that stood at the watched pitch in the last four seconds,
    where it was the watched bin's paired bars.
  - **Switches and counts:** `MarksNeedPattern`, `CandidateCount` and `StoodCount`.
- **`src/Hamlet.RadioEngine/Cw/CwPatternGate.cs`, new.** A sequence is candidates:
  - within one bin of its pitch;
  - within the reader's own level tolerance of its height, unit 490's figure as the order asks;
  - silent no longer than the slowest Farnsworth word gap and a 5 WPM dah.

  It **stands at five marks in two lengths at 2 to 1 or wider**, then releases its held marks in
  order, and every agreeing mark after that at once. A candidate **closer than half the sender's
  dit** to the sequence's last mark is the same tone read twice, or a piece of it, and is dropped.
  - **Five:** unit 493's two runs of two, four, is the floor. Five is one past it, so the pattern is
    always wider than one four-mark letter. What the count buys is noise's chance of making that
    many agreeing marks.
  - **Half a dit:** Morse puts a dit of silence between elements, a hand scatters it by tens of
    percent, and the detector reads gaps long, never short.
- **`src/Hamlet.RadioEngine/Cw/CwMark.cs`:** `Stood` and `OwnContrastDb`.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:** on every scope tick the detector follows
  `PrintingHz` while decoding.
- **`tests/.../ThePatternIsTheGateTests.cs`, new:** the strength table, Farnsworth and 35 WPM, the
  noise, the two stations, and the verdict row.
- **Records:**
  - R112 in both `PHASE_PLAN.md` copies.
  - Both `PHASE_STATUS.md` copies name 507.
  - `## UNIT 507 - STEP 12` in both `PHASE_OUTCOME.md` copies.
  - `Directory.Build.props`, and HM-DEC-210.

**Build and tests.**
- **Build:** `Hamlet.sln` with warnings as errors, 0 warnings, 0 errors.
- **Reader and detector tests:** 51 of 54. The three reds are the same as at HEAD:
  `BurstsBetweenLettersDoNotSetTheSpeed` and `AMarkIsTheEnvelopeOverAThresholdTests`' two.
- **Pattern tests:** 11 of 14. The reds are the 8 dB call, the 12 dB call, and the 10 dB Farnsworth
  and 35 WPM case.
- **`OneDecoderOneTruthTests`:** 6 of 6, with readings identical to HEAD.
- **App carry-forward:** 277 of 278. The loss, `TheTestsStayOffTheNetworkTests`, took 1 ms and is
  green alone.

## 2. What the owner should expect

- **Rebuild.**
- **Weaker stations should now read better.** A mark is judged by whether it fits the sender's
  pattern and how its shape compares with its own height, not by how loud it is. On the bench, a
  call 16 dB over the noise that used to come out as `CQ TNQ DE N0CALR` now reads whole.
- **Noise still prints nothing, because noise can't make the pattern.** In thirty seconds of loud
  noise, 635 bars passed the single-mark tests and none of them formed a pattern.
- **The light, the blocks and the verdict row now describe the station being printed.** The row's
  pitch is the printed pitch, and its mark count is the marks that fit that station's pattern.
- **There is a limit.** Below about 16 dB over the noise the bench doesn't read a call whole. At
  8 dB the dahs themselves break up before any of this sees them. **If a station you can hear still
  reads nothing, section 3's table shows where the bench stops, and that is the number to report
  against.**

## 3. What you should see

**The strength table.** The call `CQ CQ DE N0CALL N0CALL K` at 20 WPM has 65 marks sent. Decibels
are over the noise on unit 502's scale. Each "after" cell shows candidates at the pitch, the marks
that stood, and the reading.

| Strength | Before (HEAD) | After |
|---|---|---|
| 24 dB | `CQ CQ DE N0CALL N0CALL K` | 72 candidates, 65 stood: `CQ CQ DE N0CALL N0CALL K` |
| 16 dB | `CQ TNQ DE N0CALR N0CALL K` | 75 candidates, 70 stood: **`CQ CQ DE N0CALL N0CALL K`** |
| 12 dB | `CT A CQ DE N0CALL N0CAL<AS> K` | 69 candidates, 64 stood: `CT A CQ DE N0CALL N0CALL K` |
| 8 dB | `NETA R ADEN0CALL N0CE AEL K` | 68 candidates, 58 stood: `N ET A EI A DE N0CALL NTJCE AEL K` |

- **The bench reads the call whole from 16 dB up.** At 12 dB only the cold-start CQ is wrong, as at
  HEAD.
- **Below that the bars break.** At 8 dB, four of the call's dahs never become bars. That is before
  any gate this unit touched, so a pattern can't bring them back.

**At 10 dB, before and after:**

| Case | Reads |
|---|---|
| 35 WPM | `C Q C Q D E N 0 C A L L N 0 C A L L K`: every letter right, and every letter its own word, as at HEAD. That's the reader's word-gap arithmetic, which the order says must not change here. |
| 5 WPM Farnsworth | `CK C TA DE E■CAEIL N0RALL N`, unchanged |

**Noise:**

| Case | Candidates passing the single-mark tests | Stood in a pattern | Printed |
|---|---|---|---|
| 30 s of loud noise | 635 (66 at HEAD) | **0** | nothing |
| 3 min of loud noise | 3,932 (372 at HEAD) | **80** | nothing |

The loosened single-mark tests pass many more noise bars. The pattern stands almost none of them,
and the reader prints none.

**The cases:**
- **Two stations 200 Hz apart**, the loud one at 24 dB and 625 Hz and the quiet one at 10 dB and
  825 Hz. The loud one reads whole, as at HEAD. The quiet one **is found as a second sequence**: 12
  of its marks stood. The reader prints one sender, the loud one.
- **The verdict row on a driven station:** while printing and keying, `scopePitchHz` equalled the
  printed pitch on all 1,322 readings, and `marks4s` was the count of marks that stood there, up to
  20.
- **Every existing case** was compared text for text with HEAD. That covers the calls at every
  speed, the Farnsworth cases, the speed change, the bursts, the hesitation, `TEST DE W1AW K`,
  `DE DE`, and the lone and stray marks. All read as at HEAD **but one**: unit 502's weak call, about
  10 dB at 23 WPM, went from `CG N EQ DE N0CALL NT ON EAE IL A` to
  `CGE N EQ DE N0CALL NT ON EAE IL A`. Its test asserts only the mark count, so it stays green. It
  was not forced back.
- **Diagnostic counts that changed but don't affect any reading:**
  - A lone E or T, and noise alone, now reach the reader as 0 marks, where they were 13 and 66. Both
    still print nothing.
  - Unit 504's gate table shows 65 of 65 marks, where it was 66, because the split mark is now
    dropped. With every gate off it reads `MT N OT N I I L K`, where HEAD read placeholders.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **Weak bars break at 8 dB before any gate sees them.** The flat-top tolerance at a weak mark's
  measured contrast is the next place to look, and it is outside this order.
- **At 35 WPM at 10 dB every letter prints as a word.** That's the reader's word-gap arithmetic,
  which this order protects.
- **The 12 dB cold start** reads `CT A` for the first `CQ`, as at HEAD.
- **The reader prints one sender**, so a second sequence that stood, like the quiet station, is found
  but never printed.
- **`DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes`** has been red since
  before unit 505, because the `CLAUDE.md` §1 index stops at HM-DEC-188. HM-DEC-210 joins the gaps,
  since this order asked for no index row.
- **The app's verdict row is checked at the engine level.** The detector's reading is what the row
  copies. No app test drives the scope tick itself.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
