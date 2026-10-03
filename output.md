```
UNIT: 532 - all five tasks done - 2026-10-03
UNIT GOAL: the reader becomes a lookup table
NUMBER: the reader is 76 lines, from 1,122; every reading identical to HEAD after each move
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and the recording exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's one recording. HM-DEC-236 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- **HEAD was tagged `before-lookup-table` (dc80c0e6) and pushed** before any change.
- R88 stayed lifted for the one recording. Four more recordings appeared in `tests\fixtures\cw\captured` on 2026-10-03 (`cw-2026-10-03-143906` and three after it); they were not read or committed.
- Nothing keys, transmits or writes to the radio.

**Task 1: every mark leaves the gate labelled `.` or `-`.** Commit `5cbd53a4`.
- When a letter is released, the gate hands on a stream of `CwSymbol`, the new symbol type:
  - each mark as a dot or a dash, decided by the sender's own line between its two lengths;
  - a letter end, carrying how sure the gate is of the dots and dashes, the signal, the speed, the time and the marks;
  - a word end before the letter wherever the gap was a word.
- A separate step looks the letter up from those symbols.
- The rules moved unchanged: the two length clusters and the spread-weighted line between them, a hand's two kinds, the split against neighbours, and the retry over newer marks when the speed changes.
- Labels are final when a letter is released, because a sender's unprinted runs are still re-split at its newer dit up to then.

**Task 2: the sender and the lone letter are decided in the gate.** Commit `e2413cb2`. The class that decides is now `CwSenderGate`, the gate's sender stage, with every rule unchanged:
- which sender is printed: the best shape after the first word gap, held until silent for its word gap and a dah, let go only under a shape of 0.1;
- the lone-letter rule: a one-mark letter is held until a letter of two marks or more confirms it, and three in a row are dropped;
- the runs, and where words end.

All 20 files that used the old reader now use the gate.

**Task 3: what is left is the table.** Commit `38ea5891`.
- **`CwRunReader` now only takes the gate's symbols and looks each letter up:** the Morse table with its prosigns, the placeholder for an unknown pattern, printed-stays-printed, and one event per character.
- **The gate passes it the stream** and passes its characters on, so every caller is unchanged.
- **Tests:**
  - `TheReaderIsALookupTable` reads `src\Hamlet.RadioEngine\Cw\CwRunReader.cs` with its comments removed. It fails on any number other than 0 or 1, or any word for a time, length, level, pitch or score. It finds none.
  - `TheReaderReadsSymbols` reads `--.. -.. ...-` as `ZDV`, `.-.-.` as `<AR>`, and an unknown pattern as the placeholder.
- **Size: the reader is 76 lines (45 of code), down from 1,122.** The gate's stage is 1,151.

**Task 4: the recording test expects the sender's timing.** Commit `1533f550`. It expects `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, and its remark explains the 579 ms pause between C and H. Green.

**Task 5: noise agrees within half a bin.** Commit `5f43df74`.
- A mark joins a sequence only within half a bin of its pitch (`CwPatternGate.AgreeHz`).
- The three noise tests (`MostNoiseBarsHaveNoEdges`, `RealMarksScoreInsideTheShapeAndNoiseOutside`, `TheShapeTurnsAwayNoiseThatPassedFiveLines`) now count candidates instead of marks that stood, and each says so. All three are green.

**Records:**
- HM-DEC-236 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 532 - STEP 12`.
- `PHASE_STATUS` (both copies) names 532.
- Version 1.13.216 to 1.13.217.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 277 of 278. The one loss is `TheStarIsDrawnAndHittableAtAllNineSizes…`, at 1 ms ("You've caused dispatcher loop"); it passes alone.

## 2. What the owner should expect

- **Rebuild.**
- **Nothing you read changes.**
- **The reader is now a lookup table.** It is handed dots, dashes, letter ends and word ends, and turns them into letters, and that's all it does. Every decision is made where the shape is found: dot or dash, which station to print, whether a lone E or T belongs to something, where a word ends. A test now fails if any measurement creeps back into the reader.
- **Your recording's test passes,** on your sender's own timing: `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`. Every letter is right, and the space after the C is his own 579 ms pause.
- **Noise no longer forms even a passing sequence,** and the random carrier test passes at 775 Hz.

## 3. What you should see

**The identical-readings check**, every printed reading line in every set against HEAD, character for character:

| set | after task 1 | after task 2 | after task 3 |
|---|---|---|---|
| a (53 tests) | 55 of 55 identical | 55 of 55 | 55 of 55 |
| b1 (27) | 19 of 19 | 19 of 19 | 19 of 19 |
| b3 (29) | 43 of 43 | 43 of 43 | 43 of 43 |
| b4, first half (31) | 27 of 27 | 27 of 27 | 27 of 27 |
| b4, second half (8) | 13 of 13 | 13 of 13 | 13 of 13 |
| b5 with the recording (40) | 34 of 34, plus the recording as at HEAD | 36 of 36 | 36 of 36 |

**The recording:** reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`. The test expects exactly that and passes.

**Task 5's counts:**

| | before | now |
|---|---|---|
| 30 s of loud noise, marks that stood with edges / without | 44 / 0 | 0 / 0 |
| 3 minutes of loud noise, marks that stood | 12 | 0 |
| 30 s of noise, candidates past the older tests, with edges, narrow, inside the shape | — | 1,456, 1,339, 1,108, 663 |
| random carrier prints at | 775 and 825 Hz | 825 Hz |

Every reading case reads as before. The only lines that changed are noise:
- the clean sender's garbage beside a carrier 150 Hz away (`NOAM E E N DT…` became `NAEENI DT…`);
- one shape score in a "before" diagnostic.

**Reds:**
- the 8 dB call;
- Farnsworth and fast at 10 dB;
- a letter from noise (blocks);
- the random carrier at 825 Hz;
- the clean sender at the edge;
- the radio strength table at 8 and 10 dB;
- `AFistThatTightensIsFollowed`.

## 4. What's blocking us

1. **The random carrier prints at 825 Hz.**
2. **The 27 WPM fist that tightens reads `CQCQ DE`:** its first word gap falls under Morse's own midpoint.
3. **The strength table through the filter reads `CTU NIG` and `CTU CQ` at 8 and 10 dB,** before the sender's window opens.
4. **Labels are final at a letter's release, not at each mark:** a sender's unprinted runs are re-split at its newer dit until then. Labelling each mark as it arrives would change readings, so it was not done in a move.
5. **Between this tree and removing the old decoder:**
   - `CwDecoder` still builds the tone tracker (`CwToneTracker`, 1,568 lines), the probabilistic stream (`CwProbabilisticStream` over `CwProbabilisticDecoder`, 1,743 lines) and the optional second reader (`CwSecondReader`) beside the gate.
   - The app reads the timing-only path's output in places. The tag `before-cw-cleanup` names them.
   - Each needs its callers moved to the gate's stream before it can go. Nothing here removed any of them.
6. **Pre-existing app reds** outside the line are untouched: the scope's tone-line tests, two `centre` spellings, `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
