```
UNIT: 522 - partial (task 1 built and measured, off by default; tasks 2 and 3 on) - 2026-10-01
UNIT GOAL: shape first, pitch as a result, and a gauge that fills
NUMBER: shape-first reads the call whole down to 8 dB (the per-bin path garbles 8); it ships off because a 12 WPM fist reads nothing
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSequenceShape.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 522 and HM-DEC-226 were free.

**How the session ran:**
- **HEAD was tagged `before-shape-first` and pushed** before any engine change.
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**Task 1: shape first.** Commit `ec60a7d9`. **Built, measured, and off by default** (`CwEnvelopeDetector.ShapeFirst`).
- **The rectangle is found in time.** Each hop, the passband's bins are summed in linear power into one trace (a second `Bin`, with running sums). Unit 517's rectangle fit (share of variance explained) runs on it at every swept length from 25 ms to a 5 WPM dah, plus the standing senders' lengths. Each rectangle's best end in time becomes a span. **No bin is chosen to find it.**
- **Pitch is a result.** Over a span:
  - each bin's excess power (span less pads) is taken;
  - the bins standing four times the median excess, two or more together, are one sender's energy;
  - within that energy, the excess is measured again over the mark's **own samples**, at 5 Hz steps through a Hann window the mark's length. The 10 ms bins see two stations 200 Hz apart beat in step with the 5 ms hop, which reads as keyed power between them; over a 180 ms dah they are separate tones;
  - the centroid of the half-power stretch round each peak is a mark's pitch. **There is no climb, and unit 496's apex walk plays no part on this path.**
- **Overlapping senders.** Two peaks a quarter of the strongest or more, 50 Hz or more apart, are two marks. Each is refitted on its own bins for its own edges.
- **The weakest get a second pass.** A span whose whole-band step is no taller than the 1.5 dB flatness tolerance (unit 516's rule) passes only on unit 517's lobe at its centroid, which judges height against its own bin. Without that rule a 1 dB bump on a dah's own top was a mark, and `N0CALL` read `NOETCALL`.
- **A mark's level is read over the hops inside its edges.** A fist's dit read several dB under its own dahs and fell outside the pattern gate's tolerance.
- **The retired per-hop tests.** Flatness (R93), edges (497), narrowness (498, 514), key-up and promptness (492) and the gaps' wander decide nothing on this path. The bins still build their bars, which the scope, the meter and the per-bin path use. The pattern gate is unchanged in rule, and its "within one bin" is now on the centroid.
- **The costs, measured:** on the bench's call, the floor went **down**, not up. See section 3. The noise added by summing the passband is outweighed by fitting a whole mark rather than judging a hop.

**Why it ships off.** Every existing case was run with it on. It improves 8 dB, the steady-carrier dah, the clicks and noise. But with it on:
- a **12 WPM fist reads nothing** (its dahs are partly missed and nothing stands);
- the **two-station** case reads `CM CT A IE EMMCAE EL N0CALL K`;
- the **sender who speeds up** reads `… K ■H■S`;
- the 30% fist reads `N0CE LL`, and the tightening fist `DEN ■CALL`;
- the **neighbour** beside a random carrier reads nothing. The carrier no longer prints, but the clean sender's marks under the carrier's (47 of 65) are not split off; 8 of them are found.
- Two no-detection cases fail.

Off, every existing case runs the per-bin path as at the tag, and the task 1 tests turn shape-first on to print both.

**Task 2: the light reads the score.** Commit `b18c628c`. Green (`shape found` or `reading`) needs a standing sequence whose shape passes **0.2** (`CwShapeLights.GreenScore`). That is clear of noise's best (0.173, unit 520) and under a rough fist's 0.374. One that stands under it stays amber at "5 of 5". The hover says so.

**Task 3: the gauge.** Commit `4996c216`.
- **The engine.** `CwShapeLights.Fill`:
  - empty while listening;
  - a fifth per mark while a shape forms, to the mark at 0.8;
  - from the fifth mark, the shape score from 0.2 at the mark to 1.0 full;
  - full while reading.

  The reading carries `ShapeFill`.
- **The window.** The light is now a 158 px bar under its words, the width the lamp and words took. The fill is amber or green by state, and a thin line marks four-fifths. The hover reads *"Fills as Hamlet grows sure it has a station here. Past the mark, hold the frequency…"*
- **The row** gains `shapeFill`.

**Records:**
- R117 in both plans, in the owner's words.
- HM-DEC-226 in `DECISIONS.md`, naming the tag and the four faults.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 522 - STEP 12`.
- `PHASE_STATUS` (both copies) names 522.
- Version 1.13.206 to 1.13.207.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278; two 1 ms losses passed alone.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **The gauge.** Beside the scope on the CW tab, a bar under the words:
  - it fills a fifth per mark while a shape forms, amber;
  - past the line at four-fifths it turns green and says **hold here**, and it keeps filling as the shape gets cleaner;
  - it is full once letters print.

  It never crosses the line on noise. A sloppy shape that stands but scores under 0.2 sits at the line in amber and never says hold here: that is the 0.06 sequence from 18:20.
- **What prints is unchanged today.** The new way of finding marks, by shape in time with pitch as the result, is built but **switched off**. On the bench it reads a weak station further (whole at 8 dB), finds a dash next to a steady carrier, and reads every pitch and a 100 Hz dial step the same. But it still reads nothing from a slow hand sender at 12 WPM and garbles two stations at once, and switching it on now would make the radio read worse.
- **The weakest stations would read further, not less far.** The floor on the bench's call goes from garbled at 8 dB to whole at 8 dB.
- **Nothing about letters changed.**

## 3. What you should see

**The neighbour and the steady carrier**, per-bin path (before) against shape-first (after):

| case | before | after |
|---|---|---|
| clean 12 dB at 625, random 24 dB carrier 200 Hz away | `NOAM■HIV5■` (the carrier) | nothing |
| the same, 150 Hz away | `NOAM■HIV5■` | nothing |
| the same, 100 Hz away | `NOAM■HEEV■■M` | nothing |
| one 180 ms dah at 625 beside a steady 24 dB carrier at 825 | no candidate at 625 | **one mark, 175 ms at 625 Hz** |

**One click either way**, 24 dB through the 500 Hz filter on 600, before and after:

| pitch | before | after |
|---|---|---|
| 600, 610, 650, 690, 700, 750 Hz | whole | whole |
| 600, 650, 700 Hz with the dial stepped 100 Hz mid-call | whole | whole |

**The strength table**, 20 WPM, marks stood at the pitch and what reads:

| dB | before | after |
|---|---|---|
| 24 | 65, whole | 65, whole |
| 16 | 65, whole | 65, whole |
| 12 | 72, whole | 65, whole |
| 10 | 65, whole | 65, whole |
| 8 | 64, `CQ NIQ EIE N0CALL N ■NIALL K` | 65, **whole** |

**Noise:**
- With shape-first on, 30 s and 3 min of loud noise stand no sequence.
- Off (as shipped), noise's best shape is 0.061 in 30 s and 0.173 in 3 min, and the light is never green.

**The gauge's sequence**, on a clean 20 WPM call at 24 dB:

| time | fill | words |
|---|---|---|
| 3.320 s | 0.40 | `shape forming · 2 of 5` |
| 3.560 s | 0.60 | `shape forming · 3 of 5` |
| 3.680 s | 0.80 | `shape forming · 4 of 5` |
| 4.040 s | 0.80, crossing | `shape found · hold here` |
| 4.5 to 6.0 s | 0.82 → 0.85 | `shape found · hold here` |
| 6.380 s | 1.00 | `reading` (the first letter is at 6.360 s) |

- **A sloppy sequence** that stands at shape 0.005 was green for 126 steps on the old rule. Now it is amber throughout, at the line.
- **Loud noise** never fills past 0.8.
- **Amber starts at 0.4, not 0.2.** A shape is counted only once it shows a dah beside a dit (unit 521).

**Every existing case**, with the shipped default: unit 515's filter, units 517 to 521's cases and these tests all print as at HEAD. No line HEAD printed is missing.

Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

1. **Shape-first can't be switched on yet.** Its failures, each its own work:
   - a 12 WPM fist reads nothing;
   - two stations garble;
   - a sender who speeds up garbles;
   - the 30% and tightening fists slip a letter;
   - two no-detection cases fail;
   - a clean sender's marks under a louder neighbour's marks are not split off.

   The next unit should take them in that order, then switch it on.
2. **The per-hop gates' own tests** (edges, narrowness, the shape gate, which gate turns W1AW away) test the per-bin path. They pass because it stays the default, and they will need `ShapeFirst = false` when shape-first is switched on.
3. **The gauge's first step is 0.4**, because a shape needs two lengths to count.
4. **`DecisionLogOrderTests` gaps check** is red as at HEAD, for HM-DEC-166, 182, 189, 220 and 222. The order check passes.
5. **Unit 518 runs next**, as the order says.

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed (item 1). Built off, waiting on the owner. The switch is `CwEnvelopeDetector.ShapeFirst`.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by task 1's centroid over the mark's own samples (the steady-carrier dah). The random neighbour is still open, and no change sits in the climb.
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader or the gate.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
