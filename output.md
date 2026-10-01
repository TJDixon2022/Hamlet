```
UNIT: 514 - complete at task 3 of 3; task 2 measured, no code changed - 2026-09-30
UNIT GOAL: any pitch in the filter
NUMBER: pitches read whole through a 500 Hz filter on 600: 4 of 5 (700 Hz reads nothing, before and after)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, the root is `C:\Source\HamLet`, and `PROJECT_CARD.md` says Hamlet. Nothing in this report is evidence about the radio. Unit 514 and HM-DEC-218 were free.

SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked, and no ruling was added. Nothing keys, transmits or writes to the radio. Scratch probes are under `.run-unit\` and not committed. **The engine source is unchanged in this unit.**

**Task 1: the wire** (`97210f1a`).
- `ViewModels/MainWindowViewModel.cs`: new `FollowPitch(printingHz, marks, meter)`, called from `OnScopeTick`. It returns, in order of preference:
  - **the printed sender's pitch**, but only while a mark stood at that pitch within the detector's own one-second hold (`CwEnvelopeDetector.HoldSeconds`), so a pitch the reader still holds but no longer hears is not followed;
  - **else the keying meter's pitch**, where the meter's own bars say a station: verdict keying, score at least `KeyingScore` (0.10), median element within `SlowestChatterMs` to `LongestElementMs` (25 to 250 ms);
  - **else none**, so the radio's pointer or the sweep decides, as before.
- `ViewModels/TheDetectorFollowsTheMeterTests.cs`, on the view model's own `OnScopeTick`, with a real detector fed silence:
  - red first: a meter reading 500 Hz, 49 ms, score 0.28 left the detector on 600;
  - now it moves to 500 in one tick;
  - a meter reading of score 0.06 over a 4 ms median does not move it.

**Task 2: narrowness** (`c6ab5635`). Measured; **no code changed.**
- `Cw/NarrownessReadsTheFiltersBandTests.cs` runs the call at 20 WPM, 24 dB, through this test's stand-in for the radio's filter (two band-pass sections at 600 Hz, Q 1.2, about 500 Hz wide), with the detector told that passband as the app tells it. Results are at the top of section 3.
- **Narrowness is not the fault.** It turns away 0 to 3 of about 70 candidates at any of the five pitches. And with the passband from the rig, the detector's bins already end at the passband, so a probe outside it is already skipped. The order's rule is what the code already does, and changing the probe would move nothing that is broken.
- **What is broken is pairing.**
  - At **700 Hz** through the filter, all 65 marks stand and **none is keyed**, so the reader, which prints only a sender whose runs carry a keyed mark, prints nothing. Probed: 725 Hz is the same, keyed 1, nothing printed; 650 keys only 6 but reads.
  - The same 700 Hz station without the filter keys 64 of 65 and reads whole. Giving the detector the passband or not makes no difference; the filter does.
  - At 9 s into the filtered 700 Hz call, the bins at 600 and 625 Hz pair their bars and key, while 650 to 800 form a bar or two and never pair. A single dit and dah at 700 through the filter look right: flat tops within 0.2 dB, equal levels, a clean fall.
  - I could not find from outside the detector's private pairing code why they don't pair. It is the first item in section 4.
- The test is committed red at 700 Hz, as the measurement.

**Task 3: the filter's edge** (`7540a4d3`).
- `ViewModels/CwHearingViewModel.cs`, on `CwScopeFrame`:
  - `EdgeLine` reads *near the filter's edge - the radio is attenuating it* while the printed sender's pitch sits within `EdgeHz` (75) of the passband edge the rig state gives (`PassbandFromRig`).
  - `EdgeTip` names the filter's width and centre, says widening it or retuning would help, and says Hamlet changes nothing on the radio.
  - With no filter from the rig, or nobody printed, both are empty.
- `Views/MainWindow.axaml`: an amber `CwFilterEdge` text beside the tone line, on the same line so nothing moves when it appears, visible only while it has words, with the hover.
- `ViewModels/TheFiltersEdgeIsNamedTests.cs`: red first on a stub. Now 380 Hz in a 500 Hz filter on 600 is named with its hover, and 600, an unknown filter, and nobody printed are not.

**Build and tests.**
- Build `Hamlet.sln` with warnings as errors: RC=0.
- App carry-forward line: 277 of 278. The one loss, `TheStopIsAlwaysOnScreenTests` in 1 ms to *"You've caused dispatcher loop"*, passes 5 of 5 alone.
- `BindingHealthTests` 1 of 1.
- The closed hover lists 3 of 3.
- The layout tests 13 of 13.
- The app reading cases 16 of 16.

**Records.**
- Version 1.13.199 to 1.13.200.
- `PHASE_OUTCOME.md` (both copies): `## UNIT 514 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 514.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md` HM-DEC-218, in full below. Its headline says what was done; the order's headline claimed a narrowness change that wasn't made.

> **Any pitch in the filter: the detector follows the meter when the reader has nobody, and the app says when a station is at the edge; narrowness was measured and is not the fault.** The order named the headline *... narrowness reads the band the filter gives it ...*; that change was not made, so the headline says what was. Tim, 2026-09-30: *"It seems like CW out in the wild has varieties of pitch, and you just aren't getting any of that."*
>
> **The rows.** 22:59 on 7.0249: the meter read a station at 500 Hz, a 49 ms dit, score 0.28, while the detector said no keying and was told to follow 600, the pitch the reader last printed. 23:02, W1AW on 7.0475: a 41.9 dB swing, no marks, four rows in five printing nobody. Across the week stations at 350, 500, 550 and 700 Hz reached the reader with the detector on another bin, and 600 Hz stations read.
>
> **The wire.** The detector follows the printed sender only while a mark stood at its pitch within its own one-second hold; else the keying meter's pitch where its bars say a station - keying, a score of at least 0.10, a median element of 25 to 250 ms; else nothing, so the radio's pointer or the sweep decides.
>
> **Narrowness, measured.** Through a 500 Hz filter on 600, narrowness turns away 0 to 3 of about 70 candidates at 425, 500, 600, 700 and 775 Hz, and the detector's bins already end at the passband, so a probe outside it is already not read. It is not the fault and was not changed. What fails is pairing: at 700 and 725 Hz the bins at and above the tone form bars and never pair them, so no mark is keyed and the reader prints nothing, while the same station unfiltered keys 64 of 65. That is the next unit's question.
>
> **The edge.** Beside the tone line, while the printed station sits within 75 Hz of the passband's edge as the rig state gives it, the tab says *near the filter's edge - the radio is attenuating it*, with a hover naming the filter's width and centre and that widening it or retuning would help. Nothing is written to the radio.

## 2. What the owner should expect

- **Rebuild.**
- **When the reader has nobody,** the detector now goes where the keying meter hears a station, instead of sitting on the last pitch it printed. The 22:59 case moves the detector from 600 to 500 in one tick.
- **A station at the filter's edge is named as such:** *near the filter's edge - the radio is attenuating it*, beside the tone line. Hover over it for the filter's width and centre and what would help. Hamlet changes nothing on the radio.
- **A station anywhere inside the filter is not yet always read.** On the bench, through a stand-in for your 500 Hz filter on 600:
  - 425, 500, 600 and 775 Hz read whole;
  - **700 and 725 Hz read nothing**.

  The order blamed narrowness. It isn't narrowness, so this unit changed nothing there. At those pitches the detector's bars at the station never pair into keying, and the reader won't print a station that never keys. That is the next thing to fix.
- **If a station inside the filter still reads nothing,** check the pitch table at the top of section 3. A station near 700 Hz reading nothing matches what the bench shows.
- **Still red, as before this unit:**
  - **Seven verdict-row tests** in `TheVerdictCarriesTheScopeTests` and `TheOwnersVerdictIsARowTests`. They expect the row without the `mixingHz` field that unit 488 added on 2026-09-28. They are not on the carry-forward line and were not touched.
  - `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes`, the index gaps.
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`, unit 450's two "centre"s.
  - Unit 507's three strength reds.
  - New and deliberate: `NarrownessReadsTheFiltersBandTests` at 700 Hz.

## 3. What you should see

**The pitch table.** The call at 20 WPM, 24 dB over the noise, through two band-pass sections at 600 Hz with Q 1.2, the detector told the 350–850 Hz passband. Before and after are the same, since no detector code changed.

| Pitch | Narrowness passes (of candidates) | Stood | Keyed | Printed | Reads |
|---|---|---|---|---|---|
| 425 Hz | 68 of 68 | 65 | — | 65 | `CQ CQ DE N0CALL N0CALL K` |
| 500 Hz | 75 of 78 | 65 | — | 65 | whole |
| 600 Hz | 76 of 76 | 65 | 50 | 65 | whole |
| 700 Hz | 69 of 69 | 65 | **0** | **0** | nothing |
| 775 Hz | 71 of 72 | 65 | — | 65 | whole |
| 650 Hz (probe) | — | 65 | 6 | 65 | whole |
| 725 Hz (probe) | — | 65 | 1 | 0 | nothing |
| 700 Hz, no filter (probe) | — | 65 | 64 | 65 | whole |

**The bins at 9 s into the filtered 700 Hz call:**

| Bins | Bars a second | Gaps | Keying |
|---|---|---|---|
| 600, 625 Hz | 4 | 4 / 4 | true |
| 650–800 Hz | 0 to 3 | 0 | false |

Unfiltered, every bin from 600 to 800 Hz pairs 4 or 5 and keys.

**The wire cases:**

| Meter reading, reader printing nobody | Detector before the tick | After the tick |
|---|---|---|
| keying at 500 Hz, 49 ms, score 0.28 | 600 Hz | **500 Hz** (HEAD: 600) |
| at 500 Hz, 4 ms, score 0.06 | 600 Hz | 600 Hz |

**The edge cases:** 380 Hz in a 500 Hz filter on 600 shows the sentence and the hover *"The radio's filter here is 500 Hz wide around 600 Hz, and this station is at 380 Hz, close to its edge, where the radio turns a signal down before Hamlet ever hears it. Widening the filter, or tuning so the station sits nearer 600 Hz, would bring it back up. Hamlet changes nothing on the radio."* 600 Hz, an unknown filter, and nobody printed show nothing.

**The existing cases:** the engine is unchanged, so every engine reading case reads as at HEAD by construction: the strength table, both noise tests (nothing printed), the two stations, the fists, the speed change. The app reading cases pass 16 of 16.

## 4. What's blocking us

Nothing blocks. The items:

1. **Pairing fails off-centre through the filter.** At 700 and 725 Hz the bins at and above the tone form bars and never pair them, so no mark is keyed and the reader prints nothing. The cause is not yet found. A single dit and dah look right through the filter, and the same station unfiltered keys 64 of 65. The next unit should trace the private pairing code (`Evaluate`: the level agreement between bars, the "dropped" check on the gap, and the wander check) on the filtered 700 Hz call.
2. **Narrowness needs no change** while the detector's bins end at the passband, which they do whenever the rig state gives the filter.
3. **The filter in the tests is a stand-in:** two band-pass sections at Q 1.2, not the IC-7300's own filter shape. The 700 Hz finding should be confirmed at the radio.
4. **Seven verdict-row tests are red since unit 488** over the `mixingHz` field. They are named, not repaired (§12.6).
5. **Process slip.** Task 1 was committed before its neighbour tests were read, because the run script's exit status is not the tests'. Those failures were checked afterward and predate the unit.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
