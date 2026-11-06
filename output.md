UNIT:       598 - complete at task 4 of 4 (the trace, the second model, the panel, the end) - 2026-11-05 22:34
PHASE GOAL: Hamlet meets its CW requirements.
UNIT GOAL:  The learned reader reads the station at the passband edge, and the panel (pitch, speed, the light) shows what it hears, so the owner can rule on turning it on.
ADVANCED:   yes, task 2 (the model) and task 3 (the panel). The switch still ships off, so nothing the operator runs reads differently.
NUMBER:     with the switch on, KM3STU 0 -> 55 of 65; the twelve-plus-three 238 -> 286 (shipped 308); as shipped, unchanged
DRIFT:      0 - this unit advanced both tasks it named

## 1. What Claude did

**Complete, all four tasks.** Claude Code on the development computer, branch `main`, every push accepted. The prompt named `PROJECT: Hamlet`, and the order's four gate checks passed: `SHACK_FACTS.md` and `CwLearnedReader.cs` exist, and there's no `CoreHMI.sln` or `MURC.sln`, at `C:\Source\HamLet`. Nothing here is evidence about the radio. Version 1.13.284 to 1.13.285, ruling HM-DEC-303 as numbered. The lock was taken with the root named and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. The draft truth was not edited. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted. No process I did not start was stopped. The unit took about 55 minutes.

**Task 1, the trace, nothing built** (`f2bc4c65`):
- **KM3STU on the first model, switch on:** `KM3SKE CQ`, `RR`, and nothing. That's 0 of 65 on the twelve-plus-three, which reads 238.
- **The panel, by running the tab's own chain and scope feed on the vector, not by reading the code.** I used a headless probe, `TheLearnedReaderOnThePanelProbe`, because no radio can be connected here. With either switch position the panel showed `no station`, a blank speed, and a light that never went green. The `tone ... heard` line comes from the grid, not from any reader.
- **The path, by file and method.**
  - The `decoding at` line is `CwDecoder.RunsPrintingHz`, which is `CwSenderGate.StationPitchHz`, handed to `CwScopeFeed.Tick` at `MainWindowViewModel.cs:12455`.
  - The speed is `CwDecoder.WordsPerMinute`, the printed sender's dit, via `DetectedWpm` into `TerminalSpeedText`.
  - The light is `CwEnvelopeReading.ShapeLight`, set in `CwEnvelopeDetector`'s hop from the grid's standing sequences and the gate's printed or waiting pitch.
- **No path from either reader to those three existed.** `CwSenderGate.PrintSequence` hands symbols to the lookup table only, so the gate's printed pitch and dit stay empty under the sequence reader too. The work order says the sequence reader already feeds these three places; it does not (section 4).

**Task 2, the second model** (`a28da517`):
- `cwnet.onnx` and `CWNET_SPEC.md` were replaced from `model\`. The vector's wav, features and text were byte-identical to the old, and its ids differed, as the order said.
- **The four vector tests passed on the first build.** `FeaturesMatchTheVector` did not change state.
- The quick measure ran with the switch on, by `CwRules.Choose` in the trace tests, so the source never had it on. Nothing in the model, the alphabet or the feature recipe was tuned.

**Task 3, the panel** (`250e7f71`). With the switch on, the three things the panel shows now come from the learned reader. Nothing on screen moves:
- **Pitch:** `CwDecoder.PanelPitchHz`, the median of the last ten learned letters' mark pitches, blank under three. The scope's `decoding at` line reads it. The detector itself still follows `RunsPrintingHz` alone.
- **Speed:** `CwDecoder.WordsPerMinute` becomes the learned reader's letter rate, the letters and word spaces of the ten seconds up to the commit edge at five characters a word, blank under five.
- **Light:** green (`reading`) while the newest run's last second, not yet committed, carries a non-blank argmax, and dark (`listening`) otherwise.
- **Three tests in `TheLearnedReaderTests`**, each about a second: the pitch within a bin of 579 Hz, the speed 15 to 30 WPM, and the light lit during the first `ROGER` and dark before 0.2 s.
- With the switch off, the learned reader is never built, so every value comes through the shipped path unchanged.

**Task 4, the end** (this commit): the survey once with the switch on, then the engine and app lines with it off. `TheLearnedReaderTests` is now on the engine line's carry-forward list. The survey's `Rerun` line says *learned reader, second model*. The records:
- `DECISIONS.md` HM-DEC-303
- the `CLAUDE.md` §1 row
- `CW_SPEC.md` §14, one paragraph
- both `PHASE_OUTCOME.md` copies and both `PHASE_STATUS.md` copies
- a 598 row in each of the four scoreboard tables
- `Directory.Build.props`

`TheScoreboardGuardsCwTests` (4 of 4) and `DecisionLogOrderTests` (2 of 2) were run alone after the records, since the new rows could touch them.

**Decisions I made myself, each for the owner to overrule:**
- **The speed is marked not proved.** `CwSpeedProof.Proved` is defined as a speed from a measured dit, and the net measures none, so the panel shows `20 WPM, not proved`. This touches what the display asserts.
- **The pitch goes blank again** after ten seconds of audio with nothing printed. A `decoding at` line with nothing decoding describes no reading. The order didn't say either way.
- **The speed reads low while its first ten seconds fill.** On the vector it showed 6 WPM at 5 s and 18 to 20 by 11 s, because the order's rate counts over a full ten seconds even when less has been heard. It does the same after a long silence. I left the order's definition as written.
- **The light updates once a second,** since the net runs once a second. A word gap shorter than that does not darken it.
- **One headless probe in the app tests and two traces in the engine tests** (`TheBoardRows`, `TheFixtures`). All three are `Survey`, filtered, and on no line.

**HM-DEC-303 as recorded** (the order's text, figures filled):

> **HM-DEC-303 - The learned reader, second model.** `cwnet.onnx` is replaced by a model of the same shape, trained from the first on scenes the first never saw: the pitch at the edge of a receiver passband, a residual floor outside the passband pumped by the receiver's AGC, speeds down to 5 WPM, and silent scenes. It has heard no recording. The feature recipe, the alphabet and the streaming rule are unchanged, so the port is unchanged; the vector files are regenerated for the new model and the four vector tests pass. With the switch on: KM3STU 55 of 65 (was 0), the twelve-plus-three 286 (was 238, shipped 308), W1AW 2128 (was 2048, shipped 2101), Bryan 124 of 144 (was 127, shipped 118), Ken 51 of 76 (was 51, shipped 43), the wild bank 3271 (was 3307), the fist bank 5086 (was 5255), the first recording 26 of 27, letters in silences 11 (was 9), the random-keyed carrier at 20 of 20 seeds. The panel with the switch on shows the learned letters' pitch, a speed from the letter rate, and the light from the net's own activity. The switch ships off; the owner rules on turning it on. *Rejected:* tuning the first model's features or thresholds to KM3STU - the recipe is the contract with the port, and a model that needs the recipe bent for one station will need it bent for the next. *Rejected:* training on the recordings - the recordings are the yardstick, and a model that has heard them cannot be measured by them.

**What went wrong:**
- **A BOM in a fragment.** One record fragment picked up a byte-order mark from PowerShell 5.1's `Set-Content`. I caught it and stripped it before the records were written, so nothing reached a file.

## 2. What the owner should expect

- **Nothing you run reads differently.** The switch is off, the shipped reader prints, and the guards hold at 308, 2101, 3898 and 2859. The engine line is 175 of 175 (168 plus the seven learned-reader cases) and the app line 278 of 278, both with the switch off.
- **The second model reads KM3STU,** the station the first model missed at the top edge of your filter. It also reads W1AW better than the shipped reader does: 2128 against 2101, with 4 wrong against 24.
- **What will look wrong but is not:**
  - **The shipped panel shows `no station` and no speed while it prints.** That has been true since the sequence reader shipped. It feeds none of those three, and this unit did not touch it.
  - **The survey file holds switch-on figures.** That's what the run budget asks for. The shipped figures are on the scoreboard.
- **Still not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md` (until this report's commit), `tools\go-claude\GoClaude.bat`, `.run-unit\` and the `cases-*.txt` files.
- **Still untracked:** `model\` and `CwSequenceReader.cs.new`.

## 3. What you should see

**KM3STU, letters right of 65 as the board scores him, switch on:**

| recording | first model | second model |
|---|---|---|
| `cw-2026-10-08-121324`, 14.6-30 s, `POTA DE KM3STU K KQ 4PA K` | 0 of 19 | **19 of 19** |
| `cw-2026-10-08-121357`, 14.3-29.3 s, `55N 55N VA VA BK BK` | 0 of 14 | **12 of 14**, 1 wrong |
| `cw-2026-10-08-121414`, 0.3-24.1 s, `R 55N 55N VA VA BK BK TU RON VA ES 72 KM3STU` | 0 of 32 | **24 of 32** |
| **all** | **0 of 65** | **55 of 65**, 1 wrong |

What each printed, whole:
- `121324`: first `KM3SKE CQ`; second `2KWI3SR EE CQ POTADE KM3STUK KQ4PAK`
- `121357`: first `RR`; second `K FRTNX AGN URSNN5RN MBK RR UR 55D 55N VA VAK BKT`
- `121414`: first nothing; second `R55N5K BK TURON VA S72 K M3STUE CQP`

**The twelve-plus-three:** 238 with the first model, **286** with the second (302 of 353, 13 wrong, 3 invented, spaces 70 of 110 with 4 added). Shipped is 308 (325 of 353). The 22-point gap is now spread across the other rows, not one station: `221530`'s second stretch 27 of 34, `221548`'s second 14 of 18, `221828` 31 of 41, `221745` 22 of 29.

**Ken and Bryan per 20 s, right and wrong**, from the survey:

| Ken N3AFS | shipped | first model | second model | second model printed |
|---|---|---|---|---|
| 0-20 s | 2, 6 | 2, 6 | 2, 6 | `FNDEALL4W ANST SI?9` |
| 20-40 s | 11, 6 | 13, 3 | 13, 3 | `58N QTHIINWKENSEN` |
| 40-60 s | 10, 6 | 12, 4 | 12, 4 | `GTON,PT PA NRPITTS NIU` |
| 60-80 s | 13, 2 | 14, 1 | 15, 0 | `RGH U NAME IS KEN KENSG` |
| 80-100 s | 0, 0 | 10, 6 | 9, 6 | `4 W<AR> KB2UM3DN3RNS` |
| 100-120 s | 7, 5 | 0, 0 | 0, 0 | `AN3AFSLE KB2UZY /M` |
| **all** | **43 of 76, 25 wrong** | **51 of 76, 20 wrong** | **51 of 76, 19 wrong** | |

| Bryan KB2UZK/M | shipped | first model | second model | second model printed |
|---|---|---|---|---|
| 120-140 s | 16, 2 | 16, 1 | 16, 1 | `DLO GM KEN NICE TO MET` |
| 140-160 s | 16, 1 | 15, 1 | 15, 2 | `S TNX FER RPRT URIR 57` |
| 160-180 s | 8, 2 | 10, 1 | 10, 1 | `9 57N LILN IAM IN MO` |
| 180-200 s | 16, 3 | 17, 2 | 17, 2 | `BILE STNMY NAME BRYAN B` |
| 200-220 s | 15, 2 | 17, 0 | 17, 0 | `RYAN ES QTH IS CHICOP` |
| 220-240 s | 14, 2 | 16, 0 | 14, 1 | `, MA CHICOPEE M, MA P` |
| 240-260 s | 18, 2 | 20, 0 | 19, 1 | `ARKED IN MY MOBILE X SOBT` |
| 260-280 s | 15, 3 | 15, 4 | 15, 4 | `AKEN NE3ARS DE KB2UZY/ M` |
| 280-300 s | 0, 0 | 1, 0 | 1, 0 | `K R` |
| **all** | **118 of 144, 17 wrong** | **127 of 144, 9 wrong** | **124 of 144, 12 wrong** | |

The full text, second model:
- Ken: `FNDEALL4W ANST SI?9 58N QTHIINWKENSENGTON,PT PA NRPITTS NIURGH U NAME IS KEN KENSG4 W<AR> KB2UM3DN3RNS AN3AFSLE KB2UZY /M`
- Bryan: `DLO GM KEN NICE TO MET S TNX FER RPRT URIR 579 57N LILN IAM IN MOBILE STNMY NAME BRYAN BRYAN ES QTH IS CHICOP , MA CHICOPEE M, MA PARKED IN MY MOBILE X SOBTAKEN NE3ARS DE KB2UZY/ MK RRDE NIWARS NIY TOME`

The first model's full texts are in the last report:
- Ken: `FNEALL4WANST SIZ9 58N QTHII NWKENSENGTON,PT PA NRPITTS NIURGH TU NAME AS KEN KEN SGV W<AR> KB2UZ3TEN3RNS AN3AFS DE KB2UZY /M`
- Bryan: `DLOGM KEN NICE TOMETUNX FER RPRT URR D579 57N LILD IAMIN MOBILE STNMY NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA CHICOPEE , MA PARKED IN MY MOBILE <BT> SO BTR KEN NWARS DE KB2UZY/MK RRDE NIWARSNIY TOME`

Bryan is three letters under the first model, and Ken holds at 51 with one fewer wrong. Bryan's spaces: 34 of 40 with 1 added, against 29 with 1 added.

**The rest, shipped against first model against second, switch on for the two models:**

| | shipped | first model | second model |
|---|---|---|---|
| W1AW | 2101 (2125 of 2151, 24 wrong) | 2048 (2063, 15 wrong) | **2128** (2132, 4 wrong) |
| W1AW spaces | 440 of 440, 8 added | 368 of 440, 9 added | 402 of 440, 3 added |
| wild bank | 2859 | 3307 | 3271 (3372 of 3528, 101 wrong, 31 spaces added) |
| fist bank | 3898 | 5255 | 5086 (5289 of 5760, 203 wrong) |
| easy fixtures | 90 of 90 | 87 of 90 | 87 of 90 |
| 15 dB CQs | not re-measured | not re-measured | 82 of 84 |
| 5 dB CQs | 84 of 84 | 84 of 84 | 83 of 84 |
| 0 dB CQs | 5 of 84 | 61 of 84 | 52 of 84 |
| noise, four cases | silent | silent | silent |
| steady carrier | silent | silent | silent |
| random carrier | prints at 20 of 20 seeds | 20 of 20 | 20 of 20 |
| first recording | whole | 26 of 27 | 26 of 27, `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA` |
| letters in silences | 1 | 9 | 11 |
| CPU, W1AW piece-04 (283 s) | 106.6 s | 44.7 s | 44.3 s, down 58% |

The wild bank per recipe, second model (first model in brackets): `mobile` 548 (548), `bug` 444 (469), `farnsworth` 584 (583), `tight` 535 (540), `steady` 583 (581), `machine` 577 (586). W1AW piece-03 now reads `GEM BEFORE BURYING` where the first model printed `GEFORE`, and piece-04 reads `THE NORTH EAST`. The survey took 9.4 minutes. The shipped CPU figure is this run's; the last unit measured it at 108.6 s.

**The vector tests:** all four pass on the second model. The three new panel tests pass too, so `TheLearnedReaderTests` is 7 of 7, now on the engine line.

**What the panel shows with the switch on**, on the vector played through the tab's chain:
- **The decoding line** says `decoding at 579 Hz` from the third letter on.
- **The speed** shows `6 WPM, not proved` at 5 s and climbs to `20 WPM, not proved` by 12 s. It's a letter rate, letters and spaces in the last ten seconds, because the net names letters and measures no dit. It reads low until ten seconds have been heard.
- **The light** goes green (`reading`) from the first second, updates once a second, and goes dark in noise and in a gap of a second or more.
- **The tone line** is still the grid's, as before.
- With the switch off, the panel is exactly as shipped: `no station`, blank speed, no green light, as shown in section 2.

## 4. What's blocking us

**Nothing blocks the next unit. The two pending questions come back with this unit's figures.**

**1. Turn the learned reader on, with the second model.** *Pending the owner; restated, not argued.*

With the switch on, the second model reads KM3STU 55 of 65 where the first read 0, and W1AW 2128 against the shipped 2101. Ken is 51 against 43 and Bryan 124 against 118. The wild bank is 3271 against 2859, the fist bank 5086 against 3898, the 0 dB CQs 52 of 84 against 5, and CPU is down 58%. The panel now reads it. Against that, the twelve-plus-three is 286 against the shipped 308. The first recording reads 26 of 27, with its `G` lost, and the easy fixtures are 87 of 90 against 90. Letters in silences rise from 1 to 11, the 5 dB CQs fall to 83 of 84, and the random carrier prints at 20 of 20 seeds as shipped does.

- **Turning it on now:** the guards at 308, the first recording whole, one letter in silences, the easy fixtures at 90 and the 5 dB CQs at 84 would each have to be restated first. W1AW and the fist bank rise past their guards.
- **Leaving it off:** what the operator reads is unchanged, and the panel stays as blank as it is under the sequence reader.

**2. A learned letter is dimmed under the net's own probability of one half.** *Pending the owner; restated, not argued.* The line is unchanged this unit and is still unmeasured against the net's errors. The switch is off, so nobody sees it.

**Left, a line each:**
- **The order's claim that the sequence reader feeds the panel's pitch, speed and light is not true of the tree.** Under it the panel shows `no station`, no speed and no green light while it prints. Not changed: the sequence reader was not this unit's to touch. It is on screen today, and it is the same §0.0 question as the learned reader's panel.
- **The speed is marked `not proved`,** and that touches what the display asserts (section 1). Say if you want it otherwise.
- **The letter rate reads low** for its first ten seconds and after a long silence. The order's definition was kept as written.
- **The run budget was met:** one survey, one engine line, one app line, at the end. Afterwards I ran two guards alone because the records could touch them, plus one CPU trace.
- **`model\` at the root** is now a second copy of what went into the tree. Left, since it came in the zip.
- **The untracked `CwSequenceReader.cs.new`** is still there.

### Asks still outstanding

- **Unit 587, 2026-10-22:** the IC-7300's CW rise time, the owner's setting and the manual's default. `KeyingEdgeMs` is 4 ms, the default as the author recalls it from `A7292-4EX-6` p. 4-21, not re-read; the owner's setting is on no sheet. Waiting on the owner. The constant sits in `CwColumnFinder`, marked.
- **Unit 575, 2026-10-19:** what could part a hand's letter gaps from his word gaps, since they overlap in length and no line drawn from his dit alone parts every hand. Waiting on the owner. `CwRules.StraddleSplit` sits in the tree, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
