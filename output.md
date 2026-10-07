## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 15:36:05 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. **HEAD tagged `before-level-marks` and pushed before any engine change.** Version 1.13.237 to 1.13.238. Ruling HM-DEC-257, the number the order gave. Nothing was recorded under §12.1. R88 was lifted only as the order named. Every test run was one filtered invocation; the longest was the app carry-forward line, 2.5 minutes.

**Task 1, the window's own view** (`49229886`):
- **What `TheWindowsOwnViewTests` measures:** a trace of the sender's own window, hop by hop, with the sender's key-down level beside it.
  - **Excursions:** every stretch where the window sits more than 3 dB under the key-down level, its depth as a share of the station's contrast.
  - **Dips and gaps:** an excursion shorter than half the dit is a dip inside a mark, since Morse's shortest gap is a whole dit; anything longer is a gap.
  - **Sources:** W1AW, the strong 860 Hz catch, the owner's scored stretches, and synthetic 25 WPM calls at 8, 10, 12 and 24 dB through the filter with the bench's AGC.
- **Real gaps** fall 0.64 of the contrast or more nineteen times in twenty (0.36 at the 1st percentile); synthetic gaps never under 0.65.
- **Dips** have a median of 0.30. The strong catch's 72 never pass 0.77; two weak hands' recordings (`221502`, `221851`) have dips to full depth, so no level separates the two perfectly. Section 3 has the full table.
- **Committed off:** the commit carries the level path, the window opening on the waiting sender, and the cold re-read with the gate's replacement, all behind switches, all off. It reads exactly as HEAD.

**Task 2, marks by level inside a standing station's window** (`c5efa0e1`), **shipped:**
- **The rule:** inside the window a mark starts where the window rises past **0.4** of the contrast under the key-down level and ends where it falls past **0.6**.
  - The key-up level is the median of the window between marks over two seconds, seeded from the quieter half of the audio the window opened on. Below 6 dB of contrast the old path runs.
  - 0.6 sits under the gaps' fifth percentile, so a real gap ends a mark. 0.4 is the midpoint less the same tenth, wider than the window's ripple (under 0.5 dB strong, 0.8 dB on an 8 dB contrast).
- **No per-hop test:** a mark read by level meets no flat-top, edge, narrowness or shape test, but must be at least **half the sender's dit**.
  - Without that floor, the second station catch fell from 25 to 6 of 35. Short noise marks crossed the line, and the window retuned from 470 Hz to 516 Hz and lost the station.
- **Before a station stands nothing changes;** a mark's times are its crossings less the window's delay, as before.
- **The board:** 227 held; invented 2 to 0; nothing in a silence.
- **The cold re-read was built and is not shipped.**
  - **What it does:** an eight-second raw history; at the window's opening the stretch is read again by level and handed to the gate, which rebuilds the sender (if it has printed nothing) with those marks in place of the grid's.
  - **Measured:** score **208**, the first recording broken (`FER TN`), two letters in a silence. W1AW gained `EPE II E ND`.
  - **Left off:** it stays behind its switch, with the raw history past two seconds invisible to every other reader, so the board reads as without it.

**Task 3, the window opens sooner** (`3d7dc677`), **shipped:**
- **Measured how long the window takes to open** (section 3).
- **Where it opens now:** on the sender the reader holds qualified and **waiting for its first pick** (two keyed runs, two kinds, its own letter gaps), not only once it prints. That is the reader's own qualification, a little later than the bare first two kinds the order named.
- **The dit floor** now uses the dit the window opened on, held while it stays open. The sender's current dit let short noise marks drag the dit down; with it the second catch fell to 6 of 35 again.
- **The board:** **238**.

**Records:**
- `docs\cw-scoreboard.md`: a row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 553 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 553, that line only.
- `Directory.Build.props`: 1.13.238.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-257.

**Build** `-warnaserror`: no warnings.

**The scoreboard guard** passes after each task; it now holds the board at 238.

**The scan's tests:** 123 of 123.

**App carry-forward:** 275 of 278. All three failures were `You've caused dispatcher loop`, and the three pass alone (16 of 16).

## 2. What the owner should expect

Rebuild and run as usual.

- **Once Hamlet has a station, it finds the station's marks by level.** It already watched a station through a narrow window tuned to its pitch, but still judged every mark there with the tests it uses to find stations in the noise: whether the top is flat, whether the edges are sharp. A weak or fluttering dah failed those and came out as two dits or nothing.
  - Now, inside that window, a mark is simply the time the signal sits above the point between the station's own loud and quiet levels, with a margin either side so a wobble doesn't start or stop one.
  - The tests still decide what counts as a station in the first place.
- **The window opens sooner.** It used to open only once a station printed. It now opens as soon as Hamlet has decided it's a station and is waiting out a word gap before printing it, usually a quarter to a full second sooner, once twelve seconds sooner.
- **The fluttering station** (the strong 860 Hz one the scan caught) reads far better: 136 of 152 letters against 108, wrong letters 35 to 10. Four of its nine dahs that used to break in two now read as one.
- **W1AW tuned in cold** now prints `E NE II AND` for `PE II AND`, gaining AND. P is still missed: it arrives in the first second, before Hamlet has decided there's a station.
- **The weak bench test** (25 WPM through the filter at 8 to 24 dB) already read every dah but one at 8 dB, and still does.
- **The scoreboard:** 227 before, **238** after. No letter prints in a silence any more, noise prints nothing, and the first recording reads whole.
- **One thing reads a little worse:** the second, weaker station the scan caught gives 22 of 35 letters where it gave 25.
- **What will look wrong but is not:** the app's test line shows failures one at a time, a different test each run, all the "dispatcher loop" fault; each passes on its own.
- Pushed to `main`, with the tag `before-level-marks`.

## 3. What you should see

**The scoreboard rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added |
| 553 task 1 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 553 task 2 | 247 of 288 | 20 | 0 | **227** | 0 | 65 of 87, 2 added |
| 553 task 3 | 252 of 288 | 13 | 1 | **238** | 0 | 65 of 87, 2 added |

Measured along the way, not shipped:

| state | score | what |
|---|---|---|
| level marks, no dit floor | 228 | the second catch 6 of 35 |
| level marks with the cold re-read and the waiting window | 208 | the first recording `FER TN`, 2 letters in a silence |
| level marks with the waiting window, the dit floor at the current dit | 239 | the second catch 6 of 35 |

**Task 1, the window's own view:** depth as a share of the station's contrast; dips are falls shorter than half a dit.

| class | gaps: count | p1 | p5 | median | dips: count | median | p95 | max |
|---|---|---|---|---|---|---|---|---|
| W1AW | 97 | 0.19 | 0.63 | 1.00 | 0 | | | |
| the strong catch | 387 | 0.46 | 0.60 | 1.00 | 72 | 0.28 | 0.69 | 0.77 |
| the owner's stretches | 858 | 0.25 | 0.59 | 1.00 | 166 | 0.31 | 0.94 | 1.72 |
| 25 WPM synthetic, 8 to 24 dB | 494 | 0.73 | 0.78 | 1.00 | 0 | | | |
| all | 1836 | 0.36 | 0.64 | 1.00 | 238 | 0.30 | 0.90 | 1.72 |

Dips last 5 to 35 ms, median 10. The full-depth dips are on `221502` and `221851`, two weak hands.

**How long the window takes to open**, from the first mark the detector called:

| source | first mark | opens on the printed sender | opens on the waiting sender |
|---|---|---|---|
| W1AW | 0.30 s | 4.12 s | 3.19 s |
| `200157` | 0.17 s | 3.51 s | 3.18 s |
| `144045` | 0.10 s | 5.34 s | 4.57 s |
| `221745` | 1.17 s | 13.54 s | 9.59 s |
| `221851` | 0.79 s | 16.29 s | 3.90 s |
| 25 WPM, any strength | 3.01 s | 5.36 to 5.72 s | 5.09 to 5.47 s |

**W1AW read cold** (the scoreboard's stretch, reference `PE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS CME IS`):

| state | reads | right |
|---|---|---|
| HEAD | `E NE II AEED TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS` | 40 of 44 |
| task 2 | `E NE II AEED TYPE IV …` | 40 of 44 |
| task 3 | `E NE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS` | **42 of 44** |
| with the cold re-read (not shipped) | `EPE II E ND TYPE IV …` | 42 of 44 |

**The nine broken dahs on `catch-153810-7033367`:**
- At HEAD all 9 stood in two or more pieces.
- After task 2 and after task 3, 5 do and 4 read whole.
- The catch against its pending reference went from **108 to 136 of 152**, wrong 35 to 10. With the old kept-line setting off it reads 97 at HEAD and 130 after.
- The second station catch, `catch-154819-7050903`, reads 25 of 35 at HEAD and **22** after.

**The weak table** (25 WPM, the bench's AGC, through the filter; 79 dahs sent):

| strength | before | after |
|---|---|---|
| 8 dB | 78, `NEQ CQCQ DE K1ABC K1ABC K1ABC K TEST DE W1XYZ W1XYZ K` | 78, the same |
| 10 dB | 79, read whole | 79, read whole |
| 12 dB | 79, read whole | 79, read whole |
| 24 dB | 79, read whole | 79, read whole |

The order's "about 5 dahs of 40" was not seen on this bench: the call here is keyed clean with the bench's 1 dB AGC.

**Noise:**
- All four loud-noise runs print nothing.
- The random carrier prints at seed 5195 only (`N N AETOTTNEAI AN`), as at HEAD.
- Nothing prints in a silence.

**The QSO handover:** the reply tests are red as at HEAD. The synthetic QSO reads exactly as at HEAD; `144020`'s reply reads `IAN` where HEAD read `IEN`, against a reference `IUN`.

## 4. What's blocking us

- **The cold start is still half done.** W1AW's P arrives before any sender has qualified. The re-read that would recover it, built and switchable, reads 208 with the first recording broken.
- **Junk and weak hands still overlap in depth.** Two weak hands' recordings have dips to full depth, so the 0.6 line breaks some of their dahs and no level separates them.
- **The second station catch** reads 22 of 35, three letters under HEAD's 25.
- **The window opens on the reader's qualification** (two keyed runs, two kinds, its own letter gaps). Opening on the bare first two kinds, as the order named, was not tried.
- **The dispatcher-loop failure** in the app's test line hits a different test each run and is not this unit's.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 550, 2026-10-07:** the handover margin, 0.16, and the challenger's 15 s are the author's figures from measurement, and no recording yet shows the margin acting. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`.
- **Unit 552, 2026-10-07:** the key-down term's form, one to Morse's 15 of 22 and straight to nought at a key never up, is the author's. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSequenceShape.cs`.
- **Unit 553, 2026-10-07:** these figures are the author's from measurement, and wait on the owner:
  - the hysteresis, 0.4 up and 0.6 down;
  - the half-dit floor on the dit the window opened on;
  - opening the window on the waiting sender.

  The change sits in `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`.
