UNIT:       597 - complete at task 3 of 3 (one round of a possible three, the measure, the end) - 2026-11-05 11:45
PHASE GOAL: Hamlet meets its CW requirements.
UNIT GOAL:  Put the trained reader into the engine behind an off switch, prove the port against the shipped vector, and measure it for the owner's ruling.
ADVANCED:   no - the switch ships off, so nothing the operator runs reads differently; it gives the owner a measured candidate.
NUMBER:     with the switch on, Bryan 118 -> 127 of 144, Ken 43 -> 51 of 76; as shipped, unchanged
DRIFT:      not carried - the order gives no count to carry

## 1. What Claude did

**Complete.** Claude Code on the development computer, branch `main`, pushed. The prompt named `PROJECT: Hamlet`, and the order's five checks passed. Nothing here is evidence about the radio. Version 1.13.283 to 1.13.284. Rulings HM-DEC-301 and HM-DEC-302, the numbers the order named. The lock was taken with the root named and released, and it sat in `C:\Source\HamLet\SESSION.lock` this time. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. The draft truth was not edited. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted. No process I did not start was stopped.

**Round one, and the only one** (`a05b7fb0`, `fc54eea7`):
- `Microsoft.ML.OnnxRuntime` 1.31.1 restored from NuGet, with `cwnet.onnx` copied beside the engine and loaded once on one thread.
- `CwLearnedReader` built to the spec. All four vector tests passed on the first build that ran them.
- `CwRules.LearnedReader`, off by default. On, the sequence reader is not built. Off, the learned reader is never constructed.
- Letters reach the lookup table as dots, dashes and letter ends, so the terminal needed no change and nothing on screen moves.

**Rounds two and three were not run.** The order calls for them only if the vector test fails or the trace disagrees with the spec, and neither happened.

**One fix inside round one, in my own tagging and not in the port.** The first trace scored Bryan 89 of 144 with his text nearly right. I had tagged each letter's pitch from the log features, which swing widely in bins outside the radio's filter, so about 75 letters landed at the edge bins, 250 Hz and 950 Hz or more, and the scorer left them out of their stretches. Tagged from raw power, Bryan scores 127. The net's text is byte-identical before and after.

**Decisions I made for myself, all for the owner to overrule:**
- **A letter's mark.** The net gives no marks or pitch, and the scoring needs both. So each letter carries one mark measured afterwards: its span runs back from the frame that named it to the last letter, 0.6 s at most, and its pitch is the bin with the most raw power over that span. This decides no letter. The 0.6 s is my choice, and a wide span makes `invented` lenient, so read the zeros with that in mind.
- **Dimming.** A letter is dimmed where the net's own probability is under one half, the line the sequence reader uses for its own score. This touches what the display asserts, so it is also asked in section 4.
- **Spaces.** A word end at the very start of the stream or straight after another is dropped, so the terminal's spacing stays as it is. This adds no space.
- **Fixtures.** The vector files are in `tests\fixtures\cwnet\`, outside `tests\fixtures\cw\`, so the survey's folder count does not change. `Cw\Model\` holds the whole of `model\`, as the order says.
- **One extra trace, `TheEmptyRows`,** to learn why three board rows were empty. It is filtered, on no line, and not a survey.

**What went wrong:**
- **Stale trace.** A failed edit and build meant one trace ran on old binaries. Its output was identical to the run before, I discarded it, and no figure comes from it.
- **Failed starts.** The end run failed twice at its build because of a backslash in the survey's `Rerun` line. It stopped before measuring anything and put the switch back each time. Then a heredoc broke on an apostrophe, as the order warned; bash refused it whole, so nothing ran.
- **Memory notices.** At 11:08 the monitor reported the end run and two status loops stopped, with 3.7 GB free. I checked before acting: only the wrappers had gone, and the script and its test run were alive and ran to the end. So nothing was rerun, and there was one survey. The status loop was restarted in a form that watched the script.

## 2. What the owner should expect

**Ken and Bryan: the learned reader beats the shipped reader on both.** Right is the survey's figure for each 20 s, and the printed text is the learned reader's.

| Ken N3AFS | shipped right, wrong | learned right, wrong | learned printed |
|---|---|---|---|
| 0-20 s | 2, 6 | 2, 6 | `FNEALL4WANST SIZ9` |
| 20-40 s | 11, 6 | 13, 3 | `58N QTHII NWKENSEN` |
| 40-60 s | 10, 6 | 12, 4 | `GTON,PT PA NRPITTS NIU` |
| 60-80 s | 13, 2 | 14, 1 | `RGH TU NAME AS KEN KEN SG` |
| 80-100 s | 0, 0 | 10, 6 | `V W<AR> KB2UZ3TEN3RNS` |
| 100-120 s | 7, 5 | 0, 0 | `AN3AFS DE KB2UZY /M` |
| **all** | **43 of 76, 25 wrong** | **51 of 76, 20 wrong** | |

| Bryan KB2UZK/M | shipped right, wrong | learned right, wrong | learned printed |
|---|---|---|---|
| 120-140 s | 16, 2 | 16, 1 | `DLOGM KEN NICE TOMET` |
| 140-160 s | 16, 1 | 15, 1 | `UNX FER RPRT URR D57` |
| 160-180 s | 8, 2 | 10, 1 | `9 57N LILD IAMIN MO` |
| 180-200 s | 16, 3 | 17, 2 | `BILE STNMY NAME BRYAN B` |
| 200-220 s | 15, 2 | 17, 0 | `RYAN ES QTH IS CHICOP` |
| 220-240 s | 14, 2 | 16, 0 | `E, MA CHICOPEE , MA P` |
| 240-260 s | 18, 2 | 20, 0 | `ARKED IN MY MOBILE <BT> SO BT` |
| 260-280 s | 15, 3 | 15, 4 | `R KEN NWARS DE KB2UZY/M` |
| 280-300 s | 0, 0 | 1, 0 | `K R` |
| **all** | **118 of 144, 17 wrong** | **127 of 144, 9 wrong** | |

**The full text as the learned reader printed it:**
- Ken: `FNEALL4WANST SIZ9 58N QTHII NWKENSENGTON,PT PA NRPITTS NIURGH TU NAME AS KEN KEN SGV W<AR> KB2UZ3TEN3RNS AN3AFS DE KB2UZY /M`
- Bryan: `DLOGM KEN NICE TOMETUNX FER RPRT URR D579 57N LILD IAMIN MOBILE STNMY NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA CHICOPEE , MA PARKED IN MY MOBILE <BT> SO BTR KEN NWARS DE KB2UZY/MK RRDE NIWARSNIY TOME`

Bryan also gets his spaces nearly clean: 1 added against 19. It finds fewer of them, though, 29 of 40 against 36.

**Ken's 100-120 s row looks wrong against the draft truth, and it may be the truth.** The draft says `KB2UZK/M DE N3AFS K`, while both readers print `N3AFS DE KB2UZY/M`, in that order. That is one for your ear.

**The port matches the vector.** All four tests pass: the features within 1e-3 in every cell, the net on all 599 indices, and `ROGER TNX FER CALL` read through the chain in 100 ms pieces.

**The rest, with the switch on against shipped:**

| | shipped | learned reader |
|---|---|---|
| wild bank | 2859 | **3307**, wrong 274 to 91 |
| `mobile` | 522 | 548 |
| `bug` | 459 | 469 |
| `farnsworth` | 567 | 583 |
| `tight` | 489 | 540 |
| `steady` | 524 | 581 |
| `machine` | 572 | 586 |
| fist bank | 3898 | **5255** |
| twelve-plus-three | 308 | **238**, 251 of 353, 12 wrong, 1 invented, 9 in silence |
| W1AW | 2101 | **2048**, 2063 of 2151, wrong 24 to 15 |
| W1AW spaces | 440 of 440, 8 added | 368 of 440, 9 added |
| easy fixtures | 90 of 90 | 87 of 90 |
| 5 dB CQs | 84 of 84 | 84 of 84 |
| 0 dB CQs | 5 of 84 | 61 of 84 |
| noise | silent | silent |
| steady carrier | silent | silent |
| random-keyed carrier | prints at 20 of 20 seeds | prints at 20 of 20 seeds |
| first recording | whole | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA`, 26 of 27 |
| CPU, W1AW piece-04 | 108.6 s | 44.7 s, down 59% |

**Why the board fell.** KM3STU's three recordings at 800 Hz score 0 of 65: the net prints `KM3SKE CQ`, `RR` and nothing. I confirmed that is what the net prints, not the scorer dropping letters. Why it does not read him was not traced.

**What ships.** The switch is off, the shipped figures stand at 308, 2101, 3898 and 2859, and what you run reads as before.

**What will look wrong but is not:**
- `docs\cw-survey-2026-10-09.md` now shows the learned reader's figures, 238 and 2048, and its `Rerun` line says so. There is no switch-off survey, by the run budget; the shipped figures are on `docs\cw-scoreboard.md`.
- The survey's `WPM` column is blank for these reads, because the net measures no speed.
- The repository is about 11 MB larger, from the model and the vector.

**Test lines, switch off:** engine 168 of 168, app 278 of 278, `TheLearnedReaderTests` 4 of 4. After the record, `DecisionLogOrderTests` 2 of 2 and `TheScoreboardGuardsCwTests` 4 of 4. The unit took about 70 minutes, inside the budget's target.

## 3. What you should see

**The answer: the port is right, and the learned reader reads the real hands and every generated hand better than the shipped reader, but it reads the owner's boards worse, chiefly on a weak off-pitch station.**

**No visible change in the app.** The switch is off, so this unit only gives you figures to rule on.

**The trace beside the spec's stretches:**

| | the spec | the engine |
|---|---|---|
| Ken | `... QTH IIN W KENSENGTON, PT PA NR PITT NIURGH X NAME AS KEN KEN ... N3AFS DE KB2UZY/M` | `... QTHII NWKENSENGTON,PT PA NRPITTS NIURGH TU NAME AS KEN KEN ... AN3AFS DE KB2UZY /M` |
| Bryan | `GM KEN NCE TO MEET STNX FER RPRT URR 579 57N LIL NAM IN MOBILE STN AY NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA CHICOPEE M, MA PARKED IN MY MOBILE = SO BTR KEN NMARS DE KB2UZY/MK` | `DLOGM KEN NICE TOMETUNX FER RPRT URR D579 57N LILD IAMIN MOBILE STNMY NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA CHICOPEE , MA PARKED IN MY MOBILE <BT> SO BTR KEN NWARS DE KB2UZY/MK` |
| W1AW piece-03 | `... ENHANCEMENT MATERIAL GEFOR BURYING THE GROUND ROD. YOU CAN GET GEM FROM AN ELECTRICAL SPPLY HOUSE. ATTAC THE GROUND TO THE ANTENNA S RADIAL PLATE AND BOND CONNECT IT YOUR MAIN STATIO` | `WITH ROUND ENHANCEMENT MATERIAL GEM BEFORE BURYING THE GROUND ROD. YOU CAN GET GEM FROM AN ELECTRICAL SUPPLY HOUSE. ATTACI THE GROUND TO THE ANTENNAS RADIAL PLATE AND BOND CONNECTIT TO YOUR MAIN STATION` |
| W1AW piece-04 | not printed in the spec | `O CAN COM AR SIGNALSR THE NORTH EAST WEAK SIGNAL GROUP AND THE SAN DIEGO MIK ROWAVE GROUP GET TOGETHER TO COMPARE AND MEASURE STATIONS. IT IS ACHANCE TO COMPARE MINIMUM DIS CER` |
| the first recording | `FER CHAT= BST 7V 73 ! KC4ZGP DE WA` | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA` |

They agree to a letter or two a line, and they differ most in the spaces. `=` and `!` in the spec are `<BT>` and `<SK>` on the screen. The engine feeds the whole capture and the spec's windows may have been cut differently, which I would expect to account for small differences but did not test.

The spec quotes Ken 66 of 89 and Bryan 128 of 144 by letter. The survey's yardstick gives 51 of 76 and 127 of 144: it counts against the draft truth, its `?` places count no letter, and Ken's last stretch scores 0.

**The vector test:** pass, so there is no differing cell.

**The rounds:**

| round | vector | trace against the spec | twelve-plus-three | W1AW | wild bank | Bryan | Ken |
|---|---|---|---|---|---|---|---|
| one | 4 of 4 | agrees | 238 | 2048 | 3307 | 127 | 51 |
| two, three | not called for | | | | | | |

**The survey's groups, switch on, against the shipped survey:**

| group | shipped right, score | learned right, score |
|---|---|---|
| synthetic fixtures | 51 of 67, 42 | 60 of 67, 55 |
| receiver: easy | 90 of 90, 90 | 87 of 90, 86 |
| receiver: working | 36 of 81, 19 | 64 of 81, 57 |
| receiver: edge | 45 of 81, 35 | 51 of 81, 35 |
| receiver: ungraded | 72 of 98, 45 | 79 of 98, 71 |
| CQs: 15 dB | 84 of 84, 84 | 84 of 84, 84 |
| CQs: 5 dB | 84 of 84, 84 | 84 of 84, 84 |
| CQs: 0 dB | 5 of 84, 2 | 61 of 84, 50 |
| twelve-plus-three | 325 of 353, 308 | 251 of 353, 238 |
| W1AW pieces | 2125 of 2151, 2101 | 2063 of 2151, 2048 |
| fist bank | 4461 of 5760, 3898 | 5425 of 5760, 5255 |
| wild bank | 3133 of 3528, 2859 | 3398 of 3528, 3307 |
| wild hands | 161 of 220, 119 | 178 of 220, 149 |

Every file read. The strays bar: highest capture 1.5 per 10 s, under the line of six. The survey took 9.6 minutes.

**CPU on W1AW's 283 s piece-04:** 108.6 s shipped, 44.7 s learned, process time, one warming read of each thrown away. That counts the whole chain, so it includes the grid. The learned figure is lower because the sequence reader does not run.

## 4. What's blocking us

**Nothing blocks the next unit. One question needs your ruling, and two more follow from it.**

**1. The learned reader stays off for one more unit, then you rule on turning it on.** *Proposed; your call.*

It reads every hand better and costs less than half the CPU, but three things stand in the way today. It reads nothing of KM3STU, W1AW drops 62 letters right though 9 fewer are wrong, and the panel is not wired to it. The next unit would trace KM3STU and wire the panel.

- **Turning it on now, rejected:** the board falls to 238, and the guards at 308 and 2101, the first recording and the easy fixtures would all have to be restated first.
- **Dropping it, rejected:** it beat the shipped reader on the real hands in its first run, trained on no recording, where four units of rules gained five letters.

**2. A learned letter is dimmed under the net's own probability of one half.** *Proposed; it touches what the display asserts, so it is yours.*

The switch is off, so nobody sees it yet. Without some such mark, every letter would show as sure. The line is the one the sequence reader already uses for its own score, but the two numbers measure different things, and I did not measure how well the net's probability tracks its errors.

- **Every letter at full strength, rejected:** it would claim more than the net knows (§0.0).

**3. The panel with the switch on.** From reading the code, not from running the app: the detector hands on no printed station, so the printed pitch and speed have nothing to read, and the light may not reflect what prints. This needs wiring before the switch goes on.

**Left, a line each:**
- **The random carrier** still prints at all 20 seeds, as shipped does. It is 20 to 30 letters of confident text per seed, and the net knows no words to refuse it.
- **Letters in silences** rise from 1 to 9 on the board. Not traced.
- **W1AW's slow pieces** lost the most spaces and letters: piece-01 reads 222 of 242, with 38 of 63 spaces. Not traced.
- **Ken's draft truth at 100-120 s:** see section 2. Not edited, as ordered.
- **Is the run budget met?** The order is within it, so nothing was refused. The quick measure took 9 minutes, not the budget's five.
- **`TheLearnedReaderTests` is on no carry-forward line.** The order did not name it, so I ran it alone. It belongs on the engine line if the reader stays.
- **`model\` at the root** is untracked and is now a second copy of what went into the tree; it can go. Left, since it came in the zip.
- **The stray `CwSequenceReader.cs.new`** is still there, untracked.
- **Not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md`, `tools\go-claude\GoClaude.bat` and `.run-unit\`, and the `cases-*.txt` fixtures.
- **The order's shape:** it has no `PHASE GOAL`, `ADVANCES` or drift count, which `CLAUDE_CODE.md` §4.2 asks for. I filled the header from what the order says.

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
