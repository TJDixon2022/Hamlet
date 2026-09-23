# Unit 405 - the second attack on the six reds

Work instruction 405, step 3 criterion 6 of *CW decodes again*. Every number here is an
indication (FACT-004); this machine has no radio (FACT-006). *Green* means the assertion held and
nothing more (§0.0). No sentence here says a change makes CW read.

## 1. Entry

HEAD at entry `f74b6d51`. Every item of the instruction's section 5 held as stated:
`CwDecoder.cs` 795 lines with `Skip` at 513, `Process` at 590 and the half-passband test at 701;
`SlowestWpm = 8` at 438 and `FastestWpm = 40` at 459; line 203 of `CwReceiverFixtureTests.cs`
reads `CharacterDecoded`; `TheEightRedsTests` asserts nothing and is on neither line; the closing
line of `docs\unit239-failing-set.txt` names #6, #15, #42, #43, #44 and #45 red-open. `git diff
f74b6d51 HEAD` over `src`, `tests` and the carry-forward list printed nothing, so the two lines at
entry are unit 404's exit (decision 5): engine 178 of 178, app 278 of 278.

| type | entry |
|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 93 s, every row identical to unit 404's exit table |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 |
| `CwAcquisitionWindowTests` | 10 of 12; #6 0.75 against 0.79, #15 0.54 against 0.66 |
| `CwReceiverFixtureTests` | 23 of 27; #42 70, #43 5 + 37, #44 3 + 21, #45 1 + 3 |
| `CwFixtureTests` | 22 of 23, `fading-18wpm` red as parked |
| `CwAdjudicationTests` | 11 of 11 |
| `CwEmissionGateTests` | 8 of 8 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `CapturedSignalTests` | 13 of 13 |
| `CwSpeedSilenceTests` | 4 of 4 |
| `WhyTheGateDidNotFireTests` | 2 of 2 |
| `CwTwoStationTests` | 5 of 5 |

The eleven transmit files print nothing against `7e209cb4`.

## 2. The trace

The printer is `tests\Hamlet.RadioEngine.Tests\Cw\TheSixRedsTraceTests.cs`. It asserts nothing and
is on neither line. It reads the stream's window, its held gaps, the decoder's reading callback and
the unit estimator's private `TwoMeansOnLogs` and `Otsu` by reflection, in the printer only. Its
three runs are `.run-unit\unit405-trace-b.txt`, `-d.txt` and `-c.txt`, gathered in
`.run-unit\unit405-trace.txt`. No file under `src` changed for the trace. The B1 and B2 print is in
section 2.4.

### 2.1 Group B, the easy tier, character by character

For each settled character the printer gives the read that settled it, the lattice's speed at that
read, the unit estimator's dit and its two clusters, the held gaps, and where the character aligns
against the recipe. Each fixture's own speed comes from its recipe: coverage-easy and exchange-easy
12.0 wpm (dit 100 ms), tightfist-easy 12.8 wpm (dit 94 ms, element gap 80).

- **coverage-easy.** Every stranger in `QRZ?` and `DE/` settles at a fitted **18.5 to 20.9 wpm**,
  1.54 to 1.74 times the sender: Y at 24.72 s, T 25.32, T 25.72, M 26.11, T 26.52, T 30.31, T 31.11,
  and the `■` at 22.61 and 27.72. At each of those reads the estimator's short mark cluster is 3 to
  7 marks of **20 to 35 ms**, while the sender's 105 ms dits have gone into the long cluster
  (`long 305 ms x27`). The unit is then (20 + 95) / 2 = 57.5 ms. At every read where the short
  cluster holds the dits, the fit is 12.0 and those characters are right. The structure is never
  held on this fixture, so the gaps play no part.
- **exchange-easy.** One stranger sits at a doubled fit: `M` at 14.12 s, settled at 20.9 wpm with a
  short mark cluster of three 20 ms marks, the same mechanism. **The other four strangers do not.**
  The `T`s at 22.71, 26.51, 27.28 and 28.47 s, and the run `E T E T E E E T E E` in the second
  call, settle at **12.0 wpm** with the structure held and the held gaps at **element 15 ms,
  character 1127 ms, word 323 ms**, and at 10/1690/250 and 20/845/365. The character gap is longer
  than the word gap. A 100 ms element gap is then longer than the element kind allows (2.2 × 15 ms)
  and too short for the other two, so it is read as a letter break, and each element becomes its own
  letter. `I` for `E` at 12.92 s and `,` for `0` at 16.32 s settle at 12.0 wpm on textbook gaps; the
  trace names no line for those two.
- **tightfist-easy.** The fit is 13.7 wpm throughout for a 12.8 wpm sender (1.07). The only fault is
  the one trailing `■`, pattern `.`, ending at 11.08 s and settled by the flush at 11.08 s.

**Whether the strangers sit at double the speed:** on coverage-easy, yes, at 1.5 to 1.7 times,
every one. On exchange-easy, one of seven (`M`). Four sit at the sender's speed under disordered
held gaps, and two have no traced cause.

**Lines named:**

1. `CwUnitEstimator.cs` 96, `ShortClusterMedian(marks)`, through `TwoMeansOnLogs` seeded at the
   tenth percentile, line 507. With a handful of sub-element noise marks in the window, the low seed
   starts on them and the two clusters come out as *noise* and *everything else*. The dits join the
   dahs, and the unit becomes the mean of a noise mark and the element gap.
2. `CwUnitEstimator.cs` 221 to 231, in `MeasureGaps`. The three-means element centroid lands on a
   cluster of 10 to 20 ms gaps, which are dropouts inside marks. The clip at 228 holds the boundary
   at 1.3 units, and line 231 carries it back as `character = boundary² / element`. That is
   130² / 15 = 1127 ms, longer than the word gap, which breaks the kinds' own order.
   `CwProbabilisticStream.cs` 459 to 464 adopts any separated read's gaps, including these, while
   the structure is held.
3. `CwProbabilisticStream.cs` 501 to 507: at the flush, `settleBefore` is past the window, so a
   trailing unreadable character still inside the delay is settled.

### 2.2 Group D, #15 and #6

**#15, 12 wpm, run-up, 18 dB.** Seed 7919 fits 12 throughout (share 0.95). Seeds 104729 and
15485863 leave 12 for **22.9** (unit 52.5 ms), then 24.0. On 104729 the leave is at 21.50 s. The
estimator's short mark cluster goes from `110 ms x14` at 21.00 s to **`15 ms x4`** at 21.50 s. The
long cluster takes the dits (`310 ms x28`), and the short gap cluster holds at 90 ms. So the
doubled speed is (15 + 90) / 2. The noise marks grow to x40 by 30.3 s as the window fills with
them.

**The speed grid does not choose it.** At 21.50 s the grid's own best is **12 wpm at 641.900**,
against 637.594 for the 22.9 the stream imposed. Before the leave, the grid's best is 8, 12, 14 or
24, with differences under 1 part in 600, as `WpmStep`'s remarks already record: the objective is
flat. The stream hands the estimator's speed to `Decode` as `atWordsPerMinute`
(`CwProbabilisticStream.cs` 431 to 435), and the grid is not searched while the estimator is
ready. 23 is not on the grid at all (8 to 40 in steps of 2).

**Line named:** the same as group B's first, `CwUnitEstimator.cs` 96 through 507. The doubled
hypothesis wins in the unit estimator, not in the speed grid.

**#6, 25 wpm, bare, 18 dB.** On all three seeds, the envelope's first second holds `C` and `Q`
whole. The trigger finds marks of 155, 65, 155 and 60 ms, a 130 ms gap, then 160, 155, 60 and
160 ms. **The first key-down is not cut.** It starts at 0.41 s against a 0.40 s lead-in. But the
stream mixes at **600 Hz** until 1.54 s, the tracker's bank centre, reported unmeasured
(`measured False`), and then at **650 Hz**. The sender is at 640. Marks mixed at 600 peak at
**-20.3 dB** and the one mixed at 650 at **-12.5 dB**. The lattice holds, on every seed, a word gap
ending 0.73 s, a 20 ms `#` ending 0.75 s, a word gap ending 1.47 s, then `A` (`.-`) ending
1.71 s. So `C` and the first three elements of `Q` are read as key-up, and `Q`'s last dit and dah
become `A`. The second read replaces the `M` it held at 3.0 s with `Q` for the *second* `CQ`. The
misses at the second call's tail on 7919 and 15485863 were not traced to a line.

**Line named for the `Q` that becomes `■ A`:** `CwDecoder.cs` 585 to 589 hands the mixdown the
tracker's unmeasured bank pitch. `CwProbabilisticStream.cs` 241 to 249 mixes each hop at the pitch
in force and never re-mixes the envelope it holds. The lattice then judges the 8 dB weaker marks
against a signal amplitude taken from the window's upper tail (`CwProbabilisticDecoder`
`LogLikelihoods`), and key-up wins over them.

### 2.3 Group C, #42

Every hop was fed as the floors feed captures. The printer wrapped the decoder's reading callback
to see each `ToneReading` with its broadband level, tone power, SNR and `Blocked`.
`CwTransmitGuard` exposes `IsMuted`, `IsBlocked`, `BlockedHops` and `Transmissions`, and no
reason.

| recording | length | spans | own transmit | muted broadband inside the spans | unblocked broadband median | settled characters inside a span |
|---|---|---|---|---|---|---|
| `qsk-preamble.wav`, 8 kHz | 26.0 s | 13, 1.04 to 11.67 s | 9.08 s | **-82.8 to -81.6 dBFS in every span**, medians -82.1 to -82.2 | -33.0 dBFS | 28 of 54, plus 2 in the lead-in and 1 at 12.78 s |
| `cw-2026-08-17-013347`, 48 kHz | 30.0 s | 25, 0.11 to 18.93 s | 15.68 s | -83.7 to -60.0 dBFS; medians -63.5 to -80.9 | -17.4 dBFS | **46 of 59** |
| `cw-2026-08-17-013622`, 48 kHz | 30.0 s | 16, 0.11 to 12.20 s | 10.08 s | -83.6 to -60.0 dBFS; medians -63.3 to -81.6 | -18.0 dBFS | **28 of 55** |

The `VA3VRR` adjudicated reading is on `013347` itself. Its characters start at 19.90 s, 0.97 s
after the last span.

**Is there one property that separates the fixture's own-send spans from every span on the real
captures?** Yes, but it separates synthetic from recorded audio, not the operator's sending from
anyone else's. The fixture's muted level is flat to about 1 dB inside every span, because the
generator writes a -82 dBFS residue (`CwFixtureGenerator.cs` 672 to 688). Every real span's median
sits at least 4 dB above its own minimum, and span medians range across 17 dB. The fixture's
unblocked audio is also 15 dB quieter, and it runs at 8 kHz against 48 kHz. **What the two real
captures hold inside their spans is the same thing the fixture holds**: the operator's own
full-break-in sending, with slivers between his elements read as `E`, `I`, `H` and `S`. 46 of
`013347`'s 59 capture-floor characters and 28 of `013622`'s 55 are those slivers. So no property of
the spans themselves tells the fixture's sending from the captures' sending. A skip conditioned on
the flat residue would never fire on the air. And any skip that does fire on real own-send spans
takes those 46 and 28 characters off two capture floors, which the keep rule forbids.

**Does the fixture feed the decoder any report of the operator's own transmission?** No. The test
at `CwReceiverFixtureTests.cs` 259 to 295 never calls `RadioIsTransmitting`, so
`DecodingSuspended` stays false and `SuspendedChunks` is 0 on all three recordings. The only
knowledge the decoder has is the guard's broadband test inside the tracker.

The fixture also counts characters no guard span covers: `I` at 0.03 to 0.19 s and `■` at 0.30 to
0.97 s in the lead-in noise before the first span, and `■` at 12.78 s, 1.1 s after the last.

### 2.4 B1 and B2 together, #45

B2 was written into `CwProbabilisticStream.cs` as an uncommitted change. At the flush, a character
the alphabet does not know, still inside the delay, is not settled. The printer's settled list
stands in for B1, line 203 reading `CharacterSettled`. The build was clean, and group B printed:

| fixture | HEAD, settled | B1 and B2 together |
|---|---|---|
| tightfist-easy | `VVVTESTDETESTK ■`, 1 unreadable, 0 strangers | **`VVVTESTDETESTK`, 0 unreadable, 0 strangers, ends with `TESTDETESTK`** |
| coverage-easy | `VVV 1234567890 ■ A■ Y TTM T■■ DE TEETEN0CALL` | identical |
| exchange-easy | `VVV CQ CQ I M ,CALL N T 0C E T E T E E E T E E K` | identical |

**#45's trailing placeholder is gone under B1 and B2, and nothing else on the three fixtures
moves.** The change was then put back through `.run-unit\unit405-putback.sh`. `git diff --stat
HEAD -- src tests` printed nothing but the new printer, and the rebuilt binary printed HEAD's
tightfist-easy `VVVTESTDETESTK ■` again (`.run-unit\unit405-trace-b-after.txt`).

### 2.5 Added in task 2: the pitch the stream is mixing at

When S1 left #15's third seed at 0.16 with its noise marks still growing, the printer's
mix-pitch line was widened from the first 3 s to the whole run and added to group B, with the
tracker's `Retunes` and `Follows` beside it. It is the printer only, and no `src` file changed for
it (`.run-unit\unit405-trace-b-mix.txt`, `-d-mix.txt`). **It moves the cause of #15, #43 and #44
upstream of the lines named above:**

| case | mixdown pitch against the sender | where the misses are |
|---|---|---|
| coverage-easy, 615 Hz | 625 from 1.54 s, 615 from 10.04, 600 from 13.04, **575 from 22.04 to 28.54**, then 625 | every stranger and doubled-fit read, 22.6 to 32.5 s |
| exchange-easy, 615 Hz | 625 from 1.54 s, **575 from 11.54**, 585 from 19.04, 625 from 20.04 | `I` 12.92, `M` 14.12, `,` 16.32 s inside the 575 stretch; the `T`s after it, under gaps read from windows holding it |
| tightfist-easy, 615 Hz | 625, 620, then 615 from 5.04 s | none but the trailing `■` |
| #15 seed 7919, 640 Hz | 650 from 1.54 s to 28.54 | none; 0.95 |
| #15 seed 104729 | 650, then **700 from 20.54 s**, 685, 700 | fit leaves 12 for 22.9 at 21.50 s |
| #15 seed 15485863 | 650, then **700 from 7.54 s**, 685 | noise marks grow from 8.5 s; 0.11 |
| #6 seed 104729 | 650, 640 from 3.54 s, 650, 640 | first character only; 0.89 |
| #6 seed 7919 | 650, 635, 640, 625, 650, **550 from 10.54 s**, 650 from 12.54 | first character and the second call's tail; 0.68 |
| #6 seed 15485863 | 650, 640, **725 from 10.54 s** | first character and the second call's tail; 0.68 |

Every move to a pitch 25 to 85 Hz off these single-sender recordings comes with a `Retunes`
increment, and all but two with a `Follows`. That is `CwToneTracker.Switch` (1154 to 1169),
reached from line 1092 when the coarse survey admits a keyed candidate outside the fine bank's
reach. On a recording with one station, the bin it moves to holds noise and the station's skirt,
and each pitch is reported as measured. The stream mixes at it (`CwDecoder.cs` 568 to 589), and
the marks come through attenuated: dropouts inside marks, sub-element noise marks, and the unit
estimator and gap lines above. **Lines 96 and 221 to 231 are where the wrong pitch becomes a
wrong reading. The tracker's switch is where the pitch goes wrong.** `CwDecoder.cs` 658 to 665
already records the same thing for the window clear: "every one of the three was the tracker
leaving a station it was reading for a bin holding noise ... what is wrong is upstream of it."
The #6 second-call tails on 7919 and 15485863 trace to the same switch at 10.54 s. This unit
attacked the causes task 1 named. The switch is named here for the next unit and was not changed.

### Findings, one per red

- **#45.** Cause at a line: `CwProbabilisticStream.cs` 501 to 507 settles a trailing unreadable
  character still inside the delay at the flush. On the settled transcript that is the only fault.
  Change: B1 and B2 together (decision 1).
- **#43, coverage-easy.** Cause at a line: `CwUnitEstimator.cs` 96 through 507. Every stranger and
  placeholder in `QRZ?` and `DE/` settles at a fit of 1.5 to 1.7 times the sender, from a short mark
  cluster of noise marks.
- **#44, exchange-easy.** Two causes at lines. Four `T` strangers and the second call's
  `E T E T E E E T E E` come from `CwUnitEstimator.cs` 221 to 231, adopted at
  `CwProbabilisticStream.cs` 459 to 464. The `M` comes from line 96, as #43. `I` and `,` have no
  traced line.
- **#15.** Cause at a line: `CwUnitEstimator.cs` 96 through 507. The grid prefers 12 at the read
  where the estimator imposes 22.9.
- **#6.** Cause at a line for the first character: `CwDecoder.cs` 585 to 589 with
  `CwProbabilisticStream.cs` 241 to 249, the mix at the unmeasured 600 Hz bank pitch for the first
  1.14 s of a 640 Hz sender. The second call's tail on two seeds: no line.
- **#42.** A measurable property was found, and it is the generator's residue, not the operator's
  sending. The real captures' spans hold the same own sending, and their floors count it. Not
  attacked; the reasoning is in section 3.
