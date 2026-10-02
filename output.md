```
UNIT: 526 - partial (eight of nine tasks landed, task 3 dropped; the order's red cases read at HEAD, so the bench reds under AGC were fixed first by the owner's choice) - 2026-10-02
UNIT GOAL: a hand is read against itself
NUMBER: through the filter with a 3 dB AGC overshoot the 13 WPM hand reads whole (0 marks stood at HEAD); the straight key at 12 dB with AGC stands 77 to 78 marks (0 at HEAD)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwPatternGate.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. HM-DEC-230 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**First, the failing cases did not fail.**
- On clean bench audio at HEAD, five cases all read whole: the 25 → 35 → 25 WPM speed change, a hand drifting 13 → 18 → 13 WPM, the 13 WPM hand, W1AW's 5 WPM Farnsworth section, and the SKCC straight key (scatter two fifths, dahs 2 to 4 dits). Only the straight key's last word gap ran together.
- With a 3 dB AGC overshoot at each key-down, the openings were lost: `TEXT`, `CQ CQ`.
- Through the 500 Hz filter with AGC, the 13 WPM hand stood **nothing** at 24 dB, and at 12 dB the straight key stood nothing.
- So the bench says the air's stale-speed symptoms are at least partly AGC handling upstream of the reader. I asked once.
- **The owner chose to fix AGC first, then build tasks 1 and 2.**
- The work order was worked in this order: 7, 4, 1+2, 9, 6, 8, 5, with task 3 dropped.

**Task 7: the first mark of a transmission settles.** `CwEnvelopeDetector.Bin.KeyDown`, commit `b53dc847`.
- **The rule:** before anybody is keying there is no contrast. A rise above everything the bin heard in the second before the window began to rise, by more than a flat top's wobble, is now a key-down and settles.
- **The figure:** the second is the bin's existing one-second memory, and the wobble is the 1.5 dB flatness floor.
- **AGC bulletin:** the +3 and +4 rows read whole, first T included; the test now asserts the whole bulletin.
- **The weak call** reads `CQ` where it read `CGE`.
- **Noise bars:** unchanged, 38 / 30 / 27.

**Task 4: the hand test on the per-bin gate.** Commit `bb78474d`.
- **The change:** `_pattern.HandKinds = true` on both paths, guarded by unit 524's 0.2 shape line.
- **The straight key** at 12 dB with AGC stands 77 to 78 marks; it stood 0.
- **Noise** stands 0 marks in 30 s and 31 in 3 minutes, as at HEAD, and prints nothing.
- **The narrowness noise probe** (`MostEdgedNoiseBarsAreNotNarrow`) now counts before the pattern gate, the gate unit 498 measured before. Counted after, it was measuring whichever noise sequences stood.

**Tasks 1 and 2: every mark and gap judged against its neighbours.** Commit `76d95527`, one commit because both live in the reader's run code.
- **Marks.** `CwRunReader.Sender.SplitAround` splits a letter's marks by its own marks and the sender's three either side.
- **Gaps.** `CwPatternGate.KindAmongNeighbours` judges a gap inside or between letters by the three gaps either side. It is used for the next gap (`KindOfNext`) and in the re-split.
- **When neighbours decide.** Both act only where at least two on each side show a clean 2:1 jump; otherwise the sender's running clusters decide, as before. Requiring two on each side came after the straight key's K read O: a single 35 ms dit made its own "cluster".
- **Five dits is a floor** in `GapLines` wherever three clusters show and the letter gaps sit under five dits. The drifting hand's `BROWNFOX` now parts.
- **Neighbour count:** `NeighbourGaps` = 3, a letter's worth either side.

**Task 9: the listening panel has no hover.** Commit `a5289744`.
- Removed: the scope's tip in `MainWindow.axaml`; `CwScopeControl`'s pointer handlers, `TipAt`, `LetterTip` and `LetterTipWords`; and `CwHearingViewModel.ScopeTip` and `ScopeBarTip`.
- The four tests that read those tips are gone, and the tab test asserts no tip.
- `EveryControlSaysWhatItDoesTests.DeliberatelySilent` names `CwScope` with Tim's words, and a test holds it.

**Task 6: the gauge cannot count past five.** Commit `56b079d9`. `CwShapeLights.Words` says `shape forming · not yet` at five or more, and the fill already stopped at the mark. New test `TheGaugeCannotCountPastFive`.

**Task 8: the record button.** Commit `fd96d4ee`.
- **No unit removed it.** The press began as `Keep this audio` (`49b844c6`). It was deleted with the old decoder on 2026-08-21 (`4bc3bce8`) and put back the same day (`c32f0634`).
- It has said `I hear a station` since your ruling of 2026-08-26 (`979d85d0`). It is enabled only while decoding, and it writes `cw-<time>.wav` and `cw-<time>.txt` to the capture folder.
- Its hover now says it records, what and where (`MainWindowViewModel.CaptureTip`). The label stands as ruled.

**Task 5: the reader holds a standing sender.** Commit `da4f45b8`.
- **The rule:** a printed sender is released on shape only under `ReleaseScore` = 0.1.
- **What released it at 0.50, found by reading, not reproduced.**
  - The gauge reads the gate's sequence shape, but the reader released on its own sender's shape.
  - Since unit 524 that shape is judged over the marks at the sender's speed now, with only those marks' evidence.
  - Where a hand's forty marks look like two speeds, that is its newest ten or five, whose evidence alone takes a 0.5 shape under 0.2.

**Task 3 dropped.** With one split per letter, a dah shorter than a dit in the same letter cannot occur, so it would fire 0 times on every case.

**Records:**
- HM-DEC-230 in `DECISIONS.md`.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 526 - STEP 12`.
- `PHASE_STATUS` (both copies) names 526.
- Version 1.13.210 to 1.13.211.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **The opening of a transmission is no longer lost to AGC FAST.** On the bench, a strong station's first letters through the filter read where they were dropped. A 13 WPM hand through the filter stood nothing and now reads whole.
- **A straight key stands within ten marks** instead of thirty seconds of "forming". The hand test needs ten marks, so that is when it stands.
- **A speed change reads with no stretch of wrong letters** on the bench, and a drifting hand is read against its own last few marks and gaps. On clean audio this already read before today; what changed is the AGC opening and the drifting hand's word space.
- **W1AW's 5 WPM section reads as words** on the bench, with AGC or without. It did before, except with AGC, where the first word was lost.
- **A station that is being read is not dropped** until its shape falls under 0.1.
- **The gauge never counts past five.** It says "not yet" instead.
- **The record button was never gone.** It is the button that says `I hear a station`, live while decoding, and its hover now says it records the last half minute and where it puts it.
- **The listening panel no longer pops up text.** The buttons beside it still do.

## 3. What you should see

**The straight key and the speed change:**

| case | condition | HEAD | now |
|---|---|---|---|
| straight key, two fifths | 12 dB, AGC | stood 0, reads nothing | 78 stood, `CQ CQ SKCC DE N0CALL N0CALLN` |
| straight key, two fifths | 24 dB, AGC, filter | `E■■ SKCC DEN0CALLN0CALLK` | `CQ CQ SKCC DE N0CALL N0CALLK` |
| straight key, two fifths | 12 dB, AGC, filter | stood 0 | 77 stood, `CK■ASKCC DE N0CALL N0CALLK` |
| speed 25 → 35 → 25 | plain, AGC, AGC+filter, 12 dB AGC+filter | whole | whole |
| speed 25 → 35 → 25 | 12 dB plain | `… FOX W TUMPS …` | the same (a level split, not the speed) |

- The straight key's last word gap is drawn at 4.35 dits, under the five-dit floor, so `N0CALL K` reads `N0CALLK`.

**The 13 WPM hand and Farnsworth:**

| case | condition | HEAD | now |
|---|---|---|---|
| 13 WPM hand, a fifth | 24 dB, AGC, filter | stood 0 | `THIS CONNECTION IS GOOD TNX FER CALL ES QSO` |
| 13 WPM hand | 24 dB, AGC | `HIS CONNECTION …` | whole |
| drifting 13-18-13 | AGC | `E U IS FROM SEPTEMBER 2024 … BROWNFOX …` | whole |
| drifting 13-18-13 | 12 dB, AGC, filter | `E U IS FROM … BROWNFOX … TIOG` | `TEXT IS FROM SEPTEMBER 2024 A E I HE QUICK BROWN FOX … TIOG` |
| Farnsworth 5 | AGC, AGC+filter | `IS FROM SEPTEMBER 2024` | `TEXT IS FROM SEPTEMBER 2024` |
| Farnsworth 5 | plain | whole | whole |
| 4-dit word gaps | plain | runs together | runs together |

**Existing cases, shipped path:**
- **Better:**
  - the AGC +3 and +4 bulletins read whole including the first T;
  - the weak call reads `CQ`;
  - +6 with the fit on gains its T.
- **Reds as at HEAD:**
  - `TheCallReadsAtEveryStrength(8)`, `FarnsworthAndFastReadAtTenDecibels`, `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`;
  - `ACleanSenderAtTheEdgeOutranksALouderFistAtTheCentre`, now `TESTD` at 600 and `KEEN■CALAEN■KAEILK` at 825;
  - the shape-first carrier tests at 725 and 775. The 725 Hz one, 100 Hz away, prints `NOAM■ZTE■` for `NOAM■`, held by the 0.1 release.
- **Changed diagnostic and junk rows:**
  - the W1AW all-gates-off rows print `CTK TRQ DE NOMERALL NO` where they printed nothing (the hand test on the pattern gate);
  - one carrier junk row.
- **App tests outside the carry-forward line, red at unit 525's end too and not touched:**
  - `TheScopeShowsTheMarksTests.TheToneLineReadsThePitchWhileAMarkIsUp` and `TheScopeIsOnTheCwTabBesideTheButtonsAndPaints` (the tone line reads `no keying`);
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`, two `centre`s in the tone-line text.
- `DecisionLogOrderTests` gaps check is red as at HEAD.

## 4. What's blocking us

1. **Task 3 dropped.** One split per letter makes its condition impossible.
2. **The air's speed-change symptom is not reproduced on clean bench audio.** Two 12 dB cases still fail:
   - `W TUMPS`: a dah measured 7 dB low starts a second reader sender, so the reader's level grouping at low SNR is the next lead.
   - The drifting hand through the filter with AGC.
3. **Task 5's release was diagnosed by reading.** It needs a capture or an air report to confirm.
4. **The record press is labelled `I hear a station`** by your ruling of 2026-08-26. Renaming it `Record` would change that ruling, so it's yours.
5. **Pre-existing app reds** (the scope tone line, two British spellings) are outside the carry-forward line and were left, per §12.6.
6. **Shape-first (off)** was not re-measured beyond the carrier rows.

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 526, 2026-10-02:** whether the record press should say `Record` rather than `I hear a station`. Waiting on the owner. Its hover already says it records; no label change sits in the tree.
