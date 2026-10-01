```
UNIT: 521 - partial (task 2 landed; task 1 measured, no engine change shipped) - 2026-10-01
UNIT GOAL: a mark's pitch is where its own keying is, and a light says hold still
NUMBER: the bin-grid sweep reads 65 of 65 at every pitch at HEAD; the light is green 2.3 s before the first letter
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSequenceShape.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 521 and HM-DEC-225 were free.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, ticked no box, and added no ruling to either plan.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**Task 1: measured, and no engine change ships.** Commit `a3029144`.
- **The bin-grid sweep, the order's first test.** `AStationReadsTheSameBetweenBinCentres` sends a clean sender at 600, 606, 612, 618 and 625 Hz, at 24 dB and 12 dB, and as a fist scattered by a fifth at 16 dB. **At HEAD every pitch reads whole with all 65 marks.** It was never red.
  - A tone half a bin off (612 Hz) just has its marks split between the 600 and 625 bins, and the reader's one-bin tolerance holds them.
  - **The owner's one-click-off reading is not reproduced on the bench.** Something on the radio side the bench doesn't model is the likelier cause.
- **The neighbour case has a second cause under the climb.**
  - Two stations 200 Hz apart, stepped every 5 ms, beat in step with the hop. So their cross term reads as keyed power in the bins between them, whatever the window measures.
  - **Tried and reverted:**
    - **Lobe fit, then excess power.** Climbing a parabola fit on the excess power at the window's ±2 bins left the steady carrier's marks at 700–725 Hz.
    - **Pitch over the mark's own samples, climbed.** Measured less the gaps at 5 Hz steps with a long Hann window, this put all **65 of 65** clean marks beside a steady carrier at 625 Hz. The climb then stopped on the long window's sidelobes, so a lone station's shoulder bars scattered.
    - **The same, taking the highest within ±175 Hz.** This fixed lone stations, but it broke the two-station case (`ITNEW■A N0CALL N0CALL K`) and case 2, and the steady case moved again.
  - **A trace found the deeper loss.** A single 180 ms dah at 625 Hz beside a steady 24 dB carrier makes **no candidate at all**. It is lost at its own bin's bars, before any pitch is measured.
  - The engine is back at HEAD.
- **Kept:** the sweep, and the probe's grouping on the 25 Hz grid.
- **What still reads the apex bin is unchanged:** the key-up, edges, narrowness and shape tests, the fit's candidate pitch (517), and the pattern gate's within-one-bin agreement.

**Task 2: the hold-still light.** Commit `4c4bf565`.
- **`CwShapeLight.cs` (new).** Four states and their words:
  - `listening`: dark;
  - `shape forming · n of 5`: amber;
  - `shape found · hold here`: green;
  - `reading`: green.

  The hover reads: *"Green means Hamlet has the shape of a station here. Hold the frequency; the first letters print after one word gap."*
- **`CwPatternGate.cs`.** `Forming(now, 2 s)` gives how many marks the fullest sequence not yet standing holds from the last two seconds. It counts only where those marks already come in **two lengths**, a dah beside a dit.
  - **Why two lengths:** counted on all held marks, loud noise held a four-mark sequence 93% of the time; counted on the last two seconds, it held three or more 94% of the time.
- **`CwEnvelopeDetector.cs`.** The reading gains `ShapeLight` and `ShapeForming`:
  - `reading` while a sequence stands and the printed pitch is set;
  - `shape found` while a sequence stands;
  - `shape forming` while one is forming;
  - `listening` otherwise.
- **`CwHearingViewModel`.** The words, one flag per colour, the hover, and the row's new `shapeLight` field. The two row tests list it.
- **`MainWindow.axaml`.** A 12 px lamp with its words in a fixed 140 px width at the head of the buttons' column, so nothing moves when the words change. It uses the app's slate, amber and green brushes. `BindingHealthTests` passes.
- **Test.** `TheLightSaysHoldStillTests` runs on the live path: detector and reader wired as the app wires them.

**Records:**
- HM-DEC-225 in `DECISIONS.md`, quoting the owner on the light and stating the two findings.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 521 - STEP 12`.
- `PHASE_STATUS` (both copies) names 521.
- Version 1.13.205 to 1.13.206.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278; two 1 ms losses passed alone.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **The light.** Beside the scope on the CW tab:
  - **dark, `listening`:** nothing is forming;
  - **amber, `shape forming · 3 of 5`:** a station's shape is building, with a count;
  - **green, `shape found · hold here`:** the shape is found. **Hold the frequency**; letters follow within about a word;
  - **green, `reading`:** letters are printing.

  In heavy noise it flickers amber at "2 of 5" now and then, and it never goes green on noise.
- **One click either way, on the bench, reads the same.** It already did before this unit, so whatever made a station read worse one click off on your radio isn't the 25 Hz grid. The bench can't see it.
- **A station 200 Hz from a much louder one is still lost.** The cause is narrower now: the two tones beat in step with Hamlet's 5 ms analysis. A fix for it doesn't land here.
- **Nothing about letters changed.**

## 3. What you should see

**The bin-grid sweep, at HEAD and now:**

| pitch | 24 dB | 12 dB | fist, 16 dB |
|---|---|---|---|
| 600 Hz | 65 of 65, shape 0.778, whole | 65, 0.643, whole | 65, 0.609, whole |
| 606 Hz | 65, 0.775, whole | 65, 0.635, whole | 65, 0.593, whole |
| 612 Hz | 65, 0.784, whole | 65, 0.641, whole | 65, 0.628, whole |
| 618 Hz | 65, 0.782, whole | 65, 0.644, whole | 65, 0.613, whole |
| 625 Hz | 65, 0.776, whole | | |

**The neighbour cases, unchanged from HEAD:** clean 12 dB at 625 Hz beside a 24 dB carrier keyed at random.

| carrier | reads |
|---|---|
| 400 Hz away | `CQ CQ DE N0CALL N0CALL K` |
| 200 Hz | `NOAM■HIV5■` (the carrier) |
| 150 Hz | `NOAM■HIV5■` |
| 100 Hz | `NOAM■HEEV■■M` |

Where the clean marks land, at 25 Hz resolution:
- with no carrier, 69 at 625 Hz;
- with it steady, none at 625 Hz;
- with it keyed, 23 or 24 at 625 Hz and 36 or 38 at 825 Hz.

**The light's sequence**, on a clean 20 WPM call at 24 dB that begins at 3 s:

| time | light |
|---|---|
| before 3.3 s | `listening` |
| 3.320 s | `shape forming · 2 of 5` |
| 3.560 s | `shape forming · 3 of 5` |
| 3.680 s | `shape forming · 4 of 5` |
| 4.040 s | `shape found · hold here` |
| 6.380 s | `reading` (the first letter printed at 6.360 s) |
| 19.320 s | `shape found · hold here` (the last mark ended at 19.275 s) |
| 19.480 s | `listening` |

**Loud noise, 30 s:** amber 7.2% of the time, never past "2 of 5", never green.

**Every existing case:** unit 515's filter, units 517, 519 and 520's cases and these tests all print as at HEAD. No printed line changed.

Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

1. **The neighbour fix isn't found.** Two things remain:
   - a dah lost at its own bin's bars beside a steady carrier, before any pitch is measured;
   - the beat in step with the 5 ms hop.

   Pitch over the mark's own samples is the promising direction: it put all 65 marks at 625 Hz in one variant. It needs a unit that keeps it from moving the two-station case.
2. **The one-click-off effect isn't reproduced on the bench.** A capture at the radio would show what changes, but the recording ban (R88) stands.
3. **The light starts amber at "2 of 5", not 1.** A shape needs a dah beside a dit before it is counted.
4. **The light shows on the CW tab only**, beside the scope, as ordered.
5. **`DecisionLogOrderTests` gaps check** is red as at HEAD, for HM-DEC-166, 182, 189, 220 and 222. The order check passes.
6. **Unit 518 runs next**, as the order says.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Still open after this unit (item 1). The sweep, the gate table and the probe are in `TheShapePicksTheSenderTests`, and no change sits in the climb.
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader or the gate.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
