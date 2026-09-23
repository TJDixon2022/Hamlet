# Unit 409 - the decoder and the sweep agree about the pitch (3.8)

## 1. What is measured, and how

Printed by `tests\Hamlet.RadioEngine.Tests\Cw\TheTwoPitchesTableTests.cs`, a printer that asserts
nothing. Each recording is read the way the floors read a capture: a `CwDecoder` at 600 Hz fed one
hop at a time, 5 ms, then flushed.

- **decoder**: the pitch the decoder mixes at, `CwDecoder.Stream.ToneHz`. `CwDecoder.Step` sets it
  on every hop from the operator's lock, else the last pitch the tracker measured, else the
  tracker's bank centre. It is read after every hop, and the most-held value, rounded to a hertz and
  weighted by hops, is taken from the hop the first named character settled to the hop the last one
  did. With no named character it is taken over the whole recording. `low` and `high` are the
  extremes over the same span. `share apart` is the share of those hops more than 25 Hz from the
  300 to 900 sweep. `LockedToneHz` is on `CwDecoder` and is NaN unless the operator locks, so it
  is not the number used.
- **sweep 300-900**: `KeyingEnvelope.Best` over the whole recording, the tree's own range.
  **This is the column that judges.**
- **sweep 400-1200**: the same selection rule over `KeyingEnvelope.Measure` from 400 to 1200 Hz in
  25 Hz steps. It is printed only, beside the judge.
- **second-best**: the best score among 300 to 900 candidates more than 50 Hz from the winner, as a
  fraction of the winner's score.
- **keying**: `CwKeyingMeter`'s own test asked of the whole recording's best candidate, with every
  number from `CwKeyingThresholds`: score of at least 0.10, element median 25 to 250 ms, and swing
  of at least 20 dB.
- **single-sender**: not a two-station fixture of `CwTwoStationTests`, keying, and second-best
  under 0.5. `CwTwoStationTests` reads only the synthetic `receiver\two-station.wav`, so no case
  here fails the first clause. The rule was fixed before the table was read and was not changed.
- **agree**: the two pitches differ by 25 Hz or less.

The cases are the 37 capture rows and the 17:37 capture. The three anchors' recordings are among
the 37 and are marked. **A pitch agreeing is not a reading being right** (CLAUDE.md 0.0).

## 2. The table at entry, `eff57237`

Run twice, with the rows identical both times. 129 s.

| case | anchor | named | decoder Hz | low | high | share apart | sweep 300-900 | sweep 400-1200 | second-best | keying | single-sender | difference Hz | agree |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| cw-2026-08-17-013347 | anchor | 57 | 600 | 600.0 | 625.0 | 0.00 | 600 | 600 | 0.58 | no | no | 0 | agree |
| cw-2026-08-17-134712 | anchor | 21 | 500 | 500.0 | 500.0 | 0.00 | 500 | 500 | 0.74 | no | no | 0 | agree |
| cw-2026-08-18-004507 |  | 49 | 500 | 475.0 | 525.0 | 0.00 | 500 | 500 | 0.30 | keying | yes | 0 | agree |
| cw-2026-08-24-012403 |  | 21 | 440 | 435.0 | 450.0 | 0.00 | 450 | 450 | 0.29 | no | no | 10 | agree |
| cw-2026-08-22-031838 |  | 43 | 525 | 500.0 | 525.0 | 0.00 | 500 | 500 | 0.62 | keying | no | 25 | agree |
| cw-2026-08-22-031905 |  | 36 | 500 | 300.0 | 500.0 | 0.34 | 500 | 500 | 0.49 | keying | yes | 0 | agree |
| cw-2026-08-22-031948 |  | 31 | 500 | 500.0 | 500.0 | 0.00 | 500 | 500 | 0.67 | keying | no | 0 | agree |
| cw-2026-08-22-032012 |  | 43 | 500 | 500.0 | 500.0 | 0.00 | 500 | 500 | 0.71 | keying | no | 0 | agree |
| cw-2026-08-22-032050 |  | 44 | 500 | 325.0 | 500.0 | 0.23 | 500 | 500 | 0.57 | keying | no | 0 | agree |
| cw-2026-08-22-032113 |  | 47 | 500 | 500.0 | 650.0 | 0.33 | 500 | 500 | 0.69 | keying | no | 0 | agree |
| cw-2026-08-22-032129 |  | 65 | 650 | 500.0 | 650.0 | 0.65 | 500 | 500 | 0.69 | keying | no | 150 | apart |
| cw-2026-08-17-013622 |  | 51 | 600 | 600.0 | 625.0 | 0.00 | 600 | 600 | 0.43 | no | no | 0 | agree |
| cw-2026-08-18-003016 |  | 54 | 670 | 650.0 | 675.0 | 0.00 | 675 | 675 | 0.32 | keying | yes | 5 | agree |
| cw-2026-08-18-003126 |  | 48 | 670 | 650.0 | 680.0 | 0.00 | 675 | 675 | 0.32 | keying | yes | 5 | agree |
| cw-2026-08-18-003758 | anchor | 44 | 500 | 485.0 | 500.0 | 0.00 | 500 | 500 | 0.30 | keying | yes | 0 | agree |
| cw-2026-08-23-001520 |  | 1 | 600 | 600.0 | 600.0 | 1.00 | 525 | 1000 | 1.00 | keying | no | 75 | apart |
| cw-2026-08-23-001831 |  | 44 | 525 | 500.0 | 530.0 | 0.00 | 525 | 525 | 0.39 | no | no | 0 | agree |
| cw-2026-08-23-001952 |  | 57 | 525 | 375.0 | 575.0 | 0.79 | 475 | 475 | 0.72 | no | no | 50 | apart |
| cw-2026-08-23-002016 |  | 44 | 525 | 525.0 | 525.0 | 1.00 | 475 | 475 | 0.52 | no | no | 50 | apart |
| cw-2026-08-25-011552 |  | 22 | 500 | 495.0 | 500.0 | 0.00 | 500 | 500 | 0.23 | no | no | 0 | agree |
| cw-2026-08-25-012748 |  | 2 | 400 | 400.0 | 400.0 | 0.00 | 400 | 400 | 0.12 | no | no | 0 | agree |
| cw-2026-08-25-012823 |  | 26 | 500 | 500.0 | 500.0 | 0.00 | 500 | 500 | 0.18 | no | no | 0 | agree |
| cw-2026-08-25-012922 |  | 45 | 500 | 475.0 | 500.0 | 0.00 | 500 | 500 | 0.75 | keying | no | 0 | agree |
| cw-2026-08-25-013010 |  | 48 | 500 | 475.0 | 505.0 | 0.00 | 500 | 500 | 0.31 | no | no | 0 | agree |
| cw-2026-08-25-013150 |  | 51 | 500 | 495.0 | 525.0 | 0.09 | 525 | 525 | 0.41 | no | no | 25 | agree |
| cw-2026-08-25-013303 |  | 44 | 500 | 495.0 | 505.0 | 0.00 | 500 | 500 | 0.32 | no | no | 0 | agree |
| cw-2026-08-25-013402 |  | 56 | 525 | 525.0 | 550.0 | 0.00 | 550 | 550 | 0.57 | keying | no | 25 | agree |
| cw-2026-08-25-013520 |  | 55 | 550 | 525.0 | 550.0 | 0.00 | 550 | 550 | 0.58 | keying | no | 0 | agree |
| cw-2026-08-25-013637 |  | 60 | 525 | 525.0 | 550.0 | 0.00 | 550 | 550 | 0.60 | keying | no | 25 | agree |
| cw-2026-08-25-021410 |  | 36 | 535 | 535.0 | 550.0 | 0.00 | 550 | 550 | 0.53 | no | no | 15 | agree |
| cw-2026-08-25-021629 |  | 27 | 500 | 500.0 | 500.0 | 0.00 | 500 | 500 | 0.68 | no | no | 0 | agree |
| cw-2026-08-25-021825 |  | 25 | 400 | 390.0 | 410.0 | 1.00 | 325 | 600 | 0.79 | no | no | 75 | apart |
| cw-2026-08-26-125941 |  | 0 | 400 | 375.0 | 600.0 | 0.80 | 450 | 450 | 0.89 | no | no | 50 | apart |
| cw-2026-08-20-014854 |  | 0 | 600 | 600.0 | 825.0 | 0.48 | 575 | 575 | 0.33 | no | no | 25 | agree |
| cw-2026-08-20-014935 |  | 0 | 625 | 600.0 | 825.0 | 0.43 | 600 | 600 | 0.69 | no | no | 25 | agree |
| cw-2026-08-22-014113 |  | 0 | 600 | 600.0 | 725.0 | 0.43 | 600 | 600 | 0.39 | no | no | 0 | agree |
| cw-2026-08-22-014308 |  | 0 | 575 | 450.0 | 600.0 | 0.75 | 625 | 625 | 0.21 | no | no | 50 | apart |
| cw-2026-09-23-173723 |  | 46 | 575 | 575.0 | 600.0 | 0.00 | 600 | 600 | 0.07 | no | no | 25 | agree |

Every case except 17:37 is under `unadjudicated\` unless it is one of the four in the captures
folder itself.

**The count at entry: 5 of 38 cases are single-sender. Of those 5, 0 are apart.** The five are:
- `cw-2026-08-18-004507`: 500 against 500
- `cw-2026-08-22-031905`: 500 against 500
- `cw-2026-08-18-003016`: 670 against 675
- `cw-2026-08-18-003126`: 670 against 675
- `cw-2026-08-18-003758` (the anchor): 500 against 500

**So task 2 makes no change**, as the instruction orders, and the unit goes to task 3.

**Where the two sweep columns disagree**, and what the 400 to 1200 column would have made of each:
- `cw-2026-08-23-001520`: 300 to 900 says 525, and 400 to 1200 says 1000. The decoder sits at 600.
  It is apart on either column: 75 Hz on the judge, 400 Hz on the other. It is not single-sender
  either way, because its second-best is 1.00.
- `cw-2026-08-25-021825`: 300 to 900 says 325, and 400 to 1200 says 600. The decoder sits at 400.
  It is apart on either column: 75 Hz on the judge, 200 Hz on the other. It is not single-sender,
  because the sweep does not call it keying.

No other case differs between the two sweeps.

**What the fixed rule leaves out, said rather than acted on.** 33 cases are not single-sender:
- **22 fail on keying.** 18 of them fail on the swing, which is under 20 dB, and 17:37 is one of
  those: its swing is 17.0 dB while its score is 0.192. The other 4 fail on the score:
  - `013347`, an anchor, scores 0.048 at a swing of 91.5 dB
  - `134712`, an anchor, scores 0.017
  - `013622` scores 0.022
  - `001952` scores 0.054
- **11 are keying but have a runner-up of 0.5 or more.** Six are the 2026-08-22 run at 500 Hz:
  `031838`, `031948`, `032012`, `032050`, `032113` and `032129`. The other five are `001520`,
  `012922`, `013402`, `013520` and `013637`.

Among the 33, seven cases are apart:
- `032129`, by 150 Hz
- `001520`, by 75 Hz
- `001952`, by 50 Hz
- `002016`, by 50 Hz
- `021825`, by 75 Hz
- `125941`, by 50 Hz
- `014308`, by 50 Hz

**And a most-held pitch that agrees can still leave the sweep for long stretches.** Over the named
span:
- `031905`, a single-sender case, spends 0.34 of its hops at 300 Hz, from hop 2606 to 3605 and from
  5306 on. The sweep says 500.
- `032050` spends 0.23 of its hops at 325, from hop 4806.
- `032113` spends 0.33 of its hops at 600 and 650, from hop 4206.
- `032129` leaves 500 for 650 at hop 2706 and stays there, which is why its most-held value is apart.

**The rule was written before the table was read, and this unit does not change it.** It judges
3.8 on the five cases it admits.

The sweep's four figures per case, and the mix's value-and-hop segments, are in the run output
`.run-unit\unit409-table-entry2.txt` under `keying-why` and `seg`.

## 3. The table at exit, and the three parked reds

Written at task 3.
