# Unit 406 - the tracker switch, and #42 given the radio's word

Every number here is an indication (FACT-004). Nothing here says a change makes CW read.

## 1. The entry round

HEAD at entry `527b1659`. `git diff ef34d601 HEAD -- src tests docs/carry-forward-tests.txt`
prints nothing, so both carry-forward lines at entry are unit 405's exit: ENGINE 178 of 178,
APP 278 of 278 counting neither way (decision 7).

| type | entry |
|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 94 s, every row identical to unit 405's exit table |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 in 1 s |
| `CwAcquisitionWindowTests` | 10 of 12: #6 0.75, #15 0.54 |
| `CwReceiverFixtureTests` | 23 of 27: #42 70, #43 5 + 37, #44 3 + 21, #45 1 + 3 |
| `CwFixtureTests` | 22 of 23, `fading-18wpm` parked |
| `CwAdjudicationTests` | 11 of 11 |
| `CwEmissionGateTests` | 8 of 8 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `CapturedSignalTests` | 13 of 13 |
| `CwSpeedSilenceTests` | 4 of 4 |
| `WhyTheGateDidNotFireTests` | 2 of 2 |
| `CwTwoStationTests` | 5 of 5 |

**The tracker readers.** Grep for `CwToneTracker` over `tests\Hamlet.RadioEngine.Tests`, plus
`ThePitchCanBeHeldTests`, compiled types only:

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
| `WhatBandwidthTheDecoderListensThroughTests` | 4 of 6 in 154 s: `MostRealRecordingsSitInTheWidestWindow` and `HoldingTheWindowLongInTimeReadsMore(cw-2026-08-18-004507.wav)`, 48 against 50, red at entry; neither is in the 51-name set |
| `TheGateHasItsOwnWindowNowTests` | **did not fit.** 300 s, then alone at 590 s: the test host was killed with no result either time. Its two short methods, run alone, 4 of 4 in 1 s. `EveryWidthLeavesTheEmptyRecordingsSilent` is unmeasured |

`ThePeakAgainstASecondSignalTests` and `TheQuietestBinNoLongerWinsTests` name the tracker but
are removed from compile in the test project, so they are not run.

The eleven transmit files print nothing against `7e209cb4`.

## 2. The trace

Printer `tests\Hamlet.RadioEngine.Tests\Cw\TheTrackerSwitchTraceTests.cs`, run alone, 10 of 10
in 102 s, output in `.run-unit\unit406-trace.txt`. It asserts nothing and is on no line. No
file under `src` changed. `Switch` is private, so a call is recognized from outside: `Retunes`
goes up and a pitch is reported afterwards. A retune with no pitch reported is the cold move at
`CwToneTracker.cs` 1004, printed as *cold*. A move made through the hold is recognized by
`_heldSwitchHz` going from a pitch to empty. Both survey banks are read by reflection one hop
before each survey read. **The printer disturbs nothing it reads:** #6 prints 0.75 and #15
0.54, as their test does, and every capture prints its floor's character count.

### 2.1 Every call into `Switch`

59 calls into `Switch` across the reds, the two-station fixture and the 37 floor captures,
besides the cold moves. On the reds (sender 640 Hz for #6 and #15, 615 Hz for the easy tier):

| case | time | caller | from, to | sender | way | survey read when it went |
|---|---|---|---|---|---|---|
| #6 seed 7919 | 4.535 s | 1092 | 650 to 625 | 640 | 25 Hz refinement | direct, confirm 650 then 625 |
| #6 seed 7919 | 7.035 s | 959, hold | 625 to 650 | 640 | toward | admits nothing: **stale** |
| #6 seed 7919 | 10.535 s | 959, hold | 650 to 550 | 640 | **away, 90 off** | best 625, the sender's bin: **stale** |
| #6 seed 7919 | 12.535 s | 1092 | 550 to 650 | 640 | toward | direct, confirm |
| #6 seed 15485863 | 10.535 s | 959, hold | 650 to 725 | 640 | **away, 85 off** | best 650, the sender's bin: **stale** |
| #15 seed 7919 | 28.535 s | 959, hold | 650 to 600 | 640 | away, 40 off | re-admitted at 600 |
| #15 seed 104729 | 20.535 s | 959, hold | 650 to 700 | 640 | **away, 60 off** | re-admitted at 700, lift 11.9, keyed -45.6 dB |
| #15 seed 15485863 | 7.535 s | 959, hold | 650 to 700 | 640 | **away, 60 off** | admits nothing: **stale** |
| exchange-easy | 11.535 s | 959, hold | 625 to 575 | 615 | **away, 40 off** | admits nothing: **stale** |
| exchange-easy | 20.035 s | 959, hold | 575 to 625 | 615 | toward | re-admitted |
| coverage-easy | 13.035 s | 959, hold | 625 to 600 | 615 | away, 15 off | admits nothing: **stale** |
| coverage-easy | 22.035 s | 959, hold | 600 to 575 | 615 | **away, 40 off** | admits nothing: **stale** |
| coverage-easy | 28.535 s | 959, hold | 575 to 625 | 615 | toward | admits nothing: stale |
| coverage-easy | 39.535 s | 959, hold | 625 to 600 | 615 | away, 15 off | admits nothing: stale |
| tightfist-easy | - | - | no switch | 615 | - | - |

For each, the printer gives the time and caller, the pitch moved from and to, the sender's
pitch from the fixture's request, the verdict's keyed and interference readings, the reading's
power and noise, every coarse bin the survey admitted, the fine bank's own verdict at the old
centre, the HM-DEC-127 floor reading, and which condition let it through (`move-*` rows).

**Which condition let it through.** Of the ten moves away from a single sender, nine went
through **the hold** at 959. One, #6 seed 7919 at 4.535 s, went through the confirm and the
reach at 1092, and it is a 25 Hz refinement inside one coarse bin. None was stopped or allowed
by the HM-DEC-127 floor. Where a reading level existed, the candidate sat between 0.4 and
1.2 dB of it.

### 2.2 The property

**A held switch goes at 959 without asking the survey it is going on.** The hold is set at
1087 when a candidate is confirmed twice while a character is part-read. It goes at the first
later survey read with `MidCharacter` false, and that happens *before* the survey is analysed
at 963. The comment at 1084 says *the candidate keeps being re-confirmed while it waits, so a
switch deferred is not a switch abandoned.* Nothing re-confirms it. A candidate that
disappears while the hold waits is switched to anyway.

- **The wrong moves off a single sender:** 7 of the 9 that are not a 25 Hz refinement are
  **stale holds**. At the survey read where the hold went, the survey admitted nothing, or
  its best bin was the sender's own: 625 or 650 for a 640 Hz sender on #6. The other two are
  #15 seeds 104729 and 7919, where the survey still admits the far bin when the hold goes. At
  104729 that bin is 60 Hz off at -45.6 dB, about 23 dB below the sender, while the sender
  sends the five dahs of `0` and its own bins show no two-length structure. That is a
  different cause, and it is not what this property separates.
- **The right moves:** no known-right switch is stale. The two-station fixture's real
  handover, 625 to 725 at 25.035 s, is **not a switch at all**. It is a cold move at 1004,
  because the 5 dB first station was never confirmed. Its only `Switch` call, at 34.535 s, is
  re-admitted. On the captures, 15 held switches are re-admitted, each a step of 25 Hz or
  less: `013347` 28.0 s, `004507`, `003016`, `003126`, `001831`, `013402`, `013520`, `021410` and
  `021825`. **Ten held switches on captures are stale**, and whether each was right cannot be
  known from the audio: `013347` 22.5 s; `031838` 8.5 s; `031905` 13.0, 18.0 and 26.5 s, which
  flip 500, 300, 500, 300; `032113` 21.0 and 26.5 s; `032129` 13.5 s; `012823` 14.5 s;
  `013402` 16.5 s; `013520` 18.5 s. Task 2's gate judges them.
- **Named: a held switch the survey read at its execution no longer admits.** It separates
  7 of the 9 wrong single-sender moves from every known-right one. It does not reach #15's
  re-admitted image moves.

**`021410` and `013637`.** `021410` has one switch, a re-admitted hold 550 to 550 at 29.5 s.
`013637` has one direct switch, 525 to 550 at 24.0 s. A capture where the tracker moves
correctly: `004507`, 525 to 500 at 8.0 s through a re-admitted hold, the final pitch 500.

**Where the misses sit.** On #6 every miss after the first character starts at the stale hold,
0.00 to 2.13 s after it. On #15 seeds 104729 and 15485863, four invented `T` come 0.4 to 1.4 s
before the move, and everything after it is lost for the rest of the message. On the easy tier
the misses follow the moves at 11.5 and 22.0 s, as unit 405 found.

### 2.3 #6's first 1.14 s

**Not a switch still to be made. It is the tracker's initial pitch.** `CwDecoder(…, 600)` hands
600 Hz to `CwToneTracker`'s constructor, which centres the bank there (`CwToneTracker.cs` 382).
The mix stays at 600 until the survey is ready, at half its 3 s history, and the cold move at
1004 goes at 1.535 s to 650 on all three seeds. The first `Q`, ending at 1.71 s, is read `A` on
all three seeds. No change to `Switch` reaches it.

### 2.4 #42's plumbing

**The span.** It comes from the recipe and the sidecar, never the audio. `CwFixtureCatalogue`
gives `qsk-preamble` `PreambleSeconds: 12`. The generator's lead-in is 1.0 s, and the sidecar
`qsk-preamble.txt` says `messageStart 13.00 s` and `preamble 12.0 s of own-transmit mutes, 44
spans`. So the operator sends from 1.00 to 13.00 s, which is the window the test already uses.

The fixture was pumped a chunk at a time, 960 samples or 120 ms. Before each chunk the decoder
was told `RadioIsTransmitting(true)` for every chunk overlapping the span, and `false` after it:

| clock handed over | suspended in span | resumes | `during` | total | settled | `OwnTransmitSeconds` |
|---|---|---|---|---|---|---|
| none, the test as it stands | 0 of 101 | - | **70** | 134 | 65 | 9.08 s |
| audio time, epoch plus sample time | 101 of 101 | 13.68 s | **0** | 48 | 23 | **0.00 s** |
| the wall clock, `DateTime.UtcNow` | 101 of 101 | never | 0 | 0 | 0 | 0.00 s |

- **`DecodingSuspended` holds across the whole span when the clock is audio time.** No `src`
  change is needed for the clock, because `nowUtc` is a parameter and the test's own event can
  pass audio time. On the wall clock the pump outruns `ResumeAfter`, and decoding never
  resumes.
- **Line 501's skip leaves nothing that settles later inside the window.** The earliest
  character raised is at 15.465 s and the earliest settled at 13.710 s. `during` is 0.
- **But the first assertion would go red.** `OwnTransmitSeconds` is the guard's blocked hops
  (`CwDecoder.cs` 253). The guard only sees audio through `_tracker.Process`, and line 501
  skips the tracker, so it reads 0.00 s against `> 3`. **The change needs one line under
  `src`, and not a clock.** Somewhere in the suspended arm, the guard alone must go on
  observing the audio's broadband level. The survey must not, because 503 to 507 keeps it off
  the sidetone.
- **What the radio's word costs the message.** Suspension runs to 13.5 s under
  `ResumeAfter`, so the settled text starts `STDETESTK`. The first `TE` of the message is not
  read. The test asserts nothing about it.

## 3. The attacks

Decision 3's order: the tracker change, then #42, then G1. Each change was built once and run one
type per invocation with `--no-build`, starting with the reds' own types, then the gate. Every
put-back went through `.run-unit\unit406-putback.sh` and left `git diff --stat HEAD -- src tests`
empty, and the tree was rebuilt afterwards.

| change | file and line | reds' numbers | gate | kept |
|---|---|---|---|---|
| H1, the first shape: a held switch goes only if the survey it goes on still admits keying within 25 Hz of the held pitch; otherwise the hold is dropped | `CwToneTracker.cs` 957, `ReadSurvey` | **#6 0.75 to 0.89, green**; #15 0.54 to 0.61; #43 5 + 37 to 4 + 32; #44 3 + 21 to 2 + 20; #45 1 + 3; #42 70 | captures **35 of 37**: `003016` 57 to 55 and `031838` 57 to 37 red; also lower `031905` 42 to 37, `032113` 55 to 48, `032129` 66 to 44; up `013402` 61 to 62, `013520` 60 to 61; adjudicated 13 of 13; synthetics 2 of 2. The stale holds on those captures moved onto the station, which ends at 650 Hz | no, put back. The remaining gate types were not run once the floors failed |
| **H2**, the narrower shape at the same property: a held switch is dropped only when the survey it goes on admits the keying back within 25 Hz of the bank's centre and not within 25 Hz of the held pitch. A survey admitting nothing lets the hold go, as before | `CwToneTracker.cs` 957 | **#6 0.75 to 0.82 against 0.79, green**; #15 0.54; #43 5 + 37; #44 3 + 21; #45 1 + 3; #42 70 | captures 37 of 37, **every row identical to entry**; adjudicated 13 of 13; synthetics 2 of 2; `CwReceiverFixtureTests`, `CwFixtureTests`, `CwAdjudicationTests`, `CwEmissionGateTests`, `CwDisplacementFloorTests`, the four speed readers and every tracker reader identical to entry; `WhatBandwidthTheDecoderListensThroughTests` 4 of 6 as entry, 48 against 50; the gate-window reader's short methods 4 of 4 | **yes, `775907b6`** |
| #42 given the radio's word: the test's event pumps a chunk at a time and reports `RadioIsTransmitting` from the recipe's span, 1.00 to 13.00 s, on the audio clock; the decoder's suspended arm hands the transmit guard each hop's level and nothing else | `CwReceiverFixtureTests.cs` 280; `CwDecoder.cs` 513 and `ObserveOwnTransmission` | **#42 70 to 0, green**, own transmit 9.08 to 9.5 s against > 3; the others as H2 | `CwReceiverFixtureTests` 24 of 27, only #42 changed; captures 37 of 37 every row identical; adjudicated 13 of 13; synthetics 2 of 2; `CwEmissionGateTests` 8 of 8; `HamletDoesNotDecodeYourOwnSendingTests` 6 of 6 | **yes, `099b3a2a`** |
| G1 on top of H2 and #42's change, as one attack on #44 (decision 4): a clipped reading whose character gap comes out past its word gap is returned not separated | `CwUnitEstimator.cs` after 237 | #44 3 + 21 to 4 + 16; #43 5 + 37; #45 1 + 3; #6 0.82; #15 0.54; #42 green | captures 37 of 37, only `021825` up, 41 to 42 characters and 74 to 75 elements | no, put back: no red turned green |

**3.6 is not ticked.** Four of the eight have a verdict: #24 and #41 by unit 402, #6 and #42 by
this unit. #15, #43, #44 and #45 are red-open, and each count is at 0 of 3: #15, #43 and #44
moved under H1, and #45 was not attacked.
