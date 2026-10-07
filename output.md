## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 12:40:06 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. **HEAD tagged `before-shape-score` and pushed before any engine change.** Version 1.13.234 to 1.13.235. Ruling HM-DEC-254, the number the order gave. Nothing was recorded under §12.1. R88 was lifted only as the order named.

**The check was interrupted once.** The run comparing the new tree with the tag was stopped by Claude Code for low memory. At your word it was rerun at the tag in two single runs, and later on the final tree. Section 3 has the result.

**Task 1, the score taken apart** (`7212750f`):
- **The terms.** The old score was a product of six terms:
  - rectangle (the marks' own shape, height left out);
  - dit tightness and dah tightness (spread in log-length, against a hand's widest);
  - separation (dah over dit: nought at 2, one at 3 or more);
  - consistency (the share of marks within √2 of a kind);
  - evidence (one less e to the minus marks over ten).
- **Measured beside them:** the tightness of gaps inside letters (gaps under √3 dits), the dah-to-dit ratio, the gaps' fit to CW's kinds, and level spread and step.
- **How it was measured:** `TheShapeScoreTakenApartTests` runs the app's own chain and samples every gate sender and every standing detector sequence each second of audio. The four classes:
  - **perfect keying:** W1AW's printed sender, over the whole recording;
  - **real hands:** the printed sender inside every scored stretch, and both station catches;
  - **junk:** 30 s and 3 minutes of loud noise at two seeds each, the carrier catch, the random carrier at twenty seeds, and every other sender on W1AW's recording (there were none);
  - **synthetic clean:** calls at 15, 20, 25 and 35 WPM, at 24 and 12 dB, through the filter with the bench's AGC.
- **What pulls W1AW down and lifts junk up:**
  - **The rectangle.** Junk 0.88 against W1AW's 0.73, hands 0.63 and synthetic 0.57. A randomly keyed carrier has perfectly sharp edges; the filter and AGC round every real signal's edges, and the shorter the mark the more (20 WPM synthetic: 0.40).
  - **Separation.** W1AW's dah is 2.93 dits through the filter, so it scores 0.94; junk's lengths spread up to 8.4 to 1, so it scores 1.00.
  - **Evidence** counts marks and does not tell a station from junk (0.93 to 0.92).
  - **What separates:** dit and dah tightness, consistency, and the tightness of gaps inside letters.

**Task 2, a score that ranks** (`ec15fe83`):
- **The score is now dit tightness × dah tightness × inside-gap tightness × consistency.** Rectangle, separation and evidence come out of the score and stay on the record for reports. Pauses between letters, words and sections are not scored; only a key's gaps inside a letter are.
- **The new score's medians:** W1AW **0.94**, synthetic **0.95**, real hands **0.76**, junk **0.11**.
  - Every real class sits above junk at the median.
  - **The ranges overlap.** The carrier catch (0.68) and one short-lived sender in three minutes of noise (0.69) reach the roughest hands' dips (lowest 0.12).
- **Thresholds:**
  - **Release stays at 0.1:** under every real hand's least (0.125), at junk's median (0.106).
  - **A candidate must score 0.1**, where before it only had to be over nought.
  - **The gauge's green moves from 0.2 to 0.4:** junk's 75th percentile, under real hands' 5th (0.46).
  - **Unchanged:** a detector sequence still stands over nought; the scan's positive and the light's green follow the printer, not the score; the single mark's 0.003 is a different score.
- **Your follow-up, the qualify line at 0.4: measured and not taken.**
  - Five real hands' printed senders dip under it, so your condition fails:

    | Source | Samples under 0.4 | Lowest |
    |---|---|---|
    | `221502` | 2 | 0.35 |
    | `221530` | 1 | 0.30 |
    | `221745` | 1 | 0.15 |
    | station catch `153810` | 5 | 0.12 |
    | station catch `154819` | 2 | 0.18 |

  - **Junk that qualifies at 0.1:** the carrier catch (3 samples) and 12 random-carrier seeds (5193 to 5211, all but 5196, 5199, 5200 and 5208), best 0.68.
  - **Junk that qualifies at 0.4:** the carrier catch and 9 of those seeds (all but 5202, 5205 and 5210).
- **Fixed before committing:** the old `None` left the new gap term NaN, which would have kept a printed sender with no two kinds from ever being released.

**Task 3, the printed station keeps the terminal** (`a12f45d2`):
- **The rule:** at a pause, a challenger now needs to have **qualified for 15 s** and to score **0.16 above** the printed sender.
  - **0.16** is the median spread of a real hand's own score over any minute (0.158; W1AW's 0.009, synthetic 0.002).
  - **15 s** is about one call: `CQ CQ DE K1ABC K1ABC K` at 18 WPM takes 15.7 s. The author's.
- **The QSO handover is unchanged:** a printed sender gone silent is released and the reply picked, its backlog printing.
- **The new test**, `ThePrintedStationKeepsTheTerminalTests`: W1AW after a minute of a 650 Hz station calling 10 dB down, which goes quiet at 50 s and calls again beside W1AW at 70 and 85 s.
  - W1AW takes the terminal at 62.7 s, keeps it, and reads `EPE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS`.
  - **It passes with the margin set to nought too, and at the tag.** At W1AW's pauses the 650 Hz sender, qualified and scoring 0.95 to W1AW's 0.93, was not a candidate, for a reason I did not find. So this case does not prove the margin.

**Records:**
- `docs\cw-scoreboard.md`: a row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 550 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 550, that line only.
- `Directory.Build.props`: 1.13.235.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-254.

**Build** `-warnaserror`: no warnings. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **What was wrong with the shape score.** It rewarded junk for its edges. A carrier keyed at random has perfectly square marks, while your radio's filter and AGC round every real signal's marks a little, so "how square the marks are" put junk ahead of W1AW. It also gave no credit for how evenly W1AW spaces the elements inside each letter, which is what a real key does and noise does not.
- **What W1AW scores now:** about **0.94** out of 1, against 0.64 before. Clean synthetic calls score about the same, and your recorded hands about **0.76**, lower because a hand is less even, which is as it should be.
- **What junk scores now:** about **0.11** typically, against W1AW's 0.94. Two kinds of junk can still reach about 0.7: the fading carrier the scan caught, and one short burst in three minutes of loud noise. They are rare, but they are not near zero.
- **The station on the terminal keeps it** unless something clearly better comes along: another sender must have shown itself for about one call's length and score clearly higher than the printed one. When the station you are reading stops, the reply still takes the terminal straight away, as before.
- **The gauge beside the light** starts its final stretch at 0.4 where it used to start at 0.2, so a real station fills it about as far as before.
- **The scoreboard:** 227 before, 227 after, with every scored recording and every hard limit reading exactly as before. One recording with no reference (`143906`) prints three more letters, `ITT`, at its end.
- **What will look wrong but is not:** four tests are red, the same four that are red at the tag: the two reply tests, the random carrier at seed 5195, and `RealMarksScoreInsideTheShapeAndNoiseOutside`. A fifth that is red at the tag now passes.
- **The next W1AW session is captured automatically** as unit 549 set up, and its `cw_listen` rows will now carry the new scores.
- Pushed to `main`, with the tag `before-shape-score`.

## 3. What you should see

**The term-by-class table, before** (the old score, on HEAD's reading). Median (range); the gate's senders, a sample each second of audio:

| term | perfect keying (25 samples, 1 source) | real hands (239, 13) | synthetic clean (232, 8) | junk (309, 16) |
|---|---|---|---|---|
| rectangle | 0.73 (0.66-0.75) | 0.63 (0.20-0.77) | 0.57 (0.39-0.81) | 0.88 (0.33-0.93) |
| dits | 0.98 (0.95-0.98) | 0.93 (0.28-0.98) | 0.98 (0.92-0.98) | 0.52 (0.00-0.93) |
| dahs | 0.97 (0.92-0.98) | 0.96 (0.86-0.98) | 0.98 (0.96-0.99) | 0.73 (0.00-0.94) |
| separation | 0.94 (0.90-1.00) | 0.89 (0.40-1.00) | 1.00 (1.00-1.00) | 1.00 (0.30-1.00) |
| consistency | 1.00 (1.00-1.00) | 1.00 (0.83-1.00) | 1.00 (1.00-1.00) | 0.73 (0.20-1.00) |
| evidence | 1.00 (0.78-1.00) | 0.92 (0.39-1.00) | 1.00 (0.78-1.00) | 0.93 (0.39-1.00) |
| ratio dah:dit | 2.93 (2.88-3.07) | 2.87 (2.35-4.00) | 3.16 (3.10-3.68) | 3.42 (2.26-8.39) |
| gaps inside letters | 0.98 (0.95-0.98) | 0.92 (0.29-0.98) | 0.99 (0.95-0.99) | 0.83 (0.03-0.92) |
| **score** | **0.64** (0.48-0.66) | **0.44** (0.10-0.64) | **0.52** (0.38-0.69) | **0.10** (0.00-0.56) |

**After** (the new score, on the new reading):

| term | perfect keying (25, 1) | real hands (263, 13) | synthetic clean (232, 8) | junk (309, 16) |
|---|---|---|---|---|
| dits | 0.98 (0.95-0.98) | 0.93 (0.28-0.98) | 0.98 (0.92-0.98) | 0.52 (0.00-0.93) |
| dahs | 0.97 (0.92-0.98) | 0.95 (0.80-0.98) | 0.98 (0.96-0.99) | 0.73 (0.00-0.94) |
| gaps inside letters | 0.98 (0.95-0.98) | 0.92 (0.17-0.98) | 0.99 (0.95-0.99) | 0.83 (0.03-0.92) |
| consistency | 1.00 (1.00-1.00) | 1.00 (0.83-1.00) | 1.00 (1.00-1.00) | 0.73 (0.20-1.00) |
| *rectangle, not scored* | 0.73 | 0.59 | 0.57 | 0.88 |
| *separation, not scored* | 0.94 | 0.88 | 1.00 | 1.00 |
| *evidence, not scored* | 1.00 | 0.90 | 1.00 | 0.93 |
| **score** | **0.94** (0.83-0.94) | **0.76** (0.12-0.94) | **0.95** (0.85-0.95) | **0.11** (0.00-0.73) |

**The detector's standing sequences** were sampled for junk alone, since the real classes are read at the gate: old score 0.15 (0 to 0.59), new score 0.14 (0 to 0.70). **W1AW's recording:** at most one sender held at once.

**The highest junk on the new score**, per source median: the carrier catch 0.68, noise 180 s seed 5370 0.69, carrier seed 5193 0.41. The other carrier seeds score 0.02 to 0.25.

**The new score's percentiles:**

| Class | min | p5 | p25 | p50 | p75 | p95 | max |
|---|---|---|---|---|---|---|---|
| perfect keying | 0.829 | 0.868 | 0.926 | 0.937 | 0.940 | 0.942 | 0.943 |
| real hands | 0.125 | 0.459 | 0.681 | 0.757 | 0.879 | 0.937 | 0.944 |
| synthetic clean | 0.846 | 0.892 | 0.947 | 0.949 | 0.950 | 0.951 | 0.951 |
| junk | 0.000 | 0.011 | 0.036 | 0.106 | 0.403 | 0.691 | 0.726 |

**The thresholds:**

| line | before | after | reason |
|---|---|---|---|
| release | 0.1 (half the green) | **0.1** | under every real hand's least (0.125), at junk's median (0.106) |
| a candidate qualifies | over 0 | **0.1** | nothing picked under the line it would be released at |
| qualify at 0.4 (your follow-up) | — | **not taken** | five real hands dip under it |
| gauge's green | 0.2 | **0.4** | junk's p75 0.40, real hands' p5 0.46 |
| handover margin | any higher score | **0.16** | real hands' median spread over a minute |
| challenger stood | — | **15 s** | one call at 18 WPM, 15.7 s |
| detector sequence stands | over 0 | over 0 | unchanged; the gate decides printing |

**The scoreboard rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added |
| 550 task 1 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 550 task 2 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 550 task 3 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |

Task 1's whole table was identical to HEAD's. From task 2 one line differs: the unscored `143906` reads `I II E NI ST TAEEKEEIEEII IMESE ITT` for `... IMESE`. The random carrier still prints at seed 5195 only, as at HEAD.

**The handover tests:**
- **`AReplyIsReadFromItsFirstLetterTests`** reads exactly as at HEAD: the synthetic QSO `CQ CQ DE W1AW W1AW K TAW DE K3ZZ K3ZZ K`, and `144020` `ES OK ON PA <BT> WX IN NETAGIT IEN TEMP`. Both tests are red, as at HEAD.
- **`ThePrintedStationKeepsTheTerminalTests`** passes.
  - Handovers: 650 Hz printed at 8.8 s and through its calls; released at 51.7 s; W1AW at 62.7 s; W1AW released at its pauses and re-picked at 78.8 and 88.2 s; never 650 again.
  - From W1AW's first letter it prints `EPEIIANDTYPEIVRADIOEMISSIONSHOWEVER,THISNMEIS`, every letter at 600 Hz.
  - **The same with the margin at nought, and at the tag.**
- **The shape, light and handover tests together:** 83 of 87 pass. The 4 reds are the four red at the tag, which had 5. **The scan's tests:** 122 of 122.

## 4. What's blocking us

- **Junk and the roughest hands still overlap.** The carrier catch scores 0.68 and one noise sender 0.69, against hands that dip to 0.12. No single CW term measured here separates them. Level steps came closest (the carrier catch 1.07 dB, a hand at most 1.06 dB), which is not a gap.
- **The margin is not proved by a recording.** In the 650 Hz case, the 650 sender was not a candidate at W1AW's pauses for a reason not found. A case where a qualified challenger stands at a pause is still owed.
- **`RealMarksScoreInsideTheShapeAndNoiseOutside`** is red at the tag and still red. It is about single marks, which this unit did not touch.
- **The scan's tests were not run at the tag**, to spare memory. They all pass on the new tree.
- **The scratch worktree at the tag** was removed at the end.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 549, 2026-10-07:** the stray-letter narrowing. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Capture/TroubleWatch.cs`.
- **Unit 549, 2026-10-07:** the W1AW tolerance. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Capture/W1awSessionWindow.cs`.
- **Unit 550, 2026-10-07:** the handover margin, 0.16, and the challenger's 15 s are the author's figures from measurement, and no recording yet shows the margin acting. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`.
