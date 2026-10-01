```
UNIT: 523 - partial (five of six cases read with shape-first on; the switch stays off) - 2026-10-01
UNIT GOAL: shape-first reads everything the per-bin path reads, then it is the path
NUMBER: with shape-first on, 5 cases still read worse than today's path, so ShapeFirst stays off
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSequenceShape.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 523 and HM-DEC-227 were free.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.
- **To measure every case with shape-first on**, the detector read a temporary `HAMLET_SHAPE_FIRST=1` from the environment as its default, set only in the measurement script. That switch was in the commits of cases 1 to 3. **It is removed in `23aaccb7`.**

**Case 1: a 12 WPM fist read nothing.** Fixed, commit `52ea1f0c`. Measured with each true mark's span, whole-band fit and candidates printed. **Two causes:**
1. **The gate never stood the fist.** It stands a sequence only on a clean 2:1 jump between neighbouring lengths. A fist scattered by a fifth sends dits to 1.2 dits and dahs down to 2.4: the longest dit was 120 ms and the shortest dah 235 ms, a ratio of 1.96.
   - **Fix:** the gate also takes the reader's own test for a hand's two kinds (unit 513, now one shared `HandKinds`).
   - **On ten marks, not five:** on five, two clusters always look tight, and noise stood on them.
   - **On the shape-first path only (`HandKinds`):** on the per-bin path, noise stood 159 marks in three minutes where it had stood 80.
2. **Three dahs were never placed.** The whole-band fit's lengths stepped by a quarter, and a hand's 318 ms dah sat 9% from the nearest length. They now step by a tenth.

**Case 2: two stations garbled.** Fixed, commit `cf4229b2`. Measured on station A's true marks. **Three causes:**
1. **A loud sender's marks under the other's were missed or merged.** The whole band can't see one sender's edges under another's. **Fix:** where a span holds two senders, each is searched for rectangles on the bin nearest its own pitch across the span.
2. **Two of A's dahs merged into one.** B keyed steadily through A's gap, so it cancelled out of the excess and only one pitch showed. **Fix:** where one pitch's own bin shows two rectangles or more, those are the marks.
3. **A dit right after B's dah vanished.** The pitch measure's Hann window weighed the span's ends to nothing, and A's dit sat at the end. **Fix:** every sample is weighed alike. A flat window's sidelobes, about a twentieth in power, stay under the quarter that makes a second sender.

**Case 3: a sender who speeds up.** All but one space, commit `97c812a4`.
- **Cause:** a 10→20 WPM sender leaves 60, 120, 180 and 360 ms marks among its recent forty. The reader's split put 60, 120 and 180 together as dits (dit 120 ms), so 20 WPM dahs read short.
- **Fix:** a split wider than a hand on either side, or a mix refused as a hand's two kinds, is two speeds (unit 513). The reader takes the split again over the newest half, quarter and so on, down to the five marks a sequence needs to stand. The first split as tight as a hand makes is the sender's speed now.
- **Result:** with shape-first on it reads `… K TESTDE W1AW K`. The dit follows to 56 ms and the `K` is right; the first word gap at the new speed comes before enough new marks have.

**Case 4: the rough fists.** Fixed by case 1, same cause. Both read whole.

**Case 5: the two no-detection cases.** Fixed by case 1. Both pass with shape-first on, and no noise sequence stands on this path.

**Case 6: a clean sender under a louder neighbour.** Dropped. The finer sweep lets the random carrier's marks stand, so junk prints at 200, 150 and 100 Hz where unit 522's end printed nothing.

**Case 7: the switch.** Off. With shape-first on, five cases read worse than the per-bin path reads them today:
- **the speed change:** one space short;
- **the quiet dit inside a letter (511):** `CQ CQ DE N0CA DL N0CALL K`;
- **a clean sender beside a carrier 400 Hz away:** `E EG NQ N M D D EOT N I A`;
- **unit 519's switch case:** the fist reads `N0CTLD T0CALL`;
- **unit 519's edge case:** the rough fist now stands and prints beside the clean sender.

By the order's rule the switch stays off. The per-hop gates' own tests were not changed, since they still test the default.

**Files:**
- `CwEnvelopeDetector.cs`: the tenth-step sweep, the per-sender bin search, the flat window, a probe hook (`BandFitAt`), and setting the gate's `HandKinds`.
- `CwPatternGate.cs`: a hand's two kinds on ten marks, when `HandKinds` is set.
- `CwRunReader.cs`: `HandKinds` shared with the gate, and the newer-speed retry.
- Tests: `ShapeFirstReadsAFist` in `TheShapePicksTheSenderTests`, and `LastMarks` in the fist generator.

**Records:**
- HM-DEC-227 in `DECISIONS.md`. Headline per the order, with the switch stated as off.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 523 - STEP 12`.
- `PHASE_STATUS` (both copies) names 523.
- Version 1.13.207 to 1.13.208.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278; two 1 ms losses passed alone.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **What reads on your radio hasn't changed today.** The shape-first front end is still switched off: with it on, five cases still read worse than the radio reads now. The worst are a sender who speeds up (one space lost), a weak dit inside a letter, and a clean station near a random carrier 400 Hz away. The rule was to switch it on only if nothing reads worse, and something does.
- **With it on, much more now reads:** slow and rough hand senders, two stations at once, a station one click either way, a dash beside a steady carrier, and the call down to 8 dB.
- **The gauge works as unit 522 left it,** and never moves on noise.
- **Nothing that reads today reads worse.** The per-bin path reads every case as before; the only changed lines are junk text on cases that were junk before.

## 3. What you should see

**The six cases**, shape-first on:

| case | unit 522's end | now |
|---|---|---|
| 1. 12 WPM fist, a fifth | nothing | `CQ CQ DE N0CALL N0CALL K` |
| 2. two stations | `CM CT A IE EMMCAE EL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 3. speed change | `… K ■H■S` | `… K TESTDE W1AW K` |
| 4. 30% fist / tightening fist | `N0CE LL` / `DEN ■CALL` | both whole |
| 5. no-detection cases | two red | pass |
| 6. clean beside carrier, 200 Hz | nothing | `NOAM■MII■` (the carrier), dropped |

**The strength table**, per-bin and shape-first:

| dB | per-bin | shape-first |
|---|---|---|
| 24 | 65, whole | 65, whole |
| 16 | 65, whole | 65, whole |
| 12 | 72, whole | 65, whole |
| 10 | 65, whole | 65, whole |
| 8 | 64, `CQ NIQ EIE N0CALL N ■NIALL K` | 65, **whole** |

**Other shape-first measurements:**
- 600 to 750 Hz through the filter, and a 100 Hz dial step mid-call: whole.
- A dah beside a steady carrier: 170 ms at 625 Hz.

**The gauge with shape-first on**, on the live path:

| time | fill | words |
|---|---|---|
| 3.330 s | 0.40 | `shape forming · 2 of 5` |
| 3.580 s | 0.60 | `shape forming · 3 of 5` |
| 3.690 s | 0.80 | `shape forming · 4 of 5` |
| 4.050 s | 0.81 | `shape found · hold here` |
| 4.3 to 6.1 s | 0.82 → 0.88 | `shape found · hold here` |
| 6.390 s | 1.00 | `reading` |
| 19.480 s | — | `listening`, 0.2 s after the last mark |

**Noise, 30 s and 3 min, shape-first on:** nothing stands, and the gauge never crosses the mark (amber 0% of the time).

**Every existing case, shape-first on, still red:**
- the five in section 1;
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`, red at HEAD too;
- unit 522's low-scoring light test, whose precondition fails because no sloppy sequence stands on this path;
- the per-bin gates' own tests (edges, narrowness, the shape gate), which test the per-bin path.

**Every existing case, as shipped (per-bin):** the same three reds as HEAD, and every reading case prints as at HEAD. The only changed lines are junk on cases that were junk at HEAD:
- the random carrier reads `NOAM■ZE■` where it read `NOAM■HIV5■`;
- the W1AW all-gates-off rows read `IIL K` where they read nothing.

## 4. What's blocking us

1. **The switch is off.** Five cases still read worse with it on, and the next unit should take them in this order:
   - the speed change's first word gap;
   - the quiet dit (unit 511's quieter-mark rule on fitted levels);
   - a random carrier standing on the finer sweep, which is also case 6;
   - unit 519's switch case;
   - unit 519's edge case.
2. **Case 6 is dropped.** A random carrier stands on the finer sweep, and a random carrier needs to stay a non-sender on this path.
3. **The per-bin path's random-carrier junk text changed**, junk either way.
4. **`DecisionLogOrderTests` gaps check** is red as at HEAD, for HM-DEC-166, 182, 189, 220 and 222. The order check passes.
5. **Unit 518 runs next.**

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off. Five cases remain (item 1).
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by the per-sender bin search; the random neighbour is still open (case 6).
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader's word line.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
