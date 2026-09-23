# The eight reds of 3.6 - the running record

This file is one record for every 3.6 unit (`PHASE_PLAN.md` R55, 3.6; work instruction 402
decision 3). **An attack** is a change that the trace chose. It is made to the decoder under
`src\Hamlet.RadioEngine\Cw` outside the eleven transmit files, or to the red test's own event
under R12. It is aimed at the cause the trace named, built, and measured on the red's own test. A
red that was read but got no change has not been attacked. Its row says *not attacked* and why, and
its count does not start. **Movement** is the red's own printed number moving toward its assertion:

| red | number that must move |
|---|---|
| #6, #15 | the share, up |
| #24 | whether a speed is named, and its value inside 14 to 24 |
| #41 | the count of quarter-second polls that named a speed, up |
| #42 | characters inside the preamble, down |
| #43 to #45 | unreadable plus not-in-the-message, down, or the ending matching more of the expected text |

A change that moves the number without turning the test green is *movement, not kept*. It is put
back unless another red's green keeps it, and the red's count starts again from the next unit.
**Three consecutive units** means three consecutive attempts at 3.6. This is the arbiter's
reading, the author's and overrulable. A 4.7 unit in between does not break the sequence. A 3.6
unit that leaves a red unattacked does break that red's sequence. After three consecutive attacks
with no movement, the red is parked as owed in `PARKED.md` with its three measurements.

## #6 - `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(wordsPerMinute: 25, floor: 0.79)`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | not attacked | misses at the first character on all three seeds (`Q` read `A` after `■`, bare start) and at the second call's tail on two; no cause found at a line | none: decision 7 attacks #6 only on a cause the trace names | 0.75 | 0.75 | - | red-open; sequence not started |

## #15 - `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 18)`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | not attacked | two of three seeds fit 23 wpm to a 12 wpm sender and read the message as dits (0.58, 0.11; the third 0.95); candidate `CwUnitEstimator.Measure` short-cluster medians or the `CwProbabilisticDecoder` speed grid, no single line | none: decision 7 | 0.54 | 0.54 | - | red-open; sequence not started |

## #24 - `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | `CwDecoder.cs` 682 to 686: a 25 Hz follow at 1.5 s, before any text, restarted the 12 s hold of `SpeedIsReacquiring` (421 to 425), longer than the 8.6 s signal | `CwDecoder.cs` 691: a follow is a discontinuity only when it moves at least half of `CwProbabilisticDecoder.BandwidthHz`; commit `7e65aac4` | none named | 18 | yes | **green by unit 402, `7e65aac4`, decoder** |

## #41 - `Fixtures.CwAdjudicationTests.ASpeedChangeInRealisticAudio`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | same line: 25 Hz follows at 1.5, 9.5 and 16.0 s each restarted the hold, then the 100 Hz handover at 25 s held it to the end of the file | as #24, commit `7e65aac4` | 0 of 144 polls | 82 of 144 polls, at 10 and 11; none between 15 and 18; none after the handover | yes | **green by unit 402, `7e65aac4`, decoder** |

## #42 - `Fixtures.CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | `CwDecoder.cs` 590 hands every hop to the stream while the tracker's `CwTransmitGuard` blocks it, 9.1 s here; every counted character reaches `LeadingEdge`, so the event stays (decision 5) | `CwDecoder.cs` 590: a blocked hop goes to `_probabilistic.Skip`; put back, not kept, because it cost `cw-2026-08-17-013347` 59 to 48 characters and 108 to 68 elements, `013622` 55 to 13 and 84 to 20, and the adjudicated `VA3VRR` | 70 | 0, green on its own type | yes, **green but not kept** | red-open, attempt 1 of 3; the next attempt needs the guard's spans told apart from real captures' |

## #43 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "coverage-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | `CwDecoder.cs` 139 to 142 raise the leading edge again at every revision, and the test reads `CharacterDecoded` at 203; on the settled transcript 4 unreadable and 7 strangers remain in `QRZ?` and `DE/`, no cause at a line | line 203 reads `CharacterSettled` (decision 4); put back, green on none of the three | 5 + 37 | 4 + 7 | yes, movement not kept | red-open, attempt 1 of 3 |

## #44 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "exchange-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | as #43; on the settled transcript 0 unreadable and 7 strangers in `DE N0` and the second call, no cause at a line | as #43, put back | 3 + 21 | 0 + 7 | yes, movement not kept | red-open, attempt 1 of 3 |

## #45 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "tightfist-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | as #43; on the settled transcript `VVVTESTDETESTK■`, one trailing placeholder | (a) line 203 reads `CharacterSettled`, put back; (b) `CwProbabilisticStream.cs` 507: the flush does not settle a trailing unreadable character still inside the delay; put back | 1 + 3 | (a) 1 + 0; (b) 1 + 3 | (a) yes, movement not kept; (b) no | red-open, attempt 1 of 3 |
