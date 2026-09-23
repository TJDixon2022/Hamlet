# Unit 407 - the station still keying, and #45's tail on the air's terms

Every number here is an indication (FACT-004). Nothing here says a change makes CW read.

## 1. The entry round

HEAD at entry `9c8198b1`. `git diff --stat 24d14e9c HEAD -- src tests docs/carry-forward-tests.txt`
prints nothing, so both carry-forward lines at entry are unit 406's exit: ENGINE 178 of 178,
APP 278 of 278 (decision 8). Built at 1.13.94 with warnings as errors, RC 0.

Every list below is identical, name by name, to unit 406's exit list of the same type.

| type | entry |
|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 94 s, every row identical to unit 406's exit table |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 in 2 s |
| `CwAcquisitionWindowTests` | 11 of 12: #6 0.82 green, #15 0.54 |
| `CwReceiverFixtureTests` | 24 of 27: #42 green, #43 5 + 37, #44 3 + 21, #45 1 + 3 |
| `CwFixtureTests` | 22 of 23, `fading-18wpm` parked |
| `CwAdjudicationTests` | 11 of 11 |
| `CwEmissionGateTests` | 8 of 8 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `HamletDoesNotDecodeYourOwnSendingTests` | 6 of 6 |
| `CapturedSignalTests` | 13 of 13 |
| `CwSpeedSilenceTests` | 4 of 4 |
| `WhyTheGateDidNotFireTests` | 2 of 2 |
| `CwTwoStationTests` | 5 of 5 |

**The tracker readers**, unit 406's list:

| type | entry |
|---|---|
| `ThePitchCanBeHeldTests` | 5 of 5 |
| `CwSurveyThresholdPinTests` | 3 of 3 |
| `CwToneSurveyTests` | 5 of 5 |
| `CwTrackerSwitchTests` | 2 of 2 |
| `TheKeyingWitnessSaysNothingImpossibleTests` | 62 of 62 |
| `TheOperatorIsToldAboutASecondStationTests` | 5 of 5 |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 of 2 |
| `WhatDecodeScoringCostsTests` | 2 of 2 |
| `WhatBandwidthTheDecoderListensThroughTests` | 4 of 6 in 153 s: `MostRealRecordingsSitInTheWidestWindow` and `HoldingTheWindowLongInTimeReadsMore(cw-2026-08-18-004507.wav)` red at entry, as unit 406's exit; neither is in the 51-name set |
| `TheGateHasItsOwnWindowNowTests` | its two short methods, 4 of 4 in 1 s; `EveryWidthLeavesTheEmptyRecordingsSilent` unmeasured (section 3 item 5) |

The eleven transmit files print nothing against `7e209cb4`. `src\Hamlet.App` prints nothing
against `9c8198b1`.

**Mismatches against section 5 of the instruction**, reported and not repaired:

- H2's hold in `CwToneTracker.cs` is lines 958 to 978, its comment 958 to 965 and its `if` 966
  to 978 with `Switch(_heldSwitchHz)` at 974. The instruction says 957 to 966. Line 212, 1077
  to 1080 and 1091 are as stated.
- `PHASE_STATUS.md` as the launcher left it read `CURRENT_STEP: 0`, and at HEAD it names steps
  0 and 2 `not started` while the instruction says steps 1, 2 and 4 are done. Task 0 set
  `CURRENT_STEP: 3`; the step lines are not touched.
- The reload disagreement the instruction names does not hold as written: `PROJECT_STATUS.md`
  RULES_AT says HM-DEC-165 of 2026-09-19, and `CLAUDE.md` section 1 holds HM-DEC-165 dated
  2026-09-19 at line 361. A grep for `CPS-DEC-0167` in `CLAUDE.md` finds nothing.

## 2. The trace

Printer `tests\Hamlet.RadioEngine.Tests\Cw\TheStationStillKeyingTraceTests.cs`, run alone, 10 of
10 in 108 s, output in `.run-unit\unit407-trace.txt`. It asserts nothing and is on no line. No
file under `src` changed. **The printer disturbs nothing it reads:** #6 prints 0.89, 0.89 and
0.68 by seed, mean 0.82, and #15 prints 0.95, 0.58 and 0.11, mean 0.54, as their test does.
Every one of the 37 captures prints its floor's character count.

**How a move is recognized.** As unit 406's printer did, a move is `Retunes` going up. It is a
*hold* when `_heldSwitchHz` goes from a pitch to empty in the same hop. It is *cold* when no pitch
has been measured. Otherwise it is *direct*, through the confirm and the reach at 1107. A hold
that empties without a move is printed as *hold-dropped*, which is H2 at work.

**How the property is measured, decision 1.** Every hop, the printer reads the fine bank's centre
bin `_fineDb[mid]` against the reading's own `NoiseDb`, the band beside it through the same
filter. A mark starts at 10 dB over the band, `CwToneSurvey.InterferenceLiftDb`, and ends below
4 dB, which is the survey's 3 dB hysteresis either side. A 25 ms majority vote removes anything
shorter than the shortest dit. **Still keying** means at least one key-up between two marks
inside the span the hold waited through, from the hop `_heldSwitchHz` was set to the hop it
went. A direct or cold move has no hold, so its span is the half second before it. Nothing is
compared between two pitches. Beside it the printer gives the survey's own mark rule, which is
the centre bin's two-level split over its last 3 s with 3 dB hysteresis. That rule decides
nothing here.

### 2.1 The separation table

Every move on the reds, both handovers, and the five rows H1 cost. *Sender* is the fixture's
pitch. On a capture it is the pitch the decode ends on. *Marks / key-ups* are at the bank's
centre over the span, under the noise-referenced rule first and the survey's own rule second.

| case | time | caller | from, to | sender | way | span | marks / key-ups | survey rule | still keying |
|---|---|---|---|---|---|---|---|---|---|
| #15 seed 7919 | 1.535 s | cold | 600 to 650 | 640 | toward | 0.51 s | 1 / 0 | 1 / 0 | no |
| #15 seed 7919 | 28.535 s | hold | 650 to 600 | 640 | **away** | 1.01 s | 3 / 2 | 3 / 2 | **yes** |
| #15 seed 104729 | 2.035 s | cold | 600 to 650 | 640 | toward | 0.51 s | 3 / 2 | 3 / 2 | yes |
| #15 seed 104729 | 20.535 s | hold | 650 to 700 | 640 | **away** | 2.51 s | 9 / 8 | 8 / 7 | **yes** |
| #15 seed 15485863 | 1.535 s | cold | 600 to 650 | 640 | toward | 0.51 s | 1 / 0 | 1 / 0 | no |
| #15 seed 15485863 | 7.535 s | hold | 650 to 700 | 640 | **away** | 2.01 s | 5 / 4 | 4 / 3 | **yes** |
| coverage-easy #43 | 1.535 s | cold | 600 to 625 | 615 | toward | 0.51 s | 3 / 2 | 3 / 2 | yes |
| coverage-easy #43 | 13.035 s | hold | 625 to 600 | 615 | **away** | 0.51 s | 6 / 5 | 2 / 1 | **yes** |
| coverage-easy #43 | 22.035 s | hold | 600 to 575 | 615 | **away** | 4.51 s | 22 / 21 | 7 / 6 | **yes** |
| coverage-easy #43 | 28.535 s | hold | 575 to 625 | 615 | toward | 1.01 s | 11 / 10 | 6 / 5 | yes |
| coverage-easy #43 | 39.535 s | hold | 625 to 600 | 615 | **away** | 0.51 s | 6 / 5 | 0 / 0 | **yes** |
| exchange-easy #44 | 1.535 s | cold | 600 to 625 | 615 | toward | 0.51 s | 4 / 3 | 3 / 2 | yes |
| exchange-easy #44 | 11.535 s | hold | 625 to 575 | 615 | **away** | 2.01 s | 13 / 12 | 4 / 3 | **yes** |
| exchange-easy #44 | 20.035 s | hold | 575 to 625 | 615 | toward | 0.51 s | 5 / 4 | 2 / 1 | yes |
| tightfist-easy #45 | 1.535 s | cold | 600 to 625 | 615 | toward | 0.51 s | 4 / 3 | 3 / 2 | yes |
| #6 seed 7919, gate | 1.535 s | cold | 600 to 650 | 640 | toward | 0.51 s | 2 / 1 | 3 / 2 | yes |
| #6 seed 7919, gate | 4.535 s | direct | 650 to 625 | 640 | away | 0.51 s | 2 / 1 | 1 / 0 | yes |
| #6 seed 7919, gate | 7.035 s | hold | 625 to 650 | 640 | toward | 0.51 s | 3 / 2 | 2 / 1 | yes |
| #6 seed 7919, gate | 10.535 s | hold-dropped, H2 | 650, held 550 | 640 | away | 1.51 s | 9 / 8 | 9 / 8 | yes |
| #6 seed 104729, gate | 1.535 s | cold | 600 to 650 | 640 | toward | 0.51 s | 3 / 2 | 3 / 2 | yes |
| #6 seed 15485863, gate | 1.535 s | cold | 600 to 650 | 640 | toward | 0.51 s | 3 / 2 | 3 / 2 | yes |
| #6 seed 15485863, gate | 10.535 s | hold | 650 to 725 | 640 | away | 3.01 s | 15 / 14 | 16 / 15 | yes |
| two-station | 1.535 s | cold | 600 to 625 | 615 | toward | 0.51 s | 3 / 2 | 2 / 1 | yes |
| two-station | 9.535 s | cold | 625 to 600 | 615 | away | 0.51 s | 3 / 2 | 2 / 1 | yes |
| two-station | 16.035 s | cold | 600 to 625 | 615 | toward | 0.51 s | 5 / 4 | 1 / 0 | yes |
| **two-station, the handover** | 25.035 s | cold | 625 to 725 | 730 | **onto the answerer** | 0.51 s | 6 / 5, mean lift 5.8 dB | 5 / 4 | **yes** |
| two-station | 34.535 s | hold | 725 to 725 | 730 | level | 4.01 s | 17 / 16 | 13 / 12 | yes |
| #41, quarter-second chunks | 1.535 to 34.535 s | as two-station | the same five moves | | | | identical | identical | |
| **#41, the handover** | 25.035 s | cold | 625 to 725 | 730 | **onto the answerer** | 0.51 s | 6 / 5 | 5 / 4 | **yes** |
| `031838` | 1.535 s | cold | 600 to 500 | 525 | toward | 0.51 s | 2 / 1 | 3 / 2 | yes |
| `031838` | 8.535 s | hold | 500 to 525 | 525 | onto | 4.01 s | 1 / 0, mean lift 45.6 dB | **15 / 14** | no |
| `031905` | 1.535 s | cold | 600 to 500 | 300 | toward | 0.51 s | 2 / 1 | 3 / 2 | yes |
| `031905` | 13.035 s | hold | 500 to 300 | 300 | onto | 1.01 s | 1 / 0, mean lift 42.6 dB | **5 / 4** | no |
| `031905` | 18.035 s | hold | 300 to 500 | 300 | away | 0.51 s | 3 / 2 | 3 / 2 | yes |
| `031905` | 26.535 s | hold | 500 to 300 | 300 | onto | 0.51 s | 1 / 0, mean lift 47.1 dB | **3 / 2** | no |
| `032113` | 1.535 s | cold | 600 to 500 | 650 | away | 0.51 s | 2 / 1 | 2 / 1 | yes |
| `032113` | 21.035 s | hold | 500 to 600 | 650 | toward | 0.51 s | 1 / 0, mean lift 33.2 dB | **3 / 2** | no |
| **`032113`** | 26.535 s | hold | 600 to 650 | 650 | **onto** | 1.01 s | 6 / 5, mean lift 21.7 dB | 4 / 3 | **yes** |
| `032129` | 1.535 s | cold | 600 to 500 | 650 | away | 0.51 s | 2 / 1 | 3 / 2 | yes |
| `032129` | 13.535 s | hold | 500 to 650 | 650 | onto | 2.51 s | 1 / 0, mean lift 49.9 dB | **8 / 7** | no |
| `003016` | 1.535 s | cold | 600 to 675 | 670 | toward | 0.51 s | 4 / 3 | 3 / 2 | yes |
| `003016` | 22.035 s | hold | 675 to 650 | 670 | away | 2.51 s | 11 / 10 | 15 / 14 | yes |
| **`003016`** | 25.535 s | hold | 650 to 675 | 670 | **onto** | 1.51 s | 8 / 7 | 10 / 9 | **yes** |

**The other captures with a hold**, and every direct move on a capture:

| capture | time | caller | from, to | ends on | way | marks / key-ups | still keying |
|---|---|---|---|---|---|---|---|
| `003126` | 21.535 s | hold | 675 to 650 | 665 | away | 6 / 5 | yes |
| `003126` | 23.035 s | hold | 650 to 675 | 665 | toward | 3 / 2 | yes |
| `003126` | 23.535 s | direct | 675 to 650 | 665 | away | 5 / 4 | yes |
| `003126` | 24.035 s | direct | 650 to 675 | 665 | toward | 5 / 4 | yes |
| `001831` | 18.035 s | hold | 525 to 500 | 525 | away | 1 / 0 | no |
| `001831` | 19.535 s | direct | 500 to 525 | 525 | toward | 1 / 0 | no |
| `012823` | 14.535 s | hold | 500 to 450 | 450 | onto | 1 / 0 | no |
| `013402` | 7.035, 16.535, 21.035, 24.035, 27.035 s | hold | 525 and 550 alternately | 525 | two away, two toward, one level | 1 / 0 each | no |
| `013520` | 4.535, 9.035, 18.535 s | hold | 525 to 525, 550 to 550, 550 to 525 | 540 | level, level, away | 1 / 0 each | no |
| `021410` | 29.535 s | hold | 550 to 550 | 550 | level | 1 / 0 | no |
| `021825` | 24.535 s | hold | 400 to 400 | 400 | level | 1 / 0 | no |
| `032050`, `012748`, `012922`, `013010`, `013402`, `013520`, `013637` | 12 direct moves | direct | 25 to 175 Hz | | | 1 / 0 each | no |

The captures also make 124 cold moves between them, most of them on the five recordings that
hold nothing. 26 of them go while the centre is still keying.

### 2.2 Does the property separate? **No.**

**The first half holds.** Every move off a single sender on #15, #43 and #44 goes while the
bank is still keying. That is #15 at 28.535, 20.535 and 7.535 s, coverage-easy at 13.035,
22.035 and 39.535 s, and exchange-easy at 11.535 s. Each has between 3 and 22 marks at the
centre over the span.

**The second half fails.** These known-right moves go while the bank is still keying:

- **the two-station handover**, the cold move 625 to 725 Hz at 25.035 s, with 6 marks and 5
  key-ups at the old centre in the half second before it. The answerer's keying reaches a bank
  105 Hz below it, at a mean 5.8 dB over the band;
- **#41's handover**, which is the same move on the same audio fed in quarter seconds;
- **`032113` at 26.535 s**, a hold from 600 to 650 Hz onto the station the decode ends on, with
  6 marks and 5 key-ups at a mean 21.7 dB. This is the station's own keying, heard 50 Hz off
  through the bank's main lobe;
- **`003016` at 25.535 s**, a hold from 650 to 675 Hz onto a station at 670, with 8 marks and 7
  key-ups.

Under decision 1, **the property does not separate, and no tracker change is made.** The move
that breaks it most plainly is `032113` at 26.535 s. The pitch being read is still keying
because the station is 50 Hz away and still in the bank's reach, which is also why the tracker
is right to move onto it. The same holds for the moves toward the sender on the reds, which are
listed in neither half of the test, such as exchange-easy 575 to 625 at 20.035 s and
coverage-easy 575 to 625 at 28.535 s. Both go while still keying, and holding either one would
leave the tracker 40 Hz off the sender.

**Where the rule says *no* on the captures, it cannot see the key-ups.** The 17 holds and 13
direct moves on the captures that read *not keying* are each one unbroken mark across
the whole span at 31 to 50 dB mean lift. The centre never falls within 4 dB of the band beside
it, which is measured through the receiver's own skirt. The survey's own rule, a split in the
centre bin's own levels, finds 3 to 15 marks with key-ups on every H1 row's hold that the
noise-referenced rule calls *not keying*. **Under the survey's rule every known-right hold on
the five H1 rows goes while still keying.** So the separation fails under either rule, and the
*no* on those rows is a limit of the rule rather than a finding about them.

### 2.3 #15 seed 104729's reading level, decision 3

Every survey read from 17.5 to 21.5 s confirms 700 Hz. The tracker had confirmed nothing before
18.035 s: `_readingDb` is empty going in, so HM-DEC-127's floor had no level to test against.

| survey read | confirmed | `_readingDb` before, after | sender's own bins, keyed level |
|---|---|---|---|
| 18.035 s | 700 at -46.6 dB | empty, -46.6 | 625 -21.5, 650 -20.5 dB |
| 18.535 s | 700 at -46.1 dB | -46.6, -46.1 | 625 -21.7, 650 -20.7 dB |
| 19.035 s | 700 at -46.1 dB | -46.1, -46.1 | 625 -21.8, 650 -20.8 dB |
| 19.535 s | 700 at -45.6 dB | -46.1, -45.6 | 625 -21.9, 650 -21.0 dB |
| 20.035 s | 700 at -45.2 dB | -45.6, -45.2 | 625 -21.9, 650 -21.0 dB |
| 20.535 s | 700 at -45.6 dB, the move | -45.2, -45.6 | 625 -21.9, 650 -21.0 dB |

The sender's keyed level is the upper of two levels in each coarse bin's own history, found the
way the survey finds them. `_readingDb` stands **24 to 25 dB below** the sender's own keyed level
at every confirm, so the letter of decision 3's condition, more than 2 dB below, holds. **But it
is the image's own level, not a reading of the station.** The station was never confirmed on
this seed, so there is no reading level of it for line 1091 to keep. Decision 3 is judged only
as part of task 3's attack, and task 3 makes no change (section 2.2), so it is not applied.

### 2.4 #45's tail, and coverage-easy and exchange-easy, decision 4

The settle delay is `CwProbabilisticStream.DecisionDelaySeconds`, 1.0 s, and the stream reads
every 0.5 s. Decision 4's padding is 1.0 + 1.0 = **2.0 s, 16 000 samples of digital zero** at
8 kHz, fed in the source's own chunks after `PumpAll` and before `Flush`. The strangers,
placeholders and ending are computed from `CharacterSettled` exactly as the test computes them
from `CharacterDecoded`.

**tightfist-easy, #45.** The file is 11.082 s long. `K` settles at 9.595 s, pattern `-.-`. After
it the centre carries only short excursions, 15 to 60 ms each from 9.685 s to 11.035 s, which is
the fixture's noise. Nothing more is settled before the flush.

| run | settled placeholders + strangers | settled tail after `K` | ends with the message |
|---|---|---|---|
| the file as it is, then `Flush` | **1 + 0** | one `.` placeholder at 11.080 s, the file's last hop | no, only for the placeholder |
| with 2.0 s of the air's trailing silence, then `Flush` | **4 + 0** | `.` at 10.625, `-` at 10.985, `.` at 11.280 and `........` at 13.080 s, all placeholders | no |

**The decoder does not settle the placeholder away under the padding. It settles three more.**
The noise after `K` is read as three one-element placeholders once the stream has 2 s more
window to settle them in. Then the step from noise to digital zero is read as an eight-element
placeholder at the padding's end. The transmit guard does not take the zero for a mute: own
transmit reads 0.00 s and no chunk is suspended.

| fixture | as the file ends | with the padding |
|---|---|---|
| coverage-easy, #43 | 4 + 7 settled | 8 + 7: four placeholders added after the last `L` at 38.910 s |
| exchange-easy, #44 | 0 + 7 settled | 4 + 7: four `.` placeholders added after `K` at 30.510 s |

**Decision 4's condition does not hold. Task 2 makes no change, and #45 is not attacked.**

### 2.5 What follows for tasks 2 and 3

- **Task 2:** no change. #45's row says *not attacked*, because the padding adds placeholders
  and removes none.
- **Task 3:** no tracker change, under decision 1. #15's, #43's and #44's rows say *not
  attacked* and name `032113` at 26.535 s and the two handovers. Decision 3 is not applied,
  because it is judged only inside that attack. G1 is not made, because decision 5 allows it
  only on top of a kept or measured tracker change, and this instruction's section 10 forbids
  G1 alone.

## 3. The attacks

### Task 2 - #45 given the air's trailing silence

**No change.** Decision 4 allows the event change only if task 1's printer shows the decoder
settling the placeholder away under the padding. It shows the opposite. tightfist-easy's settled
transcript goes from 1 + 0 to 4 + 0 under 2.0 s of digital zero. coverage-easy goes from 4 + 7
to 8 + 7 and exchange-easy from 0 + 7 to 4 + 7 (section 2.4). `CwReceiverFixtureTests.cs` is not
touched, nothing was built, and nothing was run for this task. #45 stays at 1 + 3 on its own
test, *not attacked*. B1 alone is not made.
