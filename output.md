## 1. What Claude did

Surface: Claude Code on the development computer, branch `main`. The gate: the prompt said `PROJECT: Hamlet`; `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `tools\prototype\sequence-reader\cwhmm2.py` exist, `CoreHMI.sln` and `MURC.sln` do not, the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Hand run: `SESSION.lock` taken through `tools\arbiter\lock.bat take` and released at the end; nothing written to `RUN_LEDGER.md`; nothing under `tools\arbiter\` touched; no box ticked in `PHASE_PLAN.md`. Nothing keyed or transmitted, nothing written to the radio, no recording added, removed or promoted. The tag `before-sequence-reader` was set before the first change and pushed; the prototype's folder was committed unchanged first (`990f005e`). Version 1.13.273 to 1.13.274. HM-DEC-292 recorded; nothing recorded under §12.1.

**The port, `CwSequenceReader`**, behind `CwRules.SequenceReader`, off by default. Its static half is the prototype line for line: `Envelope` is `envelope_db`, `Decode` is `decode` (the explicit-duration Viterbi, cumulative emission sums, a flat prior of -log(range), dits and dahs after any gap or the start, gaps after a dit or dah), `RangesFromDit`, `Learn`, `Read` (three passes) and `Text`. Live, a lane every 25 Hz across the rig's passband, decoded over its last 12 s every 2 s from 6 s on, letters committed `CommitLagSeconds` 2 s behind the newest audio and printed through the lookup table. A lane prints only where its decode's pattern is Morse: dah share 0.2, dah tightness 0.12, separation 1.5, on 16 marks or more. With the rule on it is the only path to the printer: the grid's marks, the bank's handover and the column finder feed nothing, and the grid stays for the scope.

**Departures from the prototype, one line each:**
- the Butterworth is two RBJ biquads by the bilinear transform, prewarped at the cutoff, run per sample: the same filter `scipy.signal.butter` designs;
- the predecessor maximum is taken once per hop for the two predecessor sets, with the prototype's own tie order;
- the first dit, where none is given, is two-means on the run lengths above the midpoint, with hysteresis (on above 60 per cent, off under 40): the prototype's `cwhmm2.py` takes the dit as an argument;
- live, the lane is 16 Hz, not 10: lanes 25 Hz apart need a tone halfway between two to lose under half a decibel (0.45 dB at 16 Hz);
- live, a lane that printed carries the sender's rhythm and levels into its next window;
- live, a mark whose mean lies under the midpoint of the two learned levels is silence, since the longest word gap forces a mark into any long pause;
- live, a letter the window's start cut into is not printed;
- round two: a cold lane starts from several dits (below);
- round three: a station's lane holds through a window that dips (below).

**The first proof** (`TheSequenceReaderTests.TheFirstProof`), run before the survey. Built first with the tree's generator, the 0 dB case read 51 to 58 against the prototype's whole but one. The generator's noise is three cascaded band-passes, which put about 2.3 dB more noise at 615 Hz than a flat band of the same total. So the proof's audio is now built exactly as `synth.py` builds it: 12 kHz, Hann-gated 5 ms edges, Gaussian noise band-passed 350 to 870 Hz by fourth-order Butterworths, the signal measured in that band, 12 dB of QSB at 0.7 Hz. Built that way, the port matches the prototype's reads on every case at three seeds (section 3), and the band alone gives the prototype's own figures for nothing. The bars: whole, whole but one letter at 0 dB and at 5 dB fading, 52 of 60 for the fist, at every seed. The 18 WPM text is the fist bank's 60-letter ragchew, since the prototype's own text is not in the zip.

**The capture** (`TheCaptureReads`), at the prototype's 500 Hz, lane 10 Hz, dit 100 ms. 120 to 300 s, read as one stretch as the README has it, gives the name, the QTH, the report, `/M` and Ken's call. It does not read `589`, `NEW KENSINGTON` or `KB2UZK/M` whole, at any window start tried. The input is the same 48 kHz mono file, so the difference is unexplained without running the prototype. The test asserts what reads and names the three that do not.

**Round one** (`8241a44f`): the port as above. With it on: the fist bank 2817 from 1891, but the twelve-plus-three 177 and W1AW 2024.

**Round two** (`70d17639`). The largest loss was the twelve-plus-three. The trace (`TheLaneWindowsTraced`) named the cold start: from a dit guess at its 25 ms floor, the passes settled on dit 40 and dah 80 ms with one-hop inside gaps, and read every dit as a dah. KM3STU's `55N 55N` read `TTTTT TTTTT MT`; the first recording's `FER C` read `TTAT TTAT`. A floor at the lane's rise time was tried first; it changed nothing and was taken out. `ReadCold` now takes one pass from the guess and from 40, 70 and 120 ms, and keeps the start whose decode has the highest Gaussian log-likelihood under the levels it learned. The same windows read `NER C HAT <BT> BEST 7V 73 <SK> KC4ZGP` and `RR UR 55N 55N VA VA BK`. With it on: the board 231, but W1AW 1526.

**Round three** (`9c1b0633`). Traced on W1AW piece-03: under the AGC the printed lane's separation sat at 1.52 to 1.67. One window under 1.5 let it go, and every hold after began cold, on a crossed gap rhythm (letter gap 240 ms, word gap 210 ms) that merged letters into placeholders. The printed lane now stays a station, carrying its rhythm, while it read Morse within the last `YieldSeconds`, 4 s. Piece-03's holds and lets go fell from about 70 to 22. With it on: W1AW 1764, the board 187.

**Builds and runs.** The reader was built up over several builds and proof runs before round one's survey. Each round was then built once for its measure, with `SequenceReader` taken out of `OffByDefault` for that build and put back before its commit; the build servers were shut down after each. The guards with the rule on and the CPU measure were run in round three only. The record build, the solution whole with the rule off, was built once at the end and the build servers shut down. The app line, the engine line and the survey line then ran once each with `--no-build`, one at a time.

**Recorded** under the standing rules (HM-DEC-276, HM-DEC-284), not a ruling: HM-DEC-292 in `DECISIONS.md` and its `CLAUDE.md` §1 row; `docs\cw-scoreboard.md` 588 rows; `PHASE_OUTCOME.md`, both copies, `## UNIT 588 - STEP 12`; `PHASE_STATUS.md`, both copies, naming 588; `Directory.Build.props`; the survey's `Rerun` line.

## 2. What the owner should expect

**The first proof: the port matches the prototype.** On audio built the way `synth.py` builds it, the C# reader reads the 18 WPM ragchew whole at 10, 5 and 0 dB flat and at 10 dB fading. At 5 dB fading it reads 60, 59 and 59 of 60. The 12 WPM hand with a dah of 1.7 dits and 15% jitter reads 60 of 60 at every seed, where the prototype read about 52. Noise alone gives the prototype's own "nobody there" signature: dah share 0.11 to 0.15, separation 1.1. On Ken's QSO it reads `NICE TO MEET`, `57 9`, `I AM IN MOBILE STN`, `NAME BRYAN BRYAN`, `CHICOPEE, MA`, `PARKED IN MY MOBILE` and `KEN N3AFS DE KB■ZY/M`. It misses three things the prototype's table has: `589`, `NEW KENSINGTON` and the call whole.

**The whole picture, the sequence reader against two finders, at the end of round three with it on:**

| | two finders (shipped) | sequence reader |
|---|---|---|
| W1AW | 2111 (2131 right, 20 wrong) | 1764 (1833 right, 69 wrong, 0 invented) |
| the hand-sent recordings, the twelve-plus-three | 297 (318 right, 20 wrong, 1 invented) | 187 (265 right, 31 wrong, 47 invented) |
| the first recording | whole | `RER C HAT<BT> BEST 7V 73 <SK> KC4ZGP■` |
| a short-dah fist: the 1.6 and 2.0 rows of the bank | 1193 of 2880 | 1728 |
| the fist bank | 1891 (2612 right, 46 invented) | 3104 (3788 right, 1 invented) |
| a weak CQ at 5 dB: the four 5 dB CQs | 71 of 84 | 74 of 84 |
| a weak CQ at 0 dB | 0 of 84 | 6 of 84 |
| a fading station: the fading half, letters right | 439 of 2880 | 1360 |
| the flat half | 2173 | 2428 |
| the far side: `two-station` | 18 of 27 | 27 of 27 |
| the random carrier | 2 seeds of 20 | 20 seeds of 20 |
| letters in silences | 0 | 16 |
| strays summed over the 65 old captures, highest | 26.8, 3.3 | 60.8, 5.3 |
| CPU on W1AW `piece-04`, 283 s of audio | 181 s | 76 s |

**It does not ship.** `SequenceReader` stays off. The twelve-plus-three is 187 against 297 and W1AW 1764 against 2111, far outside the 1 per cent band for a ruling. The random carrier prints at every seed, 16 letters print in silences, and the first recording is not whole. What it does clear: the fist bank (3104 from 1891), the short-dah rows (1728 from 1193), the fading half (1360 from 439), the flat half (2428), the 5 dB column (185 from 157), the 5 dB CQs (74 of 84), the easy fixtures (77) and CPU (76 s).

**The reader in a sentence:** of every legal string of dits, dahs and gaps at the sender's own rhythm, it prints the one that best explains a narrow lane's whole envelope, wherever that string's pattern is Morse.

**Where it wins and where it loses.** It wins where the old finders broke a station into pieces: the fading hands (three times the letters), the far side of a QSO, the weak CQs, the short-dah hands, at less than half the CPU. It loses on the owner's real recordings in its live layer, not its method:
- **Cold starts.** A lane that has not yet learned its sender can settle on a wrong rhythm.
- **Holds.** The printed lane flickers under the AGC.
- **The pattern test.** A keyed random carrier passes it at every seed, since its two lengths look like dits and dahs.
- **Silence.** The path must lay a mark somewhere in every long pause, and some of those reach the page as letters.
- **Strong flat hands.** At 20 dB they read a little worse than shipped (dah 3.0 at 20 and 25 WPM: 45 and 43 against 60).

As shipped, nothing changes: 297 and 2111.

## 3. What you should see

**As shipped:** build clean, no warnings. The app line 278 of 278; the engine line 159 of 159 in 11.7 minutes; the survey line passed in 10.4 minutes, both boards 297 and 2111, its `Rerun` line naming 588. Pushed to `main`.

**The first proof** (audio as `synth.py`, lane and dit as the prototype's; the tree's generator in brackets):

| case | lane | seed 1 | seed 2 | seed 3 |
|---|---|---|---|---|
| 18 WPM, 10 dB flat | 10 Hz | 60 (60) | 60 (60) | 60 (60) |
| 18 WPM, 5 dB flat | 10 Hz | 60 (60) | 60 (60) | 60 (60) |
| 18 WPM, 0 dB flat | 10 Hz | 60 (51) | 60 (58) | 60 (54) |
| 18 WPM, 10 dB fading | 10 Hz | 60 (60) | 60 (60) | 60 (60) |
| 18 WPM, 5 dB fading | 10 Hz | 60 (58) | 59 (59) | 59 (59) |
| 12 WPM fist, dah 1.7, 15% jitter, 20 dB | 30 Hz | 60 (60) | 60 (58) | 60 (60) |

The band alone, three seeds: dah share 0.11, 0.15 and 0.11; tightness 0.16, 0.13 and 0.12; separation 1.10, 1.08 and 1.06; not Morse. A real sender: dah share 0.46 to 0.48, tightness 0.00 to 0.06, separation 1.63 to 4.07.

**The capture's reads**, `cw-2026-10-19-141708`, 500 Hz, lane 10 Hz, dit 100 ms:
- 0 to 60 s: `T F N B■L4 W A NST SI?■ 58 N XTH I I NEW D ENS ENBTON,<AS>S ■ NR ESITTSNIF`
- 120 to 300 s: `DLO GM KEN NICE TO MEETE E T ES TNXFER RPRT UR A I 57 9 57N LIL N I IAM IN MOBILE STN E E I■ NAME BRYAN BRYAN ES QTH IS I CHICOP E , MA CHICOP EE MW MA PARKED IN MY MOBILE <BT> SO TITU KEN N3AFS DE KB■ZY/ M …`
- live through the chain, the whole file: Ken's half `T UE N DEALL4 W A NST SI LGN 58T XTHI I N W D ENSENG TOIE…`, KB2UZK's as above.

**The rounds** (reader on):

| | shipped | round 1 | round 2 | round 3 |
|---|---|---|---|---|
| twelve-plus-three (right, wrong, invented) | 297 (318, 20, 1) | 177 (252, 43, 32) | 231 (284, 29, 24) | 187 (265, 31, 47) |
| W1AW (right, wrong) | 2111 (2131, 20) | 2024 (2055, 31) | 1526 (1605, 79) | 1764 (1833, 69) |
| fist bank (right, wrong, invented) | 1891 (2612, 675, 46) | 2817 (3399, 579, 3) | 3166 (3762, 595, 1) | 3104 (3788, 683, 1) |
| flat half, right | 2173 | 2190 | 2455 | 2428 |
| fading half, right | 439 | 1209 | 1307 | 1360 |
| 5 dB column, 20 WPM flat | 157 | 188 | 214 | 185 |
| four 5 dB CQs | 71 | 79 | 77 | 74 |
| 15 dB CQs | 78 | 70 | 84 | 84 |
| 0 dB CQs | 0 | 1 | 4 | 6 |
| letters in silences | 0 | 19 | 15 | 16 |
| strays summed, highest | 26.8, 3.3 | 65.6, 4.0 | 58.3, 4.3 | 60.8, 5.3 |
| first recording | whole | `TTAT TTAT E NTAT …` | `RER C HAT<BT> …` | `RER C HAT<BT> …` |

Round three's guards with the reader on: 155 of 159. Failed: the twelve-plus-three guard (187, carrier at 20 seeds, 16 in silences), the W1AW guard (1764), `NothingPrintsInTheSilenceAfterAWeakStation` (an `E` at 27.16 s), and `EveryHandReadsFortyEight` (0 of 60). The straight-key hands are read with no rig passband, where the reader is not built. Loud noise and the steady carrier were not reached, since the guard stops at the score.

**The three matrices**, letters right of 60, shipped / round 1 / round 2 / round 3:

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM |
|---|---|---|---|---|
| dah 1.6 | 60/36/59/59 | 60/0/32/32 | 60/0/45/47 | 60/0/13/13 |
| dah 2.0 | 59/53/52/52 | 60/59/59/59 | 60/46/58/58 | 22/0/28/35 |
| dah 2.5 | 59/56/57/57 | 59/60/60/60 | 60/58/56/58 | 60/27/48/52 |
| dah 3.0 | 60/60/60/60 | 60/60/60/60 | 60/48/48/45 | 60/44/53/43 |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB |
|---|---|---|---|
| dah 1.6 | 45/22/51/35 | 36/45/44/44 | 60/0/45/47 |
| dah 2.0 | 43/54/52/45 | 59/57/47/47 | 60/46/58/58 |
| dah 2.5 | 53/53/54/52 | 59/60/58/58 | 60/58/56/58 |
| dah 3.0 | 16/59/57/53 | 59/58/47/47 | 60/48/48/45 |

Signal by fade, dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz |
|---|---|---|
| 5 dB | 53/53/54/52 | 0/0/0/0 |
| 10 dB | 59/60/58/58 | 14/45/47/48 |
| 20 dB | 60/58/56/58 | 13/43/43/45 |

**The fixtures**, shipped / round 3 (letters right, invented):
- synthetic: 46 of 67, 0 / 60 of 67, 27 invented (`clean-12wpm` prints `T TTTT … EEEEE` in its digital silence);
- easy: 77 of 90 / 77 of 90;
- working: 4 of 81 / 17;
- edge: 0 of 81 / 30, 2 invented;
- ungraded: 73 of 98 / 83, with `two-station` 18 / 27 of 27;
- 15 dB CQs: 78 / 84 of 84.

**Both scoreboards' rows** (`docs\cw-scoreboard.md`), at the figures that ship:
- `| 588 | 2026-10-26 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | the sequence reader (work instruction 588, HM-DEC-292): three rounds, 187 with SequenceReader on (177, 231, 187), left off; the board unchanged |`
- `| 588 | 2026-10-26 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | the sequence reader (HM-DEC-292): 1764 with SequenceReader on (2024, 1526, 1764), left off; unchanged |`

## 4. What's blocking us

**Whether the port matches the prototype.** On the prototype's own cases, generated as it generates them, it does, letter for letter. On the capture it reads the same message with three tokens different, which cannot be traced further without running Python. So the next unit is the reader on the real recordings, not the port.

**Left, one line each:**
- **The carrier.** A carrier keyed at random passes the pattern test at every seed; dah share, tightness and separation do not tell it from a hand.
- **Silence.** Letters print in silences (16) and on a noiseless fixture: the longest word gap forces a mark into any long pause, and the midpoint filter does not catch them all.
- **The fading 5 dB hand** reads nothing in every round: its separation sits near 1.44 to 1.5 through the tree's generator, under the bar.
- **Cold start and hold** on the owner's recordings: the board moved 177, 231, 187 across the two fixes, and W1AW 2024, 1526, 1764.
- **Strong flat hands at 20 and 25 WPM** read worse than shipped (dah 3.0: 45 and 43 against 60).
- **Straight-key hands** are read with no rig passband in their guard, and the reader is built only across the rig's passband.
- **"Someone is there" at 0.2 dah share** refuses a window of `55N 55N`, which is mostly dits.
- **The generator's noise** is about 2.3 dB denser at 615 Hz than `synth.py`'s, so "0 dB" means different things in the proof and the bank.

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
