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
| 405 | attack 2 | `C` and `Q` are whole in the envelope, but the stream mixes at the tracker's unmeasured 600 Hz for the first 1.14 s of a 640 Hz sender (`CwDecoder.cs` 585 to 589, `CwProbabilisticStream.cs` 241 to 249). Those marks come through 8 dB down and the lattice reads them as key-up. The second call's tail on two seeds follows a tracker switch to 550 and 725 Hz at 10.54 s (`CwToneTracker.cs` 1092, 1154 to 1169), not attacked | M1, `CwProbabilisticStream.cs`: the held envelope is mixed again from the held audio at the tracker's new pitch while nothing has been read; M2, the same only before the window's first read; each put back | 0.75 | M1 0.86, M2 0.86, green on its own type | yes, **green but not kept**: captures 26 and 27 of 37, ten rows lower under M2, among them `021410` 47 to 37 and `012748` 4 to 0 | red-open; moved, so under the head's rule the count starts again: sequence 0 of 3 |

## #15 - `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 18)`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | not attacked | two of three seeds fit 23 wpm to a 12 wpm sender and read the message as dits (0.58, 0.11; the third 0.95); candidate `CwUnitEstimator.Measure` short-cluster medians or the `CwProbabilisticDecoder` speed grid, no single line | none: decision 7 | 0.54 | 0.54 | - | red-open; sequence not started |
| 405 | attack 2 | the doubled fit is the unit estimator's, not the grid's: `CwUnitEstimator.cs` 96, through the tenth-percentile seed at 507, takes 4 noise marks of 15 ms as the dit cluster, so the unit is (15 + 90) / 2 = 52.5 ms. At that read the grid prefers 12 (641.9 against 637.6). Upstream, the two failing seeds' mix moved to a measured 700 Hz for a 640 Hz sender, at 20.54 and 7.54 s (`CwToneTracker.cs` 1092, 1154 to 1169), not attacked | S1, `CwUnitEstimator.cs` 96: marks shorter than 0.45 of the element gap, the lattice's own shortest share, are left out of the dit cluster; put back | 0.54 | 0.61 | yes, movement not kept: no red green, captures 23 of 37, among them `021825` 41 to 32 characters | red-open; moved, so the count starts again: sequence 0 of 3 |

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
| 405 | not attacked | every guard span printed on the fixture and on `013347` and `013622`. The one property separating the fixture's spans is the generator's flat -82 dBFS residue, -82.8 to -81.6 in every span against -83.7 to -60.0 on the captures, along with its 8 kHz rate and 15 dB lower level. The captures' spans hold the same own sending, and 46 of `013347`'s 59 and 28 of `013622`'s 55 floor characters sit inside them. The fixture reports no transmission to the decoder | none: a skip keyed to the residue would never fire on the air, and one that fires on real own-send spans lowers two capture floors (decision 4, keep rule) | 70 | 70 | - | red-open; not attacked, which breaks the sequence: 0 of 3 |

## #43 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "coverage-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | `CwDecoder.cs` 139 to 142 raise the leading edge again at every revision, and the test reads `CharacterDecoded` at 203; on the settled transcript 4 unreadable and 7 strangers remain in `QRZ?` and `DE/`, no cause at a line | line 203 reads `CharacterSettled` (decision 4); put back, green on none of the three | 5 + 37 | 4 + 7 | yes, movement not kept | red-open, attempt 1 of 3 |
| 405 | attack 2 | every stranger and placeholder in `QRZ?` and `DE/` settles at a fitted 18.5 to 20.9 wpm, from a dit cluster of 3 to 7 noise marks (`CwUnitEstimator.cs` 96 through 507). Upstream, the mix sits at a measured 575 Hz for a 615 Hz sender from 22.04 to 28.54 s (`CwToneTracker.cs` 1092, 1154 to 1169), not attacked | A1, B1 and B2 together (decision 1), put back; S1 as #15, put back | 5 + 37 | A1 4 + 7; S1 6 + 25, settled 3 + 0 | yes, movement not kept: no change turned it green; A1 cost 6 capture rows, S1 14 | red-open; moved, so the count starts again: sequence 0 of 3 |

## #44 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "exchange-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | as #43; on the settled transcript 0 unreadable and 7 strangers in `DE N0` and the second call, no cause at a line | as #43, put back | 3 + 21 | 0 + 7 | yes, movement not kept | red-open, attempt 1 of 3 |
| 405 | attack 2 | four `T` strangers and the second call's `E T E T E E E T E E` settle at 12.0 wpm under held gaps of 15/1127/323 ms, from `CwUnitEstimator.cs` 221 to 231, where the clip puts the character gap past the word gap, adopted at `CwProbabilisticStream.cs` 459 to 464. The `M` is #43's line. `I` and `,` have no line of their own; all three sit in a stretch mixed at a measured 575 Hz for a 615 Hz sender, 11.54 to 19.04 s (`CwToneTracker.cs` 1092, 1154 to 1169), not attacked | A1, B1 and B2 together, put back; G1, `CwUnitEstimator.cs` after 237: a clipped reading out of order is not separated, so the stream keeps the last gaps it stood behind; put back | 3 + 21 | A1 0 + 7; G1 4 + 16, settled 0 + 3 with the second call whole | yes, movement not kept: G1 cost nothing (captures 37 of 37, `021825` up 41 to 42, adjudicated 13, synthetics 2, every red-holding type as entry) but turned no red green | red-open; moved, so the count starts again: sequence 0 of 3 |

## #45 - `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "tightfist-easy")`

| unit | attempt | cause as traced | change made | before | after | moved | verdict so far |
|---|---|---|---|---|---|---|---|
| 402 | 1 of 3 | as #43; on the settled transcript `VVVTESTDETESTK■`, one trailing placeholder | (a) line 203 reads `CharacterSettled`, put back; (b) `CwProbabilisticStream.cs` 507: the flush does not settle a trailing unreadable character still inside the delay; put back | 1 + 3 | (a) 1 + 0; (b) 1 + 3 | (a) yes, movement not kept; (b) no | red-open, attempt 1 of 3 |
| 405 | attack 2 | the flush settles a trailing unreadable character still inside the delay (`CwProbabilisticStream.cs` 501 to 507); on the settled transcript that is the only fault | A1, B1 and B2 together (decision 1): line 203 reads `CharacterSettled`, and the flush does not settle that character; put back | 1 + 3 | 0 + 0, green on its own type | yes, **green but not kept**: captures 31 of 37, because six floors count the trailing placeholders B2 stops settling (`134712` 63 to 60, `011552` 30 to 28, `013010` 54 to 52, and 3 more) | red-open; moved, so the count starts again: sequence 0 of 3 |
